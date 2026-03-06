using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCClanChat
    {
		[Key(0)]
		public long UserNo { get; set; }        //유저 넘버
		[Key(1)]
		public long Time { get; set; }      //채팅 시간
		[Key(2)]
		public int RankPoint { get; set; }	//랭크포인트
		[Key(3)]
		public string Nick { get; set; }    //유저 닉
		[Key(4)]
		public string Message { get; set; } //메세지
	}
}


