using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCMove
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public sbyte MoveResult { get; set; }
        [Key(2)]
        public int ObjectId { get; set; }
        [Key(3)]
        public byte Status { get; set; }
        [Key(4)]
        public byte Direction { get; set; }
        [Key(5)]
        public Vec2Int Position { get; set; }
        [Key(6)]
        public long MoveTime { get; set; }
    }
}
