using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class LanguageData : TableData<DBLanguage>
    {
        static private LanguageTable table = null;

        static public LanguageData Get(SystemLanguage lang)
        {
            if (table == null)
                table = TableManager.GetTable<LanguageTable>();

            if (table == null || !table.IsInit)
                return null;

            return table.Get((int)lang);
        }
        static public LanguageData CurData
        {
            get
            {
                var ret = Get(SBGameManager.Instance.GamePrefData.GameLanguage);
                if (ret == null || ret.USE == false)
                    ret = LanguageData.Get(SystemLanguage.English);

                return ret;
            }
        }

        static public string LanguageFolder
        {
            get
            {
                if (CurData != null)
                {
                    return CurData.RESOURCE;
                }

                return "en";
            }
        }

        public SystemLanguage Lang { get { return (SystemLanguage)Int(UNIQUE_KEY); } }
        public string LANGUAGE_SHEET => Data.LANGUAGE_SHEET;
        public string MAIL_STRING => Data.MAIL_STRING;
        public string SCRIPT_STRING => Data.SCRIPT_STRING;
        public string RESOURCE => Data.RESOURCE;
        public bool USE { get { return Data.USE > 0; } }
        public bool BETA { get { return Data.USE > 1; } }
        public string NAME => Data.NAME;

        public string SURPPORT_URL => Data.SURPPORT_URL;
        public string GAME_GUIDE_URL => Data.GAME_GUIDE_URL;
        public string SERVICE_TERMS => Data.SERVICE_TERMS;
        public string INFO_TERMS => Data.INFO_TERMS;
    }

    public class StringData : TableData<DBString>
    {
        static private StringDataManager table = null;

        static public bool HasStringKey(string key)
        {
            if (table == null)
                table = StringDataManager.Instance;

            return table.HasStringKey(key);
        }
        static public StringData Get(int index)
        {
            if (table == null)
                table = StringDataManager.Instance;

            return table.Get(index);
        }

        static public StringData Get(string key)
        {
            if (table == null)
                table = StringDataManager.Instance;

            return table.GetData(key);
        }

        static public string GetStringByIndex(int key)
        {
            if (table == null)
                table = StringDataManager.Instance;

            return table.GetStringByIndex(key);
        }

        static public string GetStringFormatByIndex(int key, params object[] value)
        {
            if (table == null)
                table = StringDataManager.Instance;

            var data = table.Get(key);
            if (data == null)
                return SBFunc.StrBuilder(value);

            return string.Format(data.TEXT, value);
        }

        static public string GetStringByStrKey(string key)
        {
            if (table == null)
                table = StringDataManager.Instance;

            return table.GetStringByStrKey(key);
        }

        static public string GetStringFormatByStrKey(string key, params object[] value)
        {
            if (table == null)
                table = StringDataManager.Instance;


            var data = table.GetData(key);
            if (data == null)
                return SBFunc.StrBuilder(value);

            return string.Format(data.TEXT, value);
        }

        static public bool IsContainStrKey(string key)
        {
            if (table == null)
                table = StringDataManager.Instance;

            return table.GetData(key) != null;
        }

        static public void Clear()
        {
            if (table == null)
                table = StringDataManager.Instance;

            table.Clear();
        }

        public int KEY { get; private set; } = -1;
        public string STR_KEY { get; private set; } = "";
        public string TEXT { get; private set; } = "";

        public StringData()
        {
        }
        public StringData(int key, string str_key, string text)
        {
            KEY = key;
            STR_KEY = str_key;
            TEXT = SBFunc.Replace(text);
        }
        public override void SetData(DBString data)
        {
            base.SetData(data);
            KEY = Int(Data.UNIQUE_KEY);
            STR_KEY = Data.STR_KEY;
            TEXT = SBFunc.Replace(Data.TEXT);
        }
    }
}

