using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterGameData : GameData
{
    public enum eCharacterType
    {
        None = 0,
        Chaser = 1,
        Survivor = 2,
    }

    static public bool IsChaserCharacter(int id)
    {
        GameData data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character, id);
        if (data != null)
        {
            return ((CharacterGameData)data).IsChaserCharacter();
        }

        return false;
    }

    static public bool IsSuvivorCharacter(int id)
    {
        GameData data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character, id);
        if (data != null)
        {
            return ((CharacterGameData)data).IsSuvivorCharacter();
        }

        return false;
    }

    static public string GetUIResourceName(int id)
    {
        CharacterGameData data = GetCharacterData(id);
        if (data != null)
        {
            return data.sprite_ui_resource.name;
        }

        return "";
    }

    static public CharacterGameData GetCharacterData(int id)
    {
        GameData data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character, id);
        if (data != null)
        {
            return data as CharacterGameData;
        }

        return null;
    }

    static public List<CharacterGameData> GetSurvivorList()
    {
        List<CharacterGameData> ret = new List<CharacterGameData>();
        List<GameData> list = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character);
        if (list != null)
        {
            foreach (CharacterGameData data in list)
            {
                if (data.IsSuvivorCharacter())
                    ret.Add(data);
            }
        }

        return ret;
    }

    static public List<CharacterGameData> GetChaserList()
    {
        List<CharacterGameData> ret = new List<CharacterGameData>();
        List<GameData> list = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character);
        if (list != null)
        {
            foreach (CharacterGameData data in list)
            {
                if (data.IsChaserCharacter())
                    ret.Add(data);
            }
        }

        return ret;
    }

    static public List<UserCharacterData> GetMySurvivorList()
    {
        List<UserCharacterData> ret = new List<UserCharacterData>();
        foreach (var data in Managers.UserData.MyCharacters)
        {
            if (IsSuvivorCharacter(data.Key))
                ret.Add(data.Value);
        }


        return ret;
    }

    static public List<UserCharacterData> GetMyChaserList()
    {
        List<UserCharacterData> ret = new List<UserCharacterData>();
        foreach (var data in Managers.UserData.MyCharacters)
        {
            if (IsChaserCharacter(data.Key))
                ret.Add(data.Value);
        }

        return ret;
    }

    public int char_type { get; private set; }
    public int char_grade { get; private set; }
    public int char_skill { get; private set; }
    public int char_skill_atk { get; private set; }
    //public string ui_resource { get; private set; }
    public int is_limited { get; private set; } = 0;
    public bool use { get; private set; }
    public Sprite sprite_ui_resource { get; private set; }
    public SkeletonDataAsset spine_resource { get; private set; }
    public float scale_ratio { get; private set; } = 1.0f;
    public string cha_dust_reource { get; private set; }

    public CharacterLevelGameData[] levelData { get; private set; }
    public CharacterSkillLevelGameData[] skillLevelData { get; private set; }
    private CharacterReinforceGameData[] enchantLevelData;
    private SkillGameData skillData;
    private SkillGameData atkSkillData;
    private static GameObject baseResource = null;

    public GameObject LoadCharacterObject(int equip = 0)
    {
        if (baseResource == null)
        {
            baseResource = Resources.Load<GameObject>("Prefabs/Character/base");
        }

        GameObject newResource = GameObject.Instantiate(baseResource);
        SkeletonAnimation animController = newResource.GetComponentInChildren<SkeletonAnimation>();
        animController.skeletonDataAsset = spine_resource;

        CharacterAnimationController anim = newResource.GetComponent<CharacterAnimationController>();
        if(anim)
        {
            anim.SetEquip(animController, equip);
        }

        animController.Initialize(true);

        return newResource;
    }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        char_type = Int(data["char_type"]);
        char_grade = Int(data["char_grade"]);
        char_skill = Int(data["char_skill"]);
        char_skill_atk = Int(data["char_skill_atk"]);
        //ui_resource = data["ui_resource"];
        use = Int(data["use"]) > 0;
        if(data.ContainsKey("limited"))
            is_limited = Int(data["limited"]);

        sprite_ui_resource = Managers.Resource.LoadAssetsBundle<Sprite>(data["ui_resource_assetsbundle"]);
        if (use) spine_resource = Managers.Resource.LoadAssetsBundle<SkeletonDataAsset>(data["spine_resource_path_assetsbundle"]);

        if (data.ContainsKey("scale_ratio"))
            scale_ratio = Int(data["scale_ratio"]) * 0.001f;
        //if (sprite_ui_resource == null)
        //{
        //    SBDebug.Log("구버전의 패스 사용");
        //    sprite_ui_resource = Managers.Resource.LoadAssetsBundle<Sprite>(ui_resource);
        //}
        //if (spine_resource == null)
        //{
        //    SBDebug.Log("구버전의 패스 사용");
        //    if (use) spine_resource = Managers.Resource.LoadAssetsBundle<SkeletonDataAsset>(data["spine_resource_path"]);
        //}

        cha_dust_reource = data["cha_dust_reource"];
    }

    public bool IsSuvivorCharacter()
    {
        return char_type == 2;
    }

    public bool IsChaserCharacter()
    {
        return char_type == 1;
    }

    public void SetLevelData(int level, CharacterLevelGameData data)
    {
        if (levelData == null)
        {
            levelData = new CharacterLevelGameData[GameConfig.Instance.MAX_CHARACTER_LEVEL + 1];
        }

        if (levelData.Length > level)
            levelData[level] = data;
    }

    public void SetSkillLevelData(int level, CharacterSkillLevelGameData data)
    {
        if (skillLevelData == null)
        {
            skillLevelData = new CharacterSkillLevelGameData[GameConfig.Instance.MAX_CHARACTER_SKILL_LEVEL + 1];
        }

        if (skillLevelData.Length > level)
            skillLevelData[level] = data;
    }

    public void SetEnchantData(int enchant, CharacterReinforceGameData data)
    {
        if (enchantLevelData == null)
        {
            enchantLevelData = new CharacterReinforceGameData[GameConfig.Instance.MAX_CHARACTER_REINFORCE + 1];
        }

        enchantLevelData[enchant] = data;
    }

    public SkillGameData GetSkillData()
    {
        if (skillData != null)
        {
            if (char_skill != skillData.GetID())
            {
                skillData = null;
            }
        }

        if (skillData == null && char_skill != 0)
        {
            skillData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill, char_skill) as SkillGameData;
        }
        
        return skillData;
    }

    public SkillGameData GetAtkSkillData()
    {
        if (atkSkillData == null && char_skill_atk != 0)
        {
            atkSkillData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill, char_skill_atk) as SkillGameData;
        }
        return atkSkillData;
    }

    public CharacterReinforceGameData GetEnchantData(int enchant)
    {
        if (enchant < enchantLevelData.Length)
            return enchantLevelData[enchant];

        return null;
    }
    public CharacterSkillLevelGameData GetCharacterSkillLevelGameData(int skill_lv)
    {
        if (skill_lv < skillLevelData.Length)
            return skillLevelData[skill_lv];
        return null;
    }

    public void ChangeSkillData(int oldID, int newID)
    {
        SBDebug.Log($"OLD : {oldID} / NEW : {newID}");
        if (skillData.GetID() == oldID)
        {
            skillData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill, newID) as SkillGameData;
        }
        else if (atkSkillData?.GetID() == oldID)
        {
            atkSkillData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill, newID) as SkillGameData;
        }
        else
        {
            SBDebug.LogError($"{oldID} 스킬을 보유하고 있지 않음");
        }
    }

    public string GetScript(int id)
    {
        return StringManager.GetString($"{GameDataManager.DATA_TYPE.character}:script:{id}");
    }
}

