using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_pass : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_PASS; }

    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PASS);
        if (necoData == null)
        {
            return;
        }

        foreach (neco_pass data in necoData)
        {
            data.necoPassMainTitle = "";
            data.necoPassSubTitle = "";            
        }
    }

    static public neco_pass GetNecoPassData(uint season)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PASS);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_pass passData in necoData)
        {
            if (passData.data.TryGetValue("season", out obj))
            {
                if (season == (uint)obj)
                {
                    return passData;
                }
            }
        }

        return null;
    }

    [NonSerialized]
    private uint necoPassID = 0;
    public uint GetNecoPassID()
    {
        if (necoPassID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoPassID = (uint)obj;
            }
        }

        return necoPassID;
    }

    [NonSerialized]
    private string necoPassDesc = "";
    public string GetNecoPassDesc()
    {
        if (necoPassDesc == "")
        {
            object obj;
            if (data.TryGetValue("desc", out obj))
            {
                necoPassDesc = (string)obj;
            }
        }

        return necoPassDesc;
    }

    [NonSerialized]
    private uint necoPassSeason = 0;
    public uint GetNecoPassSeason()
    {
        if (necoPassSeason == 0)
        {
            object obj;
            if (data.TryGetValue("season", out obj))
            {
                necoPassSeason = (uint)obj;
            }
        }

        return necoPassSeason;
    }

    [NonSerialized]
    private string necoPassMainTitle = "";
    public string GetNecoPassMainTitle()
    {
        if (necoPassMainTitle == "")
        {
            necoPassMainTitle = LocalizeData.GetText("neco_pass:title_main");
        }

        return necoPassMainTitle;
    }

    [NonSerialized]
    private string necoPassSubTitle = "";
    public string GetNecoPassSubTitle()
    {
        if (necoPassSubTitle == "")
        {
            necoPassSubTitle = LocalizeData.GetText("neco_pass:title_sub");
        }

        return necoPassSubTitle;
    }

    [NonSerialized]
    private string necoPassResPath = "";
    public string GetNecoPassBgImageResource()
    {
        if (necoPassResPath == "")
        {
            object obj;
            if (data.TryGetValue("season_image", out obj))
            {
                necoPassResPath = (string)obj;
            }
        }

        return necoPassResPath;
    }

    [NonSerialized]
    Sprite necoPassBgIcon = null;
    public Sprite GetNecoPassBgIcon()
    {
        if (necoPassBgIcon == null)
        {
            object obj;
            if (data.TryGetValue("season_image", out obj))
            {
                necoPassBgIcon = Resources.Load<Sprite>((string)obj);
            }
        }

        return necoPassBgIcon;
    }

    [NonSerialized]
    private DateTime start_date = DateTime.MaxValue;
    [NonSerialized]
    private DateTime end_date = DateTime.MinValue;

    public DateTime GetStartDate()
    {
        if (start_date == DateTime.MaxValue)
        {
            object obj;
            if (data.TryGetValue("start_date", out obj))
            {
                start_date = DateTime.Parse((string)obj);
            }
        }

        return start_date;
    }

    public DateTime GetEndDate()
    {
        if (end_date == DateTime.MinValue)
        {
            object obj;
            if (data.TryGetValue("end_date", out obj))
            {
                end_date = DateTime.Parse((string)obj);
            }
        }

        return end_date;
    }

}
