using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using DG.Tweening;

namespace SandboxNetwork { 
    public class ChampionBattleReady : MonoBehaviour, EventListener<DragonChangedEvent>, EventListener<ItemFrameEvent>
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


        private ChampionBattleLine A_SIDE_BattleLine { get { return ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].A_SIDE.GetChampionBattleFomation(teamType); } }
        private ChampionBattleLine B_SIDE_BattleLine{ get { return ChampionManager.Instance.CurChampionInfo.MatchData[curIndex].B_SIDE.GetChampionBattleFomation(teamType); } }

        List<DragonInfo> OtherDragonTagList = new List<DragonInfo>();

        private int currentIndex = -1;
        ArenaTeamData currentMatchData = null;
        private bool isMatchList = false; //대전 리스트에서 왔는지 , 방어기록에서 왔는지 체크
        private bool isStart = false;
        private bool isNetworkState = false;

        ChampionLeagueTable.ROUND_INDEX curIndex = ChampionLeagueTable.ROUND_INDEX.NONE;
        ParticipantData.eTournamentTeamType teamType = ParticipantData.eTournamentTeamType.ATTACK;
        int currentTeamPresetNo = 0;
        const int RequireArenaTicketCount = 1;

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

            SetData();
            isNetworkState = false;
        }
        void SetData()
        {
            arenaReadyDescText.text = StringData.GetStringByStrKey("main_btn_arena");

            currentTeamPresetNo = CacheUserData.GetInt("presetArenaAtkDeck", 0);
            SetPresetBtn(currentTeamPresetNo);
            SetOtherDragonTeamData();
            SetMatchUIData();
            RefreshUI();
            UIManager.Instance.MainUI.SetTownButtonCallBack(() => {
                LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Arena));
            });
            DrawDragon();
            ChampionManager.Instance.battleInfo.Init();
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

            battleStartButton.gameObject.SetActive(true);
            battleFriendStartButton.gameObject.SetActive(false);
            
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
        void SetMatchUIData()
        {
            if (userNick != null) userNick.text = ChampionManager.Instance.CurLoger.UserA.Nick;
            //if (userTrophyCount != null) userTrophyCount.text = ChampionManager.Instance.UserA.Point.ToString();
            if (otherNick != null) otherNick.text = ChampionManager.Instance.CurLoger.UserB.Nick;
            //if (otherTrophyCount != null) otherTrophyCount.text = ChampionManager.Instance.UserB.Point.ToString();

            if (myTrophyNode != null)
                myTrophyNode.SetActive(false);
            if (otherTrophyNode != null)
                otherTrophyNode.SetActive(false);
        }
        
        void DrawDragon()
        {
            DrawMyDragon();
            DrawVersusDragon();
        }

        void DrawMyDragon()
        {
            A_SIDE_BattleLine.LoadBattleLine(currentTeamPresetNo);
            DrawTeamDragon(myDragonParent, A_SIDE_BattleLine, true);
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
            B_SIDE_BattleLine.SetLine(onlyDragonTagList);
            DrawTeamDragon(otherDragonParent, B_SIDE_BattleLine, false);
        }

        void DrawTeamDragon(GameObject[] targetNodeList, ChampionBattleLine targetBattleLine, bool isMine)
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
        int DrawMyDragonByCondition(int index_i, int index_l, GameObject[] targetNodeList, ChampionBattleLine targetBattleLine)
        {
            int tempBP = 0;
            int tag = targetBattleLine.GetDragon(index_l, index_i);

            UserDragon dragonInfo = ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(tag);
            var dragonSlot = Instantiate(prefDragonSlot, targetNodeList[index_l].transform);
            dragonSlot.SetActive(true);
            dragonSlot.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            dragonSlot.transform.localEulerAngles = new Vector3(0, 0, 40);
            var characterSlotComp = dragonSlot.GetComponent<CharacterSlotFrame>();
            if (dragonInfo != null)
            {
                tempBP = dragonInfo.Status.GetTotalINF();

                if(characterSlotComp != null)
                {
                    characterSlotComp.SetDragonData(dragonInfo.Tag, true, true, targetBattleLine, false);
                    characterSlotComp.HideShadow();
                    characterSlotComp.name = dragonInfo.Tag.ToString();
                }
            }
            else
            {
                characterSlotComp.SetDragonData(0, false, true, null, false);
            }
            return tempBP;
        }
        void DrawOtherDragonByCondition(int index_i, int index_l, GameObject[] targetNodeList, ChampionBattleLine targetBattleLine)
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
            if (A_SIDE_BattleLine == null) return;

            SetAutoBattleLine(A_SIDE_BattleLine);

            DrawTeamDragon(myDragonParent, A_SIDE_BattleLine, true);
            SendFormation();
        }

        void SetAutoBattleLine(ChampionBattleLine line)
        {
            if (line == null)
                return;

            line.Clear();

            var dragonList = ChampionManager.Instance.MyInfo.GetFormationFreeDragons();

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
        void SendFormation()
        {
            if (isNetworkState)
            {
                return;
            }

            A_SIDE_BattleLine.Save();
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

        public void OnEvent(DragonChangedEvent eventType)
        {
            DrawTeamDragon(myDragonParent, A_SIDE_BattleLine, true);
        }

        public void OnEvent(ItemFrameEvent eventType)
        {
            RefreshBattleButton();
        }
    }
}