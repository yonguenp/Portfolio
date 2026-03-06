using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public abstract class game_data
{
    public abstract GameDataManager.DATA_TYPE GetDataType();
    public Dictionary<string, object> data = new Dictionary<string, object>();  

    public void SetData(JArray key, JArray valType, JArray value)
    {
        if(key.Count == value.Count && key.Count == valType.Count)
        {
            for(int i = 0; i < key.Count; i++)
            {
                string type = valType[i].ToString();
                if (type == "i")
                    data.Add(key[i].ToString(), value[i].Value<uint>());
                else if (type == "s")
                    data.Add(key[i].ToString(), value[i].Value<string>());
                else if (type == "f")
                    data.Add(key[i].ToString(), value[i].Value<float>());
                else if (type == "si")
                    data.Add(key[i].ToString(), value[i].Value<int>());
            }
        }
    }

    public static game_data CreateGameData(GameDataManager.DATA_TYPE type)
    {
        switch (type)
        {
            case GameDataManager.DATA_TYPE.CARD_GRADE :return new card_grade();      
            case GameDataManager.DATA_TYPE.CARD_DEFINE :return new card_define();     
            case GameDataManager.DATA_TYPE.CARD_COLLECTION :return new card_collection(); 
            case GameDataManager.DATA_TYPE.ITEMS :return new items();           
            case GameDataManager.DATA_TYPE.ACHIEVEMENT :return new achievement();     
            case GameDataManager.DATA_TYPE.ACHV_ACTION :return new achv_action();     
            case GameDataManager.DATA_TYPE.INTER_TOUCH :return new inter_touch();     
            case GameDataManager.DATA_TYPE.INTER_PLAY :return new inter_play();      
            case GameDataManager.DATA_TYPE.FOOD :return new food();            
            case GameDataManager.DATA_TYPE.RECIPE :return new recipe();          
            case GameDataManager.DATA_TYPE.COOK :return new cook();            
            case GameDataManager.DATA_TYPE.USER_CARD :return new user_card();       
            case GameDataManager.DATA_TYPE.USER_COLLECTION :return new user_collection(); 
            case GameDataManager.DATA_TYPE.USER_ITEMS :return new user_items();      
            case GameDataManager.DATA_TYPE.USER_ACHIEVEMENT :return new user_achievement();
            case GameDataManager.DATA_TYPE.USER_ACHV_ACTION :return new user_achv_action();
            case GameDataManager.DATA_TYPE.USER_INTER_TOUCH :return new user_inter_touch();
            case GameDataManager.DATA_TYPE.USER_INTER_PLAY :return new user_inter_play(); 
            case GameDataManager.DATA_TYPE.USER_FOOD :return new user_food();       
            case GameDataManager.DATA_TYPE.USERS :return new users();
            case GameDataManager.DATA_TYPE.EXP: return new exp();
            case GameDataManager.DATA_TYPE.CONTENT_UNLOCK: return new content_unlock();
            case GameDataManager.DATA_TYPE.GACHA_TYPE: return new gacha_type();
            case GameDataManager.DATA_TYPE.WALK_EVENT: return new walk_event();
            case GameDataManager.DATA_TYPE.WALK_AREA: return new walk_area();
            case GameDataManager.DATA_TYPE.CLIP_EVENT: return new clip_event();
            case GameDataManager.DATA_TYPE.FOOD_FULLNESS: return new food_fullness();
            case GameDataManager.DATA_TYPE.ALBUM: return new album();
            case GameDataManager.DATA_TYPE.STAGE: return new stage();
            case GameDataManager.DATA_TYPE.LOCATION: return new location();
            case GameDataManager.DATA_TYPE.USER_STAGE: return new user_stage();
            case GameDataManager.DATA_TYPE.CAT_DEF: return new cat_def();
            case GameDataManager.DATA_TYPE.CAT_LEVEL_DEF: return new cat_level_def();
            case GameDataManager.DATA_TYPE.CAT_ACTION_DEF: return new cat_action_def();
            case GameDataManager.DATA_TYPE.LOCATION_DEF: return new location_def();
            case GameDataManager.DATA_TYPE.ANI_LIST: return new ani_list();
            case GameDataManager.DATA_TYPE.USER_CATS: return new user_cats();
            case GameDataManager.DATA_TYPE.PLANTS: return new plants();
            case GameDataManager.DATA_TYPE.PLANT_TYPES: return new plant_types();
            case GameDataManager.DATA_TYPE.NECO_CAT: return new neco_cat();
            case GameDataManager.DATA_TYPE.NECO_MAP: return new neco_map();
            case GameDataManager.DATA_TYPE.NECO_SPOT: return new neco_spot();
            case GameDataManager.DATA_TYPE.NECO_GIFT: return new neco_gift();
            case GameDataManager.DATA_TYPE.NECO_ACTION: return new neco_action();
            case GameDataManager.DATA_TYPE.NECO_USER_CAT: return new neco_user_cat();
            case GameDataManager.DATA_TYPE.NECO_FOOD: return new neco_food();
            case GameDataManager.DATA_TYPE.NECO_LEVEL: return new neco_level();
            case GameDataManager.DATA_TYPE.OBJECTS: return new objects();
            case GameDataManager.DATA_TYPE.NECO_FISH_TRAP_RATE: return new neco_fish_trap_rate();
            case GameDataManager.DATA_TYPE.NECO_CAT_MEMORY: return new neco_cat_memory();
            case GameDataManager.DATA_TYPE.NECO_OBJECT_DURABILITY: return new neco_object_durability();
            case GameDataManager.DATA_TYPE.NECO_MISSION: return new neco_mission();
            case GameDataManager.DATA_TYPE.NECO_PASS: return new neco_pass();
            case GameDataManager.DATA_TYPE.NECO_PASS_REWARD_FOREVER: return new neco_pass_reward_forever();
            case GameDataManager.DATA_TYPE.NECO_SHOP: return new neco_shop();
            case GameDataManager.DATA_TYPE.NECO_PACKAGE: return new neco_package();
            case GameDataManager.DATA_TYPE.NECO_MARKET: return new neco_market();
            case GameDataManager.DATA_TYPE.NECO_CAT_OUTBREAK: return new neco_cat_outbreak();
            case GameDataManager.DATA_TYPE.NECO_OBJECT_MAPS: return new neco_object_maps();
            case GameDataManager.DATA_TYPE.NECO_OBJECT_SLOTS: return new neco_object_slots();
            case GameDataManager.DATA_TYPE.NECO_BOOSTER: return new neco_booster();
            case GameDataManager.DATA_TYPE.NECO_PHOTO_POOL_DATA: return new neco_photo_pool_data();
            case GameDataManager.DATA_TYPE.NECO_CHECK_REWARD: return new neco_check_reward();
            case GameDataManager.DATA_TYPE.NECO_MONTHLY_REWARD: return new neco_monthly_reward();
            case GameDataManager.DATA_TYPE.NECO_GIFT_BASKET: return new neco_gift_basket();
            case GameDataManager.DATA_TYPE.NECO_BOX: return new neco_box();
            case GameDataManager.DATA_TYPE.GAME_CONFIG: return new game_config();
            case GameDataManager.DATA_TYPE.CARD_DEFINE_SUB: return new card_define_sub();
            case GameDataManager.DATA_TYPE.NECO_SUBSCRIBE: return new neco_subscribe();
            case GameDataManager.DATA_TYPE.NECO_FISH_TRADE: return new neco_fish_trade();
            case GameDataManager.DATA_TYPE.NECO_EVENT_THANKS_SHOP: return new neco_event_thanks_shop();
            case GameDataManager.DATA_TYPE.NECO_EVENT_THANKS_ATTENDANCE: return new neco_event_thanks_attendance();
            case GameDataManager.DATA_TYPE.FISHING_REWARD: return new fishing_reward();
            case GameDataManager.DATA_TYPE.NECO_EVENT_HALLOWEEN_ATTENDANCE: return new neco_event_halloween_attendance();
            case GameDataManager.DATA_TYPE.CATNIP_FARM: return new catnip_farm();
            case GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_ATTENDANCE: return new neco_event_xmas_attendance();
            case GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_MISSIONS: return new neco_event_xmas_missions();
            case GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_GACHA: return new neco_event_xmas_gacha();
            case GameDataManager.DATA_TYPE.NECO_ADMIN: return new neco_admin();
            case GameDataManager.DATA_TYPE.NECO_PASS_FOREVER: return new neco_pass_forever();
        }

        return null;
    }
}

