using SBCommonLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public class GameConfig : Singleton<GameConfig>
{
    #region FOR_DEV
    public bool USE_DUMMY { get; private set; }
    public bool DUMMY_PLAYCHASER { get; private set; }
    #endregion

    public string VERSION { get; private set; }
    public int BUILD_CODE { get; private set; }
    public string BUILD_BRANCH { get; private set; }
    public bool IS_VERSION_VALIDATE { get; private set; } = true;
    public bool IS_COUPON_VALIDATE { get; private set; }
#if UNITY_IOS
                = false;
#else
                = true;
#endif
    public float REWARD_GOLD_RATIO { get; private set; }
    public float REWARD_EXP_RATIO { get; private set; }
    public float REWARD_CHARACTER_EXP_RATIO { get; private set; }
    public int MAX_CHARACTER_LEVEL { get; private set; }
    public int MAX_CHARACTER_REINFORCE { get; private set; }
    public int MAX_CHARACTER_SKILL_LEVEL { get; private set; }
    public int CLONE_PROJECTILE_FADEOUT_TIME { get; private set; }
    public float DROPPED_BATTERY_LIFETIME { get; private set; }
    public float GAMELOG_MESSAGE_LIFETIME { get; private set; }
    public float GAMELOG_MESSAGE_FADEOUT_TIME { get; private set; }
    public float SKILL_TRANSPARENT_ALPHA { get { return 0.5f; } }//Config 테이블에서 불러왔었지만 쉐이더 시스템 개발로인해 고정 알파값 0.5 사용
    public float MOVE_PAD_SENSITIVITY { get; private set; }
    public int MOVE_DIRECTION_COUNT { get; private set; }
    public int RANDOM_SHOP_REFRESH_TIME { get; private set; }
    public int RANDOM_SHOP_REFRESH_PRICE { get; private set; }
    public int REINFORCE_PER_MAX_LEVEL { get; private set; }
    public int SMALL_EXP_ITEM_GOLD { get; private set; }
    public int MEDIUM_EXP_ITEM_GOLD { get; private set; }
    public int LARGE_EXP_ITEM_GOLD { get; private set; }
    public string SUPPORT_URL { get; private set; }
    public string COUPON_URL { get; private set; }
    public bool USE_TEST_SERVER { get; private set; } = false;
    public bool USE_QA_SERVER { get; private set; } = false;

    public float HIDE_OBJECT_DELAY_TIME { get { return 10f; } } //나중에 config 테이블에 작성 할 수 있도록 변수만 설정 현재 고정값

    public int TALENT1_SOUL { get; private set; }
    public int TALENT1_GOLD { get; private set; }
    public int TALENT2_SOUL { get; private set; }
    public int TALENT2_GOLD { get; private set; }
    public int TALENT3_SOUL { get; private set; }
    public int TALENT3_GOLD { get; private set; }

    public int SOUL_STONE_TRADE_C { get; private set; }
    public int SOUL_STONE_TRADE_B { get; private set; }
    public int SOUL_STONE_TRADE_A { get; private set; }
    public int SOUL_STONE_TRADE_S { get; private set; }

    public int GACHA_ADVERTISEMENT_TIME { get; private set; }

    public int MAX_TALENT_RANK { get; private set; }

    public string GOOGLE_STORE_URL { get; private set; } = "https://play.google.com/store/apps/details?id=com.sandboxgame.mocmm";
    public string APPLE_STORE_URL { get; private set; } = "https://apps.apple.com/kr/app/%EA%B3%B5%ED%8F%AC%EC%9D%98-%EC%88%A0%EB%9E%98%EC%9E%A1%EA%B8%B0/id1597697228";
    public string CDN_URL { get; private set; }
    public string SANDBOX_URL { get; private set; }
    public bool USE_DUO_MATCH_SERVICE { get; private set; } = true;
    public bool USE_CUSTOM_MATCH_SERVICE { get; private set; } = true;
    public bool IS_INSPECTION { get; private set; } = false;
    public int RESOURCE_BUNDLE_VERSION { get; private set; } = 23;
    public int DEFAULT_CLAN_HEADCOUNT { get; private set; } = 8;
    public int CLAN_MAKE_POINT { get; private set; } = 350;
    public int CLAN_MAKE_GOLD { get; private set; } = 50000;
    public float MOVE_PACKET_DELAY { get; private set; } = 0.1f;

    public int TOOLTIP_OFFSET { get; private set; } = 50;
    public int CHRISTMAS_NORAML { get; private set; } = 5;
    public int CHRISTMAS_RARE { get; private set; } = 1;
    public int CHRISTMAS_UNIQUE { get; private set; } = 1;
    public int NEWYEAR_SKIP_FLAG { get; private set; }
    public int SPECIAL_TALENT_ITEM_NO { get; private set; } = 0;
    public string NEWYEAR_RATE_PAGE { get; private set; } = "https://game.naver.com/lounge/Moris_Nightmare_Hide_and_seek_Multi/home";
    public float NEWYEAR_NOTI_TIME { get; private set; } = 5.0f;
    public int AD_REWARD_USE { get; private set; } = 0;
    public float AD_REWARD_MULTIPLE { get; private set; } = 1;
    #region OPTION SETTING

    class OptionValue
    {
        public string key { get; private set; }
        public int value { get; private set; }

        public OptionValue(string k, int defaultValue)
        {
            key = k;
            value = GetOptionValue(defaultValue);
        }

        private int GetOptionValue(int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetOptionValue(int newVal)
        {
            value = newVal;
            PlayerPrefs.SetInt(key, value);
        }
    }
    private OptionValue optionBGM = new OptionValue("option_bgm", 1);
    private OptionValue optionSFX = new OptionValue("option_sfx", 1);
    private OptionValue optionVIB = new OptionValue("option_vibration",
#if UNITY_ANDROID
        1
#else
        0
#endif
        );
    private OptionValue optionJOYSTICK = new OptionValue("option_joystick", 0);
    private OptionValue optionLanguage = new OptionValue("option_language", (int)defaultLanguage);
    private OptionValue optionDUO = new OptionValue("option_duo", 1);

    private static SystemLanguage defaultLanguage
    {
        get
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Korean:
                    return SystemLanguage.Korean;

                //case SystemLanguage.English:
                //    return SystemLanguage.English;
                //default:
                //    return SystemLanguage.English;
                default:
                    return SystemLanguage.Korean;
            }
        }
    }
    public bool OPTION_BGM { get { return optionBGM.value > 0; } set { optionBGM.SetOptionValue(value ? 1 : 0); } }
    public bool OPTION_SFX { get { return optionSFX.value > 0; } set { optionSFX.SetOptionValue(value ? 1 : 0); } }
    public bool OPTION_VIBRATION { get { return optionVIB.value > 0; } set { optionVIB.SetOptionValue(value ? 1 : 0); } }
    public JoystickType OPTION_JOYSTICK { get { return (JoystickType)optionJOYSTICK.value; } set { optionJOYSTICK.SetOptionValue((int)value); } }
    public SystemLanguage OPTION_LANGUAGE { get { return (SystemLanguage)optionLanguage.value; } set { optionLanguage.SetOptionValue((int)value); } }

    public bool FRIEND_RECOMMEND { get; set; } = true;
    public bool DUO_ENABLE { get { return optionDUO.value > 0; } set { optionDUO.SetOptionValue(value ? 1 : 0); } }
    #endregion
    

    public GameConfig()
    {
        USE_DUMMY = false;
        DUMMY_PLAYCHASER = true;

        LoadVersionInfo();
    }

    public void LoadConfig()
    {
        //LoadVersionInfo();//refresh?

        foreach (ConfigGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.config))
        {
            LoadConfig(data);
        }
    }

    public void LoadVersionInfo()
    {
        TextAsset ver = Resources.Load("Version") as TextAsset;
        if (ver)
        {
            string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
            var lines = Regex.Split(ver.text, LINE_SPLIT_RE);

            if (lines.Length > 0)
            {
                string strCode = lines[0].Trim('\n');
                strCode = strCode.Trim('\r');

                if (string.IsNullOrEmpty(strCode))
                    BUILD_CODE = -1;
                else
                    BUILD_CODE = int.Parse(strCode);
            }

            if (lines.Length > 1)
            {
#if UNITY_EDITOR
                BUILD_BRANCH = "EDITOR";
#else
                BUILD_BRANCH = lines[1].Trim('\n');
                BUILD_BRANCH = BUILD_BRANCH.Trim('\r');
                string[] tmp = BUILD_BRANCH.Split('/');
                BUILD_BRANCH = tmp[tmp.Length - 1];
#endif
            }

            VERSION = Application.version + "." + BUILD_CODE.ToString();
        }
    }


    public void LoadConfig(ConfigGameData data)
    {
        switch (data.key)
        {
            case "reward_gold":
                REWARD_GOLD_RATIO = 0.001f * int.Parse(data.value);
                break;
            case "reward_exp":
                REWARD_CHARACTER_EXP_RATIO = 0.001f * int.Parse(data.value);
                break;
            case "reward_account_exp":
                REWARD_EXP_RATIO = 0.001f * int.Parse(data.value);
                break;
            case "max_char_level":
                MAX_CHARACTER_LEVEL = int.Parse(data.value);
                break;
            case "max_char_reinforce":
                MAX_CHARACTER_REINFORCE = int.Parse(data.value);
                break;
            case "max_skill_level":
                MAX_CHARACTER_SKILL_LEVEL = int.Parse(data.value);
                break;
            case "clone_projectile_fadeout_time":
                CLONE_PROJECTILE_FADEOUT_TIME = int.Parse(data.value);
                break;
            case "battery_drop_lifetime":
                DROPPED_BATTERY_LIFETIME = int.Parse(data.value) * 0.001f;
                break;
            case "log_message_lifetime":
                GAMELOG_MESSAGE_LIFETIME = int.Parse(data.value) * 0.001f;
                break;
            case "log_message_fadeout_time":
                GAMELOG_MESSAGE_FADEOUT_TIME = int.Parse(data.value) * 0.001f;
                break;
            case "move_pad_sensitivity":
                MOVE_PAD_SENSITIVITY = int.Parse(data.value) * 0.01f;
                break;
            case "move_direction_count":
                MOVE_DIRECTION_COUNT = int.Parse(data.value);
                break;
            case "minimum_client_version_1":
                {
                    string[] curVer = VERSION.Split('.');

                    if (IS_VERSION_VALIDATE == true)
                    {
                        IS_VERSION_VALIDATE = false;

                        if (curVer.Length > 0 && int.Parse(curVer[0]) >= int.Parse(data.value))
                        {
                            IS_VERSION_VALIDATE = true;
                        }
                    }
                }
                break;
            case "minimum_client_version_2":
                {
                    string[] curVer = VERSION.Split('.');

                    if (IS_VERSION_VALIDATE == true)
                    {
                        IS_VERSION_VALIDATE = false;

                        if (curVer.Length > 1 && int.Parse(curVer[1]) >= int.Parse(data.value))
                        {
                            IS_VERSION_VALIDATE = true;
                        }
                    }
                }
                break;
            case "minimum_client_version_3":
                {
                    string[] curVer = VERSION.Split('.');

                    if (IS_VERSION_VALIDATE == true)
                    {
                        IS_VERSION_VALIDATE = false;

                        if (curVer.Length > 2 && int.Parse(curVer[2]) >= int.Parse(data.value))
                        {
                            IS_VERSION_VALIDATE = true;
                        }
                    }
                }
                break;
            case "ios_coupon_version":
#if UNITY_IOS
                IS_COUPON_VALIDATE = data.value == VERSION;
#endif
                break;
            case "shop_random_refreshtime":
                RANDOM_SHOP_REFRESH_TIME = (int)(long.Parse(data.value) * 0.001f);
                break;
            case "shop_random_refreshprice":
                RANDOM_SHOP_REFRESH_PRICE = int.Parse(data.value);
                break;
            case "reinforce_per_max_level":
                REINFORCE_PER_MAX_LEVEL = int.Parse(data.value);
                break;
            case "small_exp_item_gold":
                SMALL_EXP_ITEM_GOLD = int.Parse(data.value);
                break;
            case "medium_exp_item_gold":
                MEDIUM_EXP_ITEM_GOLD = int.Parse(data.value);
                break;
            case "large_exp_item_gold":
                LARGE_EXP_ITEM_GOLD = int.Parse(data.value);
                break;
            case "support_url":
                SUPPORT_URL = data.value;
                break;

            case "talent1_soul":
                TALENT1_SOUL = int.Parse(data.value);
                break;
            case "talent1_gold":
                TALENT1_GOLD = int.Parse(data.value);
                break;
            case "talent2_soul":
                TALENT2_SOUL = int.Parse(data.value);
                break;
            case "talent2_gold":
                TALENT2_GOLD = int.Parse(data.value);
                break;
            case "talent3_soul":
                TALENT3_SOUL = int.Parse(data.value);
                break;
            case "talent3_gold":
                TALENT3_GOLD = int.Parse(data.value);
                break;
            case "soul_stone_trade_c":
                SOUL_STONE_TRADE_C = int.Parse(data.value);
                break;
            case "soul_stone_trade_b":
                SOUL_STONE_TRADE_B = int.Parse(data.value);
                break;
            case "soul_stone_trade_a":
                SOUL_STONE_TRADE_A = int.Parse(data.value);
                break;
            case "soul_stone_trade_s":
                SOUL_STONE_TRADE_S = int.Parse(data.value);
                break;
            case "gacha_advertisement_time":
                GACHA_ADVERTISEMENT_TIME = int.Parse(data.value);
                break;
            case "max_talent_rank":
                MAX_TALENT_RANK = int.Parse(data.value);
                break;
            case "coupon_url":
                COUPON_URL = data.value;
                break;
            case "google_store_url":
                GOOGLE_STORE_URL = data.value;
                break;
            case "apple_store_url":
                APPLE_STORE_URL = data.value;
                break;
            case "cdn_url":
                CDN_URL = data.value;
                break;
#if UNITY_IOS
            case "sandbox_url_ios":
#else       
            case "sandbox_url_android":
#endif
                SANDBOX_URL = data.value;
                break;
            case "use_duo_match_service":
                USE_DUO_MATCH_SERVICE = int.Parse(data.value) > 0;
                break;
            case "use_custom_match_service":
                USE_CUSTOM_MATCH_SERVICE = int.Parse(data.value) > 0;
                break;

            case "server_inspection":
                IS_INSPECTION = int.Parse(data.value) > 0;
                break;
            case "resource_bundle_version":
                RESOURCE_BUNDLE_VERSION = int.Parse(data.value);
                break;
            case "default_clan_headcount":
                DEFAULT_CLAN_HEADCOUNT = int.Parse(data.value);
                break;
            case "clan_make_point":
                CLAN_MAKE_POINT = int.Parse(data.value);
                break;
            case "clan_make_gold":
                CLAN_MAKE_GOLD = int.Parse(data.value);
                break;
            case "move_packet_delay":
                MOVE_PACKET_DELAY = int.Parse(data.value) * 0.001f;
                break;
            case "tooltip_offset":
                TOOLTIP_OFFSET = int.Parse(data.value);
                break;
            case "christmas_normal":
                CHRISTMAS_NORAML = int.Parse(data.value);
                break;
            case "christmas_rare":
                CHRISTMAS_RARE = int.Parse(data.value);
                break;
            case "christmas_unique":
                CHRISTMAS_UNIQUE = int.Parse(data.value);
                break;
            case "newyear_skip_flag":
                NEWYEAR_SKIP_FLAG = int.Parse(data.value);
                break;
            case "special_talent_item_no":
                SPECIAL_TALENT_ITEM_NO = int.Parse(data.value);
                break;
            case "newyear_rate_page":
                NEWYEAR_RATE_PAGE = data.value;
                break;
            case "newyear_noti_time":
                NEWYEAR_NOTI_TIME = int.Parse(data.value) * 0.001f;
                break;
            case "ad_reward_use":
                AD_REWARD_USE = int.Parse(data.value);
                break;
            case "ad_reward_multiple":
                AD_REWARD_MULTIPLE = int.Parse(data.value) * 0.001f;
                break;

            default://클라에서 쓰지않는 Config들도 존재함.
                break;
        }
    }

    public int GetHotTimeFlag()
    {
        int ret = 0;
        DateTime now = SBUtil.KoreanTime;

        try
        {
            foreach (ConfigGameData cd in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.config))
            {
                switch (cd.key)
                {
                    case "hot_time_date":
                        {
                            string[] ts = cd.value.Split('/');
                            if (ts.Length > 1)
                            {
                                DateTime s = DateTime.Parse(ts[0]);
                                DateTime e = DateTime.Parse(ts[1]).AddDays(1);
                                if (s < now && now < e)
                                {
                                    foreach (ConfigGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.config))
                                    {
                                        switch (data.key)
                                        {
                                            case "hot_time_hour":
                                            {
                                                string[] repeat = data.value.Split('/');
                                                foreach(string once in repeat)
                                                {
                                                    string[] times = once.Split('~');
                                                    if (times.Length > 1)
                                                    {
                                                        string[] start = times[0].Split(':');
                                                        string[] end = times[1].Split(':');

                                                        DateTime time = SBUtil.KoreanTime;
                                                        time = new DateTime(time.Year, time.Month, time.Day);

                                                        DateTime stime = time;
                                                        DateTime etime = time;
                                                        stime = stime.AddHours(double.Parse(start[0]));
                                                        stime = stime.AddMinutes(double.Parse(start[1]));
                                                        etime = etime.AddHours(double.Parse(end[0]));
                                                        etime = etime.AddMinutes(double.Parse(end[1]));

                                                        if (stime < now && now < etime)
                                                        {
                                                            ret += 1;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case "hot_time_item_date":
                        {
                            string[] ts = cd.value.Split('/');
                            if (ts.Length > 1)
                            {
                                DateTime s = DateTime.Parse(ts[0]);
                                DateTime e = DateTime.Parse(ts[1]).AddDays(1);
                                if (s < now && now < e)
                                {
                                    foreach (ConfigGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.config))
                                    {
                                        switch (data.key)
                                        {
                                            case "hot_time_item_hour":
                                            {
                                                string[] repeat = data.value.Split('/');
                                                foreach (string once in repeat)
                                                {
                                                    string[] times = once.Split('~');
                                                    if (times.Length > 1)
                                                    {
                                                        string[] start = times[0].Split(':');
                                                        string[] end = times[1].Split(':');

                                                            DateTime time = SBUtil.KoreanTime;
                                                            time = new DateTime(time.Year, time.Month, time.Day);

                                                            DateTime stime = time;
                                                            DateTime etime = time;
                                                            stime = stime.AddHours(double.Parse(start[0]));
                                                            stime = stime.AddMinutes(double.Parse(start[1]));
                                                            etime = etime.AddHours(double.Parse(end[0]));
                                                            etime = etime.AddMinutes(double.Parse(end[1]));

                                                            if (stime < now && now < etime)
                                                            {
                                                                ret += 2;
                                                                break;
                                                            }
                                                        }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }
        catch
        {
            return 0;
        }

        return ret;
    }

    public void SetDummy(bool enable)
    {
        USE_DUMMY = enable;
    }

    public void SetDummyChaserPlay(bool chaser)
    {
        DUMMY_PLAYCHASER = chaser;
    }

    public void SetWrongClientVersion()
    {
        IS_VERSION_VALIDATE = false;
    }

#if SB_TEST || UNITY_EDITOR
    public void SetTestServer()
    {
        USE_TEST_SERVER = true;
    }

    public void SetQAServer()
    {
        USE_QA_SERVER = true;
    }
#endif

    public void SetBundleVersion(int version)
    {
        RESOURCE_BUNDLE_VERSION = version;
    }

    public void SetCDNURL(string url)
    {
        CDN_URL = url;
    }
}

public class ConfigGameData : GameData
{
    public string key { get; private set; }
    public string value { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        key = data["key"];
        value = data["value1"];

        GameConfig.Instance.LoadConfig(this);
    }
}
