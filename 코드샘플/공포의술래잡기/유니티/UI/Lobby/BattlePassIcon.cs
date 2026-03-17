using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassIcon : MonoBehaviour
{
    [SerializeField] Image imageGauge;
    [SerializeField] Text textTime;

    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }

    public void Refresh()
    {
        CancelInvoke("Refresh");
        var curData = Managers.UserData.seasonData;

        if (curData.seasonID <= 0)
        {
            SetActive(false);
            return;
        }

        PassGameData passData = Managers.Data.GetData(GameDataManager.DATA_TYPE.pass, curData.seasonID) as PassGameData;
        if(passData == null)
        {
            SetActive(false);
            return;
        }

        SetActive(true);

        List<PassItemGameData> curSeasonItems = new List<PassItemGameData>();
        foreach (PassItemGameData d in Managers.Data.GetData(GameDataManager.DATA_TYPE.pass_item))
        {
            if (d.group == passData.pass_item_group)
            {
                curSeasonItems.Add(d);
            }
        }
        curSeasonItems.Sort((a, b) => { return a.level.CompareTo(b.level); });

        int nextNeedExp = 0;
        int curExp = curData.exp;
        if (curData.level <= curSeasonItems.Count - 1)
        {
            int prevExp = 0;
            if (curData.level > 1)
            {
                prevExp = curSeasonItems[curData.level - 2].next_point;
            }

            curExp = curExp - prevExp;
            nextNeedExp = curSeasonItems[curData.level - 1].next_point - prevExp;
        }
        else
        {
            curExp = curExp - curSeasonItems[curSeasonItems.Count - 2].next_point;
            nextNeedExp = curSeasonItems[curSeasonItems.Count - 1].next_point;

            if(curExp > 0)
            {
                curExp %= nextNeedExp;
            }
        }

        imageGauge.fillAmount = (float)curExp / (nextNeedExp);
        string remainText = "";
        var diff = passData.end_time - SBCommonLib.SBUtil.KoreanTime;
        if (diff.Days >= 1.0f)
        {
            remainText = StringManager.GetString("ui_day", diff.Days.ToString()) + " " + StringManager.GetString("ui_hour", diff.Hours.ToString());

            Invoke("Refresh", diff.Minutes * 60.0f);
        }
        else if(diff.Hours >= 1.0f)
        {
            remainText = StringManager.GetString("ui_hour", diff.Hours.ToString()) + " " + StringManager.GetString("ui_min", diff.Minutes.ToString());

            Invoke("Refresh", diff.Seconds);
        }
        else
        {
            remainText = StringManager.GetString("ui_min", diff.Minutes.ToString());

            Invoke("Refresh", diff.Seconds);
        }

        textTime.text = StringManager.GetString("ui_left_time", remainText);
    }
}
