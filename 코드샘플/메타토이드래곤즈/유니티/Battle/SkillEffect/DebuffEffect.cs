using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DebuffEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            var category = eStatusCategory.ADD_BUFF;
            if (VALUE_TYPE == eStatusValueType.PERCENT)
                category = eStatusCategory.RATIO_BUFF;

            switch (STAT_TYPE)
            {
                case eStatusType.ADD_PVP_DMG:
                case eStatusType.RATIO_PVP_DMG:
                case eStatusType.ADD_PVP_CRI_DMG:
                case eStatusType.RATIO_PVP_CRI_DMG:
                case eStatusType.ADD_SKILL_DMG:
                case eStatusType.ADD_ATK_DMG:
                case eStatusType.DEL_COOLTIME:
                case eStatusType.ADD_BUFF_TIME:
                case eStatusType.DEL_BUFF_TIME:
                case eStatusType.ATK_DMG_RESIS:
                case eStatusType.ALL_ELEMENT_DMG_RESIS:
                case eStatusType.LIGHT_DMG_RESIS:
                case eStatusType.DARK_DMG_RESIS:
                case eStatusType.WATER_DMG_RESIS:
                case eStatusType.FIRE_DMG_RESIS:
                case eStatusType.WIND_DMG_RESIS:
                case eStatusType.EARTH_DMG_RESIS:
                case eStatusType.SKILL_DMG_RESIS:
                case eStatusType.CRI_DMG_RESIS:
                case eStatusType.CRI_RESIS:
                case eStatusType.CRI_PROC:
                case eStatusType.RATIO_ATK_DMG:
                case eStatusType.RATIO_SKILL_DMG:
                case eStatusType.ACCURACY:
                case eStatusType.EVASION:
                    Target.Stat.DecreaseStatus(category, STAT_TYPE, GetValue(), true);
                    SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy);
                    break;
                default:
                    Target.Stat.AddDecreaseInfo(this);
                    SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy);
                    break;
            }
        }
        protected override void CompleteEvent()
        {
            var category = eStatusCategory.ADD_BUFF;
            if (VALUE_TYPE == eStatusValueType.PERCENT)
                category = eStatusCategory.RATIO_BUFF;

            switch (STAT_TYPE)
            {
                case eStatusType.ADD_PVP_DMG:
                case eStatusType.RATIO_PVP_DMG:
                case eStatusType.ADD_PVP_CRI_DMG:
                case eStatusType.RATIO_PVP_CRI_DMG:
                case eStatusType.ADD_SKILL_DMG:
                case eStatusType.ADD_ATK_DMG:
                case eStatusType.DEL_COOLTIME:
                case eStatusType.ADD_BUFF_TIME:
                case eStatusType.DEL_BUFF_TIME:
                case eStatusType.ATK_DMG_RESIS:
                case eStatusType.ALL_ELEMENT_DMG_RESIS:
                case eStatusType.LIGHT_DMG_RESIS:
                case eStatusType.DARK_DMG_RESIS:
                case eStatusType.WATER_DMG_RESIS:
                case eStatusType.FIRE_DMG_RESIS:
                case eStatusType.WIND_DMG_RESIS:
                case eStatusType.EARTH_DMG_RESIS:
                case eStatusType.SKILL_DMG_RESIS:
                case eStatusType.CRI_DMG_RESIS:
                case eStatusType.CRI_RESIS:
                case eStatusType.CRI_PROC:
                case eStatusType.RATIO_ATK_DMG:
                case eStatusType.RATIO_SKILL_DMG:
                case eStatusType.ACCURACY:
                case eStatusType.EVASION:
                    Target.Stat.IncreaseStatus(category, STAT_TYPE, GetValue(), true);
                    SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy);
                    break;
                default:
                    Target.Stat.DelDecreaseInfo(this);
                    SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy);
                    break;
            }
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
            Time = target.Stat.GetDebuffTime(MAX_TIME);
            if (Time < 0)
                Time = 0;
            MaxTime = Time;
        }
        protected override float GetValueDefault()
        {
            if (Passive == null)
                return base.GetValueDefault();
            else
                return base.GetValueDefault() * (Passive.IsPassiveEffect(eSkillPassiveEffect.STRONG_DEBUFF) ? (1f + Passive.CONVERT_VALUE + (Passive.CONVERT_VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f))) : 1f);
        }
    }
}