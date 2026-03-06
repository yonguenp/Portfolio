using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcEscapeRoom
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string ObjectId { get; set; }
    }
}