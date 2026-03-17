using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBTypePool<T1, T2> where T2 : Object, new()
    {
        private SBListPool<T2>.SBListPoolReuse Reuse { get; set; } = null;
        private SBListPool<T2>.SBListPoolUnuse Unuse { get; set; } = null;
        public SBTypePool(SBListPool<T2>.SBListPoolReuse reuse = null, SBListPool<T2>.SBListPoolUnuse unuse = null)
        {
            TypePool = new();
            ObjectDic = new();
            Reuse = reuse;
            Unuse = unuse;
        }
        private Transform Parent { get; set; } = null;
        private Dictionary<T1, T2> ObjectDic { get; set; } = null;
        private Dictionary<T1, SBListPool<T2>> TypePool { get; set; } = null;

        public void InitializeTransform(Transform parent)
        {
            Parent = parent;
        }
        public void InitializeType(T1 type, T2 obj)
        {
            if(false == TypePool.ContainsKey(type))
            {
                TypePool.Add(type, new(Reuse, Unuse));
            }

            if(ObjectDic.ContainsKey(type))
                ObjectDic[type] = obj;
            else
                ObjectDic.Add(type, obj);
        }
        public bool IsInitialize(T1 type)
        {
            return ObjectDic.ContainsKey(type);
        }
        public T2 Get(T1 type)
        {
            if (TypePool.TryGetValue(type, out var pool))
            {
                if (pool.Count < 1)
                    Spawn(type, 1);

                return pool.Get();
            }
            return null;
        }
        public bool Put(T1 type, T2 obj)
        {
            if (TypePool.TryGetValue(type, out var pool))
            {
                pool.Put(obj);
                return true;
            }
            return false;
        }
        private void Spawn(T1 type, int count)
        {
            if(ObjectDic.TryGetValue(type, out var obj))
            {
                if (TypePool.TryGetValue(type, out var pool))
                {
                    while (pool.Count < count)
                    {
                        if (Parent != null)
                            pool.Put(Object.Instantiate(obj, Parent));
                        else
                            pool.Put(Object.Instantiate(obj));
                    }
                }
            }
        }
        public void SetPoolDelegate(SBListPool<T2>.SBListPoolReuse reuse, SBListPool<T2>.SBListPoolUnuse unuse)
        {
            Reuse = reuse;
            Unuse = unuse;
            if (TypePool == null)
                return;

            var it = TypePool.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Value.SetReuse(Reuse);
                it.Current.Value.SetUnuse(Unuse);
            }
        }
        public void Destroy()
        {
            var it = TypePool.GetEnumerator();
            while (it.MoveNext())
            {
                var objPool = it.Current.Value;
                while (objPool.Count > 0)
                {
                    var obj = objPool.Get();
                    if (obj == null)
                        continue;

                    Object.Destroy(obj);
                }
                objPool.Clear();
            }
            TypePool.Clear();
            ObjectDic.Clear();
        }
    }
}