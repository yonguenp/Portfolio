using System.Collections.Generic;
using UnityEngine;

public class StringManager : Singleton<StringManager>
{
    Dictionary<string, string> cacheStrings = new Dictionary<string, string>();

    public void CacheClear()
    {
        cacheStrings.Clear();
    }

    public void RefreshLanguage()
    {
        ChangeLanguage(GameConfig.Instance.OPTION_LANGUAGE);
    }

    public void ChangeLanguage(SystemLanguage lang)
    {
        if (GameConfig.Instance.OPTION_LANGUAGE == lang)
            return;

        cacheStrings.Clear();
        GameConfig.Instance.OPTION_LANGUAGE = lang;

        BaseScene scene = Managers.Scene.CurrentScene;
        if (scene != null)
        {
            foreach (TextLocalize tl in scene.GetComponentsInChildren<TextLocalize>())
            {
                tl.SetText();
            }
        }

        foreach (TextLocalize tl in PopupCanvas.Instance.GetComponentsInChildren<TextLocalize>())
        {
            tl.SetText();
        }
    }

    public string GetName(GameDataManager.DATA_TYPE tableType, int uid)
    {
        return GetString(tableType.ToString(), "name", uid);
    }
    public string GetDesc(GameDataManager.DATA_TYPE tableType, int uid)
    {
        return GetString(tableType.ToString(), "desc", uid);
    }

    public string GetString(string tableName, string col, int uid)
    {
        var key = $"{tableName}:{col}:{uid}";

        return GetString(key);
    }

    public static string GetString(string _key, params object[] obj)
    {
        string key = _key.ToLower();

        if (Instance == null)
            return _key;

        if (Instance.cacheStrings.ContainsKey(key))
        {
            try
            {
                return string.Format(Instance.cacheStrings[key], obj);
            }
            catch
            {
                return "[Error] Please check the string format : " + key;
            }
        }

        StringsGameData row = StringsGameData.GetStringData(key);
        if (row == null)
        {
            return _key;
        }

        string ret = key;
        switch (GameConfig.Instance.OPTION_LANGUAGE)
        {
            case SystemLanguage.Korean:
                ret = row.korean;
                break;
            case SystemLanguage.Japanese:
                ret = row.japanese;
                break;
            default:
                ret = row.english;
                break;
        }

        ret = ret.Replace('^', ',');
        Instance.cacheStrings[key] = ret;

        try
        {
            return string.Format(ret, obj);
        }
        catch
        {
            return "[Error] Please check the string format : " + key;
        }
    }
}
