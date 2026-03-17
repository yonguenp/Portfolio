using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class HealEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
        }
        protected override void CompleteEvent()
        {
            switch (VALUE_TYPE)
            {
                case eStatusValueType.PERCENT:
                {
                    Target.HP = Mathf.FloorToInt(Target.HP + Target.MaxHP * GetValue() * SBDefine.CONVERT_FLOAT);
                } break;
                default:
                {
                    Target.HP += Mathf.FloorToInt(GetValue());
                } break;
            }

            if (Target.HP > Target.MaxHP)
                Target.HP = Target.MaxHP;
        }
    }
}