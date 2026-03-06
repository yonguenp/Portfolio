using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcUnlockEscape
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string EscapeId { get; set; }
        [Key(2)]
        public byte UnlockStatus { get; set; }
        [Key(3)]
        public int InteractionTime { get; set; }
        [Key(4)]
        public long EscapeDoorOpenTime { get; set; }
    }
}
