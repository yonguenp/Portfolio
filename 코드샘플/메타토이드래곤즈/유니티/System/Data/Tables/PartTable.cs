using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class PartTable : TableBase<PartBaseData, DBPart_base>
    {
        private Dictionary<int, List<PartBaseData>> dicGrade = new Dictionary<int, List<PartBaseData>>();
        Dictionary<string, List<PartBaseData>> dicGroup = new Dictionary<string, List<PartBaseData>>();

        public override void Init()
        {
            base.Init();
            dicGrade.Clear();
            dicGroup.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            dicGrade.Clear();
            dicGroup.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(PartBaseData data)
        {
            if (base.Add(data))
            {
                if (!dicGrade.ContainsKey(data.GRADE))
                    dicGrade.Add(data.GRADE, new List<PartBaseData>());
                if (data.ITEM != null && data.ITEM.USE)//안쓰는 장비가 있음.
                    dicGrade[data.GRADE].Add(data);

                if (data.ITEM != null && data.ITEM.USE)
                {
                    var key = data.STAT_TYPE;
                    if (!dicGroup.ContainsKey(key))
                        dicGroup[key] = new List<PartBaseData>();

                    dicGroup[key].Add(data);
                }

                return true;
            }

            return false;
        }
        public List<PartBaseData> GetGradeAll(int grade)
        {
            if (dicGrade.ContainsKey(grade))
                return dicGrade[grade];

            return new List<PartBaseData>();
        }

        public int GetMaxReinforceCount(string id)
        {
            return GetMaxReinforceCount(int.Parse(id));
        }
        public int GetMaxReinforceCount(int tag)
        {
            PartBaseData partInfo = Get(tag);
            int maxStep = 0;
            if (partInfo == null || partInfo.SUB_STEP == null)
                return maxStep;

            foreach (var step in partInfo.SUB_STEP)
            {
                if (step > 0)
                {
                    maxStep = step;
                }
            }

            return maxStep;
        }
        public int GetMaxReinforceSlotCount(string id)
        {
            return GetMaxReinforceSlotCount(int.Parse(id));
        }
        public int GetMaxReinforceSlotCount(int tag)
        {
            PartBaseData partInfo = Get(tag);
            List<int> maxSlot = new List<int>();
            if (partInfo == null || partInfo.SUB == null)
                return maxSlot.Count;

            foreach (var step in partInfo.SUB_STEP)
            {
                if (step > 0)
                {
                    maxSlot.Add(step);
                }
            }

            return maxSlot.Count;
        }

        public int GetCurrentReinforceSlotCount(object ID, int currentReinforcelevel)//현재 레벨에서 해금 가능한 슬롯 갯수
        {
            PartBaseData partInfo = Get((int)ID);
            int[] currentStepList = partInfo.SUB_STEP.ToArray();//6,0,0 // 6,9,0, // 6,9,12

            int availableSlotCount = 0;

            if (currentStepList == null || currentStepList.Length <= 0)
            {
                return availableSlotCount;
            }

            for (var i = 0; i < currentStepList.Length; i++)
            {
                var stepLevel = currentStepList[i];

                if (stepLevel <= 0)
                {
                    continue;
                }

                if (currentReinforcelevel >= stepLevel)
                {
                    availableSlotCount++;
                }
            }

            return availableSlotCount;
        }

        public PartBaseData GetBasePartFromStatType(string type)
        {
            if(dicGroup.ContainsKey(type))
            {
                if (dicGroup[type].Count > 0)
                {
                    //같은 스탯의 베이스 장비는 시트에 첫번째로 입력되었다고 약속하자.
                    //아닐경우 sort를 해줘야함.
                    return dicGroup[type][0];
                }
            }

            return null;
        }
    }

    public class PartSetTable : TableBase<PartSetData, DBPart_set>
    {
        public const int GroupSeparator = 100;
        Dictionary<int, List<PartSetData>> dicGroup = new Dictionary<int, List<PartSetData>>();

        public override void Init()
        {
            base.Init();
            dicGroup.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            dicGroup.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(PartSetData data)
        {
            if(base.Add(data))
            {
                if (!dicGroup.ContainsKey(data.GROUP))
                    dicGroup[data.GROUP] = new List<PartSetData>();

                dicGroup[data.GROUP].Add(data);
                return true;
            }
            return false;
        }
        public List<PartSetData> GetSetOptionListByGroup(int groupKey)
        {
            if (dicGroup.ContainsKey(groupKey))
                return dicGroup[groupKey].ToList();

            return new List<PartSetData>();
        }
        public List<PartSetData> GetSetOptionListByGroup(int _groupKey, int _count)//기준값보다 큰값이면 해당한다고 판단
        {
            if (dicGroup.ContainsKey(_groupKey))
            {
                var list = dicGroup[_groupKey];
                if (list == null || list.Count <= 0)
                    return null;

                List<PartSetData> tempData = new List<PartSetData>();
                foreach (var data in list)
                {
                    if (data == null)
                        continue;
                    var num = (int)data.NUM;
                    if (num <= _count)
                        tempData.Add(data);
                }
                return tempData;
            }

            return new List<PartSetData>();
        }
    }

    public class PartReinforceTable : TableBase<PartReinforceData, DBPart_reinforce>
    {
        Dictionary<int, List<PartReinforceData>> dicGrade = new Dictionary<int, List<PartReinforceData>>();

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
        protected override bool Add(PartReinforceData data)
        {
            if (base.Add(data))
            {
                if (!dicGrade.ContainsKey(data.GRADE))
                    dicGrade[data.GRADE] = new List<PartReinforceData>();

                dicGrade[data.GRADE].Add(data);
                return true;
            }
            return false;
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        public PartReinforceData GetDataByGradeAndStep(int grade, int step)
        {
            if (dicGrade.ContainsKey(grade))
            {
                var datas = dicGrade[grade];
                foreach (var data in datas)
                {
                    if (data.STEP == step)
                    {
                        return data;
                    }
                }
            }

            return null;
        }

        public int GetMaxReinforceStep(int grade)
        {
            var allDataList = new List<PartReinforceData>(datas.Values);

            var gradeList = allDataList.FindAll(part => part.GRADE == grade);

            return gradeList.Max(part => part.STEP);
        }
    }

    public class PartMergeBaseTable : TableBase<PartMergeBaseData, DBPart_merge_base>
    {
        Dictionary<int, List<PartMergeBaseData>> dicGrade = new Dictionary<int, List<PartMergeBaseData>>();

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
        protected override bool Add(PartMergeBaseData data)
        {
            if (base.Add(data))
            {
                if (!dicGrade.ContainsKey(data.GRADE))
                    dicGrade[data.GRADE] = new List<PartMergeBaseData>();

                dicGrade[data.GRADE].Add(data);
                return true;
            }
            return false;
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        public List<PartMergeBaseData> GetDataByGrade(int grade)
        {
            if (dicGrade.ContainsKey(grade))
            {
                return dicGrade[grade];
            }

            return null;
        }
    }

    public class PartMergeReinforceBonusTable : TableBase<PartMergeReinforceBonusData, DBPart_merge_reinforcebonus>
    {
        Dictionary<int, List<PartMergeReinforceBonusData>> dicGrade = new Dictionary<int, List<PartMergeReinforceBonusData>>();

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
        protected override bool Add(PartMergeReinforceBonusData data)
        {
            if (base.Add(data))
            {
                if (!dicGrade.ContainsKey(data.GRADE))
                    dicGrade[data.GRADE] = new List<PartMergeReinforceBonusData>();

                dicGrade[data.GRADE].Add(data);
                return true;
            }
            return false;
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        public int GetRateByGradeAndReinforceNum(int grade, int reinforce_num)
        {
            if (dicGrade.ContainsKey(grade))
            {
                foreach (var data in dicGrade[grade])
                {
                    if (data.REINFORCE_NUM == reinforce_num)
                        return data.ADD_RATE;
                }
            }

            return -1;
        }
    }

    public class PartMergeEquipAmountBonusTable : TableBase<PartMergeEquipAmountBonusData, DBPart_merge_equipamountbonus>
    {
        Dictionary<int, List<PartMergeEquipAmountBonusData>> dicGrade = new Dictionary<int, List<PartMergeEquipAmountBonusData>>();

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
        protected override bool Add(PartMergeEquipAmountBonusData data)
        {
            if (base.Add(data))
            {
                if (!dicGrade.ContainsKey(data.GRADE))
                    dicGrade[data.GRADE] = new List<PartMergeEquipAmountBonusData>();

                dicGrade[data.GRADE].Add(data);
                return true;
            }
            return false;
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        public int GetRateByGradeAndBonusAmountNum(int grade, int extra_num)
        {
            if (dicGrade.ContainsKey(grade))
            {
                foreach (var data in dicGrade[grade])
                {
                    if (data.EXTRA_NUM == extra_num)
                        return data.ADD_RATE;
                }
            }

            return -1;
        }
    }

    public class PartDecomposeTable : TableBase<PartDecomposeData, DBPart_decompose>
    {
        Dictionary<int, List<PartDecomposeData>> dicGrade = new Dictionary<int, List<PartDecomposeData>>();
        List<int> resultItems = new List<int>();

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
        protected override bool Add(PartDecomposeData data)
        {
            if (base.Add(data))
            {
                if (!dicGrade.ContainsKey(data.GRADE))
                    dicGrade[data.GRADE] = new List<PartDecomposeData>();

                dicGrade[data.GRADE].Add(data);
                if (!resultItems.Contains(data.ITEM))
                    resultItems.Add(data.ITEM);
                return true;
            }
            return false;
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }

        public PartDecomposeData GetDecomposeDataByGradeAndPartLevel(int grade, int partLevel)
        {
            if (dicGrade.ContainsKey(grade))
            {
                foreach (var data in dicGrade[grade])
                {
                    if (data.REINFORCE_NUM == partLevel)
                        return data;
                }
            }

            return null;
        }

        //분해로 획득 가능한 모든 아이템 리스트 가져오기
        public List<int> GetTotalResultItemList()
        {
            return resultItems;
        }
    }

    public class PartFusionTable : TableBase<PartFusionData, DBFusion_option>
    {
        
    }
}