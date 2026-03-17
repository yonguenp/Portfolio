using System;
using MessagePack;
using System.Collections.Generic;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSDuoMatch
    {
        [Key(0)]
        public IList<long> UserNos { get; set; }
    }
}

