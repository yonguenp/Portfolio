using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 
    public class GuildRecommendFilterPopup : Popup<GuildReccomendFilterData>
    {
        [SerializeField]
        Toggle[] toggles = null;
        [SerializeField]
        Color selectedBgColor = new Color();
        [SerializeField]
        Color deselectedBgColor = new Color();
        [SerializeField]
        Color selectedCheckBoxColor = new Color();
        [SerializeField]
        Color deselectedCheckBoxColor = new Color();


        eGuildRecommendFilter curFilter;
        public delegate void Func(GuildReccomendFilterData data);
        protected Func applyCallback;
        public Func ApplyCallback
        {
            get { return applyCallback; }
            set { applyCallback = value; }
        }

        public override void InitUI()
        {
            if (Data == null)
                return;
            SetData();
        }
        void SetData()
        {
            curFilter = Data.filter;
            toggles[0].isOn = curFilter.HasFlag(eGuildRecommendFilter.ApplyJoin);
            toggles[1].isOn = curFilter.HasFlag(eGuildRecommendFilter.ImmediateJoin);
        }
        public virtual void OnClickConfirm()//적용 버튼 누르면
        {
            if (applyCallback != null)
            {                
                applyCallback(new GuildReccomendFilterData(curFilter));
            }
            PopupManager.ClosePopup<GuildRecommendFilterPopup>();
        }
        public void OnClickToggleBtn(int idx)
        {
            var toggle = toggles[idx];
            if(idx == 0) { 
                if (toggle.isOn)
                {
                    curFilter |= eGuildRecommendFilter.ApplyJoin;
                }
                else
                {
                    curFilter &= ~eGuildRecommendFilter.ApplyJoin;
                }
            }
            else if(idx == 1)
            {
                if (toggle.isOn)
                {
                    curFilter |= eGuildRecommendFilter.ImmediateJoin;
                }
                else
                {
                    curFilter &= ~eGuildRecommendFilter.ImmediateJoin;
                }
            }
            RefreshToggleUI(toggle);
        }
        void RefreshToggleUI(Toggle toggleTarget)
        {
            var isCheck = toggleTarget.isOn;
            var toggleParentBG = toggleTarget.transform.parent.GetComponent<Image>();
            if (toggleParentBG != null)
                toggleParentBG.color = isCheck ? selectedBgColor : deselectedBgColor;

            toggleTarget.targetGraphic.GetComponent<Image>().color = isCheck ? selectedCheckBoxColor : deselectedCheckBoxColor;
        }
    }
}