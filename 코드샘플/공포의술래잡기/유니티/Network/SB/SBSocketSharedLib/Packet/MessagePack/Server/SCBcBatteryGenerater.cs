using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcBatteryGenerater
    {
        [Key(0)]
        public string PlayerId { get; set; }    //배터리 넣은 플레이어 아이디
        [Key(1)]
        public int ChargeCnt { get; set; }      //차지한 배터리 개수
        [Key(2)]
        public string GameObjectId { get; set; }    //배터리 넣은 오브젝트 아이디
        [Key(3)]
        public int CollectCnt { get; set; }         //모아진 배터리수

    }
}
