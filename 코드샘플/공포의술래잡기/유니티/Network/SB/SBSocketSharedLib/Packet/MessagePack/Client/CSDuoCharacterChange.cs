using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSDuoCharacterChange
    {
        [Key(0)]
        public byte CharacterType { get; set; }
        [Key(1)]
        public int SelectCharacterUID { get; set; }
    }
}
