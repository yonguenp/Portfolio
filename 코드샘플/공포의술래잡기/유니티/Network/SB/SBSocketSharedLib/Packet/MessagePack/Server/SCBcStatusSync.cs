using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcStatusSync
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public StatInfo StatInfo { get; set; }
    }
}
