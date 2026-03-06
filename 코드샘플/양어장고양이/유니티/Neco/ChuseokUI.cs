using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum CHUSEOK_CATEGORY
{
    MARBLE_GAME,
    PACKAGE_SHOP,
    SONGPYEON_SHOP,
    ATTENDANCE,
}

public class ChuseokUI : MonoBehaviour
{
    [Header("[TOP UI Info Layer]")]
    public Text eventTitleText;
    public Text eventPeriodGuideText;

    //[Header("[Common]")]

    [Header("[Chuseok Category Panel Layer]")]
    public ChuseokMarbleGamePanel marbleGamePanel;
    public ChuseokPackageShopPanel packageShopPanel;
    public ChuseokSongpyeonShopPanel songpyeonShopPanel;
    public ChuseokAttendancePanel attendancePanel;

    [Header("[Button Layer]")]
    public GameObject marbleGameButtonOn;
    public GameObject marbleGameButtonOff;
    public GameObject packageShopButtonOn;
    public GameObject packageShopButtonOff;
    public GameObject songpyeonShopButtonOn;
    public GameObject songpyeonShopButtonOff;
    public GameObject attendanceButtonOn;
    public GameObject attendanceButtonOff;
    
    bool isFirstOpen = true;    // 상점 탭 중복 입력 방지용

    CHUSEOK_CATEGORY curSelected = CHUSEOK_CATEGORY.MARBLE_GAME;
    System.Action CloseCallback = null;

    public GameObject helpPopup;
    public Text HelpText;

    public void OnClickOpenChuseokButton()
    {
        NecoCanvas.GetPopupCanvas().OnChuseokEventPopupShow();
    }

