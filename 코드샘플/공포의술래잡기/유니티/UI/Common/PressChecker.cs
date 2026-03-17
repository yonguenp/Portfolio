using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PressChecker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject target;
    public string enterFuctionName;
    public string exitFuctionName;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP))
        {
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);
            return;
        }

        target.SendMessage(enterFuctionName);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);

        target.SendMessage(exitFuctionName);
    }
}
