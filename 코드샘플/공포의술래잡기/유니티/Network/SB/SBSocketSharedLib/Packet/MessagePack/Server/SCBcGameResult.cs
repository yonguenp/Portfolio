using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcGameResult
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public byte GameResult { get; set; }

    }
}
