using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

namespace SandboxNetwork
{

    public class ChampionBattleLine : BattleLine
    {
        public ChampionBattleLine(ParticipantData.eTournamentTeamType type) : base() {
            Teamtype = type;
        }

        public ChampionBattleLine(ChampionBattleLine baseLine) : base()
        {
            Teamtype = baseLine.Teamtype;
            int i = 0;
            foreach(var dragon in Dragons)
            {
                SetDragon(i++, dragon);
            }
        }

        public ChampionBattleLine(ParticipantData.eTournamentTeamType type, JArray deck) : base() {
            Teamtype = type;
            for (int i = 0; i < deck.Count; i++)
            {
                SetDragon(i, deck[i].Value<int>());
            }
        }
        protected override int MaxDeckCount => 5;
        protected override int HiddenCount => 1;
        protected override int XSize => 3;
        protected override int YSize => 2;

        public ParticipantData.eTournamentTeamType Teamtype { get; private set; } = ParticipantData.eTournamentTeamType.NONE;
        
        public override bool LoadBattleLine(int index = 0)
        {
            var tempDragonTaglist = ChampionManager.Instance.MyInfo.GetChampionBattleFomation(Teamtype);
            if (tempDragonTaglist == null) return false;

            return SetLine(tempDragonTaglist.Dragons.ToList());
            //return true;
        }
        public bool IsContainDragon(ChampionDragon dragon)
        {
            return IsContainDragon(dragon.Tag);
        }

        public void Save(NetworkManager.SuccessCallback cb = null)
        {
            ChampionManager.Instance.CurChampionInfo.MyInfo.SaveChampionBattleFormation(Teamtype, this, cb);
        }

        public int GetTotalINF()
        {
            int ret = 0;

            for (int i = 0, count = Dragons.Length; i < count; i++)
            {
                var id = Dragons[i];
                if (id < 1)
                    continue;

                var dragon = ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(id);
                if (dragon == null)
                    continue;

                dragon.RefreshALLStatus();
                ret += dragon.GetTotalINF();
            }

            return ret;
        }
    }
    public class ChampionBattleSetting : MonoBehaviour, EventListener<DragonChangedEvent>
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
        private ChampionBattleDragonListView stageDragonList = null;
        [SerializeField]
        private Text battleLineLabel = null;
        [SerializeField]
        private Text teamSettingModeLabel = null;
        [Serializable]
        public class TeamSelectPanel
        {
            [SerializeField]
            public ParticipantData.eTournamentTeamType type;
            [SerializeField]
            ChampionBattleSetting parent;
            [SerializeField]
            Button button;
            [SerializeField]
            Image panel;
            [SerializeField]
            Text desc;
            [SerializeField]
            TimeObject timer;
            
            public enum STATE { 
                NONE,
                SELECTED,
                ENABLE,
                DISABLE,
            }
            public STATE curState { get; private set; } = STATE.NONE;
            public bool timeOver { get; private set; } = true;
            public int GetTime(ParticipantData.eTournamentTeamType t)
            {
                switch (t)
                {
                    case ParticipantData.eTournamentTeamType.DEFFENCE:
                        return ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.DEF_TEAM_SET);
                    case ParticipantData.eTournamentTeamType.ATTACK:
                        return ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.ATK_TEAM_SET);
                    case ParticipantData.eTournamentTeamType.HIDDEN:
                        return ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.HIDDEN_TEAM_SET);

