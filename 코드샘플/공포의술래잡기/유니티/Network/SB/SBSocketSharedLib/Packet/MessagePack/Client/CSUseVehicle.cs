using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSUseVehicle
    {
        [Key(0)]
        public string VehicleId { get; set; } // Vehicle GameObject Id
        [Key(1)]
        public string PlayerId { get; set; } // Player GameObject Id
    }
}