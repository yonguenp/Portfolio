using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AreaExpansionData : TableData<DBArea_expansion>
    {
        static private AreaExpansionTable table = null;
        static public Vector3Int GetBetweenFloor(int areaGroup, int areaLevel)
        {
            if (table == null)
                table = TableManager.GetTable<AreaExpansionTable>();

            return table.GetBetweenFloor(areaGroup, areaLevel);
        }
        static public int GetMaxFloorLevel()
        {
            if (table == null)
                table = TableManager.GetTable<AreaExpansionTable>();

            return table.GetMaxFloorLevel();
        }
        static public AreaExpansionData GetFloorData(int floor)
        {
            if (table == null)
                table = TableManager.GetTable<AreaExpansionTable>();

            return table.GetFloorData(floor);
        }
        static public int GetLimitFloorByAreaLv(int areaLv)
        {
            if (table == null)
                table = TableManager.GetTable<AreaExpansionTable>();

            return table.GetFloorByLevel(areaLv);
        }

        public string KEY => Data.UNIQUE_KEY;
        public int AREA_GROUP => Data.AREA_GROUP;
        public int FLOOR
        {
            get
            {
                switch (GROUND)
                {
                    case "Ground":
                        return ORIGIN_FLOOR - 1;
                    case "Basement":
                        return -ORIGIN_FLOOR;
                }
                return ORIGIN_FLOOR;
            }
        }

        public int ORIGIN_FLOOR => Data.FLOOR;
        public int OPEN_LEVEL => Data.OPEN_LEVEL;
        public string GROUND => Data.GROUND;
        public string COST_TYPE => Data.COST_TYPE;
        public int COST_NUM => Data.COST_NUM;
    }

    public class AreaLevelData : TableData<DBArea_level>
    {
        static private AreaLevelTable table = null;
        static public AreaLevelData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<AreaLevelTable>();

            return table.Get(key);
        }
        static public AreaLevelData GetByLevel(int level)
        {
            if (table == null)
                table = TableManager.GetTable<AreaLevelTable>();

            return table.GetByLevel(level);
        }
        static public int GetMaxLevel()
        {
            if (table == null)
                table = TableManager.GetTable<AreaLevelTable>();

            return table.GetMaxLevel();
        }
        public int KEY => Int(Data.UNIQUE_KEY);
        public int LEVEL => Data.LEVEL;
        public List<Asset> NEED_ITEM { get; private set; } = new List<Asset>();
        public int NEED_GOLD => Data.NEED_GOLD;
        public int NEED_MISSION => Data.NEED_MISSION;
        public int EXPANSION_AREA => Data.EXPANSION_AREA;
        public int UPGRADE_TIME => Data.UPGRADE_TIME;
        public int WIDTH => Data.WIDTH;
        public override void Init()
        {
            base.Init();
            if (NEED_ITEM == null)
                NEED_ITEM = new();
            else
                NEED_ITEM.Clear();
        }
        public override void SetData(DBArea_level data)
        {
            Init();
            base.SetData(data);
            if (Data.NEED_ITEM_1 > 0 && Data.NEED_ITEM_1_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_1, Data.NEED_ITEM_1_NUM));
            }
            if (Data.NEED_ITEM_2 > 0 && Data.NEED_ITEM_2_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_2, Data.NEED_ITEM_2_NUM));
            }
            if (Data.NEED_ITEM_3 > 0 && Data.NEED_ITEM_3_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_3, Data.NEED_ITEM_3_NUM));
            }
        }
    }

	public class AreaLevelMissionData : TableData<DBArea_level_mission>
	{
        static private AreaLevelMissionTable table = null;
        static public AreaLevelMissionData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<AreaLevelMissionTable>();

            return table.Get(key);
        }
        static public List<AreaLevelMissionData> GetGroup(int group)
        {
            if (table == null)
                table = TableManager.GetTable<AreaLevelMissionTable>();

            return table.GetGroup(group);
        }
        public string KEY => Data.UNIQUE_KEY;
        public int GROUP => Data.GROUP;
        public int SORT => Data.SORT;
        public int DESC => Data._DESC;
        public string TYPE => Data.TYPE;
        public string SUB_TYPE => Data.SUB_TYPE;
        public string TYPE_KEY => Data.TYPE_KEY;
        public int TYPE_KEY_VALUE => Data.TYPE_KEY_VALUE;
	}

	public class TravelData : TableData<DBWorld_trip>
    {
        static private TravelTable table = null;
        static public TravelData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<TravelTable>();

            return table.Get(key);
        }
        static public TravelData GetByWorldID(int id)
        {
            if (table == null)
                table = TableManager.GetTable<TravelTable>();

            return table.GetByWorldID(id);
        }
        static public List<TravelData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<TravelTable>();

            return table.GetAllList();
        }

        public int WORLD => Data.WORLD;
        public string _NAME => Data._NAME;
        public int CHAR_NUM => Data.CHAR_NUM;
        public int TIME => Data.TIME;
        public int COST_STAMINA => Data.COST_STAMINA;
        public int REWARD_ACCOUNT_EXP => Data.REWARD_ACCOUNT_EXP;
        public int REWARD_CHAR_EXP => Data.REWARD_CHAR_EXP;
        public int REWARD_GOLD => Data.REWARD_GOLD;
        public int REWARD_BONUS => Data.REWARD_BONUS;
        public int REWARD_BONUS_NUM => Data.REWARD_BONUS_NUM;
    }

    public class RestrictedAreaData : TableData<DBWorld_trip_hell>
    {
        static private RestrictedAreaTable table = null;
        static public RestrictedAreaData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<RestrictedAreaTable>();

            return table.Get(key);
        }
        static public RestrictedAreaData GetByWorldDiff(int id, StageDifficult diff)
        {
            if (table == null)
                table = TableManager.GetTable<RestrictedAreaTable>();

            return table.GetByWorldDiff(id, diff);
        }
        static public List<RestrictedAreaData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<RestrictedAreaTable>();

            return table.GetAllList();
        }

        public int CONQUEST => Data.CONQUEST;
        public int NEED_WORLD_CLEAR => Data.NEED_WORLD_CLEAR;
        public int NEED_WORLD_DIFFICULT => Data.NEED_WORLD_DIFFICULT;
        public string _NAME => Data._NAME;
        public int NEED_BP => Data.NEED_BP;
        public int TIME => Data.TIME;
        public int COST_STAMINA => Data.COST_STAMINA;
        public int COST_FEE => Data.COST_FEE;
        public int FEE_TAX => Data.FEE_TAX;
        public int PROTECT_TIME => Data.PROTECT_TIME;
        public int LOSE_TIME => Data.LOSE_TIME;
        public int REWARD_ITEM => Data.REWARD_ITEM;

        public int REWARD_MAGNET_MIN => Data.REWARD_MAGNET_MIN;
        public int REWARD_MAGNET_MAX => Data.REWARD_MAGNET_MAX;
        public int REWARD_CHIPSET_MIN => Data.REWARD_CHIPSET_MIN;
        public int REWARD_CHIPSET_MAX => Data.REWARD_CHIPSET_MAX;
        public int REWARD_GOLDBLOCK_MIN => Data.REWARD_GOLDBLOCK_MIN;
        public int REWARD_GOLDBLOCK_MAX => Data.REWARD_GOLDBLOCK_MAX;
        public int REWARD_LEADBLOCK_MIN => Data.REWARD_LEADBLOCK_MIN;
        public int REWARD_LEADBLOCK_MAX => Data.REWARD_LEADBLOCK_MAX;

        public int REWARD_ELEMENTAL_MIN => Data.REWARD_ELEMENTAL_MIN;

        public int REWARD_ELEMENTAL_MAX => Data.REWARD_ELEMENTAL_MAX;
    }
}