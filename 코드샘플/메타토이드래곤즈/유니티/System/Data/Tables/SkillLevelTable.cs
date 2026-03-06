using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class SkillLevelTable : TableBase<SkillLevelData, DBSkill_level>
    {
        private Dictionary<eJobType, List<SkillLevelData>> jobDic = null;
        private Dictionary<KeyValuePair<eJobType, int>, SkillLevelData> customDic = null;

        public override void Init()
        {
            base.Init();
            if (customDic == null)
                customDic = new();
            else
                customDic.Clear();
            if (jobDic == null)
                jobDic = new();
            else
                jobDic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (customDic == null)
                customDic = new();
            else
                customDic.Clear();
            if (jobDic == null)
                jobDic = new();
            else
                jobDic.Clear();
        }
        public int CalculateMaxLevel(UserDragon dragon)
        {
            int curLevel = dragon.SLevel;
            Dictionary<int, Item> needItems = new Dictionary<int, Item>();
            Dictionary<string, int> needCashs = new Dictionary<string, int>();

            var allExpData = GetDatasByJob(dragon.BaseData.JOB);
            allExpData.Sort((e1, e2) =>
            {
                return e1.LEVEL - e2.LEVEL;
            });

            for (var i = 0; i < allExpData.Count; ++i)
            {
                var data = allExpData[i];
                if (data == null)// || data.LEVEL <= curLevel //현재 레벨에서 레벨업에 필요한 계산 추가로 들어가야함
                {
                    continue;
                }

                if (curLevel - 1 > i)//드래곤 스킬레벨 기준 이전 인덱스 연산은 continue 처리해야할 거 같은데 왜 안해놨는지 모르겠어서 해놓음
                {
                    continue;
                }

                int needItemNo = data.ITEM;
                int needItemAmount = data.ITEM_NUM;

                if (needItemNo < 0 || needItemAmount < 0)
                    continue;

                if (!needItems.ContainsKey(needItemNo))
                    needItems.Add(needItemNo, new Item(needItemNo, needItemAmount));
                else
                    needItems[needItemNo].AddCount(needItemAmount);

                if (!CheckItems(needItems.Values.ToList()))
                    break;

                //스킬랩업 골드 소모 제거되었음
                //if (!needCashs.ContainsKey(data.SKILL_LEVEL_COST_TYPE))
                //{
                //    needCashs.Add(data.SKILL_LEVEL_COST_TYPE, data.SKILL_LEVEL_COST_VALUE);
                //}
                //else
                //{
                //    needCashs[data.SKILL_LEVEL_COST_TYPE] += data.SKILL_LEVEL_COST_VALUE;
                //}
                //if (!CheckCashes(needCashs) || !CheckItems(needItems.Values.ToList()))
                //    break;

                curLevel++;
            }

            int maxLevel = curLevel > dragon.Level ? dragon.Level : curLevel;

            return maxLevel;
        }

        bool CheckItems(List<Item> needItems)
        {
            foreach (var Item in needItems)
            {
                if (Item.Amount > User.Instance.GetItemCount(Item.ItemNo))
                    return false;
            }

            return true;
        }

        public List<SkillLevelData> GetDatasByJob(eJobType job)
        {
            if (!jobDic.ContainsKey(job))
                return null;

            return jobDic[job];
        }

        public SkillLevelData GetDataByJobAndLevel(eJobType job, int level)
        {
            var key = new KeyValuePair<eJobType, int>(job, level);
            if (!customDic.ContainsKey(key))
                return null;

            return customDic[key];
        }

        public override void Preload()
        {
            base.Preload();
            LoadAll();

            foreach (var data in datas.Values)
            {
                if (!jobDic.ContainsKey(data.JOB))
                    jobDic.Add(data.JOB, new());

                jobDic[data.JOB].Add(data);
                customDic.Add(new(data.JOB, data.LEVEL), data);
            }
        }        
    }
}