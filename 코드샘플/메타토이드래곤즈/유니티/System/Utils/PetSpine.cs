using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PetSpine : SBSpine<eSpineAnimation>
    {
		private PetBaseData data = null;
		public void SetData(PetBaseData data)
        {
			this.data = data;
        }
		override public void Init()
		{
			base.Init();
			if(data != null)
			{
				if (SkData == null)
					SkData = skeletonAni.skeletonDataAsset.GetSkeletonData(true);

				SetAnimation(eSpineAnimation.IDLE);
				SetSkin(data.SKIN);
			}
		}

		public override void InitializeTypeFunc()
		{
			GetTypeToLoop = IsTypeToLoop;
			GetTypeToName = GetAnimName;
			GetNameToType = GetAnimType;
			GetTypeToSkip = IsTypeToSkip;
		}

		public string GetAnimName(eSpineAnimation anim)
        {
			return anim switch
			{
				eSpineAnimation.DEATH => "death",
				_ => "idle"
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
		public bool IsTypeToSkip(eSpineAnimation anim1, eSpineAnimation anim2)
		{
			return anim1 switch
			{
				eSpineAnimation.DEATH => true,
				_ => false
			};
		}
	}
}