[Serializable]
public class GameDataManager
{
    public enum DATA_TYPE
    {
        DATA_TYPE_NONE = -1,
        CARD_GRADE,
        CARD_DEFINE,
        CARD_COLLECTION,
        ITEMS,
        ACHIEVEMENT,
        ACHV_ACTION,
        INTER_TOUCH,
        INTER_PLAY,
        FOOD,
        RECIPE,
        COOK,
        USER_CARD,
        USER_COLLECTION,
        USER_ITEMS,
        USER_ACHIEVEMENT,
        USER_ACHV_ACTION,
        USER_INTER_TOUCH,
        USER_INTER_PLAY,
        USER_FOOD,
        USERS,
        EXP,
        CONTENT_UNLOCK,
        GACHA_TYPE,
        WALK_EVENT,
        WALK_AREA,
        CLIP_EVENT,
        FOOD_FULLNESS,
        ALBUM,
        STAGE,
        LOCATION,
        USER_STAGE,
        CAT_DEF,
        CAT_LEVEL_DEF,
        CAT_ACTION_DEF,
        LOCATION_DEF,
        ANI_LIST,
        USER_CATS,
        PLANTS,
        PLANT_TYPES,
        NECO_CAT,
        NECO_MAP,
        NECO_SPOT,
        NECO_GIFT,
        NECO_ACTION,
        NECO_USER_CAT,       
        NECO_FOOD,
        NECO_LEVEL,
        OBJECTS,
        NECO_FISH_TRAP_RATE,
        NECO_CAT_MEMORY,
        NECO_OBJECT_DURABILITY,
        NECO_MISSION,
        NECO_PASS,
        NECO_PASS_REWARD_FOREVER,
        NECO_SHOP,
        NECO_PACKAGE,
        NECO_MARKET,
        NECO_CAT_OUTBREAK,
        NECO_OBJECT_MAPS,
        NECO_OBJECT_SLOTS,
        NECO_BOOSTER,
        NECO_PHOTO_POOL_DATA,
        NECO_CHECK_REWARD,
        NECO_MONTHLY_REWARD,
        NECO_GIFT_BASKET,
        NECO_BOX,
        GAME_CONFIG,
        CARD_DEFINE_SUB,
        NECO_SUBSCRIBE,
        NECO_FISH_TRADE,
        NECO_EVENT_THANKS_SHOP,
        NECO_EVENT_THANKS_ATTENDANCE,        
        FISHING_REWARD,
        NECO_EVENT_HALLOWEEN_ATTENDANCE,
        CATNIP_FARM,
        NECO_EVENT_XMAS_ATTENDANCE,
        NECO_EVENT_XMAS_MISSIONS,
        NECO_EVENT_XMAS_GACHA,
        NECO_ADMIN,
        NECO_PASS_FOREVER
    };

