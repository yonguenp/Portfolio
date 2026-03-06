using System;
using MessagePack;

namespace SBPacketLib
{
    [MessagePackObject]
    public class CSAuth
    {
        [Key(0)]
        public int ServerTag = -1; // 접속 서버
        [Key(1)]
        public long UserUID = 0; // 유저 유니크아이디
    }
}

