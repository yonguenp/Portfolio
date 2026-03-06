using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcMatch
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public byte Result { get; set; } // 0: 매치 성공, 1: 매치 실패, 2: 요청 시간 초과, 3: 매치 취소, 4: 진행 중, 5: 매칭 룸 해산
        [Key(2)]
        public RoomInfo RoomInfo { get; set; }
        [Key(3)]
        public int KillCount { get; set; }
        [Key(4)]
        public int MatchTimeLimit { get; set; }
        [Key(5)]
        public int EscapeTimeLimit { get; set; }
        [Key(6)]
        public long Timestamp { get; set; }
        [Key(7)]
        public int UnlockedObjectCount { get; set; }

    }
}