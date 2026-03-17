using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class BuildingBaseData : TableData<DBBuilding_base>
    {
        static private BuildingBaseTable table = null;
        static public BuildingBaseData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingBaseTable>();

            return table.Get(key);
        }

        static public List<BuildingBaseData> GetProductBuildingList()
        {
            if (table == null)
                table = TableManager.GetTable<BuildingBaseTable>();

            return table.GetProductBuildingList();
        }

        static public BuildingBaseData GetBuildingDataWithTag(int tag)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingBaseTable>();

            return table.GetBuildingDataWithTag(tag);
        }

        public string KEY => Data.UNIQUE_KEY;
        public int TYPE => Data.TYPE;
        public string _NAME => Data._NAME;
        public string _DESC => Data._DESC;
        public int SIZE => Data.SIZE;
        public int START_SLOT => Data.START_SLOT;
        public int MAX_SLOT => Data.MAX_SLOT;
        public string BUILD_AREA => Data.BUILD_AREA;
        public int BUILDING_ID => Data.BUILDING_ID;
        public bool IS_LANDMARK { get { return TYPE == 1; } }
    }

    public class BuildingLevelData : TableData<DBBuilding_level>
    {
        static private BuildingLevelTable table = null;
        static public BuildingLevelData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingLevelTable>();

            return table.Get(key);
        }
        static public BuildingLevelData GetDataByGroupAndLevel(string group, int level)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingLevelTable>();

            return table.GetDataByGroupAndLevel(group, level);
        }
        static public List<BuildingLevelData> GetDataByGroup(string group)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingLevelTable>();

            return table.GetDataByGroup(group);
        }

        static public int GetLevelUpNeedAreaLevel(string buildingGroup, int level)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingLevelTable>();

            return table.GetLevelUpNeedAreaLevel(buildingGroup, level);
        }

        static public int GetBuildingMaxLevelByGroup(string buildingGroup)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingLevelTable>();

            return table.GetBuildingMaxLevelByGroup(buildingGroup);
        }

        public string KEY => Data.UNIQUE_KEY;
        public string BUILDING_GROUP => Data.BUILDING_GROUP;
        public int LEVEL => Data.LEVEL;
        public string IMAGE => Data.IMAGE;
        public int UPGRADE_TIME => Data.UPGRADE_TIME;
        public int NEED_AREA_LEVEL => Data.NEED_AREA_LEVEL;
        public List<Asset> NEED_ITEM { get; private set; } = new List<Asset>();
        public string COST_TYPE => Data.COST_TYPE;
        public int COST_NUM => Data.COST_NUM;
        public override void Init()
        {
            base.Init();
            if (NEED_ITEM == null)
                NEED_ITEM = new();
            else
                NEED_ITEM.Clear();
        }
        public override void SetData(DBBuilding_level _data)
        {
            Init();
            base.SetData(_data);
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
            if (Data.NEED_ITEM_4 > 0 && Data.NEED_ITEM_4_NUM > 0)
            {
                NEED_ITEM.Add(new Asset(Data.NEED_ITEM_4, Data.NEED_ITEM_4_NUM));
            }
        }
    }

    public class BuildingOpenData : TableData<DBBuilding_open>
    {
        static private BuildingOpenTable table = null;
        static public List<BuildingOpenData> GetOpenListByLevel(int level)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetByBuildable(level);
        }

        static public BuildingOpenData GetWithTag(int tag)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetWithTag(tag);
        }
        static public BuildingOpenData GetByInstallTag(int tag)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetByInstallTag(tag);
        }
        static public BuildingOpenData GetAvailTotalBuilding(string buildType)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetAvailTotalBuilding(buildType);
        }

        static public List<BuildingOpenData> GetByBuildingGroup(string keyGroup)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetByBuildingGroup(keyGroup);
        }

        static public List<BuildingOpenData> GetAvailTotalBuildingList(string buildingIndex)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetAvailTotalBuildingList(buildingIndex);
        }

        static public List<BuildingOpenData> GetTotalBuildingList(string buildingIndex)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetTotalBuildingList(buildingIndex);
        }

        static public List<int> GetTagList(string buildingGroup)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetTagList(buildingGroup);
        }

        static public List<string> GetUserBuildingKeyList()
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetUserBuildingKeyList();
        }

        static public List<BuildingOpenData> GetBuildingOpenListByTagGroup(int tag)
        {
            if (table == null)
                table = TableManager.GetTable<BuildingOpenTable>();

            return table.GetBuildingOpenListByTagGroup(tag);
        }

        public string KEY => Data.UNIQUE_KEY;
        public int OPEN_LEVEL => Data.OPEN_LEVEL;
        public string BUILDING => Data.BUILDING;
        public int COUNT => Data.COUNT;
        public int INSTALL_TAG => Data.INSTALL_TAG;

        BuildingBaseData baseData = null;
        public BuildingBaseData BaseData 
        { 
            get 
            { 
                if(baseData == null)
                {
                    baseData = BuildingBaseData.Get(BUILDING);                    
                }

                return baseData;
            } 
        }
    }
}