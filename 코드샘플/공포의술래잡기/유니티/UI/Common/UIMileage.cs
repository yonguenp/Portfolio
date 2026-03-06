using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMileage : MonoBehaviour, EventListener<NotifyEvent>
{
    [SerializeField]
    Text curMileage;

    private void OnEnable()
    {
        Refresh();
        this.EventStartListening();
    }

    private void OnDisable()
    {
        this.EventStopListening();
    }

    public void Refresh()
    {
        curMileage.text = Managers.UserData.MyMileage.ToString();
    }

    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_USER_INFO:
                {
                    Refresh();
                }
                break;
        }
    }
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    public void OnButton()
    {
        //if (!PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP))
        //    PopupCanvas.Instance.ShowShopPopup(ShopPopup.TabType.MILEAGUE);
    }
}
