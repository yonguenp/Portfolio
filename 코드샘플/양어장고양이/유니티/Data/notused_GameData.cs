using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class card_grade : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CARD_GRADE; }
}


[Serializable]
public class card_collection : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CARD_COLLECTION; }

    [NonSerialized]
    private uint CollectionID = 0;
    [NonSerialized]
    private string Title = "";
    [NonSerialized]
    private List<user_card> Condition = null;
    [NonSerialized]
    private JObject Reward = null;

    public uint GetCollectionID()
    {
        if (CollectionID == 0)
        {
            object obj;
            if (data.TryGetValue("collection_id", out obj))
            {
                CollectionID = (uint)obj;
            }
        }

        return CollectionID;
    }

    public string GetCollectionTitle()
    {
        if (string.IsNullOrEmpty(Title))
        {
            object obj;
            if (data.TryGetValue("collection_title_kr", out obj))
            {
                Title = (string)obj;
            }
        }

        return Title;
    }

    public List<user_card> GetCollectionCondition()
    {
        if (Condition == null)
        {
            object obj;
            if (data.TryGetValue("card_list", out obj))
            {
                string str = (string)obj;
                string[] strCondition = str.Split(',');
                foreach (string con in strCondition)
                {
                    string[] val = con.Split('_');
                    Condition = new List<user_card>();

                    user_card card = new user_card();
                    card.data.Add("card_id", uint.Parse(val[0]));
                    card.data.Add("card_lv", uint.Parse(val[1]));

                    Condition.Add(card);
                }
            }
        }

        return Condition;
    }

    public JObject GetCollectionReward()
    {
        if (Reward == null)
        {
            object obj;
            if (data.TryGetValue("reward", out obj))
            {
                Reward = JObject.Parse((string)obj);
            }
        }

        return Reward;
    }
}


[Serializable]
public class achievement : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.ACHIEVEMENT; }
}

[Serializable]
public class achv_action : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.ACHV_ACTION; }
}

[Serializable]
public class inter_touch : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.INTER_TOUCH; }
}

[Serializable]
public class inter_play : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.INTER_PLAY; }
}

[Serializable]
public class food : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.FOOD; }
}


[Serializable]
public class cook : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.COOK; }
}


[Serializable]
public class user_collection : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_COLLECTION; }
}


[Serializable]
public class user_achievement : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_ACHIEVEMENT; }
}

[Serializable]
public class user_achv_action : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_ACHV_ACTION; }
}

[Serializable]
public class user_inter_touch : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_INTER_TOUCH; }
}

[Serializable]
public class user_inter_play : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_INTER_PLAY; }
}

[Serializable]
public class user_food : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_FOOD; }
}


[Serializable]
public class exp : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.EXP; }
}

[Serializable]
public class content_unlock : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CONTENT_UNLOCK; }
}

[Serializable]
public class gacha_type : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.GACHA_TYPE; }
}

[Serializable]
public class walk_event : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.WALK_EVENT; }
}

[Serializable]
public class walk_area : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.WALK_AREA; }
}

[Serializable]
public class food_fullness : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.FOOD_FULLNESS; }
}


[Serializable]
public class stage : game_data
{
    [NonSerialized]
    private uint stage_id = 0;
    [NonSerialized]
    private uint location_id = 0;
    [NonSerialized]
    private uint max_star = 0;
    [NonSerialized]
    private string thumbnail = "";
    [NonSerialized]
    private uint needFullness = 0;


    public uint GetStageID()
    {
        if (stage_id == 0)
        {
            object obj;
            if (data.TryGetValue("stage_id", out obj))
            {
                stage_id = (uint)obj;
            }
        }

        return stage_id;
    }

    public uint GetLocationID()
    {
        if (location_id == 0)
        {
            object obj;
            if (data.TryGetValue("location_id", out obj))
            {
                location_id = (uint)obj;
            }
        }

        return location_id;
    }

    public string GetThumbnailPath()
    {
        if (string.IsNullOrEmpty(thumbnail))
        {
            object obj;
            if (data.TryGetValue("thumbnail", out obj))
            {
                thumbnail = (string)obj;
            }
        }

        return thumbnail;
    }

    public uint GetMaxStar()
    {
        if (max_star == 0)
        {
            object obj;
            if (data.TryGetValue("max_star", out obj))
            {
                max_star = (uint)obj;
            }
        }

        return max_star;
    }