                    default:
                        return -1;
                }
            }

            public void Refresh(ParticipantData.eTournamentTeamType t)
            {
                if (type == t)//selected
                {
                    if (curState != STATE.SELECTED)
                    {
                        curState = STATE.SELECTED;
                        panel.sprite = parent.SelectedTeamBtnSprite;

                        var time = TimeManager.GetTimeCompare(GetTime(type));
                        if (time > 0)
                        {
                            timeOver = false;
                            desc.color = parent.EnableColor;
                            timer.Refresh = () =>
                            {
                                desc.text = TimeManager.GetTimeCompareString(GetTime(type));
                            };
                        }
                        else
                        {
                            timeOver = true;
                            timer.Refresh = null;
                            desc.text = StringData.GetStringByStrKey("등록마감");
                            desc.color = parent.DisableColor;
                        }
                    }
                }
                else
                {
                    var time = TimeManager.GetTimeCompare(GetTime(type));
                    if (time > 0)
                    {
                        timeOver = false;
                        if (curState != STATE.ENABLE)
                        {
                            curState = STATE.ENABLE;
                            panel.sprite = parent.EnableTeamBtnSprite;

                            desc.color = parent.EnableColor;
                            timer.Refresh = () =>
                            {
                                desc.text = TimeManager.GetTimeCompareString(GetTime(type));
                            };
                        }
                    }
                    else
                    {
                        timeOver = true;
                        if (curState != STATE.DISABLE)
                        {
                            curState = STATE.DISABLE;
                            panel.sprite = parent.DisableTeamBtnSprite;

                            timer.Refresh = null;
                            desc.text = StringData.GetStringByStrKey("등록마감");
                            desc.color = parent.DisableColor;
                        }
                    }
                }
            }
        }

        [Header("TEAM SELECT")]
        [SerializeField]
        public TeamSelectPanel[] TeamSelectButton;
        [SerializeField]
        public Sprite SelectedTeamBtnSprite;
        [SerializeField]
        public Sprite DisableTeamBtnSprite;
        [SerializeField]
        public Sprite EnableTeamBtnSprite;
        [SerializeField]
        public Color EnableColor;
        [SerializeField]
        public Color DisableColor;

        [Header("LAYERS")]
        [SerializeField]
        GameObject SettingLayer;
        [SerializeField]
        GameObject PracticeModeLayer;

        bool isHiddenMode = false;

        public ChampionBattleLine battleLine { get; private set; } = null;
        private List<ChampionBattleCharacterSlotFrame> currentDragonDeckList = new List<ChampionBattleCharacterSlotFrame>();
        public int CurSelectedDragonTag { get; private set; } = -1;

        public ParticipantData.eTournamentTeamType CurTeamType { get; private set; } = ParticipantData.eTournamentTeamType.NONE;

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

        public void OnExit()
        {
            if (SettingLayer.activeInHierarchy)
            {
                LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation);
            }
            else
            {
                Invoke("OnBattleSetting", 0.1f);
            }
        }
        private void Start()
        {            
            UIManager.Instance.InitUI(eUIType.ChampionBattle);
            UIManager.Instance.RefreshUI(eUIType.ChampionBattle);
            UIManager.Instance.MainUI.SetTownButtonCallBack(OnExit);

            ParticipantData.eTournamentTeamType teamType = ParticipantData.eTournamentTeamType.DEFFENCE;
            
            foreach(var btn in TeamSelectButton)
            {
                btn.Refresh(CurTeamType);
            }
                        
            if (TeamSelectButton[0].timeOver)
            {
                teamType = ParticipantData.eTournamentTeamType.ATTACK;
                if (TeamSelectButton[1].timeOver)
                {
                    teamType = ParticipantData.eTournamentTeamType.HIDDEN;
                    if (TeamSelectButton[2].timeOver)
                        teamType = ParticipantData.eTournamentTeamType.DEFFENCE;
                }
            }

            SetTeamMode(teamType);
            isNetworkState = false;

            if (ChampionManager.Instance.PracticeBattleData != null)
            {
                SettingLayer.SetActive(false);
                PracticeModeLayer.SetActive(true);
                ChampionManager.Instance.OnPracticeEnd();
                SetTeamModeLabel();
            }
            else
            {
                SettingLayer.SetActive(true);
                PracticeModeLayer.SetActive(false);
                SetTeamModeLabel();
            }
        }

        void RefreshUI()
        {
            SetTeamModeLabel();
            InitCurrentTotalBP();
            DrawDragon();
            SetDragonList();
        }
        void SetDragonList()
        {
            stageDragonList.OnShowList();
            RegistDragonListCallback();
            stageDragonList.Init(ChampionManager.Instance.MyInfo.ChampionDragons.GetAllUserDragons().Select(data => data.Tag).ToArray(), this);
            ChangeDragonButtonLabel(true);
            RefreshDragonBattleLineLabel();
        }
        void InitCurrentTotalBP()
        {
            tempCurrentTotalBP = 0;
        }

        void RefreshDragonBattleLineLabel()
        {
            if (battleLineLabel != null)
            {
                int currentCount = battleLine.DeckCount;
                battleLineLabel.text = string.Format("{0}/{1}", currentCount, dragonBattleLineMaxSize);
            }
        }

        void SetTeamModeLabel()
        {
            if (teamSettingModeLabel != null)
            {
                if (PracticeModeLayer.activeInHierarchy)
                {
                    teamSettingModeLabel.text = StringData.GetStringByStrKey("챔피언연습모드");
                }
                else
                {
                    switch (CurTeamType)
                    {
                        case ParticipantData.eTournamentTeamType.ATTACK:
                            teamSettingModeLabel.text = StringData.GetStringByStrKey("공격덱");
                            break;
                        case ParticipantData.eTournamentTeamType.DEFFENCE:
                            teamSettingModeLabel.text = StringData.GetStringByStrKey("방어덱");
                            break;
                        case ParticipantData.eTournamentTeamType.HIDDEN:
                            teamSettingModeLabel.text = StringData.GetStringByStrKey("히든덱");
                            break;
                    }
                }
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
            if (currentDragonDeckList == null)
                currentDragonDeckList = new List<ChampionBattleCharacterSlotFrame>();
            int i = 0, l = 0, lineLimit = 2;
            int[] points = new int[(int)eElementType.MAX] { 0, 0, 0, 0, 0, 0, 0 };
            while (l < 3)
            {
                int val = battleLine.GetDragon(l, i);

                UserDragon element = ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(val);

                ChampionBattleCharacterSlotFrame characterSlotComp = null;
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

                    characterSlotComp = dragonSlot.GetComponent<ChampionBattleCharacterSlotFrame>();
                }

                if (element != null)
                {
                    element.RefreshALLStatus();
                    myTotalBp += (int)element.Status.GetTotalINF();
                    if (points.Length > element.BaseData.ELEMENT)
                        points[element.BaseData.ELEMENT] += element.Status.GetTotalINF();

                    if (characterSlotComp != null)
                    {
                        characterSlotComp.SetDragonData(element as ChampionDragon, battleLine);
                        characterSlotComp.setCallback((CharacterSlotFrame.func)((param) =>
                        {
                            if (this.CurSelectedDragonTag < 0) return;
                            if (this.CurSelectedDragonTag > 0 && int.Parse(param) > 0)  //요 파트가 코코스에선 빠져 있어서 나중에 코코스 재작업한다면 추가 필요
                            {
                                OnclickChangeTeam(int.Parse(param), (int)this.CurSelectedDragonTag);
                                HideAllArrowAnimation();
                                return;
                            }
                            bool isHideArrow = characterSlotComp.isHideArrow();
                            if (!isHideArrow) return;
                            bool isDeckFull = battleLine.IsDeckFull();
                            if (isDeckFull)
                            {
                                OnClickReleaseTeam(int.Parse(param));
                                return;
                            }
                            OnClickRegistTeam((int)this.CurSelectedDragonTag);
                            InitClickFullDragonTag();
                        }));
                    }
                }
                else
                {
                    characterSlotComp.SetDragonData(null, battleLine);
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

            for (i = 0; i < currentDragonDeckList.Count; ++i)
            {
                ChampionBattleCharacterSlotFrame slotInfo = currentDragonDeckList[i];
                int currentDragonTag = slotInfo.DragonTag;
                if (currentDragonTag <= 0 && slotInfo == null)
                    continue;
                slotInfo.HideHiddenDragonUI();
            }


            RefreshDragonBattleLineLabel();
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
        public void OnBattleSetting()
        {
            SettingLayer.SetActive(true);
            PracticeModeLayer.SetActive(false);
            SetTeamModeLabel();

            UIManager.Instance.MainUI.SetTownButtonCallBack(OnExit);
        }

        public void OnPractice()
        {
            SettingLayer.SetActive(false);
            PracticeModeLayer.SetActive(true);

            SetTeamModeLabel();
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

            bool isFull = battleLine.IsDeckFull();
            if (!isFull)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100010045), true, false, true);
                return;
            }

            bool IsFullSet = true;
            foreach (var dragonNo in battleLine.GetList())
            {
                if (dragonNo == 0) 
                    continue;

                var dragon = ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(dragonNo) as ChampionDragon;
                if (dragon != null)
                {
                    if (!dragon.IsFullSetting())
                    {
#if UNITY_EDITOR
                        dragon.ReqSaveDragon(null, true);
                        continue;
#endif
                        IsFullSet = false;
                        for (int i = 0; i < currentDragonDeckList.Count; ++i)
                        {
                            if (currentDragonDeckList[i].DragonTag == dragonNo)
                                currentDragonDeckList[i].ShowReddotAnimNode();
                        }
                    }

                    //if (!dragon.IsPartsFullSetting())
                    //{
                    //    IsFullSet = false;
                    //}
                    //if (!dragon.IsPetFullSetting())
                    //{
                    //    IsFullSet = false;
                    //}
                    //if (!dragon.IsPassiveFullSetting())
                    //{
                    //    IsFullSet = false;
                    //}
                }
            }

            if (!IsFullSet) 
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100010044), true, false, true);
                return; 
            }

            var form = ChampionManager.Instance.MyInfo.GetChampionBattleFomation(CurTeamType);
            if (form != null)
            {
                bool same = true;
                var saved = form.GetList();
                var cur = battleLine.GetList();
                for (int i = 0; i < 6; i++)
                {
                    if (saved[i] != cur[i])
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("변경없음"));
                    return;
                }
            }

            battleLine.Save((response) => {
                int rs = response["rs"].Value<int>();
                switch ((eApiResCode)rs)
                {
                    case eApiResCode.OK:
                    {
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
                    break;
                    case eApiResCode.TOURNAMENT_INCORRECT_ROUND_STEP:
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("챔피언팀세팅타임오버팝업"));
                    }
                    break;
                    default:
                    {
                        string msgKey = "챔피언오류:" + rs.ToString();
                        string msg = StringData.GetStringByStrKey(msgKey);
                        if(msg == msgKey)
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000634));
                        else
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), msg + string.Format("\nerrorCode ::{0}", rs));
                    }
                    break;
                }
            });
        }

        bool IsShowDragonList()
        {
            if (stageDragonList != null) return stageDragonList.IsShowList();

            return false;
        }

        void ChangeDragonButtonLabel(bool isOpen)
        {
            if (teamSettingButtonLable != null)
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
            if (battleLine.GetDragon(line, index) <= 0 && CurSelectedDragonTag <= 0)
            {
                return;
            }
            battleLine.AddDragonPosition(line, index, CurSelectedDragonTag);
            DrawTeamDragon();
            stageDragonList.RefreshList(battleLine.GetArray());
            HideAllArrowAnimation();
        }
        void OnclickChangeTeam(int BeforeDTag, int AfterDTag)
        {
            battleLine.ChangeDragon(BeforeDTag, AfterDTag);
            DrawTeamDragon();
            HideAllArrowAnimation();
            stageDragonList.RefreshList(battleLine.GetArray());
        }
        void OnClickReleaseTeam(int dragonTag)
        {
            bool isVisible = IsShowDragonList();
            if (!isVisible) return;
            battleLine.DeleteDragon(dragonTag);
            DrawTeamDragon();
            stageDragonList.RefreshList(battleLine.GetArray());
            
            HideAllArrowAnimation();
        }
        void RegistDragonListCallback()
        {
            stageDragonList.SetRegistCallBack(OnClickRegistTeam);
            stageDragonList.SetReleaseCallback(OnClickReleaseTeam);
        }

        void OnClickRegistTeam(int dragonTag)
        {
            bool isVisible = IsShowDragonList();
            if (!isVisible) return;


            if (CurSelectedDragonTag > 0)
            {
                HideAllArrowAnimation();
            }
            CurSelectedDragonTag = dragonTag;
            var line = battleLine.GetList();
            if (battleLine.IsDeckFull())
            {
                for (int i = 0; i < currentDragonDeckList.Count; ++i)
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

        }
        void HideAllArrowAnimation()
        {
            InitClickFullDragonTag();
            if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0) return;

            foreach (ChampionBattleCharacterSlotFrame elem in currentDragonDeckList)
            {
                if (elem == null) return;
                elem.HideAnimArrowNode();
            }
        }
        void InitClickFullDragonTag()
        {
            CurSelectedDragonTag = -1;
        }
        void SaveComplete()
        {
            if (IsShowDragonList())
            {
                if (stageDragonList != null)
                {
                    HideAllArrowAnimation();
                }
            }
            PopupManager.ClosePopup<SystemPopup>();
            
            //버튼이 합쳐지니 씬이동은하지말자
            //RequestVariationSceneCheck();
        }
        //void onClickFullDragonDeckProcess(int currentClickTag)
        //{
        //    currentClickDragonTag = currentClickTag;
        //    ShowAllArrowAnimation();
        //}
        void ShowAllArrowAnimation()
        {
            if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0) return;

            foreach (ChampionBattleCharacterSlotFrame element in currentDragonDeckList)
            {
                if (element == null) return;
                element.ShowAnimArrowNode();
            }
        }
        public void OnClickSuggestion()
        {
            if (battleLine == null) return;

            SetAutoBattleLine(battleLine);
            DrawTeamDragon();
            if (stageDragonList != null)
            {
                stageDragonList.RefreshList(battleLine.GetArray());
            }

            HideAllArrowAnimation();
        }

        void RequestVariationSceneCheck()
        {
            LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation);
            return;
            //if (ArenaManager.Instance.IsFriendFightDataFlag)
            //{
            //    LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation);
            //    return;
            //}


            //if (isAtkSettingMode && tempOtherIndex >= 0)
            //{
            //    ArenaManager.Instance.SetArenaVersusTeamData(tempOtherIndex, tempMatchListFlag);

            //    //LoadingManager.ImmediatelySceneLoad("ArenaBattleReady");//임시
            //    LoadingManager.Instance.EffectiveSceneLoad("ArenaBattleReady", eSceneEffectType.CloudAnimation);

            //}
            //else
            //{
            //    //LoadingManager.ImmediatelySceneLoad("ArenaLobby");//임시
            //    LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation);
            //}
        }

        private void OnDestroy()
        {
            UIManager.Instance.MainUI.ReleaseTownButtonCallBack();
        }

        public void OnEvent(DragonChangedEvent eventType)
        {
            if (eventType.type == DragonChangedEvent.TYPE.MOVE_START)
            {
                if (currentDragonDeckList == null || currentDragonDeckList.Count <= 0)
                {
                    return;
                }

                foreach (ChampionBattleCharacterSlotFrame elem in currentDragonDeckList)
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
                ChampionBattleCharacterSlotFrame mover = null;
                ChampionBattleCharacterSlotFrame target = null;

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
            battleLine.LoadBattleLine(presetNo);
            DrawTeamDragon();
            SetDragonList();
        }

        public void OnTeamSelect(int team)
        {
            ParticipantData.eTournamentTeamType teamType = (ParticipantData.eTournamentTeamType)team;
            if (teamType == ParticipantData.eTournamentTeamType.NONE && teamType != CurTeamType)
                return;

            var form = ChampionManager.Instance.MyInfo.GetChampionBattleFomation(CurTeamType);
            if (form != null)
            {
                bool same = true;
                var saved = form.GetList();
                var cur = battleLine.GetList();
                for (int i = 0; i < 6; i++)
                {
                    if(saved[i] != cur[i])
                    {
                        same = false;
                        break;
                    }
                } 

                if(!same)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("교체물음"), () => {
                        SetTeamMode(teamType);
                    }, ()=> { 

                    });
                    return;
                }
            }

            SetTeamMode(teamType);
        }

        public void SetTeamMode(ParticipantData.eTournamentTeamType teamType)
        {
            CurTeamType = teamType;
            battleLine = new ChampionBattleLine(ChampionManager.Instance.MyInfo.GetChampionBattleFomation(CurTeamType));
            RefreshUI();

            foreach (var btn in TeamSelectButton)
            {
                btn.Refresh(teamType);
            }

            stageDragonList.RefreshSaveButtonState(IsCurTeamSettableTime());
        }

        public bool IsCurTeamSettableTime()
        {
            foreach (var btn in TeamSelectButton)
            {
                if (CurTeamType == btn.type)
                {
                    return !btn.timeOver;
                }
            }

            return false;
        }



        void SetAutoBattleLine(ChampionBattleLine line)
        {
            if (line == null)
                return;

            var dragonList = ChampionManager.Instance.MyInfo.GetFormationFreeDragons();
            foreach (var tag in line.GetList())
            {
                if (tag > 0)
                {
                    var dragon = ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(tag);
                    if (dragon != null)
                    {
                        bool contained = false;
                        foreach(var dr in dragonList)
                        {
                            if(dr.Tag == dragon.Tag)
                            {
                                contained = true;
                                break;
                            }
                        }

                        if(!contained)
                            dragonList.Add(dragon as ChampionDragon);
                    }
                }
            }

            line.Clear();

            List<ChampionDragon>[] dragons = new List<ChampionDragon>[3] {
                new List<ChampionDragon>(),
                new List<ChampionDragon>(),
                new List<ChampionDragon>()
            };

            foreach (ChampionDragon dragon in dragonList)
            {
                if (line.IsContainDragon(dragon))
                    continue;

                switch ((eJobType)dragon.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        dragons[0].Add(dragon);
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        dragons[1].Add(dragon);
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        dragons[2].Add(dragon);
                        break;
                }
            }

            foreach (List<ChampionDragon> list in dragons)
            {
                list.Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
            }

            Dictionary<Vector2Int, ChampionDragon> DragonLine = new Dictionary<Vector2Int, ChampionDragon>();
            var target = GetTopDragon(ref dragons, 0);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(0, 0), target);
                ExceptDragon(target, ref dragons);
            }
            target = GetTopDragon(ref dragons, 0);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(0, 1), target);
                ExceptDragon(target, ref dragons);
            }

            target = GetTopDragon(ref dragons, 1);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(1, 0), target);
                ExceptDragon(target, ref dragons);
            }
            target = GetTopDragon(ref dragons, 1);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(1, 1), target);
                ExceptDragon(target, ref dragons);
            }

            target = GetTopDragon(ref dragons, 2);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(2, 0), target);
                ExceptDragon(target, ref dragons);
            }
            target = GetTopDragon(ref dragons, 2);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(2, 1), target);
                ExceptDragon(target, ref dragons);
            }

            if (DragonLine.Count > 5)
            {
                Vector2Int weakness = new Vector2Int(-1, -1);
                foreach (var cand in DragonLine)
                {
                    if (DragonLine.ContainsKey(weakness))
                    {
                        if (DragonLine[weakness].GetTotalINF() > cand.Value.GetTotalINF())
                            weakness = cand.Key;
                    }
                    else
                    {
                        weakness = cand.Key;
                    }
                }

                DragonLine.Remove(weakness);
            }
            else if (DragonLine.Count < 5)
            {
                Dictionary<Vector2Int, ChampionDragon> empty = new Dictionary<Vector2Int, ChampionDragon>();
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        ChampionDragon candi = null;
                        switch (x)
                        {
                            case 0:
                                candi = GetTopDragon(ref dragons, 1);
                                if (candi == null)
                                    candi = GetTopDragon(ref dragons, 2);
                                break;
                            case 1:
                                var f = GetTopDragon(ref dragons, 0);
                                var b = GetTopDragon(ref dragons, 2);
                                if (f == null)
                                {
                                    candi = b;
                                    break;
                                }
                                if (b == null)
                                {
                                    candi = f;
                                    break;
                                }

                                if (f.GetTotalINF() < b.GetTotalINF())
                                    candi = b;
                                else
                                    candi = f;
                                break;
                            case 2:
                                candi = GetTopDragon(ref dragons, 1);
                                if (candi == null)
                                    candi = GetTopDragon(ref dragons, 0);
                                break;
                        }

                        if (candi == null)
                            continue;

                        var key = new Vector2Int(x, y);
                        if (DragonLine.ContainsKey(key))
                            continue;

                        empty.Add(key, candi);
                        ExceptDragon(candi, ref dragons);
                    }
                }

                while (DragonLine.Count + empty.Count > 5)
                {
                    Vector2Int weakness = new Vector2Int(-1, -1);
                    foreach (var cand in empty)
                    {
                        if (empty.ContainsKey(weakness))
                        {
                            if (empty[weakness].GetTotalINF() > cand.Value.GetTotalINF())
                                weakness = cand.Key;
                        }
                        else
                        {
                            weakness = cand.Key;
                        }
                    }
                    empty.Remove(weakness);
                }

                foreach (var cand in empty)
                {
                    DragonLine.Add(cand.Key, cand.Value);
                }
            }

            Dictionary<int, List<UserDragon>> positions = new Dictionary<int, List<UserDragon>>();
            for (int i = 0; i < 3; i++)
            {
                positions.Add(i, new List<UserDragon>());
            }

            foreach (var ca in DragonLine.Values)
            {
                int index = -1;
                switch ((eJobType)ca.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        index = 0;
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        index = 1;
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        index = 2;
                        break;
                }

                if (index < 0)
                    continue;

                positions[index].Add(ca);
            }

            List<UserDragon> migration = new List<UserDragon>();
            for (int i = 0; i < 3; i++)
            {
                positions[i].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                while (positions[i].Count > 2)
                {
                    var weakness = positions[i][positions[i].Count - 1];
                    positions[i].Remove(weakness);
                    migration.Add(weakness);
                }
            }

            int repeat = 5;
            while (migration.Count > 0 && repeat > 0)
            {
                repeat--;

                var cur = migration[0];
                migration.Remove(cur);

                switch ((eJobType)cur.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        if (positions[1].Count < 2)
                        {
                            positions[1].Add(cur);
                        }
                        else
                        {
                            positions[1].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                            var weakness = positions[1][positions[1].Count - 1];
                            positions[1].Remove(weakness);
                            migration.Add(weakness);

                            positions[1].Add(cur);
                        }
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        if (positions[2].Count < 2)
                        {
                            positions[2].Add(cur);
                        }
                        else if (positions[0].Count < 2)
                        {
                            positions[0].Add(cur);
                        }
                        else
                        {
                            //일로타면 안됨..무한룹가능성
                            Debug.LogError("로직이 삐꾸네");
                        }
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        if (positions[1].Count < 2)
                        {
                            positions[1].Add(cur);
                        }
                        else
                        {
                            positions[1].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                            var weakness = positions[1][positions[1].Count - 1];
                            positions[1].Remove(weakness);
                            migration.Add(weakness);

                            positions[1].Add(cur);
                        }
                        break;
                }
            }
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    if (positions[x].Count > y)
                        line.AddDragonPosition(x, y, positions[x][y].Tag);
                }
            }
        }
        ChampionDragon GetTopDragon(ref List<ChampionDragon>[] dragons, int index)
        {
            ChampionDragon ret = null;

            if (dragons[index].Count > 0)
                ret = dragons[index][0];

            return ret;
        }
        void ExceptDragon(ChampionDragon target, ref List<ChampionDragon>[] dragons)
        {
            int index = -1;
            switch ((eJobType)target.JOB())
            {
                case eJobType.TANKER:
                case eJobType.WARRIOR:
                    index = 0;
                    break;
                case eJobType.ASSASSIN:
                case eJobType.BOMBER:
                    index = 1;
                    break;
                case eJobType.SNIPER:
                case eJobType.SUPPORTER:
                    index = 2;
                    break;
            }
            if (index < 0)
                return;

            if (dragons[index].Count > 0 && dragons[index].Contains(target))
            {
                dragons[index].Remove(target);
            }
        }
    }
}
