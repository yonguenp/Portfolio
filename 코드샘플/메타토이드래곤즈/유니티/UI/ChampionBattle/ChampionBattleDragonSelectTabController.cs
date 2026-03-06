using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBattleDragonSelectTabController : TabController
    {
        ChampionBattleDragonSelectPopup ParentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }

        [SerializeField]
        Text titleLabel = null;

        private void OnDisable()
        {
            CurTab = -1;
            InitData();
        }
        public override void InitTab(int index, TabTypePopupData datas = null)
        {
            base.InitTab(index, datas);

            if (tabBtns != null && titleLabel != null)
            {
                if (index >= 0 && tabBtns.Count > index)
                {
                    var textIndex = tabBtns[index].GetComponentInChildren<LocalizeString>().index;
                    titleLabel.text = StringData.GetStringByIndex(textIndex);
                }
            }
        }
        public override void ChangeTab(int index, TabTypePopupData datas = null)
        {
            base.ChangeTab(index, datas);
            
            if (tabBtns != null && titleLabel != null)
            {
                if(index >= 0 && tabBtns.Count > index)
                {
                    var textIndex = tabBtns[index].GetComponentInChildren<LocalizeString>().index;
                    titleLabel.text = StringData.GetStringByIndex(textIndex);
                }
            }
        }

        protected override void SetTab(int index)
        {
            var tabCount = tabLayers.Count;
            for (var i = 0; i < tabCount; ++i)
            {
                if (tabLayers[i] == null)
                    continue;

                tabLayers[i].gameObject.SetActive(index == i);
            }

            var tabBtnCount = tabBtns.Count;

            for (var i = 0; i < tabBtnCount; ++i)
            {
                if (tabBtns[i] == null)
                    continue;

                tabBtns[i].SetInteractable(index != i);

                var buttonHandler = tabBtns[i].GetComponent<ButtonEventHandler>();
                if(buttonHandler != null)
                {
                    buttonHandler.SetStateByInteractable(tabBtns[i]);
                }
            }

            CurTab = index;
        }
        public override void SetTabDelegate()
        {
            var tabBtnCount = tabBtns.Count;
            for (var i = 0; i < tabBtnCount; ++i)
            {
                var index = i;
                tabBtns[i].onClick.AddListener(delegate { OnClickTabButton(index); });
            }
        }
        public void OnClickTabButton(int index)
        {
            InitData();
            ChangeTab(index, new ChampionBattleDragonTabTypePopupData(index, 0));
        }

        void InitData()
        {
            ParentPopup.Clear();
        }

        public void SetAllVisibleTab(bool _isVisible)
        {
            if(tabBtns != null && tabBtns.Count > 0)
            {
                foreach(var btn in tabBtns)
                {
                    if (btn == null)
                        continue;
                    btn.gameObject.SetActive(_isVisible);
                }
            }    
        }
    }
}
