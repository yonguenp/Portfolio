using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class CSMatch
    {
        [Key(0)]
        public byte OpCode { get; set; } // 0: 매치 요청, 1: 매치 취소
    }
}