using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PetStatus : UnitStatus
    {
        public override bool Initialze()
        {
            if (base.Initialze())
            {
                StatusDataType.Add(eStatusType.HP, eStatusValueType.VALUE);                      //테이블에는 없지만 혹시모르니 넣어둠
                StatusDataType.Add(eStatusType.ATK, eStatusValueType.VALUE);                     //테이블에는 없지만 혹시모르니 넣어둠
                StatusDataType.Add(eStatusType.DEF, eStatusValueType.VALUE);                     //테이블에는 없지만 혹시모르니 넣어둠
                StatusDataType.Add(eStatusType.CRI_PROC, eStatusValueType.VALUE);                //테이블에는 없지만 혹시모르니 넣어둠

                StatusDataType.Add(eStatusType.ADD_ATK_DMG, eStatusValueType.ADD_VALUE);         //추가 기본대미지
                StatusDataType.Add(eStatusType.RATIO_ATK_DMG, eStatusValueType.PERCENT);         //기본대미지 증가

                StatusDataType.Add(eStatusType.CRI_DMG, eStatusValueType.VALUE);                 //치명타 대미지

                StatusDataType.Add(eStatusType.ADD_SKILL_DMG, eStatusValueType.VALUE);           //추가 스킬대미지
                StatusDataType.Add(eStatusType.RATIO_SKILL_DMG, eStatusValueType.PERCENT);         //스킬대미지 증가

                StatusDataType.Add(eStatusType.ALL_ELEMENT_DMG, eStatusValueType.VALUE);         //모든속성 대미지증가
                StatusDataType.Add(eStatusType.LIGHT_DMG, eStatusValueType.VALUE);               //빛속성 대미지증가
                StatusDataType.Add(eStatusType.DARK_DMG, eStatusValueType.VALUE);                //어둠속성 대미지증가
                StatusDataType.Add(eStatusType.WATER_DMG, eStatusValueType.VALUE);               //물속성 대미지증가
                StatusDataType.Add(eStatusType.WIND_DMG, eStatusValueType.VALUE);                //바람속성 대미지증가
                StatusDataType.Add(eStatusType.FIRE_DMG, eStatusValueType.VALUE);                //화염속성 대미지증가
                StatusDataType.Add(eStatusType.EARTH_DMG, eStatusValueType.VALUE);               //땅속성 대미지증가

                StatusDataType.Add(eStatusType.ADD_PVP_DMG, eStatusValueType.ADD_VALUE);         //PVP 추가대미지
                StatusDataType.Add(eStatusType.RATIO_PVP_DMG, eStatusValueType.PERCENT);         //PVP 대미지증가
                StatusDataType.Add(eStatusType.ADD_PVP_CRI_DMG, eStatusValueType.ADD_VALUE);     //PVP 추가크리대미지
                StatusDataType.Add(eStatusType.RATIO_PVP_CRI_DMG, eStatusValueType.PERCENT);     //PVP 크리대미지증가

                StatusDataType.Add(eStatusType.ADD_BUFF_TIME, eStatusValueType.PERCENT);         //이로운 효과증가
                StatusDataType.Add(eStatusType.DEL_BUFF_TIME, eStatusValueType.PERCENT);         //해로운 효과감소
                /** 추가 스텟 */
                StatusDataType.Add(eStatusType.BOSS_DMG, eStatusValueType.PERCENT);
                StatusDataType.Add(eStatusType.BOSS_DMG_RESIS, eStatusValueType.PERCENT);
                //

                SetStatus(true);
                return true;
            }
            return false;
        }
        public void IncreaseStatus(string statType, eStatusValueType dataType, float value)
        {
            var category = dataType switch
            {
                eStatusValueType.PERCENT => eStatusCategory.RATIO,
                _ => eStatusCategory.ADD
            };

            IncreaseStatus(category, SBFunc.ConvertStatusType(statType.ToUpper()), value);
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
    }
}