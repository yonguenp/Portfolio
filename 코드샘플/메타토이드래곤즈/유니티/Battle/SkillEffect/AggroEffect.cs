using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AggroEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            if (Target == null)
                return;

            Target.AddPriorityTarget(Caster, Mathf.FloorToInt(VALUE));
            var spine = Target.GetSpine();
            if (spine == null)
                return;
            
            spine.SkillActionCancle();
        }
        protected override void CompleteEvent()
        {
            if (Target == null)
                return;

            Target.DelPriorityTarget(Caster, Mathf.FloorToInt(VALUE));
        }
    }
}