namespace SandboxNetwork
{
    public class PassiveBuffEffect : BuffEffect
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
        public override eSkillEffectType EFFECT_TYPE => eSkillEffectType.BUFF;
        public override eStatusType STAT_TYPE => Passive.STAT;
        public override eStatusValueType VALUE_TYPE => Passive.EFFECT_VALUE;
        public override int NEST_GROUP { get => Passive.NEST_GROUP; }
        public override int NEST_COUNT { get => Passive.NEST_COUNT; }
        public override float VALUE { get => Passive.VALUE + (Passive.VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f)); }
        public override float MAX_TIME { get => Passive.MAX_TIME; }
        protected override void TriggerEvent()
        {
            var category = eStatusCategory.ADD_BUFF;
            if (VALUE_TYPE == eStatusValueType.PERCENT)
                category = eStatusCategory.RATIO_BUFF;
            Target.Stat.IncreaseStatus(category, STAT_TYPE, GetValue(), true);
            SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy, true);
        }
        protected override void CompleteEvent()
        {
            var category = eStatusCategory.ADD_BUFF;
            if (VALUE_TYPE == eStatusValueType.PERCENT)
                category = eStatusCategory.RATIO_BUFF;
            Target.Stat.DecreaseStatus(category, STAT_TYPE, GetValue(), true);
            SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy, true);
        }
        protected override float GetValueDefault()
        {
            if (Passive == null)
                return 0;

            return Passive.VALUE + (Passive.VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f));
        }
        public override bool IsEquals(EffectInfo info)
        {
            if (info is not PassiveBuffEffect)
                return false;

            return base.IsEquals(info);
        }
        public override bool IsCover(EffectInfo info)
        {
            if (info is not PassiveBuffEffect)
                return false;

            return base.IsCover(info);
        }
    }
}