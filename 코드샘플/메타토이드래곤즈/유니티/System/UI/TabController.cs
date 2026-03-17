
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class TabController : MonoBehaviour
    {
        [SerializeField]
        protected int defaultTab = 0;
        [SerializeField]
        Text titleText = null;
        [SerializeField]
        protected List<Button> tabBtns = null;
        [SerializeField]
        protected List<TabLayer> tabLayers = null;
        [SerializeField]
        protected List<Button> backBtns = null;

		public int DefaultTab
        {
            get { return defaultTab; }
            protected set { defaultTab = value; }
        }

        public int CurTab { get; protected set; } = -1;
        protected TabTypePopupData curDatas = null;
        protected Stack<int> tabStack = null;
        protected bool isStack = false;
        protected bool isInit = false;

        protected virtual void Start()
        {
            InitUI();
        }

        public virtual void InitTabIndex()
        {
            CurTab = -1;
        }

        protected virtual void InitUI()
        {
            if (isInit)
                return;

            if (tabLayers == null)
                tabLayers = new List<TabLayer>();
            if (tabBtns == null)
                tabBtns = new List<Button>();

            for (int i = 0, count = tabLayers.Count; i < count; ++i)
            {
                if (tabLayers[i] == null)
                    continue;

                tabLayers[i].SetLayerIndex(i);
            }

            isStack = backBtns != null && backBtns.Count > 0;

            if(isStack)
            {
                if (tabStack == null)
                    tabStack = new Stack<int>();
                else
                    tabStack.Clear();

                var backCount = backBtns.Count;
                for(var i = 0; i < backCount; ++i)
                {
                    backBtns[i].onClick.AddListener(delegate { BackTab(); });
                }
            }

            SetTabDelegate();

            InitTab(defaultTab);
            isInit = true;
        }

        public virtual void SetTabDelegate()
        {
            var tabBtnCount = tabBtns.Count;
            for (var i = 0; i < tabBtnCount; ++i)
            {
                var index = i;
                tabBtns[i].onClick.AddListener(delegate { ChangeTab(index); });
            }
        }

        public virtual void InitTab(int index, TabTypePopupData datas = null)//처음 켜는 Init에서 Tab바꾸는 경우 경우
        {
            if (SetTab(index, datas))
                return;

            if (isStack && tabStack != null)
            {
                tabStack.Clear();
                tabStack.Push(CurTab);
            }
            DefaultTab = CurTab;
        }
        public virtual void ChangeTab(int index, TabTypePopupData datas = null)//스택 추가하면서 Tab바꾸는 경우
        {
            if (SetTab(index, datas))
                return;

            if (isStack && tabStack != null)
            {
                tabStack.Push(CurTab);
            }
        }
        protected virtual bool SetTab(int index, TabTypePopupData datas = null)
        {
            if (tabLayers == null)
                return true;

            if (CurTab == index)
                return true;

            var tabCount = tabLayers.Count;
            if (tabCount == 0 || tabCount <= index || index < 0 || tabLayers[index] == null)
                return true;

            SetTab(index);

            tabLayers[index].InitUI(datas);
            curDatas = datas;

            SetTitleText();
            return false;
        }
        protected virtual void SetTab(int index)
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
                tabBtns[i].SetButtonSpriteState(index != i);
			}

            CurTab = index;
        }

        protected virtual void SetSubTab(int index)
        {
            var tabCount = tabLayers.Count;
            for (var i = 0; i < tabCount; ++i)
            {
                if (tabLayers[i] == null)
                    continue;

                tabLayers[CurTab].gameObject.SetActive(index == i);
            }

            var tabBtnCount = tabBtns.Count;

            for (var i = 0; i < tabBtnCount; ++i)
            {
                if (tabBtns[i] == null)
                    continue;

                tabBtns[i].SetInteractable(index != i);
                tabBtns[i].SetButtonSpriteState(index != i);
            }

            CurTab = index;
        }

        public virtual void BackTab()
        {
            if (!isStack)
                return;

            if (tabStack == null || tabStack.Count < 2)
                return;

            tabStack.Pop();
            RefreshTab(tabStack.Peek());
            SetTab(tabStack.Peek());
        }
        public virtual void RefreshTab()//내 현재 Tab 갱신
        {
            RefreshTab(CurTab);
        }
        protected virtual void RefreshTab(int index)
        {
            if (tabLayers == null)
                return;

            var tabCount = tabLayers.Count;
            if (tabCount == 0 || tabCount <= index || index < 0 || tabLayers[index] == null)
                return;

            tabLayers[index].RefreshUI();
        }

        public TabLayer GetTabLayer(int index)
        {
            if (tabLayers == null)
                return null;

            var tabCount = tabLayers.Count;
            if (tabCount == 0 || tabCount <= index || index < 0 || tabLayers[index] == null)
                return null;

            return tabLayers[index];
        }
        public TabLayer GetCurrentTabLayer()
        {
            return GetTabLayer(CurTab);
        }

        public void CloseAllTab()
        {
            if (tabLayers == null) return;

            foreach (var tab in tabLayers)
            {
                tab.gameObject.SetActive(false);
            }

            CurTab = -1;
        }

        protected void SetTitleText()
        {
            TabLayer curTabLayerData = GetCurrentTabLayer();
            if (curTabLayerData != null && titleText != null)
            {
                titleText.text = curTabLayerData.GetTitleText();
            }
        }

        public virtual void RefreshReddot() { }
    }
}