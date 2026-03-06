using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ReverseAggroEffect : DebuffEffect
    {
        public override eSkillEffectType EFFECT_TYPE => eSkillEffectType.AGGRO_R;
        public override eStatusType STAT_TYPE => eStatusType.NONE;
        protected override void TriggerEvent()
        {
            if (Caster == null)
                return;

            Caster.AddPriorityTarget(Target, Mathf.FloorToInt(VALUE));
            SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy);
        }
        protected override void CompleteEvent()
        {
            if (Caster == null)
                return;

            Caster.DelPriorityTarget(Target, Mathf.FloorToInt(VALUE));
            SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy);
        }
    }
}