using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcSpawn
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public IList<RespawnObjectInfo> RespawnObjectList { get; set; }
        //[Key(2)]
        //public Vec2Float MoveDir { get; set; }
        //[Key(3)]
        //public Vec2Float Position { get; set; }
        //public PlayerObjectInfo ObjectInfo { get; set; }
        //[Key(2)]
        //public int SpawnPoint { get; set; }
    }
}
