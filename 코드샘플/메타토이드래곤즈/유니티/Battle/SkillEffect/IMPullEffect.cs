using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class IMPullEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            if (Target.Infos != null)
            {
                foreach (var info in Target.Infos)
                {
                    if (info.EFFECT_TYPE == eSkillEffectType.PULL)
                    {
                        info.TimeEnd();
                    }
                }
            }
        }
        protected override void CompleteEvent()
        {
        }
        public override bool IsCover(EffectInfo info)
        {
            if (info.Time > Time)
                return true;

            return false;
        }
    }
}
