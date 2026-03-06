using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcLeaveGame
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string GameObjectId { get; set; }
    }
}
