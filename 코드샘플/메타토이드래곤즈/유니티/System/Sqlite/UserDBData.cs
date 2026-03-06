using SBPacketLib;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;

namespace SandboxNetwork
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableCustomAttribute : TableAttribute
    {
        private static Dictionary<string, Type> typeDic = new();
        private static Dictionary<Type, string> nameDic = new();
        public Type TYPE { get; set; }
        public TableCustomAttribute(Type type, string name) : base(name)
        {
            TYPE = type;

            if (false == typeDic.ContainsKey(name))
                typeDic.Add(name, type);
            if (false == nameDic.ContainsKey(type))
                nameDic.Add(type, name);
        }
        public static Type GetNameToType(string name)
        {
            if (typeDic.TryGetValue(name, out var value))
                return value;

            return null;
        }
        public static string GetTypeToName(Type TYPE)
        {
            if (nameDic.TryGetValue(TYPE, out var value))
                return value;

            return string.Empty;
        }
    }

    [TableCustom(typeof(ChatDataInfo), "ChatDataInfo")]
    public class ChatDataInfo
    {
        public ChatDataInfo() { }
        public ChatDataInfo(eChatCommentType type, long sendUID, string name, string portraitID, long guildUID, string guildName, long lastTime, long recvUID, long time, string msg
            , int _pType, int _pValue)
        {
            ChatType = (int)type;
            SendUID = sendUID;
            SendNickname = name;
            SendIcon = portraitID;
            SendUserGuildUID = guildUID;
            SendUserGuildName = guildName;
            SendUserLastEnterTimestamp = lastTime;
            RecvUID = recvUID;
            PortraitType = _pType;
            PortraitValue = _pValue;
            this.Time = time;
            ServerTag = NetworkManager.ServerTag;

            SetComment(msg);
        }
        public ChatDataInfo(eChatCommentType type, User user, long recvUID, string msg)
        {
            ChatType = (int)type;
            SendUID = user.UserAccountData.UserNumber;
            SendNickname = user.UserData.UserNick;
            SendIcon = user.UserData.UserPortrait;
            SendUserGuildUID = GuildManager.Instance.GuildID;
            SendUserGuildName = GuildManager.Instance.MyGuildInfo == null ? "" : GuildManager.Instance.MyGuildInfo.GetGuildName();
            SendUserLastEnterTimestamp = SBFunc.GetDateTimeToTimeStamp();
            Time = SBFunc.GetDateTimeToTimeStamp();
            RecvUID = recvUID;
            PortraitType = (int)ePortraitEtcType.NONE;
            PortraitValue = User.Instance.UserData.UserPortraitFrameInfo.GetValue(ePortraitEtcType.NONE);
            ServerTag = NetworkManager.ServerTag;

            SetComment(msg);
        }
        //초상화 데이터(+ 터치 시 계정 정보)를 구성하기 위해 필요한 것들 세팅(유저 레벨, 초상화 image 인덱스 등등...)
        public ChatDataInfo(SCChatMessage message)
        {
            ChatType = message.ChatType;
            SendUID = message.SendUserUID;
            SendNickname = message.SendUserName;
            SendIcon = message.SendIcon;
            SendUserGuildUID = message.SendUserGuildUID;
            SendUserGuildName = message.SendUserGuildName;
            SendUserLastEnterTimestamp = message.SendUserLastEnterTimeStamp;
            RecvUID = message.RecvUserUID;
            PortraitType = Convert.ToInt32(message.SendPortraitType);
            PortraitValue = Convert.ToInt32(message.SendPortraitValue);
            Time = message.CurrTimeStamp;
            ServerTag = message.ServerTag;

            SetComment(message.Message);
        }
        [Column("KEY")]
        [PrimaryKey]
        [AutoIncrement]
        public int UNIQUE_KEY { get; set; }
        public int ChatType { get; set; }
        [Indexed]
        public long SendUID { get; set; }
        [Indexed]
        public long RecvUID { get; set; }
        public string SendNickname { get; set; }
        public string SendIcon { get; set; }
        public long SendUserGuildUID { get; set; }
        public string SendUserGuildName { get; set; }
        public long SendUserLastEnterTimestamp { get; set; }
        public string Comment { get; set; }
        public long Time { get; set; }
        public int PortraitType { get; set; }
        public int PortraitValue { get; set; }
        public int ServerTag { get; set; }

        [Ignore]
        public eChatCommentType CommentType => (eChatCommentType)ChatType;

        public void InitChatDataInfo(eChatCommentType type, long sendUID, string name, long recvUID, long time, string msg)
        {
            ChatType = (int)type;
            SendUID = sendUID;
            SendNickname = name;
            RecvUID = recvUID;
            this.Time = time;

            SetComment(msg);
        }
        private void SetComment(string msg)
        {
            if (eChatCommentType.SystemMsg == CommentType)
                Comment = msg;
            else
                Comment = Crosstales.BWF.BWFManager.Instance.ReplaceAll(msg);
        }
    }
    [TableCustom(typeof(BlockUserData), "BlockList")]
    public class BlockUserData : ThumbnailUserData
    {
        [Ignore]
        public override PortraitEtcInfoData EtcInfo { 
            get
            {
                if (base.EtcInfo == null)
                    base.EtcInfo = new((ePortraitEtcType)PortraitType, PortraitValue);

                return base.EtcInfo;
            } 

            set => base.EtcInfo = value;
        }
        public new int PortraitType { get; set; }
        public new int PortraitValue { get; set; }
        public BlockUserData() : base(-1, "")
        {
        }
        public BlockUserData(ThumbnailUserData data) : base(data.UID, data.Nick)
        {
            Level = data.Level;
            PortraitIcon = data.PortraitIcon;
            EtcInfo = data.EtcInfo;
            LastActiveTime = data.LastActiveTime;
            if (EtcInfo != null)
            {
                var etcType = EtcInfo.GetDefaultType();
                PortraitType = (int)etcType;
                PortraitValue = EtcInfo.GetValue(etcType);
            }
        }
        public void SetData(ChatDataInfo data)
        {
            UID = data.SendUID;
            Nick = data.SendNickname;
            PortraitIcon = data.SendIcon;
            Level = -1;
            EtcInfo = new((ePortraitEtcType)data.PortraitType, data.PortraitValue);
            PortraitType = data.PortraitType;
            PortraitValue = data.PortraitValue;
            LastActiveTime = -1;
        }
        public void SetData(ThumbnailUserData data)
        {
            UID = data.UID;
            Nick = data.Nick;
            PortraitIcon = data.PortraitIcon;
            Level = data.Level;
            EtcInfo = data.EtcInfo;
            PortraitType = (int)data.PortraitType;
            PortraitValue = data.PortraitValue;
            LastActiveTime = -1;
        }
    }
    
}
