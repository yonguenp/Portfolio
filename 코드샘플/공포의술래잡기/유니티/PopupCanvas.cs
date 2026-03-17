using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupCanvas : MonoBehaviour
{
    //POPUP_TYPE은 ENUM으로 관리되기도하지만,
    //프리펩들의 관리는 Prefab에 Attach된 각 Popup Class에서 하기때문에
    //순서가 꼬이면 확인 및 잔손이 많이가니까
    //애지간하면 중간삽입 말고 밑에 추가하는 방식으로 enum관리 필요합니다.
    public enum POPUP_TYPE
    {
        SHOP_POPUP,
        GACHATABLE_POPUP,
        COMBINEINVENTORY_POPUP,
        FRIEND_POPUP,
        CLAN_POPUP,
        BUY_POPUP,
        MAIL_POPUP,
        CHARACTER_POPUP,
        RANK_POPUP,

        FADE_TEXT_POPUP,
        DEVLOPING_NOTICE_POPUP,
        MATCH_INFO_POPUP,
        MESSAGE_POPUP,
        CONFIRM_POPUP,

        TUTORIAL_POPUP,

        OPTION_POPUP,
        PRACTICEMATCHING_POPUP,
        ADVANCEMENT_POPUP,
        GACHA_POPUP,
        GACHARESULT_POPUP,
        RANKREWARD_POPUP,

        NEW_CHARACTER_POPUP,
        QUEST_POPUP,
        CHARACTERLIST_POPUP,
        GAMELOG_POPUP,
        DUO_POPUP,
        REWARD_POPUP,
        TOOLTIP_POPUP,
        ITEM_INFO_POPUP,
        TOUCH_EFFECT,
        SERVER_NOTIFY_POPUP,
        ATTENDANCE_POPUP,
        BATTLEPASS_POPUP,
        TALENT_POPUP,
        EXCHANGE_POPUP,
        QUIT_POPUP,
        PACKAGE_BUY_POPUP,
        ITEM_USE_POPUP,
        HALLOWEENEVENT_POPUP,
        COLLECTION_POPUP,
        EQUIPMENT_POPUP,
        ATTENDANCEMONTH_POPUP,
        CHRISTMAS_POPUP,
        CHRISTMASATTENDANCE_POPUP,
        BOXGACHA_POPUP,
        NICKCHANGE_POPUP,
        KOREANEWYEAR_POPUP,
        CHARACTERSELECT_POPUP,

        POPUP_TYPE_MAX,
    };

    public void SetUICamera(Camera uiCamera, int sortOrder = 1000)
    {
        Canvas.worldCamera = uiCamera;
        Canvas.sortingOrder = sortOrder;

        TouchEffect te = Popup[(int)POPUP_TYPE.TOUCH_EFFECT].GetComponent<TouchEffect>();
        if (te != null)
            te.SetUICamera(uiCamera);
    }

    [SerializeField]
    Canvas Canvas;

    [SerializeField]
    PopupBackground Background;

    private Popup[] Popup = new Popup[(int)POPUP_TYPE.POPUP_TYPE_MAX];
    private List<Popup> BackgroundPopup = new List<Popup>();

    public List<Popup> PopupEscList = new List<Popup>();
    static PopupCanvas _instance = null;
    static public PopupCanvas Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject prefab = Resources.Load("Prefabs/UI/PopupCanvas") as GameObject;
                GameObject popupCanvas = GameObject.Instantiate(prefab) as GameObject;
                _instance = popupCanvas.GetComponent<PopupCanvas>();

                DontDestroyOnLoad(popupCanvas);
            }
            return _instance;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;

        AttachChildPopup();

        ClearAll();
    }
    private void Update()
    {
        PopupMathchSet();
    }

    public Popup GetPopup(POPUP_TYPE type)
    {
        if (Popup[(int)type] == null)
        {
            //GameObject popupObject = Instantiate(PopupPrefabs[(int)type]);
            //popupObject.transform.SetParent(transform);
            //popupObject.transform.localPosition = Vector3.zero;
            //popupObject.transform.localScale = Vector3.one;

            //AttachPopup(type, popupObject.GetComponentInChildren<Popup>());

            //prefab 인스턴스화는 load 비효율로 인해 생각해봐야함.일단 PopupCanvas와 함께 Preload 방식으로 통일하겠음.
            //SBDebug.LogError("팝업을 찾을 수 없슴");
            return null;
        }

        return Popup[(int)type];
    }

    void AttachChildPopup()
    {
        foreach (Popup child in GetComponentsInChildren<Popup>(true))
        {
            child.AttachPopupCanvas();
        }
    }
    public void AttachPopup(POPUP_TYPE type, Popup popup)
    {
        if (Popup[(int)type] != null)
            SBDebug.LogError($"Popup Type 중첩!! 확인 필요:: {type}, {popup.name}");

        Popup[(int)type] = popup;
    }

    public bool IsOpeningPopup(POPUP_TYPE type)
    {
        Popup target = GetPopup(type);
        if (target == null)
            return false;

        return target.IsOpening();
    }

    public bool IsOpeningPopup()
    {
        foreach (Popup popup in Popup)
        {
            if (popup != null)
            {
                if (popup.IsOpening())
                    return true;
            }
        }

        return false;
    }

    public List<Popup> GetOpeningPopups()
    {
        List<Popup> ret = new List<Popup>();
        foreach (Popup popup in Popup)
        {
            if (popup != null)
            {
                if (popup.IsOpening())
                    ret.Add(popup);
            }
        }

        return ret;
    }

    public void ShowPopup(POPUP_TYPE type, Popup.CloseCallback cb = null)
    {
        Popup target = GetPopup(type);
        target.Open(cb);

        if (target.UseBackground())
        {
            SetBackground(target);
        }

        if (type != POPUP_TYPE.TOOLTIP_POPUP && IsOpeningPopup(POPUP_TYPE.TOOLTIP_POPUP))
        {
            ClosePopup(POPUP_TYPE.TOOLTIP_POPUP);
        }

        if (IsOpeningPopup(POPUP_TYPE.MESSAGE_POPUP))
        {
            Popup[(int)POPUP_TYPE.MESSAGE_POPUP].transform.SetAsLastSibling();
        }
        if (IsOpeningPopup(POPUP_TYPE.CONFIRM_POPUP))
        {
            Popup[(int)POPUP_TYPE.CONFIRM_POPUP].transform.SetAsLastSibling();
        }
        if (IsOpeningPopup(POPUP_TYPE.MATCH_INFO_POPUP))
        {
            Popup[(int)POPUP_TYPE.MATCH_INFO_POPUP].transform.SetAsLastSibling();
            (Popup[(int)POPUP_TYPE.MATCH_INFO_POPUP] as MatchInfoPopup).CheckMode();
        }
        if (IsOpeningPopup(POPUP_TYPE.FADE_TEXT_POPUP))
        {
            Popup[(int)POPUP_TYPE.FADE_TEXT_POPUP].transform.SetAsLastSibling();
        }

        Popup[(int)POPUP_TYPE.TOUCH_EFFECT].transform.SetAsLastSibling();

        if (IsOpeningPopup(POPUP_TYPE.SERVER_NOTIFY_POPUP))
        {
            Popup[(int)POPUP_TYPE.SERVER_NOTIFY_POPUP].transform.SetAsLastSibling();
        }

        OpenPopupStack(target);
    }

    public void ClosePopup(POPUP_TYPE type)
    {
        GetPopup(type).Close();
    }

    public void OnClosedPopup(POPUP_TYPE type)
    {
        Popup target = GetPopup(type);

        if (target.UseBackground())
        {
            ReleaseBackground(target);
        }

        if (IsOpeningPopup(POPUP_TYPE.MATCH_INFO_POPUP))
        {
            (Popup[(int)POPUP_TYPE.MATCH_INFO_POPUP] as MatchInfoPopup).CheckMode();
        }

        if (IsOpeningPopup(POPUP_TYPE.TOOLTIP_POPUP))
        {
            ClosePopup(POPUP_TYPE.TOOLTIP_POPUP);
        }
    }

    public void ClearAll()
    {
        ClearBackground();

        foreach (Popup popup in Popup)
        {
            if (popup != null)
            {
                switch (popup.GetPopupType())
                {
                    case POPUP_TYPE.SERVER_NOTIFY_POPUP:
                        if (popup.IsOpening())
                        {
                            Popup[(int)POPUP_TYPE.SERVER_NOTIFY_POPUP].transform.SetAsLastSibling();
                        }
                        break;
                    default:
                        popup.CloseForce();
                        break;
                }
            }
        }
        PopupEscList.Clear();
    }

    public void SceneChanged()
    {
        if (Managers.Network.IsAlive())
            ClearAll();

        if (Managers.Scene.CurrentScene != null && (Managers.Scene.CurrentScene.name == "GameScene" || Managers.Scene.CurrentScene.name == "Start"))
        {
            ClosePopup(POPUP_TYPE.TOUCH_EFFECT);
        }
        else
        {
            ShowPopup(POPUP_TYPE.TOUCH_EFFECT);
        }
    }

    void ResetCamera()
    {
        Camera targetCam = null;
        foreach (Camera cam in Camera.allCameras)
        {
            int UILayerBit = (1 << LayerMask.NameToLayer("UI"));
            if ((cam.cullingMask & UILayerBit) > 0)
            {
                if (targetCam != null)
                {
                    if ((cam.cullingMask ^ UILayerBit) == 0)
                    {
                        targetCam = cam;
                    }
                }
                else
                {
                    targetCam = cam;
                }
            }
        }
        Canvas.worldCamera = targetCam;
    }

    public void ShowFadeText(string text, params object[] obj)
    {
        FadeText popup = GetPopup(POPUP_TYPE.FADE_TEXT_POPUP) as FadeText;
        popup.SetText(StringManager.GetString(text, obj), Color.white);

        ShowPopup(POPUP_TYPE.FADE_TEXT_POPUP);
    }

    public void ShowFadeTextWithColor(string text, Color color, params object[] obj)
    {
        FadeText popup = GetPopup(POPUP_TYPE.FADE_TEXT_POPUP) as FadeText;
        popup.SetText(StringManager.GetString(text, obj), color);


        ShowPopup(POPUP_TYPE.FADE_TEXT_POPUP);
    }

    public void ShowServerNotifyText(string text, params object[] obj)
    {
        ServerNotifyPopup popup = GetPopup(POPUP_TYPE.SERVER_NOTIFY_POPUP) as ServerNotifyPopup;
        popup.SetServerNotify(StringManager.GetString(text, obj), 30);

        ShowPopup(POPUP_TYPE.SERVER_NOTIFY_POPUP);
    }

    public void ShowServerNotifyTextWithTime(string text, float time = 30, params object[] obj)
    {
        ServerNotifyPopup popup = GetPopup(POPUP_TYPE.SERVER_NOTIFY_POPUP) as ServerNotifyPopup;
        popup.SetServerNotify(StringManager.GetString(text, obj), time);

        ShowPopup(POPUP_TYPE.SERVER_NOTIFY_POPUP);
    }

    public void ShowMessagePopup(string text, Popup.CloseCallback cb = null, params object[] obj)
    {
        ShowMessagePopup(text, StringManager.GetString("button_check"), cb, obj);
    }

    public void ShowMessagePopup(string text, string buttonText, Popup.CloseCallback cb = null, params object[] obj)
    {
        MessagePopup popup = GetPopup(POPUP_TYPE.MESSAGE_POPUP) as MessagePopup;
        popup.ShowMessage(StringManager.GetString(text, obj), StringManager.GetString(buttonText));

        ShowPopup(POPUP_TYPE.MESSAGE_POPUP, cb);
    }

    public void ShowConfirmPopup(string text, Popup.CloseCallback cb = null, params object[] obj)
    {
        ShowConfirmPopup(text, StringManager.GetString("button_check"), StringManager.GetString("button_cancel"), cb, null, obj);
    }

    public void ShowConfirmPopup(string text, string oktext, string canceltext, Popup.CloseCallback okcb = null, Popup.CloseCallback cancelcb = null, params object[] obj)
    {
        ConfirmPopup popup = GetPopup(POPUP_TYPE.CONFIRM_POPUP) as ConfirmPopup;
        popup.ShowConfirm(StringManager.GetString(text, obj), oktext, canceltext, okcb);

        ShowPopup(POPUP_TYPE.CONFIRM_POPUP, cancelcb);
    }

    public void ShowTooltip(ASSET_TYPE type, Vector3 pos)
    {
        TooltipPopup popup = GetPopup(POPUP_TYPE.TOOLTIP_POPUP) as TooltipPopup;
        popup.Init(type, pos);

        ShowPopup(POPUP_TYPE.TOOLTIP_POPUP);
    }

    public void ShowTooltip(string name, string desc, Vector3 pos)
    {
        TooltipPopup popup = GetPopup(POPUP_TYPE.TOOLTIP_POPUP) as TooltipPopup;
        popup.Init(name, desc, pos);

        ShowPopup(POPUP_TYPE.TOOLTIP_POPUP);
    }

    public void ShowTooltip(BundleInfo bundleInfos, Vector3 pos)
    {
        TooltipPopup popup = GetPopup(POPUP_TYPE.TOOLTIP_POPUP) as TooltipPopup;
        popup.Init(bundleInfos, pos);

        ShowPopup(POPUP_TYPE.TOOLTIP_POPUP);
    }

    public void ShowTooltip(string name, List<BundleInfo> bundleInfos, Vector3 pos)
    {
        TooltipPopup popup = GetPopup(POPUP_TYPE.TOOLTIP_POPUP) as TooltipPopup;
        popup.Init(name, bundleInfos, pos);

        ShowPopup(POPUP_TYPE.TOOLTIP_POPUP);
    }

    public void ShowItemInfo(string title, List<BundleInfo> bundleInfos)
    {
        ItemInfoPopup popup = GetPopup(POPUP_TYPE.ITEM_INFO_POPUP) as ItemInfoPopup;
        popup.Init(title, bundleInfos);

        ShowPopup(POPUP_TYPE.ITEM_INFO_POPUP);
    }

    public void ShowBuyPopup(ShopItemGameData shopItem, BuyPopup.BuyCallback okCB, Popup.CloseCallback cb = null)
    {
        int type = 0;
        var defineData = PopupDefine.GetData(shopItem.GetID());
        if (defineData != null)
        {
            type = defineData.type;
            if (type > 1)
            {
                PackageBuyPopup pbp = GetPopup(POPUP_TYPE.PACKAGE_BUY_POPUP) as PackageBuyPopup;
                pbp.Init(defineData, shopItem, okCB, (PackageBuyPopup.PACKAGE_POPUP_TYPE)type);

                ShowPopup(POPUP_TYPE.PACKAGE_BUY_POPUP, cb);
                return;
            }
        }

        BuyPopup popup = GetPopup(POPUP_TYPE.BUY_POPUP) as BuyPopup;
        popup.Init(shopItem, okCB, type);
        if (shopItem.menu_id == 1 && shopItem.price.type == ASSET_TYPE.ADVERTISEMENT)
        {
            popup.OnBuy();
        }
        else
        {
            ShowPopup(POPUP_TYPE.BUY_POPUP, cb);
        }
    }

    public void ShowCharacterSelectPopup(ItemGameData itemData, InventoryPopup inven, Popup.CloseCallback cb = null)
    {
        CharacterSelectPopup popup = GetPopup(POPUP_TYPE.CHARACTERSELECT_POPUP) as CharacterSelectPopup;
        popup.Init(itemData, inven);

        ShowPopup(POPUP_TYPE.CHARACTERSELECT_POPUP, cb);
    }

    public void ShowInventoryPopup(InventoryPopup.SelectedCallback itemSelectedCallback = null, List<CombineInventoryPopup.InventoryTab> tabs = null)
    {
        CombineInventoryPopup popup = GetPopup(POPUP_TYPE.COMBINEINVENTORY_POPUP) as CombineInventoryPopup;
        if (itemSelectedCallback == null)
            popup.isOpen = 0;
        else
            popup.isOpen = 1;
        popup.SetTab(itemSelectedCallback, tabs);

        ShowPopup(POPUP_TYPE.COMBINEINVENTORY_POPUP);
    }

    public void ShowInventoryPopup(bool isForce , Popup.CloseCallback cb = null, List<CombineInventoryPopup.InventoryTab> tabs = null)
    {
        CombineInventoryPopup popup = GetPopup(POPUP_TYPE.COMBINEINVENTORY_POPUP) as CombineInventoryPopup;
        popup.isOpen = 1;
        popup.SetTab(null, tabs);

        ShowPopup(POPUP_TYPE.COMBINEINVENTORY_POPUP, cb);

    }

    public void ShowRewardResult(List<SBWeb.ResponseReward> shopItem, Popup.CloseCallback cb = null)
    {
        ShowPopup(POPUP_TYPE.REWARD_POPUP, cb);

        RewardPopup popup = GetPopup(POPUP_TYPE.REWARD_POPUP) as RewardPopup;
        popup.SetData(RewardPopup.RewardType.REWARD, shopItem);
    }

    public void ShowBuyResult(List<SBWeb.ResponseReward> shopItem, Popup.CloseCallback cb = null)
    {
        ShowPopup(POPUP_TYPE.REWARD_POPUP, cb);

        RewardPopup popup = GetPopup(POPUP_TYPE.REWARD_POPUP) as RewardPopup;
        popup.SetData(RewardPopup.RewardType.BUY, shopItem);
    }
    public void ShowEquipEchantResult(UserEquipData prior, UserEquipData cur, Popup.CloseCallback cb = null)
    {
        ShowPopup(POPUP_TYPE.REWARD_POPUP, cb);

        RewardPopup popup = GetPopup(POPUP_TYPE.REWARD_POPUP) as RewardPopup;
        popup.ResultEquipEnchant(prior, cur);
    }

    public void ShowAttandancePopup(int attendance_id, Popup.CloseCallback cb = null)
    {
        AttendancePopup popup = GetPopup(POPUP_TYPE.ATTENDANCE_POPUP) as AttendancePopup;
        popup.SetID(attendance_id);

        ShowPopup(POPUP_TYPE.ATTENDANCE_POPUP, cb);
    }

    public void ShowShopPopup(ShopMenuGameData menuData = null, Popup.CloseCallback cb = null)
    {
        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("z3g8hy");
        com.adjust.sdk.Adjust.trackEvent(ae);

        ShopPopup popup = GetPopup(POPUP_TYPE.SHOP_POPUP) as ShopPopup;
        popup.OnMenuItem(menuData);

        ShowPopup(POPUP_TYPE.SHOP_POPUP, cb);
    }

    public void ShowNewCharacter(CharacterGameData data, Popup.CloseCallback cb = null, bool isNew = true)
    {
        NewCharacterPopup popup = GetPopup(POPUP_TYPE.NEW_CHARACTER_POPUP) as NewCharacterPopup;
        popup.SetData(data);

        ShowPopup(POPUP_TYPE.NEW_CHARACTER_POPUP, cb);


        if (isNew)
        {
            if (data.char_grade == 4)
            {
                foreach (var uc in Managers.UserData.MyCharacters)
                {
                    if (uc.Value.characterData != null)
                    {
                        if (uc.Value.characterData.char_grade == 4)
                        {
                            if (uc.Value.characterData.GetID() != data.GetID())
                                return;
                        }
                    }
                }
                com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("ua6j5b");
                com.adjust.sdk.Adjust.trackEvent(ae);
            }
            if (data.char_grade == 3)
            {
                foreach (var uc in Managers.UserData.MyCharacters)
                {
                    if (uc.Value.characterData != null)
                    {
                        if (uc.Value.characterData.char_grade == 3)
                        {
                            if (uc.Value.characterData.GetID() != data.GetID())
                                return;
                        }
                    }
                }
                com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("n34pdf");
                com.adjust.sdk.Adjust.trackEvent(ae);
            }
        }
    }
    public void ShowHelpPopup(Popup.CloseCallback cb = null)
    {
        foreach (Popup popup in GetOpeningPopups())
        {
            switch (popup.GetPopupType())
            {
                case POPUP_TYPE.CHARACTER_POPUP:
                case POPUP_TYPE.SHOP_POPUP:
                case POPUP_TYPE.GACHA_POPUP:
                case POPUP_TYPE.EXCHANGE_POPUP:
                    ShowHelpPopup(TutorialPopup.HelpTapType.CHARACTER, cb);
                    return;
                case POPUP_TYPE.QUEST_POPUP:
                    ShowHelpPopup(TutorialPopup.HelpTapType.RULE, cb);
                    return;
            }
        }

        ShowHelpPopup(TutorialPopup.HelpTapType.MATCH, cb);
    }

    public void ShowHelpPopup(TutorialPopup.HelpTapType type, Popup.CloseCallback cb = null)
    {
        TutorialPopup popup = GetPopup(POPUP_TYPE.TUTORIAL_POPUP) as TutorialPopup;
        popup.SetMenu(type);

        ShowPopup(POPUP_TYPE.TUTORIAL_POPUP, cb);
    }

    public void ShowItemUsePopup(ItemGameData data, UsePopup.UseCallback cb = null)
    {
        UsePopup popup = GetPopup(POPUP_TYPE.ITEM_USE_POPUP) as UsePopup;
        popup.Init(data, cb);

        ShowPopup(POPUP_TYPE.ITEM_USE_POPUP);
    }

    public void ShowShopPopup(int menuID, Popup.CloseCallback cb = null)
    {
        ShopPopup popup = GetPopup(POPUP_TYPE.SHOP_POPUP) as ShopPopup;
        popup.SetMenu(menuID);
        ShowPopup(POPUP_TYPE.SHOP_POPUP, cb);
    }

    public void ShowExchangePopup(int menuID, Popup.CloseCallback cb = null)
    {
        ExchangePopup popup = GetPopup(POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;
        popup.SetMenu(menuID);
        ShowPopup(POPUP_TYPE.EXCHANGE_POPUP, cb);
    }

    public void ShowQuestPopup(int menuID = 0, Popup.CloseCallback cb = null)
    {
        ShowPopup(POPUP_TYPE.QUEST_POPUP, cb);

        QuestPopup popup = GetPopup(POPUP_TYPE.QUEST_POPUP) as QuestPopup;
        popup.TabButton(menuID);
    }

    public void ShowGachaPopup(int menuID = 0, Popup.CloseCallback cb = null)
    {
        ShowPopup(POPUP_TYPE.GACHA_POPUP, cb);
        GachaPopup popup = GetPopup(POPUP_TYPE.GACHA_POPUP) as GachaPopup;
        popup.OnSelectMenu(menuID);
    }
    void SetBackground(Popup popup)
    {
        if (BackgroundPopup.Contains(popup))
            BackgroundPopup.Remove(popup);

        BackgroundPopup.Add(popup);
        Background.SetActive(true);
        Background.transform.SetSiblingIndex(popup.transform.GetSiblingIndex() - 1);
    }

    void ReleaseBackground(Popup popup)
    {
        if (BackgroundPopup.Contains(popup))
            BackgroundPopup.Remove(popup);

        Invoke("RefreshBackground", 0.1f);
    }

    void RefreshBackground()
    {
        CancelInvoke("RefreshBackground");

        Popup target = null;
        int lastIndex = 0;
        foreach (Popup p in BackgroundPopup)
        {
            if (lastIndex < p.transform.GetSiblingIndex())
            {
                lastIndex = p.transform.GetSiblingIndex();
                target = p;
            }
        }

        if (target == null)
            Background.SetActive(false);
        else
            SetBackground(target);
    }

    void ClearBackground()
    {
        BackgroundPopup.Clear();
        Background.SetActive(false);
    }
    public void PopupMathchSet()
    {
        if (Canvas != null)
        {
            var scaler = Canvas.GetComponent<CanvasScaler>();
            var ratio = Screen.safeArea.size.x / Screen.safeArea.size.y;
            // 가로가 더 길다.
            if (ratio >= 16f / 9f)
            {
                // 가로가 더 길면 height에 맞춘다
                scaler.matchWidthOrHeight = 1f;
            }
            else
            {
                // 가로가 더 길면 width에 맞춘다
                scaler.matchWidthOrHeight = 0f;
            }
        }
    }

    public Rect GetWorldRect()
    {
        return GetWorldRect(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)));
    }

    public Rect GetWorldRect(Rect target)
    {
        Vector2 min = Canvas.worldCamera.ScreenToWorldPoint(target.position);
        Vector2 max = Canvas.worldCamera.ScreenToWorldPoint(target.position + target.size);

        return new Rect(min, max - min);
    }

    public void OpenPopupStack(Popup popup)
    {
        switch (popup.GetPopupType())
        {
            case POPUP_TYPE.TOOLTIP_POPUP:
            case POPUP_TYPE.TOUCH_EFFECT:
            case POPUP_TYPE.MATCH_INFO_POPUP:
            case POPUP_TYPE.FADE_TEXT_POPUP:
                return;
        }

        if (PopupEscList.Contains(popup))
            return;
        PopupEscList.Add(popup);
    }
}
