using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetOptionRerollConstraintPopup : Popup<PopupData>
    {
        public Toggle toggle = null;

        public delegate void func();

        private func okCallback = null;
        private func cancelCallback = null;
        private func xCallback = null;
        public override void InitUI() 
        {
            
        }

        public void setMessage(string title, string body)
        {
            var titleObj = SBFunc.GetChildrensByName(transform, new string[] { "body", "Top_bg", "labelPopupTitle" });
            if(titleObj != null)
            {
                titleObj.GetComponent<Text>().text = title;
            }

            var bodyObj = SBFunc.GetChildrensByName(transform, new string[] { "body", "labelBody" });
            if(bodyObj != null)
            {
                bodyObj.GetComponent<Text>().text = body;
            }
        }

        public void setCallback(func ok_cb , func cancel_cb,func x_cb )
        {
            if(ok_cb != null)
            {
                okCallback = ok_cb;
            }

            if(cancel_cb != null)
            {
                cancelCallback = cancel_cb;
            }

            if(x_cb != null)
            {
                xCallback = x_cb;
            }
        }

        public void okCB()
        {
            if (okCallback == null)
            {
                ClosePopup();
                return;
            }
            okCallback();
        }

        public void cancelCB()
        {
            if (cancelCallback == null)
            {
                ClosePopup();
                return;
            }
            cancelCallback();
        }

        public void xCB()
        {
            if (xCallback == null)
            {
                ClosePopup();
                return;
            }
            xCallback();
        }
    }
}
