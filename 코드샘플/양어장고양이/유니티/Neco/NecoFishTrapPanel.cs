using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoFishTrapPanel : NecoAnimatePopup
{
    [Header("[Fish Trap List]")]
    public GameObject fishtrapListContainer;
    public GameObject fishtrapLayerCloneObject;
    public GameObject NoItem;

    [Header("[Fish Text List]")]
    public Text fishtrapLevelTitleText;
    public Text fishtrapCountText;
    public Text fishtrapNextGetTimeText;
    public Color originFishCountTextColor;
    public Color fullFishCountTextColor;

    [Header("[Button Layers]")]
    public GameObject levelupButton;
    public GameObject RequestButton;
    public GameObject closeButton;
    public Color levelupButtonColor;
    public Color originRequestButtonColor;
    public Color DimmedRequestButtonColor;

    [Header("[FishTrapBooster Layer]")]
    public GameObject AdBoostLayer;
    public GameObject AdBoosterButton;
    public GameObject activeAdBoosterButton;
    public Text AdBoostTimeText;

    public GameObject buyBoostLayer;
    public GameObject buyBoosterButton;
    public GameObject activeBuyBoosterButton;
    public Text buyBoostTimeText;

    public Text boostPriceText;

    [Header("[FishTrapBooster Layer + Ver2]")]
    public GameObject activatedBoosterLayer;
    public GameObject boosterBubbleLayer;
    public Slider boosterSlider;
    public Text sliderRemainTime;
    public Text boostGuideText;

    uint maxBoostTime = 0;

    Coroutine coroutineFishtrapBoost = null;

    neco_level necoLevelData;
    Dictionary<string, RewardData> rewardHistoryDic = new Dictionary<string, RewardData>();

    SUPPLY_UI_TYPE curUIType;

    Coroutine coroutineFishTimeCount = null;

    bool isFirstOpen = true;

    public override void OnAnimateDone()
    {
        
    }

    public void OnClickCloseButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_274"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_FISH_TRAP_POPUP);
    }

    public void OnClickGetFishtrapButton()
    {
        if (rewardHistoryDic == null || rewardHistoryDic.Count <= 0) 
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_275"));
            return; 
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_FISH_TRAP_POPUP);

        GetAllFishtrapList();
    }

    public void OnClickLevelupUIOpenButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_274"));
            return;
        }

        if (necoLevelData == null) { return; }

        if (necoLevelData.GetNecoLevel() < neco_data.FISH_TRAP_MAX_LEVEL)
        {
            NecoCanvas.GetPopupCanvas().OnLevelupPopupShow(curUIType, RefreshFishtrapReward);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_276"));
        }
    }

    public void OnClickPurchaseBoosterButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_274"));
            return;
        }

        if (neco_data.Instance.GetTrapBoostTime() > NecoCanvas.GetCurTime())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_240"), LocalizeData.GetText("LOCALIZE_241"));
            return;
        }

        // 양어장 부스터 관련 처리
        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, PurchaseBoostItem);
    }

    public void OnClickAdvertiseBoostButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_274"));
            return;
        }

        if (neco_data.Instance.GetTrapBoostTime() > NecoCanvas.GetCurTime())
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

    public void InitFishtrapListData(List<RewardData> rewardList, SUPPLY_UI_TYPE uiType)
    {
        Dictionary<string, RewardData> rewardDic = GetRewardData(rewardList);
        if (rewardDic == null) { return; }

        PrologueSetting();

        //StopAllCoroutines();

        curUIType = uiType;
        necoLevelData = neco_level.GetNecoLevelData("FISH", neco_data.Instance.GetFishtrapLevel());

        SetFishtrapListData(rewardDic);

        // 부스터 관련 처리
        RefreshBoosterData();
        if (neco_data.Instance.GetTrapBoostTime() > NecoCanvas.GetCurTime())
        {
            RefreshFishtrapData();
        }

        //MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        //if (mapController != null)
        //{
        //    //mapController.SendMessage("OnTrapUIOpened", SendMessageOptions.DontRequireReceiver);
        //}

        // 코루틴으로 인하여 OnEnable아닌 해당위치에서 호출
        // 갱신시에는 ui 애니메이션 동작하지 않도록 ui오픈 시 1회만 동작하도록 설정
        if (isFirstOpen)
        {
            base.OnEnable();
            isFirstOpen = false;
        }
    }

    void SetFishtrapListData(Dictionary<string, RewardData> rewardDic)
    {
        NoItem.SetActive(true);

        if (rewardDic == null) { return; }
        if (necoLevelData == null) { return; }
        if (fishtrapListContainer == null || fishtrapLayerCloneObject == null) { return; }

        foreach (Transform child in fishtrapListContainer.transform)
        {
            if (child.gameObject != fishtrapLayerCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        fishtrapLayerCloneObject.SetActive(true);

        rewardHistoryDic.Clear();

        rewardHistoryDic = new Dictionary<string, RewardData>(rewardDic);
        List<neco_fish_trap_rate> fishList = neco_fish_trap_rate.GetFishListByLevel(necoLevelData.GetNecoLevel());

        uint rewardCount = 0;
        bool hasItem = false;
        foreach (neco_fish_trap_rate fishData in fishList)
        {
            items itemData = items.GetItem(fishData.GetNecoFishID());

            // 보상 리스트에 있다면 데이터 갱신
            if (rewardHistoryDic.TryGetValue(itemData.GetItemID().ToString(), out RewardData dicValue))
            {
                if (dicValue != null)
                {
                    GameObject rewardInfoUI = Instantiate(fishtrapLayerCloneObject);
                    rewardInfoUI.transform.SetParent(fishtrapListContainer.transform);
                    rewardInfoUI.transform.localScale = fishtrapLayerCloneObject.transform.localScale;
                    rewardInfoUI.transform.localPosition = fishtrapLayerCloneObject.transform.localPosition;

                    // 기본 아이템 리스트 생성
                    RewardData reward = new RewardData();
                    reward.itemData = itemData;
                    rewardInfoUI.GetComponent<SupplyGetInfo>().InitSupplyItemInfo(reward);

                    rewardInfoUI.GetComponent<SupplyGetInfo>().InitSupplyItemInfo(dicValue);

                    rewardCount += dicValue.count;
                    hasItem = true;
                }
            }
        }

        NoItem.SetActive(!hasItem);

        // 프롤로그 중 체크
        if (CheckPrologue() == false)
        {
            RequestButton.GetComponentInChildren<Image>().color = hasItem ? originRequestButtonColor : DimmedRequestButtonColor;
        }

        fishtrapLayerCloneObject.SetActive(false);

        // 텍스트 레벨 관련 처리
        fishtrapLevelTitleText.text = string.Format("Lv {0}/{1}", necoLevelData.GetNecoLevel(), neco_data.FISH_TRAP_MAX_LEVEL);

        // 텍스트 관련 처리
        uint maxFishCount = necoLevelData.GetNecoLevelValue2();

        fishtrapCountText.color = originFishCountTextColor;
        fishtrapCountText.text = string.Format("({0}/{1})", rewardCount, maxFishCount);
        if (rewardCount >= maxFishCount)
        {
            fishtrapCountText.color = fullFishCountTextColor;
            fishtrapNextGetTimeText.text = LocalizeData.GetText("LOCALIZE_277");
        }
        else
        {
            // 다음 물고기 시간 텍스트 처리
            if (gameObject.activeSelf)
            {
                RefreshFishGetTimeData();
            }
        }

        // max 레벨 버튼 체크
        levelupButton.GetComponentInChildren<Image>().color = necoLevelData.GetNecoLevel() >= neco_data.FISH_TRAP_MAX_LEVEL ? DimmedRequestButtonColor : levelupButtonColor;

        // 부스터 관련 처리
        boostPriceText.text = neco_booster.GetBoosterData(2).GetPrice().ToString();
    }

    public void GetAllFishtrapList()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 1);
        data.AddField("id", (int)SUPPLY_UI_TYPE.FISH_TRAP);

        NetworkManager.GetInstance().SendApiRequest("plant", 1, data, (response) =>
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
                if (uri == "plant")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            ShowFishtrapRewardPopup();
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_267"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_268"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }          
        });
    }

    public void ShowFishtrapRewardPopup()
    {
        if (rewardHistoryDic != null && rewardHistoryDic.Count > 0)
        {
            NecoRewardPopup.Callback callback = () => {
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (mapController != null)
                {
                    if (neco_data.PrologueSeq.통발UI등장 == neco_data.GetPrologueSeq())
                        mapController.SendMessage("통발수급완료", SendMessageOptions.DontRequireReceiver);

                    if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.통발2레벨물고기10개획득)
                    {
                        int count = 0;
                        if (neco_data.Instance.GetFishtrapLevel() >= 2)
                        {                            
                            foreach (KeyValuePair<string, RewardData> data in rewardHistoryDic)
                            {
                                if (data.Value != null)
                                {
                                    items item = data.Value.itemData;
                                    if (item != null)
                                    {
                                        count += (int)data.Value.count;
                                    }
                                }
                            }
                        }

                        if (count > 0)
                        {
                            int guideQuestCount = PlayerPrefs.GetInt("GUIDE_QUEST_COUNT", 1);
                            guideQuestCount -= (count - 1);
                            if (guideQuestCount < 0)
                                guideQuestCount = 0;

                            PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", guideQuestCount);

                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발2레벨물고기10개획득, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }

                ClearData();
            };

            NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_279"), LocalizeData.GetText("LOCALIZE_280"), rewardHistoryDic, callback);
        }
        else
        {
            ClearData();
        }
    }

    public static Dictionary<string, RewardData> GetRewardData(List<RewardData> history)
    {
        Dictionary<string, RewardData> rewardDic = new Dictionary<string, RewardData>();
        if (history != null && history.Count > 0)
        {
            // 보상 데이터로 재가공
            //rewardDic.Add("gold", new RewardData());
            foreach (RewardData rewardData in history)
            {
                string strItemID = rewardData.itemData.GetItemID().ToString();

                RewardData reward = new RewardData();
                reward.itemData = rewardData.itemData;
                reward.count = rewardData.count;
                // 현재 통발에서는 골드를 주지 않으므로 주석처리 - 2021.4.29
                //reward.gold = info.gold;

                if (rewardDic.ContainsKey(strItemID))
                {
                    rewardDic[strItemID].count += rewardData.count;
                }
                else
                {
                    rewardDic.Add(strItemID, reward);
                }

                // 현재 통발에서는 골드를 주지 않으므로 주석처리 - 2021.4.29
                //rewardDic["gold"].gold += gift.gold;
            }
        }

        return rewardDic;
    }

    void RefreshFishGetTimeData()
    {
        if (coroutineFishTimeCount != null)
        {
            StopCoroutine(coroutineFishTimeCount);
        }

        coroutineFishTimeCount = StartCoroutine(RefreshFishTime());
    }

    IEnumerator RefreshFishTime()
    {
        while (neco_data.Instance.GetFishNextUpdateRemain() > 0)
        {
            SetRemainTime(neco_data.Instance.GetFishNextUpdateRemain());
            yield return new WaitForSeconds(1.0f);
        }

        Invoke("RefreshFishtrapReward", 1.0f);
    }

    void SetRemainTime(uint remainTime)
    {
        uint minute = remainTime / 60;
        uint second = remainTime % 60;

        if (minute < 1)
        {
            fishtrapNextGetTimeText.text = string.Format(LocalizeData.GetText("LOCALIZE_281"), second);
        }
        else
        {
            fishtrapNextGetTimeText.text = string.Format(LocalizeData.GetText("LOCALIZE_282"), minute, second);
        }

        //if (layoutObject != null)
        //{
        //    LayoutRebuilder.ForceRebuildLayoutImmediate(layoutObject);
        //}
    }

    void RefreshFishtrapReward()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 3);
        data.AddField("id", (int)SUPPLY_UI_TYPE.FISH_TRAP);

        NetworkManager.GetInstance().SendApiRequest("plant", 3, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            List<RewardData> rewards = new List<RewardData>();

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "plant")
                {
                    JToken opCode = row["op"];
                    if (opCode != null && opCode.Type == JTokenType.Integer)
                    {
                        int op = opCode.Value<int>();
                        switch (op)
                        {
                            case 3: //OpPlant::HARVEST  
                            {
                                JToken resultCode = row["rs"];
                                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                {
                                    int rs = resultCode.Value<int>();
                                    if (rs == 0)
                                    {
                                        if (row.ContainsKey("rew"))
                                        {
                                            if (row["rew"].HasValues)
                                            {
                                                JObject income = (JObject)row["rew"];
                                                if (income.ContainsKey("gold"))
                                                {
                                                    RewardData reward = new RewardData();
                                                    reward.gold = income["gold"].Value<uint>();
                                                    rewards.Add(reward);
                                                }


                                                if (income.ContainsKey("item"))
                                                {
                                                    JArray item = (JArray)income["item"];
                                                    foreach (JObject rw in item)
                                                    {
                                                        RewardData reward = new RewardData();
                                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                                        reward.count = rw["amount"].Value<uint>();
                                                        rewards.Add(reward);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string msg = rs.ToString();
                                        switch (rs)
                                        {
                                            case 1: msg = LocalizeData.GetText("LOCALIZE_267"); break;
                                            case 2: msg = LocalizeData.GetText("LOCALIZE_268"); break;
                                        }

                                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }

            InitFishtrapListData(rewards, SUPPLY_UI_TYPE.FISH_TRAP);
        });
    }

    #region 부스터 관련

    public void PurchaseBoostItem()
    {
        WWWForm param = new WWWForm();
        param.AddField("api", "plant");
        param.AddField("op", 4);
        param.AddField("id", 102);

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
                            Invoke("RefreshFishtrapData", 0.1f);
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
        param.AddField("id", 102);
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
                            Invoke("RefreshFishtrapData", 0.1f);
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
        //switch (neco_data.Instance.GetTrapBoostType())
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

        //AdBoosterButton.GetComponent<Image>().color = AdButton.interactable ? originRequestButtonColor : DimmedRequestButtonColor;
        //buyBoosterButton.GetComponent<Image>().color = catnipButton.interactable ? originRequestButtonColor : DimmedRequestButtonColor;

        switch (neco_data.Instance.GetTrapBoostType())
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

        AdBoostLayer.SetActive(neco_data.Instance.GetTrapBoostTime() <= NecoCanvas.GetCurTime());
        AdBoosterButton.SetActive(neco_data.Instance.GetTrapBoostTime() <= NecoCanvas.GetCurTime());
        //activeAdBoosterButton.SetActive(neco_data.Instance.GetTrapBoostTime() > 0);

        buyBoostLayer.SetActive(neco_data.Instance.GetTrapBoostTime() <= NecoCanvas.GetCurTime());
        buyBoosterButton.SetActive(neco_data.Instance.GetTrapBoostTime() <= NecoCanvas.GetCurTime());
        //activeBuyBoosterButton.SetActive(neco_data.Instance.GetTrapBoostTime() > 0);

        activatedBoosterLayer.SetActive(neco_data.Instance.GetTrapBoostTime() > NecoCanvas.GetCurTime());
    }

    public void OnAdvertiseBoostFail()
    {
        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_273"));
    }

    void RefreshFishtrapData()
    {
        // 부스트 타이머 처리
        if (gameObject.activeInHierarchy)
        {
            RefreshFishtrapBoostTimeData();
        }
    }

    void RefreshFishtrapBoostTimeData()
    {
        if (coroutineFishtrapBoost != null)
        {
            StopCoroutine(coroutineFishtrapBoost);
        }

        coroutineFishtrapBoost = StartCoroutine(RefreshFishtrapBoostTime());
    }

    IEnumerator RefreshFishtrapBoostTime()
    {
        // 부스트 기존 최대 시간 계산
        if (neco_data.Instance.GetTrapBoostType() == neco_data.BOOST_TYPE.CATNIP_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(2).GetEffectTime();
        }
        else if (neco_data.Instance.GetTrapBoostType() == neco_data.BOOST_TYPE.AD_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(4).GetEffectTime();
        }

        // 남은 부스트 시간 계산
        uint remain = neco_data.Instance.GetTrapBoostTime();

        while (remain > NecoCanvas.GetCurTime())
        {
            SetBoostRemainTime(remain - NecoCanvas.GetCurTime());
            yield return new WaitForSeconds(1.0f);
        }

        // 부스트 시간 만료시 처리
        SetBoostRemainTime(0);
        neco_data.Instance.SetTrapBoostTime(0, neco_data.BOOST_TYPE.NONE);

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
        neco_booster boosterData = neco_booster.GetBoosterData(2);

        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_235");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_236");
        popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_239");

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
        RequestButton.SetActive(true);
        closeButton.SetActive(true);

        if (CheckPrologue())
        {
            //levelupButton.SetActive(false);
            RequestButton.SetActive(true);
            closeButton.SetActive(false);

            RequestButton.GetComponentInChildren<Image>().color = originRequestButtonColor;
        }
    }

    public void PrologueGetFishEffect()
    {
        RequestButton.GetComponentInChildren<Image>().color = originRequestButtonColor;

        // 골드 수급 강조 연출 적용
        RequestButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.통발UI등장;
    }

    private void OnDisable()
    {
        RequestButton.GetComponent<RectTransform>().localScale = Vector3.one;
        RequestButton.GetComponent<RectTransform>().DORewind();
        RequestButton.GetComponent<RectTransform>().DOKill();

        isFirstOpen = true;
    }

   
}
