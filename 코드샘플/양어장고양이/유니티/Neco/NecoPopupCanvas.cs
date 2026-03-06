using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class NecoPopupCanvas : NecoCanvas
{
    public SuperBlur.SuperBlurBase MainCamBlur;
    public Camera PopupCamera;
    public GameObject TouchGuard;
    public GameObject PopupGroup;
    public GameObject[] ScaleContorlObjects;

    [Header("[TOP UI Info Layer]")]
    public GameObject topUIInfoLayer;

    [Header("Decorations")]
    public HalloweenDecoration halloweenDecoration;

    private Coroutine coroutineBlurAction = null;
    private float InterpolationMin = 0.0f;
    private float InterpolationMax = 1.0f;
    private Dictionary<RectTransform, Vector3> ResizeTranformData = new Dictionary<RectTransform, Vector3>();

    private List<GameObject> openPopupList = new List<GameObject>();
    private bool bNeedCheckProduct = false;
    private List<uint> BannerList = new List<uint>();
    public enum POPUP_TYPE {
        CAT_LIST_POPUP,
        CAT_DETAIL_POPUP,
        CAT_FEED_POPUP,
        CAT_FEED_CONFIRM_POPUP,
        CAT_COOK_LIST_POPUP,
        CAT_COOK_POPUP,
        CAT_MAKING_LIST_POPUP,
        CAT_MAKING_POPUP,
        CAT_CRAFTING_WAIT,
        CAT_SUPPLY_UI_POPUP,
        CAT_FISH_FARM_POPUP,
        CAT_FISH_TRAP_POPUP,
        REWARD_POPUP,
        LEVELUP_POPUP,
        SEASON_PASS_POPUP,
        MESSAGE_POPUP,
        VIDEO_LOADING_POPUP,
        FRIEND_INFO_POPUP,
        SCRIPT_POPUP,
        TOUCH_DEFENDER,
        TOAST_POPUP,
        IMAGE_TOAST_POPUP,
        SUCCESS_EFFECT_POPUP,
        SYSTEM_CONFIRM_POPUP,
        CAT_PHOTO_GET_POPUP,
        CAT_PHOTO_VIEWER_POPUP,
        SHOP_LIST_POPUP,
        SHOP_BUY_COUNT_POPUP,
        TOUCH_CAT_EFFECT_POPUP,        
        MAIL_BOX_POPUP,
        ATTENDANCE_POPUP,
        SETTING_POPUP,        
        CARD_LIST_POPUP,
        TUTO_SUCCESS_POPUP,
        BANNER_POPUP,
        CATNIP_BUY_POPUP,
        RANDOMBOX_INFO,
        IAP_WAITER,
        SERVER_WAIT_POPUP,        
        ID_CHECKER,
        SUBSCRIBE_POPUP,
        FISH_TRADER_POPUP,
        FISH_TRUCK_BUY_COUNT_POPUP,
        REPAIR_POPUP,
        CHUSEOK_EVENT,
        SKIN_SETTING,
        FISHING_POPUP,
        HALLOWEEN_POPUP,
        IAP_OBJECT_HELP_POPUP,
        MYTITLE_POPUP,
        CHRISTMAS_EVENT,

        NECO_POPUP_ALL,
        NECO_POPUP_MAX = NECO_POPUP_ALL,
    };

    public enum POPUP_REFRESH_TYPE
    {
        NONE,
        CAT_FOOD,
        CAT_CRAFT,
        SHOP_CARD,
    }

    public GameObject[] PopupObject;

    public void OnPopupShow(POPUP_TYPE type)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)type].SetActive(true);

        openPopupList.Add(PopupObject[(int)type]);
    }

    public void OnShowScriptPopup(string script, NecoScriptPopup.Callback cb = null)
    {
        ShowPopupCanvas();
        OffTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.SCRIPT_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.SCRIPT_POPUP].GetComponent<NecoScriptPopup>().OnShowScript(script, cb);
    }

    public void OnShowScriptsPopup(List<string> script, NecoScriptPopup.Callback cb = null)
    {
        ShowPopupCanvas();
        OffTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.SCRIPT_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.SCRIPT_POPUP].GetComponent<NecoScriptPopup>().OnShowScript(script, cb);
    }

    public void OnTouchDefend(float time, TouchDefender.Callback cb)
    {
        ShowPopupCansvasNoBlur();

        PopupObject[(int)POPUP_TYPE.TOUCH_DEFENDER].SetActive(true);
        PopupObject[(int)POPUP_TYPE.TOUCH_DEFENDER].GetComponent<TouchDefender>().SetDefend(time, cb);
    }

    public void OnVideoLodingPopup()
    {
        ShowPopupCanvas();

        OffTopUIInfoLayer();

        PopupObject[(int)POPUP_TYPE.VIDEO_LOADING_POPUP].SetActive(true);
    }

    public void OnFeedPopupShow(neco_spot spotData)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_FEED_POPUP].GetComponent<NecoFeedListPanel>().SetFeedDataUI(spotData);
        PopupObject[(int)POPUP_TYPE.CAT_FEED_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_FEED_POPUP]);
    }

    public void OnFeedConfirmPopupShow(FoodData foodData, neco_spot spotData)
    {
        ShowPopupCansvasNoBlur();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_FEED_CONFIRM_POPUP].GetComponent<NecoFeedConfirmPanel>().SetFeedConfirmDataUI(foodData, spotData);
        PopupObject[(int)POPUP_TYPE.CAT_FEED_CONFIRM_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_FEED_CONFIRM_POPUP]);
    }

    public void OnCookUIPopupShow(FoodData foodData)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_COOK_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.CAT_COOK_POPUP].GetComponent<NecoCookPanel>().SetRecipeListUI(foodData);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_COOK_POPUP]);
    }

    public void OnCraftListPopupShow(bool isMat)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_MAKING_LIST_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.CAT_MAKING_LIST_POPUP].GetComponent<NecoCraftingListPanel>().OnShowCraftingListPanel(isMat);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_MAKING_LIST_POPUP]);
    }

    public void OnCraftUIPopupShow(CraftData craftData)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_MAKING_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.CAT_MAKING_POPUP].GetComponent<NecoMakingPanel>().SetRecipeListUI(craftData);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_MAKING_POPUP]);
    }

    public void OnSupplyContentsPopupShow(List<neco_data.neco_gift_info> giftList, SUPPLY_UI_TYPE supplyUIType)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_SUPPLY_UI_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.CAT_SUPPLY_UI_POPUP].GetComponent<NecoSupplyUIPanel>().InitSupplyListData(giftList, supplyUIType);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_SUPPLY_UI_POPUP]);
    }

    public void OnFishFarmPopupShow(List<RewardData> rewardList, SUPPLY_UI_TYPE supplyUIType)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_FISH_FARM_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.CAT_FISH_FARM_POPUP].GetComponent<NecoFishFarmPanel>().InitFishFarmListData(rewardList, supplyUIType);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_FISH_FARM_POPUP]);
    }

    public void OnFishTrapPopupShow(List<RewardData> rewardList, SUPPLY_UI_TYPE supplyUIType)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_FISH_TRAP_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.CAT_FISH_TRAP_POPUP].GetComponent<NecoFishTrapPanel>().InitFishtrapListData(rewardList, supplyUIType);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_FISH_TRAP_POPUP]);
    }

    public void OnLevelupPopupShow(SUPPLY_UI_TYPE levelupUIType, NecoLevelupSuccessPanel.Callback _closeCallback = null, recipe recipeData = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.LEVELUP_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.LEVELUP_POPUP].GetComponent<NecoLevelupUIPanel>().InitLevelupData(levelupUIType, _closeCallback, recipeData);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.LEVELUP_POPUP]);
    }

    public void OnCatDetailPopupShow(neco_cat catData)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CAT_LIST_POPUP].GetComponent<NecoCatListPanel>().SetSelectedCatInfo(catData);
        //PopupObject[(int)POPUP_TYPE.CAT_LIST_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CAT_LIST_POPUP]);
    }

    public void OnChuseokEventPopupShow(CHUSEOK_CATEGORY category = CHUSEOK_CATEGORY.MARBLE_GAME, System.Action close_cb = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CHUSEOK_EVENT].SetActive(true);
        
        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CHUSEOK_EVENT]);

        PopupObject[(int)POPUP_TYPE.CHUSEOK_EVENT].GetComponent<ChuseokUI>().InitChuseokPanel(category, close_cb);
        
    }

    public void OnChristmasEventPopupShow(ChristmasEventPopup.CATEGORY category = ChristmasEventPopup.CATEGORY.DRAW, System.Action close_cb = null)
    {
        ShowPopupCanvas();
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.GUIDE_QUEST);
        OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.CHRISTMAS_EVENT].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.CHRISTMAS_EVENT]);

        PopupObject[(int)POPUP_TYPE.CHRISTMAS_EVENT].GetComponent<ChristmasEventPopup>().Init(category);
    }

    public void OnCheckEventAttendance(bool toggleChat = false)
    {
        //if(PopupObject[(int)POPUP_TYPE.CHUSEOK_EVENT].GetComponent<ChuseokUI>().EnableAttendance())
        //    OnChuseokEventPopupShow(CHUSEOK_CATEGORY.ATTENDANCE);
        //else 
        if (PopupObject[(int)POPUP_TYPE.HALLOWEEN_POPUP].GetComponent<HalloweenAttendancePanel>().EnableAttendance())
        {
            OnPopupShow(POPUP_TYPE.HALLOWEEN_POPUP);
            if (toggleChat)
            {
                PopupObject[(int)POPUP_TYPE.HALLOWEEN_POPUP].GetComponent<HalloweenAttendancePanel>().SetToggleChat();
            }
        }
    }

    public void OnSeasonPassPopupShow()
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.SEASON_PASS_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.SEASON_PASS_POPUP].GetComponent<SeasonPassPanel>().InitSeasonPassUI();

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.SEASON_PASS_POPUP]);
    }

    public void OnFishingPopupShow()
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.FISHING_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.FISHING_POPUP].GetComponent<FishngMiniGame>().InitFishingUI();

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.FISHING_POPUP]);
    }

    public void OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY category = NecoShopPanel.SHOP_CATEGORY.PACKAGE, System.Action close_cb = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP].GetComponent<NecoShopPanel>().InitShopPanel(category, close_cb);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP]);
    }

    public void OnShopPurchaseCountPopupShow(bool isSpecailItem, neco_shop shopData, neco_market marketData, uint maxBuyCount, SystemConfirmPanel.CountCallback okCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.SHOP_BUY_COUNT_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.SHOP_BUY_COUNT_POPUP].GetComponent<NecoShopPurchaseCountPanel>().InitShopPurchaseCountPanel(isSpecailItem, shopData, marketData, maxBuyCount, okCallback);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.SHOP_BUY_COUNT_POPUP]);
    }

    public void OnFishtruckPurchaseCountPopupShow(FishTruckData fishtruckData, SystemConfirmPanel.CountCallback okCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.FISH_TRUCK_BUY_COUNT_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.FISH_TRUCK_BUY_COUNT_POPUP].GetComponent<FishtruckPurchaseCountPanel>().InitCommonPurchaseCountPanel(fishtruckData, okCallback);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.FISH_TRUCK_BUY_COUNT_POPUP]);
    }

    public void OnRewardPopupShow(string title, string msg, string uriParam, JArray rewardArray, NecoRewardPopup.Callback closeCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        bool isCatPhotoReward = PopupObject[(int)POPUP_TYPE.REWARD_POPUP].GetComponent<NecoRewardPopup>().SetRewardPopupData(title, msg, uriParam, rewardArray, closeCallback);
        if (isCatPhotoReward == false)
        {
            PopupObject[(int)POPUP_TYPE.REWARD_POPUP].SetActive(true);
            openPopupList.Add(PopupObject[(int)POPUP_TYPE.REWARD_POPUP]);
        }
    }

    public void OnSingleRewardPopup(string title, string msg, RewardData rewardData, NecoRewardPopup.Callback closeCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.REWARD_POPUP].GetComponent<NecoRewardPopup>().SetSingleRewardPopup(title, msg, rewardData, closeCallback);
        PopupObject[(int)POPUP_TYPE.REWARD_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.REWARD_POPUP]);
    }

    public void OnRewardListPopup(string title, string msg, List<RewardData> rewardList, NecoRewardPopup.Callback closeCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.REWARD_POPUP].GetComponent<NecoRewardPopup>().SetRewardListPopup(title, msg, rewardList, closeCallback);
        PopupObject[(int)POPUP_TYPE.REWARD_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.REWARD_POPUP]);
    }

    public void OnRewardListPopup(string title, string msg, Dictionary<string, RewardData> rewardDic, NecoRewardPopup.Callback closeCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.REWARD_POPUP].GetComponent<NecoRewardPopup>().SetRewardListPopup(title, msg, rewardDic, closeCallback);
        PopupObject[(int)POPUP_TYPE.REWARD_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.REWARD_POPUP]);
    }

    public void OnSystemMessagePopupShow(string title, string guide, SystemPopupPanel.Callback _closeCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.MESSAGE_POPUP].GetComponent<SystemPopupPanel>().SetSystemPopupMsg(title, guide, _closeCallback);
        PopupObject[(int)POPUP_TYPE.MESSAGE_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.MESSAGE_POPUP]);
    }

    public void OnSystemConfirmPopupShow(ConfirmPopupData confirmData, CONFIRM_POPUP_TYPE confirmType = CONFIRM_POPUP_TYPE.COMMON, SystemConfirmPanel.Callback _okCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.SYSTEM_CONFIRM_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.SYSTEM_CONFIRM_POPUP].GetComponent<SystemConfirmPanel>().SetSystemConfirmPopup(confirmData, confirmType, _okCallback);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.SYSTEM_CONFIRM_POPUP]);
    }
    public void OnSystemCountConfirmPopupShow(ConfirmPopupData confirmData, string countValue, CONFIRM_POPUP_TYPE confirmType = CONFIRM_POPUP_TYPE.COMMON, SystemConfirmPanel.CountCallback _okCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.SYSTEM_CONFIRM_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.SYSTEM_CONFIRM_POPUP].GetComponent<SystemConfirmPanel>().SetSystemCountConfirmPopup(confirmData, countValue, confirmType, _okCallback);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.SYSTEM_CONFIRM_POPUP]);
    }

    public void OnToastPopupShow(string message, float time = 2.0f)
    {
        ShowPopupCansvasNoBlur();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.TOAST_POPUP].GetComponent<ToastPopup>().SetToastPopup(message, time);
        PopupObject[(int)POPUP_TYPE.TOAST_POPUP].SetActive(true);
    }

    public void OnImageToastPopupShow(string titleMsg, string contentsMsg, RewardData reward, ImageToastConfirmPopup.Callback closeCallback = null)
    {
        ShowPopupCanvas();
        OnTopUIInfoLayer();

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.IMAGE_TOAST_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.IMAGE_TOAST_POPUP].GetComponent<ImageToastConfirmPopup>().SetImageToastPopup(titleMsg, contentsMsg, reward, closeCallback);
    }

    public void OnSuccessEffectPopupShow(EFFECT_TYPE effectType, uint ID, uint count = 0, SuccessEffectPanel.Callback _closeCallback = null)
    {
        ShowPopupCansvasNoBlur();

        PopupObject[(int)POPUP_TYPE.SUCCESS_EFFECT_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.SUCCESS_EFFECT_POPUP].GetComponent<SuccessEffectPanel>().InitSuccessEffect(effectType, ID, count, _closeCallback);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.SUCCESS_EFFECT_POPUP]);
    }

    public void OnShowGetCatPhotoPopup(neco_cat cat, neco_cat_memory memory, bool isNew, CatPhotoPopup.Callback cb = null)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.CAT_PHOTO_GET_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.CAT_PHOTO_GET_POPUP].GetComponent<CatPhotoPopup>().OnShowPhotoPoup(cat, memory, isNew, cb);
    }

    public void OnShowGetCatPhotoBoxPopup(neco_cat cat, neco_cat_memory memory, CatPhotoPopup.Callback cb = null)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.CAT_PHOTO_GET_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.CAT_PHOTO_GET_POPUP].GetComponent<CatPhotoPopup>().OnShowPhotoBoxPoup(cat, memory, cb);
    }
    

    public void OnShowCatPhotoViewPopup(neco_cat_memory memory)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.CAT_PHOTO_VIEWER_POPUP].GetComponent<NecoCatPhotoViewerPopup>().OnShowCatPhoto(memory);
        PopupObject[(int)POPUP_TYPE.CAT_PHOTO_VIEWER_POPUP].SetActive(true);
    }

    public void OnShowCatTouchPopup(neco_cat.CAT_SUDDEN_STATE type, neco_cat cat, CatTouchEffectPopup.Callback cb)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.TOUCH_CAT_EFFECT_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.TOUCH_CAT_EFFECT_POPUP].GetComponent<CatTouchEffectPopup>().OnShowCatTouchEffect(type, cat, cb);
    }

    public void ShowPhotoResult(user_card card)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.CARD_LIST_POPUP].GetComponent<CardListPopup>().OnCardItemSelect(card);
        PopupObject[(int)POPUP_TYPE.CARD_LIST_POPUP].SetActive(true);
    }

    public void ShowBanner(string title, string desc, NecoBannerPopup.BANNER_TYPE type, UnityEngine.Events.UnityAction banner_cb = null, UnityEngine.Events.UnityAction close_cb = null)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.BANNER_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.BANNER_POPUP].GetComponent<NecoBannerPopup>().SetBannerInfo(title,desc,type,banner_cb,close_cb);
        
        openPopupList.Add(PopupObject[(int)POPUP_TYPE.BANNER_POPUP]);
    }

    public void ShowRandomBoxInfo(uint boxID)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.RANDOMBOX_INFO].SetActive(true);
        PopupObject[(int)POPUP_TYPE.RANDOMBOX_INFO].GetComponent<NecoRandomBoxInfoPanel>().SetBoxInfoData(boxID);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.RANDOMBOX_INFO]);
    }

    public void ShowIDChecker(string accountNo, string name, string msg)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.ID_CHECKER].SetActive(true);
        PopupObject[(int)POPUP_TYPE.ID_CHECKER].GetComponent<IDCheckPopup>().Init(accountNo, name, msg);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.ID_CHECKER]);
    }

    public void ShowProfilePopup()
    {
        ShowPopupCanvas();

        PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.MYTITLE_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.MYTITLE_POPUP]);
    }

    public void ShowRepairPopup(neco_spot spot)
    {
        ShowPopupCanvas();

        PopupObject[(int)POPUP_TYPE.REPAIR_POPUP].SetActive(true);
        PopupObject[(int)POPUP_TYPE.REPAIR_POPUP].GetComponent<RepairPopup>().SetSpotData(spot);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.REPAIR_POPUP]);
    }

    //public void OnNewCatAlarm()
    //{
    //    //ShowPopupCansvasNoBlur();
    //    PopupCamera.gameObject.SetActive(true);

    //    PopupObject[(int)POPUP_TYPE.NEW_CAT_ALARM].GetComponent<NecoNewCatIconAlarmPopup>().RefreshNewCatAlarm();
    //    PopupObject[(int)POPUP_TYPE.NEW_CAT_ALARM].SetActive(true);
    //}

    public void OnPopupClose(POPUP_TYPE type = POPUP_TYPE.NECO_POPUP_ALL)
    {
        if (type == POPUP_TYPE.NECO_POPUP_ALL)
        {
            GetGameCanvas()?.RefreshSpots();

            foreach (GameObject popup in PopupObject)
            {
                popup.SetActive(false);
            }

            NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.MAIN_UI);
            NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

            ClosePopupCanvas();

            openPopupList?.Clear();
        }
        else
        {
            PopupObject[(int)type].SetActive(false);

            if (openPopupList.Contains(PopupObject[(int)type]))
            {
                openPopupList.Remove(PopupObject[(int)type]);
            }

            foreach (GameObject popup in PopupObject)
            {
                if (popup.activeSelf)
                    return;
            }

            if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.배틀패스보상받기)
            {
                NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.MAIN_UI);
            }
            else
            {
                if(neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.배고픈길막이가이드퀘스트)
                    NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.MAIN_UI);
                NecoCanvas.GetUICanvas().ResumeTutorialUI();
            }

            NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

            //GetGameCanvas()?.RefreshSpots();
            ClosePopupCanvas();
        }
    }

    public void RefreshTopUILayer(TOP_UI_PANEL_TYPE refreshType = TOP_UI_PANEL_TYPE.ALL, bool both = true)
    {
        topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().RefreshPanelData(refreshType);
        if(both)
            GetUICanvas().RefreshTopUILayer(refreshType, false);        
    }
    public bool OnCatVisitCoin(uint catid, uint coin)
    {
        if (topUIInfoLayer.activeInHierarchy)
        {
            topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().OnCatVisitCoin(catid, coin);
        }
        else
        {
            return false;
        }

        return true;
    }

    public uint GetUserResourceFromTopUILayer(TOP_UI_RESOURCE_TYPE resourceType)
    {
        uint userResource = 0;
        if (topUIInfoLayer != null && topUIInfoLayer.activeSelf)
        {
            userResource =  topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().GetUserResource(resourceType);
        }

        return userResource;
    }

    public void RefreshPopupData(POPUP_REFRESH_TYPE refreshType = POPUP_REFRESH_TYPE.NONE)
    {
        // 팝업의 활성 상태를 기반으로 관련 UI를 갱신 
        switch (refreshType)
        {
            case POPUP_REFRESH_TYPE.CAT_FOOD:
                //관련 갱신 팝업 목록
                //CAT_FEED_POPUP
                //CAT_COOK_LIST_POPUP
                //CAT_COOK_POPUP

                if (PopupObject[(int)POPUP_TYPE.CAT_FEED_POPUP].activeSelf)
                {
                    PopupObject[(int)POPUP_TYPE.CAT_FEED_POPUP].GetComponent<NecoFeedListPanel>().RefreshFeedData();
                }
                else if (PopupObject[(int)POPUP_TYPE.CAT_COOK_LIST_POPUP].activeSelf)
                {
                    // 조리 진행 팝업이 떠있는 경우는 조리진행 팝업의 Refresh만 실행
                    if (PopupObject[(int)POPUP_TYPE.CAT_COOK_POPUP].activeSelf)
                    {
                        PopupObject[(int)POPUP_TYPE.CAT_COOK_POPUP].GetComponent<NecoCookPanel>().Refresh();
                    }
                    else
                    {
                        PopupObject[(int)POPUP_TYPE.CAT_COOK_LIST_POPUP].GetComponent<NecoCookListPanel>().Refresh();
                    }
                }

                break;
            case POPUP_REFRESH_TYPE.CAT_CRAFT:
                //관련 갱신 팝업 목록
                //CAT_MAKING_LIST_POPUP
                //CAT_MAKING_POPUP

                if (PopupObject[(int)POPUP_TYPE.CAT_MAKING_LIST_POPUP].activeSelf)
                {
                    // 제작 진행 팝업이 떠있는 경우는 제작 진행 팝업의 Refresh만 실행
                    if (PopupObject[(int)POPUP_TYPE.CAT_MAKING_POPUP].activeSelf)
                    {
                        PopupObject[(int)POPUP_TYPE.CAT_MAKING_POPUP].GetComponent<NecoMakingPanel>().Refresh();
                    }
                    else
                    {
                        PopupObject[(int)POPUP_TYPE.CAT_MAKING_LIST_POPUP].GetComponent<NecoCraftingListPanel>().Refresh();
                    }
                }

                break;
            case POPUP_REFRESH_TYPE.SHOP_CARD:
                //관련 갱신 팝업 목록
                //SHOP_LIST_POPUP
                if (PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP].activeSelf)
                {
                    PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP].GetComponent<NecoShopPanel>().RefreshLayer();
                }

                break;
        }

        // 레벨업 UI의 경우 별도 체크
        if (PopupObject[(int)POPUP_TYPE.LEVELUP_POPUP].activeSelf)
        {
            PopupObject[(int)POPUP_TYPE.LEVELUP_POPUP].GetComponent<NecoLevelupUIPanel>().Refresh();
        }
    }

    public void UpdatePrologueSetting(POPUP_TYPE type)
    {
        switch (type)
        {
            case POPUP_TYPE.CAT_FISH_FARM_POPUP:
                PopupObject[(int)POPUP_TYPE.CAT_FISH_FARM_POPUP].GetComponent<NecoFishFarmPanel>().PrologueGoldEffect();
                break;
            case POPUP_TYPE.CAT_FISH_TRAP_POPUP:
                PopupObject[(int)POPUP_TYPE.CAT_FISH_TRAP_POPUP].GetComponent<NecoFishTrapPanel>().PrologueGetFishEffect();
                break;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        PopupClear();
    }

    public void BlurActionForMapChange()
    {
        if (coroutineBlurAction != null)
            StopCoroutine(coroutineBlurAction);
        coroutineBlurAction = null;
        
        StartCoroutine(OnMainCamBlurForMapChange());
    }

    private IEnumerator OnMainCamBlurForMapChange()
    {
        PopupCamera.gameObject.SetActive(true);
        TouchGuard.gameObject.SetActive(true);

        GetUICanvas().GetComponent<Canvas>().worldCamera = PopupCamera;
        GetUICanvas().gameObject.layer = 5;

        MainCamBlur.enabled = true;

        float timeRatio = 1.0f / 0.35f;

        while (MainCamBlur.interpolation < InterpolationMax)
        {
            MainCamBlur.interpolation += Time.deltaTime * timeRatio;

            yield return new WaitForEndOfFrame();
        }

        MainCamBlur.interpolation = InterpolationMax;
        
        timeRatio = 1.0f / 0.15f;

        while (MainCamBlur.interpolation > InterpolationMin)
        {
            MainCamBlur.interpolation -= Time.deltaTime * timeRatio;

            yield return new WaitForEndOfFrame();
        }

        MainCamBlur.interpolation = InterpolationMin;

        GetUICanvas().GetComponent<Canvas>().worldCamera = Camera.main;
        GetUICanvas().gameObject.layer = 0;

        PopupClear();

        if (neco_data.PrologueSeq.스와이프가이드 == neco_data.GetPrologueSeq())
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
                mapController.SendMessage("삼색이등장", SendMessageOptions.DontRequireReceiver);
        }
    }

    [ContextMenu("ShowPopupCanvas")]
    private void ShowPopupCanvas()
    {
        if (coroutineBlurAction != null)
            StopCoroutine(coroutineBlurAction);

        coroutineBlurAction = StartCoroutine(OnMainCamBlur(1.0f));

        if (halloweenDecoration != null)
            halloweenDecoration.OnDecoration();
    }

    private void ShowPopupCansvasNoBlur()
    {
        if (coroutineBlurAction != null)
            StopCoroutine(coroutineBlurAction);

        PopupCamera.gameObject.SetActive(true);
        TouchGuard.gameObject.SetActive(true);

        if (halloweenDecoration != null)
            halloweenDecoration.OnDecoration();
    }

    [ContextMenu("ClosePopupCanvas")]
    private void ClosePopupCanvas()
    {   
        if (CheckNewLimitProduct())
        {
            return;
        }

        if (coroutineBlurAction != null)
            StopCoroutine(coroutineBlurAction);

        coroutineBlurAction = StartCoroutine(OnMainCamNormal(0.5f));

        //if (topUIInfoLayer != null && topUIInfoLayer.activeSelf == true) 
        //{
        //    topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().OnHide();
        //}
    }

    private IEnumerator OnMainCamBlur(float time)
    {
        PopupCamera.gameObject.SetActive(true);
        TouchGuard.gameObject.SetActive(true);
        MainCamBlur.enabled = true;

        float timeRatio = 1.0f / time;

        while (MainCamBlur.interpolation < InterpolationMax)
        {
            MainCamBlur.interpolation += Time.deltaTime * timeRatio;
            
            yield return new WaitForEndOfFrame();
        }

        MainCamBlur.interpolation = InterpolationMax;
    }

    private IEnumerator OnMainCamNormal(float time)
    {
        float timeRatio = 1.0f / time;

        //while (MainCamBlur.interpolation < 1.0f)
        //{
        //    MainCamBlur.interpolation += Time.deltaTime * timeRatio;

        //    yield return new WaitForEndOfFrame();
        //}

        while (MainCamBlur.interpolation > InterpolationMin)
        {
            MainCamBlur.interpolation -= Time.deltaTime * timeRatio;

            yield return new WaitForEndOfFrame();
        }

        MainCamBlur.interpolation = InterpolationMin;

        PopupClear();        
    }

    public void OnTopUIInfoLayer(params TOP_UI_PANEL_TYPE[] offUIList)
    {
        if (topUIInfoLayer == null) { return; }

        topUIInfoLayer.SetActive(true);

        // ui setting
        topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().TogglePanelState(TOP_UI_PANEL_TYPE.BOOSTER, false);

        // 기본적으로 가이드 UI는 기존 ui canvas의 상태를 따르도록 설정
        NecoTopUIInfoPanel uiCanvasTopUIInfoPanel = GetUICanvas()?.UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI]?.GetComponent<NecoTopUIInfoPanel>();
        if (uiCanvasTopUIInfoPanel != null)
        {
            bool guideState = uiCanvasTopUIInfoPanel.IsUIOpen(TOP_UI_PANEL_TYPE.GUIDE_QUEST);
            topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().TogglePanelState(TOP_UI_PANEL_TYPE.GUIDE_QUEST, guideState);
        }

        // 현재 상점이나 배틀패스, 앨범, 추석이벤트 팝업이 활성화 중인 경우
        if (IsPopupOpen(POPUP_TYPE.SEASON_PASS_POPUP) || IsPopupOpen(POPUP_TYPE.SHOP_LIST_POPUP) || IsPopupOpen(POPUP_TYPE.CARD_LIST_POPUP) || IsPopupOpen(POPUP_TYPE.CHUSEOK_EVENT))
        {
            topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().TogglePanelState(TOP_UI_PANEL_TYPE.GUIDE_QUEST, false);
        } 

        // 추가로 off 시킬 UI PANEL이 있는 경우
        foreach (TOP_UI_PANEL_TYPE type in offUIList)
        {
            topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().TogglePanelState(type, false);
        }

        //if (topUIInfoLayer != null)
        //{
        //    topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().OnShow();
        //}
    }

    public void OffTopUIInfoLayer()
    {
        if (topUIInfoLayer == null || topUIInfoLayer.activeSelf == false) { return; }

        topUIInfoLayer.SetActive(false);

        // ui reset
        topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().TogglePanelState();
    }

    private void PopupClear()
    {
        foreach (GameObject popup in PopupObject)
        {
            popup.SetActive(false);
        }

        PopupCamera.gameObject.SetActive(false);
        TouchGuard.gameObject.SetActive(false);
        OffTopUIInfoLayer();
        MainCamBlur.enabled = false;
    }

    [ContextMenu("TEST_PHOTO_ANI_POPUP")]
    public void TEST_PHOTO_ANI_POPUP()
    {
        OnShowGetCatPhotoPopup(neco_cat.GetNecoCat(1), neco_cat_memory.GetNecoMemory(102001), true);
    }

    public bool IsPopupOpen(POPUP_TYPE type = POPUP_TYPE.NECO_POPUP_ALL)
    {
        if (type == POPUP_TYPE.NECO_POPUP_ALL)
        {
            foreach (GameObject popup in PopupObject)
            {
                if (popup.activeSelf)
                    return true;
            }

            return false;
        }

        return PopupObject[(int)type].activeSelf;
    }

    //[ContextMenu("COPY OBJECT")]
    //public void COPYOBJ()
    //{
    //    ScaleContorlObjects = new GameObject[PopupObject.Length];

    //    int index = 0;
    //    foreach (GameObject popup in PopupObject)
    //    {
    //        ScaleContorlObjects[index++] = popup;
    //    }
    //}

    public void ResetBackgroundSize(Vector2 size)
    {
        (PopupGroup.transform as RectTransform).sizeDelta = size;

        size = (transform as RectTransform).sizeDelta;

        float ratio = (size.x / size.y);
        float scale = Mathf.Min(1.0f, ratio * (20.0f / 9.0f));

        foreach (GameObject obj in ScaleContorlObjects)
        {
            if (obj != null)
            {
                obj.transform.localScale = Vector3.one * scale;                
            }
        }
    }

    public void OnCheckLimitProduct(uint id = 0)
    {
        bNeedCheckProduct = true;
        if(id > 0)
        {
            switch (id)
            {
                case 34:
                    BannerList.Add(34);
                    break;
                case 36:
                    BannerList.Add(36);
                    break;
                case 37:
                    BannerList.Add(37);
                    break;
                case 42:
                    BannerList.Add(42);
                    break;
                case 43:
                    BannerList.Add(43);
                    break;
                case 45:
                    BannerList.Add(45);
                    break;
                case 24:
                default:
                    return;
            }
        }
    }

    private void Update()
    {
        // Android Back Button
        #if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 아무 팝업도 없을 시 어플리케이션 종료
            if (openPopupList != null && openPopupList.Count <= 0)
            {
                ConfirmPopupData popupData = new ConfirmPopupData();

                popupData.titleText = LocalizeData.GetText("LOCALIZE_405");
                popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_405");

                popupData.messageText_1 = LocalizeData.GetText("game_exit");

                OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => {
                    Application.Quit();
                });

                return;
            }

            // 튜토리얼중에는 백버튼 허용 안함
            if (neco_data.GetPrologueSeq() < neco_data.PrologueSeq.배틀패스보상받기)
            {
                // 튜토리얼 중에는 뒤로가기 불가능
                // 추후 필요하다면 토스트팝업 추가
                return;
            }

            if (openPopupList != null && openPopupList.Count > 0)
            {
                GameObject popup = openPopupList.Last();

                // 고양이 상세보기 팝업은 동작이 다르므로 예외처리
                if (popup == PopupObject[(int)POPUP_TYPE.CAT_DETAIL_POPUP])
                {
                    OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_DETAIL_POPUP);

                    int catListIndex = openPopupList.FindIndex(x => x == PopupObject[(int)POPUP_TYPE.CAT_LIST_POPUP]);
                    if (catListIndex >= 0)
                    {
                        openPopupList.RemoveAt(catListIndex);
                    }

                    openPopupList.Remove(popup);

                    NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_LIST_POPUP);

                    return;
                }

                if (popup == PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP])
                {
                    if(PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP].GetComponent<NecoShopPanel>().CurShopCategory() == NecoShopPanel.SHOP_CATEGORY.POINT)
                        return;

                    if (PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP].GetComponent<NecoShopPanel>().CurShopCategory() == NecoShopPanel.SHOP_CATEGORY.CARD)
                    {
                        if (PopupObject[(int)POPUP_TYPE.SHOP_LIST_POPUP].GetComponent<NecoShopPanel>().cardShopPanel.ResultPanel.activeSelf)
                            return;
                    }
                }

                popup.SetActive(false);
                openPopupList.Remove(popup);

                if (openPopupList.Count > 0)
                {
                    return;
                }

                GetGameCanvas()?.RefreshSpots();

                NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.MAIN_UI);
                NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

                ClosePopupCanvas();
            }
        }
        #endif
    }

    bool CheckNewLimitProduct()
    {
        if (bNeedCheckProduct)
        {
            bNeedCheckProduct = false;

            WWWForm data = new WWWForm();
            data.AddField("api", "iap");
            data.AddField("op", 5);

            NetworkManager.GetInstance().SendApiRequest("iap", 5, data, (res) =>
            {
                JObject root = JObject.Parse(res);
                JToken apiToken = root["api"];
                if (null == apiToken || apiToken.Type != JTokenType.Array
                    || !apiToken.HasValues)
                {
                    return;
                }

                JArray apiArr = (JArray)apiToken;
                foreach (JObject row in apiArr)
                {
                    string uri = row.GetValue("uri").ToString();
                    if (uri == "iap")
                    {
                        JToken resultCode = row["rs"];
                        if (resultCode != null && resultCode.Type == JTokenType.Integer)
                        {
                            int rs = resultCode.Value<int>();
                            if (rs == 0)
                            {
                                if (row.ContainsKey("limit"))
                                {
                                    JObject limit = (JObject)row["limit"];
                                    neco_data.Instance.SetTimeSale(limit);
                                }

                                neco_data.Instance.SetBenefit(false);
                                if (row.ContainsKey("first"))
                                {
                                    neco_data.Instance.SetBenefit(row["first"].Value<uint>() > 0);
                                }

                                GetUICanvas().UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().CheckTimeSaleIcon();
                            }
                        }
                    }
                }
            });
        }

        bool ret = BannerList.Count > 0;
        if (ret)
        {
            PopupObject[(int)POPUP_TYPE.BANNER_POPUP].GetComponent<NecoBannerPopup>().SetBannerInfo(new List<uint>(BannerList));
            BannerList.Clear();
        }


        return ret;
    }

    public void OnServerWait()
    {
        //OnPopupShow(POPUP_TYPE.SERVER_WAIT_POPUP);

        ShowPopupCanvas();
        if (neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.프리플레이)
        {
            OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);
        }
        else
        {
            OnTopUIInfoLayer();
        }

        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.MAIN_UI);
        GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

        PopupObject[(int)POPUP_TYPE.SERVER_WAIT_POPUP].SetActive(true);

        openPopupList.Add(PopupObject[(int)POPUP_TYPE.SERVER_WAIT_POPUP]);


        GameObject target = PopupObject[(int)POPUP_TYPE.SERVER_WAIT_POPUP];
        GameObject button = target.transform.Find("Button").gameObject;
        button.gameObject.SetActive(false);
    }

    public void OnServerRetry(string uri,
        WWWForm param,
        NetworkManager.SuccessCallback onSuccess,
        NetworkManager.FailCallback onFail = null)
    {
        GameObject target = PopupObject[(int)POPUP_TYPE.SERVER_WAIT_POPUP];
        GameObject button = target.transform.Find("Button").gameObject;
        button.gameObject.SetActive(true);
        Button btn = button.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            StartCoroutine(NetworkManager.GetInstance().SendPostCorutine(uri, param, onSuccess, onFail));

            GameObject po = PopupObject[(int)POPUP_TYPE.SERVER_WAIT_POPUP];
            GameObject b = po.transform.Find("Button").gameObject;
            b.gameObject.SetActive(false);
        });
    }
}
