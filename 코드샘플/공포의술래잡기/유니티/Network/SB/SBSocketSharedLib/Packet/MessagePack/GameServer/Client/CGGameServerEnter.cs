using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CGGameServerEnter
    {
        [Key(0)]
        public long UserNo { get; set; }
        [Key(1)]
        public string SessionToken { get; set; }
    }
}
