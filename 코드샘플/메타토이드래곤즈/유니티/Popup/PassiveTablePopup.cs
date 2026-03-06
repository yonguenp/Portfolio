using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PassiveTablePopupData : PopupData
    {
        public PassiveTablePopupData(int menuID1, int menuID2, int maxMenu)
        {
            MenuID1 = menuID1;
            MenuID2 = menuID2;
            MaxMenu = maxMenu;

        }

        public int MenuID1 { get; private set; } = 0;
        public int MenuID2 { get; private set; } = 0;
        public int MaxMenu { get; private set; } = 0;

    }

    public class PassiveTablePopup : Popup<PassiveTablePopupData>
    {
        [SerializeField] ScrollRect[] scrollRects = null;

        [SerializeField] GameObject passiveTableClone = null;
        [SerializeField] GameObject gachaTableSubClone = null;

        [SerializeField] GameObject UpArrow = null;
        [SerializeField] GameObject DownArrow = null;

        [SerializeField] GameObject charTableGuideLayer = null;

        [SerializeField] Button[] menuObjs = null;

        List<List<SkillPassiveRateData>> passiveRateDataLists = new();

        List<List<PassiveTableClone>> PassiveTableCloneLists = new();

        float[] maxRates;
        int currentMenuIndex = 0;

        const int CurMenuCount = 2;

        public override void InitUI()
        {
            currentMenuIndex = 0;
            SetMenuState();
            OnClickMenu(currentMenuIndex);
            SetTableData();
        }

        void SetMenuState()
        {
            for (int i = 0; i < CurMenuCount; ++i)
            {
                menuObjs[i].gameObject.SetActive(i < Data.MaxMenu);
                //menuObjs[i].SetButtonSpriteState(i != currentMenuIndex);
                menuObjs[i].interactable = (i != currentMenuIndex);
            }
        }

        private void SetTableData()
        {
            ClearData();
            SetGachaTableData();
        }

        void SetGachaTableData()
        {
            passiveTableClone.SetActive(true);
            gachaTableSubClone.SetActive(true);
            var group1 = SkillPassiveRateData.GetByGroup(Data.MenuID1);
            group1.Sort((a, b) => { return a.RATE.CompareTo(b.RATE); });
            passiveRateDataLists.Add(group1);

            var group2 = SkillPassiveRateData.GetByGroup(Data.MenuID2);
            group2.Sort((a, b) => { return a.RATE.CompareTo(b.RATE); });
            passiveRateDataLists.Add(group2);
            if (passiveRateDataLists[0] != null)
            {
                for (int i = 0, count = CurMenuCount; i < count; ++i)
                {
                    PassiveTableCloneLists.Add(new());
                    PassiveTableCloneLists[i] = new();
                    foreach (SkillPassiveRateData rateData in passiveRateDataLists[i])
                    {
                        maxRates[i] += rateData.RATE;

                        // 메인 확률표 생성
                        GameObject newTableClone = Instantiate(passiveTableClone, scrollRects[i].content);
                        PassiveTableClone tableClone = newTableClone.GetComponent<PassiveTableClone>();

                        List<GachaTableSubClone> subList = new List<GachaTableSubClone>();
                        // 서브 확률표 생성
                        var resultRateData = rateData.Child;
                        resultRateData.Sort((a, b) => { return a.RATE.CompareTo(b.RATE); });
                        bool subOpen = false;
                        if (resultRateData != null && resultRateData.Count > 0)
                        {
                            for (int j = 0, count_j = resultRateData.Count; j < count_j; ++j)
                            {
                                if(resultRateData[j].RATE > 0)
                                {
                                    GameObject newSubClone = Instantiate(gachaTableSubClone, scrollRects[i].content);
                                    var sub = newSubClone.GetComponent<GachaTableSubClone>();
                                    sub.InitSubClone(resultRateData[j], rateData.NAME);
                                    subList.Add(sub);
                                }
                            }
                        }

                        tableClone.InitClone(rateData, subList, this);
                        tableClone.SetSubTable(subOpen);

                        PassiveTableCloneLists[i].Add(tableClone);
                    }
                }
            }

            passiveTableClone.SetActive(false);
            gachaTableSubClone.SetActive(false);

            for (int i = 0; i < CurMenuCount; ++i)
            {
                if (PassiveTableCloneLists[i] != null && PassiveTableCloneLists[i].Count > 0)
                {
                    PassiveTableCloneLists[i].ForEach(clone => clone.UpdateMaxRate(maxRates[i]));
                }
            }

            CancelInvoke("CheckScrollSize");
            Invoke("CheckScrollSize", 0.1f);
        }

        public void OnSubTableClear()
        {
            foreach (var PassiveTableCloneList in PassiveTableCloneLists)
            {
                PassiveTableCloneList.ForEach(clone => clone.SetSubTable(false));
            }
        }

        public void CheckScrollSize()
        {
            CancelInvoke("CheckScrollSize");
            SetArrow(scrollRects[currentMenuIndex].content.sizeDelta.y > scrollRects[currentMenuIndex].viewport.rect.height);
        }

        void SetArrow(bool enable)
        {
            UpArrow.SetActive(enable);
            DownArrow.SetActive(enable);
        }

        public void OnUpArrow()
        {
            scrollRects[currentMenuIndex].DOVerticalNormalizedPos(1.0f, 0.1f);
        }

        public void OnDownArrow()
        {
            scrollRects[currentMenuIndex].DOVerticalNormalizedPos(0.0f, 0.1f);
        }

        void ClearData()
        {
            maxRates = new float[CurMenuCount];
            passiveRateDataLists.Clear();
            PassiveTableCloneLists.Clear();

            foreach (var scrollRect in scrollRects)
                SBFunc.RemoveAllChildrens(scrollRect.content);
        }

        public void OnClickMenu(int index)
        {
            if (index >= CurMenuCount)
                return;
            currentMenuIndex = index;
            for (int i = 0; i < CurMenuCount; ++i)
            {
                scrollRects[i].gameObject.SetActive(i == currentMenuIndex);
                //menuObjs[i].SetButtonSpriteState(i != currentMenuIndex);
                menuObjs[i].interactable = (i != currentMenuIndex);
            }
        }
    }
}