using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcGameReady
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public RoomPlayerInfo RoomPlayerInfo { get; set; }
        //public IList<RoomPlayerInfo> RoomPlayerInfos { get; set; }
    }
}