using Newtonsoft.Json.Linq;
using SBCommonLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasPopup : Popup
{
    [Header("[페이지 오브젝트]")]
    [SerializeField] GameObject rulePage;
    [SerializeField] GameObject rankPage;
    [SerializeField] GameObject boxInfoPage;
    [SerializeField] GameObject rewardinfoPopup;
    [SerializeField] GameObject equip_reward_Popup;

    [Header("[상세 정보]")]
    [SerializeField] Text myAssetCount;
    [SerializeField] Text myGiftPoint;
    [SerializeField] Text giftPointRank;
    [SerializeField] Text convertPrice;
    [SerializeField] Text advertisementText;

    [Header("[박스 정보]")]
    [SerializeField] Text[] boxCount;

    [Header("[버 튼]")]
    [SerializeField] List<Button> ui_limitBtn = new List<Button>();
    public ChristmasInfo christmasInfo { get; private set; } = null;
    public int curpage = 0;  //룰정보 -> 1, 박스정보 -> 2, 랭크 -> 5 , 6,7 서브팝업
    public TimeSpan updateTime = TimeSpan.FromSeconds(0);
    public List<ChristmasRank> rankList { get; private set; } = new List<ChristmasRank>();
    public bool isBoxEvent = true;
    public override void Close()
    {
        switch (curpage)
        {
            case 1:
                rulePage.SetActive(false);
                curpage = 0;
                return;
            case 2:
                boxInfoPage.SetActive(false);
                curpage--;
                return;
            case 5:
                rankPage.SetActive(false);
                curpage = 0;
                return;
            case 6:
                rewardinfoPopup.SetActive(false);
                curpage--;
                return;
            case 7:
                equip_reward_Popup.SetActive(false);
                curpage--;
                return;
            case 0:
                break;
            default:
                curpage = 0;
                return;
        }
        base.Close();
        curpage = 0;
    }
    public override void Open(CloseCallback cb = null)
    {
        TryOpenBox();

        base.Open(cb);
    }

    public override void RefreshUI()
    {
        rulePage.SetActive(false);
        boxInfoPage.SetActive(false);
        rankPage.SetActive(false);
        rewardinfoPopup.SetActive(false);

        base.RefreshUI();

        if (christmasInfo == null)
            return;

        myAssetCount.text = Managers.UserData.GetMyItemCount(32).ToString();
        myGiftPoint.text = christmasInfo.score.ToString();
        giftPointRank.text = christmasInfo.rank <= 0 ? StringManager.GetString("순위밖") : christmasInfo.rank.ToString();

        boxCount[0].text = Managers.UserData.GetMyItemCount(33).ToString();
        boxCount[1].text = Managers.UserData.GetMyItemCount(34).ToString();
        boxCount[2].text = Managers.UserData.GetMyItemCount(35).ToString();

        var tb = Managers.Data.GetData(GameDataManager.DATA_TYPE.event_schedule, 1002) as EventScheduleData;

        if (SBUtil.KoreanTime <= tb.ui_duration)
        {

#if !UNITY_EDITOR && UNITY_ANDROID
        var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        var isPC = packageManager.Call<bool>("hasSystemFeature", "com.google.android.play.feature.HPE_EXPERIENCE");
        if (isPC)
        {
            advertisementText.transform.parent.gameObject.SetActive(false);
        }
#endif

            if (!string.IsNullOrEmpty(christmasInfo.ad_time))
            {
                DateTime adTime = DateTime.Parse(christmasInfo.ad_time);
                adTime = adTime.AddHours(4);

                if (SBCommonLib.SBUtil.KoreanTime < adTime)
                {
                    updateTime = (adTime - SBCommonLib.SBUtil.KoreanTime);
                    string tempText = string.Empty;
                    if (updateTime.Days >= 1)
                    {
                        tempText += StringManager.GetString("ui_day", updateTime.Days.ToString("D2"));
                        tempText += StringManager.GetString("ui_hour", updateTime.Hours.ToString("D2"));
                    }
                    else
                    {
                        if (updateTime.Hours >= 1)
                        {
                            tempText += StringManager.GetString("ui_hour", updateTime.Hours.ToString("D2"));
                            tempText += StringManager.GetString("ui_min", updateTime.Minutes.ToString("D2"));
                        }
                        else
                        {
                            tempText += StringManager.GetString("ui_min", updateTime.Minutes.ToString("D2"));
                            tempText += StringManager.GetString("ui_second", updateTime.Seconds.ToString("D2"));
                        }
                    }

                    advertisementText.text = StringManager.GetString("ui_left_time", tempText);
                }
                else
                {
                    advertisementText.text = StringManager.GetString("button_ad_get");
                    updateTime = TimeSpan.FromSeconds(0);
                }
            }
        }

        if (SBUtil.KoreanTime > tb.end_time)
        {
            foreach (Button btn in ui_limitBtn)
            {
                btn.GetComponentInChildren<Text>().text = StringManager.GetString("기간종료");
                btn.interactable = false;
            }
        }
        else
        {
            foreach (Button btn in ui_limitBtn)
            {
                btn.interactable = true;
            }
            Invoke("UpdateTimer", 1.0f);
        }
    }

    public void UpdateTimer()
    {
        CancelInvoke("UpdateTimer");
        var tempText = string.Empty;
        if (updateTime.TotalSeconds > 0)
        {
            if (updateTime.Days >= 1)
            {
                tempText += StringManager.GetString("ui_day", updateTime.Days.ToString());
                tempText += StringManager.GetString("ui_hour", updateTime.Hours.ToString("D2"));
            }
            else
            {
                if (updateTime.Hours >= 1)
                {
                    tempText += StringManager.GetString("ui_hour", updateTime.Hours.ToString());
                    tempText += StringManager.GetString("ui_min", updateTime.Minutes.ToString("D2"));
                }
                else
                {
                    tempText += StringManager.GetString("ui_min", updateTime.Minutes.ToString());
                    tempText += StringManager.GetString("ui_second", updateTime.Seconds.ToString("D2"));
                }
            }

            advertisementText.text = StringManager.GetString("ui_left_time", tempText);
            updateTime -= new TimeSpan(0, 0, 1);

            Invoke("UpdateTimer", 1.0f);
        }
        else
        {
            advertisementText.text = StringManager.GetString("button_ad_get");
        }
    }

    public void ExchangeBtn()
    {
        int amount = Managers.UserData.GetMyItemCount(32);
        if (amount < 100)
        {
            PopupCanvas.Instance.ShowFadeText("이벤트교환부족");
            return;
        }

        isBoxEvent = false;
        TryOpenBox(32);
    }
    public void UseAdBtn()
    {
        if (updateTime.TotalSeconds > 0)
        {
            PopupCanvas.Instance.ShowFadeText("광고시간부족");
            return;
        }

        if (!Managers.ADS.IsAdvertiseReady())
        {
            PopupCanvas.Instance.ShowFadeText("광고로드실패");
            return;
        }

        Managers.ADS.TryADWithCallback(() =>
        {
            isBoxEvent = false;
            TryOpenBox(0, 0, 1);
        }, () =>
        {
            PopupCanvas.Instance.ShowFadeText("광고로드실패");
        });
    }
    public void GoShop()
    {
        PopupCanvas.Instance.ShowShopPopup(2, () => { RefreshUI(); });
    }

    public void UseBox(int type)
    {
        int amount = Managers.UserData.GetMyItemCount(type);
        if (amount <= 0)
        {
            PopupCanvas.Instance.ShowConfirmPopup("ui_cbox_lack", () =>
            {
                PopupCanvas.Instance.ShowShopPopup(2, () => { RefreshUI(); });
            });
            return;
        }

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.BOXGACHA_POPUP) as BoxGachaPopup;
        popup.SetUI(type);

        int bunchCount = 1;
        switch (type)
        {
            case 33:
                bunchCount = GameConfig.Instance.CHRISTMAS_NORAML;
                break;
            case 34:
                bunchCount = GameConfig.Instance.CHRISTMAS_RARE;
                break;
            case 35:
                bunchCount = GameConfig.Instance.CHRISTMAS_UNIQUE;
                break;
        }

        if (amount > bunchCount)
        {
            TryOpenBox(type, bunchCount);
        }
        else
        {
            TryOpenBox(type, 1);
        }
    }

    public void RuleBtn()
    {
        if (curpage == 1)
        {
            curpage = 0;
            rulePage.SetActive(false);
        }
        else
        {
            curpage = 1;
            rulePage.SetActive(true);
        }
    }
    public void OpenRankPage()
    {
        curpage = 5;
        rankPage.GetComponent<Christmas_RankUI>().Init(this);
        rankPage.SetActive(true);
    }
    public void OpenBoxInfoPage()
    {
        curpage = 2;
        boxInfoPage.SetActive(true);
        boxInfoPage.GetComponent<BoxInfoPopup>().Init();
    }

    public void TryOpenBox(int try_open = 0, int try_count = 0, int advertisement = 0)
    {
        SBWeb.TryXmasBox(try_open, try_count, advertisement, (res) =>
        {
            christmasInfo = new ChristmasInfo(res["result"]);
            SetRankData(res["rank"]);

            if (christmasInfo != null)
                RefreshUI();
        });
    }

    public void SetRankData(JToken data)
    {
        if (data == null)
            return;

        rankList.Clear();

        foreach (JToken obj in (JArray)data)
        {
            rankList.Add(new ChristmasRank
            {
                name = obj["name"].Value<string>(),
                score = obj["score"].Value<int>()
            });
        }
    }
}

public class ChristmasInfo
{
    public string ad_time = null;
    public int score = 0;
    public int rank = -1;

    public ChristmasInfo(JToken obj)
    {
        Setdata(obj);
    }

    public void Setdata(JToken obj)
    {
        if (obj == null)
            return;

        ad_time = obj["ad_time"].Value<string>();
        score = obj["score"].Value<int>();
        rank = obj["rank"].Value<int>();
    }
}

public class ChristmasRank
{
    public string name;
    public int score;
}