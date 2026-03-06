using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSMove
    {
#if true
        [Key(0)]
        public byte Status { get; set; }
        [Key(1)]
        public byte MoveStatus { get; set; }
        [Key(2)]
        public Vec2Float MoveDir { get; set; }
        [Key(3)]
        public Vec2Float Position { get; set; }
#else
        [Key(0)]
        public PositionInfo PosInfo { get; set; }
        [Key(1)]
        public long MoveTime { get; set; }
#endif
    }
}
