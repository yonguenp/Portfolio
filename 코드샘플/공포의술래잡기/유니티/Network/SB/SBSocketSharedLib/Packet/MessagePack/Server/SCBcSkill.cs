using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //
    [MessagePackObject]
    [Serializable]
    public class SCBcSkill
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public int SkillId { get; set; }
        [Key(2)]
        public Vec2Float Position { get; set; }
        [Key(3)]
        public Vec2Float SkillDir { get; set; }
    }

}
