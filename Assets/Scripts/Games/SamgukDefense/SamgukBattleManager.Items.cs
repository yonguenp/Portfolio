using System.Collections.Generic;
using UnityEngine;

namespace SamgukDefense
{
    /// <summary>아이템/인벤토리/장비/조합(Phase 2+3). 전투 로직(Core)·화면 생성(UI)과 분리 —
    /// "장비를 다시 계산한다"는 순수 계산과 "그걸 화면에 어떻게 보여줄지"를 한 함수에 안 섞으려는
    /// 목적(GoStop 세션들에서 반복적으로 겪은 로직/연출 혼재 버그를 처음부터 피한다).
    ///
    /// <para><b>이 파일이 다루는 inventory/equipment는 전부 Run Data(로그라이크 런 하나에만
    /// 존재하는 데이터)다.</b> 예전엔 장비를 "다음 런에도 이어져야 한다"는 전제로 설계해서
    /// SamgukSaveManager를 통해 영구 저장까지 했었는데, 그게 바로 "게임을 새로 시작해도 로비에
    /// 아이템/장비가 이미 있다"는 버그의 원인이었다(2026-09-15 수정) — 로그라이크는 아이템이
    /// 런 하나에서만 의미가 있어야 한다. 지금은 절대 저장/로드하지 않고, ResetRunData()가
    /// 매 런 시작(BeginRun)과 로비 진입(ShowLobby — Game Over 직후 포함)마다 통째로 비운다.
    /// 장비는 여전히 캐릭터 id로 관리한다(살아있는 SamgukUnit이 아니라) — 배치 단계에서도
    /// 아직 스폰되지 않은 캐릭터의 장비를 미리 바꿀 수 있어야 하기 때문일 뿐, 영구성과는
    /// 무관하다.</para></summary>
    public partial class SamgukBattleManager
    {
        [SerializeField] SamgukItemData[] itemRoster;

        readonly Dictionary<SamgukItemData, int> inventory = new Dictionary<SamgukItemData, int>();
        readonly Dictionary<string, SamgukItemData[]> equipment = new Dictionary<string, SamgukItemData[]>();

        /// <summary>Run Data(인벤토리/장비)를 전부 비운다 — BeginRun()(새 런 시작)과 ShowLobby()
        /// (로비 진입, Game Over 직후 포함) 양쪽에서 호출된다. 영구 데이터(골드/보유 캐릭터/강화
        /// 레벨)는 절대 안 건드린다.</summary>
        void ResetRunData()
        {
            inventory.Clear();
            equipment.Clear();
        }

        SamgukItemData[] GetOrCreateEquipSlots(SamgukCharacterData data)
        {
            if (!equipment.TryGetValue(data.id, out var slots) || slots.Length != data.ItemSlotCount)
            {
                var fresh = new SamgukItemData[data.ItemSlotCount];
                if (slots != null) System.Array.Copy(slots, fresh, Mathf.Min(slots.Length, fresh.Length));
                slots = fresh;
                equipment[data.id] = slots;
            }
            return slots;
        }

