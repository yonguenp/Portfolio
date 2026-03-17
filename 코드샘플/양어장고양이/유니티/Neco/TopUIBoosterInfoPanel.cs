using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopUIBoosterInfoPanel : MonoBehaviour
{
    enum BOOST_TYPE
    {
        NONE,
        FISH_FARM,
        CAT_GIFT,
        FISH_TRAP,
    }

    [Header("[FishFarmBooster Layer]")]
    public GameObject fishfarmTimeObject;
    public Text fishfarmBoostTimeText;
    Coroutine coroutineFishfarmBoost = null;

    [Header("[CatGiftBooster Layer]")]
    public GameObject catgiftTimeObject;
    public Text catgiftBoostTimeText;
    Coroutine coroutineCatgiftBoost = null;

    [Header("[FishTrapBooster Layer]")]
    public GameObject fishtrapTimeObject;
    public Text fishtrapBoostTimeText;
    Coroutine coroutineFishtrapBoost = null;

    BOOST_TYPE curSelectedType = BOOST_TYPE.NONE;

    private Color originFarmColor = Color.black;
    private Color originGiftColor = Color.black;
    private Color originTrapColor = Color.black;

    private void Awake()
    {
        originFarmColor = fishfarmTimeObject.GetComponent<Image>().color;
        originGiftColor = catgiftTimeObject.GetComponent<Image>().color;
        originTrapColor = fishtrapTimeObject.GetComponent<Image>().color;
    }
    public void OnClickFishFarmLayer()
    {
        if (neco_data.Instance.GetFarmBoostTime() > NecoCanvas.GetCurTime())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_240"), LocalizeData.GetText("LOCALIZE_241"));
            return;
        }

        // 양어장 부스터 관련 처리
        ConfirmPopupData popupData = SetConfirmPopupData(BOOST_TYPE.FISH_FARM);

        curSelectedType = BOOST_TYPE.FISH_FARM;

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, PurchaseBoostItem);
    }

    public void OnClickCatGiftLayer()
    {
        if (neco_data.Instance.GetGiftBoostTime() > NecoCanvas.GetCurTime())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_240"), LocalizeData.GetText("LOCALIZE_241"));
            return;
        }

        // 보은 바구니 부스터 관련 처리
        ConfirmPopupData popupData = SetConfirmPopupData(BOOST_TYPE.CAT_GIFT);

        curSelectedType = BOOST_TYPE.CAT_GIFT;

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, PurchaseBoostItem);
    }

    public void OnClickFishTrapLayer()
    {
        if (neco_data.Instance.GetTrapBoostTime() > NecoCanvas.GetCurTime())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_240"), LocalizeData.GetText("LOCALIZE_241"));
            return;
        }
        // 통발 부스터 관련 처리
        ConfirmPopupData popupData = SetConfirmPopupData(BOOST_TYPE.FISH_TRAP);

        curSelectedType = BOOST_TYPE.FISH_TRAP;

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, PurchaseBoostItem);
    }

    public void PurchaseBoostItem()
    {
        WWWForm param = new WWWForm();
        param.AddField("api", "plant");
        param.AddField("op", 4);
        switch(curSelectedType)
        {
            case BOOST_TYPE.CAT_GIFT:
                {
                    param.AddField("id", 103);
                }
                break;
            case BOOST_TYPE.FISH_FARM:
                {
                    param.AddField("id", 101);
                }
                break;
            case BOOST_TYPE.FISH_TRAP:
                {
                    param.AddField("id", 102);
                }
                break;
        }
        
        NetworkManager.GetInstance().SendApiRequest("plant", 4, param, (response)=> {
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
                            Invoke("RefreshResourceUI", 0.1f);
                            Invoke("RefreshBoosterData", 0.1f);
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

    public void RefreshResourceUI()
    {
        NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
    }

    public void RefreshBoosterData()
    {
        RefreshFishfarmData();
        RefreshCatGiftData();
        RefreshFishtrapData();
    }

    void RefreshFishfarmData()
    {
        // 부스트 타이머 처리
        if (gameObject.activeSelf)
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
        uint remain = neco_data.Instance.GetFarmBoostTime();

        while (remain > NecoCanvas.GetCurTime())
        {
            SetBoostRemainTime(remain - NecoCanvas.GetCurTime(), BOOST_TYPE.FISH_FARM);
            yield return new WaitForSeconds(1.0f);
        }

        SetBoostRemainTime(0, BOOST_TYPE.FISH_FARM);

        // 부스트 시간 만료시 처리
        //...
    }

    void RefreshCatGiftData()
    {
        // 부스트 타이머 처리
        if (gameObject.activeSelf)
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
        uint remain = neco_data.Instance.GetGiftBoostTime();

        while (remain > NecoCanvas.GetCurTime())
        {
            SetBoostRemainTime(remain - NecoCanvas.GetCurTime(), BOOST_TYPE.CAT_GIFT);
            yield return new WaitForSeconds(1.0f);
        }

        SetBoostRemainTime(0, BOOST_TYPE.CAT_GIFT);

        // 부스트 시간 만료시 처리
        //...
    }

    void RefreshFishtrapData()
    {
        // 부스트 타이머 처리
        if (gameObject.activeSelf)
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
        uint remain = neco_data.Instance.GetTrapBoostTime();

        while (remain > NecoCanvas.GetCurTime())
        {
            SetBoostRemainTime(remain - NecoCanvas.GetCurTime(), BOOST_TYPE.FISH_TRAP);
            yield return new WaitForSeconds(1.0f);
        }

        SetBoostRemainTime(0, BOOST_TYPE.FISH_TRAP);

        // 부스트 시간 만료시 처리
        // ...
    }

    void SetBoostRemainTime(uint remainTime, BOOST_TYPE boostType)
    {
        uint minute = remainTime / 60;
        uint second = remainTime % 60;

        switch (boostType)
        {
            case BOOST_TYPE.CAT_GIFT:
                catgiftBoostTimeText.text = string.Format("{0:D2}:{1:D2}", minute, second);
                if(remainTime <= 0)
                {
                    catgiftTimeObject.GetComponent<Image>().color = Color.gray;                    
                }
                else
                {
                    if(originGiftColor != Color.black) 
                        catgiftTimeObject.GetComponent<Image>().color = originGiftColor;
                }
                break;
            case BOOST_TYPE.FISH_FARM:
                fishfarmBoostTimeText.text = string.Format("{0:D2}:{1:D2}", minute, second);
                if (remainTime <= 0)
                {
                    fishfarmTimeObject.GetComponent<Image>().color = Color.gray;
                }
                else
                {
                    if (originFarmColor != Color.black)
                        fishfarmTimeObject.GetComponent<Image>().color = originFarmColor;
                }
                break;
            case BOOST_TYPE.FISH_TRAP:
                fishtrapBoostTimeText.text = string.Format("{0:D2}:{1:D2}", minute, second);
                if (remainTime <= 0)
                {
                    fishtrapTimeObject.GetComponent<Image>().color = Color.gray;
                }
                else
                {
                    if (originTrapColor != Color.black)
                        fishtrapTimeObject.GetComponent<Image>().color = originTrapColor;
                }
                break;
        }
    }

    ConfirmPopupData SetConfirmPopupData(BOOST_TYPE boostType)
    {
        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_235");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_236");
        neco_booster boosterData = null;
        switch (boostType)
        {
            case BOOST_TYPE.CAT_GIFT:
                popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_237");
                boosterData = neco_booster.GetBoosterData(3);
                break;
            case BOOST_TYPE.FISH_FARM:
                popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_238");
                boosterData = neco_booster.GetBoosterData(1);
                break;
            case BOOST_TYPE.FISH_TRAP:
                popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_239");
                boosterData = neco_booster.GetBoosterData(2);
                break;
        }


        switch(boosterData.GetPriceType())
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
}
