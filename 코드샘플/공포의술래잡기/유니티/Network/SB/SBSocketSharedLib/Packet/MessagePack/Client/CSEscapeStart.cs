using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSEscapeStart
    {
        [Key(0)]
        public string EscapeId { get; set; }
    }
}