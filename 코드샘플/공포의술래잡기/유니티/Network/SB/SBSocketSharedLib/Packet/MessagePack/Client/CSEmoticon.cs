using System;
using MessagePack;


namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSEmoticon
    {
        [Key(0)]
        public ushort EmoticonId { get; set; }
    }
}
