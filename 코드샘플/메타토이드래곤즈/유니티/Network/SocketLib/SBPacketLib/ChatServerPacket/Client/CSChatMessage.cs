using MessagePack;
using System;

namespace SBPacketLib
{
    [MessagePackObject]
    public class CSChatMessage
    {
        [Key(0)]
        public Byte ChatType = 0; // 0:전체, 1:1대1, 2:길드 등
        [Key(1)]
        public int ServerTag = -1; // 접속 서버
        [Key(2)]
        public long SendUserUID = 0; // 보내는 유저 유니크아이디
        [Key(3)]
        public string SendUserName = string.Empty; // 보내는 유저 이름
        [Key(4)]
        public string SendIcon = string.Empty;
        [Key(5)]
        public long SendPortraitType = 0;
        [Key(6)]
        public long SendPortraitValue = 0;
        [Key(7)]
        public long SendUserGuildUID = 0; // 보내는 유저 길드 UID
        [Key(8)]
        public string SendUserGuildName = string.Empty; // 보내는 유저 길드 네임
        [Key(9)]
        public long SendUserLastEnterTimeStamp = 0; // 보내는 유저 마지막 접속시간
        [Key(10)]
        public long RecvUserUID = 0; // 받는 유저 유니크아이디
        [Key(11)]
        public string Message = string.Empty; // 메세지
        [Key(12)]
        public long CurrTimeStamp = 0; // 보낸시간


        public override string ToString()
        {
            return @$"CSChatMessage chatType: {this.ChatType}, server: {this.ServerTag}, sender: {this.SendUserUID}, senderName: {this.SendUserName}, senderIcon: {this.SendIcon}
                    , SendPortraitType: {this.SendPortraitType}, SendPortraitValue: {this.SendPortraitValue}, senderGuildId: {this.SendUserGuildUID}
                    , senderGuildName: {this.SendUserGuildName}, senderLastEnterTimestamp: {this.SendUserLastEnterTimeStamp}
                    , recver: {this.RecvUserUID}, msg: {this.Message}, currTime: {this.CurrTimeStamp}";
        }
    }
}

