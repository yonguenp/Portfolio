using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIPetSpine : SBUISpine<eSpineAnimation>
	{
		public override void InitializeTypeFunc()
		{
			GetTypeToLoop = IsTypeToLoop;
			GetTypeToName = GetAnimName;
			GetNameToType = GetAnimType;
		}

		public override Spine.TrackEntry SetAnimation(string animName)
		{
			bool loop = false;
			switch (animName)
			{
				case "death":
				{
					Animation = eSpineAnimation.DEATH;
				}
				break;
				default:
				{
					Animation = eSpineAnimation.IDLE;
					loop = true;
				}
				break;
			}

			return base.SetAnimation(0, animName, loop);
		}

		public string GetAnimName(eSpineAnimation anim)
		{
			return anim switch
			{
				eSpineAnimation.DEATH => "death",
				_ => "stop"
			};
		}

		public eSpineAnimation GetAnimType(string anim)
		{
			return anim switch
			{
				"death" => eSpineAnimation.DEATH,
				_ => eSpineAnimation.IDLE
			};
		}
		public bool IsTypeToLoop(eSpineAnimation anim)
		{
			return anim switch
			{
				eSpineAnimation.NONE => true,
				eSpineAnimation.IDLE => true,
				_ => false
			};
		}
	}
}
