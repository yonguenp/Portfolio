using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace TransitCity
{
    [RequireComponent(typeof(UIDocument))]
    public class TransitCityUIController : MonoBehaviour
    {
        enum ToolSelection { None, Road, Demolish }

        UIDocument document;
        Label dayLabel;
        Label populationLabel;
        Label moneyLabel;

        VisualElement hintPanel;
        Label hintLabel;

        Button roadButton;
        Button demolishButton;
        Button pauseButton;

        VisualElement infoPanel;
        Label infoKindLabel;
        Label infoCoordLabel;
        Label infoPopulationLabel;
        Label infoWealthLabel;
        Label infoSatisfactionLabel;
        Label infoTierLabel;
        Label infoLaborLabel;
        Label infoWaterLabel;
        Label infoSewageLabel;
        Label infoPowerLabel;
        Label infoPoliceLabel;
        Label infoFireLabel;
        Label infoEducationLabel;
        Label infoMedicalLabel;
        Label infoConsumptionLabel;
        Label infoJobsLabel;
        Label infoLeisureLabel;
        Label infoHygieneLabel;
        Label infoTransitLabel;
        Button infoCloseButton;

        VisualElement intersectionPanel;
        Label intersectionCurrentLabel;
        Button intersectionRoundaboutButton;
        Button intersectionTrafficLightButton;
        Button intersectionCloseButton;
        GridCoord? selectedIntersectionCoord;

        ToolSelection currentSelection = ToolSelection.None;
        bool paused;
        CityTile selectedTile;

        void Awake()
        {
            document = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            var root = document.rootVisualElement;
            dayLabel = root.Q<Label>("day-label");
            populationLabel = root.Q<Label>("population-label");
            moneyLabel = root.Q<Label>("money-label");

            hintPanel = root.Q<VisualElement>("hint-panel");
            hintLabel = root.Q<Label>("build-hint-label");

            roadButton = root.Q<Button>("road-button");
            demolishButton = root.Q<Button>("demolish-button");
            pauseButton = root.Q<Button>("pause-button");

            infoPanel = root.Q<VisualElement>("info-panel");
            infoKindLabel = root.Q<Label>("info-kind");
            infoCoordLabel = root.Q<Label>("info-coord");
            infoPopulationLabel = root.Q<Label>("info-population");
            infoWealthLabel = root.Q<Label>("info-wealth");
            infoSatisfactionLabel = root.Q<Label>("info-satisfaction");
            infoTierLabel = root.Q<Label>("info-tier");
            infoLaborLabel = root.Q<Label>("info-labor");
            infoWaterLabel = root.Q<Label>("info-water");
            infoSewageLabel = root.Q<Label>("info-sewage");
            infoPowerLabel = root.Q<Label>("info-power");
            infoPoliceLabel = root.Q<Label>("info-police");
            infoFireLabel = root.Q<Label>("info-fire");
            infoEducationLabel = root.Q<Label>("info-education");
            infoMedicalLabel = root.Q<Label>("info-medical");
            infoConsumptionLabel = root.Q<Label>("info-consumption");
            infoJobsLabel = root.Q<Label>("info-jobs");
            infoLeisureLabel = root.Q<Label>("info-leisure");
            infoHygieneLabel = root.Q<Label>("info-hygiene");
            infoTransitLabel = root.Q<Label>("info-transit");
            infoCloseButton = root.Q<Button>("info-close-button");

            intersectionPanel = root.Q<VisualElement>("intersection-panel");
            intersectionCurrentLabel = root.Q<Label>("intersection-current");
            intersectionRoundaboutButton = root.Q<Button>("intersection-roundabout-button");
            intersectionTrafficLightButton = root.Q<Button>("intersection-traffic-light-button");
            intersectionCloseButton = root.Q<Button>("intersection-close-button");

            roadButton.clicked += SelectRoad;
            demolishButton.clicked += SelectDemolish;
            pauseButton.clicked += TogglePause;
            infoCloseButton.clicked += HideInfoPanel;
            intersectionRoundaboutButton.clicked += BuildRoundaboutAtSelected;
            intersectionTrafficLightButton.clicked += BuildTrafficLightAtSelected;
            intersectionCloseButton.clicked += HideIntersectionPanel;
        }

        void OnDisable()
        {
            if (roadButton != null) roadButton.clicked -= SelectRoad;
            if (demolishButton != null) demolishButton.clicked -= SelectDemolish;
            if (pauseButton != null) pauseButton.clicked -= TogglePause;
            if (infoCloseButton != null) infoCloseButton.clicked -= HideInfoPanel;
            if (intersectionRoundaboutButton != null) intersectionRoundaboutButton.clicked -= BuildRoundaboutAtSelected;
            if (intersectionTrafficLightButton != null) intersectionTrafficLightButton.clicked -= BuildTrafficLightAtSelected;
            if (intersectionCloseButton != null) intersectionCloseButton.clicked -= HideIntersectionPanel;

            var manager = TransitCityManager.Instance;
            if (manager != null)
            {
                manager.OnBuildingSelected -= HandleBuildingSelected;
                manager.OnBuildingDeselected -= HideInfoPanel;
                manager.OnPendingRoadStartChanged -= HandlePendingRoadStartChanged;
                manager.OnIntersectionSelected -= HandleIntersectionSelected;
            }

            var model = manager != null ? manager.Model : null;
            if (model != null)
            {
                model.OnTickCompleted -= RefreshStats;
                model.OnMoneyChanged -= RefreshStats;
            }
        }

        void Start()
        {
            StartCoroutine(BindToManagerWhenReady());
        }

        IEnumerator BindToManagerWhenReady()
        {
            while (TransitCityManager.Instance == null || TransitCityManager.Instance.Model == null)
                yield return null;

            var manager = TransitCityManager.Instance;
            var model = manager.Model;
            model.OnTickCompleted += RefreshStats;
            model.OnMoneyChanged += RefreshStats;
            manager.OnBuildingSelected += HandleBuildingSelected;
            manager.OnBuildingDeselected += HideInfoPanel;
            manager.OnPendingRoadStartChanged += HandlePendingRoadStartChanged;
            manager.OnIntersectionSelected += HandleIntersectionSelected;

            roadButton.text = $"도로 건설 ({manager.RoadType.cost}원)";
            intersectionRoundaboutButton.text = $"로터리 건설 ({manager.Config.roundaboutCost}원)";
            intersectionTrafficLightButton.text = $"신호등 건설 ({manager.Config.trafficLightCost}원)";
            RefreshStats();
        }

        void SelectRoad() => ToggleSelection(ToolSelection.Road);
        void SelectDemolish() => ToggleSelection(ToolSelection.Demolish);

        void ToggleSelection(ToolSelection selection)
        {
            SetSelection(currentSelection == selection ? ToolSelection.None : selection);
        }

        void SetSelection(ToolSelection selection)
        {
            currentSelection = selection;
            var manager = TransitCityManager.Instance;

            switch (selection)
            {
                case ToolSelection.Road:
                    manager.SetMode(EditMode.BuildRoad);
                    break;
                case ToolSelection.Demolish:
                    manager.SetMode(EditMode.Demolish);
                    break;
                default:
                    manager.SetMode(EditMode.Inspect);
                    break;
            }

            ApplyModeButtonStates();
            HandlePendingRoadStartChanged(null);
        }

        void ApplyModeButtonStates()
        {
            roadButton.EnableInClassList("build-button-active", currentSelection == ToolSelection.Road);
            demolishButton.EnableInClassList("demolish-button-active", currentSelection == ToolSelection.Demolish);
        }

        void HandlePendingRoadStartChanged(GridCoord? start)
        {
            if (currentSelection != ToolSelection.Road)
            {
                hintPanel.style.display = DisplayStyle.None;
                return;
            }

            hintLabel.text = start.HasValue ? "끝점을 클릭하세요 (우클릭으로 취소)" : "시작점을 클릭하세요";
            hintPanel.style.display = DisplayStyle.Flex;
        }

        void HandleIntersectionSelected(GridCoord coord, RoadCell road)
        {
            selectedIntersectionCoord = coord;
            intersectionPanel.style.display = DisplayStyle.Flex;
            RefreshIntersectionPanel();
        }

        void RefreshIntersectionPanel()
        {
            if (!selectedIntersectionCoord.HasValue) return;
            var road = TransitCityManager.Instance.Model.GetRoad(selectedIntersectionCoord.Value);
            if (road == null) { HideIntersectionPanel(); return; }

            string current = road.Control switch
            {
                IntersectionControl.Roundabout => "로터리",
                IntersectionControl.TrafficLight => "신호등",
                _ => "미설치(전원 정지 후 순서대로)",
            };
            intersectionCurrentLabel.text = $"현재: {current}";
        }

        void HideIntersectionPanel()
        {
            selectedIntersectionCoord = null;
            intersectionPanel.style.display = DisplayStyle.None;
        }

        void BuildRoundaboutAtSelected() => BuildIntersectionControlAtSelected(IntersectionControl.Roundabout, TransitCityManager.Instance.Config.roundaboutCost);
        void BuildTrafficLightAtSelected() => BuildIntersectionControlAtSelected(IntersectionControl.TrafficLight, TransitCityManager.Instance.Config.trafficLightCost);

        void BuildIntersectionControlAtSelected(IntersectionControl control, int cost)
        {
            if (!selectedIntersectionCoord.HasValue) return;
            TransitCityManager.Instance.TrySetIntersectionControl(selectedIntersectionCoord.Value, control, cost);
            RefreshIntersectionPanel();
        }

        void TogglePause()
        {
            paused = !paused;
            TransitCityManager.Instance.SetPaused(paused);
            pauseButton.text = paused ? "재생" : "일시정지";
            pauseButton.EnableInClassList("pause-button-active", paused);
        }

        void HandleBuildingSelected(CityTile tile)
        {
            HideIntersectionPanel(); // 상호 배타적 선택 — 건물을 고르면 교차로 패널은 닫는다.
            selectedTile = tile;
            infoPanel.style.display = DisplayStyle.Flex;
            RefreshInfoPanel();
        }

        void HideInfoPanel()
        {
            selectedTile = null;
            infoPanel.style.display = DisplayStyle.None;
        }

        void RefreshInfoPanel()
        {
            if (selectedTile == null) return;

            infoKindLabel.text = KindLabel(selectedTile.Kind);
            infoCoordLabel.text = $"좌표 ({selectedTile.Coord.X}, {selectedTile.Coord.Y})";

            if (selectedTile.Kind == TileKind.Residential)
            {
                infoPopulationLabel.text = $"인구 {selectedTile.Population}/{selectedTile.Capacity}";
                string wealthLabel = selectedTile.Wealth >= 0.67f ? "상" : selectedTile.Wealth >= 0.34f ? "중" : "하";
                infoWealthLabel.text = $"재산 수준: {wealthLabel}";
                infoSatisfactionLabel.text = $"만족도: {Mathf.RoundToInt(selectedTile.Satisfaction * 100)}%";
                infoTierLabel.text = $"밀도: {DensityTierLabel(selectedTile.Tier)} · {IncomeTierLabel(selectedTile.IncomeTier)}";
                infoLaborLabel.text = "";
                infoWaterLabel.text = NeedLine("상수도", selectedTile.WaterCoverage, selectedTile.InfraDemand);
                infoSewageLabel.text = NeedLine("하수도", selectedTile.SewageCoverage, selectedTile.InfraDemand);
                infoPowerLabel.text = NeedLine("전기", selectedTile.PowerCoverage, selectedTile.InfraDemand);
                infoPoliceLabel.text = NeedLine("치안", selectedTile.PoliceCoverage, selectedTile.InfraDemand);
                infoFireLabel.text = NeedLine("소방", selectedTile.FireCoverage, selectedTile.InfraDemand);
                infoEducationLabel.text = NeedLine("교육", selectedTile.EducationCoverage, selectedTile.InfraDemand);
                infoMedicalLabel.text = NeedLine("의료", selectedTile.MedicalCoverage, selectedTile.InfraDemand);
                infoConsumptionLabel.text = NeedLine("소비", selectedTile.ConsumptionCoverage, selectedTile.InfraDemand);
                infoJobsLabel.text = NeedLine("일자리", selectedTile.JobCoverage, selectedTile.InfraDemand);
                infoLeisureLabel.text = NeedLine("여유", selectedTile.LeisureCoverage, selectedTile.LeisureDemand);
                infoHygieneLabel.text = NeedLine("위생", selectedTile.HygieneCoverage, selectedTile.InfraDemand);
                infoTransitLabel.text = NeedLine("교통", selectedTile.TransitCoverage, selectedTile.InfraDemand);
            }
            else if (CityGridModel.IsUtilityKind(selectedTile.Kind))
            {
                infoPopulationLabel.text = $"근무 인원 {selectedTile.Population}/{selectedTile.Capacity}명";
                infoTierLabel.text = $"규모: {TierLabel(selectedTile.Tier)}";
                infoLaborLabel.text = LaborLine();
                ClearWealthAndNeeds();
            }
            else
            {
                infoPopulationLabel.text = "";
                infoTierLabel.text = "";
                infoLaborLabel.text = "";
                ClearWealthAndNeeds();
            }
        }

        string LaborLine()
        {
            float labor = TransitCityManager.Instance.Model.LaborAvailability;
            string state = labor >= 0.8f ? "정상" : labor >= 0.4f ? "인력난" : "인력 부족으로 사실상 정지";
            return $"인력 수급: {Mathf.RoundToInt(labor * 100)}% ({state})";
        }

        void ClearWealthAndNeeds()
        {
            infoWealthLabel.text = "";
            infoSatisfactionLabel.text = "";
            infoWaterLabel.text = infoSewageLabel.text = infoPowerLabel.text = infoPoliceLabel.text = infoFireLabel.text = "";
            infoEducationLabel.text = infoMedicalLabel.text = infoConsumptionLabel.text = infoJobsLabel.text = "";
            infoLeisureLabel.text = infoHygieneLabel.text = infoTransitLabel.text = "";
        }

        static string NeedLine(string label, float coverage, float demand)
        {
            string state = coverage >= demand ? "충족" : "부족";
            return $"{label}: {Mathf.RoundToInt(coverage * 100)}% ({state})";
        }

        static string TierLabel(int tier) => tier switch
        {
            1 => "소형",
            2 => "중형",
            3 => "대형",
            _ => tier.ToString(),
        };

        static string DensityTierLabel(int tier) => tier switch
        {
            1 => "저밀도",
            2 => "중밀도",
            3 => "고밀도",
            _ => tier.ToString(),
        };

        static string IncomeTierLabel(IncomeTier income) => income switch
        {
            IncomeTier.Low => "저소득",
            IncomeTier.Middle => "중소득",
            IncomeTier.High => "고소득",
            _ => income.ToString(),
        };

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
            _ => "건물 정보",
        };

        void RefreshStats()
        {
            var model = TransitCityManager.Instance.Model;
            populationLabel.text = $"인구 {model.Population}";
            moneyLabel.text = $"자금 {model.Money}원";
            if (selectedTile != null) RefreshInfoPanel();
        }

        /// <summary>
        /// 시각(하루 시간대)은 성장 틱과 별개로 매 프레임 계속 흐르므로,
        /// RefreshStats(틱마다 한 번)가 아니라 여기서 매 프레임 갱신해야
        /// 시계가 뚝뚝 끊기지 않고 부드럽게 간다.
        /// </summary>
        void Update()
        {
            var manager = TransitCityManager.Instance;
            if (manager == null || manager.Model == null || dayLabel == null) return;
            dayLabel.text = $"Day {manager.Model.Day} · {FormatTimeOfDay(manager.Model.TimeOfDayHours)}";
        }

        static string FormatTimeOfDay(float hours)
        {
            int h = Mathf.FloorToInt(hours) % 24;
            int m = Mathf.FloorToInt((hours - Mathf.Floor(hours)) * 60f);
            return $"{h:00}:{m:00}";
        }
    }
}
