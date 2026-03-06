using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ToastManager
    {
        public struct ToastData
        {
            public string Str;
            public bool IsSystemToast;
            public float LifeTime;
            public float DiffY;
            public bool IsEffect;
            public string detailStr;

            public ToastData(string _Str, bool _IsSystemToast, float _Time, float _DiffY, bool isEffect = false , string _detailStr = "")
            {
                Str = _Str;
                IsSystemToast = _IsSystemToast;
                LifeTime = _Time;
                DiffY = _DiffY;
                IsEffect = isEffect;
                detailStr = _detailStr;
            }
        }

        private static ToastManager instance;
        public static ToastManager Instance
        {
            get 
            {
                if (instance == null)
                    instance = new ToastManager();
                return instance;
            }
            set { instance = value; }
        }

        private SystemMessageBeacon beacon = null;
        private Transform toastRoot = null;
        private GameObject toastPrefab = null;

        private Coroutine curUICoroutine = null;
        private Queue<ToastData> toastQueue = new Queue<ToastData>();

        public void Init(SystemMessageBeacon parent, Transform root, GameObject prefab)
        {
            beacon = parent;
            toastRoot = root;
            toastPrefab = prefab;
        }

        public static void On(int stringFormatNo, params object[] data)
        {
            string formatStr = StringData.GetStringByIndex(stringFormatNo);
            if (formatStr == "")
                return;

            Instance.Set(string.Format(formatStr, data));
        }

        public static void OnCAComplete(string str)
        {
            Instance.Set(str, false, 3, 0, true);
        }

        public static void On(int stringNo)
        {
            string str = StringData.GetStringByIndex(stringNo);
            if (str == "")
                return;

            Instance.Set(str);
        }
        public static void On(string str, int lifetime = 3, int diffyLevel = 0)
        {
            Instance.Set(str, false, lifetime, diffyLevel);            
        }

        public static void OnSystem(string str, int lifetime = 3, int diffyLevel = 0)
        {
            Instance.Set(str, true, lifetime, diffyLevel);
        }

        public void Set(string str, bool isSystem = false, int lifetime = 3, int diffyLevel = 0, bool isEffect = false)
        {
            if (toastQueue.Count > 0 && toastQueue.Peek().Str == str)
            {
                return;
            }

            toastQueue.Enqueue(new ToastData(str, isSystem, lifetime, diffyLevel, isEffect));

            if (curUICoroutine != null)
            {
                return;
            }

            if(beacon == null)
            {
                Debug.Log("beacon is null");
                return;
            }
            curUICoroutine = beacon.StartCoroutine(Open());
        }

        private IEnumerator Open()
        {
            ToastMessage prevToast = null;
            while (toastQueue.TryDequeue(out ToastData curData))
            {
                var curToast = GameObject.Instantiate(toastPrefab, toastRoot);
                
                var toastMessage = curToast.GetComponent<ToastMessage>();
                toastMessage.SetData(curData.Str, curData.IsSystemToast, curData.LifeTime, curData.IsEffect, prevToast);
                prevToast = toastMessage;

                yield return SBDefine.GetWaitForSeconds(1.5f);
            }

            curUICoroutine = null;
            yield break;
        }
    }
}