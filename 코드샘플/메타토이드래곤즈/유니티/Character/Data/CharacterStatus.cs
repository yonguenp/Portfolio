using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CharacterStatus : UnitStatus
    {
        public float TotalINF { get; protected set; } = 0;
        public float StatINF { get; protected set; } = 0;
        public float SkillLevel { get; protected set; } = 0;
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

        /// <summary>
        /// 전체 전투력 계산 함수 - 항상 하지말고 스텟이 변경됬을 때만 하기
        /// </summary>
        public void CalcINF()
        {
            StatINF = GetTotalStatus(eStatusType.ATK) * SBDefine.STAT_INF_ATK;
            StatINF += GetTotalStatus(eStatusType.DEF) * SBDefine.STAT_INF_DEF;
            StatINF += GetTotalStatus(eStatusType.HP) * SBDefine.STAT_INF_HP;
            StatINF += GetTotalStatus(eStatusType.ADD_ATK_DMG) * SBDefine.STAT_INF_ADD_ATK_DMG;
            StatINF += GetTotalStatus(eStatusType.FIRE_DMG) * SBDefine.STAT_INF_ELEMENT;
            StatINF += GetTotalStatus(eStatusType.WATER_DMG) * SBDefine.STAT_INF_ELEMENT;
            StatINF += GetTotalStatus(eStatusType.EARTH_DMG) * SBDefine.STAT_INF_ELEMENT;
            StatINF += GetTotalStatus(eStatusType.WIND_DMG) * SBDefine.STAT_INF_ELEMENT;
            StatINF += GetTotalStatus(eStatusType.LIGHT_DMG) * SBDefine.STAT_INF_ELEMENT;
            StatINF += GetTotalStatus(eStatusType.DARK_DMG) * SBDefine.STAT_INF_ELEMENT;
            StatINF += GetStatus(eStatusCategory.BASE, eStatusType.CRI_DMG) * SBDefine.STAT_INF_ADD_CRI_DMG;
            StatINF += GetStatus(eStatusCategory.ADD, eStatusType.CRI_DMG) * SBDefine.STAT_INF_ADD_CRI_DMG;
            StatINF += GetTotalStatus(eStatusType.RATIO_ATK_DMG) * SBDefine.STAT_INF_RATIO_ATK_DMG;
            StatINF += GetStatus(eStatusCategory.RATIO, eStatusType.CRI_DMG) * SBDefine.STAT_INF_RATIO_CRI_DMG;
            StatINF += GetTotalStatus(eStatusType.ADD_ATKSPEED) * SBDefine.STAT_INF_ADD_ATKSPEED;
            StatINF += GetTotalStatus(eStatusType.ATK_DMG_RESIS) * SBDefine.STAT_INF_ATK_DMG_RESIS;
            StatINF += GetTotalStatus(eStatusType.CRI_DMG_RESIS) * SBDefine.STAT_INF_CRI_DMG_RESIS;
            StatINF += GetTotalStatus(eStatusType.FIRE_DMG_RESIS) * SBDefine.STAT_INF_ELEMENT_RESIS;
            StatINF += GetTotalStatus(eStatusType.WATER_DMG_RESIS) * SBDefine.STAT_INF_ELEMENT_RESIS;
            StatINF += GetTotalStatus(eStatusType.EARTH_DMG_RESIS) * SBDefine.STAT_INF_ELEMENT_RESIS;
            StatINF += GetTotalStatus(eStatusType.WIND_DMG_RESIS) * SBDefine.STAT_INF_ELEMENT_RESIS;
            StatINF += GetTotalStatus(eStatusType.LIGHT_DMG_RESIS) * SBDefine.STAT_INF_ELEMENT_RESIS;
            StatINF += GetTotalStatus(eStatusType.DARK_DMG_RESIS) * SBDefine.STAT_INF_ELEMENT_RESIS;

            TotalINF = StatINF + SkillLevel * SBDefine.STAT_INF_SKILL_LEVEL;
        }

        public override void CalcStatusAll()
        {
            foreach (var type in StatusDataType)
            {
                if (StatusFlag[type.Key] == false)
                    continue;

                //계산
                CalcStatus(type.Key);
            }

            CalcINF();
        }

        public void SetSkillLevel(int level)
        {
            SkillLevel = level;
        }
        /// <summary>
        /// 총 전투력 UI 표시용
        /// </summary>
        /// <returns>전투력(int)</returns>
        public int GetTotalINF()
        {
            return Mathf.FloorToInt(TotalINF);
        }
        /// <summary>
        /// 비교만 하는 용도로 사용하는 전투력(소숫점 자리 있음)
        /// </summary>
        /// <returns>전투력(float)</returns>
        public float GetTotalINFFloat()
        {
            return TotalINF;
        }

        public int GetTankerStatus()
        {
            return (int)(GetTotalStatus(eStatusType.HP) + (GetTotalStatus(eStatusType.DEF) * 10f));
        }

        public void IncreaseStatus(UnitStatus status, bool refresh = false)
        {
            foreach (var type in StatusDataType)
            {
                if (!status.IsFlag(type.Key))
                    continue;

                IncreaseStatus(eStatusCategory.ADD, type.Key, status.GetStatus(eStatusCategory.ADD, type.Key), refresh);
                IncreaseStatus(eStatusCategory.RATIO, type.Key, status.GetStatus(eStatusCategory.RATIO, type.Key), refresh);
            }
        }
        public void DecreaseStatus(UnitStatus status, bool refresh = false)
        {
            foreach (var type in StatusDataType)
            {
                if (!status.IsFlag(type.Key))
                    continue;

                DecreaseStatus(eStatusCategory.ADD, type.Key, status.GetStatus(eStatusCategory.ADD, type.Key), refresh);
                DecreaseStatus(eStatusCategory.RATIO, type.Key, status.GetStatus(eStatusCategory.RATIO, type.Key), refresh);
            }
        }
        public CharacterStatus Clone()
        {
            var clone = new CharacterStatus();
            clone.Initialze();

            var statIt = Status.GetEnumerator();
            while (statIt.MoveNext())
            {
                var statType = statIt.Current.Key;
                var statDic = statIt.Current.Value;
                if (statDic == null)
                    continue;

                var categoryIt = statDic.GetEnumerator();
                while (categoryIt.MoveNext())
                {
                    var category = categoryIt.Current.Key;
                    var stat = categoryIt.Current.Value;

                    clone.IncreaseStatus(category, statType, stat);
                }
            }

            clone.SetSkillLevel((int)SkillLevel);
            clone.CalcStatusAll();

            return clone;
        }
    }
    public class BossCharacterStatus : CharacterStatus
    {
        protected List<CharacterStatus> partsStatus = null;
        public void AddPartsStatus(CharacterStatus status)
        {
            if (partsStatus == null)
                partsStatus = new();

            partsStatus.Add(status);
        }
        public override void IncreaseStatus(eStatusCategory category_, eStatusType type_, float value_, bool calc_ = false)
        {
            base.IncreaseStatus(category_, type_, value_, calc_);

            if (partsStatus != null)
            {
                for (int i = 0, count = partsStatus.Count; i < count; ++i)
                {
                    if (partsStatus[i] == null)
                        continue;

                    partsStatus[i].IncreaseStatus(category_, type_, value_, calc_);
                }
            }
        }
        public override void DecreaseStatus(eStatusCategory category_, eStatusType type_, float value_, bool calc_ = false)
        {
            base.DecreaseStatus(category_, type_, value_, calc_);

            if (partsStatus != null)
            {
                for (int i = 0, count = partsStatus.Count; i < count; ++i)
                {
                    if (partsStatus[i] == null)
                        continue;

                    partsStatus[i].DecreaseStatus(category_, type_, value_, calc_);
                }
            }
        }
        public override void AddDecreaseInfo(EffectInfo info)
        {
            base.AddDecreaseInfo(info);

            if (partsStatus != null)
            {
                for (int i = 0, count = partsStatus.Count; i < count; ++i)
                {
                    if (partsStatus[i] == null)
                        continue;

                    partsStatus[i].DelDecreaseInfo(info);
                }
            }
        }
        public override void DelDecreaseInfo(EffectInfo info)
        {
            base.DelDecreaseInfo(info);

            if (partsStatus != null)
            {
                for (int i = 0, count = partsStatus.Count; i < count; ++i)
                {
                    if (partsStatus[i] == null)
                        continue;

                    partsStatus[i].DelDecreaseInfo(info);
                }
            }
        }
    }
}