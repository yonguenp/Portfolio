using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class InvincibilityEffect : EffectInfo
    {
        protected override void CompleteEvent()
        {
            SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy);
        }
        protected override void TriggerEvent()
        {
            SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy);
        }
    }
}