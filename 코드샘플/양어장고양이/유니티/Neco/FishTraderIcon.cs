using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishTraderIcon : MonoBehaviour
{
    public Text RemainText;

    private void OnEnable()
    {
        SetUI();
    }

    public void OnFishTrader()
    {
        //조건체크
        DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();
        DateTime startTime = neco_data.Instance.GetFishtruckDateTime(true);
        DateTime endTime = neco_data.Instance.GetFishtruckDateTime(false);

        if (curTime > startTime && curTime < endTime)
        {
            NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.FISH_TRADER_POPUP);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow("활어차시간만료");
            NecoCanvas.GetUICanvas().UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().CheckTimeSaleIcon();
        }
    }

    void SetUI()
    {
        CancelInvoke("SetUI");

        DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();
        DateTime endTime = neco_data.Instance.GetFishtruckDateTime(false);

        if (curTime <= endTime)
        {
            curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();

            TimeSpan diff = (endTime - curTime);

            SetFishTruckRemainTime(diff);
        }
        else
        {
            Destroy(gameObject);
            NecoCanvas.GetUICanvas().UIObject[(int)NecoUICanvas.UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().CheckTimeSaleIcon();
            return;
        }

        Invoke("SetUI", 1.0f);
    }

    void SetFishTruckRemainTime(TimeSpan remainTime)
    {
        string timeText = "";

        timeText += string.Format("{0:00}:{1:00}:{2:00}", remainTime.Hours, remainTime.Minutes, remainTime.Seconds);

        RemainText.text = timeText;
    }
}
