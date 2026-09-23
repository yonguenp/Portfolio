using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace SamgukDefense
{
    /// <summary>
    /// 화면 구성 전담 — 실제 전투/라운드 로직은 전혀 안 건드린다(SamgukBattleManager.cs 참고).
    /// 절차적으로 그림을 그리는 코드는 없다(요청에 따라 금지) — 전부 단색 Image + TMP 텍스트뿐이다.
    /// 공용 GameUI.prefab/GameUIManager를 전혀 안 쓴다 — Canvas/Camera/EventSystem까지 이
    /// 파일이 런타임에 직접 만든다(완전 독립 프로젝트 요청에 따름).
    /// </summary>
    public partial class SamgukBattleManager
    {
        static readonly Color ColorBg = new Color(0.05f, 0.05f, 0.06f);
        static readonly Color ColorPanel = new Color(0.12f, 0.11f, 0.09f, 0.96f);
        static readonly Color ColorAccent = new Color(0.85f, 0.66f, 0.24f);
        static readonly Color ColorTextMain = new Color(1f, 1f, 1f, 0.95f);
        static readonly Color ColorTextSub = new Color(1f, 1f, 1f, 0.7f);
        static readonly Color ColorButton = new Color(0.20f, 0.42f, 0.24f);
        static readonly Color ColorButtonDisabled = new Color(0.22f, 0.22f, 0.22f);
        static readonly Color ColorDim = new Color(0f, 0f, 0f, 0.72f);

        static TMP_FontAsset koreanFont;

        /// <summary>씬 부트스트랩(Canvas/EventSystem/Camera/Light) — 씬에 실제로 배치해 두면
        /// 그걸 그대로 쓰고, 비어 있으면(예전처럼) 코드로 새로 만든다(2026-09-20, "코드에서
        /// 만들어내는 걸 전부 씬 GameObject/프리팹으로" 지시에 따른 전환 — ssam.md 96.16 참고).
        /// 이름으로 `GameObject.Find`하던 예전 방식은 폴백으로만 남겨서, 이 필드들을 아직 안
        /// 연결한 씬을 열어도 회귀 없이 그대로 동작한다.</summary>
        [Header("씬 부트스트랩 — 있으면 재사용, 비어 있으면 코드로 생성(96.16)")]
        [SerializeField] GameObject sceneCanvas;
        [SerializeField] GameObject sceneEventSystem;
        [SerializeField] GameObject sceneMainCamera;
        [SerializeField] GameObject sceneBattleSun;

        RectTransform canvasRoot;

        TMP_Text roundText, goldText, monsterCountText;
        readonly List<(float mult, RectTransform rt, Image img)> speedButtons = new List<(float, RectTransform, Image)>();
        RectTransform synergyPanel;
        RectTransform startBattleButton;
        TMP_Text startBattleLabel;
        RectTransform placementHint;
        RectTransform roundClearToast;
        TMP_Text roundClearLabel;

        RectTransform characterSelectPanel;
        RectTransform characterListArea;
        RectTransform characterListContent;
        RectTransform characterCardTemplate;
        RectTransform confirmSelectButton;
        readonly List<(SamgukCharacterData data, Image card, TMP_Text label)> characterCards = new List<(SamgukCharacterData, Image, TMP_Text)>();

        RectTransform gameOverPanel;
        TMP_Text gameOverRoundText, gameOverGoldText, gameOverTotalGoldText;

        void BuildStaticUI()
        {
            EnsureEventSystem();
            EnsureCamera();
            EnsureLight();

            // 씬 참조(sceneCanvas) 우선, 없으면 이름으로 찾고(예전 방식), 그마저 없으면 새로
            // 만든다 — 최상위 오브젝트들은 MakeRect의 이름-재사용 로직을 못 타므로 각자 이
            // 3단 폴백을 직접 챙겨야 한다. 안 그러면 씬에 구워둔 뒤 Play할 때마다 Canvas가
            // 통째로 하나씩 더 생긴다.
            var existingCanvas = sceneCanvas != null ? sceneCanvas : GameObject.Find("SamgukCanvas");
            GameObject canvasGo;
            if (existingCanvas != null)
            {
                canvasGo = existingCanvas;
                canvasRoot = canvasGo.GetComponent<RectTransform>();
            }
            else
            {
                canvasGo = new GameObject("SamgukCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasRoot = canvasGo.GetComponent<RectTransform>();
                var canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }

            // 2026-09-16 3D 전환 이후 — 이 Bg는 "노치 뒤까지 덮는 배경색" 용도였는데,
            // ScreenSpaceOverlay 캔버스는 카메라 깊이와 무관하게 항상 모든 카메라 출력보다
            // 나중에 합성된다. 즉 이 화면 전체를 덮는 불투명 사각형이 battleCamera가 그린 3D
            // 전투 장면을 통째로 가려버렸다(화면이 새까맣게만 보이던 원인). 지금은 카메라 자신의
            // clearFlags=SolidColor+backgroundColor가 정확히 같은 색으로 같은 역할(노치 주변
            // 여백 포함 화면 전체 클리어)을 대신하므로, 이 오브젝트는 만들되 비활성으로 둔다.
            var bg = MakeRect(canvasRoot, "Bg", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddImage(bg, ColorBg);
            bg.gameObject.SetActive(false);

            // Bg는 노치/홈 인디케이터 뒤까지 화면 전체를 덮어야 하므로 진짜 Canvas 밑에 그대로
            // 두고, 그 아래 나머지 UI(TopBar/Board/BottomBar/각종 패널)는 전부 SafeArea 컴포넌트가
            // 붙은 별도 컨테이너 밑으로 옮긴다 — canvasRoot 필드를 여기서 리다이렉트해서, 아래
            // BuildXxx() 호출들(과 그 안의 MakeRect(canvasRoot, ...))을 하나도 안 건드려도
            // 전부 자동으로 SafeArea 밑에 붙는다.
            var safeAreaRoot = MakeRect(canvasRoot, "SafeArea", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            if (safeAreaRoot.GetComponent<SafeArea>() == null) safeAreaRoot.gameObject.AddComponent<SafeArea>();
            canvasRoot = safeAreaRoot;

            BuildTopBar();
            BuildSpeedBar();
            BuildSynergyPanel();
            BuildBoard();
            BuildBottomBar();
            BuildInventoryPanel();
            BuildRoundClearToast();
            BuildCharacterSelectPanel();
            BuildGameOverPanel();
            BuildLobbyPanel();
        }

        void EnsureEventSystem()
        {
            // 씬 참조 우선 — 이미 컴포넌트까지 갖춘 씬 오브젝트라 재사용만 하면 끝.
            if (sceneEventSystem != null) return;
            // .current는 도메인 리로드 직후(에디터에서 재컴파일 등) 아직 안 정착됐을 수 있어서
            // 이름으로도 한 번 더 확인한다 — 안 그러면 씬에 이미 있는데도 하나 더 만든다.
            if (EventSystem.current != null || GameObject.Find("EventSystem") != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            // DontDestroyOnLoad는 플레이 모드 밖(에디터 씬 굽기 등)에서 부르면 예외를 던진다 —
            // 이 게임은 씬이 하나뿐이라 애초에 살아남길 필요가 없으므로 플레이 모드에서만 건다.
            if (Application.isPlaying) DontDestroyOnLoad(go);
        }

        Camera battleCamera;

        /// <summary>2026-09-16 3D 전환 — 예전엔 이 카메라가 cullingMask=0(아무것도 안 그림, UGUI
        /// 전용 프로젝트라 형식상 존재)이었는데, 이제 전투 화면 자체가 이 카메라가 찍는 3D
        /// 장면이다. Perspective로 바꿔야 줌에 따른 피치 변화가 입체감 있게 보인다(Orthographic은
        /// 기울여도 원근감이 없어 밋밋하다).</summary>
        void EnsureCamera()
        {
            // 씬에 이미 "Main Camera"가 구워져 있으면(2026-09-16 3D 전환 이전 세션의 UGUI 전용
            // 카메라 — orthographic=true, cullingMask=0) 오브젝트는 재사용하되 설정은 항상
            // 강제로 다시 맞춘다. GameObject 재사용 로직(=중복 생성 방지)과 설정 적용을 분리해야
            // "예전에 구운 값이 그대로 남아 아무것도 안 그려지는" 사고를 구조적으로 막는다.
            var existing = sceneMainCamera != null ? sceneMainCamera : GameObject.Find("Main Camera");
            GameObject go;
            if (existing != null) { go = existing; }
            else { go = new GameObject("Main Camera", typeof(Camera)); go.tag = "MainCamera"; }
            if (go.GetComponent<Camera>() == null) go.AddComponent<Camera>();
            battleCamera = go.GetComponent<Camera>();
            battleCamera.clearFlags = CameraClearFlags.SolidColor;
            battleCamera.backgroundColor = ColorBg;
            battleCamera.orthographic = false;
            battleCamera.fieldOfView = 50f;
            battleCamera.nearClipPlane = 0.3f;
            battleCamera.farClipPlane = 200f;
            battleCamera.cullingMask = ~0;

            // 이 프로젝트의 기본 URP 렌더러는 Renderer2D(2D 전용 — UnityEngine.Light를 아예 안
            // 읽는다) 하나뿐이었다. Lit 셰이더+디렉셔널 라이트로 만든 3D 바닥이 화면에 완전히
            // 안 그려지던(까맣게만 나옴) 원인이 이것이었다 — 다른 2D 게임들에 영향 없이 이 카메라
            // 하나만 별도 3D용 Universal Renderer(인덱스 1, 에디터에서 한 번 등록해 둠)를 쓰도록
            // 오버라이드한다.
            var camData = go.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null) camData = go.AddComponent<UniversalAdditionalCameraData>();
            camData.renderType = CameraRenderType.Base;
            camData.SetRenderer(1);
        }

        /// <summary>단일 디렉셔널 라이트 — 실시간 그림자는 안 켠다(블롭 섀도로 대체, BrickBreaker3D
        /// 문서의 확립된 원칙과 동일 — Built-in RP에서 다수 스프라이트 위 실시간 그림자는 비용 대비
        /// 이득이 적다). 빛은 왼쪽 위 앞에서 — 이 프로젝트 전역에서 이미 쓰는 관례.</summary>
        void EnsureLight()
        {
            if (sceneBattleSun != null || GameObject.Find("BattleSun") != null) return;
            var go = new GameObject("BattleSun", typeof(Light));
            var light = go.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.97f, 0.9f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
            go.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.21f, 0.24f);
        }

        void BuildTopBar()
        {
            var bar = MakeRect(canvasRoot, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 90f));
            AddImage(bar, ColorPanel);

            var roundRt = MakeRect(bar, "Round", new Vector2(0f, 0f), new Vector2(0.34f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            roundText = AddLabel(roundRt, "ROUND 1", 34f, ColorAccent, TextAlignmentOptions.Left);
            (roundText.rectTransform).offsetMin += new Vector2(30f, 0f);

            var goldRt = MakeRect(bar, "Gold", new Vector2(0.34f, 0f), new Vector2(0.66f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            goldText = AddLabel(goldRt, "GOLD 100", 30f, ColorTextMain, TextAlignmentOptions.Center, autoSize: true);

            var monsterRt = MakeRect(bar, "MonsterCount", new Vector2(0.66f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            monsterCountText = AddLabel(monsterRt, "몬스터 20 / 20", 28f, ColorTextSub, TextAlignmentOptions.Right, autoSize: true);
            (monsterCountText.rectTransform).offsetMax -= new Vector2(30f, 0f);
        }

        /// <summary>TopBar 바로 아래(우측 정렬)에 붙는 작은 배속 버튼 4개. TopBar와 마찬가지로
        /// BuildStaticUI 초반에 만들어지므로, 로비/캐릭터선택/게임오버의 전체화면 Dim 패널들이
        /// 나중에 더 높은 sibling으로 그 위를 덮어 자동으로 가려진다(TopBar/보드/BottomBar가
        /// 이미 그렇게 동작하는 것과 동일한 원리 — 상태별 show/hide를 따로 안 만들어도 된다).</summary>
        void BuildSpeedBar()
        {
            var top = canvasRoot.Find("TopBar") as RectTransform;
            if (top == null)
                return;

            var bar = MakeRect(top, "SpeedBar", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -100f), new Vector2(272f, 46f));

            (float mult, string label)[] options = { (1f, "1x"), (1.5f, "1.5x"), (2f, "2x"), (3f, "3x") };
            float btnW = 62f, gap = 6f;
            speedButtons.Clear();
            for (int i = 0; i < options.Length; i++)
            {
                var (mult, label) = options[i];
                float x = -bar.sizeDelta.x * 0.5f + btnW * 0.5f + i * (btnW + gap);
                var rt = MakeRect(bar, $"Speed_{label}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(btnW, 46f));
                var img = AddImage(rt, ColorButton, true);
                var btn = GetOrAddButton(rt, img);
                float captured = mult;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnBattleSpeedSelected(captured));
                AddLabel(rt, label, 20f, Color.white);
                speedButtons.Add((mult, rt, img));
            }
            RefreshBattleSpeedButtons();
        }

        void RefreshBattleSpeedButtons()
        {
            foreach (var (mult, rt, img) in speedButtons)
                img.color = Mathf.Approximately(mult, battleSpeedMultiplier) ? ColorAccent : ColorButton;
        }

        /// <summary>TopBar 바로 아래(좌측 정렬) 빈 컨테이너만 만들어 둔다 — SpeedBar와 좌우
        /// 대칭 위치. 실제 행은 RefreshSynergyPanel()이 채운다(활성 시너지는 런 시작 시점에만
        /// 정해지므로 BuildStaticUI 시점엔 아직 비어 있을 수밖에 없다).</summary>
        void BuildSynergyPanel()
        {
            synergyPanel = MakeRect(canvasRoot, "SynergyPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -100f), new Vector2(420f, 0f));
        }

        /// <summary>현재 런에서 실제로 발동 중인 시너지만 나열한다(전체 카탈로그가 아니라
        /// "발동되면 표시"라는 요청 그대로) — BeginRun()이 ComputeActiveSynergies() 직후 부른다.
        /// 팀 구성은 런 도중 안 바뀌므로(전투 시작 후엔 재선택 불가) 런당 한 번만 채우면 된다.</summary>
        void RefreshSynergyPanel()
        {
            foreach (RectTransform child in synergyPanel) SafeDestroy(child.gameObject);
            if (activeSynergies.Count == 0) { synergyPanel.sizeDelta = new Vector2(synergyPanel.sizeDelta.x, 0f); return; }

            const float rowH = 56f, gap = 6f;
            float y = 0f;
            foreach (var syn in activeSynergies)
            {
                var row = MakeRect(synergyPanel, $"Row_{syn.id}", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(0f, rowH));
                AddImage(row, new Color(0.16f, 0.14f, 0.08f, 0.88f));

                var title = MakeRect(row, "Title", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(14f, -4f), new Vector2(-14f, 26f));
                // 이모지(⚡ 등)는 이 프로젝트 TMP 폰트에 없는 글리프라 □로 깨진다(이번 세션
                // 콘솔에도 같은 원인의 경고가 실제로 찍혔다) — 순수 텍스트+골드 강조색으로만 표시.
                AddLabel(title, $"[시너지] {syn.displayName} ({SynergyEffectSummary(syn)})", 20f, ColorAccent, TextAlignmentOptions.Left, autoSize: true);

                var desc = MakeRect(row, "Desc", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(14f, 4f), new Vector2(-14f, 22f));
                AddLabel(desc, string.IsNullOrEmpty(syn.description) ? "" : syn.description, 15f, ColorTextSub, TextAlignmentOptions.Left, autoSize: true);

                y -= rowH + gap;
            }
            synergyPanel.sizeDelta = new Vector2(synergyPanel.sizeDelta.x, -y);
        }

        static string SynergyEffectSummary(SamgukSynergyData syn)
        {
            string statName = syn.effectType switch
            {
                SamgukSynergyEffectType.HPPercent => "체력",
                SamgukSynergyEffectType.AttackPercent => "공격력",
                SamgukSynergyEffectType.DefensePercent => "방어력",
                SamgukSynergyEffectType.AttackSpeedPercent => "공격속도",
                SamgukSynergyEffectType.MoveSpeedPercent => "이동속도",
                SamgukSynergyEffectType.SkillEffectPercent => "스킬 효과",
                SamgukSynergyEffectType.GoldPercent => "골드 획득",
                _ => "?"
            };
            return $"{statName} +{Mathf.RoundToInt(syn.effectValue * 100f)}%";
        }

        /// <summary>2026-09-16 3D 전환 — 예전엔 UGUI 뷰포트에 마스크를 씌워 400개 Image 셀로
        /// 그렸는데, 이제 전투 화면 전체가 battleCamera가 찍는 3D 장면이다(캔버스와 완전 독립,
        /// HUD 패널들만 그 위에 겹쳐 그려진다 — ScreenSpaceOverlay 캔버스는 원래 모든 카메라
        /// 출력보다 나중에 합성되므로 별도 카메라 스택 설정 없이도 자연히 HUD가 맨 위에 뜬다).
        /// 유닛 사이 간격 감각을 유지하려고 cellSize는 "월드 유닛" 단위로 재해석했다(1유닛=1셀).</summary>
        void BuildBoard()
        {
            var battleRootGo = GameObject.Find("BattleRoot");
            var battleRoot = battleRootGo != null ? battleRootGo.transform : new GameObject("BattleRoot").transform;

            cellSize = 1f;
            grid = new SamgukGridBoard(battleRoot, GRID_SIZE, cellSize);
            SamgukUnit.BoardHalfExtent = grid.BoardHalfExtent;

            var panZoom = battleCamera.gameObject.GetComponent<SamgukBoardPanZoom>();
            if (panZoom == null) panZoom = battleCamera.gameObject.AddComponent<SamgukBoardPanZoom>();
            panZoom.Setup(battleCamera, grid);
        }

        void BuildBottomBar()
        {
            var bar = MakeRect(canvasRoot, "BottomBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 110f));
            AddImage(bar, ColorPanel);

            startBattleButton = MakeRect(bar, "StartBattleBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 76f));
            var img = AddImage(startBattleButton, ColorButton, true);
            var btn = GetOrAddButton(startBattleButton, img);
            btn.onClick.AddListener(OnStartBattleClicked);
            startBattleLabel = AddLabel(startBattleButton, "전투 시작", 32f, Color.white);

            placementHint = MakeRect(bar, "Hint", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(800f, 30f));
            AddLabel(placementHint, "캐릭터를 드래그해 배치를 조정할 수 있습니다", 22f, ColorTextSub);
        }

        void BuildRoundClearToast()
        {
            roundClearToast = MakeRect(canvasRoot, "RoundClearToast", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(500f, 60f));
            var img = AddImage(roundClearToast, new Color(0f, 0f, 0f, 0.7f));
            img.raycastTarget = false;
            roundClearLabel = AddLabel(roundClearToast, "ROUND CLEAR", 34f, ColorAccent);
            roundClearToast.gameObject.SetActive(false);
        }

        void BuildCharacterSelectPanel()
        {
            characterSelectPanel = MakeRect(canvasRoot, "CharacterSelectPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddImage(characterSelectPanel, ColorDim, true);

            var panel = MakeRect(characterSelectPanel, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 760f));
            AddImage(panel, ColorPanel, true);

            var title = MakeRect(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 50f));
            AddLabel(title, "출전할 장수를 선택하세요", 34f, ColorAccent); // 정확한 인원 수는 ShowCharacterSelectUI가 매번 갱신

            // 보유 캐릭터가 늘어나면(뽑기 등) 한 줄에 다 못 들어갈 수 있으므로 가로 ScrollRect로 —
            // 예전엔 그냥 폭을 늘리기만 해서 카드 개수가 많아지면 패널 밖으로 넘쳤다.
            characterListArea = BuildHorizontalScrollArea(panel, "List", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1320f, 520f), out characterListContent);

            // 카드 원본(템플릿) — 비활성 상태로 List 밑에 그대로 둔다. 실제 카드는 이걸
            // Instantiate해서 만든다 — 에디터에서 이 템플릿 하나만 고치면(색/크기/폰트 등)
            // 카드 전체 디자인이 한 번에 바뀐다.
            const float cardW = 200f, cardH = 500f;
            characterCardTemplate = MakeRect(characterListContent, "CardTemplate", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(cardW, cardH));
            var templateImg = AddImage(characterCardTemplate, new Color(0.18f, 0.17f, 0.14f), true);
            GetOrAddButton(characterCardTemplate, templateImg);
            var templateSwatch = MakeRect(characterCardTemplate, "Swatch", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(cardW - 40f, cardW - 40f));
            AddImage(templateSwatch, Color.gray);
            var templateName = MakeRect(characterCardTemplate, "Name", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(cardW - 20f, 40f));
            AddLabel(templateName, "이름", 26f, ColorTextMain, TextAlignmentOptions.Center, autoSize: true);
            var templateStar = MakeRect(characterCardTemplate, "Star", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(cardW - 20f, 30f));
            AddLabel(templateStar, "★★★", 20f, ColorAccent);
            var templateStats = MakeRect(characterCardTemplate, "Stats", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(cardW - 20f, 110f));
            AddLabel(templateStats, "HP 0\n공격 0\n방어 0", 18f, ColorTextSub);
            characterCardTemplate.gameObject.SetActive(false);

            confirmSelectButton = MakeRect(panel, "Confirm", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(320f, 76f));
            var confirmImg = AddImage(confirmSelectButton, ColorButtonDisabled, true);
            var confirmBtn = GetOrAddButton(confirmSelectButton, confirmImg);
            confirmBtn.onClick.AddListener(OnConfirmCharacterSelect);
            AddLabel(confirmSelectButton, "전투 준비", 30f, Color.white);

            // 다른 오버레이 패널들처럼 기본은 숨김 — Show/HideXxxUI가 실제 전환 시점에 켠다.
            // (에디터에서 씬을 열었을 때 이 패널이 항상 맨 위에 떠 있는 것처럼 보이지 않도록.)
            characterSelectPanel.gameObject.SetActive(false);
        }

        void ShowCharacterSelectUI(SamgukCharacterData[] roster)
        {
            gameOverPanel.gameObject.SetActive(false);
            if (lobbyPanel != null) lobbyPanel.gameObject.SetActive(false);
            characterSelectPanel.gameObject.SetActive(true);
            AddLabel(characterSelectPanel.Find("Panel/Title") as RectTransform, $"출전할 장수를 선택하세요 (최대 {TeamSizeLevel}명)", 34f, ColorAccent);
            foreach (RectTransform t in characterListContent)
                if (t != characterCardTemplate) SafeDestroy(t.gameObject);
            characterCards.Clear();

            if (roster == null) { SetHorizontalContentExtent(characterListContent, characterListArea.sizeDelta.x, 0f); return; }
            int n = roster.Length;
            float cardW = characterCardTemplate.sizeDelta.x, gap = 16f;
            float total = n * cardW + Mathf.Max(0, n - 1) * gap;
            for (int i = 0; i < n; i++)
            {
                var data = roster[i];
                float x = -total * 0.5f + cardW * 0.5f + i * (cardW + gap);

                var card = Instantiate(characterCardTemplate, characterListContent);
                card.name = $"Card_{data.id}";
                card.anchoredPosition = new Vector2(x, 0f);
                card.gameObject.SetActive(true);

                var cardImg = card.GetComponent<Image>();
                ApplyPortraitSwatch(card.Find("Swatch").GetComponent<Image>(), data.icon, data.tintColor);
                var nameLabel = card.Find("Name/Label").GetComponent<TMP_Text>();
                nameLabel.text = data.displayName;
                card.Find("Star/Label").GetComponent<TMP_Text>().text = new string('★', Mathf.Clamp(data.star, 1, 6));
                card.Find("Stats/Label").GetComponent<TMP_Text>().text = $"HP {data.baseHP:0}\n공격 {data.baseAttack:0}\n방어 {data.baseDefense:0}";

                var btn = card.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                var captured = data;
                var capturedImg = cardImg;
                btn.onClick.AddListener(() =>
                {
                    bool nowSelected = !selectedCharacters.Contains(captured);
                    OnCharacterToggled(captured, nowSelected);
                    RefreshCardVisual(captured, capturedImg);
                });
                characterCards.Add((data, cardImg, nameLabel));
                RefreshCardVisual(data, cardImg);
            }
            SetHorizontalContentExtent(characterListContent, characterListArea.sizeDelta.x, total);
            RefreshCharacterSelectConfirmButton(false);
        }

        void RefreshCardVisual(SamgukCharacterData data, Image cardImg)
        {
            bool selected = selectedCharacters.Contains(data);
            cardImg.color = selected ? new Color(ColorAccent.r, ColorAccent.g, ColorAccent.b, 0.85f) : new Color(0.18f, 0.17f, 0.14f);
        }

        void RefreshCharacterSelectConfirmButton(bool interactable)
        {
            var img = confirmSelectButton.GetComponent<Image>();
            var btn = confirmSelectButton.GetComponent<Button>();
            btn.interactable = interactable;
            img.color = interactable ? ColorButton : ColorButtonDisabled;
        }

        void HideCharacterSelectUI()
        {
            characterSelectPanel.gameObject.SetActive(false);
        }

        void BuildGameOverPanel()
        {
            gameOverPanel = MakeRect(canvasRoot, "GameOverPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddImage(gameOverPanel, ColorDim, true);

            var panel = MakeRect(gameOverPanel, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 560f));
            AddImage(panel, ColorPanel, true);

            var title = MakeRect(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(500f, 60f));
            AddLabel(title, "GAME OVER", 44f, ColorAccent);

            var roundRt = MakeRect(panel, "Round", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(500f, 50f));
            gameOverRoundText = AddLabel(roundRt, "최종 라운드: 1", 30f, ColorTextMain);

            var goldRt = MakeRect(panel, "Gold", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(500f, 50f));
            gameOverGoldText = AddLabel(goldRt, "획득 골드: 0", 30f, ColorTextSub, TextAlignmentOptions.Center, autoSize: true);

            var totalGoldRt = MakeRect(panel, "TotalGold", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(500f, 50f));
            gameOverTotalGoldText = AddLabel(totalGoldRt, "로비 골드: 0", 26f, ColorAccent, TextAlignmentOptions.Center, autoSize: true);

            var restartRt = MakeRect(panel, "Restart", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(300f, 76f));
            var restartImg = AddImage(restartRt, ColorButton, true);
            var restartBtn = GetOrAddButton(restartRt, restartImg);
            restartBtn.onClick.AddListener(OnRestartClicked);
            AddLabel(restartRt, "로비로", 30f, Color.white);

            gameOverPanel.gameObject.SetActive(false);
        }

        void ShowGameOverUI(int finalRound, int finalGold, int totalGold)
        {
            gameOverRoundText.text = $"최종 라운드: {finalRound}";
            gameOverGoldText.text = $"획득 골드: {finalGold}";
            gameOverTotalGoldText.text = $"로비 골드: {totalGold}";
            gameOverPanel.gameObject.SetActive(true);
        }

        void ShowPlacementUI(int r, int g, int monsterCount)
        {
            roundClearToast.gameObject.SetActive(false);
            startBattleButton.gameObject.SetActive(true);
            placementHint.gameObject.SetActive(true);
            inventoryButton.gameObject.SetActive(true);
            UpdateBattleHud(r, g, monsterCount);
        }

        void HidePlacementUI()
        {
            startBattleButton.gameObject.SetActive(false);
            placementHint.gameObject.SetActive(false);
            inventoryButton.gameObject.SetActive(false);
            inventoryPanel.gameObject.SetActive(false);
        }

        void UpdateBattleHud(int r, int g, int monsterCount)
        {
            roundText.text = $"ROUND {r}";
            goldText.text = $"GOLD {g}";
            monsterCountText.text = $"몬스터 {monsterCount} / {(balance != null ? balance.monstersPerRound : 20)}";
        }

        void ShowRoundClearToast(int r)
        {
            roundClearLabel.text = $"ROUND {r} CLEAR";
            roundClearToast.gameObject.SetActive(true);
        }

        // ── 로컬 UI 헬퍼 (다른 게임의 공용 UISkin/HwatuUI를 안 쓰고 완전히 독립적으로 구성) ──
        //
        // 전부 "씬(또는 템플릿)에 이미 같은 이름의 자식이 있으면 그걸 그대로 쓰고, 없을 때만
        // 코드 기본값으로 새로 만든다" — 이러면 BuildStaticUI()가 매 세션 다시 돌아도
        // 사용자가 에디터에서 색/크기/위치를 바꿔둔 값을 덮어쓰지 않는다. 단, 이 idempotent
        // 동작은 "한 번만 만들어지는 정적 UI(패널·버튼·타이틀·템플릿 원본)"에만 쓴다 —
        // 캐릭터 카드/아이템 행처럼 데이터 개수만큼 반복 생성되는 것들은 이 헬퍼 대신
        // 템플릿을 Instantiate한 뒤 컴포넌트를 직접 찾아 값을 쓰는 방식으로 처리한다
        // (그래야 스와치 색 같은 데이터 종속 값이 템플릿의 기본값에 항상 가려지는 걸 피한다).

        static RectTransform MakeRect(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null) return existing;
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return rt;
        }

        static Image AddImage(RectTransform rt, Color color, bool raycast = false)
        {
            var img = rt.GetComponent<Image>();
            if (img != null) return img;
            img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        /// <summary>라벨은 항상 별도 자식 오브젝트로 만든다 — TextMeshProUGUI도 Image처럼
        /// Graphic을 상속해서, 이미 Image가 붙은 같은 오브젝트에 더하면 Unity가 예외 없이
        /// 조용히 null을 돌려준다(버튼 배경+캡션을 한 오브젝트에 합치려다 실제로 겪은 버그).
        /// 폰트·크기·색·정렬은 이미 있으면 에디터에서 정한 값을 존중해 안 건드리지만, 텍스트
        /// 내용(text)만은 항상 인자로 받은 값으로 덮어쓴다 — 그렇지 않으면 "매번 자기 자식을
        /// Destroy하고 다시 짓는" Rebuild 계열 함수들(RebuildEquipList 등)에서 Destroy가
        /// 프레임 끝까지 지연 실행되는 동안 아직 안 지워진 이전 라벨을 "재사용"으로 잘못
        /// 판단해 새 값(예: 캐릭터 HP)이 반영이 안 되는 사고가 난다(실제로 겪은 버그).</summary>
        /// <summary>autoSize=true면 텍스트가 박스 폭을 넘길 만큼 길어져도(캐릭터/아이템 이름
        /// 등 데이터에 따라 길이가 달라지는 텍스트) 글자를 줄여서 항상 박스 안에 들어가게 한다 —
        /// 줄바꿈 대신 축소를 쓰는 이유는 카드/행처럼 세로 공간이 빠듯한 곳에서 줄바꿈은 다른
        /// 요소와 겹치기 쉽기 때문. 고정 문구(버튼 캡션 등)에는 안 쓴다(그 자리에 항상 들어가는
        /// 걸 이미 알고 있으므로).</summary>
        static TMP_Text AddLabel(RectTransform rt, string text, float fontSize, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, bool autoSize = false)
        {
            var existingLabelRt = rt.Find("Label") as RectTransform;
            if (existingLabelRt != null)
            {
                var existingLabel = existingLabelRt.GetComponent<TMP_Text>();
                if (existingLabel != null)
                {
                    existingLabel.text = text;
                    return existingLabel;
                }
            }
            if (koreanFont == null) koreanFont = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/ONE Mobile POP SDF");
            var labelRt = MakeRect(rt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var label = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            if (koreanFont != null) label.font = koreanFont;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = align;
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            if (autoSize)
            {
                label.enableAutoSizing = true;
                label.fontSizeMin = Mathf.Max(10f, fontSize * 0.45f);
                label.fontSizeMax = fontSize;
            }
            return label;
        }

        static Button GetOrAddButton(RectTransform rt, Graphic targetGraphic)
        {
            var btn = rt.GetComponent<Button>();
            if (btn == null) btn = rt.gameObject.AddComponent<Button>();
            if (btn.targetGraphic == null) btn.targetGraphic = targetGraphic;
            return btn;
        }

        /// <summary>Destroy()는 플레이 모드 밖(에디터에서 BuildStaticUI 등을 직접 불러 씬을
        /// 구울 때)에 부르면 "Destroy may not be called from edit mode!"만 찍고 조용히
        /// 무시된다 — 실제로 SamgukGridBoard.BuildFloor의 콜라이더 제거가 이렇게 안 지워진 채로
        /// 씬에 저장되는 사고가 났었다(2026-09-16). 데이터 반복 목록을 다시 그리는 모든
        /// Destroy(child.gameObject) 호출을 여기 하나로 통일해 같은 사고를 구조적으로 막는다.</summary>
        static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        /// <summary>data-driven 색(스와치·선택 상태 등)은 AddImage의 idempotent 규칙을
        /// 우회해서 항상 덮어써야 하므로, 순수 get-or-add만 하는 버전을 따로 둔다.</summary>
        static Image GetOrAddImage(RectTransform rt)
        {
            var img = rt.GetComponent<Image>();
            if (img == null) img = rt.gameObject.AddComponent<Image>();
            return img;
        }

        /// <summary>캐릭터 카드의 "Swatch" 칸 — 아이콘이 있으면 실제 스프라이트(원본 색 그대로,
        /// preserveAspect)로, 없으면 기존처럼 tintColor를 칠한 단색 사각형으로 대체한다.
        /// 캐릭터 선택 화면(UI.cs)과 로비 보유 목록(MetaUI.cs) 카드가 이 하나를 공유한다.</summary>
        static void ApplyPortraitSwatch(Image swatchImg, Sprite icon, Color tintColor)
        {
            if (icon != null)
            {
                swatchImg.sprite = icon;
                swatchImg.color = Color.white;
                swatchImg.preserveAspect = true;
            }
            else
            {
                swatchImg.sprite = null;
                swatchImg.color = tintColor;
            }
        }

        // ── 스크롤 목록 헬퍼 — 캐릭터 목록/인벤토리/장비처럼 데이터 개수가 화면 크기를 넘길 수
        // 있는 목록은 전부 이 헬퍼로 진짜 ScrollRect(Root→Viewport(RectMask2D)→Content)를 만든다.
        // Content 자체에는 LayoutGroup을 안 건다 — 각 Rebuild 함수가 이미 카드/행 간격을
        // 정확히 계산해서 anchoredPosition을 직접 찍고 있으므로(에디터에서 템플릿 크기를
        // 바꿔도 그 계산이 그대로 따라간다), 대신 그 계산 결과(총 콘텐츠 크기)를 Content.sizeDelta
        // 에 반영해서 스크롤 가능 범위 + 클리핑만 추가로 얹는다. ──

        /// <summary>세로 스크롤 목록. Rebuild 함수는 반환된 content를 부모로 행을 배치하고,
        /// 끝에서 SetVerticalContentExtent(content, viewportHeight, usedHeight)를 불러야 한다.</summary>
        static RectTransform BuildVerticalScrollArea(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, out RectTransform content)
        {
            var root = MakeRect(parent, name, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            var viewport = MakeRect(root, "Viewport", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddImage(viewport, new Color(0f, 0f, 0f, 0.001f), true); // 드래그 스크롤이 되려면 raycast는 받아야 함
            if (viewport.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();

            content = MakeRect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, sizeDelta.y));

            var scroll = root.GetComponent<ScrollRect>();
            if (scroll == null) scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;
            return root;
        }

        /// <summary>가로 스크롤 목록(캐릭터 카드 줄 등). 구조는 세로 버전과 동일하되 축만 바뀐다.</summary>
        static RectTransform BuildHorizontalScrollArea(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, out RectTransform content)
        {
            var root = MakeRect(parent, name, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta);
            var viewport = MakeRect(root, "Viewport", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddImage(viewport, new Color(0f, 0f, 0f, 0.001f), true);
            if (viewport.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();

            content = MakeRect(viewport, "Content", new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(sizeDelta.x, 0f));

            var scroll = root.GetComponent<ScrollRect>();
            if (scroll == null) scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;
            return root;
        }

        /// <summary>실제로 쓴 높이가 뷰포트보다 크면 그만큼 Content를 늘리고, 아니면 뷰포트
        /// 크기 그대로 둔다(콘텐츠가 적을 때 괜히 스크롤이 "가능한 척"하지 않도록).</summary>
        static void SetVerticalContentExtent(RectTransform content, float viewportHeight, float usedHeight)
        {
            var sd = content.sizeDelta;
            content.sizeDelta = new Vector2(sd.x, Mathf.Max(viewportHeight, usedHeight));
        }

        static void SetHorizontalContentExtent(RectTransform content, float viewportWidth, float usedWidth)
        {
            var sd = content.sizeDelta;
            content.sizeDelta = new Vector2(Mathf.Max(viewportWidth, usedWidth), sd.y);
        }
    }
}
