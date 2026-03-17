using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossRankRewardTable : TableBase<WorldBossRankRewardData, DBRaid_boss_rank_reward>
    {
        private Dictionary<int, List<WorldBossRankRewardData>> dic = null;//key : group , value list
        public override void Init()
        {
            base.Init();
            if (dic == null)
                dic = new Dictionary<int, List<WorldBossRankRewardData>>();
            else
                dic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (dic == null)
                dic = new Dictionary<int, List<WorldBossRankRewardData>>();
            else
                dic.Clear();
        }
        public override void Preload()
        {
            LoadAll();

            foreach (WorldBossRankRewardData data in datas.Values)
            {
                if (false == dic.ContainsKey(data.GROUP_ID))
                    dic.Add(data.GROUP_ID, new());

                dic[data.GROUP_ID].Add(data);
            }

            foreach (var cur in dic.Values)
            {
                cur.Sort(SortAscend);
            }
        }

        private int SortAscend(WorldBossRankRewardData a, WorldBossRankRewardData b)
        {
            if (a.HIGHEST_RANK > b.HIGHEST_RANK)
                return 1;
            else if (a.HIGHEST_RANK == b.HIGHEST_RANK)
                return 0;
            else
                return -1;
        }

        public List<WorldBossRankRewardData> GetGroup(int groupID)
        {
            if (dic is null)
                return null;

            if (false == dic.ContainsKey(groupID))
                return null;

            return dic[groupID];
        }
    }
}

