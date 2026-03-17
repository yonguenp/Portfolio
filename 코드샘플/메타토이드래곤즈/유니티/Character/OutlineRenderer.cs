using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class OutlineRenderer : MonoBehaviour
    {
        public abstract void Sync();
        public abstract void SetOutline(bool isActive);
        public abstract void LateUpdateOutline(Transform target);
    }
}