using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class TutorialTriggerTable : TableBase<TutorialTriggerData, DBTutorial_trigger>
    {
        Dictionary<ScriptTriggerType, List<TutorialTriggerData>> TypeDic = new();
        public override void DataClear()
        {
            base.DataClear();
            TypeDic.Clear();
        }
        public override void Preload()
        {
            base.Preload();

            LoadAll();

            foreach(var data in datas.Values)
            {
                if (TypeDic.ContainsKey(data.TriggerType) == false)
                {
                    TypeDic.Add(data.TriggerType, new List<TutorialTriggerData>());
                }
                TypeDic[data.TriggerType].Add(data);
            }
        }

        public List<TutorialTriggerData> GetByTriggerType(ScriptTriggerType triggerType)
        {
            if (TypeDic.ContainsKey(triggerType))
                return TypeDic[triggerType];
            return new List<TutorialTriggerData>();
        }
    }
    public class TutorialScriptTable : TableBase<TutorialScriptData, DBTutorial_script>
    {

        Dictionary<int, List<TutorialScriptData>> GroupDics = new();
        public override void DataClear()
        {
            base.DataClear();
            GroupDics.Clear();
        }
        public override void Init()
        {
            base.Init();
        }

        public override void Preload()
        {
            base.Preload();

            LoadAll();

            foreach(var data in datas.Values)
            {
                if (GroupDics.ContainsKey(data.GROUP) == false)
                {
                    GroupDics.Add(data.GROUP, new List<TutorialScriptData>());
                }
                GroupDics[data.GROUP].Add(data);
            }

            foreach (var group in GroupDics)
            {
                group.Value.Sort((x, y) => { return x.SEQUENCE.CompareTo(y.SEQUENCE); });
            }
        }

        public List<TutorialScriptData> GetByGroupList(int group)
        {
            if(GroupDics.ContainsKey(group))
                return GroupDics[group];

            return new List<TutorialScriptData>();
        }

        public TutorialScriptData Get(int group, int seq)
        {
            if (GroupDics.ContainsKey(group))
            {
                foreach (var data in GroupDics[group])
                {
                    if(data.SEQUENCE == seq)
                        return data;
                }
            }
                
            return null;
        }
    }
}

