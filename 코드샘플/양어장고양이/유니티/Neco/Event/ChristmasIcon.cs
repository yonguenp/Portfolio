using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChristmasIcon : MonoBehaviour
{
    public void OnClick()
    {
        christmas_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHRISTMAS)
                eventData = (christmas_event)evt;
        }

        if (eventData != null)
        {
            if (!eventData.IsEventTime())
            {
                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("이벤트종료안내"), ()=> Destroy(gameObject));                
                return;
            }
        }

        NecoCanvas.GetPopupCanvas().OnChristmasEventPopupShow();
    }
}
