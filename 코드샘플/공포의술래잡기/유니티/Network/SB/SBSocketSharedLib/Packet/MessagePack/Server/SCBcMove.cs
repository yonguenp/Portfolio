using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcMove
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public string ObjectId { get; set; }
        [Key(2)]
        public byte Status { get; set; }
        [Key(3)]
        public byte MoveStatus { get; set; }
        [Key(4)]
        public Vec2Float MoveDir { get; set; }
        [Key(5)]
        public Vec2Float Position { get; set; }
        [Key(6)]
        public bool ForcePos { get; set; }
    }
}
