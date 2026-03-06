using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class PetBaseData : TableData<DBPet_base>
    {
        static private PetTable table = null;

        static public PetBaseData Get(int key)
        {
            return Get(key.ToString());
        }

        static public PetBaseData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetTable>();

            return table.Get(key);
        }

        static public List<PetBaseData> GetAllForChampion()
        {
            if (table == null)
                table = TableManager.GetTable<PetTable>();

            return table.GetGradeAll((int)eDragonGrade.Legend);
        }
        public int KEY { get { return Int(UNIQUE_KEY); } }
        public int GRADE => Data.GRADE;
        public int ELEMENT => Data.ELEMENT;
        public string BUFF_TYPE => Data.ELEMENT_BUFF_TYPE;
        public eStatusType ELEMENT_BUFF_TYPE { get; private set; } = eStatusType.NONE;
        public List<int> SUB { get; private set; } = new List<int>();
        public string BACKGROUND => Data.BACKGROUND;
        public string IMAGE => Data.IMAGE;
        public string THUMBNAIL => Data.THUMBNAIL;
        public string SKIN => Data.SKIN;
        public string _NAME => Data._NAME;
        public string _DESC => Data._DESC;
        public override void SetData(DBPet_base data)
        {
            SUB.Clear();
            base.SetData(data);

            ELEMENT_BUFF_TYPE = SBFunc.ConvertStatusType(BUFF_TYPE);
            SUB.Add(Data.SUB_1);
            SUB.Add(Data.SUB_2);
            SUB.Add(Data.SUB_3);
            SUB.Add(Data.SUB_4);
        }
    }
    public class PetGradeData : TableData<DBPet_grade>
    {
        static private PetGradeTable table = null;
        static public PetGradeData Get(int key)
        {
            return Get(key.ToString());
        }
        static public PetGradeData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetGradeTable>();

            return table.Get(key);
        }
        public string KEY { get { return UNIQUE_KEY; } }
        public int _NAME => Data._NAME;
        public int START_STAT_NUM => Data.START_STAT_NUM;
        public int SKILL_MAX => Data.SKILL_MAX;
        public int UNIQUE_MIN_GROUP_NUM => Data.UNIQUE_MIN_GROUP_NUM;
        public int UNIQUE_MAX_GROUP_NUM => Data.UNIQUE_MAX_GROUP_NUM;
    }
    public class PetExpData : TableData<DBPet_exp>
    {
        static private PetExpTable table = null;
        static public PetExpData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetExpTable>();

            return table.Get(key);
        }

        static public Dictionary<string, int> GetLevelAndExpByTotalExp(int level, int grade)
        {
            if (table == null)
                table = TableManager.GetTable<PetExpTable>();

            return table.GetLevelAndExpByTotalExp(level, grade);
        }

        static public int GetCurrentAccumulateLevelExp(int level, int grade)
        {
            if (table == null)
                table = TableManager.GetTable<PetExpTable>();

            return table.GetCurrentAccumulateLevelExp(level, grade);
        }
        static public int GetCurrentRequireLevelExp(int level, int grade)
        {
            if (table == null)
                table = TableManager.GetTable<PetExpTable>();

            return table.GetCurrentRequireLevelExp(level, grade);
        }

        static public PetExpData GetExpDataByGradeAndLevel(int level, int grade)
        {
            if (table == null)
                table = TableManager.GetTable<PetExpTable>();

            return table.GetExpDataByGradeAndLevel(level, grade);
        }
        static public Dictionary<string, int> GetLevelAddExp(int currentLevel, int currentExp, int obtainExp, int grade)
        {
            if (table == null)
                table = TableManager.GetTable<PetExpTable>();

            return table.GetLevelAddExp(currentLevel, currentExp, obtainExp, grade);
        }


        public string KEY { get { return UNIQUE_KEY; } }
        public int GRADE => Data.GRADE;
        public int LEVEL => Data.LEVEL;
        public int EXP => Data.EXP;
        public int TOTAL_EXP => Data.TOTAL_EXP;
        public int offer_exp => Data.OFFER_EXP;
    }
    public class PetReinforceData : TableData<DBPet_reinforce>
    {
        static private PetReinforceTable table = null;
        static public PetReinforceData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetReinforceTable>();

            return table.Get(key);
        }
        static public PetReinforceData GetDataByGradeAndStep(int grade, int step)
        {
            if (table == null)
                table = TableManager.GetTable<PetReinforceTable>();

            return table.GetDataByGradeAndStep(grade, step);
        }

        static public int GetMaxReinforceStep(int grade)
        {
            if (table == null)
                table = TableManager.GetTable<PetReinforceTable>();

            return table.GetMaxReinforceStep(grade);
        }

        public string KEY { get { return UNIQUE_KEY; } }
        public int GRADE => Data.GRADE;
        public int STEP => Data.STEP;
        public int ELEMENT_BUFF => Data.ELEMENT_BUFF;
        public int RATE => Data.RATE;
        public int ITEM => Data.ITEM;
        public int ITEM_NUM => Data.ITEM_NUM;
        public int RAISE_ITEM_NUM => Data.RAISE_ITEM_NUM;
        public int MAX_ITEM_NUM => Data.MAX_ITEM_NUM;
        public string COST_TYPE => Data.COST_TYPE;
        public int COST_NUM => Data.COST_NUM;
        public int LEVEL_BONUS => Data.LEVEL_BONUS;
    }
    public class PetSkillNormalData : TableData<DBPet_skill_normal>
    {
        static private PetSkillNormalTable table = null;
        static public PetSkillNormalData Get(int key)
        {
            return Get(key.ToString());
        }
        static public PetSkillNormalData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetSkillNormalTable>();

            return table.Get(key);
        }
        static public float GetSkillValue(int skillID, int level)
        {
            if (table == null)
                table = TableManager.GetTable<PetSkillNormalTable>();

            return table.GetSkillValue(skillID, level);
        }
        
        public string KEY { get { return UNIQUE_KEY; } }
        public int grade => Data.GRADE;
        public int name => Data._NAME;
        public int desc => Data._DESC;
        public string icon => Data.ICON;
        public string stat => Data.STAT;
        public float value => Data.VALUE;
        public float value_factor => Data.VALUE_FACTOR;
        public int next_gene_skill => Data.NEXT_GENE_SKILL;
        public int gene_weight => Data.GENE_WEIGHT;
    }
    public class PetElementData : TableData<DBPet_element>
    {
        static private PetElementTable table = null;
        static public PetElementData Get(int key)
        {
            return Get(key.ToString());
        }
        static public PetElementData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetElementTable>();

            return table.Get(key);
        }
        public string KEY { get { return UNIQUE_KEY; } }
        public int equal_pet_bonus_atk => Data.EQUAL_PET_BONUS_ATK;
        public int equal_pet_bonus_def => Data.EQUAL_PET_BONUS_DEF;
        public int equal_pet_bonus_hp => Data.EQUAL_PET_BONUS_HP;
    }
    public class PetMergeBaseData : TableData<DBPet_merge_base>
    {
        static private PetMergeBaseTable table = null;
        static public PetMergeBaseData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetMergeBaseTable>();

            return table.Get(key);
        }

        static public PetMergeBaseData GetMergeData(List<int> array)
        {
            if (table == null)
                table = TableManager.GetTable<PetMergeBaseTable>();

            return table.GetMergeData(array);
        }

        static public PetMergeBaseData GetByRewardGrade(int targetGrade)
        {
            if (table == null)
                table = TableManager.GetTable<PetMergeBaseTable>();
            return table.GetByRewardGrade(targetGrade);
        }

        public string KEY { get { return UNIQUE_KEY; } }
        public int material_grade => Data.MATERIAL_GRADE;
        public int merge_success_rate => Data.MERGE_SUCCESS_RATE;
        public int reward_grade => Data.REWARD_GRADE;
        public int success_reward_group => Data.SUCCESS_REWARD_GROUP;
        public int fail_reward_group => Data.FAIL_REWARD_GROUP;
        public string cost_type => Data.COST_TYPE;
        public int cost_num => Data.COST_NUM;
    }
    public class PetStatData : TableData<DBPet_stat>
    {
        static private PetStatTable table = null;
        static public PetStatData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetStatTable>();

            return table.Get(key);
        }
        static public PetStatData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<PetStatTable>();

            return table.Get(key);
        }

        static public List<PetStatData> GetAllForChampion()
        {
            if (table == null)
                table = TableManager.GetTable<PetStatTable>();

            var ret = new List<PetStatData>();
            foreach(var data in table.GetAllList())
            {
                if (data.RATE <= 0)
                    continue;

                ret.Add(data);
            }
            return ret;
        }
        public int KEY { get { return Int(UNIQUE_KEY); } }
        public string STAT_TYPE => Data.STAT_TYPE;
        public int GENO_GROUP => Data.GENO_GROUP;
        public int GENO_TYPE => Data.GENO_TYPE;
        public eStatusValueType VALUE_TYPE
        {
            get
            {
                switch(Data.VALUE_TYPE)
                {
                    case "PERCENT":
                        return eStatusValueType.PERCENT;
                    case "VALUE":
                        return eStatusValueType.VALUE;
                }

                return eStatusValueType.NONE;
            }
        }
        public float START_STAT_1 => Data.START_STAT_1;
        public float START_STAT_2 => Data.START_STAT_2;
        public float STAT_LEVEL_GROW => Data.STAT_LEVEL_GROW;
        public float STAT_REINFORCE_GROW => Data.STAT_REINFORCE_GROW;
        public int RATE => Data.RATE;  // 이 옵션이 뜰 확률 RATE = 400  => 4% 를 의미

        static public float GetStatValue(string StatKey, int level, int reinforce, bool isStartStat1 = true) 
        {
            if (table == null)
                table = TableManager.GetTable<PetStatTable>();

            float stat = 0f;
            if (table.ContainsKey(StatKey))
            {
                var statData = Get(StatKey);
                float startStat = isStartStat1 ? statData.START_STAT_1 : statData.START_STAT_2;

                float levelRate = 1.0f;
                float reinforeceRate = 1.0f;

                string optionKey = "";
                switch(statData.STAT_TYPE)
                {
                    case "RATIO_ATK_DMG":
                        optionKey = "damage";
                        break;
                    case "ADD_ATK_DMG":
                        optionKey = "add_damage";
                        break;
                    case "CRI_DMG":
                        optionKey = statData.VALUE_TYPE switch
                        {
                            eStatusValueType.PERCENT => "cri_ratio",
                            eStatusValueType.VALUE => "cri_damage",
                            _ => ""
                        };
                        break;
                    case "RATIO_SKILL_DMG":
                        //optionKey = "add_damage";
                        break;
                    case "ADD_SKILL_DMG":
                        //optionKey = "add_damage";
                        break;
                    case "LIGHT_DMG":
                        optionKey = statData.VALUE_TYPE switch
                        {
                            eStatusValueType.PERCENT => "ratio_light",
                            eStatusValueType.VALUE => "add_light",
                            _ => ""
                        };
                        break;
                    case "DARK_DMG":
                        optionKey = statData.VALUE_TYPE switch
                        {
                            eStatusValueType.PERCENT => "ratio_dark",
                            eStatusValueType.VALUE => "add_dark",
                            _ => ""
                        };
                        break;
                    case "WATER_DMG":
                        optionKey = statData.VALUE_TYPE switch
                        {
                            eStatusValueType.PERCENT => "ratio_water",
                            eStatusValueType.VALUE => "add_water",
                            _ => ""
                        };
                        break;
                    case "FIRE_DMG":
                        optionKey = statData.VALUE_TYPE switch
                        {
                            eStatusValueType.PERCENT => "ratio_fire",
                            eStatusValueType.VALUE => "add_fire",
                            _ => ""
                        };
                        break;
                    case "WIND_DMG":
                        optionKey = statData.VALUE_TYPE switch
                        {
                            eStatusValueType.PERCENT => "ratio_wind",
                            eStatusValueType.VALUE => "add_wind",
                            _ => ""
                        };
                        break;
                    case "EARTH_DMG":
                        optionKey = statData.VALUE_TYPE switch
                        {
                            eStatusValueType.PERCENT => "ratio_earth",
                            eStatusValueType.VALUE => "add_earth",
                            _ => ""
                        };
                        break;
                    case "RATIO_PVP_DMG":
                        optionKey = "ratio_pvp";
                        break;
                    case "ADD_PVP_DMG":
                        optionKey = "add_pvp";
                        break;
                    case "RATIO_PVP_CRI_DMG":
                        optionKey = "ratio_cri_pvp";
                        break;
                    case "ADD_PVP_CRI_DMG":
                        optionKey = "add_cri_pvp";
                        break;
                    case "BOSS_DMG":
                        optionKey = statData.VALUE_TYPE switch
                        {
                            eStatusValueType.PERCENT => "ratio_boss",
                            eStatusValueType.VALUE => "add_boss",
                            _ => ""
                        };
                        break;
                }
                if (!string.IsNullOrEmpty(optionKey))
                {
                    levelRate = ServerOptionData.GetJsonValueFloat(optionKey, "level", 1.0f);
                    reinforeceRate = ServerOptionData.GetJsonValueFloat(optionKey, "reinforce", 1.0f);
                }

                stat = startStat + ((level-1) * statData.STAT_LEVEL_GROW * levelRate) + (reinforce * statData.STAT_REINFORCE_GROW * reinforeceRate);
            }
            return stat;
        }
    }

    public class PetDecomposeData : TableData<DBPet_decompose>
    {
        static private PetDecomposeTable table = null;
        static public PetDecomposeData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<PetDecomposeTable>();

            return table.Get(key);
        }
        public string KEY { get { return UNIQUE_KEY; } }
        public int GRADE => Data.GRADE;
        public int REINFORCE => Data.REINFORCE;
        public int ITEM => Data.ITEM;
        public int ITEM_NUM => Data.ITEM_NUM;
        static public List<int> GetTotalResultItemList()
        {
            if (table == null)
                table = TableManager.GetTable<PetDecomposeTable>();

            return table.GetTotalResultItemList();
        }
        static public Asset GetResultItemData(int grade, int reinforce) // 등급 강화에 따라 분해했을때 주는 아이템 정보 // 아이템의 갯수와 아이템 카운트를 담는 Reward로 구성했음
        {
            if (table == null)
                table = TableManager.GetTable<PetDecomposeTable>();

            return table.GetResultItemData(grade, reinforce);
        }

    }
}