using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonShopPanel : MonoBehaviour
{
    const int REFRESH_COST = 60;

    const int MAX_AD_REFRESH_COUNT = 5;

    public GameObject commonShopListContainer;
    public Color originButtonColor;
    public Color DimmedButtonColor;

    [Header("[Fish Shop Data]")]
    public GameObject normalFishScrollContainer;
    public GameObject specialFishScrollContainer;
    public GameObject fishRefreshButtonObject;
    public Text specialFishRefreshTimeText;
    public Text specialFishADCount;
    Coroutine coroutineFishTimeCount = null;

    [Header("[Hardware Shop Data]")]
    public GameObject normalHardwareScrollContainer;
    public GameObject specialHardwareScrollContainer;
    public GameObject hardwareRefreshButtonObject;
    public Text specialHardwareRefreshTimeText;
    public Text specialHardwareADCount;
    Coroutine coroutineHardwareTimeCount = null;

    [Header("[Item Clone Objects]")]
    public GameObject marketObjectCloneObject;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    NecoShopPanel rootParentPanel;

    public void OnClickRefreshFishMarket()
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        if (rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.CAT_LEAF) < REFRESH_COST)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_203"));
            return;
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, TryRefreshFishMarket);
    }

    public void OnClickRefreshFishMarketWithAD()
    {
#if UNITY_EDITOR
        NecoCanvas.GetPopupCanvas().OnToastPopupShow("유니티 에디터에서는 사용불가");
        return;
#endif
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        if(neco_data.Instance.GetMarketData().fishADCount >= MAX_AD_REFRESH_COUNT)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("광고제한"));
            return;
        }

        AdvertiseManager.GetInstance().TryADWithCallback(() => {
            TryRefreshFishMarket(true);
        }, () => {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_273"));
        });
    }

    public void TryRefreshFishMarket()
    {
        TryRefreshFishMarket(false);
    }

    public void TryRefreshFishMarket(bool useAD)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "shop");
        data.AddField("op", 3);
        data.AddField("type", 1);
        data.AddField("catnip", useAD ? 0 : 1);
        data.AddField("ad", useAD ? 1 : 0);

        NetworkManager.GetInstance().SendApiRequest("shop", 3, data, (response) =>
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
                if (uri == "shop")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshFishMarketItem", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_355"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_356"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_357"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_358"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_359"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                        }
                    }
                }
            }
        });        
    }

    public void OnClickRefreshHardwareMarket()
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        if (rootParentPanel.GetUserResource(NecoShopPanel.SHOP_RESOURCE_TYPE.CAT_LEAF) < REFRESH_COST)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_203"));
            return;
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, TryRefreshHardwareMarket);
    }

    public void OnClickRefreshHardwareMarketWithAD()
    {
#if UNITY_EDITOR
        NecoCanvas.GetPopupCanvas().OnToastPopupShow("유니티 에디터에서는 사용불가");
        return;
#endif
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        if (neco_data.Instance.GetMarketData().hardwareADCount >= MAX_AD_REFRESH_COUNT)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("광고제한"));
            return;
        }

        AdvertiseManager.GetInstance().TryADWithCallback(() => {
            TryRefreshHardwareMarket(true);
        }, () => {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_273"));
        });
    }

    public void TryRefreshHardwareMarket()
    {
        TryRefreshHardwareMarket(false);
    }

    public void TryRefreshHardwareMarket(bool useAD)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "shop");
        data.AddField("op", 3);
        data.AddField("type", 2);
        data.AddField("catnip", useAD ? 0 : 1);
        data.AddField("ad", useAD ? 1 : 0); 

        NetworkManager.GetInstance().SendApiRequest("shop", 3, data, (response) =>
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
                if (uri == "shop")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshHardwareMarketItem", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_355"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_356"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_357"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_358"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_359"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                        }
                    }
                }
            }
        });        
    }

    public void InitCommonShopPanel(NecoShopPanel parentPanel)
    {
        rootParentPanel = parentPanel;

        ClearData();

        commonShopListContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        WWWForm data = new WWWForm();
        data.AddField("api", "shop");
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("shop", 2, data, (response) =>
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
                if (uri == "shop")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshData", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_355"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_356"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_357"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_358"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_359"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                        }
                    }
                }
            }
        });
    }

    void SetNormalItemList()
    {
        if (marketObjectCloneObject == null) { return; }

        // Normal Fish
        if (normalFishScrollContainer == null) { return; }

        foreach (Transform child in normalFishScrollContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<neco_shop> normalFishList = neco_shop.GetNecoShopListByType("f_market");

        foreach (neco_shop shopData in normalFishList)
        {
            GameObject normalFish = Instantiate(marketObjectCloneObject);
            normalFish.transform.SetParent(normalFishScrollContainer.transform);
            normalFish.transform.localScale = marketObjectCloneObject.transform.localScale;
            normalFish.transform.localPosition = marketObjectCloneObject.transform.localPosition;

            normalFish.GetComponent<ShopCommonItemInfo>().SetNormalItemData(false, shopData, rootParentPanel);
        }

        // Normal Hardware
        if (normalHardwareScrollContainer == null) { return; }

        foreach (Transform child in normalHardwareScrollContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<neco_shop> normalHardwareList = neco_shop.GetNecoShopListByType("h_market");

        foreach (neco_shop shopData in normalHardwareList)
        {
            GameObject normalFish = Instantiate(marketObjectCloneObject);
            normalFish.transform.SetParent(normalHardwareScrollContainer.transform);
            normalFish.transform.localScale = marketObjectCloneObject.transform.localScale;
            normalFish.transform.localPosition = marketObjectCloneObject.transform.localPosition;

            normalFish.GetComponent<ShopCommonItemInfo>().SetNormalItemData(false, shopData, rootParentPanel);
        }
    }

    void SetSpecialFishMarketList()
    {
        // Special Fish
        if (specialFishScrollContainer == null) { return; }

        List<GameObject> specialFishObjectList = new List<GameObject>();
        foreach (Transform child in specialFishScrollContainer.transform)
        {
            child.gameObject.SetActive(false);
            specialFishObjectList.Add(child.gameObject);
        }

        List<neco_market> specialFishList = neco_market.GetNecoMarketSpecialItem("fish_store");

        if (specialFishObjectList == null || specialFishObjectList.Count <= 0) { return; }
        if (specialFishList == null || specialFishList.Count <= 0) { return; }

        // 갯수가 다르면 return.
        //if (specialFishObjectList.Count != specialFishList.Count) { return; }

        int uiIndex = 0;
        for (int i = 0; i < specialFishList.Count; ++i)
        {
            if (neco_data.Instance.GetMarketData().saleFish.ContainsKey(specialFishList[i].GetNecoMarketID()))
            {
                specialFishObjectList[uiIndex].SetActive(true);
                specialFishObjectList[uiIndex].GetComponent<ShopCommonItemInfo>().SetSpecialItemData(true, specialFishList[i], rootParentPanel, neco_data.Instance.GetMarketData().saleFish[specialFishList[i].GetNecoMarketID()]);
                uiIndex++;
            }

            if (uiIndex >= specialFishObjectList.Count)
                break;
        }

        if (gameObject.activeSelf)
        {
            // 프롤로그 체크
            if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
            {
                SetFishRemainTime(0);
                return;
            }

            RefreshFishTimeData();
        }

        if (specialFishADCount)
        {
            specialFishADCount.text = string.Format("{0}/{1}", neco_data.Instance.GetMarketData().fishADCount, MAX_AD_REFRESH_COUNT);
            specialFishADCount.color = neco_data.Instance.GetMarketData().fishADCount >= MAX_AD_REFRESH_COUNT ? new Color(0.8039216f, 0.2666667f, 0.2666667f, 1.0f) : Color.white;
            specialFishADCount.gameObject.GetComponent<Outline>().enabled = neco_data.Instance.GetMarketData().fishADCount < MAX_AD_REFRESH_COUNT;

            fishRefreshButtonObject.GetComponent<Image>().color = neco_data.Instance.GetMarketData().fishADCount >= MAX_AD_REFRESH_COUNT ? DimmedButtonColor : originButtonColor;
        }
    }

    void SetSpecialHardwareMarketList()
    {
        // Special Hardware
        if (specialHardwareScrollContainer == null) { return; }

        List<GameObject> specialHardwareObjectList = new List<GameObject>();
        foreach (Transform child in specialHardwareScrollContainer.transform)
        {
            child.gameObject.SetActive(false);
            specialHardwareObjectList.Add(child.gameObject);
        }

        List<neco_market> specialHardwareList = neco_market.GetNecoMarketSpecialItem("hardware_store");

        if (specialHardwareObjectList == null || specialHardwareObjectList.Count <= 0) { return; }
        if (specialHardwareList == null || specialHardwareList.Count <= 0) { return; }

        // 갯수가 다르면 return.
        //if (specialHardwareObjectList.Count != specialHardwareList.Count) { return; }

        int uiIndex = 0;
        for (int i = 0; i < specialHardwareList.Count; ++i)
        {
            if (neco_data.Instance.GetMarketData().saleHardware.ContainsKey(specialHardwareList[i].GetNecoMarketID()))
            {
                specialHardwareObjectList[uiIndex].SetActive(true);
                specialHardwareObjectList[uiIndex].GetComponent<ShopCommonItemInfo>().SetSpecialItemData(true, specialHardwareList[i], rootParentPanel, neco_data.Instance.GetMarketData().saleHardware[specialHardwareList[i].GetNecoMarketID()]);
                uiIndex++;
            }

            if (uiIndex >= specialHardwareObjectList.Count)
                break;
        }

        if (gameObject.activeSelf)
        {
            // 프롤로그 체크
            if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
            {
                SetHardwareRemainTime(0);
                return;
            }

            RefreshHardwareTimeData();
        }

        if (specialHardwareADCount)
        {
            specialHardwareADCount.text = string.Format("{0}/{1}", neco_data.Instance.GetMarketData().hardwareADCount, MAX_AD_REFRESH_COUNT);
            specialHardwareADCount.color = neco_data.Instance.GetMarketData().hardwareADCount >= MAX_AD_REFRESH_COUNT ? new Color(0.8039216f, 0.2666667f, 0.2666667f, 1.0f) : Color.white;
            specialHardwareADCount.gameObject.GetComponent<Outline>().enabled = neco_data.Instance.GetMarketData().hardwareADCount < MAX_AD_REFRESH_COUNT;

            hardwareRefreshButtonObject.GetComponent<Image>().color = neco_data.Instance.GetMarketData().hardwareADCount >= MAX_AD_REFRESH_COUNT ? DimmedButtonColor : originButtonColor;
        }
    }

    void RefreshFishTimeData()
    {
        if (coroutineFishTimeCount != null)
        {
            StopCoroutine(coroutineFishTimeCount);
        }

        coroutineFishTimeCount = StartCoroutine(RefreshFishTime());
    }

    IEnumerator RefreshFishTime()
    {
        uint remain = neco_data.Instance.GetMarketData().fishRefresh;
        
        while (remain > NecoCanvas.GetCurTime())
        {
            SetFishRemainTime(remain - NecoCanvas.GetCurTime());
            yield return new WaitForSecondsRealtime(1.0f);
        }

        SetFishRemainTime(0);

        WWWForm data = new WWWForm();
        data.AddField("api", "shop");
        data.AddField("op", 3);
        data.AddField("type", 1);
        data.AddField("catnip", 0);

        NetworkManager.GetInstance().SendApiRequest("shop", 3, data, (response) =>
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
                if (uri == "shop")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshFishMarketItem", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_355"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_356"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_357"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_358"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_359"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                        }
                    }
                }
            }
        });
    }

    void SetFishRemainTime(uint remainTime)
    {
        string timeText = "";
        if (remainTime > 0)
        {
            uint prevMinute = remainTime / 60;
            uint minute = prevMinute % 60;
            uint second = remainTime % 60;
            uint hour = prevMinute > 60 ? prevMinute / 60 : 0;

            if (hour > 0)
            {
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_510"), hour);
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_257"), minute);
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_211"), second);
            }
            else if (minute > 0)
            {
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_257"), minute);
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_211"), second);
            }
            else if (second > 0)
            {
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_211"), second);
            }
        }

        specialFishRefreshTimeText.text = timeText;
    }

    void RefreshHardwareTimeData()
    {
        if (coroutineHardwareTimeCount != null)
        {
            StopCoroutine(coroutineHardwareTimeCount);
        }

        coroutineHardwareTimeCount = StartCoroutine(RefreshHardwareTime());
    }

    IEnumerator RefreshHardwareTime()
    {
        uint remain = neco_data.Instance.GetMarketData().hardwareRefresh;

        while (remain > NecoCanvas.GetCurTime())
        {
            SetHardwareRemainTime(remain - NecoCanvas.GetCurTime());
            yield return new WaitForSecondsRealtime(1.0f);
        }

        SetHardwareRemainTime(0);

        WWWForm data = new WWWForm();
        data.AddField("api", "shop");
        data.AddField("op", 3);
        data.AddField("type", 2);
        data.AddField("catnip", 0);

        NetworkManager.GetInstance().SendApiRequest("shop", 3, data, (response) =>
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
                if (uri == "shop")
                {
                    JToken resultCode = row["rs"];

                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshHardwareMarketItem", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_355"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_356"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_357"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_358"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_359"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_360"), msg);
                        }
                    }
                }
            }
        });
    }

    void SetHardwareRemainTime(uint remainTime)
    {
        string timeText = "";
        if (remainTime > 0)
        {
            uint prevMinute = remainTime / 60;
            uint minute = prevMinute % 60;
            uint second = remainTime % 60;
            uint hour = prevMinute > 60 ? prevMinute / 60 : 0;

            if (hour > 0)
            {
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_510"), hour);
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_257"), minute);
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_211"), second);
            }
            else if (minute > 0)
            {
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_257"), minute);
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_211"), second);
            }
            else if (second > 0)
            {
                timeText += string.Format(LocalizeData.GetText("LOCALIZE_211"), second);
            }
        }

        specialHardwareRefreshTimeText.text = timeText;
    }

    public void RefreshData()
    {
        ClearData();

        SetNormalItemList();
        SetSpecialFishMarketList();
        SetSpecialHardwareMarketList();

        //commonShopListContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void RefreshFishMarketItem()
    {
        SetSpecialFishMarketList();

        //rootParentPanel.RefreshLayer();
        rootParentPanel.RefreshResource();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void RefreshHardwareMarketItem()
    {
        SetSpecialHardwareMarketList();

        //rootParentPanel.RefreshLayer();
        rootParentPanel.RefreshResource();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    ConfirmPopupData SetConfirmPopupData()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();
        popupData.titleText = LocalizeData.GetText("LOCALIZE_204");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_205");

        popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_206");

        popupData.amountIcon = popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
        popupData.amountText = REFRESH_COST.ToString("n0"); // todo bt - 추후 데이터 연동 필요

        return popupData;
    }

    void ClearData()
    {

    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }
    }
}
