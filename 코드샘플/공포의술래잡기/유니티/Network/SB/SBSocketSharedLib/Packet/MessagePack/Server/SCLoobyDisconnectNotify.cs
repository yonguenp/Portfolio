using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCLoobyDisconnectNotify
    {
        [Key(0)]
        public byte Reason { get; set; }
    }
}
