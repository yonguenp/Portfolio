using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class TitleSpine : MonoBehaviour
    {
        [SerializeField] protected SkeletonGraphic skeletonAni = null;
        private bool isIdle = false;

        void Start()
        {
            if (skeletonAni != null)
                skeletonAni.AnimationState.Complete += CompleteHandleAnimation;
        }

        protected virtual void CompleteHandleAnimation(TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "start")
            {
                if(skeletonAni != null)
                {
                    skeletonAni.AnimationState.SetAnimation(0, "idle", true);
                }
                isIdle = true;
            }
        }

        public IEnumerator StartCO()
        {
            while(false == isIdle)
            {
                yield return SBDefine.GetWaitForEndOfFrame();
            }

            yield return new WaitForSeconds(1.0f);

            yield break;
        }
    }
}