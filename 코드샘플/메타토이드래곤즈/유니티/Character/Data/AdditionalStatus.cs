using System.Linq;

namespace SandboxNetwork
{
    public class AdditionalStatus : UnitStatus
    {
        public override bool Initialze()
        {
            if (base.Initialze())
            {
                StatusDataType.Add(eStatusType.HP, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.ATK, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.DEF, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.RATIO_ATK_DMG, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ADD_ATK_DMG, eStatusValueType.ADD_VALUE);
                StatusDataType.Add(eStatusType.CRI_PROC, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.CRI_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.RATIO_SKILL_DMG, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ADD_SKILL_DMG, eStatusValueType.ADD_VALUE);
                StatusDataType.Add(eStatusType.ALL_ELEMENT_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.LIGHT_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.DARK_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.WATER_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.WIND_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.FIRE_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.EARTH_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.CRI_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.CRI_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ATK_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.SKILL_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ADD_BUFF_TIME, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.DEL_BUFF_TIME, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ALL_ELEMENT_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.LIGHT_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.DARK_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.WATER_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.WIND_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.FIRE_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.EARTH_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.RATIO_PVP_DMG, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ADD_PVP_DMG, eStatusValueType.ADD_VALUE);
                StatusDataType.Add(eStatusType.RATIO_PVP_CRI_DMG, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ADD_PVP_CRI_DMG, eStatusValueType.ADD_VALUE);
                StatusDataType.Add(eStatusType.DEL_COOLTIME, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ADD_ATKSPEED, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.SHIELD_POINT, eStatusValueType.ADD_VALUE);
                /** 추가 스텟 */
                StatusDataType.Add(eStatusType.BOSS_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.BOSS_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ALL_DMG_RESIS, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.COOLTIME_SPEED, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.SHIELD_BREAKER, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.VAMP, eStatusValueType.PERCENT);
                //
                /** 추가의 추가 스텟 2024-02-26 */
                StatusDataType.Add(eStatusType.PHYS_DMG_RESIS, eStatusValueType.PERCENT);
                //
                /** 추가의 추 추가 스텟 2025-11-19 */
                StatusDataType.Add(eStatusType.DEL_START_COOLTIME, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ALL_ELEMENT_DMG_PIERCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.PHYS_DMG_PIERCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.CRI_DMG_PIERCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.RATIO_PASSIVE_EFFECT, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.RATIO_PASSIVE_RATE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.DEL_KNOCKBACK_DISTANCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.RATIO_BOSS_DMG, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.ADD_MAIN_ELEMENT_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.LIGHT_DMG_PIERCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.DARK_DMG_PIERCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.WATER_DMG_PIERCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.FIRE_DMG_PIERCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.WIND_DMG_PIERCE, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.EARTH_DMG_PIERCE, eStatusValueType.PERCENT);

                StatusDataType.Add(eStatusType.ACCURACY, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.EVASION, eStatusValueType.PERCENT);
                SetStatus(true);
                return true;
            }
            return false;
        }
        public void IncreaseStatus(UnitStatus status)
        {
            foreach (var type in StatusDataType)
            {
                if (!status.IsFlag(type.Key))
                    continue;

                IncreaseStatus(eStatusCategory.ADD, type.Key, status.GetStatus(eStatusCategory.ADD, type.Key));
                IncreaseStatus(eStatusCategory.RATIO, type.Key, status.GetStatus(eStatusCategory.RATIO, type.Key));
            }
        }
        public void IncreaseStatData(string statType, string dataType, float value)
        {
            var category = dataType.ToUpper() switch
            {
                "PERCENT" => eStatusCategory.RATIO,
                _ => eStatusCategory.ADD
            };

            IncreaseStatus(category, SBFunc.ConvertStatusType(statType.ToUpper()), value);
        }
        public void IncreaseStatData(eStatusType type, eStatusValueType valueType, float value)
        {
            var category = valueType switch
            {
                eStatusValueType.PERCENT => eStatusCategory.RATIO,
                eStatusValueType.VALUE => eStatusCategory.ADD,
                _ => eStatusCategory.START
            };

            IncreaseStatus(category, type, value);
        }

        public virtual void RefreshStatus()
        {

        }
        public void ClearAll()
        {
            base.Clear();
        }
    }

    public class CollectionStatus : AdditionalStatus
    {
        public override bool Initialze()
        {
            if(base.Initialze())
            {
                RefreshStatus();
                return true;
            }
            return false;
        }

        public override void RefreshStatus()
        {
            foreach (var collection in CollectionAchievementManager.Instance.CompleteCollectionDic.Values.ToList())
            {
                if (collection == null)
                    continue;

                var baseData = collection.CollectionBaseData;
                if (baseData == null)
                    continue;

                IncreaseStatData(baseData.REWARD_STAT_TYPE, baseData.REWARD_STAT_VALUE_TYPE, baseData.REWARD_STAT_VALUE);
            }
        }
    }
    public class AchievementStatus : AdditionalStatus
    {
        public override bool Initialze()
        {
            if (base.Initialze())
            {
                RefreshStatus();
                return true;
            }
            return false;
        }

        public override void RefreshStatus()
        {
            foreach (var achievement in CollectionAchievementManager.Instance.CompleteAchievementDic.Values.ToList())
            {
                if (achievement == null)
                    continue;

                var baseData = achievement.achievementBaseData;
                if (baseData == null)
                    continue;

                IncreaseStatData(baseData.REWARD_STAT_TYPE, baseData.REWARD_STAT_VALUE_TYPE, baseData.REWARD_STAT_VALUE);
            }
        }
    }

    public class GuildStatus : AdditionalStatus
    {
        public override bool Initialze()
        {
            if (base.Initialze())
            {
                return true;
            }
            return false;
        }
    }
}
