using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCUserList
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        //[Key(1)]
        //public virtual byte RoomType { get; set; }
        [Key(1)]
        public IList<UserInfo> ObjectList { get; set; }
    }
}
