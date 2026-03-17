using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class neco_fish_trade : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_FISH_TRADE; }

    [NonSerialized]
    uint id = 0;
    public uint GetID()
    {
        if (id == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                id = (uint)obj;
            }
        }

        return id;
    }

    [NonSerialized]
    uint trade_coin = 0;
    public uint GetCoin()
    {
        if (trade_coin == 0)
        {
            object obj;
            if (data.TryGetValue("trade_coin", out obj))
            {
                trade_coin = (uint)obj;
            }
        }

        return trade_coin;
    }

    [NonSerialized]
    uint bunch_count = 0;
    public uint GetBunchCount()
    {
        if (bunch_count == 0)
        {
            object obj;
            if (data.TryGetValue("bunch_count", out obj))
            {
                bunch_count = (uint)obj;
            }
        }

        return bunch_count;
    }
};

public class NecoFishTruckPanel : MonoBehaviour
{
    public delegate void RewardCallback(List<RewardData> data);

    const int TRADE_TICKET_COST = 300;

    public GameObject tradeEffectPopup;
    public GameObject tradeEffectPopupRepeat;

    [Header("[FishTruck Info List]")]
    public GameObject fishTruckScrollContainer;
    public GameObject fishTruckCloneObject;

    [Header("[FishTruck Info UI]")]
    public Image tradeButtonImage;
    public Text fishTicketAmountText;

    public Text remainTimeText;

    [Header("[Layer Color Info]")]
    public Color originButtonColor;
    public Color dimmedButtonColor;

    [Header("[Contents Info]")]
    public GameObject contentsInfoPopup;
    public Text contentsInfoText;

    Coroutine coroutineRemainTimeCount = null;

    DateTime fishtruckStartTime;
    DateTime fishtruckEndTime;

