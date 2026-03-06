using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PartStatus : UnitStatus
    {
        public override bool Initialze()
        {
            if (base.Initialze())
            {
                StatusDataType.Add(eStatusType.HP, eStatusValueType.VALUE);                          //HP 증가
                StatusDataType.Add(eStatusType.ATK, eStatusValueType.VALUE);                         //공격력 증가
                StatusDataType.Add(eStatusType.DEF, eStatusValueType.VALUE);                         //방어력 증가
                StatusDataType.Add(eStatusType.CRI_PROC, eStatusValueType.PERCENT);                  //크리확률 증가

                StatusDataType.Add(eStatusType.CRI_RESIS, eStatusValueType.PERCENT);                 //크리확률 저항
                StatusDataType.Add(eStatusType.CRI_DMG_RESIS, eStatusValueType.PERCENT);             //크리대미지 저항

                StatusDataType.Add(eStatusType.SKILL_DMG_RESIS, eStatusValueType.PERCENT);           //스킬대미지 저항

                StatusDataType.Add(eStatusType.ADD_BUFF_TIME, eStatusValueType.PERCENT);             //이로운효과 증가
                StatusDataType.Add(eStatusType.DEL_BUFF_TIME, eStatusValueType.PERCENT);             //해로운효과 감소

                StatusDataType.Add(eStatusType.ATK_DMG_RESIS, eStatusValueType.PERCENT);             //일반공격 저항

                StatusDataType.Add(eStatusType.ALL_ELEMENT_DMG_RESIS, eStatusValueType.PERCENT);     //모든속성 저항
                StatusDataType.Add(eStatusType.LIGHT_DMG_RESIS, eStatusValueType.PERCENT);           //빛속성 저항
                StatusDataType.Add(eStatusType.DARK_DMG_RESIS, eStatusValueType.PERCENT);            //어둠속성 저항
                StatusDataType.Add(eStatusType.WATER_DMG_RESIS, eStatusValueType.PERCENT);           //물속성 저항
                StatusDataType.Add(eStatusType.FIRE_DMG_RESIS, eStatusValueType.PERCENT);            //불속성 저항
                StatusDataType.Add(eStatusType.WIND_DMG_RESIS, eStatusValueType.PERCENT);            //바람속성 저항
                StatusDataType.Add(eStatusType.EARTH_DMG_RESIS, eStatusValueType.PERCENT);           //땅속성 저항

                StatusDataType.Add(eStatusType.DEL_COOLTIME, eStatusValueType.PERCENT);              //쿨타임 감소
                StatusDataType.Add(eStatusType.ADD_ATKSPEED, eStatusValueType.PERCENT);              //공격속도 증가
                /** 추가 스텟 */
                StatusDataType.Add(eStatusType.BOSS_DMG, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.BOSS_DMG_RESIS, eStatusValueType.PERCENT);
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

                StatusDataType.Add(eStatusType.CRI_DMG, eStatusValueType.VALUE);
                StatusDataType.Add(eStatusType.ADD_ATK_DMG, eStatusValueType.VALUE);

                SetStatus(true);
                return true;
            }
            return false;
        }
        private void IncreasePartData(string statType, string dataType, float value)
        {
            var category = dataType.ToUpper() switch
            {
                "PERCENT" => eStatusCategory.RATIO,
                _ => eStatusCategory.ADD
            };

            IncreaseStatus(category, SBFunc.ConvertStatusType(statType.ToUpper()), value);
        }
        //파츠의 기본 능력치 데이터 받기
        public void IncreaseStatus(UserPart data)
        {
            if (data == null)
                return;

            IncreasePartData(data.GetPartDesignData().STAT_TYPE, data.GetPartDesignData().VALUE_TYPE, data.GetValue());
        }
        /// <summary>
        /// 서브옵션 더하기
        /// </summary>
        /// <param name="status">SubOption</param>
        public void IncreaseStatus(SubOptionStatus status)
        {
            if (status == null)
                return;

            foreach (var type in StatusDataType)
            {
                if (!status.IsFlag(type.Key))
                    continue;

                IncreaseStatus(eStatusCategory.ADD, type.Key, status.GetStatus(eStatusCategory.ADD, type.Key));
                IncreaseStatus(eStatusCategory.RATIO, type.Key, status.GetStatus(eStatusCategory.RATIO, type.Key));
            }
        }
        //세트 능력치 넣기
        public void IncreaseStatus(PartSetData data)
        {
            if (data == null)
                return;

            IncreasePartData(data.STAT_TYPE, data.VALUE_TYPE, data.VALUE);
        }
        //옵션 능력치 넣기
        public void IncreaseStatus(SubOptionData data, float value)
        {
            if (data == null)
                return;

            IncreasePartData(data.STAT_TYPE, data.VALUE_TYPE, value);
        }

        /// <summary>
        /// 융합 능력치 더하기
        /// </summary>
        /// <param name="status">SubOption</param>
        public void IncreaseStatus(PartFusionData data, float value)
        {
            if (data == null)
                return;

            IncreasePartData(data.STAT, data.VALUE_TYPE, value);
        }
    }
}