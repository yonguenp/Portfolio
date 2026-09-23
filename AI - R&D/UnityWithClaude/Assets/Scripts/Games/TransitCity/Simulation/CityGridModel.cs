using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 프로토타입 시뮬레이션 코어(순수 C#, MonoBehaviour 아님). transitcity.md
    /// §5.1~5.3/§5.8/§6을 대폭 단순화한 버전 — 정확한 축소 내역은
    /// transitcity_devlog.md 첫 항목 참고.
    ///
    /// 도로는 §4.1 설계(타일 "변" 위의 그래프)가 아니라 **타일 자체가 도로인**
    /// 고전 SimCity식 모델로 축소했다 — 구현 단순화를 위한 의도적 타협.
    ///
    /// 상업지구·공업지구라는 별도 용도지역 개념은 없앴다 — 이제 "소비"와
    /// "일자리"도 수도·전기처럼 그냥 주거지 니즈 중 하나고, 각각 상점~백화점
    /// (UtilityStore)·공장(UtilityFactory)이라는 인프라 건물이 채워준다.
    /// 도로변 빈 땅은 인프라로 채워지지 않는 한 전부 주거지가 된다.
    /// </summary>
    public sealed class CityGridModel
    {
        static readonly TileKind[] UtilityKinds =
        {
            TileKind.UtilityWater, TileKind.UtilitySewage, TileKind.UtilityPower,
            TileKind.UtilityPolice, TileKind.UtilityFire, TileKind.UtilityEducation,
            TileKind.UtilityMedical, TileKind.UtilityStore, TileKind.UtilityFactory,
            TileKind.UtilityPark, TileKind.UtilityGarbage,
        };

        // 일반 결핍-우선 자동배치 시스템에 참여하는 종류들 — 공장(일자리)은 제외.
        // 일자리는 "좋은 자리 우선"이 아니라 "제일 싼 자리, 그리고 정말 부족할 때만"이라는
        // 다른 규칙(UpdateFactorySpawning)으로 처리한다.
        static readonly TileKind[] PriorityPlacementKinds =
        {
            TileKind.UtilityWater, TileKind.UtilitySewage, TileKind.UtilityPower,
            TileKind.UtilityPolice, TileKind.UtilityFire, TileKind.UtilityEducation,
            TileKind.UtilityMedical, TileKind.UtilityStore,
            TileKind.UtilityPark, TileKind.UtilityGarbage,
        };

        static readonly (int dx, int dy)[] Orthogonal4Offsets = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        // 2x2를 구성하는 4가지 방향(원점이 그 사각형의 어느 모서리인지).
        static readonly (int dx, int dy)[][] Square2x2Offsets =
        {
            new (int dx, int dy)[] { (1, 0), (0, 1), (1, 1) },
            new (int dx, int dy)[] { (-1, 0), (0, 1), (-1, 1) },
            new (int dx, int dy)[] { (1, 0), (0, -1), (1, -1) },
            new (int dx, int dy)[] { (-1, 0), (0, -1), (-1, -1) },
        };

        readonly BalanceConfig config;

        readonly Dictionary<GridCoord, RoadCell> roads = new Dictionary<GridCoord, RoadCell>();
        readonly Dictionary<GridCoord, CityTile> tiles = new Dictionary<GridCoord, CityTile>();

        public int Money { get; private set; }
        public int Day { get; private set; }
        public int Population { get; private set; }

        /// <summary>
        /// 0~24시, 계속 순환하는 하루 시간(§생활 패턴 리듬). §Day를 매기는
        /// "성장 틱"(빠르게 흘러가는 추상적 하루)과는 완전히 별개의, 실시간으로
        /// 천천히 흐르는 시계 — 통근차·순찰차 스폰 빈도를 여기에 맞춘다.
        /// </summary>
        public float TimeOfDayHours { get; private set; }

        /// <summary>매 프레임 호출 — 하루(24시간)가 config.dayLengthSeconds만큼의 실시간에 걸쳐 흐르게 한다.</summary>
        public void AdvanceTimeOfDay(float deltaSeconds)
        {
            float hoursPerSecond = 24f / Mathf.Max(1f, config.dayLengthSeconds);
            TimeOfDayHours = Mathf.Repeat(TimeOfDayHours + deltaSeconds * hoursPerSecond, 24f);
        }

        /// <summary>0(자정) ~ 1(정오) — 낮/밤 기본 활동 수준을 나타내는 매끄러운(코사인) 곡선. 자정 전후로 자연스럽게 이어진다.</summary>
        public float DayActivityFactor01(float hour)
        {
            float phase = hour / 24f * Mathf.PI * 2f;
            float raw = Mathf.Cos(phase - Mathf.PI); // 정오(hour=12)에서 최대(1), 자정(hour=0/24)에서 최소(-1)
            return (raw + 1f) * 0.5f;
        }

        static float TentCurve(float hour, float start, float peak, float end)
        {
            if (hour <= start || hour >= end) return 0f;
            if (hour <= peak) return Mathf.InverseLerp(start, peak, hour);
            return 1f - Mathf.InverseLerp(peak, end, hour);
        }

        /// <summary>0~1 — 출퇴근 러시아워 강도(아침/저녁 중 더 강한 쪽). 러시아워가 아니면 0.</summary>
        public float CommuteRushFactor01() => Mathf.Max(
            TentCurve(TimeOfDayHours, config.morningRushStart, config.morningRushPeak, config.morningRushEnd),
            TentCurve(TimeOfDayHours, config.eveningRushStart, config.eveningRushPeak, config.eveningRushEnd));

        /// <summary>
        /// 0(출근 방향 — 집에서 바깥으로) ~ 1(퇴근 방향 — 바깥에서 집으로). 아침
        /// 러시아워 피크에서 0, 저녁 러시아워 피크에서 1이고 그 사이(주간)는
        /// 선형으로 전환되며, 저녁 피크~다음날 아침 피크(야간 포함) 구간은 다시
        /// 1에서 0으로 돌아온다 — 자정을 넘나들어도 끊김 없이 순환한다. 장식용
        /// 통근 차량이 "출근"인지 "퇴근"인지(=경로 방향)를 정하는 데 쓴다.
        /// </summary>
        public float HomeboundBias01()
        {
            float morningPeak = config.morningRushPeak;
            float eveningPeak = config.eveningRushPeak;
            float daySpan = Mathf.Max(0.01f, eveningPeak - morningPeak);
            float nightSpan = Mathf.Max(0.01f, 24f - daySpan);

            if (TimeOfDayHours >= morningPeak && TimeOfDayHours <= eveningPeak)
                return Mathf.Clamp01((TimeOfDayHours - morningPeak) / daySpan);

            float h = TimeOfDayHours < morningPeak ? TimeOfDayHours + 24f : TimeOfDayHours;
            return Mathf.Clamp01(1f - (h - eveningPeak) / nightSpan);
        }

        /// <summary>장식용 통근 차량(+보행자) 스폰 강도 배율 — 낮/밤 기본 곡선 위에 러시아워가 덧붙는다.</summary>
        public float TrafficSpawnMultiplier()
        {
            float day = DayActivityFactor01(TimeOfDayHours);
            float baseLevel = Mathf.Lerp(config.nightTrafficMultiplier, config.baseTrafficMultiplier, day);
            return Mathf.Lerp(baseLevel, config.rushHourTrafficMultiplier, CommuteRushFactor01());
        }

        /// <summary>순찰차 스폰 강도 배율 — 낮엔 활발, 밤엔 줄지만 0은 아니다("야간 순찰은 있지만 낮보다 적게").</summary>
        public float PatrolSpawnMultiplier() =>
            Mathf.Lerp(config.nightPatrolMultiplier, config.basePatrolMultiplier, DayActivityFactor01(TimeOfDayHours));

        /// <summary>
        /// 노동시장 단순화 모델 — 도시 전체 일자리 수요 대비 주거 인구 비율(0~1).
        /// 1이면 완전 고용(일할 사람이 넉넉), 낮을수록 "일자리 쪽에서 일손이
        /// 모자라다"는 뜻이라 인프라·상점의 활동/커버리지가 이 비율만큼 깎인다.
        /// 거주자 입장에서 "내가 일자리를 구할 수 있는가"는 이것의 역수 개념인
        /// JobCoverage로 따로 표현한다(§신규 — 일자리 니즈).
        /// </summary>
        public float LaborAvailability { get; private set; } = 1f;

        /// <summary>가장 최근 틱에 계산된 도로 연결성(장식용 차량 경로 계산용으로 노출).</summary>
        public IReadOnlyDictionary<GridCoord, int> LastRoadDistance { get; private set; } = new Dictionary<GridCoord, int>();
        public IReadOnlyDictionary<GridCoord, GridCoord?> LastRoadParent { get; private set; } = new Dictionary<GridCoord, GridCoord?>();

        public event Action<RoadCell> OnRoadBuilt;
        public event Action<RoadCell> OnRoadCompleted;
        public event Action<GridCoord> OnRoadRemoved;
        public event Action<CityTile> OnZoneSpawned;
        public event Action<GridCoord> OnZoneRemoved;
        public event Action<CityTile> OnUtilitySpawned;
        public event Action<CityTile> OnUtilityTierChanged;
        public event Action OnTickCompleted;
        public event Action OnMoneyChanged;

        public CityGridModel(BalanceConfig config)
        {
            this.config = config;
            Money = config.startingMoney;

            for (int x = 0; x < config.gridWidth; x++)
            for (int y = 0; y < config.gridHeight; y++)
            {
                var coord = new GridCoord(x, y);
                tiles[coord] = new CityTile(coord, TileKind.Empty);
            }

            // 예전엔 왼쪽 가장자리 전체 열이 인접 도시였다 — 도로를 어디에 이어도
            // 다 "외부와 연결"로 쳐줘서 출구가 사실상 무한히 많았고, 그래서 교통이
            // 한 곳에 몰리지 않아 정체가 전혀 안 생겼다(신고받음). 이제 4방향
            // 가장자리 중앙에 딱 한 칸씩, 총 4개 출입구만 둔다 — 어느 방향에서든
            // 연결은 가능하지만, 그 방향의 모든 통행이 그 한 칸으로 몰려야 한다.
            var neighborCityCoords = new[]
            {
                new GridCoord(0, config.gridHeight / 2),                    // 서쪽
                new GridCoord(config.gridWidth - 1, config.gridHeight / 2), // 동쪽
                new GridCoord(config.gridWidth / 2, 0),                     // 북쪽
                new GridCoord(config.gridWidth / 2, config.gridHeight - 1), // 남쪽
            };
            foreach (var c in neighborCityCoords) tiles[c] = new CityTile(c, TileKind.NeighborCity);
        }

        public static bool IsUtilityKind(TileKind k) => Array.IndexOf(UtilityKinds, k) >= 0;

        static bool IsDevelopedKind(TileKind k) => k == TileKind.Residential || IsUtilityKind(k);

        public bool IsRoad(GridCoord c) => roads.ContainsKey(c);
        public RoadCell GetRoad(GridCoord c) => roads.TryGetValue(c, out var r) ? r : null;
        public CityTile GetTile(GridCoord c) => tiles.TryGetValue(c, out var t) ? t : null;
        public IEnumerable<RoadCell> AllRoads => roads.Values;
        public IEnumerable<CityTile> AllTiles => tiles.Values;

        public bool InBounds(GridCoord c) =>
            c.X >= 0 && c.X < config.gridWidth && c.Y >= 0 && c.Y < config.gridHeight;

        public bool CanBuildRoadAt(GridCoord c)
        {
            if (!InBounds(c)) return false;
            if (roads.ContainsKey(c)) return false;
            if (!tiles.TryGetValue(c, out var tile) || tile.Kind != TileKind.Empty) return false;

            foreach (var n in GridCoord.Neighbors4(c))
            {
                if (roads.ContainsKey(n)) return true;
                if (tiles.TryGetValue(n, out var nt) && nt.Kind == TileKind.NeighborCity) return true;
            }
            return false;
        }

        /// <summary>도로를 짓는다. 비용 차감·유효성 검사는 BuildRoadCommand 쪽 책임.</summary>
        public RoadCell BuildRoad(GridCoord c, RoadTypeData type)
        {
            tiles.Remove(c);
            var road = new RoadCell(c, type);
            roads[c] = road;
            OnRoadBuilt?.Invoke(road);
            Logger.Log($"도로 착공: {c} ({type.displayName})", 1);
            return road;
        }

        /// <summary>도로가 3방향 이상 만나는 칸 — 교차로 처리(로터리/신호등/미설치)가 의미를 갖는 곳.</summary>
        public bool IsIntersection(GridCoord c)
        {
            if (!roads.TryGetValue(c, out var r) || !r.IsTraversable) return false;
            int count = 0;
            foreach (var n in GridCoord.Neighbors4(c))
                if (roads.TryGetValue(n, out var nr) && nr.IsTraversable) count++;
            return count >= config.intersectionMinConnections;
        }

        /// <summary>
        /// 이 칸의 실질 통행 용량 — 교차로가 아니면 도로 등급 그대로, 교차로면
        /// 처리 방식(로터리/신호등/미설치)에 따른 배율이 곱해진다. 미설치는
        /// "사거리 전원이 멈췄다 순서대로 지나가는" 취급이라 배율이 가장 낮다.
        /// </summary>
        public int EffectiveCapacity(RoadCell road)
        {
            if (!IsIntersection(road.Coord)) return road.Type.tripCapacity;
            float multiplier = road.Control switch
            {
                IntersectionControl.Roundabout => config.roundaboutCapacityMultiplier,
                IntersectionControl.TrafficLight => config.trafficLightCapacityMultiplier,
                _ => config.uncontrolledCapacityMultiplier,
            };
            return Mathf.Max(1, Mathf.RoundToInt(road.Type.tripCapacity * multiplier));
        }

        /// <summary>미설치 교차로가 실제로 용량을 넘었는지 — 장식용 차량이 "전원 멈췄다 순서대로" 연출을 트리거하는 조건.</summary>
        public bool IsUncontrolledOverloadedIntersection(GridCoord c)
        {
            if (!roads.TryGetValue(c, out var r) || !r.IsTraversable) return false;
            if (r.Control != IntersectionControl.Uncontrolled) return false;
            if (!IsIntersection(c)) return false;
            int cap = EffectiveCapacity(r);
            return cap > 0 && r.TripLoad >= cap;
        }

        public bool CanSetIntersectionControl(GridCoord c) =>
            roads.TryGetValue(c, out var r) && r.IsTraversable && IsIntersection(c);

        /// <summary>로터리/신호등을 짓거나(또는 철거해 미설치로 되돌리거나) — 비용 차감·유효성 검사는 SetIntersectionControlCommand 쪽 책임.</summary>
        public void SetIntersectionControl(GridCoord c, IntersectionControl control)
        {
            if (!roads.TryGetValue(c, out var r)) return;
            r.Control = control;
            Logger.Log($"교차로 처리 변경: {c} → {control}", 1);
        }

        public bool CanDemolishAt(GridCoord c)
        {
            if (!InBounds(c)) return false;
            if (roads.ContainsKey(c)) return true;
            if (!tiles.TryGetValue(c, out var t)) return false;
            return IsDevelopedKind(t.Kind) || t.Kind == TileKind.Extension;
        }

        /// <summary>도로 또는 건물 한 칸을 철거해 다시 빈 땅으로 되돌린다. 건물이 여러 칸(1x2/2x2)을 차지 중이면 전부 함께 반납된다.</summary>
        public void DemolishAt(GridCoord c)
        {
            if (roads.ContainsKey(c))
            {
                roads.Remove(c);
                tiles[c] = new CityTile(c, TileKind.Empty);
                OnRoadRemoved?.Invoke(c);
                Logger.Log($"도로 철거: {c}", 1);
                return;
            }

            if (!tiles.TryGetValue(c, out var tile)) return;

            // 확장 칸(1x2/2x2의 나머지 칸)을 클릭했으면 실제 소유주(원점) 쪽을 철거한다 —
            // 큰 건물의 일부만 뜯어낼 수는 없으니 통째로.
            if (tile.Kind == TileKind.Extension)
            {
                if (tile.OwnerCoord.HasValue) DemolishAt(tile.OwnerCoord.Value);
                return;
            }

            if (IsDevelopedKind(tile.Kind))
            {
                ReleaseFootprint(tile);
                tile.Kind = TileKind.Empty;
                tile.Population = 0;
                tile.Capacity = 0;
                tile.Potential = 0f;
                tile.Wealth = 0f;
                tile.Satisfaction = 1f;
                tile.Tier = 1;
                tile.IncomeTier = IncomeTier.Low;
                tile.GoodStreak = 0;
                tile.BadStreak = 0;
                tile.IsolatedStreak = 0;
                tile.VacancyStreak = 0;
                tile.WaterCoverage = tile.SewageCoverage = tile.PowerCoverage = tile.PoliceCoverage
                    = tile.FireCoverage = tile.EducationCoverage = tile.MedicalCoverage
                    = tile.ConsumptionCoverage = tile.JobCoverage = 0f;
                OnZoneRemoved?.Invoke(c);
                Logger.Log($"건물 철거: {c}", 1);
            }
        }

        public void AddMoney(int delta)
        {
            Money += delta;
            OnMoneyChanged?.Invoke();
        }

        /// <summary>
        /// 건설 진행만 잦은 간격(매 프레임 등)으로 부드럽게 진행 — 나머지 무거운
        /// 시뮬레이션(용도지역/인구/교통/재정)과 갱신 주기를 분리해, 도로가
        /// 부드럽게 솟아오르는 연출이 가능하게 한다.
        /// </summary>
        public void TickConstruction(float deltaSeconds)
        {
            foreach (var road in roads.Values)
            {
                bool wasTraversable = road.IsTraversable;
                bool changed = road.Tick(deltaSeconds);
                if (changed && !wasTraversable && road.IsTraversable)
                    OnRoadCompleted?.Invoke(road);
            }
        }

        /// <summary>시뮬레이션 1틱(용도지역/인구/교통/재정) 진행. 일정 간격으로 호출.</summary>
        public void RunTick()
        {
            var (distance, parent) = ComputeRoadConnectivity();
            LastRoadDistance = distance;
            LastRoadParent = parent;
            SpawnAndGrowZones(distance);
            RunTraffic(distance, parent);
            RunEconomy();

            Day++;
            OnTickCompleted?.Invoke();
        }

        /// <summary>이웃 도시에서부터 완공된 도로만 타고 가는 BFS. 거리·경로 부모를 돌려준다.</summary>
        (Dictionary<GridCoord, int> distance, Dictionary<GridCoord, GridCoord?> parent) ComputeRoadConnectivity()
        {
            var distance = new Dictionary<GridCoord, int>();
            var parent = new Dictionary<GridCoord, GridCoord?>();
            var queue = new Queue<GridCoord>();

            foreach (var road in roads.Values)
            {
                if (!road.IsTraversable) continue;
                bool touchesNeighborCity = GridCoord.Neighbors4(road.Coord)
                    .Any(n => tiles.TryGetValue(n, out var t) && t.Kind == TileKind.NeighborCity);
                if (!touchesNeighborCity) continue;

                distance[road.Coord] = 0;
                parent[road.Coord] = null;
                queue.Enqueue(road.Coord);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int d = distance[current];
                foreach (var n in GridCoord.Neighbors4(current))
                {
                    if (!roads.TryGetValue(n, out var nRoad) || !nRoad.IsTraversable) continue;
                    if (distance.ContainsKey(n)) continue;
                    distance[n] = d + 1;
                    parent[n] = current;
                    queue.Enqueue(n);
                }
            }

            return (distance, parent);
        }

        void SpawnAndGrowZones(Dictionary<GridCoord, int> roadDistance)
        {
            var entryRoadByCoord = new Dictionary<GridCoord, GridCoord?>();
            var avenueFrontage = new HashSet<GridCoord>();

            // 도시 전체 일자리 수요(공장 포함 모든 인프라 정원) 대비 지금 사는 인구
            // 비율. 상점·공장 등 인프라의 커버리지가 이 비율만큼 깎인다.
            LaborAvailability = ComputeLaborAvailability();

            // 도시 전체에서 가장 부족한 인프라 종류를 먼저 판단한다(직전 틱 커버리지
            // 기준 — 한 틱 지연, 일자리는 별도 규칙이라 여기 포함 안 됨). 부족한 게
            // 있으면, 새로 개발 가능해지는 대로변 빈 땅(철거로 다시 비워진 부지
            // 포함)이 그 시설로 우선 채워진다. 인프라 종류가 여러 개이다 보니
            // "완벽해질 때까지" 무한정 늘리게 두면 긴 대로 하나에서 주거지를
            // 계속 잡아먹는 폭주가 난다(실측 확인됨) — 도시 규모 대비 총량 상한.
            int totalUtilityCount = tiles.Values.Count(t => IsUtilityKind(t.Kind));
            int reasonableUtilityCap = Mathf.Max(PriorityPlacementKinds.Length, Population / config.utilityCapPerPopulation);
            var (priorityUtilityKind, _) = FindWorstInfraDeficit();
            // 이 총량 상한은 원래 소프트 니즈(치안·교육 등)가 무한정 늘어나 주거지를
            // 잡아먹는 폭주를 막으려는 것이었는데, 하한이 "종류 수"(10)라 사실상
            // "도시 전체에 상수도·하수도·전기 각 1개씩"까지만 허용하는 꼴이 된다.
            // 도로망이 자라 첫 발전소의 반경(§5.8.6) 밖으로 도시가 넓어지면, 진짜로
            // 2호기가 필요한데 이 상한에 막혀 영원히 못 지어지고, 커버 못 받는
            // 지역은 입주 게이트에 걸려 인구가 통째로 죽어버린다(실측 확인 —
            // 인구가 0까지 떨어진 뒤 다시는 안 돌아옴). 의식주 필수 3종은 애초에
            // IsAlreadyCoveredBySameKind가 "진짜 안 겹치는 자리"만 허용하므로,
            // 이 총량 상한에서는 제외한다.
            bool infraNeeded = priorityUtilityKind.HasValue
                && (IsEssentialUtilityKind(priorityUtilityKind.Value) || totalUtilityCount < reasonableUtilityCap);
            bool infraPlacedThisTick = false;

            // 공장이 필요하면(일자리 실제 부족 + 아직 공장이 없음) "제일 싼 땅"을
            // 미리 찜해둔다 — 이 Pass1 루프가 새로 열리는 대로변 빈 땅을 곧장
            // 주거지로 채워버리기 전에 선점해야 한다. 나중(주거지 성장 이후)에
            // 찾으면 그때는 이미 이번 틱에 열린 빈 땅이 전부 주거지가 된 뒤라
            // 영원히 자리를 못 찾는다.
            var (factoryNeeded, reservedFactorySite) = FindFactorySiteIfNeeded(roadDistance);
            bool factoryClaimed = false;

            // NIMBY/PIMFY 종류가 이번 틱의 우선순위로 뽑혔으면, "새로 열리는 땅 중
            // 아무 데나 먼저 만나는 칸"이 아니라 땅값 기준으로 제일 적합한 칸을
            // 먼저 찜해둔다(공장과 같은 패턴 — Pass1이 곧장 주거지로 채워버리기 전에).
            GridCoord? reservedLandValueSite = priorityUtilityKind.HasValue
                ? FindLandValueSite(priorityUtilityKind.Value, roadDistance)
                : null;

            // 이번 틱 우선순위 종류가 "이미 자기 담당구역(반경) 안에 같은 종류가
            // 있는" 칸까지 또 잡아먹지 않도록, 기존 같은 종류 시설들의 커버
            // 범위를 미리 계산해둔다 — 초반에 상수도·하수도·전기가 한곳에 몰려
            // 중복으로 계속 지어지던 문제의 원인이었다(도시 전체 평균 결핍만
            // 보고 "어디"는 안 따졌으므로, 이미 커버된 동네 바로 옆에 또 지어도
            // 막을 방법이 없었다).
            HashSet<GridCoord> priorityKindCoveredRoads = priorityUtilityKind.HasValue
                ? ComputeSameKindCoveredRoads(priorityUtilityKind.Value)
                : null;

            bool IsAlreadyCoveredBySameKind(GridCoord coord) =>
                priorityKindCoveredRoads != null && entryRoadByCoord.TryGetValue(coord, out var er)
                && er.HasValue && priorityKindCoveredRoads.Contains(er.Value);

            // 승급(밀도 재개발)하고 싶은데 제자리엔 공간이 없는 주거지가 있으면,
            // 발자국 전체를 놓을 수 있는 다른 빈 땅을 여기서 미리 찜해둔다 —
            // 공장·NIMBY/PIMFY와 똑같은 이유(Pass1이 새로 열리는 빈 땅을 곧장
            // 다른 용도로 채워버리기 전에 선점해야 함). 한 틱에 하나만 처리한다.
            var residentialRelocation = FindResidentialRelocationSiteIfNeeded(roadDistance);
            bool residentialRelocationClaimed = false;

            var placedThisTick = new List<CityTile>();

            foreach (var tile in tiles.Values)
            {
                if (tile.Kind == TileKind.NeighborCity || IsUtilityKind(tile.Kind) || tile.Kind == TileKind.Extension) continue;

                int? accessDistance = null;
                GridCoord? entryRoadCoord = null;
                bool hasAvenue = false;
                foreach (var n in GridCoord.Neighbors4(tile.Coord))
                {
                    if (roads.TryGetValue(n, out var nRoad) && nRoad.IsTraversable && nRoad.Type.tier == RoadTier.Avenue)
                        hasAvenue = true;

                    if (roadDistance.TryGetValue(n, out var d))
                    {
                        int candidate = d + 1;
                        if (accessDistance == null || candidate < accessDistance)
                        {
                            accessDistance = candidate;
                            entryRoadCoord = n;
                        }
                    }
                }

                float potential = accessDistance.HasValue
                    ? Mathf.Clamp01(1f - accessDistance.Value / (float)config.maxUsefulDistance)
                    : 0f;
                tile.Potential = potential;
                entryRoadByCoord[tile.Coord] = entryRoadCoord;
                if (hasAvenue) avenueFrontage.Add(tile.Coord);

                if (tile.Kind == TileKind.Empty && potential >= config.residentialPotentialThreshold)
                {
                    // 이미 살고 있는 주민이 이주해서라도 승급하려는 칸이면 최우선으로
                    // 배정한다 — 새 개발보다도 "기존 주민이 갈 곳을 잃지 않는 것"이
                    // 우선이어야 한다. 실제 이주(원래 칸 비우기)는 Pass1이 끝난 뒤
                    // 처리한다(지금 여기서 딕셔너리 구조를 건드리면 이 순회 자체가
                    // 깨진다) — 여기서는 이 칸을 "받는 쪽"으로만 채운다.
                    if (residentialRelocation.HasValue && !residentialRelocationClaimed && tile.Coord.Equals(residentialRelocation.Value.targetCoord))
                    {
                        var source = residentialRelocation.Value.source;
                        int newTier = residentialRelocation.Value.targetTier;
                        residentialRelocationClaimed = true;
                        tile.Kind = TileKind.Residential;
                        tile.Tier = newTier;
                        tile.Wealth = source.Wealth;
                        tile.IncomeTier = IncomeTierFor(tile.Wealth);
                        tile.Capacity = ResidentialCapacityFor(newTier, tile.IncomeTier);
                        tile.Population = Mathf.Min(tile.Capacity, source.Population);
                        tile.Satisfaction = source.Satisfaction;
                        tile.SizeTier = source.SizeTier;
                        // 발자국(확장 칸) 점유는 여기서 바로 하지 않는다 — ClaimFootprint가
                        // 확장 칸의 딕셔너리 엔트리를 통째로 교체하는데, 지금은 Pass1이
                        // tiles.Values를 순회하는 도중이라 그 자리에서 하면 순회 자체가
                        // 깨진다("Collection was modified" — 실제로 겪음). Pass1이 끝난
                        // 뒤(아래) 한 번에 처리한다.
                        OnZoneSpawned?.Invoke(tile);
                        Logger.Log($"주거지 이주(재개발, 제자리에 공간 없어 이전): {source.Coord} → {tile.Coord} ({newTier}티어, 인구 {tile.Population})", 1);
                        continue;
                    }

                    // 미리 찜해둔 공장 부지면 최우선으로 공장이 된다 — 인프라 결핍
                    // 배치보다도 먼저 체크한다(그래야 "일자리 부족"이 다른 결핍과
                    // 동시에 발생해도 자리를 뺏기지 않는다).
                    if (reservedFactorySite.HasValue && tile.Coord.Equals(reservedFactorySite.Value))
                    {
                        tile.Kind = TileKind.UtilityFactory;
                        tile.Tier = 1;
                        factoryClaimed = true;
                        OnUtilitySpawned?.Invoke(tile);
                        Logger.Log($"공장 발생(일자리 부족, 가장 싼 빈 땅에 배치): {tile.Coord}", 1);
                        continue;
                    }

                    // 인프라는 대로변에만 들어선다(§5.8) — 부족한 게 있으면 이 칸을 먼저 챙긴다.
                    // NIMBY/PIMFY 종류는 아무 칸이나 먼저 잡는 게 아니라, 미리 찜해둔
                    // 땅값 기준 최적 칸이어야만 배치된다(그래야 "제일 싼/비싼 땅"이라는
                    // 의미가 실제로 성립한다) — 그 칸이 없으면 이번 틱엔 보류하고
                    // 다음 기회(새로 열리는 다른 빈 땅)를 기다린다.
                    bool landValueMatch = !reservedLandValueSite.HasValue || tile.Coord.Equals(reservedLandValueSite.Value);
                    if (hasAvenue && infraNeeded && !infraPlacedThisTick && landValueMatch && !IsAlreadyCoveredBySameKind(tile.Coord))
                    {
                        infraPlacedThisTick = true;
                        tile.Kind = priorityUtilityKind.Value;
                        tile.Tier = 1;
                        placedThisTick.Add(tile);
                        continue;
                    }

                    // 상업지구·공업지구 개념이 없어졌으니, 인프라로 안 채워진 빈 땅은
                    // 도로 등급과 무관하게 전부 주거지가 된다.
                    tile.Kind = TileKind.Residential;
                    tile.IncomeTier = IncomeTierFor(tile.Wealth);
                    tile.Capacity = ResidentialCapacityFor(tile.Tier, tile.IncomeTier);
                    tile.Population = 0;
                    tile.SizeTier = Mathf.Lerp(0.85f, 1.15f, potential) * UnityEngine.Random.Range(0.95f, 1.05f);
                    OnZoneSpawned?.Invoke(tile);
                    Logger.Log($"주거지 발생: {tile.Coord} (포텐셜 {potential:F2})", 1);
                }
            }

            // 이주가 실제로 처리됐으면(새 칸을 받는 쪽은 이미 위에서 채워짐),
            // 원래 칸을 비운다 — Pass1 순회가 끝난 지금이라야 딕셔너리 구조를
            // 안전하게 건드릴 수 있다(철거와 같은 이유로 제자리 필드 리셋 사용,
            // DemolishAt과 동일 패턴).
            if (residentialRelocationClaimed)
            {
                var source = residentialRelocation.Value.source;
                var targetCoord = residentialRelocation.Value.targetCoord;
                var targetTier = residentialRelocation.Value.targetTier;

                // 원래 칸의 확장 칸부터 반납(있었다면) — 이제 딕셔너리를 건드려도
                // 안전하다(Pass1 순회가 이미 끝남).
                ReleaseFootprint(source);
                source.Kind = TileKind.Empty;
                source.Population = 0;
                source.Capacity = 0;
                source.Wealth = 0f;
                source.Satisfaction = 1f;
                source.Tier = 1;
                source.IncomeTier = IncomeTier.Low;
                source.GoodStreak = 0;
                source.BadStreak = 0;
                source.IsolatedStreak = 0;
                source.VacancyStreak = 0;
                OnZoneRemoved?.Invoke(source.Coord);

                // 새 칸의 발자국(1x2/2x2)도 이제 안전하게 점유한다.
                if (tiles.TryGetValue(targetCoord, out var newTile) && newTile.Kind == TileKind.Residential)
                {
                    var relocFootprint = FindFootprintFor(targetCoord, targetTier);
                    if (relocFootprint != null) ClaimFootprint(newTile, relocFootprint);
                }
            }

            // 이번 틱에 새로 열린 대로변 빈 땅으로 부족분을 못 채웠으면, 대로변에서
            // 가장 덜 자란(인구가 가장 적은) 건물 하나를 인프라로 강제 전환한다 —
            // "대로는 있는데 인프라가 영영 안 생긴다"는 상황을 막는 최후 수단
            // (위의 총량 상한을 이미 통과한 경우에만 여기까지 온다).
            if (infraNeeded && !infraPlacedThisTick)
            {
                // NIMBY는 재산 수준이 가장 낮은(싼) 곳을, PIMFY는 가장 높은(비싼) 곳을
                // 밀어낸다 — NORMAL은 예전처럼 그냥 제일 덜 자란(인구 최소) 곳.
                var evictionCandidates = tiles.Values.Where(t => avenueFrontage.Contains(t.Coord) && t.Kind == TileKind.Residential
                    && !IsAlreadyCoveredBySameKind(t.Coord));
                var landPref = LandValuePreference(priorityUtilityKind.Value);
                var evictTarget = landPref < 0 ? evictionCandidates.OrderBy(t => t.Wealth).FirstOrDefault()
                    : landPref > 0 ? evictionCandidates.OrderByDescending(t => t.Wealth).FirstOrDefault()
                    : evictionCandidates.OrderBy(t => t.Population).FirstOrDefault();
                if (evictTarget != null)
                {
                    // 기존 비주얼을 먼저 정리하도록 철거 이벤트부터 쏜다 — 안 그러면
                    // 옛 건물 비주얼이 안 지워진 채로 그 위에 새 비주얼이 겹쳐 남는다.
                    ReleaseFootprint(evictTarget); // 주거지가 차지하고 있던 확장 칸부터 반납 — 인프라는 1x1로 새출발.
                    OnZoneRemoved?.Invoke(evictTarget.Coord);
                    evictTarget.Kind = priorityUtilityKind.Value;
                    evictTarget.Tier = 1;
                    evictTarget.Population = 0;
                    evictTarget.Capacity = 0;
                    placedThisTick.Add(evictTarget);
                }
            }

            // 공장도 마찬가지다 — 새로 열린 빈 땅으로 못 챙겼으면(도로를 새로
            // 낸 바로 그 순간이 아닌 한 거의 항상 이 경우다, 대로변 빈 땅은
            // 열리자마자 한 틱 안에 전부 주거지가 되므로), 대로변 주거지 중
            // 재산 수준이 가장 낮은(=땅값이 가장 싼) 곳을 공장으로 강제 전환한다.
            if (factoryNeeded && !factoryClaimed)
            {
                var factoryEvictTarget = tiles.Values
                    .Where(t => avenueFrontage.Contains(t.Coord) && t.Kind == TileKind.Residential)
                    .OrderBy(t => t.Wealth)
                    .FirstOrDefault();
                if (factoryEvictTarget != null)
                {
                    ReleaseFootprint(factoryEvictTarget);
                    OnZoneRemoved?.Invoke(factoryEvictTarget.Coord);
                    factoryEvictTarget.Kind = TileKind.UtilityFactory;
                    factoryEvictTarget.Tier = 1;
                    factoryEvictTarget.Population = 0;
                    factoryEvictTarget.Capacity = 0;
                    OnUtilitySpawned?.Invoke(factoryEvictTarget);
                    Logger.Log($"공장 발생(일자리 부족, 가장 싼 기존 부지 전환): {factoryEvictTarget.Coord} (Wealth {factoryEvictTarget.Wealth:F2})", 1);
                }
            }

            foreach (var utility in placedThisTick)
            {
                OnUtilitySpawned?.Invoke(utility);
                Logger.Log($"{KindLabel(utility.Kind)} 발생: {utility.Coord} (부족 우선 배치, 누적인구 {Population})", 1);
            }

            // 도로가 철거돼 도로망과 완전히 끊긴(포텐셜 0) 건물은 쇠퇴하다 사라진다 —
            // Pass1에서 이미 갱신된 Potential을 그대로 쓰므로 여기서 실행.
            UpdateAbandonment();

            var residentials = tiles.Values.Where(t => t.Kind == TileKind.Residential).ToList();

            // §5.5.1 — 절대치가 아니라 "다른 주거지 대비 상대 순위"를 목표치로 삼되,
            // 그 목표로 즉시 점프하지 않고 서서히 수렴한다(막 생긴 땅은 항상 0에서 시작).
            if (residentials.Count > 0)
            {
                var ranked = residentials.OrderBy(t => t.Potential).ToList();
                for (int i = 0; i < ranked.Count; i++)
                {
                    float targetWealth = ranked.Count <= 1 ? 1f : i / (float)(ranked.Count - 1);
                    ranked[i].Wealth = Mathf.MoveTowards(ranked[i].Wealth, targetWealth, config.wealthDriftPerTick);
                }
            }

            var utilityTilesByKind = new Dictionary<TileKind, List<CityTile>>();
            foreach (var kind in UtilityKinds) utilityTilesByKind[kind] = new List<CityTile>();
            foreach (var t in tiles.Values)
                if (IsUtilityKind(t.Kind)) utilityTilesByKind[t.Kind].Add(t);

            // 커버리지는 이제 직선거리가 아니라 **도로망을 실제로 타고 가는 홉 수**로
            // 잰다 — "반경 안에 있기만 하면 커버"가 아니라 "도로로 실제 닿아야
            // 커버"가 되므로, 도로 설계 자체가 니즈 퍼즐이 된다. 시설 하나당 한 번만
            // 유효반경까지 플러드필해서(주거지 수와 무관하게 시설 개수만큼만) 캐싱해두고,
            // 아래 주거지 루프에서는 그 결과를 조회만 한다.
            var utilityFloods = new Dictionary<TileKind, List<(CityTile utility, Dictionary<GridCoord, int> flood, int radius)>>();
            foreach (var kind in UtilityKinds)
            {
                var list = new List<(CityTile, Dictionary<GridCoord, int>, int)>();
                foreach (var u in utilityTilesByKind[kind])
                {
                    int radius = UtilityRadiusForTier(u.Kind, u.Tier);
                    if (u.Kind == TileKind.UtilityPolice || u.Kind == TileKind.UtilityFire) radius += config.patrolRadiusBonus;
                    var entryRoad = FindEntryRoad(u.Coord);
                    if (!entryRoad.HasValue) continue; // 대로변 전용 배치라 이론상 항상 있어야 함.
                    list.Add((u, FloodRoadDistance(entryRoad.Value, radius), radius));
                }
                utilityFloods[kind] = list;
            }

            // 일자리 니즈는 거리 기반이 아니라 도시 전체 고용률(일자리 총량/인구)
            // 하나로 모든 주거지에 동일하게 적용된다 — 일자리가 충분하면 1(완전고용),
            // 부족하면 그 비율만큼 낮아진다.
            int totalJobCapacity = tiles.Values.Where(t => IsUtilityKind(t.Kind)).Sum(t => UtilityJobsForTier(t.Kind, t.Tier));
            float cityJobAvailability = Population <= 0 ? 1f : Mathf.Clamp01(totalJobCapacity / (float)Population);

            var abandonedResidential = new List<CityTile>();

            foreach (var tile in residentials)
            {
                // 혼잡은 이제 "집 앞 도로 한 칸"이 아니라 통근 경로 전체에서 가장
                // 막힌 구간(교차로 포함, EffectiveCapacity 기준) 하나로 정해진다 —
                // 경로 어디든 병목이 있으면 그게 곧 그 집의 혼잡이 되므로, 도로망
                // 설계 자체가 만족도에 직결된다("우회로를 뚫었더니 체감이 바뀐다").
                float congestion = 0f;
                if (entryRoadByCoord.TryGetValue(tile.Coord, out var entryRoadCoord) && entryRoadCoord.HasValue)
                {
                    GridCoord? cursor = entryRoadCoord;
                    while (cursor.HasValue)
                    {
                        if (roads.TryGetValue(cursor.Value, out var r))
                        {
                            int cap = EffectiveCapacity(r);
                            if (cap > 0) congestion = Mathf.Max(congestion, Mathf.Clamp01(r.TripLoad / (float)cap));
                        }
                        cursor = LastRoadParent.TryGetValue(cursor.Value, out var p) ? p : null;
                    }
                }

                // 부유할수록 혼잡에 더 민감하다(§5.6 부유층 이탈 전조).
                float wealthSensitivity = Mathf.Lerp(0.6f, 1.4f, tile.Wealth);
                float targetSatisfaction = Mathf.Clamp01(1f - congestion * wealthSensitivity);

                var residentEntryRoad = entryRoadByCoord.TryGetValue(tile.Coord, out var er) ? er : null;
                tile.WaterCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityWater]);
                tile.SewageCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilitySewage]);
                tile.PowerCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityPower]);
                tile.PoliceCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityPolice]);
                tile.FireCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityFire]);
                tile.EducationCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityEducation]);
                tile.MedicalCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityMedical]);
                tile.ConsumptionCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityStore]);
                tile.LeisureCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityPark]);
                tile.HygieneCoverage = ComputeCoverage(residentEntryRoad, utilityFloods[TileKind.UtilityGarbage]);
                tile.JobCoverage = cityJobAvailability;
                // 교통 니즈는 건물이 아니라 그 칸 바로 앞 도로의 혼잡도를 그대로
                // 환산한 값이다 — 나중에 버스·지하철 정류장 근접도가 추가되면
                // 여기 더해질 지점(지금은 버스·지하철이 없어 혼잡도만 반영).
                tile.TransitCoverage = Mathf.Clamp01(1f - congestion);

                float demand = tile.InfraDemand;
                float infraDeficit = (
                    Mathf.Max(0f, demand - tile.WaterCoverage) +
                    Mathf.Max(0f, demand - tile.SewageCoverage) +
                    Mathf.Max(0f, demand - tile.PowerCoverage) +
                    Mathf.Max(0f, demand - tile.PoliceCoverage) +
                    Mathf.Max(0f, demand - tile.FireCoverage) +
                    Mathf.Max(0f, demand - tile.EducationCoverage) +
                    Mathf.Max(0f, demand - tile.MedicalCoverage) +
                    Mathf.Max(0f, demand - tile.ConsumptionCoverage) +
                    Mathf.Max(0f, demand - tile.JobCoverage) +
                    Mathf.Max(0f, tile.LeisureDemand - tile.LeisureCoverage) +
                    Mathf.Max(0f, demand - tile.HygieneCoverage) +
                    Mathf.Max(0f, demand - tile.TransitCoverage)) / 12f;
                targetSatisfaction = Mathf.Clamp01(targetSatisfaction - infraDeficit * config.infraDissatisfactionWeight);

                // 도로망과 완전히 끊겼으면(포텐셜 0) 혼잡이 0이라 만족도가 오히려
                // 좋게 나오는 역설이 생긴다 — 고립은 혼잡보다 나쁜 상태이므로
                // 목표 만족도를 바닥으로 강제한다.
                if (tile.Potential <= 0f) targetSatisfaction = 0f;
                // 상수도·하수도·전기는 의식주 수준의 필수 인프라 — 셋 중 하나라도
                // 전혀 안 닿으면(도로 끊김과 동급으로) 도저히 못 사는 곳으로 취급한다.
                else if (!HasEssentialUtilities(tile)) targetSatisfaction = 0f;

                bool hadResidents = tile.Population > 0;
                if (hadResidents)
                {
                    // 이미 사는 사람이 있으면 만족도는 순간값이 아니라 목표치를 향해
                    // 서서히 움직인다 — "입주 처음엔 50%였다가 니즈가 안 채워지면
                    // 서서히 떨어진다"는 요청 그대로.
                    tile.Satisfaction = Mathf.MoveTowards(tile.Satisfaction, targetSatisfaction, config.satisfactionDriftPerTick);
                }

                UpdateResidentialDensityTier(tile, hadResidents ? tile.Satisfaction : 1f);

                if (hadResidents && tile.Satisfaction < config.satisfactionLeaveThreshold)
                {
                    // 만족도가 바닥을 치면 거주자가 떠난다 — 성장이 아니라 이탈.
                    int leaving = Mathf.Max(1, Mathf.RoundToInt(tile.Capacity * config.satisfactionLeaveRatePerTick));
                    tile.Population = Mathf.Max(0, tile.Population - leaving);
                }
                // 조건(목표 만족도)이 이미 이탈 문턱 밑이면 애초에 아무도 새로
                // 이사 오지 않는다 — 이게 없으면 "만족도 바닥→전원 이탈→그런데
                // 조건은 그대로 나쁜데 바로 다음 틱에 '새 입주'로 취급돼 다시
                // 채워짐"이 매 틱 반복돼 건물이 절대 공실로 안 남는 문제가 있었다.
                else if (tile.Population < tile.Capacity && tile.Potential > 0f
                         && targetSatisfaction >= config.satisfactionLeaveThreshold
                         && HasEssentialUtilities(tile))
                {
                    float growSatisfactionFactor = hadResidents ? tile.Satisfaction : 0.5f;
                    float grow = tile.Capacity * tile.Potential * config.populationGrowthRatePerTick
                                 * Mathf.Lerp(0.15f, 1f, growSatisfactionFactor);
                    int growInt = Mathf.Max(1, Mathf.RoundToInt(grow));
                    int newPopulation = Mathf.Min(tile.Capacity, tile.Population + growInt);
                    if (!hadResidents && newPopulation > 0) tile.Satisfaction = 0.5f; // 첫 입주 — 50%에서 출발
                    tile.Population = newPopulation;
                }

                // 아무도 안 사는 채로 너무 오래 방치되면(들어올 사람이 없다는 뜻) 철거한다.
                if (tile.Population <= 0)
                {
                    tile.VacancyStreak++;
                    if (tile.VacancyStreak > config.vacancyDemolishTicks) abandonedResidential.Add(tile);
                }
                else
                {
                    tile.VacancyStreak = 0;
                }
            }

            foreach (var tile in abandonedResidential)
            {
                var coord = tile.Coord;
                Logger.Log($"주거지 철거(공실 방치): {coord}", 1);
                ReleaseFootprint(tile);
                tiles[coord] = new CityTile(coord, TileKind.Empty);
                OnZoneRemoved?.Invoke(coord);
            }

            if (abandonedResidential.Count > 0)
                residentials = residentials.Except(abandonedResidential).ToList();
            // 재개발 이주(§Pass1)는 이미 Pass1 단계에서 옛 칸을 Empty로, 새 칸을
            // Residential로 직접 반영해뒀으므로 — 이 residentials 리스트(Pass1
            // 이후 상태를 그대로 다시 훑은 것)에 이미 정확히 반영돼 있다. 별도
            // 보정 불필요.

            Population = residentials.Sum(t => t.Population);

            UpdateUtilityTiers(residentials, avenueFrontage, entryRoadByCoord, roadDistance);

            // 인프라 시설의 "근무 인원" — 티어가 요구하는 정원 대비, 노동력이
            // 부족하면 실제로 채워진 인원도 그만큼 줄어든다(정보 패널 표시용).
            foreach (var utility in tiles.Values)
            {
                if (!IsUtilityKind(utility.Kind)) continue;
                utility.Capacity = UtilityJobsForTier(utility.Kind, utility.Tier);
                utility.Population = Mathf.RoundToInt(utility.Capacity * LaborAvailability);
            }
        }

        /// <summary>
        /// 도시 전체 일자리 수요(모든 인프라 정원, 공장 포함) 대비 지금 사는
        /// 인구 비율. 일자리가 아예 없으면 1(완전 고용 취급 — 초반에 인프라가
        /// 하나도 없을 때 괜히 페널티가 걸리지 않게).
        /// </summary>
        float ComputeLaborAvailability()
        {
            int jobDemand = 0;
            foreach (var t in tiles.Values)
                if (IsUtilityKind(t.Kind)) jobDemand += UtilityJobsForTier(t.Kind, t.Tier);
            return jobDemand <= 0 ? 1f : Mathf.Clamp01(Population / (float)jobDemand);
        }

        /// <summary>
        /// 각 인프라 시설이 실제로 얼마나 많은 인구를 커버하고 있는지 보고,
        /// 문턱을 넘으면 다음 티어(소형→중형→대형)로 키운다 — "동네 변전소가
        /// 사람이 늘면서 점점 큰 발전소로 진화" 요청 사항. 이제 티어가 오르면
        /// 실제로 칸을 더 차지한다(1x1→1x2→2x2) — 승급하려면 그만큼 빈 땅이
        /// 있어야 하고, 지금 자리에 없으면 공간이 있는 다른 대로변 자리로
        /// 통째로 "이주"한다. 그마저도 없으면(맵이 꽉 참) 승급을 보류하고
        /// 지금 티어에 머문다 — 무한정 커지지 않는다. 공장도 같은 티어
        /// 시스템으로 자란다(생성 규칙만 다름, UpdateFactorySpawning 참고).
        /// </summary>
        void UpdateUtilityTiers(List<CityTile> residentials, HashSet<GridCoord> avenueFrontage, Dictionary<GridCoord, GridCoord?> entryRoadByCoord, Dictionary<GridCoord, int> roadDistance)
        {
            foreach (var utility in tiles.Values.Where(t => IsUtilityKind(t.Kind)).ToList())
            {
                // 이 틱 안에서 이미 다른 시설에 흡수 합병됐으면(스냅샷이라 옛 객체가
                // 리스트에 남아있음) 건너뛴다 — 안 그러면 이미 사라진 유령 시설을
                // 다시 승급/이주시키는 버그가 난다.
                if (!tiles.TryGetValue(utility.Coord, out var current) || !ReferenceEquals(current, utility)) continue;

                int radius = UtilityRadiusForTier(utility.Kind, utility.Tier);
                // "커버되는 인구"도 이제 직선거리가 아니라 도로망 홉 수 기준 —
                // ComputeCoverage와 같은 기준으로 재는 것이라야 "커버리지 표시"와
                // "티어 성장 조건"이 서로 어긋나지 않는다.
                var entryRoad = FindEntryRoad(utility.Coord);
                var flood = entryRoad.HasValue ? FloodRoadDistance(entryRoad.Value, radius) : new Dictionary<GridCoord, int>();
                int servedPopulation = residentials
                    .Where(r => entryRoadByCoord.TryGetValue(r.Coord, out var rr) && rr.HasValue
                        && flood.TryGetValue(rr.Value, out var hops) && hops + 1 <= radius)
                    .Sum(r => r.Population);

                int desiredTier = servedPopulation >= config.utilityTier3Population ? 3
                    : servedPopulation >= config.utilityTier2Population ? 2
                    : 1;

                if (desiredTier <= utility.Tier) continue;

                var footprint = FindFootprintFor(utility.Coord, desiredTier);
                if (footprint != null)
                {
                    ClaimFootprint(utility, footprint);
                    utility.Tier = desiredTier;
                    OnUtilityTierChanged?.Invoke(utility);
                    Logger.Log($"{KindLabel(utility.Kind)} 승급(제자리): {utility.Coord} → {desiredTier}티어 (커버 인구 {servedPopulation}, 발자국 {1 + footprint.Count}칸)", 1);
                    AbsorbNearbyRedundantUtilities(utility);
                    continue;
                }

                var relocationSite = FindRelocationSiteForFootprint(avenueFrontage, desiredTier, utility.Coord, roadDistance);
                if (relocationSite == null)
                {
                    Logger.Log($"{KindLabel(utility.Kind)} 승급 보류(주변에 여유 공간 없음): {utility.Coord}", 2);
                    continue; // 자리가 없으면 지금 티어에서 성장을 멈춘다.
                }

                var relocated = RelocateUtility(utility, relocationSite.Value, desiredTier);
                AbsorbNearbyRedundantUtilities(relocated);
            }
        }

        /// <summary>
        /// 승급으로 반경이 넓어졌으면, 그 반경 안에 들어온 같은 종류의 다른
        /// (더 작은) 시설은 이제 중복이니 흡수 합병한다(철거하고 빈 땅으로) —
        /// "소형 건물 두세 개가 안 합쳐진다"는 요청 대응. 큰 시설 하나가 여럿을
        /// 대체하는 셈이라, 실제 플레이에서는 "공간을 치워주면 승급 → 승급하며
        /// 주변 소형 시설을 흡수"로 자연스럽게 이어진다.
        /// </summary>
        void AbsorbNearbyRedundantUtilities(CityTile survivor)
        {
            int radius = UtilityRadiusForTier(survivor.Kind, survivor.Tier);
            foreach (var other in tiles.Values.Where(t => t.Kind == survivor.Kind && !t.Coord.Equals(survivor.Coord)).ToList())
            {
                if (ManhattanDistance(survivor.Coord, other.Coord) > radius) continue;
                ReleaseFootprint(other);
                tiles[other.Coord] = new CityTile(other.Coord, TileKind.Empty);
                OnZoneRemoved?.Invoke(other.Coord);
                Logger.Log($"{KindLabel(survivor.Kind)} 합병: {other.Coord} → {survivor.Coord}로 흡수", 1);
            }
        }

        /// <summary>
        /// 도로가 철거돼 도로망과 완전히 끊긴 건물(포텐셜 0)은 곧장 사라지지
        /// 않고, 잠깐의 유예 기간 뒤 서서히 인구/활동을 잃다가 완전히 비면
        /// 폐허가 돼 빈 땅으로 돌아간다 — "도로 없어지면 건물도 쇠퇴하다
        /// 없어져야" 요청. 인프라 시설은 "정원이 서서히 준다"는 개념이 안
        /// 맞아서(사람이 사는 게 아니라 시설이니) 유예 기간이 지나면 바로
        /// 폐쇄한다.
        /// </summary>
        void UpdateAbandonment()
        {
            var toAbandon = new List<CityTile>();

            foreach (var tile in tiles.Values)
            {
                if (!IsDevelopedKind(tile.Kind)) continue;

                if (tile.Potential > 0f)
                {
                    tile.IsolatedStreak = 0;
                    continue;
                }

                tile.IsolatedStreak++;
                if (tile.IsolatedStreak <= config.abandonGracePeriodTicks) continue;

                if (IsUtilityKind(tile.Kind))
                {
                    toAbandon.Add(tile);
                    continue;
                }

                int decay = Mathf.Max(1, Mathf.RoundToInt(tile.Capacity * config.abandonDecayRatePerTick));
                tile.Population = Mathf.Max(0, tile.Population - decay);
                if (tile.Population <= 0) toAbandon.Add(tile);
            }

            foreach (var tile in toAbandon)
            {
                var coord = tile.Coord;
                Logger.Log($"{KindLabel(tile.Kind)} 폐허화(도로 단절): {coord}", 1);
                ReleaseFootprint(tile);
                tiles[coord] = new CityTile(coord, TileKind.Empty);
                OnZoneRemoved?.Invoke(coord);
            }
        }

        /// <summary>
        /// 주거지 밀도 재개발(§5.5.3 부활) — 인프라 티어와 같은 패턴을 주거지에도
        /// 적용한다. 만족도가 좋은/나쁜 상태가 연속 N틱 쌓이면 승급/강등한다
        /// (순간값 하나로 바로 판정하면 깜빡거리므로 디바운스). 승급하려면
        /// 실제로 옆 칸(1x2)이나 2x2 블록을 차지할 빈 땅이 있어야 한다 — 제자리에
        /// 없으면 인프라 시설과 같은 원리로 발자국 전체를 놓을 수 있는 다른
        /// 대로변 자리로 통째로 이주해서라도 진화한다(거주자·재산·만족도를
        /// 그대로 데리고 이사). 이주할 자리조차 없으면 스트릭을 유지한 채
        /// 다음 틱에 계속 재시도한다. 강등되면 확장 칸을 반납하고, 줄어든
        /// 정원을 넘는 인구는 그 자리에서 정리된다.
        /// </summary>
        void UpdateResidentialDensityTier(CityTile tile, float satisfaction)
        {
            if (satisfaction >= config.redevelopGoodSatisfaction)
            {
                tile.GoodStreak++;
                tile.BadStreak = 0;
            }
            else if (satisfaction <= config.redevelopBadSatisfaction)
            {
                tile.BadStreak++;
                tile.GoodStreak = 0;
            }
            else
            {
                tile.GoodStreak = 0;
                tile.BadStreak = 0;
            }

            if (tile.GoodStreak >= config.redevelopStreakTicks && tile.Tier < 3)
            {
                int nextTier = tile.Tier + 1;
                var footprint = FindFootprintFor(tile.Coord, nextTier);
                if (footprint == null)
                {
                    // 제자리에 공간이 없으면 여기선 그냥 보류한다 — 이주를 통한
                    // 승급은 Pass1 단계에서 이미 시도됐다(FindResidentialRelocationSiteIfNeeded
                    // + Pass1의 사전예약 매칭). 거기서도 자리를 못 찾았다는 뜻이므로
                    // 다음 틱에 다시 시도(스트릭 유지).
                    Logger.Log($"주거지 재개발 보류(공간 부족): {tile.Coord}", 2);
                    return;
                }

                ClaimFootprint(tile, footprint);
                tile.Tier = nextTier;
                tile.IncomeTier = IncomeTierFor(tile.Wealth);
                tile.Capacity = ResidentialCapacityFor(tile.Tier, tile.IncomeTier);
                tile.GoodStreak = 0;
                Logger.Log($"주거지 재개발(승급): {tile.Coord} → {tile.Tier}티어/{tile.IncomeTier} (정원 {tile.Capacity}, 발자국 {1 + footprint.Count}칸)", 1);
            }
            else if (tile.BadStreak >= config.redevelopStreakTicks && tile.Tier > 1)
            {
                ReleaseFootprint(tile);
                tile.Tier--;
                tile.IncomeTier = IncomeTierFor(tile.Wealth);
                tile.Capacity = ResidentialCapacityFor(tile.Tier, tile.IncomeTier);
                tile.Population = Mathf.Min(tile.Population, tile.Capacity);
                tile.BadStreak = 0;
                Logger.Log($"주거지 쇠퇴(강등): {tile.Coord} → {tile.Tier}티어/{tile.IncomeTier} (정원 {tile.Capacity})", 1);
            }
        }

        /// <summary>재산(연속값)을 소득 계층 3단계로 나눈다 — 정보 패널의 "재산 수준: 상/중/하"
        /// 표시와 같은 경계값(0.34/0.67)을 쓴다(다르면 화면 표시와 실제 정원 판정이 어긋난다).</summary>
        static IncomeTier IncomeTierFor(float wealth) => wealth >= 0.67f ? IncomeTier.High : wealth >= 0.34f ? IncomeTier.Middle : IncomeTier.Low;

        /// <summary>밀도 티어(1~3)×소득 계층 3×3 표에서 정원을 찾는다(§5.5.3).</summary>
        int ResidentialCapacityFor(int densityTier, IncomeTier income) => densityTier switch
        {
            2 => income switch
            {
                IncomeTier.Low => config.residentialTier2LowCapacity,
                IncomeTier.Middle => config.residentialTier2MiddleCapacity,
                _ => config.residentialTier2HighCapacity,
            },
            3 => income switch
            {
                IncomeTier.Low => config.residentialTier3LowCapacity,
                IncomeTier.Middle => config.residentialTier3MiddleCapacity,
                _ => config.residentialTier3HighCapacity,
            },
            _ => income switch
            {
                IncomeTier.Low => config.residentialTier1LowCapacity,
                IncomeTier.Middle => config.residentialTier1MiddleCapacity,
                _ => config.residentialTier1HighCapacity,
            },
        };

        /// <summary>
        /// 공장이 필요한지(다른 모든 인프라·상점 정원 합보다 인구가 많은지 =
        /// 일자리가 실제로 부족한지, 그리고 아직 공장이 없는지) 판단하고,
        /// 필요하면 "제일 싼 땅"(주변 평균 재산 수준이 가장 낮은 대로변)을
        /// 미리 찾아 돌려준다 — "저학력·저수준 잉여 노동력이 일하는 공간"이라는
        /// 설정 그대로. Pass1 루프가 시작되기 전에 미리 불러야 한다: Pass1이
        /// 새로 열리는 대로변 빈 땅을 그 틱 안에서 곧장 주거지로 채워버리므로,
        /// 나중에 찾으면 이미 자리가 없다. 반환된 좌표는 Pass1이 그 칸에
        /// 도달했을 때 실제로 공장으로 배정한다(SpawnAndGrowZones 참고).
        /// </summary>
        (bool needed, GridCoord? site) FindFactorySiteIfNeeded(Dictionary<GridCoord, int> roadDistance)
        {
            bool alreadyHasFactory = tiles.Values.Any(t => t.Kind == TileKind.UtilityFactory);
            if (alreadyHasFactory) return (false, null);

            int nonFactoryJobCapacity = tiles.Values
                .Where(t => IsUtilityKind(t.Kind) && t.Kind != TileKind.UtilityFactory)
                .Sum(t => UtilityJobsForTier(t.Kind, t.Tier));

            bool jobsShortfall = Population > nonFactoryJobCapacity;
            if (!jobsShortfall) return (false, null); // 일자리가 이미 충분하면 공장은 생기지 않는다.

            var residentials = tiles.Values.Where(t => t.Kind == TileKind.Residential).ToList();

            GridCoord? cheapestSite = null;
            float cheapestValue = float.MaxValue;
            foreach (var t in tiles.Values)
            {
                if (t.Kind != TileKind.Empty || !HasAvenueFrontage(t.Coord)) continue;
                if (ComputePotential(t.Coord, roadDistance) < config.residentialPotentialThreshold) continue;
                float value = AverageNearbyWealth(t.Coord, config.industrialLandValueRadius, residentials);
                if (value >= cheapestValue) continue;
                cheapestValue = value;
                cheapestSite = t.Coord;
            }
            return (true, cheapestSite);
        }

        bool HasAvenueFrontage(GridCoord c) =>
            GridCoord.Neighbors4(c).Any(n => roads.TryGetValue(n, out var r) && r.IsTraversable && r.Type.tier == RoadTier.Avenue);

        float ComputePotential(GridCoord c, Dictionary<GridCoord, int> roadDistance)
        {
            int? accessDistance = null;
            foreach (var n in GridCoord.Neighbors4(c))
                if (roadDistance.TryGetValue(n, out var d) && (accessDistance == null || d + 1 < accessDistance))
                    accessDistance = d + 1;
            return accessDistance.HasValue ? Mathf.Clamp01(1f - accessDistance.Value / (float)config.maxUsefulDistance) : 0f;
        }

        float AverageNearbyWealth(GridCoord coord, int radius, List<CityTile> residentials)
        {
            var nearby = residentials.Where(r => ManhattanDistance(coord, r.Coord) <= radius).ToList();
            return nearby.Count > 0 ? nearby.Average(r => r.Wealth) : 0f;
        }

        /// <summary>
        /// 시설 종류별 입지 성향 — PIMFY(+1, "우리 동네에 있었으면")는 비싼 땅을
        /// 우선 찾는다. NORMAL(0)은 땅값을 안 본다(결핍만 본다, 예전 방식 그대로).
        ///
        /// NIMBY(싼 땅 우선)는 의도적으로 여기(8종 결핍-우선 시설) 어디에도 없다 —
        /// 처음엔 하수도·발전소를 NIMBY로 뒀었는데, 헤드리스로 돌려보니 "싼 땅"이
        /// 이 게임에서는 거의 항상 "관문(이웃 도시)에서 먼 땅"과 같은 말이라, 정작
        /// 커버해야 할 주거지 대부분(관문 근처에 몰려있음)이 반경 밖으로 밀려나
        /// 결핍이 영원히 안 풀리는 악순환에 빠졌다(같은 종류가 계속 재평가돼 반복
        /// 강제 전환되는 버그로 드러남) — 특히 상수도·하수도·전기는 입주 자체를
        /// 막는 필수 게이트라 이 실패가 도시 전체를 마비시킨다. 그래서 "커버리지
        /// 범위가 실제로 중요한" 이 8종은 전부 NORMAL(치안·소방·교육·의료·소비)
        /// 아니면 PIMFY(상점·의료·교육 — 비싼 땅은 대개 관문에서 가까워 커버리지에
        /// 오히려 유리하다)로만 두고, "어디 있든 상관없는"(도달거리 개념 자체가
        /// 없는, 도시 전체 집계인) 공장만 전용 로직(FindFactorySiteIfNeeded)에서
        /// NIMBY로 다룬다.
        /// </summary>
        static float LandValuePreference(TileKind kind) => kind switch
        {
            TileKind.UtilityStore or TileKind.UtilityMedical or TileKind.UtilityEducation or TileKind.UtilityPark => 1f,
            TileKind.UtilityGarbage => -1f, // 소프트 니즈라 NIMBY로 둬도 안전(필수 3종과 달리 입주를 막지 않으므로 악순환에 안 빠짐).
            _ => 0f,
        };

        /// <summary>
        /// NIMBY/PIMFY 종류가 이번 틱 우선순위로 뽑혔을 때, 새로 열리는 빈 땅 중
        /// 땅값 기준으로 제일 적합한 칸을 미리 찾아둔다(공장의 FindFactorySiteIfNeeded와
        /// 같은 패턴 — Pass1이 "아무 칸이나 먼저 만나는 대로" 채워버리기 전에 선점).
        /// NORMAL 종류면 null(선점 없음 — 예전처럼 첫 번째로 만나는 칸에 배치).
        /// </summary>
        GridCoord? FindLandValueSite(TileKind kind, Dictionary<GridCoord, int> roadDistance)
        {
            float pref = LandValuePreference(kind);
            if (pref == 0f) return null;

            var residentials = tiles.Values.Where(t => t.Kind == TileKind.Residential).ToList();

            GridCoord? best = null;
            float bestValue = pref < 0 ? float.MaxValue : float.MinValue;
            foreach (var t in tiles.Values)
            {
                if (t.Kind != TileKind.Empty || !HasAvenueFrontage(t.Coord)) continue;
                if (ComputePotential(t.Coord, roadDistance) < config.residentialPotentialThreshold) continue;
                float value = AverageNearbyWealth(t.Coord, config.industrialLandValueRadius, residentials);
                bool better = pref < 0 ? value < bestValue : value > bestValue;
                if (!better) continue;
                bestValue = value;
                best = t.Coord;
            }
            return best;
        }

        /// <summary>
        /// 지금 존재하는(이번 틱 Pass1이 시작되기 전 기준) 같은 종류 시설들이
        /// 이미 커버하고 있는 도로 칸 전체(합집합) — "담당구역 안엔 또 안
        /// 짓는다"는 판단에 쓴다. 시설 하나당 자기 반경까지 플러드필해서
        /// 합치므로, 시설 개수에만 비례하는 비용이라 가볍다.
        /// </summary>
        HashSet<GridCoord> ComputeSameKindCoveredRoads(TileKind kind)
        {
            var covered = new HashSet<GridCoord>();
            foreach (var existing in tiles.Values.Where(t => t.Kind == kind))
            {
                var entry = FindEntryRoad(existing.Coord);
                if (!entry.HasValue) continue;
                int radius = UtilityRadiusForTier(kind, existing.Tier);
                foreach (var c in FloodRoadDistance(entry.Value, radius).Keys) covered.Add(c);
            }
            return covered;
        }

        // ── 발자국(1x1/1x2/2x2) 점유 ──────────────────────────────────────

        bool IsClaimable(GridCoord c) => InBounds(c) && tiles.TryGetValue(c, out var t) && t.Kind == TileKind.Empty;

        /// <summary>목표 티어(1~3)에 필요한 발자국을 원점 기준으로 찾는다(원점 제외한 추가 칸들) — 실패하면 null. 1티어는 항상 빈 리스트(추가 칸 불필요).</summary>
        List<GridCoord> FindFootprintFor(GridCoord origin, int tier)
        {
            if (tier <= 1) return new List<GridCoord>();

            if (tier == 2)
            {
                foreach (var (dx, dy) in Orthogonal4Offsets)
                {
                    var c = new GridCoord(origin.X + dx, origin.Y + dy);
                    if (IsClaimable(c)) return new List<GridCoord> { c };
                }
                return null;
            }

            foreach (var offsets in Square2x2Offsets)
            {
                var candidates = offsets.Select(o => new GridCoord(origin.X + o.dx, origin.Y + o.dy)).ToList();
                if (candidates.All(IsClaimable)) return candidates;
            }
            return null;
        }

        /// <summary>확장 칸들을 실제로 점유(Extension으로 마킹)한다. 기존에 갖고 있던 확장은 먼저 반납한다.</summary>
        void ClaimFootprint(CityTile owner, List<GridCoord> extensions)
        {
            ReleaseFootprint(owner);
            owner.FootprintExtensions = extensions;
            foreach (var c in extensions)
                tiles[c] = new CityTile(c, TileKind.Extension) { OwnerCoord = owner.Coord };
        }

        /// <summary>이 건물이 차지하고 있던 확장 칸들을 전부 빈 땅으로 되돌린다.</summary>
        void ReleaseFootprint(CityTile owner)
        {
            if (owner.FootprintExtensions.Count == 0) return;
            foreach (var c in owner.FootprintExtensions)
            {
                if (tiles.TryGetValue(c, out var t) && t.Kind == TileKind.Extension)
                {
                    tiles[c] = new CityTile(c, TileKind.Empty);
                    OnZoneRemoved?.Invoke(c);
                }
            }
            owner.FootprintExtensions = new List<GridCoord>();
        }

        /// <summary>
        /// 목표 티어의 발자국을 통째로 놓을 수 있는 빈 땅을 찾는다(이주용).
        /// 인프라는 대로변 전용이라 requireAvenue=true(기본값)로 쓰고, 주거지는
        /// 상업·공업지구 개념이 없어 도로 등급을 안 가리므로(§5.8) false로 쓴다.
        ///
        /// Potential은 캐시된 tile.Potential 필드가 아니라 roadDistance로 매번
        /// 새로 계산한다(ComputePotential) — 공장·NIMBY/PIMFY와 같은 이유로
        /// Pass1이 시작되기 전에 호출될 수 있는데, 그 시점엔 tile.Potential이
        /// (이번 틱 Pass1이 아직 안 돌았으니) 죽 갱신 안 된 값이다. 특히 방금
        /// 막 비워진 칸이나 도로가 막 닿은 칸은 캐시값이 아예 0(기본값)이라,
        /// 캐시를 썼더니 "이주할 곳이 하나도 없다"고 잘못 판단하는 버그가
        /// 실제로 났었다.
        /// </summary>
        GridCoord? FindRelocationSiteForFootprint(HashSet<GridCoord> avenueFrontage, int desiredTier, GridCoord avoid, Dictionary<GridCoord, int> roadDistance, bool requireAvenue = true)
        {
            foreach (var t in tiles.Values)
            {
                if (t.Kind != TileKind.Empty || t.Coord.Equals(avoid)) continue;
                if (ComputePotential(t.Coord, roadDistance) < config.residentialPotentialThreshold) continue;
                if (requireAvenue && !avenueFrontage.Contains(t.Coord)) continue;
                if (FindFootprintFor(t.Coord, desiredTier) != null) return t.Coord;
            }
            return null;
        }

        /// <summary>인프라 종류별 정원(티어당) — 상점·공장은 실제 고용 규모를 흉내내 훨씬 크게 잡는다.</summary>
        int UtilityJobsForTier(TileKind kind, int tier)
        {
            if (kind == TileKind.UtilityStore || kind == TileKind.UtilityFactory)
                return 15 + (tier - 1) * 25;
            return 3 + (tier - 1) * 3;
        }

        /// <summary>기존 자리를 빈 땅으로 되돌리고, 새 자리를 승급된 티어(및 그 발자국)로 채운다 — "타일을 벗어나는 대신 공간이 있는 곳으로 이주". 새 타일을 돌려준다.</summary>
        CityTile RelocateUtility(CityTile utility, GridCoord newCoord, int newTier)
        {
            var oldCoord = utility.Coord;
            var kind = utility.Kind;

            ReleaseFootprint(utility);
            tiles[oldCoord] = new CityTile(oldCoord, TileKind.Empty);
            OnZoneRemoved?.Invoke(oldCoord);

            var newTile = tiles[newCoord];
            newTile.Kind = kind;
            newTile.Tier = newTier;
            var footprint = FindFootprintFor(newCoord, newTier);
            if (footprint != null) ClaimFootprint(newTile, footprint);
            OnUtilitySpawned?.Invoke(newTile);
            Logger.Log($"{KindLabel(kind)} 이주: {oldCoord} → {newCoord} ({newTier}티어, 공간 부족으로 이전)", 1);
            return newTile;
        }

        /// <summary>
        /// 주거지가 제자리에 승급할 공간이 없을 때, 발자국 전체를 놓을 수 있는
        /// 다른 대로변 빈 땅으로 통째로 이주시킨다(인프라 이주와 같은 원리) —
        /// 거주자 수·재산·만족도·필지 크기는 그대로 데리고 간다("이사"이지
        /// "새 입주"가 아니므로). 목적지가 그 사이 다른 이주 요청에 선점됐으면
        /// (같은 틱에 여러 곳이 같은 최적 후보를 찾을 수 있음) null을 돌려주고
        /// 원래 자리는 그대로 둔다 — 다음 틱에 스트릭이 유지된 채 다시 시도된다.
        /// </summary>
        /// <summary>
        /// 승급하고 싶은데(GoodStreak 충분) 제자리엔 공간이 없는 주거지가 있으면,
        /// 발자국 전체를 놓을 수 있는 다른 빈 땅을 찾아 (원본 타일, 목적지 좌표,
        /// 목표 티어)로 돌려준다 — Pass1이 새로 열리는 빈 땅을 곧장 다른 용도로
        /// 채워버리기 전에 먼저 선점해야 하므로 Pass1 이전에 호출된다(공장·
        /// NIMBY/PIMFY와 같은 이유). 여러 후보가 있어도 한 틱에 하나만 처리—
        /// 찾은 첫 번째로 확정한다.
        /// </summary>
        (CityTile source, GridCoord targetCoord, int targetTier)? FindResidentialRelocationSiteIfNeeded(Dictionary<GridCoord, int> roadDistance)
        {
            foreach (var t in tiles.Values)
            {
                if (t.Kind != TileKind.Residential || t.Tier >= 3 || t.GoodStreak < config.redevelopStreakTicks) continue;
                int nextTier = t.Tier + 1;
                if (FindFootprintFor(t.Coord, nextTier) != null) continue; // 제자리에 공간 있음 — 이주 필요 없음, 일반 로직이 처리.
                var site = FindRelocationSiteForFootprint(EmptyAvenueFrontageFallback, nextTier, t.Coord, roadDistance, requireAvenue: false);
                if (site.HasValue) return (t, site.Value, nextTier);
            }
            return null;
        }

        static readonly HashSet<GridCoord> EmptyAvenueFrontageFallback = new HashSet<GridCoord>();

        static bool IsEssentialUtilityKind(TileKind kind) =>
            kind == TileKind.UtilityWater || kind == TileKind.UtilitySewage || kind == TileKind.UtilityPower;

        int UtilityRadiusForTier(TileKind kind, int tier) => IsEssentialUtilityKind(kind)
            ? config.essentialUtilityRadius + (tier - 1) * config.essentialUtilityRadiusPerTierBonus
            : config.utilityRadius + (tier - 1) * config.utilityRadiusPerTierBonus;

        static int ManhattanDistance(GridCoord a, GridCoord b) => Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);

        /// <summary>
        /// entryRoad에서 시작해 도로망을 따라 각 도로 칸까지 홉 수를 잰다
        /// (entryRoad 자체가 0홉). maxHops를 넘는 곳은 아예 방문하지 않는다 —
        /// 인프라 개수 × 반경만큼만 도는 유계 BFS라 주거지 수와 무관하게 싸다.
        /// 직선거리(맨해튼)가 아니라 **실제로 도로가 이어져야만** 도달로 치므로,
        /// "반경 안에 있지만 도로가 안 뚫려서 못 미치는" 상황이 처음으로 표현된다.
        /// </summary>
        Dictionary<GridCoord, int> FloodRoadDistance(GridCoord entryRoad, int maxHops)
        {
            var dist = new Dictionary<GridCoord, int>();
            if (!roads.TryGetValue(entryRoad, out var r0) || !r0.IsTraversable) return dist;

            dist[entryRoad] = 0;
            var queue = new Queue<GridCoord>();
            queue.Enqueue(entryRoad);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                int d = dist[cur];
                if (d >= maxHops) continue;
                foreach (var n in GridCoord.Neighbors4(cur))
                {
                    if (dist.ContainsKey(n)) continue;
                    if (!roads.TryGetValue(n, out var nr) || !nr.IsTraversable) continue;
                    dist[n] = d + 1;
                    queue.Enqueue(n);
                }
            }
            return dist;
        }

        /// <summary>건물 c에 인접한 도로 칸 하나(있다면) — 대로변 전용 배치인 인프라는 항상 있어야 정상이다.</summary>
        public GridCoord? FindEntryRoad(GridCoord c)
        {
            foreach (var n in GridCoord.Neighbors4(c))
                if (roads.TryGetValue(n, out var r) && r.IsTraversable) return n;
            return null;
        }

        /// <summary>
        /// 두 도로 칸 사이의 경로(양 끝 포함) — 이웃 도시로 향하는 고정 트리
        /// (LastRoadParent)와 달리, 건물↔건물 업무 통행(가게→주거지 배달 등)처럼
        /// 임의의 두 지점 사이를 잇는 용도라 그때그때 새로 BFS한다. 도달 불가하면
        /// 빈 리스트.
        /// </summary>
        public List<GridCoord> FindRoadPath(GridCoord from, GridCoord to)
        {
            if (from.Equals(to)) return new List<GridCoord> { from };

            var cameFrom = new Dictionary<GridCoord, GridCoord>();
            var visited = new HashSet<GridCoord> { from };
            var queue = new Queue<GridCoord>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (cur.Equals(to)) break;
                foreach (var n in GridCoord.Neighbors4(cur))
                {
                    if (visited.Contains(n)) continue;
                    if (!roads.TryGetValue(n, out var r) || !r.IsTraversable) continue;
                    visited.Add(n);
                    cameFrom[n] = cur;
                    queue.Enqueue(n);
                }
            }

            if (!visited.Contains(to)) return new List<GridCoord>();

            var path = new List<GridCoord> { to };
            var cursor = to;
            while (!cursor.Equals(from))
            {
                cursor = cameFrom[cursor];
                path.Add(cursor);
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// 인프라 시설도 사람이 있어야 돌아간다 — 노동력이 모자라면(LaborAvailability)
        /// 커버리지 자체가 그 비율만큼 깎인다. 거리는 직선(맨해튼)이 아니라 도로망을
        /// 실제로 타고 가는 홉 수 — instances는 시설별로 이미 계산해둔 플러드 결과.
        /// </summary>
        float ComputeCoverage(GridCoord? residentEntryRoad, List<(CityTile utility, Dictionary<GridCoord, int> flood, int radius)> instances)
        {
            if (!residentEntryRoad.HasValue) return 0f;

            float best = 0f;
            foreach (var (utility, flood, radius) in instances)
            {
                if (!flood.TryGetValue(residentEntryRoad.Value, out var hops)) continue;
                // flood는 시설의 진입로 기준 홉 수라, 주거지 쪽 마지막 한 칸(도로→건물)을 더해야
                // "건물 대 건물" 실거리에 가깝다.
                int totalHops = hops + 1;
                if (totalHops > radius) continue;
                // 상수도·하수도·전기는 노동시장(LaborAvailability)과 무관하게 항상
                // 정상 가동한다고 친다 — 안 그러면 "인구 0 → 일자리 수요만 생겨
                // LaborAvailability 0 → 커버리지 0 → 필수 인프라 게이트 때문에
                // 아무도 못 들어옴 → 인구 영원히 0" 순환 교착에 빠진다(실제로
                // 헤드리스 테스트에서 재현됨). 의식주 필수 시설이 도시 노동시장
                // 형편에 좌우된다는 것도 애초에 이상하다 — 그냥 상시 가동으로 취급.
                float laborFactor = IsEssentialUtilityKind(utility.Kind) ? 1f : LaborAvailability;
                // 상수도·하수도·전기는 의식주 필수라 "닿으면 100, 안 닿으면 0"으로
                // 극단적으로 취급한다 — 거리 비례 그라데이션은 소프트 니즈에나
                // 어울리지, "물이 반만 나온다"는 개념 자체가 이상하다. 게다가
                // 어차피 같은 칸을 두 개가 중복으로 짓지 못하게(§위 커버 범위
                // 확인) 막아뒀으니 "여러 개를 지어 그라데이션을 메운다"는 시나리오
                // 자체가 없다.
                float rawCoverage = IsEssentialUtilityKind(utility.Kind) ? 1f : Mathf.Clamp01(1f - totalHops / (float)radius);
                float coverage = rawCoverage * laborFactor;
                if (coverage > best) best = coverage;
            }
            return best;
        }

        /// <summary>상수도·하수도·전기 중 하나라도 전혀 안 닿으면(커버리지 0) 거주 불가로 친다 — "의식주 필수" 요청.</summary>
        static bool HasEssentialUtilities(CityTile tile) =>
            tile.WaterCoverage > 0f && tile.SewageCoverage > 0f && tile.PowerCoverage > 0f;

        /// <summary>
        /// 주거지 전체 평균으로 인프라 각 종류별(공장·일자리 제외) 결핍치를
        /// 재보고, 가장 심한 종류를 돌려준다(요청: "부족한 시설 우선으로").
        /// 결핍이 기준치 미만이면 null — "지금은 딱히 인프라가 급하지 않다"는 뜻.
        /// 상수도·하수도·전기는 의식주 필수라 **항상 최우선**이다 — 이 셋 중
        /// 하나라도 결핍이 있으면 나머지 5종(치안·소방·교육·의료·소비)은
        /// 아예 고려하지 않는다("맨 처음 도로를 깔면 이 셋부터 들어와야" 요청).
        /// </summary>
        (TileKind? kind, float deficit) FindWorstInfraDeficit()
        {
            // 부트스트랩: "결핍 평균"은 주거지가 있어야 계산되는데, 그러면 도로를
            // 막 깔았을 때(주거지 0채)는 결핍이 항상 0으로 잡혀 아래 residentials.Count==0
            // 분기로 곧장 null을 돌려주고, Pass1은 대로변 빈 땅을 전부 그냥
            // 주거지로 채워버린다 — "맨 처음 도로를 깔면 상수도·하수도·전기부터
            // 들어와야" 요청과 순서가 뒤집힌다(실제로 겪음: 주거지가 먼저 생기고
            // 몇 틱 뒤에야 그중 일부가 강제 철거·전환되는 식으로 뒤늦게 맞춰짐).
            // 그래서 주거지 유무와 무관하게, 의식주 필수 3종 중 도시 전체에
            // 하나도 없는 게 있으면 그것부터 최우선(결핍 1=최댓값)으로 본다.
            if (!tiles.Values.Any(t => t.Kind == TileKind.UtilityWater)) return (TileKind.UtilityWater, 1f);
            if (!tiles.Values.Any(t => t.Kind == TileKind.UtilitySewage)) return (TileKind.UtilitySewage, 1f);
            if (!tiles.Values.Any(t => t.Kind == TileKind.UtilityPower)) return (TileKind.UtilityPower, 1f);

            var residentials = tiles.Values.Where(t => t.Kind == TileKind.Residential).ToList();
            if (residentials.Count == 0) return (null, 0f);

            float Deficit(Func<CityTile, float> coverage) =>
                residentials.Average(t => Mathf.Max(0f, t.InfraDemand - coverage(t)));

            var essentialOptions = new[]
            {
                (kind: TileKind.UtilityWater, deficit: Deficit(t => t.WaterCoverage)),
                (kind: TileKind.UtilitySewage, deficit: Deficit(t => t.SewageCoverage)),
                (kind: TileKind.UtilityPower, deficit: Deficit(t => t.PowerCoverage)),
            };
            var worstEssential = essentialOptions.OrderByDescending(o => o.deficit).First();
            if (worstEssential.deficit >= config.infraPriorityThreshold)
                return (worstEssential.kind, worstEssential.deficit);

            var options = new[]
            {
                (kind: TileKind.UtilityPolice, deficit: Deficit(t => t.PoliceCoverage)),
                (kind: TileKind.UtilityFire, deficit: Deficit(t => t.FireCoverage)),
                (kind: TileKind.UtilityEducation, deficit: Deficit(t => t.EducationCoverage)),
                (kind: TileKind.UtilityMedical, deficit: Deficit(t => t.MedicalCoverage)),
                (kind: TileKind.UtilityStore, deficit: Deficit(t => t.ConsumptionCoverage)),
                (kind: TileKind.UtilityPark, deficit: residentials.Average(t => Mathf.Max(0f, t.LeisureDemand - t.LeisureCoverage))),
                (kind: TileKind.UtilityGarbage, deficit: Deficit(t => t.HygieneCoverage)),
            };

            var worst = options.OrderByDescending(o => o.deficit).First();
            return worst.deficit >= config.infraPriorityThreshold ? (worst.kind, worst.deficit) : ((TileKind?)null, worst.deficit);
        }

        static string KindLabel(TileKind k) => k switch
        {
            TileKind.Residential => "주거지",
            TileKind.UtilityWater => "상수도",
            TileKind.UtilitySewage => "하수처리장",
            TileKind.UtilityPower => "발전소",
            TileKind.UtilityPolice => "치안시설",
            TileKind.UtilityFire => "소방시설",
            TileKind.UtilityEducation => "학교",
            TileKind.UtilityMedical => "병원",
            TileKind.UtilityStore => "상점",
            TileKind.UtilityFactory => "공장",
            TileKind.UtilityPark => "공원",
            TileKind.UtilityGarbage => "쓰레기장",
            _ => k.ToString(),
        };

        void RunTraffic(Dictionary<GridCoord, int> roadDistance, Dictionary<GridCoord, GridCoord?> roadParent)
        {
            foreach (var road in roads.Values) road.ResetTripLoad();

            foreach (var tile in tiles.Values)
            {
                if (tile.Kind != TileKind.Residential || tile.Population <= 0) continue;

                GridCoord? entryRoad = null;
                int bestDistance = int.MaxValue;
                foreach (var n in GridCoord.Neighbors4(tile.Coord))
                {
                    if (roadDistance.TryGetValue(n, out var d) && d < bestDistance)
                    {
                        bestDistance = d;
                        entryRoad = n;
                    }
                }
                if (entryRoad == null) continue;

                int trips = Mathf.Max(1, tile.Population / 5);
                GridCoord? cursor = entryRoad;
                while (cursor.HasValue)
                {
                    if (roads.TryGetValue(cursor.Value, out var r))
                        for (int i = 0; i < trips; i++) r.AddTrip();
                    cursor = roadParent.TryGetValue(cursor.Value, out var p) ? p : null;
                }
            }
        }

        void RunEconomy()
        {
            float roadUpkeep = roads.Values.Where(r => r.IsTraversable)
                .Sum(r => r.Type.tier == RoadTier.Avenue ? config.roadUpkeepPerTilePerTick * 2f : config.roadUpkeepPerTilePerTick);

            // 상점·공장의 근무 인원을 "사업 활동"으로 쳐서 세율을 더 얹는다 —
            // 예전 상업·공업지구가 하던 세수 역할을 이제 이 둘이 대신한다.
            int businessActivity = tiles.Values
                .Where(t => t.Kind == TileKind.UtilityStore || t.Kind == TileKind.UtilityFactory)
                .Sum(t => t.Population);

            float income = Population * config.taxPerCapitaPerTick + businessActivity * config.taxPerCapitaPerTick * 1.5f;
            AddMoney(Mathf.RoundToInt(income - roadUpkeep));
        }
    }
}
