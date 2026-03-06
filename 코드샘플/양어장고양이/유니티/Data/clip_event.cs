using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ClipUIInfo
{
    public enum UI_TYPE
    {
        TOUCH,
        PLAY,
        FISHING,
        FEED,
        PROGRESS,
    };

    public UI_TYPE type;
    public Vector2 position;
    public float startTime;
    public float expireTime;
}

public class ClipSciptInfo
{
    public string scriptText;
    public float startTime;
    public float expireTime;
}

[Serializable]
public class clip_event : game_data
{
    static public clip_event GetClipEvent(uint clipID)
    {
        List<game_data> clip = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
        foreach (clip_event data in clip)
        {
            if (data.GetEventID() == clipID)
            {
                return data;
            }
        }

        return null;
    }

    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
        if (necoData == null)
        {
            return;
        }

        foreach (clip_event data in necoData)
        {
            data.title = "";
            if (data.scriptList != null)
            {
                data.scriptList.Clear();
                data.scriptList = null;
            }
        }
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CLIP_EVENT; }
    public enum CLIP_EVENT_TYPE { 
        FINISH,
        TOUCH,        
        PLAY,
        FEED,
        FISH,
    };
    public enum CLIP_INTERACTION_TYPE
    {
        UNKNOWN = -1,
        NONE = 0,
        TOUCHABLE = 1,
    };

    public enum CLIP_ENTRY_TYPE
    { 
        UNKNOWN = -1,
        NONE = 0,
        ENTRY = 1,
    };


    [NonSerialized]
    private uint event_id = 0;
    [NonSerialized]
    private CLIP_EVENT_TYPE event_type = CLIP_EVENT_TYPE.FINISH;
    [NonSerialized]
    private string clip = "";
    [NonSerialized]
    private string title = "";
    [NonSerialized]
    private List<ClipSciptInfo> scriptList = null;
    [NonSerialized]
    private List<ClipUIInfo> uiInfoList = null;
    [NonSerialized]
    private CLIP_INTERACTION_TYPE clip_interaction_type = CLIP_INTERACTION_TYPE.UNKNOWN;
    [NonSerialized]
    private CLIP_ENTRY_TYPE entryType = CLIP_ENTRY_TYPE.NONE;
    [NonSerialized]
    private uint areaNo = 0;
    [NonSerialized]
    private uint stageNo = 0;
    [NonSerialized]
    private JArray rewardData = null;
    [NonSerialized]
    private JObject conditionData = null;
    [NonSerialized]
    private Dictionary<string, List<clip_event>> next_events = null;
    [NonSerialized]
    private string audio = "";
    public uint GetEventID()
    {
        if (event_id == 0)
        {
            object obj;
            if (data.TryGetValue("event_id", out obj))
            {
                event_id = (uint)obj;
            }
        }

        return event_id;
    }

    public CLIP_EVENT_TYPE GetEventType()
    {
        if (event_type == CLIP_EVENT_TYPE.FINISH)
        {
            object obj;
            if (data.TryGetValue("event_type", out obj))
            {
                event_type = (CLIP_EVENT_TYPE)((uint)obj);
            }
        }

        return event_type;
    }

    public string GetClipPath()
    {
        if (string.IsNullOrEmpty(clip))
        {
            object obj;
            if (data.TryGetValue("clip", out obj))
            {
                clip = (string)obj;
            }
        }

        return clip;
    }

    public string GetClipTitle()
    {
        if (title == "")
        {
            title = LocalizeData.GetText("clip_event:title:" + GetEventID().ToString());
        }

        return title;
    }

    public List<ClipSciptInfo> GetScriptList()
    {
        if (scriptList == null)
        {
            scriptList = new List<ClipSciptInfo>();
            string subtitle = LocalizeData.GetText("clip_event:subtitle:" + GetEventID().ToString());

            if (!string.IsNullOrEmpty(subtitle))
            {
                JArray array = JArray.Parse(subtitle);
                foreach (JToken data in array)
                {
                    if (data.Type == JTokenType.Array)
                    {
                        JArray scriptInfo = ((JArray)data);
                        ClipSciptInfo info = new ClipSciptInfo();
                        info.scriptText = scriptInfo[0].ToString();
                        info.startTime = scriptInfo[1].Value<float>();
                        info.expireTime = scriptInfo[2].Value<float>();
                        scriptList.Add(info);
                    }
                }
            }
        }

        return scriptList;
    }

    public List<ClipUIInfo> GetUIInfoList()
    {
        if (uiInfoList == null)
        {
            uiInfoList = new List<ClipUIInfo>();
            object obj;
            if (data.TryGetValue("on_buttons", out obj))
            {
                JArray array = JArray.Parse((string)obj);
                foreach (JToken data in array)
                {
                    JArray buttonInfo = ((JArray)data);
                    ClipUIInfo info = new ClipUIInfo();
                    switch(buttonInfo[0].ToString())
                    {
                        case "touch":
                            info.type = ClipUIInfo.UI_TYPE.TOUCH;
                            break;
                        case "play":
                            info.type = ClipUIInfo.UI_TYPE.PLAY;
                            break;
                        case "feed":
                            info.type = ClipUIInfo.UI_TYPE.FEED;
                            break;
                        case "progressbar":
                            info.type = ClipUIInfo.UI_TYPE.PROGRESS;
                            break;
                        case "fishing":
                            info.type = ClipUIInfo.UI_TYPE.FISHING;
                            break;
                    }
                    
                    info.startTime = buttonInfo[1].Value<float>();
                    info.expireTime = buttonInfo[2].Value<float>();

                    if(buttonInfo.Count > 4)
                    {
                        info.position = new Vector2(buttonInfo[3].Value<float>(), buttonInfo[4].Value<float>());
                    }

                    uiInfoList.Add(info);
                }
            }
        }

        return uiInfoList;
    }

    public CLIP_INTERACTION_TYPE GetInteractionType()
    {
        if (clip_interaction_type == CLIP_INTERACTION_TYPE.UNKNOWN)
        {
            object obj;
            if (data.TryGetValue("is_touchable", out obj))
            {
                clip_interaction_type = (CLIP_INTERACTION_TYPE)((uint)obj);
            }
        }

        return clip_interaction_type;
    }

    public CLIP_ENTRY_TYPE IsEntryClip()
    {
        if(entryType == CLIP_ENTRY_TYPE.UNKNOWN)
        {
            object obj;
            if (data.TryGetValue("is_entry", out obj))
            {
                entryType = (CLIP_ENTRY_TYPE)((uint)obj);
            }
        }

        return entryType;
    }

    public uint GetAreaNo()
    {
        if (areaNo == 0)
        {
            object obj;
            if (data.TryGetValue("area", out obj))
            {
                areaNo = (uint)obj;
            }
        }

        return areaNo;
    }
    public uint GetStageNo()
    {
        if (stageNo == 0)
        {
            object obj;
            if (data.TryGetValue("stage_id", out obj))
            {
                stageNo = (uint)obj;
            }
        }

        return stageNo;
    }

    public JArray GetRewardData()
    {
        if(rewardData == null)
        {
            object obj;
            if (data.TryGetValue("reward", out obj))
            {
                rewardData = JArray.Parse((string)obj);
            }
            else
            {
                rewardData = new JArray();
            }
        }

        return rewardData;
    }

    public JObject GetSuccessConditionData()
    {
        if (conditionData == null)
        {
            object obj;
            if (data.TryGetValue("success_condition", out obj))
            {
                string data = (string)obj;
                if (string.IsNullOrEmpty(data))
                    conditionData = new JObject();
                else
                    conditionData = JObject.Parse(data);
            }
        }

        return conditionData;
    }

    public List<clip_event> GetNextClipEvent(string param)
    {
        
        if (next_events == null)
        {
            next_events = new Dictionary<string, List<clip_event>>();
            object obj;
            if (data.TryGetValue("next_events", out obj))
            {
                string data = (string)obj;
                JObject jobj = JObject.Parse(data);
                foreach (JProperty contentProp in jobj.Properties())
                {
                    List<clip_event> eventArray = new List<clip_event>();
                    JArray Array = (JArray)jobj[contentProp.Name];
                    foreach (JToken clipID in Array)
                    {
                        uint id = clipID.Value<uint>();
                        clip_event clip = GetClipEvent(id);
                        
                        if(id == 0)
                        {
                            clip = new clip_event();
                        }

                        eventArray.Add(clip);
                    }
                    next_events[contentProp.Name] = eventArray;
                }
            }
        }

        List<clip_event> ret = null;
        if(next_events.ContainsKey(param))
        {
            ret = next_events[param];
        }

        return ret;
    }

    public string GetAudioPath()
    {
        if (string.IsNullOrEmpty(audio))
        {
            object obj;
            if (data.TryGetValue("audio", out obj))
            {
                audio = (string)obj;
            }
        }

        return audio;
    }
}

