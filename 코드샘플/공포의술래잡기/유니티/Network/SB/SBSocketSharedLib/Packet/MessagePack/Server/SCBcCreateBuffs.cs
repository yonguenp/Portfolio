using System;
using System.Collections.Generic;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcCreateBuffs
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public IList<int> EffectIds { get; set; } //이펙트 아이디
        [Key(2)]
        public Vec2Float Direction { get; set; }    //방향
    }
}
