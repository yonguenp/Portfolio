using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 각 탭에 따라서 UI가 바뀌는 것이아닌, 데이터만 바뀌어서 그냥 레이어 1개로 쓰려함.
/// </summary>
namespace SandboxNetwork
{
    public abstract class MagicShowcaseComponent : MonoBehaviour
    {
        protected int index = -1;
        protected eShowcaseGroupType type = eShowcaseGroupType.NONE;
        protected MagicShowcaseInfoData infoData = null;
        public VoidDelegate RefreshCallBack = null;

        public virtual void InitUI(int _tabIndex)
        {
            index = _tabIndex;
            type = MagicShowcaseManager.Instance.GetGroupType(_tabIndex + 1);
            infoData = MagicShowcaseManager.Instance.GetInfoDataByType(type);
        }
    }

    public class MagicShowcaseTabLayer : TabLayer
    {
        [SerializeField] List<MagicShowcaseComponent> componentList = new List<MagicShowcaseComponent>();

        private int currentTabIndex = -1;
        public override void InitUI(TabTypePopupData datas = null)
        {
            currentTabIndex = datas.TabIndex;//현재 탭을 세팅한 이후에, 각 타입에 맞는 데이터 참조해서 긁어온 후, UI 전체 갱신

            InitUIComponent();
        }

        void InitUIComponent()
        {
            foreach(var comp in componentList)
            {
                if (comp == null)
                    continue;
                comp.InitUI(currentTabIndex);
            }
        }

        public override void RefreshUI()//forceUpdate 에서 들어옴.
        {
            foreach (var comp in componentList)
            {
                if (comp == null)
                    continue;
                comp.InitUI(currentTabIndex);
                comp.RefreshCallBack?.Invoke();
            }
        }

        
    }
}
