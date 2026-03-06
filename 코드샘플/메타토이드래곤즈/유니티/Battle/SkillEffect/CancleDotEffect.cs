using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CancleDotEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            int count = 0;
            var max = GetValue();
            Target.Infos.ForEach(info =>
            {
                if (info.EFFECT_TYPE == eSkillEffectType.DOT)
                {
                    ++count;
                    if(max >= count)
                    {
                        info.TimeEnd();
                    }
                }
            });
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