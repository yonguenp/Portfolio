using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIDamageObject : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup group = null;
        [SerializeField]
        private Animator animator = null;
        [SerializeField]
        private string animName = null;
        [SerializeField]
        private Text text = null;

        private void Start()
        {
            if (group == null)
                group = GetComponent<CanvasGroup>();
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        public void SetText(string text, Color color)
        {
            if (this.text == null)
                return;

            this.text.text = text;
            this.text.color = color;
        }

        public void PlayAnim()
        {
            if (animator != null)
            {
                animator.Play(animName, 0);
                animator.SetBool("playing", true);
            }
        }

        public bool IsPlaying()
        {
            if (animator == null)
                return false;

            return animator.GetBool("playing");
        }

        public void AnimEnd()
        {
            if (animator != null)
            {
                animator.SetBool("playing", false);
            }
        }

        public void DestroyObject()
        {
            Destroy(gameObject);
        }
    }
}