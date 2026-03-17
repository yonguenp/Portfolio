using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eSettingPopupLayer
    {
        GAME,
        ACCOUNT,
        SUPPORT
    }

    public class SettingPopup : Popup<PopupData>
    {
        // 탭 공통
        [Header("[Tab Button]")]
        public eSettingPopupLayer defaultTap = eSettingPopupLayer.GAME;
        [SerializeField] Color selectedColor = new Color();
        [SerializeField] Color defaultColor = new Color();
        [SerializeField] Button gameTabButon = null;
        [SerializeField] Button accountTabButton = null;
        [SerializeField] Button supportTabButton = null;
        
        [Header("[Tab Layer]")]
        [SerializeField] SettingGameLayer gameTabLayer = null;
        [SerializeField] SettingAccountLayer accountTabLayer = null;
        [SerializeField] SettingSupportLayer supportTabLayer = null;

        [Space(10)]
        [SerializeField] RectTransform layoutGroup = null;

        public override void InitUI()
        {
            if (gameTabLayer == null || accountTabLayer == null || supportTabLayer == null) { return; }
            if (Town.Instance != null)
            {
                Town.Instance.CameraUnFollowObject();
            }

            OnClickTapButton((int)defaultTap);
        }

        public void OnClickTapButton(int buttonType)
        {
            switch ((eSettingPopupLayer)buttonType)
            {
                case eSettingPopupLayer.GAME:
                    gameTabLayer.Init();
                    break;
                case eSettingPopupLayer.ACCOUNT:
                    accountTabLayer.Init();
                    break;
                case eSettingPopupLayer.SUPPORT:
                    supportTabLayer.Init();
                    break;
            }

            gameTabLayer.gameObject.SetActive(buttonType == (int)eSettingPopupLayer.GAME);
            accountTabLayer.gameObject.SetActive(buttonType == (int)eSettingPopupLayer.ACCOUNT);
            supportTabLayer.gameObject.SetActive(buttonType == (int)eSettingPopupLayer.SUPPORT);

            gameTabButon.targetGraphic.color = buttonType == (int)eSettingPopupLayer.GAME ? selectedColor : defaultColor;
            accountTabButton.targetGraphic.color = buttonType == (int)eSettingPopupLayer.ACCOUNT ? selectedColor : defaultColor;
            supportTabButton.targetGraphic.color = buttonType == (int)eSettingPopupLayer.SUPPORT ? selectedColor : defaultColor;

            // 레이아웃 갱신
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
        }

        public override void BackButton()
        {
            
            ClosePopup();
        }
	}
}
