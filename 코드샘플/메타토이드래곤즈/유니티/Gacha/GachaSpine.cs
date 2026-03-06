using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
	public enum eGachaSpineState
	{
		IDLE = 0,
		PLAY,
		HIT_IDLE,

		PET_IDLE,
		PET_PLAY,
		PET_HIT_IDLE
	}

	public class GachaSpine: SBUISpine<eGachaSpineState>
	{
		private eGachaType curType;
		const string DragonSkinName = "metod";
		const string PetSkinName = "pet";

		string skinName
		{
			get
			{
				switch (curType)
				{
					case eGachaType.DRAGON:
						return DragonSkinName;
					case eGachaType.PET:
						return PetSkinName;
				}

				return "";
			}
		}

		public eGachaSpineState CurAnimationState { get; private set; } = eGachaSpineState.IDLE;

		private bool isInit;

		public override void Init()
		{
			if(!isInit)
			{
				spineObj = gameObject;
				base.Init();
				isInit = true;
			}			
		}

		public void SetType(eGachaType type)
        {
			curType = type;
			SetState(eGachaSpineState.IDLE);
		}

		public void SetState(eGachaSpineState state)
		{
			CurAnimationState = state;

			bool loop = false;

			switch (state)
			{
				case eGachaSpineState.IDLE:
					loop = true;
					break;
			}

			SetSkin(skinName);
			Loop = loop;
			SetAnimation(state);
		}

		public TrackEntry GetTrackEntry()
		{
			return skeletonAni.AnimationState.GetCurrent(0);
		}

		public override TrackEntry SetAnimation(int trackIndex, string animName, bool loop)
		{
			if (skeletonAni == null || (skeletonAni.AnimationState.GetCurrent(0).Animation.Name == animName && skeletonAni.AnimationState.GetCurrent(0).Loop == loop))
				return null;

			return skeletonAni.AnimationState.SetAnimation(trackIndex, animName, loop);
		}

		public override void InitializeTypeFunc()
        {
			GetNameToType = AnimationNameToType;
			GetTypeToName = AnimationTypeToName;
			GetTypeToLoop = AnimationTypeToLoop;
		}

		public string AnimationTypeToName(eGachaSpineState anim)
        {
			return anim switch
			{
				eGachaSpineState.IDLE => "idle",
				eGachaSpineState.PLAY => "play",
				eGachaSpineState.HIT_IDLE => "result",

				eGachaSpineState.PET_IDLE => "idle",
				eGachaSpineState.PET_PLAY => "play",
				eGachaSpineState.PET_HIT_IDLE => "result",
				_ => "idle"
			};
		}
		public bool AnimationTypeToLoop(eGachaSpineState anim)
		{
			return anim switch
			{
				eGachaSpineState.IDLE => true,
				eGachaSpineState.PLAY => false,
				eGachaSpineState.HIT_IDLE => true,

				eGachaSpineState.PET_IDLE => true,
				eGachaSpineState.PET_PLAY => true,
				eGachaSpineState.PET_HIT_IDLE => true,
				_ => true
			};
		}

		public eGachaSpineState AnimationNameToType(string anim)
		{
			return anim switch
			{
				"idle" => eGachaSpineState.IDLE,
				"play" => eGachaSpineState.PLAY,
				"result" => eGachaSpineState.HIT_IDLE,

				//"play" => eGachaSpineState.PET_PLAY,
				//"result" => eGachaSpineState.PET_HIT_IDLE,
				_ => eGachaSpineState.PET_IDLE
			};
		}
	}
}
