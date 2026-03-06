using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SkillRangeType
{
    None = 0,
    Circle = 1,
    Rectangle = 2,
}

public class SkillGameData : GameData
{
    public float CastingTime { get; private set; }
    public float CoolTime { get; private set; }
    public int CoolTimeCheck { get; private set; }
    public int CasterEffectID { get; private set; }
    public int SkillBaseID_1 { get; private set; }
    public int SkillBaseID_2 { get; private set; }
    public int SkillBaseID_3 { get; private set; }
    public string IconName { get; private set; }
    public int SkillAnimationType { get; private set; }
    public int Skill_use_type { get; private set; }
    public Dictionary<int, SkillBaseGameData[]> BaseGameDatas { get; private set; }
    int[] _skillIDs;

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        CastingTime = Int(data["casting_time"]) * 0.001f;
        CoolTime = Int(data["cool_time"]) * 0.001f;
        CoolTimeCheck = Int(data["cool_check"]);
        CasterEffectID = Int(data["caster_effect_resource_id"]);
        SkillBaseID_1 = Int(data["base1"]);
        SkillBaseID_2 = Int(data["base2"]);
        SkillBaseID_3 = Int(data["base3"]);
        IconName = data["skill_icon"];
        SkillAnimationType = Int(data["type"]);
        Skill_use_type = Int(data["skill_use_type"]);

        BaseGameDatas = new Dictionary<int, SkillBaseGameData[]>();
        _skillIDs = new int[] { SkillBaseID_1, SkillBaseID_2, SkillBaseID_3 };

        var baseDataList = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.skill_base).Cast<SkillBaseGameData>().ToList();
        foreach (var id in _skillIDs)
        {
            if (id > 0)
            {
                var candidateDataList = baseDataList.FindAll(x => x.SkillBaseUID == id);
                if (candidateDataList.Count > 0)
                {
                    candidateDataList.Sort((x, y) => { return x.Level.CompareTo(y.Level); });
                    BaseGameDatas.Add(id, candidateDataList.ToArray());

                    foreach (SkillBaseGameData basedata in candidateDataList)
                    {
                        basedata.SetSkillUID(GetID());
                    }
                }
            }
        }
    }

    // 주 스킬은 첫 번째 스킬로 정해져 있다. 이 스킬을 기준으로 스킬 범위 등을 표시해준다.
    public SkillBaseGameData GetMajorSkill(int level)
    {
        try
        {
            var candidateData = BaseGameDatas[SkillBaseID_1][level - 1];
            return candidateData;
        }
        catch
        {
            return null;
        }
    }

    public SkillBaseGameData GetSkill(int id, int level)
    {
        try
        {
            var candidateData = BaseGameDatas[id][level - 1];
            return candidateData;
        }
        catch
        {
            return null;
        }
    }

    public Sprite GetIcon()
    {
        if (string.IsNullOrEmpty(IconName)) return null;
        return Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/" + IconName);
    }
}

public class SkillBaseGameData : GameData
{
    public enum SkillType
    {
        None = 0,
        Active = 1,
        Projectile = 2,
        NormalAttack = 3,
    }

    public int SkillBaseUID { get; private set; }
    public int Level { get; private set; }
    public int SummonID { get; private set; }
    public int RangeStart { get; private set; }
    public int RangeDistance { get; private set; }
    public int RangeAngle { get; private set; }

    int _skillType;
    int _rangeType;

    int Skill_UID = 0;
    public SkillRangeType RangeType { get { return (SkillRangeType)_rangeType; } }
    public SkillType Type { get { return (SkillType)_skillType; } }

