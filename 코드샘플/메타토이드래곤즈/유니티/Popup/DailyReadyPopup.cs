using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class DailyReadyData : PopupData
    {
        public eDailyType Daily { get; private set; } = eDailyType.None;
        public int World { get; private set; } = -1;
        public int Stage { get; private set; } = -1;
        public int Difficulty { get; private set; } = -1;

        public DailyReadyData(eDailyType daily, int world, int stage, int diff)
        {
            Daily = daily;
            World = world;
            Stage = stage;
            Difficulty = diff;
        }
    }
    public class DailyReadyPopup : Popup<DailyReadyData>, EventListener<DragonChangedEvent>
    {
        private static eDailyType dailyType = eDailyType.None;
        private static int worldIndex = -1;

		[Space]
		[Header("BG Trasnform")]
		[SerializeField] private Transform worldBGParent = null;

		[Space]
		[Header("Mics")]
		[SerializeField] private Text labelMyBattlePoint = null;
        [SerializeField] private Image teamSettingButtonImg = null;
        [SerializeField] private Sprite[] teamSettingButtonSprites = null;
        [SerializeField] private Text teamSettingButtonLabel = null;
        [SerializeField] private GameObject prefDragonSlot = null;
        [SerializeField] private BattleDragonListView stageDragonList = null;
        [SerializeField] private Transform StageInfoParentTr = null;

        [Space]
		[Header("ElementPowers")]
		[SerializeField] private GameObject[] myBattlePointDetail = null;

		[Space]
		[Header("DragonPosTransform")]
		[SerializeField] private GameObject[] arrDragonParent = null;

		[Space]
		[Header("BG Prefabs")]
		[SerializeField] private GameObject[] bgObject = null;

        [SerializeField]
        private RawImage blurImage = null;

        [Space]
        [Header("StageLabel")]
        [SerializeField] private Text stageLabel = null;
        [SerializeField] private Text enemyBPText = null;

        [Space]
        [Header("Bot Layer")]
        [SerializeField] GameObject dailyBotObj = null;
        [Space]
        [Header("daily layer")]
        [SerializeField] Button dailyEnterBtn = null;
        [SerializeField] Text dailyEnterableCntText = null;
        [SerializeField] Transform spineParent = null;
        [SerializeField] GameObject advIconObj = null;
        [SerializeField] private Button autoDailyWithTicketButton = null;
        [Header("team preset")]
        [SerializeField] Button[] teamPresets = null;

        private DailyDungeonBattleLine battleLine { get { return User.Instance.PrefData.DailyBattleLine; } }
        private List<CharacterSlotFrame> currentDragonDeckList = new List<CharacterSlotFrame>();
        private int currentClickDragonTag = -1;
        private GameObject curBackground = null;

        readonly int dragonBattleLineMaxSize = 5;
        private Dictionary<string, GameObject> prefabObj = null;
        private Dictionary<string, GameObject> backGroundObject = null;

        private VoidDelegate dragonSaveCallBack = null;

        bool isInit = false;
        bool isNetworkState = false;
        bool isAdvertising = false;
        bool isBattleStart = false;

        private int currentTeamPresetNo = 0;
        private void OnEnable()
        {
            EventManager.AddListener<DragonChangedEvent>(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<DragonChangedEvent>(this);
        }
        public override void InitUI()
        {
            //최초에 1번만 해야하지 않나?
            if (bgObject != null)
            {
                if (prefabObj == null)
                    prefabObj = new Dictionary<string, GameObject>();

                prefabObj.Clear();

                for (int i = 0; i < bgObject.Length; ++i)
                {
                    if (bgObject[i] == null)
                        continue;

                    prefabObj[bgObject[i].name] = bgObject[i];
                }
            }
            var screenUpperPos = Screen.safeArea.y;
            if (screenUpperPos > 0)
            {
                float ratio = 1 - (screenUpperPos / (float)Screen.height);
                StageInfoParentTr.localScale = Vector3.one * ratio;
                stageDragonList.transform.localScale = Vector3.one * ratio;
            }

            isAdvertising = false;
            isBattleStart = false;
            if (backGroundObject == null)
                backGroundObject = new();

            
            currentTeamPresetNo = CacheUserData.GetInt("presetDailyDeck"+ Data.World, 0);
            SetPresetBtn(currentTeamPresetNo);
            Initialize();
        }
        void Initialize()
        {
            InitializeData();

            InitializeWorldBG();
            InitializeWorldBGEffect();

            InitializeTeam();

            InitializeInfo();
            InitializeBtn();

            SetTopUI();
            isNetworkState = false;
        }

        void SetTopUI()
        {
            var stageInfo = StageBaseData.GetByWorldStage(Data.World, Data.Stage);
            List<Asset> rewards = stageInfo.REWARD_ITEMS;
            
            var targetItem = -1;
            
            foreach (var item in rewards)
            {
                if (item == null)
                    continue;

                var itemData = ItemBaseData.Get(item.ItemNo);
                if (itemData != null && 
                    (item.GoodType == eGoodType.MAGNET
                    || itemData.KIND == eItemKind.SKILL_UP
                    || itemData.KIND == eItemKind.SHOWCASE))
                {
                    targetItem = item.ItemNo;
                    break;
                }
            }

            if (targetItem > 0)
                PopupManager.Instance.Top.SetInvenItemUI(targetItem);
        }
        void InitializeData()
        {
            if (Data == null)
            {
                LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation);
                return;
            }

            if (dailyType != Data.Daily)
            {
                dailyType = Data.Daily;
                isInit = false;
            }
            if (worldIndex != Data.World)
            {
                worldIndex = Data.World;
                isInit = false;
            }

            //팀 설정이 켜진 상태에서 껏을경우 리셋시켜줌
            if (IsShowDragonList())
			{
				stageDragonList.OnHideList();
                foreach (var deck in currentDragonDeckList)
                {
                    deck.EnableDrag = false;
                }
                ChangeDragonButtonLabel(false);
				HideAllArrowAnimation();
            }
        }
        void InitializeBtn()
        {
            if (dailyEnterBtn == null || dailyEnterableCntText == null)
                return;

            var nCount = GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CLEAR_COUNT");
            int adCount;
            var adInfo = AdvertisementData.Get(SBDefine.AD_DAILY_KEY);
            if (adInfo != null)
                adCount = adInfo.LIMIT;
            else
                adCount = GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CHARGE_COUNT");
            var allCount = nCount + adCount;
            int availableCount = StageManager.Instance.DailyDungeonProgressData.DailyDungeonTicketCount;
            bool enableAuto = false;
            if (availableCount < nCount)
            {
                advIconObj.SetActive(false);
                dailyEnterBtn.interactable = true;
                dailyEnterableCntText.text = string.Format("{0}/{1}", nCount - availableCount,nCount);
                enableAuto = true;
            }
            else if(availableCount < allCount)
            {
                advIconObj.SetActive(true);
                dailyEnterBtn.interactable = true;
                dailyEnterableCntText.text = string.Format("x{0}", allCount - availableCount);
                enableAuto = true;
            }
            else
            {
                advIconObj.SetActive(true);
                dailyEnterBtn.interactable = false;
                dailyEnterableCntText.text = string.Format("<color=#FF0000>x{0}</color>", allCount - availableCount);
            }

            if (autoDailyWithTicketButton != null)
            {
                if (enableAuto)
                {
                    int starCount = 0;
                    enableAuto = GameConfigTable.GetConfigIntValue("sweep_active", 1) > 0;
                    if (enableAuto)
                    {   
                        var progress = StageManager.Instance.DailyDungeonProgressData.GetDailyProgressData(worldIndex);
                        if (progress != null)
                        {
                            if (progress.Count > Data.Stage - 1)
                            {
                                starCount = progress[Data.Stage - 1];
                            }
                        }

                        enableAuto = starCount > 0;
                    }

                    autoDailyWithTicketButton.SetButtonSpriteState(starCount == 3);
                }
                autoDailyWithTicketButton.gameObject.SetActive(enableAuto);
            }
        }

        void InitializeDragonList()
        {
            stageDragonList.OnShowList();
            RegistDragonListCallback();
            stageDragonList.Init(battleLine.GetArray());
            ChangeDragonButtonLabel(true);
            RefreshDragonBattleLineLabel();
        }
        void InitializeWorldBG()
        {
            StageBaseData data = StageBaseData.GetByWorldStage(Data.World, Data.Stage);

            if (data != null)
            {
                if (worldBGParent == null)
                    return;

                if (!prefabObj.TryGetValue(data.IMAGE, out GameObject e))
                    return;

                if (e == null)
                    return;

                if (!backGroundObject.TryGetValue(data.IMAGE, out GameObject backGround))
                    backGroundObject.Add(data.IMAGE, backGround = Instantiate(e, worldBGParent.transform));

                curBackground = backGround;
            }

            ChangeDragonButtonLabel(false);
        }

        void InitializeWorldBGEffect()
        {
            if (backGroundObject != null)
            {
                foreach (var it in backGroundObject)
                {
                    if (it.Value == null)
                        continue;

                    it.Value.SetActive(false);
                }
            }
            if (curBackground != null)
            {
                curBackground.SetActive(true);
            }
        }

        void InitializeInfo()
        {
            SetStageLabel(Data.World, Data.Stage);

            StageBaseData stageInfo = StageBaseData.GetByWorldStage(Data.World, Data.Stage);
            if (stageInfo != null)
            {
                List<MonsterSpawnData> spawnInfo = MonsterSpawnData.GetBySpawnGroup(stageInfo.SPAWN);
                if (spawnInfo != null)
                {
                    int enemyTotalBp = 0;
                    foreach (MonsterSpawnData elem in spawnInfo)
                    {
                        if (elem == null)
                            return;

                        enemyTotalBp += elem.INF;
                    }
                    enemyBPText.text = enemyTotalBp.ToString();
                }
            }

            if (isInit)
                return;

            var dailyStageData = DailyStageData.GetByDay(dailyType);
            if (dailyStageData == null)
                return;
            var dailyData = dailyStageData.Find((cData) => cData.WORLD_NUM == Data.World);
            if (dailyData == null)
                return;

            if (spineParent != null)
            {
                var spinePrefab = dailyData.GetDailySpinePrefab();
                if (spinePrefab != null)
                {
                    SBFunc.RemoveAllChildrens(spineParent);
                    Instantiate(spinePrefab, spineParent);
                }
            }
        }
        void SetStageLabel(int worldNumber, int stageNumber)
        {
            var worldData = WorldData.GetByWorldNumber(worldNumber);
            if (stageLabel != null)
            {
                if (worldData == null)
                    stageLabel.text = SBFunc.StrBuilder(StringData.GetStringByStrKey("스테이지"), " ", string.Format(StringData.GetStringByIndex(100000056), stageNumber));
                else
                    stageLabel.text = SBFunc.StrBuilder(StringData.GetStringByIndex(worldData._NAME), " ", string.Format(StringData.GetStringByIndex(100000056), stageNumber));
            }
        }
        void InitializeTeam()
        {
            battleLine.SetLine(User.Instance.PrefData.GetDailyFormation(currentTeamPresetNo));
            DrawDragon();
        }
        void DrawDragon()
        {
            int myTotalBp = 0;
            RemoveAllDragonPrefab();

            int[] points = new int[(int)eElementType.MAX] { 0, 0, 0, 0, 0, 0, 0 };
            if (currentDragonDeckList == null)
                currentDragonDeckList = new List<CharacterSlotFrame>();
            //currentDragonDeckList.Clear();
            int i = 0, l = 0, lineLimit = 2;
            while (l < 3)
            {
                int val = battleLine.GetDragon(l, i);
                UserDragon element = User.Instance.DragonData.GetDragon(val);

                CharacterSlotFrame characterSlotComp = null;
                if (currentDragonDeckList != null && currentDragonDeckList.Count > l * 2 + i)
                {
                    characterSlotComp = currentDragonDeckList[l * 2 + i];
                }

                if (characterSlotComp == null)
                {
                    var dragonSlot = Instantiate(prefDragonSlot, arrDragonParent[l].transform);
                    dragonSlot.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    dragonSlot.transform.localEulerAngles = new Vector3(0, 0, 40);
                    dragonSlot.SetActive(true);
                    characterSlotComp = dragonSlot.GetComponent<CharacterSlotFrame>();
                }

                if (element != null)
                {
                    myTotalBp += (int)element.Status.GetTotalINF();
                    if (points.Length > element.BaseData.ELEMENT)
                        points[element.BaseData.ELEMENT] += (int)element.Status.GetTotalINF();

                    if (characterSlotComp != null)
                    {
                        characterSlotComp.SetDragonData(element.Tag, true, false, battleLine);
                        characterSlotComp.setCallback((param) =>
                        {
                            if (currentClickDragonTag < 0) 
								return;

                            if (currentClickDragonTag > 0 && int.Parse(param) > 0)  //요 파트가 코코스에선 빠져 있어서 나중에 코코스 재작업한다면 추가 필요
                            {
                                OnClickChangeTeam(int.Parse(param), currentClickDragonTag);
                                HideAllArrowAnimation();
                                return;
                            }

                            bool isHideArrow = characterSlotComp.isHideArrow();

                            if (!isHideArrow) 
								return;

                            bool isDeckFull = battleLine.IsDeckFull();

                            if (isDeckFull)
                            {
                                OnClickReleaseTeam(int.Parse(param));
                                return;
                            }

                            OnClickRegistTeam(currentClickDragonTag);
                            InitClickFullDragonTag();
                        });
                    }
                }
                else
                {
                    characterSlotComp.setEmptyData(l, i);
                    characterSlotComp.setCallback((param) =>
                    {
                        OnClickRegistPosition(characterSlotComp.Line, characterSlotComp.Index);
                        InitClickFullDragonTag();
                    });
                }
                if(currentDragonDeckList.Count <= i + (l * 2))
                {
                    currentDragonDeckList.Add(characterSlotComp);
                }
                else
                {
                    currentDragonDeckList[i + (l * 2)] = characterSlotComp;
                }
                
                //

                if (++i >= lineLimit)
                {
                    i = 0;
                    ++l;
                }
            }

            int elemCount = 0;
            int pCount = points.Length;
            for (i = 0; i < pCount; i++)
            {
                if (points[i] > 0)
                    elemCount++;
            }

            for (i = 1; i < pCount; i++)
            {
                myBattlePointDetail[i - 1].SetActive(points[i] > 0);

                if (points[i] > 0)
                {
                    Text point = myBattlePointDetail[i - 1].transform.Find("Icon").Find("Point").GetComponent<Text>();
                    point.text = points[i].ToString();

                    //if(point.transform is RectTransform rt)
                    //    rt.sizeDelta = new Vector2(50.0f * (5.0f / elemCount), rt.sizeDelta.y);
                }
            }

            labelMyBattlePoint.text = myTotalBp.ToString();
            RefreshDragonBattleLineLabel();
        }
        void OnClickChangeTeam(int BeforeDTag, int AfterDTag)
        {
            battleLine.ChangeDragon(BeforeDTag, AfterDTag);
            DrawDragon();
            HideAllArrowAnimation();
            stageDragonList.RefreshList(battleLine.GetArray());
        }
        void RemoveAllDragonPrefab()
        {
            if (currentDragonDeckList != null)
            {
                foreach (var deck in currentDragonDeckList)
                {
                    deck.SetClear();
                }
            }
        }
        public void OnShowDragonList()
        {
            if (0 == User.Instance.DragonData.GetAllUserDragons().Count)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000623), true, false, true);
                return;
            }

            if (IsShowDragonList())
            {
                OnClickReleaseAllDragon();
            }
            else
            {
                if (null != stageDragonList)
                {
                    InitializeDragonList();
                }
            }
        }
        public void OnHideDragonList()
        {
            if (currentClickDragonTag > 0)
            {
                HideAllArrowAnimation();
                return;
            }

            if (battleLine.IsDeckEmpty())
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393), true, false, true);
                return;
            }

            if (IsChangeDragonList())
            {
                if (stageDragonList != null)
                {
                    stageDragonList.OnHideList();
                    foreach (var deck in currentDragonDeckList)
                    {
                        deck.EnableDrag = false;
                    }
                    ChangeDragonButtonLabel(false);
                    HideAllArrowAnimation();
                }
            }
            else
            {
                DragonOverWriteDataPopup();
            }
        }
        void RegistDragonListCallback()
        {
            stageDragonList.SetRegistCallBack((param) =>
            {
                int tag = int.Parse(param);
                OnClickRegistTeam(tag);

            });

            stageDragonList.SetReleaseCallback((param) =>
            {
                int tag = int.Parse(param);
                OnClickReleaseTeam(tag);

            });
        }
        void DragonOverWriteDataPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000648),
            () => {
                OnClickSaveCurrentDragonFormation();
                if (stageDragonList != null)
                {
                    stageDragonList.OnHideList();
                    foreach (var deck in currentDragonDeckList)
                    {
                        deck.EnableDrag = false;
                    }
                    ChangeDragonButtonLabel(false);
                    HideAllArrowAnimation();
                }
            },
            () => {
                currentTeamPresetNo = CacheUserData.GetInt("presetDailyDeck" + Data.World);
                SetPresetBtn(currentTeamPresetNo);
                battleLine.SetLine(User.Instance.PrefData.GetDailyFormation(currentTeamPresetNo));
                DrawDragon();
                if (stageDragonList != null)
                {
                    HideAllArrowAnimation();
                    ChangeDragonButtonLabel(false);
                    HideAllArrowAnimation();
                    stageDragonList.RefreshList(battleLine.GetArray());
                }
            },
            () => {
            });
        }
        bool IsChangeDragonList()
        {
            var daily = User.Instance.PrefData.DailyDungeonFormationData;
            if (daily == null) 
				return false;

            var teamArr = daily.TeamFormation;//각 탭별로 세팅해줘야함.
            var dragonArr = teamArr[currentTeamPresetNo];
            var currentDragonArr = battleLine.GetList();//현재 등록된 드래곤 리스트

            if (dragonArr == null || dragonArr.Count <= 0) //서버에서 받아온 드래곤리스트가 미등록(빈값) 일 때
            {
                if (battleLine.IsDeckEmpty()) 
					return true;  //내가 가진덱과 서버의 덱이 둘다 비어져 있는 경우
                return false;
            }

            return IsArrayEqual(dragonArr.ToList(), currentDragonArr);
        }
        bool IsArrayEqual(List<int> list1, List<int> list2)
        {
            var areListsEqual = true;

            for (var i = 0; i < list1.Count; i++)
            {
                if (list2[i] != list1[i])
                {
                    areListsEqual = false;
                }
            }

            return areListsEqual;
        }
        bool IsShowDragonList()
        {
            if (null == stageDragonList)
                return false;

            return stageDragonList.IsShowList();
        }
        void ChangeDragonButtonLabel(bool isOpen)
        {
            if (null != teamSettingButtonLabel)
            {
                teamSettingButtonLabel.text = isOpen ? StringData.GetStringByIndex(100000394) : StringData.GetStringByIndex(100000221);
                teamSettingButtonImg.sprite = isOpen ? teamSettingButtonSprites[1] : teamSettingButtonSprites[0];
            }
        }

        void OnClickReleaseAllDragon()
        {
            if (battleLine != null)
            {
                if (battleLine.IsDeckEmpty())
                    return;

                battleLine.Clear();
                DrawDragon();
                stageDragonList.RefreshList(battleLine.GetArray());
                HideAllArrowAnimation();
            }
        }

        public void OnClickRegistTeam(int dragonTag)
        {
            if (false == IsShowDragonList()) 
                return;

            if (currentClickDragonTag > 0)
            {
                HideAllArrowAnimation();
            }

            currentClickDragonTag = dragonTag;

            if (battleLine.IsDeckFull())
            {
                for (int i = 0; i < currentDragonDeckList.Count; ++i)
                {
                    if (battleLine.GetDragon(i) > 0)
                    {
                        currentDragonDeckList[i].ShowAnimArrowNode(false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < currentDragonDeckList.Count; ++i)
                {
                    currentDragonDeckList[i].ShowAnimArrowNode(false);
                }
            }
        }

        void OnClickRegistPosition(int line, int index)
        {
            if (battleLine.GetDragon(line, index) <= 0 && currentClickDragonTag <= 0) 
				return;

            battleLine.AddDragonPosition(line, index, currentClickDragonTag);
            DrawDragon();
            stageDragonList.RefreshList(battleLine.GetArray());
            HideAllArrowAnimation();
        }

        public void OnClickReleaseTeam(int dragonTag)
        {
            bool isVisible = IsShowDragonList();
            if (!isVisible) 
				return;

            battleLine.DeleteDragon(dragonTag);
            DrawDragon();
            stageDragonList.RefreshList(battleLine.GetArray());
            HideAllArrowAnimation();
        }

        protected override IEnumerator OpenAnimation()
        {
            dimClose = false;
            InitUI();
            dimClose = true;
            yield break;
        }

        public override void ClosePopup()
        {
            if (!isAdvertising)
            {
                if (SceneManager.GetActiveScene().name.Equals("DailyReward")&& isBattleStart ==false)
                {
                    LoadingManager.Instance.EffectiveSceneLoad("DailyDungeonLobby");
                    
                }
                base.ClosePopup();
            }
        }
        public override bool IsModeless()
        {
            return false;
        }
        public void OnClickSaveCurrentDragonFormation(bool isTryOut = false)
        {
            if (isNetworkState)
                return;

            isNetworkState = true;

            HideAllArrowAnimation();
            bool isEmpty = battleLine.IsDeckEmpty();
            if (isEmpty)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393), true, false, true);
                return;
            }

            //var dailyIndex = SBFunc.GetDailyWorldIndexConvertIndex(Data.World);

            WWWForm param = new WWWForm();
            param.AddField("teamno", currentTeamPresetNo);
            param.AddField("dragons", SBFunc.ListToString(battleLine.GetList()));
            param.AddField("items", "[0, 0, 0]"); //임시 - 팀장비 갱신 데이터 넘겨야함
            param.AddField("teamname", "daily");
            NetworkManager.Send("preference/setteam", param, (JObject jsonData) =>
            {
                isNetworkState = false;
                if (jsonData["err"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                {
                    User.Instance.PrefData.DailyDungeonFormationData.ClearFomationData(currentTeamPresetNo); //0 = 임시 - 드래곤 페이지 인덱스
                    User.Instance.PrefData.DailyDungeonFormationData.SetFormationData(currentTeamPresetNo, battleLine.GetList()); //0 = 임시 - 드래곤 페이지 인덱스
                    CacheUserData.SetInt("presetDailyDeck"+ Data.World, currentTeamPresetNo);
                    dragonSaveCallBack?.Invoke();
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000624), 
                        isTryOut ? SaveCompleteAndRequest : SaveComplete, 
                        null, 
                        isTryOut ? SaveCompleteAndRequest : SaveComplete
                    );
                }
            }, (string arg) =>
            {
                isNetworkState = false;
            });
        }

        public void SetDragonSaveCallBack(VoidDelegate cb)
        {
            dragonSaveCallBack = cb;
        }

        void SaveCompleteAndRequest()
        {
            SaveComplete();
            RequestVariationSceneCheck();
        }

        void SaveComplete()
        {
            if (IsShowDragonList())
            {
                if (stageDragonList != null)
                {
                    stageDragonList.OnHideList();
                    foreach (var deck in currentDragonDeckList)
                    {
                        deck.EnableDrag = false;
                    }
                    ChangeDragonButtonLabel(false);
                    HideAllArrowAnimation();
                }
            }

            PopupManager.ClosePopup<SystemPopup>();
        }

        void InitClickFullDragonTag()
        {
            currentClickDragonTag = -1;
        }

        void ShowAllArrowAnimation()
        {
            if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0)
                return;

            foreach (CharacterSlotFrame elem in currentDragonDeckList)
            {
                if (elem == null)
                    return;

                elem.ShowAnimArrowNode(false);
            }
        }

        void HideAllArrowAnimation()
        {
            InitClickFullDragonTag();

            if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0) 
                return;

            foreach (CharacterSlotFrame elem in currentDragonDeckList)
            {
                if (elem == null) 
                    continue;

                elem.HideAnimArrowNode();
            }
        }

        void RefreshDragonBattleLineLabel()
        {
            if (stageDragonList == null || battleLine == null)
                return;

            stageDragonList.RefreshDragonCountLabel(battleLine.DeckCount, dragonBattleLineMaxSize);
        }

        public void OnClickExpectGameUpdate()
        {
            ToastManager.On(100000326);   //100000326 토스트메세지
		}

        public void OnClickStageSelectScene()
        {
            if (IsChangeDragonList())
            {
                RequestVariationSceneCheck();
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000648),
                    () => {
                        OnClickSaveCurrentDragonFormation(true);
                        if (stageDragonList != null)
                        {
                            HideAllArrowAnimation();
                        }
                    },
                    () => {
                        battleLine.SetLine(User.Instance.PrefData.GetDailyFormation(currentTeamPresetNo));
                        DrawDragon();
                        if (stageDragonList != null)
                        {
                            HideAllArrowAnimation();
                            RequestVariationSceneCheck();
                        }
                    },
                    null,
                true, true, true);
            }
        }
        void RequestVariationSceneCheck()
        {
            StageManager.Instance.SetWorld(Data.World);
            LoadingManager.Instance.EffectiveSceneLoad("AdventureStageSelect", eSceneEffectType.CloudAnimation);
        }

        #region BtnClick
        public override void OnClickDimd()
        {
            if (!isAdvertising)
            {
                base.ClosePopup();
            }
            
        }
        void SuggestionOkProcess()
        {
            if (null == battleLine)
                return;

            SBFunc.SetAutoBattleLine(battleLine);

            DrawDragon();

            if (null != stageDragonList)
            {
                stageDragonList.InitSuggest(battleLine.GetArray());
                stageDragonList.RefreshList(battleLine.GetArray());
            }

            HideAllArrowAnimation();
            OnClickSaveCurrentDragonFormation();
        }
        public void OnClickSuggestion()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey(("battle_popup_team_recommend")), StringData.GetStringByIndex(100001251),
                () => { SuggestionOkProcess(); }, () => { }, () => { });
        }
        public void OnClickDailyBattleStart()
        {
            if (battleLine.IsDeckEmpty())
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393), true, false, true);
                return;
            }
            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, Data.World, Data.Stage) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () => {
                        //LoadingManager.ImmediatelySceneLoad("Town");
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                    },
                    () => {
                    },
                    () => {
                    }
                );
                return;
            }

            var nCount = GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CLEAR_COUNT");
            int adCount;
            var adInfo = AdvertisementData.Get(SBDefine.AD_DAILY_KEY);
            if (adInfo != null)
                adCount = adInfo.LIMIT;
            else
                adCount = GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CHARGE_COUNT");
            var allCount = nCount + adCount;
            int availableCount = StageManager.Instance.DailyDungeonProgressData.DailyDungeonTicketCount;
            if(availableCount < nCount)
            {
                DailyBattleStart();
            }
            else if((availableCount - nCount) < adCount)
            {
                dailyEnterBtn.interactable = false;
                isAdvertising = true;
                AdvertiseManager.Instance.TryADWithPopup((log) => {
                    DailyBattleStart(true, log);
                    dailyEnterBtn.interactable = true;
                    isAdvertising = false;
                }, () =>
                {
                    ToastManager.On(StringData.GetStringByStrKey("광고실패"));
                    dailyEnterBtn.interactable = true;
                    isAdvertising = false;
                }, () =>
                {
                    dailyEnterBtn.interactable = true;
                    isAdvertising = false;
                });
                //if (!AdvertiseManager.Instance.IsAdvertiseReady())
                //{
                //    ToastManager.On("광고 로드하는 중");
                //    dailyEnterBtn.interactable = false;
                //    StartCoroutine(nameof(AdBtnClick));
                //    return;
                //}
            }
            else
            {
                ToastManager.On(100002103);
                return;
            }
        }

        public void OnClickDailyBattleAutoStart()
        {
            var progress = StageManager.Instance.DailyDungeonProgressData.GetDailyProgressData(worldIndex);
            if (progress != null)
            {
                if (progress.Count > Data.Stage - 1)
                {
                    if(progress[Data.Stage - 1] <= 0)
                    {
                        ToastManager.On("소탕_요일던전클리어제한");
                        return;
                    }
                }
                else
                {
                    ToastManager.On("소탕_요일던전클리어제한");
                    return;
                }
            }


            if (battleLine.IsDeckEmpty())
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393), true, false, true);
                return;
            }
            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, Data.World, Data.Stage) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () => {
                        //LoadingManager.ImmediatelySceneLoad("Town");
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => PopupManager.OpenPopup<InventoryPopup>()));
                    },
                    () => {
                    },
                    () => {
                    }
                );
                return;
            }

            var nCount = GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CLEAR_COUNT");
            int adCount;
            var adInfo = AdvertisementData.Get(SBDefine.AD_DAILY_KEY);
            if (adInfo != null)
                adCount = adInfo.LIMIT;
            else
                adCount = GameConfigTable.GetConfigIntValue("DAY_DUNGEON_CHARGE_COUNT");
            var allCount = nCount + adCount;
            int availableCount = StageManager.Instance.DailyDungeonProgressData.DailyDungeonTicketCount;
            
            if (availableCount < nCount)
            {
                DailyBattleStartWithTicketStart();
            }
            else if ((availableCount - nCount) < adCount)
            {
                isAdvertising = true;
                AdvertiseManager.Instance.TryADWithPopup((log) => {
                    DailyBattleStartWithTicketStart(true, log);
                    isAdvertising = false;
                }, () =>
                {
                    ToastManager.On(StringData.GetStringByStrKey("광고실패"));
                    isAdvertising = false;
                }, () =>
                {

                });
            }
            else
            {
                ToastManager.On(100002103);
                return;
            }
        }
        private void DailyBattleStart(bool isAd=false, string log = "")
        {
            WWWForm param = new WWWForm();
            param.AddField("world", Data.World);
            param.AddField("diff", Data.Difficulty);
            param.AddField("stage", Data.Stage);
            param.AddField("deck", battleLine.GetJsonString());
            param.AddField("is_ad", isAd ? 1 : 0);
            param.AddField("ad_log", log);
            param.AddField("deckinf", battleLine.GetJsonStringINF());
            NetworkManager.Send("daily/dailystart", param, (JObject jsonData) =>
            {
                switch ((eApiResCode)(int)jsonData["rs"])
                {
                    case eApiResCode.OK:
                        DailyManager.Instance.SetStartData(Data.World, Data.Stage, jsonData);
                        if (!DailyManager.Instance.IsStartCheck())
                            return;
                        isBattleStart = true;
                        LoadingManager.Instance.EffectiveSceneLoad("DailyBattle", eSceneEffectType.CloudAnimation, DailyManager.Instance.LoadingCoroutine);
                        break;
                    case eApiResCode.DAILY_DAY_NOT_MATCHED:
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("요일던전 초기화 문구"),
                            DailyDungeonMisMatchProcess,
                            null,
                            DailyDungeonMisMatchProcess
                        , true, false, true);
                        break;
                    case eApiResCode.ADV_INVALID_DRAGON:
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000635), true, false, false);
                        break;
                }
            });
        }

        public void DailyBattleStartWithTicketStart(bool isAd = false, string log = "")
        {
            WWWForm param = new();
            param.AddField("world", Data.World);
            param.AddField("diff", Data.Difficulty);
            param.AddField("stage", Data.Stage);
            param.AddField("deck", battleLine.GetJsonString());
            param.AddField("is_ad", isAd ? 1 : 0);
            param.AddField("ad_log", log);
            param.AddField("deckinf", battleLine.GetJsonStringINF());
            param.AddField("repeat", 1);

            NetworkManager.Send("daily/sweep", param, (JObject jsonData) =>
            {
                if (jsonData["rs"] != null)
                {
                    switch ((eApiResCode)(int)jsonData["rs"])
                    {
                        case eApiResCode.OK:
                            DailyManager.Instance.SetAutoTicketData(Data.World, Data.Stage, jsonData);
                            LoadingManager.Instance.EffectiveSceneLoad("DailyReward", eSceneEffectType.CloudAnimation);
                            break;

                        case eApiResCode.COST_SHORT:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100002249), StringData.GetStringByIndex(100002249));
                            break;
                        case eApiResCode.ADV_INVALID_DRAGON:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000635), true, false, false);
                            break;
                    }
                }
            }, (string arg) =>
            {

            });
        }
        private void DailyDungeonMisMatchProcess()
        {
            LoadingManager.Instance.EffectiveSceneLoad("DailyDungeonLobby");
            ClosePopup();
        }

        private IEnumerator AdBtnClick()
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN      
            while (!AdvertiseManager.Instance.IsAdvertiseReady())
            {
                yield return SBDefine.GetWaitForSecondsRealtime(0.5f);
            }                  
