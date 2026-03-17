using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace SandboxNetwork
{
    public class CharTranscendenceTable : TableBase<CharTranscendenceData, DBChar_transcendence>
    {
        private Dictionary<KeyValuePair<eDragonGrade, int>, CharTranscendenceData> transcendenceDic = null;
        private Dictionary<eDragonGrade, int> transcendenceStepDic = null;
        public override void Init()
        {
            base.Init();
            if (transcendenceDic == null)
                transcendenceDic = new();
            else
                transcendenceDic.Clear();
            if (transcendenceStepDic == null)
                transcendenceStepDic = new();
            else
                transcendenceStepDic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (transcendenceDic == null)
                transcendenceDic = new();
            else
                transcendenceDic.Clear();
            if (transcendenceStepDic == null)
                transcendenceStepDic = new();
            else
                transcendenceStepDic.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(CharTranscendenceData data)
        {
            if (base.Add(data))
            {
                if (transcendenceDic.TryAdd(new(data.TARGET_GRADE, data.STEP), data))
                {

                }
#if DEBUG
                else
                {
                    Debug.Log("중복된 TargetGrade 및 Step이 존재합니다.");
                }
#endif
                if (false == transcendenceStepDic.TryGetValue(data.TARGET_GRADE, out var step))
                {
                    transcendenceStepDic.Add(data.TARGET_GRADE, data.STEP);
                    step = data.STEP;
                }

                if (step < data.STEP)
                    transcendenceStepDic[data.TARGET_GRADE] = data.STEP;
                return true;
            }
            return false;
        }
        public CharTranscendenceData Get(eDragonGrade targetGrade, int step)
        {
            if (transcendenceDic == null)
                return null;

            if (transcendenceDic.TryGetValue(new(targetGrade, step), out var value))
                return value;

            return null;
        }
        public int GetStepMax(eDragonGrade targetGrade)
        {

            if (transcendenceStepDic == null)
                return 0;
            if (transcendenceStepDic.TryGetValue(targetGrade, out var value))
                return value;

            return 0;
        }

        public List<CharTranscendenceData> GetByGrade(eDragonGrade targetGrade)
        {
            List<CharTranscendenceData> ret = new List<CharTranscendenceData>();
            var keys = transcendenceDic.Keys;
            foreach (var key in keys)
            {
                if(key.Key == targetGrade)
                {
                    ret.Add(transcendenceDic[key]);
                }
            }
            return ret;
        }

        public int GetNewSkillSlotMinimumStep(eDragonGrade targetGrade)
        {
            var minimumStep = 2;
            var list = GetByGrade(targetGrade);
            foreach (var item in list)
            {
                if(item.SKILL_SLOT_MAX > 0)
                {
                    minimumStep = math.min(minimumStep, item.STEP);
                }
            }
            return minimumStep;
        }

    }
}