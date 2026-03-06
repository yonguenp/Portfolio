using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcSkillCasting
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public int SkillId { get; set; }
        [Key(2)]
        public Vec2Float Position { get; set; }         //스킬 사용할때의 현재 위치 체크를 위해서
        [Key(3)]
        public Vec2Float SkillDir { get; set; }			//스킬 방향

    }
}
