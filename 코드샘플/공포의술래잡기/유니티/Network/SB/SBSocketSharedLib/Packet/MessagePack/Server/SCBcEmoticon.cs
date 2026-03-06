using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcEmoticon
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public ushort EmoticonId { get; set; }
    }
}
