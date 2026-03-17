using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SandboxNetwork
{
    public class SubwayPlatformTable : TableBase<SubwayPlatformData, DBSubway_platform>
    {
        Dictionary<int, SubwayPlatformData> dicPlatform = new Dictionary<int, SubwayPlatformData>();
        public override void Init()
        {
            base.Init();
        }

        public override void DataClear()
        {
            base.DataClear();
            dicPlatform.Clear();
        }

        public override void Preload()
        {
            base.Preload();
            LoadAll();

            foreach (var data in datas.Values)
            {
                dicPlatform[data.PLATFORM] = data;
            }
        }
        public SubwayPlatformData GetByFlatform(int platform)
        {
            if(dicPlatform.ContainsKey(platform))
                return dicPlatform[platform];

            return null;
        }
    }
    public class SubwayDeliveryTable : TableBase<SubwayDeliveryData, DBSubway_delivery>
    {
        Dictionary<int, SubwayDeliveryData> dicNeedItem = new Dictionary<int, SubwayDeliveryData>();
        public override void Init()
        {
            base.Init();
        }
        public override void DataClear()
        {
            base.DataClear();
            dicNeedItem.Clear();
        }

        public override void Preload()
        {
            base.Preload();

            LoadAll();

            foreach(var data in datas.Values)
            {
                dicNeedItem[data.NEED_ITEM] = data;
            }
        }

        public int GetDeliveringTime(int itemNo)
        {
            if (dicNeedItem.ContainsKey(itemNo))
            {
                return dicNeedItem[itemNo].DELIVERY_TIME;
            }

            return 0;
        }

        static public int GetTotalDeliveringTime(LandmarkSubwayPlantData platformData)
        {
            var instance = TableManager.GetTable<SubwayDeliveryTable>();

            int ret = 0;            
            for (int i = 0, count = platformData.Slots.Count; i < count; ++i)
            {
                ret += instance.GetDeliveringTime(platformData.Slots[i][0]);
            }

            return ret;
        }
    }
}