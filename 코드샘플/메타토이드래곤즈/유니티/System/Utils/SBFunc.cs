
using Com.LuisPedroFonseca.ProCamera2D;
using Newtonsoft.Json.Linq;
using SRF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SandboxNetwork
{
    public static class SBFunc
    {
        public static void Log(params object[] arr)
        {
#if DEBUG
            Debug.Log(GetStrBuilder(arr));
#endif
        }
        public static eAuthAccount GetApplicationPlatform()
        {
            eAuthAccount result = eAuthAccount.NONE;

            if (Application.platform == RuntimePlatform.Android) result = eAuthAccount.GOOGLE;
            if (Application.platform == RuntimePlatform.IPhonePlayer) result = eAuthAccount.APPLE;

            return result;
        }

        private static readonly StringBuilder builder = new();
        public static StringBuilder GetBuilder()
        {
            builder.Clear();
            return builder;
        }
        private static StringBuilder GetStrBuilder(params object[] arr)
        {
            var builder = GetBuilder();

            var arrCount = arr.Length;
            for (var i = 0; i < arrCount; ++i)
            {
                builder.Append(arr[i]);
            }

            return builder;
        }
        public static string StrBuilder(params object[] arr)
        {
            var builder = GetStrBuilder(arr);
            if (builder == null)
                return "";

            return builder.ToString();
        }
        public static string ObjectsConvertLimitString(int checkLength, params object[] arr)
        {
            var builder = GetStrBuilder(arr);
            if (builder == null)
                return "";

            if (builder.Length > checkLength)
            {
                builder.Remove(checkLength, builder.Length - checkLength);
                builder.Append("...");
            }

            return builder.ToString();
        }
        public static string TimeShortString(int sec)
        {
            var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            if (day > 0)
            {
                return StrBuilder(day.ToString(), StringData.GetStringByStrKey("time_day"));
            }
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            if (hour > 0)
            {
                return StrBuilder(hour.ToString(), StringData.GetStringByStrKey("time_hr"));
            }
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            if (min > 0)
            {
                return StrBuilder(min.ToString(), StringData.GetStringByStrKey("time_min"));
            }

            if (sec > 0)
            {
                return StrBuilder(sec.ToString(), StringData.GetStringByStrKey("time_sec"));
            }

            return "";
        }
        public static string TimeShortString(float sec)
        {
            return TimeShortString(Mathf.FloorToInt(sec));
        }
        // 해당 시간에서 가장 큰 시간단위순으로 count 갯수만큼의 단위까지만 표시
        // (ex1: count == 2 && sec > day 일 경우 day/hour까지 표시, ex2: count == 3 && sec > hour일 경우 hour/minute/sec까지 표시)
        public static string TimeCustomString(int sec, int count, bool isTimeSpan = false)
        {
            if (count <= 0) return "";

            string resultString = "";

            var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            if (day > 0 && count-- > 0)
            {
                sec -= day * SBDefine.Day;
                resultString = StrBuilder(resultString, day.ToString(), StringData.GetStringByStrKey("time_day"));
            }
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            if (hour > 0 && count-- > 0)
            {
                sec -= hour * SBDefine.Hour;
                resultString = StrBuilder(resultString, hour.ToString(), isTimeSpan ? StringData.GetStringByStrKey("timespan_hr") : StringData.GetStringByStrKey("time_hr"));
            }
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            if (min > 0 && count-- > 0)
            {
                sec -= min * SBDefine.Minute;
                resultString = StrBuilder(resultString, min.ToString(), StringData.GetStringByStrKey("time_min"));
            }

            if (sec > 0 && count-- > 0)
            {
                resultString = StrBuilder(resultString, sec.ToString(), StringData.GetStringByStrKey("time_sec"));
            }

            return resultString;
        }
        public static string TimeCustomString(int sec, bool isShowDay, bool isShowHour, bool isShowMin, bool isShowSec, bool isTimeSpan = false)
        {
            string resultString = "";

            var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            if (day > 0 && isShowDay)
            {
                sec -= day * SBDefine.Day;
                resultString = StrBuilder(resultString, day.ToString(), StringData.GetStringByStrKey("time_day"));
            }
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            if (hour > 0 && isShowHour)
            {
                sec -= hour * SBDefine.Hour;
                resultString = StrBuilder(string.IsNullOrEmpty(resultString) ? "" : resultString + " ", hour.ToString(), isTimeSpan ? StringData.GetStringByStrKey("timespan_hr") : StringData.GetStringByStrKey("time_hr"));
            }
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            if (min > 0 && isShowMin)
            {
                sec -= min * SBDefine.Minute;
                resultString = StrBuilder(string.IsNullOrEmpty(resultString) ? "" : resultString + " ", min.ToString(), StringData.GetStringByStrKey("time_min"));
            }

            if (sec > 0 && isShowSec)
            {
                resultString = StrBuilder(string.IsNullOrEmpty(resultString) ? "" : resultString + " ", sec.ToString(), StringData.GetStringByStrKey("time_sec"));
            }

            return resultString;
        }
        public static string TimeCustomString(float sec)
        {
            return TimeCustomString(Mathf.FloorToInt(sec));
        }
        public static string TimeString(int sec, string strKey = "")
        {
            var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            sec -= day * SBDefine.Day;
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            sec -= hour * SBDefine.Hour;
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            sec -= min * SBDefine.Minute;

            if (sec < 0)
                sec = 0;
            if (strKey == "")
            {
                var isEmtpy = hour == 0 && min == 0 && sec == 0;
                var timeString = StrBuilder(hour.ToString("D2"), ":", min.ToString("D2"), ":", sec.ToString("D2"));
                string result = isEmtpy ? "" : " " + timeString;
                if (day > 0)
                    return StrBuilder(day.ToString(), StringData.GetStringByIndex(100000060), result);

                return StrBuilder(hour.ToString("D2"), ":", min.ToString("D2"), ":", sec.ToString("D2"));
            }
            else
            {
                return StringData.GetStringFormatByStrKey(strKey, day, hour.ToString("D2"), min.ToString("D2"), sec.ToString("D2"));

            }
        }
        public static string TimeStringInt64(long sec, string strKey = "")
        {
            var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            sec -= day * SBDefine.Day;
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            sec -= hour * SBDefine.Hour;
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            sec -= min * SBDefine.Minute;

            if (sec < 0)
                sec = 0;
            if (strKey == "")
            {
                if (day > 0)
                    return StrBuilder(day.ToString(), StringData.GetStringByIndex(100000060), " ", hour.ToString("D2"), ":", min.ToString("D2"), ":", sec.ToString("D2"));

                return StrBuilder(hour.ToString("D2"), ":", min.ToString("D2"), ":", sec.ToString("D2"));
            }
            else
            {
                return StringData.GetStringFormatByStrKey(strKey, day, hour.ToString("D2"), min.ToString("D2"), sec.ToString("D2"));

            }
        }
        public static string TimeString(float sec)
        {
            return TimeString(Mathf.FloorToInt(sec));
        }
        public static string TimeStringMinute(int sec)
        {
            var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            sec -= day * SBDefine.Day;
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            sec -= hour * SBDefine.Hour;
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            sec -= min * SBDefine.Minute;

            if (sec < 0)
                sec = 0;

            if (day > 0)
                return StrBuilder(day.ToString(), StringData.GetStringByIndex(100000060), " ", hour.ToString("D2"), ":", min.ToString("D2"), ":", sec.ToString("D2"));

            if (hour > 0)
                return StrBuilder(hour.ToString("D2"), ":", min.ToString("D2"), ":", sec.ToString("D2"));

            return StrBuilder(min.ToString("D2"), ":", sec.ToString("D2"));
        }
        public static string TimeStringMinute(float sec)
        {
            return TimeStringMinute(Mathf.FloorToInt(sec));
        }

        public static string TimeStringHourMinute(int sec)
        {
            //var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            //sec -= day * SBDefine.Day;
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            sec -= hour * SBDefine.Hour;
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            sec -= min * SBDefine.Minute;

            if (sec < 0)
                sec = 0;

            if (hour > 0)
                return StrBuilder(hour.ToString("D2"), ":", min.ToString("D2"));

            return StrBuilder(min.ToString("D2"));
        }
        public static string TimeStringHourMinute(float sec)
        {
            return TimeStringHourMinute(Mathf.FloorToInt(sec));
        }

        public static DateTime TimeStampToDateTime(int _timeStamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(_timeStamp).ToLocalTime();
            return dt;
        }
        public static DateTime TimeStampToDateTime(long _timeStamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(_timeStamp).ToLocalTime();
            return dt;
        }
        public static long GetNowDateTimeToTimeStamp()
        {
            return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        }
        public static string TimeStampDeepString(long _timeStamp)
        {
            var sec = TimeManager.GetTime() - _timeStamp;

            var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            sec -= day * SBDefine.Day;
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            sec -= hour * SBDefine.Hour;
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            sec -= min * SBDefine.Minute;

            if (day > SBDefine.USER_ACCESS_DAY_MAX)
                return StringData.GetStringFormatByIndex(100000764, SBDefine.USER_ACCESS_DAY_MAX);
            else if (day > 0)
                return StringData.GetStringFormatByIndex(100000764, day);
            else if (hour > 0)
                return StringData.GetStringFormatByIndex(100000763, hour);
            else if (min > 0)
                return StringData.GetStringFormatByIndex(100000762, min);
            else
                return StringData.GetStringFormatByIndex(100000761, sec);
        }
        public static string TimeStampDeepRemainString(long _timeStamp)
        {
            var sec = _timeStamp - TimeManager.GetTime();

            var day = sec >= SBDefine.Day ? Mathf.FloorToInt(sec / SBDefine.Day) : 0;
            sec -= day * SBDefine.Day;
            var hour = sec >= SBDefine.Hour ? Mathf.FloorToInt(sec / SBDefine.Hour) : 0;
            sec -= hour * SBDefine.Hour;
            var min = sec >= SBDefine.Minute ? Mathf.FloorToInt(sec / SBDefine.Minute) : 0;
            sec -= min * SBDefine.Minute;

            if (day > 0)
                return StringData.GetStringFormatByStrKey("guild_desc:118", day);
            else if (hour > 0)
                return StringData.GetStringFormatByIndex(100002263, hour);
            else if (min > 0)
                return StringData.GetStringFormatByStrKey("guild_desc:119", min);
            else
                return StringData.GetStringFormatByStrKey("guild_desc:120", 1);
        }
        public static long GetDateTimeToTimeStamp()
        {
            var now = DateTime.Now.ToLocalTime();
            var span = (now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            int timestamp = (int)span.TotalSeconds;

            return timestamp;
        }
        public static int GetItemNoByGoodType(eGoodType type, int itemNo)
        {
            switch (type)
            {
                case eGoodType.GOLD:
                    return SBDefine.ITEM_GOLD;
                case eGoodType.ENERGY:
                    return SBDefine.ITEM_ENERGY;
                case eGoodType.ARENA_TICKET:
                    return SBDefine.ITEM_TICKET_PVP;
                case eGoodType.GEMSTONE:
                case eGoodType.CASH:
                    return SBDefine.ITEM_GEMSTONE;
                case eGoodType.ACCOUNT_EXP:
                    return SBDefine.ITEM_ACC_EXP;
                case eGoodType.MAGNET:
                    return SBDefine.ITEM_MAGNET;
                default:
                    break;
            }

            return itemNo;
        }
        /// <summary>
        /// 요구 수량만큼 필요한 아이템 만들기
        /// </summary>
        /// <param name="_count"></param>
        /// <returns></returns>
        public static List<Asset> GetNeedItemList(List<Asset> _targetList, int _count = 1, bool _exceptInventory = false)
        {
            List<Asset> needItemDataList = new List<Asset>();

            if (_targetList == null || _targetList.Count <= 0)
                return needItemDataList;

            foreach (var needItem in _targetList)
            {
                if (needItem == null)
                    continue;

                var itemNo = needItem.ItemNo;
                var designData = ItemBaseData.Get(itemNo);
                if (designData == null)
                    continue;

                if (designData.USE == false || designData.BUY == 0)
                    continue;

                var needOnceAmount = needItem.Amount;
                if (!_exceptInventory && needOnceAmount * _count <= User.Instance.GetItemCount(itemNo))//인벤토리에 많으면 continue
                    continue;

                needItemDataList.Add(new Asset(itemNo, needOnceAmount * _count));
            }

            return needItemDataList;
        }
        #region Random
        public static void SetRandomSeed(int seed)
        {
            UnityEngine.Random.InitState(seed);
        }
        public static void ReleaseRandomSeed()
        {
            UnityEngine.Random.InitState(DateTime.Now.Second * DateTime.Now.Millisecond);
        }

        public static float RandomValue => UnityEngine.Random.value;

        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        #endregion
        public static bool IsTarget(this eSkillTargetType targetType, IBattleCharacterData caster, IBattleCharacterData target)
        {
            switch (targetType)
            {
                case eSkillTargetType.ALL:
                    return true;
                case eSkillTargetType.ALLY:
                    return caster != target && target.IsEnemy == caster.IsEnemy;
                case eSkillTargetType.FRIENDLY:
                    return target.IsEnemy == caster.IsEnemy;
                case eSkillTargetType.SELF:
                    return caster == target;
                case eSkillTargetType.ENEMY:
                default:
                    return target.IsEnemy != caster.IsEnemy;
            }
        }
        public static int CalcRatioInt(float charATK, float ratio = 100f)
        {
            return Mathf.FloorToInt(CalcRatio(charATK, ratio));
        }
        public static float CalcRatio(float charATK, float ratio = 100f)
        {
            return charATK * ratio * SBDefine.CONVERT_FLOAT;
        }
        public static float Defense(float charDEF)
        {
            return charDEF / (SBDefine.DEF_WEIGHT + charDEF);
        }
        public static int Offense(float DMG, float DEF_RATE)
        {
            var offense = Mathf.FloorToInt(DMG * (1f - DEF_RATE));
            return offense < 1 ? 1 : offense;
        }
        private static float BaseStat(eStatusType type, int level, float baseStat, float factorStat)
        {
            return type switch
            {
                eStatusType.ATK => baseStat + ((level - 1) * factorStat),
                eStatusType.DEF => baseStat + ((level - 1) * factorStat),
                eStatusType.HP => baseStat + ((level - 1) * factorStat),
                eStatusType.CRI_PROC => baseStat + ((level - 1) * factorStat),
                eStatusType.CRI_DMG => baseStat + ((level - 1) * factorStat),
                eStatusType.LIGHT_DMG => baseStat + ((level - 1) * factorStat),
                eStatusType.DARK_DMG => baseStat + ((level - 1) * factorStat),
                eStatusType.FIRE_DMG => baseStat + ((level - 1) * factorStat),
                eStatusType.EARTH_DMG => baseStat + ((level - 1) * factorStat),
                eStatusType.WIND_DMG => baseStat + ((level - 1) * factorStat),
                eStatusType.WATER_DMG => baseStat + ((level - 1) * factorStat),
                _ => baseStat
            };
        }
        private static void SetBaseStatus(CharacterStatus status, eStatusType type, int level, float baseStat, float factorStat)
        {
            status.IncreaseStatus(eStatusCategory.BASE, type, BaseStat(type, level, baseStat, factorStat), true);
        }
        public static CharacterStatus BaseCharStatus(int level, CharBaseData BaseData, StatFactorData FactorData)
        {
            if (level < 1 || BaseData == null || FactorData == null)
                return null;

            var status = new CharacterStatus();
            status.Initialze();

            float HP_rate = 1.0f;
            float ATK_rate = 1.0f;
            float DEF_rate = 1.0f;
            float CRID_rate = 1.0f;
            float CRIP_rate = 1.0f;
            float DARK_rate = 1.0f;
            float FIRE_rate = 1.0f;
            float EARTH_rate = 1.0f;
            float WIND_rate = 1.0f;
            float WATER_rate = 1.0f;

            string jobKey = "";
            switch (BaseData.JOB)
            {
                case eJobType.TANKER:
                    jobKey = "tanker";
                    break;
                case eJobType.WARRIOR:
                    jobKey = "warrior";
                    break;
                case eJobType.ASSASSIN:
                    jobKey = "assasin";
                    break;
                case eJobType.SNIPER:
                    jobKey = "sniper";
                    break;
                case eJobType.BOMBER:
                    jobKey = "bomber";
                    break;
                case eJobType.SUPPORTER:
                    jobKey = "surporter";
                    break;

            }
            if (!string.IsNullOrEmpty(jobKey))
            {
                HP_rate = ServerOptionData.GetJsonValueFloat(jobKey, "hp", 1.0f);
                ATK_rate = ServerOptionData.GetJsonValueFloat(jobKey, "atk", 1.0f);
                DEF_rate = ServerOptionData.GetJsonValueFloat(jobKey, "def", 1.0f);
                CRID_rate = ServerOptionData.GetJsonValueFloat(jobKey, "cri_dmg", 1.0f);
                //CRIP_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "def", 1.0f);
                switch (BaseData.ELEMENT_TYPE)
                {
                    case eElementType.DARK:
                        DARK_rate = ServerOptionData.GetJsonValueFloat(jobKey, "elem_dmg", 1.0f);
                        break;
                    case eElementType.FIRE:
                        FIRE_rate = ServerOptionData.GetJsonValueFloat(jobKey, "elem_dmg", 1.0f);
                        break;
                    case eElementType.EARTH:
                        EARTH_rate = ServerOptionData.GetJsonValueFloat(jobKey, "elem_dmg", 1.0f);
                        break;
                    case eElementType.WIND:
                        WIND_rate = ServerOptionData.GetJsonValueFloat(jobKey, "elem_dmg", 1.0f);
                        break;
                    case eElementType.WATER:
                        WATER_rate = ServerOptionData.GetJsonValueFloat(jobKey, "elem_dmg", 1.0f);
                        break;
                }
            }

            //성장 스텟
            SetBaseStatus(status, eStatusType.HP, level, BaseData.HP, FactorData.HP * HP_rate);
            SetBaseStatus(status, eStatusType.ATK, level, BaseData.ATK, FactorData.ATK * ATK_rate);
            SetBaseStatus(status, eStatusType.DEF, level, BaseData.DEF, FactorData.DEF * DEF_rate);
            SetBaseStatus(status, eStatusType.CRI_PROC, level, BaseData.CRI_PROC, FactorData.CRI_PROC);
            SetBaseStatus(status, eStatusType.CRI_DMG, level, BaseData.CRI_DMG, FactorData.CRI_DMG * CRID_rate);
            SetBaseStatus(status, eStatusType.LIGHT_DMG, level, BaseData.LIGHT_DMG, FactorData.LIGHT_DMG);
            SetBaseStatus(status, eStatusType.DARK_DMG, level, BaseData.DARK_DMG, FactorData.DARK_DMG * DARK_rate);
            SetBaseStatus(status, eStatusType.FIRE_DMG, level, BaseData.FIRE_DMG, FactorData.FIRE_DMG * FIRE_rate);
            SetBaseStatus(status, eStatusType.EARTH_DMG, level, BaseData.EARTH_DMG, FactorData.EARTH_DMG * EARTH_rate);
            SetBaseStatus(status, eStatusType.WIND_DMG, level, BaseData.WIND_DMG, FactorData.WIND_DMG * WIND_rate);
            SetBaseStatus(status, eStatusType.WATER_DMG, level, BaseData.WATER_DMG, FactorData.WATER_DMG * WATER_rate);
            //비성장 스텟
            SetBaseStatus(status, eStatusType.RATIO_PVP_DMG, level, BaseData.RATIO_PVP_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_PVP_DMG, level, BaseData.ADD_PVP_DMG, 0);
            SetBaseStatus(status, eStatusType.RATIO_PVP_CRI_DMG, level, BaseData.RATIO_PVP_CRI_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_PVP_CRI_DMG, level, BaseData.ADD_PVP_CRI_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_ATKSPEED, level, BaseData.ADD_ATKSPEED, 0);

            return status;
        }

        public static CharacterStatus BaseMonsterStatus(int level, ICharacterBaseData BaseData, StatFactorData FactorData, eStageType type)
        {
            if (level < 1 || BaseData == null || FactorData == null)
                return null;

            var status = new CharacterStatus();
            status.Initialze();
            //성장 스텟
            float HP_rate = 1.0f;
            float ATK_rate = 1.0f;
            float DEF_rate = 1.0f;
            switch (type)
            {
                case eStageType.ADVENTURE:
                    if (!TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Adventure))//튜토리얼이 아닐때만
                    {                        
                        HP_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "hp", 1.0f);
                        ATK_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "atk", 1.0f);
                        DEF_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "def", 1.0f);
                    }
                    break;
                case eStageType.DAILY_DUNGEON:
                    HP_rate = ServerOptionData.GetJsonValueFloat("daily_balance", "hp", 1.0f);
                    ATK_rate = ServerOptionData.GetJsonValueFloat("daily_balance", "atk", 1.0f);
                    DEF_rate = ServerOptionData.GetJsonValueFloat("daily_balance", "def", 1.0f);
                    break;
                case eStageType.WORLD_BOSS:
                    HP_rate = ServerOptionData.GetJsonValueFloat("raid_balance", "hp", 1.0f);
                    ATK_rate = ServerOptionData.GetJsonValueFloat("raid_balance", "atk", 1.0f);
                    DEF_rate = ServerOptionData.GetJsonValueFloat("raid_balance", "def", 1.0f);
                    break;
                default:
                    break;
            }

            SetBaseStatus(status, eStatusType.HP, level, BaseData.HP * HP_rate, FactorData.HP * HP_rate);
            SetBaseStatus(status, eStatusType.ATK, level, BaseData.ATK * ATK_rate, FactorData.ATK * ATK_rate);
            SetBaseStatus(status, eStatusType.DEF, level, BaseData.DEF * DEF_rate, FactorData.DEF * DEF_rate);

            SetBaseStatus(status, eStatusType.CRI_PROC, level, BaseData.CRI_PROC, FactorData.CRI_PROC);
            SetBaseStatus(status, eStatusType.CRI_DMG, level, BaseData.CRI_DMG, FactorData.CRI_DMG);
            SetBaseStatus(status, eStatusType.LIGHT_DMG, level, BaseData.LIGHT_DMG, FactorData.LIGHT_DMG);
            SetBaseStatus(status, eStatusType.DARK_DMG, level, BaseData.DARK_DMG, FactorData.DARK_DMG);
            SetBaseStatus(status, eStatusType.FIRE_DMG, level, BaseData.FIRE_DMG, FactorData.FIRE_DMG);
            SetBaseStatus(status, eStatusType.EARTH_DMG, level, BaseData.EARTH_DMG, FactorData.EARTH_DMG);
            SetBaseStatus(status, eStatusType.WIND_DMG, level, BaseData.WIND_DMG, FactorData.WIND_DMG);
            SetBaseStatus(status, eStatusType.WATER_DMG, level, BaseData.WATER_DMG, FactorData.WATER_DMG);
            //비성장 스텟
            SetBaseStatus(status, eStatusType.RATIO_PVP_DMG, level, BaseData.RATIO_PVP_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_PVP_DMG, level, BaseData.ADD_PVP_DMG, 0);
            SetBaseStatus(status, eStatusType.RATIO_PVP_CRI_DMG, level, BaseData.RATIO_PVP_CRI_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_PVP_CRI_DMG, level, BaseData.ADD_PVP_CRI_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_ATKSPEED, level, BaseData.ADD_ATKSPEED, 0);

            return status;
        }
        public static BossCharacterStatus BaseBossCharStatus(ICharacterBaseData BaseData, StatFactorData FactorData, eStageType type)
        {
            if (BaseData == null || FactorData == null)
                return null;

            var status = new BossCharacterStatus();
            status.Initialze();

            float HP_rate = 1.0f;
            float ATK_rate = 1.0f;
            float DEF_rate = 1.0f;
            switch (type)
            {
                case eStageType.ADVENTURE:
                    HP_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "hp", 1.0f);
                    ATK_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "atk", 1.0f);
                    DEF_rate = ServerOptionData.GetJsonValueFloat("stage_balance", "def", 1.0f);
                    break;
                case eStageType.DAILY_DUNGEON:
                    HP_rate = ServerOptionData.GetJsonValueFloat("daily_balance", "hp", 1.0f);
                    ATK_rate = ServerOptionData.GetJsonValueFloat("daily_balance", "atk", 1.0f);
                    DEF_rate = ServerOptionData.GetJsonValueFloat("daily_balance", "def", 1.0f);
                    break;
                case eStageType.WORLD_BOSS:
                    HP_rate = ServerOptionData.GetJsonValueFloat("raid_balance", "hp", 1.0f);
                    ATK_rate = ServerOptionData.GetJsonValueFloat("raid_balance", "atk", 1.0f);
                    DEF_rate = ServerOptionData.GetJsonValueFloat("raid_balance", "def", 1.0f);
                    break;
                default:
                    break;
            }
            //성장 스텟
            SetBaseStatus(status, eStatusType.HP, 1, BaseData.HP * HP_rate, FactorData.HP * HP_rate);
            SetBaseStatus(status, eStatusType.ATK, 1, BaseData.ATK * ATK_rate, FactorData.ATK * ATK_rate);
            SetBaseStatus(status, eStatusType.DEF, 1, BaseData.DEF * DEF_rate, FactorData.DEF * DEF_rate);

            SetBaseStatus(status, eStatusType.CRI_PROC, 1, BaseData.CRI_PROC, FactorData.CRI_PROC);
            SetBaseStatus(status, eStatusType.CRI_DMG, 1, BaseData.CRI_DMG, FactorData.CRI_DMG);
            SetBaseStatus(status, eStatusType.LIGHT_DMG, 1, BaseData.LIGHT_DMG, FactorData.LIGHT_DMG);
            SetBaseStatus(status, eStatusType.DARK_DMG, 1, BaseData.DARK_DMG, FactorData.DARK_DMG);
            SetBaseStatus(status, eStatusType.FIRE_DMG, 1, BaseData.FIRE_DMG, FactorData.FIRE_DMG);
            SetBaseStatus(status, eStatusType.EARTH_DMG, 1, BaseData.EARTH_DMG, FactorData.EARTH_DMG);
            SetBaseStatus(status, eStatusType.WIND_DMG, 1, BaseData.WIND_DMG, FactorData.WIND_DMG);
            SetBaseStatus(status, eStatusType.WATER_DMG, 1, BaseData.WATER_DMG, FactorData.WATER_DMG);
            //비성장 스텟
            SetBaseStatus(status, eStatusType.RATIO_PVP_DMG, 1, BaseData.RATIO_PVP_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_PVP_DMG, 1, BaseData.ADD_PVP_DMG, 0);
            SetBaseStatus(status, eStatusType.RATIO_PVP_CRI_DMG, 1, BaseData.RATIO_PVP_CRI_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_PVP_CRI_DMG, 1, BaseData.ADD_PVP_CRI_DMG, 0);
            SetBaseStatus(status, eStatusType.ADD_ATKSPEED, 1, BaseData.ADD_ATKSPEED, 0);

            return status;
        }

        /*
         * return param
         * { HP : number, ATK : number, DEF : number, CRI : number, HP_PER : number, ATK_PER : number, DEF_PER : number, CRI_PER : number }
         */

        public static SubOptionStatus GetSubOption(List<KeyValuePair<int, float>> arrOptions, SubOptionStatus stat = null)
        {
            if (stat == null)
            {
                stat = new SubOptionStatus();
                stat.Initialze();
            }

            if (arrOptions != null)
            {
                for (var i = 0; i < arrOptions.Count; i++)
                {
                    var tData = arrOptions[i];

                    if (tData.Key < 1)
                        continue;

                    var subOptionData = SubOptionData.Get(tData.Key);
                    if (subOptionData == null)
                        continue;

                    stat.IncreaseStatus(subOptionData.STAT_TYPE, subOptionData.VALUE_TYPE, tData.Value);
                }
            }

            return stat;
        }

        /*
         * return param
         * HP: number, ATK: number, DEF: number, CRI: number, 
         * HP_PER: number, ATK_PER: number, DEF_PER: number, CRI_PER: number
         */
        public static PartStatus GetPartSetEffectOption(int[] arrOptions)
        {
            if (arrOptions == null)
                return null;

            PartStatus stat = new();
            stat.Initialze();

            for (var i = 0; i < arrOptions.Length; i++)
            {
                stat.IncreaseStatus(PartSetData.Get(arrOptions[i].ToString()));
            }

            return stat;
        }

        // 리워드 아이템 JArray -> ProductReward 타입 리스트 형태 반환
        public static List<ProductReward> ConvertToRewardItemList(List<ProductReward> prevList, JArray rewardList)
        {
            List<ProductReward> resultList = new();
            if (IsJArray(rewardList) == false)
            {
                return resultList;
            }

            foreach (JArray rewardArray in rewardList)
            {
                List<int> tempRewardList = rewardArray.ToObject<List<int>>();
                if (tempRewardList == null || tempRewardList.Count < 3) { continue; }

                eGoodType type = (eGoodType)tempRewardList[0];
                int no = tempRewardList[1];
                int amount = tempRewardList[2];

                bool isNew = prevList != null;
                if (isNew)
                {
                    foreach (var prev in prevList)
                    {
                        if (prev.GoodType == type && prev.ItemNo == no && prev.Amount == amount)
                        {
                            isNew = false;
                            break;
                        }
                    }
                }


                resultList.Add(new ProductReward(type, no, amount, isNew));
            }


            return resultList;
        }

        public static List<Asset> ConvertSystemRewardDataList(JArray rewardList, bool _isMerge = false)
        {
            if (IsJArray(rewardList) == false) { return null; }

            List<Asset> resultList = new();

            Dictionary<int, Dictionary<int, Asset>> tempDic = new Dictionary<int, Dictionary<int, Asset>>(); //key : itemNo , value : asset

            if (rewardList.Count >= 0)
            {
                if (!IsJArray(rewardList[0]) && rewardList.Count == 3)
                {
                    List<int> tempRewardList = rewardList.ToObject<List<int>>();

                    var itemNo = tempRewardList[1];
                    var amount = tempRewardList[2];
                    var type = tempRewardList[0];

                    Asset newItem = new Asset(itemNo, amount, type);
                    resultList.Add(newItem);

                    if (!tempDic.ContainsKey(type))
                    {
                        tempDic.Add(type, new Dictionary<int, Asset>());
                    }

                    if (tempDic[type].ContainsKey(itemNo))
                        tempDic[type][itemNo].AddCount(amount);
                    else
                        tempDic[type].Add(itemNo, newItem);
                }
                else
                {
                    foreach (JArray rewardArray in rewardList)
                    {
                        List<int> tempRewardList = rewardArray.ToObject<List<int>>();
                        if (tempRewardList == null || tempRewardList.Count < 3) { continue; }

                        var itemNo = tempRewardList[1];
                        var amount = tempRewardList[2];
                        var type = tempRewardList[0];

                        Asset newItem = new Asset(itemNo, amount, type);
                        resultList.Add(newItem);

                        if (!tempDic.ContainsKey(type))
                        {
                            tempDic.Add(type, new Dictionary<int, Asset>());
                        }

                        if (tempDic[type].ContainsKey(itemNo))
                            tempDic[type][itemNo].AddCount(amount);
                        else
                            tempDic[type].Add(itemNo, new Asset(itemNo, amount, type));
                    }
                }
            }

            if (_isMerge)
            {
                resultList = new List<Asset>();
                foreach (var typs in tempDic.Values)
                {
                    resultList.AddRange(typs.Values.ToList());
                }
            }

            return resultList;
        }
        public static eQuestType ConvertToQuestType(string value)
        {
            return value switch
            {
                "MAIN" => eQuestType.MAIN,
                "SUB" => eQuestType.SUB,
                "EVENT" => eQuestType.EVENT,
                "DAILY" => eQuestType.DAILY,
                "WEEKLY" => eQuestType.WEEKLY,
                "CHAIN" => eQuestType.CHAIN,
                "TOWN" => eQuestType.TOWN,
                "PASS" => eQuestType.BATTLE_PASS,
                "HOLDER" => eQuestType.HOLDER_PASS,
                _ => eQuestType.NONE
            };
        }

        #region Comma
        public static string CommaFromNumber(int item)
        {
            NumberFormatInfo nfi = GamePreference.Instance.Culture.NumberFormat;
            nfi.NumberDecimalDigits = 0;
            return item.ToString("N", nfi);
        }
        public static string CommaFromNumber(float item)
        {
            return item.ToString("N");
        }
        public static string CommaFromNumber(double item)
        {
            return item.ToString("N");
        }
        public static string CommaFromNumber(long item)
        {
            return item.ToString("N0");
        }
        public static string CommaFromMoney(int item)
        {
            return item.ToString("C");
        }
        public static string CommaFromMoney(float item)
        {
            return item.ToString("C");
        }
        public static string CommaFromMoney(double item)
        {
            return item.ToString("C");
        }
        #endregion
        #region UI
        public static Vector2 WorldToUICanvasPosition(Vector3 worldPos)
        {
            Vector2 screenCanvasPos = Vector2.zero;

            Camera uiCamera = UICanvas.Instance.GetCamera();
            if (uiCamera != null)
            {
                Vector3 viewPos = uiCamera.WorldToViewportPoint(worldPos);
                RectTransform canvasRect = UICanvas.Instance.GetCanvasRectTransform();

                screenCanvasPos = new Vector2(
                    ((viewPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
                    ((viewPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));
            }

            return screenCanvasPos;
        }
        public static Vector2 WorldToCanvasPosition(Vector3 worldPos)
        {
            Vector2 screenCanvasPos = Vector2.zero;

            Camera camera = Camera.main;
            if (camera != null)
            {
                Vector3 viewPos = camera.WorldToViewportPoint(worldPos);
                RectTransform canvasRect = UICanvas.Instance.GetCanvasRectTransform();

                screenCanvasPos = new Vector2(
                    ((viewPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
                    ((viewPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));
            }

            return screenCanvasPos;
        }
        public static Sprite GetGoodTypeIcon(eGoodType goodType, int itemID = 0)
        {
            Sprite resultIcon = null;

            switch (goodType)
            {
                case eGoodType.GOLD:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                    break;
                case eGoodType.ENERGY:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "energy");
                    break;
                case eGoodType.ACCOUNT_EXP:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "acc_exp_icon");
                    break;
                case eGoodType.ARENA_TICKET:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "item_pvp_ticket_1");
                    break;
                case eGoodType.ARENA_POINT:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "icon_pvp_point");
                    break;
                case eGoodType.ADVERTISEMENT:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "icon_ad");
                    break;
                case eGoodType.MILEAGE:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "icon_mileage");
                    break;
                case eGoodType.GEMSTONE:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                    break;
                case eGoodType.MAGNET:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "magnet");
                    break;
                case eGoodType.MAGNITE:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "magnite");
                    break;
                case eGoodType.CASH:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "cash");
                    break;
                case eGoodType.FRIENDLY_POINT:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "icon_heart");
                    break;
                case eGoodType.GUILD_POINT:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "guild_point");
                    break;
                case eGoodType.GUILD_EXP:
                    resultIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "guild_exp");
                    break;
                case eGoodType.ITEM:
                    if (itemID > 0)
                    {
                        ItemBaseData itemData = ItemBaseData.Get(itemID);
                        if (itemData != null)
                        {
                            resultIcon = itemData.ICON_SPRITE;
                        }
                    }
                    break;
            }

            return resultIcon;
        }
        public static string GetGoodTypeAmountText(eGoodType goodType, int itemID = 0)
        {
            string amountText = "";

            switch (goodType)
            {
                case eGoodType.GOLD:
                    amountText = User.Instance.GOLD.ToString();
                    break;
                case eGoodType.ENERGY:
                    amountText = User.Instance.ENERGY.ToString();
                    break;
                case eGoodType.ITEM:
                    if (itemID > 0)
                    {
                        amountText = User.Instance.GetItemCount(itemID).ToString();
                    }
                    break;
                case eGoodType.ADVERTISEMENT:
                    break;
                case eGoodType.GEMSTONE:
                    amountText = User.Instance.GEMSTONE.ToString();
                    break;
            }

            return amountText;
        }
        public static eItemFrameType GetFrameTypeByGoodType(eGoodType _type)
        {
            return _type switch
            {
                eGoodType.GOLD => eItemFrameType.GOLD,
                eGoodType.ENERGY => eItemFrameType.STEMINA,
                eGoodType.ARENA_TICKET => eItemFrameType.ARENA_TICKET,
                eGoodType.GEMSTONE => eItemFrameType.GEMSTONE,
                eGoodType.ACCOUNT_EXP => eItemFrameType.ACCOUNT_EXP,
                eGoodType.MAGNET => eItemFrameType.MAGNET,
                eGoodType.MAGNITE => eItemFrameType.MAGNITE,
                eGoodType.ARENA_POINT => eItemFrameType.ARENA_POINT,
                eGoodType.FRIENDLY_POINT => eItemFrameType.FRIEND_POINT,
                _ => eItemFrameType.ITEM
            };
        }
        #endregion
        #region JTokenCheck
        public static bool IsJTokenCheck(JToken token)
        {
            if (token == null)
                return false;

            switch (token.Type)
            {
                case JTokenType.None:
                case JTokenType.Null:
                case JTokenType.Undefined: return false;
                case JTokenType.Array:
                case JTokenType.Object: return token.HasValues;
                case JTokenType.String: return token.ToString() != string.Empty;
            }

            return true;
        }
        public static bool IsJTokenType(JToken token, JTokenType type)
        {
            if (!IsJTokenCheck(token))
                return false;

            return token.Type == type;
        }
        public static bool IsJArray(JToken token)
        {
            return IsJTokenType(token, JTokenType.Array);
        }
        public static bool IsJObject(JToken token)
        {
            return IsJTokenType(token, JTokenType.Object);
        }
        #endregion
        #region Childrens
        public static GameObject[] GetChildren(GameObject parent)
        {
            var count = parent.transform.childCount;
            GameObject[] children = new GameObject[count];

            for (int i = 0; i < count; i++)
            {
                children[i] = parent.transform.GetChild(i).gameObject;
            }

            return children;
        }
        public static Transform GetChildrensByName(Transform target, params string[] targets)
        {
            var count = targets.Length;
            for (var i = 0; i < count; ++i)
            {
                if (target == null)
                    break;
                target = target.Find(targets[i]);
            }

            return target;
        }
        public static void RemoveAllChildrens(Transform target)
        {
            var count = target.childCount;

            for (var i = 0; i < count; ++i)
            {
                var cur = target.GetChild(i);
                if (cur != null)
                    UnityEngine.Object.Destroy(cur.gameObject);
            }

            return;
        }
        public static Transform[] GetChildren(Transform parent)
        {
            var count = parent.childCount;
            Transform[] children = new Transform[count];

            for (int i = 0; i < count; i++)
            {
                children[i] = parent.GetChild(i);
            }

            return children;
        }
        public static void SetLayer(Transform parent, int layer)
        {
            if (parent == null)
                return;

            parent.gameObject.layer = layer;
            var count = parent.childCount;

            for (int i = 0; i < count; i++)
            {
                var target = parent.GetChild(i);
                if (target == null)
                    continue;

                SetLayer(target, layer);
            }
        }
        public static void SetLayer(Transform parent, string layerName)
        {
            SetLayer(parent, LayerMask.NameToLayer(layerName));
        }
        public static void SetLayer(GameObject obj, int layer)
        {
            if (obj == null)
                return;

            SetLayer(obj.transform, layer);
        }
        public static void SetLayer(GameObject obj, string layerName)
        {
            if (obj == null)
                return;

            SetLayer(obj.transform, LayerMask.NameToLayer(layerName));
        }
        #endregion
        #region Bezier
        public static float BezierCurve(float start, float end, float curTime, float maxTime)
        {
            var min = 0f;
            if (start != 0f)
            {
                min = start;
                start = 0;
                end -= min;
            }
            var normalT = curTime / maxTime;
            var normalS = 1 - normalT;
            var startS = start * normalS;
            var endT = end * normalT;
            return min + startS + endT;
        }
        public static float BezierCurve2(float start, float wayPoint, float end, float curTime, float maxTime)
        {
            var normalT = curTime / maxTime;
            var normalS = 1 - normalT;
            var startS = Mathf.Pow(normalS, 2) * start;
            var wayPointST = 2 * normalS * normalT * wayPoint;
            var endT = Mathf.Pow(normalT, 2) * end;
            return startS + wayPointST + endT;
        }
        public static Vector3 BezierCurveVec3(Vector3 start, Vector3 end, float curTime, float maxTime)
        {
            var min = Vector3.zero;
            if (start.x != 0 || start.y != 0 || start.z != 0)
            {
                min.x = start.x;
                min.y = start.y;
                min.z = start.z;
                start = Vector3.zero;
                end.x -= min.x;
                end.y -= min.y;
                end.z -= min.z;
            }
            var normalT = curTime / maxTime;
            var normalS = 1 - normalT;
            var startS = new Vector3(start.x * normalS, start.y * normalS, start.z * normalS);
            var endT = new Vector3(end.x * normalT, end.y * normalT, end.z * normalT);
            return new Vector3(min.x + startS.x + endT.x, min.y + startS.y + endT.y, min.z + startS.z + endT.z);
        }
        public static Vector3 BezierCurve2Vec3(Vector3 start, Vector3 wayPoint, Vector3 end, float curTime, float maxTime)
        {
            var min = Vector3.zero;
            if (start.x != 0 || start.y != 0 || start.z != 0)
            {
                min.x = start.x;
                min.y = start.y;
                min.z = start.z;
                start = Vector3.zero;
                wayPoint.x -= min.x;
                wayPoint.y -= min.y;
                wayPoint.z -= min.z;
                end.x -= min.x;
                end.y -= min.y;
                end.z -= min.z;
            }
            var normalT = curTime / maxTime;
            var normalS = 1 - normalT;
            var S = Mathf.Pow(normalS, 2);
            var W = 2 * normalS * normalT;
            var T = Mathf.Pow(normalT, 2);
            var startS = new Vector3(start.x * S, start.y * S, start.z * S);
            var wayPointST = new Vector3(wayPoint.x * W, wayPoint.y * W, wayPoint.z * W);
            var endT = new Vector3(end.x * T, end.y * T, end.z * T);
            return new Vector3(min.x + startS.x + wayPointST.x + endT.x, min.y + startS.y + wayPointST.y + endT.y, min.x + startS.z + wayPointST.z + endT.z);
        }
        public static Vector2 BezierCurve3Vec2(Vector2 start, Vector2 wayPoint1, Vector2 wayPoint2, Vector2 end, float curTime, float maxTime)
        {
            var min = Vector2.zero;
            if (start.x != 0 || start.y != 0)
            {
                min.x = start.x;
                min.y = start.y;
                start = Vector2.zero;
                wayPoint1.x -= min.x;
                wayPoint1.y -= min.y;
                wayPoint2.x -= min.x;
                wayPoint2.y -= min.y;
                end.x -= min.x;
                end.y -= min.y;
            }
            var normalT = curTime / maxTime;
            var normalS = 1 - normalT;
            var S = Mathf.Pow(normalS, 3);
            var W1 = 3 * Mathf.Pow(normalS, 2) * normalT;
            var W2 = 3 * Mathf.Pow(normalT, 2) * normalS;
            var T = Mathf.Pow(normalT, 3);
            var startS = new Vector2(S * start.x, S * start.y);
            var wayPointST = new Vector2(W1 * wayPoint1.x, W1 * wayPoint1.y);
            var wayPointTS = new Vector2(W2 * wayPoint2.x, W2 * wayPoint2.y);
            var endT = new Vector2(T * end.x, T * end.y);
            return new Vector2(min.x + startS.x + wayPointST.x + wayPointTS.x + endT.x, min.y + startS.y + wayPointST.y + wayPointTS.y + endT.y);
        }
        public static Vector3 BezierCurve3Vec3(Vector3 start, Vector3 wayPoint1, Vector3 wayPoint2, Vector3 end, float curTime, float maxTime)
        {
            var min = Vector3.zero;
            if (start.x != 0 || start.y != 0 || start.z != 0)
            {
                min.x = start.x;
                min.y = start.y;
                min.z = start.z;
                start = Vector3.zero;
                wayPoint1.x -= min.x;
                wayPoint1.y -= min.y;
                wayPoint1.z -= min.z;
                wayPoint2.x -= min.x;
                wayPoint2.y -= min.y;
                wayPoint2.z -= min.z;
                end.x -= min.x;
                end.y -= min.y;
                end.z -= min.z;
            }
            var normalT = curTime / maxTime;
            var normalS = 1 - normalT;
            var S = Mathf.Pow(normalS, 3);
            var W1 = 3 * Mathf.Pow(normalS, 2) * normalT;
            var W2 = 3 * Mathf.Pow(normalT, 2) * normalS;
            var T = Mathf.Pow(normalT, 3);
            var startS = new Vector3(S * start.x, S * start.y, S * start.z);
            var wayPointST = new Vector3(W1 * wayPoint1.x, W1 * wayPoint1.y, W1 * wayPoint1.z);
            var wayPointTS = new Vector3(W2 * wayPoint2.x, W2 * wayPoint2.y, W2 * wayPoint2.z);
            var endT = new Vector3(T * end.x, T * end.y, T * end.z);
            return new Vector3(min.x + startS.x + wayPointST.x + wayPointTS.x + endT.x, min.y + startS.y + wayPointST.y + wayPointTS.y + endT.y, min.z + startS.z + wayPointST.z + wayPointTS.z + endT.z);
        }
        public static float BezierCurveSpeed(float start, float end, float curTime, float maxTime, Vector4 bezierSpeedVec)
        {
            var min = 0f;
            if (start != 0f)
            {
                min = start;
                start = 0;
                end -= min;
            }

            var normalT = BezierCurve3Vec2(new Vector2(0, 0), new Vector2(bezierSpeedVec.x, bezierSpeedVec.y), new Vector2(bezierSpeedVec.z, bezierSpeedVec.w), new Vector2(1, 1), curTime, maxTime).y;
            var normalS = 1 - normalT;
            var startS = start * normalS;
            var endT = end * normalT;
            return min + startS + endT;
        }
        public static float BezierCurve2Speed(float start, float wayPoint, float end, float curTime, float maxTime, Vector4 bezierSpeedVec)
        {
            var normalT = BezierCurve3Vec2(new Vector2(0, 0), new Vector2(bezierSpeedVec.x, bezierSpeedVec.y), new Vector2(bezierSpeedVec.z, bezierSpeedVec.w), new Vector2(1, 1), curTime, maxTime).y;
            var normalS = 1 - normalT;
            var startS = Mathf.Pow(normalS, 2) * start;
            var wayPointST = 2 * normalS * normalT * wayPoint;
            var endT = Mathf.Pow(normalT, 2) * end;
            return startS + wayPointST + endT;
        }
        #endregion
        #region Casts
        public static RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color, bool drawGizmo = false)
        {
            if (drawGizmo)
            {
                Debug.DrawRay(rayOriginPoint, rayDirection * rayDistance, color);
            }
            return Physics2D.Raycast(rayOriginPoint, rayDirection, rayDistance, mask);
        }
        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float length, LayerMask mask, Color color, bool drawGizmo = false)
        {
            if (drawGizmo)
            {
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

                Vector3[] points = new Vector3[8];

                float halfSizeX = size.x / 2f;
                float halfSizeY = size.y / 2f;

                points[0] = rotation * (origin + (Vector2.left * halfSizeX) + (Vector2.up * halfSizeY)); // top left
                points[1] = rotation * (origin + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY)); // top right
                points[2] = rotation * (origin + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY)); // bottom right
                points[3] = rotation * (origin + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY)); // bottom left

                points[4] = rotation * ((origin + Vector2.left * halfSizeX + Vector2.up * halfSizeY) + length * direction); // top left
                points[5] = rotation * ((origin + Vector2.right * halfSizeX + Vector2.up * halfSizeY) + length * direction); // top right
                points[6] = rotation * ((origin + Vector2.right * halfSizeX - Vector2.up * halfSizeY) + length * direction); // bottom right
                points[7] = rotation * ((origin + Vector2.left * halfSizeX - Vector2.up * halfSizeY) + length * direction); // bottom left

                Debug.DrawLine(points[0], points[1], color);
                Debug.DrawLine(points[1], points[2], color);
                Debug.DrawLine(points[2], points[3], color);
                Debug.DrawLine(points[3], points[0], color);

                Debug.DrawLine(points[4], points[5], color);
                Debug.DrawLine(points[5], points[6], color);
                Debug.DrawLine(points[6], points[7], color);
                Debug.DrawLine(points[7], points[4], color);

                Debug.DrawLine(points[0], points[4], color);
                Debug.DrawLine(points[1], points[5], color);
                Debug.DrawLine(points[2], points[6], color);
                Debug.DrawLine(points[3], points[7], color);

            }
            return Physics2D.BoxCast(origin, size, angle, direction, length, mask);
        }
        public static RaycastHit2D MonoRayCastNonAlloc(RaycastHit2D[] array, Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color, bool drawGizmo = false)
        {
            if (drawGizmo)
            {
                Debug.DrawRay(rayOriginPoint, rayDirection * rayDistance, color);
            }
            if (Physics2D.RaycastNonAlloc(rayOriginPoint, rayDirection, array, rayDistance, mask) > 0)
            {
                return array[0];
            }
            return new RaycastHit2D();
        }
        public static RaycastHit Raycast3D(Vector3 rayOriginPoint, Vector3 rayDirection, float rayDistance, LayerMask mask, Color color, bool drawGizmo = false)
        {
            if (drawGizmo)
            {
                Debug.DrawRay(rayOriginPoint, rayDirection * rayDistance, color);
            }
            RaycastHit hit;
            Physics.Raycast(rayOriginPoint, rayDirection, out hit, rayDistance, mask);
            return hit;
        }
        #endregion
        #region DebugDraw
        public static void DrawGizmoArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 3f, float arrowHeadAngle = 25f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(origin, direction);

            DrawArrowEnd(true, origin, direction, color, arrowHeadLength, arrowHeadAngle);
        }
        public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 0.2f, float arrowHeadAngle = 35f)
        {
            Debug.DrawRay(origin, direction, color);

            DrawArrowEnd(false, origin, direction, color, arrowHeadLength, arrowHeadAngle);
        }
        public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowLength, float arrowHeadLength = 0.20f, float arrowHeadAngle = 35.0f)
        {
            Debug.DrawRay(origin, direction * arrowLength, color);

            DrawArrowEnd(false, origin, direction * arrowLength, color, arrowHeadLength, arrowHeadAngle);
        }
        public static void DebugDrawCross(Vector3 spot, float crossSize, Color color)
        {
            Vector3 tempOrigin = Vector3.zero;
            Vector3 tempDirection = Vector3.zero;

            tempOrigin.x = spot.x - crossSize / 2;
            tempOrigin.y = spot.y - crossSize / 2;
            tempOrigin.z = spot.z;
            tempDirection.x = 1;
            tempDirection.y = 1;
            tempDirection.z = 0;
            Debug.DrawRay(tempOrigin, tempDirection * crossSize, color);

            tempOrigin.x = spot.x - crossSize / 2;
            tempOrigin.y = spot.y + crossSize / 2;
            tempOrigin.z = spot.z;
            tempDirection.x = 1;
            tempDirection.y = -1;
            tempDirection.z = 0;
            Debug.DrawRay(tempOrigin, tempDirection * crossSize, color);
        }
        private static void DrawArrowEnd(bool drawGizmos, Vector3 arrowEndPosition, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
        {
            if (direction == Vector3.zero)
            {
                return;
            }
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
            Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
            if (drawGizmos)
            {
                Gizmos.color = color;
                Gizmos.DrawRay(arrowEndPosition + direction, right * arrowHeadLength);
                Gizmos.DrawRay(arrowEndPosition + direction, left * arrowHeadLength);
                Gizmos.DrawRay(arrowEndPosition + direction, up * arrowHeadLength);
                Gizmos.DrawRay(arrowEndPosition + direction, down * arrowHeadLength);
            }
            else
            {
                Debug.DrawRay(arrowEndPosition + direction, right * arrowHeadLength, color);
                Debug.DrawRay(arrowEndPosition + direction, left * arrowHeadLength, color);
                Debug.DrawRay(arrowEndPosition + direction, up * arrowHeadLength, color);
                Debug.DrawRay(arrowEndPosition + direction, down * arrowHeadLength, color);
            }
        }
        public static void DrawHandlesBounds(Bounds bounds, Color color)
        {
#if UNITY_EDITOR
            Vector3 boundsCenter = bounds.center;
            Vector3 boundsExtents = bounds.extents;

            Vector3 v3FrontTopLeft = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front top left corner
            Vector3 v3FrontTopRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front top right corner
            Vector3 v3FrontBottomLeft = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front bottom left corner
            Vector3 v3FrontBottomRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front bottom right corner
            Vector3 v3BackTopLeft = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back top left corner
            Vector3 v3BackTopRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back top right corner
            Vector3 v3BackBottomLeft = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back bottom left corner
            Vector3 v3BackBottomRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back bottom right corner


            UnityEditor.Handles.color = color;

            UnityEditor.Handles.DrawLine(v3FrontTopLeft, v3FrontTopRight);
            UnityEditor.Handles.DrawLine(v3FrontTopRight, v3FrontBottomRight);
            UnityEditor.Handles.DrawLine(v3FrontBottomRight, v3FrontBottomLeft);
            UnityEditor.Handles.DrawLine(v3FrontBottomLeft, v3FrontTopLeft);

            UnityEditor.Handles.DrawLine(v3BackTopLeft, v3BackTopRight);
            UnityEditor.Handles.DrawLine(v3BackTopRight, v3BackBottomRight);
            UnityEditor.Handles.DrawLine(v3BackBottomRight, v3BackBottomLeft);
            UnityEditor.Handles.DrawLine(v3BackBottomLeft, v3BackTopLeft);

            UnityEditor.Handles.DrawLine(v3FrontTopLeft, v3BackTopLeft);
            UnityEditor.Handles.DrawLine(v3FrontTopRight, v3BackTopRight);
            UnityEditor.Handles.DrawLine(v3FrontBottomRight, v3BackBottomRight);
            UnityEditor.Handles.DrawLine(v3FrontBottomLeft, v3BackBottomLeft);
#endif
        }
        public static void DrawSolidRectangle(Vector3 position, Vector3 size, Color borderColor, Color solidColor)
        {
#if UNITY_EDITOR
            Vector3 halfSize = size / 2f;

            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
            verts[1] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            verts[2] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            verts[3] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(verts, solidColor, borderColor);

#endif
        }
        public static void DrawGizmoPoint(Vector3 position, float size, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(position, size);
        }
        public static void DrawCube(Vector3 position, Color color, Vector3 size)
        {
            Vector3 halfSize = size / 2f;

            Vector3[] points = new Vector3[]
            {
                position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),
                position + new Vector3(halfSize.x,halfSize.y,-halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,-halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,-halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,-halfSize.z),
            };

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[3], points[0], color);
        }
        public static void DrawGizmoCube(Transform transform, Vector3 offset, Vector3 cubeSize, bool wireOnly)
        {
            Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
            Gizmos.matrix = rotationMatrix;
            if (wireOnly)
            {
                Gizmos.DrawWireCube(offset, cubeSize);
            }
            else
            {
                Gizmos.DrawCube(offset, cubeSize);
            }
        }
        public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Color color)
        {
            Gizmos.color = color;

            Vector3 v3TopLeft = new Vector3(center.x - size.x / 2, center.y + size.y / 2, 0);
            Vector3 v3TopRight = new Vector3(center.x + size.x / 2, center.y + size.y / 2, 0); ;
            Vector3 v3BottomRight = new Vector3(center.x + size.x / 2, center.y - size.y / 2, 0); ;
            Vector3 v3BottomLeft = new Vector3(center.x - size.x / 2, center.y - size.y / 2, 0); ;

            Gizmos.DrawLine(v3TopLeft, v3TopRight);
            Gizmos.DrawLine(v3TopRight, v3BottomRight);
            Gizmos.DrawLine(v3BottomRight, v3BottomLeft);
            Gizmos.DrawLine(v3BottomLeft, v3TopLeft);
        }
        public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Matrix4x4 rotationMatrix, Color color)
        {
            GL.PushMatrix();

            Gizmos.color = color;

            Vector3 v3TopLeft = rotationMatrix * new Vector3(center.x - size.x / 2, center.y + size.y / 2, 0);
            Vector3 v3TopRight = rotationMatrix * new Vector3(center.x + size.x / 2, center.y + size.y / 2, 0); ;
            Vector3 v3BottomRight = rotationMatrix * new Vector3(center.x + size.x / 2, center.y - size.y / 2, 0); ;
            Vector3 v3BottomLeft = rotationMatrix * new Vector3(center.x - size.x / 2, center.y - size.y / 2, 0); ;


            Gizmos.DrawLine(v3TopLeft, v3TopRight);
            Gizmos.DrawLine(v3TopRight, v3BottomRight);
            Gizmos.DrawLine(v3BottomRight, v3BottomLeft);
            Gizmos.DrawLine(v3BottomLeft, v3TopLeft);
            GL.PopMatrix();
        }
        public static void DrawRectangle(Rect rectangle, Color color)
        {
            Vector3 pos = new Vector3(rectangle.x + rectangle.width / 2, rectangle.y + rectangle.height / 2, 0.0f);
            Vector3 scale = new Vector3(rectangle.width, rectangle.height, 0.0f);

            DrawRectangle(pos, color, scale);
        }
        public static void DrawRectangle(Vector3 position, Color color, Vector3 size)
        {
            Vector3 halfSize = size / 2f;

            Vector3[] points = new Vector3[]
            {
                position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),
            };

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[3], points[0], color);
        }
        public static void DrawPoint(Vector3 position, Color color, float size)
        {
            Vector3[] points = new Vector3[]
            {
                position + (Vector3.up * size),
                position - (Vector3.up * size),
                position + (Vector3.right * size),
                position - (Vector3.right * size),
                position + (Vector3.forward * size),
                position - (Vector3.forward * size)
            };

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[4], points[5], color);
            Debug.DrawLine(points[0], points[2], color);
            Debug.DrawLine(points[0], points[3], color);
            Debug.DrawLine(points[0], points[4], color);
            Debug.DrawLine(points[0], points[5], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[1], points[3], color);
            Debug.DrawLine(points[1], points[4], color);
            Debug.DrawLine(points[1], points[5], color);
            Debug.DrawLine(points[4], points[2], color);
            Debug.DrawLine(points[4], points[3], color);
            Debug.DrawLine(points[5], points[2], color);
            Debug.DrawLine(points[5], points[3], color);
        }
        #endregion
        #region Info
        public static string GetSystemInfo()
        {
            string result = "SYSTEM INFO";

#if UNITY_IOS
            result += "\n[iPhone generation]iPhone.generation.ToString()";
#endif
#if UNITY_ANDROID
            result += "\n[system info]" + SystemInfo.deviceModel;
#endif

            result += "\n<color=#FFFFFF>Device Type :</color> " + SystemInfo.deviceType;
            result += "\n<color=#FFFFFF>OS Version :</color> " + SystemInfo.operatingSystem;
            result += "\n<color=#FFFFFF>System Memory Size :</color> " + SystemInfo.systemMemorySize;
            result += "\n<color=#FFFFFF>Graphic Device Name :</color> " + SystemInfo.graphicsDeviceName + " (version " + SystemInfo.graphicsDeviceVersion + ")";
            result += "\n<color=#FFFFFF>Graphic Memory Size :</color> " + SystemInfo.graphicsMemorySize;
            result += "\n<color=#FFFFFF>Graphic Max Texture Size :</color> " + SystemInfo.maxTextureSize;
            result += "\n<color=#FFFFFF>Graphic Shader Level :</color> " + SystemInfo.graphicsShaderLevel;
            result += "\n<color=#FFFFFF>Compute Shader Support :</color> " + SystemInfo.supportsComputeShaders;

            result += "\n<color=#FFFFFF>Processor Count :</color> " + SystemInfo.processorCount;
            result += "\n<color=#FFFFFF>Processor Type :</color> " + SystemInfo.processorType;
            result += "\n<color=#FFFFFF>3D Texture Support :</color> " + SystemInfo.supports3DTextures;
            result += "\n<color=#FFFFFF>Shadow Support :</color> " + SystemInfo.supportsShadows;

            result += "\n<color=#FFFFFF>Platform :</color> " + Application.platform;
            result += "\n<color=#FFFFFF>Screen Size :</color> " + Screen.width + " x " + Screen.height;
            result += "\n<color=#FFFFFF>DPI :</color> " + Screen.dpi;

            return result;
        }
        #endregion
        #region Others
        public static string ListToString<T>(List<T> list)  // WWWForm 에서는 list 형태를 넣을수 없으니깐 변환하기 위해 제작했음
        {
            string result = string.Join(", ", list.Select(i => i.ToString()).ToArray());
            return "[" + result + "]";
        }

        // 현재 활성화 되어있는 자식의 갯수를 반환
        public static int GetActiveChildCount(Transform parentTr)
        {
            int result = 0;

            foreach (Transform childTr in parentTr)
            {
                if (childTr.gameObject.activeInHierarchy)
                {
                    result++;
                }
            }

            return result;
        }

        public static string GetElementConvertString(int e_type)
        {
            var elementStr = "";
            switch (e_type)
            {
                case 1:
                    elementStr = "fire";
                    break;
                case 2:
                    elementStr = "water";
                    break;
                case 3:
                    elementStr = "soil";
                    break;
                case 4:
                    elementStr = "wind";
                    break;
                case 5:
                    elementStr = "light";
                    break;
                case 6:
                    elementStr = "dark";
                    break;
                default:
                    break;
            }
            return elementStr;
        }
        public static Sprite GetGradeBGSprite(int grade)
        {
            string bgImageName = grade switch
            {
                1 => "bggrade_common",
                2 => "bggrade_uncommon",
                3 => "bggrade_rare",
                4 => "bggrade_unique",
                5 => "bggrade_legendary",
                _ => "default_infobg"
            };

            return ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, bgImageName);
        }
        public static string GetGradeConvertString(int grade)
        {
            return ((eDragonGrade)grade) switch
            {
                eDragonGrade.Normal => "COMMON",
                eDragonGrade.Uncommon => "UNCOMMON",
                eDragonGrade.Rare => "RARE",
                eDragonGrade.Unique => "UNIQUE",
                eDragonGrade.Legend => "LEGEND",
                _ => ""
            };
        }
        public static eDragonGrade GetDragonGrade(int grade)
        {
            return grade switch
            {
                2 => eDragonGrade.Uncommon,
                3 => eDragonGrade.Rare,
                4 => eDragonGrade.Unique,
                5 => eDragonGrade.Legend,
                _ => eDragonGrade.Normal
            };
        }
        public static eJobType GetJobType(int job)
        {
            return job switch
            {
                1 => eJobType.TANKER,
                2 => eJobType.WARRIOR,
                3 => eJobType.ASSASSIN,
                4 => eJobType.BOMBER,
                5 => eJobType.SNIPER,
                6 => eJobType.SUPPORTER,
                _ => eJobType.NONE,
            };
        }
        public static eJobFilter GetJobFilterType(int job)
        {
            return job switch
            {
                1 => eJobFilter.TANKER,
                2 => eJobFilter.WARRIOR,
                3 => eJobFilter.ASSASSIN,
                4 => eJobFilter.BOMBER,
                5 => eJobFilter.SNIPER,
                6 => eJobFilter.SUPPORTER,
                _ => eJobFilter.None,
            };
        }
        public static eElementFilter GetElemFilterType(int elem)
        {
            return elem switch
            {
                1 => eElementFilter.Fire,
                2 => eElementFilter.Water,
                3 => eElementFilter.Earth,
                4 => eElementFilter.Wind,
                5 => eElementFilter.Light,
                6 => eElementFilter.Dark,
                _ => eElementFilter.None,
            };
        }

        public static ePetStatFilter GetPetStatFilterType(UserPet pet)
        {
            ePetStatFilter ret = ePetStatFilter.None;
            foreach (var stat in pet.Stats)
            {
                switch (stat.Key)
                {
                    case 1:
                        ret |= ePetStatFilter.RATIO_ATK_DMG_PERCENT; break;
                    case 2:
                        ret |= ePetStatFilter.ADD_ATK_DMG_VALUE; break;
                    case 3:
                        ret |= ePetStatFilter.CRI_DMG_PERCENT; break;
                    case 4:
                        ret |= ePetStatFilter.CRI_DMG_VALUE; break;
                    case 7:
                        ret |= ePetStatFilter.LIGHT_DMG_PERCENT; break;
                    case 8:
                        ret |= ePetStatFilter.LIGHT_DMG_VALUE; break;
                    case 9:
                        ret |= ePetStatFilter.DARK_DMG_PERCENT; break;
                    case 10:
                        ret |= ePetStatFilter.DARK_DMG_VALUE; break;
                    case 11:
                        ret |= ePetStatFilter.WATER_DMG_PERCENT; break;
                    case 12:
                        ret |= ePetStatFilter.WATER_DMG_VALUE; break;
                    case 13:
                        ret |= ePetStatFilter.FIRE_DMG_PERCENT; break;
                    case 14:
                        ret |= ePetStatFilter.FIRE_DMG_VALUE; break;
                    case 15:
                        ret |= ePetStatFilter.WIND_DMG_PERCENT; break;
                    case 16:
                        ret |= ePetStatFilter.WIND_DMG_VALUE; break;
                    case 17:
                        ret |= ePetStatFilter.EARTH_DMG_PERCENT; break;
                    case 18:
                        ret |= ePetStatFilter.EARTH_DMG_VALUE; break;
                    case 19:
                        ret |= ePetStatFilter.RATIO_PVP_DMG_PERCENT; break;
                    case 20:
                        ret |= ePetStatFilter.ADD_PVP_DMG_VALUE; break;
                    case 21:
                        ret |= ePetStatFilter.RATIO_PVP_CRI_DMG_PERCENT; break;
                    case 22:
                        ret |= ePetStatFilter.ADD_PVP_CRI_DMG_VALUE; break;
                    case 23:
                        ret |= ePetStatFilter.BOSS_DMG_PERCENT; break;
                    case 24:
                        ret |= ePetStatFilter.BOSS_DMG_VALUE; break;
                }
            }
            return ret;
        }

        public static ePetOptionFilter GetPetOptionFilterType(UserPet pet)
        {
            ePetOptionFilter ret = ePetOptionFilter.None;
            foreach (var stat in pet.SubOptionList)
            {
                switch (stat.Key)
                {
                    case 2000:
                        ret |= ePetOptionFilter.RATIO_ATK_DMG_PERCENT; break;
                    case 2001:
                        ret |= ePetOptionFilter.CRI_DMG_PERCENT; break;
                    case 2003:
                        ret |= ePetOptionFilter.LIGHT_DMG_PERCENT; break;
                    case 2004:
                        ret |= ePetOptionFilter.DARK_DMG_PERCENT; break;
                    case 2005:
                        ret |= ePetOptionFilter.WATER_DMG_PERCENT; break;
                    case 2006:
                        ret |= ePetOptionFilter.WIND_DMG_PERCENT; break;
                    case 2007:
                        ret |= ePetOptionFilter.FIRE_DMG_PERCENT; break;
                    case 2008:
                        ret |= ePetOptionFilter.EARTH_DMG_PERCENT; break;
                    case 2009:
                        ret |= ePetOptionFilter.RATIO_PVP_DMG_PERCENT; break;
                    case 2010:
                        ret |= ePetOptionFilter.RATIO_PVP_CRI_DMG_PERCENT; break;
                    case 2011:
                        ret |= ePetOptionFilter.ADD_BUFF_TIME_PERCENT; break;
                    case 2012:
                        ret |= ePetOptionFilter.DEL_BUFF_TIME_PERCENT; break;
                    case 2013:
                        ret |= ePetOptionFilter.RATIO_ATK_DMG_PERCENT; break;
                    case 2014:
                        ret |= ePetOptionFilter.BOSS_DMG_PERCENT; break;
                    case 2015:
                        ret |= ePetOptionFilter.CRI_DMG_PERCENT; break;
                    case 2016:
                        ret |= ePetOptionFilter.RATIO_PVP_DMG_PERCENT; break;
                    case 2017:
                        ret |= ePetOptionFilter.RATIO_PVP_CRI_DMG_PERCENT; break;
                    case 2018:
                        ret |= ePetOptionFilter.BOSS_DMG_PERCENT; break;
                }
            }
            return ret;
        }

        public static eGradeFilter GetGradeFilterType(int grade)
        {
            return grade switch
            {
                1 => eGradeFilter.Common,
                2 => eGradeFilter.Uncommon,
                3 => eGradeFilter.Rare,
                4 => eGradeFilter.Unique,
                5 => eGradeFilter.Legendary,
                _ => eGradeFilter.None,
            };
        }
        public static eSkillPassiveGroupType GetSkillPassiveGroupType(int groupType)
        {
            return groupType switch
            {
                1 => eSkillPassiveGroupType.COMMON,
                2 => eSkillPassiveGroupType.UNCOMMON,
                _ => eSkillPassiveGroupType.NONE,
            };
        }
        public static eSkillPassiveEffect GetSkillPassiveEffect(string effectName)
        {
            return effectName switch
            {
                "STAT" => eSkillPassiveEffect.STAT,
                "STAT_MAIN_ELEMENT" => eSkillPassiveEffect.STAT_MAIN_ELEMENT,
                "BUFF" => eSkillPassiveEffect.BUFF,
                "BUFF_MAIN_ELEMENT" => eSkillPassiveEffect.BUFF_MAIN_ELEMENT,
                "DEBUFF" => eSkillPassiveEffect.DEBUFF,
                "HIT" => eSkillPassiveEffect.HIT,
                "REDUCE_COOLTIME" => eSkillPassiveEffect.REDUCE_COOLTIME,
                "REDUCE_BUFF" => eSkillPassiveEffect.REDUCE_BUFF,
                "REDUCE_DEBUFF" => eSkillPassiveEffect.REDUCE_DEBUFF,
                "STRONG_BUFF" => eSkillPassiveEffect.STRONG_BUFF,
                "STRONG_DEBUFF" => eSkillPassiveEffect.STRONG_DEBUFF,
                "CC_REFLECT" => eSkillPassiveEffect.CC_REFLECT,
                "DMG_REFLECT" => eSkillPassiveEffect.DMG_REFLECT,
                "SILENCE" => eSkillPassiveEffect.SILENCE,
                "R_KNOCK_BACK" => eSkillPassiveEffect.R_KNOCK_BACK,
                _ => eSkillPassiveEffect.NONE,
            };
        }
        public static eSkillPassiveStartType GetSkillPassiveStartType(int groupType)
        {
            return groupType switch
            {
                1 => eSkillPassiveStartType.ALWAYS,
                2 => eSkillPassiveStartType.HIT,
                3 => eSkillPassiveStartType.NORMAL_ATTACK,
                4 => eSkillPassiveStartType.CRITICAL_ATTACK,
                5 => eSkillPassiveStartType.SKILL_ATTACK,
                6 => eSkillPassiveStartType.ABNORMAL_STATUS,
                _ => eSkillPassiveStartType.NONE,
            };
        }
        public static eSkillPassiveRateType GetSkillPassiveRateType(int groupType)
        {
            return groupType switch
            {
                1 => eSkillPassiveRateType.ALWAYS,
                2 => eSkillPassiveRateType.CASTER_HP_UP,
                3 => eSkillPassiveRateType.TARGET_HP_DOWN,
                4 => eSkillPassiveRateType.PERCENTAGE,
                5 => eSkillPassiveRateType.CASTER_POSITIVE_ELEMENT,
                6 => eSkillPassiveRateType.CASTER_ADVERSE_ELEMENT,
                7 => eSkillPassiveRateType.CASTER_HP_DOWN,
                _ => eSkillPassiveRateType.NONE,
            };
        }
        public static eContentType GetContentType(int groupType)
        {
            return groupType switch
            {
                1 => eContentType.EVERY,
                2 => eContentType.ARENA,
                _ => eContentType.NONE,
            };
        }
        public static eDirectionBit Reverse(this eDirectionBit type)
        {
            return type switch
            {
                eDirectionBit.Left => eDirectionBit.Right,
                eDirectionBit.Right => eDirectionBit.Left,
                eDirectionBit.Up => eDirectionBit.Down,
                eDirectionBit.Down => eDirectionBit.Up,
                _ => eDirectionBit.None,
            };
        }
        public static bool IsPassiveRateCheck(this SkillPassiveData passive, IBattleCharacterData caster, IBattleCharacterData target)
        {            
            if(SBGameManager.Instance.IsFixedDelta)
            {
                int ret = ChampionManager.Instance.Random.Next(0, 100);

                if(ChampionManager.Instance.PracticeBattleData == null)
                    ret = ChampionManager.Instance.OnRandomLog(ChampionManager.Instance.ChampionData.Time, ret, RandomLog.RandomReason.PassiveRate);

                if ((passive.RATE + passive.GetPassiveRate(caster, target)) + ((passive.RATE + passive.GetPassiveRate(caster, target)) * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_RATE)) > ret)
                    return true;
            }
            else
            {
                if ((passive.RATE + passive.GetPassiveRate(caster, target)) + ((passive.RATE + passive.GetPassiveRate(caster, target)) * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_RATE)) > Random(0f, 100f))
                    return true;
            }

            return false;
        }
        public static float GetPassiveRate(this SkillPassiveData passive, IBattleCharacterData caster, IBattleCharacterData target)
        {
            switch (passive.RATE_TYPE)
            {
                //시전자 체력 높을 수록
                case eSkillPassiveRateType.CASTER_HP_UP:
                    return passive.ADD_RATE_MAX * ((float)caster.HP / caster.MaxHP);
                //타겟 체력 낮을 수록
                case eSkillPassiveRateType.TARGET_HP_DOWN:
                    return passive.ADD_RATE_MAX * Mathf.Clamp01((1f - ((float)target.HP / target.MaxHP)) / 0.9f);
                //유리한 속성
                case eSkillPassiveRateType.CASTER_POSITIVE_ELEMENT:
                {
                    if (caster.BaseData.ELEMENT_TYPE.PositiveElement() == target.BaseData.ELEMENT_TYPE)
                    {
                        return passive.ADD_RATE_MAX;
                    }
                    return 0f;
                }
                //불리한 속성
                case eSkillPassiveRateType.CASTER_ADVERSE_ELEMENT:
                {
                    if (caster.BaseData.ELEMENT_TYPE.AdverseElement() == target.BaseData.ELEMENT_TYPE)
                    {
                        return passive.ADD_RATE_MAX;
                    }
                    return 0f;
                }
                //시전자 체력 낮을 수록
                case eSkillPassiveRateType.CASTER_HP_DOWN:
                    return passive.ADD_RATE_MAX * Mathf.Clamp01((1f - ((float)caster.HP / caster.MaxHP)) / 0.9f);
                //지정 확률
                case eSkillPassiveRateType.PERCENTAGE:
                {
                    return passive.ADD_RATE_MAX;
                }
                //항시 발동은 100%
                case eSkillPassiveRateType.ALWAYS: return 100f;
                default: return 0f;
            }
        }
        public static bool IsPassiveEffect(this SkillPassiveData passive, params eSkillPassiveEffect[] passiveType)
        {
            if (passive == null)
                return false;

            return passiveType.Contains(passive.PASSIVE_EFFECT);
        }
        public static bool IsPassiveStartSkip(this SkillPassiveData passive, eSkillPassiveStartType startType)
        {
            if (passive == null)
                return false;

            if (startType != passive.START_TYPE)
                return true;

            return false;
        }
        public static bool IsPassiveTargetSkip(this SkillPassiveData passive, IBattleCharacterData caster, IBattleCharacterData target)
        {
            if (passive == null)
                return false;

            return passive.TARGET switch
            {
                eSkillTargetType.ALL => false,
                eSkillTargetType.ENEMY => caster.IsEnemy == target.IsEnemy,
                eSkillTargetType.ALLY => caster == target || caster.IsEnemy != target.IsEnemy,
                eSkillTargetType.FRIENDLY => caster.IsEnemy != target.IsEnemy,
                eSkillTargetType.SELF => caster != target,
                eSkillTargetType.NONE => caster == target,
                _ => true
            };
        }
        public static bool IsPassiveSelf(this SkillPassiveData passive)
        {
            if (passive == null)
                return false;

            if (eSkillTargetType.SELF == passive.TARGET)
                return true;

            return false;
        }
        public static bool IsPassiveContentSkip(this SkillPassiveData passive, eBattleType battleType)
        {
            if (passive == null)
                return false;

            if (passive.USE_CONTENTS == 1)
                return false;

            eContentType ct = eContentType.NONE;
            switch(battleType)
            {
                case eBattleType.ARENA:
                    ct = eContentType.ARENA;
                    break;
                case eBattleType.ChampionBattle:
                    ct = eContentType.CHAMPION;
                    break;
                case eBattleType.WORLD_BOSS:
                    ct = eContentType.WORLDBOSS;
                    break;
            }

            if ((passive.USE_CONTENTS & (int)ct) <= 0)
                return true;

            return false;
        }
        /// <summary> string 재화타입 판별 후 eGoodType 리턴 </summary>
        public static eGoodType GetGoodType(string resourceName)
        {
            string lowerName = resourceName.ToLower();

            switch (lowerName)
            {
                case "gold":
                    return eGoodType.GOLD;
                case "energy":
                    return eGoodType.ENERGY;
                case "gemstone" or "gemstonefree":
                    return eGoodType.GEMSTONE;
            }

            return eGoodType.NONE;
        }
        public static string GetResourceNameByLang(string resourceName, string resourcePath, string lang = "")
        {
            if (string.IsNullOrEmpty(lang))
                lang = LanguageData.LanguageFolder;

            string ret = resourcePath + "/" + lang + "/" + resourceName;
            return ret;
        }
        public static void OnEventBuff(SkillBuffEvent eventType, int ID, int Position, Transform buffParent, Dictionary<string, GameObject> buffListDic, GameObject buffObject)
        {
            if (ID != eventType.Tag || Position != eventType.Info.Target.Position)
                return;

            var key = StrBuilder(eventType.Tag, eventType.Info.Name);
            switch (eventType.Event)
            {
                case SkillBuffEvent.eSkillBuffEventEnum.regist:
                {
                    if (!buffListDic.ContainsKey(key))
                    {
                        GameObject newBuff = UnityEngine.Object.Instantiate(buffObject, buffParent);
                        newBuff.transform.localScale = Vector3.one;

                        newBuff.GetComponent<buffSlotFrame>().SetIcon(eventType.Info.Data, eventType.Info);
                        buffListDic.Add(key, newBuff);
                    }
                }
                break;
                case SkillBuffEvent.eSkillBuffEventEnum.delete:
                case SkillBuffEvent.eSkillBuffEventEnum.DELETE_PASSIVE:
                {
                    if (buffListDic.ContainsKey(key))
                    {
                        UnityEngine.Object.Destroy(buffListDic[key]);
                        buffListDic.Remove(key);
                    }
                }
                break;
                case SkillBuffEvent.eSkillBuffEventEnum.REGIST_PASSIVE:
                {
                    if (!buffListDic.ContainsKey(key))
                    {
                        GameObject newBuff = UnityEngine.Object.Instantiate(buffObject, buffParent);
                        newBuff.transform.localScale = Vector3.one;

                        newBuff.GetComponent<buffSlotFrame>().SetIcon(eventType.Info.Passive, eventType.Info);
                        buffListDic.Add(key, newBuff);
                    }
                }
                break;
                default: return;
            }
        }
        #endregion
        public static float Sinusoidal(float curTime, float cycle, float power = 1f)
        {
            var curPos = curTime * 6f / cycle;
            return power * Mathf.Sin(curPos);
        }
        public static bool IsTypeToLoop(eSpineAnimation anim)
        {
            return anim switch
            {
                eSpineAnimation.WALK => true,
                eSpineAnimation.IDLE => true,
                eSpineAnimation.WIN => true,
                eSpineAnimation.LOSE => true,
                _ => false
            };
        }
        public static bool IsAnimSkip(eSpineAnimation eAniState, eSpineAnimation anim)
        {
            switch (eAniState)
            {
                case eSpineAnimation.DEATH:
                    return true;
                case eSpineAnimation.A_CASTING:
                case eSpineAnimation.ATTACK:
                case eSpineAnimation.CASTING:
                case eSpineAnimation.SKILL:
                    return anim == eSpineAnimation.IDLE || anim == eSpineAnimation.WALK || anim == eSpineAnimation.HIT;
                case eSpineAnimation.IDLE:
                    return anim == eSpineAnimation.IDLE;
                case eSpineAnimation.HIT:
                    return anim == eSpineAnimation.IDLE;
                default:
                    break;
            }
            return false;
        }
        public static IEnumerator CallBackCoroutine(VoidDelegate callback)
        {
            if (callback != null)
                callback();

            yield break;
        }
        public static eGoodType ConvertStringToItemType(string type)
        {
            return type switch
            {
                "ITEM" => eGoodType.ITEM,
                "ENERGY" => eGoodType.ENERGY,
                "GOLD" => eGoodType.GOLD,
                "PVP_TICKET" => eGoodType.ARENA_TICKET,     //이거는 들어오면 안됨(미사용)
                "ARENA_TICKET" => eGoodType.ARENA_TICKET,
                "EQUIPMENT" => eGoodType.EQUIPMENT,
                "EQUIP" => eGoodType.EQUIPMENT,
                "GEMSTONE" => eGoodType.GEMSTONE,
                "GEM" => eGoodType.GEMSTONE,                //이거는 들어오면 안됨(미사용)
                "ARENA_POINT" => eGoodType.ARENA_POINT,
                "MAGNET" => eGoodType.MAGNET,
                "MILEAGE" => eGoodType.MILEAGE,
                "DICE_GROUP" => eGoodType.DICE_GROUP,
                "CHAR" => eGoodType.CHARACTER,
                "GUILD_POINT" => eGoodType.GUILD_POINT,
                "GUILD_EXP" => eGoodType.GUILD_EXP,
                "AD_REMOVE" => eGoodType.AD_REMOVE,
                _ => eGoodType.ITEM
            };
        }
        public static string ConvertJobTypeToString(eJobType _type, bool _isLower = true)
        {
            return _isLower ? _type.ToString().ToLower() : _type.ToString().ToUpper();
        }
        public static string ErrorString(this eApiResCode type)
        {
            return type switch
            {
                eApiResCode.OK => "성공",
                eApiResCode.GENERIC_SERVER_FAIL => "[알 수 없음]",
                eApiResCode.SQL_ERROR => "sql db 오류",
                eApiResCode.NETWORK_ERROR => "서버간 통신 오류",
                eApiResCode.SCRIPT_INCLUDE_FAIL => "서버 프로젝트 내부 오류",
                eApiResCode.REDIS_ERROR => "redis server error",
                eApiResCode.SERVER_BUSY => "특정 구간 타임아웃",
                eApiResCode.DATA_ERROR => "기획데이터 오류",
                eApiResCode.VERSION_ERROR => "버전미스매칭",
                eApiResCode.SESSIONID_NOT_MATCH => "세션아이디 오류",
                eApiResCode.PARAM_ERROR => "INVALID_PARAM",
                eApiResCode.INVENTORY_FULL => "인벤 가득참",
                eApiResCode.COST_SHORT => "비용 부족하여 진행 불가",
                eApiResCode.SERVICE_UNAVAILABLE => "서버 점검 등.",
                eApiResCode.SERVICE_UNAVAILABLE_TEMP => "서버 점검 등. 오픈 예정시각 안내 포함",
                eApiResCode.AUTH_FAILED => "JWT 인증 실패",
                eApiResCode.ACCOUNT_EXISTS => "계정 생성 요청시 : 이미 생성됨",
                eApiResCode.NICKNAME_DUPLICATES => "닉네임 중복되어 생성 불가",
                eApiResCode.INVALID_NICK_LENGTH => "닉네임 길이 부적절",
                eApiResCode.INVALID_NICK_CHAR => "닉네임에 사용 불가능한 문자 포함",
                eApiResCode.ACCOUNT_NOT_EXISTS => "로그인 요청시 : 계정 미생성 상태",
                eApiResCode.ID_STRING_TOO_SHORT => "ID 길이 짧음",
                eApiResCode.ID_STRING_TOO_LONG => "ID 길이 긺",
                eApiResCode.ID_STRING_INVALID_ENCODING => "ID 조건 미달성",
                eApiResCode.ID_STRING_INVALID_NUMERIC => "ID 조건 미달성",
                eApiResCode.ID_STRING_MIXED_LANGS => "ID 조건 미달성",
                eApiResCode.ID_STRING_FILTERED => "ID 적절하지 않은 단어 사용",
                eApiResCode.ID_DUPLICATES => "dev signup 호출에서 id 중복",
                eApiResCode.ALREADY_BUILT => "이미 건설하였음",
                eApiResCode.YOU_CANT_BUILD_THERE => "해당 장소에는 설치할 수 없음",
                eApiResCode.EXTERIOR_LEVEL_TOO_LOW => "외형 레벨 요구 조건 불충족",
                eApiResCode.CONSTRUCT_COST_NOT_MET => "설치/레벨업 비용 부족",
                eApiResCode.CONSTRUCTION_ONGOING => "현재 건설중임",
                eApiResCode.BUILDING_NOT_EXISTS => "건물이 존재하지 않음",
                eApiResCode.BUILDING_QUEUE_NOT_EMPTY => "현재 진행중인 작업이 있음",
                eApiResCode.BUILDING_SLOT_FULLY_EXPANDED => "건물의 생산슬롯이 이미 최대치",
                eApiResCode.BUILDING_NO_JOB_IS_RUNNING => "건물에서 현재 진행중인 작업이 없음",
                eApiResCode.BUILDING_LEVEL_FULL => "현재 최고 레벨",
                eApiResCode.BUILDING_STATE_NOT_MET => "해당 건물 상태가 eBuildingState::NORMAL이 아님",
                eApiResCode.INVALID_RECIPE_ID => "존재하지 않거나 해당 건물에 귀속되지 않은 레시피",
                eApiResCode.REQUIRED_LEVEL_NOT_MET => "건물 레벨 문제로 생산 불가",
                eApiResCode.PRODUCE_SLOT_FULL => "건물의 생산 슬롯이 가득참",
                eApiResCode.NOTHING_TO_HARVEST => "생산물 획득 시도했으나 완료된 작업이 없음",
                eApiResCode.INVENTORY_FULLY_EXPANDED => "이미 최대로 확장되었음",
                eApiResCode.NOT_ENOUGH_ITEM_TO_SELL => "판매 시도했으나 보유량 부족",
                eApiResCode.ITEM_IS_NOT_FOR_SALE => "판매할 수 없는 아이템",
                eApiResCode.LANDMARK_NOT_EXISTS => "랜드마크 정보가 없음",
                eApiResCode.DOZER_EMPTY => "도저에서 수확 시도했으나 보상 없음",
                eApiResCode.TRAVEL_CANNOT_START_NOW => "여행 출발할 수 있는 상태 아님",
                eApiResCode.TRAVEL_INVALID_WORLD => "대상지 파라미터 오류",
                eApiResCode.TRAVEL_NOT_ENOUGH_DRAGONS => "드래곤 부족",
                eApiResCode.TRAVEL_DRAGON_NOT_EXISTS => "존재하지 않는 드래곤",
                eApiResCode.TRAVEL_NOT_ENOUGH_ENERGY => "에너지 부족",
                eApiResCode.TRAVEL_NOT_FINISHED => "아직 귀환하지 않음",
                eApiResCode.TRAVEL_NOT_RUNNING => "여행 진행중이 아님",
                eApiResCode.SUBWAY_PLAT_LOCKED => "플랫폼이 잠김 상태",
                eApiResCode.SUBWAY_PLAT_NOT_BUILT => "플랫폼 건설 전",
                eApiResCode.SUBWAY_PLAT_BUILT => "플랫폼 이미 건설됨",
                eApiResCode.SUBWAY_PLAT_RUNNING => "납품 진행중",
                eApiResCode.SUBWAY_SLOT_FULL => "이미 슬롯 재료 가득참",
                eApiResCode.SUBWAY_INVALID_SLOT => "슬롯 넘버 오류",
                eApiResCode.SUBWAY_SLOT_NOT_FULL => "슬롯 가득 차지 않음",
                eApiResCode.TRAVEL_ADVENTURE_NOT_CLEAR => "여행 탐험지역 미클리어",
                eApiResCode.ADV_NO_SUCH_WORLD => "존재하지 않는 월드",
                eApiResCode.ADV_NO_SUCH_STAGE => "존재하지 않는 스테이지",
                eApiResCode.ADV_WORLD_LOCKED => "잠긴 월드",
                eApiResCode.ADV_STAGE_LOCKED => "잠긴 스테이지",
                eApiResCode.ADV_DECK_TOO_SMALL => "드래곤 참여 미달",
                eApiResCode.ADV_INVALID_DRAGON => "드래곤 조건 미달",
                eApiResCode.ADV_STAGE_NOT_RUNNING => "개방되지 않은 스테이지",
                eApiResCode.ADV_REWARD_CONDITION_NOT_MET => "보상 조건 미달",
                eApiResCode.ADV_ALREADY_REWARDED => "이미 수령한 보상",
                eApiResCode.DRA_NO_SUCH_DRAGON => "소지하지 않은 드래곤",
                eApiResCode.DRA_SKILL_LEVEL_MAX => "드래곤 스킬 최대치 도달",
                eApiResCode.DRA_MERGE_NO_SUCH_CARD => "합성 재료로 요청한 카드 미보유",
                eApiResCode.DRA_MERGE_INVALID_CARDS => "합성 재료로 요청한 카드 조합 불가",
                eApiResCode.QUEST_NO_SUCH_QUEST => "찾는 퀘스트가 없음",
                eApiResCode.QUEST_ALEADY_ACCEPT => "이미 수락한 퀘스트",
                eApiResCode.QUEST_ALEADY_ACCOMPLISH => "이미 완료한 퀘스트",
                eApiResCode.QUEST_UNDER_CONDITION => "조건 미달성",
                eApiResCode.TUTORIAL_ALEADY_ACCOMPLISH => "이미 완료한 튜토리얼",
                eApiResCode.PART_NOT_EXISTS => "올바른 장비가 아님",
                eApiResCode.PART_INVALID_LEVEL_TO_REINFORCE => "강화 요청시 대상 레벨이 부적절함",
                eApiResCode.PART_INVALID_MATERIAL_TO_MERGE => "합성 요청시 올바른 장비 재료가 아님",
                eApiResCode.PART_INVALID_GRADE_MATERIAL_TO_MERGE => "합성 요청시 올바른 장비 등급이 아님",
                eApiResCode.PART_INVALID_MATERIAL_COUNT_TO_MERGE => "합성 요청시 합성 최소 재료 요구 수량이 아님",
                eApiResCode.PART_INVALID_TAG_TO_DECOMPOUND => "분해 요청시 올바른 장비가 아님",
                eApiResCode.PART_FULL => "장비 최대수량 도달",
                eApiResCode.ARENA_DRAGON_NOT_EXISTS => "방어덱에 드래곤 장착 안됨",
                eApiResCode.ARENA_TICKET_REFILL_OVERCOUNT => "티켓 리필 횟수 초과",
                eApiResCode.ARENA_FIRST_STEP => "아레나 첫 입장",
                eApiResCode.ARENA_INVALID_MATCH_ID => "존재하지 않는 매치 id 전달됨",
                eApiResCode.ARENA_MATCH_ALREADY_DONE => "이미 진행한 매치 id로 전투 요청",
                eApiResCode.ARENA_TICKET_SHORT => "티켓 부족",
                eApiResCode.ARENA_INVAILD_SEASON => "시즌 정보 부적합",
                eApiResCode.ARENA_NO_PENDED_MATCH => "arena/result 호출했으나 진행중인 전투 없음",
                eApiResCode.PET_NO_SUCH_PET => "해당 tag 펫 없음",
                eApiResCode.PET_NO_SUCH_DRAGON => "해당 id 드래곤 없음",
                eApiResCode.PET_ALREADY_EQUIPPED => "펫 관련이지만 알 수 없음",
                eApiResCode.PET_NOT_EQUIPPED => "펫 관련이지만 알 수 없음",
                eApiResCode.PET_TOO_MUCH_YOU_HAVE => "펫 관련이지만 알 수 없음",
                eApiResCode.PET_NOT_EXIST => "없는 펫 태그",
                eApiResCode.INVAILD_PET_MATERIALS => "펫 경험치 재료 조건 미달성",
                eApiResCode.PET_FULL => "펫 최대수량 도달",
                eApiResCode.PET_INVALID_LEVEL_TO_REINFORCE => "펫 강화 요청시 대상 레벨이 부적절함",
                eApiResCode.POST_NO_SEARCH_MAIL => "없는 메일",
                eApiResCode.POST_USED_MAIL => "사용한(아이템 획득) 메일",
                eApiResCode.POST_ACCEPTABLE_FAIL => "우편 수령 실패",
                eApiResCode.POST_NO_DELETE_MAIL => "지운 우편이 없음",
                eApiResCode.DAILY_DATA_ERROR => "요일 던전 생성 오류",
                eApiResCode.DAILY_REFILL_OVERCOUNT => "요일 던전 리필 한도 초과",
                eApiResCode.DAILY_SWEEP_LOCKED => "요일 던전 소탕 조건 미달",
                eApiResCode.DAILY_DAY_NOT_MATCHED => "요일 조건 미달",
                eApiResCode.NOT_IMPLEMENTED_ERROR => "TODO 블럭을 완료해야",
                eApiResCode.WALLET_NOT_CONNECTED => "지갑이 연결되지 않았습니다",
                _ => StrBuilder("알 수 없는 오류 : ", type.ToString())
            };
        }
        public static void Quit()
        {
#if !UNITY_EDITOR
            Application.Quit();
#elif UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        public static string GetSkillName(this SkillEffectData data)
        {
            if (data.NEST_GROUP == 0)
                return "";
            else
                return SBFunc.StrBuilder(data.NEST_GROUP.ToString(), data.TYPE.ToString(), data.STAT_TYPE);
        }
        public static string GetPassiveName(this SkillPassiveData data)
        {
            if (data.NEST_GROUP == 0)
                return "";
            else
                return SBFunc.StrBuilder(data.NEST_GROUP.ToString(), data.PASSIVE_EFFECT.ToString(), data.STAT);
        }
        public static void SkillDataSetLoadingList(Dictionary<eResourcePath, List<string>> loadDatas, SkillCharData data)
        {
            if (data == null)
                return;

            if (data.CASTING_EFFECT_RSC_KEY > 0)
                AddedResourceKey(loadDatas, data.CASTING_EFFECT_RSC_KEY);

            var summon = SkillSummonData.Get(data.SUMMON_KEY);
            if (summon != null)
            {
                if (summon.ARROW_RSC_KEY > 0)
                    AddedResourceKey(loadDatas, summon.ARROW_RSC_KEY);
                if (summon.SKILL_EFFECT_RSC_KEY > 0)
                    AddedResourceKey(loadDatas, summon.SKILL_EFFECT_RSC_KEY);

                var effects = SkillEffectData.GetGroup(summon.EFFECT_GROUP_KEY);
                if (effects != null)
                {
                    for (int j = 0, jCount = effects.Count; j < jCount; j++)
                    {
                        var effect = effects[j];
                        if (effect == null)
                            continue;

                        if (effect.TARGET_EFFECT_RSC_KEY > 0)
                            AddedResourceKey(loadDatas, effect.TARGET_EFFECT_RSC_KEY);
                    }
                }
            }
        }
        public static void PassiveDataSetLoadingList(Dictionary<eResourcePath, List<string>> loadDatas, TranscendenceData transcendence)
        {
            if (null == transcendence)
                return;

            if (0 < transcendence.Step && 0 < transcendence.PassiveSlot)
            {
                for (int i = 1, count = transcendence.PassiveSlot; i <= count; ++i)
                {
                    var passive = transcendence.GetPassiveData(i);
                    if (passive == null)
                        continue;

                    AddedResourceKey(loadDatas, passive.SELF_EFFECT_RESOURCE);
                    AddedResourceKey(loadDatas, passive.TARGET_EFFECT_RESOURCE);
                }
            }
        }
        public static void AddedResourceKey(Dictionary<eResourcePath, List<string>> loadDatas, int resourceKey)
        {
            var rData = SkillResourceData.Get(resourceKey);
            if (rData != null)
            {
                AddedResourceKey(loadDatas, eResourcePath.EffectPrefabPath, rData.FILE);
                AddedResourceKey(loadDatas, eResourcePath.ProjectileSpritePath, rData.IMAGE);
            }
        }
        public static void AddedResourceKey(Dictionary<eResourcePath, List<string>> loadDatas, eResourcePath ePath, string name)
        {
            switch (ePath)
            {
                case eResourcePath.None:
                case eResourcePath.Max:
                    return;
                default:
                    break;
            }

            if (name != "NONE")
            {
                if (false == loadDatas.TryGetValue(ePath, out var list))
                {
                    list = new();
                    loadDatas.Add(ePath, list);
                }

                if (false == list.Contains(name))
                    list.Add(name);
            }
        }
        // 거지같은 요구사항과 코에걸면 코걸이 귀에걸면 귀걸이 기획.
        // 기분따라서 바뀌는 것 같은 규칙때문에 최대한 절차지향적으로 코딩
        // 구라가 아니라 기획 3줄임 
        static public void SetAutoBattleLine(BattleLine line, List<UserDragon> dragonList = null)
        {
            if (line == null)
                return;

            line.Clear();

            if (dragonList == null)
            {
                dragonList = User.Instance.DragonData.GetAllUserDragons();
            }

            List<UserDragon>[] dragons = new List<UserDragon>[3] {
                new List<UserDragon>(),
                new List<UserDragon>(),
                new List<UserDragon>()
            };

            foreach (UserDragon dragon in dragonList)
            {
                switch ((eJobType)dragon.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        dragons[0].Add(dragon);
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        dragons[1].Add(dragon);
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        dragons[2].Add(dragon);
                        break;
                }
            }

            foreach (List<UserDragon> list in dragons)
            {
                list.Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
            }

            Dictionary<Vector2Int, UserDragon> DragonLine = new Dictionary<Vector2Int, UserDragon>();
            var target = GetTopDragon(ref dragons, 0);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(0, 0), target);
                ExceptDragon(target, ref dragons);
            }
            target = GetTopDragon(ref dragons, 0);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(0, 1), target);
                ExceptDragon(target, ref dragons);
            }

            target = GetTopDragon(ref dragons, 1);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(1, 0), target);
                ExceptDragon(target, ref dragons);
            }
            target = GetTopDragon(ref dragons, 1);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(1, 1), target);
                ExceptDragon(target, ref dragons);
            }

            target = GetTopDragon(ref dragons, 2);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(2, 0), target);
                ExceptDragon(target, ref dragons);
            }
            target = GetTopDragon(ref dragons, 2);
            if (target != null)
            {
                DragonLine.Add(new Vector2Int(2, 1), target);
                ExceptDragon(target, ref dragons);
            }

            if (DragonLine.Count > 5)
            {
                Vector2Int weakness = new Vector2Int(-1, -1);
                foreach (var cand in DragonLine)
                {
                    if (DragonLine.ContainsKey(weakness))
                    {
                        if (DragonLine[weakness].GetTotalINF() > cand.Value.GetTotalINF())
                            weakness = cand.Key;
                    }
                    else
                    {
                        weakness = cand.Key;
                    }
                }

                DragonLine.Remove(weakness);
            }
            else if (DragonLine.Count < 5)
            {
                Dictionary<Vector2Int, UserDragon> empty = new Dictionary<Vector2Int, UserDragon>();
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        UserDragon candi = null;
                        switch (x)
                        {
                            case 0:
                                candi = GetTopDragon(ref dragons, 1);
                                if (candi == null)
                                    candi = GetTopDragon(ref dragons, 2);
                                break;
                            case 1:
                                var f = GetTopDragon(ref dragons, 0);
                                var b = GetTopDragon(ref dragons, 2);
                                if (f == null)
                                {
                                    candi = b;
                                    break;
                                }
                                if (b == null)
                                {
                                    candi = f;
                                    break;
                                }

                                if (f.GetTotalINF() < b.GetTotalINF())
                                    candi = b;
                                else
                                    candi = f;
                                break;
                            case 2:
                                candi = GetTopDragon(ref dragons, 1);
                                if (candi == null)
                                    candi = GetTopDragon(ref dragons, 0);
                                break;
                        }

                        if (candi == null)
                            continue;

                        var key = new Vector2Int(x, y);
                        if (DragonLine.ContainsKey(key))
                            continue;

                        empty.Add(key, candi);
                        ExceptDragon(candi, ref dragons);
                    }
                }

                while (DragonLine.Count + empty.Count > 5)
                {
                    Vector2Int weakness = new Vector2Int(-1, -1);
                    foreach (var cand in empty)
                    {
                        if (empty.ContainsKey(weakness))
                        {
                            if (empty[weakness].GetTotalINF() > cand.Value.GetTotalINF())
                                weakness = cand.Key;
                        }
                        else
                        {
                            weakness = cand.Key;
                        }
                    }
                    empty.Remove(weakness);
                }

                foreach (var cand in empty)
                {
                    DragonLine.Add(cand.Key, cand.Value);
                }
            }

            Dictionary<int, List<UserDragon>> positions = new Dictionary<int, List<UserDragon>>();
            for (int i = 0; i < 3; i++)
            {
                positions.Add(i, new List<UserDragon>());
            }

            foreach (var ca in DragonLine.Values)
            {
                int index = -1;
                switch ((eJobType)ca.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        index = 0;
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        index = 1;
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        index = 2;
                        break;
                }

                if (index < 0)
                    continue;

                positions[index].Add(ca);
            }

            List<UserDragon> migration = new List<UserDragon>();
            for (int i = 0; i < 3; i++)
            {
                positions[i].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                while (positions[i].Count > 2)
                {
                    var weakness = positions[i][positions[i].Count - 1];
                    positions[i].Remove(weakness);
                    migration.Add(weakness);
                }
            }

            int repeat = 5;
            while (migration.Count > 0 && repeat > 0)
            {
                repeat--;

                var cur = migration[0];
                migration.Remove(cur);

                switch ((eJobType)cur.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        if (positions[1].Count < 2)
                        {
                            positions[1].Add(cur);
                        }
                        else
                        {
                            positions[1].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                            var weakness = positions[1][positions[1].Count - 1];
                            positions[1].Remove(weakness);
                            migration.Add(weakness);

                            positions[1].Add(cur);
                        }
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        if (positions[2].Count < 2)
                        {
                            positions[2].Add(cur);
                        }
                        else if (positions[0].Count < 2)
                        {
                            positions[0].Add(cur);
                        }
                        else
                        {
                            //일로타면 안됨..무한룹가능성
                            Debug.LogError("로직이 삐꾸네");
                        }
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        if (positions[1].Count < 2)
                        {
                            positions[1].Add(cur);
                        }
                        else
                        {
                            positions[1].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                            var weakness = positions[1][positions[1].Count - 1];
                            positions[1].Remove(weakness);
                            migration.Add(weakness);

                            positions[1].Add(cur);
                        }
                        break;
                }
            }
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    if (positions[x].Count > y)
                        line.AddDragonPosition(x, y, positions[x][y].Tag);
                }
            }
        }

        static public void SetAutoBattleLineWithCap(BattleLine line, List<UserDragon> dragonList, int cap)
        {
            if (line == null)
                return;

            line.Clear();

            if (dragonList == null)
            {
                dragonList = User.Instance.DragonData.GetAllUserDragons();
            }

            List<UserDragon>[] dragons = new List<UserDragon>[3] {
                new List<UserDragon>(),
                new List<UserDragon>(),
                new List<UserDragon>()
            };

            foreach (UserDragon dragon in dragonList)
            {
                switch ((eJobType)dragon.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        dragons[0].Add(dragon);
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        dragons[1].Add(dragon);
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        dragons[2].Add(dragon);
                        break;
                }
            }

            foreach (List<UserDragon> list in dragons)
            {
                list.Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
            }

            Dictionary<Vector2Int, UserDragon> DragonLine = new Dictionary<Vector2Int, UserDragon>();

            
            var target = GetTopDragon(ref dragons, 0);
            if(target != null)
                ExceptDragon(target, ref dragons);

            while (target != null && target.GetTotalINF() > cap)
            {
                target = GetTopDragon(ref dragons, 0);
                if (target != null)
                    ExceptDragon(target, ref dragons);
            }

            if (target != null)
            {
                cap -= target.GetTotalINF();
                DragonLine.Add(new Vector2Int(0, 0), target);                
            }

            target = GetTopDragon(ref dragons, 0);
            if (target != null)
                ExceptDragon(target, ref dragons);

            while (target != null && target.GetTotalINF() > cap)
            {
                target = GetTopDragon(ref dragons, 0);
                if (target != null)
                    ExceptDragon(target, ref dragons);
            }

            if (target != null)
            {
                cap -= target.GetTotalINF();
                DragonLine.Add(new Vector2Int(0, 1), target);
            }

            target = GetTopDragon(ref dragons, 1);
            if (target != null)
                ExceptDragon(target, ref dragons);

            while (target != null && target.GetTotalINF() > cap)
            {
                target = GetTopDragon(ref dragons, 1);
                if (target != null)
                    ExceptDragon(target, ref dragons);
            }

            if (target != null)
            {
                cap -= target.GetTotalINF();
                DragonLine.Add(new Vector2Int(1, 0), target);
            }
            target = GetTopDragon(ref dragons, 1);
            if (target != null)
                ExceptDragon(target, ref dragons);

            while (target != null && target.GetTotalINF() > cap)
            {
                target = GetTopDragon(ref dragons, 1);
                if (target != null)
                    ExceptDragon(target, ref dragons);
            }

            if (target != null)
            {
                cap -= target.GetTotalINF();
                DragonLine.Add(new Vector2Int(1, 1), target);
            }

            target = GetTopDragon(ref dragons, 2);
            if (target != null)
                ExceptDragon(target, ref dragons);
            while (target != null && target.GetTotalINF() > cap)
            {
                target = GetTopDragon(ref dragons, 2);
                if (target != null)
                    ExceptDragon(target, ref dragons);
            }

            if (target != null)
            {
                cap -= target.GetTotalINF();
                DragonLine.Add(new Vector2Int(2, 0), target);
            }
            target = GetTopDragon(ref dragons, 2);
            if (target != null)
                ExceptDragon(target, ref dragons);

            while (target != null && target.GetTotalINF() > cap)
            {
                target = GetTopDragon(ref dragons, 2);
                if (target != null)
                    ExceptDragon(target, ref dragons);

            }
            if (target != null)
            {
                cap -= target.GetTotalINF();
                DragonLine.Add(new Vector2Int(2, 1), target);
            }

            if (DragonLine.Count > 5)
            {
                Vector2Int weakness = new Vector2Int(-1, -1);
                foreach (var cand in DragonLine)
                {
                    if (DragonLine.ContainsKey(weakness))
                    {
                        if (DragonLine[weakness].GetTotalINF() > cand.Value.GetTotalINF())
                            weakness = cand.Key;
                    }
                    else
                    {
                        weakness = cand.Key;
                    }
                }

                DragonLine.Remove(weakness);
            }
            else if (DragonLine.Count < 5)
            {
                //예외 제거
            }

            Dictionary<int, List<UserDragon>> positions = new Dictionary<int, List<UserDragon>>();
            for (int i = 0; i < 3; i++)
            {
                positions.Add(i, new List<UserDragon>());
            }

            foreach (var ca in DragonLine.Values)
            {
                int index = -1;
                switch ((eJobType)ca.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        index = 0;
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        index = 1;
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        index = 2;
                        break;
                }

                if (index < 0)
                    continue;

                positions[index].Add(ca);
            }

            List<UserDragon> migration = new List<UserDragon>();
            for (int i = 0; i < 3; i++)
            {
                positions[i].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                while (positions[i].Count > 2)
                {
                    var weakness = positions[i][positions[i].Count - 1];
                    positions[i].Remove(weakness);
                    migration.Add(weakness);
                }
            }

            int repeat = 5;
            while (migration.Count > 0 && repeat > 0)
            {
                repeat--;

                var cur = migration[0];
                migration.Remove(cur);

                switch ((eJobType)cur.JOB())
                {
                    case eJobType.TANKER:
                    case eJobType.WARRIOR:
                        if (positions[1].Count < 2)
                        {
                            positions[1].Add(cur);
                        }
                        else
                        {
                            positions[1].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                            var weakness = positions[1][positions[1].Count - 1];
                            positions[1].Remove(weakness);
                            migration.Add(weakness);

                            positions[1].Add(cur);
                        }
                        break;
                    case eJobType.ASSASSIN:
                    case eJobType.BOMBER:
                        if (positions[2].Count < 2)
                        {
                            positions[2].Add(cur);
                        }
                        else if (positions[0].Count < 2)
                        {
                            positions[0].Add(cur);
                        }
                        else
                        {
                            //일로타면 안됨..무한룹가능성
                            Debug.LogError("로직이 삐꾸네");
                        }
                        break;
                    case eJobType.SNIPER:
                    case eJobType.SUPPORTER:
                        if (positions[1].Count < 2)
                        {
                            positions[1].Add(cur);
                        }
                        else
                        {
                            positions[1].Sort((a, b) => { return b.GetTotalINF().CompareTo(a.GetTotalINF()); });
                            var weakness = positions[1][positions[1].Count - 1];
                            positions[1].Remove(weakness);
                            migration.Add(weakness);

                            positions[1].Add(cur);
                        }
                        break;
                }
            }
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    if (positions[x].Count > y)
                        line.AddDragonPosition(x, y, positions[x][y].Tag);
                }
            }
        }

        static void PushDragonPosition(BattleLine line, int target, int line_index, int pos_index)
        {
            var tmp = line.GetDragon(line_index, pos_index);
            line.DeleteDragon(target);
            line.AddDragonPosition(line_index, pos_index, target);

            if (tmp > 0)
            {
                if (pos_index == 0)
                    pos_index = 1;
                else
                {
                    line_index += 1;
                    pos_index = 0;
                }

                PushDragonPosition(line, tmp, line_index, pos_index);
            }
        }
        public static void SetUIDataAsset(UIDragonSpine spine, UIDragonSpine spine1, UIDragonSpine spine2, UIDragonSpine spine3)
        {
            SetDataAsset(spine, "metatoy_1");
            SetDataAsset(spine1, "metatoy_2");
            SetDataAsset(spine2, "legendary");
            SetDataAsset(spine3, "legendary_2");
        }
        public static void SetDataAsset(UIDragonSpine spine, string name)
        {
            if (spine == null || spine.SkeletonAni == null)
                return;

            spine.SkeletonAni.skeletonDataAsset = CharBaseData.GetSkeletonDataAsset(name);
        }
        static void ExceptDragon(UserDragon target, ref List<UserDragon>[] dragons)
        {
            int index = -1;
            switch ((eJobType)target.JOB())
            {
                case eJobType.TANKER:
                case eJobType.WARRIOR:
                    index = 0;
                    break;
                case eJobType.ASSASSIN:
                case eJobType.BOMBER:
                    index = 1;
                    break;
                case eJobType.SNIPER:
                case eJobType.SUPPORTER:
                    index = 2;
                    break;
            }
            if (index < 0)
                return;

            if (dragons[index].Count > 0 && dragons[index].Contains(target))
            {
                dragons[index].Remove(target);
            }
        }
        static UserDragon GetTopDragon(ref List<UserDragon>[] dragons, int index)
        {
            UserDragon ret = null;

            if (dragons[index].Count > 0)
                ret = dragons[index][0];

            return ret;
        }
        /// <summary> '하루동안 보이지 않기' 시간 체크 로직 통합 </summary>
        public static void SetTimeValue(string checkStr)
        {
            if (CacheUserData.GetString(checkStr) != "")
            {
                CacheUserData.DeleteKey(checkStr);//기존값 삭제하고 새로운 값 갱신
            }

            DateTime nextDay = DateTime.Today.AddDays(1);
            DateTime goalTime = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);//다음날로 설정

            string prefsData = goalTime.ToString("yyyy-MM-dd HH:mm:ss");
            CacheUserData.SetString(checkStr, prefsData);
        }
        /// <summary> 현재 유저데이터에 값을 들고있는지 </summary>
        public static bool HasTimeValue(string checkStr)
        {
            string goalTimeStr = CacheUserData.GetString(checkStr);
            if (goalTimeStr != "")
            {
                DateTime curTime = DateTime.Now;
                DateTime goalTime = DateTimeParse(goalTimeStr);

                TimeSpan timeDiff = curTime - goalTime;

                // 목표 시간을 지나면 다시 팝업 띄워야함
                if (timeDiff.TotalSeconds > 0)
                {
                    return false;
                }
                // 아직 목표시간에 도달하지 않음
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        public static string Replace(string str)
        {
            return str.Replace("\\n", "\n").Replace('^', ',').Replace('$', '\"').Replace(';', '\'');
        }
        public static DateTime DateTimeParse(string _dateTime)
        {
            if (string.IsNullOrEmpty(_dateTime))
                return DateTime.MinValue;

            return DateTime.Parse(_dateTime, GamePreference.Instance.Culture);
        }
        #region ConvertEnum
        public static eSkillEffectType ConvertSkillEffectType(string strType)
        {
            return strType switch
            {
                "NORMAL_DMG" => eSkillEffectType.NORMAL_DMG,
                "SKILL_DMG" => eSkillEffectType.SKILL_DMG,
                "DMG" => eSkillEffectType.DMG,
                "SKILL_ELEMENT_DMG" => eSkillEffectType.SKILL_ELEMENT_DMG,
                "SKILL_CRI_DMG" => eSkillEffectType.SKILL_CRI_DMG,
                "BUFF" => eSkillEffectType.BUFF,
                "BUFF_MAIN_ELEMENT" => eSkillEffectType.BUFF_MAIN_ELEMENT,
                "DEBUFF" => eSkillEffectType.DEBUFF,
                "STUN" => eSkillEffectType.STUN,
                "FROZEN" => eSkillEffectType.FROZEN,
                "AIRBORNE" => eSkillEffectType.AIRBORNE,
                "INVINCIBILITY" => eSkillEffectType.IMMUNE_DMG,
                "IMMUNE_DMG" => eSkillEffectType.IMMUNE_DMG,
                "IMMUNE_HARM" => eSkillEffectType.IMMUNE_HARM,
                "SILENCE" => eSkillEffectType.SILENCE,
                "TICK_DMG" => eSkillEffectType.TICK_DMG,
                "POISON" => eSkillEffectType.POISON,
                "KNOCK_BACK" => eSkillEffectType.KNOCK_BACK,
                "HEAL" => eSkillEffectType.HEAL,
                "SHIELD" => eSkillEffectType.SHIELD,
                "AGGRO" => eSkillEffectType.AGGRO,
                "AGGRO_R" => eSkillEffectType.AGGRO_R,
                "DOT" => eSkillEffectType.DOT,
                "PULL" => eSkillEffectType.PULL,
                "STAT" => eSkillEffectType.STAT,
                "D_BUFF" => eSkillEffectType.D_BUFF,
                "D_ABUFF" => eSkillEffectType.D_ABUFF,
                "D_DEBUFF" => eSkillEffectType.D_DEBUFF,
                "D_ADEBUFF" => eSkillEffectType.D_ADEBUFF,
                "D_DOT" => eSkillEffectType.D_DOT,
                "D_SHIELD" => eSkillEffectType.D_SHIELD,
                "D_STUN" => eSkillEffectType.D_STUN,
                "D_AGGRO" => eSkillEffectType.D_AGGRO,
                "ENV_BUFF" => eSkillEffectType.ENV_BUFF,
                "TRIGGER" => eSkillEffectType.TRIGGER,
                "EFFECT" => eSkillEffectType.EFFECT,
                "IMN_STUN" => eSkillEffectType.IMN_STUN,
                "IMN_AGGRO" => eSkillEffectType.IMN_AGGRO,
                "IMN_AIR" => eSkillEffectType.IMN_AIR,
                "IMN_KNOCK" => eSkillEffectType.IMN_KNOCK,
                "IMN_PULL" => eSkillEffectType.IMN_PULL,
                "IMN_CC" => eSkillEffectType.IMN_CC,
                _ => eSkillEffectType.NONE
            };
        }
        public static eStatusType ConvertStatusType(string strType)
        {
            return strType.ToUpper() switch
            {
                "ATK" => eStatusType.ATK,
                "DEF" => eStatusType.DEF,
                "HP" => eStatusType.HP,
                "ADD_ATK_DMG" => eStatusType.ADD_ATK_DMG,
                "RATIO_ATK_DMG" => eStatusType.RATIO_ATK_DMG,
                "CRI_PROC" => eStatusType.CRI_PROC,
                "CRI_DMG" => eStatusType.CRI_DMG,
                "RATIO_SKILL_DMG" => eStatusType.RATIO_SKILL_DMG,
                "ADD_SKILL_DMG" => eStatusType.ADD_SKILL_DMG,
                "ALL_ELEMENT_DMG" => eStatusType.ALL_ELEMENT_DMG,
                "LIGHT_DMG" => eStatusType.LIGHT_DMG,
                "DARK_DMG" => eStatusType.DARK_DMG,
                "WATER_DMG" => eStatusType.WATER_DMG,
                "FIRE_DMG" => eStatusType.FIRE_DMG,
                "WIND_DMG" => eStatusType.WIND_DMG,
                "EARTH_DMG" => eStatusType.EARTH_DMG,
                "CRI_RESIS" => eStatusType.CRI_RESIS,
                "CRI_DMG_RESIS" => eStatusType.CRI_DMG_RESIS,
                "SKILL_DMG_RESIS" => eStatusType.SKILL_DMG_RESIS,
                "ADD_BUFF_TIME" => eStatusType.ADD_BUFF_TIME,
                "DEL_BUFF_TIME" => eStatusType.DEL_BUFF_TIME,
                "ATK_DMG_RESIS" => eStatusType.ATK_DMG_RESIS,
                "ALL_ELEMENT_DMG_RESIS" => eStatusType.ALL_ELEMENT_DMG_RESIS,
                "LIGHT_DMG_RESIS" => eStatusType.LIGHT_DMG_RESIS,
                "DARK_DMG_RESIS" => eStatusType.DARK_DMG_RESIS,
                "WATER_DMG_RESIS" => eStatusType.WATER_DMG_RESIS,
                "FIRE_DMG_RESIS" => eStatusType.FIRE_DMG_RESIS,
                "WIND_DMG_RESIS" => eStatusType.WIND_DMG_RESIS,
                "EARTH_DMG_RESIS" => eStatusType.EARTH_DMG_RESIS,
                "ADD_PVP_DMG" => eStatusType.ADD_PVP_DMG,
                "RATIO_PVP_DMG" => eStatusType.RATIO_PVP_DMG,
                "ADD_PVP_CRI_DMG" => eStatusType.ADD_PVP_CRI_DMG,
                "RATIO_PVP_CRI_DMG" => eStatusType.RATIO_PVP_CRI_DMG,
                "DEL_COOLTIME" => eStatusType.DEL_COOLTIME,
                "ADD_ATKSPEED" => eStatusType.ADD_ATKSPEED,
                "SHIELD_POINT" => eStatusType.SHIELD_POINT,
                "BOSS_DMG" => eStatusType.BOSS_DMG,
                "BOSS_DMG_RESIS" => eStatusType.BOSS_DMG_RESIS,
                "ALL_DMG_RESIS" => eStatusType.ALL_DMG_RESIS,
                "COOLTIME_SPEED" => eStatusType.COOLTIME_SPEED,
                "SHIELD_BREAKER" => eStatusType.SHIELD_BREAKER,
                "VAMP" => eStatusType.VAMP,
                "PHYS_DMG_RESIS" => eStatusType.PHYS_DMG_RESIS,
                "PHYS_DMG_PIERCE" => eStatusType.PHYS_DMG_PIERCE, // 물리 데미지 관통
                "CRI_DMG_PIERCE" => eStatusType.CRI_DMG_PIERCE, // 치명 대미지 관통
                "ALL_ELEMENT_DMG_PIERCE" => eStatusType.ALL_ELEMENT_DMG_PIERCE, // 모든 속성 관통
                "LIGHT_DMG_PIERCE" => eStatusType.LIGHT_DMG_PIERCE, // 빛속성대미지 관통
                "DARK_DMG_PIERCE" => eStatusType.DARK_DMG_PIERCE, // 어둠속성대미지 관통
                "WATER_DMG_PIERCE" => eStatusType.WATER_DMG_PIERCE, // 물속성대미지 관통
                "FIRE_DMG_PIERCE" => eStatusType.FIRE_DMG_PIERCE, // 불속성대미지 관통
                "WIND_DMG_PIERCE" => eStatusType.WIND_DMG_PIERCE, // 바람속성대미지 관통
                "EARTH_DMG_PIERCE" => eStatusType.EARTH_DMG_PIERCE, // 땅속성대미지 관통
                "DEL_START_COOLTIME" => eStatusType.DEL_START_COOLTIME, // 스킬 시작쿨 감소
                "RATIO_PASSIVE_EFFECT" => eStatusType.RATIO_PASSIVE_EFFECT, // 패시브 효과 증폭
                "RATIO_PASSIVE_RATE" => eStatusType.RATIO_PASSIVE_RATE, // 패시브 확률 증폭
                "DEL_KNOCKBACK_DISTANCE" => eStatusType.DEL_KNOCKBACK_DISTANCE, // 넉백 거리 감소
                "RATIO_BOSS_DMG" => eStatusType.RATIO_BOSS_DMG, // 보스 대미지 증폭
                "ADD_MAIN_ELEMENT_DMG" => eStatusType.ADD_MAIN_ELEMENT_DMG, // 자기 속댐 증가
                "ACCURACY" => eStatusType.ACCURACY, // 명중률
                "EVASION" => eStatusType.EVASION, // 회피율
                _ => eStatusType.NONE
            };
        }
        public static eStatusValueType ConvertValueType(string strType)
        {
            return strType.ToUpper() switch
            {
                "ADD_VALUE" => eStatusValueType.ADD_VALUE,
                "PERCENT" => eStatusValueType.PERCENT,
                _ => eStatusValueType.VALUE
            };
        }
        public static eSkillTarget ConvertSkillTarget(string strType)
        {
            return strType switch
            {
                "TARGET" => eSkillTarget.TARGET,
                "CENTER" => eSkillTarget.CENTER,
                _ => eSkillTarget.NONE
            };
        }
        public static eSkillTargetType ConvertSkillTargetType(string strType)
        {
            return strType switch
            {
                "ENEMY" => eSkillTargetType.ENEMY,
                "SELF" => eSkillTargetType.SELF,
                "CASTER" => eSkillTargetType.SELF,
                "ALLY" => eSkillTargetType.ALLY,
                "FRIEND" => eSkillTargetType.FRIENDLY,
                "FRIENDLY" => eSkillTargetType.FRIENDLY,
                "ALL" => eSkillTargetType.ALL,
                _ => eSkillTargetType.NONE
            };
        }
        public static eSkillTargetSort ConvertSkillTargetSort(string strType)
        {
            return strType switch
            {
                "NEARBY" => eSkillTargetSort.NEARBY,
                "FAR" => eSkillTargetSort.FAR,
                "FHP_HIGH" => eSkillTargetSort.FHP_HIGH,
                "FHP_LOW" => eSkillTargetSort.FHP_LOW,
                "HP_HIGH" => eSkillTargetSort.HP_HIGH,
                "HP_LOW" => eSkillTargetSort.HP_LOW,
                "ATK_HIGH" => eSkillTargetSort.ATK_HIGH,
                "ATK_LOW" => eSkillTargetSort.ATK_LOW,
                "DEF_HIGH" => eSkillTargetSort.DEF_HIGH,
                "DEF_LOW" => eSkillTargetSort.DEF_LOW,

                "LIGHT_DMG_HIGH" => eSkillTargetSort.LIGHT_DMG_HIGH,
                "LIGHT_DMG_LOW" => eSkillTargetSort.LIGHT_DMG_LOW,
                "DARK_DMG_HIGH" => eSkillTargetSort.DARK_DMG_HIGH,
                "DARK_DMG_LOW" => eSkillTargetSort.DARK_DMG_LOW,
                "WATER_DMG_HIGH" => eSkillTargetSort.WATER_DMG_HIGH,
                "WATER_DMG_LOW" => eSkillTargetSort.WATER_DMG_LOW,
                "FIRE_DMG_HIGH" => eSkillTargetSort.FIRE_DMG_HIGH,
                "FIRE_DMG_LOW" => eSkillTargetSort.FIRE_DMG_LOW,
                "WIND_DMG_HIGH" => eSkillTargetSort.WIND_DMG_HIGH,
                "WIND_DMG_LOW" => eSkillTargetSort.WIND_DMG_LOW,
                "EARTH_DMG_HIGH" => eSkillTargetSort.EARTH_DMG_HIGH,
                "EARTH_DMG_LOW" => eSkillTargetSort.EARTH_DMG_LOW,

                "CRI_PROC_HIGH" => eSkillTargetSort.CRI_PROC_HIGH,
                "CRI_PROC_LOW" => eSkillTargetSort.CRI_PROC_LOW,

                "CRI_DMG_HIGH" => eSkillTargetSort.CRI_DMG_HIGH,
                "CRI_DMG_LOW" => eSkillTargetSort.CRI_DMG_LOW,

                _ => eSkillTargetSort.NONE
            };
        }
        public static eSkillCharCondition ConvertSkillCondition(string strType)
        {
            return strType switch
            {
                "COOL_TIME" => eSkillCharCondition.COOL_TIME,
                "HP_LOW" => eSkillCharCondition.HP_LOW,
                "FRIEND_DIE" => eSkillCharCondition.FRIEND_DIE,
                _ => eSkillCharCondition.NONE
            };
        }
        public static eSkillSummonType ConvertSkillSummonType(string str)
        {
            return str switch
            {
                "MELEE" => eSkillSummonType.IMMEDIATELY,
                "IMMEDIATELY" => eSkillSummonType.IMMEDIATELY,
                "ARROW" => eSkillSummonType.ARROW,
                "PIERCE" => eSkillSummonType.PIERCE,
                "RAPID" => eSkillSummonType.RAPID_R,
                "RAPID_R" => eSkillSummonType.RAPID_R,
                "LAND" => eSkillSummonType.LAND,
                "BACKSTAB" => eSkillSummonType.BACKSTAB,
                "SUMMON" => eSkillSummonType.SUMMON,
                "SPECIAL" => eSkillSummonType.SPECIAL,
                "CHARGE" => eSkillSummonType.CHARGE,

                _ => eSkillSummonType.NONE
            };
        }
        public static eSkillRangeType ConvertSkillRangeType(string str)
        {
            return str switch
            {
                "CIRCLE" => eSkillRangeType.CIRCLE_C,
                "CIRCLE_C" => eSkillRangeType.CIRCLE_C,
                "CIRCLE_F" => eSkillRangeType.CIRCLE_F,
                "SQUARE" => eSkillRangeType.SQUARE_C,
                "SQUARE_C" => eSkillRangeType.SQUARE_C,
                "SQUARE_F" => eSkillRangeType.SQUARE_F,
                "SECTOR_F" => eSkillRangeType.SECTOR_F,
                _ => eSkillRangeType.NONE
            };
        }
        public static eSkillTriggerType ConvertSkillTriggerType(string str)
        {
            return str switch
            {
                "NEXT" => eSkillTriggerType.NEXT,
                "DEAD" => eSkillTriggerType.DEAD,
                "END" => eSkillTriggerType.END,
                "HIT" => eSkillTriggerType.HIT,
                _ => eSkillTriggerType.NONE
            };
        }
        public static eSkillResourceOrder ConvertSkillResourceOrder(string str)
        {
            return str switch
            {
                "BACK" => eSkillResourceOrder.BACK,
                "FRONT" => eSkillResourceOrder.FRONT,
                _ => eSkillResourceOrder.AUTO
            };
        }
        public static eSkillResourceOrderType ConvertSkillResourceOrderType(string val)
        {
            return val switch
            {
                "CHAR" => eSkillResourceOrderType.CHAR,
                "WORLD" => eSkillResourceOrderType.WORLD,
                _ => eSkillResourceOrderType.NONE
            };
        }
        public static eSkillResourceLocation ConvertSkillResourceLocation(int val)
        {
            return val switch
            {
                1 => eSkillResourceLocation.COLLIDER,
                2 => eSkillResourceLocation.TOP,
                _ => eSkillResourceLocation.BOTTOM
            };
        }
        public static eSkillResourceFollow ConvertSkillResourceFollow(int val)
        {
            return val switch
            {
                1 => eSkillResourceFollow.FOLLOW,
                _ => eSkillResourceFollow.NONE
            };
        }
        public static eSkillResourceOrder ConvertSkillResourceOrder(int val)
        {
            return val switch
            {
                0 => eSkillResourceOrder.BACK,
                2 => eSkillResourceOrder.FRONT,
                _ => eSkillResourceOrder.AUTO
            };
        }
        public static float ConvertPos(this IBattleCharacterData data, float pos)
        {
            if (data == null)
                return pos;

            return data.IsEnemy ? -pos : pos;
        }
        #endregion
        #region Skill
        static public SkillResourceData GetEffectResource(this SkillSummonData data)
        {
            return SkillResourceData.Get(data.SKILL_EFFECT_RSC_KEY);
        }
        static public SkillResourceData GetArrowResource(this SkillSummonData data)
        {
            return SkillResourceData.Get(data.ARROW_RSC_KEY);
        }
        static public GameObject GetResourcePrefab(this SkillResourceData data)
        {
            return ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, data.FILE);
        }
        static public SkillLevelStat GetEffectStat(this SkillEffectData effect, int level)
        {
            if (effect == null)
                return null;

            var stat = new SkillLevelStat(effect);
            List<SkillLevelGroupData> levelGroup = SkillLevelGroupData.GetGroup(effect.GROW_GROUP_KEY);

            if (level > 1 && levelGroup != null)
            {
                int lastLevel = 0;
                for (int i = 0, count = levelGroup.Count; i < count; ++i)
                {
                    var curData = levelGroup[i];
                    if (curData == null)
                        continue;

                    var tempLevel = lastLevel == 0 ? 1 : lastLevel;//최초는 1 감소
                    var curLevelCount = curData.GROW_LEVEL;
                    if (curLevelCount >= level)
                    {
                        stat.AddedData(curData, level - tempLevel);
                        break;
                    }

                    curLevelCount -= tempLevel;//이전 계산한 값들이 있기 때문에 
                    stat.AddedData(curData, curLevelCount);
                    lastLevel = curLevelCount + tempLevel;//계산용 보정치
                }
            }

            return stat;
        }
        static List<string> GetSplitTypeComponent(string _originDesc)//DESC에 정의된 특문 기준으로 잘라서 배열화 -> skill Effect Data row의 키조합 str
        {
            string beforeWord = "#";
            string afterWord = "@";

            List<string> splitList = new List<string>();

            string[] splitDatas = _originDesc.Split(new string[] { beforeWord }, StringSplitOptions.None);
            foreach (string s in splitDatas)
            {
                var keyWordArr = s.Split(new string[] { afterWord }, StringSplitOptions.None);
                if (keyWordArr.Length > 1)
                    splitList.Add(keyWordArr[1]);
            }

            return splitList;
        }
        /// <summary>
        /// "@BUFF/ATK/PERCENT#" //@로 시작해서 # 으로 끝나는, 
        /// 기본 split 구분자 '/' 타입 + 스탯타입 + VALUE 타입의 조합값.
        /// </summary>
        /// <param name="_originDesc"></param>
        /// <param name="_list"></param>
        /// <returns></returns>
        public static string GetConvertStatStr(string _originDesc, List<SkillEffectData> _list, int _currentSlevel)
        {
            var splitList = GetSplitTypeComponent(_originDesc);//치환 해야하는 키값 - 이걸 들고 skillEffectData 에 해당하는 수치값 가져오기

            List<string> replaceList = new List<string>(); //치환 키워드 기준으로 desc 변경 값 생성하기
            foreach (var splitStr in splitList)
            {
                if (string.IsNullOrEmpty(splitStr))
                    continue;

                var splitData = splitStr.Split('/');

                if (splitData.Length <= 1)
                    continue;

                var type = ConvertSkillEffectType(splitData[0]);
                if (splitData.Length == 2)
                {
                    var findEffectData = _list.Find(element => element.TYPE == type);
                    if (findEffectData == null)
                        continue;

                    var levelStatus = findEffectData.GetEffectStat(_currentSlevel);
                    var value = levelStatus.GetStatByStr(splitData[1]);//split 값 그대로 씀

                    string key = splitStr;
                    switch (type)
                    {
                        case eSkillEffectType.PULL:
                            if (value > 50)
                                key += "_S";
                            else if (value > 100)
                                key += "_M";
                            else if (value > 150)
                                key += "_L";
                            break;
                        case eSkillEffectType.AIRBORNE:
                            if (value > 50)
                                key += "_S";
                            else if (value > 100)
                                key += "_M";
                            else if (value > 150)
                                key += "_L";
                            break;
                        case eSkillEffectType.KNOCK_BACK:
                            if (value > 50)
                                key += "_S";
                            else if (value > 100)
                                key += "_M";
                            else if (value > 150)
                                key += "_L";
                            break;
                    }

                    var originStr = StrBuilder("@", splitStr, "#");//이게 원본
                    var modifyStr = StrBuilder("@", key, "#");//이게 원본
                    var replaceStr = value <= 0f ? "" : StringData.GetStringFormatByStrKey(modifyStr, Math.Round(value, 2).ToString());

                    _originDesc = _originDesc.Replace(originStr, replaceStr);//string 교체
                }
                else if (splitData.Length == 3)//3개는 (Buff / Debuff/ Shield) -> stat 의 value값 고정
                {
                    var statType = ConvertStatusType(splitData[1]);
                    var valueType = ConvertValueType(splitData[2]);

                    var findEffectData = _list.Find(element => element.TYPE == type && element.STAT_TYPE == statType && valueType == element.VALUE_TYPE);
                    if (findEffectData != null)
                    {
                        var levelStatus = findEffectData.GetEffectStat(_currentSlevel);
                        if (levelStatus != null)
                        {
                            var value = levelStatus.VALUE;
                            var modifyStr = StrBuilder("@", splitStr, "#");//이게 원본
                            var replaceStr = value <= 0f ? "" : StringData.GetStringFormatByStrKey(modifyStr, Math.Round(value, 2).ToString());

                            _originDesc = _originDesc.Replace(modifyStr, replaceStr);//string 교체
                        }
                    }
                }
            }
            return _originDesc;
        }
        public static eElementType PositiveElement(this eElementType curType)
        {
            return curType switch
            {
                eElementType.FIRE => eElementType.WIND,
                eElementType.WATER => eElementType.FIRE,
                eElementType.EARTH => eElementType.WATER,
                eElementType.WIND => eElementType.EARTH,
                eElementType.LIGHT => eElementType.DARK,
                eElementType.DARK => eElementType.LIGHT,
                _ => eElementType.None
            };
        }
        public static eElementType AdverseElement(this eElementType curType)
        {
            return curType switch
            {
                eElementType.FIRE => eElementType.WATER,
                eElementType.WATER => eElementType.EARTH,
                eElementType.EARTH => eElementType.WIND,
                eElementType.WIND => eElementType.FIRE,
                eElementType.LIGHT => eElementType.DARK,
                eElementType.DARK => eElementType.LIGHT,
                _ => eElementType.None
            };
        }
        public static eStatusType StatDMG(this eElementType curType)
        {
            return curType switch
            {
                eElementType.FIRE => eStatusType.FIRE_DMG,
                eElementType.WATER => eStatusType.WATER_DMG,
                eElementType.EARTH => eStatusType.EARTH_DMG,
                eElementType.WIND => eStatusType.WIND_DMG,
                eElementType.LIGHT => eStatusType.LIGHT_DMG,
                eElementType.DARK => eStatusType.DARK_DMG,
                _ => eStatusType.NONE
            };
        }
        public static eElementType ConvertStatToElement(eStatusType curType)
        {
            return curType switch
            {
                eStatusType.FIRE_DMG => eElementType.FIRE,
                eStatusType.FIRE_DMG_RESIS => eElementType.FIRE,
                eStatusType.WATER_DMG => eElementType.WATER,
                eStatusType.WATER_DMG_RESIS => eElementType.WATER,
                eStatusType.EARTH_DMG => eElementType.EARTH,
                eStatusType.EARTH_DMG_RESIS => eElementType.EARTH,
                eStatusType.WIND_DMG => eElementType.WIND,
                eStatusType.WIND_DMG_RESIS => eElementType.WIND,
                eStatusType.DARK_DMG => eElementType.DARK,
                eStatusType.DARK_DMG_RESIS => eElementType.DARK,
                eStatusType.LIGHT_DMG => eElementType.LIGHT,
                eStatusType.LIGHT_DMG_RESIS => eElementType.LIGHT,
                _ => eElementType.None
            };
        }
        #endregion
        #region 바로가기 기능

        public const string LANDMARK_SUBWAY_SUBTYPE_KEY = "SUBWAY";
        public const string LANDMARK_DOZER_SUBTYPE_KEY = "DOZER";
        public const string LANDMARK_TRAVEL_SUBTYPE_KEY = "TRAVEL";
        public const string LANDMARK_REQUEST_SUBTYPE_KEY = "REQUEST_CENTER";

        public const string DUNGEON_SELECT_POPUP_NAME = "DungeonSelectPopup";
        public const string TOWN_MANAGE_POPUP_NAME = "TownManagePopup";

        public const string GACHA_SCENE_NAME = "Gacha";
        public const string ADVENTURE_SCENE_NAME = "AdventureStageSelect";
        public const string ARENA_LOBBY_SCENE_NAME = "ArenaLobby";
        public const string DAILY_DUNGEON_SCENE_NAME = "DailyDungeonLobby";
        public const string WORLD_BOSS_SCENE_NAME = "WorldBossLobby";

        public const string BATTERY_TYPE_ITEM_KEY = "exp_battery";

        public static bool IsTargetScene(string targetSceneName)
        {
            return SceneManager.GetActiveScene().name.CompareTo(targetSceneName) == 0;
        }

        /// <summary>
        /// Scene 이동 함수
        /// </summary>
        /// <param name="_delegate"> Scene Load 시 콜백 함수 </param>
        /// <param name="_sceneName"> 타겟 Scene 이름 </param>
        /// <param name="isCallbackCompleteLoaded"> 0:Scene 진입전 (구름애니메이션)에 호출, 1:Scene 진입 후 호출 </param>
        public static void MoveScene(VoidDelegate _delegate = null, string _sceneName = "Town", bool isCallbackCompleteLoaded = false)
        {
            bool isTownScene = IsTargetScene(_sceneName);
            if (!isTownScene)
            {
                PopupManager.AllClosePopup();//현재 팝업 전체 지우기 하고 나서 이동

                if (isCallbackCompleteLoaded == false)
                {
                    LoadingManager.Instance.EffectiveSceneLoad(_sceneName, eSceneEffectType.CloudAnimation,
                        CallBackCoroutine(() =>
                        {
                            if (_delegate != null)
                                _delegate();

                        }));
                }
                else
                {
                    LoadingManager.Instance.EffectiveSceneLoad(_sceneName, eSceneEffectType.CloudAnimation, null);

                    LoadingManager.Instance.SceneCallback = _delegate;
                }
            }
            else
            {
                Camera.main.targetTexture = null;
                Town.Instance.SetSubCamState(false);

                if (_delegate != null)
                {
                    PopupManager.AllClosePopup();//팝업 위에 팝업 띄워도 상관없는지 물어보고 결정
                    _delegate();
                }
            }
        }

        // 가챠 씬 이동 (groupMenuType 지정 시 해당탭이 켜진 상태로 Load)
        public static void MoveGachaScene(eGachaGroupMenu groupMenuType = eGachaGroupMenu.NONE)
        {
            PopupManager.AllClosePopup();//현재 팝업 전체 지우기 하고 나서 이동

            LoadingManager.Instance.EffectiveSceneLoad(GACHA_SCENE_NAME, eSceneEffectType.CloudAnimation, null);

            LoadingManager.Instance.SceneCallback = () =>
            {
                GachaEvent.SetGachaTab(groupMenuType);
            };
        }

        public static void MoveWorldBossScene()
        {
            PopupManager.AllClosePopup();//현재 팝업 전체 지우기 하고 나서 이동

            LoadingManager.Instance.EffectiveSceneLoad(WORLD_BOSS_SCENE_NAME, eSceneEffectType.CloudAnimation, null);
        }

        /*
		 * 현재 오픈 시도 하려는 건물이 타운에 1개 있고, 각 상태 별로 달리 표현해야함
		 * 건설 전		--> 건설 팝업
		 * 건설 중		--> 해당 건물의 가속 팝업
		 * 건설 완료	--> 건물 포커싱된 타운 화면
		 */
        public static void RequestBuildingPopup(string _subType, int materailItemNo = 0)
        {
            if (_subType == LANDMARK_DOZER_SUBTYPE_KEY || _subType == LANDMARK_REQUEST_SUBTYPE_KEY
                || _subType == LANDMARK_SUBWAY_SUBTYPE_KEY || _subType == LANDMARK_TRAVEL_SUBTYPE_KEY)
            {
                OpenLandmarkLayer(_subType);
                return;
            }

            var lowerSubTypeKey = _subType.ToLower();
            var buildingList = GetTownBuilding(lowerSubTypeKey);    //subType 소문자 변환
            var isExistBuilding = buildingList.Count > 0;       //요구 아이템 건물이 타운에 존재
            if (isExistBuilding)
            {
                //if(normalBuildingList.Count == 1)//현재 퀘스트 바로가기에서 건물이 단 하나만 있다면
                //            {
                //	var currentInstallTag = GetCurrentTagBySubKey(lowerSubTypeKey);
                //	var buildingData = Town.Instance.GetBuilding(currentInstallTag);
                //	if (buildingData == null)
                //	{
                //		Debug.LogError("잘못된 건물 installTag : " + currentInstallTag);
                //		return;
                //	}
                //	buildingData.OnClickBuilding();
                //}
                //else
                //            {
                //	ProductPopup.OpenPopup(lowerSubTypeKey);
                //}

                //var normalBuilding = buildingList.Find(building => building.State == eBuildingState.NORMAL);	// 운용가능한 건물을 포커싱하도록 설정
                var normalBuilding = buildingList[0];       // 무조건 첫번째를 포커싱하도록 설정

                if (materailItemNo > 0) // 그 재료에 관한 생산 가능 건물 조회
                {
                    var normalBuildings = buildingList.FindAll(building => building.State == eBuildingState.NORMAL);	// 운용가능한 건물을 포커싱하도록 설정
                    int minRequireLv = int.MaxValue;

                    // auto product 타입 검사
                    string buildingIndex = ProductData.GetBuildingGroupByProductItem(materailItemNo);
                    if (ProductData.IsProductBuilding(buildingIndex))
                    {
                        var prodBuildingDatas = ProductData.GetProductDatasByItemNo(materailItemNo);

                        foreach (var prodBuildingData in prodBuildingDatas)
                        {
                            minRequireLv = Mathf.Min(prodBuildingData.BUILDING_LEVEL, minRequireLv); // 해당 재료를 생산하기 위한 건물의 최소 레벨 조회
                        }
                    }

                    foreach (var building in normalBuildings)
                    {
                        if (building.Level >= minRequireLv)
                        {
                            normalBuilding = building;
                            break;
                        }
                    }
                }

                if (normalBuilding != null)
                {
                    if (Town.Instance == null) return;
                    var buildingData = Town.Instance.GetBuilding(normalBuilding.Tag);
                    if (buildingData == null)
                    {
                        Debug.LogError("잘못된 건물 installTag : " + normalBuilding.Tag);
                        return;
                    }
                    buildingData.OnClickBuilding();
                }
                else
                {
                    Debug.LogError("Building info list is null");
                    return;
                }
            }
            else
            {
                var openBuildingData = new BuildingConstructListData();
                var isAvailableOpen = openBuildingData.IsAvailableOpen();
                if (isAvailableOpen)
                    BuildingConstructListPopup.OpenPopup(openBuildingData);
                else//불가능 팝업 연결 - 해당레벨에 건물이 다 지음 & 그래도 빈 땅이 있을 경우
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002193));
                }
            }
        }

        static void OpenLandmarkLayer(string _subType)
        {
            int landmark = 0;
            switch (_subType)
            {
                case LANDMARK_DOZER_SUBTYPE_KEY:
                    landmark = (int)eLandmarkType.Dozer;
                    break;

                case LANDMARK_TRAVEL_SUBTYPE_KEY:
                    landmark = (int)eLandmarkType.Travel;
                    break;

                case LANDMARK_SUBWAY_SUBTYPE_KEY:
                    landmark = (int)eLandmarkType.SUBWAY;
                    break;
                case LANDMARK_REQUEST_SUBTYPE_KEY:
                    landmark = (int)eLandmarkType.EXCHANGE;
                    break;
            }

            var landmarkData = Town.Instance.GetBuilding(landmark);
            if (landmarkData != null)
                landmarkData.OnClickLandmark();
        }
        static List<BuildInfo> GetTownBuilding(string _subType)
        {
            List<BuildInfo> resultList = new List<BuildInfo>();

            List<BuildInfo> townList = User.Instance.GetUserBuildingList();//타운에서 유저가 설치한 건물 리스트
            foreach (BuildInfo building in townList)
            {
                BuildingOpenData openData = BuildingOpenData.GetByInstallTag(building.Tag);
                if (openData != null && openData.BUILDING == _subType/* && building.State == eBuildingState.NORMAL*/)
                {
                    resultList.Add(building);
                }
            }

            return resultList;
        }
        public static bool InvokeCustomAction(eActionType type, string param, Action cb = null)
        {
            switch (type)
            {
                case eActionType.URL_LINK:
                {
                    Application.OpenURL(param);
                }
                break;
                case eActionType.SHOP_OPEN:
                {
                    var goods_id = int.Parse(param);
                    ShopGoodsData goodsData = ShopGoodsData.Get(goods_id);
                    if (goodsData == null)
                        return false;

                    PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(goodsData.MENU));
                }
                break;
                case eActionType.BANNER_OPEN:
                {
                    var goods_id = int.Parse(param);
                    ShopGoodsData goodsData = ShopGoodsData.Get(goods_id);
                    if (goodsData == null)
                        return false;

                    PopupManager.OpenPopup<ShopBannerPopup>(new ShopBuyPopupData(goodsData));
                }
                break;
                case eActionType.GACHA_OPEN:
                {
                    SBFunc.MoveGachaScene((eGachaGroupMenu)int.Parse(param));

                }
                break;
                case eActionType.LEVEL_PASS:
                {
                    LevelPassPopup.RequestLevelPassPopup(() =>
                    {
                        LevelPassPopup.OpenPopup();
                    }, () =>
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("서버데이터호출실패"));
                    });
                }
                break;
                case eActionType.ANNOUNCE_OPEN:
                {
                    if (User.Instance.ENABLE_P2E)
                    {
                        DAppManager.Instance.OpenDAppNoticePage(int.Parse(param));
                    }
                    else
                    {
                        AnnouncePopup popup = PopupManager.OpenPopup<AnnouncePopup>();
                        popup.SetDefaultMenuId(int.Parse(param));
                    }
                }
                break;
                case eActionType.EVENT_ATTENDANCE://이벤트 출석 체크
                {
                    int id = int.Parse(param);
                    EventScheduleData data = EventScheduleData.Get(id);
                    if (data != null)
                    {
                        EventAttendancePopup.OpenPopup(data);
                    }
                }
                break;
                case eActionType.EVENT_DICE://2023 연말 이벤트
                case eActionType.EVENT_LUCKY_BAG://2024 구정 이벤트
                {
                    int id = int.Parse(param);
                    EventScheduleData data = EventScheduleData.Get(id);
                    if (data != null)
                    {
                        //todo 팝업 오브젝트 만들어서 Open연결                        
                    }
                }
                break;
                case eActionType.OPEN_EVENT:
                {
                    if (User.Instance.ENABLE_P2E)
                    {
                        DAppManager.Instance.OpenDAppEventPage(int.Parse(param));
                    }
                    else
                    {
                        OpenEventRankingPopup popup = PopupManager.OpenPopup<OpenEventRankingPopup>(new EventRankingPopupData(int.Parse(param)));
                    }
                }
                break;
                case eActionType.UNIONRAID_RANKING:
                {
                    if (User.Instance.ENABLE_P2E)
                    {
                        DAppManager.Instance.OpenDAppEventPage(int.Parse(param));
                    }
                    else
                    {
                        OpenEventRankingPopup popup = PopupManager.OpenPopup<UnionRaidEventPopup>(new EventRankingPopupData(int.Parse(param)));
                    }
                }
                break;
                case eActionType.CHAMPIONEVENT_RANKING:
                {
                    if (User.Instance.ENABLE_P2E)
                    {
                        DAppManager.Instance.OpenDAppEventPage(int.Parse(param));
                    }
                    else
                    {
                        OpenEventRankingPopup popup = PopupManager.OpenPopup<ChampionEventRankingPopup>(new EventRankingPopupData(int.Parse(param)));
                    }
                }
                break;
                case eActionType.LUNASERVER_OPEN_EVENT:
                {
                    LunaServerEventPopup popup = LunaServerEventPopup.OpenPopup(new TabTypePopupData(0,0));
                }
                break;
                case eActionType.RESTRICTED_AREA_EVENT:
                {
                    PopupManager.OpenPopup<RestrictedAreaEventPopup>(new EventRankingPopupData(int.Parse(param)));
                }
                break;
                default:
                    return false;
            }

            cb?.Invoke();
            return true;
        }
        #endregion

        public static void SendSupportURL(string defaultURL = "")
        {
            if (string.IsNullOrWhiteSpace(defaultURL))
                defaultURL = GameConfigTable.GetSurpportURL();


            if (string.IsNullOrWhiteSpace(defaultURL) == false)
            {
                string paramString = "";

                long[] tfID =
                {
                        900011725766, //닉네임 / 닉네임
                        5354068699161, //회원 번호 / 유저넘버                        
                        1900000508328, //기기 정보
                        49917122612377,
                };

                for (int i = 0; i < tfID.Length; i++)
                {
                    if (i == 0)
                    {
                        if (defaultURL.Contains("?"))
                            paramString += "&";
                        else
                            paramString += "?";
                    }
                    else
                        paramString += "&";

                    paramString += "tf_" + tfID[i].ToString() + "=";
                    switch (i)
                    {
                        case 0://닉네임
                            paramString += UnityEngine.Networking.UnityWebRequest.EscapeURL(User.Instance.UserData.UserNick);
                            break;
                        case 1://회원 번호
                            paramString += User.Instance.UserAccountData.UserNumber;
                            break;
                        case 2://기기 정보
                            paramString += SystemInfo.deviceModel;
                            break;
                        case 3://서버 정보
                            switch(NetworkManager.ServerTag)
                            {
                                case 1:
                                    paramString += "angel";
                                    break;
                                case 2:
                                    paramString += "wonder";
                                    break;
                                case 0:
                                default:
                                    paramString += "unknown";
                                    break;
                            }
                            break;
                    }
                }

                Application.OpenURL(defaultURL + paramString);
            }
        }
        public static IEnumerator DownloadStateEvent(DownloadState state, int cur, int max, int totalMax)
        {
            while (cur < max)
            {
                cur++;
                state?.Invoke(cur, totalMax); //"디폴트이미지로드"
                yield return SBDefine.GetWaitForEndOfFrame();
            }
            yield break;
        }
        #region Portrait
        public static Dictionary<ePortraitEtcType, int> GetPortraitData(JToken _jsonData)
        {
            Dictionary<ePortraitEtcType, int> tempData = new();
            if (!IsJObject(_jsonData))
                return tempData;

            var jobj = (JObject)_jsonData;
            foreach (var val in jobj.Properties().ToArray())
            {
                if (IsJTokenType(val.Value, JTokenType.Integer) && int.TryParse(val.Name, out int resultKey))
                {
                    var key = (ePortraitEtcType)resultKey;
                    var value = val.Value.Value<int>();
                    if (false == tempData.TryAdd(key, value))
                        tempData[key] = value;
                }
            }
            return tempData;
        }
        public static void UpdatePortraitData(JToken _jsonData, Dictionary<ePortraitEtcType, int> Portrait_Info)
        {
            if (Portrait_Info == null)
                return;

            if (!SBFunc.IsJObject(_jsonData))
                return;

            var jobj = (JObject)_jsonData;

            foreach (var val in jobj.Properties().ToArray())
            {
                var key = val.Name;
                var value = val.Value.Value<int>();

                if (int.TryParse(key, out int resultKey))
                {
                    ePortraitEtcType _type = (ePortraitEtcType)resultKey;
                    if (Portrait_Info.ContainsKey(_type))
                        Portrait_Info[_type] = value;
                    else
                        Portrait_Info.Add(_type, value);
                }
            }
        }
        public static int GetRaidRewardIndexByValue(this ePortraitEtcType ePortraitType, int _rankingValue)
        {
            int returnValue = 0;
            switch (ePortraitType)
            {
                case ePortraitEtcType.RAID:
                {
                    if (_rankingValue >= 21 && _rankingValue < 101)
                    {
                        returnValue = 6;
                    }
                    else if (_rankingValue >= 11 && _rankingValue < 21)
                    {
                        returnValue = 5;
                    }
                    else if (_rankingValue >= 4 && _rankingValue < 11)
                    {
                        returnValue = 4;
                    }
                    else if (_rankingValue == 3)
                    {
                        returnValue = 3;
                    }
                    else if (_rankingValue == 2)
                    {
                        returnValue = 2;
                    }
                    else if (_rankingValue == 1)
                    {
                        returnValue = 1;
                    }
                }
                break;
                case ePortraitEtcType.NONE:
                default: break;
            }
            return returnValue;
        }
        public static string GetPortraitFrontSpriteName(this ePortraitEtcType ePortraitType, int _value)
        {
            return ePortraitType switch
            {
                ePortraitEtcType.RAID => SBDefine.RAID_FRONT_SPRITE_PREFIX + _value,
                _ => ""
            };
        }
        public static string GetPortraitTopSpriteName(this ePortraitEtcType ePortraitType, int _value)
        {
            return ePortraitType switch
            {
                ePortraitEtcType.RAID => SBDefine.RAID_TOP_SPRITE_PREFIX + _value,
                _ => ""
            };
        }
        public static string GetPortraitBotSpriteName(this ePortraitEtcType ePortraitType, int _value)
        {
            return ePortraitType switch
            {
                ePortraitEtcType.RAID => SBDefine.RAID_BOTTOM_SPRITE_PREFIX + _value,
                _ => ""
            };
        }
        public static Sprite GetPortraitSprite(this ePortraitEtcType ePortraitType, int _value)
        {
            var resultValue = ePortraitType.GetRaidRewardIndexByValue(_value);
            if (resultValue <= 0)
                return null;

            return ResourceManager.GetResource<Sprite>(eResourcePath.PortraitFrameIconPath, ePortraitType.GetPortraitFrontSpriteName(resultValue));
        }
        public static List<Sprite> GetPortraitAddSprite(this ePortraitEtcType ePortraitType, int _value)
        {
            var resultValue = ePortraitType.GetRaidRewardIndexByValue(_value);
            if (resultValue <= 0)
                return null;

            List<Sprite> tempList = new List<Sprite>();
            string resultTopSpiretName = ePortraitType.GetPortraitTopSpriteName(resultValue);
            string resultBottomSpriteName = ePortraitType.GetPortraitBotSpriteName(resultValue);
            if (resultBottomSpriteName != "" && resultTopSpiretName != "")
            {
                tempList.Add(ResourceManager.GetResource<Sprite>(eResourcePath.PortraitFrameIconPath, resultBottomSpriteName));
                tempList.Add(ResourceManager.GetResource<Sprite>(eResourcePath.PortraitFrameIconPath, resultTopSpiretName));
            }

            return tempList;
        }
        #endregion
        #region Passive
        public static Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> GetPassives(IBattleCharacterData caster, IBattleCharacterData target, eBattleType battleType)
        {
            if (caster.TranscendenceData.Step > 0 && caster.TranscendenceData.PassiveSlot > 0)
            {
                var res = new Dictionary<eSkillPassiveStartType, List<SkillPassiveData>>();
                for (int i = 1, count = caster.TranscendenceData.PassiveSlot; i <= count; ++i)
                {
                    var passive = caster.TranscendenceData.GetPassiveData(i);
                    //패시브 스킬 타입 및 돌릴지 확인
                    if (null == passive)
                        continue;

                    if (true == passive.IsPassiveStartSkip(eSkillPassiveStartType.ALWAYS) &&
                        false == passive.IsPassiveTargetSkip(caster, passive.IsPassiveSelf() ? caster : target) &&
                        false == passive.IsPassiveContentSkip(battleType) &&
                        true == passive.IsPassiveRateCheck(caster, target))
                    {
                        if (false == res.TryGetValue(passive.START_TYPE, out var value))
                        {
                            value = new List<SkillPassiveData>();
                            res.Add(passive.START_TYPE, value);
                        }

                        value.Add(passive);
                    }
                }
                if (res.Count > 0)
                    return res;
            }
            return null;
        }
        #endregion

        /// <summary>
        /// p2e 유저인지 판단해서, p2e 아이템 타입 체크해서 rewardPopup 에 뿌려줄 아이템 정제
        /// </summary>
        /// <param name="_paramItemList"></param>
        /// <returns></returns>
        public static List<Asset> GetVisibleItemByP2E(List<Asset> _paramItemList)
        {
            List<Asset> tempList = new List<Asset>();
            bool isP2EUser = GameConfigTable.WEB3_MENU_OPEN_ON_KOREAN || User.Instance.ENABLE_P2E;

            if (_paramItemList == null || _paramItemList.Count <= 0)
                return tempList;

            foreach (var item in _paramItemList)
            {
                if (item == null || item.Amount <= 0)
                    continue;

                var itemData = ItemBaseData.Get(item.ItemNo);
                if (itemData == null && item.GoodType != eGoodType.ITEM)
                {
                    tempList.Add(item);//재화
                    continue;
                }

                var isP2E = itemData.ENABLE_P2E;
                if (!isP2EUser && isP2E)//p2e유저가 아니고 , p2e 아이템을 받으려하면 패스
                    continue;
                else
                    tempList.Add(item);
            }

            return tempList;
        }


        public static GameObject GetPetSpineByName(string prefabName, eSpineType temp = eSpineType.FIELD)
        {
            return ResourceManager.GetResource<GameObject>(eResourcePath.PetClonePath, SBFunc.StrBuilder((temp == eSpineType.UI ? "UI/" : ""), prefabName));
        }

        public static string GetRankText(int rank)
        {
            switch(rank)
            {
                case 1:
                    return StringData.GetStringFormatByStrKey("순위_1", rank);
                case 2:
                    return StringData.GetStringFormatByStrKey("순위_2", rank);
                case 3:
                    return StringData.GetStringFormatByStrKey("순위_3", rank);
                default:
                    return StringData.GetStringFormatByStrKey("순위", rank);
            }
            
        }
    }
}