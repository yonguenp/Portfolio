using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CollectionConditionInfo : CollectionAchievementConditionInfo
    {
        private List<int> currentList = new List<int>();
        public List<int> DragonList { get { return currentList; } }

        CollectionData tableData = null;
        public CollectionConditionInfo(CollectionData _tableData) : base()
        {
            tableData = _tableData;

            if (currentList == null)
                currentList = new List<int>();
            
            currentList.Clear();

            completeValue = tableData.CollectionIDList.Count;
        }

        public override void UpdateCondition(List<int> _currentList)//기본 리스트 + 업데이트값 누적으로 세팅
        {
            if (_currentList == null || _currentList.Count <= 0)
                return;

            foreach(var dID in _currentList)
            {
                var isContain = currentList.Contains(dID);
                if (isContain)
                    continue;
                currentList.Add(dID);
            }

            currentList.Sort();//오름차순

            if (currentList.Count < CompleteValue)
                SetCurrentValue(currentList.Count);
            else
                SetCurrentValue(CompleteValue);
        }
    }

    public class Collection : CollectionAchievement
    {
        public CollectionData CollectionBaseData { get; private set; } = null;
        public Collection(CollectionData _collection)
        {
            if (_collection == null)
                return;

            SetData(_collection);
        }

        public List<int> GetCurrentDragonList()
        {
            if (condition != null)
                return ((CollectionConditionInfo)condition).DragonList;
            else
                return new List<int>() { };
        }

        public override void SetData(ICollectionAchievementBaseData _tableData)
        {
            SetBase(_tableData);
            CollectionBaseData = (CollectionData)_tableData;
            condition = new CollectionConditionInfo(CollectionBaseData);
        }

        public override void UpdateCondition(List<int> cValue)
        {
            if (condition == null)
            {
                Debug.LogError("collection condition data is null");
                return;
            }
            ((CollectionConditionInfo)condition).UpdateCondition(cValue);
        }
    }
}
