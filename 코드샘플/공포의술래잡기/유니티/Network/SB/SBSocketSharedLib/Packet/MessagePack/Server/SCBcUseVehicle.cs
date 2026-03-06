using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcUseVehicle
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string VehicleId { get; set; } // Vehicle GameObject Id
        [Key(2)]
        public string PlayerId { get; set; } // Player GameObject Id
    }
}
