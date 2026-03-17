using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandboxPlatform.SAMANDA;


public class UITop : MonoBehaviour, EventListener<NotifyEvent>
{
    public enum TOPUI_TYPE
    {
        COMMON,
        LOBBY,
        SHOP,
        GACHAPOPUP,
        GACHARESUTL,
        EXCHANGEPOPUP,
        CHARACTERPOPUP,
    };
    [Header("UI Type")]
    [SerializeField] TOPUI_TYPE uiType = TOPUI_TYPE.COMMON;

    [Header("Atr")]
    [SerializeField] GameObject GOLD;
    [SerializeField] GameObject DIA;
    [SerializeField] GameObject MILEAGE;
    [SerializeField] GameObject SOULSTONE;
    [SerializeField] GameObject TICKETS;
    [SerializeField] GameObject MAIL;
    [SerializeField] GameObject MENU;

    [Header("UI Container")]
    [SerializeField] UISubMenu UISubMenu;
    [SerializeField] GameObject PlayerInfo;
    [SerializeField] GameObject CurInfo;

    [Header("유져정보")]
    [SerializeField] Text UserName;
    [SerializeField] Text ClanName;
    [SerializeField] Image userRankIcon;
    [SerializeField] Image userRankGauge;
    [SerializeField] Text userRankGaugeText;

    [Header("[RedDot]")]
    [SerializeField] GameObject userRankReddot;
    [SerializeField] GameObject mailRedDot;


    private void OnEnable()
    {
        switch (uiType)
        {
            case TOPUI_TYPE.LOBBY:
                PlayerInfo.SetActive(true);
                CurInfo.SetActive(false);
                GOLD.SetActive(true);
                MILEAGE.SetActive(false);
                DIA.SetActive(true);
                MAIL.SetActive(true);
                MENU.SetActive(true);
                this.EventStartListening();
                break;
            case TOPUI_TYPE.COMMON:
                PlayerInfo.SetActive(false);
                CurInfo.SetActive(true);
                GOLD.SetActive(true);
                MILEAGE.SetActive(false);
                DIA.SetActive(true);
                MAIL.SetActive(false);
                MENU.SetActive(true);
                if (SOULSTONE != null)
                    SOULSTONE.SetActive(false);
                if (TICKETS != null)
                    TICKETS.SetActive(false);
                break;
            case TOPUI_TYPE.CHARACTERPOPUP:
                PlayerInfo.SetActive(false);
                CurInfo.SetActive(true);
                GOLD.SetActive(true);
                MILEAGE.SetActive(false);
                DIA.SetActive(true);
                MAIL.SetActive(false);
                MENU.SetActive(true);
                if (SOULSTONE != null)
                    SOULSTONE.SetActive(true);
                if (TICKETS != null)
                    TICKETS.SetActive(false);
                break;
            case TOPUI_TYPE.SHOP:
                PlayerInfo.SetActive(false);
                CurInfo.SetActive(true);

                MAIL.SetActive(false);
                MENU.SetActive(true);
                break;
            case TOPUI_TYPE.GACHAPOPUP:
                GOLD.SetActive(false);
                DIA.SetActive(true);
                TICKETS.SetActive(true);
                MILEAGE.SetActive(true);
                break;
            case TOPUI_TYPE.GACHARESUTL:
                MILEAGE.SetActive(true);
                CurInfo.SetActive(false);
                break;
            case TOPUI_TYPE.EXCHANGEPOPUP:
                GOLD.SetActive(true);
                DIA.SetActive(true);
                SOULSTONE.SetActive(true);
                break;

        }
    }

    private void OnDisable()
    {
        switch (uiType)
        {
            case TOPUI_TYPE.LOBBY:
                this.EventStopListening();
                break;
            case TOPUI_TYPE.COMMON:
                break;
            case TOPUI_TYPE.SHOP:
                break;
        }
    }

