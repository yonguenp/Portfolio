using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class FollowPosition : MonoBehaviour
    {
        public Transform Target { get; protected set; } = null;
        public Transform Parent { get; protected set; } = null;
        private Vector3 pos = Vector3.zero;
        private bool isLeft = false;
        private bool isScale = false;

        void Start()
        {
            Refresh();
        }

        void LateUpdate()
        {
            if (Target == null)
                return;

            Refresh();
        }

        public void Set(Transform target, Transform parent, Vector3 pos, bool isLeft, bool isScale = true)
        {
            Target = target;
            Parent = parent;
            this.pos = pos;
            this.isLeft = isLeft;
            this.isScale = isScale;

            Refresh();
        }

        public void Refresh()
        {
            if (Target != null)
            {
                transform.position = Target.position + pos;
                if (isScale)
                {
                    if (Target != Parent)
                    {
                        var scale = Target.localScale;
                        scale.x = Mathf.Abs(scale.x) * GetDir();
                        transform.localScale = scale;
                    }
                    else
                    {
                        transform.localScale = Vector3.one;
                    }
                }
            }
        }

        private float GetDir()
        {
            if (Parent == null || Target == null)
                return 1f;

            if (!isLeft)
                return Parent.localScale.x * Target.localScale.x < 0 ? 1 : -1;
            else
                return Parent.localScale.x * Target.localScale.x < 0 ? -1 : 1;
        }
    }
}