using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CharacterSpine : MonoBehaviour
	{
		[SerializeField] private GameObject spineObj = null;
		[SerializeField] private SkeletonAnimation skeletonAni = null;
		private SkeletonData SkData { get; set; } = null;
		private void Awake()
		{
			if (spineObj == null)
				spineObj = SBFunc.GetChildrensByName(transform, "spine").gameObject;

			if (skeletonAni == null && !spineObj.TryGetComponent(out skeletonAni))
				skeletonAni = spineObj.AddComponent<SkeletonAnimation>();
		}
        public void SetData(ISpineCharacterData data)
		{
			if (data == null)
				return;

			SetDataAsset(data.GetSkeletonDataAsset());
		}
		protected void SetDataAsset(SkeletonDataAsset asset)
		{
			if (skeletonAni.skeletonDataAsset == asset)
				return;

			skeletonAni.skeletonDataAsset = asset;
			skeletonAni.SkeletonDataAsset.Clear();
			SkData = skeletonAni.skeletonDataAsset.GetSkeletonData(true);
			skeletonAni.initialSkinName = GetDefaultSkin();
			skeletonAni.ClearState();
			skeletonAni.Initialize(false);
			skeletonAni.AnimationState.ClearTracks();
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
	}
}