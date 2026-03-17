using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EffectParticleSpine : EffectSpine
    {
        protected override void Start()
        {
            base.Start();
            DelComplete();
        }
    }
}