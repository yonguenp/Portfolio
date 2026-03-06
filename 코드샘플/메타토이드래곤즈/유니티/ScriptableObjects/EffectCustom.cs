using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EffectCustom : ScriptableObject
    {
        [Header("[Common Value]")]
        public eEffectCustomType effectCustomType = eEffectCustomType.None;
        public eSpineAnimation animState = eSpineAnimation.NONE;            // 해당 효과 발생 타이밍
        public float delayTime = 0;
    }
}