    public int GetCurStageState()
    {
        List<game_data> user_statges = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_STAGE);
        if (user_statges != null)
        {
            uint stageID = GetStageID();
            object obj;
            foreach (game_data stage in user_statges)
            {
                if (stage.data.TryGetValue("stage_id", out obj))
                {
                    if (stageID == (uint)obj)
                    {
                        return (int)stage.data["flag"];
                    }
                }
            }
        }

        return -1;
    }

    public uint GetCurStar()
    {
        uint ret = 0;
        int state = GetCurStageState();

        if (state > 0)
        {
            ret = System.Convert.ToUInt32(state);
            if (ret > GetMaxStar())
            {
                ret = GetMaxStar();
            }
        }

        return ret;
    }

    public uint GetNeedFullness()
    {
        if (needFullness == 0)
        {
            object obj;
            if (data.TryGetValue("total_fullness", out obj))
            {
                needFullness = (uint)obj;
            }
        }

        return needFullness;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.STAGE; }
}


[Serializable]
public class location : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.LOCATION; }
}

[Serializable]
public class user_stage : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_STAGE; }
}


[Serializable]
public class cat_def : game_data
{
    static public cat_def GetCatInfo(uint catID)
    {
        List<game_data> cats = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CAT_DEF);
        foreach (game_data data in cats)
        {
            cat_def cat = (cat_def)data;
            if (cat.GetCatID() == catID)
            {
                return cat;
            }
        }

        return null;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CAT_DEF; }

    [NonSerialized]
    uint cat_id = 0;
    [NonSerialized]
    string name = "";
    [NonSerialized]
    string tag = "";
    [NonSerialized]
    uint open_lv = 0;
    [NonSerialized]
    uint max_lv = 0;
    [NonSerialized]
    uint max_affection = 0;
    [NonSerialized]
    clip_event appearClip = null;
    [NonSerialized]
    Dictionary<uint, cat_level_def> catLevelInfo = new Dictionary<uint, cat_level_def>();
    [NonSerialized]
    Dictionary<location_def, List<ani_list>> catAnimationInfo = null;
    [NonSerialized]
    user_cats userCat = null;

    public uint GetCatID()
    {
        if (cat_id == 0)
        {
            object obj;
            if (data.TryGetValue("cat_id", out obj))
            {
                cat_id = (uint)obj;
            }
        }

        return cat_id;
    }
    public string GetCatName()
    {
        if (string.IsNullOrEmpty(name))
        {
            object obj;
            if (data.TryGetValue("name", out obj))
            {
                name = (string)obj;
            }
        }

        return name;
    }

    public string GetCatSkinName()
    {
        if (string.IsNullOrEmpty(tag))
        {
            object obj;
            if (data.TryGetValue("tag", out obj))
            {
                tag = (string)obj;
            }
        }

        return tag;
    }

    public uint GetApearLevel()
    {
        if (open_lv == 0)
        {
            object obj;
            if (data.TryGetValue("open_lv", out obj))
            {
                open_lv = (uint)obj;
            }
        }

        return open_lv;
    }

    public uint GetCatMaxLevel()
    {
        if (max_lv == 0)
        {
            object obj;
            if (data.TryGetValue("max_lv", out obj))
            {
                max_lv = (uint)obj;
            }
        }

        return max_lv;
    }

    public uint GetCatMaxAffection()
    {
        if (max_affection == 0)
        {
            object obj;
            if (data.TryGetValue("max_affection", out obj))
            {
                max_affection = (uint)obj;
            }
        }

        return max_affection;
    }

    public clip_event GetAppearClip()
    {
        if (appearClip == null)
        {
            uint clipID = 0;
            object obj;
            if (data.TryGetValue("appear_clip", out obj))
            {
                clipID = (uint)obj;
            }

            List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
            foreach (game_data clip in clip_event)
            {
                clip_event evtData = (clip_event)clip;
                if (evtData.GetEventID() == clipID)
                {
                    appearClip = evtData;
                }
            }
        }

        return appearClip;
    }

    public cat_level_def GetLevelInfo(uint level)
    {
        cat_level_def ret;

        if (catLevelInfo.TryGetValue(level, out ret))
        {
            return ret;
        }
        else
        {
            cat_level_def levelInfo = cat_level_def.GetCatLevelInfo(cat_id, level);
            catLevelInfo.Add(level, levelInfo);
            return levelInfo;
        }
    }

    public List<ani_list> GetLocationAnimationList(location_def location)
    {
        if (catAnimationInfo == null)
        {
            catAnimationInfo = ani_list.GetCatAnimationList(this);
        }

        if (catAnimationInfo.ContainsKey(location))
        {
            return catAnimationInfo[location];
        }

        return null;
    }

    public ani_list GetRandomAnimationInfo()
    {
        if (catAnimationInfo == null)
        {
            catAnimationInfo = ani_list.GetCatAnimationList(this);
        }

        if (catAnimationInfo.Count == 0)
            return null;

        List<location_def> keyList = new List<location_def>(catAnimationInfo.Keys);

        location_def location = keyList[Random.Range(0, keyList.Count)];

        List<ani_list> list = catAnimationInfo[location];
        if (list.Count == 0)
            return null;

        return list[Random.Range(0, list.Count)];
    }

    public user_cats GetUserCatInfo()
    {
        if (userCat == null)
        {
            userCat = user_cats.GetUserCatData(GetCatID());
        }

        return userCat;
    }


}

