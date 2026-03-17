using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcProjectileSpawn
    {

        [Key(0)]
        public string GameObjectId { get; set; }
        [Key(1)]
        public string OwnerId { get; set; }
        [Key(2)]
        public int Skill_SummonId { get; set; }
        [Key(3)]
        public Vec2Float Position { get; set; }         //위치
        [Key(4)]
        public Vec2Float SkillDir { get; set; }			//방향

    }
}
