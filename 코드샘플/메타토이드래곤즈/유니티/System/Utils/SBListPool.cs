using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBListPool<T> where T : new()
    {
        public delegate void SBListPoolReuse(T item);
        public delegate void SBListPoolUnuse(T item);
        public SBListPoolReuse Reuse { get; protected set; } = null;
        public SBListPoolUnuse Unuse { get; protected set; } = null;
        public List<T> datas { get; private set; } = null;

        public SBListPool()
        {
            Initialize();
        }
        public SBListPool(SBListPoolReuse reuse, SBListPoolUnuse unuse)
        {
            Initialize();
            Reuse = reuse;
            Unuse = unuse;
        }
        /// <summary>
        /// 현재 Pool에 확보된 수량
        /// </summary>
        public int Count
        {
            get
            {
                if (datas == null)
                    return 0;
                return datas.Count;
            }
        }
        protected void Initialize()
        {
            Reuse = null;
            Unuse = null;
            Clear();
        }
        /// <summary>
        /// Pool에 사용완료한 아이템 넣기
        /// </summary>
        /// <param name="item">T</param>
        public void Put(T item)
        {
            if (datas == null)
                datas = new List<T>();

            if (!datas.Contains(item))
            {
                Unuse?.Invoke(item);
                datas.Add(item);
            }
        }
        /// <summary>
        /// Pool 정리
        /// </summary>
        public void Clear()
        {
            if (datas == null)
                datas = new List<T>();
            else
                datas.Clear();
        }
        /// <summary>
        /// 풀에 확보된 내용중 첫번째 가져오기
        /// </summary>
        /// <returns>확보된 T</returns>
        public T Get()
        {
            var last = Count - 1;
            if (last < 0)
                return default;

            // Pop the last object in pool
            var item = datas[last];
            datas.RemoveAt(last);

            Reuse?.Invoke(item);

            return item;
        }
        /// <summary>
        /// T 필요 확보 수량 만큼 만들기
        /// </summary>
        /// <param name="count">확보 수량</param>
        public void Spawn(int count)
        {
            while (Count < count)
            {
                Put(new T());
            }
        }
        public void SetReuse(SBListPoolReuse reuse)
        {
            Reuse = reuse;
        }
        public void SetUnuse(SBListPoolUnuse unuse)
        {
            Unuse = unuse;
        }
    }
}