[Serializable]
public class cat_level_def : game_data
{
    static public cat_level_def GetCatLevelInfo(uint catID, uint catLevel)
    {
        List<game_data> CatLevelList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CAT_LEVEL_DEF);
        foreach (game_data data in CatLevelList)
        {
            cat_level_def levelInfo = (cat_level_def)data;
            if (levelInfo.GetCatID() == catID)
            {
                if (levelInfo.GetCatLevel() == catLevel)
                {
                    return levelInfo;
                }
            }
        }

        return null;
    }
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CAT_LEVEL_DEF; }

    [NonSerialized]
    uint cat_id = 0;
    [NonSerialized]
    uint level = 0;
    [NonSerialized]
    uint exp = 0;
    [NonSerialized]
    uint max_hunger = 0;
    [NonSerialized]
    uint need_gold = 0;
    [NonSerialized]
    uint item_reward = 0;
    [NonSerialized]
    uint pic_reward = 0;
    [NonSerialized]
    cat_action_def newActionData = null;
    public uint GetCatID()
    {
        if (cat_id == 0)
        {
            object obj;
            if (data.TryGetValue("cat_id", out obj))
            {
                cat_id = (uint)obj;
            }
        }

        return cat_id;
    }

    public uint GetCatLevel()
    {
        if (level == 0)
        {
            object obj;
            if (data.TryGetValue("level", out obj))
            {
                level = (uint)obj;
            }
        }

        return level;
    }

    public uint GetCatExp()
    {
        if (exp == 0)
        {
            object obj;
            if (data.TryGetValue("exp", out obj))
            {
                exp = (uint)obj;
            }
        }

        return exp;
    }

    public uint GetCatMaxHunger()
    {
        if (max_hunger == 0)
        {
            object obj;
            if (data.TryGetValue("max_hunger", out obj))
            {
                max_hunger = (uint)obj;
            }
        }

        return max_hunger;
    }

    public cat_action_def GetCatNewAction()
    {
        if (newActionData == null)
        {
            object obj;
            if (data.TryGetValue("new_action", out obj))
            {
                newActionData = cat_action_def.GetCatActionInfo((uint)obj);
            }
        }

        return newActionData;
    }

    public uint GetCatLevelNeedGold()
    {
        if (need_gold == 0)
        {
            object obj;
            if (data.TryGetValue("need_gold", out obj))
            {
                need_gold = (uint)obj;
            }
        }

        return need_gold;
    }

    public uint GetCatItemReward()
    {
        if (item_reward == 0)
        {
            object obj;
            if (data.TryGetValue("item_reward", out obj))
            {
                item_reward = (uint)obj;
            }
        }

        return item_reward;
    }

    public uint GetCatPictureReward()
    {
        if (pic_reward == 0)
        {
            object obj;
            if (data.TryGetValue("pic_reward", out obj))
            {
                pic_reward = (uint)obj;
            }
        }

        return pic_reward;
    }
}



[Serializable]
public class cat_action_def : game_data
{
    static public cat_action_def GetCatActionInfo(uint actionID)
    {
        List<game_data> CatActionList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CAT_ACTION_DEF);
        foreach (game_data data in CatActionList)
        {
            cat_action_def actionInfo = (cat_action_def)data;
            if (actionInfo.GetCatActionID() == actionID)
            {
                return actionInfo;
            }
        }

