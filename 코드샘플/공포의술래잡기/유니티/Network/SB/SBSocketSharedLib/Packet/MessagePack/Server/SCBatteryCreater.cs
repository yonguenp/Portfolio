using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBatteryCreater
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }

        [Key(1)]
        public sbyte FailReason { get; set; }
    }
}
