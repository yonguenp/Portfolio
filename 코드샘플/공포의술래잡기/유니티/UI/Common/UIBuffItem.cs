using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBuffItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    string title = "";
    string desc = "";

    public void SetInfo(string t, string d)
    {
        title = t;
        desc = d;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP))
        {
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);
            return;
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc))
            return;

        PopupCanvas.Instance.ShowTooltip(title, desc, transform.position);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);
    }

}
