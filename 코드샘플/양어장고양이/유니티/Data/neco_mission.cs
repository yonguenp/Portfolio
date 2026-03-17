using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_mission : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_MISSION; }

    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MISSION);
        if (necoData == null)
        {
            return;
        }

        foreach (neco_mission data in necoData)
        {
            data.necoMissionNameKr = "";
        }
    }

    static public neco_mission GetNecoMissionData(uint missionID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MISSION);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_mission missionData in necoData)
        {
            if (missionData.data.TryGetValue("id", out obj))
            {
                if (missionID == (uint)obj)
                {
                    return missionData;
                }
            }
        }

        return null;
    }

    static public List<neco_mission> GetNecoMissionDataListByRepeatType(MISSION_TYPE repeat_type)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MISSION);
        if (necoData == null)
        {
            return null;
        }

        List<neco_mission> missionList = new List<neco_mission>();

        object obj;
        foreach (neco_mission missionData in necoData)
        {
            if(repeat_type == missionData.GetMissionType())
            {
                missionList.Add(missionData);
            }
        }

        return missionList;
    }

    static public List<neco_mission> GetNecoMissionDataListByTriggerType(MISSION_TYPE repeat_type, string trigger_type)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MISSION);
        if (necoData == null)
        {
            return null;
        }

        List<neco_mission> missionList = new List<neco_mission>();

        object obj;
        foreach (neco_mission missionData in necoData)
        {
            if (missionData.data.TryGetValue("repeat_type", out obj))
            {
                if (repeat_type == missionData.GetMissionType())
                {
                    if (missionData.data.TryGetValue("trigger_type", out obj))
                    {
                        if (trigger_type == (string)obj)
                        {
                            missionList.Add(missionData);
                        }
                    }
                }
            }
        }

        return missionList;
    }

    public enum MISSION_TYPE { 
        NONE,
        DAILY_MISSION,
        SEASON_MISSON
    };

    [NonSerialized]
    private uint necoMissionID = 0;
    public uint GetNecoMissionID()
    {
        if (necoMissionID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoMissionID = (uint)obj;
            }
        }

        return necoMissionID;
    }

    [NonSerialized]
    private string necoMissionDesc = "";
    public string GetNecoMissionDesc()
    {
        if (necoMissionDesc == "")
        {
            object obj;
            if (data.TryGetValue("desc", out obj))
            {
                necoMissionDesc = (string)obj;
            }
        }

        return necoMissionDesc;
    }

    [NonSerialized]
    private string necoMissionRepeatType = "";
    public string GetNecoMissionRepeatType()
    {
        if (necoMissionRepeatType == "")
        {
            object obj;
            if (data.TryGetValue("repeat_type", out obj))
            {
                necoMissionRepeatType = (string)obj;
            }
        }

        return necoMissionRepeatType;
    }

    [NonSerialized]
    private string necoMissionIcon = "";
    public string GetNecoMissionIcon()
    {
        if (necoMissionIcon == "")
        {
            object obj;
            if (data.TryGetValue("icon_resource", out obj))
            {
                necoMissionIcon = (string)obj;
            }
        }

        return necoMissionIcon;
    }

    [NonSerialized]
    private string necoMissionTriggerType = "";
    public string GetNecoMissionTriggerType()
    {
        if (necoMissionTriggerType == "")
        {
            object obj;
            if (data.TryGetValue("trigger_type", out obj))
            {
                necoMissionTriggerType = (string)obj;
            }
        }

        return necoMissionTriggerType;
    }

    [NonSerialized]
    private uint necoMissionCount = 0;
    public uint GetMissionMaxCount()
    {
        if (necoMissionCount == 0)
        {
            object obj;
            if (data.TryGetValue("count", out obj))
            {
                necoMissionCount = (uint)obj;
            }
        }

        return necoMissionCount;
    }

    [NonSerialized]
    private uint necoMissionExp = 0;
    public uint GetNecoMissionExp()
    {
        if (necoMissionExp == 0)
        {
            object obj;
            if (data.TryGetValue("exp", out obj))
            {
                necoMissionExp = (uint)obj;
            }
        }

        return necoMissionExp;
    }

    [NonSerialized]
    private string necoMissionNameKr = "";
    public string GetNecoMissionNameKr()
    {
        if (necoMissionNameKr == "")
        {
            necoMissionNameKr = LocalizeData.GetText("neco_mission:mission_name_kor:" + GetNecoMissionID().ToString());
        }

        return necoMissionNameKr;
    }

    [NonSerialized]
    private MISSION_TYPE mission_type = MISSION_TYPE.NONE;
    public MISSION_TYPE GetMissionType()
    {
        if(mission_type == MISSION_TYPE.NONE)
        {
            object obj;
            if (data.TryGetValue("repeat_type", out obj))
            {
                switch ((string)obj)
                {
                    case "DAILY":
                        mission_type = MISSION_TYPE.DAILY_MISSION;
                        break;
                    case "SEASON":
                        mission_type = MISSION_TYPE.SEASON_MISSON;
                        break;
                }
            }
        }
        return mission_type;
    }
}