        /// <summary>기본 스탯(강화 반영) + 장비 보너스(주인 배율 포함) + 활성 시너지(%)를 전부
        /// 합산한 최종 스탯. 스폰 시점·장비 변경 시점 양쪽에서 이 하나만 부르면 된다.</summary>
        (float hp, float atk, float def, float aspd, float mspd, float range) ComputeEffectiveStats(SamgukCharacterData baseData, IReadOnlyList<SamgukItemData> items)
        {
            float hp = baseData.baseHP, atk = baseData.baseAttack, def = baseData.baseDefense;
            float aspd = baseData.baseAttackSpeed, mspd = baseData.baseMoveSpeed, range = baseData.baseAttackRange;

            if (characterUpgrades.TryGetValue(baseData.id, out var lv) && lv != null && lv.Length >= 6)
            {
                // 레벨당 절대값 +1이 아니라 "자기 기본 스탯의 N%"로 스케일한다(SamgukBalanceConfig.
                // upgradePercentPerLevel 참고) — 기본 공격력/공격속도가 캐릭터마다 몇 배씩 다른데
                // 절대값 +1을 그대로 더하면 골드 대비 %DPS 상승폭이 캐릭터마다 크게 벌어진다.
                float pct = balance != null ? balance.upgradePercentPerLevel : 0.02f;
                hp += baseData.baseHP * pct * lv[0];
                atk += baseData.baseAttack * pct * lv[1];
                def += baseData.baseDefense * pct * lv[2];
                aspd += baseData.baseAttackSpeed * pct * lv[3];
                mspd += baseData.baseMoveSpeed * pct * lv[4];
                range += baseData.baseAttackRange * pct * lv[5];
            }

            if (items != null)
            {
                foreach (var it in items)
                {
                    if (it == null) continue;
                    bool isOwner = !string.IsNullOrEmpty(it.ownerCharacterId) && it.ownerCharacterId == baseData.id;
                    float mult = isOwner ? SamgukItemData.OwnerBonusMultiplier : 1f;
                    hp += it.bonusHP * mult;
                    atk += it.bonusAttack * mult;
                    def += it.bonusDefense * mult;
                    aspd += it.bonusAttackSpeed * mult;
                    mspd += it.bonusMoveSpeed * mult;
                    range += it.bonusAttackRange * mult;
                }
            }

            float hpPct = 0f, atkPct = 0f, defPct = 0f, aspdPct = 0f, mspdPct = 0f;
            foreach (var syn in activeSynergies)
            {
                if (syn == null || syn.requiredCharacterIds == null) continue;
                bool member = System.Array.IndexOf(syn.requiredCharacterIds, baseData.id) >= 0;
                if (!member) continue;
                switch (syn.effectType)
                {
                    case SamgukSynergyEffectType.HPPercent: hpPct += syn.effectValue; break;
                    case SamgukSynergyEffectType.AttackPercent: atkPct += syn.effectValue; break;
                    case SamgukSynergyEffectType.DefensePercent: defPct += syn.effectValue; break;
                    case SamgukSynergyEffectType.AttackSpeedPercent: aspdPct += syn.effectValue; break;
                    case SamgukSynergyEffectType.MoveSpeedPercent: mspdPct += syn.effectValue; break;
                    // 스킬 시스템이 없어 공격력 가산으로 근사 처리(SamgukSynergyData 문서 참고).
                    case SamgukSynergyEffectType.SkillEffectPercent: atkPct += syn.effectValue; break;
                    // GoldPercent는 여기서 처리 안 함(HandleMonsterDied가 팀 단위로 따로 적용).
                }
            }
            hp *= 1f + hpPct;
            atk *= 1f + atkPct;
            def *= 1f + defPct;
            aspd *= 1f + aspdPct;
            mspd *= 1f + mspdPct;

            return (hp, atk, def, aspd, mspd, range);
        }

        void AddToInventory(SamgukItemData item, int count)
        {
            if (item == null || count == 0) return;
            inventory.TryGetValue(item, out int cur);
            inventory[item] = cur + count;
        }

        bool TryRemoveFromInventory(SamgukItemData item, int count)
        {
            if (item == null) return false;
            if (!inventory.TryGetValue(item, out int cur) || cur < count) return false;
            inventory[item] = cur - count;
            return true;
        }

        void RollItemDrops(SamgukUnit u)
        {
            var src = u.SourceMonsterData;
            if (src == null || src.possibleDrops == null) return;
            foreach (var item in src.possibleDrops)
            {
                if (item == null) continue;
                if (Random.value <= item.dropRate)
                {
                    AddToInventory(item, 1);
                    AppendInventoryLog($"[{GradeLabel(item.grade)}] {item.displayName} 획득!");
                }
            }
        }

        SamgukUnit FindLiveUnit(string characterId)
        {
            foreach (var p in playerUnits)
            {
                if (p != null && p.SourceData != null && p.SourceData.id == characterId) return p;
            }
            return null;
        }

        /// <summary>아이템의 ownerCharacterId(문자열)를 실제 캐릭터 데이터로 역참조한다 —
        /// 인벤토리 UI가 "OO 전용" 문구를 이름으로 보여주는 데 쓰인다.</summary>
        SamgukCharacterData FindCharacterData(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || characterRoster == null) return null;
            foreach (var c in characterRoster) if (c != null && c.id == characterId) return c;
            return null;
        }

