using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class LCGameServerEnter
    {
        [Key(0)]
        public string GameServerIP { get; set; }
        [Key(1)]
        public short GameServerPort { get; set; }
    }
}
