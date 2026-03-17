using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Popup : MonoBehaviour
{
    [SerializeField]
    PopupCanvas.POPUP_TYPE curPopupType;

    [SerializeField]
    protected bool popupAnimation = false;
    [SerializeField]
    bool useBackground = false;

    public delegate void CloseCallback();
    protected CloseCallback closeCallback = null;

    //private void Awake()
    //{
    //    AttachPopupCanvas();
    //}

    public void AttachPopupCanvas()
    {
        PopupCanvas.Instance.AttachPopup(curPopupType, this);
    }

    public virtual void Open(CloseCallback cb = null)
    {
        closeCallback = cb;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        RefreshUI();

        if(popupAnimation)
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public virtual void Close()
    {
        CloseCallback cb = closeCallback;
        closeCallback = null;
        gameObject.SetActive(false);
        transform.SetAsFirstSibling();
        cb?.Invoke();

        PopupCanvas.Instance.OnClosedPopup(curPopupType);
        if(PopupCanvas.Instance.PopupEscList.Contains(this))
        {
            PopupCanvas.Instance.PopupEscList.Remove(this);
        }
    }

    public void CloseForce()
    {
        closeCallback = null;
        gameObject.SetActive(false);
        transform.SetAsFirstSibling();

        PopupCanvas.Instance.OnClosedPopup(curPopupType);

        PopupCanvas.Instance.OnClosedPopup(GetPopupType());
        if (PopupCanvas.Instance.PopupEscList.Contains(this))
        {
            PopupCanvas.Instance.PopupEscList.Remove(this);
        }
    }

    public PopupCanvas.POPUP_TYPE GetPopupType()
    {
        return curPopupType;
    }
    public bool UseBackground()
    {
        return useBackground;
    }

    public virtual bool IsOpening()
    {
        return gameObject.activeInHierarchy;
    }

    public virtual void RefreshUI()
    {

    }
}

