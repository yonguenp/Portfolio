using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCResMatch
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
       
    }
}