public class CharacterLevelGameData : GameData
{
    public int character_uid { get; private set; }
    public int level { get; private set; }
    public int need_exp { get; private set; }
    public int move_speed { get; private set; }
    public int hp { get; private set; }
    public int shield_cnt { get; private set; }
    public int quest_speed { get; private set; }
    public int atk_point { get; private set; }
    public int reduce_attack_time { get; private set; }
    public int max_battary { get; private set; }
    public int attack_cool_time { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        character_uid = Int(data["character_uid"]);
        level = Int(data["level"]);
        need_exp = Int(data["need_exp"]);
        move_speed = Int(data["move_speed"]);
        hp = Int(data["hp"]);
        shield_cnt = Int(data["shield_cnt"]);
        quest_speed = Int(data["quest_speed"]);
        atk_point = Int(data["atk_point"]);
        reduce_attack_time = Int(data["reduce_attack_time"]);
        max_battary = Int(data["max_battery"]);
        attack_cool_time = Int(data["attack_cool_time"]);

        CharacterGameData.GetCharacterData(character_uid).SetLevelData(level, this);
    }
}

public class CharacterReinforceGameData : GameData
{
    public int character_uid { get; private set; }
    public int reinforce_grade { get; private set; }
    public int need_item_reinforce { get; private set; }
    public int reinforce_item_count { get; private set; }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        character_uid = Int(data["character_uid"]);
        reinforce_grade = Int(data["reinforce_grade"]);
        need_item_reinforce = Int(data["need_item_reinforce"]);
        reinforce_item_count = Int(data["reinforce_item_count"]);

        var charData = CharacterGameData.GetCharacterData(character_uid);
        if (charData != null)
        {
            charData.SetEnchantData(reinforce_grade, this);
        }
        else
        {
            SBDebug.LogError("캐릭터 데이터가 없는 강화 정보. character_uid : " + character_uid.ToString());
        }
    }
}

public class CharacterSkillLevelGameData : GameData
{
    public int skill_uid { get; private set; }
    public int skill_level { get; private set; }
    public int need_item { get; private set; }
    public int need_item_count { get; private set; }
    public int need_gold { get; private set; }
    public int need_reinforce { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        skill_uid = Int(data["skill_uid"]);
        skill_level = Int(data["skill_level"]);
        need_item = Int(data["need_item"]);
        need_item_count = Int(data["need_item_count"]);
        need_gold = Int(data["need_gold"]);
        need_reinforce = Int(data["need_reinforce"]);

        List<GameData> list = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character);
        if (list != null)
        {
            foreach (CharacterGameData character in list)
            {
                if (character.char_skill == skill_uid)
                    character.SetSkillLevelData(skill_level, this);
            }
        }
    }

}
