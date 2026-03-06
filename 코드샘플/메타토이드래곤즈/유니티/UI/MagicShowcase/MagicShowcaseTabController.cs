using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이 UI는 다른 팝업들과 다르게 단일 TabLayer를 사용해서, 고정 레이어로 갱신만 쳐줌.
/// </summary>
namespace SandboxNetwork
{
    public class MagicShowcaseTabController : TabController
    {
        [SerializeField] List<Color> selectFrameColorList = new List<Color>();
        [SerializeField] List<Image> tabFrame = new List<Image>();
        public override void SetTabDelegate()
        {
            var tabBtnCount = tabBtns.Count;
            for (var i = 0; i < tabBtnCount; ++i)
            {
                var index = i;
                tabBtns[i].onClick.AddListener(delegate { ChangeTab(index , new TabTypePopupData(index, -1)); });
            }
        }
        protected override void SetTab(int index)
        {
            tabLayers[0].gameObject.SetActive(true);

            var tabBtnCount = tabBtns.Count;

            for (var i = 0; i < tabBtnCount; ++i)
            {
                if (tabBtns[i] == null)
                    continue;

                var isOtherTabIndex = index != i;

                tabBtns[i].SetInteractable(isOtherTabIndex);
                tabBtns[i].SetButtonSpriteState(isOtherTabIndex);
                var frame = tabFrame[i];
                if (frame != null)
                {
                    if (!isOtherTabIndex)
                        tabFrame[i].color = selectFrameColorList[i];

                    tabFrame[i].fillAmount = 0.0f;

                    var type = MagicShowcaseManager.Instance.GetGroupType(i + 1);
                    var infoData = MagicShowcaseManager.Instance.GetInfoDataByType(type);
                    var maxLevel = MagicShowcaseData.GetMaxLevelByGroup(type);
                    if (infoData != null)
                    {
                        tabFrame[i].fillAmount = (float)infoData.LEVEL / maxLevel;
                    }
                }

                var dimmedObj = SBFunc.GetChildrensByName(tabBtns[i].transform, new string[] { "dimmed" }).gameObject;
                if (dimmedObj != null)
                {
                    dimmedObj.SetActive(isOtherTabIndex);
                }
            }

            CurTab = index;
        }

        public void OnLevelUp()
        {
            for (var i = 0; i < tabFrame.Count; ++i)
            {
                var frame = tabFrame[i];
                if (frame != null)
                {
                    var type = MagicShowcaseManager.Instance.GetGroupType(i + 1);
                    var infoData = MagicShowcaseManager.Instance.GetInfoDataByType(type);
                    var maxLevel = MagicShowcaseData.GetMaxLevelByGroup(type);
                    if (infoData != null)
                    {
                        tabFrame[i].fillAmount = (float)infoData.LEVEL / maxLevel;
                    }
                }
            }
        }
        protected override bool SetTab(int index, TabTypePopupData datas = null)
        {
            if (tabLayers == null)
                return true;

            if (CurTab == index)
                return true;

            var tabCount = tabLayers.Count;
            if (tabCount == 0 || index < 0)
                return true;

            SetTab(index);

            tabLayers[0].InitUI(datas);
            curDatas = datas;
            RefreshReddot();
            return false;
        }

        protected override void RefreshTab(int index)
        {
            if (tabLayers == null)
                return;

            var tabCount = tabLayers.Count;
            if (tabCount == 0 || index < 0)
                return;

            tabLayers[0].RefreshUI();
            RefreshReddot();
        }

        public override void RefreshReddot()
        {
            for (int i = 0; i < tabBtns.Count; i++)
            {
                var reddot = SBFunc.GetChildrensByName(tabBtns[i].gameObject.transform, new string[] { "red" });
                if (reddot == null)
                    continue;

                var condition = MagicShowcaseManager.Instance.IsReddotConditionByType((eShowcaseGroupType)i + 1);
                reddot.gameObject.SetActive(condition);
            }
        }
    }
}
