using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GameDataManager
{
    public static void SetSaveGameData(string key, string value)
    {
        PlayerPrefs.SetString(GameConfig.Instance.VERSION + "/" + key.ToString(), FileReader.Encrypt(value));
    }

    public static string GetSavedGameData(DATA_TYPE type)
    {
        return GetSavedGameData(type.ToString());
    }

    public static string GetSavedGameData(string key)
    {
        string datakey = GameConfig.Instance.VERSION + "/" + key;
        if (PlayerPrefs.HasKey(datakey))
        {
            try
            {
                return FileReader.Decrypt(PlayerPrefs.GetString(datakey));
            }
            catch
            {
                Debug.LogError($"정말 {key} 쓰고있습니까?");
                return PlayerPrefs.GetString(datakey);
            }
        }

        return FileReader.ReadString("Data/" + key);
    }

    public static string GetMD5hashData(DATA_TYPE type)
    {
        string key = type.ToString();
        string datakey = GameConfig.Instance.VERSION + "/" + key;
        string data = FileReader.ReadOriginString("Data/" + key);
        if (PlayerPrefs.HasKey(datakey))
        {
            data = PlayerPrefs.GetString(datakey);
        }

        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] byteArray = Encoding.UTF8.GetBytes(data);
        MemoryStream stream = new MemoryStream(byteArray);
        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty).ToLower();
    }

    public static string GetMD5hashData(string file)
    {
        string datakey = GameConfig.Instance.VERSION + "/" + file;
        string data = null;
        
        if (PlayerPrefs.HasKey(datakey))
        {
            data = PlayerPrefs.GetString(datakey);
        }

        if (string.IsNullOrEmpty(data))
        {
            data = FileReader.ReadOriginString("Data/" + file);            
        }

        if (string.IsNullOrEmpty(data))
        {
            return null;
        }

        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] byteArray = Encoding.UTF8.GetBytes(data);
        MemoryStream stream = new MemoryStream(byteArray);
        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty).ToLower();
    }

    private Dictionary<DATA_TYPE, List<GameData>> data = new Dictionary<DATA_TYPE, List<GameData>>();
    public enum DATA_TYPE
    {
        DATA_MIN = -1,

        config = 0,
        @object,//매우 좋지 못한 테이블 이름
        reward_point,
        @string,//매우 좋지 못한 테이블 이름

        character,
        character_level,
        character_reinforce,
        character_skill_level,
        object_hide,
        object_key,
        object_door,
        object_obstacle,
        object_vehicle,
        object_vent,

        // 기현 : 스킬 테이블들은 순서가 중요하다, 변경시 주의
        skill_effect,
        skill_summon,
        skill_base,
        skill,

        item,

        effect_resource_data,

        gacha_base,
        gacha_types,

        shop_menu,
        shop_goods,
        shop_random,//서버에 있어야될데이터, 임시구현때문에
        shop_package,//서버에 있어야될데이터, 임시구현때문에

        //battery_effect,
        object_battery_creater,
        object_battery_generator,

        map_type,
        rank_grade,

        quest_info,
        limited_quest_info,
        collection_char_group,
        event_banner,
        attendance,
        pass,
        pass_item,
        character_talent,
        ranking,
        ranking_reward,
        popup_define,
        subscription_item,
        sound_resource,
        event_schedule,
        event_bingo_reward,
        event_bingo_info,
        emoticon,
        clan_emblem,
        clan_level,
        equipment_config,
        equipment_info,
        equipment_level,
        equipment_reinforce,
        collection_buff,
        attendance_month,
        event_box,
        attendance_christmas,
        event_newyear,
        select_box,

        DATA_COUNT,
    }

    public void TempInit()//시트 동기화전 임시로 쓰기위해 1차 미니멈 로드
    {
        data.Add(DATA_TYPE.@string, GameData.LoadGameData(DATA_TYPE.@string, "Data/" + DATA_TYPE.@string.ToString()));
        data.Add(DATA_TYPE.config, GameData.LoadGameData(DATA_TYPE.config, "Data/" + DATA_TYPE.config.ToString()));
    }

    public void Init()
    {
        LoadTableAll();

        GameConfig.Instance.LoadConfig();
        StringManager.Instance.CacheClear();
    }

    public IEnumerator InitAsync()
    {
        yield return LoadTableAllAsync();

        GameConfig.Instance.LoadConfig();
        yield return new WaitForEndOfFrame();
        StringManager.Instance.CacheClear();
        yield return new WaitForEndOfFrame();
    }

    public void LoadTable(DATA_TYPE type)
    {
        data.Clear();
        data.Add(type, GameData.LoadGameData(type, "Data/" + type.ToString()));
    }

    public void LoadTableAll()
    {
        data.Clear();

        for (DATA_TYPE type = DATA_TYPE.DATA_MIN + 1; type < DATA_TYPE.DATA_COUNT; type++)
        {
#if !UNITY_EDITOR//에디터에서는 문제부분 로깅을 위해 일부로 진행중단시킨다.
            try
#endif
            {
                if (!data.ContainsKey(type))
                {
                    data.Add(type, GameData.LoadGameData(type, "Data/" + type.ToString()));
                }
            }
#if !UNITY_EDITOR
            catch
            {
                SBDebug.LogError("=-=-=-=-=- " + type.ToString() + " is Error -=-=-=-=-=");
            }
#endif
        }
    }

    public IEnumerator LoadTableAllAsync()
    {
        data.Clear();

        for (DATA_TYPE type = DATA_TYPE.DATA_MIN + 1; type < DATA_TYPE.DATA_COUNT; type++)
        {
#if !UNITY_EDITOR//에디터에서는 문제부분 로깅을 위해 일부로 진행중단시킨다.
            try
#endif
            {
                if (!data.ContainsKey(type))
                {
                    data.Add(type, GameData.LoadGameData(type, "Data/" + type.ToString()));
                }
            }
#if !UNITY_EDITOR
            catch
            {
                SBDebug.LogError("=-=-=-=-=- " + type.ToString() + " is Error -=-=-=-=-=");
            }
#endif
            yield return new WaitForEndOfFrame();
        }
    }

    public List<GameData> GetData(DATA_TYPE type, bool quiet = false)
    {
        if (data.ContainsKey(type))
            return data[type];
        else
        {
            if (!quiet)
                SBDebug.LogError($"=-=-=-=-=- No Data : {type} -=-=-=-=-=");
            return new List<GameData>();
        }
    }

    public GameData GetData(DATA_TYPE type, int id, bool quiet = false)
    {
        foreach (GameData d in data[type])
        {
            if (d.GetID() == id)
                return d;
        }

        if (!quiet)
            SBDebug.LogError($"=-=-=-=-=- No Data : {type} / {id} -=-=-=-=-=");

        return null;
    }

    static public GameDataManager Instance
    {
        get
        {
            return Managers.Data;
        }
    }

    public static GameData NewGameData(GameDataManager.DATA_TYPE type)
    {
        GameData ret = null;
        switch (type)
        {
            case DATA_TYPE.config:
                ret = new ConfigGameData();
                break;
            case DATA_TYPE.reward_point:
                ret = new RewardGameData();
                break;
            case DATA_TYPE.@string:
                ret = new StringsGameData();
                break;

            case DATA_TYPE.@object:
                ret = new ObjectGameData();
                break;
            case DATA_TYPE.object_hide:
                ret = new ObjectHideGameData();
                break;
            case DATA_TYPE.object_key:
                ret = new ObjectKeyGameData();
                break;
            case DATA_TYPE.object_door:
                ret = new ObjectDoorGameData();
                break;
            case DATA_TYPE.object_obstacle:
                ret = new ObjectObsturctGameData();
                break;
            case DATA_TYPE.object_vehicle:
                ret = new ObjectVehicleGameData();
                break;
            case DATA_TYPE.object_vent:
                ret = new ObjectVentGameData();
                break;
            case DATA_TYPE.object_battery_creater:
                ret = new ObjectBatteryCreatorGameData();
                break;
            case DATA_TYPE.object_battery_generator:
                ret = new ObjectBatteryGeneratorGameData();
                break;

            case DATA_TYPE.character:
                ret = new CharacterGameData();
                break;
            case DATA_TYPE.character_level:
                ret = new CharacterLevelGameData();
                break;
            case DATA_TYPE.character_reinforce:
                ret = new CharacterReinforceGameData();
                break;
            case DATA_TYPE.character_skill_level:
                ret = new CharacterSkillLevelGameData();
                break;

            case DATA_TYPE.skill:
                ret = new SkillGameData();
                break;

            case DATA_TYPE.skill_base:
                ret = new SkillBaseGameData();
                break;

            case DATA_TYPE.skill_effect:
                ret = new SkillEffectGameData();
                break;

            case DATA_TYPE.skill_summon:
                ret = new SkillSummonGameData();
                break;

            case DATA_TYPE.item:
                ret = new ItemGameData();
                break;
            case DATA_TYPE.effect_resource_data:
                ret = new EffectResourceGameData();
                break;

            case DATA_TYPE.gacha_base:
                ret = new GachaGameData();
                break;
            case DATA_TYPE.gacha_types:
                ret = new GachaTypesGameData();
                break;

            case DATA_TYPE.shop_menu:
                ret = new ShopMenuGameData();
                break;
            case DATA_TYPE.shop_goods:
                ret = new ShopItemGameData();
                break;
            case DATA_TYPE.shop_package:
                ret = new ShopPackageGameData();
                break;
            case DATA_TYPE.map_type:
                ret = new MapTypeGameData();
                break;
            case DATA_TYPE.rank_grade:
                ret = new RankType();
                break;
            case DATA_TYPE.quest_info:
                ret = new QuestData();
                break;
            case DATA_TYPE.limited_quest_info:
                ret = new LimitedQuestInfoData();
                break;
            case DATA_TYPE.collection_char_group:
                ret = new CollectionCharGroupData();
                break;
            case DATA_TYPE.event_banner:
                ret = new BannerData();
                break;
            case DATA_TYPE.attendance:
                ret = new AttendanceGameData();
                break;
            case DATA_TYPE.pass:
                ret = new PassGameData();
                break;
            case DATA_TYPE.pass_item:
                ret = new PassItemGameData();
                break;
            case DATA_TYPE.character_talent:
                ret = new CharacterTalent();
                break;
            case DATA_TYPE.ranking:
                ret = new RankingGameData();
                break;
            case DATA_TYPE.ranking_reward:
                ret = new RankingRewardData();
                break;
            case DATA_TYPE.popup_define:
                ret = new PopupDefine();
                break;
            case DATA_TYPE.subscription_item:
                ret = new SubscriptionItem();
                break;
            case DATA_TYPE.sound_resource:
                ret = new SoundResourceData();
                break;
            case DATA_TYPE.event_schedule:
                ret = new EventScheduleData();
                break;
            case DATA_TYPE.event_bingo_reward:
                ret = new EventBingoRewardData();
                break;
            case DATA_TYPE.event_bingo_info:
                ret = new EventBingoInfoData();
                break;
            case DATA_TYPE.emoticon:
                ret = new EmoticonItemData();
                break;
            case DATA_TYPE.clan_emblem:
                ret = new ClanEmblemData();
                break;
            case DATA_TYPE.clan_level:
                ret = new ClanLevelData();
                break;
            case DATA_TYPE.equipment_config:
                ret = new EquipConfig();
                break;
            case DATA_TYPE.equipment_info:
                ret = new EquipInfo();
                break;
            case DATA_TYPE.equipment_level:
                ret = new EquipLevel();
                break;
            case DATA_TYPE.equipment_reinforce:
                ret = new EquipReinforce();
                break;
            case DATA_TYPE.collection_buff:
                ret = new CollectionBuff();
                break;
            //case DATA_TYPE.attendance_month:
            //    ret = new AttendanceMonthGameData();
            //    break;
            case DATA_TYPE.event_box:
                ret = new EventBoxData();
                break;
            //case DATA_TYPE.attendance_christmas:
            //    ret = new XmasAttendanceGameData();
            //    break;
            case DATA_TYPE.event_newyear:
                ret = new EventNewYear();
                break;
            case DATA_TYPE.select_box:
                ret = new SelectBox();
                break;

            default:
                ret = new GameData();
                break;
        }

        ret.SetDataType(type);
        return ret;
    }

    public bool UseLocalData()
    {
#if UNITY_EDITOR
        return PlayerPrefs.GetInt("UseLocalData", 0) > 0;
#else
        return false;
#endif
    }
    public void SetUseLocalData(bool use)
    {
#if UNITY_EDITOR
        PlayerPrefs.SetInt("UseLocalData", use ? 1 : 0);
#endif
    }
}