        return null;
    }
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CAT_ACTION_DEF; }
    public enum INTERACTION_TYPE
    {
        UNKOWN,
        NOTHING,
        CAMERA,
        OBSERVATION,
        TOUCH,
        FEED,
        PLAY,
    };

    [NonSerialized]
    uint action_id = 0;
    [NonSerialized]
    uint need_hunger = 0;
    [NonSerialized]
    uint need_open_gold = 0;
    [NonSerialized]
    uint get_exp = 0;
    [NonSerialized]
    uint get_gold = 0;
    [NonSerialized]
    uint get_love = 0;
    [NonSerialized]
    uint get_item = 0;
    [NonSerialized]
    uint open_order = 0;
    [NonSerialized]
    string name = "";
    [NonSerialized]
    string open_desc = "";
    [NonSerialized]
    string need_hunger_desc = "";
    [NonSerialized]
    INTERACTION_TYPE action_type = INTERACTION_TYPE.UNKOWN;
    [NonSerialized]
    clip_event actionClipData = null;
    [NonSerialized]
    cat_def targetCat = null;

    public uint GetCatActionID()
    {
        if (action_id == 0)
        {
            object obj;
            if (data.TryGetValue("action_id", out obj))
            {
                action_id = (uint)obj;
            }
        }

        return action_id;
    }

    public clip_event GetActionClip()
    {
        if (actionClipData == null)
        {
            object obj;
            if (data.TryGetValue("action_clip", out obj))
            {
                uint clipID = (uint)obj;
                List<game_data> clip_event = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);

                foreach (game_data clip in clip_event)
                {
                    clip_event evtData = (clip_event)clip;
                    if (evtData.GetEventID() == clipID)
                    {
                        actionClipData = evtData;
                    }
                }
            }
        }

        return actionClipData;
    }

    public uint GetOrder()
    {
        if (open_order == 0)
        {
            object obj;
            if (data.TryGetValue("open_order", out obj))
            {
                open_order = (uint)obj;
            }
        }

        return open_order;
    }

    public cat_def GetTargetCat()
    {
        if (targetCat == null)
        {
            object obj;
            if (data.TryGetValue("cat_id", out obj))
            {
                uint cat_id = (uint)obj;
                targetCat = cat_def.GetCatInfo(cat_id);
            }
        }

        return targetCat;
    }

    public bool IsEnabled()
    {
        uint openCount = 0;
        cat_def target = GetTargetCat();
        if (target != null)
        {
            user_cats cat = target.GetUserCatInfo();
            if (cat != null)
            {
                openCount = cat.GetActionOpenCount();
            }

            return openCount >= GetOrder();
        }

        return false;
    }

    public INTERACTION_TYPE GetActionType()
    {
        if (action_type == INTERACTION_TYPE.UNKOWN)
        {
            object obj;
            if (data.TryGetValue("action_type", out obj))
            {
                switch ((string)obj)
                {
                    case "C":
                        action_type = INTERACTION_TYPE.CAMERA;
                        break;
                    case "O":
                        action_type = INTERACTION_TYPE.OBSERVATION;
                        break;
                    case "H":
                        action_type = INTERACTION_TYPE.TOUCH;
                        break;
                    case "F":
                        action_type = INTERACTION_TYPE.FEED;
                        break;
                    case "P":
                        action_type = INTERACTION_TYPE.PLAY;
                        break;

                    default:
                        action_type = INTERACTION_TYPE.NOTHING;
                        break;
                }
            }
        }
        return action_type;
    }

    public string GetActionName()
    {
        if (string.IsNullOrEmpty(name))
        {
            object obj;
            if (data.TryGetValue("name", out obj))
            {
                name = (string)obj;
            }
        }

        return name;
    }

    public string GetActionOpenDesc()
    {
        if (string.IsNullOrEmpty(open_desc))
        {
            object obj;
            if (data.TryGetValue("open_desc", out obj))
            {
                open_desc = (string)obj;
            }
        }

        return open_desc;
    }

    public uint GetNeedGold()
    {
        if (need_open_gold == 0)
        {
            object obj;
            if (data.TryGetValue("need_open_gold", out obj))
            {
                need_open_gold = (uint)obj;
            }
        }

        return need_open_gold;
    }

    public string GetActionHungerDesc()
    {
        if (string.IsNullOrEmpty(need_hunger_desc))
        {
            object obj;
            if (data.TryGetValue("need_hunger_desc", out obj))
            {
                need_hunger_desc = (string)obj;
            }
        }

        return need_hunger_desc;
    }
}


