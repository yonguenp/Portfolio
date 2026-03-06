using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public class GuildStartPopup : Popup<TabTypePopupData>
    {
        [SerializeField]
        TabController tabController;
        public override void InitUI()
        {
            InitTab();
        }
        
        void InitTab()
        {
            if (tabController == null)
                return;
            int tabIndex = 0;
            int subIndex = 0;
            if (Data != null) 
            {
                tabIndex = Data.TabIndex;
                if (Data.SubIndex != -1)
                    subIndex = Data.SubIndex;
            }
            if (tabIndex < 0)
            {
                tabIndex = 0;
            }
            

            tabController.InitTab(tabIndex, new TabTypePopupData(tabIndex, subIndex));
        }

        public void OnClickGuildMake()
        {
            tabController.ChangeTab(1);
            tabController.RefreshTab();
        }
        public void OnClikeGuildJoin()
        {
            tabController.ChangeTab(2);
            tabController.RefreshTab();
        }

        public override void ClosePopup()
        {
            if (tabController.CurTab == 0)
                base.ClosePopup();
            else
                tabController.ChangeTab(0);
        }

        public void ForceClose()
        {
            base.ClosePopup();
        }
    }
}