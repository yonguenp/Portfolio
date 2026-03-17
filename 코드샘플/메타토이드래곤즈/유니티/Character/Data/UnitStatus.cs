using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public enum eExtraStatContent
    {
        NONE = 0,

        ACHIEVEMENT = 1,
        COLLECTION = 2,
        SHOWCASE = 3,
        DAPP = 4,
        ARTBLOCK = 5,

        MAX = 6,
    }
    public enum eStatusCategory
    {
        NONE = -1,

        START = 0,
        /// <summary>  베이스 스텟 </summary>
        BASE = START,
        /// <summary> +증가 </summary>
        ADD,
        /// <summary> %증가 </summary>
        RATIO,
        /// <summary> +증가 버프 </summary>
        ADD_BUFF,
        /// <summary> %증가 버프 </summary>
        RATIO_BUFF,
        /// <summary> 계산 완료된 총합 </summary>
        TOTAL,

        MAX,
    }
    public enum eStatusType
    {
        NONE = -1,

        START,
        /// <summary>  체력(스텟)  </summary>
        ATK = START,
        /// <summary> 공격력(스텟) </summary>
        DEF,
        /// <summary> 방어력(스텟) </summary>
        HP,
        /// <summary> 추가 기본 대미지 </summary> 
        ADD_ATK_DMG,
        /// <summary> 기본 대미지 증가 </summary> 
        RATIO_ATK_DMG,
        /// <summary> 크리 확률(스텟) </summary> 
        CRI_PROC,
        /// <summary> 크리 대미지(스텟) </summary> 
        CRI_DMG,
        /// <summary> 스킬 대미지 증가 </summary> 
        RATIO_SKILL_DMG,
        /// <summary> 추가 스킬 대미지 </summary> 
        ADD_SKILL_DMG,
        /// <summary> 모든 속성 대미지(스텟, 직접 사용 X) </summary> 
        ALL_ELEMENT_DMG,
        /// <summary> 빛 속성 대미지(스텟) </summary> 
        LIGHT_DMG,
        /// <summary> 어둠 속성 대미지(스텟) </summary> 
        DARK_DMG,
        /// <summary> 물 속성 대미지(스텟) </summary> 
        WATER_DMG,
        /// <summary> 불 속성 대미지(스텟) </summary> 
        FIRE_DMG,
        /// <summary> 바람 속성 대미지(스텟) </summary> 
        WIND_DMG,
        /// <summary> 땅 속성 대미지(스텟) </summary> 
        EARTH_DMG,
        /// <summary> 크리 확률 저항 </summary> 
        CRI_RESIS,
        /// <summary> 크리 대미지 저항 </summary> 
        CRI_DMG_RESIS,
        /// <summary> 스킬 대미지 저항 </summary> 
        SKILL_DMG_RESIS,
        /// <summary> 이로운 효과 증가 </summary> 
        ADD_BUFF_TIME,
        /// <summary> 해로운 효과 감소 </summary> 
        DEL_BUFF_TIME,
        /// <summary> 기본 대미지 저항 </summary> 
        ATK_DMG_RESIS,
        /// <summary> 모든 속성 저항(직접 사용 X) </summary> 
        ALL_ELEMENT_DMG_RESIS,
        /// <summary> 빛 속성 저항 </summary>  
        LIGHT_DMG_RESIS,
        /// <summary> 어둠 속성 저항 </summary> 
        DARK_DMG_RESIS,
        /// <summary> 물 속성 저항 </summary> 
        WATER_DMG_RESIS,
        /// <summary> 불 속성 저항 </summary> 
        FIRE_DMG_RESIS,
        /// <summary> 바람 속성 저항 </summary> 
        WIND_DMG_RESIS,
        /// <summary> 땅 속성 저항 </summary> 
        EARTH_DMG_RESIS,
        /// <summary> 추가 PvP 대미지 </summary> 
        ADD_PVP_DMG,
        /// <summary> PvP 대미지 </summary> 
        RATIO_PVP_DMG,
        /// <summary> 추가 PvP 크리 대미지 </summary> 
        ADD_PVP_CRI_DMG,
        /// <summary> PvP 크리 대미지 </summary> 
        RATIO_PVP_CRI_DMG,
        /// <summary> 쿨타임 감소 </summary> 
        DEL_COOLTIME,
        /// <summary> 공격 속도 증가 </summary> 
        ADD_ATKSPEED,
        /// <summary> 보호막 </summary>
        SHIELD_POINT,
        /// <summary> 보스 대미지 </summary>
        BOSS_DMG,
        /// <summary> 보스 대미지 저항 </summary>
        BOSS_DMG_RESIS,
        /// <summary> 캐릭터 속성 대미지 </summary>
        ALL_DMG_RESIS,
        /// <summary> 쿨타임 감소하는 속도 (ATK_SPEED[공격속도]와 유사함) </summary>
        COOLTIME_SPEED,
        /// <summary> 실드 타입 대미지 </summary>
        SHIELD_BREAKER,
        /// <summary> 대미지 생명력 회복 </summary>
        VAMP,
        /// <summary> 물리 대미지 저항(기본 대미지, 크리 대미지 둘 다 올라감) </summary>
        PHYS_DMG_RESIS,
        
        //2025-11-17 추가 옵션들
        PHYS_DMG_PIERCE = 43, // 물리 데미지 관통
        CRI_DMG_PIERCE = 44, // 치명 대미지 관통
        ALL_ELEMENT_DMG_PIERCE = 45, // 모든 속성 관통
        LIGHT_DMG_PIERCE = 46, // 빛속성대미지 관통
        DARK_DMG_PIERCE = 47, // 어둠속성대미지 관통
        WATER_DMG_PIERCE = 48, // 물속성대미지 관통
        FIRE_DMG_PIERCE = 49, // 불속성대미지 관통
        WIND_DMG_PIERCE = 50, // 바람속성대미지 관통
        EARTH_DMG_PIERCE = 51, // 땅속성대미지 관통
        DEL_START_COOLTIME = 52, // 스킬 시작쿨 감소
        RATIO_PASSIVE_EFFECT = 53, // 패시브 효과 증폭
        RATIO_PASSIVE_RATE = 54, // 패시브 확률 증폭
        DEL_KNOCKBACK_DISTANCE = 55, // 넉백 거리 감소
        RATIO_BOSS_DMG = 56, // 보스 대미지 증폭
        ADD_MAIN_ELEMENT_DMG = 57, // 자기 속댐 증가

        //2026-01-15 추가
        ACCURACY = 58,// 명중률
        EVASION = 59,// 회피율

        /// <summary> 일반 스텟 MAX </summary>
        MAX,
        /// <summary> 서버 퍼센트 수치 기본 </summary>
        PERC_BASE = SBDefine.THOUSAND,
        /// <summary>  체력(스텟)  </summary>
        ATK_PREC = PERC_BASE + ATK,
        /// <summary> 공격력(스텟) </summary>
        DEF_PREC = PERC_BASE + DEF,
        /// <summary> 방어력(스텟) </summary>
        HP_PREC = PERC_BASE + HP,
        /// <summary> 추가 기본 대미지 </summary> 
        ADD_ATK_DMG_PREC = PERC_BASE + ADD_ATK_DMG,
        /// <summary> 기본 대미지 증가 </summary> 
        RATIO_ATK_DMG_PREC = PERC_BASE + RATIO_ATK_DMG,
        /// <summary> 크리 확률(스텟) </summary> 
        CRI_PROC_PREC = PERC_BASE + CRI_PROC,
        /// <summary> 크리 대미지(스텟) </summary> 
        CRI_DMG_PREC = PERC_BASE + CRI_DMG,
        /// <summary> 스킬 대미지 증가 </summary> 
        RATIO_SKILL_DMG_PREC = PERC_BASE + RATIO_SKILL_DMG,
        /// <summary> 추가 스킬 대미지 </summary> 
        ADD_SKILL_DMG_PREC = PERC_BASE + ADD_SKILL_DMG,
        /// <summary> 모든 속성 대미지(스텟, 직접 사용 X) </summary> 
        ALL_ELEMENT_DMG_PREC = PERC_BASE + ALL_ELEMENT_DMG,
        /// <summary> 빛 속성 대미지(스텟) </summary> 
        LIGHT_DMG_PREC = PERC_BASE + LIGHT_DMG,
        /// <summary> 어둠 속성 대미지(스텟) </summary> 
        DARK_DMG_PREC = PERC_BASE + DARK_DMG,
        /// <summary> 물 속성 대미지(스텟) </summary> 
        WATER_DMG_PREC = PERC_BASE + WATER_DMG,
        /// <summary> 불 속성 대미지(스텟) </summary> 
        FIRE_DMG_PREC = PERC_BASE + FIRE_DMG,
        /// <summary> 바람 속성 대미지(스텟) </summary> 
        WIND_DMG_PREC = PERC_BASE + WIND_DMG,
        /// <summary> 땅 속성 대미지(스텟) </summary> 
        EARTH_DMG_PREC = PERC_BASE + EARTH_DMG,
        /// <summary> 크리 확률 저항 </summary> 
        CRI_RESIS_PREC = PERC_BASE + CRI_RESIS,
        /// <summary> 크리 대미지 저항 </summary> 
        CRI_DMG_RESIS_PREC = PERC_BASE + CRI_DMG_RESIS,
        /// <summary> 스킬 대미지 저항 </summary> 
        SKILL_DMG_RESIS_PREC = PERC_BASE + SKILL_DMG_RESIS,
        /// <summary> 이로운 효과 증가 </summary> 
        ADD_BUFF_TIME_PREC = PERC_BASE + ADD_BUFF_TIME,
        /// <summary> 해로운 효과 감소 </summary> 
        DEL_BUFF_TIME_PREC = PERC_BASE + DEL_BUFF_TIME,
        /// <summary> 기본 대미지 저항 </summary> 
        ATK_DMG_RESIS_PREC = PERC_BASE + ATK_DMG_RESIS,
        /// <summary> 모든 속성 저항(직접 사용 X) </summary> 
        ALL_ELEMENT_DMG_RESIS_PREC = PERC_BASE + ALL_ELEMENT_DMG_RESIS,
        /// <summary> 빛 속성 저항 </summary>  
        LIGHT_DMG_RESIS_PREC = PERC_BASE + LIGHT_DMG_RESIS,
        /// <summary> 어둠 속성 저항 </summary> 
        DARK_DMG_RESIS_PREC = PERC_BASE + DARK_DMG_RESIS,
        /// <summary> 물 속성 저항 </summary> 
        WATER_DMG_RESIS_PREC = PERC_BASE + WATER_DMG_RESIS,
        /// <summary> 불 속성 저항 </summary> 
        FIRE_DMG_RESIS_PREC = PERC_BASE + FIRE_DMG_RESIS,
        /// <summary> 바람 속성 저항 </summary> 
        WIND_DMG_RESIS_PREC = PERC_BASE + WIND_DMG_RESIS,
        /// <summary> 땅 속성 저항 </summary> 
        EARTH_DMG_RESIS_PREC = PERC_BASE + EARTH_DMG_RESIS,
        /// <summary> 추가 PvP 대미지 </summary> 
        ADD_PVP_DMG_PREC = PERC_BASE + ADD_PVP_DMG,
        /// <summary> PvP 대미지 </summary> 
        RATIO_PVP_DMG_PREC = PERC_BASE + RATIO_PVP_DMG,
        /// <summary> 추가 PvP 크리 대미지 </summary> 
        ADD_PVP_CRI_DMG_PREC = PERC_BASE + ADD_PVP_CRI_DMG,
        /// <summary> PvP 크리 대미지 </summary> 
        RATIO_PVP_CRI_DMG_PREC = PERC_BASE + RATIO_PVP_CRI_DMG,
        /// <summary> 쿨타임 감소 </summary> 
        DEL_COOLTIME_PREC = PERC_BASE + DEL_COOLTIME,
        /// <summary> 공격 속도 증가 </summary> 
        ADD_ATKSPEED_PREC = PERC_BASE + ADD_ATKSPEED,
        /// <summary> 보호막 </summary>
        SHIELD_POINT_PREC = PERC_BASE + SHIELD_POINT,
        /// <summary> 보스 대미지 </summary>
        BOSS_DMG_PERC = PERC_BASE + BOSS_DMG,
        /// <summary> 보스 대미지 저항 </summary>
        BOSS_DMG_RESIS_PREC = PERC_BASE + BOSS_DMG_RESIS,
        /// <summary> 캐릭터 속성 대미지 </summary>
        ALL_DMG_RESIS_PERC = PERC_BASE + ALL_DMG_RESIS,
        /// <summary> 쿨타임 감소하는 속도 (ATK_SPEED[공격속도]와 유사함) </summary>
        COOLTIME_SPEED_PREC = PERC_BASE + COOLTIME_SPEED,
        /// <summary> 실드 타입 대미지 </summary>
        SHILD_BREAKER_PREC = PERC_BASE + SHIELD_BREAKER,
        /// <summary> 대미지 생명력 회복 </summary>
        VAMP_PREC = PERC_BASE + VAMP,
        /// <summary> 물리 대미지 저항(기본 대미지, 크리 대미지 둘 다 올라감) </summary>
        PHYS_DMG_RESIS_PREC = PERC_BASE + PHYS_DMG_RESIS,

        //2025-11-17 추가 옵션들
        PHYS_DMG_PIERCE_PREC = PERC_BASE + PHYS_DMG_PIERCE, // 물리 데미지 관통
        CRI_DMG_PIERCE_PREC = PERC_BASE + CRI_DMG_PIERCE, // 치명 대미지 관통
        ALL_ELEMENT_DMG_PIERCE_PREC = PERC_BASE + ALL_ELEMENT_DMG_PIERCE, // 모든 속성 관통
        LIGHT_DMG_PIERCE_PREC = PERC_BASE + LIGHT_DMG_PIERCE, // 빛속성대미지 관통
        DARK_DMG_PIERCE_PREC = PERC_BASE + DARK_DMG_PIERCE, // 어둠속성대미지 관통
        WATER_DMG_PIERCE_PREC = PERC_BASE + WATER_DMG_PIERCE, // 물속성대미지 관통
        FIRE_DMG_PIERCE_PREC = PERC_BASE + FIRE_DMG_PIERCE, // 불속성대미지 관통
        WIND_DMG_PIERCE_PREC = PERC_BASE + WIND_DMG_PIERCE, // 바람속성대미지 관통
        EARTH_DMG_PIERCE_PREC = PERC_BASE + EARTH_DMG_PIERCE, // 땅속성대미지 관통
        DEL_START_COOLTIME_PREC = PERC_BASE + DEL_START_COOLTIME, // 스킬 시작쿨 감소
        RATIO_PASSIVE_EFFECT_PREC = PERC_BASE + RATIO_PASSIVE_EFFECT, // 패시브 효과 증폭
        RATIO_PASSIVE_RATE_PREC = PERC_BASE + RATIO_PASSIVE_RATE, // 패시브 확률 증폭
        DEL_KNOCKBACK_DISTANCE_PREC = PERC_BASE + DEL_KNOCKBACK_DISTANCE, // 넉백 거리 감소
        RATIO_BOSS_DMG_PREC = PERC_BASE + RATIO_BOSS_DMG, // 보스 대미지 증폭
        ADD_MAIN_ELEMENT_DMG_PREC = PERC_BASE + ADD_MAIN_ELEMENT_DMG, // 자기 속댐 증가
        //2026-01-15 추가
        ACCURACY_PREC = PERC_BASE + ACCURACY, // 명중률
        EVASION_PREC = PERC_BASE + EVASION, // 회피율
        /// <summary> 서버 퍼센트 수치 MAX </summary>
        PREC_MAX,
    }
    /// <summary>
    /// 스텟의 계산 방식
    /// </summary>
    public enum eStatusValueType
    {
        NONE = -1,
        /// <summary>
        /// 기본 방식 base * ratio + add
        /// </summary>
        VALUE = 0,
        /// <summary>
        /// base + ratio + add
        /// </summary>
        PERCENT,
        /// <summary>
        /// Mathf.FloorToInt(base + ratio + add)
        /// </summary>
        ADD_VALUE
    }
    public enum eJobType
    {
        NONE = 0,

        START,

        TANKER = START,
        WARRIOR,

        ASSASSIN,
        BOMBER,

        SNIPER,
        SUPPORTER,

        MAX
    }
    public abstract class UnitStatus
    {
        protected Dictionary<eStatusType, Dictionary<eStatusCategory, float>> Status { get; set; } = null;
        protected Dictionary<eStatusType, bool> StatusFlag { get; set; } = null;
        protected Dictionary<eStatusType, eStatusValueType> StatusDataType { get; set; } = null;
        protected Dictionary<eStatusType, Dictionary<EffectInfo, float>> DecreaseTypeStatus { get; set; } = null;

        public virtual bool Initialze()//하위에서 재구현해서 사용 스텟 결정하고 클리어하기.
        {
            if (Status == null)
                Status = new();
            else
                Status.Clear();

            if (StatusFlag == null)
                StatusFlag = new();
            else
                StatusFlag.Clear();

            if (StatusDataType == null)
                StatusDataType = new();
            else
                StatusDataType.Clear();

            if (DecreaseTypeStatus == null)
                DecreaseTypeStatus = new();
            else
                DecreaseTypeStatus.Clear();

            return true;
        }

        protected void Clear()
        {
            foreach (var type in StatusDataType)
            {
                ClearStatus(type.Key);
            }
        }

        /// <summary>
        /// 해당 모든 능력치 0으로 초기화
        /// </summary>
        /// <param name="type_">Type</param>
        public void ClearStatus(eStatusType type_)
        {
            StatusFlag[type_] = true;

            for (eStatusCategory category = eStatusCategory.START; category < eStatusCategory.MAX; ++category)
            {
                if (!Status.ContainsKey(type_))
                    Status.Add(type_, new());

                Status[type_][category] = 0;
            }
        }
        public void ClearCategory(eStatusCategory category_)
        {
            for (eStatusType type_ = eStatusType.START; type_ < eStatusType.MAX; ++type_)
            {
                if (!Status.ContainsKey(type_))
                    continue;

                if (Status[type_].ContainsKey(category_))
                {
                    if (Status[type_][category_] != 0)
                        StatusFlag[type_] = true;
                }
                Status[type_][category_] = 0;
            }
        }

        public bool IsFlag(eStatusType type_)
        {
            return IsStatus(type_);
        }

        private bool IsStatus(eStatusCategory category_, eStatusType type_)
        {
            if (category_ < eStatusCategory.START || eStatusCategory.MAX <= category_)
                return false;

            return IsStatus(type_);
        }

        private bool IsStatus(eStatusType type_)
        {
            return Status.ContainsKey(type_);
        }

        /// <summary>
        /// 선택한 캐릭터의 스텟으로 셋팅을 해준다.(테이블 구조에 따라서는 공통으로 안할수도 있을거 같다)
        /// </summary>
        public bool SetStatus(bool calc_ = false)
        {
            Clear();

            if (calc_)
                CalcStatusAll();

            return true;
        }

        /// <summary>
        /// 스텟 증가
        /// </summary>
        /// <param name="_category"></param>
        /// <param name="type"></param>
        /// <param name="_value"></param>
        public virtual void IncreaseStatus(eStatusCategory category_, eStatusType type_, float value_, bool calc_ = false)
        {
            if (IsStatus(category_, type_) == false)
                return;

            Status[type_][category_] += value_;
            SetStatusFlag(type_, true);

            if (calc_)
                CalcStatus(type_);
        }

        /// <summary>
        /// 스텟 감소
        /// </summary>
        /// <param name="_category"></param>
        /// <param name="type"></param>
        /// <param name="_value"></param>
        public virtual void DecreaseStatus(eStatusCategory category_, eStatusType type_, float value_, bool calc_ = false)
        {
            if (IsStatus(category_, type_) == false)
                return;

            Status[type_][category_] -= value_;
            SetStatusFlag(type_, true);

            if (calc_)
                CalcStatus(type_);
        }

        /// <summary>
        /// 계산을 할지 말지 flag값 셋팅
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        private void SetStatusFlag(eStatusType type_, bool value_)
        {
            if (IsStatus(type_) == false)
                return;

            StatusFlag[type_] = value_;
        }

        /// <summary>
        /// 전체 스텟 계산 함수 - 재계산할 필요 없는거는 무시
        /// </summary>
        public virtual void CalcStatusAll()
        {
            foreach (var type in StatusDataType)
            {
                if (StatusFlag[type.Key] == false)
                    continue;

                //계산
                CalcStatus(type.Key);
            }
        }

        /// <summary>
        /// 특정 스텟 계산
        /// </summary>
        /// <param name="type"></param>
        protected virtual void CalcStatus(eStatusType type_)
        {
            var value = Status[type_][eStatusCategory.BASE];
            var added = Status[type_][eStatusCategory.ADD] + Status[type_][eStatusCategory.ADD_BUFF];
            float ratio = 1.0f + (Status[type_][eStatusCategory.RATIO] + Status[type_][eStatusCategory.RATIO_BUFF]) * SBDefine.CONVERT_FLOAT;
            switch (type_) //토탈 계산방식이 다 다름...
            {
                case eStatusType.ALL_ELEMENT_DMG:
                case eStatusType.ALL_ELEMENT_DMG_RESIS:
                case eStatusType.PHYS_DMG_RESIS:
                case eStatusType.ALL_ELEMENT_DMG_PIERCE:
                    SetStatusFlag(type_, false);
                    return;
                case eStatusType.BOSS_DMG:
                {
                    Status[type_][eStatusCategory.TOTAL] = (value + added) * ratio;
                }
                break;
                case eStatusType.FIRE_DMG:
                case eStatusType.WATER_DMG:
                case eStatusType.EARTH_DMG:
                case eStatusType.WIND_DMG:
                case eStatusType.DARK_DMG:
                case eStatusType.LIGHT_DMG:
                {
                    var curTypeValue = value + added;
                    var allTypeValue = Status[eStatusType.ALL_ELEMENT_DMG][eStatusCategory.BASE] + Status[eStatusType.ALL_ELEMENT_DMG][eStatusCategory.ADD] + Status[eStatusType.ALL_ELEMENT_DMG][eStatusCategory.ADD_BUFF];
                    var allTypeRatio = (Status[eStatusType.ALL_ELEMENT_DMG][eStatusCategory.RATIO] * Status[eStatusType.ALL_ELEMENT_DMG][eStatusCategory.RATIO_BUFF]) * SBDefine.CONVERT_FLOAT;
                    Status[type_][eStatusCategory.TOTAL] = Mathf.FloorToInt((curTypeValue + allTypeValue) * (ratio + allTypeRatio));
                }
                break;
                case eStatusType.ATK_DMG_RESIS:
                case eStatusType.CRI_DMG_RESIS:
                {
                    var curTypeValue = value + added + Status[type_][eStatusCategory.RATIO] + Status[type_][eStatusCategory.RATIO_BUFF];
                    //물리 대미지 저항 추가
                    var allTypeValue = Status[eStatusType.PHYS_DMG_RESIS][eStatusCategory.BASE]
                        + Status[eStatusType.PHYS_DMG_RESIS][eStatusCategory.ADD]
                        + Status[eStatusType.PHYS_DMG_RESIS][eStatusCategory.RATIO]
                        + Status[eStatusType.PHYS_DMG_RESIS][eStatusCategory.ADD_BUFF]
                        + Status[eStatusType.PHYS_DMG_RESIS][eStatusCategory.RATIO_BUFF];
                    //
                    var res = curTypeValue + allTypeValue;
                    //저항은 100% 넘으면 안됨...
                    Status[type_][eStatusCategory.TOTAL] = res >= 100 ? 100f : res;
                }
                break;
                case eStatusType.SKILL_DMG_RESIS://지금은 미사용
                {
                    var res = value + added + Status[type_][eStatusCategory.RATIO] + Status[type_][eStatusCategory.RATIO_BUFF];
                    //저항은 100% 넘으면 안됨...
                    Status[type_][eStatusCategory.TOTAL] = res >= 100 ? 100f : res;
                }
                break;
                case eStatusType.FIRE_DMG_RESIS:
                case eStatusType.WATER_DMG_RESIS:
                case eStatusType.EARTH_DMG_RESIS:
                case eStatusType.WIND_DMG_RESIS:
                case eStatusType.DARK_DMG_RESIS:
                case eStatusType.LIGHT_DMG_RESIS:
                {
                    var curTypeValue = value + added + Status[type_][eStatusCategory.RATIO] + Status[type_][eStatusCategory.RATIO_BUFF];
                    var allTypeValue = Status[eStatusType.ALL_ELEMENT_DMG_RESIS][eStatusCategory.BASE]
                        + Status[eStatusType.ALL_ELEMENT_DMG_RESIS][eStatusCategory.ADD]
                        + Status[eStatusType.ALL_ELEMENT_DMG_RESIS][eStatusCategory.RATIO]
                        + Status[eStatusType.ALL_ELEMENT_DMG_RESIS][eStatusCategory.ADD_BUFF]
                        + Status[eStatusType.ALL_ELEMENT_DMG_RESIS][eStatusCategory.RATIO_BUFF];
                    var res = curTypeValue + allTypeValue;
                    //저항은 100% 넘으면 안됨...
                    Status[type_][eStatusCategory.TOTAL] = res >= 100 ? 100f : res;
                }
                break;
                case eStatusType.FIRE_DMG_PIERCE:
                case eStatusType.WATER_DMG_PIERCE:
                case eStatusType.EARTH_DMG_PIERCE:
                case eStatusType.WIND_DMG_PIERCE:
                case eStatusType.DARK_DMG_PIERCE:
                case eStatusType.LIGHT_DMG_PIERCE:
                {
                    var curTypeValue = value + added + Status[type_][eStatusCategory.RATIO] + Status[type_][eStatusCategory.RATIO_BUFF];
                    var allTypeValue = Status[eStatusType.ALL_ELEMENT_DMG_PIERCE][eStatusCategory.BASE]
                        + Status[eStatusType.ALL_ELEMENT_DMG_PIERCE][eStatusCategory.ADD]
                        + Status[eStatusType.ALL_ELEMENT_DMG_PIERCE][eStatusCategory.RATIO]
                        + Status[eStatusType.ALL_ELEMENT_DMG_PIERCE][eStatusCategory.ADD_BUFF]
                        + Status[eStatusType.ALL_ELEMENT_DMG_PIERCE][eStatusCategory.RATIO_BUFF];
                    var res = curTypeValue + allTypeValue;
                    //관통은 100% 넘으면 안됨...
                    Status[type_][eStatusCategory.TOTAL] = res >= 100 ? 100f : res;
                }
                break;
                case eStatusType.CRI_PROC:
                {
                    var baseValue = value + Status[type_][eStatusCategory.ADD] + Status[type_][eStatusCategory.RATIO];
                    Status[type_][eStatusCategory.TOTAL] = (baseValue + Status[type_][eStatusCategory.ADD_BUFF]) * (1f + Status[type_][eStatusCategory.RATIO_BUFF] * SBDefine.CONVERT_FLOAT);
                }
                break;
                default:
                {
                    Status[type_][eStatusCategory.TOTAL] = StatusDataType[type_] switch
                    {
                        eStatusValueType.PERCENT => value + added + Status[type_][eStatusCategory.RATIO] + Status[type_][eStatusCategory.RATIO_BUFF],
                        eStatusValueType.ADD_VALUE => Mathf.FloorToInt(value + added + Status[type_][eStatusCategory.RATIO] + Status[type_][eStatusCategory.RATIO_BUFF]),
                        _ => Mathf.FloorToInt((value * ratio) + added),
                    };
                }
                break;
            }

            Status[type_][eStatusCategory.TOTAL] = StatTypeData.GetStatValue(type_, Status[type_][eStatusCategory.TOTAL]);
            SetStatusFlag(type_, false);
        }

        /// <summary> 특정 카테고리 스텟값 가지고 오기 </summary>
        public float GetStatus(eStatusCategory category_, eStatusType type_)
        {
            if (IsStatus(category_, type_) == false)
                return 0f;

            float stat = Status[type_][category_];
            if (category_ == eStatusCategory.TOTAL && DecreaseTypeStatus.TryGetValue(type_, out var dic))
            {
                var calcType = eStatusValueType.VALUE;
                if (StatusDataType.ContainsKey(type_) && StatusDataType[type_] == eStatusValueType.PERCENT)
                    calcType = eStatusValueType.PERCENT;

                foreach (var decrease in dic)
                {
                    float value = decrease.Value;
                    if (calcType != eStatusValueType.PERCENT && decrease.Key.VALUE_TYPE == eStatusValueType.PERCENT)
                    {
                        var item = 1f - value * SBDefine.CONVERT_FLOAT;
                        if (item <= 0f)
                            item = 0f;

                        stat *= item;
                    }
                    else
                    {
                        stat -= value;
                    }
                }
            }
            return stat;
        }
        public int GetStatusInt(eStatusCategory category_, eStatusType type_)
        {
            return Mathf.RoundToInt(GetStatus(category_, type_));
        }
        /// <summary> 외부에서 스텟 최종값 가지고 오기(스텟 컨버트 하기 전) </summary>
        public float GetTotalStatus(eStatusType type_)
        {
            return GetStatus(eStatusCategory.TOTAL, type_);
        }
        public float GetTotalStatusConvert(eStatusType type_)
        {
            if (!StatusDataType.ContainsKey(type_))
                return 0f;

            return StatusDataType[type_] switch
            {
                eStatusValueType.PERCENT => GetTotalStatus(type_) * SBDefine.CONVERT_FLOAT,
                _ => GetTotalStatus(type_)
            };
        }

        public int GetTotalStatusInt(eStatusType type_)
        {
            return GetStatusInt(eStatusCategory.TOTAL, type_);
        }
        public eStatusValueType GetType(eStatusType type_)
        {
            if (StatusDataType.TryGetValue(type_, out var value))
                return value;

            return eStatusValueType.VALUE;
        }
        public float GetSkillCoolSpeed(float time)
        {
            return time * GetSkillCoolSpeed();
        }
        private float GetSkillCoolSpeed()
        {
            var speed = GetTotalStatusConvert(eStatusType.COOLTIME_SPEED);
            if (float.IsInfinity(speed))
                return 1f;

            if (speed <= -1f)
                return 0.01f;

            return 1f + speed;
        }
        public float GetSkillCoolDown(float time)
        {
            return time - time * GetTotalStatusConvert(eStatusType.DEL_COOLTIME);
        }
        public float GetBuffTime(float time)
        {
            return time + time * GetTotalStatusConvert(eStatusType.ADD_BUFF_TIME);
        }
        public float GetDebuffTime(float time)
        {
            return time - time * GetTotalStatusConvert(eStatusType.DEL_BUFF_TIME);
        }
        public float GetAttackSpeed()
        {
            var speed = GetTotalStatusConvert(eStatusType.ADD_ATKSPEED);
            if (float.IsInfinity(speed))
                return 1f;

            if (speed <= -1f)
                return 0.01f;

            return 1f + speed;
        }
        public virtual void AddDecreaseInfo(EffectInfo info)
        {
            if (DecreaseTypeStatus == null)
                return;

            if (false == DecreaseTypeStatus.ContainsKey(info.STAT_TYPE))
                DecreaseTypeStatus.Add(info.STAT_TYPE, new());

            if (false == DecreaseTypeStatus[info.STAT_TYPE].TryAdd(info, info.GetValue()))
                return;
        }
        public virtual void DelDecreaseInfo(EffectInfo info)
        {
            if (DecreaseTypeStatus == null)
                return;

            if (false == DecreaseTypeStatus.ContainsKey(info.STAT_TYPE))
                DecreaseTypeStatus.Add(info.STAT_TYPE, new());

            DecreaseTypeStatus[info.STAT_TYPE].Remove(info);
        }
        public int GetDecreaseCount(EffectInfo info)
        {
            if (DecreaseTypeStatus == null)
                return 0;

            if (false == DecreaseTypeStatus.ContainsKey(info.STAT_TYPE))
                return 0;

            return DecreaseTypeStatus[info.STAT_TYPE].Count;
        }
    }
}