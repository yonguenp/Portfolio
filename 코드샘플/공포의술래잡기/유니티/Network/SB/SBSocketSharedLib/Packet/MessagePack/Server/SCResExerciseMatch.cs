using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCResExerciseMatch
    {
        [Key(0)]
        public byte Result { get; set; }	//enum matchResult

    }
}

