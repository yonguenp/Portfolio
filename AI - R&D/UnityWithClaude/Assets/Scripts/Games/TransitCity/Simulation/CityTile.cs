using UnityEngine;

namespace TransitCity
{
    public enum TileKind
    {
        Empty,
        NeighborCity,
        Residential,

        UtilityWater,
        UtilitySewage,
        UtilityPower,
        UtilityPolice,
        UtilityFire,
        UtilityEducation,
        UtilityMedical,

        /// <summary>소비 니즈 — 상점~백화점(티어로 성장). 대로변 결핍-우선 배치 시스템에 그대로 참여.</summary>
        UtilityStore,

        /// <summary>일자리 니즈 — 공장. 다른 인프라에 취직 못한 잉여 노동력을 흡수하는 용도라
        /// 배치 규칙이 다르다(결핍-우선이 아니라 "땅값이 제일 싼 곳", 그리고 "일자리가
        /// 실제로 부족할 때만" 생긴다). CityGridModel.UpdateFactorySpawning 참고.</summary>
        UtilityFactory,

        /// <summary>여유 니즈 — 공원/녹지. 소득 상위 니즈(재산이 많을수록 요구치가 훨씬 가파르게 오름, LeisureDemand 참고). PIMFY(비싼 땅 우선).</summary>
        UtilityPark,

        /// <summary>위생 니즈 — 쓰레기장. 대표적인 NIMBY(싼 땅 우선) 시설. 도시 위생 수치가 일정 이하로 떨어지면 결핍-우선 시스템이 자동으로 채운다.</summary>
        UtilityGarbage,

        /// <summary>다른 건물이 자기 규모(1x2/2x2)를 위해 차지한 칸 — 독립적인 개발 대상이 아니다. OwnerCoord가 소유주를 가리킨다.</summary>
        Extension,
    }

    /// <summary>주거지의 소득 계층 — 재산 수준(Wealth, 연속값)을 3단계로 나눈 것.
    /// 밀도 티어(Tier, 1~3)와 교차해 저밀도-고소득처럼 3×3=9가지 조합을 만든다(§5.5.3).
    /// 재개발(승급/강등/이주) 시점의 Wealth로 한 번 정해지고, 다음 재개발 때까지 고정된다
    /// (매 틱 흔들리는 Wealth를 그대로 쓰면 재개발 없이도 정원이 들쭉날쭉해진다).</summary>
    public enum IncomeTier
    {
        Low,
        Middle,
        High,
    }

    /// <summary>도로가 아닌 칸(빈 땅/이웃 도시/주거지/공공시설/확장 칸)의 시뮬레이션 상태.</summary>
    public sealed class CityTile
    {
        public readonly GridCoord Coord;
        public TileKind Kind;
        public float Potential;
        public int Population;
        public int Capacity;

        /// <summary>다른 주거지 대비 상대적 재산 수준(0=최빈, 1=최부유) — §5.5.1 상대 백분위.
        /// 목표치를 향해 매 틱 서서히 수렴한다(즉시 도달 아님) — 방금 생긴 땅에
        /// 갑자기 부유층이 정착하지 않고, 자리 잡고 안정될수록 계층이 올라간다.</summary>
        public float Wealth;

        /// <summary>혼잡도·인프라 충족도로 매 틱 갱신되는 만족도(0~1). 인구 성장률에 되먹임된다.</summary>
        public float Satisfaction = 1f;

        /// <summary>발생 시점에 한 번 정해지는 건물 크기 배율 — 건물마다 크기가 달라 보이게 한다.</summary>
        public float SizeTier = 1f;

        /// <summary>밀도/규모 단계(1~3). 공공시설=소형→중형→대형, 주거지=저밀도→중밀도→고밀도.
        /// 이제 단계마다 실제로 더 많은 칸(1x1→1x2→2x2)을 차지한다 — FootprintExtensions 참고.</summary>
        public int Tier = 1;

        /// <summary>주거지 전용 — 소득 계층(§5.5.3). Tier(밀도)와 교차해 정원을 정한다.
        /// 재개발 시점에만 갱신되고, 그 사이 Wealth가 흔들려도 이 값은 고정된다.</summary>
        public IncomeTier IncomeTier = IncomeTier.Low;

        /// <summary>이 건물이 원점(자기 칸) 말고 추가로 차지하고 있는 칸들 — 1x1이면 비어있음, 1x2면 1칸, 2x2면 3칸.</summary>
        public System.Collections.Generic.List<GridCoord> FootprintExtensions = new System.Collections.Generic.List<GridCoord>();

        /// <summary>Kind==Extension일 때만 의미 있음 — 이 칸을 실제로 소유한 건물의 원점 좌표.</summary>
        public GridCoord? OwnerCoord;

        /// <summary>만족도가 좋은/나쁜 상태로 연속 몇 틱째인지 — 재개발(승급/강등) 판정용 디바운스. 순간값 하나로 바로 판정하면 깜빡거리므로.</summary>
        public int GoodStreak;
        public int BadStreak;

        /// <summary>도로망과 완전히 끊긴(포텐셜 0) 상태로 연속 몇 틱째인지 — 폐허화 판정용.</summary>
        public int IsolatedStreak;

        /// <summary>사람/입주가 하나도 없는 상태로 연속 몇 틱째인지 — 공실 방치 철거 판정용.</summary>
        public int VacancyStreak;

        // 인프라 니즈(§5.8) — 각 공공시설로부터의 커버리지(0~1). 주거지에만 의미가 있다.
        public float WaterCoverage;
        public float SewageCoverage;
        public float PowerCoverage;
        public float PoliceCoverage;
        public float FireCoverage;
        public float EducationCoverage;
        public float MedicalCoverage;
        public float ConsumptionCoverage;
        public float LeisureCoverage;
        public float HygieneCoverage;

        /// <summary>혼잡도를 니즈로 환산한 값(1-혼잡도) — 유일하게 전용 인프라 건물이 없는 니즈. 나중에 버스·지하철 정류장 근접도가 추가되면 여기 합산될 예정.</summary>
        public float TransitCoverage;

        /// <summary>일자리 니즈 — 거리 기반이 아니라 도시 전체 고용률(일자리 총량/인구)을 그대로 쓰는 값이라 모든 주거지가 같은 값을 갖는다.</summary>
        public float JobCoverage;

        /// <summary>재산이 많을수록 인프라를 더 촘촘히 요구한다(상수도 없어도 버티는 빈민가 vs 정전에 민감한 부촌).</summary>
        public float InfraDemand => Mathf.Lerp(0.2f, 0.7f, Wealth);

        /// <summary>여유(공원) 니즈 전용 수요 곡선 — 재산에 제곱으로 비례해 일반 니즈보다 훨씬 가파르다("소득 상위 니즈" — 저소득층은 거의 신경 안 쓰고 고소득층만 뚜렷이 신경 씀).</summary>
        public float LeisureDemand => Mathf.Lerp(0f, 0.8f, Wealth * Wealth);

        public CityTile(GridCoord coord, TileKind kind)
        {
            Coord = coord;
            Kind = kind;
        }
    }
}
