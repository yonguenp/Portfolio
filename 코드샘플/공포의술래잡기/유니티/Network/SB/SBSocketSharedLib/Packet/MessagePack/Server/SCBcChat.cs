using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcChat
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string Nick { get; set; }
        [Key(2)]
        public string Message { get; set; }
    }
}
