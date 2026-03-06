using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIManager : IManagerBase
    {
        private static UIManager instance = null;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UIManager();
                }
                return instance;
            }
        }
        MainUIObject mainUI = null;
        public MainUIObject MainUI
        {
            get {
                if (mainUI == null && beacon != null && beacon.UIObjects != null && beacon.UIObjects.Count > 0)
                {
                    mainUI = Beacon.UIObjects[0].GetComponent<MainUIObject>();
                }
                return mainUI;
            }
        }

        // LMenu
        MainPopupUIObject mainPopupUI = null;       
        public MainPopupUIObject MainPopupUI
        {
            get
            {
                if (mainPopupUI == null)
                {
                    mainPopupUI = Beacon.UIObjects[1].GetComponent<MainPopupUIObject>();
                }
                return mainPopupUI;
            }
        }

        UIEditTown uiEditTown = null;
        public UIEditTown UIEditTown
        {
            get
            {
                if (uiEditTown == null)
                {
                    uiEditTown = Beacon.UIObjects[8].GetComponent<UIEditTown>();
                }
                return uiEditTown;
            }
        }

        UIObject bannerGroup = null;

        public UIObject BannerGroup
        {
            get
            {
                if(bannerGroup == null)
                {
                    bannerGroup = Beacon.UIObjects[14].GetComponent<UIObject>();
                }
                return bannerGroup;
            }
        }
                   

        private UIBeacon beacon = null;
        public UIBeacon Beacon 
        {
            get 
            {
                return beacon;
            }
        }

        private Camera uiCamera = null;
        public Camera UICamera
        {
            get
            {
                if (uiCamera == null)
                {
                    var camera = GameObject.FindGameObjectWithTag("UICamera");
                    if (camera != null)
                        uiCamera = camera.GetComponent<Camera>();
                }
                return uiCamera;
            }
        }

        public Queue<eUIType> refreshQueue = new Queue<eUIType>();

        public eUIType CurrentUIType { get; private set; } = eUIType.None;

        public void Initialize()
        {
            if (Beacon != null && Beacon.UIObjects != null)
            {
                var count = Beacon.UIObjects.Count;
                for(var i = 0; i < count; ++i)
                {
                    var obj = Beacon.UIObjects[i];
                    if (obj == null)
                        continue;
                    obj.Init();
                }
            }
        }

        public void InitUI(eUIType type)
        {
            CurrentUIType = type;
            if (Beacon != null && Beacon.UIObjects != null)
            {
                var count = Beacon.UIObjects.Count;
                for (var i = 0; i < count; ++i)
                {
                    var obj = Beacon.UIObjects[i];
                    if (obj == null)
                        continue;
                    obj.InitUI(type);
                }
            }
        }

        public void RefreshUI()
        {
            if (Beacon != null && Beacon.UIObjects != null)
            {
                var count = Beacon.UIObjects.Count;
                for (var i = 0; i < count; ++i)
                {
                    var obj = Beacon.UIObjects[i];
                    if (obj == null)
                        continue;
                    obj.RefreshUI();
                }
            }
        }

        public void RefreshUI(eUIType type)
        {
            if (Beacon != null && Beacon.UIObjects != null)
            {
                var count = Beacon.UIObjects.Count;
                for (var i = 0; i < count; ++i)
                {
                    var obj = Beacon.UIObjects[i];
                    if (obj == null)
                        continue;
                    obj.RefreshUI(type);
                }
            }
            else
            {
                refreshQueue.Enqueue(type);
            }
        }

        public IEnumerator RefreshUICor(eUIType type)
        {
            if (Beacon != null && Beacon.UIObjects != null)
            {
                var count = Beacon.UIObjects.Count;
                for (var i = 0; i < count; ++i)
                {
                    var obj = Beacon.UIObjects[i];
                    if (obj == null)
                        continue;
                    obj.RefreshUI(type);
                    yield return null;
                }
            }
            else
            {
                refreshQueue.Enqueue(type);
            }
        }

        public void RefreshCurrentUI()
        {
            if (Beacon != null && Beacon.UIObjects != null)
            {
                var count = Beacon.UIObjects.Count;
                for (var i = 0; i < count; ++i)
                {
                    var obj = Beacon.UIObjects[i];
                    if (obj == null)
                        continue;
                    obj.RefreshUI(CurrentUIType);
                }
            }
        }

        public void Update(float dt) { }

        public void SetBeacon(UIBeacon beacon)
        {
            if (beacon == null)
                return;

            this.beacon = beacon;
            Initialize();
            InitUI(CurrentUIType);

            while(refreshQueue.Count > 0)
            {
                RefreshUI(refreshQueue.Dequeue());
            }
        }

        public static IEnumerator InitUICoroutine(eUIType type)
        {
            Instance.InitUI(type);
            yield break;
        }

        public static IEnumerator RefreshUICoroutine(eUIType type)
        {
            yield return Instance.RefreshUICor(type);
            //Instance.RefreshUI(type);
            //yield break;
        }
    }
}