    void Start()
    {
        userRankReddot.SetActive((PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.RANKREWARD_POPUP) as RankRewardPopup).IsNew);
        Refresh();
    }

    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_RANK_REWARD:
                {
                    userRankReddot.SetActive((PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.RANKREWARD_POPUP) as RankRewardPopup).IsNew);
                }
                break;
            case NotifyEvent.NotifyEventMessage.ON_MAIL_INFO:
                {
                    mailRedDot.SetActive((PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MAIL_POPUP) as MailPopup).isNew);
                }
                break;
        }
    }

    public void Refresh()
    {
        switch (uiType)
        {
            case TOPUI_TYPE.LOBBY:
                RefreshPlayerInfo();
                break;
            case TOPUI_TYPE.COMMON:
                break;
            case TOPUI_TYPE.SHOP:
                break;
        }
    }

    void RefreshPlayerInfo()
    {
        UserName.SetText(Managers.UserData.MyName, SHelper.TEXT_TYPE.ME);
        if (ClanName != null)
        {
            ClanName.SetText(Managers.UserData.MyClanName);
            ClanName.SetLayoutDirty();
        }

        userRankIcon.sprite = Managers.UserData.MyRank.rank_resource;

        float rangeRankPoint = Managers.UserData.MyRank.end_point - Managers.UserData.MyRank.start_point;
        float curRankPoint = Managers.UserData.MyPoint - Managers.UserData.MyRank.start_point;
        Vector2 size = userRankGauge.rectTransform.sizeDelta;
        if (!(Managers.UserData.MyRank.start_point >= 40000))
        {
            size.x = (userRankGauge.transform.parent.transform as RectTransform).sizeDelta.x * (curRankPoint / rangeRankPoint);
            size.x = Mathf.Min(size.x, userRankGauge.rectTransform.sizeDelta.x);
        }
        userRankGauge.rectTransform.sizeDelta = size;

        //if (Managers.UserData.MyRank == RankType.MaxRank)
        //    userRankGaugeText.text = string.Format($"{curRankPoint} / -");
        //else
        //    userRankGaugeText.text = string.Format($"{curRankPoint} / {rangeRankPoint}");
        userRankGaugeText.text = Managers.UserData.MyPoint.ToString();
    }

    public void SetDisplayAsset(ShopMenuGameData.UI_TYPE type)
    {
        GOLD.SetActive((type & ShopMenuGameData.UI_TYPE.GOLD) > 0);
        DIA.SetActive((type & ShopMenuGameData.UI_TYPE.DIA) > 0);
        MILEAGE.SetActive((type & ShopMenuGameData.UI_TYPE.MILEAGE) > 0);
        SOULSTONE.SetActive((type & ShopMenuGameData.UI_TYPE.SOULSTONE) > 0);
    }

    public void OnDiaButton()
    {
        PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
    }

    public void OnGoldButton()
    {
        PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
    }

    public void OnMail()
    {
        Managers.IAP.CheckPendingProducts();
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.MAIL_POPUP);
    }

    public void OnOption()
    {
        UISubMenu.Clear();
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.OPTION_POPUP);
    }

    public void OnMatchLog()
    {
        UISubMenu.Clear();
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.GAMELOG_POPUP);
    }

    public void OnHelp()
    {
        UISubMenu.Clear();
        PopupCanvas.Instance.ShowHelpPopup();
    }

    public void OnInventory()
    {
        UISubMenu.Clear();
        PopupCanvas.Instance.ShowInventoryPopup();
    }
    public void OnRank()
    {
        UISubMenu.Clear();
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.RANK_POPUP);
    }
    public void OnNotice()
    {
        UISubMenu.Clear();
        SAMANDA.Instance.UI.SetUIState(LOGIN_STATE.MAIN_OPEN);
        Managers.UserData.ShowFirstNotice();
    }

    public void OnAttendance()
    {
        UISubMenu.Clear();
        PopupCanvas.Instance.ShowAttandancePopup(1);
    }
    public void OnAttendanceMonth()
    {
        UISubMenu.Clear();
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.ATTENDANCEMONTH_POPUP);
    }

    public void OnEmoticon()
    {
        UISubMenu.Clear();
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("매치대기캐릭터변경불가"));
            return;
        }
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.COMBINEINVENTORY_POPUP);
    }

    public void OnCollection()
    {
        UISubMenu.Clear();
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.COLLECTION_POPUP);
    }
}
