using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
	public enum eSpineDirtyFlag
	{
		NONE = 0,

		DataAsset = 1 << 0,

		ALL = DataAsset
	}
	public abstract class SBSpine<T>: MonoBehaviour, ISBSpine where T : System.Enum
	{
		#region Generic
		public T Animation { get; protected set; } = default;

		protected delegate T NameToType(string type);
		protected delegate string TypeToName(T type);
		protected delegate bool TypeToLoop(T type);
		protected delegate bool TypeToSkip(T type1, T type2);
		protected TypeToName GetTypeToName { get; set; } = null;
		protected NameToType GetNameToType { get; set; } = null;
		protected TypeToLoop GetTypeToLoop { get; set; } = null;
		protected TypeToSkip GetTypeToSkip { get; set; } = null;
		#endregion
		#region SpineDefault
		[Header("SpineDefault")]
		[SerializeField] protected GameObject spineObj = null;
		[SerializeField] protected SpriteRenderer shadow = null;
		[SerializeField] protected SkeletonAnimation skeletonAni = null;
		[SerializeField] protected SortingGroup sortingGroup = null;
		[SerializeField] protected OutlineRenderer outlineRenderer = null;
		public virtual Transform SpineTransform => spineObj?.transform;
		public virtual Transform ShadowTransform => shadow?.transform;
		public virtual SkeletonAnimation SkeletonAni => skeletonAni;
		public TrackEntry CurrentTrack { get; protected set; } = null;
		public ISpineCharacterData CharacterData { get; protected set; } = null;
		public SkeletonData SkData { get; protected set; } = null;
		public int Order
		{
			get => null != sortingGroup ? sortingGroup.sortingOrder : 0;
			protected set => SetOrder(value);
		}
		public bool Loop
		{
			get => null != skeletonAni && skeletonAni.loop;
			protected set { if (null != skeletonAni) skeletonAni.loop = value; }
		}
		#endregion

		#region Initialize
		protected virtual void Awake()
		{
			InitializeTypeFunc();
			InitializeComponent();
		}
		protected virtual void Start()
		{
			Init();
		}
		/// <summary>
		/// 상속 받은 쪽에서 함수 입력할 것.
		/// GetTypeName, GetTypeLoop
		/// </summary>
		public abstract void InitializeTypeFunc();
		public virtual void InitializeComponent()
		{
			if (spineObj == null)
				spineObj = SBFunc.GetChildrensByName(transform, "spine").gameObject;

			if (skeletonAni == null && !spineObj.TryGetComponent(out skeletonAni))
				skeletonAni = spineObj.AddComponent<SkeletonAnimation>();

			if (shadow == null)
				shadow = GetComponentInChildren<SpriteRenderer>();

			if (sortingGroup == null)
				sortingGroup = GetComponent<SortingGroup>();

			if (outlineRenderer == null)
				outlineRenderer = GetComponentInChildren<OutlineRenderer>();
		}
		public virtual void Init()
		{
			if (skeletonAni.skeletonDataAsset != null)
			{
				skeletonAni.Initialize(true);
				skeletonAni.ClearState();
			}

			SetOutline(false);
			SetShadow(false);
		}
		#endregion
		#region Updates
		/// <summary>
		/// 캐릭터의 행동 정의
		/// </summary>
		/// <param name="dt">시간</param>
		/// <returns>상위 Update 무결성 체크용도</returns>		
		protected virtual void LateUpdate()
		{
			if (outlineRenderer == null)
				return;

			outlineRenderer.LateUpdateOutline(spineObj.transform);
		}
		#endregion
		#region SpineFunction
		protected void SetData(ISpineCharacterData data)
		{
			if (data == null)
				return;

			CharacterData = data;
			SetDataAsset(CharacterData.GetSkeletonDataAsset());
		}
		protected void SetDataAsset(SkeletonDataAsset asset)
		{
			if (skeletonAni.skeletonDataAsset == asset)
			{
				if (SkData == null)
					SkData = skeletonAni.skeletonDataAsset.GetSkeletonData(true);
				return;
			}

			skeletonAni.skeletonDataAsset = asset;
			skeletonAni.SkeletonDataAsset.Clear();
			SkData = skeletonAni.skeletonDataAsset.GetSkeletonData(true);
			skeletonAni.initialSkinName = GetDefaultSkin();
			skeletonAni.ClearState();
			skeletonAni.Initialize(false);
			skeletonAni.AnimationState.ClearTracks();
		}

		public virtual void SetSkin(string skinName)
		{
			if (skeletonAni == null)
				return;

			if (SkData == null)
				return;

			var skinData = SkData.FindSkin(skinName);
			if (skinData == null)
				skinName = GetDefaultSkin();

			skeletonAni.initialSkinName = skinName;
			skeletonAni.Initialize(true);
			skeletonAni.Skeleton.SetSkin(skinName);
		}
		private string GetDefaultSkin()
		{
			if (SkData == null)
				return "default";

			if (SkData.DefaultSkin != null)
				return SkData.DefaultSkin.Name;

			var it = SkData.Skins.GetEnumerator();
			while (it.MoveNext())
			{
				if (it.Current == null)
					continue;

				return it.Current.Name;
			}
			return "default";
		}
		public virtual TrackEntry SetAnimation(T anim)
		{
			if (GetTypeToSkip(Animation, anim))
				return null;

			Animation = anim;
			var name = GetTypeToName(anim);
			if (skeletonAni == null || skeletonAni.AnimationName == name)
				return null;

			return SetAnimation(0, name, GetTypeToLoop(anim));
		}
		public virtual TrackEntry SetAnimation(int trackIndex, string animnation, bool loop)
		{
			if (skeletonAni == null || skeletonAni.AnimationName == animnation)
				return CurrentTrack;

			if (CurrentTrack != null)
				CurrentTrack.TrackTime = 0;
			CurrentTrack = skeletonAni.AnimationState.SetAnimation(trackIndex, animnation, loop);
			return CurrentTrack;
		}
		#endregion
		#region ETC
		public void SetShadow(bool show)
		{
			if (shadow != null)
				shadow.gameObject.SetActive(show);
		}
		public virtual void SetOutline(bool active)
		{
			if (outlineRenderer == null)
				return;

			outlineRenderer.SetOutline(active);
		}
		public virtual void MixAnim(string animName1, string animName2, float anim2Duration, float animMixDuration, float delay)
		{
			var spineAnimationState = skeletonAni.AnimationState;
			spineAnimationState.SetAnimation(0, animName1, true);
			spineAnimationState.SetEmptyAnimation(1, 0);
			spineAnimationState.AddAnimation(1, animName2, false, 0).MixDuration = anim2Duration;
			spineAnimationState.AddEmptyAnimation(1, animMixDuration, delay);
		}
		public virtual float GetAnimaionTime(T anim)
		{
			if (GetTypeToName == null)
				return 0f;

			var name = GetTypeToName(anim);
			if (SkData == null)
				return 0f;

			var animation = SkData.FindAnimation(name);
			if (animation != null)
				return animation.Duration;

			return 0f;
		}
		public virtual void SetOrder(int order)
        {
			if (null != sortingGroup)
				sortingGroup.sortingOrder = order;
        }
		#endregion
	}
}