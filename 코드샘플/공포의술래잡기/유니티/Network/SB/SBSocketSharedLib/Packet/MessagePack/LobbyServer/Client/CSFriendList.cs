using System;
using MessagePack;
using System.Collections.Generic;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSFriendList
    {
        [Key(0)]
        public IList<long> FriendInfos { get; set; }
    }
}

