using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SandboxNetwork
{
    public class QuestData : TableData<DBQuest_base>
    {
        static private QuestTable table = null;
        static public QuestData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<QuestTable>();

            return table.Get(key);
        }

        static public QuestData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<QuestTable>();

            return table.Get(key);
        }

        static public eQuestType GetQuestType(int key)
        {
            if (table == null)
                table = TableManager.GetTable<QuestTable>();

            var data = table.Get(key);
            if (data == null)
                return eQuestType.MAIN;
            return data.TYPE;
        }

        public int KEY { get { return Int(UNIQUE_KEY); } }
        public eQuestType TYPE => SBFunc.ConvertToQuestType(Data.TYPE);
        public eQuestGroup GROUP => (eQuestGroup)Data.GROUP;
        public string ICON => Data.ICON;
        public string _SUBJECT => Data._SUBJECT;
        public bool IS_START_POPUP => Data.IS_START_POPUP == 1;
        public int START_GROUP => Data.START_GROUP;
        public int CONDITION_GROUP => Data.CONDITION_GROUP;
        public int REWARD_ACCOUNT_EXP => Data.REWARD_ACCOUNT_EXP;
        public int REWARD_GOLD => Data.REWARD_GOLD;
        public int REWARD_ENERGY => Data.REWARD_ENERGY;
        public int REWARD_TICKET_ARENA => Data.REWARD_TICKET_PVP;
        public int REWARD_GROUP => Data.REWARD_GROUP;

        public int EVENT_KEY => Data.EVENT_KEY;

        private List<Asset> rewards = null;
        public List<Asset> REWARDS {
            get {
                if (rewards == null)
                {
                    rewards = new List<Asset>();
                    if (REWARD_ACCOUNT_EXP > 0)
                    {
                        rewards.Add(new Asset(eGoodType.NONE, 10000003, REWARD_ACCOUNT_EXP));
                    }

                    if (REWARD_GOLD > 0)
                    {
                        rewards.Add(new Asset(eGoodType.GOLD, 10000001, REWARD_GOLD));
                    }

                    if (REWARD_ENERGY > 0)
                    {
                        rewards.Add(new Asset(eGoodType.ENERGY, 10000002, REWARD_ENERGY));
                    }

                    if (REWARD_TICKET_ARENA > 0)
                    {
                        rewards.Add(new Asset(eGoodType.ARENA_TICKET, 10000007, REWARD_TICKET_ARENA));
                    }

                    if(REWARD_GROUP > 0)
                    {
                        var rewardItemList = ItemGroupData.Get(REWARD_GROUP);
                        if(rewardItemList != null && rewardItemList.Count > 0)
                        {
                            foreach(var rewardItem in rewardItemList)
                            {
                                rewards.Add(rewardItem.Reward);
                            }
                        }

                    }
                }
                return rewards; 
            } 
        }
	}
    public class QuestTriggerData : TableData<DBQuest_trigger_group>
    {
        static private QuestTriggerTable table = null;
        static public List<QuestTriggerData> Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<QuestTriggerTable>();

            if(int.TryParse(key, out int iKey))
                return table.GetByGroup(iKey);

            return null;
        }

        static public QuestTriggerData GetTrigger(int triggerID)
        {
            if (table == null)
                table = TableManager.GetTable<QuestTriggerTable>();

            return table.Get(triggerID);
        }

        public string KEY { get { return UNIQUE_KEY; } }
        public int TRIGGER_TYPE => Data.TRIGGER_TYPE;
        public int GROUP => Data.GROUP;
        public string TYPE => Data.TYPE;
        public string SUB_TYPE => Data.SUB_TYPE;
        public string TYPE_KEY => Data.TYPE_KEY;
        public int TYPE_KEY_VALUE => Data.TYPE_KEY_VALUE;
        public string _NOTIE => Data._NOTIE;
    }
}