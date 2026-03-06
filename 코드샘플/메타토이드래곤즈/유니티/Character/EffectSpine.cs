using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EffectSpine : MonoBehaviour
    {
        [SerializeField] 
        protected GameObject spineObj = null;
        [SerializeField] 
        protected SkeletonAnimation skeletonAni = null;
        public SkeletonAnimation SkeletonAni
        {
            get { return skeletonAni; }
        }

        protected VoidDelegate callBack = null;
        public VoidDelegate CallBack
        {
            get { return callBack; }
            set { callBack = value; }
        }

        protected float maxTime = 0f;
        protected float curTime = 0f;
        protected bool isCallback = false;


        protected virtual void Start()
        {
            if (spineObj == null)
            {
                var spine = transform.Find("spine");
                if (spine != null)
                    spineObj = spine.gameObject;
            }
            if (skeletonAni == null)
                skeletonAni = GetComponentInChildren<SkeletonAnimation>();

            AddComplete();
        }

        public void SetCallBackTime(float value)
        {
            curTime = value;
            maxTime = value;
            isCallback = false;
        }

        protected void Complete(TrackEntry trackEntry)
        {
            if(!isCallback)
                callBack?.Invoke();

            Destroy(gameObject);
        }

        public void AddComplete()
        {
            if (skeletonAni == null)
                return;

            skeletonAni.AnimationState.Complete += Complete;
        }

        public void DelComplete()
        {
            if (skeletonAni == null)
                return;

            skeletonAni.AnimationState.Complete -= Complete;
        }

        private void FixedUpdate()
        {
            if (maxTime <= 0f || spineObj == null || skeletonAni == null || isCallback)
                return;

            curTime -= SBGameManager.Instance.DTime;
            if (curTime <= 0)
            {
                isCallback = true;
                callBack?.Invoke();
            }
        }
    }
}