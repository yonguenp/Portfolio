using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json.Linq;

public class SeasonPassPanel : MonoBehaviour
{
    enum SEASON_PASS_BUTTON
    {
        DAILY_MISSION,
        SEASON_MISSION,
        SEASON_REWARD
    }

    [Header("[Special Gift Info Layer]")]
    public GameObject topBgPanel;

    [Header("[Level Info Layer]")]
    public Text levelText;
    public Slider levelSlider;
    public Text sliderExpText;

    //[Header("[Title Info Layer]")]
    ////public Image seasonPassBgImage;
    //public Text seasonPassTitleText;
    //public Text seasonPassSubTitleText;

    [Header("[Season Info Layer]")]
    public Image seasonPasslevelIcon;
    public Text seasonPassLevelText;
    public Slider seasonPassExpSlider;
    public Text seasonPassExpText;
    public Text seasonPassRemainTimeText;

    [Header("[Button Layer]")]
    public GameObject dailyMissionButtonOn;
    public GameObject dailyMissionButtonOff;
    public GameObject seasonMissionButtonOn;
    public GameObject seasonMissionButtonOff;
    public GameObject seasonPassRewardButtonOn;
    public GameObject seasonPassRewardButtonOff;

    public GameObject dailyMissionRedDot_On;
    public GameObject dailyMissionRedDot_Off;
    public GameObject seasonMissionRedDot_On;
    public GameObject seasonMissionRedDot_Off;
    public GameObject passRewardRedDot_On;
    public GameObject passRewardRedDot_Off;

    [Header("[Receive Button Layer]")]
    public Button receiveAllButton;
    public Color originButtonColor;
    public Color dimmedButtonColor;

    [Header("[Contents Info]")]
    public GameObject contentsInfoPopup;

    [Header("[SeasonPass Contents Layer]")]
    public DailyMissionPanel dailyMissionPanel;
    public SeasonMissionPanel seasonMissionPanel;
    public SeasonPassRewardPanel seasonRewardPanel;

    public Coffee.UIEffects.UITransitionEffect backgroundPanelEffect;

    public GameObject DonationObject;

    neco_pass curPassData;

    bool isFirstOpen = true;    // 상점 탭 중복 입력 방지용

    SEASON_PASS_BUTTON curSelected = SEASON_PASS_BUTTON.DAILY_MISSION;

    public delegate void Callback();
    Coroutine levelAnimation = null;

    Coroutine coroutineSeasonPassTimeCount = null;

    public void OnClickOpenSeasonPassButton()
    {
        if (neco_data.PrologueSeq.배틀패스강조및대사 == neco_data.GetPrologueSeq() || neco_data.PrologueSeq.프리플레이 <= neco_data.GetPrologueSeq())
        {
            RefreshSeasonPass();
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
            return;
        }
        
    }

