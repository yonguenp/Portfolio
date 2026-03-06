using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcEscapeStart
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }

        [Key(1)]
        public string PlayerId { get; set; } // Player GameObject Id

    }
}
