using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcBatteryCreate
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public int BatteryCnt { get; set; } //플레이어가 가진 개수
        [Key(2)]
        public string GameObjectId { get; set; }    //배터리가 생성된 오브젝트 아이디
    }
}
