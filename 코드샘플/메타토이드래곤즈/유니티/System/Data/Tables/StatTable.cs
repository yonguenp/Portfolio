using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class StatTable : TableBase<StatFactorData, DBStat_factor>
    {
        
    }
    public class StatTypeTable : TableBase<StatTypeData, DBStat_type>
    {
        private Dictionary<eStatusType, StatTypeData> typeDic = new Dictionary<eStatusType, StatTypeData>();
        public override void Init()
        {
            base.Init();
            typeDic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            typeDic.Clear();
        }
        public override void Preload()
        {
            base.Preload();

            LoadAll();

            foreach (var data in datas.Values)
            {
                if (typeDic.ContainsKey(data.STAT_TYPE))
                    typeDic[data.STAT_TYPE] = data;
                else
                    typeDic.Add(data.STAT_TYPE, data);
            }
        }

        public StatTypeData Get(eStatusType type)
        {
            if (typeDic.ContainsKey(type))
                return typeDic[type];

            return null;
        }
    }
}