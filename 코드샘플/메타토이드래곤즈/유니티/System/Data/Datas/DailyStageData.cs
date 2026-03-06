using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork {
    public class DailyStageData : TableData<DBDaily_stage>
    {
        static private DailyStageTable table = null;
        private GameObject daliySpinePrefab = null;

        static public List<DailyStageData> GetByDay(eDailyType dailyType)
        {
            if(table == null)
                table = TableManager.GetTable<DailyStageTable>();
            return table.GetDailyDungenDataByDay(dailyType);
        }
        static public DailyStageData GetByWorld(int worldNum)
        {
            if (table == null)
                table = TableManager.GetTable<DailyStageTable>();
            return table.GetByWorld(worldNum);
        }

        static public DailyStageData GetByWorldAndDay(int worldNum , int _day)
        {
            if (table == null)
                table = TableManager.GetTable<DailyStageTable>();
            return table.GetByWorldAndDay(worldNum, _day);
        }


        public int KEY => Int(Data.UNIQUE_KEY);
        public int DAY_GROUP => Data.DAY_GROUP;
        public int WORLD_NUM => Data.WORLD_NUM;
        public string STAGE_DESC => Data.STAGE_DESC;
        public string REWARD_DESC => Data.REWARD_DESC;
        public string REWARD_ICON => Data.REWARD_ICON;
        public string HOLDER_REWARD_DESC => Data.HOLDER_REWARD_DESC;
        public string HOLDER_REWARD_ICON => Data.HOLDER_REWARD_ICON;
        public string STAGE_IMAGE => Data.STAGE_IMAGE;
        public GameObject GetDailySpinePrefab()
        {
            if(daliySpinePrefab == null)
                daliySpinePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, STAGE_IMAGE);

            return daliySpinePrefab;
        }
    }
}