using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcGameStart
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public long Timestamp { get; set; }
        //[Key(2)]
        //public string OwnerId { get; set; }
        //[Key(3)]
        //public PlayerObjectInfo ObjectInfo { get; set; }
        //[Key(4)]
        //public CharacterInfo CharacterInfo { get; set; }
        //[Key(4)]
        //public int PosX { get; set; }
        //[Key(5)]
        //public int PosY { get; set; }
        //[Key(6)]
        //public int PosZ { get; set; }
    }
}
