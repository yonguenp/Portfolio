using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AnimationEffect : MonoBehaviour
    {
        [SerializeField]
        protected Animator animator = null;
        void Start()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}