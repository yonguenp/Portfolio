using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SandboxNetwork
{
    public class GameConfigTable : TableBase<GameConfigData, DBGame_config>
    {
        static GameConfigTable instance = null;
#if UNITY_EDITOR
        public string BRANCH_NAME { get; set; } = "";
        public string BRANCH_TABLE
        {
            get
            {
                return BRANCH_NAME switch
                {
                    "branch_2311" => "_2311",
                    "branch_2312" => "_2312",
                    "branch_2401" => "_2401",
                    _ => ""
                };
            }
        }
#endif
        public static GameConfigTable Instance
        {
            get
            {
                if (instance == null)
                    instance = TableManager.GetTable<GameConfigTable>();
                return instance;
            }
        }

        public override void Init()
        {
#if UNITY_EDITOR_WIN && CHECK_BRANCH
            if (string.IsNullOrEmpty(BRANCH_NAME))
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("git.exe");

                startInfo.UseShellExecute = false;
                startInfo.WorkingDirectory = Application.dataPath;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.Arguments = "rev-parse --abbrev-ref HEAD";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = startInfo;
                process.Start();

                BRANCH_NAME = process.StandardOutput.ReadLine().ToLower();
                Debug.Log("Current branch name : " + BRANCH_NAME);
            }
#endif//UNITY_EDITOR
            base.Init();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        public override GameConfigData Get(string key)
        {
            if (false == ContainsKey(key))
            {
                return null;
            }
            return datas[key];
        }
        public static int GetAdventureStaminaRechargeAmount()
        {
            return GetConfigIntValue("STAMINA_RECHARGE_NUM", 8);
        }

        public static int GetAdventureStaminaRechargeTime()
        {
            return GetConfigIntValue("STAMINA_RECHARGE_TIME", 60);
        }
        public static int GetAdventureStaminaFakeUpdateCount()
        {
            return GetConfigIntValue("STAMINA_FAKE_UPDATE", 5);
        }
        public static int GetArenaUserMaxTicketCount()
        {
            return GetConfigIntValue("PVP_TICKET_RECHARGE_MAX_NUM", -1);
        }

        public static int GetArenaUserTicketRechargeCount()
        {
            return GetConfigIntValue("PVP_TICKET_RECHARGE_NUM", -1);
        }

        public static int GetArenaDailyMaxTicketRechargeCount()
        {
            return GetConfigIntValue("PVP_TICKET_RECHARGE_DAILY_COUNT", -1);
        }

        public static int GetArenaOneTicketRechargeTime()
        {
            return GetConfigIntValue("PVP_TICKET_RECHARGE_TIME", -1);
        }

        public static int GetPartMergeLimitCount()
        {
            return GetConfigIntValue("PART_MERGE_LIMIT_COUNT", 99);
        }

        public static int GetLastWorld()
        {
            return GetConfigIntValue("WORLD_PLAYABLE_LAST_NUM", 8);
        }
        /// <summary>
        /// 월드 레이드 오픈 조건(3월드 클리어)
        /// </summary>
        /// <returns></returns>
        public static int GetRaidOpenWorld()
        {
            return GetConfigIntValue("RAID_OPEN_WORLD", 3);
        }

        public static int GetDragonLevelMax()
        {
            return GetConfigIntValue("DRAGON_MAX_LEVEL", 30);
        }

        public static int GetSkillLevelMax()
        {
            return GetConfigIntValue("DRAGON_SKILL_MAX_LEVEL", 60);
        }

        public static int GetArenaBattleTime()
        {
            return GetConfigIntValue("PVP_BATTLE_TIME", 180);
        }

        public static int GetArenaBuffTime()
        {
            return GetConfigIntValue("PVP_TIMER_BUFF_TIME", 30);
        }
        public static int GetArenaBuffEffectGroup()
        {
            return GetConfigIntValue("PVP_TIMER_BUFF_SKILL", -1);
        }

        public static int GetPetLevelMax(int petGrade)
        {
            return petGrade switch
            {
                1 => GetConfigIntValue("N_PET_MAX_LEVEL", 60),
                2 => GetConfigIntValue("R_PET_MAX_LEVEL", 60),
                3 => GetConfigIntValue("SR_PET_MAX_LEVEL", 60),
                4 => GetConfigIntValue("UR_PET_MAX_LEVEL", 60),
                5 => GetConfigIntValue("L_PET_MAX_LEVEL", 60),
                _ => GetConfigIntValue("PET_MAX_LEVEL", 60)
            };
        }

        public static int GetPetReinforceLevelMax(int petGrade)
        {
            return petGrade switch
            {
                1 => GetConfigIntValue("N_PET_MAX_REINFORCE", 10),
                2 => GetConfigIntValue("R_PET_MAX_REINFORCE", 10),
                3 => GetConfigIntValue("SR_PET_MAX_REINFORCE", 10),
                4 => GetConfigIntValue("UR_PET_MAX_REINFORCE", 10),
                5 => GetConfigIntValue("L_PET_MAX_REINFORCE", 10),
                _ => GetConfigIntValue("PET_REINFORCE_MAX_LEVEL", 10)
            };
        }
        public static int GetReceiveMaxCount()//친구 받은 신청 수 최대 수량
        {
            return GetConfigIntValue("FRIEND_RECEIVE_MAX_NUM", 30);
        }
        public static int GetSendFriendDailyGiftMax()//친구포인트 보내는 수량 제한
        {
            return GetConfigIntValue("FRIEND_DAILY_GIFT_MAX_NUM", 30);
        }
        public static int GetFriendPointMaxCount()//친구포인트 받는 최대 수량 제한(하루제한이 아님! 최대 수량)
        {
            return GetConfigIntValue("FRIENDLY_POINT_MAX_NUM", 9999);
        }
        public static int GetFriendSendPointCount()//친구에게 우정 포인트 전송 1회 시 수량
        {
            return GetConfigIntValue("FRIENDLY_POINT_ONCE_SEND_NUM", 1);
        }

        public static int GetDailyRewardItemGroup(int index = 1)
        {
            string key = index switch
            {
                1 => "FIRST_DAILY_REWARD_ITEM_GROUP",
                2 => "SECOND_DAILY_REWARD_ITEM_GROUP",
                3 => "THIRD_DAILY_REWARD_ITEM_GROUP",
                _ => "FIRST_DAILY_REWARD_ITEM_GROUP"
            };
            return GetConfigIntValue(key, -1);
        }
        public static int GetMergeURSuccessCount()//드래곤 조합 천장 기준 카운트
        {
            return GetConfigIntValue("MERGE_SUCCESS_COUNT_UR", 20);
        }
        public static int GetMergeSRSuccessCount()//드래곤 조합 천장 기준 카운트
        {
            return GetConfigIntValue("MERGE_SUCCESS_COUNT_SR", 20);
        }
        public static int GetPartMergeMaterialMaxCount()//드래곤 조합 천장 기준 카운트
        {
            return GetConfigIntValue("PART_MERGE_MATERIAL_MAX_COUNT", 5);
        }
        public static int GetPartReinforceFirstStep()
        {
            return GetConfigIntValue("REINFORCE_FIRST_STEP", 6);
        }
        public static int GetPartReinforceSecondStep()
        {
            return GetConfigIntValue("REINFORCE_SECOND_STEP", 9);
        }
        public static int GetPartReinforceThirdStep()
        {
            return GetConfigIntValue("REINFORCE_THIRD_STEP", 12);
        }
        public static int GetPartReinforceFourthStep()
        {
            return GetConfigIntValue("REINFORCE_FOURTH_STEP", 15);
        }
        public static int GetDailyContentResetTime()
        {
            return GetConfigIntValue("DAILY_CONTENT_RESET_TIME", 4);
        }
        /// <summary>
        /// 이벤트 콘텐츠 초기화 시간
        /// </summary>
        /// <returns></returns>
        public static int GetDailyEventContentResetTime()
        {
            return GetConfigIntValue("EVENT_CONTENT_RESET_TIME", 0);
        }
        public static bool IsChampionActive()
        {
            return (GetConfigIntValue("TM_IS_ACTIVE", 0) & NetworkManager.ServerTag) > 0;
        }

        public static int GetCheerReward()
        {
            return GetConfigIntValue("TM_CHEER_REWARD", 100);
        }

        public static string GetCheerRewardWithRound(string _round = "1")
        {
            //16강 : _round = "1"
            //8강  : _round = "11"
            //4강  : _round = "101"
            //결승 : _round = "1001"
            return GetConfigValue("TM_CHEER_REWARD_" + _round, "3,30000006,1");
        }
        public static string GetCheerRewardData(string _round = "1", string _type = "itemNo")
        {
            //16강 : _round = "1"
            //8강  : _round = "11"
            //4강  : _round = "101"
            //결승 : _round = "1001"
            string[] rewardDatas = GetConfigValue("TM_CHEER_REWARD_" + _round, "3,30000006,1").Split(',');
            string rewardData = "";
            switch (_type)
            {
                case "type":
                    rewardData = rewardDatas[0];
                    break;
                case "itemNo":
                    rewardData = rewardDatas[1];
                    break;
                case "amount":
                    rewardData = rewardDatas[2];
                    break;
            }
            return rewardData;
        }

        public static string GetSurpportURL()
        {
            string value = string.Empty;
            var data = LanguageData.Get(GamePreference.Instance.GameLanguage);
            if (data != null)
                value = data.SURPPORT_URL;

            if (string.IsNullOrEmpty(value))
            {
                data = LanguageData.Get(SystemLanguage.English);
                if (data != null)
                    value = data.SURPPORT_URL;
            }

            if (string.IsNullOrEmpty(value))
                return "https://sandboxnetwork.zendesk.com/hc/en-us/requests/new?ticket_form_id=22568004516633";
            else
                return value;
        }
        public static string GetGameGuideURL()
        {
            string value = string.Empty;
            var data = LanguageData.Get(GamePreference.Instance.GameLanguage);
            if (data != null)
                value = data.GAME_GUIDE_URL;

            if (string.IsNullOrEmpty(value))
            {
                data = LanguageData.Get(SystemLanguage.English);
                if (data != null)
                    value = data.GAME_GUIDE_URL;
            }

            if (string.IsNullOrEmpty(value))
                return "https://sandboxnetwork.zendesk.com/hc/en-us";
            else
                return value;
        }
        public static string GetNaverLoungeURL()
        {
            return GetConfigValue("NAVER_LOUNGE", "https://game.naver.com/lounge/Meta_Toy_DragonZ_SAGA/home");
        }
        public static string GetStoreURL()
        {
            string key =
#if UNITY_IOS
			"IOS_STORE";
#elif ONESTORE
			"ONESTORE_STORE";
#else
            "GOOGLE_STORE";
#endif
            return GetConfigValue(key, "https://play.google.com/store/apps/details?id=com.sandboxgame.mtdsaga");
        }

        bool registed_version = false;
        bool version_check = false;
        public static bool IsRegistedVersion()//iOS 검수 시에 찔리는 컨텐츠 숨기기려고
        {
            if (!instance.version_check)
            {
#if SB_TEST
                instance.registed_version = true;
#else
                instance.registed_version = instance.CheckVersion();
#endif
                instance.version_check = true;
            }

            return instance.registed_version;

        }

        bool CheckVersion()
        {
#if UNITY_IOS
            GameConfigData data = Instance.Get("IOS_SERVICE_VERSION");            
#elif UNITY_ANDROID
            GameConfigData data = Instance.Get("ANDROID_SERVICE_VERSION");
#else
            GameConfigData data = null;
#endif
            if (data == null)
            {
                return false;
            }

            var cur_version = GamePreference.Instance.VERSION.Split('.');
            var val_versions = data.VALUE.Split('/');
            foreach (var val_ver in val_versions)
            {
                if (val_ver == GamePreference.Instance.VERSION)
                    return true;

                if (cur_version.Length == 3)
                {
                    var val = val_ver.Split('.');
                    if (cur_version.Length == val.Length)
                    {
                        if (int.Parse(cur_version[0]) <= int.Parse(val[0]))
                        {
                            if (int.Parse(cur_version[1]) <= int.Parse(val[1]))
                            {
                                if (int.Parse(cur_version[2]) <= int.Parse(val[2]))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
        public static string GetServiceTermsURL()
        {
            string value = string.Empty;
            var data = LanguageData.Get(GamePreference.Instance.GameLanguage);
            if (data != null)
                value = data.SERVICE_TERMS;

            if (string.IsNullOrEmpty(value))
            {
                data = LanguageData.Get(SystemLanguage.English);
                if (data != null)
                    value = data.SERVICE_TERMS;
            }

            if (string.IsNullOrEmpty(value))
                return "https://sites.google.com/sandboxnetwork.net/policies/en";
            else
                return value;
        }

        public static string GetPrivateTermsURL()
        {
            string value = string.Empty;
            var data = LanguageData.Get(GamePreference.Instance.GameLanguage);
            if (data != null)
                value = data.INFO_TERMS;

            if (string.IsNullOrEmpty(value))
            {
                data = LanguageData.Get(SystemLanguage.English);
                if (data != null)
                    value = data.INFO_TERMS;
            }

            if (string.IsNullOrEmpty(value))
                return "https://sites.google.com/sandboxnetwork.net/policies/en";
            else
                return value;
        }

        public static string GetArtBlockURL()
        {
            return GetConfigValue("ART_BLOCK_URL", "https://dapps.meta-toy.world/");
        }

        public static string GetConfigValue(string key, string defaultValue = null)
        {
            GameConfigData data = Instance.Get(key);
            if (data == null)
                return defaultValue;

            return data.VALUE;
        }
        public static int GetConfigIntValue(string key, int defaultValue = 0)
        {
            GameConfigData data = Instance.Get(key);
            if (data == null)
                return defaultValue;

            if (false == int.TryParse(data.VALUE, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int result))
                return defaultValue;

            return result;
        }
        public static float GetConfigFloatValue(string key, float defaultValue = 0f)
        {
            GameConfigData data = Instance.Get(key);
            if (data == null)
                return defaultValue;

            if (false == float.TryParse(data.VALUE, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
                return defaultValue;

            return result;
        }

        public static bool IsAdventureHotTime
        {
            get
            {
                var datas = EventScheduleData.GetEventTypeData(eActionType.EVENT_HOT_TIME_ADVENTURE);
                foreach (var data in datas)
                {
                    if (data.IsEventPeriod(false))
                        return true;
                }

                return false;
            }
        }
        public static bool IsDailyDungeonHotTime
        {
            get
            {
                var datas = EventScheduleData.GetEventTypeData(eActionType.EVENT_HOT_TIME_DAILYDUNGEON);
                foreach (var data in datas)
                {
                    if (data.IsEventPeriod(false))
                        return true;
                }

                return false;
            }
        }
        public static bool IsRaidHotTime
        {
            get
            {
                var datas = EventScheduleData.GetEventTypeData(eActionType.EVENT_HOT_TIME_WORLDBOSS);
                foreach (var data in datas)
                {
                    if (data.IsEventPeriod(false))
                        return true;
                }

                return false;
            }
        }
        public static bool IsGemDungeonHotTime
        {
            get
            {
                var datas = EventScheduleData.GetEventTypeData(eActionType.EVENT_HOT_TIME_GEMDUNGEON);
                foreach (var data in datas)
                {
                    if (data.IsEventPeriod(false))
                        return true;
                }

                return false;
            }
        }
        
        public static DateTime GetHotTimeEndTime(HotTimeDescType type)
        {
            EventScheduleData data = null;
            switch (type)
            {
                case HotTimeDescType.ADVENTURE:
                    foreach (var d in EventScheduleData.GetEventTypeData(eActionType.EVENT_HOT_TIME_ADVENTURE))
                    {
                        if (d.IsEventPeriod(false))
                        {
                            data = d;
                            break;
                        }
                    }
                    break;
                case HotTimeDescType.DAILYDUNGEON:
                    foreach (var d in EventScheduleData.GetEventTypeData(eActionType.EVENT_HOT_TIME_DAILYDUNGEON))
                    {
                        if (d.IsEventPeriod(false))
                        {
                            data = d;
                            break;
                        }
                    }
                    break;
                case HotTimeDescType.WORLDBOSS:
                    foreach (var d in EventScheduleData.GetEventTypeData(eActionType.EVENT_HOT_TIME_WORLDBOSS))
                    {
                        if (d.IsEventPeriod(false))
                        {
                            data = d;
                            break;
                        }
                    }
                    break;
                case HotTimeDescType.GEMDUNGEON:
                    foreach (var d in EventScheduleData.GetEventTypeData(eActionType.EVENT_HOT_TIME_GEMDUNGEON))
                    {
                        if (d.IsEventPeriod(false))
                        {
                            data = d;
                            break;
                        }
                    }
                    break;
            }

            if (data == null)
                return DateTime.MinValue;

            return data.END_TIME;
        }

        public static float GetHotTimeRate(HotTimeDescType type)
        {
            switch (type)
            {
                case HotTimeDescType.ADVENTURE:
                    return GetConfigIntValue("HOTTIME_ADVENTURE_RATE", 1000) * 0.001f;
                case HotTimeDescType.DAILYDUNGEON:
                    return GetConfigIntValue("HOTTIME_DAILYDUNGEON_RATE", 1000) * 0.001f;
                case HotTimeDescType.WORLDBOSS:
                    return GetConfigIntValue("HOTTIME_RAID_RATE", 1000) * 0.001f;
                case HotTimeDescType.GEMDUNGEON:
                    return GetConfigIntValue("HOTTIME_GEMDUNGEON_RATE", 1000) * 0.001f;
            }

            return 1.0f;
        }

        public static int CHAMPION_CONTENT_TIME => GetConfigIntValue("CHAMPION_CONTENT_TIME", 12);
        public static int CHAMPION_START_DAY => GetConfigIntValue("CHAMPION_START_DAY", 15);
        public static int CHAMPION_DRAWING_TIME => GetConfigIntValue("CHAMPION_DRAWING_TIME", 6);
        public static int CHAMPION_MATCH_HOUR => GetConfigIntValue("CHAMPION_MATCH_HOUR", 16);
        public static int CHAMPION_DEF_OPEN => GetConfigIntValue("CHAMPION_DEF_OPEN", 12);
        public static int CHAMPION_ATK_OPEN => GetConfigIntValue("CHAMPION_ATK_OPEN", 14);
        public static int CHAMPION_MINIMUM_TIER => GetConfigIntValue("CHAMPION_MINIMUM_TIER", 111);
        public static int GetSystemMessageGrade()
        {
            return GetConfigIntValue("SYSTEM_MSG_DRAGON_GRADE", (int)eDragonGrade.Unique);
        }


        List<int> ignoreList = null;
        public static bool IsIgnoreSystemMessageTarget(int no)
        {
            if (Instance.ignoreList == null)
            {
                string value = GetConfigValue("SYSTEM_MSG_IGNORE_NO", "");
                if (!string.IsNullOrEmpty(value))
                {
                    Instance.ignoreList = new List<int>();
                    var values = value.Split('/');
                    foreach (var val in values)
                    {
                        Instance.ignoreList.Add(int.Parse(val));
                    }
                }
            }

            if (Instance.ignoreList == null)
                return false;

            return Instance.ignoreList.Contains(no);
        }
               

        public static bool INTRO_FORCE => GetConfigIntValue("INTRO_FORCE", 0) > 0;

        public static int ATTTimeOutInterval => GetConfigIntValue("ATT_TIME_OUT", 120);

        public static bool WEB3_MENU_OPEN_ON_KOREAN => IsRegistedVersion() ? (GetConfigIntValue("WEB3_OPEN_ON_KOREAN", 1) > 0) : false;

        public static bool ON_MAGNET_CLAIM_WITH_AD => GetConfigIntValue("ON_MAGNET_CLAIM_WITH_AD", 0) > 0;

        public static int PRACTICE_PRICE_VALUE => GetConfigIntValue("TM_PRACTICE_PRICE", 50);

        public static bool GEM_TO_NFT {
            get 
            {
                return (GetConfigIntValue("GEM_TO_NFT", 0) & NetworkManager.ServerTag) > 0;
            } 
        }
        public static bool SKILLCUBE_TO_NFT
        {
            get
            {
                return (GetConfigIntValue("CUBE_TO_NFT", 0) & NetworkManager.ServerTag) > 0;
            }
        }
    }
}