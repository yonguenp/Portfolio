using System;
using MessagePack;
using System.Collections.Generic;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCFriendList
    {
        [Key(0)]
        public IList<FriendInfo> FriendInfos { get; set; }
    }
}