    public void OnClickMarbleGameButton()
    {
        if (!CheckDiceAnimation())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("고양이이동중토스트"));
            return;
        }

        if (marbleGamePanel == null) { return; }
        //if (isFirstOpen == false && curSelected == CHUSEOK_CATEGORY.MARBLE_GAME) { return; }
        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData == null)
        {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
            return;
        }

        if(!eventData.IsEnableMarble())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_469"), LocalizeData.GetText("이벤트종료안내"));
            return;
        }

        ClearUI();
        
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHUSEOK);
        data.AddField("op", 10);

        NetworkManager.GetInstance().SendApiRequest("event", 10, data, (response) =>
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
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("OnMarbleUI", 0.01f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("Event_Res_1"); break;
                                case 2: msg = LocalizeData.GetText("Event_Res_2"); break;
                                case 11: msg = LocalizeData.GetText("Event_Res_11"); break;
                                case 12: msg = LocalizeData.GetText("Event_Res_12"); break;
                                case 13: msg = LocalizeData.GetText("Event_Res_13"); break;
                                case 14: msg = LocalizeData.GetText("Event_Res_14"); break;
                                case 31: msg = LocalizeData.GetText("Event_Res_31"); break;
                                case 32: msg = LocalizeData.GetText("Event_Res_32"); break;
                                case 41: msg = LocalizeData.GetText("Event_Res_41"); break;
                                case 42: msg = LocalizeData.GetText("Event_Res_42"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), msg);
                        }
                    }
                }
            }
        });
    }

    public void OnMarbleUI()
    {
        UpdateUIState(CHUSEOK_CATEGORY.MARBLE_GAME);

        marbleGamePanel.gameObject.SetActive(true);
        marbleGamePanel.InitMarbleGamePanel(this);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);
    }

    public void OnClickPackageShopButton()
    {
        if (!CheckDiceAnimation())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("고양이이동중토스트"));
            return;
        }

        if (packageShopPanel == null) { return; }
        if (isFirstOpen == false && curSelected == CHUSEOK_CATEGORY.PACKAGE_SHOP) { return; }

        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData == null)
        {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
            return;
        }

        if (!eventData.IsEnablePackage())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_469"), LocalizeData.GetText("이벤트종료안내"));
            return;
        }

        ClearUI();

        UpdateUIState(CHUSEOK_CATEGORY.PACKAGE_SHOP);

        packageShopPanel.gameObject.SetActive(true);
        packageShopPanel.InitPakcageShopPanel(this);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);
    }

    public void OnClickSongpyeonShopButton()
    {
        if (!CheckDiceAnimation())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("고양이이동중토스트"));
            return;
        }

        if (songpyeonShopPanel == null) { return; }
        if (isFirstOpen == false && curSelected == CHUSEOK_CATEGORY.SONGPYEON_SHOP) { return; }

        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData == null)
        {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
            return;
        }

        if (!eventData.IsEnableShop())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_469"), LocalizeData.GetText("이벤트종료안내"));
            return;
        }

        ClearUI();

        UpdateUIState(CHUSEOK_CATEGORY.SONGPYEON_SHOP);

        songpyeonShopPanel.gameObject.SetActive(true);
        songpyeonShopPanel.InitSongpyeonShopPanel(this);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);
    }

    public void OnClickAttendanceButton()
    {
        if (!CheckDiceAnimation())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("고양이이동중토스트"));
            return;
        }

        if (attendancePanel == null) { return; }
        if (isFirstOpen == false && curSelected == CHUSEOK_CATEGORY.ATTENDANCE) { return; }

        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData == null)
        {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
            return;
        }

        if (!eventData.IsEnableAttendance())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_469"), LocalizeData.GetText("이벤트종료안내"));
            return;
        }

        ClearUI();

        UpdateUIState(CHUSEOK_CATEGORY.ATTENDANCE);

        attendancePanel.gameObject.SetActive(true);
        //attendancePanel.InitAttendancePanel(this);
    }

    public void OnClickCloseButton()
    {
        if (!CheckDiceAnimation())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("고양이이동중토스트"));
            return;
        }

        CloseCallback?.Invoke();
        NecoCanvas.GetPopupCanvas().OnPopupClose();
    }

    public bool CheckDiceAnimation()
    {
        if(curSelected == CHUSEOK_CATEGORY.MARBLE_GAME)
        {
            return marbleGamePanel.DiceRollButton;
        }

        return true;
    }

    public void InitChuseokPanel(CHUSEOK_CATEGORY category = CHUSEOK_CATEGORY.MARBLE_GAME, System.Action close_cb = null)
    {
        CloseCallback = close_cb;

        curSelected = category;

        helpPopup.SetActive(false);
        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData == null)
        {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
            return;
        }

        switch (curSelected)
        {
            case CHUSEOK_CATEGORY.MARBLE_GAME:
                if(!eventData.IsEnableMarble())
                {
                    if (eventData.IsEnableShop())
                    {
                        curSelected = CHUSEOK_CATEGORY.SONGPYEON_SHOP;
                    }
                    else
                    {
                        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
                        return;
                    }
                }
                else if(EnableAttendance())
                {
                    curSelected = CHUSEOK_CATEGORY.ATTENDANCE;
                }
                break;
            case CHUSEOK_CATEGORY.PACKAGE_SHOP:
                if (!eventData.IsEnablePackage())
                {
                    if (eventData.IsEnableShop())
                    {
                        curSelected = CHUSEOK_CATEGORY.SONGPYEON_SHOP;
                    }
                    else
                    {
                        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
                        return;
                    }
                }
                else if (EnableAttendance())
                {
                    curSelected = CHUSEOK_CATEGORY.ATTENDANCE;
                }
                break;
            case CHUSEOK_CATEGORY.SONGPYEON_SHOP:
                if (!eventData.IsEnableShop())
                {
                    if (eventData.IsEnableShop())
                    {
                        curSelected = CHUSEOK_CATEGORY.SONGPYEON_SHOP;
                    }
                    else
                    {
                        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
                        return;
                    }
                }
                else if (EnableAttendance())
                {
                    curSelected = CHUSEOK_CATEGORY.ATTENDANCE;
                }
                break;
            case CHUSEOK_CATEGORY.ATTENDANCE:
                if (!eventData.IsEnableAttendance())
                {
                    if (eventData.IsEnableShop())
                    {
                        curSelected = CHUSEOK_CATEGORY.SONGPYEON_SHOP;
                    }
                    else
                    {
                        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT);
                        return;
                    }
                }
                else if (EnableAttendance())
                {
                    curSelected = CHUSEOK_CATEGORY.ATTENDANCE;
                }
                break;
        }

        switch (curSelected)
        {
            case CHUSEOK_CATEGORY.MARBLE_GAME:
                OnClickMarbleGameButton();
                break;
            case CHUSEOK_CATEGORY.PACKAGE_SHOP:
                OnClickPackageShopButton();
                break;
            case CHUSEOK_CATEGORY.SONGPYEON_SHOP:
                OnClickSongpyeonShopButton();
                break;
            case CHUSEOK_CATEGORY.ATTENDANCE:
                OnClickAttendanceButton();
                break;
        }

        isFirstOpen = false;
    }

    public void RefreshLayer()
    {
        // 재화 갱신 타이밍으로 인한 딜레이 적용
        Invoke("RefreshCurrentPanel", 0.1f);
    }

    void RefreshCurrentPanel()
    {
        switch (curSelected)
        {
            case CHUSEOK_CATEGORY.MARBLE_GAME:
                break;
            case CHUSEOK_CATEGORY.PACKAGE_SHOP:
                break;
            case CHUSEOK_CATEGORY.SONGPYEON_SHOP:
                break;
            case CHUSEOK_CATEGORY.ATTENDANCE:
                break;
        }
    }

    void UpdateUIState(CHUSEOK_CATEGORY buttonState)
    {
        if (marbleGamePanel == null || packageShopPanel == null || songpyeonShopPanel == null || attendancePanel == null) { return; }
        if (marbleGameButtonOn == null || packageShopButtonOn == null || songpyeonShopButtonOn == null || attendanceButtonOn == null) { return; }
        if (marbleGameButtonOff == null || packageShopButtonOff == null || songpyeonShopButtonOff == null || attendanceButtonOff == null) { return; }

        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData == null)
        {
            return;
        }

        switch (buttonState)
        {
            case CHUSEOK_CATEGORY.MARBLE_GAME:
                marbleGameButtonOn.SetActive(true);
                packageShopButtonOff.SetActive(true);
                songpyeonShopButtonOff.SetActive(true);
                attendanceButtonOff.SetActive(true);

                eventTitleText.text = LocalizeData.GetText("추석냥이마블타이틀");
                
                eventPeriodGuideText.text = eventData.GetRealEventTimeString();   // 추후 추가 필요

                curSelected = CHUSEOK_CATEGORY.MARBLE_GAME;
                break;
            case CHUSEOK_CATEGORY.PACKAGE_SHOP:
                marbleGameButtonOff.SetActive(true);
                packageShopButtonOn.SetActive(true);
                songpyeonShopButtonOff.SetActive(true);
                attendanceButtonOff.SetActive(true);

                eventTitleText.text = LocalizeData.GetText("추석패키지상점타이틀");
                eventPeriodGuideText.text = eventData.GetRealEventTimeString();   // 추후 추가 필요

                curSelected = CHUSEOK_CATEGORY.PACKAGE_SHOP;
                break;
            case CHUSEOK_CATEGORY.SONGPYEON_SHOP:
                marbleGameButtonOff.SetActive(true);
                packageShopButtonOff.SetActive(true);
                songpyeonShopButtonOn.SetActive(true);
                attendanceButtonOff.SetActive(true);

                eventTitleText.text = LocalizeData.GetText("추석송편상점타이틀");
                eventPeriodGuideText.text = eventData.GetEventTimeString();   // 추후 추가 필요

                curSelected = CHUSEOK_CATEGORY.SONGPYEON_SHOP;
                break;
            case CHUSEOK_CATEGORY.ATTENDANCE:
                marbleGameButtonOff.SetActive(true);
                packageShopButtonOff.SetActive(true);
                songpyeonShopButtonOff.SetActive(true);
                attendanceButtonOn.SetActive(true);

                eventTitleText.text = LocalizeData.GetText("추석특별출석타이틀");
                eventPeriodGuideText.text = eventData.GetRealEventTimeString();   // 추후 추가 필요

                curSelected = CHUSEOK_CATEGORY.ATTENDANCE;
                break;
        }
    }

    void ClearUI()
    {
        // 기본 상태로 리셋
        marbleGameButtonOn.SetActive(false);
        marbleGameButtonOff.SetActive(false);
        packageShopButtonOn.SetActive(false);
        packageShopButtonOff.SetActive(false);
        songpyeonShopButtonOn.SetActive(false);
        songpyeonShopButtonOff.SetActive(false);
        attendanceButtonOn.SetActive(false);
        attendanceButtonOff.SetActive(false);

        marbleGamePanel.gameObject.SetActive(false);
        packageShopPanel.gameObject.SetActive(false);
        songpyeonShopPanel.gameObject.SetActive(false);
        attendancePanel.gameObject.SetActive(false);

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);
    }

    private void OnDisable()
    {
        isFirstOpen = true;

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);
    }

    public CHUSEOK_CATEGORY CurChuseockUICategory()
    {
        return curSelected;
    }

    public bool EnableAttendance()
    {
        bool result = false;

        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData != null)
        {
            if (eventData.IsEnableAttendance())
            {
                chuseok_event.chuseok_attendance attendanceData = eventData.GetAttendanceData();
                if (attendanceData != null)
                {
                    result = attendanceData.enableAttendance;
                }
            }
        }

        return result;
    }

    public void OnHelpButton()
    {
        if (!CheckDiceAnimation())
            return;

        helpPopup.SetActive(!helpPopup.activeSelf);
#if UNITY_IOS
        HelpText.text = LocalizeData.GetText("추석이벤트설명_FORIOS");
#else
        HelpText.text = LocalizeData.GetText("추석이벤트설명");
#endif
    }
}
