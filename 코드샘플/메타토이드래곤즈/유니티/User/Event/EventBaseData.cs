using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventBaseData
{
    protected EventScheduleData scheduleData = null;
    public EventBaseData(EventScheduleData eData)
    {
        scheduleData = eData;
    }
    public abstract void Clear();
    public abstract void SetData(JObject jsonData);
    public virtual string GetEventButtonString()
    {
        return scheduleData.GetEventString();
    }

    public virtual bool IsNeedUpdate()
    {
        return false;
    }

    public int GetScheduleDataKey()
    {
        if (scheduleData == null)
            return -1;
        return int.Parse(scheduleData.KEY);
    }

    public EventScheduleData GetScheduleData()
    {
        return scheduleData;
    }
}
