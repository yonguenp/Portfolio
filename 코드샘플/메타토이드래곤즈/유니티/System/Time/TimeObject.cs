using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public delegate void RefreshTime();
    public class TimeObject : MonoBehaviour, ITimeObject
    {
        protected float time = -1;
        public float Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }
        protected RefreshTime refresh = null;
        public RefreshTime Refresh
        {
            get
            {
                return refresh;
            }

            set
            {
                refresh = value;
                refresh?.Invoke();
            }
        }

        protected virtual void Start()
        {
            Init();
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }

        public virtual void Init()
        {
            TimeManager.AddObject(this);
        }

        public virtual void Clear()
        {
            TimeManager.DelObject(this);
        }
    }
}
