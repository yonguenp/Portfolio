using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 컬렉션 항목 터치 하면 드래곤 목록 뜨는 부분
    /// </summary>
    public class CollectionDetailDataScrollView : MonoBehaviour, EventListener<CollectionAchievementUIEvent>
    {
        [SerializeField] ScrollRect scrollRect = null;
        [SerializeField] GameObject dragonPrefab = null;
        [SerializeField] List<DragonPortraitFrame> dragonDataList = new List<DragonPortraitFrame>();//15개 일단 넣어버림
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
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.INIT_DETAIL_SCROLLVIEW:
                    InitDragonPortrait();
                    break;
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.TOUCH_DATA:
                    var cID = eventType.Key;
                    var data = CollectionAchievementManager.Instance.GetCollectionDataByKey(cID);
                    if (data == null)
                        return;
                    RefreshDetailScroll(data);
                    break;
            }
        }

        void InitDragonPortrait()
        {
            if (dragonDataList == null || dragonDataList.Count <= 0)
                return;

            foreach (var dragonPortrait in dragonDataList)
                if (dragonPortrait != null)
                    dragonPortrait.gameObject.SetActive(false);
        }
        void RefreshDetailScroll(Collection _data)
        {
            var dragonList = _data.CollectionBaseData.CollectionIDList;//드래곤 리스트(전체 테이블)
            var currentDragonList = _data.GetCurrentDragonList();//보유 드래곤 상태 인덱스 리스트
            if (dragonList == null || dragonList.Count <= 0)
                return;

            InitDragonPortrait();

            //현재 데이터가 이미 획득된 상태면 서버에서 complete 처리 나기 때문에 강제로켜기
            bool isClearData = CollectionAchievementManager.Instance.IsCompleteUserData(eCollectionAchievementType.COLLECTION, _data.KEY);

            int index = 0;
            foreach (var dID in dragonList)
            {
                if (dID <= 0)
                    continue;

                var dragonComp = dragonDataList[index];
                dragonComp.gameObject.SetActive(true);
                dragonComp.SetCustomPotraitFrame(dID, 0);
                dragonComp.SetVisibleLockNode(isClearData ? false : !currentDragonList.Contains(dID));//(임시) 일단 미등록 드래곤은 딤드
                dragonComp.setCallback((param) =>
                {
                    ShowNameTagPopup(param, dragonComp.gameObject);
                });
                index++;
            }
        }

        void ShowNameTagPopup(string dID, GameObject _parent)
        {
            var dTag = int.Parse(dID);
            if (dTag <= 0)
                return;

            var dragonData = CharBaseData.Get(dTag);
            if (dragonData == null)
                return;

            NameTagToolTip.OpenPopup(StringData.GetStringByStrKey(dragonData._NAME), _parent);
        }
    }
}