using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildRankingLayer : TabLayer
    {
        [Header("Column")]
        [SerializeField]
        GameObject rankColumnObj;
        [SerializeField]
        GameObject rankingLvTextObj;
        [SerializeField]
        Text rankingExpText;
        [SerializeField]
        Text rankingUnionName;
        [SerializeField]
        GameObject rankingUserAmount;

        [Header("Left")]
        [SerializeField]
        GuildMarkObject myGuildMark;
        [SerializeField]
        Text myGuildNameText;
        [SerializeField]
        Text myGuildLvText;
        [SerializeField]
        Text myGuildRankText;

        [Header("right")]
        [SerializeField]
        Button[] tabs;
        [SerializeField]
        TableView rankTableView;
        [SerializeField]
        TableView rankWeekMonthTableView;
        [SerializeField]
        TableView rankUnifiedTableView;

        bool isInit = false;
        List<GuildRankData> rankList = new List<GuildRankData>();
        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);
            if(isInit == false)
            {
                isInit = true;
                rankTableView.OnStart();
                rankWeekMonthTableView.OnStart();
                rankUnifiedTableView.OnStart();
            }
            rankList.Clear();
            WWWForm form = new WWWForm();
            form.AddField("gno", GuildManager.Instance.GuildID);
            form.AddField("page", (int)eGuildRankingPage.Guild);

            GuildManager.Instance.ReqGuildRanking(() => { OnClickTab(0); });
            SetMyGuildInfo();
            
        }

        void SetMyGuildInfo()
        {
            var guildData = GuildManager.Instance.MyGuildInfo;
            if (guildData != null)
            {
                myGuildMark.SetGuildMark(guildData.GetGuildEmblem(), guildData.GetGuildMark());
                myGuildNameText.text = guildData.GetGuildName();
                myGuildLvText.text = StringData.GetStringFormatByStrKey("user_info_lv_02", guildData.GetGuildLevel());

                myGuildRankText.text = SBFunc.GetRankText(guildData.GetGuildRank());

            }
        }

        public void OnClickTab(int idx)
        {
            foreach(var tab in tabs)
            {
                if (tab != null)
                    tab.interactable = true;
            }
            tabs[idx].interactable = false;

            DrawScrollViewByType((eGuildRankType)idx);
        }

        public void OnClickRankingInfoBtn()
        {
            PopupManager.OpenPopup<GuildRankingRewardPopup>(new GuildRankRewardPopupData(eGuildRankRewardGroup.GuildRank));
        }
        void DrawScrollViewByType(eGuildRankType type)
        {
            List<ITableData> list = new List<ITableData>();
            rankingLvTextObj.SetActive(false);
            rankTableView.gameObject.SetActive(false);
            rankWeekMonthTableView.gameObject.SetActive(false);
            rankUnifiedTableView.gameObject.SetActive(false);
            rankingUserAmount.SetActive(true);
            switch (type)
            {
                case eGuildRankType.SumRanking:
                    rankingUnionName.text = StringData.GetStringByStrKey("guild_errorcode_25");
                    rankingExpText.text = StringData.GetStringByStrKey("guild_desc:65");
                    rankingLvTextObj.SetActive(true);
                    rankTableView.gameObject.SetActive(true);
                    rankList = GuildManager.Instance.GuildRankList.OrderByDescending(rank => { 
                        if (rank.Rank > 0) 
                            return -rank.Rank; 
                        return int.MinValue; 
                    }).ToList();
                    foreach (var rankData in rankList)
                    {
                        list.Add(rankData);
                    }
                    rankTableView.SetDelegate(new TableViewDelegate(list, (GameObject itemNode, ITableData item) => {
                        if (itemNode == null || item == null)
                        {
                            return;
                        }
                        var rankObj = itemNode.GetComponent<GuildRankObj>();
                        if (rankObj == null)
                        {
                            return;
                        }
                        var rankData = (GuildRankData)item;
                        rankObj.Init(rankData, type);
                    }));
                    rankTableView.ReLoad();
                    break;
                case eGuildRankType.WeeklyRanking:
                    rankingUnionName.text = StringData.GetStringByStrKey("guild_errorcode_25");
                    rankingExpText.text = StringData.GetStringByStrKey("guild_desc:125");
                    rankWeekMonthTableView.gameObject.SetActive(true);
                    rankList = GuildManager.Instance.GuildRankList.OrderByDescending(rank => {
                        if (rank.WeeklyRank > 0)
                            return -rank.WeeklyRank;
                        return int.MinValue;
                    }).ToList();
                    foreach (var rankData in rankList)
                    {
                        list.Add(rankData);
                    }
                    rankWeekMonthTableView.SetDelegate(new TableViewDelegate(list, (GameObject itemNode, ITableData item) => {
                        if (itemNode == null || item == null)
                        {
                            return;
                        }
                        var rankObj = itemNode.GetComponent<GuildRankObj>();
                        if (rankObj == null)
                        {
                            return;
                        }
                        var rankData = (GuildRankData)item;
                        rankObj.Init(rankData, type);
                    }));
                    rankWeekMonthTableView.ReLoad();
                    break;
                case eGuildRankType.MonthlyRanking:
                    rankingUnionName.text = StringData.GetStringByStrKey("guild_errorcode_25");
                    rankingExpText.text = StringData.GetStringByStrKey("guild_desc:126");
                    rankWeekMonthTableView.gameObject.SetActive(true);
                    rankList = GuildManager.Instance.GuildRankList.OrderByDescending(rank => {
                        if (rank.MonthlyRank > 0)
                            return -rank.MonthlyRank;
                        return int.MinValue;
                    }).ToList();
                    foreach (var rankData in rankList)
                    {
                        list.Add(rankData);
                    }
                    rankWeekMonthTableView.SetDelegate(new TableViewDelegate(list, (GameObject itemNode, ITableData item) => {
                        if (itemNode == null || item == null)
                        {
                            return;
                        }
                        var rankObj = itemNode.GetComponent<GuildRankObj>();
                        if (rankObj == null)
                        {
                            return;
                        }
                        var rankData = (GuildRankData)item;
                        rankObj.Init(rankData, type);
                    }));
                    rankWeekMonthTableView.ReLoad();
                    break;
                case eGuildRankType.UnifiedRanking:
                    rankingUserAmount.SetActive(false);
                    GuildManager.Instance.ReqUnifiedGuildRanking((() => {
                        rankingExpText.text = StringData.GetStringByStrKey("guild_desc:125");
                        rankingUnionName.text = StringData.GetStringByStrKey("guild_desc:22");
                        rankUnifiedTableView.gameObject.SetActive(true);
                        rankList = GuildManager.Instance.UnifiedGuildRankList.Where((System.Func<GuildRankData, bool>)(g => g.Rank > 0)).OrderByDescending((System.Func<GuildRankData, int>)(rank => {
                            if (rank.Rank > 0)
                                return (int)-rank.Rank;
                            return int.MinValue;
                        })).ToList();
                        foreach (var rankData in rankList)
                        {
                            list.Add(rankData);
                        }
                        rankUnifiedTableView.SetDelegate(new TableViewDelegate(list, (GameObject itemNode, ITableData item) => {
                            if (itemNode == null || item == null)
                            {
                                return;
                            }
                            var rankObj = itemNode.GetComponent<GuildRankObj>();
                            if (rankObj == null)
                            {
                                return;
                            }
                            var rankData = (GuildRankData)item;
                            rankObj.Init(rankData, type);
                        }));
                        rankUnifiedTableView.ReLoad();
                    }));
                    break;
            }
            
            
        }
    }

}
