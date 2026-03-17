using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCInit
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public int Result { get; set; }
        [Key(2)]
        public long UserNo { get; set; }
    }
}