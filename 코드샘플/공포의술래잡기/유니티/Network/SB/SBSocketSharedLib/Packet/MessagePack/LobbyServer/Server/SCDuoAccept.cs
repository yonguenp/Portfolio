using System;
using MessagePack;

namespace SBSocketSharedLib
{
	[MessagePackObject]
	[Serializable]
	public class SCDuoAccept
	{
		[Key(0)]
		public sbyte ErrorCode { get; set; }

	}
}
