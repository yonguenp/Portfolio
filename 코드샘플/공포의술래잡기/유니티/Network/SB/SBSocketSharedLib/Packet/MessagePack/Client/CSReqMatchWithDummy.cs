using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSReqMatchWithDummy
    {
        [Key(1)]
        public bool IsChaser { get; set; }
    }
}