    public void RefreshSeasonPass()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "mission");
        data.AddField("op", 1);

        NetworkManager.GetInstance().SendApiRequest("mission", 1, data, (response) =>
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
                if (uri == "mission")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            NecoCanvas.GetPopupCanvas().Invoke("OnSeasonPassPopupShow", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_509"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_499"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_500"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_501"); break;
                                case 6: msg = LocalizeData.GetText("LOCALIZE_502"); break;
                                case 7: msg = LocalizeData.GetText("LOCALIZE_278"); break;
                                case 8: msg = LocalizeData.GetText("LOCALIZE_500"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }
        });
    }

    public void OnClickDailyMissionButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        if (dailyMissionPanel == null) { return; }
        if (isFirstOpen == false && curSelected == SEASON_PASS_BUTTON.DAILY_MISSION) { return; }

        ClearUI();

        UpdateUIState(SEASON_PASS_BUTTON.DAILY_MISSION);
        RefreshMissionData();
    }

    public void OnClickSeasonMissionButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        if (seasonMissionPanel == null) { return; }
        if (isFirstOpen == false && curSelected == SEASON_PASS_BUTTON.SEASON_MISSION) { return; }

        ClearUI();

        UpdateUIState(SEASON_PASS_BUTTON.SEASON_MISSION);
        RefreshMissionData();
    }

    public void OnClickSeasonRewardButton()
    {
        if (seasonRewardPanel == null) { return; }
        if (isFirstOpen == false && curSelected == SEASON_PASS_BUTTON.SEASON_REWARD) { return; }

        ClearUI();

        UpdateUIState(SEASON_PASS_BUTTON.SEASON_REWARD);
        RefreshMissionData();
    }

    public void OnClickContentsInfoButton()
    {
        if (contentsInfoPopup == null) { return; }

        contentsInfoPopup.SetActive(!contentsInfoPopup.activeSelf);

        if (contentsInfoPopup.activeSelf)
        {
            Text text = contentsInfoPopup.transform.Find("ContentsInfoText").GetComponent<Text>();
            if (text != null)
            {
#if UNITY_IOS
                text.text = LocalizeData.GetText("LOCALIZE_122_FORIOS");
#else
                text.text = LocalizeData.GetText("LOCALIZE_122");
#endif
            }
        }
    }

    public void OnClickDonationListButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        Application.OpenURL(NetworkManager.DONATION_URL);
    }

    public void OnClickReceiveAllButton()
    {
        switch (curSelected)
        {
            case SEASON_PASS_BUTTON.DAILY_MISSION:
                dailyMissionPanel.DoReceiveAllReward();
                break;
            case SEASON_PASS_BUTTON.SEASON_MISSION:
                seasonMissionPanel.DoReceiveAllReward();
                break;
            case SEASON_PASS_BUTTON.SEASON_REWARD:
                seasonRewardPanel.DoReceiveAllReward();
                break;
        }
    }

    public void OnClickCloseButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        ClearUI();
        transform.Find("Background").gameObject.SetActive(false);
        backgroundPanelEffect.Hide();
        Invoke("CloseMissionPopup", 0.5f);
    }

    public void CloseMissionPopup()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose();
    }

    public void InitSeasonPassUI()
    {
        ClearUI();

        backgroundPanelEffect.Show();

        transform.Find("Background").gameObject.SetActive(true);
        curPassData = neco_pass.GetNecoPassData(neco_data.Instance.GetPassData().GetCurMissionID());

        SetTopUIInfoLayer();
        if (gameObject.activeInHierarchy)
        {
            RefreshSeasonPassTimeData();
        }
        SetSeasonInfoLayer();

        OnClickSeasonRewardButton();

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.PASS_TICKET);

        isFirstOpen = false;

        UpdateRedDotState();

#if UNITY_IOS
        //DonationObject.SetActive(false);
#else
        //DonationObject.SetActive(true);
