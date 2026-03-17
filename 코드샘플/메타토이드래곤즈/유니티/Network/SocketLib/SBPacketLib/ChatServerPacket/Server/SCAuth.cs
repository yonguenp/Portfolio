using System;
using MessagePack;

namespace SBPacketLib
{
    [MessagePackObject]
    public class SCAuth
    {
        [Key(0)]
        public ResultCode Result = 0;     //성공여부
        [Key(1)]
        public ErrorCode ErrorCode = 0;     //에러코드
    }
}

