using System;
using MessagePack;

namespace SBSocketSharedLib
{
	[MessagePackObject]
	[Serializable]
	public class SCDuoAcceptNotify
	{
		[Key(0)]
		public byte Response { get; set; }  //0 : 취소, 1 : 수락
		[Key(1)]
		public long GuestUserNo { get; set; }
		[Key(2)]
		public string NickName { get; set; }
		[Key(3)]
		public int ChaserUID { get; set; }
		[Key(4)]
		public int SurvivorUID { get; set; }
		[Key(5)]
		public int RankPoint { get; set; }
		[Key(6)]
		public byte DuoType { get; set; } //듀오 타입
	}
}

