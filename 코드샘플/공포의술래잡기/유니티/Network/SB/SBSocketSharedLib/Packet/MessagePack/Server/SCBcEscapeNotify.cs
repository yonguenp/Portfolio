using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcEscapeNotify
    {

        [Key(0)]
        public IList<KeyValuePair<string, int>> EscapeInfos { get; set; }
    }
}
