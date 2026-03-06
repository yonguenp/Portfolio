using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventChatAddMessage : UnityEvent<sChatData>{}

public enum eChatType
{
    None,
    Send,
    Receive,
}

public struct sChatData
{
    public eChatType Type;
    public long ChatId;
    public long TargetId;
    public string TargetNick;
    public string Message;
    public long Time;
}