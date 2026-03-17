using System;
using MessagePack;
using System.Collections.Generic;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcProjectileDespawn
    {
        [Key(0)]
        public IList<DelGameObject> DelObjects { get; set; }
    }

}
