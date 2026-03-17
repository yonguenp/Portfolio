using DG.Tweening;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public enum eProductionCapsuleSpineState
    {
        IDLE_NORMAL = 0,
        IDLE_RARE,

        OPEN_NORMAL,
        OPEN_RARE,
        OPEN_UNIQUE,
        OPEN_LEGENDARY,//유니크 볼에서 레전

        HIT_NORMAL,//oneTake spine 끝쪽에서 처리
        HIT_RARE,
    }
    public class ProductionCapsuleSpine : SBUISpine<eProductionCapsuleSpineState>
    {
		public eProductionCapsuleSpineState CurAnimationState { get; private set; } = eProductionCapsuleSpineState.IDLE_NORMAL;

		private bool isInit;

		Action successInitCallback = null;
		public Action SuccessInitCallback { set { successInitCallback = value; } }
		
		public override void Init()
		{
			if (!isInit)
			{
				spineObj = gameObject;
				base.Init();
				isInit = true;

				if (successInitCallback != null)
					successInitCallback();
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		/// <param name="_instancingSpineCallback"></param>//캡슐 터지면서 중간에 드래곤, 펫 스파인 생성 콜백
		/// <param name="_animationDoneCallback"></param>//애니메이션 연출 끝난 콜백
		public void SetState(eProductionCapsuleSpineState state,Action _instancingSpineCallback = null ,Action _animationDoneCallback = null)
		{
			CurAnimationState = state;

			bool loop = false;

			switch (state)
			{
				case eProductionCapsuleSpineState.IDLE_NORMAL:
				case eProductionCapsuleSpineState.IDLE_RARE:
					loop = true;
					break;
			}

			Loop = loop;
			SetAnimation(state);

			if (_animationDoneCallback == null)
				return;

			if(!loop)//loop가 아닐때만 _animationDoneCallback 실행하게 
			{
				var animCompleteCheck = DOTween.Sequence();
				float spineInstanceDiff = 0.3f;
				var animationLength = GetAnimaionTime(state) - spineInstanceDiff;//끝나고 스파인(펫, 드래곤)생성 시 프레임 비는 것 방지
				if (animationLength <= 0)
					animationLength = 0f;
				animCompleteCheck.AppendInterval(animationLength).AppendCallback(() => {
					if (_instancingSpineCallback != null)
						_instancingSpineCallback();
				}).AppendInterval(spineInstanceDiff).AppendCallback(()=> {
					if (_animationDoneCallback != null)
						_animationDoneCallback();
				});
				animCompleteCheck.Play();
            }
		}

		public TrackEntry GetTrackEntry()
		{
			return skeletonAni.AnimationState.GetCurrent(0);
		}

		public override TrackEntry SetAnimation(int trackIndex, string animName, bool loop)
		{
			//if (skeletonAni == null || (skeletonAni.AnimationState.GetCurrent(0).Animation.Name == animName && skeletonAni.AnimationState.GetCurrent(0).Loop == loop))
			//	return null;

			return skeletonAni.AnimationState.SetAnimation(trackIndex, animName, loop);
		}

		public override void InitializeTypeFunc()
		{
			GetNameToType = AnimationNameToType;
			GetTypeToName = AnimationTypeToName;
			GetTypeToLoop = AnimationTypeToLoop;
		}

		public string AnimationTypeToName(eProductionCapsuleSpineState anim)
		{
			return anim switch
			{
				eProductionCapsuleSpineState.IDLE_NORMAL => "idle_normal",
				eProductionCapsuleSpineState.IDLE_RARE => "idle_rare",

				eProductionCapsuleSpineState.OPEN_NORMAL => "open_normal",
				eProductionCapsuleSpineState.OPEN_RARE => "open_rare",
				eProductionCapsuleSpineState.OPEN_UNIQUE => "open_unique",
				eProductionCapsuleSpineState.OPEN_LEGENDARY => "open_legendary",

				eProductionCapsuleSpineState.HIT_NORMAL => "hit_normal",
				eProductionCapsuleSpineState.HIT_RARE => "hit_rare",
				_ => "idle_rare"
			};
		}
		public bool AnimationTypeToLoop(eProductionCapsuleSpineState anim)
		{
			return anim switch
			{
				eProductionCapsuleSpineState.IDLE_NORMAL => true,
				eProductionCapsuleSpineState.IDLE_RARE => true,

				eProductionCapsuleSpineState.OPEN_NORMAL => false,
				eProductionCapsuleSpineState.OPEN_RARE => false,
				eProductionCapsuleSpineState.OPEN_UNIQUE => false,
				eProductionCapsuleSpineState.OPEN_LEGENDARY => false,

				eProductionCapsuleSpineState.HIT_NORMAL => false,
				eProductionCapsuleSpineState.HIT_RARE => false,
				_ => true
			};
		}

		public eProductionCapsuleSpineState AnimationNameToType(string anim)
		{
			return anim switch
			{
				"idle_normal" => eProductionCapsuleSpineState.IDLE_NORMAL,
				"idle_rare" => eProductionCapsuleSpineState.IDLE_RARE,

				"open_normal" => eProductionCapsuleSpineState.OPEN_NORMAL,
				"open_rare" => eProductionCapsuleSpineState.OPEN_RARE,
				"open_unique" => eProductionCapsuleSpineState.OPEN_UNIQUE,
				"open_legendary" => eProductionCapsuleSpineState.OPEN_LEGENDARY,

				"hit_normal" => eProductionCapsuleSpineState.HIT_NORMAL,
				"hit_rare" => eProductionCapsuleSpineState.HIT_RARE,
				_ => eProductionCapsuleSpineState.IDLE_RARE
			};
		}

		public eProductionCapsuleSpineState GetOpenAnimationStateByGrade(int _grade, bool _isGacha = true)
        {
			switch(_grade)
            {
				case (int)eDragonGrade.Normal:
				case (int)eDragonGrade.Uncommon:
					return eProductionCapsuleSpineState.OPEN_NORMAL;
				case (int)eDragonGrade.Rare:
					return eProductionCapsuleSpineState.OPEN_RARE;
				case (int)eDragonGrade.Unique:
					return eProductionCapsuleSpineState.OPEN_UNIQUE;
				case (int)eDragonGrade.Legend:
					return eProductionCapsuleSpineState.OPEN_LEGENDARY;
				default:
					return eProductionCapsuleSpineState.OPEN_NORMAL;
			}
		}

		public eProductionCapsuleSpineState GetIdleAnimationByGrade(int _grade)
        {
			return _grade <= (int)eDragonGrade.Uncommon ? eProductionCapsuleSpineState.IDLE_NORMAL : eProductionCapsuleSpineState.IDLE_RARE;
		}


	}
}