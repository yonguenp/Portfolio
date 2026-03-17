using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ServerOptionTable : TableBase<ServerOptionData, DBServer_option>
    {
        public override void Preload()
        {
            //base.Preload();
            //LoadAll();
        }
    }

    public class ServerOptionData : TableData<DBServer_option>
    {
        static private ServerOptionTable table = null;
        //static public ServerOptionData Get(string key)
        //{
        //    if (table == null)
        //        table = TableManager.GetTable<ServerOptionTable>();

        //    return table.Get(key);
        //}

        static public ServerOptionData Get(string key)
        {
            //if (table == null)
            //    table = TableManager.GetTable<ServerOptionTable>();

            return null;
        }

        static public float GetFloat(string key, float defaultValue = 1.0f)
        {
            //if (table == null)
            //    table = TableManager.GetTable<ServerOptionTable>();

            //var data = table.Get(key);
            //if (data != null)
            //    return data.FloatValue;

            return defaultValue;
        }
        static public int GetInt(string key, int defaultValue = 0)
        {
            //if (table == null)
            //    table = TableManager.GetTable<ServerOptionTable>();

            //var data = table.Get(key);
            //if (data != null)
            //    return data.IntValue;

            return defaultValue;
        }

        static public JToken GetJsonValue(string optionkey, string valuekey, JToken defaultValue = null)
        {
            //var data = Get(optionkey);
            //if (data != null)
            //{
            //    return data.GetJsonValue(valuekey, defaultValue);
            //}

            return defaultValue;
        }

        static public string GetJsonValueStr(string optionkey, string valuekey, string defaultValue = "")
        {
            //var data = Get(optionkey);
            //if (data != null)
            //{
            //    return data.GetJsonValueStr(valuekey, defaultValue);
            //}

            return defaultValue;
        }

        static public int GetJsonValueInt(string optionkey, string valuekey, int defaultValue = 0)
        {
            //var data = Get(optionkey);
            //if (data != null)
            //{
            //    return data.GetJsonValueInt(valuekey, defaultValue);
            //}

            return defaultValue;
        }
        static public float GetJsonValueFloat(string optionkey, string valuekey, float defaultValue = 0.0f)
        {
            //var data = Get(optionkey);
            //if (data != null)
            //{
            //    return data.GetJsonValueFloat(valuekey, defaultValue);
            //}

            return defaultValue;
        }

        public JToken GetJsonValue(string valuekey, JToken defaultValue = null)
        {
            //if (JSON_VALUE.ContainsKey(valuekey))
            //    return JSON_VALUE[valuekey];

            return defaultValue;
        }

        public string GetJsonValueStr(string valuekey, string defaultValue = "")
        {
            //if (JSON_VALUE.ContainsKey(valuekey))
            //    return JSON_VALUE[valuekey].Value<string>();

            return defaultValue;
        }

        public int GetJsonValueInt(string valuekey, int defaultValue = 0)
        {
            //if (JSON_VALUE.ContainsKey(valuekey))
            //{
            //    if (JSON_VALUE[valuekey].Type == JTokenType.Integer)
            //        return JSON_VALUE[valuekey].Value<int>();

            //    if (JSON_VALUE[valuekey].Type == JTokenType.Float)
            //        return (int)JSON_VALUE[valuekey].Value<float>();
            //}

            return defaultValue;
        }
        public float GetJsonValueFloat(string valuekey, float defaultValue = 0.0f)
        {
            //if (JSON_VALUE.ContainsKey(valuekey))
            //{
            //    if(JSON_VALUE[valuekey].Type == JTokenType.Integer)
            //        return JSON_VALUE[valuekey].Value<int>();

            //    if (JSON_VALUE[valuekey].Type == JTokenType.Float)
            //        return JSON_VALUE[valuekey].Value<float>();
            //}
            return defaultValue;
        }

        public string KEY => Data.UNIQUE_KEY;
        public string VALUE => Data.VALUE;
        public float FloatValue => float.Parse(VALUE);
        public int IntValue => int.Parse(VALUE);

        private JObject json = null;
        public JObject JSON_VALUE
        {
            get
            {
                if (json == null)
                {
                    try
                    {
                        json = JObject.Parse(VALUE);
                    }
                    catch
                    {
                        json = new JObject();
                    }
                }

                return json;
            }
        }
    }
}