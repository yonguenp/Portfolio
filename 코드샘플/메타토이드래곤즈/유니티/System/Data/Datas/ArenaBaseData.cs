using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class ArenaRankData : TableData<DBPvp_rank>
    {
        static private ArenaRankTable table = null;
        static public ArenaRankData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<ArenaRankTable>();

            return table.Get(key);
        }
        static public ArenaRankData GetCurrentRankData(int currentPoint)
        {
            if (table == null)
                table = TableManager.GetTable<ArenaRankTable>();

            return table.GetCurrentRankData(currentPoint);
        }
        static public int GetNeedPointByTierLevelUP(int currentPoint)
        {
            if (table == null)
                table = TableManager.GetTable<ArenaRankTable>();

            return table.GetNeedPointByTierLevelUP(currentPoint);
        }

        static public ArenaRankData GetFirstInGroup(int group)
        {
            if (table == null)
                table = TableManager.GetTable<ArenaRankTable>();
            return table.GetFirstInGroup(group);
        }

        static public List<ArenaRankData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<ArenaRankTable>();
            return table.GetAllList();
        }

        static public string GetIconNameByPoint(eArenaRankGrade _grade, int _point)
        {
            ArenaRankData currentRankData = GetFirstInGroup((int)_grade);
            if (currentRankData == null)
                currentRankData = GetCurrentRankData(_point);

            return currentRankData.ICON;
        }


        /// <summary>
        ///  Dictionary (초기화 랭크 번호 : 초기화 되기전 랭크 리스트) 반환
        /// </summary>

        static public Dictionary<int, List<ArenaRankData>> GetResetRankDic()
        {
            if (table == null)
                table = TableManager.GetTable<ArenaRankTable>();
            return table.GetAllResetRankDic();
        }

        public int KEY => Int(Data.UNIQUE_KEY);
        public int GROUP => Data.GROUP;
        public string _NAME => Data._NAME;
        public string ICON => Data.ICON;
        public int NEED_POINT => Data.NEED_POINT;
        public int FIRST_REWARD_GROUP => Data.FIRST_REWARD_GROUP;
        public int HOLDER_FIRST_REWARD_GROUP => Data.FIRST_REWARD_GROUP_HD;
        public int SEASON_REWARD_GROUP => Data.SEASON_REWARD_GROUP;
        public int HOLDER_SEASON_REWARD_GROUP => Data.SEASON_REWARD_GROUP_HD;
        public int RESET_RANK => Data.RESET_RANK;
    }

    public class ArenaRankSeasonRewardData : TableData<DBPvp_rank_season_reward>
    {
        static private ArenaRankSeasonRewardTable table = null;
        static public ArenaRankSeasonRewardData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<ArenaRankSeasonRewardTable>();

            return table.Get(key);
        }
        static public List<ArenaRankSeasonRewardData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<ArenaRankSeasonRewardTable>();

            return table.GetAllList();
        }
        public int KEY => Int(Data.UNIQUE_KEY);
        public string TYPE => Data.TYPE;
        public int MIN => Data.MIN;
        public int MAX => Data.MAX;
        public int REWARD_GOLD => Data.REWARD_GOLD;
        public int REWARD_CASH => Data.REWARD_CASH;
        public int REWARD_ITEM => Data.REWARD_ITEM;
        public int REWARD_ITEM_NUM => Data.REWARD_ITEM_NUM;
    }

    //더이상 쓰지 않기로하자
    public class ArenaSeasonData : TableData<DBPvp_season>
    {
        static private ArenaSeasonTable table = null;

        static public ArenaSeasonData CUR
        {
            get
            {
                if (table == null)
                    table = TableManager.GetTable<ArenaSeasonTable>();

                return table.Get(ArenaManager.Instance.UserArenaData.season_id * SBDefine.DIV_ARENA_SEASON + ArenaManager.Instance.UserArenaData.season_type);
            }
        }

        static public ArenaSeasonData NEXT
        {
            get
            {
                if (table == null)
                    table = TableManager.GetTable<ArenaSeasonTable>();

                ArenaSeasonData ret = null;
                var curTIme = TimeManager.GetDateTime();
                foreach (var data in table.GetAllList())
                {
                    if (data.START > curTIme)
                    {
                        if (ret == null)
                        {
                            ret = data;
                        }
                        else
                        {
                            if (ret.START > data.START)
                                ret = data;
                        }
                    }
                }

                return ret;
            }
        }

        public int KEY => Int(Data.UNIQUE_KEY);
        public string NAME => Data._NAME;
        public DateTime START => SBFunc.DateTimeParse(Data.START_TIME);
        public DateTime END => SBFunc.DateTimeParse(Data.END_TIME);

        public eElementType ATK_ELEM => (eElementType)Data.ATK_ELEM;
        public eElementType DEF_ELEM => (eElementType)Data.DEF_ELEM;
        public eElementType HP_ELEM => (eElementType)Data.HP_ELEM;
    }
}