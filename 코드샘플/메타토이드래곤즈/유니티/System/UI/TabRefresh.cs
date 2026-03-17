
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public delegate void TabRefreshDelegate(int index);
    public class TabRefresh : MonoBehaviour//단독으로 사용하는 스크립트가 아님.
    {
        [SerializeField]
        protected List<Button> tabBtns = null;

        public int DefaultTab { get; protected set; } = 0;

        protected TabRefreshDelegate TabDelegate { get; set; } = null;
        public int CurTab { get; protected set; } = -1;
        protected bool isInit = false;

        public virtual void Initialze(int index, TabRefreshDelegate tabDelegate)//다른 스크립트에서 호출하여 초기화.
        {
            if (isInit)
                return;

            InitialzeData(index, tabDelegate);
            InitialzeUI();

            isInit = true;
        }
        protected virtual void InitialzeData(int index, TabRefreshDelegate tabDelegate)//다른 스크립트에서 호출하여 초기화.
        {
            DefaultTab = index;
            SetDelegate(tabDelegate);
        }
        protected virtual void InitialzeUI()//다른 스크립트에서 호출하여 초기화.
        {

            if (tabBtns == null)
                tabBtns = new List<Button>();

            var tabBtnCount = tabBtns.Count;
            for (int i = 0; i < tabBtnCount; ++i)
            {
                var index = i;
                tabBtns[i].onClick.AddListener(delegate { ChangeTab(index); });
            }

            InitTab(DefaultTab);
        }
        public virtual void InitTab(int index)//처음 켜는 Init에서 Tab바꾸는 경우 경우
        {
            if (SetTab(index))
                return;

            DefaultTab = CurTab;
        }
        public virtual void ChangeTab(int index)//Tab바꾸는 경우
        {
            if (SetTab(index))
                return;
        }
        protected virtual bool SetTab(int index)
        {
            if (CurTab == index)
                return true;

            var tabBtnCount = tabBtns.Count;

            for (var i = 0; i < tabBtnCount; ++i)
            {
                if (tabBtns[i] == null)
                    continue;

                tabBtns[i].SetInteractable(index != i);
            }

            TabDelegate?.Invoke(index);

            CurTab = index;
            return false;
        }
        public virtual void SetDelegate(TabRefreshDelegate tabDelegate)//혹시 리플레쉬 하는 경우에 쓸 수도?
        {
            TabDelegate = tabDelegate;
        }
    }
}