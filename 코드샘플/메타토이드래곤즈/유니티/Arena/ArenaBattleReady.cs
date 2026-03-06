using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using DG.Tweening;

namespace SandboxNetwork { 
    public class ArenaBattleReady : MonoBehaviour, EventListener<DragonChangedEvent>, EventListener<ItemFrameEvent>
    {
        //[SerializeField]
        //Text labelMyBattlePoint = null;
        [SerializeField]
        GameObject prefDragonSlot = null;
        
        [Header("user Info")]
        [SerializeField]
        Text userNick = null;
        [SerializeField]
        Text userTrophyCount = null;
        [SerializeField]
        Text userBattlePointLabel = null;
        [SerializeField]
        GameObject[] myDragonParent = null;
        [SerializeField]
        GameObject myTrophyNode = null;


        [Header("other Info")]
        [SerializeField]
        Text otherNick = null;
        [SerializeField]
        Text otherTrophyCount = null;
        [SerializeField]
        Text otherBattlePointLabel = null;
        [SerializeField]
        GameObject[] otherDragonParent = null;
        [SerializeField]
        GameObject otherTrophyNode = null;

        [Header("buttons")]

        [SerializeField]
        Button battleStartButton = null;
        [SerializeField] Vector2 bubbleNodeOriginPos = new Vector2(0.5f, 13.8f);
        [SerializeField]
        Button battleFriendStartButton = null;

        [SerializeField]
        Text countLabel = null;

        [Header("ETC")]
        [SerializeField]
        Button[] teamPresets = null;
        [SerializeField]
        ElemBuffInfoUI elemBuffUI = null;
        [SerializeField]
        Text arenaReadyDescText = null;


        private ArenaBattleLine myBattleLine = new ArenaBattleLine();
        private ArenaBattleLine otherBattleLine = new ArenaBattleLine();

        List<DragonInfo> OtherDragonTagList = new List<DragonInfo>();

        private int currentIndex = -1;
        ArenaTeamData currentMatchData = null;
        private bool isMatchList = false; //대전 리스트에서 왔는지 , 방어기록에서 왔는지 체크
        private bool isStart = false;
        private bool isNetworkState = false;
        int currentTeamPresetNo = 0;
        const int RequireArenaTicketCount = 1;

