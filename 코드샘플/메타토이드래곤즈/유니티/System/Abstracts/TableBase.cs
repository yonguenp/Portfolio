using Newtonsoft.Json.Linq;
using SandboxNetwork.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class TableBase<T> : ITableBase where T : class, ITableData
    {
        protected Dictionary<string, T> datas = null;
        public bool IsInit { get { return datas != null; } }
        public virtual void Init()
        {
            if (datas == null)
                datas = new Dictionary<string, T>();
            else
                datas.Clear();
        }
        public virtual void Preload() { }
        public abstract void SetTable(JArray jsonArray);
        public virtual void Localize() { }
        public virtual void DataClear()
        {
            if (datas == null)
                return;

            var it = datas.GetEnumerator();

            while (it.MoveNext())
            {
                it.Current.Value.Init();
            }

            datas.Clear();
        }

        public virtual T Get(object key)
        {
            return Get(key.ToString());
        }
        public virtual T Get(int key)
        {
            return Get(key.ToString());
        }
        public virtual T Get(string key)
        {
            if (ContainsKey(key))
            {
                return datas[key];
            }
            return null;
        }

        public List<T> GetAllList()
        {
            return datas.Values.ToList();
        }

        public Dictionary<string, T> GetAllDic()
        {
            return datas;
        }

        protected virtual bool Add(T data)
        {
            if (data == null)
            {
                Debug.LogError("!!!! Table Data is null !!!!");
                return false;
            }

            if (data.GetKey() == null)
            {
                Debug.LogError("!!!! Table Data Key is null !!!!");
                return false;
            }

            if (ContainsKey(data.GetKey()))
            {
                UnityEngine.Debug.LogError("SBTableBase Error : 중복 키 => " + data.GetKey());
                return false;
            }

            datas.Add(data.GetKey(), data);
            return true;
        }

        protected virtual bool Add(string key, T data)
        {
            if (ContainsKey(key))
            {
                UnityEngine.Debug.LogError("SBTableBase Error : 중복 키 => " + data.GetKey());
                return false;
            }

            datas.Add(key, data);
            return true;
        }

        protected virtual bool Remove(T data)
        {
            return Remove(data.GetKey());
        }

        protected virtual bool Remove(string _index)
        {
            if (ContainsKey(_index))
            {
                return datas.Remove(_index);
            }
            return false;
        }
        public bool ContainsKey(object key)
        {
            return ContainsKey(key.ToString());
        }
        public virtual bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return datas.ContainsKey(key);
        }
    }
    //수정
    public abstract class TableBase<T, T2> : ITableBase where T : TableData<T2>, new() where T2 : DBData, new()
    {
        protected Dictionary<string, T> datas = new();
        public bool IsInit { get { return datas != null; } }
        protected bool IsLoadAll { get; set; } = false;
        public virtual void Init()
        {
            datas.Clear();

            IsLoadAll = false;
        }
        public virtual void Preload() { DataClear(); }
        public virtual void DataClear()
        {
            var it = datas.GetEnumerator();

            while (it.MoveNext())
            {
                it.Current.Value.Init();
            }

            datas.Clear();
            IsLoadAll = false;
        }

        public virtual T Get(object key)
        {
            return Get(key.ToString());
        }
        public virtual T Get(int key)
        {
            return Get(key.ToString());
        }
        public virtual T Get(string key)
        {
            if (false == ContainsKey(key))
            {
                var data = Load(key);                
                if (data == null)
                    return null;

                Add(data);
            }
            return datas[key];
        }
        protected virtual bool Add(T data)
        {
            return datas.TryAdd(data.GetKey(), data);
        }
        protected virtual T Load(string key)
        {
            try
            {
                var DBData = DataBase.Select<T2>(key);

                if (DBData == null || string.IsNullOrEmpty(DBData.UNIQUE_KEY))
                    return null;

                var data = new T();
                data.SetData(DBData);
                return data;
            }
            catch
            {
                return null;
            }
        }
        protected virtual void LoadAll()
        {
            if (false == IsLoadAll || null == datas)
            {
                var tableData = DataBase.SelectAll<T2>();
                if (tableData == null)
                    return;

                try
                {
                    var array = tableData.ToArray();
                    for (int i = 0, count = array.Length; i < count; ++i)
                    {
                        var data = new T();
                        data.SetData(array[i]);
                        if (false == Add(data))
                        {
#if DEBUG
                            Debug.Log("TableName => " + nameof(T2) + ", Duplication Key => " + data.GetKey());
#else
                        datas[data.GetKey()] = data;
#endif
                            continue;
                        }
                    }
                }
                catch
                {
                    Debug.LogError(nameof(T2) + "시트 못 찾음");
                }

                IsLoadAll = true;
            }
        }
        public List<T> GetAllList()
        {
            LoadAll();
            return datas.Values.ToList();
        }
        public Dictionary<string, T> GetAllDic()
        {
            LoadAll();
            return datas;
        }
        public bool ContainsKey(object key)
        {
            return ContainsKey(key.ToString());
        }
        public virtual bool ContainsKey(string key)
        {
            if (datas == null)
                return false;

            if (string.IsNullOrEmpty(key))
                return false;

            return datas.ContainsKey(key);
        }
    }
    //
}
