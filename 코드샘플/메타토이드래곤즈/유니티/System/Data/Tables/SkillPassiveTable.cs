using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SkillPassiveTable : TableBase<SkillPassiveData, DBSkill_passive>
    {

    }

    public class SkillPassiveGroupTable : TableBase<SkillPassiveGroupData, DBSkill_passive_group>
    {
        private Dictionary<KeyValuePair<eJobType, eSkillPassiveGroupType>, SkillPassiveGroupData> skillPassiveGroupDic = new();

        public override void DataClear()
        {
            base.DataClear();
            skillPassiveGroupDic.Clear();
        }
        public SkillPassiveGroupData Get(eJobType job, eSkillPassiveGroupType type)
        {
            if(skillPassiveGroupDic == null)
                return null;
            if(skillPassiveGroupDic.TryGetValue(new KeyValuePair<eJobType, eSkillPassiveGroupType>(job, type), out var value))
                return value;
            return null;
        }

        public override void Preload()
        {
            base.Preload();

            LoadAll();

            foreach (SkillPassiveGroupData data in datas.Values)
            {
                data.Init();

                if (skillPassiveGroupDic.TryAdd(new KeyValuePair<eJobType, eSkillPassiveGroupType>(data.JOB, data.TYPE), data))
                {

                }
#if DEBUG
                else
                {
                    Debug.Log("중복 JOB, LEVEL 존재합니다.");
                }
#endif
            }
        }        
    }

    public class SkillPassiveRateTable : TableBase<SkillPassiveRateData, DBSkill_passive_rate>
    {
        Dictionary<int, List<SkillPassiveRateData>> skillPassiveRateDic = new();
        public override void DataClear()
        {
            base.DataClear();
            skillPassiveRateDic.Clear();
        }
        public override void Preload()
        {
            base.Preload();

            LoadAll();

            foreach (var data in datas.Values)
            {
                if (skillPassiveRateDic.ContainsKey(data.GROUP_ID) == false)
                {
                    skillPassiveRateDic.Add(data.GROUP_ID, new());
                }
                
                skillPassiveRateDic[data.GROUP_ID].Add(data);
            }

            foreach (var data in datas.Values)
            {
                if (data.RESULT_TYPE == "SKILL_GROUP")
                {
                    var child = GetByGroup(data.RESULT_GROUP);
                    if (child != null)
                    {
                        data.AddChild(child);
                    }
                }
            }
        }
        
        public List<SkillPassiveRateData> GetByGroup(int group)
        {
            if (skillPassiveRateDic.ContainsKey(group))
                return skillPassiveRateDic[group];
            return null;
        }

        public string GetSkillGroupName(int result_key)
        {
            foreach (var data in datas.Values)
            {
                if (data.Child.Count>0)
                {
                    foreach(var child in data.Child)
                    {
                        if (child.RESULT_GROUP == result_key)
                            return data.NAME;
                    }
                }
            }
             return string.Empty;
        }
    }

}
