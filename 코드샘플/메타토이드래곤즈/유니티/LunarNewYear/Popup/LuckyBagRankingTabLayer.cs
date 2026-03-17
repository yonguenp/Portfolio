using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class LuckyBagRankingTabLayer : EventRankingTabLayer
    {
        const int RANK_PAGE_FACTOR = 3;//기존 event_rank_reward 테이블의 그룹값을 맞추는 factor

        EventLuckyBagBaseData eventData = null;


        protected override void SetEventData()
        {
            if (eventData == null)
                eventData = LuckyBagEventPopup.GetEventData();
        }
        protected override void ShowRankPageBySubLayerIndex()
        {
            switch (SubLayerIndex)
            {
                case 0:  // 강화랭킹
                    ShowPageProcess(false);
                    break;
                case 1: // 보스랭킹
                    ShowPageProcess(false);
                    break;
                case 2: // pvp 랭킹
                    ShowPageProcess(false);
                    break;
            }
        }
        protected override void RequestRankingData(int _page)//4,5,6
        {
            if (eventData == null)
                return;

            _page += RANK_PAGE_FACTOR;

            eventData.RequestToServer(eLuckyBagEventState.REQUEST_RANKING, (jsonData) =>
            {
                RefreshRankingScrollview(jsonData);
            },null, _page);
        }

        protected override void SetRankRewardTable()
        {
            List<ITableData> rankRewardData = new List<ITableData>();
            List<EventRankRewardData> tableList = new List<EventRankRewardData>();

            var eventRankingList = EventRankRewardData.GetGroup(SubLayerIndex + 1 + RANK_PAGE_FACTOR);
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
            RankRewardTableView.ReLoad();
        }
    }
}
