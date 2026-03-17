using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISoulstone : MonoBehaviour, EventListener<NotifyEvent>
{
    [SerializeField]
    int itemNo;
    [SerializeField]
    Text curAmount;

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
        curAmount.text = Managers.UserData.GetMyItemCount(itemNo).ToString("N0");

    }

    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_ITEM_INFO:
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
}
