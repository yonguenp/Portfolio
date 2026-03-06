using System;
using System.Collections.Generic;
using System.Text;

namespace SBPacketLib
{

    public enum ResultCode : byte
    {
        Failed = 0,     //실패
        Success = 1,    //성공
    }
    public enum ErrorCode : byte
    {
        None = 0,

    }

    public enum eChatCommentType : byte
    {
        None = 0,
        World = 1 << 0,
        SystemMsg = 1 << 1,
        Guild = 1 << 2,
        Whisper = 1 << 3,
    }

    public enum ChatServer_PacketID : int
    {
        CSAuth = 1000,  //인증
        SCAuth,

        CSPing,
        SCPong,

        CSChatMessage,   //채팅
        SCChatMessage,   //채팅
    }

}
