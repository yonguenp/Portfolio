using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSReconnectRoom
    {
        [Key(0)]
        public long UserId { get; set; }
        [Key(1)]
        public string SessionToken { get; set; }
        [Key(2)]
        public bool Result { get; set; }
    }
}