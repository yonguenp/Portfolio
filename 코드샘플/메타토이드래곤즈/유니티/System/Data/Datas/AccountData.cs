using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class AccountData : TableData<DBAccount_exp>
    {
        static private AccountTable table = null;
        static public AccountData Get(string key)
        {
            if(table == null)
                table = TableManager.GetTable<AccountTable>();

            return table.Get(key);
        }

        static public AccountData GetLevel(int level)
        {
            if(table == null)
                table = TableManager.GetTable<AccountTable>();

            return table.GetByLevel(level);
        }

        static public int GetLevelByExp(int exp)
        {
            if (table == null)
                table = TableManager.GetTable<AccountTable>();

            return table.GetLevelByExp(exp);
        }
        /// <summary>
        /// 보상 받을 수 있는 전체 데이터
        /// </summary>
        /// <returns></returns>
        static public List<AccountData> GetTotalRewardList()
        {
            if (table == null)
                table = TableManager.GetTable<AccountTable>();

            return table.GetTotalRewardList();
        }

        public int KEY => Int(Data.UNIQUE_KEY);
        public int LEVEL => Data.LEVEL;
        public int EXP => Data.EXP;
        public int TOTAL_EXP => Data.TOTAL_EXP;
        public int MAX_STAMINA => Data.MAX_STAMINA;
        public int NORMAL_REWARD {
            get
            {
                switch(SBGameManager.CurServerTag)
                {
                    case 3:
                        return Data.LUNA_NORMAL;
                    default:
                        return Data.NORMAL_REWARD;
                }
                
            }
        }
        public int SPECIAL_REWARD
        {
            get
            {
                switch (SBGameManager.CurServerTag)
                {
                    case 3:
                        return Data.LUNA_SPECIAL;
                    default:
                        return Data.SPECIAL_REWARD;
                }

            }
        }

        public Asset NormalReward
        {
            get
            {
                if (NORMAL_REWARD != 0)
                    return ItemGroupData.Get(NORMAL_REWARD)[0].Reward;
                return null;
            }
        }
        public Asset SpecialReward
        {
            get
            {
                if (SPECIAL_REWARD == 0) return null;
                return ItemGroupData.Get(SPECIAL_REWARD)[0].Reward;
            }
        }
    }
}