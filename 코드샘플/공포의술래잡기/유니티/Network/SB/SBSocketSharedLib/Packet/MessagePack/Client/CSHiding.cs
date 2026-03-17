using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSHiding
    {
        [Key(0)]
        public string HidingId { get; set; }
    }
}
