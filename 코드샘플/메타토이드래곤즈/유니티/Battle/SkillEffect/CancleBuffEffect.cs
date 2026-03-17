using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CancleBuffEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            if (Target.Infos != null)
            {
                foreach (var info in Target.Infos)
                {
                    switch (info.EFFECT_TYPE)
                    {
                        case eSkillEffectType.BUFF:
                        case eSkillEffectType.BUFF_MAIN_ELEMENT:
                            if (info.STAT_TYPE == STAT_TYPE)
                                info.TimeEnd();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        protected override void CompleteEvent()
        {
        }
        public override bool IsEquals(EffectInfo info)
        {
            return false;
        }
    }
}