using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSInit
    {
        [Key(0)]
        public long UserNo { get; set; }
        [Key(1)]
        public string SessionToken { get; set; }
        [Key(2)]
        public string Version { get; set; }
    }
}