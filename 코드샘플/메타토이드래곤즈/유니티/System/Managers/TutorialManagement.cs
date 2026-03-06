using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using Com.LuisPedroFonseca.ProCamera2D;

namespace SandboxNetwork
{
    public struct TutorialEndEvent
    {
        public int tutorialGroupID;
        public TutorialEndEvent(int groupKey = 0)
        {
            tutorialGroupID = groupKey;
        }
        static public void Event(int groupKey = 0)
        {
            EventManager.TriggerEvent(new TutorialEndEvent(groupKey));
        }
    }
    /// <summary>
    /// 모든 튜토리얼 데이터를 다 들고 있는 매니저
    /// </summary>
    public class TutorialManager  // [MonoBehavior인 튜토리얼 매니지먼트]가 생성되기 전에 튜토리얼이 등록되면 씹혀 버리기 때문에 분리
    {   
        static TutorialManager instance = null;
        public static TutorialManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TutorialManager();
                }
                return instance;
            }
        }
        /// <summary>
        /// 튜토리얼을 해야 되는 막 생성한 계정인지 체크
        /// </summary>
        public static TutorialManagement tutorialManagement { get; private set; } = null;
        public Dictionary<int, Dictionary<int, TutorialSeqData>> seqDataDic { get; private set; } = new Dictionary<int, Dictionary<int, TutorialSeqData>>();
        private Dictionary<int, RectTransform> recordObjDic = new Dictionary<int, RectTransform>();

        private readonly Dictionary<TutorialDefine, List<int>> privateKeyInGroup = new Dictionary<TutorialDefine, List<int>>()
        {   
            {TutorialDefine.Construct,new List<int>(){ 1101 } },  // 브릭 공장 키값
            {TutorialDefine.Product,new List<int>(){1101, 1100002 } },  // 브릭 공장 키값, 퀘스트 키값
            {TutorialDefine.ConstructUI,new List<int>(){ 1501 } } , // 브릭 툴 샵 키값
            {TutorialDefine.ProductUI,new List<int>(){1101,1501 } }, // 브릭 공장 및 브릭 툴 샵 키값
            {TutorialDefine.DragonGacha,new List<int>(){3,108, 272, 1100005 } },  // 3번 탭, 드래곤 뽑기 메뉴 키값, 뽑기 버튼에 들어갈 뽑기 키값, 퀘스트 키값
            {TutorialDefine.DragonManage,new List<int>() },
            {TutorialDefine.Battery,new List<int>(){1001 } }, // 건전지 공장 키값
            {TutorialDefine.Adventure,new List<int>(){1,1, 1100007 } },  // 1월드 1스테이지, 퀘스트 키값
        };
        private readonly Dictionary<TutorialDefine, int> tutorialStartRequireQuest = new Dictionary<TutorialDefine, int>()
        {
            {TutorialDefine.Construct,0 }, 
            {TutorialDefine.Product,1100001 },  // Product 튜토리얼은 1100001번 퀘스트 보상까지 다 받아야 실행해야 함
            {TutorialDefine.ConstructUI,1100002 } , // ConstructUI 튜토리얼은 1100002번 퀘스트 보상까지 다 받아야 실행해야 함
            {TutorialDefine.ProductUI, 1100003 }, // ProductUI 튜토리얼은 1100003번 퀘스트 보상까지 다 받아야 실행해야 함
            {TutorialDefine.DragonGacha, 1100004 },  // ProductUI 튜토리얼은 1100004번 퀘스트 보상까지 다 받아야 실행해야 함
            {TutorialDefine.DragonManage, 0},    
            {TutorialDefine.Battery, 1100005 },  // Battery 튜토리얼은 1100005번 퀘스트 보상까지 다 받아야 실행해야 함
            {TutorialDefine.Adventure, 1100006 },  // Adventure 튜토리얼은 1100006번 퀘스트 보상까지 다 받아야 실행해야 함
        };

        public void SetSeqDataDic(int group, int seq, TutorialSeqData seqData)
        {
            if (seqDataDic == null) return;

            if (seqDataDic.ContainsKey(group) == false)
            {
                seqDataDic.Add(group, new Dictionary<int, TutorialSeqData>());
            }
            
            if (seqDataDic[group].ContainsKey(seq) == false)
            {
                seqDataDic[group].Add(seq, seqData);
            }
            else
            {
                seqDataDic[group][seq] = seqData;
            }

            SetTutorialEvent(group, seq);
        }
        public TutorialSeqData GetSeqDataDic(int group, int seq)
        {
            TutorialSeqData result = null;
            if (seqDataDic.ContainsKey(group))
            {
                seqDataDic[group].TryGetValue(seq, out result);
            }

            return result;
        }

        public void SetManagement(TutorialManagement management)
        {
            if (management == null) return;
            tutorialManagement = management;
        }

        public int GetPrivateKey(TutorialDefine tutorialDefine, int index = 0)
        {
            if (privateKeyInGroup.ContainsKey(tutorialDefine))
                return privateKeyInGroup[tutorialDefine][index];
            return 0;
        }

        public bool IsTutorialStartCondition(TutorialDefine tutorialDefine)
        {
            if (tutorialStartRequireQuest[tutorialDefine] == 0)
                return true;
            return QuestManager.Instance.IsCompleteQuest(tutorialStartRequireQuest[tutorialDefine]);
            
        }
        public List<int> GetPrivateKeyList(TutorialDefine tutorialDefine)
        {
            if (privateKeyInGroup.ContainsKey(tutorialDefine))
                return privateKeyInGroup[tutorialDefine];
            return new List<int>();
        }
        public void SetRecordObject(int id, RectTransform objRect) // 어쩔 수 없지만 하드 코딩으로 오브젝트들 저장하자
        {
            if (recordObjDic.ContainsKey(id))
                recordObjDic[id] = objRect;
            else
                recordObjDic.Add(id, objRect);
        }

        public RectTransform GetRecordObjectRect(int id)
        {
            if (recordObjDic.ContainsKey(id))
                return recordObjDic[id];
            return null;
        }

        #region 튜토리얼 이벤트 등록하기 힘든 파트 하드 코딩


        /// <summary>
        /// 특정 튜토리얼을 실행했을 때 그 튜토리얼만 가지고 있는 특별한 행동 수행
        /// </summary>
        void SetTutorialEvent(int group, int seq)
        {
            VoidDelegate btnEvent = null;
            var groupDefine = (TutorialDefine)group;
            if (groupDefine == TutorialDefine.Construct)
            {
                switch (seq)
                {
                    case 11:
                    {
                        btnEvent = () =>
                        {
                            BuildingPopupData popupData = new BuildingPopupData(GetPrivateKey(groupDefine));
                            PopupManager.OpenPopup<BuildingConstructPopup>(popupData);
                        };
                    }
                    break;
                }
            }
            else if (groupDefine == TutorialDefine.Product)
            {
                switch (seq)
                {
                    case 2:
                        btnEvent = () =>
                        {
                            var cam = Camera.main.GetComponent<ProCamera2D>();
                            cam.CameraTargets[0].TargetTransform.position = Vector3.up * 2;
                            cam.UpdateScreenSize((Screen.height / (float)Screen.width) * 6f);
                        };
                        break;
                    case 3:
                        btnEvent = () => PopupManager.OpenPopup<ProductTutorialPopup>();
                        break;
                }
            }
            else if (groupDefine == TutorialDefine.ConstructUI)
            {
                switch (seq)
                {
                    case 3:
                        btnEvent = () =>
                        {
                            var cam = Camera.main.GetComponent<ProCamera2D>();
                            cam.CameraTargets[0].TargetTransform.position = Vector3.up * 2;
                            cam.UpdateScreenSize((Screen.height / (float)Screen.width) * 6f);
                        };
                        break;
                    case 4:
                        btnEvent = () => BuildingConstructListPopup.OpenPopup(new BuildingConstructListData());
                        break;
                    case 5:
                        btnEvent = () =>
                        {
                            BuildingPopupData popupData = new BuildingPopupData(GetPrivateKey(groupDefine));
                            PopupManager.OpenPopup<BuildingConstructPopup>(popupData);
                        };
                        break;
                    case 6:
                        btnEvent = () =>
                        {
                            var grid = User.Instance.ExteriorData.ExteriorGrid;
                            int floorCount = TownMap.Height;
                            int cellCount = TownMap.Width;
                            int xPos = 0;
                            int yPos = 0;
                            for (int y = 0; y < floorCount; ++y) //지하철 제외
                            {
                                for (int x = 0; x < cellCount; ++x)
                                {
                                    if (grid.ContainsKey(y) && grid[y].ContainsKey(x) && grid[y][x] == 0)
                                    {
                                        xPos = x;
                                        yPos = y;
                                        break;
                                    }
                                }
                            }

                            if (Town.Instance != null)
                            {
                                Town.Instance.SetPreconstruct(GetPrivateKey(groupDefine), xPos, yPos);
                            }
                        };
                        break;
                }
            }
            else if (groupDefine == TutorialDefine.ProductUI)
            {
                switch (seq)
                {
                    case 3:
                        btnEvent = () => PopupManager.OpenPopup<ProductManageTutorialPopup>();
                        break;
                }
            }
            else if (groupDefine == TutorialDefine.Battery)
            {
                switch (seq)
                {
                    case 3:
                        btnEvent = () =>
                        {
                            var openBuildingData = new BuildingConstructListData();
                            var isAvailableOpen = openBuildingData.IsAvailableOpen();
                            if (isAvailableOpen)
                                BuildingConstructListPopup.OpenPopup(openBuildingData);
                        };
                        break;
                    case 4:
                        btnEvent = () =>
                        {
                            BuildingPopupData popupData = new BuildingPopupData(GetPrivateKey(groupDefine));
                            PopupManager.OpenPopup<BuildingConstructPopup>(popupData);
                        };
                        break;
                    case 5: // 가짜 오브젝트 세팅
                        btnEvent = () =>
                        {
                            tutorialManagement.SetFakeBuilding();
                            PopupManager.ClosePopup<BuildingConstructPopup>();
                            PopupManager.ClosePopup<BuildingConstructListPopup>();
                        };
                        break;
                    case 8: // 가짜 가속권 팝업 오픈
                        btnEvent = () =>
                        {
                            var buildingTag = GetPrivateKey(groupDefine);
                            string buildingKey = BuildingOpenData.GetByInstallTag(buildingTag).BUILDING;
                            var time = BuildingLevelData.GetDataByGroupAndLevel(buildingKey, 0).UPGRADE_TIME;
                            var grid = User.Instance.ExteriorData.ExteriorGrid;
                            int floorCount = TownMap.Height;
                            int cellCount = TownMap.Width;
                            Vector2Int cellPos = Vector2Int.zero;
                            for (int y = 0; y < floorCount; ++y) //지하철 제외
                            {
                                for (int x = 0; x < cellCount; ++x)
                                {
                                    if (grid.ContainsKey(y) && grid[y].ContainsKey(x) && grid[y][x] == 0)
                                    {
                                        cellPos = new Vector2Int(x, y);
                                    }
                                }
                            }
                            var popup = PopupManager.OpenPopup<AccelerationTutorialPopup>(new AccelerationMainData(eAccelerationType.CONSTRUCT, buildingTag, time, time + TimeManager.GetTime(), () =>
                            {
                                tutorialManagement.ForceClearTempObj();
                            }));
                            popup.SetBuildingInfo(buildingTag, cellPos);
                        };
                        break;
                    case 10:
                        btnEvent = () => BuildCompleteEvent.Send(Town.Instance.GetBuilding(GetPrivateKey(groupDefine)), eBuildingState.CONSTRUCT_FINISHED);
                        break;
                    case 12:
                        btnEvent = () => ProductPopup.OpenPopup(GetPrivateKey(groupDefine));
                        break;
                }
            }
            seqDataDic[group][seq].StartEvent = btnEvent;
        }

        #endregion

    }



    public class TutorialManagement : MonoBehaviour, EventListener<QuestEvent>, EventListener<BuildCompleteEvent>, EventListener<ScriptEndEvent>, EventListener<TutorialEndEvent>
    {
        enum TUTORIAL_TYPE
        {
            NONE = 0,
            MAIN = 1,
        }

        // 튜토리얼 레이어

        [SerializeField] GameObject dimmedObject = null;
        [SerializeField] Image dimmedImage = null;
        [SerializeField] Button dimmedBtn = null;
        [SerializeField] GameObject messageBoxObject = null;
        [SerializeField] Button buttonHandleObject = null;
        [SerializeField] RectTransform arrowGuideTr = null;

        [SerializeField] RectTransform focusAreaTr = null;
        [SerializeField] Text tutorialMessageText = null;

        public int currentTutorialGroup { get; private set; } = 0;
        public int currentTutorialSeq { get; private set; } = 0;

        TutorialSeqData currentTutorialSeqData = null;

        List<int> curPrivateKeyList = new List<int>();

        private readonly float defaultDelay = .1f;
        public bool IsPlayingTutorial { get; private set; } = false;
        /// <summary>
        /// 튜토리얼 탈주 후 진행복귀를 했는지
        /// </summary>


        bool isSubTutorialForceMode = false; // 사용 이유 나중에 느낌표 버튼 클릭해서 튜토리얼 실행 시킬 때 그 상태를 위해 사용
        bool resumeCheck = false; 
        
        public bool IsDataLoadCheck { 
            get {
                if (currentTutorialSeqData != null)
                {
                    if (currentTutorialSeqData.Option.HasFlag(eTutorialOption.DimmedAction) || currentTutorialSeqData.Option.HasFlag(eTutorialOption.FocusAction))
                    {
                        if (currentTutorialSeqData.FocusTarget != eFocusTarget.TargetTransform)
                            return true;
                        if (currentTutorialSeqData.targetTransform != null && currentTutorialSeqData.targetTransform.GetComponent<Button>() != null)
                        {
                            return true;
                        }
                        else if (currentTutorialSeqData.StartEvent != null)
                        {
                            return true;
                        }
                    }
                    else if (currentTutorialSeqData.UseWorldCanvas.HasFlag(eObjectPos.UICanvas) == false)
                    {
                        if (currentTutorialSeqData.targetTransform != null)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // 전체 딤드 튜토로 간주
                        return true;
                    }
                }

                return false;
            } 
        }
        VoidDelegate MainTutorialCompleteCallBack = null;
        VoidDelegate CurrentTutorialEndCallBack = null;

        List<GameObject> tempObjs = new List<GameObject>();

        // 완료한 튜토리얼 목록 리스트
        static int lastClearMainTutorial = -1;
        

        // 튜토리얼 시퀀스 데이터
        Dictionary<int, Dictionary<int, TutorialSeqData>> seqDataDic = new Dictionary<int, Dictionary<int, TutorialSeqData>>();

        Queue<int> questEventQueue = new();
        List<TutorialScriptData> scriptGroupData = null;
        QuestUIObject questUI = null;

        Coroutine resize_coroutine = null;
        private void Start()
        {
            TutorialManager.Instance.SetManagement(this);
            seqDataDic = TutorialManager.Instance.seqDataDic;
            var beacon = UIManager.Instance.Beacon;
            if (beacon != null && beacon.UIObjects != null && beacon.UIObjects.Count > 4)
            {
                questUI = beacon.UIObjects[4] as QuestUIObject;
            }
        }

        #region 이벤트 관련

        private void OnEnable()
        {
            EventManager.AddListener<QuestEvent>(this);
            EventManager.AddListener<ScriptEndEvent>(this);
            EventManager.AddListener<BuildCompleteEvent>(this);
            EventManager.AddListener<TutorialEndEvent>(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<QuestEvent>(this);
            EventManager.RemoveListener<ScriptEndEvent>(this);
            EventManager.RemoveListener<BuildCompleteEvent>(this);
            EventManager.RemoveListener<TutorialEndEvent>(this);
        }

        public void OnEvent(TutorialEndEvent e)
        {
            foreach (var sub in TutorialTriggerData.GetByTriggerType(ScriptTriggerType.TUTORIAL_END))
            {
                if (OnCheckScript(sub, e))
                    return;
            }
        }
        public bool OnCheckScript(TutorialTriggerData trigger, TutorialEndEvent e)
        {
            if (trigger == null)
                return false;
            if (trigger.TriggerParam == e.tutorialGroupID)
            {
                StartTutorial(trigger);
                return true;
            }

            return false;
        }
        public void OnEvent(BuildCompleteEvent e)
        {
            if (e.eType == eBuildingState.CONSTRUCTING)
            {
                foreach (var sub in TutorialTriggerData.GetByTriggerType(ScriptTriggerType.CONSTRUCT_START))
                {
                    if (OnCheckScript(sub, e))
                        return;
                }
            }
            if (e.eType == eBuildingState.NORMAL)
            {
                foreach (var sub in TutorialTriggerData.GetByTriggerType(ScriptTriggerType.CONSTRUCT_DONE))
                {
                    if (OnCheckScript(sub, e))
                        return;
                }
            }
        }
        public bool OnCheckScript(TutorialTriggerData trigger, BuildCompleteEvent e)
        {
            if (trigger == null)
                return false;

            if ((trigger.TriggerType == ScriptTriggerType.CONSTRUCT_START && e.eType == eBuildingState.CONSTRUCTING) ||
                (trigger.TriggerType == ScriptTriggerType.CONSTRUCT_DONE && e.eType == eBuildingState.NORMAL))
                return false;

            BuildingLevelData levelData = BuildingLevelData.Get(trigger.TriggerParam.ToString());
            if (levelData != null && levelData.BUILDING_GROUP == e.building.BName && levelData.LEVEL == e.building.Data.Level)
            {
                StartTutorial(trigger);
                return true;
            }

            return false;
        }
        public void OnEvent(ScriptEndEvent eventType)
        {
            foreach (var sub in TutorialTriggerData.GetByTriggerType(ScriptTriggerType.SCRIPT_END))
            {
                if (sub != null)
                {
                    if (sub.TriggerParam == eventType.scriptID)
                    {
                        StartTutorial(sub);
                    }
                }
            }
        }
        public void OnEvent(QuestEvent eventType)
        {
            if (eventType.e == QuestEvent.eEvent.QUEST_DONE)
            {
                foreach (var sub in TutorialTriggerData.GetByTriggerType(ScriptTriggerType.QUEST_CLEAR))
                {
                    if (OnClearQuest(sub, eventType))
                        return;
                }
            }
            if (eventType.e == QuestEvent.eEvent.TUTORIAL_QUEST_CHECK)
            {
                List<int> marked = QuestManager.Instance.GetQuestMarked();
                if (marked == null || marked.Count == 0)
                    return;
                foreach (var sub in TutorialTriggerData.GetByTriggerType(ScriptTriggerType.QUEST_START))
                {
                    if (sub.TriggerParam == marked[0])
                    {
                        QuestEvent.Event(QuestEvent.eEvent.TUTORIAL_QUEST_OPEN);
                        return;
                    }

                }
            }
            if (eventType.e == QuestEvent.eEvent.QUEST_START)
            {
                foreach (var sub in TutorialTriggerData.GetByTriggerType(ScriptTriggerType.QUEST_START))
                {
                    if (OnStartQuest(sub, eventType))
                        return;
                }
            }
        }

        public bool OnClearQuest(TutorialTriggerData trigger, QuestEvent eventType)
        {
            if (trigger == null)
                return false;

            if (trigger.TriggerType != ScriptTriggerType.QUEST_CLEAR)
                return false;
            if (trigger.TriggerParam != eventType.eventQID)
                return false;
            StartTutorial(trigger);
            return true;
        }

        public bool OnStartQuest(TutorialTriggerData trigger, QuestEvent eventType)
        {
            if (trigger == null)
                return false;

            if (trigger.TriggerType != ScriptTriggerType.QUEST_START)
                return false;
            if (trigger.TriggerParam != eventType.eventQID)
                return false;
            StartTutorial(trigger);
            return true;
        }

        #endregion

        // 튜토리얼 시작 요청
        public void StartTutorial(TutorialTriggerData data, VoidDelegate tutorialCompleteCallback = null)
        {
            StartTutorial(data.KEY, data.FIRST_SEQ,false, tutorialCompleteCallback);
        }
        public void StartTutorial(int tutoGroup, int tutoSeq = 1, bool isForceTutorial = false /*강제 진행 튜토 여부*/, VoidDelegate mainTutorialEndCallback =null)
        {
            if (mainTutorialEndCallback != null)
                MainTutorialCompleteCallBack = mainTutorialEndCallback;

            var triggerDat = TutorialTriggerData.Get(tutoGroup.ToString());
            if (triggerDat == null)
                return;
            if(triggerDat.TutorialType == eTutorialType.Main) // Main 튜토리얼 껏따 켜도 복귀 때문에 나눔
            {
                if (IsRemainMainTutorial() == false && isForceTutorial == false)
                {
                    MainTutorialCompleteCallBack?.Invoke();
                    return;
                }
                int tutorialGroup = tutoGroup;
                int tutorialSeq = tutoSeq;
                if (isForceTutorial == false)
                {
                    if (currentTutorialGroup >= tutoGroup && currentTutorialSeq >= tutoSeq) //이미 실행중이거나 예전 튜토리얼
                        return;
                    if (tutoSeq == 1)
                    {
                        if ((lastClearMainTutorial > 0 && lastClearMainTutorial >= tutoGroup)) //진행 완료한 튜토리얼이 있는지 체크
                        {
                            if (lastClearMainTutorial >= (int)TutorialDefine.MainTutorialEnd) // 진행 완료한 튜토리얼이 마지막인지
                                return;
                            else
                                tutorialGroup = (int)GetNextTutorial((TutorialDefine)lastClearMainTutorial, 1);// 진행 완료한 튜토리얼의 다음 튜토리얼로 세팅
                            if (resumeCheck)
                                return;
                        }
                        ResumeTutorial(ref tutorialGroup, ref tutorialSeq); //현재 튜토리얼이 진행 가능 상태인지 체크
                    }
                }
                SetData(tutorialGroup, tutorialSeq);
            }
            else if (triggerDat.TutorialType == eTutorialType.Sub)
            {
                if(isForceTutorial)
                    isSubTutorialForceMode = true;
                if (IsFinishedTutorial((TutorialDefine)triggerDat.KEY) && isSubTutorialForceMode == false) // 완료한 튜토리얼인가
                    return;
                SetData(tutoGroup, tutoSeq);
            }

           

            IsPlayingTutorial = true;
            
            if (IsDataLoadCheck)
            {
                ResetTutorialLayer();
                BlockTouch();
                SetCamPos();

                CancelInvoke(nameof(SetTutorialLayer));
                Invoke(nameof(SetTutorialLayer), currentTutorialSeqData.delay);
            }

            questUI?.SetQuestHighlight(false);
        }

        /// <summary>
        ///  건물 강조시 건물이 UI에 가려지지 않는 위치로 세팅
        /// </summary>
        void SetCamPos() 
        {
            if (currentTutorialSeqData.FocusTarget == eFocusTarget.Building)
            {
                Camera.main.GetComponent<ProCamera2D>().CameraTargets[0].TargetTransform.position = Vector2.up * 2;
            }
        }

        void SetData(int tutoGroup, int tutoSeq)
        {
            if (currentTutorialGroup != tutoGroup)
                scriptGroupData = TutorialScriptData.GetByGroupList(tutoGroup);
            currentTutorialGroup = tutoGroup;
            currentTutorialSeq = tutoSeq;
            curPrivateKeyList = TutorialManager.Instance.GetPrivateKeyList((TutorialDefine)currentTutorialGroup);
            currentTutorialSeqData = TutorialManager.Instance.GetSeqDataDic(currentTutorialGroup, currentTutorialSeq);
        }

        void BlockTouch() // 튜토리얼 딜레이 동안 발생하는 터치 막음
        {
            buttonHandleObject.GetComponent<RectTransform>().sizeDelta = focusAreaTr.sizeDelta = Vector2.zero;
            dimmedImage.color = new Color(0, 0, 0, 0);
            dimmedObject?.SetActive(true);
        }

        void SetTutorialLayer()
        {
            CancelInvoke(nameof(SetTutorialLayer));

            if (currentTutorialSeqData == null) 
                return;

#if DEBUG
            Debug.LogFormat("group : {0} seq : {1}", currentTutorialGroup, currentTutorialSeq);
#endif
            switch (currentTutorialSeqData.FocusTarget)
            {
                case eFocusTarget.MainQuest:
                    if (questUI != null)
                        currentTutorialSeqData.targetTransform = questUI.TopQuestIcon.transform as RectTransform;
                    break;
                case eFocusTarget.Building:
                    var building = Town.Instance.GetBuilding(currentTutorialSeqData.TargetValue);
                    if (building != null)
                    {
                        currentTutorialSeqData.targetTransform = building.transform as RectTransform;
                    }
                    if (currentTutorialSeqData.TargetValue == 0)// 빈 공간
                    {
                        var grid = User.Instance.ExteriorData.ExteriorGrid;
                        int floorCount = TownMap.Height;
                        int cellCount = TownMap.Width;
                        for (int y = 0; y < floorCount; ++y) //지하철 제외
                        {
                            for (int x = 0; x < cellCount; ++x)
                            {
                                if (grid.ContainsKey(y) && grid[y].ContainsKey(x) && grid[y][x] == 0)
                                {
                                    Vector2 cellPos = TownMap.GetBuildingPos(x, y);
                                    var obj = Instantiate(new GameObject(), Town.Instance.TownBaseTransoform);
                                    obj.transform.localPosition = cellPos;
                                    currentTutorialSeqData.targetTransform = obj.AddComponent<RectTransform>();
                                    tempObjs.Add(obj);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case eFocusTarget.RecordedObj:
                    currentTutorialSeqData.targetTransform = TutorialManager.Instance.GetRecordObjectRect(currentTutorialSeqData.TargetValue);
                    break;
            }

            if (resize_coroutine != null)
                StopCoroutine(resize_coroutine);

            resize_coroutine = StartCoroutine(ButtonSizePosition());
            
            // 딤드 레이어 설정
            dimmedImage.color = new Color(dimmedImage.color.r, dimmedImage.color.g, dimmedImage.color.b, currentTutorialSeqData.dimmedAlpha);
            dimmedObject?.SetActive(!currentTutorialSeqData.Option.HasFlag(eTutorialOption.DimmedOff));

            // 포커스 레이어 설정
            buttonHandleObject.gameObject.SetActive(focusAreaTr.sizeDelta != Vector2.zero);

            // 버튼 형태 튜토리얼 세팅
            SetButtonEvent();

            // 튜토리얼 가이드 박스 생성
            SetTutorialMessageBox();

            // 튜토리얼 화살표 가이드 생성
            SetTutorialArrowGuide(currentTutorialSeqData);

            // 타이머형 튜토리얼 세팅
            SetTimerTutorial();

            // 튜토리얼 포커스 박스 늘어놨다가 줄어들다가 이펙트 세팅
            SetTutorialTween();
        }

        IEnumerator ButtonSizePosition()
        {
            Vector2 resultCanvasPos = Vector2.zero;
            RectTransform rt = buttonHandleObject.GetComponent<RectTransform>();

            while (currentTutorialSeqData != null)
            {
                var targetTransform = currentTutorialSeqData.targetTransform;
                switch (currentTutorialSeqData.UseWorldCanvas)
                {
                    case eObjectPos.UICanvas:
                        if (currentTutorialSeqData.targetTransform != null)
                        {
                            Vector2 screenCanvasPos = SBFunc.WorldToUICanvasPosition(currentTutorialSeqData.targetTransform.position);
                            resultCanvasPos = screenCanvasPos + currentTutorialSeqData.targetPos;
                        }

                        focusAreaTr.anchoredPosition = currentTutorialSeqData.targetTransform != null ? resultCanvasPos : currentTutorialSeqData.targetPos;
                        focusAreaTr.sizeDelta = currentTutorialSeqData.targetSize;
                        break;
                    case eObjectPos.WorldCanvas:
                        float worldScale = 4f / Camera.main.orthographicSize;
                        if (targetTransform != null)
                        {
                            Vector2 screenCanvasPos = SBFunc.WorldToCanvasPosition(targetTransform.position) + currentTutorialSeqData.targetPos * worldScale;


                            focusAreaTr.anchoredPosition = screenCanvasPos;
                            focusAreaTr.sizeDelta = (targetTransform.sizeDelta + currentTutorialSeqData.targetSize) * worldScale;
                        }
                        break;
                    case eObjectPos.WorldObject:
                        float screenRatio = Screen.height / (float)Screen.width;
                        float scale = UICanvas.Instance.GetCamera().orthographicSize / ((screenRatio < 0.6) ? Camera.main.orthographicSize : 3.375f); // 3.375 : 패드, 탭 전용 크기
                        if (targetTransform != null)
                        {
                            Vector2 screenCanvasPos = SBFunc.WorldToCanvasPosition(targetTransform.position) + currentTutorialSeqData.targetPos * scale;
                            focusAreaTr.anchoredPosition = screenCanvasPos;
                            focusAreaTr.sizeDelta = (targetTransform.sizeDelta + currentTutorialSeqData.targetSize) * scale;
                        }
                        break;
                    default:
                        break;
                }

                rt.position = focusAreaTr.position;
                rt.sizeDelta = focusAreaTr.rect.size;


                Vector2 focusAreaPos = Vector2.zero;
                if (currentTutorialSeqData.targetTransform != null)
                    focusAreaPos = focusAreaTr.transform.localPosition;

                messageBoxObject.GetComponent<RectTransform>().anchoredPosition = currentTutorialSeqData.Option.HasFlag(eTutorialOption.MsgBoxTargetPos) ? focusAreaPos + currentTutorialSeqData.messageBoxPos : currentTutorialSeqData.messageBoxPos;

                yield return new WaitForEndOfFrame();
            }
        }

        // 버튼 이동형 튜토리얼의 경우 처리
        void SetButtonEvent()
        {
            if (currentTutorialSeqData == null) 
                return;

            if (buttonHandleObject != null)
            {
                buttonHandleObject.onClick.RemoveAllListeners();
                var isFocusAction = currentTutorialSeqData.Option.HasFlag(eTutorialOption.FocusAction);
                var isFocusNextAction = currentTutorialSeqData.Option.HasFlag(eTutorialOption.FocusNextAction);
                if (isFocusAction && !isFocusNextAction)
                {
                    buttonHandleObject.onClick.AddListener(BtnEventNoNextTuto);
                    buttonHandleObject.onClick.AddListener(CheckTutorialEnd);
                }
                else
                {
                    if (isFocusAction)
                    {
                        buttonHandleObject.onClick.AddListener(CopyButtonEvent);
                        buttonHandleObject.onClick.AddListener(CheckTutorialEnd);
                    }
                    if (isFocusNextAction)
                    {
                        buttonHandleObject.onClick.AddListener(OnClickNextTutorial);
                    }
                }
            }

            if (dimmedBtn != null)
            {
                dimmedBtn.onClick.RemoveAllListeners();
                if (currentTutorialSeqData.Option.HasFlag(eTutorialOption.DimmedAction))
                {
                    dimmedBtn.onClick.AddListener(OnClickDimmedAction);
                    dimmedBtn.onClick.AddListener(CheckTutorialEnd);
                }
                if (currentTutorialSeqData.Option.HasFlag(eTutorialOption.DimmedNextAction))
                    dimmedBtn.onClick.AddListener(OnClickNextTutorial);
            }
        }

        void CheckTutorialEnd()
        {
            if (resize_coroutine != null)
            {
                StopCoroutine(resize_coroutine);
                resize_coroutine = null;
            }

            if (currentTutorialSeqData.Option.HasFlag(eTutorialOption.LastInGroup))
            {
                if (IsFinishedTutorial((TutorialDefine)currentTutorialGroup) == false)
                {
                    if(currentTutorialGroup <= (int)TutorialDefine.MainTutorialEnd) {

                        Dictionary<string, string> param = new Dictionary<string, string>();
                        param.Add("skip", "0");
                        
                        WWWForm paramData = new WWWForm();
                        paramData.AddField("tuto_type", (int)TUTORIAL_TYPE.MAIN);
                        paramData.AddField("tuto_id", currentTutorialGroup);

                        if(currentTutorialGroup == 8001)
                        {
                            AppsFlyerSDK.AppsFlyer.sendEvent("tutorial_complete", new Dictionary<string, string>());
                        }

                        NetworkManager.Send("tutorial/advance", paramData, (jsonData) =>
                        {
                            if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                            {
                                switch (jsonData["rs"].Value<int>())
                                {
                                    case (int)eApiResCode.OK:
                                        if (SBFunc.IsJTokenCheck(jsonData["reward"]))
                                        {
                                            var reward = (JArray)jsonData["reward"];
                                            if (reward != null && reward.Count > 0)
                                            {
                                                var rewardPopup = SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(reward)));
                                                rewardPopup.SetText(StringData.GetStringByStrKey("튜토리얼보상"));
                                                rewardPopup.SetDimmedClickAction(() => { }); // 딤드 눌러도 안꺼지고 아무것도 안하게 세팅
                                            }
                                        }
                                        EndTutorialEvent();
                                    break;
                                    default:
                                        EndTutorialEvent();
                                        break;
                                }
                            }
                            else
                                EndTutorialEvent();
                        }, (string str) =>
                        {
                            EndTutorialEvent();
                        });
                    }
                    else // SubTutorial
                    {
                        CacheUserData.SetBoolean("Tutoiral"+currentTutorialGroup.ToString(), true);
                        EndTutorialEvent();
                    }
                }
                else
                    EndTutorialEvent();
            }
        }
        void BtnEventNoNextTuto()
        {
            if (currentTutorialSeqData == null)
                EndTutorialEvent();
            arrowGuideTr.gameObject.SetActive(false);
            if (currentTutorialSeqData.targetTransform != null && currentTutorialSeqData.targetTransform.GetComponent<Button>() != null && currentTutorialSeqData.Option.HasFlag(eTutorialOption.IgnoreBtnEvent) == false)
            {
                if (currentTutorialSeqData != null)
                {
                    currentTutorialSeqData.targetTransform.GetComponent<Button>().onClick.Invoke();
                    messageBoxObject.SetActive(false);
                    dimmedObject.SetActive(false);
                }
            }
            else
            {
                messageBoxObject.SetActive(false);
                dimmedObject.SetActive(false);
            }
            VoidDelegate targetButton = currentTutorialSeqData.StartEvent;
            if (currentTutorialSeqData != null)
            {
                targetButton?.Invoke();
            }
        }

        void CopyButtonEvent()
        {
            if (currentTutorialSeqData == null)
            {
                EndTutorialEvent();
            }
            if (currentTutorialSeqData.targetTransform != null && currentTutorialSeqData.targetTransform.GetComponent<Button>() != null && currentTutorialSeqData.Option.HasFlag(eTutorialOption.IgnoreBtnEvent) == false)
            {
                if (currentTutorialSeqData != null)
                    currentTutorialSeqData.targetTransform.GetComponent<Button>().onClick.Invoke();
            }
            VoidDelegate targetButton = currentTutorialSeqData.StartEvent;
            if (currentTutorialSeqData != null)
                targetButton?.Invoke();
        }

        void SetTutorialMessageBox()
        {
            if (messageBoxObject == null || tutorialMessageText == null) 
                return;

            messageBoxObject.transform.DOKill();
            focusAreaTr?.DOKill();
            buttonHandleObject.transform?.DOKill();
            focusAreaTr.transform.localScale = buttonHandleObject.transform.localScale = Vector3.one;
            messageBoxObject.transform.localScale = Vector3.one;

            // 메시지 박스 위치 조정
            Vector2 resultCanvasPos = Vector2.zero;
            if (currentTutorialSeqData.targetTransform != null)
                resultCanvasPos = focusAreaTr.transform.localPosition;
            

            messageBoxObject.GetComponent<RectTransform>().anchoredPosition = currentTutorialSeqData.Option.HasFlag(eTutorialOption.MsgBoxTargetPos) ? resultCanvasPos + currentTutorialSeqData.messageBoxPos : currentTutorialSeqData.messageBoxPos;
            messageBoxObject.SetActive(!currentTutorialSeqData.Option.HasFlag(eTutorialOption.MsgBoxOff));
            messageBoxObject.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.35f).SetEase(Ease.InOutElastic);
            if (scriptGroupData == null)
                return;

            string messageStr = string.Empty;

            foreach (var dat in scriptGroupData)
            {
                if (dat.SEQUENCE == currentTutorialSeq)
                {
                    messageStr = dat.STRING;
                    break;
                }
            }
            messageBoxObject.SetActive(messageStr != string.Empty);
            tutorialMessageText.text = messageStr;
            SoundManager.Instance.PlaySFX("FX_TUTORIAL_OPEN");
        }

        void SetTutorialArrowGuide(TutorialSeqData seqData)
        {
            if (seqData == null || arrowGuideTr == null)
                return;

            arrowGuideTr.DOKill();
            //focusAreaTr?.DOKill();
            float calcPos = 0;
            Vector2 calcVec2 = new Vector2(arrowGuideTr.sizeDelta.x / 2, arrowGuideTr.sizeDelta.y / 2);

            switch (seqData.arrowType)
            {
                case eTutorialGuideArrowDir.NONE:
                    break;
                case eTutorialGuideArrowDir.UP:
                    arrowGuideTr.localEulerAngles = new Vector3(0, 0, 0);
                    calcPos = (focusAreaTr.transform.localPosition.y + calcVec2.y) + calcVec2.y;
                    arrowGuideTr.anchoredPosition = new Vector2(focusAreaTr.transform.localPosition.x, calcPos) + seqData.arrowPos;
                    arrowGuideTr.DOMoveY(0.5f, 0.5f).SetRelative().SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Yoyo);
                    break;
                case eTutorialGuideArrowDir.DOWN:
                    arrowGuideTr.localEulerAngles = new Vector3(0, 0, 180);
                    calcPos = (focusAreaTr.transform.localPosition.y - calcVec2.y) - calcVec2.y;
                    arrowGuideTr.anchoredPosition = new Vector2(focusAreaTr.transform.localPosition.x, calcPos) + seqData.arrowPos;
                    arrowGuideTr.DOMoveY(-0.5f, 0.5f).SetRelative().SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Yoyo);
                    break;
                case eTutorialGuideArrowDir.LEFT:
                    arrowGuideTr.localEulerAngles = new Vector3(0, 0, 90);
                    calcPos = (focusAreaTr.transform.localPosition.x - calcVec2.x) - calcVec2.x;
                    arrowGuideTr.anchoredPosition = new Vector2(calcPos, focusAreaTr.transform.localPosition.y) + seqData.arrowPos;
                    arrowGuideTr.DOMoveX(-0.5f, 0.5f).SetRelative().SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Yoyo);
                    break;
                case eTutorialGuideArrowDir.RIGHT:
                    arrowGuideTr.localEulerAngles = new Vector3(0, 0, -90);
                    calcPos = (focusAreaTr.transform.localPosition.x + calcVec2.x) + calcVec2.x;
                    arrowGuideTr.anchoredPosition = new Vector2(calcPos, focusAreaTr.transform.localPosition.y) + seqData.arrowPos;
                    arrowGuideTr.DOMoveX(0.5f, 0.5f).SetRelative().SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Yoyo);
                    break;
            }
            arrowGuideTr.gameObject.SetActive(seqData.arrowType != eTutorialGuideArrowDir.NONE);
        }

        void SetTimerTutorial() // 특정시간 이후 다음 시퀀스 강제 진행
        {
            if (currentTutorialSeqData.waitTime <= 0) 
                return;
            //Invoke(buttonHandleObject.onClick, currentTutorialSeqData.waitTime);
            Invoke(nameof(NextTutorialStart), currentTutorialSeqData.waitTime);
        }

        void SetTutorialTween()
        {
            if (currentTutorialSeqData.tweenScale > 0 && currentTutorialSeqData.tweenTimer > 0f) 
            { 
                 buttonHandleObject.transform.DOScale(currentTutorialSeqData.tweenScale, currentTutorialSeqData.tweenTimer).SetLoops(-1, LoopType.Yoyo);
                focusAreaTr.DOScale(currentTutorialSeqData.tweenScale, currentTutorialSeqData.tweenTimer).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                focusAreaTr?.DOKill();
                buttonHandleObject.transform?.DOKill();
            }
        }

        public void NextTutorialStart()
        {
            if (IsPlayingTutorial == false) 
                return;
            CheckTutorialEnd();
            StartTutorial(currentTutorialGroup, currentTutorialSeq + 1);
        }

        public void EndTutorialEvent(bool isLogOut =false)
        {
            int tempTutoGroupID = currentTutorialGroup;
            IsPlayingTutorial = false;
            isSubTutorialForceMode = false;
            CurrentTutorialEndCallBack?.Invoke();
            ClearTutorialData();
            if (isLogOut)
                resumeCheck = false;
            if (tempTutoGroupID == (int)TutorialDefine.MainTutorialEnd)
            {
                MainTutorialCompleteCallBack?.Invoke();
                MainTutorialCompleteCallBack = null;
            }
            questUI?.SetQuestHighlight(true);
            TutorialEndEvent.Event(tempTutoGroupID);
        }

        public void SetButtonObjectDic(int group = -1, int seq = -1, VoidDelegate buttonObject = null)
        {
            if (seqDataDic == null) 
                return;
            if (seqDataDic.ContainsKey(group))
            {
                if (seqDataDic[group].ContainsKey(seq))
                {
                    TutorialSeqData newData = seqDataDic[group][seq];
                    newData.StartEvent = buttonObject;
                    seqDataDic[group][seq] = newData;
                }
            }
        }

        public void SetCurrentTutorialEndCallBack(VoidDelegate cb)
        {
            if (cb != null)
                CurrentTutorialEndCallBack = cb;
        }

        public void OnClickNextTutorial()
        {
            if (currentTutorialSeqData == null)
                return;

            // setting clear
            dimmedBtn.onClick.RemoveAllListeners();
            buttonHandleObject?.onClick.RemoveAllListeners();
            focusAreaTr.sizeDelta = Vector2.zero;
            buttonHandleObject.gameObject.SetActive(focusAreaTr.sizeDelta != Vector2.zero);

            NextTutorialStart();
        }

        public void OnClickDimmedAction()
        {
            if (currentTutorialSeqData == null) 
                return;
            if (currentTutorialSeqData.Option.HasFlag(eTutorialOption.DimmedAction))
                currentTutorialSeqData.StartEvent?.Invoke();
        }

        public int GetCurTutoPrivateKey(int index = 0)
        {
            if (curPrivateKeyList.Count > index)
                return curPrivateKeyList[index];
            return 0;
        }

        public static void SetTutorialData(JObject data)
        {
            if (data == null)
                return;

            if(SBFunc.IsJTokenType(data["m"], JTokenType.Integer))
                lastClearMainTutorial = data["m"].Value<int>();
        }

        public bool IsPlayingTutorialByGroup(TutorialDefine group)
        {
            if (currentTutorialSeqData == null)
                return false;
            return IsPlayingTutorial && (int)group == currentTutorialGroup && currentTutorialSeqData.Option.HasFlag(eTutorialOption.LastInGroup) == false;
        }

        public bool IsFinishedTutorial(TutorialDefine group)
        {
            if (group <= TutorialDefine.MainTutorialEnd)
                return lastClearMainTutorial >= (int)group;
            else
                return CacheUserData.GetBoolean("Tutoiral" + ((int)group).ToString(), false);            
        }

        void ResetTutorialLayer()
        {
            //dimmedObject?.SetActive(false);
            messageBoxObject?.SetActive(false);
            arrowGuideTr?.gameObject.SetActive(false);
            //focusAreaTr.sizeDelta = Vector2.zero;
            dimmedBtn.onClick.RemoveAllListeners();
            buttonHandleObject?.onClick.RemoveAllListeners();
        }

        void ClearTutorialData()
        {
            currentTutorialGroup = 0;
            currentTutorialSeq = 0;
            scriptGroupData = null;

            ForceClearTempObj();

            // 튜토리얼 레이어 초기화
            dimmedObject?.SetActive(false);
            messageBoxObject?.SetActive(false);
            arrowGuideTr?.gameObject.SetActive(false);
            messageBoxObject?.transform.DOKill();
            arrowGuideTr?.DOKill();
            focusAreaTr?.DOKill();
            buttonHandleObject.onClick.RemoveAllListeners();
            dimmedBtn.onClick.RemoveAllListeners();

            CurrentTutorialEndCallBack = null;
            currentTutorialSeqData = null;
        }

        public void ForceClearTempObj()
        {
            foreach (var obj in tempObjs)
            {
                if (obj != null)
                    Destroy(obj);
            }
            tempObjs.Clear();
        }

        public bool IsOtherContentsBlock()
        {
            return (IsFinishedTutorial(TutorialDefine.MainTutorialEnd) == false || IsPlayingTutorial);
        }

        public bool IsRemainMainTutorial()
        {
            if (lastClearMainTutorial < 0)
                return false;

            if (IsFinishedTutorial(TutorialDefine.MainTutorialEnd))
                return false;

            return true;
        }

        public void SetFakeBuilding()
        {
            var grid = User.Instance.ExteriorData.ExteriorGrid;
            int floorCount = TownMap.Height;
            int cellCount = TownMap.Width;
            for (int y = 0; y < floorCount; ++y) //지하철 제외
            {
                for (int x = 0; x < cellCount; ++x)
                {
                    if (grid.ContainsKey(y) && grid[y].ContainsKey(x) && grid[y][x] == 0)
                    {
                        Vector2 cellPos = TownMap.GetBuildingPos(x, y);
                        GameObject obj = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.TutorialPrefabPath, "tutorial_building"), Town.Instance.TownBaseTransoform);
                        obj.transform.localPosition = cellPos;
                        tempObjs.Add(obj);
                        break;
                    }
                }
            }
        }

        void ResumeTutorial(ref int group, ref int seq) // 튜토리얼 강제 종료 등 여러가지 행위 이후 튜토리얼 가능한지 체크
        {
            
            int tempSeq = 0;
            var ticketItemDataList = ItemBaseData.GetItemListByKind(eItemKind.ACC_TICKET);
            ticketItemDataList = ticketItemDataList.OrderBy(elemet => elemet.VALUE).ToList();
            int curAccelItemKey = 0;
            if (ticketItemDataList.Count > 0)
            {
                ItemBaseData itemData = ticketItemDataList[0];
                curAccelItemKey = itemData.KEY;
            }
            if (TutorialManager.Instance.IsTutorialStartCondition((TutorialDefine)group)==false) //
            {
                if((TutorialDefine)group > TutorialDefine.Construct)
                {
                    group = (int)GetNextTutorial((TutorialDefine)group, -1);
                }
            }
            curPrivateKeyList = TutorialManager.Instance.GetPrivateKeyList((TutorialDefine)group);
            switch ((TutorialDefine)group)
            {
                case TutorialDefine.Construct:
                    var brickFactory = Town.Instance.GetBuilding(curPrivateKeyList[0]);
                    if (brickFactory == null)
                        tempSeq = 1;
                    else
                    {
                        if (brickFactory.Data.State == eBuildingState.CONSTRUCTING)
                        {
                            WWWForm paramData = new WWWForm();
                            paramData.AddField("type", (int)eAccelerationType.CONSTRUCT);
                            paramData.AddField("tag", curPrivateKeyList[0]);
                            paramData.AddField("item", curAccelItemKey);
                            paramData.AddField("count", 1);
                            NetworkManager.Send("building/haste", paramData, (jsonData) =>
                            {
                                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                                {
                                    switch (jsonData["rs"].ToObject<int>())
                                    {
                                        case (int)eApiResCode.OK:
                                            WWWForm paramData = new WWWForm();
                                            paramData.AddField("tag", curPrivateKeyList[0]);
                                            NetworkManager.Send("building/complete", paramData, (jsonData) =>
                                            {
                                                Town.Instance.RefreshMap();
                                                UIManager.Instance.RefreshCurrentUI();
                                                brickFactory.ShutterForceOff();
                                            });
                                            break;
                                    }
                                }
                            });
                        }
                        else if (brickFactory.Data.State == eBuildingState.CONSTRUCT_FINISHED)
                        {
                            WWWForm paramData = new WWWForm();
                            paramData.AddField("tag", curPrivateKeyList[0]);
                            NetworkManager.Send("building/complete", paramData, (jsonData) =>
                            {
                                Town.Instance.RefreshMap();
                                UIManager.Instance.RefreshCurrentUI();
                                brickFactory.ShutterForceOff();
                            });
                        }
                        tempSeq = 16;
                    }
                    break;
                case TutorialDefine.Product:
                    var quest = QuestManager.Instance.GetQuest(curPrivateKeyList[1]);
                    if (quest!=null && quest.IsQuestClear())
                    {
                        var popup = PopupManager.OpenPopup<ProductTutorialPopup>();
                        var queueItem1 = User.Instance.GetProduces(curPrivateKeyList[0]).Items;
                        // 아이템 미수령시
                        if(queueItem1 !=null && queueItem1.Count > 0)
                        {
                            popup.EnqueueItem();
                            popup.OnChangeQueueItem();
                            tempSeq = 14;
                        }
                        // 아이템 수령시 
                        else
                            tempSeq = 15;
                    }
                    else
                    {
                        QuestEvent.Event(QuestEvent.eEvent.QUEST_OPEN);
                        tempSeq = 1;
                    }
                    break;
                case TutorialDefine.ConstructUI:
                    var brickToolShop = Town.Instance.GetBuilding(curPrivateKeyList[0]);
                    if (brickToolShop == null)
                    {
                        QuestEvent.Event(QuestEvent.eEvent.QUEST_OPEN);
                        tempSeq = 1;
                    }
                    else
                    {
                        if (brickToolShop.Data.State == eBuildingState.CONSTRUCTING)
                        {
                            WWWForm paramData = new WWWForm();
                            paramData.AddField("type", (int)eAccelerationType.CONSTRUCT);
                            paramData.AddField("tag", curPrivateKeyList[0]);
                            paramData.AddField("item", curAccelItemKey);
                            paramData.AddField("count", 1);
                            NetworkManager.Send("building/haste", paramData, (jsonData) =>
                            {
                                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                                {
                                    switch (jsonData["rs"].ToObject<int>())
                                    {
                                        case (int)eApiResCode.OK:
                                            WWWForm paramData = new WWWForm();
                                            paramData.AddField("tag", curPrivateKeyList[0]);
                                            NetworkManager.Send("building/complete", paramData, (jsonData) =>
                                            {
                                                Town.Instance.RefreshMap();
                                                UIManager.Instance.RefreshCurrentUI();
                                                brickToolShop.ShutterForceOff();
                                            });
                                            break;
                                    }
                                }
                            });
                        }
                        else if (brickToolShop.Data.State == eBuildingState.CONSTRUCT_FINISHED)
                        {
                            WWWForm paramData = new WWWForm();
                            paramData.AddField("tag", curPrivateKeyList[0]);
                            NetworkManager.Send("building/complete", paramData, (jsonData) =>
                            {
                                Town.Instance.RefreshMap();
                                UIManager.Instance.RefreshCurrentUI();
                                brickToolShop.ShutterForceOff();
                            });
                        }
                        tempSeq = 8;
                    }
                        
                    break;
                case TutorialDefine.ProductUI:
                    var queueItem2 = User.Instance.GetProduces(curPrivateKeyList[1]).Items;
                    if (queueItem2 != null && queueItem2.Count > 0)
                    {
                        PopupManager.OpenPopup<ProductManageTutorialPopup>();
                        tempSeq = 12;
                    }
                    else
                    {
                        QuestEvent.Event(QuestEvent.eEvent.QUEST_OPEN);
                        tempSeq = 1;
                    }
                    break;
                case TutorialDefine.DragonGacha:
                    var gachaQuest = QuestManager.Instance.GetQuest(curPrivateKeyList[3]);
                    if (gachaQuest != null && gachaQuest.IsQuestClear())
                        tempSeq = 12;
                    else
                        tempSeq = 1;
                     break;
                case TutorialDefine.DragonManage:
                    tempSeq = 1;
                    break;
                case TutorialDefine.Battery:
                    var batteryFactory = Town.Instance.GetBuilding(curPrivateKeyList[0]);
                    if (batteryFactory == null)
                    {
                        QuestEvent.Event(QuestEvent.eEvent.QUEST_OPEN);
                        tempSeq = 1;
                    }
                    else
                    {
                        if(batteryFactory.Data.State== eBuildingState.NORMAL)
                        {
                            tempSeq = 11;
                        }
                        else
                        {
                            if (batteryFactory.Data.State == eBuildingState.CONSTRUCTING)
                            {
                                WWWForm paramData = new WWWForm();
                                paramData.AddField("type", (int)eAccelerationType.CONSTRUCT);
                                paramData.AddField("tag", curPrivateKeyList[0]);
                                paramData.AddField("item", curAccelItemKey);
                                paramData.AddField("count", 1);
                                NetworkManager.Send("building/haste", paramData, (jsonData) =>
                                {
                                    if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                                    {
                                        switch (jsonData["rs"].ToObject<int>())
                                        {
                                            case (int)eApiResCode.OK:
                                                Town.Instance.RefreshMap();
                                                UIManager.Instance.RefreshCurrentUI();
                                                break;
                                        }
                                    }
                                });
                            }
                            tempSeq = 10;
                        }
                    }
                        
                    break;
                case TutorialDefine.Adventure:
                    var adventureQuest = QuestManager.Instance.GetQuest(curPrivateKeyList[2]);
                    if (adventureQuest != null && adventureQuest.IsQuestClear())
                        tempSeq = 18;
                    else
                    {
                        QuestEvent.Event(QuestEvent.eEvent.QUEST_OPEN);
                        tempSeq = 1;
                    }
                    break;
            }




            resumeCheck = true;
            var tutorialData = TutorialScriptData.Get(group, tempSeq).RestartTuto;
            group = tutorialData.GROUP;
            seq = tutorialData.SEQUENCE;
        }


        TutorialDefine GetNextTutorial(TutorialDefine CurTutorial, int nextAmount)
        {
            TutorialDefine ret = CurTutorial;
            int repeat = nextAmount;
            while (repeat != 0)
            {
                switch (ret)
                {
                    case TutorialDefine.Construct:
                        ret = (repeat > 0) ?  TutorialDefine.Product : TutorialDefine.Adventure;
                        break;
                    case TutorialDefine.Product:
                        ret = (repeat > 0) ? TutorialDefine.ConstructUI : TutorialDefine.Construct;
                        break;
                    case TutorialDefine.ConstructUI:
                        ret = (repeat > 0) ? TutorialDefine.ProductUI : TutorialDefine.Product;
                        break;
                    case TutorialDefine.ProductUI:
                        ret = (repeat > 0) ? TutorialDefine.DragonGacha : TutorialDefine.ConstructUI;
                        break;
                    case TutorialDefine.DragonGacha:
                        ret = (repeat > 0) ? TutorialDefine.DragonManage : TutorialDefine.ProductUI;
                        break;
                    case TutorialDefine.DragonManage:
                        ret = (repeat > 0) ? TutorialDefine.Battery : TutorialDefine.DragonGacha;
                        break;
                    case TutorialDefine.Battery:
                        ret = (repeat > 0) ? TutorialDefine.Adventure : TutorialDefine.DragonManage;
                        break;
                    case TutorialDefine.Adventure:
                        ret = (repeat > 0) ? TutorialDefine.Construct : TutorialDefine.Battery;
                        break;
                }
                if (repeat > 0) --repeat;
                else if (repeat < 0) ++repeat;
            }
            return ret;
        }

    }
}