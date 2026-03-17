using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventIcon : MonoBehaviour
{
    [SerializeField] Text textTime;
    EventScheduleData data = null;

    List<string> users = new List<string>();
    public void SetData(EventScheduleData d)
    {
        data = d;
        Refresh();

        if(data.GetID() == 2003)//복주머니이벤트 하드코딩..
        {
            users.Clear();
            Invoke("GetLevel10Users", 15);
        }
    }

    public void Refresh()
    {
        CancelInvoke("Refresh");

        string remainText = "";
        var diff = data.ui_duration - SBCommonLib.SBUtil.KoreanTime;
        if (diff.Days >= 1.0f)
        {
            remainText = StringManager.GetString("ui_day", diff.Days.ToString()) + " " + StringManager.GetString("ui_hour", diff.Hours.ToString());

            Invoke("Refresh", diff.Minutes * 60.0f);
        }
        else if (diff.Hours >= 1.0f)
        {
            remainText = StringManager.GetString("ui_hour", diff.Hours.ToString()) + " " + StringManager.GetString("ui_min", diff.Minutes.ToString());

            Invoke("Refresh", diff.Minutes * 60.0f);
        }
        else
        {
            remainText = StringManager.GetString("ui_min", diff.Minutes.ToString());

            Invoke("Refresh", diff.Seconds);
        }

        textTime.text = StringManager.GetString("ui_left_time", remainText);
    }

    public void GetLevel10Users()
    {
        CancelInvoke("GetLevel10Users");

        SBWeb.SendPost("event/newyear23_maxuser", null, (response) =>
        {
            JToken res = SBWeb.GetResultData(response);
            if (res != null)
            {
                users.Clear();
                if (res.Type == JTokenType.Array)
                {
                    foreach(JObject row in (JArray)res)
                    {
                        if (row == null)
                            continue;

                        if(row.ContainsKey("name"))
                        {
                            string name = row["name"].Value<string>();
                            if (!string.IsNullOrEmpty(name))
                                users.Add(name);
                        }
                    }

                    ShowLevel10Users();
                    return;
                }
                Invoke("GetLevel10Users", 30);
            }
        });
    }

    public void ShowLevel10Users()
    {
        CancelInvoke("ShowLevel10Users");

        if (users.Count == 0)
        {
            Invoke("GetLevel10Users", 30);
            return;
        }

        PopupCanvas.Instance.ShowServerNotifyTextWithTime("복주머니칭찬해", GameConfig.Instance.NEWYEAR_NOTI_TIME, users[0]);

        users.RemoveAt(0);

        Invoke("ShowLevel10Users", 15);
    }
}