    public void SetSkillUID(int uid)
    {
        Skill_UID = uid;
    }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        SkillBaseUID = Int(data["skill_uid"]);
        Level = Int(data["skill_level"]);
        _skillType = Int(data["skill_type"]);
        SummonID = Int(data["summon_info"]);
        _rangeType = Int(data["range_type"]);
        RangeStart = Int(data["range_start"]);
        RangeDistance = Int(data["range_distance"]);
        RangeAngle = Int(data["range_angle"]);
    }

    public override string GetDesc()
    {
        var key = $"{GameDataManager.DATA_TYPE.skill}:desc:{SkillBaseUID}:{Level}";

        return StringManager.GetString(key);
    }

    public override string GetName()
    {
        var key = $"{GameDataManager.DATA_TYPE.skill}:name:{Skill_UID}";

        return StringManager.GetString(key);
    }
}

public class SkillEffectGameData : GameData
{
    public enum EffectType
    {
        None = 0,
        Rush = 1,
        Stun = 2,
        Confuse = 3,
        BlockSight = 4,
        StatBuff = 5,
        Invisible = 6,
        StatDebuff = 7,
        Teleport = 8,
        Damage = 9,
        BatteryBuff = 10,
        Heal = 11,
        SkillChange = 12,
        UnlockCoolTime = 13,
        Knockback = 14,
        ReduceCoolTime = 15,
        Pluck = 16,
        Location_Create = 17,     //좌표 생성
        Location_Teleport = 18,   //생선된 좌표로 이동
        Location_Exchange = 19,   //좌표 교환
        Location_Tracking = 20,      //위치 추적
        Duration_Increase = 21,   //지속시간 증가
        Access_Trigger = 22,      //접근시 발동
        Delete_Effect = 23,       //이펙트 삭제
        IncreaseCoolTime = 24,
        Skill_Preview_Effect = 25,
        Stuck = 26, //행동불가
        Silence = 27, //침묵
        Reflect = 28,            //반사
        Buffdelete = 29,         //버프 삭제
        Respawn_shield = 30,     //리스폰 보호막(INCREASE_STATUS 기능은 같다)
        Trap_detect = 31,        //덫 탐지
        Nullity_buff = 32,      // 버프 무효화
    }

    public enum EffectStatType
    {
        None = 0,
        MoveSpeed = 1,
        HP = 2,
        QuestSpeed = 3,
        Attack = 4,
        Cooltime = 5,
        Shield = 6,
    }

    int _effectType;
    int _stat;
    public int Value1 { get; private set; }
    public int Value2 { get; private set; }
    public float ActiveTime { get; private set; }
    public int ResourceID { get; private set; }

    public EffectType Type { get { return (EffectType)_effectType; } }
    public EffectStatType StatType { get { return (EffectStatType)_stat; } }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        _effectType = Int(data["effect_type"]);
        _stat = Int(data["stat"]);
        Value1 = Int(data["value1"]);
        Value2 = Int(data["value2"]);
        ActiveTime = Int(data["active_effect_time"]) * 0.001f;
        ResourceID = Int(data["effect_resource_id"]);
    }
}

public class SkillSummonGameData : GameData
{
    public enum SummonType
    {
        None = 0,
        CharacterClone = 1,
        Trap = 2,
        AreaOfEffect = 3,
        Projectile = 4,
        Block = 5,
        InvisibleObject = 6,
    }

    int _type;
    int _rangeType;
    public int Speed { get; private set; }
    public int RangeDistance { get; private set; }
    public int RangeAngle { get; private set; }
    public float Lifetime { get; private set; }
    public string ResourcePath { get; private set; }
    public string summon_explosion_sound_id { get; private set; }


    public SummonType Type { get { return (SummonType)_type; } }
    public SkillRangeType RangeType { get { return (SkillRangeType)_rangeType; } }

    public int explosion_effect_id { get; private set; }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        _type = Int(data["summon_type"]);
        Speed = Int(data["summon_speed"]);
        _rangeType = Int(data["summon_range_type"]);
        RangeDistance = Int(data["summon_range_distance"]);
        RangeAngle = Int(data["summon_range_angle"]);
        Lifetime = Int(data["summon_lifetime"]) * 0.001f;
        ResourcePath = data["summon_resource"];
        summon_explosion_sound_id = data["summon_explosion_sound_id"];

        explosion_effect_id = Int(data["summon_resource_explosion"]);
    }
}