    public void OnClickTradeButton()
    {
        // 교환하기 버튼 눌렀을 때 처리
        if (user_items.GetUserItemAmount(138) < TRADE_TICKET_COST)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("활어차교환재료부족"));
            return;
        }

        // 교환하기 처리
        ConfirmPopupData popupData = SetConfirmPopupTradeInfoData();

        if(user_items.GetUserItemAmount(138) >= (TRADE_TICKET_COST * 10))
            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, ShowTradeEffectPopupRepeat);
        else
            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, ShowTradeEffectPopup);
    }

    public void OnClickContentsInfoButton()
    {
        if (contentsInfoPopup == null) { return; }

        contentsInfoPopup.SetActive(!contentsInfoPopup.activeSelf);

        if (contentsInfoPopup.activeSelf)
        {
            if (contentsInfoText != null)
            {
#if UNITY_IOS
                contentsInfoText.text = LocalizeData.GetText("활어차도움말_FORIOS");
#else
                contentsInfoText.text = LocalizeData.GetText("활어차도움말");
#endif
            }
        }
    }

    public void OnClickCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.FISH_TRADER_POPUP);
    }

    public void OnClickCloseTradeEffectPopup()
    {
        tradeEffectPopup.SetActive(false);
        tradeEffectPopupRepeat.SetActive(false);
    }

    private void OnEnable()
    {
        InitFishTruckData();

        fishTruckScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }

    public void InitFishTruckData()
    {
        SetFishTimeDataByJson();

        SetFishTruckData();
    }

    public void SetFishTruckData()
    {
        if (fishTruckScrollContainer == null || fishTruckCloneObject == null) { return; }

        foreach (Transform child in fishTruckScrollContainer.transform)
        {
            if (child.gameObject != fishTruckCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        fishTruckCloneObject.SetActive(true);

        List<game_data> fishTrades = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_FISH_TRADE);
        
        if (fishTrades != null)
        {
            foreach (neco_fish_trade fishtrade in fishTrades)
            {
                GameObject fishInfoUI = Instantiate(fishTruckCloneObject);
                fishInfoUI.transform.SetParent(fishTruckScrollContainer.transform);
                fishInfoUI.transform.localScale = fishTruckCloneObject.transform.localScale;
                fishInfoUI.transform.localPosition = fishTruckCloneObject.transform.localPosition;

                FishTruckData data = new FishTruckData();
                data.fishTradeData = fishtrade;
                data.itemData = items.GetItem(fishtrade.GetID());

                fishInfoUI.GetComponent<FishTruckItem>().SetFishTruckItemData(data, this);
            }
        }

        fishTruckCloneObject.SetActive(false);

        // 재화 정보 세팅
        uint fishTicket = user_items.GetUserItemAmount(138);
        fishTicketAmountText.text = fishTicket.ToString("n0");

        // 버튼 상태 설정
        tradeButtonImage.color = fishTicket >= TRADE_TICKET_COST ? originButtonColor : dimmedButtonColor;

        // 리프레쉬 타이머 작동
        if (gameObject.activeInHierarchy)
        {
            RemainFishTruckTimeData();
        }
    }

    public void ShowTradeEffectPopup()
    {
        tradeEffectPopup.SetActive(true);
        tradeEffectPopup.transform.Find("Close_Bt").GetComponent<Button>().interactable = true;

        Transform background = tradeEffectPopup.transform.Find("BackgroundPanel");
        background.GetComponent<Button>().onClick.RemoveAllListeners();

        Transform BoxObject = tradeEffectPopup.transform.Find("BoxSpineObject");

        background.GetComponent<Button>().onClick.AddListener(() => { OnTradeSelect(); });
        BoxObject.Find("ItemImage").gameObject.SetActive(false);

        Spine.Unity.SkeletonGraphic spine = BoxObject.GetComponent<Spine.Unity.SkeletonGraphic>();
        
        spine.startingAnimation = "wait";
        spine.AnimationState.TimeScale = 1.0f;

        spine.startingLoop = true;
        spine.skeletonDataAsset.Clear();
        spine.skeletonDataAsset.scale = 0.02f;
        spine.Initialize(true);

        tradeEffectPopup.transform.Find("BoxSpineObject").Find("WarningTextLayer").gameObject.SetActive(true);
        tradeEffectPopup.transform.Find("BoxSpineObject").Find("Touch_Text").gameObject.SetActive(true);

        Text touchText = tradeEffectPopup.transform.Find("BoxSpineObject").Find("Touch_Text").GetComponent<Text>();
        touchText.DOKill();
        touchText.color = Color.white;
        touchText.DOColor(new Color(1, 1, 1, 0.7f), 0.3f).SetLoops(-1, LoopType.Yoyo);

        return;
    }

    public void ShowTradeEffectPopupRepeat()
    {
        tradeEffectPopupRepeat.SetActive(true);
        tradeEffectPopupRepeat.transform.Find("Close_Bt").GetComponent<Button>().interactable = true;

        Transform background = tradeEffectPopupRepeat.transform.Find("BackgroundPanel");
        background.GetComponent<Button>().onClick.RemoveAllListeners();

        SkeletonGraphic[] BoxObjects = tradeEffectPopupRepeat.GetComponentsInChildren<SkeletonGraphic>();

        background.GetComponent<Button>().onClick.AddListener(() => { OnTradeSelectRepeat(); });
        foreach (SkeletonGraphic BoxObject in BoxObjects)
        {
            BoxObject.transform.Find("ItemImage").gameObject.SetActive(false);
            Spine.Unity.SkeletonGraphic spine = BoxObject.GetComponent<Spine.Unity.SkeletonGraphic>();

            spine.startingAnimation = "wait";
            spine.AnimationState.TimeScale = 1.0f;

            spine.startingLoop = true;
            spine.skeletonDataAsset.Clear();
            spine.skeletonDataAsset.scale = 0.013f;
            spine.Initialize(true);
        }

        background.Find("WarningTextLayer").gameObject.SetActive(true);
        background.Find("Touch_Text").gameObject.SetActive(true);

        Text touchText = background.Find("Touch_Text").GetComponent<Text>();
        touchText.DOKill();
        touchText.color = Color.white;
        touchText.DOColor(new Color(1, 1, 1, 0.7f), 0.3f).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnTradeSelect()
    {
        Transform background = tradeEffectPopup.transform.Find("BackgroundPanel");
        background.GetComponent<Button>().onClick.RemoveAllListeners();

        TryTradeFishTicket(OnBoxOpenAction);

        tradeEffectPopup.transform.Find("BoxSpineObject").Find("WarningTextLayer").gameObject.SetActive(false);
        tradeEffectPopup.transform.Find("BoxSpineObject").Find("Touch_Text").gameObject.SetActive(false);

        tradeEffectPopup.transform.Find("Close_Bt").GetComponent<Button>().interactable = false;
    }

    public void OnTradeSelectRepeat()
    {
        Transform background = tradeEffectPopupRepeat.transform.Find("BackgroundPanel");
        background.GetComponent<Button>().onClick.RemoveAllListeners();

        TryTradeFishTicketRepeat(OnBoxOpenActionRepeat);

        background.Find("WarningTextLayer").gameObject.SetActive(false);
        background.Find("Touch_Text").gameObject.SetActive(false);

        tradeEffectPopupRepeat.transform.Find("Close_Bt").GetComponent<Button>().interactable = false;
    }

    public void OnBoxOpenAction(List<RewardData> reward)
    {
        if (reward.Count == 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_344"));
            return;
        }

        if (reward[0].memoryData != null)
        {
            neco_cat_memory memory = reward[0].memoryData;
            NecoCanvas.GetPopupCanvas().OnShowGetCatPhotoBoxPopup(neco_cat.GetNecoCat(memory.GetNecoMemoryCatID()), memory, () => {
                if (reward[0].point > 0)
                {
                    reward[0].memoryData = null;
                    NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_342"), LocalizeData.GetText("LOCALIZE_345"), reward[0]);
                }
            });
            return;
        }

        Transform BoxObject = tradeEffectPopup.transform.Find("BoxSpineObject");
        Spine.Unity.SkeletonGraphic spine = BoxObject.GetComponent<Spine.Unity.SkeletonGraphic>();

        spine.startingAnimation = "open";
        spine.startingLoop = false;
        spine.Initialize(true);

        Image itemIcon = BoxObject.Find("ItemImage").GetComponent<Image>();
        Text Amount = itemIcon.transform.Find("Amount").GetComponent<Text>();
        RewardData data = reward[0];
        if (data.gold > 0)
        {
            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
            Amount.text = data.gold.ToString("n0");
        }
        else if (data.catnip > 0)
        {
            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
            Amount.text = data.catnip.ToString("n0");
        }
        else
        {
            itemIcon.sprite = data.itemData.GetItemIcon();
            Amount.text = data.count.ToString("n0");
        }

        float speedRaito = 3.0f;
        Spine.Animation animationObject = spine.skeletonDataAsset.GetSkeletonData(false).FindAnimation("open");
        spine.AnimationState.TimeScale = speedRaito;

        itemIcon.gameObject.SetActive(true);
        StartCoroutine(DelayCallFunc((animationObject.Duration / speedRaito) + 1, () => { OnReciveItem(reward); }));
    }

    public void OnBoxOpenActionRepeat(List<RewardData> reward)
    {
        if (reward.Count == 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_344"));
            return;
        }
        
        SkeletonGraphic[] BoxObjects = tradeEffectPopupRepeat.GetComponentsInChildren<SkeletonGraphic>();
        if(BoxObjects.Length != reward.Count)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_344"));
            return;
        }

        StartCoroutine(EachBoxAnimation(reward, BoxObjects));
    }

    IEnumerator EachBoxAnimation(List<RewardData> reward, SkeletonGraphic[] BoxObjects)
    {
        float animTime = 0.0f;
        for (int i = 0; i < reward.Count; i++)
        {
            Spine.Unity.SkeletonGraphic spine = BoxObjects[i];

            spine.startingAnimation = "open";
            spine.startingLoop = false;            
            spine.Initialize(true);

            Image itemIcon = BoxObjects[i].transform.Find("ItemImage").GetComponent<Image>();
            Text Amount = itemIcon.transform.Find("Amount").GetComponent<Text>();
            RewardData data = reward[i];
            if (data.gold > 0)
            {
                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                Amount.text = data.gold.ToString("n0");
            }
            else if (data.catnip > 0)
            {
                itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
                Amount.text = data.catnip.ToString("n0");
            }
            else
            {
                itemIcon.sprite = data.itemData.GetItemIcon();
                Amount.text = data.count.ToString("n0");
            }

            float speedRaito = 3.0f;
            Spine.Animation animationObject = spine.skeletonDataAsset.GetSkeletonData(false).FindAnimation("open");
            spine.AnimationState.TimeScale = speedRaito;

            itemIcon.gameObject.SetActive(true);

            float delay = 0.1f;
            yield return new WaitForSeconds(delay);

            animTime = (animationObject.Duration / speedRaito);            
        }

        StartCoroutine(DelayCallFunc(animTime + 0.5f, () => { OnReciveItem(reward); }));
    }


    IEnumerator DelayCallFunc(float seconds, Action act)
    {
        yield return new WaitForSeconds(seconds);
        act();
    }

    public void OnReciveItem(List<RewardData> reward)
    {
        if (reward.Count == 0)
        {
            OnClickCloseTradeEffectPopup();
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("활어차교환실패"));
        }
        else if (reward.Count == 1)
        {
            NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("활어차교환완료"), LocalizeData.GetText("LOCALIZE_343"), reward[0], () => {

                OnClickCloseTradeEffectPopup();
                RefreshData();
            });
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("활어차교환완료"), LocalizeData.GetText("LOCALIZE_343"), reward, () => {

                OnClickCloseTradeEffectPopup();
                RefreshData();
            });
        }
    }

    public void TryTradeFishTicket(RewardCallback cb)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "trade");
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("trade", 2, data, (response) =>
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
                if (uri == "trade")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            List<RewardData> ret = new List<RewardData>();
                            if (row.ContainsKey("rew") && row["rew"].HasValues)
                            {
                                JObject income = (JObject)row["rew"];
                                if (income.ContainsKey("gold"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.gold = income["gold"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("catnip"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.catnip = income["catnip"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("point"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.point = income["point"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("item"))
                                {
                                    JArray item = (JArray)income["item"];
                                    foreach (JObject rw in item)
                                    {
                                        RewardData reward = new RewardData();
                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                        reward.count = rw["amount"].Value<uint>();
                                        ret.Add(reward);
                                    }
                                }

                                if (income.ContainsKey("memory"))
                                {
                                    JArray memory = (JArray)income["memory"];
                                    foreach (JArray rw in memory)
                                    {
                                        RewardData reward = new RewardData();
                                        reward.memoryData = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                        reward.point = rw[1].Value<uint>();
                                        ret.Add(reward);
                                    }
                                }
                            }

                            cb?.Invoke(ret);
                        }
                        else
                        {
                            List<RewardData> ret = new List<RewardData>();
                            cb?.Invoke(ret);                            
                        }
                    }
                }
            }
        }, (err) => {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_344"));
        });
    }

    public void TryTradeFishTicketRepeat(RewardCallback cb)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "trade");
        data.AddField("op", 3);

        NetworkManager.GetInstance().SendApiRequest("trade", 3, data, (response) =>
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
                if (uri == "trade")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            List<RewardData> ret = new List<RewardData>();
                            if (row.ContainsKey("rew") && row["rew"].HasValues)
                            {
                                JArray rews = (JArray)row["rew"];
                                foreach (JObject income in rews)
                                {
                                    if (income.ContainsKey("gold"))
                                    {
                                        RewardData reward = new RewardData();
                                        reward.gold = income["gold"].Value<uint>();
                                        ret.Add(reward);
                                    }

                                    if (income.ContainsKey("catnip"))
                                    {
                                        RewardData reward = new RewardData();
                                        reward.catnip = income["catnip"].Value<uint>();
                                        ret.Add(reward);
                                    }

                                    if (income.ContainsKey("point"))
                                    {
                                        RewardData reward = new RewardData();
                                        reward.point = income["point"].Value<uint>();
                                        ret.Add(reward);
                                    }

                                    if (income.ContainsKey("item"))
                                    {
                                        JArray item = (JArray)income["item"];
                                        foreach (JObject rw in item)
                                        {
                                            RewardData reward = new RewardData();
                                            reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                            reward.count = rw["amount"].Value<uint>();
                                            ret.Add(reward);
                                        }
                                    }

                                    if (income.ContainsKey("memory"))
                                    {
                                        JArray memory = (JArray)income["memory"];
                                        foreach (JArray rw in memory)
                                        {
                                            RewardData reward = new RewardData();
                                            reward.memoryData = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                            reward.point = rw[1].Value<uint>();
                                            ret.Add(reward);
                                        }
                                    }
                                }
                            }

                            cb?.Invoke(ret);
                        }
                        else
                        {
                            List<RewardData> ret = new List<RewardData>();
                            cb?.Invoke(ret);
                        }
                    }
                }
            }
        }, (err) => {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_338"), LocalizeData.GetText("LOCALIZE_344"));
        });
    }

    // 일반 ui 갱신용
    public void RefreshData()
    {
        SetFishTimeDataByJson();

        SetFishTruckData();
    }

    void RemainFishTruckTimeData()
    {
        if (coroutineRemainTimeCount != null)
        {
            StopCoroutine(coroutineRemainTimeCount);
        }

        coroutineRemainTimeCount = StartCoroutine(RemainFishTruckTime());
    }

    IEnumerator RemainFishTruckTime()
    {
        DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();
        DateTime endTime = fishtruckEndTime;

        while (curTime <= endTime)
        {
            curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();

            TimeSpan diff = (endTime - curTime);

            SetFishTruckRemainTime(diff);

            yield return new WaitForSecondsRealtime(1.0f);
        }

        SetFishTruckRemainTime(TimeSpan.Zero);

        // 활어차 시간 종료 시 처리
        NecoCanvas.GetPopupCanvas().OnPopupClose();
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("활어차교환시간종료"), LocalizeData.GetText("활어차시간만료"));
    }

    void SetFishTruckRemainTime(TimeSpan remainTime)
    {
        string timeText = "";

        timeText += string.Format(LocalizeData.GetText("LOCALIZE_510"), remainTime.Hours);
        timeText += string.Format(LocalizeData.GetText("LOCALIZE_257"), remainTime.Minutes);
        timeText += string.Format(LocalizeData.GetText("LOCALIZE_211"), remainTime.Seconds);

        remainTimeText.text = timeText;
    }

    ConfirmPopupData SetConfirmPopupTradeInfoData()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();
        popupData.titleText = LocalizeData.GetText("아이템교환");

        if (user_items.GetUserItemAmount(138) >= (TRADE_TICKET_COST * 10))
        {
            popupData.titleMessageText = LocalizeData.GetText("연뽑활어차교환가이드");

            popupData.contentsSprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_05");
            popupData.contentsNameText = LocalizeData.GetText("연뽑활어차상자이름");

            //popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_206");

            popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_fishticket");
            popupData.amountText = (TRADE_TICKET_COST * 10).ToString("n0");
        }
        else
        {
            popupData.titleMessageText = LocalizeData.GetText("아이템교환가이드");

            popupData.contentsSprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Box_05");
            popupData.contentsNameText = LocalizeData.GetText("활어차행운상자이름");

            //popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_206");

            popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_fishticket");
            popupData.amountText = TRADE_TICKET_COST.ToString("n0");
        }
        return popupData;
    }

    void SetFishTimeDataByJson()
    {
        fishtruckStartTime = neco_data.Instance.GetFishtruckDateTime(true);
        fishtruckEndTime = neco_data.Instance.GetFishtruckDateTime(false);
    }

    private void OnDisable()
    {
        contentsInfoPopup.SetActive(false);

        tradeEffectPopup.SetActive(false);
        tradeEffectPopupRepeat.SetActive(false);
    }
}
