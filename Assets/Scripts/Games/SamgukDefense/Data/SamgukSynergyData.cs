using UnityEngine;

namespace SamgukDefense
{
    /// <summary>2026-09-16 ssam_info.md 시너지 표를 반영하며 확장 — 이 게임엔 스킬 시스템
    /// 자체가 없어서(순수 오토배틀 스탯전) SkillEffectPercent는 실제로는 AttackPercent와
    /// 같은 자리(공격력 가산)에 합산된다. 표시 라벨만 "스킬 효과"로 따로 남겨서, 이후 실제
    /// 스킬 시스템이 생기면 그때 분리하면 된다 — 지금은 근사치임을 명시.
    /// GoldPercent는 유닛별 스탯이 아니라 팀 전체의 몬스터 처치 골드 획득량에 곱해지는
    /// 배율이라 ComputeEffectiveStats가 아니라 HandleMonsterDied에서 따로 처리한다.</summary>
    public enum SamgukSynergyEffectType { HPPercent, AttackPercent, DefensePercent, AttackSpeedPercent, MoveSpeedPercent, SkillEffectPercent, GoldPercent }

    [CreateAssetMenu(fileName = "Synergy_", menuName = "SamgukDefense/Synergy Data")]
    public class SamgukSynergyData : ScriptableObject
    {
        public string id;
        public string displayName;

        /// <summary>SamgukCharacterData.id 목록 — 전투에 참여한 캐릭터 중 이 전부가 포함돼야
        /// 발동한다. 효과는 이 목록에 있는 캐릭터에게만 적용된다(ssam.md 16절 — 파티에 다른
        /// 캐릭터가 더 있어도 시너지 멤버 본인들만 받는다).</summary>
        public string[] requiredCharacterIds;

        public SamgukSynergyEffectType effectType;
        [Tooltip("0.2 = +20%")] public float effectValue;

        public string description;
    }
}
