using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCSystemMessageNotify
    {
        [Key(0)]
        public string Message { get; set; }
    }
}
