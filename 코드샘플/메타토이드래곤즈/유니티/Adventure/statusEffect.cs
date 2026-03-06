using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork {
    public class statusEffect : MonoBehaviour
    {
        [SerializeField]
        private Transform transcendTr;
        [SerializeField]
        private Transform effectTr;
        [SerializeField]
        private float animationFramePerSec = 60.0f;

        public delegate void StateEndCallBack();
        StateEndCallBack cb= null;

        public eAbnormalState State { get; protected set; } = eAbnormalState.None;

        public Transform EffectTr
        {
            get
            {
                return effectTr;
            }
        }

        public Transform TranscendTr
        {
            get { return transcendTr;}
        }

        public delegate void StateCallBack();
        public void AddState(eAbnormalState State)
        {
            if (effectTr != null) { 
                effectTr.gameObject.SetActive(true);
                this.State |= State;
            }
        }
        public void EndState(eAbnormalState State)
        {
            this.State &= ~State;
            if(cb != null)
            {
                cb.Invoke();
                cb = null;
            }
        }

        public void SetEffectTransform(Transform target)
        {
            effectTr = target;
        }

        public void RemoveAllState()
        {
            effectTr.gameObject.SetActive(false);
            State = eAbnormalState.None;
        }

        public void SetStateEndCallBack(StateEndCallBack callBack)
        {
            cb = callBack;
        }

        public bool IsState(eAbnormalState State)
        {
            return (this.State & State) >0;
        }

        public eAbnormalState GetState()
        {
            return State;
        }
        
        public float GetAnimationFrame()
        {
            if (animationFramePerSec > 0.0f)
                return 1.0f / animationFramePerSec;

            return 0.0f;
        }
    }
}