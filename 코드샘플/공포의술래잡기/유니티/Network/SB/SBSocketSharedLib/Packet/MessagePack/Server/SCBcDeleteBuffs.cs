using System;
using System.Collections.Generic;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcDeleteBuffs
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public IList<int> EffectIds { get; set; } //이펙트 아이디
    }
}
