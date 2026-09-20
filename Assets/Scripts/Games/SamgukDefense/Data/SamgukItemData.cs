using UnityEngine;

namespace SamgukDefense
{
    public enum SamgukItemType { Weapon, Armor, Accessory }
    public enum SamgukAttackType { SingleHit, Projectile, Splash }
    public enum SamgukTargetType { Nearest, Farthest, LowestHP, HighestHP, Random }

    [CreateAssetMenu(fileName = "Item_", menuName = "SamgukDefense/Item Data")]
    public class SamgukItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        public SamgukItemType itemType;

        /// <summary>이 아이템의 주인 캐릭터 id(SamgukCharacterData.id). 비워두면 주인 없음.</summary>
        public string ownerCharacterId;
        public const float OwnerBonusMultiplier = 1.2f; // ssam.md 18절 — 주인 장착 시 +20%

        /// <summary>3개 모아 조합하면 만들어지는 상위 아이템. null이면 최상급(더 조합 불가).</summary>
        public SamgukItemData upgradesTo;

        /// <summary>1=일반, 2=고급, 3=희귀, 4=영웅, 5=전설, 6=신화 — 인벤토리/장비 UI에서
        /// 등급명·등급색을 표시하는 용도로만 쓰인다. 전투 스탯 계산에는 전혀 관여하지 않는다
        /// (실제 파워는 그 아래 보너스 필드들이 결정) — 그래서 조합 체인이 없는 아이템(예:
        /// iron_bow)도 자유롭게 등급을 매길 수 있다. (2026-09-16 3→6등급으로 확장.)</summary>
        [Range(1, 6)] public int grade = 1;

        [Header("공통 보너스 — 무기/방어구/장식 전부 적용 가능")]
        public float bonusHP;
        public float bonusAttack;
        public float bonusDefense;
        public float bonusAttackSpeed;
        public float bonusMoveSpeed;
        public float bonusAttackRange;

        [Header("무기 전용 (itemType == Weapon)")]
        public SamgukAttackType attackType = SamgukAttackType.SingleHit;
        public SamgukTargetType targetType = SamgukTargetType.Nearest;
        public float weaponDamage = 30f;
        [Tooltip("최종 무기 피해량 = weaponDamage + 장착자.Attack * weaponDamageAttackScaling")]
        public float weaponDamageAttackScaling = 0.3f;
        public float weaponCooldown = 1.5f;
        public float weaponRange = 3f;
        public float projectileSpeed = 14f;
        public float splashRadius = 1.2f;

        [Header("무기 옵션 — 명중 시 발동 (itemType == Weapon, 2026-09-16 추가)")]
        [Tooltip("명중 시 대상을 공격 방향으로 이 거리(보드 셀 단위)만큼 밀어낸다. 0이면 없음.")]
        public float knockbackDistance = 0f;
        [Range(0f, 1f), Tooltip("명중당 대상을 기절시킬 확률")] public float stunChance = 0f;
        [Tooltip("기절 지속시간(초) — 기절한 대상은 이동/공격/무기발동을 전부 멈춘다.")] public float stunDuration = 0f;
        [Range(0f, 1f), Tooltip("명중당 대상에게 출혈을 거는 확률")] public float bleedChance = 0f;
        [Tooltip("출혈 틱당 피해량")] public float bleedDamagePerTick = 0f;
        [Tooltip("출혈 지속시간(초)")] public float bleedDuration = 0f;
        [Tooltip("출혈 틱 간격(초)")] public float bleedTickInterval = 0.5f;

        [Header("방어구 옵션 (itemType == Armor, 2026-09-16 추가)")]
        [Tooltip("초당 자연 회복량 — 전투 중 계속 적용된다.")] public float hpRegenPerSecond = 0f;
        [Range(0f, 1f), Tooltip("장착자가 입힌 피해의 이 비율만큼 자신을 회복한다(기본 공격+무기 명중 전부 적용).")]
        public float lifestealPercent = 0f;

        [Header("장신구 옵션 (itemType == Accessory, 2026-09-16 추가 — 이동속도 증가는 위 공통 bonusMoveSpeed로 이미 표현 가능)")]
        [Tooltip("돌진 재사용 대기시간(초). 0이면 돌진 없음.")] public float dashCooldown = 0f;
        [Tooltip("가장 가까운 적을 향해 즉시 접근하는 거리(보드 셀 단위) — 사거리 밖 교전을 빠르게 시작하게 해준다.")]
        public float dashDistance = 0f;

        [Header("드랍 (몬스터의 possibleDrops에 넣었을 때만 의미 있음)")]
        [Range(0f, 1f)] public float dropRate = 0.15f;

        public Color tintColor = Color.white;

        /// <summary>비어있으면 인벤토리/장비 UI가 텍스트 라벨만 표시(SamgukCharacterData.icon과
        /// 동일한 폴백 규칙).</summary>
        public Sprite icon;
    }
}
