using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCOneOnOneMessage
    {
        [Key(0)]
        public long FromUserId { get; set; }
        [Key(1)]
        public string FromUserName { get; set; }
        [Key(2)]
        public long ToUserId { get; set; }
        [Key(3)]
        public string ToUserName { get; set; }
        [Key(4)]
        public string Message { get; set; }
    }
}
