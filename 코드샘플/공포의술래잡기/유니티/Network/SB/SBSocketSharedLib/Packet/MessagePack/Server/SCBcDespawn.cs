using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcDespawn
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string ObjectId { get; set; }
        [Key(2)]
        public Vec2Float MoveDir { get; set; }
        [Key(3)]
        public Vec2Float Position { get; set; }
        [Key(4)]
        public long RespawnTime { get; set; }
    }
}
