using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCNotifyReconnect
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public long UserId { get; set; }
        [Key(2)]
        public string GameServerIP { get; set; }
        [Key(3)]
        public short GameServerPort { get; set; }
    }
}