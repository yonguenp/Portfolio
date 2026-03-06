using System;
using System.Collections.Generic;

using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcBatteryPickUp
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public int BatteryCnt { get; set; } //플레이어가 가진 개수
        [Key(2)]
        public IList<string> GameObjectIds { get; set; }    //사라지는 오브젝트 아이디
    }
}
