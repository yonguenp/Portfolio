using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    [CreateAssetMenu(fileName = "EffectCustomBackground", menuName = "Scriptable Object Asset/EffectCustom/EffectCustomBackground", order = 1)]
    public class EffectCustomBackground : EffectCustom
    {
        public float BackgroundDuration = 0.6f;
        [Range(0f, 1f)]
        public float Alpha = 0.7f;
        public eSkillDimmedType DimmedType = eSkillDimmedType.None;

        private void Awake()
        {
            effectCustomType = eEffectCustomType.EffectBackground;
        }
    }
}
