using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDataManager
{
    public void Clear()
    {
        foreach(var data in EventScheduleData.GetAll())
        {
            data.EventBaseData?.Clear();
        }
    }

    public List<EventScheduleData> GetActiveEvents(bool isUITime)
    {
        return EventScheduleData.GetActiveEvents(isUITime);
    }

    /// <summary>
    /// 이벤트 출석체크 기간인지? // inUse param 추가
    /// </summary>
    /// <returns></returns>
    public bool IsEventPeriod(EventScheduleData target, bool _isUITime = true)
    {
        if (target == null)
            return false;

        var isPeriod = target.IsEventPeriod(_isUITime);
        if (isPeriod)
            return true;
        else
            return false;
    }
    public int GetRemainTime(EventScheduleData target, bool _isUITime = true)
    {
        if (!IsEventPeriod(target, _isUITime))
            return 0;

        var endTime = _isUITime ? target.UI_END_TIME : target.END_TIME;
        return (int)(endTime - TimeManager.GetDateTime()).TotalSeconds;
    }

    public bool IsNeedUpdate(EventScheduleData target)
    {
        if (target == null)
            return false;

        if (target.EventBaseData == null)
            return false;

        return target.EventBaseData.IsNeedUpdate();
    }

    public void SetData(EventScheduleData target, JObject jsonData)
    {
        if (target == null)
            return;

        if (target.EventBaseData == null)
            return;

        target.EventBaseData.SetData(jsonData);
    }
}
