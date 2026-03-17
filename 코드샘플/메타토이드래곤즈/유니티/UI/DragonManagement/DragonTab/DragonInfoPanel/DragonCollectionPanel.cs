using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 드래곤 Info쪽에서 스토리 버튼 클릭하면 
 * 드래곤의 이야기 서브 팝업 나옴
 */
namespace SandboxNetwork
{
    public class DragonCollectionPanel : DragonManageSubPanel, EventListener<CollectionAchievementUIEvent>
    {
        [SerializeField]
        Text collectionCount = null;
        [SerializeField]
        CollectionAchievementDataClone collection_clone = null;
        [SerializeField]
        DragonPortraitFrame dragon_clone = null;
        [SerializeField]
        GameObject emptyPanel = null;

        Dictionary<int, Collection> dragon_collection = new Dictionary<int, Collection>();
        int currentSelectKey = 0;
        List<CollectionAchievementDataClone> childs = new List<CollectionAchievementDataClone>();
        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        public override void ShowPanel(VoidDelegate _successCallback = null)
        {
            base.ShowPanel(_successCallback);
        }

        public override void HidePanel()
        {
            base.HidePanel();
        }

        public override void Init()
        {
            SetData();
            dragon_collection.Clear();
            var collections = CollectionAchievementManager.Instance.GetTotalDataByType(eCollectionAchievementType.COLLECTION);

            foreach (Collection col in collections)
            {
                if (col.CollectionBaseData.CollectionIDList.Contains(dragonBase.KEY))
                {
                    dragon_collection.Add(col.KEY, col);
                }
            }

            emptyPanel.SetActive(dragon_collection.Count == 0);

            base.Init();

            RefreshUI();
        }

        void RefreshUI()
        {
            childs.Clear();

            foreach (Transform child in collection_clone.transform.parent)
            {
                if (collection_clone.transform == child)
                    continue;

                Destroy(child.gameObject);
            }

            int clearCount = 0;
            collection_clone.gameObject.SetActive(true);
            foreach (var collection in dragon_collection)
            {
                var node = Instantiate(collection_clone.gameObject, collection_clone.transform.parent);

                var clone = node.GetComponent<CollectionAchievementDataClone>();
                if (clone == null)
                    return;

                var data = (CollectionAchievement)collection.Value;
                clone.InitUI(eCollectionAchievementType.COLLECTION, data);
                
                childs.Add(clone);

                if(CollectionAchievementManager.Instance.IsCompleteUserData(eCollectionAchievementType.COLLECTION, collection.Key))
                {
                    clearCount++;
                }
            }


            collection_clone.gameObject.SetActive(false);
            collectionCount.text = SBFunc.StrBuilder(clearCount, "/", dragon_collection.Count);

            InitDragonPortrait();
            RefreshSelect();
        }

        void RefreshSelect()
        {
            foreach(var child in childs)
            {
                child.SetVisibleSelectNode(child.KEY == currentSelectKey);
            }
        }


        public void OnEvent(CollectionAchievementUIEvent eventType)
        {
            switch (eventType.Event)
            {
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.INIT_DETAIL_SCROLLVIEW:
                    InitDragonPortrait();
                    break;
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.TOUCH_DATA://데이터 클릭하면 드로우
                    currentSelectKey = eventType.Key;
                    RefreshSelect();
                    InitDragonPortrait();
                    break;
            }
        }

        void InitDragonPortrait()
        {
            foreach (Transform child in dragon_clone.transform.parent)
            {
                if (dragon_clone.transform == child)
                    continue;

                Destroy(child.gameObject);
            }

            dragon_clone.gameObject.SetActive(false);
            if (!dragon_collection.ContainsKey(currentSelectKey))
                return;

            var curData = dragon_collection[currentSelectKey];
            var dragonDataList = curData.CollectionBaseData.CollectionIDList;

            if (dragonDataList == null || dragonDataList.Count <= 0)
                return;

            //현재 데이터가 이미 획득된 상태면 서버에서 complete 처리 나기 때문에 강제로켜기
            bool isClearData = CollectionAchievementManager.Instance.IsCompleteUserData(eCollectionAchievementType.COLLECTION, currentSelectKey);
            var currentDragonList = curData.GetCurrentDragonList();
            currentDragonList.Sort();
            dragon_clone.gameObject.SetActive(true);
            foreach (var dno in dragonDataList)
            {
                if (dno <= 0)
                    continue;

                var node = Instantiate(dragon_clone.gameObject, dragon_clone.transform.parent);

                var dragonComp = node.GetComponent<DragonPortraitFrame>();
                if (dragonComp == null)
                    return;


                dragonComp.SetCustomPotraitFrame(dno, 0);
                dragonComp.SetVisibleLockNode(isClearData ? false : !currentDragonList.Contains(dno));//(임시) 일단 미등록 드래곤은 딤드
                dragonComp.setCallback((param) =>
                {
                    ShowNameTagPopup(param, dragonComp.gameObject);
                });
            }

            dragon_clone.gameObject.SetActive(false);

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
