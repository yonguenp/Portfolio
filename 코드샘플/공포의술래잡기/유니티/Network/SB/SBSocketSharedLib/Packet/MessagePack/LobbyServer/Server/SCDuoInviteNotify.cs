using System;
using MessagePack;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCDuoInviteNotify
    {
		[Key(0)]
		public long HostUserNo { get; set; }
		[Key(1)]
		public string NickName { get; set; }
		[Key(2)]
		public int ChaserUID { get; set; }
		[Key(3)]
		public int SurvivorUID { get; set; }
		[Key(4)]
		public int RankPoint { get; set; }
		[Key(5)]
		public byte DuoType { get; set; } //듀오 타입
	}
}
