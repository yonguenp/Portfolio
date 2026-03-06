using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class BuffMainElementEffect : BuffEffect
    {
        public override eSkillEffectType EFFECT_TYPE => eSkillEffectType.BUFF_MAIN_ELEMENT;
        public override eStatusType STAT_TYPE => Target != null ? Target.Element.StatDMG() : eStatusType.NONE;
    }
}