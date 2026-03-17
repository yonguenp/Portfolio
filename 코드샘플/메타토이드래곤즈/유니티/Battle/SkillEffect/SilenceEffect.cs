using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SilenceEffect : EffectInfo
    {
        protected override void CompleteEvent()
        {
            var spine = Target.GetSpine();
            if (spine == null)
                return;

            spine.SkillActionCancle();
            SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy);
        }
        protected override void TriggerEvent()
        {
            SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy);
        }

        public override bool IsCover(EffectInfo info)
        {
            if (info.MAX_TIME >= MAX_TIME)
                return true;

            return base.IsCover(info);
        }
    }
}