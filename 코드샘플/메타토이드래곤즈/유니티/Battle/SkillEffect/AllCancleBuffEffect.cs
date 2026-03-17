using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AllCancleBuffEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            if (Target.Infos != null)
            {
                foreach (var info in Target.Infos)
                {
                    if (info is BuffEffect)
                    {
                        info.TimeEnd();
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
