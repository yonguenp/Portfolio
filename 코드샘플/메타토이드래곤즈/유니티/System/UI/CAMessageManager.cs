using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CAMessageManager
    {
        private static CAMessageManager instance;
        public static CAMessageManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new CAMessageManager();
                return instance;
            }
            set { instance = value; }
        }

        private SystemMessageBeacon beacon = null;
        private Transform toastRoot = null;
        private GameObject toastPrefab = null;

        private Coroutine curUICoroutine = null;
        private Queue<ToastManager.ToastData> toastQueue = new Queue<ToastManager.ToastData>();

        public void Init(SystemMessageBeacon parent, Transform root, GameObject prefab)
        {
            beacon = parent;
            toastRoot = root;
            toastPrefab = prefab;
        }

        public static void OnCAComplete(string _title, string _detail)
        {
            Instance.Set(_title, _detail);
        }
        public void Set(string _title, string _detail)
        {
            toastQueue.Enqueue(new ToastManager.ToastData(_title, false, 1.5f, 0, false, _detail));

            if (curUICoroutine != null)
            {
                return;
            }

            curUICoroutine = beacon.StartCoroutine(Open());
        }
        private IEnumerator Open()
        {
            ToastMessage prevToast = null;
            while (toastQueue.TryDequeue(out ToastManager.ToastData curData))
            {
                var curToast = GameObject.Instantiate(toastPrefab, toastRoot);

                var toastMessage = curToast.GetComponent<ToastMessage>();
                toastMessage.SetData(curData.Str, curData.detailStr, curData.LifeTime, prevToast);
                prevToast = toastMessage;

                float time = 1.5f;
                while (time > 0.0f)
                {
                    if(!IsEnableOpen())
                    {
                        if(toastMessage != null)
                            toastMessage.Clear();

                        toastQueue.Clear();
                        curUICoroutine = null;                        
                        yield break;
                    }

                    time -= Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }

            curUICoroutine = null;
            yield break;
        }

        bool IsEnableOpen()//타운 씬에서만 나오게
        {
            if(SBFunc.IsTargetScene("Town"))
            {
                return !PopupManager.IsPopupOpening();
            }

            return false;
        }
    }

}

