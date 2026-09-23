using System.Collections.Generic;
using UnityEngine;

namespace SamgukDefense
{
    /// <summary>로비 영역 로직(Phase 4~6) — 캐릭터 보유·강화, 시너지 판정, 뽑기, 저장/로드.
    /// 화면 생성은 SamgukBattleManager.MetaUI.cs. 전투 중 스탯 계산(SamgukBattleManager.Items.cs
    /// 의 ComputeEffectiveStats)이 여기 선언된 characterUpgrades/activeSynergies를 그대로 읽는다.
    /// 이 파일이 다루는 건 전부 Persistent Data(영구 데이터)다 — 인벤토리/장비(Run Data)는
    /// SamgukBattleManager.Items.cs에서 관리하고 여기서는 절대 저장/로드하지 않는다.</summary>
    public partial class SamgukBattleManager
    {
        [SerializeField] SamgukSynergyData[] synergyRoster;

        /// <summary>캐릭터 id → 보유 마릿수. 예전엔 HashSet(있다/없다)였는데, 가챠 중복 캐릭터가
        /// 그냥 버려지지 않고 "조합(승급) 재료"로 쌓여야 해서 2026-09-16에 카운트 기반으로
        /// 바꿨다 — 조조를 두 번 뽑으면 조조가 2마리가 된다.</summary>
        readonly Dictionary<string, int> ownedCharacterCounts = new Dictionary<string, int>();
        readonly Dictionary<string, int[]> characterUpgrades = new Dictionary<string, int[]>();
        readonly List<SamgukSynergyData> activeSynergies = new List<SamgukSynergyData>();

        int persistentGold;

        /// <summary>런당 출전 가능 인원(3~10, AbsoluteMaxTeamSize). 캐릭터 선택 화면의
        /// 상한(SamgukBattleManager.cs의 OnCharacterToggled)이 이 값을 그대로 쓴다.</summary>
        int teamSizeLevel = 3;
        const int BaseTeamSize = 3;
        public const int AbsoluteMaxTeamSize = 10;

        static readonly string[] UpgradeStatNames = { "HP", "공격력", "방어력", "공격속도", "이동속도", "사거리" };

        // ── 저장/로드 (ssam.md 85~86절 — 영구 데이터만, 전투 상태는 절대 저장 안 함) ──

        void LoadMeta()
        {
            if (synergyRoster == null || synergyRoster.Length == 0)
                synergyRoster = Resources.LoadAll<SamgukSynergyData>("Data/Samguk/Synergies");

            var save = SamgukSaveManager.Load();
            persistentGold = save.persistentGold;
            teamSizeLevel = Mathf.Clamp(save.teamSizeLevel, BaseTeamSize, AbsoluteMaxTeamSize);

            // ownedCharacterIds는 여전히 평범한 List<string>이다(저장 스키마는 안 바꿈) — 그저
            // 같은 id가 소유 마릿수만큼 여러 번 들어있는 멀티셋으로 재해석한다.
            ownedCharacterCounts.Clear();
            foreach (var id in save.ownedCharacterIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                ownedCharacterCounts.TryGetValue(id, out int c);
                ownedCharacterCounts[id] = c + 1;
            }

            characterUpgrades.Clear();
            for (int i = 0; i < save.upgradeCharIds.Count && i < save.upgradeLevelsCsv.Count; i++)
            {
                var parts = save.upgradeLevelsCsv[i].Split(',');
                var lv = new int[6];
                for (int j = 0; j < 6 && j < parts.Length; j++) int.TryParse(parts[j], out lv[j]);
                characterUpgrades[save.upgradeCharIds[i]] = lv;
            }

            // 인벤토리/장비는 Run Data라 여기서 절대 로드하지 않는다 — 매 런은 항상 빈 인벤토리로
            // 시작해야 한다(ResetRunData가 BeginRun()/ShowLobby() 양쪽에서 이미 비워 둔다).

            // 신규 유저 — 1성 서로 다른 2명 + 2성 1명, 총 3마리 지급(2026-09-16 사용자 확정 사양).
            // "로스터 배열 순서상 첫 N명"으로 뽑아서 캐릭터 데이터를 새로 추가/재배열해도 항상
            // 안정적으로 동작한다(특정 id를 하드코딩하지 않음).
            if (ownedCharacterCounts.Count == 0 && characterRoster != null && characterRoster.Length > 0)
            {
                int granted1Star = 0;
                foreach (var c in characterRoster)
                {
                    if (c == null || c.star != 1 || granted1Star >= 2) continue;
                    AdjustOwnedCount(c.id, 1);
                    granted1Star++;
                }
                foreach (var c in characterRoster)
                {
                    if (c == null || c.star != 2) continue;
                    AdjustOwnedCount(c.id, 1);
                    break;
                }
                // 1~2성 캐릭터가 로스터에 아예 없는 극단적 경우의 안전망 — 그래도 빈손으로
                // 로비에 들어오는 것보단 첫 캐릭터라도 쥐어주는 게 낫다.
                if (ownedCharacterCounts.Count == 0) AdjustOwnedCount(characterRoster[0].id, 1);
            }
        }

