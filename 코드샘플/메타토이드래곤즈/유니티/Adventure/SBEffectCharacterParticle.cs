using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBEffectCharacterParticle : SBObjectEffect
    {
        [SerializeField]
        protected eSpineAnimation targetAnim = eSpineAnimation.NONE;
        protected ParticleSystem particle = null;
        protected override bool AwakeInitialize()
        {
            particle = GetComponent<ParticleSystem>();
            return true;
        }

        protected override bool InitializeData()
        {
            if (particle != null && Data != null)
            {
                var main = particle.main;
                if (main.startSpeed.constant > 0f)
                {
                    main.simulationSpeed = Data.GetSpine().GetAnimScale(targetAnim);
                }
                var particles = particle.GetComponentsInChildren<ParticleSystem>();
                if (particles != null)
                {
                    for (int i = 0, count = particles.Length; i < count; ++i)
                    {
                        if (particles[i] == null)
                            continue;
                        main = particles[i].main;
                        if (main.startSpeed.constant > 0f)
                        {
                            main.simulationSpeed = Data.GetSpine().GetAnimScale(targetAnim);
                        }
                    }
                }
                return IsInit = true;
            }
            return IsInit = false;
        }

        void FIxedUpdate()
        {
            if (!IsInit)
                return;

            if (particle != null)
            {
                if (particle.IsAlive(false))
                    return;
            }

            EffectDestory();
        }
    }
}