#endif
    }

    public void RefreshLayer()
    {
        //ClearUI();

        SetTopUIInfoLayer();
        if (gameObject.activeInHierarchy)
        {
            RefreshSeasonPassTimeData();
        }
        SetSeasonInfoLayer();

        UpdateUIState(curSelected);
        RefreshMissionData();

        // 재화 레이어 갱신
        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        UpdateRedDotState();
    }

    public void RefreshLayerForPassTicket()
    {
        SetTopUIInfoLayer();
        if (gameObject.activeInHierarchy)
        {
            RefreshSeasonPassTimeData();
        }
        SetSeasonInfoLayer();

        seasonRewardPanel.RefreshLayerForPassTicket();

        UpdateRedDotState();        
    }

    void SetTopUIInfoLayer()
    {
        // 상단 정보 관련 데이터 세팅
        // 배경 이미지 세팅
        //seasonPassBgImage.sprite = curPassData.GetNecoPassBgIcon();

        //// 스페셜 보상 정보 세팅
        //List<neco_shop> shopDataList = neco_shop.GetNecoShopListByType("pass");
        //if (shopDataList.Count > 0)
        //{
        //    neco_shop step2Data = shopDataList.Find(x => x.GetNecoShopGoodsID() == 1);
        //    neco_shop step3Data = shopDataList.Find(x => x.GetNecoShopGoodsID() == 2);

        //    step2GiftImage.sprite = step2Data?.GetNecoShopIcon();
        //    step3GiftImage.sprite = step3Data?.GetNecoShopIcon();
        //}

        neco_pass_data necoPassData = neco_data.Instance.GetPassData();

        // 배경 이미지 세팅
        foreach (Transform child in topBgPanel.transform)
        {
            Destroy(child.gameObject);
        }

        string resourcePath = game_config.GetConfigByKeyAndVersion("season_pass", "15");
        if (!string.IsNullOrEmpty(resourcePath))
        {
            GameObject packegePrefab = Resources.Load<GameObject>(resourcePath);
            GameObject packageItem = Instantiate(packegePrefab);
            packageItem.transform.SetParent(topBgPanel.transform);
            packageItem.transform.localScale = packegePrefab.transform.localScale;
            packageItem.transform.localPosition = topBgPanel.transform.localPosition;

            packageItem.GetComponent<SeasonPassBannerPanel>().SetSeasonPassBannerData(curPassData);
        }

        //// 타이틀 세팅
        //seasonPassTitleText.text = curPassData.GetNecoPassMainTitle();
        //seasonPassSubTitleText.text = curPassData.GetNecoPassSubTitle();

        // 레벨 데이터 세팅
        // todo bt - 상수값 부분 추후 max 데이터로 바꿔야함
        uint nowLevel = necoPassData.GetCurLevel() >= 20 ? necoPassData.GetCurLevel() : necoPassData.GetCurLevel() + 1;
        neco_pass_reward_forever necoRewardData = neco_pass_reward_forever.GetNecoPassReward(nowLevel);

        if (isFirstOpen)
        {
            levelText.text = string.Format("Lv.{0}", necoPassData.GetCurLevel());
            levelSlider.value = (float)necoPassData.GetTotalExp() / necoRewardData.GetNecoPassRewardExp();
            sliderExpText.text = string.Format("{0}/{1}", necoPassData.GetTotalExp(), necoRewardData.GetNecoPassRewardExp());
        }
        else
        {
            if (levelAnimation != null)
                StopCoroutine(levelAnimation);
            
            levelAnimation = StartCoroutine(LevelAction());
        }
    }

    IEnumerator LevelAction()
    {
        string strTarget = levelText.text;
        string strTmp = System.Text.RegularExpressions.Regex.Replace(strTarget, @"\D", "");        
        int prevLevel = int.Parse(strTmp);

        float prevSlider = levelSlider.value;

        sliderExpText.gameObject.SetActive(false);

        neco_pass_data necoPassData = neco_data.Instance.GetPassData();
        while (prevLevel != necoPassData.GetCurLevel())
        {
            float animTime = 1.0f - prevSlider;
            levelSlider.DOValue(1.0f, animTime);
            
            yield return new WaitForSeconds(animTime);

            prevLevel++;
            levelText.text = string.Format("Lv.{0}", prevLevel);
            levelSlider.value = 0.0f;
            levelText.transform.localScale = Vector3.one * 2.0f;
            levelText.transform.DOScale(1.0f, 0.5f);
            yield return new WaitForSeconds(0.1f);
        }

        uint nowLevel = necoPassData.GetCurLevel() >= 20 ? necoPassData.GetCurLevel() : necoPassData.GetCurLevel() + 1;
        neco_pass_reward_forever necoRewardData = neco_pass_reward_forever.GetNecoPassReward(nowLevel);
        float value = (float)necoPassData.GetTotalExp() / necoRewardData.GetNecoPassRewardExp();
        float time = 1.0f - value;
        levelSlider.DOValue(value, time);

        yield return new WaitForSeconds(time);

        sliderExpText.gameObject.SetActive(true);
        sliderExpText.text = string.Format("{0}/{1}", necoPassData.GetTotalExp(), necoRewardData.GetNecoPassRewardExp());
    }

    void RefreshSeasonPassTimeData()
    {
        if (coroutineSeasonPassTimeCount != null)
        {
            StopCoroutine(coroutineSeasonPassTimeCount);
        }

        coroutineSeasonPassTimeCount = StartCoroutine(SetRemainTimeInfoLayer());
    }

    IEnumerator SetRemainTimeInfoLayer()
    {
        DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();
        DateTime endTime = curPassData.GetEndDate();
        while (curTime <= endTime)
        {
            curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();

            TimeSpan diff = (endTime - curTime);

            if (diff.Days <= 0 && diff.Hours <= 0 && diff.Minutes <= 0)
            {
                seasonPassRemainTimeText.text = LocalizeData.GetText("1분미만");
            }
            else
            {
                seasonPassRemainTimeText.text = diff.Days.ToString() + LocalizeData.GetText("LOCALIZE_349") + diff.Hours.ToString() + LocalizeData.GetText("LOCALIZE_350") + diff.Minutes.ToString() + LocalizeData.GetText("LOCALIZE_351");
            }

            yield return new WaitForSecondsRealtime(1.0f);
        }

        seasonPassRemainTimeText.text = LocalizeData.GetText("LOCALIZE_352");

        NecoCanvas.GetPopupCanvas().OnPopupClose();
        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_352"), LocalizeData.GetText("LOCALIZE_354"));
    }

    void SetSeasonInfoLayer()
    {
        Sprite levSprite = null;
        switch(neco_data.Instance.GetPassData().GetCurPassStep())
        {
            case 3:
                //levSprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_B_lv3");
                break;
            case 2:
                //levSprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_B_lv2");
                break;
            case 0:
            case 1:
            default:
                levSprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_B_lv1");
                break;
        }

        //seasonPasslevelIcon.sprite = levSprite;
        //uint curLevel = neco_data.Instance.GetPassData().GetCurLevel();
        //seasonPassLevelText.text = "LV." + curLevel.ToString();

        //neco_pass_reward curRewardData = neco_pass_reward.GetNecoPassReward(curPassData.GetNecoPassSeason(), curLevel);

        //uint nowExp = neco_data.Instance.GetPassData().GetCurExp();
        //uint maxExp = curRewardData.GetNecoPassRewardExp(); 
        
        //seasonPassExpSlider.value = (float)nowExp / maxExp;
        //seasonPassExpText.text = string.Format("{0} / {1}", nowExp, maxExp);
    }

    void UpdateUIState(SEASON_PASS_BUTTON buttonState)
    {
        if (dailyMissionPanel == null || seasonMissionPanel == null || seasonRewardPanel == null) { return; }
        if (dailyMissionButtonOn == null || seasonMissionButtonOn == null || seasonPassRewardButtonOn == null) { return; }
        if (dailyMissionButtonOff == null || seasonMissionButtonOff == null || seasonPassRewardButtonOff == null) { return; }

        switch (buttonState)
        {
            case SEASON_PASS_BUTTON.DAILY_MISSION:
                dailyMissionButtonOn.SetActive(true);
                seasonMissionButtonOff.SetActive(true);
                seasonPassRewardButtonOff.SetActive(true);
                curSelected = SEASON_PASS_BUTTON.DAILY_MISSION;
                break;
            case SEASON_PASS_BUTTON.SEASON_MISSION:
                dailyMissionButtonOff.SetActive(true);
                seasonMissionButtonOn.SetActive(true);
                seasonPassRewardButtonOff.SetActive(true);
                curSelected = SEASON_PASS_BUTTON.SEASON_MISSION;
                break;
            case SEASON_PASS_BUTTON.SEASON_REWARD:
                dailyMissionButtonOff.SetActive(true);
                seasonMissionButtonOff.SetActive(true);
                seasonPassRewardButtonOn.SetActive(true);
                curSelected = SEASON_PASS_BUTTON.SEASON_REWARD;
                break;
        }
    }

    void ClearUI()
    {
        // 기본 상태로 리셋
        dailyMissionButtonOn.SetActive(false);
        dailyMissionButtonOff.SetActive(false);
        seasonMissionButtonOn.SetActive(false);
        seasonMissionButtonOff.SetActive(false);
        seasonPassRewardButtonOn.SetActive(false);
        seasonPassRewardButtonOff.SetActive(false);

        dailyMissionPanel.gameObject.SetActive(false);
        seasonMissionPanel.gameObject.SetActive(false);
        seasonRewardPanel.gameObject.SetActive(false);
    }

    void RefreshMissionData()
    {
        int opCode = 0;

        WWWForm data = new WWWForm();
        data.AddField("api", "mission");
        switch (curSelected)
        {
            case SEASON_PASS_BUTTON.DAILY_MISSION:
                opCode = 4;
                data.AddField("type", 1);
                break;
            case SEASON_PASS_BUTTON.SEASON_MISSION:
                opCode = 4;
                data.AddField("type", 2);
                break;
            case SEASON_PASS_BUTTON.SEASON_REWARD:
                opCode = 1;
                break;
        }

        data.AddField("op", opCode);

        NetworkManager.GetInstance().SendApiRequest("mission", opCode, data, (response) =>
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
                if (uri == "mission")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RefreshMissionUI", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_509"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_499"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_500"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_501"); break;
                                case 6: msg = LocalizeData.GetText("LOCALIZE_502"); break;
                                case 7: msg = LocalizeData.GetText("LOCALIZE_278"); break;
                                case 8: msg = LocalizeData.GetText("LOCALIZE_500"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }
        });   
    }

    void RefreshMissionUI()
    {
        switch (curSelected)
        {
            case SEASON_PASS_BUTTON.DAILY_MISSION:
                if (dailyMissionPanel.gameObject.activeSelf == false)
                {
                    dailyMissionPanel.gameObject.SetActive(true);
                }
                dailyMissionPanel.InitDailyMissionUI(this);                
                break;
            case SEASON_PASS_BUTTON.SEASON_MISSION:
                if (seasonMissionPanel.gameObject.activeSelf == false)
                {
                    seasonMissionPanel.gameObject.SetActive(true);
                }
                seasonMissionPanel.InitSeasonMissionUI(this);                
                break;
            case SEASON_PASS_BUTTON.SEASON_REWARD:
                if (seasonRewardPanel.gameObject.activeSelf == false)
                {
                    seasonRewardPanel.gameObject.SetActive(true);
                }
                seasonRewardPanel.InitSeasonPassRewardUI(this);
                break;
        }

        UpdateRedDotState();
    }

    public void UpdateRedDotState()
    {
        dailyMissionRedDot_On.SetActive(neco_data.Instance.GetPassData().IsDailyAlarm());
        dailyMissionRedDot_Off.SetActive(neco_data.Instance.GetPassData().IsDailyAlarm());

        seasonMissionRedDot_On.SetActive(neco_data.Instance.GetPassData().IsSeasonAlarm());
        seasonMissionRedDot_Off.SetActive(neco_data.Instance.GetPassData().IsSeasonAlarm());

        passRewardRedDot_On.SetActive(neco_data.Instance.GetPassData().IsPassAlarm());
        passRewardRedDot_Off.SetActive(neco_data.Instance.GetPassData().IsPassAlarm());
    }

    public void SetActiveReciveAllButton(bool enable)
    {
        receiveAllButton.interactable = enable;
        receiveAllButton.GetComponent<Image>().color = enable ? originButtonColor : dimmedButtonColor;
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.배틀패스강조및대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                return true;
            case neco_data.PrologueSeq.길막이낚시장난감배치:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
        }

        return false;
    }

    private void OnDisable()
    {
        isFirstOpen = true;

        contentsInfoPopup.SetActive(false);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);
    }
}
