using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcEnterLobby
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public UserInfo UserInfo { get; set; }
    }
}
