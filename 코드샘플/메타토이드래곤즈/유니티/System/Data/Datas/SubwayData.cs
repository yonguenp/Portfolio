using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SandboxNetwork
{
    public class SubwayPlatformData : TableData<DBSubway_platform>
    {
        static private SubwayPlatformTable table = null;
        static public SubwayPlatformData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SubwayPlatformTable>();

            return table.Get(key);
        }

        static public SubwayPlatformData GetByFlatform(int platform)
        {
            if (table == null)
                table = TableManager.GetTable<SubwayPlatformTable>();

            return table.GetByFlatform(platform);
        }

        public int PLATFORM => Data.PLATFORM;
        public int OPEN_LEVEL => Data.OPEN_LEVEL;
        public string COST_TYPE => Data.COST_TYPE;
        public int COST_NUM => Data.COST_NUM;        
    }
    public class SubwayDeliveryData : TableData<DBSubway_delivery>
    {
        static private SubwayDeliveryTable table = null;
        static public SubwayDeliveryData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SubwayDeliveryTable>();

            return table.Get(key);
        }

        public int NEED_ITEM => Data.NEED_ITEM;
        public int NEED_NUM => Data.NEED_NUM;
        public int NEED_PRODUCT_COUNT => Data.NEED_PRODUCT_COUNT;
        public int DELIVERY_TIME => Data.DELIVERY_TIME;
        public int REWARD_GROUP => Data.REWARD_GROUP;
    }
}