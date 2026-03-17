using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcEnterGame
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public long Timestamp { get; set; }
        [Key(2)]
        public long OwnerId { get; set; }
        [Key(3)]
        public PlayerObjectInfo ObjectInfo { get; set; }
        //[Index(4)]
        //public int PosX { get; set; }
        //[Index(5)]
        //public int PosY { get; set; }
        //[Index(6)]
        //public int PosZ { get; set; }
    }
}
