using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PassInfoTable : TableBase<PassInfoData>
    {
        const int PASS_START_TIME = 1695150000;
        public override void SetTable(JArray dataArray)
        {
            DataClear();

            JArray keys = new JArray();
            keys.Add("KEY");
            keys.Add("USE");
            keys.Add("TYPE");
            keys.Add("START_TIME");
            keys.Add("END_TIME");
            keys.Add("REWARD");
            keys.Add("QUEST_GROUP");
            keys.Add("PASS_GOODS_ID");
            keys.Add("PASS_TITLE_KEY");

            TableData.SetDataKeys(typeof(PassInfoData), keys);

            JArray holderPass = new JArray() { 
                "99999",
                "1",
                "2",
                "",
                "",
                "0",
                "7",
                "0",
                "holder_pass_season_name1",
            };

            Add(new PassInfoData(holderPass));

            //System.DateTime now = TimeManager.GetDateTime();
            //System.DateTime pivot =TimeManager.GetCustomDateTime(GameConfigTable.GetConfigIntValue("PASS_START_TIME", PASS_START_TIME));
            //int season = 0;
            //const int season_days = 21;
            //while (now > pivot)
            //{
            //    pivot = pivot.AddDays(season_days);
            //    season++;
            //}

            //JArray curPass = new JArray() {
            //    (season + 10000).ToString(),
            //    "1",
            //    "1",
            //    pivot.AddDays(-season_days).ToString("yyyy-MM-dd HH:mm:ss"),
            //    pivot.ToString("yyyy-MM-dd HH:mm:ss"),
            //    "800126",
            //    "6",
            //    "999999",
            //    "battle_pass_season_name" + season,
            //};

            //Add(new PassInfoData(curPass));

            //season += 1;
            //JArray nextPass = new JArray() {
            //    (season + 10000).ToString(),
            //    "1",
            //    "1",                
            //    pivot.ToString("yyyy-MM-dd HH:mm:ss"),
            //    pivot.AddDays(season_days).ToString("yyyy-MM-dd HH:mm:ss"),
            //    "800126",
            //    "6",
            //    "999999",
            //    "battle_pass_season_name" + season,
            //};

            //Add(new PassInfoData(nextPass));
        }

        public bool IsPassGoods(int goodsNum)
        {

            foreach(var dat in datas.Values)
            {
                if (dat.PASS_GOODS_ID == goodsNum)
                    return true;
            }

            return false;
        }
    }

    public class PassItemTable : TableBase<PassItemData, DBPass_item>
    {
        Dictionary<int, List<PassItemData>> dicGroup = new Dictionary<int, List<PassItemData>>();
        public override void Init()
        {
            base.Init();
            dicGroup.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            dicGroup.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(PassItemData data)
        {
            if (base.Add(data))
            {
                var key = data.GROUP;
                if (!dicGroup.ContainsKey(key))
                    dicGroup[key] = new List<PassItemData>();

                dicGroup[key].Add(data);
                return true;
            }

            return false;
        }

        public List<PassItemData> GetByGroupID(int id)
        {
            if(dicGroup.TryGetValue(id, out var value))
                return value;

            return new List<PassItemData>();
        }
    }
}