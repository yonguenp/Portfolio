using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcEscapeEnd
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string EscapeId { get; set; }
        [Key(2)]
        public int InteractionTime { get; set; }
        [Key(3)]
        public string PlayerId { get; set; } // Player GameObject Id
    }

}
