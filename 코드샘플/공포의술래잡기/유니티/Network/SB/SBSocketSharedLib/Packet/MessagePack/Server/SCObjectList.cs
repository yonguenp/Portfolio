using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCObjectList
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        //[Key(1)]
        //public byte RoomType { get; set; }
        [Key(1)]
        public IList<PlayerObjectInfo> ObjectList { get; set; }
    }
}
