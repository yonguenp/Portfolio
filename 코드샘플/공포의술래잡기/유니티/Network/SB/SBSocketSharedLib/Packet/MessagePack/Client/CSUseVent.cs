using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSUseVent
    {
        [Key(0)]
        public string VentId { get; set; } // Vehicle GameObject Id
        [Key(1)]
        public string PlayerId { get; set; } // Player GameObject Id
    }
}