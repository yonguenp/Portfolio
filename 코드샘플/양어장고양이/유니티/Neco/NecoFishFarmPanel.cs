using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoFishFarmPanel : NecoAnimatePopup
{
    [Header("[Text Layers]")]
    public Text titleLevelText;
    public Text goldAmountText;

    [Header("[RemainGoldTime Layers]")]
    
    public GameObject remainTimeTextLayer;
    public Text remainTimeText;
    public GameObject fullGoldTimeTextLayer;

    [Header("[Slider Layers]")]
    public Slider goldCountSlider;
    public Image goldCountSliderImage;
    public Color originSliderColor;
    public Color maxSliderColor;
    public Text goldCountText;

    [Header("[Button Layers]")]
    public GameObject levelupButton;
    public GameObject receiveButton;
    public GameObject closeButton;
    public Color levelupButtonColor;
    public Color originReceiveButtonColor;
    public Color DimmedReceiveButtonColor;

    [Header("[FishFarmBooster Layer]")]
    public GameObject AdBoostLayer;
    public GameObject AdBoosterButton;
    public GameObject activeAdBoosterButton;
    public Text AdBoostTimeText;

    public GameObject buyBoostLayer;
    public GameObject buyBoosterButton;
    public GameObject activeBuyBoosterButton;
    public Text buyBoostTimeText;

    public Text boostPriceText;

    [Header("[FishFarmBooster Layer + Ver2]")]
    public GameObject activatedBoosterLayer;
    public GameObject boosterBubbleLayer;
    public Slider boosterSlider;
    public Text sliderRemainTime;
    public Text boostGuideText;

    uint maxBoostTime = 0;

    Coroutine coroutineFishfarmBoost = null;

    neco_level necoLevelData;
    Dictionary<string, RewardData> rewardHistoryDic = new Dictionary<string, RewardData>();

    SUPPLY_UI_TYPE curUIType;

    Coroutine coroutineGoldTimeCount = null;

    public override void OnAnimateDone()
    {
        PrologueSetting();

        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if (seq >= neco_data.PrologueSeq.양어장UI등장 && seq <= neco_data.PrologueSeq.양어장닫고대사)
        {
            uint nowGold = rewardHistoryDic["gold"].gold;
            uint maxGold = necoLevelData.GetNecoLevelValue2();
            float sliderValue = (float)nowGold / maxGold;

            goldCountSliderImage.color = originSliderColor;
            goldCountSlider.value = 0;
            goldCountSlider.DOValue(sliderValue, 1.0f).OnComplete(()=> {
                if (sliderValue >= 1.0f)
                {
                    goldCountSliderImage.color = maxSliderColor;
                }
                PrologueGoldEffect();
                receiveButton.GetComponentInChildren<Button>().interactable = true;
            });

            goldCountText.DOTextInt(0, (int)nowGold, 1.0f, it => string.Format("({0}/{1})", it, maxGold.ToString("n0")));
        }
    }

    protected override void OnEnable()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if (seq >= neco_data.PrologueSeq.양어장UI등장 && seq <= neco_data.PrologueSeq.양어장닫고대사)
        {
            goldCountSliderImage.color = originSliderColor;
            goldCountSlider.value = 0;
            receiveButton.GetComponentInChildren<Image>().color = DimmedReceiveButtonColor;
            receiveButton.GetComponentInChildren<Button>().interactable = false;
        }

        base.OnEnable();
    }

    public void OnClickFishFarmListOpenButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_FISH_FARM_POPUP);
    }

    public void OnClickCloseButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue()) 
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_262"));
            return; 
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_FISH_FARM_POPUP);
    }

    public void OnClickGetFishFarmButton()
    {
        if (rewardHistoryDic == null || rewardHistoryDic["gold"].gold <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_263"));
            return; 
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_FISH_FARM_POPUP);

        GetAllFishFarmReward();
    }

    public void OnClickLevelupUIOpenButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_262"));
            return;
        }

        if (necoLevelData == null) { return; }

        if (necoLevelData.GetNecoLevel() < neco_data.FISH_FARM_MAX_LEVEL)
        {
            NecoCanvas.GetPopupCanvas().OnLevelupPopupShow(curUIType, RefreshData);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_264"));
        }
    }

    public void OnClickPurchaseBoosterButton()
    {
        // 프롤로그 중 방어코드
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_262"));
            return;
        }

        if (neco_data.Instance.GetFarmBoostTime() > NecoCanvas.GetCurTime())
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
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_262"));
            return;
        }

        if (neco_data.Instance.GetFarmBoostTime() > NecoCanvas.GetCurTime())
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

    public void OnClickActivatedBoosterButton()
    {
        if (neco_data.Instance.GetFarmBoostTime() > NecoCanvas.GetCurTime())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_241"));
            return;
        }
    }

    public void InitFishFarmListData(List<RewardData> rewardList, SUPPLY_UI_TYPE uiType)
    {
        Dictionary<string, RewardData> rewardDic = GetRewardData(rewardList);
        if (rewardDic == null) { return; }

        curUIType = uiType;
        necoLevelData = neco_level.GetNecoLevelData("GOLD", neco_data.Instance.GetFishfarmLevel());

        SetFishFarmData(rewardDic);

        // 부스터 관련 처리
        RefreshBoosterData();
        if (neco_data.Instance.GetFarmBoostTime() > NecoCanvas.GetCurTime())
        {
            RefreshFishfarmData();
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.양어장UI등장)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("양어장골드연출", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void SetFishFarmData(Dictionary<string, RewardData> rewardDic)
    {
        if (rewardDic == null) { return; }
        if (necoLevelData == null) { return; }

        rewardHistoryDic.Clear();

        rewardHistoryDic = new Dictionary<string, RewardData>(rewardDic);

        // 텍스트 레벨 관련 처리
        titleLevelText.text = string.Format("Lv {0}/{1}", necoLevelData.GetNecoLevel(), neco_data.FISH_FARM_MAX_LEVEL);

        // 슬라이더 관련 처리
        uint nowGold = rewardHistoryDic["gold"].gold;
        uint GetGold = necoLevelData.GetNecoLevelValue1();
        uint maxGold = necoLevelData.GetNecoLevelValue2();

        goldCountSliderImage.color = nowGold >= maxGold ? maxSliderColor : originSliderColor;

        goldCountText.text = string.Format("({0}/{1})", nowGold.ToString("n0"), maxGold.ToString("n0"));
        goldCountSlider.value = (float)nowGold / maxGold;

        // 텍스트 관련 처리
        fullGoldTimeTextLayer.SetActive(nowGold >= maxGold);
        remainTimeTextLayer.SetActive(nowGold < maxGold);

        if (remainTimeTextLayer.activeSelf)
        {
            //uint calcAmountMinute = (uint)Mathf.Max((maxGold - nowGold) / GetGold, 1);
            //remainTimeText.text = string.Format(LocalizeData.GetText("LOCALIZE_265"), calcAmountMinute);
            RefreshGoldGetTimeData();
        }

        goldAmountText.text = string.Format(LocalizeData.GetText("LOCALIZE_266"), GetGold.ToString("n0"));

        // 버튼 관련 처리
        UpdateButtonColor();

        // max 레벨 버튼 체크
        levelupButton.GetComponentInChildren<Image>().color = necoLevelData.GetNecoLevel() >= neco_data.FISH_FARM_MAX_LEVEL ? DimmedReceiveButtonColor : levelupButtonColor;

        // 부스터 관련 처리
        boostPriceText.text = neco_booster.GetBoosterData(1).GetPrice().ToString();
    }

    public void GetAllFishFarmReward()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 1);
        data.AddField("id", (int)SUPPLY_UI_TYPE.FISH_FARM);

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
                            ShowFishFarmRewardPopup();
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

    public void ShowFishFarmRewardPopup()
    {
        if (rewardHistoryDic != null && rewardHistoryDic.Count > 0)
        {
            NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_269"), LocalizeData.GetText("LOCALIZE_270"), rewardHistoryDic["gold"], ()=> {
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (mapController != null)
                {
                    mapController.SendMessage("양어장골드수급완료", SendMessageOptions.DontRequireReceiver);
                }
            });
        }
    }

    public Dictionary<string, RewardData> GetRewardData(List<RewardData> history)
    {
        Dictionary<string, RewardData> rewardDic = new Dictionary<string, RewardData>();
        if (history != null && history.Count > 0)
        {
            // 보상 데이터로 재가공
            rewardDic.Add("gold", new RewardData());
            foreach (RewardData rewardData in history)
            {
                // 양어장은 골드만 처리 - 2021.4.30

                RewardData reward = new RewardData();
                reward.gold = rewardData.gold;

                // 현재 통발에서는 골드를 주지 않으므로 주석처리 - 2021.4.29
                rewardDic["gold"].gold += reward.gold;
            }
        }

        return rewardDic;
    }

    void RefreshGoldGetTimeData()
    {
        if (coroutineGoldTimeCount != null)
        {
            StopCoroutine(coroutineGoldTimeCount);
        }

        coroutineGoldTimeCount = StartCoroutine(RefreshGoldTime());
    }

    IEnumerator RefreshGoldTime()
    {
        while (neco_data.Instance.GetGoldFullTick() > NecoCanvas.GetCurTime())
        {
            SetRemainTime(neco_data.Instance.GetGoldFullTick() - NecoCanvas.GetCurTime());
            yield return new WaitForSeconds(1.0f);
        }

        Invoke("RefreshData", 1.0f);
    }

    void SetRemainTime(uint remainTime)
    {
        float minute = (float)remainTime / 60;
        int resultMinute = Mathf.CeilToInt(minute);
        
        if (remainTimeTextLayer.activeSelf)
        {
            //uint calcAmountMinute = (uint)Mathf.Max((maxGold - nowGold) / GetGold, 1);
            remainTimeText.text = string.Format(LocalizeData.GetText("LOCALIZE_265"), resultMinute);
        }
    }

    public void RefreshData()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 3);
        data.AddField("id", (int)SUPPLY_UI_TYPE.FISH_FARM);

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
            RewardData goldReward = new RewardData();
            goldReward.gold = 0;
            rewards.Add(goldReward);

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
                                            }
                                            //if (income.ContainsKey("exp"))
                                            //{
                                            //    reward.type = "exp";
                                            //    reward.amount = income["exp"].Value<uint>();
                                            //}
                                            //if (income.ContainsKey("item"))
                                            //{
                                            //    reward.type = "item";
                                            //    JArray item = (JArray)income["item"];
                                            //    foreach (JObject rw in item)
                                            //    {
                                            //        reward.amount = rw["amount"].Value<uint>();
                                            //    }
                                            //}
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

            InitFishFarmListData(rewards, SUPPLY_UI_TYPE.FISH_FARM);
        });
    }

    #region 부스터 관련

    public void PurchaseBoostItem()
    {
        WWWForm param = new WWWForm();
        param.AddField("api", "plant");
        param.AddField("op", 4);
        param.AddField("id", 101);

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
                            Invoke("RefreshFishfarmData", 0.1f);
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
        param.AddField("id", 101);
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
                            Invoke("RefreshFishfarmData", 0.1f);
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
        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        //activatedBoosterLayer.SetActive(false);
        //activeAdBoosterButton.SetActive(false);
        //activeBuyBoosterButton.SetActive(false);

        //Button AdButton = AdBoosterButton.GetComponent<Button>();
        //Button catnipButton = buyBoosterButton.GetComponent<Button>();
        //switch (neco_data.Instance.GetFarmBoostType())
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

        AdBoostLayer.SetActive(neco_data.Instance.GetFarmBoostTime() <= NecoCanvas.GetCurTime());
        AdBoosterButton.SetActive(neco_data.Instance.GetFarmBoostTime() <= NecoCanvas.GetCurTime());
        //activeAdBoosterButton.SetActive(neco_data.Instance.GetFarmBoostTime() > 0);

        buyBoostLayer.SetActive(neco_data.Instance.GetFarmBoostTime() <= NecoCanvas.GetCurTime());
        buyBoosterButton.SetActive(neco_data.Instance.GetFarmBoostTime() <= NecoCanvas.GetCurTime());
        //activeBuyBoosterButton.SetActive(neco_data.Instance.GetFarmBoostTime() > 0);

        activatedBoosterLayer.SetActive(neco_data.Instance.GetFarmBoostTime() > NecoCanvas.GetCurTime());
    }

    public void OnAdvertiseBoostFail()
    {
        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_273"));
    }

    void RefreshFishfarmData()
    {
        // 부스트 타이머 처리
        if (gameObject.activeInHierarchy)
        {
            RefreshFishfarmBoostTimeData();
        }
    }

    void RefreshFishfarmBoostTimeData()
    {
        if (coroutineFishfarmBoost != null)
        {
            StopCoroutine(coroutineFishfarmBoost);
        }

        coroutineFishfarmBoost = StartCoroutine(RefreshFishfarmBoostTime());
    }

    IEnumerator RefreshFishfarmBoostTime()
    {
        // 부스트 기존 최대 시간 계산
        if (neco_data.Instance.GetFarmBoostType() == neco_data.BOOST_TYPE.CATNIP_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(1).GetEffectTime();
        }
        else if (neco_data.Instance.GetFarmBoostType() == neco_data.BOOST_TYPE.AD_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(4).GetEffectTime();
        }

        // 남은 부스트 시간 계산
        uint remain = neco_data.Instance.GetFarmBoostTime();

        while (remain > NecoCanvas.GetCurTime())
        {
            SetBoostRemainTime(remain - NecoCanvas.GetCurTime());
            yield return new WaitForSeconds(1.0f);
        }

        // 부스트 시간 만료시 처리
        SetBoostRemainTime(0);
        neco_data.Instance.SetFarmBoostTime(0, neco_data.BOOST_TYPE.NONE);

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
        neco_booster boosterData = neco_booster.GetBoosterData(1);

        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_235");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_236");
        popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_238");

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
        }
    }

    public void PrologueGoldEffect()
    {
        receiveButton.GetComponentInChildren<Image>().color = originReceiveButtonColor;

        // 골드 수급 강조 연출 적용
        receiveButton.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        //Sequence buttonStroke = DOTween.Sequence().SetLoops(-1, LoopType.Restart);
        //buttonStroke.Append(receiveStroke.GetComponent<RectTransform>().DOSizeDelta(new Vector2(320, 100), 1.5f));
        //buttonStroke.Join(receiveStroke.transform.DOScale(1.2f, 1.5f));
        //buttonStroke.Join(receiveStroke.GetComponent<Image>().DOFade(0, 1.5f));

        //buttonStroke.Restart();
    }

    void UpdateButtonColor()
    {
        if (rewardHistoryDic == null || rewardHistoryDic["gold"].gold <= 0)
        {
            receiveButton.GetComponentInChildren<Image>().color = DimmedReceiveButtonColor;
        }
        else
        {
            receiveButton.GetComponentInChildren<Image>().color = originReceiveButtonColor;
        }
    }

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() == neco_data.PrologueSeq.양어장UI등장 || neco_data.GetPrologueSeq() == neco_data.PrologueSeq.양어장골드연출;
    }

    private void OnDisable()
    {
        receiveButton.GetComponent<RectTransform>().localScale = Vector3.one;
        receiveButton.GetComponent<RectTransform>().DORewind();
        receiveButton.GetComponent<RectTransform>().DOKill();
    }
}
