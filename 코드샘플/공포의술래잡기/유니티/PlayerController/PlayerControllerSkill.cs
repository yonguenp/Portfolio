using SBSocketSharedLib;
using System.Collections;
using UnityEngine;

public partial class PlayerController
{
    public enum SKILL_TYPE
    {
        NORMAL_ATK = 0,
        ACTIVE_SKILL = 1,

        SKILL_TYPE_MAX
    }

    CharacterSkill _characterSkill = new CharacterSkill();
    SkillRangeIndicator skillEffectRectangle = null;
    SkillRangeIndicator skillEffectCircle = null;

    private void InitializeSkill()
    {
        var userCharData = Managers.UserData.GetMyCharacterInfo(Character.CharacterType);
        if (CharacterGameData.IsChaserCharacter(Character.CharacterType))
        {
            _characterSkill.AddSkillInfo(SKILL_TYPE.NORMAL_ATK, new CharacterSkill.SkillInfo
            {
                Data = Managers.PlayData.RoomChaserSkillData[Character.Id],
                SkillLevel = 1,//userCharData.skillLv,
                ReducedCoolTime = Character.Stat.ReduceAttackTime,     // 추격자의 평타만 해당 스탯을 사용한다.
                CoolTimeImage = ControllerPad.Joysticks.Find(x => x.InputType == JoystickInputType.Normal).CooltimeImage,
            });
        }

        _characterSkill.AddSkillInfo(SKILL_TYPE.ACTIVE_SKILL, new CharacterSkill.SkillInfo
        {
            Data = Managers.PlayData.RoomPlayerActiveSkillData[Character.Id],
            SkillLevel = userCharData.skillLv,
            ReducedCoolTime = Character.Stat.ReduceSkillTime,
            CoolTimeImage = ControllerPad.Joysticks.Find(x => x.InputType == JoystickInputType.ActiveSkill).CooltimeImage,
        });

        Game.Instance.UIGame.InitSkillGuide(userCharData.GetSkillBaseData().GetName(), userCharData.GetSkillBaseData().GetDesc());
    }

    public void ChangeSkillData(int oldID, int newID)
    {
        for (SKILL_TYPE type = SKILL_TYPE.NORMAL_ATK; type < SKILL_TYPE.SKILL_TYPE_MAX; ++type)
        {
            var skillInfo = _characterSkill.GetSkillInfo(type);
            if (skillInfo == null) continue;
            if (skillInfo.Data.GetID() == oldID)
            {
                var newSkillData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill, newID) as SkillGameData;
                _characterSkill.AddSkillInfo(type, new CharacterSkill.SkillInfo
                {
                    Data = newSkillData,
                    SkillLevel = skillInfo.SkillLevel,
                    ReducedCoolTime = skillInfo.ReducedCoolTime,
                    CoolTimeImage = skillInfo.CoolTimeImage,
                });
                ControllerPad.ChangeSkillIcon(type, newSkillData);
            }
        }
    }

    private void UpdateSkillRangePosition(Color color, SkillRangeType rangeType)
    {
        SkillRangeIndicator candidateObject;
        switch (rangeType)
        {
            case SkillRangeType.Circle:
                candidateObject = skillEffectCircle;
                break;
            case SkillRangeType.Rectangle:
                candidateObject = skillEffectRectangle;
                break;
            default:
                return;
        }

        if (candidateObject == null) return;

        candidateObject.SetPosition(Character.transform.position);
        candidateObject.SetColor(color);
    }

    private void ShowSkillRangeGuideUI(Vector2 dir, Vector2 size, Color color, float startPos)     // 직사각형
    {
        if (skillEffectRectangle == null)
        {
            skillEffectRectangle = Managers.Resource.Instantiate("UI/Game/skill_range_rect").GetComponent<SkillRangeIndicator>();
        }

        skillEffectRectangle.SetRange(dir, size, color, Character.transform.position, startPos);
    }

    private void ShowSkillRangeGuideUI(Vector2 dir, float range, int degree, Color color, float startPos)      // 부채꼴 등 원형
    {
        if (skillEffectCircle == null)
        {
            skillEffectCircle = Managers.Resource.Instantiate("UI/Game/skill_range_circle").GetComponent<SkillRangeIndicator>();
        }

        skillEffectCircle.SetRange(dir, range, degree, color, Character.transform.position, startPos);
    }

    public void RemoveSkillRangeGuideUI()
    {
        if (skillEffectCircle)
        {
            skillEffectCircle.SetColor(new Color(0, 0, 0, 0));
        }
        if (skillEffectRectangle)
        {
            skillEffectRectangle.SetColor(new Color(0, 0, 0, 0));
        }
    }

    public void UnlockAllCoolTime()
    {
        _characterSkill.UnlockAllCoolTime();
    }

    public void OnResetSkillCoolTime(int skillID)
    {
        _characterSkill.ResetCoolTime(skillID);
    }

    public void UnlockCoolTime(int skillID)
    {
        _characterSkill.UnlockCoolTime(skillID);
    }

    public void OnSetSkillRemainTime(float time)
    {
        if (time > 0)
            _characterSkill.SetSkillRemainTime(time);
    }

    public void OnResetSkillRemainTime()
    {
        _characterSkill.ResetSkillRemainTime();
    }

    public void ReduceCurrentCoolTime(int skillID, float seconds)
    {
        // 쿨타임 줄이는 것은 음수 변환
        _characterSkill.ChangeCurrentCoolTime(skillID, -seconds);
    }

    public void IncreaseCurrentCoolTime(int skillID, float seconds)
    {
        _characterSkill.ChangeCurrentCoolTime(skillID, seconds);
    }

    public bool IsEnableSkill(SKILL_TYPE type)
    {
        if (Character.IsVehicle)
            return false;

        return _characterSkill.CanSkill(type);
    }

    public void ChangeReducedCoolTime(float coolTime, float skilltime)
    {
        //if (Character.IsChaser)
        _characterSkill.ChangeReducedCoolTime(coolTime, skilltime);
    }

    public void UseSkill(int skillID, float dirX, float dirY)
    {
        Managers.GameServer.SendUseSkill(skillID, new Vec2Float(dirX, dirY), Character.PosInfo.Pos);
    }

    public void UseSkillCasting(int skillID, float dirX, float dirY, float time)
    {
        Managers.GameServer.SendUseSkillCasting(skillID, new Vec2Float(dirX, dirY), Character.PosInfo.Pos);
    }

    public void OnSkillCasting(int skillID)
    {
    }
}
