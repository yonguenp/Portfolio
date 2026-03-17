using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSMatchSelectCharacter
    {
        [Key(0)]
        public int CharacterUID { get; set; }
    }
}
