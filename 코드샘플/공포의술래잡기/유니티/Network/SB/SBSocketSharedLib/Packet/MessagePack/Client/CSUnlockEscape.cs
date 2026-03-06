using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSUnlockEscape
    {
        [Key(0)]
        public string EscapeId { get; set; }
        [Key(1)]
        public int InteractionTime { get; set; }
    }
}