        /// <summary>장착/해제 — slotIndex에 newItem(null이면 해제)을 넣는다. 기존 아이템은
        /// 인벤토리로 돌아가고, 새 아이템은 인벤토리 재고가 있어야만 장착된다. 로비에서도,
        /// 전투 중(살아있는 유닛이 있을 때)에도 똑같이 쓸 수 있다 — 살아있는 유닛이 있으면
        /// 그 자리에서 바로 스탯을 다시 적용한다.</summary>
        bool EquipItem(SamgukCharacterData data, int slotIndex, SamgukItemData newItem)
        {
            var slots = GetOrCreateEquipSlots(data);
            if (slotIndex < 0 || slotIndex >= slots.Length) return false;
            var old = slots[slotIndex];
            if (old == newItem) return true;
            if (newItem != null && !TryRemoveFromInventory(newItem, 1)) return false;
            if (old != null) AddToInventory(old, 1);
            slots[slotIndex] = newItem;

            var live = FindLiveUnit(data.id);
            if (live != null)
            {
                var eff = ComputeEffectiveStats(data, slots);
                live.ApplyStatRecalc(eff.hp, eff.atk, eff.def, eff.aspd, eff.mspd, eff.range);
                live.SetEquipment(slots);
            }
            return true;
        }

        /// <summary>동일 아이템 3개 → upgradesTo 1개. 재료 3개가 있고 상위 등급이 지정돼
        /// 있어야만 성립한다.</summary>
        bool TryCraft(SamgukItemData item)
        {
            if (item == null || item.upgradesTo == null) return false;
            if (!TryRemoveFromInventory(item, 3)) return false;
            AddToInventory(item.upgradesTo, 1);
            AppendInventoryLog($"[{GradeLabel(item.grade)}] {item.displayName} ×3 → [{GradeLabel(item.upgradesTo.grade)}] {item.upgradesTo.displayName}");
            return true;
        }

        /// <summary>일괄 조합 — 지금 인벤토리에서 가능한 조합을 전부, 그것도 한 번에 끝나는 게
        /// 아니라 다음 등급으로 올라간 결과물이 또 3개가 모이면 그 등급도 이어서 계속 조합한다
        /// (예: 1등급 9개 → 2등급 3개 → 3등급 1개까지 한 번의 클릭으로 연쇄). 매 조합마다
        /// TryCraft가 로그를 남기므로 여기서는 총 횟수만 마지막 요약으로 덧붙인다.
        /// 조합 도중 인벤토리에 새 등급 아이템이 추가될 수 있어(never craft 대상이던 아이템이
        /// 새로 조합 가능해짐) — 바깥쪽 while로 "이번 패스에 뭐라도 조합됐으면 처음부터 다시
        /// 훑는다"를 반복해서 이런 연쇄를 놓치지 않는다.</summary>
        int CraftAll()
        {
            int totalCrafts = 0;
            bool progressedThisPass = true;
            while (progressedThisPass)
            {
                progressedThisPass = false;
                // 순회 중 inventory 딕셔너리 자체가 바뀌므로(TryRemoveFromInventory/AddToInventory)
                // 키 목록을 먼저 스냅샷 떠서 돈다 — 그 안에서 바뀐 값은 TryCraft가 직접 다시 읽는다.
                var keysSnapshot = new List<SamgukItemData>(inventory.Keys);
                foreach (var item in keysSnapshot)
                {
                    while (TryCraft(item))
                    {
                        totalCrafts++;
                        progressedThisPass = true;
                    }
                }
            }
            if (totalCrafts > 0) AppendInventoryLog($"일괄 조합 완료 — 총 {totalCrafts}회");
            else AppendInventoryLog("조합 가능한 아이템이 없습니다");
            return totalCrafts;
        }

        /// <summary>candidate가 current보다 "더 좋은 장비"인지 — 등급을 1순위로, 같은 등급이면
        /// 전용장비(ownerData와 ownerCharacterId 일치)를 2순위로 비교한다(사용자 확정 우선순위:
        /// "가장 높은 등급이고 전용장비 우선"). 완전히 동률이면 false — 이미 장착 중인 걸 굳이
        /// 같은 등급의 다른 아이템으로 갈아치우지 않는다(불필요한 인벤토리 요동 방지).</summary>
        static bool IsBetterEquip(SamgukItemData candidate, SamgukItemData current, SamgukCharacterData ownerData)
        {
            if (candidate == null) return false;
            if (current == null) return true;
            if (candidate.grade != current.grade) return candidate.grade > current.grade;
            bool candidateOwner = candidate.ownerCharacterId == ownerData.id;
            bool currentOwner = current.ownerCharacterId == ownerData.id;
            return candidateOwner && !currentOwner;
        }

