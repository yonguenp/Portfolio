using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcStateChanged
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
#if false
        public IList<ChangedStat> ChangedState { get; set; }
#else
        //public IList<byte> ChangedState { get; set; }
        public byte ChangedState { get; set; }
#endif
        [Key(2)]
        public PlayerObjectInfo ObjectInfo { get; set; }
    }
}
