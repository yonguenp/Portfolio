using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class CSReqMatchCancel
    {
        //[Key(0)]
        //public byte MatchType { get; set; }     //매칭 타입 1 : 랭크전, 2 : 연습매칭
    }
}