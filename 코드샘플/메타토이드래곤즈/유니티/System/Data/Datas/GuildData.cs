using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace SandboxNetwork {

    public class GuildExpData : TableData<DBGuild_exp>
    {
        static private GuildExpTable table = null;
        static public GuildExpData Get(string key)
        {
            if (table == null)
                table = new GuildExpTable();
            return table.Get(key);
        }

        public string KEY => Data.UNIQUE_KEY;
        public int LEVEL => Data.LEVEL;
        public int EXP => Data.EXP;
        public int TOTAL_EXP => Data.TOTAL_EXP;
        public string REWARD_STAT_TYPE => Data.REWARD_STAT_TYPE;
        public int VALUE_TYPE => Data.VALUE_TYPE;
        public float REWARD_STAT_VALUE => Data.REWARD_STAT_VALUE;

        public eStatusType STAT_TYPE => SBFunc.ConvertStatusType(Data.REWARD_STAT_TYPE);
        public eStatusValueType STAT_VALUE_TYPE => (eStatusValueType)Data.VALUE_TYPE;

        static public List<GuildExpData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<GuildExpTable>();
            return table.GetAllList();
        }
        static public Dictionary<string, float> GetStatsByLv(int lv)
        {
            if (table == null)
                table = TableManager.GetTable<GuildExpTable>();
            var data  = table.GetAllList();
            Dictionary<string, float> ret = new Dictionary<string, float>();
            foreach (var item in data)
            {
                if (item.LEVEL <= lv)
                {
                    string key = StatTypeData.GetDescStringByStatType(item.REWARD_STAT_TYPE, false);

                    if (item.VALUE_TYPE == 1) // 퍼센트 표기
                        key = StatTypeData.GetDescStringByStatType(item.REWARD_STAT_TYPE, true);

                    if (!ret.ContainsKey(key))
                        ret.Add(key, 0);

                    ret[key] += item.REWARD_STAT_VALUE;
                }
            }
            

            return ret;
        }

        static public GuildExpData GetDataByLv(int lv)
        {
            if (table == null)
                table = TableManager.GetTable<GuildExpTable>();

            return table.GetAllList().Find(element => element.LEVEL == lv);
        }

        static public int GetLvByExp(int exp)
        {
            if (table == null)
                table = TableManager.GetTable<GuildExpTable>();
            var dataList = table.GetAllList().ToList();
           
            foreach(var data in dataList.OrderByDescending(data => data.TOTAL_EXP))
            {
                if (data.TOTAL_EXP <= exp)
                    return data.LEVEL;
            }
            return 1;
        }
    }

    public class GuildDonationData : TableData<DBGuild_donation>
    {
        static private GuildDonationTable table = null;
        static public GuildDonationData Get(string key)
        {
            if (table == null)
                table = new GuildDonationTable();
            return table.Get(key);
        }

        public string KEY => Data.UNIQUE_KEY;
        public string NEED_TYPE => Data.NEED_TYPE;
        public int NEED_NUM => Data.NEED_NUM;
        public int REWARD_ACCOUNT_EXP => Data.REWARD_ACCOUNT_EXP;
        public int REWARD_GUILD_EXP => Data.REWARD_GUILD_EXP;
        public int REWARD_GUILD_POINT => Data.REWARD_GUILD_POINT;

        static public List<GuildDonationData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<GuildDonationTable>();

            return table.GetAllList();
        }
    }

    public class GuildRankRewardData : TableData<DBGuild_rank_reward>
    {
        static private GuildRankRewardTable table = null;
        static public GuildRankRewardData Get(string key)
        {
            if(table == null)
                table = TableManager.GetTable<GuildRankRewardTable>();

            return table.Get(key);
        }
        public int GROUP =>  Data.GROUP;
        public int HIGHEST_RANK => Data.HIGHEST_RANK;
        public uint LOWEST_RANK => Data.LOWEST_RANK;
        public int ACCUMULATE_REWARD => Data.REWARD_GROUP_3;
        public int WEEK_REWARD => Data.WEEKLY_REWARD_GROUP;
        public int MONTH_REWARD => Data.MONTHLY_REWARD_GROUP;

        static public List<GuildRankRewardData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<GuildRankRewardTable>();
            return table.GetAllList();
        }
        static public List<GuildRankRewardData> GetByGroup(eGuildRankRewardGroup group)
        {
            if (table == null)
                table = TableManager.GetTable<GuildRankRewardTable>();
            return table.GetByGroup(group);
        }

        static public GuildRankRewardData GetByRankGroup(int rank, eGuildRankRewardGroup group)
        {
            if (table == null)
                table = TableManager.GetTable<GuildRankRewardTable>();
            return table.GetByRankGroup(rank, group);
        }

    }

    public class GuildResourceData : TableData<DBGuild_resource>
    {
        static private GuildResourceTable table = null;
        static public GuildResourceData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<GuildResourceTable>();

            return table.Get(key);
        }
        static public List<GuildResourceData> GetEmblems()
        {
            if (table == null)
                table = TableManager.GetTable<GuildResourceTable>();
            return table.GetEmblems();
        }

        static public List<GuildResourceData> GetMarks()
        {
            if (table == null)
                table = TableManager.GetTable<GuildResourceTable>();
            return table.GetMarks();
        }

        static public List<GuildResourceData> GetFlags()
        {
            if (table == null)
                table = TableManager.GetTable<GuildResourceTable>();
            return table.GetFlags();
        }
        static public List<GuildResourceData> GetGuildPick()
        {
            if (table == null)
                table = TableManager.GetTable<GuildResourceTable>();
            return table.GetGuildPick();
        }
        static public List<GuildResourceData> GetUserPick()
        {
            if (table == null)
                table = TableManager.GetTable<GuildResourceTable>();
            return table.GetUserPick();
        }

        static private Sprite default_emblem = null;
        static private Sprite default_mark = null;
        static private Sprite default_flag = null;
        static private Sprite default_guildPick = null;
        static private Sprite default_userPick = null;
        static public Sprite DEFAULT_EMBLEM 
        { 
            get {
                if (default_emblem == null)
                    default_emblem = ResourceManager.GetResource<Sprite>(eResourcePath.GuildResourcePath, "default_emblem");
                return default_emblem;
            } 
        }
        static public Sprite DEFAULT_MARK
        {
            get
            {
                if (default_mark == null)
                    default_mark = ResourceManager.GetResource<Sprite>(eResourcePath.GuildResourcePath, "default_mark");
                return default_mark;
            }
        }
        static public Sprite DEFAULT_FLAG
        {
            get
            {
                if (default_flag == null)
                    default_flag = ResourceManager.GetResource<Sprite>(eResourcePath.GuildResourcePath, "default_flag");
                return default_flag;
            }
        }
        static public Sprite DEFAULT_GUILD_PICK
        {
            get
            {
                if (default_guildPick == null)
                    default_guildPick = ResourceManager.GetResource<Sprite>(eResourcePath.GuildResourcePath, "DEFAULT_GUILD_PICK");
                return default_guildPick;
            }
        }
        static public Sprite DEFAULT_USER_PICK
        {
            get
            {
                if (default_userPick == null)
                    default_userPick = ResourceManager.GetResource<Sprite>(eResourcePath.GuildResourcePath, "DEFAULT_USER_PICK");
                return default_userPick;
            }
        }
        public enum RESOURCE_TYPE { NONE, EMBLEM, MARK, FLAG, GUILD_PICK, USER_PICK }
        public int KEY => Int(Data.UNIQUE_KEY);

        public RESOURCE_TYPE TYPE => (RESOURCE_TYPE)Data.TYPE;
        public string FILE_NAME => Data.FILE_NAME;

        private Sprite resource = null;
        public Sprite RESOURCE { 
            get { 
                if(resource == null)
                {
                    resource = ResourceManager.GetResource<Sprite>(eResourcePath.GuildResourcePath, FILE_NAME);
                }

                if(resource == null)
                {
                    switch(TYPE)
                    {
                        case RESOURCE_TYPE.EMBLEM:
                            return DEFAULT_EMBLEM;
                        case RESOURCE_TYPE.MARK:
                            return DEFAULT_MARK;
                        case RESOURCE_TYPE.FLAG:
                            return DEFAULT_FLAG;
                    }
                }

                return resource;
            } 
        }
    }
}