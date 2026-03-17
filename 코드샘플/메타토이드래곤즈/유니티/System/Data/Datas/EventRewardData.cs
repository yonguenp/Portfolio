using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EventRewardData : TableData<DBEvent_reward>
    {
        static private EventRewardTable table = null;
        static public List<EventRewardData> GetGroup(int group)
        {
            if (table == null)
                table = TableManager.GetTable<EventRewardTable>();

            return table.GetGroup(group);
        }
        static public EventRewardData Get(int group, int seq)
        {
            var datas = GetGroup(group);
            if (datas == null)
                return null;

            return datas.Find((element)=> element.SEQ == seq);
        }
        public string KEY => Data.UNIQUE_KEY;
        public int GROUP_ID => Data.GROUP;
        /// <summary>
        /// event Type 에 대한 정의
        /// </summary>
        public int TYPE => Data.TYPE;
        public int SEQ => Data.SEQ;
        public int REWARD_ID => Data.REWARD_ID;
        public int RARITY => Data.RARITY;
    }
}