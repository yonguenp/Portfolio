using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class game_config : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.GAME_CONFIG; }

    static public string GetConfig(string key)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.GAME_CONFIG);
        if (necoData == null)
        {
            return "";
        }

        foreach (game_config data in necoData)
        {
            if (data.GetKey() == key)
                return data.GetValue();
        }

        return "";
    }

    static public string GetConfigByKeyAndVersion(string key, string version)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.GAME_CONFIG);
        if (necoData == null)
        {
            return "";
        }

        foreach (game_config data in necoData)
        {
            if (data.GetKey() == key)
            {
                if (data.GetVersion() == version)
                {
                    return data.GetValue();
                }
            }
                
        }
        return "";
    }

    [NonSerialized]
    string version = "";
    public string GetVersion()
    {
        if (string.IsNullOrEmpty(version))
        {
            object obj;
            if (data.TryGetValue("version", out obj))
            {
                version = (string)obj;
            }
        }

        return version;
    }


    [NonSerialized]
    string key = "";
    public string GetKey()
    {
        if (string.IsNullOrEmpty(key))
        {
            object obj;
            if(data.TryGetValue("key", out obj))
            {
                key = (string)obj;
            }
        }

        return key;
    }

    [NonSerialized]
    string value = "";
    public string GetValue()
    {
        if (string.IsNullOrEmpty(value))
        {
            object obj;
            if (data.TryGetValue("value", out obj))
            {
                value = (string)obj;
            }
        }

        return value;
    }
}
