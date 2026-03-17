using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    [CreateAssetMenu(fileName = "EffectCustomOutlineScale", menuName = "Scriptable Object Asset/EffectCustom/EffectCustomOutlineScale", order = 2)]
    public class EffectCustomOutlineScale : EffectCustom
    {
        public float inTime = 0.15f;
        public float duration = 0.3f;
        public float outTime = 0.15f;
        public float changeScale = 0.5f;
        [System.NonSerialized]
        public Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f);

        private void Awake()
        {
            effectCustomType = eEffectCustomType.EffectOutlineScale;
        }
    }
}
