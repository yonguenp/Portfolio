using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeAsset : MonoBehaviour, EventListener<NotifyEvent>
{
    [SerializeField] Image Image;
    [SerializeField] Text amount;
    [SerializeField] int item_id;


    private void OnEnable()
    {
        this.EventStartListening();
        Refresh();
    }
    private void OnDisable()
    {
        this.EventStopListening();
    }
    public void OnEvent(NotifyEvent eventType)
    {
        switch (eventType.Message)
        {
            case NotifyEvent.NotifyEventMessage.ON_ITEM_INFO:
            case NotifyEvent.NotifyEventMessage.ON_ITEM_UPDATE:
                {
                    Refresh();
                }
                break;
        }
    }

    private void Refresh()
    {
        if (!gameObject.activeSelf)
            return;
        
        if (item_id <= 0)
            return;

        amount.text = Managers.UserData.GetMyItemCount(item_id).ToString();
    }
    public void SetSlot(Sprite icon, int id)
    {
        Image.sprite = icon;
        item_id = id;
        amount.text = Managers.UserData.GetMyItemCount(item_id).ToString();
    }
}
