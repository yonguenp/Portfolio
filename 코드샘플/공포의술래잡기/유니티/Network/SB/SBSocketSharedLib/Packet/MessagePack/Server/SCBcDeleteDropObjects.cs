using System;
using System.Collections.Generic;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcDeleteDropObjects
    {

        [Key(0)]
        public IList<string> GameObjects { get; set; } //삭제 되는 리스트 아이디
    }
}