[Serializable]
public class location_def : game_data
{
    static public location_def GetLocation(uint locationID)
    {
        List<game_data> locations = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.LOCATION_DEF);
        foreach (game_data data in locations)
        {
            location_def locInfo = (location_def)data;
            if (locInfo.GetLocationID() == locationID)
            {
                return locInfo;
            }
        }

        return null;
    }
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.LOCATION_DEF; }
    [NonSerialized]
    uint point_id = 0;
    public uint GetLocationID()
    {
        if (point_id == 0)
        {
            object obj;
            if (data.TryGetValue("point_id", out obj))
            {
                point_id = (uint)obj;
            }
        }

        return point_id;
    }
}

[Serializable]
public class ani_list : game_data
{
    static public Dictionary<location_def, List<ani_list>> GetCatAnimationList(cat_def cat)
    {
        Dictionary<location_def, List<ani_list>> ret = new Dictionary<location_def, List<ani_list>>();

        List<game_data> anims = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ANI_LIST);
        foreach (game_data data in anims)
        {
            ani_list aniInfo = (ani_list)data;
            if (aniInfo.GetTargetCat() == cat)
            {
                if (!ret.ContainsKey(aniInfo.GetStartLocation()))
                {
                    List<ani_list> list = new List<ani_list>();
                    ret.Add(aniInfo.start, list);
                }

                ret[aniInfo.start].Add(aniInfo);
            }
        }

        return ret;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.ANI_LIST; }
    public enum SIBLING_TYPE
    {
        UNKOWN,
        FIRST,
        LAST,
    };

    struct ActionStruct
    {
        public float ratio;
        public cat_action_def action;
    };
    [NonSerialized]
    uint move_id = 0;
    [NonSerialized]
    string move_ani_tag = "";
    [NonSerialized]
    location_def start = null;
    [NonSerialized]
    location_def end = null;
    [NonSerialized]
    uint ratio = 0;
    [NonSerialized]
    cat_def cat = null;

    [NonSerialized]
    List<ActionStruct> interactionList = null;
    [NonSerialized]
    SIBLING_TYPE siblingType = SIBLING_TYPE.UNKOWN;
    public uint GetMoveID()
    {
        if (move_id == 0)
        {
            object obj;
            if (data.TryGetValue("point_id", out obj))
            {
                move_id = (uint)obj;
            }
        }

        return move_id;
    }

    public string GetMoveAnimation()
    {
        if (string.IsNullOrEmpty(move_ani_tag))
        {
            object obj;
            if (data.TryGetValue("move_ani_tag", out obj))
            {
                move_ani_tag = (string)obj;
            }
        }

        return move_ani_tag;
    }

    public location_def GetStartLocation()
    {
        if (start == null)
        {
            object obj;
            if (data.TryGetValue("start_location", out obj))
            {
                start = location_def.GetLocation((uint)obj);
            }
        }

        return start;
    }

    public location_def GetEndLocation()
    {
        if (end == null)
        {
            object obj;
            if (data.TryGetValue("end_location", out obj))
            {
                end = location_def.GetLocation((uint)obj);
            }
        }

        return end;
    }

    public uint GetRatio()
    {
        if (ratio == 0)
        {
            object obj;
            if (data.TryGetValue("ratio", out obj))
            {
                ratio = (uint)obj;
            }
        }

        return ratio;
    }

    public cat_def GetTargetCat()
    {
        if (cat == null)
        {
            object obj;
            if (data.TryGetValue("cat_id", out obj))
            {
                cat = cat_def.GetCatInfo((uint)obj);
            }
        }

        return cat;
    }

    public cat_action_def GetRandomCatAction()
    {
        if (interactionList == null)
        {
            interactionList = new List<ActionStruct>();

            object obj;
            if (data.TryGetValue("able_actions", out obj))
            {
                string data = (string)obj;
                JToken actions = JToken.Parse(data);
                foreach (JArray action in actions)
                {
                    uint actionID = action[0].Value<uint>();
                    float ratio = action[1].Value<float>();
                    ActionStruct actionStruct = new ActionStruct();
                    actionStruct.action = cat_action_def.GetCatActionInfo(actionID);
                    actionStruct.ratio = ratio;
                    interactionList.Add(actionStruct);
                }
            }
        }

        float maxVal = 0.0f;
        List<ActionStruct> enalbeAction = new List<ActionStruct>();
        foreach (ActionStruct action in interactionList)
        {
            if (action.action != null && action.action.IsEnabled())
            {
                enalbeAction.Add(action);
                maxVal += action.ratio;
            }
        }

        if (enalbeAction.Count <= 0)
            return null;
        if (enalbeAction.Count == 1)
            return enalbeAction[0].action;

        float randVal = Random.Range(0.0f, maxVal);
        float searchVal = 0.0f;
        foreach (ActionStruct action in enalbeAction)
        {
            searchVal += action.ratio;
            if (searchVal > randVal)
            {
                return action.action;
            }
        }

        return null;
    }

    public SIBLING_TYPE GetSiblingType()
    {
        if (siblingType == SIBLING_TYPE.UNKOWN)
        {
            object obj;
            if (data.TryGetValue("last_sibling", out obj))
            {
                if ((uint)obj == 1)
                {
                    siblingType = SIBLING_TYPE.LAST;
                }
                else
                {
                    siblingType = SIBLING_TYPE.FIRST;
                }
            }
        }

        return siblingType;
    }
}

