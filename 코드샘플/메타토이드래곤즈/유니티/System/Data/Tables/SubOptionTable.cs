using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{

    public class SubOptionTable : TableBase<SubOptionData, DBSub_option>
    {
        Dictionary<int, List<SubOptionData>> dicGroup = new Dictionary<int, List<SubOptionData>>();

        public override void Init()
        {
            base.Init();
        }
        public override void DataClear()
        {
            dicGroup.Clear();
            base.DataClear();
        }

        public override void Preload()
        {
            base.Preload();

            LoadAll();

            foreach(var data in datas.Values)
            {
                if (!dicGroup.ContainsKey(data.GROUP))
                    dicGroup[data.GROUP] = new List<SubOptionData>();

                if(data.RATE > 0)
                    dicGroup[data.GROUP].Add(data);
            }
        }

        public List<SubOptionData> GetOptionByGroup(int groupKey)
        {
            if (dicGroup.ContainsKey(groupKey))
                return dicGroup[groupKey];

            return new List<SubOptionData>();
        }
    }
}