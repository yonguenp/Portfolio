using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public abstract class PopupBase : MonoBehaviour, IPopup
    {
        abstract public int GetOrder();
        abstract public void Init(PopupData data);
        abstract public void ForceUpdate(PopupData data = null);        
        abstract public void ClosePopup();
        abstract public void Close();
        abstract public void SetActive(bool v);

        abstract public void BackButton();
		abstract public void OnClickDimd();
        abstract public bool IsModeless();

        abstract public void SetExitCallback(Action callback, float delay = 0);
        abstract public bool HasExitCallback();
    }

    public abstract class Popup<T> : PopupBase where T : class
    {
        protected int order = 0;
        protected Animator popupActionAnim = null;
        protected bool isAnimation = false;
        protected bool backClose = true;
        protected bool dimClose = true;
        protected Coroutine openAnimation = null;

        public Action ExitCallback { get; private set; } = null;
        float callbackDelayTime = 0;

        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        protected T Data { get; private set; } = null;

        //상속 재구현 금지        
        public override void Init(PopupData data)
        {
            Init(data as T);
        }

        public override void ForceUpdate(PopupData data = null)
        {
            ForceUpdate(data as T);
        }

        //public void Init(object data)
        //{
        //    Init(data as T);
        //}
        //public void DataRefresh(object data)
        //{
        //    DataRefresh(data as T);
        //}


        public virtual void Init(T data)
        {
            Data = data;

            if (openAnimation != null)
                StopCoroutine(openAnimation);

            openAnimation = StartCoroutine(OpenAnimation());
        }
        public abstract void InitUI();
        public virtual void ForceUpdate(T data) 
        {
            DataRefresh(data);
        }

        public virtual void DataRefresh(T data)
        {
            if (data == null)
                return;

            Data = data;
        }

        protected virtual IEnumerator OpenAnimation()
        {
            isAnimation = true;

            InitUI();

            if (popupActionAnim == null)
            {
                popupActionAnim = GetComponent<Animator>();
            }

            if (popupActionAnim != null)
                popupActionAnim.Play("PopupOpen", 0);

            yield return SBDefine.GetWaitForSeconds(0.5f);

            isAnimation = false;
        }
        protected virtual IEnumerator CloseAnimation()
        {
            if (popupActionAnim == null)
            {
                popupActionAnim = GetComponent<Animator>();
            }

            if (popupActionAnim != null)
            {
                popupActionAnim.Play("PopupClose", 0);
                yield return new WaitUntil(() => popupActionAnim.GetCurrentAnimatorStateInfo(0).IsName("PopupClose"));
                yield return new WaitUntil(() => popupActionAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            }
            
            SetActive(false);

            RunExitCallback();

            yield break;
        }

        public void RunExitCallback()
        {
            if (callbackDelayTime > 0f)
                Invoke(nameof(InvokeExitCallback), callbackDelayTime);
            else
                InvokeExitCallback();
        }

        public override void SetExitCallback(Action callback, float delay = 0)
        {
            ExitCallback = callback;
            callbackDelayTime = delay;
        }

        public override void Close()
        {
            if (this != null && gameObject != null && gameObject.activeInHierarchy)
            {
                if (openAnimation != null)
                    StopCoroutine(openAnimation);

                openAnimation = StartCoroutine(CloseAnimation());
            }
        }
        public override int GetOrder()
        {
            return Order;
        }

        public override void ClosePopup() 
        {
            Data = null;
            PopupManager.RemovePopupList(this);
            Close();
        }

        public override void BackButton()
        {
            if (backClose && !isAnimation)
                ClosePopup();
        }
        public override void OnClickDimd()
        {
            if (dimClose && !isAnimation)
                ClosePopup();
        }

        public override void SetActive(bool state)
        {
            gameObject.SetActive(state);
        }
        public virtual void SetDimd(bool dimd)
        {
            dimClose = dimd;
        }
        public virtual void SetBackBtn(bool back)
        {
            backClose = back;
        }

        void InvokeExitCallback()
        {
            ExitCallback?.Invoke();
            ExitCallback = null;
        }

        public override bool IsModeless()
        {
            return true;
        }

        public override bool HasExitCallback()
        {
            return ExitCallback != null;
        }
    }
}
