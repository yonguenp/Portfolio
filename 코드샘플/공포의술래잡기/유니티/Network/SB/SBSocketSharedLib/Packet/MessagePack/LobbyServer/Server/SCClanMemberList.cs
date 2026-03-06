using System;
using MessagePack;
using System.Collections.Generic;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCClanMemberList
    {
        [Key(0)]
        public IList<ClanMemberInfo> GuildMemberInfos { get; set; }
    }
}

