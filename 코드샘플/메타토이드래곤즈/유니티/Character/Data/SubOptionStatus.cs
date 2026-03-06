using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SubOptionStatus : UnitStatus
    {
        public override bool Initialze()
        {
            if (base.Initialze())
            {
                StatusDataType.Add(eStatusType.HP, eStatusValueType.VALUE);                          //HP 증가
                StatusDataType.Add(eStatusType.ATK, eStatusValueType.VALUE);                         //공격력 증가
                StatusDataType.Add(eStatusType.DEF, eStatusValueType.VALUE);                         //방어력 증가
                StatusDataType.Add(eStatusType.CRI_PROC, eStatusValueType.PERCENT);                  //크리확률 증가

                StatusDataType.Add(eStatusType.ADD_ATK_DMG, eStatusValueType.ADD_VALUE);             //추가 기본대미지
                StatusDataType.Add(eStatusType.RATIO_ATK_DMG, eStatusValueType.PERCENT);             //기본대미지 증가

                StatusDataType.Add(eStatusType.CRI_DMG, eStatusValueType.VALUE);                     //치명타 대미지

                StatusDataType.Add(eStatusType.ADD_SKILL_DMG, eStatusValueType.VALUE);               //추가 스킬대미지
                StatusDataType.Add(eStatusType.RATIO_SKILL_DMG, eStatusValueType.PERCENT);             //스킬대미지 증가

                StatusDataType.Add(eStatusType.ALL_ELEMENT_DMG, eStatusValueType.VALUE);             //모든속성 대미지증가
                StatusDataType.Add(eStatusType.LIGHT_DMG, eStatusValueType.VALUE);                   //빛속성 대미지증가
                StatusDataType.Add(eStatusType.DARK_DMG, eStatusValueType.VALUE);                    //어둠속성 대미지증가
                StatusDataType.Add(eStatusType.WATER_DMG, eStatusValueType.VALUE);                   //물속성 대미지증가
                StatusDataType.Add(eStatusType.WIND_DMG, eStatusValueType.VALUE);                    //바람속성 대미지증가
                StatusDataType.Add(eStatusType.FIRE_DMG, eStatusValueType.VALUE);                    //화염속성 대미지증가
                StatusDataType.Add(eStatusType.EARTH_DMG, eStatusValueType.VALUE);                   //땅속성 대미지증가

                StatusDataType.Add(eStatusType.ADD_PVP_DMG, eStatusValueType.ADD_VALUE);             //PVP 추가대미지
                StatusDataType.Add(eStatusType.RATIO_PVP_DMG, eStatusValueType.PERCENT);             //PVP 대미지증가
                StatusDataType.Add(eStatusType.ADD_PVP_CRI_DMG, eStatusValueType.ADD_VALUE);         //PVP 추가크리대미지
                StatusDataType.Add(eStatusType.RATIO_PVP_CRI_DMG, eStatusValueType.PERCENT);         //PVP 크리대미지증가

                StatusDataType.Add(eStatusType.ADD_BUFF_TIME, eStatusValueType.PERCENT);             //이로운 효과증가
                StatusDataType.Add(eStatusType.DEL_BUFF_TIME, eStatusValueType.PERCENT);             //해로운 효과감소

                StatusDataType.Add(eStatusType.CRI_RESIS, eStatusValueType.PERCENT);                 //크리확률 저항
                StatusDataType.Add(eStatusType.CRI_DMG_RESIS, eStatusValueType.PERCENT);             //크리대미지 저항

                StatusDataType.Add(eStatusType.SKILL_DMG_RESIS, eStatusValueType.PERCENT);           //스킬대미지 저항

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

                StatusDataType.Add(eStatusType.PHYS_DMG_RESIS, eStatusValueType.PERCENT);              //공격속도 증가

                //추가 스텟 스펙? 
                StatusDataType.Add(eStatusType.BOSS_DMG, eStatusValueType.PERCENT);              //보스 대미지 증폭

                SetStatus(true);
                return true;
            }
            return false;
        }

        public void IncreaseStatus(string statType, string dataType, float value)
        {
            var category = dataType.ToUpper() switch
            {
                "PERCENT" => eStatusCategory.RATIO,
                _ => eStatusCategory.ADD
            };

            IncreaseStatus(category, SBFunc.ConvertStatusType(statType.ToUpper()), value);
        }
    }
}
