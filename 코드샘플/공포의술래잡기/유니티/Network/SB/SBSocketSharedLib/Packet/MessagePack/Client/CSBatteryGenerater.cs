using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSBatteryGenerater
    {
        [Key(0)]
        public string GameObjectId { get; set; }
    }
}
