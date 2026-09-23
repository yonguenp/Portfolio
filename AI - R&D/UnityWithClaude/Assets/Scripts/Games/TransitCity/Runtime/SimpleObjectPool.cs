using System.Collections.Generic;
using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 자주 생성/소멸되는 오브젝트(장식용 차량 등)를 위한 최소한의 오브젝트 풀(§18).
    /// 프리팹은 씬/에셋에 미리 있는 것을 Instantiate만 한다 — new GameObject 없음.
    /// </summary>
    public class SimpleObjectPool<T> where T : Component
    {
        readonly T prefab;
        readonly Transform parent;
        readonly Stack<T> free = new Stack<T>();

        public SimpleObjectPool(T prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        public T Get()
        {
            T instance = free.Count > 0 ? free.Pop() : Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            free.Push(instance);
        }
    }
}
