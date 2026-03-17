using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SandboxNetwork
{
	public abstract class SBUISpine<T> : MonoBehaviour, ISBSpine where T : Enum
	{
		public delegate T NameToType(string type);
		public delegate string TypeToName(T type);
		public delegate bool TypeToLoop(T type);

		[SerializeField] protected GameObject spineObj = null;
		[SerializeField] protected SkeletonGraphic skeletonAni = null;
		public SkeletonGraphic SkeletonAni { get { return skeletonAni; } }

		protected TypeToName GetTypeToName { get; set; } = null;
		protected NameToType GetNameToType { get; set; } = null;
		protected TypeToLoop GetTypeToLoop { get; set; } = null;
		protected eSpineDirtyFlag dirty = eSpineDirtyFlag.ALL;

		protected ISpineCharacterData CharacterData { get; set; } = null;
		private SkeletonData skData = null;

		public T Animation { get; protected set; } = default;

		public Transform SpineTransform
		{
			get => spineObj == null ? null : spineObj.transform;
		}

		public bool Loop
		{
			get { return skeletonAni.startingLoop; }
			set { skeletonAni.startingLoop = value; }
		}

		protected virtual void Awake()
		{
			InitializeTypeFunc();
			InitializeComponent();
		}

		protected virtual void Start()
		{
			Init();
		}

		public virtual void Init()
		{
			if (skeletonAni.skeletonDataAsset != null)
				skeletonAni.Initialize(true);

			SetShadow();
		}

		public virtual void InitializeComponent()
		{
			if (spineObj == null)
				spineObj = SBFunc.GetChildrensByName(transform, "spine").gameObject;

			if (skeletonAni == null) // 불필요한 TryGetComponent 제거
				skeletonAni = spineObj.GetComponent<SkeletonGraphic>() ?? spineObj.AddComponent<SkeletonGraphic>();
		}

		/// <summary>
		/// 상속 받은 쪽에서 함수 입력할 것.
		/// GetTypeName, GetTypeLoop
		/// </summary>
		public abstract void InitializeTypeFunc();
		/// <summary>
		/// 메뉴얼이 아닐 경우 상속 받아서 변경할 것.
		/// </summary>

		public virtual TrackEntry SetAnimation(T anim)
		{
			return SetAnimation(0, GetTypeToName(anim), GetTypeToLoop(anim));
		}

		public virtual TrackEntry SetAnimation(int trackIndex, string animName, bool loop)
		{
			if (skeletonAni == null || skeletonAni.AnimationState.GetCurrent(0) == null || skeletonAni.AnimationState.GetCurrent(0).Animation.Name == animName)
				return null;

			return skeletonAni.AnimationState.SetAnimation(trackIndex, animName, loop);
		}

		public void SetDataAsset(SkeletonDataAsset asset)
		{
			if (skeletonAni.skeletonDataAsset == asset)
			{
				skData = skeletonAni.skeletonDataAsset.GetSkeletonData(true);
				return;
			}

			skeletonAni.skeletonDataAsset = asset;
			skeletonAni.SkeletonDataAsset.Clear();
			skData = skeletonAni.skeletonDataAsset.GetSkeletonData(true);
			skeletonAni.initialSkinName = GetDefaultSkin();
			skeletonAni.Clear();
			skeletonAni.Initialize(false);
			skeletonAni.AnimationState.ClearTracks();
		}

		public virtual void SetSkin(string skinName)
		{
			if (skeletonAni == null)
				return;

			if (skeletonAni.Skeleton != null && skeletonAni.Skeleton.Skin != null && skeletonAni.Skeleton.Skin.Name == skinName)
				return;

			if (skData == null)
			{
				skData = skeletonAni.skeletonDataAsset.GetSkeletonData(true);
				if (skData == null)
					return;
			}

			var skinData = skData.FindSkin(skinName);
			if (skinData == null)
				skinName = GetDefaultSkin();
			
			skeletonAni.initialSkinName = skinName;
			skeletonAni.Skeleton.SetSkin(skinName);
			skeletonAni.Initialize(true);
		}

		private string GetDefaultSkin()
		{
			if (skData == null)
				return "default";

			if (skData.DefaultSkin != null)
				return skData.DefaultSkin.Name;

			var it = skData.Skins.GetEnumerator();
			while (it.MoveNext())
			{
				if (it.Current == null)
					continue;

				return it.Current.Name;
			}
			return "default";
		}

		public void MixAnim(string animName1, string animName2, float anim2Duration, float animMixDuration, float delay)
		{
			var spineAnimationState = skeletonAni.AnimationState;
			spineAnimationState.SetAnimation(0, animName1, true);
			spineAnimationState.SetEmptyAnimation(1, 0);
			spineAnimationState.AddAnimation(1, animName2, false, 0).MixDuration = anim2Duration;
			spineAnimationState.AddEmptyAnimation(1, animMixDuration, delay);
		}

		public void SetShadow(bool show = true)
		{
		}

		public SkeletonData GetSkeletonData()
		{
			if (skData == null)
				skData = skeletonAni.SkeletonDataAsset.GetSkeletonData(true);

			return skData;
		}

		public virtual float GetAnimaionTime(T anim)
		{
			if (GetTypeToName == null)
				return 0f;

			var name = GetTypeToName(anim);

			var animation = GetAnimation(name);
			if (animation != null)
				return animation.Duration;

			return 0f;
		}

		public Spine.Animation GetAnimation(string name)
		{
			var skData = GetSkeletonData();
			if (skData == null)
				return null;

			return skData.FindAnimation(name);
		}

		public virtual void SetData(ISpineCharacterData data)
		{
			if (data == null)
				return;

			CharacterData = data;
			SetDataAsset(CharacterData.GetSkeletonDataAsset());
		}

		public virtual TrackEntry SetAnimation(string animName)
		{
			return SetAnimation(0, animName, GetTypeToLoop(GetNameToType(animName)));
		}

		public virtual List<Skin> GetSkinList()
		{
			var skData = GetSkeletonData();
			if (skData != null)
				return skData.Skins.ToList();

			return null;
		}
		public Material GetSpineMaterial()
		{
			if (skeletonAni != null)
			{
				return skeletonAni.material;
			}
			return null;
		}
		public void SetSpineMaterial(Material mat)
		{
			if (mat != null && skeletonAni != null)
			{
				skeletonAni.material = mat;
			}
		}
	}
}
