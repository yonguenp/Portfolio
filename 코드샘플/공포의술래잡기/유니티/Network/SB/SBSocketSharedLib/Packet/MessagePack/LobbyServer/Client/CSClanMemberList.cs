using System;
using MessagePack;
using System.Collections.Generic;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSClanMemberList
    {
        [Key(0)]
        public IList<long> MemberInfos { get; set; }
    }
}

