using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public interface ITableData
    {
        void Init();
        string GetKey();
    }
    public class TableData : ITableData
    {
        protected string UNIQUE_KEY { get; private set; }

        protected Dictionary<string, string> data = new Dictionary<string, string>();
        protected static Dictionary<Type, List<string>> data_keys = new Dictionary<Type, List<string>>();

        public TableData(JArray datas)
        {
            SetData(datas);
        }

        public virtual void Init() { }
        public string GetKey() { return UNIQUE_KEY; }
        public static void SetDataKeys(Type type, JArray key)
        {
            List<string> array = new List<string>();

            foreach (JToken k in key)
            {
                array.Add(k.Value<string>());
            }

            data_keys[type] = array;
        }

        public virtual void SetData(JArray datas)
        {
            if (data_keys == null || !data_keys.ContainsKey(GetType()) || data_keys[GetType()] == null)
            {
#if UNITY_EDITOR
                Debug.LogError("not found data keys");
#endif
                return;
            }

            var key = data_keys[GetType()];
            int len = Mathf.Min(key.Count, datas.Count);
            for (int i = 0; i < len; i++)
            {
                data[key[i]] = datas[i].Value<string>();
            }

            SetUniqueID();
        }

        void SetUniqueID()
        {
            if (!data.ContainsKey(GetUniqueKeyName()))
                return;

            try
            {
                if (string.IsNullOrEmpty(data[GetUniqueKeyName()]))
                    UNIQUE_KEY = "-1";
                else
                    UNIQUE_KEY = data[GetUniqueKeyName()];
            }
            catch
            {
                UNIQUE_KEY = data[GetUniqueKeyName()];
#if UNITY_EDITOR
                throw;
#endif
            }
        }

        protected virtual string GetUniqueKeyName()
        {
            return "KEY";
        }

        protected int Int(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;

            if (int.TryParse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int ret))
                return ret;

            return 0;
        }
        protected uint uInt(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;

            if (uint.TryParse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out uint ret))
                return ret;

            return 0;
        }

        protected float Float(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0.0f;

            if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ret))
                return ret;

            return 0.0f;
        }
        protected long Long(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;

            if (long.TryParse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long ret))
                return ret;

            return 0;
        }
        protected DateTime Date(string val)
        {
            if (string.IsNullOrEmpty(val))
                return DateTime.MinValue;

            if (DateTime.TryParse(val, out DateTime result))
                return result;
            else
                return DateTime.MaxValue;
        }
    }
    //수정
    public class TableData<T> : ITableData where T : DBData, new()
    {
        protected string UNIQUE_KEY => Data.UNIQUE_KEY;
        protected T Data { get; set; } = null;

        public virtual void Init() { }
        public string GetKey() { return UNIQUE_KEY; }

        public virtual void SetData(T data)
        {
            Data = data;
        }

        protected int Int(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;

            if (int.TryParse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int ret))
                return ret;

            return 0;
        }
        protected uint uInt(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;

            if (uint.TryParse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out uint ret))
                return ret;

            return 0;
        }

        protected float Float(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0.0f;

            if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ret))
                return ret;

            return 0.0f;
        }
        protected long Long(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;

            if (long.TryParse(val, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long ret))
                return ret;

            return 0;
        }
        protected DateTime Date(string val)
        {
            if (string.IsNullOrEmpty(val))
                return DateTime.MinValue;

            if (DateTime.TryParse(val, out DateTime result))
                return result;
            else
                return DateTime.MaxValue;
        }
    }
    //
}
