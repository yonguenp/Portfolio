using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildRankingRewardPopup : Popup<GuildRankRewardPopupData>
    {
        [SerializeField]
        Text Title = null;
        [SerializeField]
        Text Desc = null;

        [SerializeField]
        Button[] tabs = null;
        [SerializeField]
        TableView rankingRewardTableView = null;
        [SerializeField]
        private List<GuildRankingRewardObject> guildRewardObjList = null;
        [SerializeField]
        private List<GuildRankingRewardObject> userRewardObjList = null;

        private List<GuildRankingRewardObject> rewardObjList = null;

        bool isInit = false;

        List<GuildRankRewardData> rewardData = null;
        public override void InitUI()
        {
            if (isInit == false)
            {
                //rankingRewardTableView.OnStart();
                isInit = true;
            }
            rewardData = GuildRankRewardData.GetByGroup(Data.group);

            switch (Data.group)
            {
                case eGuildRankRewardGroup.GuildRank:
                    Title.text = StringData.GetStringByStrKey("길드랭킹");
                    break;
                case eGuildRankRewardGroup.UserRank:
                    Title.text = StringData.GetStringByStrKey("길드개인랭킹");
                    break;
            }

            OnClickTab(0);
        }

        void SetAllTabOff()
        {
            foreach (var tab in tabs)
            {
                tab.interactable = true;
            }
        }

        public void OnClickTab(int index)
        {
            SetAllTabOff();
            if (index < tabs.Length)
            {
                tabs[index].interactable = false;
            }

            Desc.text = "";
            switch (Data.group)
            {
                case eGuildRankRewardGroup.GuildRank:
                    foreach (var list in userRewardObjList)
                    {
                        list.gameObject.SetActive(false);
                    }
                    foreach (var list in guildRewardObjList)
                    {
                        list.gameObject.SetActive(true);
                    }
                    rewardObjList = guildRewardObjList;
                    switch ((eGuildRankType)index)
                    {
                        case eGuildRankType.WeeklyRanking:
                            Desc.text = StringData.GetStringByStrKey("길드주간랭킹보상안내");
                            break;
                        case eGuildRankType.MonthlyRanking:
                            Desc.text = StringData.GetStringByStrKey("길드월간랭킹보상안내");
                            break;
                    }
                    break;
                case eGuildRankRewardGroup.UserRank:
                    foreach (var list in guildRewardObjList)
                    {
                        list.gameObject.SetActive(false);
                    }
                    foreach (var list in userRewardObjList)
                    {
                        list.gameObject.SetActive(true);
                    }
                    rewardObjList = userRewardObjList;
                    switch ((eGuildRankType)index)
                    {
                        case eGuildRankType.WeeklyRanking:
                            Desc.text = StringData.GetStringByStrKey("길드개인주간랭킹보상안내");
                            break;
                        case eGuildRankType.MonthlyRanking:
                            Desc.text = StringData.GetStringByStrKey("길드개인월간랭킹보상안내");
                            break;
                    }
                    break;
            }

            Desc.gameObject.SetActive(!string.IsNullOrEmpty(Desc.text));

            //DrawScrollRect(index);
            DrawObj(index);

        }

        void DrawObj(int index)
        {
            for(int i = 0; i < rewardData.Count; i++)
            {
                if (rewardObjList[i] != null)
                {
                    int itemGroupKey = 0;
                    switch ((eGuildRankType)index)
                    {
                        case eGuildRankType.SumRanking:
                            itemGroupKey = rewardData[i].ACCUMULATE_REWARD;
                            break;
                        case eGuildRankType.WeeklyRanking:
                            itemGroupKey = rewardData[i].WEEK_REWARD;
                            break;
                        case eGuildRankType.MonthlyRanking:
                            itemGroupKey = rewardData[i].MONTH_REWARD;
                            break;
                    }

                    int resultLow = 0;
                    var low = rewardData[i].LOWEST_RANK;//초과값
                    if (low >= uint.MaxValue)
                        resultLow = -1;
                    else
                        resultLow = ((int)rewardData[i].LOWEST_RANK - 1);

                    rewardObjList[i].Init(index, (int)Data.group, rewardData[i].HIGHEST_RANK, resultLow, itemGroupKey);
                }
            }
        }

        void DrawScrollRect(int index)
        {
            // To do index 에 따라 세팅하기
            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (rewardData != null && rewardData.Count > 0)
            {
                for (var i = 0; i < rewardData.Count; i++)
                {
                    var data = rewardData[i];
                    if (data == null)
                    {
                        continue;
                    }

                    tableViewItemList.Add(data);
                }
            }
            rankingRewardTableView.SetDelegate(
            new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                if (node == null) return;
                var slotInfo = node.GetComponent<GuildRankingRewardObject>();
                if (slotInfo == null) return;
                var data = (GuildRankRewardData)item;
                string str = string.Empty;
                int itemGroupKey = 0;
                switch ((eGuildRankType)index)
                {
                    case eGuildRankType.SumRanking:
                        // 휘장 마크가 들어갈 예정이라 새로운 타입이 생긴다고 생각해야 함
                        //str = data.ACCUMULATE_REWARD.ToString();
                        itemGroupKey = data.ACCUMULATE_REWARD;
                        break;
                    case eGuildRankType.WeeklyRanking:
                        //var weeklyItems = ItemGroupData.Get(data.WEEK_REWARD);
                        //foreach (var iteminfo in weeklyItems)
                        //{
                        //    var reward = iteminfo.Reward;
                        //    int amount = reward.Amount;
                        //    string itemName = "";
                        //    if (reward.GoodType == eGoodType.ITEM)
                        //    {
                        //        itemName = ItemBaseData.Get(reward.ItemNo).NAME;
                        //    }
                        //    else if (reward.GoodType == eGoodType.GUILD_POINT)
                        //    {
                        //        itemName = "임시 길드포인트";
                        //    }

                        //    str += string.Format("{0}({1}), ", itemName, StringData.GetStringFormatByStrKey("production_quantity", amount));
                        //}
                        //str.TrimEnd(',');
                        itemGroupKey = data.WEEK_REWARD;
                        break;
                    case eGuildRankType.MonthlyRanking:
                        //var monthlyItems = ItemGroupData.Get(data.MONTH_REWARD);
                        //foreach (var iteminfo in monthlyItems)
                        //{
                        //    var reward = iteminfo.Reward;
                        //    int amount = reward.Amount;
                        //    string itemName = "";
                        //    if (reward.GoodType == eGoodType.ITEM)
                        //    {
                        //        itemName = ItemBaseData.Get(reward.ItemNo).NAME;
                        //    }
                        //    else if (reward.GoodType == eGoodType.GUILD_POINT)
                        //    {
                        //        itemName = "임시 길드포인트";
                        //    }
                        //    str += string.Format("{0}({1}), ", itemName, StringData.GetStringFormatByStrKey("production_quantity", amount));
                        //}
                        //str.TrimEnd(',');
                        itemGroupKey = data.MONTH_REWARD;
                        break;
                }

                int resultLow = 0;
                var low = data.LOWEST_RANK;//초과값
                if (low >= uint.MaxValue)
                    resultLow = -1;
                else
                    resultLow = ((int)data.LOWEST_RANK - 1);

                slotInfo.Init(index, (int)Data.group,data.HIGHEST_RANK, resultLow, itemGroupKey);
                node.SetActive(true);
            }));

            rankingRewardTableView.ReLoad(true);
        }
    }

}
