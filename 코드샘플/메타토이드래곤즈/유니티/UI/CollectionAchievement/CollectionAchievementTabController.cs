using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CollectionAchievementTabController : TabController
    {
        public override void ChangeTab(int index, TabTypePopupData datas = null)
        {
            base.ChangeTab(index, datas);

            CollectionAchievementUIEvent.RefreshReddot();
        }
    }
}