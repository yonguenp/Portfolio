using System;
using System.Collections.Generic;

using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcBatteryDrop
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public IList<DropInfo> DropGameObjects { get; set; }

    }
}
