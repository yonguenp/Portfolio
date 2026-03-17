using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class StageTable : TableBase<StageBaseData, DBStage_base>
    {
        private Dictionary<StageDifficult, Dictionary<int, Dictionary<int, StageBaseData>>> dicWorldStage = new Dictionary<StageDifficult, Dictionary<int, Dictionary<int, StageBaseData>>>();
        private Dictionary<StageDifficult, Dictionary<int, int>> dicStageCount = new Dictionary<StageDifficult, Dictionary<int, int>>();

        public override void Init()
        {
            base.Init();
        }
        public override void DataClear()
        {
            base.DataClear();

            dicWorldStage.Clear();
            dicStageCount.Clear();
        }

        public override void Preload()
        {
            base.Preload();

            LoadAll();

            foreach(var data in datas.Values)
            {
                if (!dicStageCount.ContainsKey(data.DIFFICULT))
                    dicStageCount[data.DIFFICULT] = new Dictionary<int, int>();

                if (!dicStageCount[data.DIFFICULT].ContainsKey(data.WORLD))
                    dicStageCount[data.DIFFICULT].Add(data.WORLD, 0);

                dicStageCount[data.DIFFICULT][data.WORLD] += 1;

                if (!dicWorldStage.ContainsKey(data.DIFFICULT))
                    dicWorldStage.Add(data.DIFFICULT, new Dictionary<int, Dictionary<int, StageBaseData>>());

                if (!dicWorldStage[data.DIFFICULT].ContainsKey(data.WORLD))
                    dicWorldStage[data.DIFFICULT].Add(data.WORLD, new Dictionary<int, StageBaseData>());

                if (!dicWorldStage[data.DIFFICULT][data.WORLD].ContainsKey(data.STAGE))
                    dicWorldStage[data.DIFFICULT][data.WORLD].Add(data.STAGE, data);
                else
                {
#if DEBUG || UNITY_EDITOR
                    UnityEngine.Debug.LogError("중복 데이터 들어감. 중복 삽입 key : " + data.KEY + " 현재 있는 데이터 key : " + dicWorldStage[data.DIFFICULT][data.WORLD][data.STAGE].KEY);
#endif
                }
            }
        }

        public List<StageBaseData> GetByWorld(int world, StageDifficult difficult = StageDifficult.NORMAL)
        {
            if (dicWorldStage.ContainsKey(difficult))
            {
                if (dicWorldStage[difficult].ContainsKey(world))
                {
                    return dicWorldStage[difficult][world].Values.ToList();
                }
            }

            return new List<StageBaseData>();
        }

        public StageBaseData GetByWorldStage(int world, int stage, StageDifficult difficult = StageDifficult.NORMAL)
        {
            if (dicWorldStage.ContainsKey(difficult))
            {
                if (dicWorldStage[difficult].ContainsKey(world))
                {
                    if (dicWorldStage[difficult][world].ContainsKey(stage))
                    {
                        return dicWorldStage[difficult][world][stage];
                    }
                }
            }

            return null;
        }

        public List<StageBaseData> GetDailyStageByDay(int day)
        {
            List<StageBaseData> ret = new List<StageBaseData>();
            foreach (var data in GetByWorld(day + 100))
            {
                if (data.TYPE == eStageType.DAILY_DUNGEON)
                {
                    ret.Add(data);
                }
            }
            return ret;
        }

        public List<StageBaseData> GetNormalStage(StageDifficult diff = StageDifficult.NORMAL)
        {
            List<StageBaseData> ret = new List<StageBaseData>();
            foreach (KeyValuePair<string, StageBaseData> element in datas)
            {
                if (element.Value.TYPE == eStageType.ADVENTURE && diff == element.Value.DIFFICULT)
                {
                    ret.Add(element.Value);
                }
            }
            return ret;
        }


        public List<StageBaseData> GetDailyStage()
        {
            List<StageBaseData> ret = new List<StageBaseData>();
            foreach (KeyValuePair<string, StageBaseData> element in datas)
            {
                if (element.Value.TYPE == eStageType.DAILY_DUNGEON)
                {
                    ret.Add(element.Value);
                }
            }
            return ret;
        }

        static public int GetWorldStageCount(int world, StageDifficult difficult = StageDifficult.NORMAL)
        {
            var instance = TableManager.GetTable<StageTable>();

            if (instance.dicWorldStage.ContainsKey(difficult))
            {
                if (instance.dicStageCount[difficult].ContainsKey(world))
                    return instance.dicStageCount[difficult][world];
            }
            return 0;
        }
    }
}