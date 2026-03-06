using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCSkillCasting
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }
        [Key(1)]
        public int SkillId { get; set; }
        [Key(2)]
        public sbyte AttackFailReason { get; set; }
    }
}
