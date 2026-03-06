using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PassiveSilenceEffect : SilenceEffect
    {
        protected override string NAME
        {
            get
            {
                if (Passive == null)
                    return "";

                if (Passive.NEST_GROUP == 0)
                {
                    ++uniqueKey;
                    return SBFunc.StrBuilder(uniqueKey, "-");
                }
                else
                    return Passive.GetPassiveName();
            }
        }
        public override eSkillEffectType EFFECT_TYPE => eSkillEffectType.SILENCE;
        public override eStatusType STAT_TYPE => Passive.STAT;
        public override eStatusValueType VALUE_TYPE => Passive.EFFECT_VALUE;
        public override int NEST_GROUP { get => Passive.NEST_GROUP; }
        public override int NEST_COUNT { get => Passive.NEST_COUNT; }
        public override float VALUE { get => Passive.VALUE + (Passive.VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f)); }
        public override float MAX_TIME { get => Passive.MAX_TIME; }

        protected override void CompleteEvent()
        {
            var spine = Target.GetSpine();
            if (spine == null)
                return;

            spine.SkillActionCancle();
            SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy, true);
        }
        protected override void TriggerEvent()
        {
            var spine = Target.GetSpine();
            if (spine == null)
                return;

            spine.SkillActionCancle();
            SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy, true);
        }
        protected override float GetValueDefault()
        {
            if (Passive == null)
                return 0;

            return Passive.VALUE + (Passive.VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f));
        }
        public override bool IsEquals(EffectInfo info)
        {
            if (info is not PassiveSilenceEffect)
                return false;

            return base.IsEquals(info);
        }

        public override bool IsCover(EffectInfo info)
        {
            if (info is not PassiveSilenceEffect)
                return false;

            if (info.MAX_TIME >= MAX_TIME)
                return true;

            return base.IsCover(info);
        }
    }
}