        /// <summary>인벤토리(미장착 재고)를 통틀어 지금 이 슬롯에 넣을 수 있는 가장 좋은 아이템을
        /// 찾는다. 이미 장착 중인 아이템(current)보다 안 좋으면 그대로 current를 돌려줘서
        /// "바꿀 필요 없음"을 표현한다.</summary>
        SamgukItemData FindBestAvailableItem(SamgukCharacterData data, SamgukItemData current)
        {
            SamgukItemData best = null;
            foreach (var kv in inventory)
            {
                if (kv.Value <= 0) continue;
                if (IsBetterEquip(kv.Key, best, data)) best = kv.Key;
            }
            return IsBetterEquip(best, current, data) ? best : current;
        }

        /// <summary>일괄 장착 — 현재 배치된(playerUnits) 캐릭터 전원의 빈 슬롯·더 낮은 등급/
        /// 비전용 슬롯을 인벤토리에서 구할 수 있는 최선의 아이템으로 채운다. 슬롯 하나를 채울
        /// 때마다 인벤토리 재고가 실제로 줄어들므로(EquipItem이 TryRemoveFromInventory를
        /// 호출), 다음 슬롯을 채울 때는 자동으로 남은 재고 기준 "다음으로 좋은 아이템"이
        /// 뽑힌다 — 별도 재고 추적 로직 없이 자연스럽게 순위가 내려간다.</summary>
        int EquipAllBest()
        {
            int totalEquipped = 0;
            foreach (var unit in playerUnits)
            {
                if (unit == null || unit.SourceData == null) continue;
                var data = unit.SourceData;
                var slots = GetOrCreateEquipSlots(data);
                for (int i = 0; i < slots.Length; i++)
                {
                    var current = slots[i];
                    var best = FindBestAvailableItem(data, current);
                    if (best == current) continue; // 더 나은 게 없음(빈 슬롯인데 인벤토리도 비었거나, 이미 최선)
                    EquipItem(data, i, best);
                    totalEquipped++;
                }
            }
            if (totalEquipped > 0) AppendInventoryLog($"일괄 장착 완료 — 총 {totalEquipped}칸 교체");
            else AppendInventoryLog("장착할 더 좋은 아이템이 없습니다");
            return totalEquipped;
        }

        /// <summary>아이템 1개의 정산 가치(원) — 1등급 1원, 등급이 하나 오를 때마다 뒤에 0이
        /// 하나씩 더 붙는다(2~6등급 = 10/100/1,000/10,000/100,000원). float Mathf.Pow 대신
        /// 정수 루프로 계산해 고등급에서 부동소수점 반올림 오차가 안 생기게 한다.</summary>
        static int ItemSettlementValue(SamgukItemData item)
        {
            int v = 1;
            for (int i = 1; i < item.grade; i++) v *= 10;
            return v;
        }

        /// <summary>게임 오버 직전 최종 정산에 쓰는 "지금 갖고 있는 장비 전부"의 총 가치 —
        /// 인벤토리 재고(미장착) + equipment 딕셔너리에 낀 장비(장착 중, 캐릭터 id 기준이라
        /// 그 유닛이 이미 죽었어도 여전히 잡힌다) 양쪽을 다 더한다. playerUnits(살아있는
        /// 유닛만)를 순회하면 죽은 캐릭터의 장비를 놓치므로 반드시 equipment.Values를 직접
        /// 훑어야 한다.</summary>
        int ComputeItemSettlementValue()
        {
            int total = 0;
            foreach (var kv in inventory)
            {
                if (kv.Key == null || kv.Value <= 0) continue;
                total += ItemSettlementValue(kv.Key) * kv.Value;
            }
            foreach (var slots in equipment.Values)
            {
                if (slots == null) continue;
                foreach (var item in slots)
                {
                    if (item != null) total += ItemSettlementValue(item);
                }
            }
            return total;
        }
    }
}