[Serializable]
public class user_cats : game_data
{
    static public user_cats GetUserCatData(uint catID)
    {
        List<game_data> cats = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CATS);
        foreach (game_data data in cats)
        {
            if (data != null)
            {
                user_cats catInfo = (user_cats)data;
                if (catInfo != null && catInfo.GetCatID() == catID)
                {
                    return catInfo;
                }
            }
        }

        return null;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_CATS; }

    public uint GetCatID()
    {
        return (uint)data["id"];
    }
    public uint GetCatLevel()
    {
        return (uint)data["lvl"];
    }
    public uint Getfullness()
    {
        return (uint)data["full"];
    }
    public uint GetAffection()
    {
        return (uint)data["aff"];
    }

    public uint GetActionOpenCount()
    {
        return (uint)data["act"];
    }
}

[Serializable]
public class plants : game_data
{
    static public plants GetPlant(uint plantID)
    {
        List<game_data> plantData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.PLANTS);
        foreach (game_data data in plantData)
        {
            plants plant = (plants)data;
            if (plant != null && plant.GetPlantID() == plantID)
            {
                return plant;
            }
        }

        return null;
    }
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.PLANTS; }

    [NonSerialized]
    uint id = 0;
    [NonSerialized]
    plant_types curPlantType = null;

    public enum PLANT_STATE
    {
        WAIT,
        SOME,
        MAX
    };
    public uint GetPlantID()
    {
        if (id == 0)
        {
            id = (uint)data["id"];
        }

        return id;
    }

    public PLANT_STATE GetPlantState()
    {
        if (GetCurVal() == GetMaxStack())
        {
            return PLANT_STATE.MAX;
        }
        else if (GetCurVal() > 1)
        {
            return PLANT_STATE.SOME;
        }
        else
        {
            return PLANT_STATE.WAIT;
        }
    }

    public uint GetCurVal()
    {
        return (uint)data["val"];
    }

    public uint GetNextIncrement()
    {
        return (uint)data["exp"];
    }

    public uint GetPerTick()
    {
        return GetPlantType().GetPerTick();
    }

    public uint GetTickPeriod()
    {
        return GetPlantType().GetTickPeriod();
    }

    public uint GetMaxStack()
    {
        return GetPlantType().GetMaxStack();
    }

    private plant_types GetPlantType()
    {
        if (curPlantType == null)
        {
            switch (GetPlantID())
            {
                case 1:
                    return plant_types.GetPlantTypeData(1);
                case 2:
                    return plant_types.GetPlantTypeData(2);
                case 3:
                    return plant_types.GetPlantTypeData(3);
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    return plant_types.GetPlantTypeData(4);
                default:
                    return null;
            }
        }

        return curPlantType;
    }
}

[Serializable]
public class plant_types : game_data
{
    static public plant_types GetPlantTypeData(uint plantType)
    {
        List<game_data> plantData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.PLANT_TYPES);
        foreach (plant_types data in plantData)
        {
            if (data != null && data.GetPlantType() == plantType)
            {
                return data;
            }
        }

        return null;
    }
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.PLANT_TYPES; }

    public uint GetPlantType()
    {
        return (uint)data["plant_type"];
    }

    public uint GetTickPeriod()
    {
        return (uint)data["tick_period"];
    }

    public uint GetPerTick()
    {
        return (uint)data["per_tick"];
    }

    public uint GetMaxStack()
    {
        return (uint)data["max_stack"];
    }
}

