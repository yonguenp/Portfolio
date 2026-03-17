using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NecoShopPanel : MonoBehaviour
{
    public enum SHOP_CATEGORY
    {
        COMMON,
        PACKAGE,
        CAT_LEAF,
        POINT,
        CARD,
    }

    public enum SHOP_RESOURCE_TYPE
    {
        GOLD,
        CAT_LEAF,
        POINT,
    }

    [Header("[Common]")]
    public GameObject withdrawalLayerObject;
    public GameObject withdrawalScrollObject;
    public GameObject probabilityLayerObject;
    public GameObject probabilityScrollObject;
    public GameObject probabilityContainer;

    [Header("[Shop Category Panel Layer]")]
    public CommonShopPanel commonShopPanel;
    public PackageShopPanel packageShopPanel;
    public CatLeafShopPanel catLeafShopPanel;
    public PointShopPanel pointShopPanel;
    public CardShopPopup cardShopPanel;

    [Header("[Button Layer]")]
    public GameObject commonShopButtonOn;
    public GameObject commonShopButtonOff;
    public GameObject packageShopButtonOn;
    public GameObject packageShopButtonOff;
    public GameObject catLeafShopButtonOn;
    public GameObject catLeafShopButtonOff;
    public GameObject pointShopButtonOn;
    public GameObject pointShopButtonOff;
    public GameObject cardShopButtonOn;
    public GameObject cardShopButtonOff;

    public Coffee.UIEffects.UITransitionEffect backgroundPanelEffect;

    uint userGold;
    uint userCatLeaf;
    uint userPoint;

    bool isFirstOpen = true;    // 상점 탭 중복 입력 방지용

    SHOP_CATEGORY curSelected = SHOP_CATEGORY.PACKAGE;
    System.Action CloseCallback = null;
    public void OnClickProbabilityChartButton()
    {
        if (probabilityLayerObject == null || probabilityScrollObject == null) { return; }
        if (probabilityContainer == null) { return; }

        foreach(Transform child in probabilityContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<string> url = new List<string>();
        string value = game_config.GetConfig("rate_table");
        if(!string.IsNullOrEmpty(value))
        {
            JObject json = JObject.Parse(value);
            if(json != null)
            {
                LANGUAGE_TYPE defaultLanguage = LANGUAGE_TYPE.LANGUAGE_KOR;
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Korean:
                        defaultLanguage = LANGUAGE_TYPE.LANGUAGE_KOR;
                        break;
                    case SystemLanguage.English:
                    default:
                        defaultLanguage = LANGUAGE_TYPE.LANGUAGE_ENG;
                        break;
                }
                string key = "en";
                LANGUAGE_TYPE langType = (LANGUAGE_TYPE)PlayerPrefs.GetInt("Setting_Language", (int)defaultLanguage);
                switch (langType)
                {
                    case LANGUAGE_TYPE.LANGUAGE_KOR:
                        key = "ko";
                        break;
                    case LANGUAGE_TYPE.LANGUAGE_ENG:
                        key = "en";
                        break;
                    case LANGUAGE_TYPE.LANGUAGE_JPN:
                        key = "ja";
                        break;
                    case LANGUAGE_TYPE.LANGUAGE_IND:
                        key = "id";
                        break;
                }

                if (!json.ContainsKey(key))
                {
                    key = "en";                    
                }


                if (json.ContainsKey(key))
                {
                    string baseURL = NetworkManager.DOWNLOAD_URL;
                    if (json[key].Type == JTokenType.Array)
                    {
                        JArray array = (JArray)json[key];
                        foreach(JToken val in array)
                        {
                            url.Add(baseURL + val.Value<string>());
                        }
                    }
                    else
                    {
                        url.Add(baseURL + json[key].Value<string>());
                    }
                }
            }
        }

        for (int i = 0; i < url.Count; i++)
        {
            GameObject menu = new GameObject();
            menu.transform.SetParent(probabilityContainer.transform);
            menu.transform.localPosition = Vector3.zero;
            menu.transform.localScale = Vector3.one;
            menu.AddComponent<RectTransform>().pivot = new Vector2(0.0f, 0.5f);

            StartCoroutine(WWWLoadImageTexture(menu.AddComponent<RawImage>(), url[i]));
        }


        probabilityContainer.SetActive(true);
        probabilityScrollObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        probabilityLayerObject.SetActive(!probabilityLayerObject.activeInHierarchy);

        LayoutRebuilder.ForceRebuildLayoutImmediate(probabilityScrollObject.GetComponent<RectTransform>());
    }

    IEnumerator WWWLoadImageTexture(RawImage rawImage, string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            rawImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            rawImage.SetNativeSize();            
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(probabilityScrollObject.GetComponent<RectTransform>());
        yield return new WaitForSeconds(0.1f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(probabilityScrollObject.GetComponent<RectTransform>());
    }

    public void OnClickWithdrawalInfoPopup()
    {
        if (withdrawalLayerObject == null || withdrawalScrollObject == null) { return; }

        withdrawalScrollObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        withdrawalLayerObject.SetActive(!withdrawalLayerObject.activeInHierarchy);

        LayoutRebuilder.ForceRebuildLayoutImmediate(withdrawalScrollObject.GetComponent<RectTransform>());
    }

    public void OnClickOpenShopButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        // 추가 프롤로그 검사
        if (neco_data.GetPrologueSeq() < neco_data.PrologueSeq.프리플레이 && neco_data.GetPrologueSeq() != neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("튜토리얼상점제한"));
            return;
        }

        NecoCanvas.GetPopupCanvas()?.OnShopListPopupShow();
    }

    public void OnClickCommonShopButton()
    {
        if (commonShopPanel == null) { return; }
        if (isFirstOpen == false && curSelected == SHOP_CATEGORY.COMMON) { return; }

        ClearUI();

        UpdateUIState(SHOP_CATEGORY.COMMON);

        commonShopPanel.gameObject.SetActive(true);
        commonShopPanel.InitCommonShopPanel(this);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.CAT_MARKET_SHOP);
    }

    public void OnClickPackageShopButton()
    {
        if (packageShopPanel == null) { return; }
        if (isFirstOpen == false && curSelected == SHOP_CATEGORY.PACKAGE) { return; }

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_296"));
            return;
        }

        ClearUI();

        UpdateUIState(SHOP_CATEGORY.PACKAGE);

        packageShopPanel.gameObject.SetActive(true);
        packageShopPanel.InitPackageShopPanel(this);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.CAT_PACKAGE_SHOP);
    }

    public void OnClickCatLeafShopButton()
    {
        if (catLeafShopPanel == null) { return; }
        if (isFirstOpen == false && curSelected == SHOP_CATEGORY.CAT_LEAF) { return; }

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_296"));
            return;
        }

        ClearUI();

        UpdateUIState(SHOP_CATEGORY.CAT_LEAF);

        catLeafShopPanel.gameObject.SetActive(true);
        catLeafShopPanel.InitCatLeafShopPanel(this);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.CAT_LEAF_SHOP);
    }

    public void OnClickPointShopButton()
    {
        if (pointShopPanel == null) { return; }
        if (isFirstOpen == false && curSelected == SHOP_CATEGORY.POINT) { return; }

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_296"));
            return;
        }

        ClearUI();

        UpdateUIState(SHOP_CATEGORY.POINT);

        pointShopPanel.gameObject.SetActive(true);
        pointShopPanel.InitPointShopPanel(this);
    }

    public void OnClickCardShopButton()
    {
        if (cardShopPanel == null) { return; }
        if (isFirstOpen == false && curSelected == SHOP_CATEGORY.CARD) { return; }

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_296"));
            return;
        }

        ClearUI();

        UpdateUIState(SHOP_CATEGORY.CARD);

        cardShopPanel.gameObject.SetActive(true);
        //cardShopPanel.InitPointShopPanel(this);
    }

    public void OnClickCloseButton()
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_202"));
            return;
        }

        foreach (Transform child in transform)
        {
            if (backgroundPanelEffect.transform != child)
                child.gameObject.SetActive(false);
        }

        backgroundPanelEffect.Hide();

        Invoke("CloseShopPopup", 0.5f);
    }

    public void CloseShopPopup()
    {
        CloseCallback?.Invoke();
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP);        
    }

    public void InitShopPanel(SHOP_CATEGORY category = SHOP_CATEGORY.PACKAGE, System.Action close_cb = null)
    {
        CloseCallback = close_cb;

        foreach (Transform child in transform)
        {
            if(backgroundPanelEffect.transform != child)
                child.gameObject.SetActive(false);
        }

        backgroundPanelEffect.Show();

        curSelected = category;

        Invoke("InitShopPanelAfterEffect", 0.5f);
    }

    public void InitShopPanelAfterEffect()
    {
        foreach (Transform child in transform)
        {
            if (backgroundPanelEffect.transform != child)
                child.gameObject.SetActive(true);
        }

        RefreshResource();

        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.상점배스구매가이드퀘스트)
        {
            OnClickCommonShopButton();
        }
        else
        {
            switch (curSelected)
            {
                case SHOP_CATEGORY.COMMON:
                    OnClickCommonShopButton();
                    break;
                case SHOP_CATEGORY.PACKAGE:
                    OnClickPackageShopButton();
                    break;
                case SHOP_CATEGORY.CAT_LEAF:
                    OnClickCatLeafShopButton();
                    break;
                case SHOP_CATEGORY.POINT:
                    OnClickPointShopButton();
                    break;
                case SHOP_CATEGORY.CARD:
                    OnClickCardShopButton();
                    break;
            }
        }

        isFirstOpen = false;
    }

    public uint GetUserResource(SHOP_RESOURCE_TYPE resourceType)
    {
        RefreshResource();

        switch (resourceType)
        {
            case SHOP_RESOURCE_TYPE.GOLD:
                return userGold;
            case SHOP_RESOURCE_TYPE.CAT_LEAF:
                return userCatLeaf;
            case SHOP_RESOURCE_TYPE.POINT:
                return userPoint;
            default:
                return 0;
        }
    }

    public void RefreshLayer()
    {
        // 재화 갱신 타이밍으로 인한 딜레이 적용
        Invoke("RefreshCurrentPanel", 0.1f);
    }

    public void RefreshResource()
    {
        users user = GameDataManager.Instance.GetUserData();
        if (user == null)
            return;

        object obj;
        uint money = 0;
        if (user.data.TryGetValue("gold", out obj))
        {
            money = (uint)obj;
        }

        userGold = money;

        uint catnip = 0;
        if (user.data.TryGetValue("catnip", out obj))
        {
            catnip = (uint)obj;
        }

        userCatLeaf = catnip;

        uint point = 0;
        if (user.data.TryGetValue("point", out obj))
        {
            point = (uint)obj;
        }

        userPoint = point;

        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
    }

    void RefreshCurrentPanel()
    {
        RefreshResource();

        switch (curSelected)
        {
            case SHOP_CATEGORY.COMMON:
                commonShopPanel.InitCommonShopPanel(this);
                break;
            case SHOP_CATEGORY.PACKAGE:
                packageShopPanel.RefreshData();
                break;
            case SHOP_CATEGORY.CAT_LEAF:
                catLeafShopPanel.RefreshData();
                break;
            case SHOP_CATEGORY.POINT:
                pointShopPanel.RefreshData();
                break;
            case SHOP_CATEGORY.CARD:
                cardShopPanel.RefreshUserGold();
                break;
        }
    }

    void UpdateUIState(SHOP_CATEGORY buttonState)
    {
        if (commonShopPanel == null || packageShopPanel == null || catLeafShopPanel == null || pointShopPanel == null || cardShopPanel == null) { return; }
        if (commonShopButtonOn == null || packageShopButtonOn == null || catLeafShopButtonOn == null || pointShopButtonOn == null || cardShopButtonOn == null) { return; }
        if (commonShopButtonOff == null || packageShopButtonOff == null || catLeafShopButtonOff == null || pointShopButtonOff == null || cardShopButtonOff == null) { return; }

        switch (buttonState)
        {
            case SHOP_CATEGORY.COMMON:
                commonShopButtonOn.SetActive(true);
                packageShopButtonOff.SetActive(true);
                catLeafShopButtonOff.SetActive(true);
                pointShopButtonOff.SetActive(true);
                cardShopButtonOff.SetActive(true);
                curSelected = SHOP_CATEGORY.COMMON;
                break;
            case SHOP_CATEGORY.PACKAGE:
                commonShopButtonOff.SetActive(true);
                packageShopButtonOn.SetActive(true);
                catLeafShopButtonOff.SetActive(true);
                pointShopButtonOff.SetActive(true);
                cardShopButtonOff.SetActive(true);
                curSelected = SHOP_CATEGORY.PACKAGE;
                break;
            case SHOP_CATEGORY.CAT_LEAF:
                commonShopButtonOff.SetActive(true);
                packageShopButtonOff.SetActive(true);
                catLeafShopButtonOn.SetActive(true);
                pointShopButtonOff.SetActive(true);
                cardShopButtonOff.SetActive(true);
                curSelected = SHOP_CATEGORY.CAT_LEAF;
                break;
            case SHOP_CATEGORY.POINT:
                commonShopButtonOff.SetActive(true);
                packageShopButtonOff.SetActive(true);
                catLeafShopButtonOff.SetActive(true);
                pointShopButtonOn.SetActive(true);
                cardShopButtonOff.SetActive(true);
                curSelected = SHOP_CATEGORY.POINT;
                break;
            case SHOP_CATEGORY.CARD:
                commonShopButtonOff.SetActive(true);
                packageShopButtonOff.SetActive(true);
                catLeafShopButtonOff.SetActive(true);
                pointShopButtonOff.SetActive(true);
                cardShopButtonOn.SetActive(true);
                curSelected = SHOP_CATEGORY.CARD;
                break;
        }
    }

    void ClearUI()
    {
        // 기본 상태로 리셋
        commonShopButtonOn.SetActive(false);
        commonShopButtonOff.SetActive(false);
        packageShopButtonOn.SetActive(false);
        packageShopButtonOff.SetActive(false);
        catLeafShopButtonOn.SetActive(false);
        catLeafShopButtonOff.SetActive(false);
        pointShopButtonOn.SetActive(false);
        pointShopButtonOff.SetActive(false);
        cardShopButtonOff.SetActive(false);
        cardShopButtonOff.SetActive(false);

        commonShopPanel.gameObject.SetActive(false);
        packageShopPanel.gameObject.SetActive(false);
        catLeafShopPanel.gameObject.SetActive(false);
        pointShopPanel.gameObject.SetActive(false);
        cardShopPanel.gameObject.SetActive(false);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);
    }

    private void OnDisable()
    {
        isFirstOpen = true;

        withdrawalLayerObject.SetActive(false);
        probabilityLayerObject.SetActive(false);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.배스구이강조:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
                return true;
            case neco_data.PrologueSeq.배스구이완료후밥그릇강조:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_400"));
                return true;
            case neco_data.PrologueSeq.뒷길막이선물생성:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("뒷길막이선물생성"));
                return true;
            case neco_data.PrologueSeq.보은바구니UI등장:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ25"));
                return true;
            case neco_data.PrologueSeq.낚시장난감만들기:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
                return true;
            case neco_data.PrologueSeq.길막이낚시장난감배치:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
            case neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
                return true;
            case neco_data.PrologueSeq.길막이만지기돌발발생:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
            case neco_data.PrologueSeq.사진찍기돌발대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
            case neco_data.PrologueSeq.스와이프가이드:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_402"));
                return true;
            case neco_data.PrologueSeq.배틀패스강조및대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                return true;
        }

        return false;
    }

    public SHOP_CATEGORY CurShopCategory()
    {
        return curSelected;
    }
}
