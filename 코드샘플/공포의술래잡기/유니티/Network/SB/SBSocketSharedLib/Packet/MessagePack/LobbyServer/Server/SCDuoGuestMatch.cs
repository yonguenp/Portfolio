using System;
using MessagePack;

namespace SBSocketSharedLib
{
	[MessagePackObject]
	[Serializable]
	public class SCDuoGuestMatch
	{
		[Key(0)]
		public sbyte ErrorCode { get; set; }	
	}
}

