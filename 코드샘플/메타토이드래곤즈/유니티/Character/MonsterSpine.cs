using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class MonsterSpine : SBSpine<eSpineAnimation>
	{
		public override void Init()
		{
			base.Init();
		}

		public override void InitializeTypeFunc()
		{
			GetTypeToLoop = SBFunc.IsTypeToLoop;
			GetTypeToName = SBDefine.GetMonsterAnimTypeToName;
			GetNameToType = SBDefine.GetMonsterAnimNameToType;
		}

		public override Spine.TrackEntry SetAnimation(eSpineAnimation anim)
		{
			if (Animation == anim)
				return null;

			switch (Animation)
			{
				case eSpineAnimation.ATTACK:
				case eSpineAnimation.SKILL:
				{
					if (anim == eSpineAnimation.WALK ||
						anim == eSpineAnimation.IDLE)
						return null;
				}
				break;
			}

			Animation = anim;

			return base.SetAnimation(anim);
		}
    }
}