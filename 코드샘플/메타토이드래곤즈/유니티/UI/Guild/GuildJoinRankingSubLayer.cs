using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class GuildJoinRankingSubLayer : SubLayer
    {
        [SerializeField]
        Button[] tabs = null;

        [SerializeField]
        GameObject rankingColumnObj = null;
        [SerializeField]
        GameObject rankingLvTextObj = null;
        [SerializeField]
        Text rankingExpText = null;

        [SerializeField]
        TableView rankTableView = null;
        [SerializeField]
        TableView rankTableViewWeekMonth = null;

        public int CurTabIndex { get; private set; } = 0;

        bool isInit = false;

        List<GuildRankData> rankList = new List<GuildRankData>();
        public override void Init()
        {
            base.Init();
            if(isInit == false)
            {
                rankTableView.OnStart();
                rankTableViewWeekMonth.OnStart();
                isInit = true;
            }

            GuildManager.Instance.ReqGuildRanking(() => { OnClickTab(CurTabIndex); });            
        }


        public void OnClickTab(int tabIndex)
        {
            foreach (var tab in tabs)
            {
                tab.interactable = true;
            }
            if (tabs.Length <= tabIndex)
                return;
            tabs[tabIndex].interactable = false;

            var type = (eGuildRankType)tabIndex;

            List<ITableData> list = new List<ITableData>();
            rankingLvTextObj.SetActive(false);
            rankTableView.gameObject.SetActive(false);
            rankTableViewWeekMonth.gameObject.SetActive(false);
            switch (type)
            {
                case eGuildRankType.SumRanking:
                    rankTableView.gameObject.SetActive(true);
                    rankingExpText.text = StringData.GetStringByStrKey("guild_desc:65");
                    rankingLvTextObj.SetActive(true);
                    rankList = GuildManager.Instance.GuildRankList.OrderByDescending(rank => {
                        if (rank.Rank > 0)
                            return -rank.Rank;
                        return int.MinValue;
                        }).ToList();

                    foreach (var rankData in rankList)
                    {
                        if (list.Count > 100)
                            break;

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
                    rankTableViewWeekMonth.gameObject.SetActive(true);
                    rankingExpText.text = StringData.GetStringByStrKey("guild_desc:125");
                    rankList = GuildManager.Instance.GuildRankList.OrderByDescending(rank => {
                        if (rank.WeeklyRank > 0)
                            return -rank.WeeklyRank;
                        return int.MinValue;
                    }).ToList();
                    foreach (var rankData in rankList)
                    {
                        if (list.Count > 100)
                            break;

                        list.Add(rankData);
                    }
                    rankTableViewWeekMonth.SetDelegate(new TableViewDelegate(list, (GameObject itemNode, ITableData item) => {
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
                    rankTableViewWeekMonth.ReLoad();
                    break;
                case eGuildRankType.MonthlyRanking:
                    rankTableViewWeekMonth.gameObject.SetActive(true);
                    rankingExpText.text = StringData.GetStringByStrKey("guild_desc:126");
                    rankList = GuildManager.Instance.GuildRankList.OrderByDescending(rank => {
                        if (rank.MonthlyRank > 0)
                            return -rank.MonthlyRank;
                        return int.MinValue;
                    }).ToList();
                    foreach (var rankData in rankList)
                    {
                        if (list.Count > 100)
                            break;

                        list.Add(rankData);
                    }
                    rankTableViewWeekMonth.SetDelegate(new TableViewDelegate(list, (GameObject itemNode, ITableData item) => {
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
                    rankTableViewWeekMonth.ReLoad();
                    break;
            }
            
        }
    }
}