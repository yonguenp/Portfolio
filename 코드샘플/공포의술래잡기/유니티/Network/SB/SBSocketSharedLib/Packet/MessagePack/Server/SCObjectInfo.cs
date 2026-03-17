using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCObjectInfo
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public PlayerObjectInfo ObjectInfo { get; set; }
    }
}
