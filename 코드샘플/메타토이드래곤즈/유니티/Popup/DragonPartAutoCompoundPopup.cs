using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *장비 일괄 합성 팝업
 */
namespace SandboxNetwork
{
    public class DragonPartAutoTabTypePopupData : TabTypePopupData
    {
        public int CompoundCount = -1;//조합 가능 카운트

        public DragonPartCompoundPanel CompoundPanel = null;

        public DragonPartAutoTabTypePopupData(int tab, int sub, int _compoundCount, DragonPartCompoundPanel _compoundPanel = null) : base(tab,sub)
        {
            CompoundCount = _compoundCount;
            CompoundPanel = _compoundPanel;
        }
    }
    public class DragonPartAutoCompoundPopup : Popup<DragonPartAutoTabTypePopupData>
    {
        static public int PART_MERGE_MATERIAL_MAX_COUNT = -1;

        [SerializeField]
        TabController tabController = null;

        DragonPartCompoundPanel partCompoundPanel = null;

        private void OnDisable()
        {
            if (tabController != null)
                tabController.InitTabIndex();
        }

        public override void InitUI()
        {
            if (tabController == null)
            {
                return;
            }

            if (PART_MERGE_MATERIAL_MAX_COUNT < 0)
                PART_MERGE_MATERIAL_MAX_COUNT = GameConfigTable.GetPartMergeMaterialMaxCount();

            InitTabController();
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

            if (partCompoundPanel == null && Data.CompoundPanel != null)
                partCompoundPanel = Data.CompoundPanel;

            tabController.InitTab(tabIndex, new DragonPartAutoTabTypePopupData(tabIndex, subIndex , 0, partCompoundPanel));
        }
        public void moveTab(DragonPartAutoTabTypePopupData data)
        {
            if (data == null)
            {
                return;
            }

            int tabIndex = data.TabIndex;
            if (tabIndex >= 0)
            {
                tabController.ChangeTab(tabIndex, new DragonPartAutoTabTypePopupData(data.TabIndex, data.SubIndex, data.CompoundCount, partCompoundPanel));
            }
        }
        public override void ClosePopup()
        {
            if(tabController.CurTab == 1)
            {
                moveTab(new DragonPartAutoTabTypePopupData(0, 0, 0, partCompoundPanel));
            }
            else
            {
                base.ClosePopup();
            }
        }
    }
}
