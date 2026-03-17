using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSKeyStart
    {
        [Key(0)]
        public string KeyId { get; set; }
    }
}