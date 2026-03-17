using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSkill
{
    public class SkillInfo
    {
        public SkillGameData Data;
        public int SkillLevel;
        public float ReducedCoolTime;
        public Image CoolTimeImage;

        public float CoolTime
        {
            get
            {
                if (Data.Skill_use_type == 1)
                    return (Data.CoolTime) - ReducedCoolTime;
                else
                    return (Data.CoolTime) * (1.0f - ReducedCoolTime);
            }
        }

        public float CurCoolTime;
        public float LastCoolTime;
        public bool CoolTimeLocked;
    }

    bool isShowSkillRemainTime = false;
    float skillRemainTime = 0f;
    float currentSkillTime = 0f;

    Dictionary<PlayerController.SKILL_TYPE, SkillInfo> skillInfos = new Dictionary<PlayerController.SKILL_TYPE, SkillInfo>();

    public bool AddSkillInfo(PlayerController.SKILL_TYPE id, SkillInfo skillinfo)
    {
        // id 0 : 추격자 평타, 1 : 캐릭터 액티브 스킬
        // 일단 스킬을 바로 사용할 수 있도록 초기화
        skillinfo.CurCoolTime = 0;
        skillinfo.LastCoolTime = skillinfo.CoolTime;
        skillInfos[id] = skillinfo;

        return true;
    }

    public void ChangeReducedCoolTime(float coolTime, float skilltime)
    {
        if (skillInfos.ContainsKey(PlayerController.SKILL_TYPE.NORMAL_ATK))
            skillInfos[PlayerController.SKILL_TYPE.NORMAL_ATK].ReducedCoolTime = coolTime;
        if (skillInfos.ContainsKey(PlayerController.SKILL_TYPE.ACTIVE_SKILL))
            skillInfos[PlayerController.SKILL_TYPE.ACTIVE_SKILL].ReducedCoolTime = skilltime;
    }

    public SkillInfo GetSkillInfo(PlayerController.SKILL_TYPE id)
    {
        if (!skillInfos.ContainsKey(id))
            return null;

        return skillInfos[id];
    }

    public bool CanSkill(PlayerController.SKILL_TYPE id)
    {
        if (!skillInfos.ContainsKey(id) || skillInfos[id] == null) return false;

        return GetSkillCoolTime(id) <= 0;
    }

    public float GetSkillCoolTime(PlayerController.SKILL_TYPE id)
    {
        if (skillInfos[id] == null) return float.MaxValue;

        return skillInfos[id].CurCoolTime;
    }

    public void UnlockAllCoolTime()
    {
        foreach (var skillInfo in skillInfos)
        {
            skillInfo.Value.CoolTimeLocked = false;
        }
    }

    public bool ResetCoolTime(int skillID)
    {
        if (skillInfos == null) return false;
        foreach (var skillKVPair in skillInfos)
        {
            var skillType = skillKVPair.Key;
            var skillInfo = skillKVPair.Value;

            // 추적자 평타는 1레벨 초과할 수 없기 때문에 널 익셉션이 나지 않도록 코드 추가
            if (skillType == PlayerController.SKILL_TYPE.NORMAL_ATK && skillInfo.SkillLevel > 1) continue;

            if (skillInfo.Data.GetID() == skillID)
            {
                return ResetCoolTime(skillType, skillInfo.Data.CoolTimeCheck);
            }
        }

        return false;
    }

    private bool ResetCoolTime(PlayerController.SKILL_TYPE id, int cooltimeType)
    {
        if (skillInfos[id] == null) return false;
        skillInfos[id].CurCoolTime = skillInfos[id].CoolTime;
        skillInfos[id].LastCoolTime = skillInfos[id].CoolTime;

        switch (cooltimeType)
        {
            case 0:
                skillInfos[id].CoolTimeLocked = false;
                break;
            case 1:
                skillInfos[id].CoolTimeLocked = true;
                break;
        }
        return true;
    }

    public void SetSkillRemainTime(float time)
    {
        isShowSkillRemainTime = true;
        skillRemainTime = currentSkillTime = time;
        skillInfos[PlayerController.SKILL_TYPE.ACTIVE_SKILL].CoolTimeImage.fillClockwise = true;
    }

    public void ResetSkillRemainTime()
    {
        isShowSkillRemainTime = false;
        skillRemainTime = currentSkillTime = 0;
        skillInfos[PlayerController.SKILL_TYPE.ACTIVE_SKILL].CoolTimeImage.fillClockwise = false;
    }

    public bool UnlockCoolTime(int skillID)
    {
        if (skillInfos == null) return false;
        foreach (var skillKVPair in skillInfos)
        {
            var skillType = skillKVPair.Key;
            var skillInfo = skillKVPair.Value;

            // 추적자 평타는 1레벨 초과할 수 없기 때문에 널 익셉션이 나지 않도록 코드 추가
            if (skillType == PlayerController.SKILL_TYPE.NORMAL_ATK && skillInfo.SkillLevel > 1) continue;

            if (skillInfo.Data.GetID() == skillID)
            {
                skillInfo.CoolTimeLocked = false;
                return true;
            }
        }

        return false;
    }

    public bool ChangeCurrentCoolTime(int skillID, float seconds)
    {
        if (skillInfos == null) return false;
        foreach (var skillKVPair in skillInfos)
        {
            var skillType = skillKVPair.Key;
            var skillInfo = skillKVPair.Value;

            // 추적자 평타는 1레벨 초과할 수 없기 때문에 널 익셉션이 나지 않도록 코드 추가
            if (skillType == PlayerController.SKILL_TYPE.NORMAL_ATK && skillInfo.SkillLevel > 1) continue;

            if (skillInfo.Data.GetID() == skillID)
            {
                skillInfo.CurCoolTime += seconds;
                if (skillInfo.CurCoolTime < 0)
                    skillInfo.CurCoolTime = 0f;
                return true;
            }
        }

        return false;
    }

    public void Update()
    {
        foreach (var iter in skillInfos)
        {
            var skillInfo = iter.Value;
            if (skillInfo == null)
                continue;
            if (!skillInfo.CoolTimeLocked)
            {
                skillInfo.CurCoolTime -= Time.deltaTime;
                if (skillInfo.CurCoolTime < 0)
                    skillInfo.CurCoolTime = 0;
            }

            if (skillInfo.LastCoolTime == 0)
            {
                skillInfo.CoolTimeImage.fillAmount = 0;
            }
            else
            {
                var rate = skillInfo.CurCoolTime / skillInfo.LastCoolTime;
                if (isShowSkillRemainTime && skillInfo.CoolTimeLocked)
                {
                    currentSkillTime -= Time.deltaTime;
                    if (currentSkillTime < 0)
                        currentSkillTime = 0;
                    rate = currentSkillTime / skillRemainTime;
                    if (rate < 0)
                        isShowSkillRemainTime = false;
                }
                if (rate < 0)
                    rate = 0;

                skillInfo.CoolTimeImage.fillAmount = rate;
            }
        }
    }
}
