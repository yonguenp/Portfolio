using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDia : MonoBehaviour, EventListener<NotifyEvent>
{
    [SerializeField]
    Text curDia;

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
        curDia.text = Managers.UserData.MyDia.ToString("N0");
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
}
