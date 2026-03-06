using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DailyRewardData : TableData<DBDaily_reward>
    {
        static private DailyRewardTable table = null;
        static public List<DailyRewardData> GetGroup(int group)
        {
            if (table is null)
                table = TableManager.GetTable<DailyRewardTable>();

            return table.GetGroup(group);
        }

        public virtual string KEY => Data.UNIQUE_KEY;
        public virtual int GROUP_ID => Data.TYPE;
        public virtual int DAY => Data.DAY;

        protected List<Asset> reward = null;
        public virtual List<Asset> REWARDS
        {
            get
            {
                if (reward == null)
                {
                    reward = new List<Asset>();
                    var postItems = PostRewardData.GetGroup(Data.NORMAL_REWARD_ID);
                    if (postItems is null && postItems.Count < 1)
                        return null;

                    foreach (var p in postItems)
                    {
                        reward.Add(p.Reward);
                    }
                }

                return reward;
            }
        }
        /// <summary>
        /// 참초 테이블 -> PostReward
        /// </summary>
        //public virtual int HOLDER_REWARD_ID => Data.HOLDER_REWARD_ID;

        public virtual eDailyRewardRarity RARITY => (eDailyRewardRarity)Data.RARITY;
    }

    public class ServerOptionDailyRewardData : DailyRewardData
    {
        int Day = -1;
        int Group = -1;
        private JObject option_data;
        public ServerOptionDailyRewardData(int day, int group, JObject data)
        {
            Day = day;
            Group = group;
            option_data = data;


            reward = new List<Asset>();
            reward.Add(new Asset(eGoodType.ITEM, data["reward_id"].Value<int>(), data["amount"].Value<int>()));
        }

        
        public override string KEY => "";
        public override int GROUP_ID => Group;
        public override int DAY => Day;        
        public override eDailyRewardRarity RARITY => eDailyRewardRarity.NORMAL;
    }
}