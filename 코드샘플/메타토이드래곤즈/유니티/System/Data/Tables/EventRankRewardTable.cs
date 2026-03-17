using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class EventRankRewardTable : TableBase<EventRankRewardData, DBEvent_rank_reward>
    {
        private Dictionary<int, List<EventRankRewardData>> dic = null;//key : group , value list
        public override void Init()
        {
            base.Init();
            if (dic == null)
                dic = new Dictionary<int, List<EventRankRewardData>>();
            else
                dic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (dic == null)
                dic = new Dictionary<int, List<EventRankRewardData>>();
            else
                dic.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();

            foreach (var cur in dic.Values)
            {
                cur.Sort(SortAscend);
            }
        }
        protected override bool Add(EventRankRewardData data)
        {
            if (base.Add(data))
            {
                if (false == dic.ContainsKey(data.GROUP_ID))
                    dic.Add(data.GROUP_ID, new());

                dic[data.GROUP_ID].Add(data);
                return true;
            }
            return false;
        }
        private int SortAscend(EventRankRewardData a, EventRankRewardData b)
        {
            if (a.HIGHEST_RANK > b.HIGHEST_RANK)
                return 1;
            else if (a.HIGHEST_RANK == b.HIGHEST_RANK)
                return 0;
            else
                return -1;
        }
        public List<EventRankRewardData> GetGroup(int groupID)
        {
            if (dic is null)
                return null;

            if (false == dic.ContainsKey(groupID))
                return null;

            return dic[groupID];
        }
    }
}
