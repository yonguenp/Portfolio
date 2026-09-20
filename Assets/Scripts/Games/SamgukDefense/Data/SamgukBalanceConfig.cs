using UnityEngine;
// force domain reload

namespace SamgukDefense
{
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "SamgukDefense/Balance Config")]
    public class SamgukBalanceConfig : ScriptableObject
    {
        public int monstersPerRound = 20;
        public float minDamage = 1f;

        [Header("Round-over-round monster growth (multiplicative, applied Round-1 times)")]
        public float hpGrowth = 1.12f;
        public float attackGrowth = 1.10f;
        public float defenseGrowth = 1.08f;
        public float moveSpeedGrowth = 1.02f;

        /// <summary>더 이상 안 읽힌다(ssam.md §96.12 — 런 시작 지급 골드를 0으로 고정하면서
        /// SamgukBattleManager.BeginRun()이 이 값을 참조하지 않게 됐다). 인스펙터 값을 지우면
        /// 기존 밸런스 에셋의 직렬화 데이터가 깨질 수 있어 필드 자체는 남겨뒀다.</summary>
        public int startingGold = 100;

        /// <summary>1성이 아닌 별도 등급의 확률 = "그 등급을 조합으로 만드는 데 필요한 하위
        /// 등급 3마리를 전부 뽑을 복합 확률"으로 역산한 값이다(2026-09-16, 사용자 확정 공식).
        /// P(2성)=25%, P(3성)=P(2성)³=1.5625%, P(4성)=P(3성)³≈0.00038%, P(5성)/P(6성)은 같은
        /// 규칙을 계속 적용하면 float 정밀도 이하로 사실상 0에 수렴한다 — 의도된 결과다: 5~6성은
        /// 가챠로는 사실상 못 얻고 조합(TryCombineCharacter)으로만 만든다. P(1성)은 나머지 전부.</summary>
        [Header("뽑기 — 1~6성 확률(%), 합계 100")]
        public int gachaCost = 30;
        public float[] gachaRatesByStar = { 73.4371185f, 25f, 1.5625f, 0.00038147f, 0.0000000000000055511f, 0f };

        [Header("캐릭터 강화 — Cost = BaseCost * GrowthRate^UpgradeLevel")]
        public float upgradeBaseCost = 10f;
        public float upgradeGrowthRate = 1.2f;

        /// <summary>강화 레벨당 스탯 증가량 = "그 캐릭터 자신의 기본 스탯 × 이 비율"(2026-09-16,
        /// 사용자 확정 — "골드는 동일하게 소모되는데 올라가는 수치들이 편차가있다"는 DPS 밸런스
        /// 제보로 수정). 예전엔 모든 캐릭터·모든 스탯이 무조건 "레벨당 +1"이라는 절대값 하나를
        /// 공유했는데, 캐릭터마다 기본 공격력(13~53)·공격속도(0.8~1.3)가 몇 배씩 차이 나서 같은
        /// 골드를 써도 %DPS 상승폭이 캐릭터마다 최대 4배(공격력 강화), 공격속도는 아예 레벨 1
        /// 한 번에 77~125%씩 치솟는 수준으로 벌어졌었다(기본값이 워낙 작아서 절대값 +1이 체감상
        /// 거의 2배에 가까웠기 때문). 자기 자신의 기본값 비율로 환산하면 이 스탯 격차·기본값
        /// 크기와 무관하게 모든 캐릭터가 레벨당 정확히 같은 %만큼 강해진다 — 골드 대비 DPS
        /// 효율이 캐릭터 전원 동일해진다.</summary>
        public float upgradePercentPerLevel = 0.02f;

        /// <summary>캐릭터 조합(승급) 비용 = 재료 등급 × 이 값. 1성 3마리→2성=100, 2성 3마리→3성
        /// =200, 3성 3마리→4성=300... (2026-09-16 사용자 확정).</summary>
        public float characterCombineCostPerStar = 100f;

        public float GrowthScale(float baseValue, float growth, int round) =>
            baseValue * Mathf.Pow(growth, Mathf.Max(0, round - 1));

        public int UpgradeCost(int currentLevel) =>
            Mathf.RoundToInt(upgradeBaseCost * Mathf.Pow(upgradeGrowthRate, currentLevel));
    }
}
