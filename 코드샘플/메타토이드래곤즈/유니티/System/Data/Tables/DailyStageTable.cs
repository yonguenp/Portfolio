using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork { 
    public class DailyStageTable : TableBase<DailyStageData, DBDaily_stage>
    {
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        public List<DailyStageData> GetDailyDungenDataByDay(eDailyType dayType)
        {
            List<DailyStageData> dailyDungeonDatas = new List<DailyStageData>();
            foreach(var data in datas.Values)
            {
                if(data.DAY_GROUP == (int)dayType && data.WORLD_NUM >= 100 && data.WORLD_NUM < 200)
                {
                    dailyDungeonDatas.Add(data);
                }
            }

            return dailyDungeonDatas;
        }

        public DailyStageData GetByWorld(int worldNum)
        {
            foreach (var data in datas.Values)
            {
                if (data.WORLD_NUM == worldNum)
                {
                    return data;
                }
            }
            return null;
        }

        public DailyStageData GetByWorldAndDay(int _world, int _day)
        {
            var dataList = datas.Values.ToList();
            if (dataList == null || dataList.Count <= 0)
                return null;
            return dataList.Find(element => element.DAY_GROUP == _day && element.WORLD_NUM == _world);
        }
    }
}