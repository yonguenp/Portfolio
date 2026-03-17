using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork {
    public class GuildExpTable : TableBase<GuildExpData, DBGuild_exp>
    {
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
    }

    public class GuildDonationTable : TableBase<GuildDonationData, DBGuild_donation>
    {
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
    }

    public class GuildRankRewardTable : TableBase<GuildRankRewardData, DBGuild_rank_reward>
    {
        private Dictionary<int, List<GuildRankRewardData>> groupDic = null;
        public override void Init()
        {
            base.Init();
            groupDic = new();
        }
        protected override bool Add(GuildRankRewardData data)
        {
            if (base.Add(data))
            {
                int group = data.GROUP;
                if (!groupDic.ContainsKey(group))
                    groupDic.Add(group, new());

                groupDic[group].Add(data);
                return true;
            }
            return false;
        }
        public override void DataClear()
        {
            base.DataClear();
            if (groupDic == null)
                groupDic = new();
            else
                groupDic.Clear();
        }
        public List<GuildRankRewardData> GetByGroup(eGuildRankRewardGroup group)
        {
            if (groupDic == null || !groupDic.ContainsKey((int)group))
                return null;

            return groupDic[(int)group];
        }

        public GuildRankRewardData GetByRankGroup(int rank, eGuildRankRewardGroup group)
        {
            if (groupDic == null || !groupDic.ContainsKey((int)group))
                return null;

            foreach(var item in groupDic[(int)group])
            {
                if(item.HIGHEST_RANK <= rank && item.LOWEST_RANK > rank)
                {
                    return item;
                }
            }
            return null;
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
    }

    public class GuildResourceTable : TableBase<GuildResourceData, DBGuild_resource>
    {
        List<GuildResourceData> emblems = new List<GuildResourceData>();
        List<GuildResourceData> marks = new List<GuildResourceData>();
        List<GuildResourceData> flags = new List<GuildResourceData>();
        List<GuildResourceData> guildPick = new List<GuildResourceData>();
        List<GuildResourceData> userPick = new List<GuildResourceData>();
        public override void DataClear()
        {
            base.DataClear();
            emblems.Clear();
            marks.Clear();
            flags.Clear();
            guildPick.Clear();
            userPick.Clear();
        }
        protected override bool Add(GuildResourceData data)
        {
            if (base.Add(data))
            {
                switch(data.TYPE)
                {
                    case GuildResourceData.RESOURCE_TYPE.EMBLEM:
                        emblems.Add(data);
                        break;
                    case GuildResourceData.RESOURCE_TYPE.MARK:
                        marks.Add(data);
                        break;
                    case GuildResourceData.RESOURCE_TYPE.FLAG:
                        flags.Add(data);
                        break;
                    case GuildResourceData.RESOURCE_TYPE.GUILD_PICK:
                        guildPick.Add(data);
                        break;
                    case GuildResourceData.RESOURCE_TYPE.USER_PICK:
                        userPick.Add(data);
                        break;
                }
                return true;
            }
            return false;
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }

        public List<GuildResourceData> GetEmblems()
        {
            return emblems;
        }

        public List<GuildResourceData> GetMarks()
        {
            return marks;
        }

        public List<GuildResourceData> GetFlags()
        {
            return flags;
        }

        public List<GuildResourceData> GetGuildPick()
        {
            return guildPick;
        }
        public List<GuildResourceData> GetUserPick()
        {
            return userPick;
        }
    }
}