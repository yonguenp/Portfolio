using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class SkillEffectTable : TableBase<SkillEffectData, DBSkill_effect>
    {
        private Dictionary<long, List<SkillEffectData>> groupDic = null;
        public override void Init()
        {
            base.Init();
            if (groupDic == null)
                groupDic = new();
            else
                groupDic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (groupDic == null)
                groupDic = new();
            else
                groupDic.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();

            foreach(var data in datas.Values)
            {
                if (!groupDic.ContainsKey(data.GROUP_KEY))
                    groupDic.Add(data.GROUP_KEY, new());

                groupDic[data.GROUP_KEY].Add(data);
            }
        }
        public List<SkillEffectData> GetGroup(long key)
        {
            if (groupDic != null && groupDic.TryGetValue(key, out var val))
                return val;

            return null;
        }
    }
}