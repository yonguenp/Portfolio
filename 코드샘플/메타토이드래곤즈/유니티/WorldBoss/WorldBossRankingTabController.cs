using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
     public enum eWorldBossRankingPage
    {
        NONE,
        DAILY,
        WEEKLY,
        MONTHLY,
        CUMULATIVE,
    }
    
    /// <summary>
    /// 각 탭별로 보상 & 랭킹 세팅
    /// </summary>
    public class WorldBossRankingTabController : MonoBehaviour
    {
        [Header("Tab Button")]
        [SerializeField]
        private Button tabButtonDaily;
        [SerializeField]
        private Button tabButtonWeekly;
        [SerializeField]
        private Button tabButtonAcc;

        [SerializeField]
        private TableView rankingInfoTableView = null;

        [SerializeField]
        private TableView rankingRewardTableView = null;

        [SerializeField]
        private WorldBossRankingInfoSlot myRankSlot = null;
        [SerializeField]
        private GameObject emptyRankingText = null;

        private bool isFirst = false;

        private int tabIndex = 0;

        private void OnDisable()
        {
            tabIndex = 0;
        }
        public void Init(int _clickIndex = 0)
        {
            if (isFirst == false)
            {
                rankingInfoTableView?.OnStart();
                rankingRewardTableView?.OnStart();

                if (myRankSlot != null)
                    myRankSlot.InitMyRanking();

                isFirst = true;
            }

            tabIndex = _clickIndex;

            RefreshButton();

            RequestRankingPage();
        }

        eWorldBossRankingPage GetPageIndexByButtonIndex()
        {
            switch(tabIndex)
            {
                case 0:
                    return eWorldBossRankingPage.DAILY;
                case 1:
                    return eWorldBossRankingPage.WEEKLY;
                case 2:
                    return eWorldBossRankingPage.CUMULATIVE;
            }

            return eWorldBossRankingPage.NONE;
        }

        void RequestRankingPage()
        {
            var curPage = GetPageIndexByButtonIndex();

            if (curPage == eWorldBossRankingPage.NONE)
                return;


            WorldBossManager.Instance.RequestRankingData((int)curPage, (jsonData) => {
                RefreshRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
            }, (failString) => {

            });

            SetRankingRewardTable();
        }

        int RefreshDefenceBattlePoint()
        {
            var defBattleLine = User.Instance.PrefData.ArenaFormationData.TeamFormationDEF[0];
            return CalcTotalBattlePoint(defBattleLine);
        }
        int CalcTotalBattlePoint(List<int> dragonTagList)
        {
            var totalPoint = 0;
            if (dragonTagList == null || dragonTagList.Count <= 0)
            {
                return totalPoint;
            }
            foreach (int tag in dragonTagList)
            {
                var dragonData = User.Instance.DragonData.GetDragon(tag);
                if (dragonData == null) continue;

                totalPoint += (int)dragonData.Status.GetTotalINF();//레벨 스탯, 장비, 등등 포함 총 값
            }
            return totalPoint;
        }

        protected void RefreshRankingScrollview(JObject _jsonData)
        {
            if (_jsonData.ContainsKey("mine"))
            {
                if (SBFunc.IsJTokenType(_jsonData["mine"], JTokenType.Object))//내 정보가 없으면 빈값 "[]" 으로 옴.
                {
                    var myRankData = (JObject)_jsonData["mine"];
                    var infoList = GetRankingData(myRankData, true);
                    if (infoList.Count <= 0)
                    {
                        Debug.Log("error");
                        return;
                    }

                    var myData = infoList[0];
                    myRankSlot?.Init(GetPageIndexByButtonIndex(), myData);
                }
                else //내정보 없을 때 빈 값 처리 
                    myRankSlot?.InitMyRanking();
            }

            if (_jsonData.ContainsKey("list"))
            {
                List<ITableData> rankDataList = new List<ITableData>();
                if (SBFunc.IsJTokenType(_jsonData["list"], JTokenType.Object))//내 정보가 없으면 빈값 "[]" 으로 옴.
                {
                    var rankingObject = (JObject)_jsonData["list"];
                    var infoList = GetRankingData(rankingObject);

                    foreach (var data in infoList)
                    {
                        rankDataList.Add(data);
                    }
                }

                var isEmpty = rankDataList.Count <= 0;
                if (emptyRankingText != null)
                    emptyRankingText.SetActive(isEmpty);

                rankingInfoTableView.SetDelegate(
                     new TableViewDelegate(rankDataList, (GameObject node, ITableData item) => {
                         if (node == null) return;
                         var slotInfo = node.GetComponent<WorldBossRankingInfoSlot>();
                         if (slotInfo == null) return;
                         slotInfo.Init(GetPageIndexByButtonIndex(), (WorldBossRankingUserData)item);
                         node.SetActive(true);
                     }));
                rankingInfoTableView.ReLoad();
            }
        }

        List<WorldBossRankingUserData> GetRankingData(JObject _jsonData, bool isMine = false)
        {
            List<WorldBossRankingUserData> ret = new List<WorldBossRankingUserData>();

            var properties = _jsonData.Properties();
            foreach (JProperty it in properties)
            {
                var rankInfo = (JObject)it.Value;

                var rank = rankInfo["rank"].Value<int>();
                var userNo = isMine ? User.Instance.UserAccountData.UserNumber : rankInfo["user_no"].Value<long>();
                var nick = isMine ? User.Instance.UserData.UserNick : rankInfo["nick"].Value<string>();
                var userPortrait = isMine ? User.Instance.UserData.UserPortrait : rankInfo["icon"].Value<string>();
                var userLevel = isMine ? User.Instance.UserData.Level : rankInfo["level"].Value<int>();
                var userBattlePoint = rankInfo["combat_power"].Value<int>();
                uint highScore = rankInfo["high_score"].Value<uint>();
                string prefix_total = rankInfo["total_high"].Value<string>();
                string postfix_total = rankInfo["total_low"].Value<string>();
                string totalPoint = prefix_total == "0" ? postfix_total : prefix_total + postfix_total;

                var guildNo = 0;
                if (SBFunc.IsJTokenType(rankInfo["guild_no"], JTokenType.Integer))
                    guildNo = rankInfo["guild_no"].Value<int>();

                var guildName = "";
                if (guildNo > 0 && SBFunc.IsJTokenType(rankInfo["guild_name"], JTokenType.String))
                    guildName = rankInfo["guild_name"].Value<string>();

                var guildMarkNo = 0;
                if (guildNo > 0 && SBFunc.IsJTokenType(rankInfo["mark_no"], JTokenType.Integer))
                    guildMarkNo = rankInfo["mark_no"].Value<int>();

                var guildEmblemNo = 0;
                if (guildNo > 0 && SBFunc.IsJTokenType(rankInfo["emblem_no"], JTokenType.Integer))
                    guildEmblemNo = rankInfo["emblem_no"].Value<int>();

                PortraitEtcInfoData tempPortraitData = null;

                if(isMine)//userData 에서 가져옴 - 해당 api 들어올 때 push api 들어옴(portrait_update)
                    tempPortraitData = User.Instance.UserData.UserPortraitFrameInfo;
                else
                {
                    if (rankInfo.ContainsKey("portrait") && SBFunc.IsJObject(rankInfo["portrait"]))
                        tempPortraitData = new PortraitEtcInfoData(rankInfo["portrait"]);
                }

                WorldBossRankingUserData info = new WorldBossRankingUserData(userNo , nick, userPortrait, userLevel, userBattlePoint, highScore, totalPoint,rank, guildNo, guildName, guildMarkNo, guildEmblemNo, tempPortraitData);

                ret.Add(info);
            }

            return ret;
        }

        void SetRankingRewardTable()
        {
            if (rankingRewardTableView == null) return;

            var groupIndex = (int)GetPageIndexByButtonIndex();
            var rankingDataList = WorldBossRankRewardData.GetGroup(groupIndex);
            List<ITableData> rankRewardData = new List<ITableData>();
            List<WorldBossRankRewardData> tableList = new List<WorldBossRankRewardData>();

            var eventRankingList = WorldBossRankRewardData.GetGroup(tabIndex + 1);
            if (eventRankingList != null && eventRankingList.Count > 0)
                tableList = eventRankingList.ToList();

            foreach (var rankData in tableList)
            {
                var rewardGroup = rankData.REWARD_GROUP;
                var high = rankData.HIGHEST_RANK;

                int resultLow = 0;
                var low = rankData.LOWEST_RANK;//초과값
                if (low >= int.MaxValue)
                    resultLow = -1;
                else
                    resultLow = ((int)low - 1);

                rankRewardData.Add(new EventRankRewardInfoData(rankData.GROUP_ID, high, resultLow, rewardGroup));
            }

            rankingRewardTableView.SetDelegate(
                new TableViewDelegate(rankRewardData, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<WorldBossRankingRewardSlot>();
                    if (slotInfo == null) return;
                    var data = (EventRankRewardInfoData)item;
                    slotInfo.Init(data.Key, data.startRank, data.endRank, data.ItemGroupNo);
                }));
            rankingRewardTableView.ReLoad();
        }

        public void OnClickTabButton(int _index)
        {

            if (tabIndex == _index)
                return;

            Init(_index);
        }

        void RefreshButton()
        {
            if (tabButtonDaily == null || tabButtonWeekly == null || tabButtonAcc == null) return;

            tabButtonDaily.interactable = true;
            tabButtonWeekly.interactable = true;
            tabButtonAcc.interactable = true;

            switch (tabIndex)
            {
                case 0:
                    tabButtonDaily.interactable = false;
                    break;
                case 1:
                    tabButtonWeekly.interactable = false;
                    break;
                case 2:
                    tabButtonAcc.interactable = false;
                    break;
            }
        }
    }
}
