using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class MagicShowcasePopup : Popup<TabTypePopupData>
    {
        [SerializeField] TabController tabController = null;

        #region OpenPopup
        public static MagicShowcasePopup OpenPopup(int tab, int subTab = -1)
        {
            return OpenPopup(new TabTypePopupData(tab, subTab));
        }
        public static MagicShowcasePopup OpenPopup(TabTypePopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<MagicShowcasePopup>(data);
        }
        #endregion
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

        public override void InitUI()
        {
            if (tabController == null)
                return;

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);

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
            if (Data.SubIndex != -1)
                subIndex = Data.SubIndex;

            tabController.InitTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
        }
        public void moveTab(TabTypePopupData data)
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
        public override void ForceUpdate(TabTypePopupData data)
        {
            base.DataRefresh(data);
            tabController.RefreshTab();
        }
        public override void ClosePopup()
        {
            base.ClosePopup();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);

            SetSubCamTextureOff();
            tabController.InitTabIndex();

            UIManager.Instance.MainPopupUI.RequestUpdateMagicShowcaseReddot();
        }

        public void OnLevelUp()
        {
            MagicShowcaseTabController tabs = (tabController as MagicShowcaseTabController);
            if(tabs != null)
            {
                tabs.OnLevelUp();
            }
        }
    }
}