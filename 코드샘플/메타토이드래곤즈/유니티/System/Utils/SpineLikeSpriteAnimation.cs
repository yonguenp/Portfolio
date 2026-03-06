using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineLikeSpriteAnimation : MonoBehaviour
{
	[SerializeField] protected SkeletonAnimation[] skeletons = null;

	void Start()
    {
		if(skeletons == null)
        {
			skeletons = GetComponentsInChildren<SkeletonAnimation>();
		}
	}


#if false//UNITY_EDITOR
	[ContextMenu("Auto Load Skeletons")]
	void AutoLoadSkeletons()
	{
		skeletons = GetComponentsInChildren<SkeletonAnimation>();
	}
#endif
}
