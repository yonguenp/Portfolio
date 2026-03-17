using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace SandboxNetwork {

    public class ArenaBattleLine : BattleLine
    {
        public ArenaBattleLine() : base() { }
        protected override int MaxDeckCount => 5;
        protected override int HiddenCount => 1;
        protected override int XSize => 3;
        protected override int YSize => 2;

        public bool isAtkSeetingMode = false;
        public List<int> hiddenDragonList = new List<int>();
        public override bool LoadBattleLine(int index=0)
        {
            var formationData = User.Instance.PrefData.ArenaFormationData;
            if (formationData == null) return false;

            Clear();
            List<int> tempDragonTaglist;
            if (isAtkSeetingMode)
                tempDragonTaglist = formationData.TeamFormationATK[index];
            else
            {
                ClearHiddenDragonList();
                hiddenDragonList = formationData.TeamFormationHidden;
                if (index == -1)
                    tempDragonTaglist = formationData.TeamFormationDEF[0];
                else
                    tempDragonTaglist = formationData.TeamFormationATK[index];
            }

            return SetLine(tempDragonTaglist);
        }
        public void ClearHiddenDragonList()
        {
            hiddenDragonList.Clear();
        }
        public bool AddHiddenDragonList(int tag)
        {
            bool AddCheck = isFullHiddenList();
            if (AddCheck)
            {
                return false;
            }
            else
            {
                int indexCheck = hiddenDragonList.IndexOf(tag);
                if (indexCheck >= 0) return true;
                hiddenDragonList.Add(tag);
                return true;
            }
        }
        public bool DeleteHiddenDragonList(int tag)
        {
            int indexCheck = hiddenDragonList.IndexOf(tag);
            if (indexCheck < 0) return false;
            hiddenDragonList.Remove(tag);
            return true;
        }
        public bool isFullHiddenList()
        {
            if (hiddenDragonList.Count >= HiddenCount)
            {
                return true;
            }
            return false;
        }

    }
    public class ArenaTeamSetting : MonoBehaviour, EventListener<DragonChangedEvent>
    {
        [Header("default Info")]
        [SerializeField]
        private Text labelMyBattlePoint = null;

        [SerializeField] 
        private GameObject[] myBattlePointDetail = null;

        [SerializeField]
        private Text teamSettingButtonLable = null;
        [SerializeField]
        private GameObject prefDragonSlot = null;
        [SerializeField]
        private GameObject[] arrDragonParent = null;
        [SerializeField]
        private BattleDragonListView stageDragonList = null;
        [SerializeField]
        private Text battleLineLabel = null;
        [SerializeField]
        private Text teamSettingModeLabel = null;

        [Header("Hidden Info")]
        [SerializeField]
        private GameObject hiddenDragonLabelNode = null;
        [SerializeField]
        private GameObject hiddenBtnObj = null;
        [SerializeField]
        private Image hiddenModeBtnImg = null;
        [SerializeField]
        private Sprite[] hiddenModeBtnSprites = null;


        [Header("ETC")]
        [SerializeField]
        private Button[] teamPresets = null;
        [SerializeField]
        private ElemBuffInfoUI elemBuffUI = null;


        bool isHiddenMode= false;

        private ArenaBattleLine battleLine = new ArenaBattleLine();
        private List<CharacterSlotFrame> currentDragonDeckList = new List<CharacterSlotFrame>();
        private int currentClickDragonTag = -1;
        private bool isAtkSettingMode = false;

        private int tempOtherIndex = -1;
        private bool tempMatchListFlag = false;

        private int tempCurrentTotalBP = -1;
        private const int dragonBattleLineMaxSize = 5;
        private const int HiddenDragonCount = 1;

        private bool isNetworkState = false;

        private int currentTeamPresetNo = 0;

        private void OnEnable()
        {
            EventManager.AddListener<DragonChangedEvent>(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<DragonChangedEvent>(this);
        }
        private void Start()
        {
            UIManager.Instance.InitUI(eUIType.Arena);
            UIManager.Instance.RefreshUI(eUIType.Arena);
            UIManager.Instance.MainUI.SetTownButtonCallBack(()=>{

                if(ArenaManager.Instance.IsFriendFightDataFlag)
                {
                    LoadingManager.Instance.EffectiveSceneLoad("ArenaBattleReady", eSceneEffectType.CloudAnimation);
                    return;
                }


                LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Arena));
            });

            InitTeamPreset();
            SetData();
            isNetworkState = false;
            TutorialCheck();
        }

        void TutorialCheck()
        {
            if (TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Arena))
            {
                TutorialManager.tutorialManagement.NextTutorialStart();
            }
        }


        void InitTeamPreset()
        {
            if (ArenaManager.Instance.IsAtk)
            {
                currentTeamPresetNo = CacheUserData.GetInt("presetArenaAtkDeck", 0);
            }
            else
            {
                currentTeamPresetNo = -1; // 방어덱용
            }
            SetPresetBtn(currentTeamPresetNo);

        }

        void SetData()
        {
            InitCurrentTotalBP();
            SetAtkModeFlag();
            SetHiddenDragonLabelNode();
            SetAtkModeLabel();
            DrawDragon();
            SetDragonList();
        }
        void SetDragonList()
        {
            stageDragonList.OnShowList();
            RegistDragonListCallback();
            stageDragonList.Init(battleLine.GetArray());
            ChangeDragonButtonLabel(true);
            RefreshDragonBattleLineLabel();
            SetSeasonBuffState();
        }
        void InitCurrentTotalBP()
        {
            tempCurrentTotalBP = 0;
        }
        void RegistHiddenDragon(string dragonTag)
        {
            int tag = int.Parse(dragonTag);
            bool isSuccess = battleLine.AddHiddenDragonList(tag);
            if (isSuccess)
            {
                RefreshHiddenDragonUI();
            }
        }


        void RefreshDragonBattleLineLabel()
        {
            if(battleLineLabel != null)
            {
                int currentCount = battleLine.DeckCount;
                battleLineLabel.text = string.Format("{0}/{1}",currentCount, dragonBattleLineMaxSize);
            }
        }

        void SetSeasonBuffState()
        {
            int maxCount = battleLine.MaxCount;
            eElementType[] elemArr = new eElementType[maxCount];
            for(int i = 0; i < maxCount; i++)
            {
                var dragon = battleLine.GetDragon(i);
                if (dragon != 0)
                {
                    var data = CharBaseData.Get(dragon);
                    if (data != null)
                    {
                        elemArr[i] = data.ELEMENT_TYPE;
                        continue;
                    }
                }

                elemArr[i] = eElementType.None;
            }
            elemBuffUI.SetEffect(elemArr);
        }

        void RefreshHiddenDragonUI()
        {
            if(currentDragonDeckList == null|| currentDragonDeckList.Count <= 0)
            {
                return;
            }
            List<int> hiddenDragonList = battleLine.hiddenDragonList;
            
            bool isFull = hiddenDragonList.Count >= HiddenDragonCount;
            int registIndex = 0;


            for (int i = 0;  i < currentDragonDeckList.Count; ++i){
                CharacterSlotFrame slotInfo = currentDragonDeckList[i];
                if(slotInfo == null)
                {
                    continue;
                }
                int currentDragonTag = slotInfo.DragonTag;
                if (currentDragonTag <= 0) continue;
                bool isRegist = hiddenDragonList.Contains(currentDragonTag);
                if (isRegist)
                {
                    ++registIndex;
                }
                slotInfo.SetBtnNodeState(false);
                slotInfo.ShowHiddenDragonUI(isRegist, isFull, registIndex, HiddenDragonCount);
                slotInfo.SetHiddenDragonButtonCallback(RegistHiddenDragon, ReleaseHiddenDragon);
            }
        }

        void ReleaseHiddenDragon(string dragonTag)
        {
            int tag = int.Parse(dragonTag);
            bool isSuccess = battleLine.DeleteHiddenDragonList(tag);
            if (isSuccess)
            {
                RefreshHiddenDragonUI();
            }
        }
        void SetAtkModeFlag()
        {
            battleLine.isAtkSeetingMode = isAtkSettingMode = ArenaManager.Instance.IsAtk;
            hiddenBtnObj.SetActive(!isAtkSettingMode);
            tempOtherIndex = ArenaManager.Instance.OtherTeamIndex;
            tempMatchListFlag = ArenaManager.Instance.IsMatchList;
        }
        void SetAtkModeLabel()
        {
            if(teamSettingModeLabel != null)
            {
                teamSettingModeLabel.text = (isAtkSettingMode) ? StringData.GetStringByIndex(100001148) : StringData.GetStringByIndex(100001150);
            }

        }
        void SetHiddenDragonLabelNode()
        {
            if(hiddenDragonLabelNode != null)
            {
                hiddenDragonLabelNode.SetActive(!isAtkSettingMode);
                
            }
        }
        void DrawDragon()
        {
            battleLine.LoadBattleLine(currentTeamPresetNo);
          //  
            DrawTeamDragon();
        }
        void DrawTeamDragon()
        {
            int myTotalBp = 0;
            RemoveAllDragonPrefab();
            if(currentDragonDeckList == null)
                currentDragonDeckList = new List<CharacterSlotFrame>();
            int i = 0, l = 0, lineLimit = 2;
            int[] points = new int[(int)eElementType.MAX] { 0, 0, 0, 0, 0, 0, 0 };
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
                    var dragonSlot = Instantiate(prefDragonSlot, arrDragonParent[l * 2 + i].transform);
                    dragonSlot.transform.localPosition = Vector3.zero;
                    dragonSlot.SetActive(true);
                    dragonSlot.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

                    characterSlotComp = dragonSlot.GetComponent<CharacterSlotFrame>();
                }
                
                if (element != null)
                {
                    myTotalBp += (int)element.Status.GetTotalINF();
                    if (points.Length > element.BaseData.ELEMENT)
                        points[element.BaseData.ELEMENT] += element.Status.GetTotalINF();

                    if (characterSlotComp != null)
                    {
                        characterSlotComp.SetDragonData(element.Tag, true, false, battleLine);
                        characterSlotComp.setCallback((param) =>
                        {
                            if (currentClickDragonTag < 0) return;
                            if (currentClickDragonTag > 0 && int.Parse(param) > 0)  //요 파트가 코코스에선 빠져 있어서 나중에 코코스 재작업한다면 추가 필요
                            {
                                OnclickChangeTeam(int.Parse(param), currentClickDragonTag);
                                HideAllArrowAnimation();
                                return;
                            }
                            bool isHideArrow = characterSlotComp.isHideArrow();
                            if (!isHideArrow) return;
                            bool isDeckFull = battleLine.IsDeckFull();
                            if (isDeckFull) { 
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
                if (currentDragonDeckList.Count <= i + (l * 2))
                {
                    currentDragonDeckList.Add(characterSlotComp);
                }
                else
                {
                    currentDragonDeckList[i + (l * 2)] = characterSlotComp;
                };
                ++i;
                if (i >= lineLimit)
                {
                    i = 0;
                    //arrDragonParent[l].GetComponent<VerticalLayoutGroup>().enabled = false;
                    
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
                    RectTransform rt = (point.transform as RectTransform);
                    rt.sizeDelta = new Vector2(100f * (5.0f / elemCount), rt.sizeDelta.y);
                }
            }

            labelMyBattlePoint.text = myTotalBp.ToString();
            tempCurrentTotalBP = myTotalBp;

            if (isAtkSettingMode==false)
            {
                for (i = 0; i < currentDragonDeckList.Count; ++i)
                {
                    CharacterSlotFrame slotInfo = currentDragonDeckList[i];
                    int currentDragonTag = slotInfo.DragonTag;
                    if (currentDragonTag <= 0 && slotInfo == null)
                        continue;
                    bool isRegist = battleLine.hiddenDragonList.Contains(currentDragonTag);
                    if (isRegist)
                    {
                        slotInfo.ShowOnlyHideBubble();
                    }
                    else
                    {
                        slotInfo.HideHiddenDragonUI();
                    }
                }
            }

            
            RefreshDragonBattleLineLabel();
            SetSeasonBuffState();

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
        public void onShowDragonList()
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
                if(stageDragonList != null)
                {
                    stageDragonList.OnShowList();
                    RegistDragonListCallback();
                    stageDragonList.Init(battleLine.GetArray());
                    ChangeDragonButtonLabel(true);
                    RefreshDragonBattleLineLabel();
                }
            }
            if (isHiddenMode)
            {
                OnClickHiddenBtn();
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
            if(isEmpty)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393));
                return;
            }
            if (isAtkSettingMode)
            {
                AtkModeEqualCheck();
            }
            else
            {
                DefModeEqualCheck();
            }
        }
        
        void AtkModeEqualCheck()
        {
            bool isEqual = IsChangeDragonList();
            if (isEqual)
            {
                if(stageDragonList != null)
                {
                    ChangeDragonButtonLabel(false);
                    HideAllArrowAnimation();
                    RequestVariationSceneCheck();
                }
            }
            else
            {
                DragonOverWriteDataPopup();
            }
        }
        void DefModeEqualCheck()
        {
            bool isHiddenEqual = ArrayEqual(battleLine.hiddenDragonList,User.Instance.PrefData.ArenaFormationData.TeamFormationHidden);
            bool isEqual = IsChangeDragonList();
            if (isEqual&& isHiddenEqual)
            {
                if (stageDragonList != null)
                {
                    ChangeDragonButtonLabel(false);
                    HideAllArrowAnimation();
                    RequestVariationSceneCheck();
                }
            }
            else
            {
                DragonOverWriteDataPopup();
            }
        }

        void DragonOverWriteDataPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000648),
            () =>
            {
                onClickSaveCurrentDragonFormation();
                if (stageDragonList != null)
                {
                    HideAllArrowAnimation();
                }
            },
            () =>
            {
                if (ArenaManager.Instance.IsAtk) { 
                    currentTeamPresetNo =  CacheUserData.GetInt("presetArenaAtkDeck", 0);
                    SetPresetBtn(currentTeamPresetNo);
                    battleLine.LoadBattleLine(currentTeamPresetNo);
                }
                else
                {
                    SetPresetBtn(-1);
                    battleLine.LoadBattleLine(-1);
                }
                DrawTeamDragon();
                if(stageDragonList != null)
                {
                    HideAllArrowAnimation();
                    RequestVariationSceneCheck();
                }
            }, null, true, true, true);
        }
        public void onClickSaveCurrentDragonFormation()
        {
            HideAllArrowAnimation();
            bool isEmpty = battleLine.IsDeckEmpty();
            if (isEmpty)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393), true, false, true);
                return;
            }

            if (isAtkSettingMode)
            {
                SendAtkFormation();
            }
            else
            {
                SendDefFormation();
            }
        }
        void SendAtkFormation()
        {
            WWWForm param = new WWWForm();

            List<int> dragons = battleLine.GetList();

            param.AddField("teamno", currentTeamPresetNo); // 몇번째 팀인지 설정해줘야 됨 현재 임시
            param.AddField("dragons", SBFunc.ListToString(dragons));
            param.AddField("items", "[0,0,0]");
            param.AddField("teamname", "arena");
            if(isNetworkState) { return; }
            isNetworkState = true;
            NetworkManager.Send("preference/setteam", param, (JObject jsonData) =>
            {
                isNetworkState = false;
                if (jsonData["err"]!=null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                {
                    User.Instance.PrefData.ArenaFormationData.ClearATKFomationData(currentTeamPresetNo);
                    User.Instance.PrefData.ArenaFormationData.SetATKFormationData(currentTeamPresetNo, dragons);
                    CacheUserData.SetInt("presetArenaAtkDeck", currentTeamPresetNo);

                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000624), 
                        () => 
                        { 
                            SaveComplete(); 
                        },
                        null, 
                        () => 
                        { 
                            SaveComplete(); 
                        });

                }
                else
                {
                    Debug.Log("unsuccess response" + jsonData["rs"].ToObject<string>());
                }
            },
            (string arg) =>
            {
                isNetworkState = false;
            }
            );
        }
        void SendDefFormation()
        {
            if (battleLine.hiddenDragonList.Count> HiddenDragonCount) 
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002655), true, false, true);
                SetData();
                return;

            }
            WWWForm param = new WWWForm();
            List<int> dragons = battleLine.GetList();
            param.AddField("teamno", 0); // 몇번째 팀인지 설정해줘야 됨 현재 임시
            param.AddField("deck", SBFunc.ListToString(dragons));
            param.AddField("items", "[0,0,0]");
            if(battleLine.hiddenDragonList.Count> HiddenDragonCount)
            {
                battleLine.hiddenDragonList = battleLine.hiddenDragonList.GetRange(0, 1);
            }
            param.AddField("hidden",SBFunc.ListToString(battleLine.hiddenDragonList));


            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("arena/defencedeck", param, (JObject jsonData) =>
            {
                isNetworkState=false;
                if (jsonData["err"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                {
                    User.Instance.PrefData.ArenaFormationData.ClearDEFFormationData(0);
                    User.Instance.PrefData.ArenaFormationData.SetDEFFormationData(0, dragons);
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000624),
                    () => { 
                        SaveComplete(); 
                    }, 
                    null, 
                    () => { 
                        SaveComplete(); 
                    });
                    if (TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Arena))
                    {
                        TutorialManager.tutorialManagement.NextTutorialStart();
                    }
                        
                }
                else
                {
                    Debug.Log("unsuccess response" + jsonData["rs"].ToObject<string>());
                }
            }
            , (string arg) =>
            {
                isNetworkState = false;
            });
        }
        bool IsChangeDragonList()
        {
            var formationData = User.Instance.PrefData.ArenaFormationData;

            if (formationData == null) return false;

            List<int> teamArr = null;
            if (isAtkSettingMode)
            {
                teamArr = formationData.TeamFormationATK[currentTeamPresetNo];
            }
            else
            {
                teamArr = formationData.TeamFormationDEF[0];
            }
            var dragonArr = teamArr;

            if(dragonArr ==null || dragonArr.Count <= 0)
            {
                return false;
            }

            var currentDragonArr = battleLine.GetList();
            var PrevSortDragonArr = dragonArr.ToList();
            return ArrayEqual(PrevSortDragonArr, currentDragonArr);
        }

        bool ArrayEqual(List<int> list1, List<int> list2)
        {
            var areListsEqual = true;;

            for (var i = 0; i < list1.Count; i++)
            {
                if (list2[i] != list1[i])
                {
                    areListsEqual = false;
                }
            }
            return areListsEqual;
        }

        void OnClickReleaseAllDragon()
        {
            if(battleLine != null)
            {
                if (battleLine.DeckCount == 0) return;
            }
            battleLine.Clear();
            battleLine.ClearHiddenDragonList();
            DrawTeamDragon();
            stageDragonList.RefreshList(battleLine.GetArray());
            HideAllArrowAnimation();
        }
        bool IsShowDragonList()
        {
            if (stageDragonList != null) return stageDragonList.IsShowList();

            return false;
        }

        void ChangeDragonButtonLabel(bool isOpen)
        {
            if(teamSettingButtonLable != null)
            {
                if (isOpen)
                {
                    teamSettingButtonLable.text = StringData.GetStringByIndex(100000394);
                }
                else
                {
                    teamSettingButtonLable.text = StringData.GetStringByIndex(100000221);
                }
            }
        }
        void OnClickRegistPosition(int line, int index)
        {
            if (battleLine.GetDragon(line, index) <= 0 && currentClickDragonTag <= 0)
            {
                return;
            }
            battleLine.AddDragonPosition(line, index, currentClickDragonTag);
            DrawTeamDragon();
            stageDragonList.RefreshList(battleLine.GetArray());
            HideAllArrowAnimation();
        }
        void OnclickChangeTeam(int BeforeDTag, int AfterDTag)
        {
            battleLine.ChangeDragon(BeforeDTag,AfterDTag);
            battleLine.DeleteHiddenDragonList(BeforeDTag);
            DrawTeamDragon();
            HideAllArrowAnimation();
            stageDragonList.RefreshList(battleLine.GetArray());
        }
        void OnClickReleaseTeam(int dragonTag)
        {
            bool isVisible = IsShowDragonList();
            if (!isVisible) return;
            battleLine.DeleteDragon(dragonTag);
            battleLine.DeleteHiddenDragonList(dragonTag);
            DrawTeamDragon();
            stageDragonList.RefreshList(battleLine.GetArray());
            if (isHiddenMode)
            {
                OnClickHiddenBtn();
            }
            HideAllArrowAnimation();
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

        void OnClickRegistTeam(int dragonTag)
        {
            bool isVisible = IsShowDragonList();
            if (!isVisible) return;
                

            if (currentClickDragonTag > 0)
            {
                HideAllArrowAnimation();
            }
            currentClickDragonTag = dragonTag;
            var line = battleLine.GetList();
            if (battleLine.IsDeckFull())
            {
                for(int i=0; i < currentDragonDeckList.Count; ++i)
                {
                    if (line[i] > 0)
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

            if (isHiddenMode)
            {
                OnClickHiddenBtn();
            }
        }
        void HideAllArrowAnimation()
        {
            InitClickFullDragonTag();
            if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0) return;

            foreach(CharacterSlotFrame elem in currentDragonDeckList)
            {
                if (elem == null) return;
                elem.HideAnimArrowNode();
            }
        }
        void InitClickFullDragonTag()
        {
            currentClickDragonTag = -1;
        }
        void SaveComplete()
        {
            if (IsShowDragonList())
            {
                if(stageDragonList != null)
                {
                    HideAllArrowAnimation();
                }
            }
            PopupManager.ClosePopup<SystemPopup>();
            RequestVariationSceneCheck();
        }
        //void onClickFullDragonDeckProcess(int currentClickTag)
        //{
        //    currentClickDragonTag = currentClickTag;
        //    ShowAllArrowAnimation();
        //}
        void ShowAllArrowAnimation()
        {
            if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0) return;

            foreach(CharacterSlotFrame element in currentDragonDeckList)
            {
                if (element == null) return;
                element.ShowAnimArrowNode();
            }
        }
        public void OnClickSuggestion()
        {
            if (battleLine == null) return;
            
            SBFunc.SetAutoBattleLine(battleLine);
            DrawTeamDragon();
            if(stageDragonList != null)
            {
                stageDragonList.RefreshList(battleLine.GetArray());
            }

            HideAllArrowAnimation();            
        }

        void RequestVariationSceneCheck()
        {
            if (ArenaManager.Instance.IsFriendFightDataFlag)
            {
                LoadingManager.Instance.EffectiveSceneLoad("ArenaBattleReady", eSceneEffectType.CloudAnimation);
                return;
            }


            if (isAtkSettingMode && tempOtherIndex >= 0)
            {
                ArenaManager.Instance.SetArenaVersusTeamData(tempOtherIndex, tempMatchListFlag);

                //LoadingManager.ImmediatelySceneLoad("ArenaBattleReady");//임시
                LoadingManager.Instance.EffectiveSceneLoad("ArenaBattleReady", eSceneEffectType.CloudAnimation);

            }
            else
            {
                 //LoadingManager.ImmediatelySceneLoad("ArenaLobby");//임시
                LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation);
            }
        }

        public void OnClickHiddenBtn()
        {
            if (isAtkSettingMode)
            {
                return;
            }
            if (isHiddenMode)
            {
                hiddenModeBtnImg.sprite = hiddenModeBtnSprites[0];
                List<int> hiddenDragonList = battleLine.hiddenDragonList;
                for (int i = 0; i < currentDragonDeckList.Count; ++i)
                {
                    CharacterSlotFrame slotInfo = currentDragonDeckList[i];
                    int currentDragonTag = slotInfo.DragonTag;
                    if (currentDragonTag <= 0) continue;
                    bool isRegist = hiddenDragonList.Contains(currentDragonTag);
                    if (isRegist)
                    {
                        slotInfo.ShowOnlyHideBubble();
                    }
                    else
                    {
                        slotInfo.HideHiddenDragonUI();
                    }
                    slotInfo.SetBtnNodeState(true);
                }
                
                isHiddenMode = false;

            }
            else
            {
                if(currentClickDragonTag> 0)
                {
                    HideAllArrowAnimation();
                    currentClickDragonTag = 0;
                }
                hiddenModeBtnImg.sprite = hiddenModeBtnSprites[1];
                RefreshHiddenDragonUI();
                
                isHiddenMode = true;
            }
        }

        private void OnDestroy()
        {
            UIManager.Instance.MainUI.ReleaseTownButtonCallBack();
        }

        public void OnEvent(DragonChangedEvent eventType)
        {
            if(eventType.type == DragonChangedEvent.TYPE.MOVE_START)
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

            DrawTeamDragon();
        }

        public void OnClickTeamPreset(int presetNo)
        {
            currentTeamPresetNo = presetNo;
            SetPresetBtn(presetNo);
            battleLine.LoadBattleLine(presetNo);
            DrawTeamDragon();
            SetDragonList();
        }

        void SetPresetBtn(int clickedIndex)
        {
            foreach(var btn in teamPresets)
            {
                btn.interactable = true;
            }
            if (clickedIndex < 0)
                return;
            teamPresets[clickedIndex].interactable = false;
        }
    }
}
