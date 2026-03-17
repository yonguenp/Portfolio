using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIEffects;

namespace SandboxNetwork
{
    public class GuildPopupTabController : TabController
    {
        public override void InitTab(int index, TabTypePopupData datas = null)
        {
            if (SetTab(index, datas))
                return;

            if (isStack && tabStack != null)
            {
                tabStack.Clear();
                tabStack.Push(CurTab);
            }
            DefaultTab = CurTab;

            SetGrayScale();
        }

        public override void ChangeTab(int index, TabTypePopupData datas = null)
        {
            if (SetTab(index, datas))
                return;

            if (isStack && tabStack != null)
            {
                tabStack.Push(CurTab);
            }

            SetGrayScale();
        }

        void SetGrayScale()
        {
            if (tabBtns == null || tabBtns.Count <= 0) return;

            for(var i = 0; i < tabBtns.Count; i++)
            {
                tabBtns[i].transform.Find("Image").GetComponent<UIEffect>().effectFactor = i == CurTab ? 0 : 1;
            }
        }

    }
}
