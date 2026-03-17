using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class BuffEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            var category = eStatusCategory.ADD_BUFF;
            if (VALUE_TYPE == eStatusValueType.PERCENT)
                category = eStatusCategory.RATIO_BUFF;
            Target.Stat.IncreaseStatus(category, STAT_TYPE, GetValue(), true);
            SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy);
        }
        protected override void CompleteEvent()
        {
            var category = eStatusCategory.ADD_BUFF;
            if (VALUE_TYPE == eStatusValueType.PERCENT)
                category = eStatusCategory.RATIO_BUFF;
            Target.Stat.DecreaseStatus(category, STAT_TYPE, GetValue(), true);
            SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy);
        }
        public override bool IsEquals(EffectInfo info)
        {
            if (info.STAT_TYPE != STAT_TYPE)
                return false;

            return base.IsEquals(info);
        }
        public override void SetEffectData(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData data, int skillLevel)
        {
            if (caster == null || target == null)
                return;

            base.SetEffectData(caster, target, data, skillLevel);
            Time = target.Stat.GetBuffTime(MAX_TIME);
            if (Time < 0)
                Time = 0;
            MaxTime = Time;
        }
        protected override float GetValueDefault()
        {
            if (Passive == null)
                return base.GetValueDefault();
            else
                return base.GetValueDefault() * (Passive.IsPassiveEffect(eSkillPassiveEffect.STRONG_BUFF) ? (1f + Passive.CONVERT_VALUE + (Passive.CONVERT_VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f))) : 1f);
        }
    }
}