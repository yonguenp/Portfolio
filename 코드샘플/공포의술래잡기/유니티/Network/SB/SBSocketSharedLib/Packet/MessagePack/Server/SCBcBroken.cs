using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcBroken
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public PlayerObjectInfo AttackerInfo { get; set; }
        [Key(2)]
        public MapObjectInfo TargetInfo { get; set; }
    }
}
