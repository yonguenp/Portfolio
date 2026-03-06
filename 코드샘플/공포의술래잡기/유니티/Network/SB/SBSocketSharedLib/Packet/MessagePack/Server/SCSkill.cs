using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCSkill
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public int SkillId { get; set; }
        [Key(2)]
        public sbyte AttackFailReason { get; set; }
        [Key(3)]
        public byte CurrState { get; set; }
    }
}
