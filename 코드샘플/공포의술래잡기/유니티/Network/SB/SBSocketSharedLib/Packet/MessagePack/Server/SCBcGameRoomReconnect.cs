using System;
using MessagePack;

namespace SBSocketSharedLib
{

    [MessagePackObject]
    [Serializable]
    public class SCBcGameRoomReconnect
    {
        [Key(0)]
        public PlayerObjectInfo PlayerInfo { get; set; }        //새롭게 들어온 유저 정보
    }
}
