using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCResExerciseRoomCreate
    {
        [Key(0)]
        public sbyte ErrorCode { get; set; }

    }
}
