using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcDisconnect
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public long UserId { get; set; }
        [Key(2)]
        public byte Reason { get; set; }
    }
}