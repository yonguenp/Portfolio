using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetLevelUpConstraintPopup : Popup<PopupData>
    {
        public Toggle toggle = null;

        public Text titleLabel = null;
        public Text bodyLabel = null;

        public delegate void func();

        private func okCallback = null;
        private func cancelCallback = null;
        private func xCallback = null;
        public override void InitUI() { }

        public void setMessage(string title, string body)
        {
            if (titleLabel != null)
            {
                titleLabel.text = title;
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = body;
            }
        }

    

        public void setCallback(func ok_cb, func cancel_cb, func x_cb)
        {
            if (ok_cb != null)
            {
                this.okCallback = ok_cb;
            }

            if (cancel_cb != null)
            {
                this.cancelCallback = cancel_cb;
            }

            if (x_cb != null)
            {
                this.xCallback = x_cb;
            }
        }

        public void okCB()
        {
            if (this.okCallback == null)
            {
                ClosePopup();
                return;
            }
            this.okCallback();
            ClosePopup();

        }

        public void cancelCB()
        {
            if (this.cancelCallback == null)
            {
                ClosePopup();
                return;
            }
            this.cancelCallback();
        }

        public void xCB()
        {
            if (this.xCallback == null)
            {
                ClosePopup();
                return;
            }
            this.xCallback();
        }
    }
}
