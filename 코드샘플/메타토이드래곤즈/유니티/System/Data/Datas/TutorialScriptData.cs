using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class TutorialTriggerData : TableData<DBTutorial_trigger>
    {
        List<TutorialScriptData> scripts = null;
        public List<TutorialScriptData> SCRIPTS {
            get
            {
                if (scripts == null)
                {
                    scripts = TutorialScriptData.GetByGroupList(KEY);
                }

                return scripts;
            }
        }

        public int KEY => Int(Data.UNIQUE_KEY);
        public int FIRST_SEQ 
        { 
            get { return SCRIPTS != null && SCRIPTS.Count > 0 ? SCRIPTS[0].SEQUENCE : -1; } 
        }
        public ScriptTriggerType TriggerType => (ScriptTriggerType)Data.TRIGGER_TYPE;

        public eTutorialType TutorialType => (eTutorialType)Data.TUTORIAL_TYPE;
        public int TriggerParam => Data.TRIGGER_PARAM;

        private int rewardKey => Data.REWARD;
        public List<Asset> REWARD { get
            {
                List<Asset> ret = new List<Asset>();
                var ItemDats = ItemGroupData.Get(rewardKey);
                if(ItemDats != null)
                {
                    foreach (var item in ItemDats)
                    {
                        ret.Add(item.Reward);
                    }
                }
                return ret;
            }
        }
        
        static public List<TutorialTriggerData> GetByTriggerType(ScriptTriggerType triggerType)
        {
            if (table == null)
                table = TableManager.GetTable<TutorialTriggerTable>();

            return table.GetByTriggerType(triggerType);
        }

        static private TutorialTriggerTable table = null;
        static public TutorialTriggerData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<TutorialTriggerTable>();
            return table.Get(key);
        }
    }
    public class TutorialScriptData : TableData<DBTutorial_script>
    {
        static private TutorialScriptTable table = null;
        static public List<TutorialScriptData> GetByGroupList(int groupNo)
        {
            if (table == null)
                table = TableManager.GetTable<TutorialScriptTable>();

            return table.GetByGroupList(groupNo);
        }

        static public TutorialScriptData Get(int groupNo, int seqNo)
        {
            if (table == null)
                table = TableManager.GetTable<TutorialScriptTable>();
            return table.Get(groupNo, seqNo);
        }
        static public TutorialScriptData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<TutorialScriptTable>();
            return table.Get(key);
        }

        public string KEY => Data.UNIQUE_KEY;
        public int GROUP => Data.GROUP;
        public int SEQUENCE => Data.SEQUENCE;
        
        private int SCRIPT_STR => Data.SCRIPT_STR;

        private int RESTART_TUTORIAL => Data.RESTART_TUTORIAL;

        public TutorialScriptData RestartTuto 
        { 
            get
            {
                return Get(RESTART_TUTORIAL);
            } 
        }

        public string STRING
        {
            get { 
                if(SCRIPT_STR !=0 && ScriptStringData.Get(SCRIPT_STR) != null)
                {
                    return ScriptStringData.Get(SCRIPT_STR).TEXT;
                }
                return string.Empty;
            }
        }
    }
}