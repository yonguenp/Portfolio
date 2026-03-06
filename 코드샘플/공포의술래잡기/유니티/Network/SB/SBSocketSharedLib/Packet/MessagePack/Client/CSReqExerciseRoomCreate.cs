using System;
using MessagePack;


namespace SBSocketSharedLib
{

    [MessagePackObject]
    [Serializable]
    public class CSReqExerciseRoomCreate
    {
        [Key(0)]
        public string NickName { get; set; }    //키값으로 쓰일 nickname
        [Key(1)]
        public string Password { get; set; }	//비번
        [Key(2)]
        public int MapNo { get; set; }          //테스트를 위한 맵번호
    }
}
