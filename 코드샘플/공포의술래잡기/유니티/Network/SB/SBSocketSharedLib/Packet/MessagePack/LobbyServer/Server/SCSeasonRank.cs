using System;
using MessagePack;
using System.Collections.Generic;

namespace SBSocketSharedLib
{
	[MessagePackObject]
	[Serializable]
	public class SCSeasonRank
	{
		[Key(0)]
		public IList<SeasonRankUser> RankInfos { get; set; }
		[Key(1)]
		public SeasonRankUser MyRankInfo { get; set; }
	}
}