    private static GameDataManager instance = null;

    public static GameDataManager Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new GameDataManager();

#if UNITY_EDITOR
                instance.LoadGameData();
#endif
            }
            return instance;
        }
    }

    private Dictionary<DATA_TYPE, List<game_data>> data = new Dictionary<DATA_TYPE, List<game_data>>();
    
    public DATA_TYPE GetStringToDataType(string strType)
    {
        switch(strType)
        {
            case "card_grade": return DATA_TYPE.CARD_GRADE;
            case "card_define": return DATA_TYPE.CARD_DEFINE;
            case "card_collection": return DATA_TYPE.CARD_COLLECTION;
            case "items": return DATA_TYPE.ITEMS;
            case "achievement": return DATA_TYPE.ACHIEVEMENT;
            case "achv_action": return DATA_TYPE.ACHV_ACTION;
            case "inter_touch": return DATA_TYPE.INTER_TOUCH;
            case "inter_play": return DATA_TYPE.INTER_PLAY;
            case "food": return DATA_TYPE.FOOD;
            case "recipe": return DATA_TYPE.RECIPE;
            case "cook": return DATA_TYPE.COOK;
            case "user_card": return DATA_TYPE.USER_CARD;
            case "user_collection": return DATA_TYPE.USER_COLLECTION;
            case "user_items": return DATA_TYPE.USER_ITEMS;
            case "user_achievement": return DATA_TYPE.USER_ACHIEVEMENT;
            case "user_achv_action": return DATA_TYPE.USER_ACHV_ACTION;
            case "user_inter_touch": return DATA_TYPE.USER_INTER_TOUCH;
            case "user_inter_play": return DATA_TYPE.USER_INTER_PLAY;
            case "user_food": return DATA_TYPE.USER_FOOD;
            case "users": return DATA_TYPE.USERS;
            case "exp": return DATA_TYPE.EXP;
            case "content_unlock": return DATA_TYPE.CONTENT_UNLOCK;
            case "gacha_type": return DATA_TYPE.GACHA_TYPE;            
            case "walk_event": return DATA_TYPE.WALK_EVENT;
            case "walk_area": return DATA_TYPE.WALK_AREA;
            case "clip_event": return DATA_TYPE.CLIP_EVENT;
            case "food_fullness": return DATA_TYPE.FOOD_FULLNESS;
            case "album": return DATA_TYPE.ALBUM;
            case "stage": return DATA_TYPE.STAGE;
            case "location": return DATA_TYPE.LOCATION;
            case "user_stage": return DATA_TYPE.USER_STAGE;
            case "cat_def": return DATA_TYPE.CAT_DEF;
            case "cat_level_def": return DATA_TYPE.CAT_LEVEL_DEF;
            case "cat_action_def": return DATA_TYPE.CAT_ACTION_DEF;
            case "location_def": return DATA_TYPE.LOCATION_DEF;
            case "ani_list": return DATA_TYPE.ANI_LIST;
            case "user_cats": return DATA_TYPE.USER_CATS;
            case "plants": return DATA_TYPE.PLANTS;
            case "plant_types": return DATA_TYPE.PLANT_TYPES;
            case "neco_cat": return DATA_TYPE.NECO_CAT;
            case "neco_map": return DATA_TYPE.NECO_MAP;
            case "neco_spot": return DATA_TYPE.NECO_SPOT;
            case "neco_gift": return DATA_TYPE.NECO_GIFT;
            case "neco_action": return DATA_TYPE.NECO_ACTION;
            case "neco_user_cat": return DATA_TYPE.NECO_USER_CAT;
            case "neco_food": return DATA_TYPE.NECO_FOOD;
            case "neco_level": return DATA_TYPE.NECO_LEVEL;
            case "objects": return DATA_TYPE.OBJECTS;
            case "neco_fish_trap_rate": return DATA_TYPE.NECO_FISH_TRAP_RATE;
            case "neco_cat_memory": return DATA_TYPE.NECO_CAT_MEMORY;
            case "neco_object_durability": return DATA_TYPE.NECO_OBJECT_DURABILITY;
            case "neco_mission": return DATA_TYPE.NECO_MISSION;
            case "neco_pass": return DATA_TYPE.NECO_PASS;
            case "neco_pass_reward_forever": return DATA_TYPE.NECO_PASS_REWARD_FOREVER;
            case "neco_shop": return DATA_TYPE.NECO_SHOP;
            case "neco_package": return DATA_TYPE.NECO_PACKAGE;
            case "neco_market": return DATA_TYPE.NECO_MARKET;
            case "neco_cat_outbreak": return DATA_TYPE.NECO_CAT_OUTBREAK;
            case "neco_object_maps": return DATA_TYPE.NECO_OBJECT_MAPS;
            case "neco_object_slots": return DATA_TYPE.NECO_OBJECT_SLOTS;
            case "neco_booster": return DATA_TYPE.NECO_BOOSTER;
            case "neco_photo_pool_data": return DATA_TYPE.NECO_PHOTO_POOL_DATA;
            case "neco_check_reward_new": return DATA_TYPE.NECO_CHECK_REWARD;
            case "neco_monthly_reward_new": return DATA_TYPE.NECO_MONTHLY_REWARD;
            case "neco_gift_basket": return DATA_TYPE.NECO_GIFT_BASKET;
            case "neco_box": return DATA_TYPE.NECO_BOX;
            case "game_config": return DATA_TYPE.GAME_CONFIG;
            case "card_define_sub": return DATA_TYPE.CARD_DEFINE_SUB;
            case "neco_subscribe": return DATA_TYPE.NECO_SUBSCRIBE;
            case "neco_fish_trade": return DATA_TYPE.NECO_FISH_TRADE;
            case "neco_event_thanks_shop": return DATA_TYPE.NECO_EVENT_THANKS_SHOP;
            case "neco_event_thanks_attendance": return DATA_TYPE.NECO_EVENT_THANKS_ATTENDANCE;
            case "fishing_reward": return DATA_TYPE.FISHING_REWARD;
            case "neco_event_halloween_attendance": return DATA_TYPE.NECO_EVENT_HALLOWEEN_ATTENDANCE;
            case "catnip_farm": return DATA_TYPE.CATNIP_FARM;
            case "neco_event_xmas_attendance": return DATA_TYPE.NECO_EVENT_XMAS_ATTENDANCE;
            case "neco_event_xmas_missions": return DATA_TYPE.NECO_EVENT_XMAS_MISSIONS;
            case "neco_event_xmas_gacha": return DATA_TYPE.NECO_EVENT_XMAS_GACHA;
            case "neco_admin": return DATA_TYPE.NECO_ADMIN;
            case "neco_pass_reward":
            case "neco_pass_forever": return DATA_TYPE.NECO_PASS_FOREVER;
            default:
                Debug.LogError("Not found DATA_TYPE : " + strType);
                break;
        }   

        return DATA_TYPE.DATA_TYPE_NONE;
    }

    public List<game_data> GetEmptyGameDataWithType(DATA_TYPE type)
    {
        data.Remove(type);
        List<game_data> data_array = new List<game_data>();
        data.Add(type, data_array);

        return GetGameData(type);
    }

    public void SetGameDataArray(string strType, JArray key, JArray val_type, JArray array)
    {
        DATA_TYPE type = GetStringToDataType(strType);
        if (type == DATA_TYPE.DATA_TYPE_NONE)
            return;

        Debug.Log("SetGameDataArray : " + strType);

        data.Remove(type);
        List<game_data> data_array = new List<game_data>();
        data.Add(type, data_array);

        foreach (JToken row in array)
        {
            game_data dt = game_data.CreateGameData(type);
            dt.SetData(key, val_type, (JArray)row);
            data_array.Add(dt);
        }
    }

    public void SetGameDataArray(DATA_TYPE type)
    {
        data.Remove(type);
        List<game_data> data_array = new List<game_data>();
        data.Add(type, data_array);
    }

    public void SetUserData(game_data obj)
    {
        data.Remove(DATA_TYPE.USERS);

        List<game_data> data_array = new List<game_data>();
        data.Add(DATA_TYPE.USERS, data_array);

        data_array.Add(obj);
    }

    public users GetUserData()
    {
        List<game_data> data_array;
        if(data.TryGetValue(DATA_TYPE.USERS,out data_array))
        {
            return (users)data_array[0];
        }

        return null;
    }

    public List<game_data> GetGameData(DATA_TYPE type)
    {
        List<game_data> ret;
        if(data.TryGetValue(type, out ret))
            return ret;

        return null;
    }

    public void SaveGameData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/GD.dat");

        //foreach(KeyValuePair<DATA_TYPE, List<game_data>> d in data)
        //{
        //    try
        //    {
        //        foreach (game_data a in d.Value)
        //        {
        //            try
        //            {
        //                foreach(KeyValuePair<string, object> t in a.data)
        //                {
        //                    try
        //                    {
        //                        bf.Serialize(file, t.Value);
        //                    }
        //                    catch
        //                    {
        //                        Debug.LogError(t.Key);
        //                    }
        //                }
                        
        //            }
        //            catch
        //            {
        //                Debug.LogError(a);
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        Debug.LogError(d.Key);
        //    }
        //}
        bf.Serialize(file, data);

        file.Close();

        PlayerPrefs.SetInt("last_data_sync", NetworkManager.GetInstance().ServerTime);

        //HHHCardManager.Instance.Init();
    }

    public void LoadGameData()
    {
        string path = Application.persistentDataPath + "/GD.dat";
        if (!File.Exists(path))
            return;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.OpenRead(path);

        try
        {
            data = (Dictionary<DATA_TYPE, List<game_data>>)bf.Deserialize(file);
        }
        catch
        {
            file.Close();

            SaveGameData();
        }

        file.Close();
    }


    string buildCode = "";
    bool isTestServer = false;

    public bool UseTestServer()
    {
        if (string.IsNullOrEmpty(buildCode))
        {
            GetVersionCode();
        }

        return isTestServer;
    }

    public string GetVersionCode()
    {
        if (string.IsNullOrEmpty(buildCode))
        {
            TextAsset data = Resources.Load("Version") as TextAsset;
            if (data)
            {
                buildCode = data.text.Trim('\n');
                buildCode = buildCode.Trim('\r');

                if (buildCode.Contains("T"))
                {
#if !UNITY_IOS
                    isTestServer = true;
#endif
                    buildCode = buildCode.Replace("T", "");
                }
            }
        }

        return buildCode;
    }

    string buildVer = "";
    public string GetVersion()
    {
        if (string.IsNullOrEmpty(buildVer))
        {
            buildVer = Application.version + "." + GetVersionCode();
        }

        return buildVer;
    }

    public string GetVersionWithFlag()
    {
        string version = GetVersion();
        if (isTestServer)
        {
            return version + "T";
        }
        else
        {
            return version + "R";
        }        
    }
}


