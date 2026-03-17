using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 업적 및 콜렉션 효과 스탯 보여주는 스크롤 뷰 (하위 스탯 터치 시 -> CollectionAchievementDataScrollview 가 터치한 스탯만 보여주는 필터 기능 있음.)
    /// </summary>
    public class CollectionAchievementEffectScrollview : MonoBehaviour, EventListener<CollectionAchievementUIEvent>
    {
        [SerializeField] GameObject clone = null;
        [SerializeField] ScrollRect scrollView = null;

        List<CollectionAchievementEffectDataClone> cloneList = new List<CollectionAchievementEffectDataClone>();
        eCollectionAchievementType currentTabType = eCollectionAchievementType.NONE;
        List<CollectionAchievement> completeStatList = new List<CollectionAchievement>();//각 타입 (업적 & 콜렉션 별 세팅)            
        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }
        public void OnEvent(CollectionAchievementUIEvent eventType)
        {
            switch (eventType.Event)
            {
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.TOUCH_EFFECT://효과 클릭 시.
                    RefreshScrollClick(eventType.statType, eventType.statValueType, eventType.isButtonVisible);
                    break;
            }
        }

        public void InitUI(eCollectionAchievementType _tabType)//기본 스탯 일단 쭉 깔아놓음(statType Data 에 모든 raw 일단 깔아놓고, 빼놓을건 빼달라고 요청)
        {
            currentTabType = _tabType;
            completeStatList = GetCompleteStatByType();

            if (cloneList == null)
                cloneList = new List<CollectionAchievementEffectDataClone>();
            cloneList.Clear();

            SBFunc.RemoveAllChildrens(scrollView.content.transform);

            var list = CollectionAchievementManager.Instance.GetStatTypeListByType(_tabType);
            var sortExcludeList = list.FindAll(element => StatTypeData.Get(element.Key) != null && StatTypeData.Get(element.Key).SORT_GROUP != 0);//0인 것은 출력안하게.

            var valueList = sortExcludeList.FindAll(element => element.Value == eStatusValueType.VALUE);
            var percentList = sortExcludeList.FindAll(element => element.Value == eStatusValueType.PERCENT);

            valueList.Sort(SortComparison());
            percentList.Sort(SortComparison());

            valueList.AddRange(percentList);

            foreach (var typeData in valueList)
            {
                var effectClone = Instantiate(clone, scrollView.content.transform);
                var dataClone = effectClone.GetComponent<CollectionAchievementEffectDataClone>();
                if (dataClone == null)
                {
                    Destroy(effectClone);
                    continue;
                }

                dataClone.InitUI(typeData.Key, typeData.Value, GetCompleteStatBuff(typeData), currentTabType);//complete 갱신 추가
                cloneList.Add(dataClone);
            }
            scrollView.verticalNormalizedPosition = 1f;//초기 상태는 맨 윗단으로
        }

        System.Comparison<KeyValuePair<eStatusType, eStatusValueType>> SortComparison()
        {
            return Sort;
        }

        private int Sort(KeyValuePair<eStatusType, eStatusValueType> ad, KeyValuePair<eStatusType, eStatusValueType> bd)
        {
            var a = StatTypeData.Get(ad.Key);
            var b = StatTypeData.Get(bd.Key);

            var sortValue = a.SORT_GROUP - b.SORT_GROUP;
            if (sortValue == 0)
                return int.Parse(a.KEY) - int.Parse(b.KEY);
            else
                return sortValue;
        }

        void RefreshScrollClick(eStatusType _type, eStatusValueType _valueType, bool _isVisible)//효과 눌렀을 때 처리할 부분
        {
            SetAllCloneVisibleOff();//일단 모든 클론 전부 끄기
            RefreshEffectClone(_type, _valueType, _isVisible);//들어온것 하나만 체크

            eStatusType tempType = _isVisible ? _type : eStatusType.NONE;

            CollectionAchievementUIEvent.SendFilterScrollView(tempType, _valueType);
        }

        void RefreshEffectClone(eStatusType _type, eStatusValueType _valueType, bool _isVisible)
        {
            var selectClone = GetCloneByType(_type, _valueType);
            if(selectClone != null)
            {
                selectClone.RefreshUI(_isVisible);
            }
        }

        void SetAllCloneVisibleOff()
        {
            if (cloneList == null || cloneList.Count <= 0)
                return;

            foreach (var clone in cloneList)
            {
                if (clone == null)
                    continue;
                clone.RefreshUI(false);
            }
        }

        CollectionAchievementEffectDataClone GetCloneByType(eStatusType _type, eStatusValueType _valueType)
        {
            if (cloneList == null || cloneList.Count <= 0)
                return null;

            foreach(var clone in cloneList)
            {
                if (clone == null)
                    continue;
                if (clone.StatType == _type)
                {
                    if(clone.StatValueType == _valueType)
                        return clone;
                }
            }
            return null;
        }


        public List<CollectionAchievement> GetCompleteStatByType()//완료 한 스탯 버프 가져오기
        {
            return CollectionAchievementManager.Instance.GetCompleteDataByType(currentTabType);
        }

        public float GetCompleteStatBuff(KeyValuePair<eStatusType, eStatusValueType> keyValuePair)
        {
            float returnValue = 0;

            foreach(var completeData in completeStatList)
            {
                if (completeData == null)
                    continue;
                if (completeData.StatType == keyValuePair.Key && completeData.StatValueType == keyValuePair.Value)
                    returnValue += (completeData.RewardValue);
            }

            return returnValue;
        }
    }
}

