using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class RewardItemInfo : ITableData
    {
        public eGoodType type = eGoodType.ITEM;
        public int itemNo = 0;
        public int count = 0;
        public int star = 0;
        public bool check = false;

        public RewardItemInfo(int no, int cnt, int starCnt, bool chk)
        {
            itemNo = no;
            count = cnt;
            star = starCnt;
            check = chk;
        }
        public RewardItemInfo(Asset reward, int starCnt, bool chk)
        {
            type = reward.GoodType;
            itemNo = reward.ItemNo;
            count = reward.Amount;
            star = starCnt;
            check = chk;
        }

        public void Init() { }

        public string GetKey() { return itemNo.ToString(); }
    }
    public class StageInfoPopupData : StagePopupData
    {
        public int Star { get; private set; } = -1;

        public StageInfoPopupData(int world, int stage, int diff, int star = 0)
            : base(world, stage, diff)
        {
            Star = star;
        }
    }

    public class RestrictedInfoPopupData : StagePopupData
    {
        public List<int> UsingDragons = new List<int>();
        public int BP_CAP = -1;
        public RestrictedInfoPopupData(int world, StageDifficult diff, List<int> usingDragons, int cap = -1)
            : base(world, 1, (int)diff)
        {
            UsingDragons = usingDragons;
            BP_CAP = cap;
        }
    }
    public class AdventureReadyPopup : Popup<StageInfoPopupData>, EventListener<DragonChangedEvent>
    {
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
        [SerializeField] private WorldStageInfo currentWolrdStageInfo = null;
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
        [SerializeField] private Image bodyUI = null;
        [SerializeField] private Sprite[] bodySprite = null;
        [SerializeField] private Image[] bodyBacks = null;
        [SerializeField] private Color[] bodyColors = null;
        [SerializeField] private Text difficultText = null;
        [SerializeField] private Color[] textColors = null;

        [SerializeField]
        private RawImage blurImage = null;

        [Space]
        [Header("StageLabel")]
        [SerializeField] private Image stageIconSprite = null;
        [SerializeField] private Text stageLabel = null;
        [SerializeField] private Sprite[] unLockIconList = null;
        [SerializeField] private Sprite[] lockIconList = null;
        protected bool IsLock { get; private set; } = false;

        [Space]
        [Header("Bot Layer")]
        [SerializeField] GameObject adventureBotObj = null;
        [SerializeField] GameObject dailyBotObj = null;
        [Space]
        [Header("daily layer")]
        [SerializeField] Button dailyEnterBtn = null;
        [SerializeField] Text dailyEnterableCntText = null;
        [Header("team preset")]
        [SerializeField] Button[] teamPresets = null;

        private RenderTexture blurTexture = null;

        private AdventureBattleLine battleLine { get { return User.Instance.PrefData.AdventureBattleLine; } }
        private List<CharacterSlotFrame> currentDragonDeckList = new List<CharacterSlotFrame>();
        private int currentClickDragonTag = -1;
        private string drawBG = "";
        private GameObject curBackground = null;

        const int dragonBattleLineMaxSize = 5;
        private Dictionary<string, GameObject> prefabObj = null;
        private Dictionary<string, GameObject> backGroundObject = null;

        private VoidDelegate dragonSaveCallBack = null;
        private VoidDelegate stageStartCallBack = null;

        bool isAdventureStart = false;
        bool isNetworkState = false;

        private int currentTeamPresetNo = 0;
        private void OnEnable()
        {
            EventManager.AddListener<DragonChangedEvent>(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<DragonChangedEvent>(this);
        }
        void SetDefaultData()
        {
            if (Data == null)
            {
                LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation);
                return;
            }
            adventureBotObj.SetActive(true);
            dailyBotObj.SetActive(false);
            //UIManager.Instance.InitUI(eUIType.Adventure);
            //UIManager.Instance.RefreshUI(eUIType.Adventure);

            foreach (var deck in currentDragonDeckList)
            {
                deck.EnableDrag = false;
            }
            //팀 설정이 켜진 상태에서 껏을경우 리셋시켜줌
            if (IsShowDragonList())
            {
                stageDragonList.OnHideList();
                ChangeDragonButtonLabel(false);
                HideAllArrowAnimation();
            }
        }
        void SetData()
        {
            SetDefaultData();

            InitWorldBG();
            InitWorldBGEffect();
            SetStageInfo();
            DrawDragon();
            PlayBGMSound();
            isNetworkState = false;
        }
        void SetDragonList()
        {
            stageDragonList.OnShowList();
            foreach (var deck in currentDragonDeckList)
            {
                deck.EnableDrag = true;
            }

            RegistDragonListCallback();
            stageDragonList.Init(battleLine.GetArray());
            ChangeDragonButtonLabel(true);
            RefreshDragonBattleLineLabel();
        }
        void InitWorldBG()
        {
#if false
            if (blurTexture == null)
                blurTexture = new RenderTexture((int)(Screen.width * 0.25f), (int)(Screen.height * 0.25f), 0);

            Camera.main.targetTexture = blurTexture;
            blurImage.texture = blurTexture;
            blurImage.enabled = true;
#endif
            StageBaseData data = StageBaseData.GetByAdventureWorldStage(Data.World, Data.Stage);

            if (data != null)
            {
                if (worldBGParent == null)
                    return;

                if (!prefabObj.TryGetValue(data.IMAGE, out GameObject e))
                    return;

                if (e == null)
                    return;

                drawBG = data.IMAGE;

                if (!backGroundObject.TryGetValue(data.IMAGE, out GameObject backGround))
                    backGroundObject.Add(data.IMAGE, backGround = Instantiate(e, worldBGParent.transform));

                curBackground = backGround;
            }

            ChangeDragonButtonLabel(false);
        }
        void InitWorldBGEffect()
        {
#if false
            var effect = blurImage.GetComponent<Coffee.UIEffects.UIEffect>();
            if (effect != null)
            {
                effect.enable = true;
                effect.blurFactor = 0.0f;
                DG.Tweening.DOTween.To(() => 0.0f, factor =>
                {
                    effect.blurFactor = factor;
                }, 1.0f, 1.0f);
            }
#endif
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
                //foreach (var graphic in curBackground.GetComponentsInChildren<MaskableGraphic>())
                //{
                //    Color color = graphic.color;
                //    color.a = 0.0f;
                //    graphic.color = color; 
                //    graphic.DOFade(1.0f, 0.5f).SetEase(Ease.InQuad);
                //}
            }
        }
        void SetStageInfo()
        {
            SetStageLabel(Data.World, Data.Stage);
            SetDesignByWorldMapName();
            currentWolrdStageInfo.SetData(Data.World, Data.Stage, Data.Star, false, true, onClickBattleStart);
        }
        void SetDesignByWorldMapName()
        {
            int difficult = CacheUserData.GetInt("adventure_difficult", 1);
            int checkIndex = difficult - 1;
            if (unLockIconList != null && unLockIconList.Length > 0 && unLockIconList.Length > checkIndex && checkIndex >= 0)
                stageIconSprite.sprite = unLockIconList[checkIndex];

            bodyUI.sprite = bodySprite[checkIndex];
            foreach (var back in bodyBacks)
            {
                back.color = bodyColors[checkIndex];
            }

            switch ((StageDifficult)difficult)
            {
                case StageDifficult.HARD:
                    difficultText.text = StringData.GetStringByStrKey("어려움난이도");
                    break;
                case StageDifficult.HELL:
                    difficultText.text = StringData.GetStringByStrKey("지옥난이도");
                    break;
                case StageDifficult.NORMAL:
                default:
                    difficultText.text = StringData.GetStringByStrKey("보통난이도");
                    break;
            }

            difficultText.color = textColors[checkIndex];
        }
        void SetStageLabel(int worldNumber, int stageNumber)
        {
            var worldData = WorldData.GetByWorldNumber(worldNumber);
            if (stageLabel != null)
            {
                if (worldData == null)
                    stageLabel.text = string.Format(SBFunc.StrBuilder(StringData.GetStringByStrKey("스테이지"), " {0} - {1}"), worldNumber, stageNumber);
                else
                    stageLabel.text = string.Format(SBFunc.StrBuilder(StringData.GetStringByIndex(worldData._NAME), " {0} - {1}"), worldNumber, stageNumber);
            }
        }
        void DrawDragon()
        {
            battleLine.LoadBattleLine(currentTeamPresetNo);
            DrawTeamDragon();
        }
        public void DrawTeamDragon()
        {
            int myTotalBp = 0;
            RemoveAllDragonPrefab();
            int[] points = new int[(int)eElementType.MAX] { 0, 0, 0, 0, 0, 0, 0 };
            if (currentDragonDeckList == null)
                currentDragonDeckList = new List<CharacterSlotFrame>();
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
                        characterSlotComp.SetDragonData(element.Tag, true, false, battleLine, IsShowDragonList());
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

                            onClickRegistTeam(currentClickDragonTag);
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

                if (currentDragonDeckList.Count <= i + (l * 2))
                {
                    currentDragonDeckList.Add(characterSlotComp);
                }
                else
                {
                    currentDragonDeckList[i + (l * 2)] = characterSlotComp;
                }

                if (++i >= lineLimit)
                {
                    i = 0;
                    ++l;
                }
            }

            int elemCount = 0;
            for (i = 0; i < points.Length; i++)
            {
                if (points[i] > 0)
                    elemCount++;
            }

            for (i = 1; i < points.Length; i++)
            {
                myBattlePointDetail[i - 1].SetActive(points[i] > 0);

                if (points[i] > 0)
                {
                    Text point = myBattlePointDetail[i - 1].transform.Find("Icon").Find("Point").GetComponent<Text>();
                    point.text = points[i].ToString();

                    //RectTransform rt = (point.transform as RectTransform);
                    //rt.sizeDelta = new Vector2(50.0f * (5.0f / elemCount), rt.sizeDelta.y);
                }
            }

            labelMyBattlePoint.text = myTotalBp.ToString();
            RefreshDragonBattleLineLabel();

            currentWolrdStageInfo.UpdateMyBattlePoint(myTotalBp);
        }
        void OnClickChangeTeam(int BeforeDTag, int AfterDTag)
        {
            battleLine.ChangeDragon(BeforeDTag, AfterDTag);
            DrawTeamDragon();
            HideAllArrowAnimation();
            stageDragonList.RefreshList(battleLine.GetArray());
        }
        void onClickBattleStart()
        {
            if (battleLine.IsDeckEmpty())
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393), true, false, true);
                return;
            }

            if (User.Instance.CheckInventoryForContentsEnter(eInvenSlotCheckContentType.Adventure, Data.World, Data.Stage) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () =>
                    {
                        LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() =>
                        {
                            PopupManager.OpenPopup<InventoryPopup>();
                        }));
                    }
                );
                return;
            }

            WWWForm param = new();
            param.AddField("world", Data.World);
            param.AddField("diff", Data.Diff);
            param.AddField("stage", Data.Stage);
            param.AddField("deck", battleLine.GetJsonString());
            param.AddField("deckinf", battleLine.GetJsonStringINF());
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("adventure/start", param, (JObject jsonData) =>
            {
                isNetworkState = false;
                if (jsonData["rs"] != null)
                {
                    switch ((eApiResCode)(int)jsonData["rs"])
                    {
                        case eApiResCode.OK:
                            AdventureManager.Instance.SetStartData(Data.World, Data.Stage, jsonData);
                            StageManager.Instance.AdventureProgressData.SetCursorDataIndex(Data.World, Data.Stage, Data.Diff);

                            if (!AdventureManager.Instance.IsStartCheck())
                                return;
                            stageStartCallBack?.Invoke();
                            isAdventureStart = true;
                            LoadingManager.Instance.EffectiveSceneLoad("AdventureBattle", eSceneEffectType.CloudAnimation, AdventureManager.Instance.LoadingCoroutine);
                            break;

                        case eApiResCode.COST_SHORT:
                            var costType = StageBaseData.GetByAdventureWorldStage(Data.World, Data.Stage).COST_TYPE;
                            string needItemName = "";

                            switch (costType)
                            {
                                case "ENERGY":
                                    needItemName = StringData.GetStringByStrKey("item_base:name:10000002");
                                    break;
                                case "DAILY_TICKET":
                                    needItemName = string.Format(StringData.GetStringByIndex(100002096), "");
                                    break;
                            }

                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), string.Format(StringData.GetStringByIndex(100000224), needItemName));
                            break;
                        case eApiResCode.ADV_INVALID_DRAGON:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000635), true, false, false);
                            break;
                    }
                }
            }, (string arg) =>
            {
                isNetworkState = false;
            });
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
            if (User.Instance.DragonData.GetAllUserDragons().Count == 0)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000623), true, false, true);
                return;
            }

            bool isVisiable = IsShowDragonList();

            if (isVisiable)
            {
                OnClickReleaseAllDragon();
            }
            else
            {
                if (stageDragonList != null)
                {
                    SetDragonList();
                }
            }
        }
        public void onHideDragonList()
        {
            if (currentClickDragonTag > 0)
            {
                HideAllArrowAnimation();
                return;
            }

            bool isEmpty = battleLine.IsDeckEmpty();
            if (isEmpty)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393), true, false, true);
                return;
            }

            bool isEqual = isChangeDragonList();

            if (isEqual)
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
                dragonOverWriteDataPopup();
            }
        }
        void RegistDragonListCallback()
        {
            stageDragonList.SetRegistCallBack((param) =>
            {
                int tag = int.Parse(param);
                onClickRegistTeam(tag);

            });

            stageDragonList.SetReleaseCallback((param) =>
            {
                int tag = int.Parse(param);
                OnClickReleaseTeam(tag);

            });
        }
        void dragonOverWriteDataPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000648),
            () =>
            {
                onClickSaveCurrentDragonFormation();
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
            () =>
            {
                currentTeamPresetNo = CacheUserData.GetInt("presetAdventureDeck", 0);
                SetPresetBtn(currentTeamPresetNo);
                battleLine.LoadBattleLine(currentTeamPresetNo);
                DrawTeamDragon();
                if (stageDragonList != null)
                {
                    HideAllArrowAnimation();
                    ChangeDragonButtonLabel(false);
                    HideAllArrowAnimation();
                    stageDragonList.RefreshList(battleLine.GetArray());
                }
            },
            () =>
            {
            });
        }
        bool isChangeDragonList()
        {
            var adventureData = User.Instance.PrefData.AdventureFormationData;

            if (adventureData == null)
                return false;

            var teamArr = adventureData.TeamFormation;//각 탭별로 세팅해줘야함.
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
            if (stageDragonList != null)
            {
                return stageDragonList.IsShowList();
            }

            return false;
        }
        void ChangeDragonButtonLabel(bool isOpen)
        {
            if (teamSettingButtonLabel != null)
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
                DrawTeamDragon();
                stageDragonList.RefreshList(battleLine.GetArray());
                HideAllArrowAnimation();
            }
        }
        public void onClickRegistTeam(int dragonTag)
        {
            bool isVisible = IsShowDragonList();
            if (!isVisible) return;

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
            DrawTeamDragon();
            stageDragonList.RefreshList(battleLine.GetArray());
            HideAllArrowAnimation();
        }
        public void OnClickReleaseTeam(int dragonTag)
        {
            bool isVisible = IsShowDragonList();
            if (!isVisible)
                return;

            battleLine.DeleteDragon(dragonTag);
            DrawTeamDragon();
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
        public void onClickSaveCurrentDragonFormation(bool isTryOut = false)
        {
            HideAllArrowAnimation();
            bool isEmpty = battleLine.IsDeckEmpty();
            if (isEmpty)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393), true, false, true);
                return;
            }

            WWWForm param = new WWWForm();
            param.AddField("teamno", currentTeamPresetNo); //임시 - 드래곤 페이지 인덱스
            param.AddField("dragons", SBFunc.ListToString(battleLine.GetList()));
            param.AddField("items", "[0, 0, 0]"); //임시 - 팀장비 갱신 데이터 넘겨야함
            param.AddField("teamname", "adventure");
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("preference/setteam", param, (JObject jsonData) =>
            {
                isNetworkState = false;
                if (jsonData["err"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                {
                    User.Instance.PrefData.AdventureFormationData.ClearFomationData(currentTeamPresetNo); //0 = 임시 - 드래곤 페이지 인덱스
                    User.Instance.PrefData.AdventureFormationData.SetFormationData(currentTeamPresetNo, battleLine.GetList()); //0 = 임시 - 드래곤 페이지 인덱스
                    CacheUserData.SetInt("presetAdventureDeck", currentTeamPresetNo);
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
            if (cb != null)
            {
                dragonSaveCallBack = cb;
            }
        }
        public void SetStageStartCallBack(VoidDelegate cb)
        {
            if (cb != null)
            {
                stageStartCallBack = cb;
            }
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
        void OnClickFullDragonDeckProcess(int currentClickTag)
        {
            currentClickDragonTag = currentClickTag;
            ShowAllArrowAnimation();
        }
        void ShowAllArrowAnimation()
        {
            if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0)
            {
                return;
            }

            foreach (CharacterSlotFrame elem in currentDragonDeckList)
            {
                if (elem == null)
                {
                    return;
                }
                elem.ShowAnimArrowNode(false);
            }
        }
        void HideAllArrowAnimation()
        {
            InitClickFullDragonTag();
            if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0) return;

            foreach (CharacterSlotFrame elem in currentDragonDeckList)
            {
                if (elem == null) continue;
                elem.HideAnimArrowNode();
            }
        }
        void RefreshDragonBattleLineLabel()
        {
            if (battleLine == null || stageDragonList == null)
                return;

            stageDragonList.RefreshDragonCountLabel(battleLine.DeckCount, dragonBattleLineMaxSize);
        }
        public void onClickExpectGameUpdate()
        {
            ToastManager.On(100000326);   //100000326 토스트메세지
        }
        public void onClickStageSelectScene()
        {
            bool isEqual = isChangeDragonList();

            if (isEqual)
            {
                RequestVariationSceneCheck();
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000648),
                    () =>
                    {
                        onClickSaveCurrentDragonFormation(true);
                        if (stageDragonList != null)
                        {
                            HideAllArrowAnimation();
                        }
                    },
                    () =>
                    {
                        battleLine.LoadBattleLine();
                        DrawTeamDragon();
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
        void SuggestionOkProcess()
        {
            if (battleLine == null)
                return;

            SBFunc.SetAutoBattleLine(battleLine);

            DrawTeamDragon();

            if (stageDragonList != null)
            {
                stageDragonList.InitSuggest(battleLine.GetArray());
                stageDragonList.RefreshList(battleLine.GetArray());
            }

            HideAllArrowAnimation();
            onClickSaveCurrentDragonFormation();
        }
        public void OnClickSuggestion()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey(("battle_popup_team_recommend")), StringData.GetStringByIndex(100001251),
                () => { SuggestionOkProcess(); }, () => { }, () => { });
        }
        void PlayBGMSound()
        {
            SoundManager.Instance.PushBGM("BGM_WORLD_" + Data.World + "_SELECT", true);
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

            if (backGroundObject == null)
                backGroundObject = new();
            currentTeamPresetNo = CacheUserData.GetInt("presetAdventureDeck", 0);
            SetPresetBtn(currentTeamPresetNo);
            SetData();
            isAdventureStart = false;
        }
        public override void ClosePopup()
        {
            //WJ - 06.24 - 해당 코드로 인해서, adventureReward(탐험보상씬)에서 이 컴포넌트를 띄우고 있는데(히스토리는 모르겟음) - 다음 스테이지를 누르면 
            //adventureBattleScene 로드하려다가 AllClosePoup 타서 여기서 체크해서 강제로 탐험 스테이지 선택씬으로 가버림. - 수정 -> adventureReward도 예외 조건 추가
            var currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (isAdventureStart == false && currentSceneName == "AdventureReward")
            {
                RequestVariationSceneCheck();
            }

            base.ClosePopup();

            blurImage.texture = null;
            blurTexture?.Release();
            blurTexture = null;
            Camera.main.targetTexture = null;
        }
        public override void OnClickDimd()
        {
            ClosePopup();
        }
        public override bool IsModeless()
        {
            return false;
        }
        public void OnClickTeamPreset(int presetNo)
        {
            currentTeamPresetNo = presetNo;
            SetPresetBtn(presetNo);
            battleLine.LoadBattleLine(presetNo);
            DrawTeamDragon();
            //SetDragonList();
            stageDragonList.Init(battleLine.GetArray());
            ChangeDragonButtonLabel(IsShowDragonList());
            RefreshDragonBattleLineLabel();
            CacheUserData.SetInt("presetAdventureDeck", currentTeamPresetNo);
        }
        void SetPresetBtn(int clickedIndex)
        {
            foreach (var btn in teamPresets)
            {
                btn.interactable = true;
            }
            teamPresets[clickedIndex].interactable = false;
        }
        public void OnEvent(DragonChangedEvent eventType)
        {
            if (IsShowDragonList())
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
            }

            DrawTeamDragon();
        }
    }
}