        void SaveMeta()
        {
            var save = new SamgukSaveData { persistentGold = persistentGold, teamSizeLevel = teamSizeLevel };
            foreach (var kv in ownedCharacterCounts)
                for (int i = 0; i < kv.Value; i++) save.ownedCharacterIds.Add(kv.Key);

            foreach (var kv in characterUpgrades)
            {
                // GetUpgradeLevels가 로비에서 카드를 구경만 해도(스탯 표시를 위해) 0으로 채운
                // 항목을 만들어 둔다 — 실제로 한 번도 강화 안 한 캐릭터는 저장하지 않는다.
                bool anyNonZero = false;
                foreach (var v in kv.Value) if (v != 0) { anyNonZero = true; break; }
                if (!anyNonZero) continue;
                save.upgradeCharIds.Add(kv.Key);
                save.upgradeLevelsCsv.Add(string.Join(",", kv.Value));
            }

            // 인벤토리/장비는 Run Data라 여기서 절대 저장하지 않는다 — SamgukBattleManager.Items.cs 참고.

            SamgukSaveManager.Save(save);
        }

        // ── 보유 마릿수 헬퍼 ──

        int GetOwnedCount(string characterId) => ownedCharacterCounts.TryGetValue(characterId, out int c) ? c : 0;

        bool IsOwned(string characterId) => GetOwnedCount(characterId) > 0;

        void AdjustOwnedCount(string characterId, int delta)
        {
            ownedCharacterCounts.TryGetValue(characterId, out int cur);
            cur += delta;
            if (cur <= 0) ownedCharacterCounts.Remove(characterId);
            else ownedCharacterCounts[characterId] = cur;
        }

        // ── 뽑기 (ssam.md 14절) ──

        bool TryGacha(out SamgukCharacterData result)
        {
            result = null;
            if (balance == null || characterRoster == null || characterRoster.Length == 0) return false;
            if (persistentGold < balance.gachaCost) return false;
            persistentGold -= balance.gachaCost;

            int star = RollStar();
            var pool = new List<SamgukCharacterData>();
            foreach (var c in characterRoster) if (c != null && c.star == star) pool.Add(c);
            if (pool.Count == 0) pool.AddRange(characterRoster); // 해당 등급 캐릭터가 없으면 안전망으로 전체에서
            result = pool[Random.Range(0, pool.Count)];
            AdjustOwnedCount(result.id, 1); // 이미 보유 중이면 중복 캐릭터로 마릿수만 늘어난다(조합 재료)
            SaveMeta();
            return true;
        }

        // ── 캐릭터 조합(승급) — 같은 등급 아무 3마리(같은 장수 반복 선택도 허용) → 상위 등급
        // 랜덤 캐릭터 1마리 (2026-09-16 신설, 2026-09-16 개정 — 처음엔 "같은 장수 3마리"만
        // 됐는데, 사용자 피드백으로 "등급만 같으면 서로 다른 장수 조합도" 가능하게 넓혔다.
        // 로비 UI(SamgukBattleManager.MetaUI.cs)에서 카드를 최대 3번 클릭해 재료를 고르고,
        // 그 선택 목록을 그대로 여기 넘긴다 — 같은 카드를 여러 번 고르면 같은 장수를 여러 마리
        // 선택한 것으로 취급된다.) ──

