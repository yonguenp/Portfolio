using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /// <summary>
    /// 현재 컬렉션의 진행상태
    /// </summary>
    public class CollectionAchievementConditionInfo
    {
        protected int completeValue;
        public int CompleteValue { get { return completeValue; } }
        private int currentValue;//서버 데이터에 의존하는게 맞는지?
        public int CurrentValue { get { return currentValue; } }

        public CollectionAchievementConditionInfo()
        {
            completeValue = 0;
            currentValue = 0;
        }
        public void SetCurrentValue(int _currentValue)
        {
            currentValue = _currentValue;
        }

        public virtual void UpdateCondition(int _currentValue)
        {
            if (_currentValue < completeValue)
                currentValue = _currentValue;
            else
                currentValue = completeValue;
        }
        public virtual void UpdateCondition(List<int> _currentList) { }//기본 리스트 + 업데이트값 누적으로 세팅
        
        public string GetCurrentConditionDesc()//진행상황
        {
            return SBFunc.StrBuilder(currentValue, "/", completeValue);
        }
    }
    /// <summary>
    /// collection 과 achievement 데이터 객체(진행도 및 테이블 데이터 , 상태 저장 객체)의 형태가 비슷해서 통합으로 묶음
    /// </summary>
    public abstract class CollectionAchievement : ITableData
    {
        private ICollectionAchievementBaseData baseData = null;
        public int KEY { get { return baseData.KEY; } }//default : 테이블 데이터의 키값
        public string NameKey { get { return baseData._NAME; } }//subject stringKey
        public eStatusType StatType { get { return baseData.REWARD_STAT_TYPE; } }
        public eStatusValueType StatValueType { get { return baseData.REWARD_STAT_VALUE_TYPE; } }
        public float RewardValue { get { return baseData.REWARD_STAT_VALUE; } }//보상 버프 수치

        public CollectionAchievementConditionInfo condition;
        public int CurrentValue { get {
                if (condition == null)
                    return 0;
                return condition.CurrentValue;
            } }

        public string GetKey() { return KEY.ToString(); }

        public virtual void Init() { }

        public virtual void SetData(ICollectionAchievementBaseData _tableData)
        {
            SetBase(_tableData);
            condition = new CollectionAchievementConditionInfo();
        }

        protected void SetBase(ICollectionAchievementBaseData _tableData)
        {
            baseData = _tableData;
        }

        public virtual void UpdateCondition(int cValue) 
        {
            if (condition == null)
            {
                Debug.LogError("collection condition data is null");
                return;
            }
            condition.UpdateCondition(cValue);
        }
        public virtual void UpdateCondition(List<int> cList) { }
        
        public string GetCurrentConditionToString()//진행상황 string 받기
        {
            if (condition == null)
                return "";
            return condition.GetCurrentConditionDesc();
        }
        public string GetRewardValueToString()//버프 라벨 string 변환
        {
            var statData = StatTypeData.Get(StatType);
            if (statData == null)
                return "";
            var statPostFix = statData.VALUE_TYPE switch
            {
                eStatusValueType.PERCENT => "%",
                eStatusValueType.VALUE => "",
                _ => ""
            };

            string preFix = "";
            if(baseData != null)
            {
                statPostFix = baseData.REWARD_STAT_VALUE_TYPE switch
                {
                    eStatusValueType.PERCENT => "%",
                    eStatusValueType.VALUE => "",
                    _ => ""
                };

                preFix = baseData.REWARD_STAT_VALUE_TYPE switch
                {
                    eStatusValueType.PERCENT => statData.PERCENT_DESC,
                    eStatusValueType.VALUE => statData.VALUE_DESC,
                    _ => ""
                };
            }

            return SBFunc.StrBuilder(preFix, " +", RewardValue, statPostFix);
        }
    }
}
