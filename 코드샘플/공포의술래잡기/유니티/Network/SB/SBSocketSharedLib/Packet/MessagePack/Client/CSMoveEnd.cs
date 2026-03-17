using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSMoveEnd
    {
        [Key(0)]
        public byte Status { get; set; }
        [Key(1)]
        public byte MoveStatus { get; set; }
        [Key(2)]
        public Vec2Float MoveDir { get; set; }
        [Key(3)]
        public Vec2Float Position { get; set; }
    }
}
