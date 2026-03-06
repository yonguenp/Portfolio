using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSGetOffVehicle
    {
        [Key(0)]
        public string PlayerId { get; set; } // Player GameObject Id
    }
}