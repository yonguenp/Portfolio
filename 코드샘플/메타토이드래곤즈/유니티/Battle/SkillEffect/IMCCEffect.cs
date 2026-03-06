using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class IMCCEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            if (Target.Infos != null)
            {
                foreach (var info in Target.Infos)
                {
                    switch(info.EFFECT_TYPE)
                    {
                        case eSkillEffectType.STUN:
                        case eSkillEffectType.AGGRO:
                        case eSkillEffectType.PULL:
                        case eSkillEffectType.AIRBORNE:
                        case eSkillEffectType.KNOCK_BACK:
                            info.TimeEnd();
                            break;
                        default:
                            continue;
                    }
                }
            }
            SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy);
        }
        protected override void CompleteEvent()
        {
            SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy);
        }
        public override bool IsCover(EffectInfo info)
        {
            if (info.Time > Time)
                return true;

            return false;
        }
    }
}