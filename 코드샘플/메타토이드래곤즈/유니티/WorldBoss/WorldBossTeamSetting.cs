using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossBattleLine : BattleLine
    {
        protected override int MaxDeckCount => 6;//월드 보스는 6슬롯으로 한다고함.
        protected override int HiddenCount => 0;
        protected override int XSize => 3;
        protected override int YSize => 2;
        public int GetMaxCount => MaxDeckCount;

        /// <summary> 월드보스는 사용하지 않음 </summary>
        public override bool LoadBattleLine(int index = 0)
        {
            return false;
        }
    }

    public class WorldBossTeamSetting : MonoBehaviour, EventListener<DragonChangedEvent>
    {

        private int curDeckIndex = -1;

        [Space]
        [Header("Mics")]
        [SerializeField] private Text labelMyBattlePoint = null;
        [SerializeField] private GameObject prefDragonSlot = null;
        [SerializeField] private WorldBossDragonListView dragonListUI = null;
        [SerializeField] List<Button> deckButtonList = new List<Button>();

        [Space]
        [Header("ElementPowers")]
        [SerializeField] private GameObject[] myBattlePointDetail = null;

        [Space]
        [Header("DragonPosTransform")]
        [SerializeField] private GameObject[] arrDragonParent = null;

        private WorldBossBattleLine battleLine = new WorldBossBattleLine();
        private List<CharacterSlotFrame> currentDragonDeckList = new List<CharacterSlotFrame>();
        private int currentClickDragonTag = -1;

        readonly int dragonBattleLineMaxSize = 6;

        bool isInit = false;
        bool isNetworkState = false;

        private void OnEnable()
        {
            EventManager.AddListener<DragonChangedEvent>(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<DragonChangedEvent>(this);
            WorldBossManager.Instance.UIDeckIndex = -1;
            WorldBossManager.Instance.UITeamSettingFlag = false;

            if(UIManager.Instance.MainUI != null)
                UIManager.Instance.MainUI.ReleaseTownButtonCallBack();
        }
        public void Start()
        {
            UIManager.Instance.InitUI(eUIType.WorldBoss);
            UIManager.Instance.RefreshUI(eUIType.WorldBoss);
            UIManager.Instance.MainUI.SetTownButtonCallBack(() => {

                var isEqualData = User.Instance.PrefData.WorldBossFormationData.isEqualFormationFromServerData(curDeckIndex , battleLine.GetList());//서버기준과 현재 덱 상태 체크
                if(!isEqualData)
                {
                    var isEmpty = IsTotalDeckEmptyCheck();
                    if (isEmpty)
                        EmptyDeckProcess();
                    else
                        NotEmptyDeckPopupProcess();
                }
                else
                {
                    LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground);
                }
            });

            Initialize();
        }

        void NotEmptyDeckPopupProcess()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000648),
            () => {
                OnClickSaveCurrentDragonFormation();
            },
            () => {
                LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground);
            },
            () => {
                LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground);
            });
        }

        void EmptyDeckProcess()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("보스레이드_미저장종료"),
            () => {
                LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground);
            },
            () => {
            },
            () => {
            });
        }

        void Initialize()
        {
            curDeckIndex = WorldBossManager.Instance.UIDeckIndex;

            if(!WorldBossManager.Instance.UITeamSettingFlag)//씬 입장시 최초 한번만 실행.
            {
                User.Instance.PrefData.WorldBossFormationData.InitTemporaryFormation();
                WorldBossManager.Instance.UITeamSettingFlag = true;
            }

            InitializeTeam();
            OnShowDragonList();
            RefreshDeckButton();
            RefreshDeckReddot();//빈덱 레드닷 체크
            isNetworkState = false;
        }
        
        void InitializeDragonList()
        {
            HideAllArrowAnimation();
            InitDragonListUI();
            RefreshDragonBattleLineLabel();
        }

        void InitDragonListUI()
        {
            RegistDragonListCallback();
            dragonListUI.Init();
            dragonListUI.RefreshList(battleLine.GetArray());//현재 선택한 드래곤 만이 아니라, 전체(공,방) 의 덱 상태를 알아야함.
            dragonListUI.RefreshSort();
            dragonListUI.OnShowList();
            dragonListUI.RefreshSaveButtonState(!IsTotalDeckEmptyCheck());
        }

        void InitializeTeam()
        {
            battleLine.SetLine(User.Instance.PrefData.WorldBossFormationData.GetTemporaryFormation(curDeckIndex));
            DrawDragon();
        }
        void DrawDragon()
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
            dragonListUI.RefreshList(battleLine.GetArray());
            dragonListUI.RefreshSaveButtonState(!IsTotalDeckEmptyCheck());
        }
        void RemoveAllDragonPrefab()
        {
            if (currentDragonDeckList != null)
            {
                foreach(var deck in currentDragonDeckList)
                {
                    deck.SetClear();
                }
            }
        }
        public void OnShowDragonList()
        {
            if (null != dragonListUI)
            {
                InitializeDragonList();
            }
        }

        void RegistDragonListCallback()
        {
            dragonListUI.SetRegistCallBack((param) =>
            {
                int tag = int.Parse(param);
                OnClickRegistTeam(tag);

            });

            dragonListUI.SetReleaseCallback((param) =>
            {
                int tag = int.Parse(param);
                OnClickReleaseTeam(tag);

            });
        }
        public void OnClickReleaseAllDragon()
        {
            if (battleLine != null)
            {
                //if (battleLine.isDragonDeckEmpty())
                //{
                //    ToastManager.On(StringData.GetStringByStrKey("덱슬롯없음알림"));
                //    return;
                //}

                battleLine.Clear();
                DrawDragon();
                dragonListUI.RefreshList(battleLine.GetArray());
                HideAllArrowAnimation();

                //바로 저장요청 하지 말고 임시저장소에 저장
                //OnClickSaveCurrentDragonFormation();
                var tempBattleLine = battleLine.GetList().ToList();
                User.Instance.PrefData.WorldBossFormationData.SetTemporaryFormation(curDeckIndex, tempBattleLine);
                RefreshDeckReddot();
                dragonListUI.RefreshSaveButtonState(!IsTotalDeckEmptyCheck());
            }
        }

        public void OnClickRegistTeam(int dragonTag)
        {
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
            RefreshDeckReddot();
        }

        void OnClickRegistPosition(int line, int index)
        {
            if (battleLine.GetDragon(line, index) <= 0 && currentClickDragonTag <= 0)
                return;

            battleLine.AddDragonPosition(line, index, currentClickDragonTag);
            DrawDragon();
            dragonListUI.RefreshList(battleLine.GetArray());
            dragonListUI.RefreshSaveButtonState(!IsTotalDeckEmptyCheck());
            RefreshDeckReddot();
        }

        public void OnClickReleaseTeam(int dragonTag)
        {
            if (dragonTag <= 0)
                return;

            var selectDragonDeckIndex = GetTeamIndexByTag(dragonTag);
            if(selectDragonDeckIndex >= 0 && curDeckIndex != selectDragonDeckIndex)//다른 드래곤 덱의 초상화 누르면 덱에서 빼달라고함.
            {
                //비어있는 덱은 일단 처리안함.
                //var targetDragonCount = User.Instance.PrefData.WorldBossFormationData.GetDragonCountInTemporaryDeck(selectDragonDeckIndex);
                //if(targetDragonCount <= 1)
                //{
                //    ToastManager.On("임시 - 다른 덱을 비어있는 상태로 만들수가 없습니다.");
                //    return;
                //}

                var isRemove = User.Instance.PrefData.WorldBossFormationData.RemoveTemporarySpecificDragon(selectDragonDeckIndex, dragonTag);
                if (isRemove)
                {
                    var modifyDeck = User.Instance.PrefData.WorldBossFormationData.GetTemporaryFormation(selectDragonDeckIndex).ToList();
                    User.Instance.PrefData.WorldBossFormationData.SetTemporaryFormation(selectDragonDeckIndex, modifyDeck);
                }
            }
            else//같을 경우
            {
                battleLine.DeleteDragon(dragonTag);
                DrawDragon();
            }
            
            dragonListUI.RefreshList(battleLine.GetArray());
            dragonListUI.RefreshSaveButtonState(!IsTotalDeckEmptyCheck());
            RefreshDeckReddot();
        }

        int GetTeamIndexByTag(int _dragonTag)
        {
            var curTempDeck = User.Instance.PrefData.WorldBossFormationData.TeamTemporarystorage;

            for (int i = 0; i < curTempDeck.Count; i++)
            {
                var list = curTempDeck[i];
                if (list == null || list.Count <= 0)
                    continue;

                var isContain = list.Contains(_dragonTag);
                if (isContain)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 전체 저장으로 방식 변경
        /// </summary>
        /// <param name="isTryOut"></param>
        public void OnClickSaveCurrentDragonFormation(bool isTryOut = true)
        {
            if (isNetworkState)
                return;

            isNetworkState = true;

            HideAllArrowAnimation();

            var curBattleLine = battleLine.GetList().ToList();//현재 배틀라인 상태 임시저장
            User.Instance.PrefData.WorldBossFormationData.SetTemporaryFormation(curDeckIndex, curBattleLine);

            //현재 덱 체크
            if (battleLine.IsDeckEmpty())
            {
                ToastManager.On(StringData.GetStringByIndex(100000393));
                isNetworkState = false;
                return;
            }

            var diffList = User.Instance.PrefData.WorldBossFormationData.GetEqualCheckListFromServerData(curDeckIndex, curBattleLine);//서버에서 온 데이터와 다른 것 찾기
            var isDiffListAllTrue = diffList.FindAll(element => element == true).Count == diffList.Count && diffList != null;

            for (int i = 0; i < diffList.Count; i++)
            {
                var tempDragonDeckCount = User.Instance.PrefData.WorldBossFormationData.GetDragonCountInTemporaryDeck(i);
                if (tempDragonDeckCount <= 0)//빈덱 저장 하려고 할 때
                {
                    //저장 안하고 나가기 시도 시 저장할 거냐는 팝업 뜨는데 해당 팝업이 같은 프레임 동시 호출이라 일단 토스트로 세팅
                    ToastManager.On(StringData.GetStringByIndex(100000393));
                    isNetworkState = false;
                    return;
                }
            }

            if (isDiffListAllTrue)//변경점이 없다.
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000624),
                () => {
                        SaveCompleteAndRequest();
                    },
                    null, () =>
                    {
                        SaveCompleteAndRequest();
                    }
                );
                return;
            }

            var getTeamParam = User.Instance.PrefData.WorldBossFormationData.GetWorldBossTemporaryFormationSpecificDeck(diffList);
            WWWForm param = new WWWForm();
            param.AddField("teams", JsonConvert.SerializeObject(getTeamParam));
            NetworkManager.Send("raid/setteams", param, (JObject jsonData) =>
            {
                isNetworkState = false;
                if (jsonData["err"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
                {
                    User.Instance.PrefData.WorldBossFormationData.SaveToTeamFormation();//임시 저장 리스트 서버 데이터에 덧씌움.

                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000624), () => 
                        {
                            SaveCompleteAndRequest();
                        },
                        null, () => 
                        {
                            SaveCompleteAndRequest();
                        }
                    );
                }
                else if(jsonData["err"] != null)
                {
                    var errorValue = (eApiResCode)((int)jsonData["err"]);
                    switch(errorValue)
                    {
                        case eApiResCode.DATA_ERROR:
                            ToastManager.On(StringData.GetStringByStrKey("서버요청실패"));
                            break;
                    }
                    isNetworkState = false;
                }
            }, (string arg) =>
            {
                isNetworkState = false;
            });
        }

        void SaveCompleteAndRequest()
        {
            SaveComplete();

            LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground);
        }

        void SaveComplete()
        {
            if (dragonListUI != null)
            {
                HideAllArrowAnimation();
            }
        }

        void InitClickFullDragonTag()
        {
            currentClickDragonTag = -1;
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
            if (dragonListUI != null)
            {
                dragonListUI.RefreshDragonCountLabel(battleLine.DeckCount, dragonBattleLineMaxSize);
            }
        }

        void SuggestionOkProcess()
        {
            if (null == battleLine)
                return;

            var autoLogic = new WorldBossAutoDeckLogic();

            battleLine.Clear();
            var resultList = autoLogic.GetAutoMerge(curDeckIndex);
            if(resultList.Count < battleLine.GetMaxCount)
            {
                var remainCount = battleLine.GetMaxCount - resultList.Count;
                for (int i = 0; i < remainCount; i++)
                    resultList.Add(0);
            }
            battleLine.SetLine(resultList);

            DrawDragon();

            if (null != dragonListUI)
            {
                dragonListUI.InitSuggest(battleLine.GetArray());
                dragonListUI.RefreshList(battleLine.GetArray());
            }

            HideAllArrowAnimation();

            //바로 저장요청 하지 말고 임시저장소에 저장
            //OnClickSaveCurrentDragonFormation();
            var tempBattleLine = battleLine.GetList().ToList();
            User.Instance.PrefData.WorldBossFormationData.SetTemporaryFormation(curDeckIndex, tempBattleLine);
            dragonListUI.RefreshSaveButtonState(!IsTotalDeckEmptyCheck());
            RefreshDeckReddot();
        }
        public void OnClickSuggestion()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey(("battle_popup_team_recommend")), StringData.GetStringByIndex(100001251),
                () => { SuggestionOkProcess(); }, () => { }, () => { });
        }

        public void OnClickOtherDeck(int _slotIndex)
        {
            if(curDeckIndex != _slotIndex)
            {
                WorldBossManager.Instance.UIDeckIndex = _slotIndex;

                Initialize();
            }
        }

        void RefreshDeckButton()
        {
            if (deckButtonList == null || deckButtonList.Count <= 0)
                return;

            for(int i = 0; i< deckButtonList.Count; i++)
            {
                var button = deckButtonList[i];
                if (button == null)
                    continue;

                var isSelected = i == curDeckIndex;

                //var childIcon = SBFunc.GetChildrensByName(button.transform, new string[] {"icon"});
                //if (childIcon == null)
                //    continue;

                //childIcon.gameObject.SetActive(isSelected);

                button.SetButtonSpriteState(isSelected);
            }
        }

        //현재 temporary 기준으로 레드닷 체크를 하자.
        void RefreshDeckReddot()
        {
            if (deckButtonList == null || deckButtonList.Count <= 0)
                return;

            for (int i = 0; i < deckButtonList.Count; i++)
            {
                var button = deckButtonList[i];
                if (button == null)
                    continue;

                var childIcon = SBFunc.GetChildrensByName(button.transform, new string[] { "ReddotObject" });
                if (childIcon == null)
                    continue;

                var dragonCount = User.Instance.PrefData.WorldBossFormationData.GetDragonCountInTemporaryDeck(i);//각 임시 덱에서 드래곤 카운트.
                var isEmptyDeck = dragonCount <= 0;

                if(curDeckIndex == i)
                    isEmptyDeck = battleLine.IsDeckEmpty();

                childIcon.gameObject.SetActive(isEmptyDeck);
            }
        }

        public void OnClickDeckButton(int _slotIndex)
        {
            if (curDeckIndex == _slotIndex)
                return;

            WorldBossManager.Instance.UIDeckIndex = _slotIndex;

            //다른 덱 선택 전에 현재 세팅된 battleLine 임시 저장
            var tempBattleLine = battleLine.GetList().ToList();
            User.Instance.PrefData.WorldBossFormationData.SetTemporaryFormation(curDeckIndex, tempBattleLine);

            Initialize();
        }

        bool IsTotalDeckEmptyCheck()//true 면 빈덱이 존재함.
        {
            var otherDeckCondition = User.Instance.PrefData.WorldBossFormationData.IsTotalDeckEmptyCheck(curDeckIndex, false);
            var curbattleLineCondition = battleLine.IsDeckEmpty();
            return otherDeckCondition || curbattleLineCondition;
        }
        
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
    }
}

