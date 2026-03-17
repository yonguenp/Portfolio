using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork {
    public class PassInfoData : TableData
    {
        public PassInfoData(JArray datas)
                : base(datas)
        {
        }

        public string KEY { get { return UNIQUE_KEY; } }

        public bool USE { get; private set; } = false;

        private string pass_title_key = string.Empty;

        public string Pass_Title { get { return StringData.GetStringByStrKey(pass_title_key); } }
        public eBattlePassType TYPE { get; private set; } = eBattlePassType.NONE;
        public DateTime START_TIME { get; private set; } = DateTime.MinValue;
        public DateTime END_TIME { get; private set; } = DateTime.MinValue;



        public List<Quest> QuestProcessDone { get {
                List<Quest> ret = new List<Quest>();
                var quests = QuestManager.Instance.GetTotalQuestDataByType((eQuestType)quest_group_id);
                foreach (var quest in quests)
                {
                    if (quest.State == eQuestState.PROCESS_DONE)
                        ret.Add(quest);
                }
                return ret;
            } }
        public List<Quest> QuestIncludeTerminate
        {
            get
            {
                return QuestManager.Instance.GetTotalQuestDataByType((eQuestType)quest_group_id);
            }
        }

        public List<Quest> QuestTypeUnique(bool isIncludeDoneQuest) // isIncludeDoneQuest == true : 현재 수행중인 종류별 퀘스트 + 완료한 모든 퀘스트 // '수행중인'의 범위 : 보상을 받지 않은 상태의 퀘스트까지
        {                                                           // isIncludeDoneQuest == false : 현재 수행중인 종류별 퀘스트만 출력
            List<Quest> ret = new List<Quest>();
            List<string> uniqueType = new List<string>();
            var quests = QuestManager.Instance.GetTotalQuestDataByType((eQuestType)quest_group_id);
            foreach (var quest in quests)
            {
                if (isIncludeDoneQuest) 
                {
                    if (quest.State == eQuestState.TERMINATE || quest.State == eQuestState.PROCESS_DONE)
                    {
                        ret.Add(quest);
                        continue;
                    }
                }
                else
                {
                    if (quest.State == eQuestState.TERMINATE || quest.State == eQuestState.PROCESS_DONE)
                        continue;
                }
                string typeString = quest.QuestTableData._SUBJECT;
                if (uniqueType.Contains(typeString) == false)
                {
                    ret.Add(quest);
                    uniqueType.Add(typeString);
                }

            }
            return ret;
            
        }


        private int quest_group_id = -1;

        public int PASS_GOODS_ID { get; private set; } = 0;

        private int reward = 0;

        public Asset REWARD { get { 
                if(ItemGroupData.Get(reward) !=null)
                    return ItemGroupData.Get(reward)[0].Reward;
                return null;
            } }


        List<PassItemData> passItems = null;
        public List<PassItemData> PassItems
        {
            get
            {
                switch (Int(KEY))
                {
                    case 99999:
                        passItems = PassItemData.GetByGroupID(Int(KEY));
                        break;
                    default:
                        passItems = PassItemData.GetByGroupID(10000);
                        break;
                }

                List<PassItemData> ret = new List<PassItemData>();
                foreach (PassItemData item in passItems)
                {
                    ret.Add(item);
                }

                return ret;
            }
        }

        static private PassInfoTable table = null;
        static public PassInfoData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<PassInfoTable>();

            return table.Get(key);
        }

        static public bool IsPassGoods(int goodsNum)
        {
            if (table == null)
                table = TableManager.GetTable<PassInfoTable>();
            return table.IsPassGoods(goodsNum);
        }
        public override void SetData(JArray datas)
        {
            base.SetData(datas);

            PassItems.Clear();

            foreach (var d in data)
            {
                switch (d.Key)
                {
                    case "KEY":
                        break;
                    case "TYPE":
                        TYPE = (eBattlePassType)Int(d.Value);
                        break;
                    case "START_TIME":
                        if (!string.IsNullOrEmpty(d.Value) && DateTime.TryParse(d.Value, out DateTime sResult))
                            START_TIME = sResult;
                        else
                            START_TIME = DateTime.MinValue;
                        break;
                    case "END_TIME":
                        if (!string.IsNullOrEmpty(d.Value) && DateTime.TryParse(d.Value, out DateTime eResult))
                            END_TIME = eResult;
                        else
                            END_TIME = DateTime.MaxValue;
                        break;
                    case "USE":
                        USE = Int(d.Value) > 0;
                        break;
                    case "REWARD":
                        reward = Int(d.Value);
                        break;
                    case "QUEST_GROUP":
                        quest_group_id = Int(d.Value);
                        break;
                    case "PASS_GOODS_ID":
                        PASS_GOODS_ID = Int(d.Value);
                        break;
                    case "PASS_TITLE_KEY":
                        pass_title_key = d.Value;
                        break;
                }

            }
        }
        public bool IsValid()
        {
            if (data.Count <= 0)
                return false;
            if (USE==false)
                return false; 
            if (START_TIME <= TimeManager.GetDateTime() && END_TIME > TimeManager.GetDateTime())
                return true;
            return false;
        }


        public int GetCurrentLevel(int passPoint)
        {
            foreach(var item in PassItems)
            {
                if (passPoint < item.NEXT_POINT)
                    return item.LEVEL;
                    
            }
            return GetMaxLevel();
        }

        public int GetMaxLevel()
        {
            int max = -1;
            foreach(var item in PassItems)
            {
                max = Mathf.Max(max, item.LEVEL);
            }
            return max;
        }
        public int GetLvUpNeedPoint(int level)
        {
            foreach (var item in PassItems)
            {
                if (level == item.LEVEL)
                    return item.NEXT_POINT;

            }
            return 0;
        }
        public int PassStartLv()
        {
            int lv = int.MaxValue;
            foreach (var passitem in PassItems)
            {
                if (passitem == null)
                    continue;
                lv = (int)MathF.Min(lv,passitem.LEVEL);
            }
            return lv;
        }

        static public PassInfoData GetCurPass(eBattlePassType passType, bool valid_only = true)
        {
            List<PassInfoData> ret = new List<PassInfoData>();
            if (table == null)
                table = TableManager.GetTable<PassInfoTable>();
            foreach (var pass in table.GetAllList())
            {
                if (pass.USE && pass.TYPE == passType)
                {
                    if (pass.IsValid() && valid_only)
                        return pass;
                }
            }

            return null;
        }

        static public bool IsAvailablePassDataExist(eBattlePassType passType)
        {

            foreach (var pass in table.GetAllList())
            {
                if (pass.USE && pass.TYPE == passType)
                {
                    if (pass.IsValid())
                        return true;
                }
            }
            return false;
        }

    }


    public class PassItemData : TableData<DBPass_item>
    {
        static private PassItemTable table = null;
        static public List<PassItemData> GetByGroupID(int id)
        {
            if (table == null)
                table = TableManager.GetTable<PassItemTable>();
            return table.GetByGroupID(id);
        }
        public string KEY { get { return UNIQUE_KEY; } }
        public int GROUP => Data.GROUP;
        public int LEVEL => Data.LEVEL;
        public int NEXT_POINT => Data.NEXT_POINT;
        private int normalReward => Data.NORMAL_REWARD; // 아이템 그룹 ID
        private int specialReward => Data.SPECIAL_REWARD; // 아이템 그룹 ID
        private int holderSpReward => Data.HOLDER_SP_REWARD; // 아이템 그룹 ID
        public Asset NormalReward { get => GetReward(normalReward); }
        public Asset SpecialReward { get => GetReward(specialReward); }
        public Asset HolderReward { get => GetReward(holderSpReward); }
        private Asset GetReward(int value)
        {
            if (value == 0)
                return null;

            var itemGroup = ItemGroupData.Get(value);
            if (itemGroup == null || itemGroup.Count < 1)
                return null;

            return itemGroup[0].Reward;
        }
    }
}