        /// <summary>picks(정확히 3장, 순서 무관, 같은 캐릭터 중복 선택 허용)를 실제로 그만큼
        /// 보유하고 있고 전부 같은 등급이며, 그 등급+1 캐릭터가 로스터에 하나라도 있으면 조합
        /// 가능. 비용은 (picks[0].star × characterCombineCostPerStar). 결과는 상위 등급 풀에서
        /// 무작위 1명(뽑기와 동일한 풀 선택 방식).</summary>
        bool TryCombineCharacters(IReadOnlyList<SamgukCharacterData> picks, out SamgukCharacterData result)
        {
            result = null;
            if (!ValidateCombinePicks(picks, out int star, out var need)) return false;

            int targetStar = star + 1;
            var pool = new List<SamgukCharacterData>();
            foreach (var c in characterRoster) if (c != null && c.star == targetStar) pool.Add(c);
            if (pool.Count == 0) return false; // 이미 최고 등급이거나, 상위 등급 캐릭터가 로스터에 없음

            int cost = Mathf.RoundToInt(star * balance.characterCombineCostPerStar);
            if (persistentGold < cost) return false;

            persistentGold -= cost;
            foreach (var kv in need) AdjustOwnedCount(kv.Key, -kv.Value);
            result = pool[Random.Range(0, pool.Count)];
            AdjustOwnedCount(result.id, 1);
            SaveMeta();
            return true;
        }

        /// <summary>지금 이 3마리 선택으로 조합할 수 있는지 — 버튼을 켤지 말지 + 안 되는
        /// 이유(reason, 사용자에게 보여줄 한글 문구)를 UI가 판단하는 용도.</summary>
        bool CanCombineCharacters(IReadOnlyList<SamgukCharacterData> picks, out int cost, out string reason)
        {
            cost = 0;
            reason = "";
            if (picks == null || picks.Count < 3) { reason = "같은 등급의 장수 3마리를 선택하세요"; return false; }
            if (!ValidateCombinePicksWithReason(picks, out int star, out reason)) return false;
            cost = Mathf.RoundToInt(star * balance.characterCombineCostPerStar);
            if (persistentGold < cost) { reason = "골드가 부족합니다"; return false; }
            return true;
        }

        /// <summary>picks가 정확히 3장·전부 같은 등급·실제 보유량 충분한지까지만 검증한다
        /// (골드/상위등급 존재 여부는 호출부가 각자 필요에 맞게 따로 확인). need에는 캐릭터id별
        /// 필요 마릿수(같은 캐릭터 중복 선택 시 합산)가 담겨 나온다.</summary>
        bool ValidateCombinePicks(IReadOnlyList<SamgukCharacterData> picks, out int star, out Dictionary<string, int> need)
        {
            star = 0;
            need = null;
            if (picks == null || picks.Count != 3 || balance == null || characterRoster == null) return false;
            if (picks[0] == null) return false;
            star = picks[0].star;
            for (int i = 1; i < 3; i++) if (picks[i] == null || picks[i].star != star) return false;

            need = new Dictionary<string, int>();
            foreach (var p in picks) { need.TryGetValue(p.id, out int c); need[p.id] = c + 1; }
            foreach (var kv in need) if (GetOwnedCount(kv.Key) < kv.Value) return false;
            return true;
        }

        /// <summary>ValidateCombinePicks와 같은 검증이지만 실패 사유를 한글 문구로 돌려준다
        /// (CanCombineCharacters 전용 — UI 안내 메시지용).</summary>
        bool ValidateCombinePicksWithReason(IReadOnlyList<SamgukCharacterData> picks, out int star, out string reason)
        {
            star = 0;
            reason = "";
            if (picks == null || picks.Count != 3 || balance == null || characterRoster == null)
            {
                reason = "같은 등급의 장수 3마리를 선택하세요";
                return false;
            }
            if (picks[0] == null) { reason = "같은 등급의 장수 3마리를 선택하세요"; return false; }
            star = picks[0].star;
            for (int i = 1; i < 3; i++)
                if (picks[i] == null || picks[i].star != star)
                {
                    reason = "같은 등급끼리만 조합할 수 있습니다";
                    return false;
                }

            var need = new Dictionary<string, int>();
            foreach (var p in picks) { need.TryGetValue(p.id, out int c); need[p.id] = c + 1; }
            foreach (var kv in need)
                if (GetOwnedCount(kv.Key) < kv.Value) { reason = "재료가 부족합니다"; return false; }

            bool hasHigherTier = false;
            foreach (var c in characterRoster) if (c != null && c.star == star + 1) { hasHigherTier = true; break; }
            if (!hasHigherTier) { reason = "이미 최고 등급입니다"; return false; }
            return true;
        }

