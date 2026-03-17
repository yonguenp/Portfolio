using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SUPPLY_UI_TYPE
{
    UNKNOWN,
    FISH_FARM = 101,    // 양어장
    FISH_TRAP = 102,    // 통발
    CAT_GIFT = 103,     // 보은 바구니
    WORKBENCH = 104,    // 작업대  
    COUNTERTOP = 105,   // 조리대
    OBJECT = 106,       // 설치물
}

public class NecoSupplyUIPanel : NecoAnimatePopup
{
    [Header("[Common]")]
    public Text titleLevelText;

    [Header("[Supply Info List]")]
    public GameObject supplyListContainer;
    public GameObject supplyLayerCloneObject;
    
    public Text supplyCountText;
    public GameObject NoItem;

    [Header("[Slider Layers]")]
    public Slider supplyCountSlider;
    public Color originSliderColor;
    public Color maxSliderColor;

    [Header("[Button Layers]")]
    public GameObject levelupButton;
    public GameObject receiveButton;
    public GameObject closeButton;
    public Color levelupButtonColor;
    public Color originReceiveButtonColor;
    public Color DimmedReceiveButtonColor;

    [Header("[CatGiftBooster Layer]")]
    public GameObject AdBoostLayer;
    public GameObject AdBoosterButton;
    public GameObject activeAdBoosterButton;
    public Text AdBoostTimeText;

    public GameObject buyBoostLayer;
    public GameObject buyBoosterButton;
    public GameObject activeBuyBoosterButton;
    public Text buyBoostTimeText;

    public Text boostPriceText;

    [Header("[CatGiftBooster Layer + Ver2]")]
    public GameObject activatedBoosterLayer;
    public GameObject boosterBubbleLayer;
    public Slider boosterSlider;
    public Text sliderRemainTime;
    public Text boostGuideText;

    uint maxBoostTime = 0;

    Coroutine coroutineCatgiftBoost = null;

    neco_level necoLevelData;
    Dictionary<string, RewardData> rewardHistoryDic = new Dictionary<string, RewardData>();

    SUPPLY_UI_TYPE curUIType;

    public override void OnAnimateDone()
    {

    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public void OnClickSupplyListOpenButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_SUPPLY_UI_POPUP);
    }

    public void OnClickCloseButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_300"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_SUPPLY_UI_POPUP);
    }

    public void OnClickGetSupplyButton()
    {
        if (rewardHistoryDic == null || rewardHistoryDic.Count <= 0) 
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_301"));
            return; 
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_SUPPLY_UI_POPUP);

        GetAllSupplyList();
    }

    public void OnClickLevelupUIOpenButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_300"));
            return;
        }

        if (necoLevelData == null) { return; }

        if (necoLevelData.GetNecoLevel() < neco_data.GIFT_MAX_LEVEL)
        {
            NecoCanvas.GetPopupCanvas().OnLevelupPopupShow(curUIType, RefreshCatGiftData);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_302"));
        }
    }

    public void OnClickPurchaseBoosterButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_300"));
            return;
        }

        if (neco_data.Instance.GetGiftBoostTime() > NecoCanvas.GetCurTime())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_240"), LocalizeData.GetText("LOCALIZE_241"));
            return;
        }

        // 보은 바구니 부스터 관련 처리
        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, PurchaseBoostItem);
    }

    public void OnClickAdvertiseBoostButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_300"));
            return;
        }

        if (neco_data.Instance.GetGiftBoostTime() > NecoCanvas.GetCurTime())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_240"), LocalizeData.GetText("LOCALIZE_241"));
            return;
        }

#if UNITY_EDITOR
        NecoCanvas.GetPopupCanvas().OnToastPopupShow("유니티 에디터에서는 사용불가");
        return;
