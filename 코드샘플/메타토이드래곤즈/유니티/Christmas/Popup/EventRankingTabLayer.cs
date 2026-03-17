using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class EventRankInfoData : ITableData
    {
        public int Rank { get; private set; } = 1;
        public long UserNo { get; private set; } = 0;
        public string UserPortrait { get; private set; } = string.Empty;
        public string UserNick { get; private set; } = string.Empty;
        public int Point { get; private set; } = 0;

        public int Current { get; private set; } = 0;//내꺼일때만 유효허다

        public EventRankInfoData(int rank, string userPortrait, string userNick, int point, long userNo = 0, int current = 0)
        {
            Rank = rank;
            UserPortrait = userPortrait;
            UserNick = userNick;
            Point = point;
            UserNo = userNo;
            Current = current;
        }
        public string GetKey() { return Rank.ToString(); }
        public void Init() { }
    }

    public class EventRankRewardInfoData : ITableData
    {
        public int startRank { get; private set; } = -1;
        public int endRank { get; private set; } = -1;
        public int ItemGroupNo { get; private set; } = 0;

        public int Key { get; private set; } = 0;
        public EventRankRewardInfoData(int key, int start_rank, int end_rank, int itemGroupNo)
        {
            Key = key;
            endRank = end_rank;
            startRank = start_rank;
            ItemGroupNo = itemGroupNo;
        }
        public string GetKey() { return Key.ToString(); }
        public void Init() { }
    }
    public class EventRankingTabLayer : TabLayer
    {
        [SerializeField]
        Text tipText = null;
        [Header("rank menu")]
        [SerializeField]
        Button[] rankingTabs;


        [Header("rank info")]
        [SerializeField]
        TableView RankTableView = null;
        [SerializeField]
        DiceEventRankingSlot myRankSlot;

        [Header("rank reward info")]
        [SerializeField]
        GameObject BoxLayer = null;
        [SerializeField]
        TableView RankRewardTableViewWithBox = null; // 테이블 뷰를 가변 크기로 해서 두면 OnStart를 너무 자주 호출하는 구조로 됨, OnStart에서 크기 계산하기 때문에 2개로 호출 안하면 UI 깨짐
        [SerializeField]
        protected Text rankRewardMissionText = null;

        [SerializeField]
        GameObject noneBoxLayer = null;
        [SerializeField]
        protected TableView RankRewardTableView = null;
        [SerializeField]
        Text emptyRankingText = null;

        [SerializeField]
        Text todayRankTitleText = null;

        eEventDailyRankingType todayRank = eEventDailyRankingType.NONE;
        bool isInit = false;

        private void OnDisable()
        {
            ClearData();
            InitSubLayerIndex();
            InitButtonInteractable();
        }

        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);
            if (isInit == false)
            {
                RankTableView?.OnStart();
                RankRewardTableViewWithBox?.OnStart();
                RankRewardTableView?.OnStart();
                InitEventSchedule();
                isInit = true;

                SetEventData();
            }

            if (SubLayerIndex == -1)
                OnClickRankTab(0);
            else
                OnClickRankTab(SubLayerIndex);
        }

        protected virtual void SetEventData() { }

        public override void RefreshUI()//데이터 유지 갱신
        {
            if (SubLayerIndex == -1)
                OnClickRankTab(0);
            else
                OnClickRankTab(SubLayerIndex);
        }

        void InitEventSchedule()
        {
            tipText.text = StringData.GetStringByStrKey("2023_EVENT_MENU3_2_TIP");
        }

        void InitButtonInteractable()
        {
            foreach (var btn in rankingTabs)
                if (btn != null)
                    btn.interactable = true;
        }

        protected void SetVisibleTipText(bool _isVisible)
        {
            if (tipText != null)
                tipText.gameObject.SetActive(_isVisible);
        }

        public void OnClickRankTab(int kind)
        {
            InitButtonInteractable();
            rankingTabs[kind].interactable = false;
            SetSubLayerIndex(kind);

            if (emptyRankingText != null)
                emptyRankingText.gameObject.SetActive(false);

            if (rankRewardMissionText != null)
                rankRewardMissionText.text = "-";

            SetTodayTitleText("-");

            SetVisibleTipText(false);

            ShowRankPageBySubLayerIndex();

            SetRankRewardTable();
        }

        /// <summary>
        /// sublayerIndex 에 따른 case 처리는 내부 구현
        /// </summary>
        protected virtual void ShowRankPageBySubLayerIndex() { }
        protected void ShowPageProcess(bool _isBoxExist)
        {
            SetRewardTableViewLayer(_isBoxExist);
            RequestRankingData(SubLayerIndex + 1);
        }

        protected void SetTodayTitleText(string _text = "")
        {
            if (todayRankTitleText != null)
                todayRankTitleText.text = _text;
        }

        /// <summary>
        /// 보상 리스트
        /// </summary>
        protected virtual void SetRankRewardTable()
        {
            List<ITableData> rankRewardData = new List<ITableData>();
            List<EventRankRewardData> tableList = new List<EventRankRewardData>();

            var eventRankingList = EventRankRewardData.GetGroup(SubLayerIndex + 1);
            if (eventRankingList != null && eventRankingList.Count > 0)
                tableList = eventRankingList.ToList();

            foreach (var rankData in tableList)
            {
                var key = int.Parse(rankData.KEY);
                var rewardGroup = rankData.REWARD_GROUP;
                var high = rankData.HIGHEST_RANK;

                int resultLow = 0;
                var low = rankData.LOWEST_RANK;//초과값
                if (low >= uint.MaxValue)
                    resultLow = -1;
                else
                    resultLow = (int)(low - 1);

                rankRewardData.Add(new EventRankRewardInfoData(key, high, resultLow, rewardGroup));
            }

            RankRewardTableView.SetDelegate(
                new TableViewDelegate(rankRewardData, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<DiceEventRankingRewardSlot>();
                    if (slotInfo == null) return;
                    var data = (EventRankRewardInfoData)item;
                    slotInfo.Init(data.Key, data.startRank, data.endRank, data.ItemGroupNo);
                }));
            RankRewardTableViewWithBox.SetDelegate(
                new TableViewDelegate(rankRewardData, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<DiceEventRankingRewardSlot>();
                    if (slotInfo == null) return;
                    var data = (EventRankRewardInfoData)item;
                    slotInfo.Init(data.Key, data.startRank, data.endRank, data.ItemGroupNo);
                }));
            RankRewardTableView.ReLoad();
            RankRewardTableViewWithBox.ReLoad();
        }

        public void OnClickHelpBtn()
        {
            PopupManager.OpenPopup<DiceEventHelpPopup>(new HelpPopupData(3, SubLayerIndex + 1));
        }

        void SetRewardTableViewLayer(bool isBoxExist)
        {
            BoxLayer.SetActive(isBoxExist);
            noneBoxLayer.SetActive(!isBoxExist);
        }

        protected virtual void RequestRankingData(int _page) { }


        public void ClearRankingScroll(int page)
        {
            if (emptyRankingText != null)
                emptyRankingText.gameObject.SetActive(true);

            myRankSlot.InitMyRanking(todayRank == eEventDailyRankingType.DAILY_EVENT_UNION_ARENA_WIN);

            RankTableView.SetDelegate(
                     new TableViewDelegate(new List<ITableData>(), (GameObject node, ITableData item) => {
                         if (node == null) return;
                         var slotInfo = node.GetComponent<DiceEventRankingSlot>();
                         if (slotInfo == null) return;
                         slotInfo.Init((OpenEventRankingData)item, todayRank == eEventDailyRankingType.DAILY_EVENT_UNION_ARENA_WIN);
                         node.SetActive(true);
                     }));
            RankTableView.ReLoad();
        }

        protected void RefreshRankingScrollview(JObject _jsonData)
        {
            todayRank = eEventDailyRankingType.NONE;
            if (_jsonData.ContainsKey("daily_type"))
            {
                todayRank = (eEventDailyRankingType)_jsonData["daily_type"].Value<int>();
            }

            if (_jsonData.ContainsKey("mine"))
            {
                if (SBFunc.IsJTokenType(_jsonData["mine"], JTokenType.Object))//내 정보가 없으면 빈값 "[]" 으로 옴.
                {
                    var myData = new OpenEventRankingData((JObject)_jsonData["mine"]);
                    myRankSlot.Init(myData, todayRank == eEventDailyRankingType.DAILY_EVENT_UNION_ARENA_WIN);
                }
                else //내정보 없을 때 빈 값 처리 
                    myRankSlot.InitMyRanking(todayRank == eEventDailyRankingType.DAILY_EVENT_UNION_ARENA_WIN);
            }

            SetMissionText((int)todayRank);

            if (_jsonData.ContainsKey("ranking"))
            {
                List<ITableData> rankingList = new List<ITableData>();
                if (SBFunc.IsJTokenType(_jsonData["ranking"], JTokenType.Array))//내 정보가 없으면 빈값 "[]" 으로 옴.
                {
                    foreach (var arrToken in _jsonData["ranking"].ToObject<JArray>())
                    {
                        if (!arrToken.HasValues)
                            continue;

                        OpenEventRankingData tempMatchDataSet = new OpenEventRankingData((JObject)arrToken);
                        rankingList.Add(tempMatchDataSet);
                    }
                }

                var isEmpty = rankingList.Count <= 0;
                if (emptyRankingText != null)
                    emptyRankingText.gameObject.SetActive(isEmpty);

                RankTableView.SetDelegate(
                     new TableViewDelegate(rankingList, (GameObject node, ITableData item) => {
                         if (node == null) return;
                         var slotInfo = node.GetComponent<DiceEventRankingSlot>();
                         if (slotInfo == null) return;
                         slotInfo.Init((OpenEventRankingData)item, todayRank == eEventDailyRankingType.DAILY_EVENT_UNION_ARENA_WIN);
                         node.SetActive(true);
                     }));
                RankTableView.ReLoad();
            }
        }

        protected virtual void SetMissionText(int curPoint){  }

        List<EventRankInfoData> GetEventRankingData(JObject _jsonData, bool isMine = false)
        {
            List<EventRankInfoData> ret = new List<EventRankInfoData>();

            var properties = _jsonData.Properties();
            foreach (JProperty it in properties)
            {
                var rankInfo = (JObject)it.Value;

                var rank = rankInfo["rank"].Value<int>();
                var nick = isMine ? User.Instance.UserData.UserNick : rankInfo["nick"].Value<string>();
                var userNo = isMine ? User.Instance.UserAccountData.UserNumber : rankInfo["user_no"].Value<long>();
                var userPortrait = isMine ? User.Instance.UserData.UserPortrait : rankInfo["icon"].Value<string>();

                int userPoint;
                if (SubLayerIndex == 2)
                    userPoint = rankInfo["max_point"].Value<int>();
                else
                    userPoint = rankInfo["point"].Value<int>();

                int curPoint = 0;
                if (rankInfo.ContainsKey("curr_point"))
                    curPoint = rankInfo["curr_point"].Value<int>();

                EventRankInfoData info = new EventRankInfoData(rank, userPortrait, nick, userPoint, userNo, curPoint);

                ret.Add(info);
            }

            return ret;
        }
    }
}
