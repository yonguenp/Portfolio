using SBSocketSharedLib;
using System.Collections;
using UnityEngine;

public partial class CharacterObject
{
    Coroutine skillCastingCoroutine;

    public void OnSkill(int skillID, Vec2Float skillDir)
    {
        var charData = CharacterGameData.GetCharacterData(CharacterType);
        var skillData = charData.GetSkillData();

        if (skillData.GetID() != skillID)
        {
            if (IsChaser)
            {
                skillData = charData.GetAtkSkillData();
                if (skillData.GetID() != skillID)
                    return;
            }
            else
            {
                return;
            }
        }

        _characterRenderer.SetAnimation(skillDir.X, skillDir.Y, State, (MoveStatus)PosInfo.MoveStatus, true);
        _characterRenderer.PlaySkillAnimation(skillDir.X, skillDir.Y, skillData.SkillAnimationType);

        // TODO : 일단은 스킬 레벨별로 타입은 변하지 않는다고 가정하고 구현한 것이다.
        var skillBaseData = skillData.GetMajorSkill(1);

        switch (skillBaseData.Type)
        {
            case SkillBaseGameData.SkillType.NormalAttack:
            case SkillBaseGameData.SkillType.Projectile:
                if (IsChaser)
                {
                    soundController.Play("effect/EF_ATTACK_SWING");
                }
                break;

            case SkillBaseGameData.SkillType.Active:
                break;

            default:
                throw new System.Exception($"Wrong skill data : ID {skillID}");
        }

        if (skillData.CasterEffectID > 0 && skillData.CastingTime <= 0)
        {
            Managers.Effect.PlayEffect(skillData.CasterEffectID, RootEffect, 0, false, skillDir.X, skillDir.Y);
        }

        if (IsShow)
            ShowSkillPopup(skillID);
    }

    void ShowSkillPopup(int skillId)
    {
        var key = $"{GameDataManager.DATA_TYPE.skill}:skill_popup:{skillId}";
        var skillPopupText = StringManager.GetString(key);
        if (string.IsNullOrEmpty(skillPopupText))
            return;

        if (key == skillPopupText)
            return;

        //soundController.Play("voice/" + Random.Range(3, 11).ToString(), SoundController.PlayType.Broadcast);

        var hudSkillPopup = Game.Instance.HudNode.CreateHudSkillPopup(this, IsChaser);
        if (hudSkillPopup == null)
        {
            Debug.LogError("hudSkillPopup is null");
            return;
        }

        var skillPopup = hudSkillPopup.gameObject.GetComponent<SkillPopup>();
        if (skillPopup == null)
        {
            Debug.LogError("skillPopup is null");
            return;
        }

        skillPopup.SetText(skillPopupText);

        Destroy(skillPopup.gameObject, 1.5f);
        //Debug.Log($"SkillPopup {skillPopupText}");
    }

    public void WaitSkillCasting(float dirX, float dirY, CharacterObject charObj = null)
    {
        _buff.BuffStatus.SetStatusFlag(ObjectBuffStatus.SkillCasting);

        //Managers.GameServer.SendMove((byte)CreatureStatus.Idle,
        //    (byte)MoveStatus.None,
        //    dirX,
        //    dirY,
        //    PosInfo.Pos.X,
        //    PosInfo.Pos.Y);
        charObj.SetState(CreatureStatus.Idle);
    }

    public void ClearSkillCastring()
    {
        _buff.BuffStatus.ClearStatusFlag(ObjectBuffStatus.SkillCasting);
    }

    public void OnSkillCasting(int skillID, Vec2Float skillDir, CharacterObject charObj)
    {
        WaitSkillCasting(skillDir.X, skillDir.Y, charObj);
        PosInfo.MoveDir = skillDir;
        _characterRenderer.PlayCastingAnimation(skillDir.X, skillDir.Y);
        skillCastingCoroutine = StartCoroutine(SkillCastingCompleteCoroutine(skillID, skillDir));
    }

    private IEnumerator SkillCastingCompleteCoroutine(int skillID, Vec2Float skillDir)
    {
        var skillData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill, skillID) as SkillGameData;

        //cast effect
        if (skillData.CasterEffectID > 0)
            Managers.Effect.PlayEffect(skillData.CasterEffectID, RootEffect, 0, false, skillDir.X, skillDir.Y);

        yield return new WaitForSecondsRealtime(skillData.CastingTime);
        ClearSkillCastring();
        if (IsMe)
        {
            Game.Instance.PlayerController.UseSkill(skillID, skillDir.X, skillDir.Y);
        }
    }

    public void OnCancelSkillCasting()
    {
        if (skillCastingCoroutine != null)
        {
            StopCoroutine(skillCastingCoroutine);
            skillCastingCoroutine = null;
            _buff.BuffStatus.ClearStatusFlag(ObjectBuffStatus.SkillCasting);
        }
    }
}