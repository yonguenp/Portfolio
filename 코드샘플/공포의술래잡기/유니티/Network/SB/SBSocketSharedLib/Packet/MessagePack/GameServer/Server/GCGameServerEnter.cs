using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class GCGameServerEnter
    {
        [Key(0)]
        public byte Result { get; set; }		//접속 성공 여부
    }
}
