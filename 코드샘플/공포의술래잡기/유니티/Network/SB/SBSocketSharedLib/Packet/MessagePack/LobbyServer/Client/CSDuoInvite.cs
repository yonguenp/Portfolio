using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSDuoInvite
    {
        [Key(0)]
        public long GuestUserNo { get; set; }
        [Key(1)]
        public byte DuoType { get; set; } //듀오 타입
    }
}