#endif
            dailyEnterBtn.interactable = true;
            yield break;
        }

#endregion
        public void OnEvent(DragonChangedEvent eventType)
        {
            if (eventType.type == DragonChangedEvent.TYPE.MOVE_START)
            {
                if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0)
                {
                    return;
                }

                foreach (CharacterSlotFrame elem in currentDragonDeckList)
                {
                    if (elem.DragonTag == eventType.tag)
                        continue;

                    if (elem == null)
                    {
                        return;
                    }
                    elem.ShowAnimArrowNode(false);
                }

                return;
            }
            else if (eventType.type == DragonChangedEvent.TYPE.MOVE_DONE)
            {
                CharacterSlotFrame mover = null;
                CharacterSlotFrame target = null;

                foreach (var deck in currentDragonDeckList)
                {
                    if (deck.DragonTag == eventType.tag)
                    {
                        mover = deck;
                    }
                    else if (RectTransformUtility.RectangleContainsScreenPoint(deck.transform as RectTransform, eventType.targetSlotPos, UICanvas.Instance.GetCamera()))
                    {
                        target = deck;
                    }
                }

                if (mover != null && target != null)
                {
                    battleLine.SwapSlot(mover.Line * 2 + mover.Index, target.Line * 2 + target.Index);
                }

                HideAllArrowAnimation();
            }

            DrawDragon();
        }



        public void OnClickTeamPreset(int presetNo)
        {
            currentTeamPresetNo = presetNo;
            SetPresetBtn(presetNo);
            battleLine.SetLine(User.Instance.PrefData.GetDailyFormation(presetNo));
            DrawDragon();
            
            stageDragonList.Init(battleLine.GetArray());
            ChangeDragonButtonLabel(IsShowDragonList());
            RefreshDragonBattleLineLabel();
            CacheUserData.SetInt("presetDailyDeck" + Data.World, currentTeamPresetNo);
        }

        void SetPresetBtn(int clickedIndex)
        {
            foreach (var btn in teamPresets)
            {
                btn.interactable = true;
            }
            teamPresets[clickedIndex].interactable = false;
        }
    }
}