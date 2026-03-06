using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 필터 갱신 리스트 - 전체 / 미완료 / 완료 필터 보기 - 단순 UI 갱신
    /// </summary>
    public class CollectionAchievementFilter : MonoBehaviour, EventListener<CollectionAchievementUIEvent>
    {
        [SerializeField] List<Button> buttonList = new List<Button>();


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
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.TOUCH_FILTER:
                    var filterType = eventType.filterType;
                    RefreshButtonUI((int)filterType - 1);
                    break;
            }
        }

        void RefreshButtonUI(int _type)
        {
            if (buttonList == null || buttonList.Count <= 0)
                return;

            if (_type < 0 || _type >= buttonList.Count)
                return;

            for(int i = 0; i< buttonList.Count; i++)
            {
                var button = buttonList[i];
                if (button == null)
                    continue;

                button.interactable = (i != _type);
                button.SetButtonSpriteState(i != _type);
            }
        }

        public void OnClickFilterButton(int _type)
        {
            RefreshButtonUI(_type - 1);

            CollectionAchievementUIEvent.SendFilterScrollView((eCollectionAchievementFilterType)_type);
        }
    }
}
