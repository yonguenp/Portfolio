using System;
using MessagePack;

namespace SBPacketLib
{
    [MessagePackObject]
    public class SCPong
    {
        [Key(0)]
        public int ServerTag = -1; // 접속 서버
        [Key(1)]
        public long UserUID = 0; // 유저 유니크아이디
        [Key(2)]
        public long Timestamp = 0;
    }
}

