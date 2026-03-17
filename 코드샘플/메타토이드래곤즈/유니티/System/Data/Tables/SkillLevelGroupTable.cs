using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class SkillLevelGroupTable : TableBase<SkillLevelGroupData, DBSkill_level_group>
    {
        private Dictionary<int, List<SkillLevelGroupData>> groupDic = null;
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
                if (!groupDic.ContainsKey(data.GROW_GROUP_KEY))
                    groupDic.Add(data.GROW_GROUP_KEY, new());

                groupDic[data.GROW_GROUP_KEY].Add(data);
            }
        }
        public List<SkillLevelGroupData> GetGroup(int key)
        {
            if (groupDic != null && groupDic.TryGetValue(key, out var val))
                return val;

            return null;
        }
    }
}