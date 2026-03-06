using Com.LuisPedroFonseca.ProCamera2D;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class BuildingCompletePopup : Popup<BuildingUpGradeStateData>
    {
        [SerializeField]
        Text completeLabel = null;

        [SerializeField]
        Image bgTargetSprite = null;

        [Space(10)]
        [Header("bg label setting")]
        [SerializeField]
        int upgradeLabelIndex = 0;
        [SerializeField]
        int buildingLabelIndex = 0;

        [Space(10)]
        [Header("bg color setting")]
        [SerializeField]
        Color upgradeBgColor = new Color();
        [SerializeField]
        Color buildingBgColor = new Color();

        VoidDelegate CloseCallBack = null;
        #region OpenPopup
        
        public static BuildingCompletePopup OpenPopup(bool isUp, VoidDelegate closeFunction = null)
        {
            return OpenPopup(new BuildingUpGradeStateData(isUp), closeFunction);
        }
        public static BuildingCompletePopup OpenPopup(BuildingUpGradeStateData data = null, VoidDelegate closeFunction = null)
        {
            var popup = PopupManager.OpenPopup<BuildingCompletePopup>(data);
            if (popup == null)
                return null;

            popup.SetCloseCallBack(closeFunction);
            return popup;
        }

        #endregion
        public void OnClickOkButton()
        {
            PopupManager.ClosePopup<BuildingCompletePopup>();
        }
        public override void ClosePopup()
        {
            CloseCallBack?.Invoke();
            if (PopupManager.OpenPopupCount >= 1)  // 이 팝업은 닫히지만 다른 팝업이 살아 있을 경우를 위한 예외처리 ex) 타운 업그레이드에서 호출되는 이 팝업
            {
                UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
                UIManager.Instance.InitUI(eUIType.Town);
            }
            if (TutorialManager.tutorialManagement.IsPlayingTutorial)
                TutorialManager.tutorialManagement.NextTutorialStart();
            base.ClosePopup();
        }

        public override void Init(BuildingUpGradeStateData data)
        {
            SetCloseCallBack(null);
            base.Init(data);
        }

        public override void InitUI()
        {
            if (Data != null)
                SetDetailData(Data.isUpGrade);
            else
                SetDetailData();
        }

        public void SetCloseCallBack(VoidDelegate callback)
        {
            if (callback != null)
                CloseCallBack = callback;
            else
                CloseCallBack = null;
        }

        void SetDetailData(bool isUpgrade = false)
        {
            if (completeLabel == null || bgTargetSprite == null)
            {
                return;
            }

            var bgColor = isUpgrade ? upgradeBgColor : buildingBgColor;
            bgTargetSprite.color = bgColor;

            var stringIndex = isUpgrade ? upgradeLabelIndex : buildingLabelIndex;
            completeLabel.text = StringData.GetStringByIndex(stringIndex);
        }
    }
}