using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SandboxNetwork
{
    public class PetTable : TableBase<PetBaseData, DBPet_base>
    {
        private Dictionary<int, List<PetBaseData>> dicGrade = new Dictionary<int, List<PetBaseData>>();
        public override void Init()
        {
            base.Init();
            dicGrade.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            dicGrade.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(PetBaseData data)
        {
            if (base.Add(data))
            {
                if (!dicGrade.ContainsKey(data.GRADE))
                    dicGrade.Add(data.GRADE, new List<PetBaseData>());

                dicGrade[data.GRADE].Add(data);
                return true;
            }
            return false;
        }

        public List<PetBaseData> GetGradeAll(int grade)
        {
            if(dicGrade.ContainsKey(grade))
                return dicGrade[grade];

            return new List<PetBaseData>();
        }
    }
    public class PetGradeTable : TableBase<PetGradeData, DBPet_grade>
    {
    }
    public class PetExpTable : TableBase<PetExpData, DBPet_exp>
    {
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }

        public Dictionary<string,int> GetLevelAddExp(int currentLevel, int currentExp, int obtainExp, int grade)
        {
            int petMaxLv = GameConfigTable.GetPetLevelMax(grade);
            PetExpData levelData;
            if (currentLevel > petMaxLv)
            {
                levelData = GetExpDataByGradeAndLevel(petMaxLv, grade);
            }
            else
            {
                levelData = GetExpDataByGradeAndLevel(currentLevel, grade);
            }
    
            Dictionary<string, int> returnData = new Dictionary<string, int>();
            if (currentLevel >= petMaxLv)
            {
                returnData.Add("finallevel", petMaxLv);
                returnData.Add("reduceExp", this.GetExpDataByGradeAndLevel(petMaxLv, grade).EXP);
                returnData.Add("overExp", obtainExp);
                return returnData;
            }
            if (levelData == null)
            {
                return returnData;
            }
            int requireExp = levelData.EXP;

            
            int currentLevelRequireExp = requireExp - currentExp;
            if (requireExp == 0)//최대값 도달
            {
                returnData.Add("finallevel", petMaxLv);
                returnData.Add("reduceExp", this.GetExpDataByGradeAndLevel(petMaxLv, grade).EXP);
                returnData.Add("overExp", obtainExp);
                return returnData;
            }
            if (obtainExp >= currentLevelRequireExp)
            {
                return this.GetLevelAddExp(currentLevel + 1, 0, obtainExp - currentLevelRequireExp, grade);
            }
            else
            {
                returnData.Add("finallevel", currentLevel);
                returnData.Add("reduceExp", obtainExp);
                return returnData;
            }
        }



        public int GetCurrentRequireLevelExp(int level, int grade)
        {
            PetExpData levelData = this.GetExpDataByGradeAndLevel(level, grade);
            if (levelData != null) return levelData.EXP;
            return 0;
        }

        public int GetCurrentAccumulateLevelExp(int level, int grade)
        {
            PetExpData levelData = this.GetExpDataByGradeAndLevel(level, grade);
            if (levelData != null) return levelData.TOTAL_EXP;
            return 0;
        }

        public Dictionary<string, int> GetLevelAndExpByTotalExp(int inComeTotalExp, int grade)
        {
            List<PetExpData> data = new List<PetExpData>();
            Dictionary<string, int> returnData = new Dictionary<string, int>();
            foreach (KeyValuePair<string, PetExpData> element in datas)
            {
                PetExpData Petdata = element.Value;
                if (Petdata.GRADE == grade)
                {
                    data.Add(Petdata);
                }
            }
            if (data.Count<=0||data==null) return returnData;
            int checkIndex = 0;
            bool isSearch = false;
            for(int i = 0; i < data.Count; ++i)
            {
                if (isSearch) break;
                int totalExp = data[i].TOTAL_EXP;
                int level = data[i].LEVEL;
                if (totalExp > inComeTotalExp)
                {
                    checkIndex = level;
                    isSearch = true;
                    break;
                }
            }

            int expectLevel = checkIndex - 1;
            if (expectLevel < 0)
            {
                returnData.Add("finallevel", GameConfigTable.GetPetLevelMax(grade));
                returnData.Add("reduceExp", 0);
                return returnData;
            }
            PetExpData expectData = this.GetExpDataByGradeAndLevel(expectLevel, grade);
            if (expectData==null)
            {
                return null;
            }
            int expectEXP = inComeTotalExp - expectData.TOTAL_EXP;
            returnData.Add("finallevel", expectLevel);
            returnData.Add("reduceExp", expectEXP);
            return returnData;
        }

        public PetExpData GetExpDataByGradeAndLevel(int level, int grade)
        {
            PetExpData data = null;
            foreach (KeyValuePair<string, PetExpData> element in this.datas)
            {
                PetExpData Petdata = element.Value;
                if (Petdata.LEVEL == level && Petdata.GRADE == grade)
                {
                    data = Petdata;
                    break;
                }
            }
            return data;

        }
    }
    public class PetReinforceTable : TableBase<PetReinforceData, DBPet_reinforce>
    {
        private Dictionary<int, Dictionary<int, PetReinforceData>> dicGradeLevel = new Dictionary<int, Dictionary<int, PetReinforceData>>();
        public override void Init()
        {
            base.Init();
            dicGradeLevel.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            dicGradeLevel.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(PetReinforceData data)
        {
            if (base.Add(data))
            {
                if (!dicGradeLevel.ContainsKey(data.GRADE))
                    dicGradeLevel.Add(data.GRADE, new Dictionary<int, PetReinforceData>());

                dicGradeLevel[data.GRADE].Add(data.STEP, data);
                return true;
            }
            return false;
        }
        public PetReinforceData GetDataByGradeAndStep(int grade, int step)
        {
            if(dicGradeLevel.ContainsKey(grade))
            {
                if (dicGradeLevel[grade].ContainsKey(step))
                    return dicGradeLevel[grade][step];
            }

            return null;
        }
        public int GetMaxReinforceStep(int grade)
        {
            var allDataList = new List<PetReinforceData>(datas.Values);

            var gradeList = allDataList.FindAll(part => part.GRADE == grade);

            return gradeList.Max(part => part.STEP);
        }
    }

    public class PetSkillNormalTable : TableBase<PetSkillNormalData, DBPet_skill_normal>
    {
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        public float GetSkillValue(int skillID, int level)
        {
            PetSkillNormalData skillNormalData = Get(skillID);
            if (skillNormalData==null) return 0;
            float value = skillNormalData.value;
            float value_factor = skillNormalData.value_factor;
            return value + (value_factor * (level - 1));
        }
        public List<PetSkillNormalData> GetSkillDataByGrade(int _grade)
        {
            return datas.Select(dic => dic.Value).Where(dic => dic.grade == _grade).ToList();
        }
    }

    public class PetElementTable : TableBase<PetElementData, DBPet_element>
    {
    }
    public class PetMergeBaseTable : TableBase<PetMergeBaseData, DBPet_merge_base>
    {
        public PetMergeBaseData GetMergeData(List<int> array)
        {
            if (array == null || array.Count < 1)
                return null;

            var list = GetAllList();
            if (list != null)
                return list.Find(element => element.material_grade == array[0]);

            return null;
        }

        public PetMergeBaseData GetByRewardGrade(int rewardGrade)
        {
            var list = GetAllList();
            if (list != null)
                return list.Find(element => element.reward_grade == rewardGrade);

            return null;
        }

    }
    public class PetStatTable : TableBase<PetStatData, DBPet_stat>
    {
    }

    public class PetDecomposeTable : TableBase<PetDecomposeData, DBPet_decompose>
    {
        List<int> resultItems = null;

        public Asset GetResultItemData(int grade, int reinforce)
        {
            var list = GetAllList();
            if(list != null)
            {
                var find = list.Find(data => data.GRADE == grade && data.REINFORCE == reinforce);
                if (find != null)
                    return new Asset(find.ITEM, find.ITEM_NUM);
            }

            return null;
        }
        public List<int> GetTotalResultItemList()
        {
            if(resultItems == null || resultItems.Count < 1)
            {
                var list = GetAllList();
                if (list != null)
                {
                    resultItems = new List<int>();
                    foreach (var data in list)
                    {
                        if (resultItems.Contains(data.ITEM)) 
                            continue;

                        resultItems.Add(data.ITEM);
                    }
                }
            }
            return resultItems;
        }
    }
}
