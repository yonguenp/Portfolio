using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //
    [MessagePackObject]
    [Serializable]
    public class SCBcCreateGameRoom
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public RoomInfo RoomInfo { get; set; }
        [Key(2)]
        public int GameTimeLimit { get; set; }
        [Key(3)]
        public int EscapeTimeLimit { get; set; }
        [Key(4)]
        public long Timestamp { get; set; }


    }
}