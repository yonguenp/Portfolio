using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 프로토타입 전역 밸런스 수치. transitcity.md의 "자리표시 값"들을
    /// 여기 하나로 모아 코드에서 리터럴 숫자가 안 보이게 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "TransitCity/Balance Config")]
    public class BalanceConfig : ScriptableObject
    {
        [Header("맵")]
        public int gridWidth = 24;
        public int gridHeight = 18;
        public float cellSize = 2f;

        [Header("틱")]
        [Tooltip("실시간 몇 초마다 시뮬레이션 1틱(하루)을 진행하는지.")]
        public float tickIntervalSeconds = 2f;

        [Header("재정")]
        public int startingMoney = 5000;
        public float taxPerCapitaPerTick = 0.6f;
        public float roadUpkeepPerTilePerTick = 0.2f;

        [Header("주거 성장 (§5.1~5.3 단순화)")]
        [Tooltip("이웃 도시로부터 도로망 거리(칸)가 이 값을 넘으면 R포텐셜이 0에 수렴.")]
        public int maxUsefulDistance = 26;
        [Tooltip("R포텐셜이 이 값을 넘어야 빈 타일에 주거가 자연 발생한다.")]
        public float residentialPotentialThreshold = 0.15f;
        [Tooltip("포화도 대비 인구 증가율(자연증가+이주를 하나로 단순화, §5.3 참고). 실제 적용치는 만족도로 감쇠된다.")]
        public float populationGrowthRatePerTick = 0.22f;
        [Tooltip("재산 수준이 목표치(상대 백분위)를 향해 틱당 얼마나 수렴하는지 — 낮을수록 '자리 잡고 안정되는 데' 오래 걸린다.")]
        public float wealthDriftPerTick = 0.04f;

        [Header("만족도 — 입주 시 50%에서 시작해 서서히 변함 (신규)")]
        [Tooltip("실제 만족도가 목표치(혼잡·인프라로 계산되는 값)를 향해 틱당 얼마나 수렴하는지.")]
        public float satisfactionDriftPerTick = 0.08f;
        [Tooltip("만족도가 이 밑으로 떨어지면 거주자/근무자가 떠나기 시작한다.")]
        public float satisfactionLeaveThreshold = 0.1f;
        [Tooltip("만족도가 바닥을 친 뒤 틱당 정원 대비 떠나는 비율.")]
        public float satisfactionLeaveRatePerTick = 0.3f;

        [Header("공실 방치 시 철거 (신규 — 주거·상업·공업)")]
        [Tooltip("이만큼 연속으로 비어있으면(입주자 0) 들어올 사람이 없다고 보고 철거해 빈 땅으로 되돌린다.")]
        public int vacancyDemolishTicks = 15;

        [Header("치안·소방 순찰 (신규)")]
        [Tooltip("경찰서·소방서는 순찰 덕에 정적 반경보다 이만큼 더 멀리까지 커버한다.")]
        public int patrolRadiusBonus = 4;

        [Header("폐허화 — 도로가 끊기면 쇠퇴 후 소멸 (신규)")]
        [Tooltip("도로 연결이 완전히 끊긴(포텐셜 0) 상태가 이만큼 연속되면 쇠퇴가 시작된다 — 곧바로 시작하면 도로 재공사 중에도 가혹하므로 약간의 유예를 둔다.")]
        public int abandonGracePeriodTicks = 3;
        [Tooltip("쇠퇴가 시작된 뒤 틱당 정원 대비 잃는 인구 비율 — 0이 되면 건물이 통째로 사라져 빈 땅으로 돌아간다.")]
        public float abandonDecayRatePerTick = 0.25f;

        [Header("재개발 — 주거·상업 밀도 티어 (신규, 도시가 살아있는 유기체처럼 보이게)")]
        [Tooltip("만족도가 이 값을 넘는 틱이 연속으로 이만큼 쌓이면 재개발(승급) — 저밀도→중밀도→고밀도.")]
        public float redevelopGoodSatisfaction = 0.6f;
        [Tooltip("만족도가 이 값 밑인 틱이 연속으로 이만큼 쌓이면 쇠퇴(강등).")]
        public float redevelopBadSatisfaction = 0.3f;
        [Tooltip("승급/강등이 실제로 발동하기까지 필요한 연속 틱 수 — 순간적인 혼잡 한 번으로 깜빡거리지 않게 하는 디바운스.")]
        public int redevelopStreakTicks = 8;

        [Header("주거지 정원 — 밀도×소득 3×3 표 (§5.5.3, 9단계 세분화)")]
        [Tooltip("저밀도(1티어)·저소득 — 재개발 이전 기본값이던 houseCapacity(20)를 대체.")]
        public int residentialTier1LowCapacity = 15;
        public int residentialTier1MiddleCapacity = 20;
        public int residentialTier1HighCapacity = 26;
        public int residentialTier2LowCapacity = 32;
        public int residentialTier2MiddleCapacity = 40;
        public int residentialTier2HighCapacity = 48;
        public int residentialTier3LowCapacity = 58;
        public int residentialTier3MiddleCapacity = 70;
        public int residentialTier3HighCapacity = 85;

        [Header("공장 입지 — 제일 싼 땅 찾기 (신규)")]
        [Tooltip("공장이 들어설 자리를 고를 때 '주변 땅값'(평균 Wealth)을 잴 반경(맨해튼 거리) — 이 안에서 가장 낮은 자리에 짓는다.")]
        public int industrialLandValueRadius = 4;

        [Header("공공 인프라 (§5.8 단순화 — 상수도/하수도/전기/치안/소방/교육/의료/소비, 대로변 전용)")]
        [Tooltip("도시 전체 평균 결핍치가 이 값을 넘는 인프라 종류가 있으면, 새로 개발 가능해지는(철거로 다시 빈 땅이 된 경우 포함) 대로변 땅이 그 시설로 우선 채워진다.")]
        public float infraPriorityThreshold = 0.15f;
        [Tooltip("인프라 시설의 기본(1티어) 커버리지 반경(도로망 홉 수) — 예전(4)엔 체감상 너무 좁아서 2배로 올림.")]
        public int utilityRadius = 8;
        [Tooltip("티어가 하나 오를 때마다 커버리지 반경에 더해지는 값 — 3티어면 20홉까지.")]
        public int utilityRadiusPerTierBonus = 6;
        [Tooltip("상수도·하수도·전기(의식주 필수) 전용 기본 반경 — 일반 인프라보다 더 넓다.")]
        public int essentialUtilityRadius = 14;
        [Tooltip("상수도·하수도·전기가 티어 오를 때마다 반경에 더해지는 값 — 최고 티어(3)면 40홉, 웬만한 맵을 하나로 커버할 만큼 크게.")]
        public int essentialUtilityRadiusPerTierBonus = 13;
        [Tooltip("이 인구 이상을 커버하고 있으면 인프라 시설이 2티어(중형)로 성장한다.")]
        public int utilityTier2Population = 40;
        [Tooltip("이 인구 이상을 커버하고 있으면 인프라 시설이 3티어(대형)로 성장한다.")]
        public int utilityTier3Population = 100;
        [Tooltip("주거지가 인프라 요구치를 못 채웠을 때 만족도에서 깎이는 감점 가중치.")]
        public float infraDissatisfactionWeight = 0.5f;
        [Tooltip("기존 건물을 강제로 인프라로 전환하는 최후 수단은, 그 종류가 이미 '인구 이 값당 1개' 꼴보다 많으면 더는 발동하지 않는다 — 긴 대로 하나에서 주거지를 계속 잡아먹는 폭주를 막는다.")]
        public int utilityCapPerPopulation = 40;

        [Header("장식용 교통(§11)")]
        [Tooltip("통근 차량 최대 동시 개수 — 순찰차와는 별도 상한(예전엔 하나로 묶여 있어서 러시아워에 늘어야 할 통근차가 순찰차한테 자리를 뺏겼다).")]
        public int maxCommuterCars = 30;
        [Tooltip("순찰차 최대 동시 개수.")]
        public int maxPatrolCars = 6;
        public float carSpeed = 3.5f;
        [Tooltip("우측통행 표현 — 진행 방향 기준 오른쪽으로 비켜서 달리는 거리(칸 크기 대비 비율).")]
        public float carLaneOffsetRatio = 0.18f;
        [Tooltip("앞차가 비킬 때까지 최대 이만큼(초) 기다린다 — 넘으면 그냥 밀고 들어간다(영구 정체 방지).")]
        public float carMaxQueueWaitSeconds = 4f;
        [Tooltip("도로 위를 걷는 보행자(원) 장식 오브젝트 최대 동시 개수.")]
        public int maxPedestrians = 10;
        public float pedestrianSpeed = 1.4f;
        [Tooltip("배율 1(평상시) 기준 통근 차량 스폰 간격(초) — 실제 간격은 이 값을 그때그때의 TrafficSpawnMultiplier로 나눈 값.")]
        public float carSpawnIntervalSeconds = 0.5f;
        [Tooltip("배율 1(주간) 기준 순찰차 스폰 간격(초).")]
        public float patrolSpawnIntervalSeconds = 3f;
        [Tooltip("배율 1(평상시) 기준 보행자 스폰 간격(초).")]
        public float pedestrianSpawnIntervalSeconds = 1.2f;
        [Tooltip("낮 시간대 업무 통행(상점 배달·물류 등) 기준 스폰 간격(초) — 낮/밤 활동 곡선에 따라 실제 간격이 달라진다.")]
        public float businessTripSpawnIntervalSeconds = 2.5f;

        [Header("하루 시간대 — 생활 패턴 리듬(§신규)")]
        [Tooltip("게임 속 하루(24시간)가 실시간으로 이만큼(초) 걸린다 — Day를 매기는 성장 틱과는 별개의, 훨씬 느린 시계.")]
        public float dayLengthSeconds = 60f;
        public float morningRushStart = 6.5f;
        public float morningRushPeak = 7.5f;
        public float morningRushEnd = 9f;
        public float eveningRushStart = 17.5f;
        public float eveningRushPeak = 18.5f;
        public float eveningRushEnd = 20f;
        [Tooltip("러시아워도 심야도 아닌 평상시 통근 차량 스폰 배율.")]
        public float baseTrafficMultiplier = 0.6f;
        [Tooltip("러시아워 피크 시 통근 차량 스폰 배율.")]
        public float rushHourTrafficMultiplier = 2.5f;
        [Tooltip("심야(자정 근처) 통근 차량 스폰 배율 — 가장 낮음(0은 아님).")]
        public float nightTrafficMultiplier = 0.15f;
        [Tooltip("주간 순찰차 스폰 배율.")]
        public float basePatrolMultiplier = 1f;
        [Tooltip("심야 순찰차 스폰 배율 — 야간 순찰은 있지만 낮보다 적게.")]
        public float nightPatrolMultiplier = 0.4f;

        [Header("교차로 처리(로터리/신호등/미설치)")]
        [Tooltip("도로 칸에 이어진 도로가 이 개수 이상이면 교차로로 취급한다.")]
        public int intersectionMinConnections = 3;
        public int roundaboutCost = 200;
        public int trafficLightCost = 120;
        [Tooltip("로터리는 거의 안 막힘 — 그 칸 기본 용량에 곱하는 배율.")]
        public float roundaboutCapacityMultiplier = 0.9f;
        [Tooltip("신호등은 어느 정도 처리하지만 대기가 있음.")]
        public float trafficLightCapacityMultiplier = 0.65f;
        [Tooltip("아무것도 안 지으면 사거리 전원이 멈췄다 순서대로 지나가는 취급 — 실효 용량이 크게 낮다.")]
        public float uncontrolledCapacityMultiplier = 0.3f;
        [Tooltip("신호등 칸을 지날 때 빨간불에 걸릴 확률(장식용 차량이 잠깐 멈추는 연출).")]
        public float trafficLightStopChance = 0.4f;
        public float trafficLightStopSecondsMin = 0.2f;
        public float trafficLightStopSecondsMax = 0.6f;
        [Tooltip("미설치 교차로가 실제로 용량을 넘었을 때(과부하) 차량이 멈춰 순서를 기다리는 시간 범위.")]
        public float uncontrolledJamStopSecondsMin = 0.5f;
        public float uncontrolledJamStopSecondsMax = 1.5f;
    }
}
