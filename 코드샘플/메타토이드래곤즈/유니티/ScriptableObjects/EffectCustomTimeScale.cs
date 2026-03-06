using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    [CreateAssetMenu(fileName = "EffectCustomTimeScale", menuName = "Scriptable Object Asset/EffectCustom/EffectCustomTimeScale", order = 3)]
    public class EffectCustomTimeScale : EffectCustom
    {
        public float inTime = 0.1f;
        public float duration = 0.3f;
        public float outTime = 0.1f;
        public float changeScale = 0.1f;
        [System.NonSerialized]
        public float startScale = 1f;

        private void Awake()
        {
            effectCustomType = eEffectCustomType.EffectTimeScale;
        }
    }
}
