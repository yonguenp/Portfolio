using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SBPacketLib;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SandboxNetwork
{
    public enum eChatCommentType : byte
    {
        None = 0,
        World = 1 << 0,
        SystemMsg = 1 << 1,
        Guild = 1 << 2,
        Whisper = 1 << 3,
    }

    public enum eGuildSystemMsgType
    {
        Normal = 0,
        Exile = 1,
        Donation = 2,
    }
    public class GuildSystemMessage
    {
        public eGuildSystemMsgType type { get; private set; } = eGuildSystemMsgType.Normal;
        public string SenderNick { get; private set; } = string.Empty;
        public string Param { get; private set; } = string.Empty;

        public GuildSystemMessage(eGuildSystemMsgType t, string target)            
        {
            type = t;
            Param = target;
            SenderNick = User.Instance.UserData.UserNick;
        }
        public GuildSystemMessage(string msg)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("<([^<>]*)>");
            System.Text.RegularExpressions.MatchCollection matches = regex.Matches(msg);
            if (matches.Count > 0)
            {
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    string[] values = match.Groups[1].Value.Split(",");
                    foreach (var value in values)
                    {
                        string[] val = value.Split("=");
                        if (val.Length == 2)
                        {
                            switch (val[0])
                            {
                                case "type":
                                    switch (val[1])
                                    {
                                        case "exile":
                                            type = eGuildSystemMsgType.Exile;
                                            break;
                                        case "donation":
                                            type = eGuildSystemMsgType.Donation;
                                            break;
                                    }
                                    break;
                                case "sender":
                                    SenderNick = val[1];
                                    break;
                                case "target":
                                    Param = val[1];
                                    break;
                            }
                        }
                    }
                }                
            }
        }

        public string ToComment()
        {
            string strType = "";
            switch(type)
            {
                case eGuildSystemMsgType.Exile:
                    strType = "exile";
                    break;
                case eGuildSystemMsgType.Donation:
                    strType = "donation";
                    break;
            }

            return "<type=" + strType + ",sender=" + SenderNick + ",target=" + Param + ">";
        }
    }

    public class ChatManager : IManagerBase
    {
        const string CHAT_BLOCK_USER_LIST = "chat_block_user_list_new";
        public const string CHAT_WHISPER_CHECKREQ = "<checkreq>";
        public const string CHAT_WHISPER_CHECKRES = "<checkres>";
        /// <summary> 소켓 매니저 연결 </summary>
        private CChatServer ChatServer { get; set; } = null;

        public const int CHAT_QUEUE_MAX_SIZE = 50;
        public const int CHAT_MACRO_MAX_SIZE = 5;

        readonly int[] defaultMacroStringIndexArray = new int[]//매크로 서버에서 받아오는 기능 전에 임시 구현
        {
            100002496,
            100002497,
            100002498,
            100002499,
            100002500,
        };

        private static ChatManager instance = null;
        public static ChatManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ChatManager();
                }
                return instance;
            }
        }

        public Queue<ChatDataInfo> ChatQueueWorld { get; private set; } = new Queue<ChatDataInfo>();
        public Queue<ChatDataInfo> ChatQueueSystem { get; private set; } = new Queue<ChatDataInfo>();
        public Queue<ChatDataInfo> ChatQueueGuild { get; private set; } = new Queue<ChatDataInfo>();
        public Dictionary<long, Queue<ChatDataInfo>> ChatQueueWhisperSend { get; private set; } = new();
        public Dictionary<long, Queue<ChatDataInfo>> OneOnOneChatDataDic { get; private set; } = new Dictionary<long, Queue<ChatDataInfo>>();
        public Dictionary<long, long> OneOnOneLastChatID = new Dictionary<long, long>();

        private string[] macroArray = new string[CHAT_MACRO_MAX_SIZE];//매크로 리스트 (서버쪽에서 받아서 갱신)

        private Dictionary<long, BlockUserData> blockUserDic = null;//key : userID, value : ChatDataInfo

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitPlayMode()
        {
            if (instance != null)
            {
                instance = null;
            }
        }
        /// <summary> 초기화 및 기본 세팅 </summary>
        public void Initialize()
        {
            if (ChatServer == null)
            {
                ChatServer = new();
            }

            ChatServer.Initialize(NetworkManager.CHAT_SERVER, NetworkManager.SERVER_PORT);
            AllClearData();
            ClearMacroData();
        }
        /// <summary>
        /// 업데이트 순서
        /// 1. ChatServer 연결 확인
        /// 2. 패킷 확인
        /// 3. 채팅 UI 갱신
        /// </summary>
        public void Update(float dt)
        {
            if (null == ChatServer || false == ChatServer.Update(dt))
                return;

            var packets = ChatServer.GetPacket();
            if (packets.Count < 1)
                return;

            AddPacket(packets);
        }
        void ClearMacroData()
        {
            if (macroArray == null)
                macroArray = new string[CHAT_MACRO_MAX_SIZE];

            Array.Fill(macroArray, "");
        }

        public void AddPacket(List<SBSocketClientLib.PacketData> packets)
        {
            foreach (var packet in packets)
            {
                switch ((ChatServer_PacketID)packet.PacketID)
                {
                    case ChatServer_PacketID.SCChatMessage:
                    {
                        ChatDataInfo chatData = new(MessagePack.MessagePackSerializer.Deserialize<SCChatMessage>(packet.BodyData));
                        if (chatData.ServerTag != NetworkManager.ServerTag)
                            continue;

                        if (IsInBlockUserList(chatData.SendUID) && chatData.Comment != "exile") //차단 목록 유저에 있는 유저가 한말이면 넘기기
                            continue;

                        if (chatData.CommentType == eChatCommentType.Whisper)
                        {
                            if (chatData.SendUID != User.Instance.UserAccountData.UserNumber && chatData.RecvUID == User.Instance.UserAccountData.UserNumber)
                            {
                                if (chatData.Comment == CHAT_WHISPER_CHECKRES)
                                {
                                    ChatWisperSendResponse(chatData);
                                    return;
                                }
                                else if (chatData.Comment == CHAT_WHISPER_CHECKREQ)
                                {
                                    ChatServer.SendCheckMessage(new ChatDataInfo(eChatCommentType.Whisper, User.Instance, chatData.SendUID, CHAT_WHISPER_CHECKRES));
                                    return;
                                }
                                else
                                {
                                    ChatServer.SendCheckMessage(new ChatDataInfo(eChatCommentType.Whisper, User.Instance, chatData.SendUID, CHAT_WHISPER_CHECKRES));
                                }
                            }
                        }
                        AddData(chatData);
                        ChatEvent.RefreshChatUI();
                        //Debug.Log(string.Format("packet_ID{0}, size : {1}, Message : {2}", packet.PacketID, packet.DataSize, reqData.Message));
                    }
                    break;
                    case ChatServer_PacketID.SCPong:
                    {
                        var reqData = MessagePack.MessagePackSerializer.Deserialize<SCPong>(packet.BodyData);
                        var chatpopup = PopupManager.GetPopup<ChattingPopup>();
                        if (chatpopup != null)
                        {
                            chatpopup.RecvPong(reqData.Timestamp);
                        }
                    }
                    break;
                    default:
                    {
                        //Debug.Log(string.Format("packet_ID{0}, size : {1}, Message : {2}", packet.PacketID, packet.DataSize, reqData.Message));
                    }
                    break;
                }
            }
        }
        public bool AddData(ChatDataInfo _chatData)
        {
            bool ret = false;
            if ((_chatData.CommentType & eChatCommentType.SystemMsg) > 0)
            {
                if (ChatQueueSystem.Count > CHAT_QUEUE_MAX_SIZE)
                    ChatQueueSystem.Dequeue();

                // 시스템 메시지 문자열 재구성
                _chatData.Comment = ReorganizeSystemMessage(_chatData.Comment);
                if (string.IsNullOrWhiteSpace(_chatData.Comment))
                {
                    return ret;
                }

                ChatQueueSystem.Enqueue(_chatData);
                
                if(_chatData.SendUID == 0)
                {
                    ToastManager.On(_chatData.Comment);
                }
                else
                {
                    SystemMessage.PushMsg(_chatData.Comment);
                }

                ret = true;
            }

            if ((_chatData.CommentType & eChatCommentType.World) > 0)
            {
                if (ChatQueueWorld.Count > CHAT_QUEUE_MAX_SIZE)
                    ChatQueueWorld.Dequeue();

                UserDB.Insert(_chatData);
                ChatQueueWorld.Enqueue(_chatData);
                ret = true;
            }

            if ((_chatData.CommentType & eChatCommentType.Guild) > 0)
            {
                if (_chatData.SendUserGuildUID == GuildManager.Instance.GuildID)
                {
                    var msg = new GuildSystemMessage(_chatData.Comment);
                    if (msg.type != eGuildSystemMsgType.Normal)
                    {
                        _chatData.ChatType = (int)eChatCommentType.SystemMsg;
                        switch (msg.type)
                        {
                            case eGuildSystemMsgType.Exile:
                                if (User.Instance.UserData.UserNick != msg.Param)
                                {
                                    _chatData.Comment = StringData.GetStringFormatByStrKey("길드원추방안내메시지", msg.Param);
                                }
                                else
                                {
                                    _chatData.Comment = StringData.GetStringByStrKey("길드추방안내메시지");
                                }
                                break;
                            case eGuildSystemMsgType.Donation:
                                _chatData.Comment = StringData.GetStringFormatByStrKey("길드기부안내메시지" + msg.Param, msg.SenderNick);
                                break;
                            default:
                                return false;
                        }
                        

                        GuildManager.Instance.NetworkSend("guild/state", new WWWForm());
                        SystemMessage.PushMsg(_chatData.Comment);
                        
                        ChatQueueSystem.Enqueue(_chatData);
                        ret = true;
                    }
                    else
                    {
                        //일반 채팅
                        if (ChatQueueGuild.Count > CHAT_QUEUE_MAX_SIZE)
                            ChatQueueGuild.Dequeue();

                        UserDB.Insert(_chatData);
                        ChatQueueGuild.Enqueue(_chatData);
                        if (_chatData.SendUID != User.Instance.UserAccountData.UserNumber)
                            SystemMessage.PushGuildMsg(_chatData);
                        ret = true;
                    }                    
                }
            }

            if ((_chatData.CommentType & eChatCommentType.Whisper) > 0)
            {
                ///레드닷 표시
                if (_chatData.SendUID != User.Instance.UserAccountData.UserNumber)
                {
                    if (_chatData.RecvUID == User.Instance.UserAccountData.UserNumber)
                    {
                        if (FriendManager.FriendIdList.ContainsKey(_chatData.SendUID))
                            ReddotManager.Set(eReddotEvent.FRIEND, true);
                        else
                            ReddotManager.Set(eReddotEvent.GUILD_WISPER, true);
                    }
                }
                //
                //if (ChatQueueWhisper.Count > CHAT_QUEUE_MAX_SIZE)
                //    ChatQueueWhisper.Dequeue();
                //ChatQueueWhisper.Enqueue(_chatData);
                if (false == OneOnOneChatDataDic.TryGetValue(_chatData.SendUID, out var queue))
                {
                    queue = new();
                    OneOnOneChatDataDic.Add(_chatData.SendUID, queue);
                }
                if (queue.Count > CHAT_QUEUE_MAX_SIZE)
                    queue.Dequeue();
                queue.Enqueue(_chatData);
                UserDB.Insert(_chatData);
                SystemMessage.PushWisperMsg(_chatData);
                if (OneOnOneLastChatID.ContainsKey(_chatData.SendUID))
                    OneOnOneLastChatID[_chatData.SendUID] = TimeManager.GetTime();
                else
                    OneOnOneLastChatID.Add(_chatData.SendUID, TimeManager.GetTime());
                ret = true;
            }

            return ret;
        }

        public void AllClearData()
        {
            ChatQueueWorld.Clear();
            ChatQueueGuild.Clear();
            ChatQueueSystem.Clear();
            ChatQueueWhisperSend.Clear();
            OneOnOneChatDataDic.Clear();
        }

        public bool DeleteChatData(ChatDataInfo chatData)
        {
            if (ChatQueueWorld.Count > 0)
            {
                var tempChatList = ChatQueueWorld.ToList();
                tempChatList.Remove(chatData);

                Queue<ChatDataInfo> cChatDataQueue = new Queue<ChatDataInfo>();
                foreach (var item in tempChatList)
                {
                    cChatDataQueue.Enqueue(item);
                }
                ChatQueueWorld.Clear();
                ChatQueueWorld = cChatDataQueue;
                return true;
            }

            return false;
        }

        public void SetBlockUserList()//차단 유저 리스트를 어떤식으로 제어할지 논의 해야함(삭제, 추가 , 갱신)
        {

        }

        public string[] GetMacroArray()
        {
            return macroArray;
        }

        public void AddMacroComment(int _index, string _comment)
        {

        }

        public int[] GetMacroStringIndexArray()
        {
            return defaultMacroStringIndexArray;
        }

        public void SendMessage(ChatDataInfo data)
        {
            if ((data.CommentType & eChatCommentType.Whisper) > 0)
            {
                //WWWForm param = new WWWForm();
                //param.AddField("friend_uno", data.recvUserUID.ToString());
                //param.AddField("msg", data.comment);
                //if (OneOnOneLastChatID.TryGetValue(data.recvUserUID, out var last_id))
                //    param.AddField("last_id", last_id.ToString());

                //NetworkManager.Send("friend/chat", param, (res) =>
                //{
                //    OnWebChatResponse(res, () =>
                //    {
                //        ChatQueueWhisper.Clear();
                //        ChatEvent.RefreshChatUI();
                //    });
                //});

                if(false == ChatQueueWhisperSend.TryGetValue(data.RecvUID, out var queue))
                {
                    queue = new();
                    ChatQueueWhisperSend.Add(data.RecvUID, queue);
                }
                queue.Enqueue(data);
            }

            ChatServer.SendMessage(data);
        }

        public void ChatWisperSendResponse(ChatDataInfo res)
        {
            if (false == ChatQueueWhisperSend.TryGetValue(res.SendUID, out var queue))
            {
                queue = new();
                ChatQueueWhisperSend.Add(res.SendUID, queue);
            }

            bool isRefresh = queue.Count > 0;
            while (queue.Count > 0)
            {
                var data = queue.Dequeue();

                if (false == OneOnOneChatDataDic.TryGetValue(data.RecvUID, out var chatList))
                {
                    chatList = new();
                    OneOnOneChatDataDic.Add(data.RecvUID, chatList);
                }
                var chatData = new ChatDataInfo(eChatCommentType.Whisper, User.Instance, data.RecvUID, data.Comment);
                chatList.Enqueue(chatData);
                UserDB.Insert(chatData);
                if (OneOnOneLastChatID.ContainsKey(data.RecvUID))
                    OneOnOneLastChatID[data.RecvUID] = TimeManager.GetTime();
                else
                    OneOnOneLastChatID.Add(data.RecvUID, TimeManager.GetTime());
            }

            if (isRefresh)
                ChatEvent.RefreshChatUI();

            ChatEvent.SendWhisperRes(res.SendUID);
        }

        public void SendWhisperCheck(long resUID)
        {
            if (ChatServer == null)
                return;

            ChatServer.SendCheckMessage(new ChatDataInfo(eChatCommentType.Whisper, User.Instance, resUID, CHAT_WHISPER_CHECKREQ));
        }

        public void EnterOneOnOne(long friend_uno, Action cb)
        {
            //WWWForm param = new();
            //param.AddField("friend_uno", friend_uno.ToString());
            //if (OneOnOneLastChatID.TryGetValue(friend_uno, out var last_id))
            //    param.AddField("last_id", last_id.ToString());

            //NetworkManager.Send("friend/chat", param, (res) => { OnWebChatResponse(res, cb); });

            if (false == OneOnOneChatDataDic.TryGetValue(friend_uno, out var chat))
            {
                chat = new();
                OneOnOneChatDataDic.Add(friend_uno, chat);
                var chatList = UserDB.GetWisperChat(friend_uno);
                if (chatList != null)
                {
                    var myUID = User.Instance.UserAccountData.UserNumber;
                    for (int i = 0, count = chatList.Count; i < count; ++i)
                    {
                        chat.Enqueue(chatList[i]);

                        long curUID;
                        if (chatList[i].RecvUID != myUID)
                        {
                            curUID = chatList[i].RecvUID;
                        }
                        else
                        {
                            curUID = chatList[i].SendUID;
                        }

                        if (OneOnOneLastChatID.TryGetValue(curUID, out var data))
                        {
                            if (data < chatList[i].Time)
                                OneOnOneLastChatID[curUID] = chatList[i].Time;
                        }
                        else
                            OneOnOneLastChatID.Add(curUID, chatList[i].Time);
                    }
                }
            }

            cb?.Invoke();
        }

        public void SendGuildExileSystemMessage(GuildSystemMessage msg)
        {
            if (msg.type != eGuildSystemMsgType.Exile)
                return;

            ChatDataInfo data = new(eChatCommentType.Guild, User.Instance.UserAccountData.UserNumber, msg.SenderNick, User.Instance.UserData.UserPortrait, GuildManager.Instance.GuildID, GuildManager.Instance.GuildName, SBFunc.GetDateTimeToTimeStamp(), 0,
    SBFunc.GetDateTimeToTimeStamp(), msg.ToComment(), 1, 0);

            if (!ChatServer.IsSocketConnect())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat CCConnectSocket => SendAchieveSystemMessage In false == IsSocketConnect"));
#endif
                AddData(data);
                return;
            }
            ChatServer.SendMessage(data);
        }
        public void SendGuildDonationSystemMessage(GuildSystemMessage msg)
        {
            if (msg.type != eGuildSystemMsgType.Donation)
                return;

            ChatDataInfo data = new(eChatCommentType.Guild, User.Instance.UserAccountData.UserNumber, msg.SenderNick, User.Instance.UserData.UserPortrait, GuildManager.Instance.GuildID, GuildManager.Instance.GuildName, SBFunc.GetDateTimeToTimeStamp(), 0,
    SBFunc.GetDateTimeToTimeStamp(), msg.ToComment(), 1, 0);

            if (!ChatServer.IsSocketConnect())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat CCConnectSocket => SendAchieveSystemMessage In false == IsSocketConnect"));
#endif
                AddData(data);
                return;
            }

            ChatServer.SendMessage(data);
        }

        /// <summary>
        /// 시스템 메시지 전송 관련 코드
        /// </summary>
        public void SendAchieveSystemMessage(eAchieveSystemMessageType messageType, string userName, int targetID = 0)
        {
            // 발신자 로컬라이징 이슈로 따로 문자열 양식을 구분하여 전송 처리

            string resultMessage = string.Empty;

            switch (messageType)
            {
                case eAchieveSystemMessageType.GET_DRAGON_U:
                case eAchieveSystemMessageType.GET_DRAGON_L:
                case eAchieveSystemMessageType.GET_PET_U:
                case eAchieveSystemMessageType.GET_PET_L:
                case eAchieveSystemMessageType.COMPOUND_DRAGON_U:
                case eAchieveSystemMessageType.COMPOUND_DRAGON_L:
                case eAchieveSystemMessageType.COMPOUND_PET_U:
                case eAchieveSystemMessageType.COMPOUND_PET_L:
                case eAchieveSystemMessageType.PET_MAX_REINFORCE:
                case eAchieveSystemMessageType.EQUIPMENT_9_REINFORCE:
                case eAchieveSystemMessageType.EQUIPMENT_12_REINFORCE:
                case eAchieveSystemMessageType.EQUIPMENT_15_REINFORCE:
                case eAchieveSystemMessageType.GET_EQUIPMENT:
                case eAchieveSystemMessageType.EVENT_LOTTO_WINNER:
                case eAchieveSystemMessageType.EVENT_POCKET_LEVEL10:
                case eAchieveSystemMessageType.SERVER_MAINTENANCE:
                case eAchieveSystemMessageType.TRANSCENDENCE_STEP1:
                case eAchieveSystemMessageType.TRANSCENDENCE_STEP2:
                case eAchieveSystemMessageType.TRANSCENDENCE_STEP3:
                    resultMessage = string.Join(",", (int)messageType, userName, targetID);
                    break;
                case eAchieveSystemMessageType.ACHIEVE_TOWN_MAX_LEVEL:
                    resultMessage = string.Join(",", (int)messageType, userName);
                    break;
            }

            ChatDataInfo data = new(eChatCommentType.SystemMsg, User.Instance.UserAccountData.UserNumber, User.Instance.UserData.UserNick, User.Instance.UserData.UserPortrait, 0, "GB글로리", SBFunc.GetDateTimeToTimeStamp(), 0,
                SBFunc.GetDateTimeToTimeStamp(), resultMessage, 1, 0);

            if (!ChatServer.IsSocketConnect())
            {
#if DEBUG
                Debug.Log(SBFunc.StrBuilder("##Chat CCConnectSocket => SendAchieveSystemMessage In false == IsSocketConnect"));
#endif
                AddData(data);
                return;
            }
            ChatServer.SendMessage(data);
        }

        // 시스템 메시지 - 문자열 양식을 재구성하여 반환
        public string ReorganizeSystemMessage(string msg)
        {
            string result = "";

            string[] splitStrings = msg.Split(",");
            if (splitStrings.Length > 2)
            {
                eAchieveSystemMessageType msgType = (eAchieveSystemMessageType)int.Parse(splitStrings[0]);
                switch (msgType)
                {
                    case eAchieveSystemMessageType.GET_DRAGON_U:
                        CharBaseData charData_u = CharBaseData.Get(splitStrings[2]);
                        if (charData_u != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림1", splitStrings[1], StringData.GetStringByStrKey(charData_u._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.GET_DRAGON_L:
                        CharBaseData charData_l = CharBaseData.Get(splitStrings[2]);
                        if (charData_l != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림4", splitStrings[1], StringData.GetStringByStrKey(charData_l._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.GET_PET_U:
                        PetBaseData petData_u = PetBaseData.Get(splitStrings[2]);
                        if (petData_u != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림14", splitStrings[1], StringData.GetStringByStrKey(petData_u._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.GET_PET_L:
                        PetBaseData petData_l = PetBaseData.Get(splitStrings[2]);
                        if (petData_l != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림15", splitStrings[1], StringData.GetStringByStrKey(petData_l._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.COMPOUND_DRAGON_U:
                        CharBaseData charCompoundData_u = CharBaseData.Get(splitStrings[2]);
                        if (charCompoundData_u != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림5", splitStrings[1], StringData.GetStringByStrKey(charCompoundData_u._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.COMPOUND_DRAGON_L:
                        CharBaseData charCompoundData_l = CharBaseData.Get(splitStrings[2]);
                        if (charCompoundData_l != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림6", splitStrings[1], StringData.GetStringByStrKey(charCompoundData_l._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.COMPOUND_PET_U:
                        PetBaseData petCompoundData_u = PetBaseData.Get(splitStrings[2]);
                        if (petCompoundData_u != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림16", splitStrings[1], StringData.GetStringByStrKey(petCompoundData_u._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.COMPOUND_PET_L:
                        PetBaseData petCompoundData_l = PetBaseData.Get(splitStrings[2]);
                        if (petCompoundData_l != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림17", splitStrings[1], StringData.GetStringByStrKey(petCompoundData_l._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.EQUIPMENT_9_REINFORCE:
                    {
                        ItemBaseData itemData = ItemBaseData.Get(splitStrings[2]);
                        if (itemData != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림2", splitStrings[1], itemData.NAME);
                        }
                    }
                    break;
                    case eAchieveSystemMessageType.EQUIPMENT_12_REINFORCE:
                    {
                        ItemBaseData itemData = ItemBaseData.Get(splitStrings[2]);
                        if (itemData != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림9", splitStrings[1], itemData.NAME);
                        }
                    }
                    break;
                    case eAchieveSystemMessageType.EQUIPMENT_15_REINFORCE:
                    {
                        ItemBaseData itemData = ItemBaseData.Get(splitStrings[2]);
                        if (itemData != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림10", splitStrings[1], itemData.NAME);
                        }
                    }
                    break;
                    case eAchieveSystemMessageType.GET_EQUIPMENT:
                        ItemBaseData maxEquipitemData = ItemBaseData.Get(splitStrings[2]);
                        if (maxEquipitemData != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림8", splitStrings[1], maxEquipitemData.NAME);
                        }
                        break;
                    case eAchieveSystemMessageType.PET_MAX_REINFORCE:
                        PetBaseData petReinforceData = PetBaseData.Get(splitStrings[2]);
                        if (petReinforceData != null)
                        {
                            result = StringData.GetStringFormatByStrKey("시스템전체알림7", splitStrings[1], StringData.GetStringByStrKey(petReinforceData._NAME));
                        }
                        break;
                    case eAchieveSystemMessageType.ACHIEVE_TOWN_MAX_LEVEL:
                        result = StringData.GetStringFormatByStrKey("시스템전체알림3", splitStrings[1]);
                        break;
                    case eAchieveSystemMessageType.EVENT_LOTTO_WINNER://크리스마스 이벤트 로또 당첨자
                        result = StringData.GetStringFormatByStrKey("시스템전체알림99", splitStrings[1]);
                        break;
                    case eAchieveSystemMessageType.EVENT_POCKET_LEVEL10://크리스마스 이벤트 로또 당첨자
                        result = StringData.GetStringFormatByStrKey("시스템전체알림100", splitStrings[1]);
                        break;
                    case eAchieveSystemMessageType.TRANSCENDENCE_STEP1:
                        CharBaseData tr_char1 = CharBaseData.Get(splitStrings[2]);
                        if (tr_char1 != null)
                        {
                            var dragonName = StringData.GetStringByStrKey(tr_char1._NAME);
                            switch (tr_char1.GRADE)
                            {
                                case 4:
                                    dragonName = string.Format("<color=#c898ff>{0}</color>", dragonName);
                                    break;
                                case 5:
                                    dragonName = string.Format("<color=#f49907>{0}</color>", dragonName);
                                    break;
                            }
                            result = StringData.GetStringFormatByStrKey("시스템전체알림11", splitStrings[1], dragonName);
                        }
                        break;
                    case eAchieveSystemMessageType.TRANSCENDENCE_STEP2:
                        CharBaseData tr_char2 = CharBaseData.Get(splitStrings[2]);
                        if (tr_char2 != null)
                        {
                            var dragonName = StringData.GetStringByStrKey(tr_char2._NAME);
                            switch (tr_char2.GRADE)
                            {
                                case 4:
                                    dragonName = string.Format("<color=#c898ff>{0}</color>", dragonName);
                                    break;
                                case 5:
                                    dragonName = string.Format("<color=#f49907>{0}</color>", dragonName);
                                    break;
                            }
                            result = StringData.GetStringFormatByStrKey("시스템전체알림12", splitStrings[1], dragonName);
                        }
                        break;
                    case eAchieveSystemMessageType.TRANSCENDENCE_STEP3:
                        CharBaseData tr_char3 = CharBaseData.Get(splitStrings[2]);
                        if (tr_char3 != null)
                        {
                            var dragonName = StringData.GetStringByStrKey(tr_char3._NAME);
                            switch (tr_char3.GRADE)
                            {
                                case 4:
                                    dragonName = string.Format("<color=#c898ff>{0}</color>", dragonName);
                                    break;
                                case 5:
                                    dragonName = string.Format("<color=#f49907>{0}</color>", dragonName);
                                    break;
                            }
                            result = StringData.GetStringFormatByStrKey("시스템전체알림13", splitStrings[1], dragonName);
                        }
                        break;

                    case eAchieveSystemMessageType.SERVER_MAINTENANCE://서버점검알림.
                        result = StringData.GetStringByStrKey("서버점검알림");
                        break;
                }
            }
            else
            {
                result = msg;
            }

            return result;
        }

        /// <summary> 유저 넘버로 차단 리스트에 있는 유저 데이터 가져오기 </summary>
        public BlockUserData GetBlockUserData(long _blockUserID)
        {
            if (!IsInBlockUserList(_blockUserID))
                return null;

            return blockUserDic[_blockUserID];
        }

        /// <summary> 차단 리스트 가져오기 </summary>
        public List<BlockUserData> GetBlockUserDataList()
        {
            return blockUserDic.Values.ToList();
        }

        /// <summary> 차단 해제 </summary>
        public void RemoveBlockUser(long _blockUserID)
        {
            if (IsInBlockUserList(_blockUserID))
            {
                blockUserDic.Remove(_blockUserID);
                SetBlockUserList(blockUserDic);
            }
        }

        public bool IsInBlockUserList(long _blockUserID)
        {
            if (blockUserDic == null)
                return false;

            return blockUserDic.ContainsKey(_blockUserID);
        }
        /// <summary> 새로 들어온 리스트로 다시 세팅 </summary>
        void SetBlockUserList(Dictionary<long, BlockUserData> _blockUserDic)
        {
            UserDB.DeleteBlockList();
            UserDB.Insert(_blockUserDic.Values.ToList());
        }

        public void AddUserBlockList(ChatDataInfo chatDataInfo)
        {
            if (chatDataInfo == null)
                return;

            if (false == blockUserDic.TryGetValue(chatDataInfo.SendUID, out var oldUserData))
            {
                oldUserData = new();
                blockUserDic.Add(chatDataInfo.SendUID, oldUserData);
            }
            oldUserData.SetData(chatDataInfo);
            SetBlockUserList(blockUserDic);

            ChatEvent.RefreshChatUI();
            ToastManager.On(100002141);//"해당 유저의 대화 내용을 차단하였습니다."
        }
        public void AddUserBlockList(ProfileUserData userData)
        {
            if (userData == null)
                return;

            if (false == blockUserDic.TryGetValue(userData.UID, out var oldUserData))
            {
                oldUserData = new();
                blockUserDic.Add(userData.UID, oldUserData);
            }
            oldUserData.SetData(userData);
            SetBlockUserList(blockUserDic);

            ChatEvent.RefreshChatUI();
            ToastManager.On(100002141);//"해당 유저의 대화 내용을 차단하였습니다."
        }

        public void LoadDBData()
        {
            blockUserDic = UserDB.GetBlockUser();
            ChatQueueWorld = new Queue<ChatDataInfo>(UserDB.GetWorldChat());
        }
        public void LoadDBGuildData(long guildUID)
        {
            if (guildUID > 0)
                ChatQueueGuild = new Queue<ChatDataInfo>(UserDB.GetGuildChat(guildUID));
            else
                ChatQueueGuild = new Queue<ChatDataInfo>();
        }
        public void OnWebChatResponse(JObject response, Action cb)
        {
            if (response.ContainsKey("rs") && response["rs"].Value<int>() != 0)
            {
                return;
            }

            if (response.ContainsKey("senders"))
            {

            }

            if (response.ContainsKey("msg"))
            {
                JArray msg_array = (JArray)response["msg"];
                long myUserNo = User.Instance.UserAccountData.UserNumber;
                long targetUserNo = response["friend_no"].Value<long>();
                FriendUserData targetUserInfo = FriendManager.FriendIdList[targetUserNo];
                var userPortraitInfo = User.Instance.UserData.UserPortraitFrameInfo;

                foreach (JObject msg in msg_array)
                {
                    ChatDataInfo _chatData = null;
                    long sender = msg["from_user"].Value<long>();
                    long reciver = msg["to_user"].Value<long>();
                    string comment = msg["message"].Value<string>();

                    DateTime send_at = SBFunc.DateTimeParse(msg["send_at"].Value<string>());
                    long time = TimeManager.GetTimeStamp(send_at);
                    if (sender == myUserNo)
                    {
                        _chatData = new ChatDataInfo(eChatCommentType.Whisper, User.Instance ,reciver, comment);
                    }
                    else if (sender == targetUserNo)
                    {
                        _chatData = new ChatDataInfo(eChatCommentType.Whisper, targetUserNo, targetUserInfo.Nick, targetUserInfo.PortraitIcon, 0, "", time, reciver, time, comment, (int)targetUserInfo.PortraitType, targetUserInfo.PortraitValue);
                    }

                    if (false == OneOnOneLastChatID.ContainsKey(targetUserNo))
                        OneOnOneLastChatID.Add(targetUserNo, msg["row_no"].Value<long>());
                    else
                        OneOnOneLastChatID[targetUserNo] = msg["row_no"].Value<long>();

                    if (false == OneOnOneChatDataDic.TryGetValue(targetUserNo, out var queue))
                    {
                        queue = new Queue<ChatDataInfo>();
                        OneOnOneChatDataDic.Add(targetUserNo, queue);
                    }

                    queue.Enqueue(_chatData);
                    while (queue.Count > CHAT_QUEUE_MAX_SIZE)
                        queue.Dequeue();
                }

                cb.Invoke();
            }
        }
        public void Disconnect()
        {
            if (ChatServer == null)
                return;

            ChatServer.DisConnect();
        }

        public bool IsConnect()
        {
            if (ChatServer == null)
                return false;

            return ChatServer.IsConnect();
        }

        public void ResetConnectCount()
        {
            if (ChatServer == null)
                return;

            ChatServer.ResetConnectCount();
        }

        public void InitConnectChatServer()
        {
            if (ChatServer == null)
                return;

            ChatServer.InitConnectChatServer();
        }

        public void Send(byte[] packet)
        {
            if (ChatServer == null)
                return;

            ChatServer.Send(packet);
        }
#if UNITY_EDITOR

        [MenuItem("Custom/Chat/TestOpenPopup")]
        public static void TestOpenPopup()
        {
            PopupManager.OpenPopup<ChattingPopup>();
        }
        [MenuItem("Custom/Toast/TestToast")]
        public static void TestToast()
        {
            ToastManager.On(StringData.GetStringFormatByStrKey("achievements_info:complete:10001", "테스트트트"));
        }

        [MenuItem("Custom/Toast/TestToast50")]
        public static void TestToast50()
        {
            for (int i = 0; i < 50; i++)
            {
                ToastManager.On(StringData.GetStringFormatByStrKey("achievements_info:complete:10001", "테스트트트"));
            }
        }


        [MenuItem("Custom/Chat/TestMyMsgDebug")]
        public static void TestMyMsgCreate()
        {
            Instance.AddData(new ChatDataInfo(eChatCommentType.World, User.Instance, 0, "Hello World"));
            ChatEvent.RefreshChatUI();
        }
        [MenuItem("Custom/Chat/TestMsgDebug")]
        public static void TestOtherMsgCreate()
        {
            var num = SBFunc.Random(1, 1000);
            Instance.AddData(new ChatDataInfo(eChatCommentType.World,
                User.Instance.UserAccountData.UserNumber + num,
                User.Instance.UserData.UserNick + num.ToString(),
                User.Instance.UserData.UserPortrait, 0, "GB글로리",
                SBFunc.GetDateTimeToTimeStamp(), 0, SBFunc.GetDateTimeToTimeStamp(), "Hello World", 1, 0));
            ChatEvent.RefreshChatUI();
        }
        [MenuItem("Custom/Chat/TestMyMsgDebug - 50")]
        public static void TestMyMsgCreate_50()
        {
            for (int i = 0; i < 50; ++i)
            {
                Instance.AddData(new ChatDataInfo(eChatCommentType.World, User.Instance, 0, "Hello World"));
            }

            ChatEvent.RefreshChatUI();
        }
        [MenuItem("Custom/Chat/TestMsgDebug - 50")]
        public static void TestOtherMsgCreate_50()
        {
            for (int i = 0; i < 50; ++i)
            {
                var num = SBFunc.Random(1, 1000);
                Instance.AddData(new ChatDataInfo(eChatCommentType.World,
                    User.Instance.UserAccountData.UserNumber + num,
                    User.Instance.UserData.UserNick + num.ToString(),
                    User.Instance.UserData.UserPortrait, 0, "GB글로리",
                    SBFunc.GetDateTimeToTimeStamp(), 0, SBFunc.GetDateTimeToTimeStamp(), "Hello World", 1, 0));
            }

            ChatEvent.RefreshChatUI();
        }

        [MenuItem("Custom/Chat/TestMixMsgDebug - 50")]
        public static void TestMixMsgCreate_50()
        {
            for (int i = 0; i < 25; ++i)
            {
            Instance.AddData(new ChatDataInfo(eChatCommentType.World, User.Instance, 0, "Hello World"));

                var num = SBFunc.Random(1, 1000);
                Instance.AddData(new ChatDataInfo(eChatCommentType.World,
                    User.Instance.UserAccountData.UserNumber + num,
                    User.Instance.UserData.UserNick + num.ToString(),
                    User.Instance.UserData.UserPortrait, 0, "GB글로리",
                    SBFunc.GetDateTimeToTimeStamp(), 0, SBFunc.GetDateTimeToTimeStamp(), "Hello World", 1, 0));
            }

            ChatEvent.RefreshChatUI();
        }
        [MenuItem("Custom/Chat/TestMySystemMsgDebug")]
        public static void TestMySystemMsgDebug()
        {
            Instance.ChatServer.SendMessage(new(eChatCommentType.SystemMsg,
                User.Instance.UserAccountData.UserNumber, User.Instance.UserData.UserNick,
                User.Instance.UserData.UserPortrait, 0, "", SBFunc.GetDateTimeToTimeStamp(),
                0, SBFunc.GetDateTimeToTimeStamp(), "System Msg Check", 1, 0));
        }

        //[MenuItem("Custom/Chat/Disconnect")]
        public void MenuDisconnect()
        {
            Instance.Disconnect();
        }
#endif
    }
}
