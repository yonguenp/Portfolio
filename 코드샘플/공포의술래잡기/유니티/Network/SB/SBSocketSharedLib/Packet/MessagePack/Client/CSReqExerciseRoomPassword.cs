using System;
using MessagePack;


namespace SBSocketSharedLib
{

    [MessagePackObject]
    [Serializable]
    public class CSReqExerciseRoomPassword
    {
        [Key(0)]
        public string NickName { get; set; }    //키값으로 쓰일 nickname
        [Key(1)]
        public string Password { get; set; }	//비번
    }
}
