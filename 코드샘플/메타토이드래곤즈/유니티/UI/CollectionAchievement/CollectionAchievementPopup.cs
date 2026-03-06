using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eCollectionAchievementFilterType
    {
        NONE,
        ALL,
        COMPLETE,
        INCOMPLETE,
        MAX,
    }

    public struct CollectionAchievementUIEvent
    {
        public enum CollectionAchievementUIEventEnum
        {
            TOUCH_DATA_AUTO,//처음 진입 시에 자동으로 첫 인덱스 터치 해주는 기능
            TOUCH_DATA,//(콜렉션 & 업적) 데이터 부분 터치 시 -> 업적은 X , 컬렉션은 업적 달성 조건 나옴
            TOUCH_FILTER,//(전체, 완료, 미완료) 필터 버튼 터치시 -> scrollview쪽에서 받아서 쓰는 용도
            TOUCH_EFFECT,//(업적, 컬렉션)스탯 효과 터치 시

            INIT_DETAIL_SCROLLVIEW,//결과 데이터가 없을 때 상세 스크롤 초기화

            SEND_DEFAULT_SCROLLIVEW,//dataScrollview default 필터 세팅
            SEND_FILTER_SCROLLVIEW,//param 나눠야함.(filter 일때는 condition data 기준 필터, effect 일때는 스탯 기준 필터)
            REFRESH_UI,

            REFRESH_TAB_REDDOT,
        }

        static CollectionAchievementUIEvent e;
        public CollectionAchievementUIEventEnum Event;
        public eCollectionAchievementFilterType filterType;
        public eCollectionAchievementType tabType;//콜렉션인지 업적인지
        public int Key;//데이터 테이블의 key 값을 주는 걸로.
        public eStatusType statType;//토글이긴 한데 단일 선택만 된다고함.
        public eStatusValueType statValueType;//토글이긴 한데 단일 선택만 된다고함.
        public bool isButtonVisible;//토글에서 끌지 말지의 상태
        public bool isAutoSort;

        public CollectionAchievementUIEvent(CollectionAchievementUIEventEnum _Event, eCollectionAchievementFilterType _type, eStatusType _statType, eStatusValueType _valueType,
            eCollectionAchievementType _tabType, int _key, bool _isButtonVisible, bool _isAutoSort)
        {
            Event = _Event;
            filterType = _type;
            statType = _statType;
            statValueType = _valueType;
            tabType = _tabType;
            Key = _key;
            isButtonVisible = _isButtonVisible;
            isAutoSort = _isAutoSort;
        }
        public static void TouchDataAutoUI()
        {
            e.Event = CollectionAchievementUIEventEnum.TOUCH_DATA_AUTO;
            EventManager.TriggerEvent(e);
        }
        public static void TouchDataUI(int _key, bool _isAutoSort = false)
        {
            e.Event = CollectionAchievementUIEventEnum.TOUCH_DATA;
            e.Key = _key;
            e.isAutoSort = _isAutoSort;
            EventManager.TriggerEvent(e);
        }
        public static void TouchFilterUI(eCollectionAchievementFilterType _type)
        {
            e.Event = CollectionAchievementUIEventEnum.TOUCH_FILTER;
            e.filterType = _type;
            EventManager.TriggerEvent(e);
        }
        public static void InitDetailScrollView()
        {
            e.Event = CollectionAchievementUIEventEnum.INIT_DETAIL_SCROLLVIEW;
            EventManager.TriggerEvent(e);
        }
        public static void TouchEffectUI(eStatusType _statType, eStatusValueType _statValueType, bool _isButtonVisible)
        {
            e.Event = CollectionAchievementUIEventEnum.TOUCH_EFFECT;
            e.statType = _statType;
            e.statValueType = _statValueType;
            e.isButtonVisible = _isButtonVisible;
            EventManager.TriggerEvent(e);
        }
        public static void SendDefaultScrollView()
        {
            e.Event = CollectionAchievementUIEventEnum.SEND_DEFAULT_SCROLLIVEW;
            EventManager.TriggerEvent(e);
        }

        public static void SendFilterScrollView(eStatusType _statType, eStatusValueType _statValueType)//효과 버튼 누를 때 - _statType 던짐.
        {
            e.Event = CollectionAchievementUIEventEnum.SEND_FILTER_SCROLLVIEW;
            e.statType = _statType;
            e.statValueType = _statValueType;
            e.filterType = eCollectionAchievementFilterType.NONE;
            EventManager.TriggerEvent(e);
        }
        public static void SendFilterScrollView(eCollectionAchievementFilterType _type)//필터 타입 버튼 누를 때
        {
            e.Event = CollectionAchievementUIEventEnum.SEND_FILTER_SCROLLVIEW;
            e.filterType = _type;
            EventManager.TriggerEvent(e);
        }
        public static void RefreshReddot()//레드닷 갱신
        {
            e.Event = CollectionAchievementUIEventEnum.REFRESH_TAB_REDDOT;
            EventManager.TriggerEvent(e);
        }

        public static void RefreshUI()//현재 UI 갱신
        {
            e.Event = CollectionAchievementUIEventEnum.REFRESH_UI;
            EventManager.TriggerEvent(e);
        }
    }

    public class CollectionAchievementPopup : Popup<TabTypePopupData>, EventListener<CollectionAchievementUIEvent>
    {
        [SerializeField]
        Text titlePopupLabel = null;

        [SerializeField]
        TabController tabController = null;

        [SerializeField]
        List<GameObject> reddotList = new List<GameObject>();

        bool isComeAchievementTab = false;

        Action closeCallback = null;

        #region OpenPopup
        public static CollectionAchievementPopup OpenPopup(int tab, int subTab = -1)
        {
            return OpenPopup(new TabTypePopupData(tab, subTab));
        }
        public static CollectionAchievementPopup OpenPopup(TabTypePopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<CollectionAchievementPopup>(data);
        }
        #endregion

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
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.REFRESH_TAB_REDDOT:
                    RefreshReddotVisible();
                    break;
            }
        }
        public void OnClickCloseBtn()
        {
            ClosePopup();
        }
        public override void ForceUpdate(TabTypePopupData data)
        {
            base.DataRefresh(data);
            tabController.RefreshTab();
        }

        public override void InitUI()
        {
            if (tabController == null)
            {
                return;
            }

            RefreshReddotVisible();
            InitTabController();
            SetSubCamTextureOn();
        }

        void InitTabController()
        {
            int tabIndex;
            int subIndex = 0;
            if (Data == null)
            {
                tabIndex = 0;
            }
            else
            {
                tabIndex = Data.TabIndex;
            }

            if (tabIndex < 0)
            {
                tabIndex = 0;
            }
            if (Data != null && Data.SubIndex != -1)
                subIndex = Data.SubIndex;

            isComeAchievementTab = tabIndex > 0;//업적 버튼을 통해 들어왔는지

            if(isComeAchievementTab)
                UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

            tabController.InitTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
        }

        void RefreshReddotVisible()//초기값 세팅 - 콜렉션 / 업적 따로
        {
            foreach (var reddot in reddotList)
                if (reddot != null)
                    reddot.SetActive(false);
            
            var isCollectionReddotShow = CollectionAchievementManager.Instance.IsShowCollectionReddot();
            var isAchievementReddotShow = CollectionAchievementManager.Instance.IsShowAchievementReddot();
            reddotList[0].SetActive(isCollectionReddotShow);
            reddotList[1].SetActive(isAchievementReddotShow);
        }

        void SetSubCamTextureOn()
        {
            Town.Instance?.SetSubCamState(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        void SetSubCamTextureOff()
        {
            Town.Instance?.SetSubCamState(false);
            UICanvas.Instance.EndBackgroundBlurEffect();
        }

        void RefreshTitleLabel(int labelIndex)
        {
            if (labelIndex <= 0)
            {
                return;
            }

            if (titlePopupLabel != null)
            {
                titlePopupLabel.text = StringData.GetStringByIndex(labelIndex);
            }
        }
        public void MoveTab(TabTypePopupData data)
        {
            if (data == null)
            {
                return;
            }

            int tabIndex = data.TabIndex;
            int subIndex = 0;

            if (data.SubIndex != -1)
                subIndex = data.SubIndex;

            if (tabIndex >= 0)
            {
                tabController.ChangeTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
            }
        }
        public void onClickExpectGameAlphaUpdate()
        {
            ToastManager.On(100000326);
        }

        public override void OnClickDimd()
        {
            ClosePopup();
        }
        public override void BackButton()
        {
            ClosePopup();
        }

        public override void ClosePopup()
        {
            base.ClosePopup();

            if(isComeAchievementTab)
                UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            else
                PopupManager.OpenPopup<DragonManagePopup>(new DragonTabTypePopupData(0, 0));

            //reddot event
            UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_COLLECTION_REDDOT, UIObjectEvent.eUITarget.RB);//레드닷 갱신
            UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_ACHIEVEMENT_REDDOT, UIObjectEvent.eUITarget.HAMBURGER);

            //if(closeCallback != null)
            //{
            //    closeCallback();
            //    closeCallback = null;
            //}

            SetSubCamTextureOff();
            tabController.InitTabIndex();
        }

        public void SetCloseCallback(Action _closeCallbcak)//드래곤 리스트에서 접근하면 타이틀라벨이 겹쳐서, 드래곤 리스트 이름 꺼주려고함.
        {
            closeCallback = _closeCallbcak;
        }
        public override bool IsModeless()
        {
            return false;
        }
    }
}
