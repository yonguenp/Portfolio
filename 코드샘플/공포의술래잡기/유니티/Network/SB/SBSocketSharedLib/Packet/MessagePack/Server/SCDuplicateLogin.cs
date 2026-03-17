using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCDuplicateLogin
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public long UserId { get; set; }
    }
}