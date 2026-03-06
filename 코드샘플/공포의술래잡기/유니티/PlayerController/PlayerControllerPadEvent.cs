using SBSocketSharedLib;
using System;
using UnityEngine;

public partial class PlayerController
{
    private bool IsValidInput()
    {
        bool ret = !PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.TUTORIAL_POPUP);

        return ret;
    }

    public virtual void OnPadEvent(int eventType, TouchPhase touchPhase, Vector2 vec, float level)
    {
        if (!IsValidInput()) return;

        if (ObserverModeCharacterContainer != null && ObserverModeCharacterContainer.Escaped)
        {
            if (eventType == 1)
            {
                if (touchPhase == TouchPhase.Ended)
                    ChangeSurvivorObserverMode();
            }
            return;
        }

        if (!Game.Instance.IsPlay) return;
        if (Game.Instance.PlayerController.Character.State == CreatureStatus.Hiding) return;
        if (Character.Escaped) return;
        if (level > 1) level = 1;
        var moveDir = Character.PosInfo.MoveDir;
        var moveDirVec = new Vector2(moveDir.X, moveDir.Y);

        if (eventType == 0)             // 이동
        {
            if (!Character.IsMovable)
            {
                _inputDir = Vector2.zero;
                return;
            }
            OnMoveEvent(touchPhase, vec, level);
        }
        else if (eventType == 1)        // 추격자 전용 평타 스킬 혹은 맵 오브젝트와 인터랙션
        {
            if (Managers.PlayData.AmIChaser())
                OnUseAttackSkill(touchPhase, vec, _inputDir, level);
            else
                OnUseMapObject(touchPhase);
        }
        else if (eventType == 2)        // 액티브 스킬
        {
            OnUseActiveSkill(touchPhase, vec, _inputDir, level);
        }
    }

    protected virtual void OnMoveEvent(TouchPhase touchPhase, Vector2 vec, float level)
    {
        if (touchPhase == TouchPhase.Began)
        {
            _inputDir = Vector2.zero;
        }
        else if (touchPhase == TouchPhase.Moved)
        {
            if (level < GameConfig.Instance.MOVE_PAD_SENSITIVITY)
            {
                if (Character.State == CreatureStatus.Moving)
                {
                    var curInputTime = DateTime.Now.Ticks;
                    if (curInputTime - _inputTime > 1000000)
                    {
                        OnMove(Vector2.zero);
                        _inputTime = curInputTime;
                        _inputDir = Vector2.zero;
                    }
                }
                return;
            }
            var finalVec = vec;
            var tan = Mathf.Atan2(finalVec.y, finalVec.x);
            var angle = tan * Mathf.Rad2Deg;

            if (angle <= 0)
                angle += 360;

            var angleInfo = GetAngleInfo(angle);
            finalVec.x = angleInfo.x;
            finalVec.y = angleInfo.y;

            // SBDebug.Log($"input move 1 TouchPhase[{touchPhase.ToString()}][vec:{vec}][final:{finalVec}][inputDir:{_inputDir}");

            // 혼란 상태면 반대 방향으로 이동하도록 강제 변경한다
            if (Character.IsConfused)
            {
                finalVec = -finalVec;
            }

            if (_inputDir != finalVec)
            {
                var curInputTime = DateTime.Now.Ticks;
                if (curInputTime - _inputTime > 1000000)
                {
                    OnMove(finalVec);
                    _inputTime = curInputTime;
                    // SBDebug.Log($"input move 2 TouchPhase[{touchPhase.ToString()}][vec:{vec}][final:{finalVec}][inputDir:{_inputDir}");
                }
            }
        }
        else if (touchPhase == TouchPhase.Ended)
        {
            OnMove(Vector2.zero);
        }
    }

    private void OnUseAttackSkill(TouchPhase touchPhase, Vector2 vec, Vector2 moveDirVec, float level)
    {
        if (Character.IsVehicle) return;

        bool isAttack = true;

        if (Target != null && touchPhase == TouchPhase.Ended)
        {
            var mapObject = Target as PropController;
            if (mapObject != null)
            {
                isAttack = false;
                switch (mapObject.MapObjectType)
                {
                    default:
                        isAttack = true;
                        break;
                }
            }
        }

        if (isAttack == false)
        {
            RemoveSkillRangeGuideUI();
            return;
        }

        ShowSkillRangeOnPadEvent(SKILL_TYPE.NORMAL_ATK, touchPhase, vec, moveDirVec, level);
    }

    private void OnUseMapObject(TouchPhase touchPhase)
    {
        //if (Target == null) return;

        if (Character.IsVehicle)//탑승물 탑승시에는 생성/충전 불가
            return;
        if (touchPhase != TouchPhase.Ended)
            return;

        var iter = Managers.Object.Objects.GetEnumerator();
        Vector2 retVec = new Vector2(1000, 1000);
        float retVecMag = retVec.magnitude;
        var basePos = Character.PosInfo.Pos;
        PropController target = null;
        while (iter.MoveNext())
        {
            var go = iter.Current.Value;
            if (go == null || go.activeSelf == false) continue;
            if (go == Character.gameObject) continue;

            var prop = go.GetComponent<PropController>();
            if (prop == null) continue;
            if (prop.enabled == false) continue;
            if (prop.GameObjectType != GameObjectType.MapObject) continue;
            if (prop.MapObjectType != ObjectGameData.MapObjectType.BatteryCreater
                && prop.MapObjectType != ObjectGameData.MapObjectType.BatteryGenerater) continue;

            var vecDis = new Vector2(Mathf.Abs(prop.transform.position.x - basePos.X), Mathf.Abs(prop.transform.position.y - basePos.Y));
            float mag = vecDis.magnitude;
            if (mag < retVecMag)
            {
                retVec = vecDis;
                retVecMag = mag;
                target = prop;
            }
        }

        if (target == null)
        {
            SBDebug.LogError("target is null");
            return;
        }

        if (target.ObjData.interaction_x >= retVec.x && target.ObjData.interaction_y >= retVec.y)
        {
            OnUseMapObject(target);
        }
    }

    public bool OnUseMapObject(PropController mapObject)
    {
        bool buttonAction = false;
        if (mapObject != null && !mapObject.IsBroken)
        {
            switch (mapObject.MapObjectType)
            {
                case ObjectGameData.MapObjectType.Hide:
                    Managers.GameServer.SendHide(mapObject.Id);
                    buttonAction = true;
                    break;

                case ObjectGameData.MapObjectType.Key:
                    if (mapObject.ObjectKeyType == ObjectKeyGameData.ObjectKeyType.ElectricBox)
                    {
                        buttonAction = true;
                        ReqStartEscapeKey();
                    }
                    else if (mapObject.ObjectKeyType == ObjectKeyGameData.ObjectKeyType.EscapeDoor)
                    {
                        buttonAction = true;
                        ReqStartOpenDoor();
                    }
                    break;

                case ObjectGameData.MapObjectType.Vehicle:
                    {
                        RequestRideFromTarget();
                        buttonAction = true;
                    }
                    break;

                case ObjectGameData.MapObjectType.BatteryCreater:
                    {
                        ReqBatteryCreator(mapObject);
                        buttonAction = true;
                    }
                    break;

                case ObjectGameData.MapObjectType.BatteryGenerater:
                    {
                        ReqBatteryGenerator(mapObject);
                        buttonAction = true;
                    }
                    break;
                case ObjectGameData.MapObjectType.Vent:
                    {
                        RequestUserFromVent();
                        buttonAction = true;
                    }break;
            }
        }

        return buttonAction;
    }

    private void OnUseActiveSkill(TouchPhase touchPhase, Vector2 vec, Vector2 moveDirVec, float level)
    {
        ShowSkillRangeOnPadEvent(SKILL_TYPE.ACTIVE_SKILL, touchPhase, vec, moveDirVec, level);
    }

    private void ShowSkillRangeOnPadEvent(SKILL_TYPE skillType, TouchPhase touchPhase, Vector2 vec, Vector2 moveDirVec, float level)
    {
        bool isCanUseSkill = IsEnableSkill(skillType);
        Color color = isCanUseSkill ? new Color(0f, 1f, 1f, 1f) : new Color(1f, 0, 0, 1f);

        var skillInfo = _characterSkill.GetSkillInfo(skillType);
        var skillLevel = skillType == SKILL_TYPE.ACTIVE_SKILL ? skillInfo.SkillLevel : 1;
        var skill = skillInfo.Data.GetMajorSkill(skillLevel);
        var skillVec = level > 0.2f ? vec : moveDirVec;
        skillVec = skillVec.normalized;

        bool isAoe = false;
        SkillSummonGameData summonData = null;
        if (skill.Type == SkillBaseGameData.SkillType.Projectile)
        {
            summonData = Managers.Data.GetData(GameDataManager.DATA_TYPE.skill_summon, skill.SummonID) as SkillSummonGameData;
            isAoe = (summonData.Type == SkillSummonGameData.SummonType.Trap || summonData.Type == SkillSummonGameData.SummonType.AreaOfEffect);
        }

        float plusDist = 0.0f;
        if (skillType == SKILL_TYPE.NORMAL_ATK)
            plusDist = Character.Stat.AttackDist;
        else 
            plusDist = Character.Stat.SkillDist;
        var startRange = skill.RangeStart * 0.001f;
        switch (touchPhase)
        {
            case TouchPhase.Began:
            case TouchPhase.Moved:

                if (skill.RangeType == SkillRangeType.Rectangle)
                {
                    

                    var rangeArea = new Vector2((skill.RangeDistance * 0.001f) + plusDist, skill.RangeAngle * 0.001f);
                    if (isAoe)
                    {
                        if (summonData.RangeType == SkillRangeType.Rectangle)
                        {
                            rangeArea = new Vector2(summonData.RangeDistance * 0.001f, summonData.RangeAngle * 0.001f);
                            startRange = skill.RangeDistance * 0.0005f;
                        }
                        else if (summonData.RangeType == SkillRangeType.Circle)
                        {
                            startRange = skill.RangeDistance * 0.001f;
                            ShowSkillRangeGuideUI(skillVec, summonData.RangeDistance * 0.001f, summonData.RangeAngle, color, startRange);
                            break;
                        }
                    }
                    ShowSkillRangeGuideUI(skillVec, rangeArea, color, startRange);
                }
                else if (skill.RangeType == SkillRangeType.Circle)
                {
                    ShowSkillRangeGuideUI(skillVec, (skill.RangeDistance * 0.001f) + plusDist, skill.RangeAngle, color, startRange);
                }

                if (skillType == SKILL_TYPE.ACTIVE_SKILL)
                {
                    ShowSkillDescGuide();
                }
                break;

            case TouchPhase.Stationary:
                UpdateSkillRangePosition(color, skill.RangeType);
                break;

            case TouchPhase.Ended:
                if (isCanUseSkill)
                {
                    if (skillInfo.Data.CastingTime > 0)
                    {
                        // 캐스팅 스킬
                        UseSkillCasting(skillInfo.Data.GetID(), skillVec.x, skillVec.y, skillInfo.Data.CastingTime);
                        Character.WaitSkillCasting(skillVec.x, skillVec.y, Character);
                    }
                    else
                    {
                        // 즉발 스킬
                        UseSkill(skillInfo.Data.GetID(), skillVec.x, skillVec.y);
                    }
                }
                RemoveSkillRangeGuideUI();

                if (skillType == SKILL_TYPE.ACTIVE_SKILL)
                {
                    HideSkillDescGuide();
                }
                break;
        }
    }

    void ShowSkillDescGuide()
    {
        Game.Instance.UIGame.ShowSkillGuide();
    }

    void HideSkillDescGuide()
    {
        Game.Instance.UIGame.HideSkillGuide();
    }
}
