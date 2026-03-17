using System;
using MessagePack;


namespace SBSocketSharedLib
{

    [MessagePackObject]
    [Serializable]
    public class CSReqExerciseMatch
    {
        [Key(0)]
        public string NickName { get; set; }    //키값으로 쓰일 nickname

    }
}
