using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class LanguageTable : TableBase<LanguageData, DBLanguage>
    {

    }

    public class StringDataManager
    {
        private readonly bool StringAllLoaded = false;
        private static StringDataManager instance = null;
        public static StringDataManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new();
                return instance;
            }
        }
        #region String
        private Dictionary<int, StringData> intKeyDic = new Dictionary<int, StringData>();
        private Dictionary<string, StringData> strKeyDic = new Dictionary<string, StringData>();
        private Dictionary<string, MailStringData> mailDic = new Dictionary<string, MailStringData>();
        private Dictionary<string, ScriptStringData> scriptDic = new Dictionary<string, ScriptStringData>();
        public void Clear()
        {
            intKeyDic.Clear();
            strKeyDic.Clear();
            mailDic.Clear();
            scriptDic.Clear();

            if (StringAllLoaded)
            {
                IEnumerable<DBString> DBtable;
                switch (GamePreference.Instance.GameLanguage)
                {
                    case SystemLanguage.Korean:
                        DBtable = DataBase.SelectAll<DBString_kor>();
                        break;
                    case SystemLanguage.Japanese:
                        DBtable = DataBase.SelectAll<DBString_jpn>();
                        break;
                    case SystemLanguage.ChineseSimplified:
                        DBtable = DataBase.SelectAll<DBString_chs>();
                        break;
                    case SystemLanguage.ChineseTraditional:
                        DBtable = DataBase.SelectAll<DBString_cht>();
                        break;
                    case SystemLanguage.Portuguese:
                        DBtable = DataBase.SelectAll<DBString_prt>();
                        break;
                    default:
                        DBtable = DataBase.SelectAll<DBString_eng>();
                        break;
                }

                foreach (var dbData in DBtable)
                {
                    if (int.TryParse(dbData.UNIQUE_KEY, out int resultKey))
                    {
                        var data = new StringData(resultKey, dbData.STR_KEY, dbData.TEXT);
                        intKeyDic.Add(data.KEY, data);
                        if (!string.IsNullOrEmpty(data.STR_KEY) && false == strKeyDic.TryAdd(data.STR_KEY, data))
                        {
#if DEBUG
                            Debug.Log("STRING_DATA STR_KEY Duplication => " + data.STR_KEY);
#endif
                            strKeyDic[data.STR_KEY] = data;
                        }
                    }
                    else
                        Debug.LogError(">>>>>>STRING_DATA UNIQUE_KEY int parse Fail => " + dbData.UNIQUE_KEY);
                }
            }
        }
        public StringData Get(int key)
        {
            if (false == intKeyDic.TryGetValue(key, out var value))
            {
                value = new StringData();
                DBString dbData;
                switch (GamePreference.Instance.GameLanguage)
                {
                    case SystemLanguage.Korean:
                        dbData = DataBase.Select<DBString_kor>(key.ToString());
                        break;
                    case SystemLanguage.Japanese:
                        dbData = DataBase.Select<DBString_jpn>(key.ToString());
                        break;
                    case SystemLanguage.ChineseSimplified:
                        dbData = DataBase.Select<DBString_chs>(key.ToString());
                        break;
                    case SystemLanguage.ChineseTraditional:
                        dbData = DataBase.Select<DBString_cht>(key.ToString());
                        break;
                    case SystemLanguage.Portuguese:
                        dbData = DataBase.Select<DBString_prt>(key.ToString());
                        break;
                    default:
                        dbData = DataBase.Select<DBString_eng>(key.ToString());
                        break;
                }

                if (dbData == null)
                    return null;

                value.SetData(dbData);
                intKeyDic.Add(value.KEY, value);

                if (false == strKeyDic.ContainsKey(value.STR_KEY))
                    strKeyDic.Add(value.STR_KEY, value);
            }

            return value;
        }

        public StringData GetData(string key)
        {
            if (false == strKeyDic.TryGetValue(key, out var value))
            {
                value = new StringData();
                DBString dbData;
                switch (GamePreference.Instance.GameLanguage)
                {
                    case SystemLanguage.Korean:
                        dbData = DataBase.Select<DBString_kor>(data => data.STR_KEY == key);
                        break;
                    case SystemLanguage.Japanese:
                        dbData = DataBase.Select<DBString_jpn>(data => data.STR_KEY == key);
                        break;
                    case SystemLanguage.ChineseSimplified:
                        dbData = DataBase.Select<DBString_chs>(data => data.STR_KEY == key);
                        break;
                    case SystemLanguage.ChineseTraditional:
                        dbData = DataBase.Select<DBString_cht>(data => data.STR_KEY == key);
                        break;
                    case SystemLanguage.Portuguese:
                        dbData = DataBase.Select<DBString_prt>(data => data.STR_KEY == key);
                        break;
                    default:
                        dbData = DataBase.Select<DBString_eng>(data => data.STR_KEY == key);
                        break;
                }

                if (dbData == null)
                    return null;

                value.SetData(dbData);
                strKeyDic.Add(value.STR_KEY, value);

                if (false == intKeyDic.ContainsKey(value.KEY))
                    intKeyDic.Add(value.KEY, value);
            }

            return value;
        }
        public bool HasStringKey(string key)
        {
            return GetData(key) != null;
        }
        public string GetStringByIndex(int index)
        {
            StringData data = Get(index);
            if (data != null)
            {
                return data.TEXT;
            }

            return index.ToString();
        }
        public string GetStringByStrKey(string key)
        {
            StringData data = GetData(key);
            if (data != null)
            {
                return data.TEXT;
            }

            return key;
        }
        public bool ContainsKey(int key)
        {
            return intKeyDic.ContainsKey(key);
        }
        #endregion
        #region MailString        
        private MailStringData GetMail(string key)
        {
            if (false == mailDic.TryGetValue(key, out var value))
            {
                value = new MailStringData();
                DBMail_string dbData;
                switch (GamePreference.Instance.GameLanguage)
                {
                    case SystemLanguage.Korean:
                        dbData = DataBase.Select<DBMail_kor>(key);
                        break;
                    case SystemLanguage.Japanese:
                        dbData = DataBase.Select<DBMail_jpn>(key);
                        break;
                    case SystemLanguage.ChineseSimplified:
                        dbData = DataBase.Select<DBMail_chs>(key);
                        break;
                    case SystemLanguage.ChineseTraditional:
                        dbData = DataBase.Select<DBMail_cht>(key);
                        break;
                    case SystemLanguage.Portuguese:
                        dbData = DataBase.Select<DBMail_prt>(key);
                        break;
                    default:
                        dbData = DataBase.Select<DBMail_eng>(key);
                        break;
                }

                if (dbData == null)
                    return null;

                value.SetData(dbData);
                mailDic.Add(value.GetKey(), value);
            }

            return value;
        }
        public string GetMailStringByIndex(string index)
        {
            var data = GetMail(index);
            if (data != null)
                return data.TEXT;

            return index;
        }
        #endregion
        #region ScriptString        
        public ScriptStringData GetScript(string key)
        {
            if (false == scriptDic.TryGetValue(key, out var value))
            {
                value = new ScriptStringData();
                DBScript_string dbData;
                switch (GamePreference.Instance.GameLanguage)
                {
                    case SystemLanguage.Korean:
                        dbData = DataBase.Select<DBScript_kor>(key);
                        break;
                    case SystemLanguage.Japanese:
                        dbData = DataBase.Select<DBScript_jpn>(key);
                        break;
                    case SystemLanguage.ChineseSimplified:
                        dbData = DataBase.Select<DBScript_chs>(key);
                        break;
                    case SystemLanguage.ChineseTraditional:
                        dbData = DataBase.Select<DBScript_cht>(key);
                        break;
                    case SystemLanguage.Portuguese:
                        dbData = DataBase.Select<DBScript_prt>(key);
                        break;
                    default:
                        dbData = DataBase.Select<DBScript_eng>(key);
                        break;
                }

                if (dbData == null)
                    return null;

                value.SetData(dbData);
                scriptDic.Add(value.GetKey(), value);
            }

            return value;
        }
        #endregion
    }
}
