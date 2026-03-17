using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSDuoGuestMatch
    {
        [Key(0)]
        public long HostUserNo { get; set; }
    }
}