        int RollStar()
        {
            var rates = balance.gachaRatesByStar;
            if (rates == null || rates.Length == 0) return 1;
            float total = 0f;
            foreach (var r in rates) total += r;
            float roll = Random.value * total;
            float acc = 0f;
            for (int i = 0; i < rates.Length; i++)
            {
                acc += rates[i];
                if (roll <= acc) return i + 1;
            }
            return rates.Length;
        }

        // ── 강화 (ssam.md 13절 — 스탯마다 +1, 비용은 Cost = BaseCost × GrowthRate^Level) ──

        int[] GetUpgradeLevels(string charId)
        {
            if (!characterUpgrades.TryGetValue(charId, out var lv) || lv == null || lv.Length < 6)
            {
                lv = new int[6];
                characterUpgrades[charId] = lv;
            }
            return lv;
        }

        int UpgradeStatCost(string charId, int statIndex) =>
            balance != null ? balance.UpgradeCost(GetUpgradeLevels(charId)[statIndex]) : 999999;

        bool TryUpgradeStat(string charId, int statIndex)
        {
            if (balance == null) return false;
            int cost = UpgradeStatCost(charId, statIndex);
            if (persistentGold < cost) return false;
            persistentGold -= cost;
            GetUpgradeLevels(charId)[statIndex] += 1;
            SaveMeta();
            return true;
        }

        // ── 출전 인원 확장 (2026-09-16 사용자 확정 — 1명 시작, 골드로 10명까지 영구 확장) ──

        /// <summary>N번째 확장(3명→4명이 1번째, ..., 9명→10명이 7번째) 비용 — 한국 화폐
        /// 단위처럼 1000/5000/10000/50000/100000/500000/1000000으로 ×5,×2를 번갈아 가며
        /// 커진다(2026-09-16 — 처음엔 100부터였는데 기본 인원을 1→3으로 올리면서 그만큼
        /// 뒤의 두 단계(100,500)를 건너뛰고 1000부터 시작하도록 같이 조정했다). 정수 연산만
        /// 써서 float 반올림 오차 없이 정확하다.</summary>
        static int TeamSizeUpgradeCost(int currentLevel)
        {
            int k = currentLevel - BaseTeamSize; // 0-indexed 단계
            int mult = (k % 2 == 0) ? 1 : 5;
            int pow10 = 1000;
            for (int i = 0; i < k / 2; i++) pow10 *= 10;
            return mult * pow10;
        }

        int TeamSizeLevel => teamSizeLevel;

        bool CanUpgradeTeamSize(out int cost)
        {
            cost = 0;
            if (teamSizeLevel >= AbsoluteMaxTeamSize) return false;
            cost = TeamSizeUpgradeCost(teamSizeLevel);
            return persistentGold >= cost;
        }

        bool TryUpgradeTeamSize()
        {
            if (!CanUpgradeTeamSize(out int cost)) return false;
            persistentGold -= cost;
            teamSizeLevel++;
            SaveMeta();
            return true;
        }

        // ── 시너지 (ssam.md 15~16절) ──

        void ComputeActiveSynergies()
        {
            activeSynergies.Clear();
            if (synergyRoster == null) return;
            foreach (var syn in synergyRoster)
            {
                if (syn == null || syn.requiredCharacterIds == null || syn.requiredCharacterIds.Length == 0) continue;
                bool all = true;
                foreach (var reqId in syn.requiredCharacterIds)
                {
                    bool found = false;
                    foreach (var sel in selectedCharacters) if (sel.id == reqId) { found = true; break; }
                    if (!found) { all = false; break; }
                }
                if (all) activeSynergies.Add(syn);
            }
        }
    }
}
