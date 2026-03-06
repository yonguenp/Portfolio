using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork { 

    public class SystemPreopenPopup : SystemPopup
    {
        static private SystemPreopenPopup instance = null;
        static public SystemPreopenPopup Instance { get { return instance; } }

        public void InitInstance()
        {
            instance = this;
            ClosePopup();
        }

        public void ClearInstance()
        {
            ClosePopup();
            if (instance == this)
                instance = null;
        }
        private void OnDestroy()
        {
            ClearInstance();
        }

        public void InitPopup()
        {
            gameObject.SetActive(true);
        }
        public override void SetMessage(string title = "", string body = "", string yes = "", string no = "")
        {
            InitPopup();
            base.SetMessage(title,body,yes,no);
        }
        public override void ClickOkCall()
        {
            if (okCall == null)
            {   
                ClosePopup();
                return;
            }
            okCall();
        }
        public override void ClickCancelCall()
        {
            if (cancelCall == null)
            {
                ClosePopup();
                return;
            }
            cancelCall();
        }
        public override void ClickExitCall()
        {
            if (exitCall == null)
            {
                ClosePopup();
                return;
            }
            exitCall();
        }
        public override void ClosePopup()
        {
            gameObject.SetActive(false);
        }
        public override void BackButton()
        {
            
        }
        public override void OnClickDimd()
        {

        }

        public bool IsOpening()
        {
            return gameObject.activeInHierarchy;
        }
    }
}