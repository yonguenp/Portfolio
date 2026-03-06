using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EffectAnim : MonoBehaviour
    {
        [SerializeField]
        private Animator animator = null;
        // Start is called before the first frame update
        void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        public void DestroyObject()
        {
            Destroy(gameObject);
        }
    }
}