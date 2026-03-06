using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마법진열장 UI 진입전 입장 팝업 (버튼 5개 - 스탯타입별로 & 진행도 정도만 노출 하면 될듯)
/// </summary>
/// 

namespace SandboxNetwork
{
    public class MagicShowcaseEnterPopup : Popup<PopupData>
    {
        [SerializeField] List<MagicShowcaseEnterButtonSlot> buttonList = new List<MagicShowcaseEnterButtonSlot>();

        private bool isOpenMainPopup = false;//진열장으로 들어가는지// mainUI 애니메이션 한번만 실행되게

        #region OpenPopup
        public static MagicShowcaseEnterPopup OpenPopup()
        {
            return OpenPopup(new PopupData());
        }
        public static MagicShowcaseEnterPopup OpenPopup(PopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<MagicShowcaseEnterPopup>(data);
        }
        #endregion
        public override void InitUI()
        {
            isOpenMainPopup = false;
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            SetSubCamTextureOn();
        }

        void SetSubCamTextureOn()
        {
            Town.Instance?.SetSubCamState(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        void SetSubCamTextureOff()
        {
            Town.Instance?.SetSubCamState(false);
            UICanvas.Instance.EndBackgroundBlurEffect();
        }
        public override void ClosePopup()
        {
            base.ClosePopup();

            if(!isOpenMainPopup)
                UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            SetSubCamTextureOff();

            isOpenMainPopup = false;
        }

        public void OnClickButton(int _index)
        {
            isOpenMainPopup = true;
            MagicShowcasePopup.OpenPopup(_index);
        }

        void SetData()//각 타입 별 진행도 (레벨단계 가져오기)
        {

        }
    }
}
