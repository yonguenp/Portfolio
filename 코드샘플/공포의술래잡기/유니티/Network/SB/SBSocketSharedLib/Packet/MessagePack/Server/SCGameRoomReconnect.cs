using System;
using MessagePack;
using System.Collections.Generic;

namespace SBSocketSharedLib
{
    [MessagePackObject]
    [Serializable]
    public class SCGameRoomReconnect
    {
		[Key(0)]
		public long GameStartTime { get; set; }						//게임 시작시간
		[Key(1)]
		public long CurrGameTime { get; set; }						//게임 현재시간
		[Key(2)]
		public long EscapeDoorOpenTime { get; set; }			//출구 오픈 시간
		[Key(3)]
		public IList<RePlayerObjectInfo> PlayerInfos { get; set; }	//플레이어 정보
		[Key(4)]
		public IList<ReMapObjectInfo> MapObjectInfos { get; set; }	//오브젝트 정보
		[Key(5)]
		public IList<KeyValuePair<string, int>> GeneraterBatteryInfos { get; set; }				//발전기에 들어가져 있는 개수
		[Key(6)]
		public IList<ReSkillObjectInfo> SkillObjectInfos { get; set; }	//스킬 오브젝트 정보
	}
}