public class UserProfile
{
    public enum FriendType { 
        NONE,
        FRIEND,
        SENT,
        DECLINED,
        TAKEN,
        RECOMMAND,
        BLOCKED,
    };

    public uint uno = 0;
    public string nick = "";
    public uint level = 0;
    public FriendType type = FriendType.NONE;
    public uint gift_flag = 0;
    public uint last_update = 0;
    public string lastMessage = "";
    
    public void UIShown(uint curTime)
    {
        string json = PlayerPrefs.GetString("UserProfile", "");
        try
        {
            JObject root;
            if (string.IsNullOrEmpty(json))
                root = new JObject();
            else
                root = JObject.Parse(json);            

            if (root.ContainsKey(uno.ToString()))
            {
                root[uno.ToString()] = curTime;
            }
            else
            {
                root.Add(uno.ToString(), curTime);
            }

            PlayerPrefs.SetString("UserProfile", root.ToString(Newtonsoft.Json.Formatting.None));
        }
        catch
        {
            PlayerPrefs.SetString("UserProfile", "");
        } 
    }
    
    public bool isAlarm 
    { 
        get {
            string json = PlayerPrefs.GetString("UserProfile", "");
            if (string.IsNullOrEmpty(json))
                return true;

            JObject root = JObject.Parse(json);
            if (root != null && root.ContainsKey(uno.ToString()))
            {
                return root[uno.ToString()].Value<uint>() < last_update;
            }
            else
            {
                return true;
            }
        } 
    }
}

