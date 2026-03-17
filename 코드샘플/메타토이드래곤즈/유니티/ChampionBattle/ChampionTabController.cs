using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace SandboxNetwork
{
    enum ChampionLobbyTabType
    {
        SET = 1,
        BET,
        MAX
    }
    public class ChampionTabController : MonoBehaviour
    {
        [Header("Tab Button")]
        [SerializeField]
        private Button tabButtonSetting;
        [SerializeField]
        private Button tabButtonBetting;
        [SerializeField]
        private Image SetButtonSprite;
        [SerializeField]
        private Image BetButtonSprite;
        [SerializeField]
        private Sprite DisableBtnSprite;
        [SerializeField]
        private Sprite EnableBtnSprite;

        [Header("Tab Layer")]
        [SerializeField]
        private GameObject tabLayerSetting;
        [SerializeField]
        private GameObject tabLayerBetting;
        [SerializeField]
        private GameObject shopBtn;

        [Header("Tab Title")]
        [SerializeField]
        private Text tabSetTitle;
        [SerializeField]
        private Text tabBetTitle;


        private Color DeactiveColor = new Color(82f / 255f, 87f / 255f, 127f / 255f);
        private Color DefaultColor = new Color(1f, 1f, 1f);

        private ChampionLobbyTabType CurTabType = ChampionLobbyTabType.BET;

        private bool IsParticipant = false;
        private bool IsInTime = false;

        public void Init(bool _isParticipant, bool _isInTime)
        {
            IsParticipant = _isParticipant;
            IsInTime = _isInTime;
            if(IsParticipant)
                CurTabType = ChampionLobbyTabType.SET;
            else
                CurTabType = ChampionLobbyTabType.BET;

            OnClickTabButton((int)CurTabType);
        }
        public void OnClickTabButton(int _tabType)
        {
            if (tabButtonSetting == null || tabButtonBetting == null) return;
            if (tabLayerSetting == null || tabLayerBetting == null || shopBtn == null) return;
            if (tabSetTitle == null || tabBetTitle == null) return;
            if (SetButtonSprite == null || BetButtonSprite == null || DisableBtnSprite == null || EnableBtnSprite == null) return;

            tabButtonSetting.interactable = true;
            tabButtonBetting.interactable = true;

            switch (_tabType)
            { 
                case (int)ChampionLobbyTabType.SET:
                    if (ChampionManager.Instance.CurChampionInfo.CurState >= ChampionInfo.ROUND_STATE.PREPARATION)
                    {
                        SetButtonSprite.sprite = EnableBtnSprite;
                    }
                    else
                    {
                        SetButtonSprite.sprite = DisableBtnSprite;
                    }
                    if (IsParticipant)
                    {
                        CurTabType = ChampionLobbyTabType.SET;
                        tabLayerSetting.SetActive(true);
                        tabLayerBetting.SetActive(false);
                        tabButtonSetting.interactable = false;
                        tabSetTitle.color = DefaultColor;
                        tabBetTitle.color = DeactiveColor;
                    }
                    else
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트10"), StringData.GetStringByStrKey("확인"), "",
                        () => { }
                        );
                    }
                    break;
                case (int)ChampionLobbyTabType.BET:
                    if(!User.Instance.ENABLE_P2E)
                    {
                        shopBtn.SetActive(false);
                    }
                    if (IsInTime)
                    {
                        BetButtonSprite.sprite = EnableBtnSprite;
                    }
                    else
                    {
                        BetButtonSprite.sprite = DisableBtnSprite;
                    }
                    CurTabType = ChampionLobbyTabType.BET;
                    tabButtonBetting.interactable = false;
                    tabLayerBetting.SetActive(true);
                    tabLayerSetting.SetActive(false);
                    tabSetTitle.color = DeactiveColor;
                    tabBetTitle.color = DefaultColor;
                    break;
            }
        }
    }
}