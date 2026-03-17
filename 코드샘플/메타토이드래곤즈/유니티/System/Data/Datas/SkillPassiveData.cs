
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace SandboxNetwork
{
    public class SkillPassiveData : TableData<DBSkill_passive>
    {
        static private SkillPassiveTable table = null;

        static public SkillPassiveData Get(int key)
        {
            return Get(key.ToString());
        }
        static public SkillPassiveData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillPassiveTable>();
            return table.Get(key);
        }

        static public List<SkillPassiveData> GetForChampion(eJobType job, int slotIndex)
        {
            if (table == null)
                table = TableManager.GetTable<SkillPassiveTable>();

            Dictionary<string, SkillPassiveData> ret = new Dictionary<string, SkillPassiveData>();
            
            var commonData = SkillPassiveGroupData.Get(job, eSkillPassiveGroupType.COMMON);
            if (commonData != null)
            {
                foreach (var rate in SkillPassiveRateData.GetByGroup(slotIndex == 0 ? commonData.GROUP_ID_SLOT_1 : commonData.GROUP_ID_SLOT_2))
                {
                    foreach (var child in rate.Child)
                    {
                        var data = Get(child.RESULT_GROUP);
                        if (data != null)
                        {
                            string key = data.Data.PASSIVE_EFFECT + data.Data.STAT + data.Data.EFFECT_VALUE;
                            if (ret.ContainsKey(key))
                            {
                                if (ret[key].VALUE < data.VALUE)
                                    ret[key] = data;
                            }
                            else
                            {
                                ret.Add(key, data);
                            }
                        }
                    }
                }
            }
            var uncommonData = SkillPassiveGroupData.Get(job, eSkillPassiveGroupType.UNCOMMON);
            if (uncommonData != null)
            {
                foreach (var rate in SkillPassiveRateData.GetByGroup(slotIndex == 0 ? commonData.GROUP_ID_SLOT_1 : commonData.GROUP_ID_SLOT_2))
                {
                    foreach (var child in rate.Child)
                    {
                        var data = Get(child.RESULT_GROUP);
                        if (data != null)
                        {
                            string key = data.Data.PASSIVE_EFFECT + data.Data.STAT + data.Data.EFFECT_VALUE;
                            if (ret.ContainsKey(key))
                            {
                                if (ret[key].VALUE < data.VALUE)
                                    ret[key] = data;
                            }
                            else
                            {
                                ret.Add(key, data);
                            }
                        }
                    }
                }
            }

            return ret.Values.ToList();
        }
        public int KEY => Int(UNIQUE_KEY);
        public eSkillPassiveEffect PASSIVE_EFFECT => SBFunc.GetSkillPassiveEffect(Data.PASSIVE_EFFECT);
        public eStatusType STAT => SBFunc.ConvertStatusType(Data.STAT);
        public eStatusValueType EFFECT_VALUE => SBFunc.ConvertValueType(Data.EFFECT_VALUE);
        public float VALUE => Data.VALUE;
        public float CONVERT_VALUE { get => VALUE * SBDefine.CONVERT_FLOAT; }
        public eSkillTargetType TARGET => SBFunc.ConvertSkillTargetType(Data.TARGET);
        public eSkillPassiveStartType START_TYPE => SBFunc.GetSkillPassiveStartType(Data.START_TYPE);
        public float RATE => Data.RATE;
        public eSkillPassiveRateType RATE_TYPE => SBFunc.GetSkillPassiveRateType(Data.RATE_TYPE);
        public float ADD_RATE_MAX => Data.ADD_RATE_MAX;
        public float MAX_TIME => Data.MAX_TIME;
        public int NEST_GROUP => Data.NEST_GROUP;
        public int NEST_COUNT => Data.NEST_COUNT;
        public int SELF_EFFECT_RESOURCE => Data.SELF_EFFECT_RESOURCE;
        public int TARGET_EFFECT_RESOURCE => Data.TARGET_EFFECT_RESOURCE;
        private string DESC => Data.DESC;
        public string STRING { get { return StringData.GetStringFormatByStrKey(DESC, VALUE, MAX_TIME, RATE, RATE+ADD_RATE_MAX ); } }
        // {0} : 값, {1} : 시간, {2} : 기본확률, {3} : 최대확률
        // RATE_TYPE == eSkillPassiveRateType.PERCENTAGE 라면 RATE + ADD_RATE_MAX 를 사용하지 말 것 // 지정된 확률이라 RATE 만 사용하는게 올바름
        // RATE ~ RATE+ADD_RATE_MAX %의 확률로 00한다 구조
        public int USE_CONTENTS => Data.USE_CONTENTS;
    }

    public class SkillPassiveGroupData : TableData<DBSkill_passive_group>
    {
        static private SkillPassiveGroupTable table = null;

        static public SkillPassiveGroupData Get(int key)
        {
            return Get(key.ToString());
        }
        static public SkillPassiveGroupData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillPassiveGroupTable>();
            return table.Get(key);
        }
        static public SkillPassiveGroupData Get(eJobType jobType, eSkillPassiveGroupType type)
        {
            if (table == null)
                table = TableManager.GetTable<SkillPassiveGroupTable>();
            return table.Get(jobType, type);
        }

        public string KEY => Data.UNIQUE_KEY;
        public eJobType JOB => SBFunc.GetJobType(Int(KEY) / 100);
        public eSkillPassiveGroupType TYPE => SBFunc.GetSkillPassiveGroupType(Int(KEY) % 10);
        public int GROUP_ID_SLOT_1 => Data.GROUP_ID_SLOT_1;
        public int GROUP_ID_SLOT_2 => Data.GROUP_ID_SLOT_2;
        public Asset NeedItem = null;
        public Asset NeedGold = null;
        private int PRICE_ITEM => Data.PRICE_ITEM;
        private int PRICE_VALUE => Data.PRICE_VALUE;
        private int PRICE_GOLD => Data.PRICE_GOLD;

        public override void Init()
        {
            NeedItem = new Asset(PRICE_ITEM, PRICE_VALUE);
            NeedGold = new Asset(eGoodType.GOLD, 0, PRICE_GOLD);
        }
    }

    public class SkillPassiveRateData : TableData<DBSkill_passive_rate>
    {
        static private SkillPassiveRateTable table = null;

        static public SkillPassiveRateData Get(int key)
        {
            return Get(key.ToString());
        }
        static public SkillPassiveRateData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillPassiveRateTable>();
            return table.Get(key);
        }

        static public List<SkillPassiveRateData> GetByGroup(int group)
        {
            if (table == null)
                table = TableManager.GetTable<SkillPassiveRateTable>();
            return table.GetByGroup(group);
        }

        static public string GetSkillGroupName(int result_key)
        {
            if (table == null)
                table = TableManager.GetTable<SkillPassiveRateTable>();
            return table.GetSkillGroupName(result_key);
        }

        public int GROUP_ID => Data.GROUP_ID;
        public int RATE => Data.RATE;
        public string RESULT_TYPE => Data.RESULT_TYPE;
        public int RESULT_GROUP => Data.RESULT_GROUP;

        private string passive_name => Data.PASSIVE_NAME;

        public string NAME { get { return StringData.GetStringByStrKey(passive_name); } }

        public List<SkillPassiveRateData> Child { get; private set; } = new List<SkillPassiveRateData>();
        
        public void AddChild(List<SkillPassiveRateData> childs)
        {
            if(childs != null &&childs.Count > 0)
            {
                Child.AddRange(childs);
            }
        }
    }
}