        bool isFriendIncome
        {
            get { return ArenaManager.Instance.IsFriendFightDataFlag; }
        }
        private void OnEnable()
        {
            EventManager.AddListener<DragonChangedEvent>(this);
            EventManager.AddListener<ItemFrameEvent>(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<DragonChangedEvent>(this);
            EventManager.RemoveListener<ItemFrameEvent>(this);
        }
        void Start()
        {
            if(isFriendIncome)
            {
                UIManager.Instance.InitUI(eUIType.Arena);
                UIManager.Instance.RefreshUI(eUIType.Arena);
                UIManager.Instance.MainUI.SetActiveChildObject(mainUIObjectType.arenaticket, false);
                UIManager.Instance.MainUI.SetActiveChildObject(mainUIObjectType.friendPoint, true, true);
            }

            SetData();
            isNetworkState = false;
        }
        void SetData()
        {
            arenaReadyDescText.text = !isFriendIncome ? StringData.GetStringByStrKey("main_btn_arena") : StringData.GetStringByIndex(100002139);

            currentTeamPresetNo = CacheUserData.GetInt("presetArenaAtkDeck", 0);
            SetPresetBtn(currentTeamPresetNo);
            SetOtherDragonTeamData();
            SetMatchUIData();
            RefreshUI();
            UIManager.Instance.MainUI.SetTownButtonCallBack(() => {
                if(!isFriendIncome)
                    LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Arena));
                else
                {
                    //var userChatInfo = ArenaManager.Instance.GetFriendFightDataSet();
                    //ChatUserData chatUserData = new ChatUserData(userChatInfo.user_no,userChatInfo.name,userChatInfo.icon_name,userChatInfo.level,userChatInfo.portraitData);

                    ArenaManager.Instance.ClearFriendTeamDataSet();

                    LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town),
                        SBFunc.CallBackCoroutine(() => { PopupManager.OpenPopup<ChattingPopup>().SetDirectFriendListLayer(); }
                    /*PopupManager.OpenPopup<FriendPopup>()*/));
                }
            });
            DrawDragon();
            ArenaManager.Instance.battleInfo.Init();
        }

        public void OnClickTeamPreset(int presetNo)
        {
            currentTeamPresetNo = presetNo;
            SetPresetBtn(presetNo);
            DrawMyDragon();
            //battleLine.CheckBattleLine(presetNo);
            //DrawTeamDragon();
            //SetDragonList();
        }

        void SetPresetBtn(int clickedIndex)
        {
            foreach (var btn in teamPresets)
            {
                btn.interactable = true;
            }
            if (clickedIndex < 0)
                return;
            teamPresets[clickedIndex].interactable = false;
        }

        void RefreshBattleButton() {
            if (battleStartButton == null) return;

            battleStartButton.gameObject.SetActive(!isFriendIncome);
            battleFriendStartButton.gameObject.SetActive(isFriendIncome);
            if (isFriendIncome)
                battleFriendStartButton.SetButtonSpriteState(GetFriendBattleButtonCondition());

            int currentTicket = ArenaManager.Instance.UserArenaData.Arena_Ticket;
            bool isAvailableArena = currentTicket >= RequireArenaTicketCount;
            battleStartButton.interactable = isAvailableArena;
            battleStartButton.SetButtonSpriteState(isAvailableArena);
			SetBubbleNodeEffect(battleStartButton.gameObject, isAvailableArena);

			if (countLabel == null) return;

            countLabel.color = isAvailableArena ? Color.white : Color.red;
        }
        void RefreshUI()  //UI 메니져에서 이 버튼 refresh 하도록 연결해야됨
        {
            UIManager.Instance.RefreshUI(eUIType.Arena);
            RefreshBattleButton();
            UIManager.Instance.MainUI.setArenaTimeCallBack(RefreshBattleButton);
        }
        private void OnDestroy()
        {
            UIManager.Instance.MainUI.setArenaTimeCallBack(null);
            UIManager.Instance.MainUI.ReleaseTownButtonCallBack();
            UIManager.Instance.MainUI.SetActiveChildObject(mainUIObjectType.friendPoint, false);
        }
        void SetOtherDragonTeamData()
        {
            if(!isFriendIncome)
            {
                int checkIndex = ArenaManager.Instance.VersusTeamIndex;
                isMatchList = ArenaManager.Instance.IsVersusMatchList;
                List<ArenaTeamData> totalList = isMatchList ? ArenaManager.Instance.MatchList : ArenaManager.Instance.DefenceList;
                if (totalList != null && totalList.Count > checkIndex)
                {
                    currentIndex = checkIndex;
                    currentMatchData = totalList[currentIndex];
                    OtherDragonTagList = currentMatchData.DefDeck;
                }
            }
            else
            {
                currentMatchData = ArenaManager.Instance.GetFriendFightDataSet();
                OtherDragonTagList = currentMatchData.DefDeck;
            }
        }
        void SetMatchUIData()
        {
            if (userNick != null) userNick.text = User.Instance.UserData.UserNick;
            if (userTrophyCount != null) userTrophyCount.text = ArenaManager.Instance.UserArenaData.season_point.ToString();
            if (otherNick != null) otherNick.text = currentMatchData.Nick;
            if (otherTrophyCount != null) otherTrophyCount.text = currentMatchData.Point.ToString();

            if (myTrophyNode != null)
                myTrophyNode.SetActive(!isFriendIncome);
            if (otherTrophyNode != null)
                otherTrophyNode.SetActive(!isFriendIncome);
        }
        void SetSeasonBuffState()
        {
            int maxCount = myBattleLine.MaxCount;
            eElementType[] elemArr = new eElementType[maxCount];
            for (int i = 0; i < maxCount; i++)
            {
                var tag = myBattleLine.GetDragon(i);
                if (tag != 0)
                {
                    var data = CharBaseData.Get(tag);
                    if (data != null)
                    {
                        elemArr[i] = data.ELEMENT_TYPE;
                        continue;
                    }
                }

                elemArr[i] = eElementType.None;
            }
            elemBuffUI.SetEffect(elemArr);
            elemBuffUI.gameObject.SetActive(!isFriendIncome);
        }
        void DrawDragon()
        {
            DrawMyDragon();
            DrawVersusDragon();
        }

        void DrawMyDragon()
        {
            myBattleLine.isAtkSeetingMode = true;
            myBattleLine.LoadBattleLine(currentTeamPresetNo);
            DrawTeamDragon(myDragonParent, myBattleLine, true);
            SetSeasonBuffState();
        }
        void DrawVersusDragon()
        {
            if (OtherDragonTagList == null || OtherDragonTagList.Count <= 0) return;
            List<int> onlyDragonTagList = new List<int>();
            for(int i = 0; i < OtherDragonTagList.Count; ++i)
            {
                var dataSet = OtherDragonTagList[i];
                int dragonTag = dataSet.Tag;

                onlyDragonTagList.Add(dragonTag);
            }
            otherBattleLine.SetLine(onlyDragonTagList);
            DrawTeamDragon(otherDragonParent, otherBattleLine, false);
        }

        void DrawTeamDragon(GameObject[] targetNodeList, ArenaBattleLine targetBattleLine, bool isMine)
        {
            int myTotalBp = 0;
            RemoveAllDragonPrefab(targetNodeList);
            int i = 0, l = 0, lineLimit = 2;
            while (l < 3)
            {
                if (isMine)
                {
                    myTotalBp += DrawMyDragonByCondition(lineLimit - 1 - i, l, targetNodeList, targetBattleLine);
                }
                else
                {
                    DrawOtherDragonByCondition(lineLimit - 1 - i, l, targetNodeList, targetBattleLine);
                }
                ++i;
                if (i >= lineLimit)
                {
                    i = 0;
                    ++l;
                }
            }
            if (isMine)
            {
             //   labelMyBattlePoint.text = myTotalBp.ToString();
                userBattlePointLabel.text = myTotalBp.ToString();
            }
            else
            {
                otherBattlePointLabel.text = currentMatchData.DefBattlePoint.ToString();
            }
        }
        int DrawMyDragonByCondition(int index_i, int index_l, GameObject[] targetNodeList, ArenaBattleLine targetBattleLine)
        {
            int tempBP = 0;
            int tag = targetBattleLine.GetDragon(index_l, index_i);
            UserDragon element = User.Instance.DragonData.GetDragon(tag);
            var dragonSlot = Instantiate(prefDragonSlot, targetNodeList[index_l].transform);
            dragonSlot.SetActive(true);
            dragonSlot.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            dragonSlot.transform.localEulerAngles = new Vector3(0, 0, 40);
            var characterSlotComp = dragonSlot.GetComponent<CharacterSlotFrame>();
            if (element != null)
            {
                tempBP = element.Status.GetTotalINF();

                if(characterSlotComp != null)
                {
                    characterSlotComp.SetDragonData(element.Tag, true, true, targetBattleLine, false);
                    characterSlotComp.HideShadow();
                    characterSlotComp.name = element.Tag.ToString();
                }
            }
            else
            {
                characterSlotComp.SetDragonData(0, false, true, null, false);
            }
            return tempBP;
        }
        void DrawOtherDragonByCondition(int index_i, int index_l, GameObject[] targetNodeList, ArenaBattleLine targetBattleLine)
        {
            int currentDragonTag = targetBattleLine.GetDragon(index_l, index_i);
            
            if (currentDragonTag> 0)
            {
                int dragonLv = GetOtherDragonLevel(currentDragonTag);
                int transcendLv= GetOtherDragonTranscendence(currentDragonTag);
                var dragonSlot = Instantiate(prefDragonSlot, targetNodeList[index_l].transform);
                dragonSlot.SetActive(true);
                dragonSlot.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                dragonSlot.transform.localEulerAngles = new Vector3(0, 0, 40);
                var characterSlotComp = dragonSlot.GetComponent<CharacterSlotFrame>();

                if (characterSlotComp != null)
                {
                    characterSlotComp.SetCustomData(currentDragonTag, dragonLv, transcendLv, true);
                    characterSlotComp.HideShadow();
                    characterSlotComp.SetBtnNodeState(false);
                    characterSlotComp.name = currentDragonTag.ToString();

                    var infoPanelNode = SBFunc.GetChildrensByName(characterSlotComp.transform,"infopanel");
                    if(infoPanelNode != null)
                    {
                        var currentScale = infoPanelNode.transform.localScale;
                        var MirrorScaleX = new Vector3(currentScale.x * -1, currentScale.y, currentScale.z);
                        infoPanelNode.transform.localScale = MirrorScaleX;
                    }
                }
            }
            else if (currentDragonTag < 0)  // 히든 드래곤
            {
                var dragonSlot = Instantiate(prefDragonSlot, targetNodeList[index_l].transform);
                dragonSlot.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                dragonSlot.transform.localEulerAngles = new Vector3(0, 0, 40);
                var characterSlotComp = dragonSlot.GetComponent<CharacterSlotFrame>();
                if(characterSlotComp != null)
                {
                    characterSlotComp.SetHiddenDragon();
                    characterSlotComp.HideShadow();
                    characterSlotComp.gameObject.SetActive(true);
                    characterSlotComp.gameObject.name = "hiddenDragon";
                }
            }
            else
            {
                var dragonSlot = Instantiate(prefDragonSlot, targetNodeList[index_l].transform);
                dragonSlot.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                dragonSlot.transform.localEulerAngles = new Vector3(0, 0, 40);
            }
           
        }

        int GetOtherDragonLevel(int tag)
        {
            if (OtherDragonTagList == null || OtherDragonTagList.Count <= 0) return 1;

            for(int i =0; i < OtherDragonTagList.Count; ++i)
            {
                var dataSet = OtherDragonTagList[i];
                if (dataSet == null)
                {
                    continue;
                }
                int dragonTag = dataSet.Tag;
                int dragonLevel = dataSet.Level;

                if(dragonTag == tag)
                {
                    return dragonLevel;
                }
            }
            return 1;
        }

        int GetOtherDragonTranscendence(int tag)
        {
            if (OtherDragonTagList == null || OtherDragonTagList.Count <= 0) return 0;

            for (int i = 0; i < OtherDragonTagList.Count; ++i)
            {
                var dataSet = OtherDragonTagList[i];
                if (dataSet == null)
                {
                    continue;
                }
                int dragonTag = dataSet.Tag;
                int transcendenceLv = dataSet.TranscendenceStep;

                if (dragonTag == tag)
                {
                    return transcendenceLv;
                }
            }
            return 0;
        }

        void RemoveAllDragonPrefab(GameObject[] targetNodeList)
        {
            if (targetNodeList == null || targetNodeList.Length <= 0) return;
            for(int i = 0; i < targetNodeList.Length; ++i)
            {
                if (targetNodeList[i] == null) return;
                SBFunc.RemoveAllChildrens(targetNodeList[i].transform);
            }
        }

        public void OnClickSuggetion()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey(("battle_popup_team_recommend")), StringData.GetStringByIndex(100001251), () =>
                {
                    SuggestionOkProcess();
                }, () => {
                },
                () => {
                }
            );
        }
        void SuggestionOkProcess()
        {
            if (myBattleLine == null) return;

            SBFunc.SetAutoBattleLine(myBattleLine);

            DrawTeamDragon(myDragonParent, myBattleLine, true);
            SendAtkFormation();
        }
        void SendAtkFormation()
        {
            if (isNetworkState)
            {
                return;
            }
            WWWForm param = new WWWForm();

            param.AddField("teamno", currentTeamPresetNo);
            param.AddField("dragons", SBFunc.ListToString(myBattleLine.GetList()));
            param.AddField("items", "[0, 0, 0]");//팀장비 갱신 데이터 넘겨야함
            param.AddField("teamname", "arena");
            isNetworkState = true;
            NetworkManager.Send("preference/setteam", param, (JObject jsonData) =>
              {
                  isNetworkState = false;
                  if (jsonData["err"] !=null&& jsonData["rs"]!=null && (int)jsonData["rs"]==0)
                  {
                      User.Instance.PrefData.ArenaFormationData.ClearATKFomationData(currentTeamPresetNo);
                      User.Instance.PrefData.ArenaFormationData.SetATKFormationData(currentTeamPresetNo, myBattleLine.GetList());
                      CacheUserData.SetInt("presetArenaAtkDeck", currentTeamPresetNo);

                      // 토스트 메세지 =100001250,"팀 구성이 변경되었습니다."
                  }
              },
              (string arg) =>
              {
                  isNetworkState = false;
              });
        }
        public void OnClickArenaList()
        {
            if (!isMatchList)
            {
                ArenaManager.Instance.battleInfoTabIdx = 2;
            }
            //LoadingManager.ImmediatelySceneLoad("ArenaLobby");//임시
            LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation);
        }

        public void OnClickTeamSetting()
        {
            ArenaManager.Instance.SetArenaTeamModeData(true, currentIndex, ArenaManager.Instance.IsVersusMatchList);
            //LoadingManager.ImmediatelySceneLoad("ArenaTeamSetting");//임시
            LoadingManager.Instance.EffectiveSceneLoad("ArenaTeamSetting", eSceneEffectType.CloudAnimation);
        }
        public void OnClickStart()
        {
            var matchIndex = currentMatchData != null && currentMatchData.SlotID >= 0 ? currentMatchData.SlotID : currentIndex;
            if (isStart|| matchIndex < 0) return;
            if (myBattleLine.IsDeckEmpty())
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393));
                return;
            }

            isStart = true;
            var deck = myBattleLine.GetList();
            if (isMatchList)
                ArenaManager.Instance.SendInvade(matchIndex, deck, BattleResponse);
            else
                ArenaManager.Instance.SendRevenge(currentMatchData.CombatID, deck, BattleResponse);
            
        }

        public void OnClickFriendStart()
        {
            if(!GetFriendBattleButtonCondition())
            {
                ToastManager.On(StringData.GetStringByStrKey("우정포인트부족"));
                return;
            }

            if (myBattleLine.IsDeckEmpty())
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000393));
                return;
            }

            ArenaManager.Instance.SendFriendFight();
        }

        public void BattleResponse(JObject jsonData)
        {
            if (jsonData == null|| jsonData["rs"] == null || (int)jsonData["rs"] != 0)
            {
                isStart = false;
                return;
            }

            ArenaManager.Instance.ColosseumData.Initialize();
            ArenaManager.Instance.ColosseumData.Set(jsonData);

            ArenaManager.Instance.SetArenaBattleInfo(new ArenaManager.BattleInfo(int.Parse(userBattlePointLabel.text), currentMatchData.DefBattlePoint, currentMatchData.Nick));
            
            LoadingManager.Instance.EffectiveSceneLoad("ArenaColosseum", eSceneEffectType.CloudAnimation, ArenaManager.Instance.ColosseumData.StartLoadingCO());
        }

        Sequence bubbleTween = null;
		void InitTween()
		{
			if (bubbleTween != null)
			{
				bubbleTween.Kill();
			}
			bubbleTween = null;
		}
		void SetBubbleNodeEffect(GameObject _targetButtonObject, bool _isNormal)
		{
			InitTween();

			var bubbleEffect = SBFunc.GetChildrensByName(_targetButtonObject.transform, "bubble");
			if (bubbleEffect != null)
			{
				if (_isNormal)
				{
					float duration = 1f;

					bubbleTween = DOTween.Sequence();
					bubbleTween.AppendCallback(() =>
					{
						duration = 2.0f + SBFunc.RandomValue;
						bubbleEffect.transform.DOLocalJump(bubbleNodeOriginPos, 5.0f + SBFunc.RandomValue, 3, duration);
					});
					bubbleTween.AppendInterval(duration);
					bubbleTween.SetLoops(-1, LoopType.Yoyo);
					bubbleTween.Play();
				}

				else
				{
					bubbleEffect.GetComponent<RectTransform>().anchoredPosition = bubbleNodeOriginPos;
				}
			}
		}

        bool GetFriendBattleButtonCondition()
        {
            return User.Instance.UserData.Friendly_Point > 0;
        }

        public void OnEvent(DragonChangedEvent eventType)
        {
            DrawTeamDragon(myDragonParent, myBattleLine, true);
        }

        public void OnEvent(ItemFrameEvent eventType)
        {
            RefreshBattleButton();
        }
    }
}