using System;
using System.Collections.Generic;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCBcGamePoint
    {
        [Key(0)]
        public string PlayerId { get; set; }
        [Key(1)]
        public IList<KeyValuePair<int, int>> RewardInfos { get; set; } //점수 정보 key : reward_point테이블 UID, value : 횟수
    }
}

