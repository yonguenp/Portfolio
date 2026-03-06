using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EventRankRewardData : TableData<DBEvent_rank_reward>
    {
        static private EventRankRewardTable table = null;
        static public List<EventRankRewardData> GetGroup(int group)
        {
            if (table == null)
                table = TableManager.GetTable<EventRankRewardTable>();

            return table.GetGroup(group);
        }

        public string KEY => Data.UNIQUE_KEY;
        public int GROUP_ID => Data.GROUP;
        public int HIGHEST_RANK => Data.HIGHEST_RANK;
        public uint LOWEST_RANK => Data.LOWEST_RANK;
        public int REWARD_GROUP => Data.REWARD_GROUP;
    }
}