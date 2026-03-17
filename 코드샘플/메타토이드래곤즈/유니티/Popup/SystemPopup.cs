using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class SystemPopup : Popup<PopupData>
    {
        [SerializeField]
        private Text titleText;
        [SerializeField]
        private Text bodyText;
        [SerializeField]
        private Text yesText;
        [SerializeField]
        private Text noText;
        [SerializeField]
        private GameObject yesBtn;
        [SerializeField]
        private GameObject noBtn;
        [SerializeField]
        private GameObject exitBtn;
        [SerializeField]
        private GameObject popupTitleObj;
        
        protected Action okCall= null;
        protected Action cancelCall = null;
        protected Action exitCall = null;

        public static SystemPopup OnSystemPopup(int type, string title, string body, string yes, string no, Action okCallBack = null, Action cancelCallBack = null, Action exitCallBack = null)
        {
            switch (type)
            {
                case 1:
                {
                    SystemPopup popup = SystemPreopenPopup.Instance;
                    if (popup != null)
                    {
                        popup.SetMessage(title, body, yes, no);
                        popup.SetCallBack(okCallBack, cancelCallBack, exitCallBack);

                        return popup;
                    }
                }
                break;
                default:
                case 0:
                    break;
            }

            return OnSystemPopup(title, body, yes, no, okCallBack, cancelCallBack, exitCallBack);
        }

        public static SystemPopup OnSystemPopup(string title, string body, string yes, string no, Action okCallBack = null, Action cancelCallBack = null, Action exitCallBack = null)
        {
            SystemPopup popup = SystemLoadingPopup.Instance;
            if (popup == null)
                popup = PopupManager.OpenPopup<SystemPopup>();

            popup.SetMessage(title, body, yes, no);
            popup.SetCallBack(okCallBack, cancelCallBack, exitCallBack);

            return popup;
        }

        public static SystemPopup OnSystemPopup(string title, string body, Action okCallBack = null, Action cancelCallBack = null, Action exitCallBack = null)
        {
            return OnSystemPopup(title, body, "", "", okCallBack, cancelCallBack, exitCallBack);
        }

        public static SystemPopup OnSystemPopup(string title, string body, Action okCallBack, Action cancelCallBack, Action exitCallBack, bool yesBtnOn, bool NoBtnOn, bool ExitBtnOn)
        {
            var popup = OnSystemPopup(title, body, "", "", okCallBack, cancelCallBack, exitCallBack);
            popup.SetButtonState(yesBtnOn, NoBtnOn, ExitBtnOn);
            return popup;
        }
        public static SystemPopup OnSystemPopup(string title, string body, bool yesBtnOn, bool NoBtnOn, bool ExitBtnOn)
        {
            var popup = OnSystemPopup(title, body, "", "");
            popup.SetButtonState(yesBtnOn, NoBtnOn, ExitBtnOn);
            return popup;
        }

        public void SetCallBack(Action okCallBack = null, Action cancelCallBack = null, Action exitCallBack = null) //callback 에 null 넣으면 ok 아닌 버튼들은 사라집니다
        {
            SetDimd(true);
            SetBackBtn(true);
            ClearCallback();

            yesBtn.SetActive(true);
            noBtn.SetActive(false);
            exitBtn.SetActive(false);
            if (okCallBack != null)
            {
                okCall = okCallBack;
            }
            if(cancelCallBack != null)
            {
                cancelCall = cancelCallBack;
                noBtn.SetActive(true);
            }
            if (exitCallBack != null)
            {
                exitCall = exitCallBack;
                exitBtn.SetActive(true);
            }
        }
        protected void ClearCallback()
        {
            okCall = null;
            cancelCall = null;
            exitCall = null;
        }
        public virtual void ClickOkCall()
        {
            Debug.Log("ok Call is called");
            if (okCall == null)
            {
                Debug.Log("ok Call back is null");
                ClosePopup();
                return;
            }

            ClosePopup();
            okCall();
        }
        public virtual void ClickCancelCall()
        {
            Debug.Log("cancel Call is called");
            if (cancelCall == null)
            {
                Debug.Log("cancel Call back is null");
                ClosePopup();
                return;
            }

            ClosePopup();
            cancelCall();
        }
        public virtual void ClickExitCall()
        {
            Debug.Log("exit Call is called");
            if (exitCall == null)
            {
                Debug.Log("exit Call back is null");
                ClosePopup();
                return;
            }

            ClosePopup();
            exitCall();            
        }
        public virtual void SetMessage(string title="", string body="", string yes="", string no="")
        {
            if (popupTitleObj != null) { 
                popupTitleObj.SetActive(title != "");
            }
            titleText.text = title;
            bodyText.text = body;
            if (yes != "")
            {
                yesText.text = yes;
            }
            else
            {
                yes = StringData.GetStringByIndex(100000199);
                if (yes != "")
                    yesText.text = yes;
            }
            if (no != "")
            {
                noText.text = no;
            }
            else
            {
                no = StringData.GetStringByIndex(100000200);
                if (no != "")
                    noText.text = no;
            }
        }
        public void SetButtonState(bool yesBtnOn, bool NoBtnOn, bool ExitBtnOn)
        {
            yesBtn.SetActive(yesBtnOn);
            noBtn.SetActive(NoBtnOn);
            exitBtn.SetActive(ExitBtnOn);
        }
        public override void InitUI()
        {
            noBtn.SetActive(false); 
            ClearCallback();
        }
		public override void BackButton()
        {
            exitCall?.Invoke();
            base.BackButton();
		}
        public override void OnClickDimd()
		{
            exitCall?.Invoke();
            base.OnClickDimd();
        }
		// Start is called before the first frame update

	}
}
