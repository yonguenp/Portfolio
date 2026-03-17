using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class QuestTable : TableBase<QuestData, DBQuest_base>
    {
    }

    public class QuestTriggerTable : TableBase<QuestTriggerData, DBQuest_trigger_group>
    {
        Dictionary<int, List<QuestTriggerData>> groupDic = null;

        public override void Init()
        {
            base.Init();
            groupDic = new();
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
        }

        protected override bool Add(QuestTriggerData data)
        {
            if (base.Add(data))
            {
                if (!groupDic.ContainsKey(data.GROUP))
                    groupDic.Add(data.GROUP, new());

                groupDic[data.GROUP].Add(data);
                return true;
            }
            return false;
        }
        public List<QuestTriggerData> GetByGroup(int group)
        {
            if (groupDic == null || !groupDic.ContainsKey(group))
                return null;

            return groupDic[group];
        }
    }
}