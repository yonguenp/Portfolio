using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopTitle : UITimer
{
    [SerializeField]
    Text Title;
    [SerializeField]
    GameObject TimeInfo;
    [SerializeField]
    GameObject DiaPanel;
    [SerializeField]
    GameObject AdvertisePanel;
    [SerializeField]
    Text DiaAmount;
    public void SetRefreshCount(int count)
    {
            DiaPanel.SetActive(true);
            AdvertisePanel.SetActive(false);

            DiaAmount.text = Mathf.Min(count * 10, GameConfig.Instance.RANDOM_SHOP_REFRESH_PRICE).ToString();
        //else
        //{
        //    DiaPanel.SetActive(false);
        //    AdvertisePanel.SetActive(true);
        //}
    }

    public void SetTitle(string title, bool useTimer = false)
    {
        //Title.text = title;
        TimeInfo.SetActive(useTimer);
    }

    public override void SetText(int Remain)
    {
        if (Remain < 0)
        {
            if (TimerText != null)
                TimerText.text = "-";
            return;
        }

        const int min = 60;
        const int hour = min * 60;
        const int day = hour * 24;

        string remainText = "";
        if (Remain > day)
        {
            int dayVal = (Remain / day);
            remainText += StringManager.GetString("ui_day", dayVal.ToString()) + " ";
            Remain -= dayVal * day;
        }
        if (Remain > hour)
        {
            int hourVal = (Remain / hour);
            remainText += StringManager.GetString("ui_hour", hourVal.ToString()) + " ";
            Remain -= hourVal * hour;
        }
        if (Remain > min)
        {
            int minVal = (Remain / min);
            remainText += StringManager.GetString("ui_min", minVal.ToString()) + " ";
            Remain -= minVal * min;
        }

        remainText += StringManager.GetString("ui_second", Remain.ToString());

        if (TimerText != null)
            TimerText.text = remainText;
    }
}