#endif
        AdvertiseManager.GetInstance().TryADWithCallback(OnAdvertiseBoostStart, OnAdvertiseBoostFail);
    }

    public void InitSupplyListData(List<neco_data.neco_gift_info> giftList, SUPPLY_UI_TYPE uiType)
    {
        Dictionary<string, RewardData> rewardDic = GetRewardData(giftList);
        if (rewardDic == null) { return; }

        PrologueSetting();

        curUIType = uiType;
        necoLevelData = neco_level.GetNecoLevelData("GIFT", neco_data.Instance.GetGiftBasketLevel());

        SetSupplyGiftListData(rewardDic);

        // 부스터 관련 처리
        RefreshBoosterData();
        if (neco_data.Instance.GetGiftBoostTime() > NecoCanvas.GetCurTime())
        {
            RefreshCatGiftBoostData();
        }

        supplyListContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }

    void SetSupplyGiftListData(Dictionary<string, RewardData> rewardDic)
    {
        if (rewardDic == null) { return; }
        if (necoLevelData == null) { return; }
        if (supplyListContainer == null || supplyLayerCloneObject == null) { return; }

        foreach (Transform child in supplyListContainer.transform)
        {
            if (child.gameObject != supplyLayerCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        supplyLayerCloneObject.SetActive(true);

        rewardHistoryDic.Clear();

        rewardHistoryDic = new Dictionary<string, RewardData>(rewardDic);
        List<items> giftItemList = neco_gift.GetNecoGiftItemList();

        uint rewardCount = 0;
        //foreach (items giftItem in giftItemList)
        //{
        //    string strItemID = giftItem.GetItemID().ToString();

        //    GameObject rewardInfoUI = Instantiate(supplyLayerCloneObject);
        //    rewardInfoUI.transform.SetParent(supplyListContainer.transform);
        //    rewardInfoUI.transform.localScale = supplyLayerCloneObject.transform.localScale;
        //    rewardInfoUI.transform.localPosition = supplyLayerCloneObject.transform.localPosition;

        //    // 기본 아이템 리스트 생성
        //    RewardData reward = new RewardData();
        //    reward.itemData = giftItem;
        //    rewardInfoUI.GetComponent<SupplyGetInfo>().InitSupplyItemInfo(reward);

        //    // 보상 리스트에 있다면 데이터 갱신
        //    if (rewardHistoryDic.TryGetValue(strItemID, out RewardData dicValue))
        //    {
        //        if (dicValue != null)
        //        {
        //            rewardInfoUI.GetComponent<SupplyGetInfo>().InitSupplyItemInfo(dicValue);

        //            rewardCount += dicValue.count;
        //        }
        //    }
        //}

        foreach (KeyValuePair<string, RewardData> rewardData in rewardHistoryDic)
        {
            string strItemID = rewardData.Key;

            GameObject rewardInfoUI = Instantiate(supplyLayerCloneObject);
            rewardInfoUI.transform.SetParent(supplyListContainer.transform);
            rewardInfoUI.transform.localScale = supplyLayerCloneObject.transform.localScale;
            rewardInfoUI.transform.localPosition = supplyLayerCloneObject.transform.localPosition;

            rewardInfoUI.GetComponent<SupplyGetInfo>().InitSupplyItemInfo(rewardData.Value);

            rewardCount += rewardData.Value.count;
        }

        // 텍스트 레벨 관련 처리
        titleLevelText.text = string.Format("Lv {0}/{1}", necoLevelData.GetNecoLevel(), neco_data.GIFT_MAX_LEVEL);

        // 텍스트 관련 처리
        uint maxGiftCount = necoLevelData.GetNecoLevelValue2();

        supplyCountText.text = string.Format("({0}/{1})", rewardCount.ToString("n0"), maxGiftCount.ToString("n0"));

        supplyCountSlider.value = (float)rewardCount / maxGiftCount;
        supplyCountSlider.fillRect.GetComponent<Image>().color = rewardCount >= maxGiftCount ? maxSliderColor : originSliderColor;

        supplyLayerCloneObject.SetActive(false);

        // 아이템 없을 경우 처리
        NoItem.SetActive(rewardCount <= 0);

        // max 레벨 버튼 체크
        levelupButton.GetComponentInChildren<Image>().color = necoLevelData.GetNecoLevel() >= neco_data.GIFT_MAX_LEVEL ? DimmedReceiveButtonColor : levelupButtonColor;

        UpdateButtonColor();

        // 부스터 관련 처리
        boostPriceText.text = neco_booster.GetBoosterData(3).GetPrice().ToString();
    }

    public void GetAllSupplyList()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "object");
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("object", 2, data, (response) =>
        {
            JObject root = JObject.Parse(response);
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
                if (uri == "object")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            ShowGiftRewardPopup();
                            ClearData();
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_267"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_268"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow("서버 오류", msg);
                        }
                    }
                }
            }
        });
    }

    public void ShowGiftRewardPopup()
    {
        if (rewardHistoryDic != null && rewardHistoryDic.Count > 0)
        {
            NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), rewardHistoryDic, ()=> {
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (mapController != null)
                {
                    mapController.SendMessage("첫보은받기성공", SendMessageOptions.DontRequireReceiver);
                }
            });
        }
    }

    public Dictionary<string, RewardData> GetRewardData(List<neco_data.neco_gift_info> history)
    {
        Dictionary<string, RewardData> rewardDic = new Dictionary<string, RewardData>();
        if (history != null && history.Count > 0)
        {
            // 보상 데이터로 재가공
            //rewardDic.Add("gold", new RewardData());
            foreach (neco_data.neco_gift_info gift in history)
            {
                string strItemID = gift.item.GetItemID().ToString();

                RewardData reward = new RewardData();
                reward.itemData = gift.item;
                reward.count = gift.count;
                // 현재 보은에서는 골드를 주지 않으므로 주석처리 - 2021.4.21
                //reward.gold = info.gold;

                if (rewardDic.ContainsKey(strItemID))
                {
                    rewardDic[strItemID].count += gift.count;
                }
                else
                {
                    rewardDic.Add(strItemID, reward);
                }

                // 현재 보은에서는 골드를 주지 않으므로 주석처리 - 2021.4.21
                //rewardDic["gold"].gold += gift.gold;
            }
        }

        return rewardDic;
    }

    void RefreshCatGiftData()
    {
        InitSupplyListData(neco_data.Instance.GetGiftList(), SUPPLY_UI_TYPE.CAT_GIFT);
    }

    void UpdateButtonColor()
    {
        if (rewardHistoryDic != null && rewardHistoryDic.Count > 0)
        {
            receiveButton.GetComponentInChildren<Image>().color = originReceiveButtonColor;
        }
        else
        {
            receiveButton.GetComponentInChildren<Image>().color = DimmedReceiveButtonColor;
        }
    }

    #region 부스터 관련

    public void PurchaseBoostItem()
    {
        WWWForm param = new WWWForm();
        param.AddField("api", "plant");
        param.AddField("op", 4);
        param.AddField("id", 103);

        NetworkManager.GetInstance().SendApiRequest("plant", 4, param, (response) => {
            JObject root = JObject.Parse(response);
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
                if (uri == "plant")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshBoosterData", 0.1f);
                            Invoke("RefreshCatGiftBoostData", 0.1f);
                        }
                        else
                        {
                            if (rs == 4)
                            {
                                NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CATNIP_BUY_POPUP);
                            }
                            else
                            {
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_271"), LocalizeData.GetText("LOCALIZE_272"));
                            }
                        }
                    }
                }
            }
        });
    }

    public void OnAdvertiseBoostStart()
    {
        WWWForm param = new WWWForm();
        param.AddField("api", "plant");
        param.AddField("op", 4);
        param.AddField("id", 103);
        param.AddField("ad", 1);

        NetworkManager.GetInstance().SendApiRequest("plant", 4, param, (response) => {
            JObject root = JObject.Parse(response);
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
                if (uri == "plant")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshBoosterData", 0.1f);
                            Invoke("RefreshCatGiftBoostData", 0.1f);
                        }
                        else
                        {
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_271"), LocalizeData.GetText("LOCALIZE_272"));
                        }
                    }
                }
            }
        });
    }

    public void RefreshBoosterData()
    {
        NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        //activatedBoosterLayer.SetActive(false);
        //activeAdBoosterButton.SetActive(false);
        //activeBuyBoosterButton.SetActive(false);

        //Button AdButton = AdBoosterButton.GetComponent<Button>();
        //Button catnipButton = buyBoosterButton.GetComponent<Button>();
        //switch (neco_data.Instance.GetGiftBoostType())
        //{
        //    case neco_data.BOOST_TYPE.NONE:
        //        AdButton.interactable = true;
        //        catnipButton.interactable = true;
        //        break;
        //    case neco_data.BOOST_TYPE.CATNIP_BOOST:
        //        AdButton.interactable = false;
        //        catnipButton.interactable = false;
        //        activeBuyBoosterButton.SetActive(true);
        //        break;
        //    case neco_data.BOOST_TYPE.AD_BOOST:
        //        AdButton.interactable = false;
        //        catnipButton.interactable = false;
        //        activeAdBoosterButton.SetActive(true);
        //        break;

        //}

        //AdBoosterButton.GetComponent<Image>().color = AdButton.interactable ? originReceiveButtonColor : DimmedReceiveButtonColor;
        //buyBoosterButton.GetComponent<Image>().color = catnipButton.interactable ? originReceiveButtonColor : DimmedReceiveButtonColor;
        switch(neco_data.Instance.GetGiftBoostType())
        {
            case neco_data.BOOST_TYPE.AD_BOOST:
                boosterBubbleLayer.SetActive(false);
                boostGuideText.text = LocalizeData.GetText("AD_BOOST_GUIDE");
                break;
            case neco_data.BOOST_TYPE.CATNIP_BOOST:
                boosterBubbleLayer.SetActive(true);
                boostGuideText.text = LocalizeData.GetText("LOCALIZE_74");
                break;
            default:
                boosterBubbleLayer.SetActive(false);
                boostGuideText.text = "";
                break;
        }
        

        AdBoostLayer.SetActive(neco_data.Instance.GetGiftBoostTime() <= NecoCanvas.GetCurTime());
        AdBoosterButton.SetActive(neco_data.Instance.GetGiftBoostTime() <= NecoCanvas.GetCurTime());
        //activeAdBoosterButton.SetActive(neco_data.Instance.GetGiftBoostTime() > 0);

        buyBoostLayer.SetActive(neco_data.Instance.GetGiftBoostTime() <= NecoCanvas.GetCurTime());
        buyBoosterButton.SetActive(neco_data.Instance.GetGiftBoostTime() <= NecoCanvas.GetCurTime());
        //activeBuyBoosterButton.SetActive(neco_data.Instance.GetGiftBoostTime() > 0);

        activatedBoosterLayer.SetActive(neco_data.Instance.GetGiftBoostTime() > NecoCanvas.GetCurTime());
    }

    public void OnAdvertiseBoostFail()
    {
        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_273"));
    }

    void RefreshCatGiftBoostData()
    {
        // 부스트 타이머 처리
        if (gameObject.activeInHierarchy)
        {
            RefreshCatGiftBoostTimeData();
        }
    }

    void RefreshCatGiftBoostTimeData()
    {
        if (coroutineCatgiftBoost != null)
        {
            StopCoroutine(coroutineCatgiftBoost);
        }

        coroutineCatgiftBoost = StartCoroutine(RefreshCatGiftBoostTime());
    }

    IEnumerator RefreshCatGiftBoostTime()
    {
        // 부스트 기존 최대 시간 계산
        if (neco_data.Instance.GetGiftBoostType() == neco_data.BOOST_TYPE.CATNIP_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(3).GetEffectTime();
        }
        else if (neco_data.Instance.GetGiftBoostType() == neco_data.BOOST_TYPE.AD_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(4).GetEffectTime();
        }

        // 남은 부스트 시간 계산
        uint remain = neco_data.Instance.GetGiftBoostTime();

        while (remain > NecoCanvas.GetCurTime())
        {
            SetBoostRemainTime(remain - NecoCanvas.GetCurTime());
            yield return new WaitForSeconds(1.0f);
        }

        // 부스트 시간 만료시 처리
        SetBoostRemainTime(0);
        neco_data.Instance.SetGiftBoostTime(0, neco_data.BOOST_TYPE.NONE);

        AdBoostLayer.SetActive(true);
        AdBoosterButton.SetActive(true);
        //activeAdBoosterButton.SetActive(false);

        buyBoostLayer.SetActive(true);
        buyBoosterButton.SetActive(true);
        //activeBuyBoosterButton.SetActive(false);

        activatedBoosterLayer.SetActive(false);
    }

    void SetBoostRemainTime(uint remainTime)
    {
        uint minute = remainTime / 60;
        uint second = remainTime % 60;

        buyBoostTimeText.text = string.Format("{0:D2}:{1:D2}", minute, second);
        //AdBoostTimeText.text = string.Format("{0:D2}:{1:D2}", minute, second);

        boosterSlider.value = (float)remainTime / maxBoostTime;
        sliderRemainTime.text = string.Format("{0:D2}:{1:D2}", minute, second);
    }

    public ConfirmPopupData SetConfirmPopupData()
    {
        neco_booster boosterData = neco_booster.GetBoosterData(3);

        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_235");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_236");
        popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_237");

        string boostTimeText = string.Format(LocalizeData.GetText("booster_01"), boosterData.GetEffectTime() / 60);
        string boostEffectText = string.Format(LocalizeData.GetText("booster_02"), boosterData.GetEffect() - 100);
        popupData.messageText_2 = string.Format("{0}\n{1}", boostTimeText, boostEffectText);
        
        switch (boosterData.GetPriceType())
        {
            case "dia":
                popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
                break;
            case "gold":
                popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                break;
            case "point":
                popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");
                break;
        }

        popupData.amountText = boosterData.GetPrice().ToString("n0"); // todo bt - 추후 데이터 연동 필요

        return popupData;
    }

    #endregion

    void ClearData()
    {
        rewardHistoryDic.Clear();
        necoLevelData = null;
    }

    void PrologueSetting()
    {
        levelupButton.SetActive(true);
        receiveButton.SetActive(true);
        closeButton.SetActive(true);

        if (CheckPrologue())
        {
            //levelupButton.SetActive(false);
            receiveButton.SetActive(true);
            closeButton.SetActive(false);

            // 골드 수급 강조 연출 적용
            receiveButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.보은바구니UI등장;
    }

    private void OnDisable()
    {
        receiveButton.GetComponent<RectTransform>().localScale = Vector3.one;
        receiveButton.GetComponent<RectTransform>().DORewind();
        receiveButton.GetComponent<RectTransform>().DOKill();
    }
}
