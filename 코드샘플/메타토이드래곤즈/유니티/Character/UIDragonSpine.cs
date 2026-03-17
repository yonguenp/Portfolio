

using Coffee.UIExtensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public class UIDragonSpine: SBUISpine<eSpineAnimation>
	{
		public UIParticle TranscendParent = null;
		public bool ApplyRandomFrame = true;

		public delegate void func(UIDragonSpine data = null);
		private func initComplete;
		public func InitComplete
		{
			get { return initComplete; }
			set { initComplete = value; }
		}

		public ICharacterBaseData Data { get; protected set; } = null;
		
		private int skillAnimType = 0;

		/*
			Data프로퍼티의 자동 갱신 Init 으로 인한 무한루프 주의
			Init()에서 프로퍼티 접근 수정 및 Init(CharBaseData) 생성 금지
		*/
		public override void Init()
		{			
			base.Init();

			if (Animation == default)
				Animation = eSpineAnimation.IDLE;

			DrawSpine();

			if (GetTypeToLoop(Animation))
				RandomAnimationFrame();
		}


		public override void InitializeTypeFunc()
		{
			GetTypeToLoop = SBFunc.IsTypeToLoop;
			GetTypeToName = GetAnimName;
			GetNameToType = SBDefine.GetDragonAnimNameToType;
		}

		void DrawSpine()
		{
			if (Data != null)
			{
				SetSkin(Data.SKIN);
				SetAnimation(SBDefine.GetDragonAnimTypeToName(Animation));
				if (skeletonAni != null)
				{
					if (completeCallback != null)
					{
						skeletonAni.AnimationState.Complete += SpineCompleteEvent;
					}

					var mat = skeletonAni.material;
					if (mat != null)
						mat.SetFloat("_FillPhase", 0f);
				}

				if (initComplete != null)
				{
					initComplete(this);
				}
			}
		}

		public override Spine.TrackEntry SetAnimation(eSpineAnimation anim)
		{
			if(anim == Animation)
				return null;
			var animName = GetAnimName(anim);

			// 오브젝트가 생성되고 SetAnimation을 호출하면 지정한 애니메이션을 호출 후 startingAnimation에 등록된 애니메이션이 실행되어
			// 내가 실행하고자 하는 애니메이션이 씹혀버림, 이를 막고자 설정 startingAnimation을 None 설정하면 null reference 발생
			var animData = skeletonAni.SkeletonData.FindAnimation(animName);
			if(animData != null)
            {
				skeletonAni.startingAnimation = animName;
				return SetAnimation(animName);
			}
			return null;
		}

		public virtual void SetData(UserDragon dragonData)
        {
			if (dragonData == null)
				return;

			var baseData = dragonData.BaseData;
			SetData(baseData);
			SetTranscendEffect(dragonData.TranscendenceStep);
		}
		public override void SetData(ISpineCharacterData data)
		{
			base.SetData(data);
			Data = data as ICharacterBaseData;
			if(Data != null)
				SetSkin(Data.SKIN);
		}

		public void SetDragonData(CharBaseData baseData)
        {
			SkeletonAni.initialSkinName = baseData.SKIN;
			SkeletonAni.Initialize(true);
			SkeletonAni.Skeleton.SetSkin(baseData.SKIN);
			SetAnimation(eSpineAnimation.IDLE);

			skillAnimType = (baseData != null && baseData.SKILL1 != null) ? baseData.SKILL1.ANI : 0;
		}

		public void SetTranscendEffect(int transcendStep) // 임시 초월 캐릭터 이펙트로 쓰기 위해 준비 중인 코드
		{
			if (TranscendParent == null)
				return;

			if (TranscendParent.transform.childCount > 0)
				SBFunc.RemoveAllChildrens(TranscendParent.transform);

			if (transcendStep <= 0)
				return;

			//GameObject obj = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, SBDefine.TRANSCENDENCE_EFFECT_NAME);//HEAD

			GameObject obj = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, SBDefine.GetTranscendEffectName(transcendStep));

			if (obj != null)
				Instantiate(obj, TranscendParent.transform);

			TranscendParent.RefreshParticles();
		}

		protected string GetAnimName(eSpineAnimation anim)
		{
			switch (anim)
			{
				case eSpineAnimation.A_CASTING:
				case eSpineAnimation.ATTACK:
					return SBDefine.GetDragonAnimTypeToName(anim, 1);
				case eSpineAnimation.CASTING:
				case eSpineAnimation.SKILL:
					return SBDefine.GetDragonAnimTypeToName(anim, (skillAnimType > 0) ? skillAnimType : 1);
				default:
					return SBDefine.GetDragonAnimTypeToName(anim, 1);
			}
		}

        public override Spine.TrackEntry SetAnimation(string animName)
		{
			var type = SBDefine.GetDragonAnimNameToType(animName);
			if (Animation == type)
				return null;

			Animation = type;

			return SetAnimation(0, animName, SBFunc.IsTypeToLoop(Animation));
		}

		public void RandomAnimationFrame()
        {
			if (!ApplyRandomFrame)
				return;

			if(skeletonAni != null)
				skeletonAni.Update(SBFunc.RandomValue * 3.0f);
		}

		func startCallback = null;
		public void SetStartAniCallback(func callback)
		{
			if (skeletonAni != null)
			{
                startCallback = callback;
                skeletonAni.AnimationState.Start += SpineStartEvent;
			}
			
		}

		func completeCallback = null;
		public void SetCompleteAniCallback(func callback)
		{
			if(skeletonAni != null)
            {
				skeletonAni.AnimationState.Complete += SpineCompleteEvent;
			}
			completeCallback = callback;
		}

		void SpineCompleteEvent(Spine.TrackEntry e)
        {
			if(completeCallback != null && Animation != eSpineAnimation.IDLE)
            {
				completeCallback(this);
			}

			switch (e.Animation.Name)
			{
				case "atk_ani1":
				case "atk_ani2":
				case "atk_ani3":
					Animation = eSpineAnimation.NONE;
					SetAnimation(eSpineAnimation.IDLE);
					break;
				case "skill_ani1":
				case "skill_ani2":
				case "skill_ani3":
				case "skill_ani4":
					Animation = eSpineAnimation.NONE;
					SetAnimation(eSpineAnimation.IDLE);
					break;
			}
		}

		void SpineStartEvent(Spine.TrackEntry e)
		{
			if (startCallback != null && Animation != eSpineAnimation.IDLE)
			{
				startCallback(this);
			}
		}

		public void SetSpineRaycastTargetState(bool state)
		{
			skeletonAni.raycastTarget = state;
		}

		public void SetEnableSkeletonGraphic(bool enable)
        {
			skeletonAni.enabled = enable;
		}

		public void SetColor(Color color)
        {
			skeletonAni.color = color;
        }

		public void SetScale(Vector2 scale)
        {
			spineObj.transform.localScale = scale;
        }

		public void SetPortrait(float interval, CharBaseData dragonInfo, Action cb)
        {
			ReleasePortraitLoad();

			DelayQueue[this] = new DelayLoadData(dragonInfo, cb);

			Invoke("OnPortrait", interval);
		}

		public void ReleasePortraitLoad()
        {
			CancelInvoke("OnPortrait");

			if (DelayQueue.ContainsKey(this))
				DelayQueue.Remove(this);
		}

		public void OnPortrait()
        {
			if (!DelayQueue.ContainsKey(this))
			{
				ReleasePortraitLoad();
				return;
			}

			CharBaseData dragonInfo = DelayQueue[this].data;
			Action callback = DelayQueue[this].cb;

			ReleasePortraitLoad();

			SetData(dragonInfo);
			//SetSkin(dragonInfo.SKIN);
			InitComplete = PortraitInitCallback;
			gameObject.SetActive(true);
			SetEnableSkeletonGraphic(true);

			callback.Invoke();
		}

		void PortraitInitCallback(UIDragonSpine spineData)
		{
			if (spineData.Data.GRADE >= (int)eDragonGrade.Legend)
				spineData.SetAnimation(eSpineAnimation.IDLE);
			else
				spineData.SkeletonAni.timeScale = 0f;
		}

        private void OnDisable()
        {
			ReleasePortraitLoad();
        }

        struct DelayLoadData
        {
			public CharBaseData data;
			public Action cb;

			public DelayLoadData(CharBaseData d, Action c)
            {
				data = d;
				cb = c;
            }
		}

        static Dictionary<UIDragonSpine, DelayLoadData> DelayQueue = new Dictionary<UIDragonSpine, DelayLoadData>();

    }
}