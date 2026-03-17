using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DiceEventTabController : TabController
    {
        [SerializeField]
        Text titleLabel = null;

        public delegate void TabChangeCallBack(int curTabIndex);
        TabChangeCallBack tabChangeCallBack = null;

        private void OnDisable()
        {
            CurTab = -1;
        }

        public void SetTabChangeCallBack(TabChangeCallBack cb)
        {
            tabChangeCallBack = cb;
        }

        void SetTitleLabel(int _index)
        {
            if (tabBtns != null && titleLabel != null)
            {
                if (_index >= 0 && tabBtns.Count > _index)
                {
                    var textIndex = tabBtns[_index].GetComponentInChildren<LocalizeString>().index;
                    titleLabel.text = StringData.GetStringByIndex(textIndex);
                }
            }
        }
        public override void InitTab(int index, TabTypePopupData datas = null)
        {
            base.InitTab(index, datas);
            SetTitleLabel(index);
        }
        public override void ChangeTab(int index, TabTypePopupData datas = null)
        {
            base.ChangeTab(index, datas);
            SetTitleLabel(index);
        }
        protected override bool SetTab(int index, TabTypePopupData datas = null)
        {
            if(datas != null)
            {
                var selectTab = GetTabLayer(index);
                if (selectTab != null)
                    selectTab.SetSubLayerIndex(datas.SubIndex);
            }

            return base.SetTab(index, datas);
        }
        protected override void SetTab(int index)
        {
            var tabCount = tabLayers.Count;
            for (var i = 0; i < tabCount; ++i)
            {
                if (tabLayers[i] == null)
                    continue;

                tabLayers[i].gameObject.SetActive(false);
            }

            if(tabCount > index)
                tabLayers[index].gameObject.SetActive(true);

            var tabBtnCount = tabBtns.Count;

            for (var i = 0; i < tabBtnCount; ++i)
            {
                if (tabBtns[i] == null)
                    continue;

                var isSelect = index == i;
                tabBtns[i].SetInteractable(!isSelect);
                var tabItemComp = tabBtns[i].GetComponent<DiceEventTabItem>();
                if (tabItemComp == null)
                    continue;

                tabItemComp.SetSelectState(isSelect);
                SetCustomReddot(tabItemComp, i);
            }

            tabChangeCallBack?.Invoke(index);

            CurTab = index;
        }

        public override void RefreshReddot()
        {
            var tabBtnCount = tabBtns.Count;
            for (var i = 0; i < tabBtnCount; ++i)
            {
                if (tabBtns[i] == null)
                    continue;

                var tabItemComp = tabBtns[i].GetComponent<DiceEventTabItem>();
                if (tabItemComp == null)
                    continue;

                SetCustomReddot(tabItemComp, i);
            }
        }

        protected virtual void SetCustomReddot(DiceEventTabItem _target, int _index)
        {
            _target.SetReddotState(false);
            //시간이없어서 루나서버껄로
            //switch (_index)
            //{
            //    case 0:
            //        _target.SetReddotState(DiceEventPopup.GetDiceBoardReddotCondition() || DiceEventPopup.GetDiceQuestReddotCondition());
            //        break;
            //    case 1:
            //        _target.SetReddotState(DiceEventPopup.GetBoxReddotCondition());
            //        break;
            //}

            switch (_index)
            {
                case 0:
                    _target.SetReddotState(LunaServerEventPopup.GetLunaQuestReddotCondition(_index));
                    break;
                case 1:
                    _target.SetReddotState(LunaServerEventPopup.GetLunaQuestReddotCondition(_index));
                    break;
                case 2:
                    _target.SetReddotState(LunaServerEventPopup.GetLunaQuestReddotCondition(_index));
                    break;
            }
        }
    }
}
