using System;
using System.Collections.Generic;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public class SCBcMatchInfo
    {
        [Key(0)]
        public byte Result { get; set; } // 0: 매치 성공, 1: 매치 실패, 2: 요청 시간 초과, 3: 매치 취소, 4: 진행 중, 5: 매칭 룸 해산
        [Key(1)]
        public byte MatchLimit { get; set; }
        [Key(2)]
        public byte GameRoomType { get; set; } // 1 : 랭크게임, 2 : 연습게임
        [Key(3)]
        public IList<MatchPlayerInfo> MatchPlayerInfos { get; set; }

    }
}