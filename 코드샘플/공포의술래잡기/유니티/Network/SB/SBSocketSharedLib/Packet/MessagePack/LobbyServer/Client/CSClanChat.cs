using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSClanChat
    {
        [Key(0)]
        public string Message { get; set; }	//메세지
    }
}

