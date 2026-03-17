using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGold : MonoBehaviour, EventListener<NotifyEvent>
{
    [SerializeField]
    Text curGold;

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
        curGold.text = Managers.UserData.MyGold.ToString("N0");

    }

    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_USER_INFO:
                {
                    Managers.UserData.RefreshQuestGold();
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
