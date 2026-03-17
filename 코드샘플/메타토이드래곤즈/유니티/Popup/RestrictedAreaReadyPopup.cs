using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class RestrictedAreaReadyPopup : Popup<RestrictedInfoPopupData>, EventListener<DragonChangedEvent>
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

        [Space]
        [Header("ElementPowers")]
        [SerializeField] private GameObject[] myBattlePointDetail = null;

        [Space]
        [Header("DragonPosTransform")]
        [SerializeField] private GameObject[] arrDragonParent = null;

        [Space]
        [Header("BG Prefabs")]
        [SerializeField] private GameObject[] bgObject = null;
        [SerializeField] private Sprite[] bodySprite = null;
        [SerializeField] private Image[] bodyBacks = null;
        [SerializeField] private Color[] textColors = null;

        [SerializeField]
        private RawImage blurImage = null;

        protected bool IsLock { get; private set; } = false;
                
        private RenderTexture blurTexture = null;

        private RestrictedBattleLine battleLine = new RestrictedBattleLine();
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

        RestrictedAreaData curData;

        public List<int> UsingDragons { get { return Data.UsingDragons; } }

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
            
            battleLine.LoadBattleLine(Data.World, (StageDifficult)Data.Diff);

            SetDragonList();
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
            StageBaseData data = StageBaseData.GetByAdventureWorldStage(Data.World, 1);

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
            SetStageLabel(Data.World, (StageDifficult)Data.Diff);
            SetDesignByWorldMapName();
        }
        void SetDesignByWorldMapName()
        {
            int difficult = 3;
            int checkIndex = difficult - 1;
        }
        void SetStageLabel(int worldNumber, StageDifficult diff)
        {
            RestrictedAreaData curData = RestrictedAreaData.GetByWorldDiff(worldNumber, diff);
            //if (stageLabel != null)
            //{
            //    stageLabel.text = string.Format(SBFunc.StrBuilder(StringData.GetStringByStrKey("제한구역팀세팅")), worldNumber);
            //}
        }
        void DrawDragon()
        {
            battleLine.LoadBattleLine(Data.World, (StageDifficult)Data.Diff);
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
        }
        void OnClickChangeTeam(int BeforeDTag, int AfterDTag)
        {
            battleLine.ChangeDragon(BeforeDTag, AfterDTag);
            DrawTeamDragon();
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
                battleLine.LoadBattleLine(Data.World, (StageDifficult)Data.Diff);
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

            battleLine.Save();
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
        
        void RequestVariationSceneCheck()
        {
            StageManager.Instance.SetWorld(Data.World);
            LoadingManager.Instance.EffectiveSceneLoad("AdventureStageSelect", eSceneEffectType.CloudAnimation);
        }
        void SuggestionOkProcess()
        {
            if (battleLine == null)
                return;
            if(Data.Diff == 2)
                SBFunc.SetAutoBattleLineWithCap(battleLine, stageDragonList.GetAbleDragons(), Data.BP_CAP);
            else
                SBFunc.SetAutoBattleLine(battleLine, stageDragonList.GetAbleDragons());

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
                stageDragonList.transform.localScale = Vector3.one * ratio;
            }

            if (backGroundObject == null)
                backGroundObject = new();

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
        
        void SetPresetBtn(int clickedIndex)
        {
            
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

        public void Save()
        {
            battleLine.Save();

            DragonChangedEvent.Refresh();

            ClosePopup();
        }
    }
}