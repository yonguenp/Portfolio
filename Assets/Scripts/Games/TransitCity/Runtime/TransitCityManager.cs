using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace TransitCity
{
    /// <summary>
    /// 트랜짓시티 프로토타입의 진입점 — 매니저는 싱글톤(§18). 시뮬레이션 코어
    /// (CityGridModel)와 비주얼/입력을 잇는 얇은 접착 레이어 역할만 한다.
    /// </summary>
    public class TransitCityManager : MonoBehaviour
    {
        public static TransitCityManager Instance { get; private set; }

        [SerializeField] BalanceConfig config;
        /// <summary>도로 계층 구분(소로/대로)을 없애고 "도로 건설" 하나로 합쳤다 — 이 필드가 그 유일한 도로 종류.</summary>
        [SerializeField] RoadTypeData avenueRoadType;
        [SerializeField] Transform gridRoot;
        [SerializeField] Camera worldCamera;

        [Header("프리팹 (씬/에셋에 미리 있는 것만 Instantiate)")]
        [SerializeField] RoadVisual roadVisualPrefab;
        [SerializeField] HouseVisual houseVisualPrefab;
        [SerializeField] GameObject neighborCityTilePrefab;
        [SerializeField] CarVisual carVisualPrefab;
        [SerializeField] PersonVisual personVisualPrefab;

        /// <summary>도로 비주얼의 완공 높이(로컬) — 차량이 그 위를 달리게 하는 기준점으로도 쓴다.</summary>
        const float RoadTopHeight = 0.5f;

        public CityGridModel Model { get; private set; }
        public CommandInvoker Invoker { get; private set; }
        public RoadTypeData RoadType => avenueRoadType;
        public BalanceConfig Config => config;
        public bool IsPaused { get; private set; }

        /// <summary>Inspect 모드에서 개발된 칸(주거/상업/공업/인프라)을 클릭하면 발생 — UI가 정보 패널을 채우는 데 쓴다.</summary>
        public event Action<CityTile> OnBuildingSelected;
        /// <summary>빈 땅/도로를 클릭했거나 선택 중이던 건물이 철거됐을 때 발생.</summary>
        public event Action OnBuildingDeselected;
        /// <summary>2클릭 도로 건설의 시작점이 정해지거나(값) 취소/완료되면(null) 발생 — UI 힌트용.</summary>
        public event Action<GridCoord?> OnPendingRoadStartChanged;
        /// <summary>Inspect 모드에서 교차로(도로 3방향 이상)를 클릭하면 발생 — UI가 로터리/신호등 선택 패널을 띄우는 데 쓴다.</summary>
        public event Action<GridCoord, RoadCell> OnIntersectionSelected;

        readonly Dictionary<GridCoord, RoadVisual> roadVisuals = new Dictionary<GridCoord, RoadVisual>();
        readonly Dictionary<GridCoord, HouseVisual> houseVisuals = new Dictionary<GridCoord, HouseVisual>();
        readonly List<CarVisual> activeCommuterCars = new List<CarVisual>();
        readonly List<CarVisual> activePatrolCars = new List<CarVisual>();
        readonly Dictionary<GridCoord, CarVisual> tileOccupants = new Dictionary<GridCoord, CarVisual>();
        readonly List<PersonVisual> activePeople = new List<PersonVisual>();

        UIDocument hudDocument;
        SimpleObjectPool<CarVisual> carPool;
        SimpleObjectPool<PersonVisual> personPool;
        EditMode currentMode = EditMode.Inspect;
        GridCoord? lastPaintedCoord;
        GridCoord? pendingRoadStart;
        float carRideHeight;

        readonly List<GameObject> ghostObjects = new List<GameObject>();
        Material ghostMaterial;
        GridCoord? lastGhostHoverCoord;
        static readonly Color GhostBlockedColor = new Color(0.9f, 0.25f, 0.25f, 0.45f);
        static readonly Color GhostRoadColor = new Color(0.55f, 0.55f, 0.95f, 0.45f);

        readonly List<GameObject> coverageMarkers = new List<GameObject>();
        GridCoord? selectedCoord;
        static readonly Color CoverageRingColor = new Color(1f, 0.95f, 0.4f, 0.85f);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            hudDocument = UnityEngine.Object.FindFirstObjectByType<UIDocument>();

            Model = new CityGridModel(config);
            Invoker = new CommandInvoker();
            carPool = new SimpleObjectPool<CarVisual>(carVisualPrefab, gridRoot);
            carRideHeight = RoadTopHeight + carVisualPrefab.transform.localScale.y * 0.5f;
            if (personVisualPrefab != null) personPool = new SimpleObjectPool<PersonVisual>(personVisualPrefab, gridRoot);

            Model.OnRoadBuilt += HandleRoadBuilt;
            Model.OnRoadRemoved += HandleRoadRemoved;
            Model.OnZoneSpawned += HandleZoneSpawned;
            Model.OnZoneRemoved += HandleZoneRemoved;
            Model.OnUtilitySpawned += HandleUtilitySpawned;
            Model.OnUtilityTierChanged += HandleUtilityTierChanged;
            Model.OnTickCompleted += HandleTickCompleted;

            SpawnNeighborCityVisuals();

            StartCoroutine(ConstructionLoop());
            StartCoroutine(TickLoop());

            Logger.Log("트랜짓시티 프로토타입 시작", 1);
        }

        void OnDestroy()
        {
            if (Model == null) return;
            Model.OnRoadBuilt -= HandleRoadBuilt;
            Model.OnRoadRemoved -= HandleRoadRemoved;
            Model.OnZoneSpawned -= HandleZoneSpawned;
            Model.OnZoneRemoved -= HandleZoneRemoved;
            Model.OnUtilitySpawned -= HandleUtilitySpawned;
            Model.OnUtilityTierChanged -= HandleUtilityTierChanged;
            Model.OnTickCompleted -= HandleTickCompleted;
        }

        public void SetMode(EditMode mode)
        {
            currentMode = mode;
            lastPaintedCoord = null;
            ClearPendingRoadStart();
        }

        public void SetPaused(bool paused) => IsPaused = paused;

        void Update()
        {
            if (Mouse.current == null) return;

            // UI(버튼 등) 위의 클릭이 월드 클릭으로 새는 걸 막는다 — 안 막으면 "소로 건설"
            // 버튼을 누르는 바로 그 클릭이 버튼 아래 화면 좌표에 해당하는 격자 칸을
            // 시작점으로 잡아버려서, 2클릭 시작/끝점이 한 클릭씩 밀리는 버그가 난다.
            // 이 씬은 UGUI Canvas가 없는 순수 UI Toolkit 구성이라 EventSystem
            // 기반 IsPointerOverGameObject()는 라이커스터가 등록돼 있지 않으면
            // 항상 false만 돌려줄 수 있다 — 대신 UI Toolkit 자신의 피킹 시스템
            // (panel.Pick)으로 직접 물어본다. 이게 버튼이 클릭을 받을지 말지
            // 판단하는 것과 같은 시스템이라 더 확실하다.
            bool pointerOverUI = IsPointerOverUI();

            bool pressed = !pointerOverUI && Mouse.current.leftButton.wasPressedThisFrame;
            bool held = !pointerOverUI && Mouse.current.leftButton.isPressed;
            bool released = Mouse.current.leftButton.wasReleasedThisFrame;

            if (!pointerOverUI && Mouse.current.rightButton.wasPressedThisFrame && pendingRoadStart.HasValue)
                ClearPendingRoadStart();

            if (released) lastPaintedCoord = null;

            if (currentMode == EditMode.BuildRoad)
            {
                GridCoord hoverCoord = default;
                bool hasHover = !pointerOverUI && TryGetGridCoordUnderMouse(out hoverCoord);
                if (hasHover) UpdateGhostPreview(hoverCoord);
                else if (pendingRoadStart.HasValue) ClearGhosts();

                if (pressed && hasHover) HandleBuildRoadClick(hoverCoord);
                return;
            }

            ClearGhosts();

            if (!held) return;
            if (!TryGetGridCoordUnderMouse(out var heldCoord)) return;

            switch (currentMode)
            {
                case EditMode.Demolish:
                    PaintCoord(heldCoord, () => Invoker.TryExecute(new DemolishCommand(Model, heldCoord)));
                    break;
                case EditMode.Inspect:
                    if (pressed) HandleInspectClick(heldCoord);
                    break;
            }
        }

        /// <summary>
        /// 로터리/신호등 건설 — 건설 모드로 미리 선택해두는 게 아니라, 교차로를
        /// 직접 선택(Inspect)한 뒤 뜨는 패널에서 어느 쪽을 지을지 고르는 방식.
        /// UI(TransitCityUIController)가 이 메서드를 직접 호출한다.
        /// </summary>
        public bool TrySetIntersectionControl(GridCoord coord, IntersectionControl control, int cost)
        {
            if (!Invoker.TryExecute(new SetIntersectionControlCommand(Model, coord, control, cost))) return false;
            RefreshIntersectionMarkerAt(coord);
            return true;
        }

        /// <summary>마우스를 누른 채 드래그하면 지나간 칸마다 한 번씩만 action을 실행 — 철거를 구간으로 이어 할 수 있게 한다.</summary>
        void PaintCoord(GridCoord coord, Action action)
        {
            if (lastPaintedCoord.HasValue && lastPaintedCoord.Value.Equals(coord)) return;
            lastPaintedCoord = coord;
            action();
        }

        /// <summary>첫 클릭 = 시작점, 두 번째 클릭 = 끝점 — 그 사이를 ㄱ자 경로로 이어 짓는다.</summary>
        void HandleBuildRoadClick(GridCoord coord)
        {
            if (!pendingRoadStart.HasValue)
            {
                pendingRoadStart = coord;
                OnPendingRoadStartChanged?.Invoke(pendingRoadStart);
                return;
            }

            var start = pendingRoadStart.Value;
            ClearPendingRoadStart();

            if (start.Equals(coord))
            {
                Invoker.TryExecute(new BuildRoadCommand(Model, avenueRoadType, coord));
                return;
            }

            BuildRoadLine(start, coord);
        }

        void ClearPendingRoadStart()
        {
            if (pendingRoadStart.HasValue)
            {
                pendingRoadStart = null;
                OnPendingRoadStartChanged?.Invoke(null);
            }
            ClearGhosts();
        }

        /// <summary>start~end 사이 ㄱ자 경로(start 포함, 가로 먼저·세로 나중) — 직선도 이 경로의 특수한 경우.</summary>
        static List<GridCoord> ComputeLPath(GridCoord start, GridCoord end)
        {
            var path = new List<GridCoord> { start };
            int x = start.X, y = start.Y;
            int stepX = end.X > start.X ? 1 : (end.X < start.X ? -1 : 0);
            while (x != end.X) { x += stepX; path.Add(new GridCoord(x, y)); }
            int stepY = end.Y > start.Y ? 1 : (end.Y < start.Y ? -1 : 0);
            while (y != end.Y) { y += stepY; path.Add(new GridCoord(x, y)); }
            return path;
        }

        /// <summary>
        /// 지을 수 있는 칸이 없어질 때까지 반복 시도한다 — start/end 어느 쪽이
        /// 기존 도로망에 붙어 있는지 몰라도(혹은 둘 다 새로 짓는 경우도)
        /// 자연스럽게 이어진다.
        /// </summary>
        void BuildRoadLine(GridCoord start, GridCoord end)
        {
            var remaining = ComputeLPath(start, end);

            bool progressed = true;
            while (progressed && remaining.Count > 0)
            {
                progressed = false;
                for (int i = remaining.Count - 1; i >= 0; i--)
                {
                    if (Invoker.TryExecute(new BuildRoadCommand(Model, avenueRoadType, remaining[i])))
                    {
                        remaining.RemoveAt(i);
                        progressed = true;
                    }
                }
            }
        }

        /// <summary>시작점을 찍은 뒤 마우스를 움직이는 동안, 실제로 지어질 ㄱ자 경로를 반투명 박스로 미리 보여준다.</summary>
        void UpdateGhostPreview(GridCoord hoverCoord)
        {
            if (!pendingRoadStart.HasValue)
            {
                ClearGhosts();
                return;
            }
            if (lastGhostHoverCoord.HasValue && lastGhostHoverCoord.Value.Equals(hoverCoord)) return;
            lastGhostHoverCoord = hoverCoord;

            ClearGhosts();
            EnsureGhostMaterial();

            var path = ComputeLPath(pendingRoadStart.Value, hoverCoord);
            Color buildableColor = GhostRoadColor;
            float width = config.cellSize * 0.98f;

            foreach (var c in path)
            {
                if (Model.IsRoad(c)) continue; // 이미 도로인 칸은 바뀌는 게 없으니 표시하지 않는다.

                var tile = Model.GetTile(c);
                bool blocked = tile == null || tile.Kind != TileKind.Empty;

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "RoadGhost";
                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);
                go.transform.SetParent(gridRoot, false);
                go.transform.localPosition = CoordToLocal(c) + Vector3.up * 0.05f;
                go.transform.localScale = new Vector3(width, 0.1f, width);
                var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = ghostMaterial;
                renderer.material.color = blocked ? GhostBlockedColor : buildableColor;
                ghostObjects.Add(go);
            }
        }

        void EnsureGhostMaterial()
        {
            if (ghostMaterial != null) return;
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            ghostMaterial = new Material(Shader.Find("Unlit/Transparent")) { mainTexture = tex };
        }

        void ClearGhosts()
        {
            foreach (var go in ghostObjects) Destroy(go);
            ghostObjects.Clear();
            lastGhostHoverCoord = null;
        }

        void HandleInspectClick(GridCoord coord)
        {
            var road = Model.GetRoad(coord);
            if (road != null && road.IsTraversable && Model.IsIntersection(coord))
            {
                selectedCoord = coord;
                OnBuildingDeselected?.Invoke(); // 건물 정보 패널이 열려 있었다면 닫는다 — 상호 배타적 선택.
                ClearCoverageMarkers();
                OnIntersectionSelected?.Invoke(coord, road);
                return;
            }

            var tile = Model.GetTile(coord);
            bool isDeveloped = tile != null && tile.Kind != TileKind.Empty && tile.Kind != TileKind.NeighborCity;
            if (isDeveloped)
            {
                selectedCoord = coord;
                OnBuildingSelected?.Invoke(tile);
                RefreshCoverageMarkers(tile);
            }
            else
            {
                selectedCoord = null;
                OnBuildingDeselected?.Invoke();
                ClearCoverageMarkers();
            }
        }

        /// <summary>
        /// 선택된 인프라 시설의 실제 커버리지 반경(맨해튼 거리 다이아몬드 경계)을
        /// 표시한다. 바닥에 납작하게 깔면 그 위에 건물이 있을 때 파묻혀 안
        /// 보인다는 지적이 있어서, 세로로 긴 얇은 기둥(펜스처럼)으로 바꿔
        /// 어떤 건물보다도 높이 솟아 항상 보이게 했다.
        /// </summary>
        void RefreshCoverageMarkers(CityTile tile)
        {
            ClearCoverageMarkers();
            if (!CityGridModel.IsUtilityKind(tile.Kind)) return;

            EnsureGhostMaterial();
            int radius = config.utilityRadius + (tile.Tier - 1) * config.utilityRadiusPerTierBonus;
            const float pillarHeight = 3.5f;
            float pillarWidth = config.cellSize * 0.18f;

            foreach (var c in DiamondBoundary(tile.Coord, radius))
            {
                if (!Model.InBounds(c)) continue;

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "CoverageMarker";
                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);
                go.transform.SetParent(gridRoot, false);
                go.transform.localPosition = CoordToLocal(c) + Vector3.up * (pillarHeight * 0.5f);
                go.transform.localScale = new Vector3(pillarWidth, pillarHeight, pillarWidth);
                var renderer = go.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = ghostMaterial;
                renderer.material.color = CoverageRingColor;
                coverageMarkers.Add(go);
            }
        }

        void ClearCoverageMarkers()
        {
            foreach (var go in coverageMarkers) Destroy(go);
            coverageMarkers.Clear();
        }

        /// <summary>맨해튼 거리 radius인 다이아몬드의 경계 칸들.</summary>
        static List<GridCoord> DiamondBoundary(GridCoord center, int radius)
        {
            var result = new List<GridCoord>();
            if (radius <= 0) { result.Add(center); return result; }
            for (int dx = -radius; dx <= radius; dx++)
            {
                int dy = radius - Mathf.Abs(dx);
                result.Add(new GridCoord(center.X + dx, center.Y + dy));
                if (dy != 0) result.Add(new GridCoord(center.X + dx, center.Y - dy));
            }
            return result;
        }

        readonly Dictionary<GridCoord, GameObject> intersectionMarkers = new Dictionary<GridCoord, GameObject>();
        static readonly Color RoundaboutMarkerColor = new Color(0.3f, 0.85f, 0.75f, 0.9f);
        static readonly Color TrafficLightMarkerColor = new Color(0.95f, 0.75f, 0.2f, 0.9f);
        const float TrafficLightPoleHeight = 1.2f;

        /// <summary>로터리는 납작한 원반, 신호등은 가는 기둥 — 교차로 처리 방식을 도로 위에서 바로 구분되게 한다.</summary>
        void RefreshIntersectionMarkerAt(GridCoord coord)
        {
            RemoveIntersectionMarkerAt(coord);
            var road = Model.GetRoad(coord);
            if (road == null || road.Control == IntersectionControl.Uncontrolled) return;

            EnsureGhostMaterial();
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.SetParent(gridRoot, false);

            if (road.Control == IntersectionControl.Roundabout)
            {
                go.name = "RoundaboutMarker";
                go.transform.localScale = new Vector3(config.cellSize * 0.5f, 0.08f, config.cellSize * 0.5f);
                go.transform.localPosition = CoordToLocal(coord) + Vector3.up * (RoadTopHeight + 0.05f);
            }
            else
            {
                go.name = "TrafficLightMarker";
                go.transform.localScale = new Vector3(0.12f, TrafficLightPoleHeight * 0.5f, 0.12f);
                go.transform.localPosition = CoordToLocal(coord) + Vector3.up * (RoadTopHeight + TrafficLightPoleHeight * 0.5f);
            }

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = ghostMaterial;
            renderer.material.color = road.Control == IntersectionControl.Roundabout ? RoundaboutMarkerColor : TrafficLightMarkerColor;
            intersectionMarkers[coord] = go;
        }

        void RemoveIntersectionMarkerAt(GridCoord coord)
        {
            if (!intersectionMarkers.TryGetValue(coord, out var go)) return;
            Destroy(go);
            intersectionMarkers.Remove(coord);
        }

        IEnumerator ConstructionLoop()
        {
            while (true)
            {
                if (!IsPaused)
                {
                    Model.TickConstruction(Time.deltaTime);
                    Model.AdvanceTimeOfDay(Time.deltaTime);
                    foreach (var visual in roadVisuals.Values) visual.Refresh();
                    UpdateDecorativeTrafficSpawning(Time.deltaTime);
                }
                yield return null;
            }
        }

        float carSpawnTimer;
        float patrolSpawnTimer;
        float pedestrianSpawnTimer;
        float businessTripSpawnTimer;

        /// <summary>
        /// 장식용 통근차·순찰차·보행자 스폰을 (성장 틱이 아니라) 하루 시간대
        /// 리듬에 맞춰 계속 흐르는 타이머로 굴린다 — 배율이 높을수록(러시아워)
        /// 타이머가 더 빨리 줄어들어 더 자주 스폰되고, 낮을수록(심야) 뜸해진다.
        /// </summary>
        void UpdateDecorativeTrafficSpawning(float deltaSeconds)
        {
            carSpawnTimer -= deltaSeconds * Model.TrafficSpawnMultiplier();
            if (carSpawnTimer <= 0f)
            {
                carSpawnTimer = config.carSpawnIntervalSeconds;
                TrySpawnDecorativeCar();
            }

            patrolSpawnTimer -= deltaSeconds * Model.PatrolSpawnMultiplier();
            if (patrolSpawnTimer <= 0f)
            {
                patrolSpawnTimer = config.patrolSpawnIntervalSeconds;
                TrySpawnPatrolCar();
            }

            pedestrianSpawnTimer -= deltaSeconds * Model.TrafficSpawnMultiplier();
            if (pedestrianSpawnTimer <= 0f)
            {
                pedestrianSpawnTimer = config.pedestrianSpawnIntervalSeconds;
                TrySpawnPedestrian();
            }

            // 업무 통행(상점 배달 등)은 출퇴근 러시아워가 아니라 낮/밤 기본 활동
            // 곡선만 따른다 — 자정 부근에도 완전히 0으로 죽지 않게 최소치를 둔다.
            float businessActivity = Mathf.Lerp(0.1f, 1f, Model.DayActivityFactor01(Model.TimeOfDayHours));
            businessTripSpawnTimer -= deltaSeconds * businessActivity;
            if (businessTripSpawnTimer <= 0f)
            {
                businessTripSpawnTimer = config.businessTripSpawnIntervalSeconds;
                TrySpawnBusinessTrip();
            }
        }

        IEnumerator TickLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(config.tickIntervalSeconds);
                if (!IsPaused) Model.RunTick();
            }
        }

        void HandleRoadBuilt(RoadCell road)
        {
            var visual = Instantiate(roadVisualPrefab, gridRoot);
            visual.transform.localPosition = CoordToLocal(road.Coord);
            float width = road.Type.tier == RoadTier.Avenue ? config.cellSize * 0.98f : config.cellSize * 0.92f;
            visual.Bind(road, road.Type, new Vector3(width, RoadTopHeight, width));
            roadVisuals[road.Coord] = visual;
        }

        void HandleRoadRemoved(GridCoord coord)
        {
            RemoveIntersectionMarkerAt(coord);
            if (!roadVisuals.TryGetValue(coord, out var visual)) return;
            Destroy(visual.gameObject);
            roadVisuals.Remove(coord);
        }

        void HandleZoneSpawned(CityTile tile)
        {
            DestroyExistingVisualAt(tile.Coord);
            var visual = Instantiate(houseVisualPrefab, gridRoot);
            visual.transform.localPosition = CoordToLocal(tile.Coord); // 발생 시점엔 항상 1x1이라 원점=발자국 중심.
            visual.Bind(tile, new Vector3(config.cellSize * 0.7f, 1f, config.cellSize * 0.7f), config.cellSize, 0.3f, 1.8f);
            houseVisuals[tile.Coord] = visual;
        }

        /// <summary>발자국(원점+확장 칸들) 전체의 로컬 중심 좌표 — 건물이 1x2/2x2로 자라면 시각적 중심도 그만큼 옮겨줘야 한다.</summary>
        Vector3 FootprintCenterLocal(CityTile owner)
        {
            float sumX = owner.Coord.X, sumY = owner.Coord.Y;
            int count = 1;
            foreach (var c in owner.FootprintExtensions)
            {
                sumX += c.X;
                sumY += c.Y;
                count++;
            }
            return new Vector3(sumX / count * config.cellSize, 0f, sumY / count * config.cellSize);
        }

        /// <summary>발자국이 차지하는 바운딩 박스 크기(칸 단위) — 1x1/1x2/2x2.</summary>
        Vector2Int FootprintSpanCells(CityTile owner)
        {
            int minX = owner.Coord.X, maxX = owner.Coord.X, minY = owner.Coord.Y, maxY = owner.Coord.Y;
            foreach (var c in owner.FootprintExtensions)
            {
                minX = Mathf.Min(minX, c.X);
                maxX = Mathf.Max(maxX, c.X);
                minY = Mathf.Min(minY, c.Y);
                maxY = Mathf.Max(maxY, c.Y);
            }
            return new Vector2Int(maxX - minX + 1, maxY - minY + 1);
        }

        void HandleZoneRemoved(GridCoord coord)
        {
            DestroyExistingVisualAt(coord);
            if (selectedCoord.HasValue && selectedCoord.Value.Equals(coord))
            {
                selectedCoord = null;
                ClearCoverageMarkers();
            }
            OnBuildingDeselected?.Invoke();
        }

        /// <summary>
        /// 이 칸에 이미 건물 비주얼이 있으면 먼저 지운다 — 모델 쪽에서 타일
        /// Kind를 직접 바꾸는 경로(인프라 강제 전환 등)가 OnZoneRemoved를
        /// 안 쏘고 넘어가면, 옛 비주얼 위에 새 비주얼이 겹쳐 남는 버그가
        /// 난다(신고받은 "건물이 겹쳐서 생기는" 버그의 원인). 어떤 경로로
        /// 오든 여기서 한 번 더 방어한다.
        /// </summary>
        void DestroyExistingVisualAt(GridCoord coord)
        {
            if (!houseVisuals.TryGetValue(coord, out var existing)) return;
            Destroy(existing.gameObject);
            houseVisuals.Remove(coord);
        }

        static readonly Color WaterColor = new Color(0.3f, 0.55f, 0.85f);
        static readonly Color SewageColor = new Color(0.5f, 0.42f, 0.28f);
        static readonly Color PowerColor = new Color(0.85f, 0.75f, 0.25f);
        static readonly Color PoliceColor = new Color(0.25f, 0.3f, 0.55f);
        static readonly Color FireColor = new Color(0.8f, 0.25f, 0.2f);
        static readonly Color BusinessTripColor = new Color(0.85f, 0.55f, 0.15f);
        static readonly Color EducationColor = new Color(0.35f, 0.65f, 0.55f);
        static readonly Color MedicalColor = new Color(0.85f, 0.4f, 0.45f);
        static readonly Color StoreColor = new Color(0.75f, 0.5f, 0.85f);
        static readonly Color FactoryColor = new Color(0.55f, 0.5f, 0.45f);
        static readonly Color ParkColor = new Color(0.35f, 0.75f, 0.35f);
        static readonly Color GarbageColor = new Color(0.45f, 0.4f, 0.3f);

        static Color UtilityColor(TileKind kind) => kind switch
        {
            TileKind.UtilityWater => WaterColor,
            TileKind.UtilitySewage => SewageColor,
            TileKind.UtilityPower => PowerColor,
            TileKind.UtilityPolice => PoliceColor,
            TileKind.UtilityFire => FireColor,
            TileKind.UtilityEducation => EducationColor,
            TileKind.UtilityMedical => MedicalColor,
            TileKind.UtilityStore => StoreColor,
            TileKind.UtilityFactory => FactoryColor,
            TileKind.UtilityPark => ParkColor,
            TileKind.UtilityGarbage => GarbageColor,
            _ => Color.gray,
        };

        Vector3 UtilityFootprint => new Vector3(config.cellSize * 0.85f, 1f, config.cellSize * 0.85f);
        const float UtilityBaseHeight = 1.1f;

        void HandleUtilitySpawned(CityTile tile)
        {
            DestroyExistingVisualAt(tile.Coord);
            var visual = Instantiate(houseVisualPrefab, gridRoot);
            visual.transform.localPosition = CoordToLocal(tile.Coord); // 발생 시점엔 항상 1x1이라 원점=발자국 중심.
            visual.BindUtility(UtilityFootprint, config.cellSize, UtilityBaseHeight, UtilityColor(tile.Kind), tile.Tier, FootprintSpanCells(tile));
            houseVisuals[tile.Coord] = visual;
        }

        void HandleUtilityTierChanged(CityTile tile)
        {
            if (!houseVisuals.TryGetValue(tile.Coord, out var visual)) return;
            // 승급하며 발자국이 넓어졌을 수 있으니 시각적 중심도 다시 맞춘다.
            visual.transform.localPosition = FootprintCenterLocal(tile);
            visual.BindUtility(UtilityFootprint, config.cellSize, UtilityBaseHeight, UtilityColor(tile.Kind), tile.Tier, FootprintSpanCells(tile));
            if (selectedCoord.HasValue && selectedCoord.Value.Equals(tile.Coord)) RefreshCoverageMarkers(tile);
        }

        void HandleTickCompleted()
        {
            // 주거지는 밀도 재개발로 발자국이 매 틱 바뀔 수 있어(전용 이벤트가
            // 없음) 매 틱 중심을 다시 맞춘다 — 인프라는 티어 변경 이벤트에서만
            // 바뀌므로 중복이지만, 매번 같은 값이라 비용은 무시할 만하다.
            foreach (var kvp in houseVisuals)
            {
                var ownerTile = Model.GetTile(kvp.Key);
                if (ownerTile == null) continue;
                var center = FootprintCenterLocal(ownerTile);
                var p = kvp.Value.transform.localPosition;
                p.x = center.x;
                p.z = center.z;
                kvp.Value.transform.localPosition = p;
                kvp.Value.Refresh(FootprintSpanCells(ownerTile));
            }
            foreach (var visual in roadVisuals.Values) visual.Refresh();
        }

        void SpawnNeighborCityVisuals()
        {
            if (neighborCityTilePrefab == null) return;
            foreach (var tile in Model.AllTiles.Where(t => t.Kind == TileKind.NeighborCity))
            {
                var go = Instantiate(neighborCityTilePrefab, gridRoot);
                go.transform.localPosition = CoordToLocal(tile.Coord);
            }
        }

        void TrySpawnDecorativeCar()
        {
            if (carVisualPrefab == null || activeCommuterCars.Count >= config.maxCommuterCars) return;

            var candidates = Model.AllTiles.Where(t => t.Kind == TileKind.Residential && t.Population > 0).ToList();
            if (candidates.Count == 0) return;

            var tile = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            GridCoord? entry = null;
            int best = int.MaxValue;
            foreach (var n in GridCoord.Neighbors4(tile.Coord))
            {
                if (Model.LastRoadDistance.TryGetValue(n, out var d) && d < best)
                {
                    best = d;
                    entry = n;
                }
            }
            if (entry == null) return;

            // 집 → 이웃 도시 방향("바깥")으로 나가는 경로 — 출근은 이 방향 그대로,
            // 퇴근은 이걸 뒤집어서 바깥에서 집으로 들어오는 경로로 쓴다.
            var outwardRoadCoords = new List<GridCoord>();
            GridCoord? cursor = entry;
            while (cursor.HasValue)
            {
                outwardRoadCoords.Add(cursor.Value);
                cursor = Model.LastRoadParent.TryGetValue(cursor.Value, out var p) ? p : null;
            }

            // 도로 "위"를 달리도록 차량 높이만큼 들어 올린다 — 그렇지 않으면 도로 바닥(y=0) 밑을 지나가 버린다.
            Vector3 rideOffset = Vector3.up * carRideHeight;
            bool goingHome = outwardRoadCoords.Count > 0 && UnityEngine.Random.value < Model.HomeboundBias01();

            List<Vector3> path;
            List<GridCoord> roadCoords;
            if (goingHome)
            {
                // 퇴근 러시아워 — 바깥(이웃 도시 쪽)에서 출발해 집으로 들어온다.
                var inwardRoadCoords = new List<GridCoord>(outwardRoadCoords);
                inwardRoadCoords.Reverse();
                path = new List<Vector3>();
                foreach (var c in inwardRoadCoords) path.Add(CoordToWorld(c) + rideOffset);
                path.Add(CoordToWorld(tile.Coord) + rideOffset);
                // 마지막 구간(도로→집)은 건물이라 칸 예약이 필요 없다 — 출근 때 첫 구간(집→도로)이
                // 예약 대상이 아니었던 것과 대칭.
                roadCoords = inwardRoadCoords.Count > 1 ? inwardRoadCoords.GetRange(1, inwardRoadCoords.Count - 1) : new List<GridCoord>();
            }
            else
            {
                // 출근 — 집에서 나가 이웃 도시 방향으로 향한다.
                path = new List<Vector3> { CoordToWorld(tile.Coord) + rideOffset };
                foreach (var c in outwardRoadCoords) path.Add(CoordToWorld(c) + rideOffset);
                roadCoords = outwardRoadCoords;
            }

            var (speedScale, pauses) = BuildTrafficLists(roadCoords);

            var car = carPool.Get();
            car.SetColor(null); // 순찰차로 재사용됐을 때 입혔던 색을 기본색으로 되돌린다.
            car.transform.position = path[0];
            activeCommuterCars.Add(car);
            car.OnJourneyComplete += HandleCarJourneyComplete;
            car.Drive(path, roadCoords, config.carSpeed, config.cellSize * config.carLaneOffsetRatio, config.carMaxQueueWaitSeconds, speedScale, pauses);
        }

        /// <summary>
        /// 경로 위 도로 칸들의 혼잡도로부터 구간별 속도 배율과 칸별 정차 시간을
        /// 만든다 — 막힌 구간은 느려지고, 신호등은 가끔 잠깐 멈추며, 미설치
        /// 교차로가 과부하면 "전원 정지 후 순서대로"를 흉내 내 확실히 멈췄다 간다.
        /// </summary>
        (List<float> speedScale, List<float> pauses) BuildTrafficLists(List<GridCoord> roadCoords)
        {
            var speedScale = new List<float>();
            var pauses = new List<float> { 0f }; // 출발점(건물)은 대기 없음.

            foreach (var c in roadCoords)
            {
                var road = Model.GetRoad(c);
                float ratio = 0f;
                if (road != null)
                {
                    int cap = Model.EffectiveCapacity(road);
                    if (cap > 0) ratio = Mathf.Clamp01(road.TripLoad / (float)cap);
                }
                speedScale.Add(Mathf.Lerp(1f, 0.25f, ratio));

                float pause = 0f;
                if (Model.IsUncontrolledOverloadedIntersection(c))
                {
                    pause = UnityEngine.Random.Range(config.uncontrolledJamStopSecondsMin, config.uncontrolledJamStopSecondsMax);
                }
                else if (road != null && road.Control == IntersectionControl.TrafficLight && Model.IsIntersection(c)
                         && UnityEngine.Random.value < config.trafficLightStopChance)
                {
                    pause = UnityEngine.Random.Range(config.trafficLightStopSecondsMin, config.trafficLightStopSecondsMax);
                }
                pauses.Add(pause);
            }

            return (speedScale, pauses);
        }

        /// <summary>
        /// 경찰서·소방서에서 순찰차가 도로를 타고 순찰 반경만큼 나갔다가 되돌아온다.
        /// 이 순찰은 장식이 아니라 실제로 치안·소방 커버리지 반경을 늘려준다
        /// (CityGridModel.ComputeCoverage의 patrolRadiusBonus) — "순찰 돌면서
        /// 수치를 올려줬으면" 요청대로.
        /// </summary>
        void TrySpawnPatrolCar()
        {
            if (carVisualPrefab == null || activePatrolCars.Count >= config.maxPatrolCars) return;

            var stations = Model.AllTiles
                .Where(t => t.Kind == TileKind.UtilityPolice || t.Kind == TileKind.UtilityFire)
                .ToList();
            if (stations.Count == 0) return;

            var station = stations[UnityEngine.Random.Range(0, stations.Count)];

            var entryOptions = GridCoord.Neighbors4(station.Coord)
                .Where(n => { var r = Model.GetRoad(n); return r != null && r.IsTraversable; })
                .ToList();
            if (entryOptions.Count == 0) return;
            GridCoord? entry = entryOptions[UnityEngine.Random.Range(0, entryOptions.Count)];

            // 순찰 반경만큼 "무작위로" 걸어나간다 — 예전엔 이웃 도시까지 가는 고정
            // 최단경로(BFS 부모 체인)를 그대로 썼는데, 그건 도로가 여러 갈래여도
            // 항상 똑같은 길 하나로만 순찰을 도는 것처럼 보이는 원인이었다(신고받음).
            // 매번 갈 수 있는 이웃 도로 중 하나를 무작위로 골라(방금 온 칸은 제외 —
            // 바로 되돌아오는 것 방지) 걸어나가고, 갈 곳이 없으면(막다른 길) 거기서
            // 멈추고 왔던 길을 되짚어 돌아온다.
            var outCoords = new List<GridCoord> { entry.Value };
            GridCoord? prev = station.Coord;
            GridCoord? cursor = entry;
            int steps = Mathf.Max(2, config.utilityRadius);
            for (int i = 0; i < steps; i++)
            {
                var options = GridCoord.Neighbors4(cursor.Value)
                    .Where(n => !n.Equals(prev.Value))
                    .Where(n => { var r = Model.GetRoad(n); return r != null && r.IsTraversable; })
                    .ToList();
                if (options.Count == 0) break;
                var next = options[UnityEngine.Random.Range(0, options.Count)];
                prev = cursor;
                cursor = next;
                outCoords.Add(cursor.Value);
            }

            var roundTripRoadCoords = new List<GridCoord>(outCoords);
            for (int i = outCoords.Count - 2; i >= 0; i--) roundTripRoadCoords.Add(outCoords[i]);

            Vector3 rideOffset = Vector3.up * carRideHeight;
            var path = new List<Vector3> { CoordToWorld(station.Coord) + rideOffset };
            foreach (var c in roundTripRoadCoords) path.Add(CoordToWorld(c) + rideOffset);
            path.Add(CoordToWorld(station.Coord) + rideOffset);
            var (speedScale, pauses) = BuildTrafficLists(roundTripRoadCoords);

            // 순찰 반경 끝(방향이 뒤집히는 지점)에서 잠깐 멈춰 "둘러보고 돌아간다"는
            // 느낌을 준다 — 안 그러면 아무 이유 없이 갑자기 유턴하는 것처럼 보인다
            // (실제로 신고받은 문제).
            int turnaroundPauseIndex = outCoords.Count;
            if (turnaroundPauseIndex < pauses.Count)
                pauses[turnaroundPauseIndex] = Mathf.Max(pauses[turnaroundPauseIndex], UnityEngine.Random.Range(0.4f, 0.9f));

            var car = carPool.Get();
            car.SetColor(station.Kind == TileKind.UtilityPolice ? PoliceColor : FireColor);
            car.transform.position = path[0];
            activePatrolCars.Add(car);
            car.OnJourneyComplete += HandleCarJourneyComplete;
            car.Drive(path, roundTripRoadCoords, config.carSpeed, config.cellSize * config.carLaneOffsetRatio, config.carMaxQueueWaitSeconds, speedScale, pauses);
        }

        /// <summary>
        /// 낮 시간대 업무 통행 — 출퇴근(집↔바깥)과는 별개로, 가동 중인(근무 인원이
        /// 있는) 인프라 건물에서 출발하는 트립. 건물 특성에 따라 목적지가 다르다:
        /// 상점은 배달(건물→건물, 무작위 주거지로), 그 외(공장·발전소 등)는
        /// 물류/업무상 외부로 나가는 트립(건물→바깥, 기존 출퇴근과 같은 "이웃
        /// 도시 쪽으로" 경로를 재사용).
        /// </summary>
        void TrySpawnBusinessTrip()
        {
            if (carVisualPrefab == null || activeCommuterCars.Count >= config.maxCommuterCars) return;

            var origins = Model.AllTiles.Where(t => CityGridModel.IsUtilityKind(t.Kind) && t.Population > 0).ToList();
            if (origins.Count == 0) return;
            var origin = origins[UnityEngine.Random.Range(0, origins.Count)];

            var originEntry = Model.FindEntryRoad(origin.Coord);
            if (!originEntry.HasValue) return;

            Vector3 rideOffset = Vector3.up * carRideHeight;
            List<GridCoord> roadCoords;
            List<Vector3> path;

            if (origin.Kind == TileKind.UtilityStore)
            {
                var homes = Model.AllTiles.Where(t => t.Kind == TileKind.Residential && t.Population > 0).ToList();
                if (homes.Count == 0) return;
                var home = homes[UnityEngine.Random.Range(0, homes.Count)];
                var homeEntry = Model.FindEntryRoad(home.Coord);
                if (!homeEntry.HasValue) return;

                roadCoords = Model.FindRoadPath(originEntry.Value, homeEntry.Value);
                if (roadCoords.Count == 0) return; // 도로로 이어지지 않음

                path = new List<Vector3> { CoordToWorld(origin.Coord) + rideOffset };
                foreach (var c in roadCoords) path.Add(CoordToWorld(c) + rideOffset);
                path.Add(CoordToWorld(home.Coord) + rideOffset); // 도착(건물) — 예약 대상 아님, roadCoords 그대로 둠
            }
            else
            {
                roadCoords = new List<GridCoord>();
                GridCoord? cursor = originEntry;
                while (cursor.HasValue)
                {
                    roadCoords.Add(cursor.Value);
                    cursor = Model.LastRoadParent.TryGetValue(cursor.Value, out var p) ? p : null;
                }

                path = new List<Vector3> { CoordToWorld(origin.Coord) + rideOffset };
                foreach (var c in roadCoords) path.Add(CoordToWorld(c) + rideOffset);
            }

            var (speedScale, pauses) = BuildTrafficLists(roadCoords);

            var car = carPool.Get();
            car.SetColor(BusinessTripColor);
            car.transform.position = path[0];
            activeCommuterCars.Add(car);
            car.OnJourneyComplete += HandleCarJourneyComplete;
            car.Drive(path, roadCoords, config.carSpeed, config.cellSize * config.carLaneOffsetRatio, config.carMaxQueueWaitSeconds, speedScale, pauses);
        }

        void HandleCarJourneyComplete(CarVisual car)
        {
            car.OnJourneyComplete -= HandleCarJourneyComplete;
            activeCommuterCars.Remove(car);
            activePatrolCars.Remove(car);
            ReleaseAllTilesFor(car); // 방어적 정리 — 정상 경로라면 CarVisual이 이미 다 반납했을 것.
            carPool.Release(car);
        }

        /// <summary>
        /// 칸 단위 점유 예약 — 여러 차가 겹쳐서 그냥 지나가 버리지 않도록, 한 칸엔
        /// 한 번에 한 대만 있을 수 있게 한다("줄을 서는" 정체의 실체). 이미 다른
        /// 차가 있으면 실패(false)를 돌려주고, 호출 쪽(CarVisual)이 비워질 때까지
        /// 기다린다.
        /// </summary>
        public bool TryReserveTile(GridCoord coord, CarVisual car)
        {
            if (tileOccupants.TryGetValue(coord, out var occupant) && occupant != car) return false;
            tileOccupants[coord] = car;
            return true;
        }

        public void ReleaseTile(GridCoord coord, CarVisual car)
        {
            if (tileOccupants.TryGetValue(coord, out var occupant) && occupant == car) tileOccupants.Remove(coord);
        }

        void ReleaseAllTilesFor(CarVisual car)
        {
            var toRemove = tileOccupants.Where(kvp => kvp.Value == car).Select(kvp => kvp.Key).ToList();
            foreach (var c in toRemove) tileOccupants.Remove(c);
        }

        /// <summary>
        /// 사람(작은 원)이 집에서 나와 가장 가까운 도로까지 걸어가는 "라스트마일"만
        /// 표현한다 — 도로에 올라선 뒤는 차량(네모)이 이어받는다는 설정. 버스/
        /// 지하철/공항철도가 나중에 추가되면 이 지점이 그 정류장·역까지 걸어가는
        /// 구간으로 자연스럽게 확장된다.
        /// </summary>
        void TrySpawnPedestrian()
        {
            if (personVisualPrefab == null || personPool == null || activePeople.Count >= config.maxPedestrians) return;

            var candidates = Model.AllTiles.Where(t => t.Kind == TileKind.Residential && t.Population > 0).ToList();
            if (candidates.Count == 0) return;

            var tile = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            GridCoord? entry = null;
            int best = int.MaxValue;
            foreach (var n in GridCoord.Neighbors4(tile.Coord))
            {
                if (Model.LastRoadDistance.TryGetValue(n, out var d) && d < best)
                {
                    best = d;
                    entry = n;
                }
            }
            if (entry == null) return;

            // 차량과 같은 기준 — 퇴근 시간대면 도로에서 집으로 걸어 들어오는 방향.
            bool goingHome = UnityEngine.Random.value < Model.HomeboundBias01();
            var path = goingHome
                ? new List<Vector3> { CoordToWorld(entry.Value), CoordToWorld(tile.Coord) }
                : new List<Vector3> { CoordToWorld(tile.Coord), CoordToWorld(entry.Value) };

            var person = personPool.Get();
            person.transform.position = path[0];
            activePeople.Add(person);
            person.OnJourneyComplete += HandlePersonJourneyComplete;
            person.Walk(path, config.pedestrianSpeed);
        }

        void HandlePersonJourneyComplete(PersonVisual person)
        {
            person.OnJourneyComplete -= HandlePersonJourneyComplete;
            activePeople.Remove(person);
            personPool.Release(person);
        }

        Vector3 CoordToLocal(GridCoord c) => new Vector3(c.X * config.cellSize, 0f, c.Y * config.cellSize);
        Vector3 CoordToWorld(GridCoord c) => gridRoot.position + CoordToLocal(c);

        /// <summary>UI Toolkit 자체 피킹으로 현재 마우스 아래 픽업 가능한(picking-mode가 Ignore가 아닌) UI 요소가 있는지 묻는다.</summary>
        bool IsPointerOverUI()
        {
            if (hudDocument == null || Mouse.current == null) return false;
            var root = hudDocument.rootVisualElement;
            var panel = root?.panel;
            if (panel == null) return false;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);
            return panel.Pick(panelPos) != null;
        }

        bool TryGetGridCoordUnderMouse(out GridCoord coord)
        {
            coord = default;
            if (worldCamera == null || Mouse.current == null) return false;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Ray ray = worldCamera.ScreenPointToRay(screenPos);
            var groundPlane = new Plane(Vector3.up, gridRoot.position);
            if (!groundPlane.Raycast(ray, out float enter)) return false;

            Vector3 hit = ray.GetPoint(enter);
            int gx = Mathf.RoundToInt((hit.x - gridRoot.position.x) / config.cellSize);
            int gy = Mathf.RoundToInt((hit.z - gridRoot.position.z) / config.cellSize);
            coord = new GridCoord(gx, gy);
            return Model.InBounds(coord);
        }
    }
}
