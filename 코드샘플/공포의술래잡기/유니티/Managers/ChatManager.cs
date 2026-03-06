using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChatManager
{
    Dictionary<long, List<sChatData>> messages = new Dictionary<long, List<sChatData>>();
    Dictionary<long, long> readMessages = new Dictionary<long, long>();
    Dictionary<Type, EventChatAddMessage> addMessageCallback = new Dictionary<Type, EventChatAddMessage>();

    public UserProfile CurChatTarget { get; set; }

    public void SetAddMessageCallback(Type type, UnityAction<sChatData> action)
    {
        if (addMessageCallback.ContainsKey(type)) return;
        var eventListener = new EventChatAddMessage();
        eventListener.AddListener(action);
        addMessageCallback.Add(type, eventListener);
    }

    public void RemoveAddMessageCallback(Type type)
    {
        addMessageCallback.Remove(type);
    }

    eChatType GetChatType(long to_id)
    {
        if (Managers.UserData.MyUserID == to_id)
            return eChatType.Receive;
        return eChatType.Send;
    }

    long GetTargetId(long to_id, long from_id)
    {
        if (Managers.UserData.MyUserID == to_id)
            return from_id;
        return to_id;
    }

    public void ReadMessage(long id)
    {
        if (id == Managers.UserData.MyUserID) return;

        var updateTime = SBCommonLib.SBUtil.GetCurrentMilliSecTimestamp(false);
        if (readMessages.ContainsKey(id) == false)
        {
            readMessages.Add(id, updateTime);
        }

        readMessages[id] = updateTime;
    }

    public bool IsChatNotice(long id)
    {
        if (messages.ContainsKey(id) == false)
        {
            return false;
        }

        int count = messages[id].Count;

        if (count == 0) return false;

        long lastMessageTime = 0;
        for (int i = count - 1; i >= 0; --i)
        {
            var item = messages[id][count - 1];
            if (item.Type == eChatType.Receive)
            {
                lastMessageTime = item.Time;
                return IsChatNotice(id, lastMessageTime);
            }
        }

        return false;
    }

    public bool IsChatNotice(long id, long time)
    {
        if (readMessages.ContainsKey(id) == false)
        {
            return true;
        }

        return readMessages[id] < time;
    }

    public void AddMessage(long from_id, string from_nick, long to_id, string to_nick, string message)
    {
        var time_ms = SBCommonLib.SBUtil.GetCurrentMilliSecTimestamp(false);

        var targetId = GetTargetId(to_id, from_id);
        string targetNick = from_nick;
        if (targetId != from_id)
            targetNick = to_nick;
        sChatData chatData = new sChatData()
        {
            Type = GetChatType(to_id),
            TargetId = targetId,
            TargetNick = targetNick,
            ChatId = targetId,              //그룹채팅 대비
            Message = message,
            Time = time_ms
        };

        if (messages.ContainsKey(targetId) == false)
        {
            var chatDatas = new List<sChatData>();
            chatDatas.Add(chatData);
            messages.Add(targetId, chatDatas);
        }
        else
        {
            messages[targetId].Add(chatData);
        }

        var it = addMessageCallback.GetEnumerator();
        while (it.MoveNext())
        {
            var eventListener = it.Current.Value;
            if (eventListener == null) continue;
            eventListener.Invoke(chatData);
        }
    }

    public List<sChatData> GetChatDatas(long chatId)
    {
        var list = new List<sChatData>();
        if (messages.ContainsKey(chatId) == false)
            return list;
        messages.TryGetValue(chatId, out list);
        return list;
    }

    public bool IsChatNotice()
    {
        foreach (var item in messages)
        {
            var key = item.Key;
            if (IsChatNotice(key))
                return true;
        }

        return false;
    }
}

public class ClanChatData
{
    public long user_id { get; private set; }
    public string user_nick { get; private set; }
    public int user_point { get; private set; }
    public string message { get; private set; }
    public long tiem_ms { get; private set; }

    public void SetData(long userid, string usernick, int point, string msg, long time)
    {
        user_id = userid;
        user_nick = usernick;
        user_point = point;
        message = msg;
        tiem_ms = time;
    }
}

public class ClanChatManager
{
    const int MAX_CHAT_LEN = 50;
    Queue<ClanChatData> ClanChats = new Queue<ClanChatData>();

    public void Clear()
    {
        ClanChats.Clear();
    }
    public void AddMessage(long userid, string usernick, int point, string msg, long time)
    {
        ClanChatData chat = new ClanChatData();
        chat.SetData(userid, usernick, point, msg, time);

        CheckMaxQueueSize();

        ClanChats.Enqueue(chat);
        CheckClanChatAddMessage();
        //SBDebug.Log(chat);
    }
    public void CheckClanChatAddMessage()
    {
        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby != null && PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP).IsOpening())
        {
            var popup =  PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
            popup.clanInfoPage.info_Chat.RefreshChatUI();
        }
    }

    public void CheckMaxQueueSize()
    {
        while (ClanChats.Count > MAX_CHAT_LEN)
        {
            ClanChats.Dequeue();
        }
    }

    public ClanChatData[] GetAllChat()
    {
        return ClanChats.ToArray();
    }
}
