using SBCommonLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public DateTime endTime;
    public TimeSpan updateTime;
    public Text time;
    public IEnumerator coroutine = null;
    Action refresh_cb = null;

    private void OnEnable()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = null;

        if (this.endTime != null && this.endTime > DateTime.MinValue)
            InitTime(this.endTime, refresh_cb);
    }
    private void OnDisable()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }

    public void InitTime(DateTime endTime, Action action = null)
    {
        this.endTime = endTime;
        updateTime = endTime - SBUtil.KoreanTime;
        if (action != null)
            refresh_cb = action;

        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = UpdateTimeCo();
        StartCoroutine(coroutine);

    }

    IEnumerator UpdateTimeCo()
    {
        while (updateTime > TimeSpan.Zero)
        {
            var tempText = string.Empty;
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

            time.text = StringManager.GetString("ui_left_time", tempText);

            yield return new WaitForSeconds(1f);

            updateTime -= new TimeSpan(0, 0, 1);
        }

        refresh_cb?.Invoke();
    }
}
