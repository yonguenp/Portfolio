using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class StageBaseData : TableData<DBStage_base>
    {        
        static private StageTable table = null;
        static public StageBaseData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<StageTable>();

            return table.Get(key);
        }

        static public StageBaseData Get(int key)
        {
            return Get(key.ToString());
        }

        static public List<StageBaseData> GetByAdventureWorld(int world)
        {
            return GetByWorld(world, (StageDifficult)CacheUserData.GetInt("adventure_difficult", 1));
        }

        static public List<StageBaseData> GetByWorld(int world, StageDifficult difficult = StageDifficult.NORMAL)
        {
            if (table == null)
                table = TableManager.GetTable<StageTable>();

            return table.GetByWorld(world, difficult);
        }

        static public StageBaseData GetByAdventureWorldStage(int world, int stage)
        {
            return GetByWorldStage(world, stage, (StageDifficult)CacheUserData.GetInt("adventure_difficult", 1));
        }

        static public StageBaseData GetByWorldStage(int world, int stage, StageDifficult difficult = StageDifficult.NORMAL)
        {
            if (table == null)
                table = TableManager.GetTable<StageTable>();

            return table.GetByWorldStage(world, stage, difficult);
        }

        static public List<StageBaseData> GetDailyStage(int day, int stage)
        {
            if (table == null)
                table = TableManager.GetTable<StageTable>();

            List<StageBaseData> ret = new List<StageBaseData>();
            foreach (var data in table.GetDailyStage())
            {
                if (data.WORLD == day && data.STAGE == stage)
                    ret.Add(data);
            }

            return ret;
        }

        public int KEY => Int(Data.UNIQUE_KEY);
        public eStageType TYPE => (eStageType)Data.TYPE;
        public StageDifficult DIFFICULT => (StageDifficult)Data.DIFFICULT;
        public int WORLD => Data.WORLD;
        public int STAGE => Data.STAGE;
        public int _NAME => Data._NAME;
        public string IMAGE => Data.IMAGE;
        public int PROPERTY_PAGE => Data.PROPERTY_PAGE;
        public string COST_TYPE => Data.COST_TYPE;
        public int COST_VALUE => Data.COST_VALUE;
        public int CLEAR_COUNT => Data.CLEAR_COUNT;
        public int TIME => Data.TIME;
        public int SPAWN => Data.SPAWN;
        public int REWARD_ACCOUNT_EXP => Data.REWARD_ACCOUNT_EXP;
        public int REWARD_CHAR_EXP => (int)(Data.REWARD_CHAR_EXP * ServerOptionData.GetFloat("dragon_exp", 1.0f));
        public int REWARD_GOLD => Data.REWARD_GOLD;
        public List<Asset> REWARD_ITEMS
        {
            get
            {
                List<Asset> ret = new List<Asset>();
                float rate = 1.0f;
                switch (TYPE)
                {
                    case eStageType.ADVENTURE:
                        rate = ServerOptionData.GetFloat("adventure_reward", 1.0f);
                        break;
                    case eStageType.DAILY_DUNGEON:
                        rate = ServerOptionData.GetFloat("daily_reward", 1.0f);
                        break;
                    case eStageType.WORLD_BOSS:
                        rate = ServerOptionData.GetFloat("raid_reward", 1.0f);
                        break;
                }

                var groupData = ItemGroupData.Get(Data.REWARD_ITEM);
                if (groupData != null)
                {
                    foreach (var item in groupData)
                    {
                        if (item == null)
                            continue;

                        if (TYPE == eStageType.DAILY_DUNGEON)
                        {
                            var baseData = ItemBaseData.Get(item.Reward.ItemNo);
                            if (baseData != null && baseData.KIND != eItemKind.SKILL_UP)
                            {
                                ret.Add(new Asset(item.Reward.GoodType, item.Reward.ItemNo, (int)(item.Reward.Amount)));
                                continue;
                            }
                        }

                        ret.Add(new Asset(item.Reward.GoodType, item.Reward.ItemNo, (int)(item.Reward.Amount * rate)));
                    }
                }

                return ret;
            }
        }

        public int HOLDER_REWARD_ITEM => Data.HOLDER_REWARD_ITEM;
        public int REWARD_ITEM_COUNT => Data.REWARD_ITEM_COUNT;

        private List<int> rewards = null;
        public List<int> REWARDS
        {
            get
            {
                if(rewards == null)
                {
                    rewards = new List<int>();
                    rewards.Add(Data.FIRST_REWARD_STAR_1);
                    rewards.Add(Data.FIRST_REWARD_STAR_2);
                    rewards.Add(Data.FIRST_REWARD_STAR_3);
                }
                return rewards;
            }
        }
        public string CHARGE_COST_TYPE => Data.CHARGE_COST_TYPE;
        public int CHARGE_COST_VALUE => Data.CHARGE_COST_VALUE;
        public int CHARGE_COUNT => Data.CHARGE_COUNT;
        public int CHARGE_MAX => Data.CHARGE_MAX;
        public int UNLOCK_MISSION => Data.UNLOCK_MISSION;

        public List<ItemGroupData> GetStarRewards(int star)
        {
            if(star < GetStarRewardCount())
                return ItemGroupData.Get(REWARDS[star]);
            
            return new List<ItemGroupData>();
        }

        public int GetStarRewardCount()
        {
            return REWARDS.Count;
        }
    }
}