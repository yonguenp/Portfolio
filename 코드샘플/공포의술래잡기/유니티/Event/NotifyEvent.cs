using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NotifyEvent
{
    public enum NotifyEventMessage
    {
        ON_USER_INFO,
        ON_CHARACTERS_INFO,
        ON_CHARACTER_UPDATE,
        ON_ITEM_INFO,
        ON_ITEM_UPDATE,
        ON_SHOP_INFO,
        ON_RANK_REWARD,
        ON_QUEST_REWARD,
        ON_MAIL_INFO,
    }

    public NotifyEventMessage Message;
    public NotifyEvent(NotifyEventMessage message)
    {
        Message = message;
    }
    static NotifyEvent e;
    public static void Trigger(NotifyEventMessage message)
    {
        e.Message = message;
        EventManager.TriggerEvent(e);
    }
}
