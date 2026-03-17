using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcUnlockKey
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string KeyId { get; set; }
        [Key(2)]
        public byte UnlockStatus { get; set; }
        [Key(3)]
        public int InteractionTime { get; set; }
        [Key(4)]
        public byte UnlockedObjectCount { get; set; }
    }
}
