using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSDuoAccept
    {
        [Key(0)]
        public byte Response { get; set; }  //0 : 취소, 1 : 수락
        [Key(1)]
        public long HostUserNo { get; set; }
        [Key(2)]
        public byte DuoType { get; set; } //듀오 타입
    }
}

