using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamgukDefense
{
    /// <summary>
    /// 삼국지 로그라이크 디펜스의 Phase 1(코어 전투) 오케스트레이터. ssam.md 64절이 제안한
    /// BattleManager/RoundManager/SpawnManager 분리는 Phase 1 범위에서는 실제로 다른 라운드
    /// 진행 책임이 갈릴 일이 없어서(스폰도 라운드 시작의 한 단계일 뿐) 이 클래스 하나로 합쳤다 —
    /// 아이템 드랍 등 Phase 2+ 기능이 붙으면 그때 SpawnManager를 분리해도 늦지 않는다.
    /// 상태 흐름: CharacterSelect → Placement → Battle → (RoundClear → Placement 반복) → GameOver.
    /// 로직(이 파일)과 UI 생성(SamgukBattleManager.UI.cs)을 분리했다 — GoStop 세션들에서
    /// 반복적으로 겪은 "로직과 연출이 한 함수에 섞여 실수로 서로를 깨뜨리는" 문제를 처음부터 피하기 위함.
    /// </summary>
    public partial class SamgukBattleManager : MonoBehaviour
    {
        const int GRID_SIZE = 20;

        [SerializeField] SamgukCharacterData[] characterRoster;
        [SerializeField] SamgukMonsterData[] monsterRoster;
        [SerializeField] SamgukBalanceConfig balance;

        enum GameState { Lobby, CharacterSelect, Placement, Battle, RoundClear, GameOver }
        GameState state;

        // 배속(1x/1.5x/2x/3x) — 전투 중에만 Time.timeScale에 실제로 반영된다(다른 상태에서는
        // 항상 1로 강제). 유닛 Tick의 dt·SamgukProjectile.Update의 Time.deltaTime·DOTween 기본
        // 업데이트(피격 플래시 등, SetUpdate(true) 안 쓴 것들)까지 전부 이 값 하나로 같이
        // 빨라진다 — 별도로 dt를 손으로 꿰어야 했다면(특히 투사체는 BattleManager와 무관하게
        // 자기 Update()에서 직접 Time.deltaTime을 읽으므로) 누락되기 쉬웠을 지점인데, timeScale은
        // 이 전부를 한 번에 커버한다. 선택값 자체는 라운드를 넘어도 유지(사용자가 매번 다시
        // 누르지 않아도 되게) — 다만 이 씬은 여러 게임이 공유하는 프로젝트라, 전투 상태를 벗어나는
        // 모든 경로에서 반드시 1로 되돌려야 다른 게임에 새지 않는다(OnDestroy 안전망 포함).
        float battleSpeedMultiplier = 1f;

        SamgukGridBoard grid;
        float cellSize;

        int gold;
        int round;
        int alivePlayerCount;
        int aliveMonsterCount;

        readonly List<SamgukUnit> playerUnits = new List<SamgukUnit>();
        readonly List<SamgukUnit> monsterUnits = new List<SamgukUnit>();
        readonly List<SamgukCharacterData> selectedCharacters = new List<SamgukCharacterData>();
        readonly List<SamgukUnit> allUnitsBuffer = new List<SamgukUnit>();

        void Start()
        {
            if (balance == null)
            {
                balance = Resources.Load<SamgukBalanceConfig>("Data/Samguk/BalanceConfig");
            }
            if (characterRoster == null || characterRoster.Length == 0)
            {
                characterRoster = Resources.LoadAll<SamgukCharacterData>("Data/Samguk/Characters");
            }
            if (monsterRoster == null || monsterRoster.Length == 0)
            {
                monsterRoster = Resources.LoadAll<SamgukMonsterData>("Data/Samguk/Monsters");
            }
            if (itemRoster == null || itemRoster.Length == 0)
            {
                itemRoster = Resources.LoadAll<SamgukItemData>("Data/Samguk/Items");
            }
            SamgukUnit.MinDamage = balance != null ? balance.minDamage : 1f;

            LoadMeta();
            BuildStaticUI();
            ShowLobby();
        }

        void Update()
        {
            if (state != GameState.Battle) return;
            float dt = Time.deltaTime;

            allUnitsBuffer.Clear();
            allUnitsBuffer.AddRange(playerUnits);
            allUnitsBuffer.AddRange(monsterUnits);

            for (int i = 0; i < monsterUnits.Count; i++) monsterUnits[i].Tick(dt, allUnitsBuffer, playerUnits);
            for (int i = 0; i < playerUnits.Count; i++) playerUnits[i].Tick(dt, allUnitsBuffer, monsterUnits);
        }

        void OnDestroy()
        {
            // 씬을 어떤 경로로 나가든(다른 게임으로 이동 등) 여러 게임이 공유하는 이
            // 프로젝트에 배속 설정이 새어나가면 안 된다 — GoStop의 Screen.orientation
            // 안전망과 같은 이유·같은 패턴.
            Time.timeScale = 1f;
        }

        // ── 로비 ─────────────────────────────────────────────────────

        void ShowLobby()
        {
            state = GameState.Lobby;
            Time.timeScale = 1f;
            selectedCharacters.Clear();
            ClearMonsters();
            ClearPlayers();
            // 로비는 Persistent Data만 보여준다 — Run Data(인벤토리/장비)는 여기 있으면 안 된다.
            // Start() 최초 진입과 Game Over → OnRestartClicked() 양쪽 다 이 함수를 거치므로
            // 한 곳만 비워도 두 경로 다 커버된다.
            ResetRunData();
            ShowLobbyUI();
        }

        // ── 캐릭터 선택 ──────────────────────────────────────────────

        void ShowCharacterSelect()
        {
            state = GameState.CharacterSelect;
            selectedCharacters.Clear();
            ClearMonsters();
            ClearPlayers();

            var owned = new List<SamgukCharacterData>();
            if (characterRoster != null)
                foreach (var c in characterRoster) if (c != null && IsOwned(c.id)) owned.Add(c);
            ShowCharacterSelectUI(owned.ToArray());
        }

        void OnCharacterToggled(SamgukCharacterData data, bool selected)
        {
            if (selected)
            {
                // 상한은 이제 고정값이 아니라 로비에서 골드로 확장한 TeamSizeLevel(1~10)이다
                // (2026-09-16 사용자 확정 — 처음엔 1명뿐이고 업그레이드로 늘어난다).
                if (selectedCharacters.Count >= TeamSizeLevel) return;
                if (!selectedCharacters.Contains(data)) selectedCharacters.Add(data);
            }
            else
            {
                selectedCharacters.Remove(data);
            }
            RefreshCharacterSelectConfirmButton(selectedCharacters.Count is >= 1 && selectedCharacters.Count <= TeamSizeLevel);
        }

        void OnConfirmCharacterSelect()
        {
            if (selectedCharacters.Count < 1) return;
            HideCharacterSelectUI();
            BeginRun();
        }

        // ── 런 시작/라운드 진행 ──────────────────────────────────────

        void BeginRun()
        {
            gold = 0; // ssam.md 96.12 — 런 시작 지급 골드 폐지, 라운드 클리어/장비 정산으로만 번다(balance.startingGold는 더 이상 안 읽음)
            round = 0;
            ClearPlayers();
            // 새 런은 항상 빈 Run Data로 시작한다 — ShowLobby()에서 이미 비워졌지만, "런 시작 =
            // 빈 인벤토리"라는 요구사항을 이 시점에도 명시적으로 보장한다(로비를 거치지 않는
            // 미래의 호출 경로가 생겨도 안전하도록).
            ResetRunData();
            ComputeActiveSynergies();
            RefreshSynergyPanel();
            foreach (var data in selectedCharacters) SpawnPlayer(data);
            BeginRound(1);
        }

        void BeginRound(int newRound)
        {
            round = newRound;
            ClearMonsters();

            // 살아있는 플레이어만 재배치 대상 — 죽은 캐릭터는 되살아나지 않는다(퍼머데스).
            for (int i = playerUnits.Count - 1; i >= 0; i--)
            {
                if (!playerUnits[i].IsAlive)
                {
                    // equipment는 이제 캐릭터 id로 관리한다(로비에서도 계속 이어져야 하므로) —
                    // 이 유닛이 죽어도 그 캐릭터에게 물려있던 장비 기록 자체는 그대로 둔다.
                    Destroy(playerUnits[i].gameObject);
                    playerUnits.RemoveAt(i);
                }
            }

            grid.ClearAllOccupied();
            foreach (var p in playerUnits)
            {
                if (grid.TryRandomEmptyCell(out var cell)) p.SnapToCell(cell, grid);
            }

            int count = balance != null ? balance.monstersPerRound : 20;
            aliveMonsterCount = 0;

            // 일반 스폰 풀 — 이 라운드까지 등장 가능한(minRound<=round) 비-보스 몬스터 전부.
            // 예전엔 monsterRoster 전체에서 무작위였는데, 이제 라운드에 따라 동물→도적→정예부대
            // →황건적 순으로 새 몬스터가 누적되며 풀이 넓어진다(2026-09-16, 몬스터 다양성 요청).
            var normalPool = new List<SamgukMonsterData>();
            if (monsterRoster != null)
                foreach (var m in monsterRoster) if (m != null && !m.isBoss && m.minRound <= round) normalPool.Add(m);
            for (int i = 0; i < count; i++)
            {
                if (normalPool.Count == 0) break;
                var data = normalPool[Random.Range(0, normalPool.Count)];
                SpawnMonster(data, round);
            }

            SpawnBossWave(round);

            state = GameState.Placement;
            foreach (var p in playerUnits) p.SetDraggable(true);
            ShowPlacementUI(round, gold, aliveMonsterCount);
        }

        /// <summary>10라운드부터 보스 웨이브를 추가로 스폰한다. 11라운드짜리 블록마다 보스
        /// 등급(bossStar)이 1씩 오르고, 블록 안에서는 라운드마다 마릿수가 1씩 늘어난다 —
        /// R10=1성보스 1마리, R11=2마리, ..., R20=11마리, R21=2성보스 1마리로 리셋, 이후 같은
        /// 규칙 반복(2026-09-16 사용자 확정 사양 — 예시로 든 세 수치가 서로 완전히 일치하진
        /// 않아 "라운드마다 1씩 증가" 쪽으로 통일 해석했다). 정의된 최고 보스 등급을 넘어서는
        /// 라운드는 그 최고 등급 보스로 계속 대체한다(무한 라운드 안전망).</summary>
        void SpawnBossWave(int r)
        {
            if (r < 10 || monsterRoster == null) return;
            int tier = (r - 10) / 11 + 1;
            int bossCount = (r - 10) % 11 + 1;

            var pool = new List<SamgukMonsterData>();
            foreach (var m in monsterRoster) if (m != null && m.isBoss && m.bossStar == tier) pool.Add(m);
            if (pool.Count == 0)
            {
                int maxStar = 0;
                foreach (var m in monsterRoster) if (m != null && m.isBoss && m.bossStar > maxStar) maxStar = m.bossStar;
                if (maxStar <= 0) return;
                foreach (var m in monsterRoster) if (m != null && m.isBoss && m.bossStar == maxStar) pool.Add(m);
            }
            if (pool.Count == 0) return;

            for (int i = 0; i < bossCount; i++) SpawnMonster(pool[Random.Range(0, pool.Count)], r);
        }

        void OnStartBattleClicked()
        {
            if (state != GameState.Placement) return;
            foreach (var p in playerUnits) p.SetDraggable(false);
            state = GameState.Battle;
            Time.timeScale = battleSpeedMultiplier;
            HidePlacementUI();
        }

        /// <summary>배속 버튼 클릭 — 값만 바꾸고, 지금 실제로 전투 중이면 즉시 반영한다(전투
        /// 밖이면 다음 전투 시작 때 적용될 값만 기억해 둔다).</summary>
        void OnBattleSpeedSelected(float multiplier)
        {
            battleSpeedMultiplier = multiplier;
            if (state == GameState.Battle) Time.timeScale = multiplier;
            RefreshBattleSpeedButtons();
        }

        void HandleMonsterDied(SamgukUnit u)
        {
            if (state != GameState.Battle) return;
            aliveMonsterCount--;
            gold += Mathf.RoundToInt(u.GoldReward * GoldSynergyMultiplier());
            RollItemDrops(u);
            UpdateBattleHud(round, gold, aliveMonsterCount);
            if (aliveMonsterCount <= 0) StartCoroutine(RoundClearSeq());
        }

        /// <summary>GoldPercent 시너지는 유닛별 스탯이 아니라 팀 전체의 몬스터 처치 골드
        /// 획득량에 곱해지는 배율이라(SamgukSynergyData 문서 참고) ComputeEffectiveStats가
        /// 아니라 여기서 따로 합산한다 — activeSynergies는 이미 "팀 전원이 조건을 만족했을
        /// 때만" 채워지므로 멤버 여부를 다시 확인할 필요가 없다.</summary>
        float GoldSynergyMultiplier()
        {
            float pct = 0f;
            foreach (var syn in activeSynergies)
            {
                if (syn != null && syn.effectType == SamgukSynergyEffectType.GoldPercent) pct += syn.effectValue;
            }
            return 1f + pct;
        }

        void HandlePlayerDied(SamgukUnit u)
        {
            if (state != GameState.Battle) return;
            alivePlayerCount--;
            UpdateBattleHud(round, gold, aliveMonsterCount);
            if (alivePlayerCount <= 0)
            {
                // ssam.md 96.12 — 최종 정산 직전에 장비 가치를 한 번에 더한다. 인벤토리 재고뿐
                // 아니라 equipment 딕셔너리(캐릭터 id 기준, 죽은 캐릭터도 안 지워짐 — BeginRound
                // 참고)를 그대로 훑으므로 이미 죽은 캐릭터가 장착 중이던 장비도 빠짐없이 잡힌다.
                gold += ComputeItemSettlementValue();
                ShowGameOver(round, gold);
            }
        }

        IEnumerator RoundClearSeq()
        {
            state = GameState.RoundClear;
            Time.timeScale = 1f; // 클리어 토스트는 배속과 무관하게 항상 같은 길이로 읽혀야 한다
            // ssam.md 96.12 — 라운드 보상: N라운드 클리어 시 N×100원(1R=100, 2R=200, ...).
            // "클리어"라는 트리거 자체가 이 코루틴이 실제로 도는 시점(=몬스터를 전부 잡았을
            // 때)과 정확히 일치하므로, 죽어서 라운드를 못 끝낸 경우엔 자연히 그 라운드분은
            // 안 들어간다(round 변수로 사후 계산하면 "죽은 그 라운드"까지 포함되는 오류가
            // 생길 수 있어 이 시점에 직접 더하는 쪽을 택했다).
            gold += round * 100;
            ShowRoundClearToast(round);
            yield return new WaitForSecondsRealtime(1.1f);
            BeginRound(round + 1);
        }

        void ShowGameOver(int finalRound, int finalGold)
        {
            state = GameState.GameOver;
            Time.timeScale = 1f;
            persistentGold += finalGold; // ssam.md 2절 — 런 종료 시 획득 보상을 로비 골드로 정산
            SaveMeta();
            ShowGameOverUI(finalRound, finalGold, persistentGold);
        }

        void OnRestartClicked()
        {
            ShowLobby();
        }

        // ── 스폰 ────────────────────────────────────────────────────

        void SpawnPlayer(SamgukCharacterData data)
        {
            var go = new GameObject($"Player_{data.displayName}");
            var unit = go.AddComponent<SamgukUnit>();
            var slots = GetOrCreateEquipSlots(data);
            var eff = ComputeEffectiveStats(data, slots);
            unit.Init(SamgukFaction.Player, data.displayName, eff.hp, eff.atk, eff.def,
                eff.aspd, eff.mspd, eff.range, data.tintColor, grid.BoardRoot, cellSize, data.id, data.icon);
            unit.SourceData = data;
            unit.OnDied += HandlePlayerDied;
            unit.SetEquipment(slots);
            playerUnits.Add(unit);
            alivePlayerCount++;
        }

        void SpawnMonster(SamgukMonsterData data, int forRound)
        {
            var go = new GameObject($"Monster_{data.displayName}");
            var unit = go.AddComponent<SamgukUnit>();
            float hp = balance != null ? balance.GrowthScale(data.baseHP, balance.hpGrowth, forRound) : data.baseHP;
            float atk = balance != null ? balance.GrowthScale(data.baseAttack, balance.attackGrowth, forRound) : data.baseAttack;
            float def = balance != null ? balance.GrowthScale(data.baseDefense, balance.defenseGrowth, forRound) : data.baseDefense;
            float mspd = balance != null ? balance.GrowthScale(data.baseMoveSpeed, balance.moveSpeedGrowth, forRound) : data.baseMoveSpeed;
            unit.Init(SamgukFaction.Monster, data.displayName, hp, atk, def, data.baseAttackSpeed, mspd, data.baseAttackRange,
                data.tintColor, grid.BoardRoot, cellSize, "", data.icon);
            unit.GoldReward = data.goldReward;
            unit.SourceMonsterData = data;
            unit.OnDied += HandleMonsterDied;
            if (grid.TryRandomEmptyCell(out var cell)) unit.SnapToCell(cell, grid);
            monsterUnits.Add(unit);
            aliveMonsterCount++;
        }

        void ClearMonsters()
        {
            foreach (var m in monsterUnits) if (m != null) Destroy(m.gameObject);
            monsterUnits.Clear();
            aliveMonsterCount = 0;
        }

        void ClearPlayers()
        {
            foreach (var p in playerUnits) if (p != null) Destroy(p.gameObject);
            playerUnits.Clear();
            alivePlayerCount = 0;
        }
    }
}
