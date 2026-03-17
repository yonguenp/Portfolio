using System;
using System.Collections.Generic;
using System.Numerics;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
#if true
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcMoveUpdate
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public int ObjectId { get; set; }
        [Key(2)]
        public byte Status { get; set; }
        [Key(3)]
        public byte MoveStatus { get; set; }
        [Key(4)]
        public Vec2Float MoveDir { get; set; }
        [Key(5)]
        public Vec2Float Position { get; set; }
        [Key(6)]
        public long LastMoveTime { get; set; }
        //[Key(3)]
        //public byte Direction { get; set; }
        //[Key(4)]
        //public Vec2Int Position { get; set; }
        //[Key(5)]
        //public long LastMoveTime { get; set; }
    }
#else
    [ZeroFormattable]
    [Serializable]
    public class MoveUpdate
    {
        [Index(0)]
        public int ObjectId { get; set; }
        [Index(1)]
        public byte Direction { get; set; }
        [Index(2)]
        public Vec2Int Position { get; set; }
        [Index(3)]
        public long MoveUpdateTime { get; set; }
    }

    [ZeroFormattable]
    [Serializable]
    public class SCBcMoveUpdate
    {
        [Index(0)]
        public sbyte ErrorCode { get; set; }
        [Index(1)]
        public IList<MoveUpdate> MoveUpdate { get; set; }
    }
#endif
}
