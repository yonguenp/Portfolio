using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSBatteryCreater
    {
        [Key(0)]
        public string GameObjectId { get; set; }
    }
}
