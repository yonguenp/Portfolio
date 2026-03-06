using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCResMatchCancel
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public byte Reason { get; set; }
    }
}