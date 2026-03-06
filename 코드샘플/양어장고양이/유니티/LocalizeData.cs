using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeData
{
    private static LocalizeData _instance = null;

    private Dictionary<string, string> LocalizeText = new Dictionary<string, string>();
    private SystemLanguage curLanguage = SystemLanguage.English;
    private string DATA_PATH = "Data/string";
    private string DownloadData = "";
    private bool _isLoadDone = false;
    public bool isLoadDone 
    { 
        get { return _isLoadDone; } 
        set { _isLoadDone = value; } 
    }

    public static LocalizeData instance
    {
        get {
            if (_instance == null)
            {
                _instance = new LocalizeData();
            }
            return _instance;
        }
    }

    public void SetDownloadData(string text, SystemLanguage lang)
    {
        DownloadData = text;
        SetLanguage(lang);
    }

    public void SetLanguage(SystemLanguage lang)
    {
        isLoadDone = false;
        curLanguage = lang;
        LoadData();
    }

    private void LoadData()
    {
        ClearPreLoadData();

        LocalizeText.Clear();
        LocalizeText = new Dictionary<string, string>();

        string DefaultLanguageKey = "en";
        string LanguageKey = "en";
        switch (curLanguage)
        {
            case SystemLanguage.Korean:
                LanguageKey = "ko";
                break;
            case SystemLanguage.Japanese:
                LanguageKey = "ja";
                break;
            case SystemLanguage.Indonesian:
                LanguageKey = "id";
                break;
            case SystemLanguage.English:
            default:
                LanguageKey = "en";
                break;
        }

        List<Dictionary<string, string>> data = null;
        if (string.IsNullOrEmpty(DownloadData))
        {
            if (string.IsNullOrEmpty(DATA_PATH))
            {
                //아직 준비가 되지 않았으므로
                return;
            }

            data = CSVReader.Read(DATA_PATH, '\t');
        }
        else
        {
            data = CSVReader.ReadData(DownloadData, '\t');
        }

        if (data == null)
            return;

        for (int i = 0; i < data.Count; i++)
        {
            try
            {
                string key = data[i]["key"].ToString();
                string value = data[i][LanguageKey].ToString();

                if (string.IsNullOrEmpty(value))
                    value = data[i][DefaultLanguageKey].ToString();

                value = value.Replace("\\n", "\n");

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    LocalizeText.Add(key, value);
            }
            catch(System.Exception e)
            {
                Debug.LogError("key : " + data[i]["key"]);
            }
        }

        
        isLoadDone = true;
    }

    public static bool HasText(string key)
    {
        return instance.LocalizeText.ContainsKey(key);
    }

    public static string GetText(string Key)
    {
#if UNITY_EDITOR
        if (_instance == null)
        {
            instance.SetLanguage(SystemLanguage.Korean);
        }
#endif

        if (instance.LocalizeText.ContainsKey(Key))
        {
            return instance.LocalizeText[Key];
        }

        return "";
    }

    public static bool isDataLoaded()
    {
        if (_instance != null)
            return instance.isLoadDone;

        return false;
    }


#if UNITY_EDITOR
    public static string GetKeyWithText(string val)
    {
        foreach (KeyValuePair<string, string> iter in instance.LocalizeText)
        {
            if (iter.Value == val)
                return iter.Key;
        }

        return "";
    }
#endif

    public void ClearPreLoadData()
    {
        neco_shop.ClearLocalizeData();
        clip_event.ClearLocalizeData();
        items.ClearLocalizeData();
        neco_cat.ClearLocalizeData();
        neco_cat_memory.ClearLocalizeData();
        neco_mission.ClearLocalizeData();
        neco_package.ClearLocalizeData();
        neco_pass.ClearLocalizeData();
        recipe.ClearLocalizeData();
    }

    public SystemLanguage CurLanguage()
    {
        return curLanguage;
    }
}