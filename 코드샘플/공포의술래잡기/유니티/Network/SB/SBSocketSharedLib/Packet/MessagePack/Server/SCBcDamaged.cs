using System;
using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]

    public class SCBcDamaged
    {
        [Key(0)]
        public string TargetID { get; set; }       //타겟ID
        [Key(1)]
        public byte TargetShieldCnt { get; set; }  //타겟 쉴드카운트
        [Key(2)]
        public ushort TargetHp { get; set; }         //타겟 hp
        [Key(3)]
        public byte DamageType { get; set; }       //데미지 타입 ( 일반 공격, 힐 회복등 hp에 관한 타입으로 사용)
        [Key(4)]
        public ushort DamagePoint { get; set; }		//데미지 수치
        [Key(5)]
        public string AttackerID { get; set; }      //공격자 ID
    }

}