public class FriendsManager
{
    private static FriendsManager instance = null;

    public static FriendsManager Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new FriendsManager();
            }
            return instance;
        }
    }

    private List<UserProfile> users = new List<UserProfile>();
    private uint newFriendCount = 0;
    private uint newReciveCount = 0;

    public List<UserProfile> GetFriendList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach(UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.FRIEND)
                ret.Add(user);
        }

        return ret;
    }

    public List<UserProfile> GetSentList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.SENT)
                ret.Add(user);
        }

        return ret;
    }

    public List<UserProfile> GetDeclinedList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.DECLINED)
                ret.Add(user);
        }

        return ret;
    }

    public List<UserProfile> GetTakenList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.TAKEN)
                ret.Add(user);
        }

        return ret;
    }
    public List<UserProfile> GetRecommandList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.RECOMMAND)
                ret.Add(user);
        }

        return ret;
    }

    public List<UserProfile> GetBlockedList()
    {
        List<UserProfile> ret = new List<UserProfile>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.BLOCKED)
                ret.Add(user);
        }

        return ret;
    }

    public List<string> GetBlockedAccountNoList()
    {
        List<string> ret = new List<string>();
        foreach (UserProfile user in users)
        {
            if (user.type == UserProfile.FriendType.BLOCKED)
                ret.Add(user.uno.ToString());
        }

        return ret;
    }

    public UserProfile UpdateUserProfile(JObject data, UserProfile.FriendType type)
    {
        uint un = data["uno"].Value<uint>();
        foreach (UserProfile user in users)
        {
            if(user.uno == un)
            {
                user.level = data["lvl"].Value<uint>();
                user.nick = data["nick"].Value<string>();
                user.type = type;

                if (data.ContainsKey("gift_flag"))
                    user.gift_flag = data["gift_flag"].Value<uint>();
                if (data.ContainsKey("last_update"))
                {
                    if(user.last_update < data["last_update"].Value<uint>())
                        user.last_update = data["last_update"].Value<uint>();
                }
                return user;
            }
        }

        UserProfile new_user = new UserProfile();
        new_user.uno = un;
        new_user.level = data["lvl"].Value<uint>();
        new_user.nick = data["nick"].Value<string>();
        new_user.type = type;

        if (data.ContainsKey("gift_flag"))
            new_user.gift_flag = data["gift_flag"].Value<uint>();
        if (data.ContainsKey("last_update"))
            new_user.last_update = data["last_update"].Value<uint>();

        users.Add(new_user);

        return new_user;
    }

    public void DeleteUserProfile(UserProfile profile)
    {
        users.Remove(profile);
    }

    public void DeleteUserProfiles(List<UserProfile> list)
    {
        foreach(UserProfile user in list)
        {
            DeleteUserProfile(user);
        }
    }

    public void SetNewFriendCount(uint newFriend)
    {
        newFriendCount = newFriend;
    }


    public void SetNewRecivedCount(uint newRecive)
    {
        newReciveCount = newRecive;
    }

    public uint GetNewFriendCount()
    {
        return newFriendCount;
    }

    public uint GetNewRecivedCount()
    {
        return newReciveCount;
    }
}

