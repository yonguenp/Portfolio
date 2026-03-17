using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcGetOffVehicle
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string PlayerId { get; set; } // Player GameObject Id
    }
}
