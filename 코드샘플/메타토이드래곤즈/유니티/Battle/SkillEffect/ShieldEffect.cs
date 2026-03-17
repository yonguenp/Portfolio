using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ShieldEffect : EffectInfo
    {
        private int shieldPoint = 0;
        private int maxShieldPoint = 0;
        protected override void TriggerEvent()
        {
            float value;
            switch (STAT_TYPE)
            {
                case eStatusType.ATK:
                    value = Target.Stat.GetTotalStatusInt(eStatusType.ATK);
                    break;
                case eStatusType.DEF:
                    value = Target.Stat.GetTotalStatusInt(eStatusType.DEF);
                    break;
                case eStatusType.HP:
                    value = Target.MaxHP;
                    break;
                case eStatusType.CRI_DMG:
                    value = Target.Stat.GetTotalStatusInt(eStatusType.CRI_DMG);
                    break;
                default:
                    value = GetValue();
                    break;
            }

            switch (VALUE_TYPE)
            {
                case eStatusValueType.PERCENT:
                    shieldPoint = Mathf.FloorToInt(value * GetValue() * SBDefine.CONVERT_FLOAT);
                    break;
                default:
                    shieldPoint = Mathf.FloorToInt(value);
                    break;
            }
            var skillStat = Stat;
            if(skillStat == null)
                skillStat = Data.GetEffectStat(SkillLevel);
            if (skillStat.ATK > 0)
                shieldPoint += SBFunc.CalcRatioInt(Caster.Stat.GetTotalStatusInt(eStatusType.ATK), skillStat.ATK);
            if (skillStat.DEF > 0)
                shieldPoint += SBFunc.CalcRatioInt(Caster.Stat.GetTotalStatusInt(eStatusType.DEF), skillStat.DEF);
            if (skillStat.HP > 0)
                shieldPoint += SBFunc.CalcRatioInt(Caster.Stat.GetTotalStatusInt(eStatusType.HP), skillStat.HP);

            maxShieldPoint = shieldPoint;
            Target.Stat.IncreaseStatus(eStatusCategory.BASE, eStatusType.SHIELD_POINT, maxShieldPoint, true);
            Target.Stat.IncreaseStatus(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT, shieldPoint, true);
        }
        protected override void CompleteEvent()
        {
            Target.Stat.DecreaseStatus(eStatusCategory.BASE, eStatusType.SHIELD_POINT, maxShieldPoint, true);
            Target.Stat.DecreaseStatus(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT, shieldPoint, true);
        }
        public override bool IsEquals(EffectInfo info)
        {
            return false;
        }
        public override int SetDamage(int damage)
        {
            shieldPoint -= damage;
            if (shieldPoint <= 0)
            {
                var temp = shieldPoint;
                shieldPoint = 0;
                Time = 0f;
                Target.Stat.DecreaseStatus(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT, damage + temp, true);
                return -temp;
            }
            Target.Stat.DecreaseStatus(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT, damage, true);
            return 0;
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