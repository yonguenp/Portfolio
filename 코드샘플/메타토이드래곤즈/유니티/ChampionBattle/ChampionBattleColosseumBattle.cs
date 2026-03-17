using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
    public class ChampionBattleColosseumBattle : BattleStateLogic
    {
        private int TimeBuffCount = 0;
        protected List<CameraTarget> tempInfluence = null;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                offenses.Clear();
                defenses.Clear();
                targets.Clear();
                soundDelays.Clear();
                projectiles.Clear();
                TimeBuffCount = 0;

                for (int i = 0, count = Stage.OffenseSpines.Count; i < count; ++i)
                {
                    var it = Stage.OffenseSpines[i];
                    if (it == null)
                        continue;
                    for (int j = 0, jCount = it.Count; j < jCount; ++j)
                    {
                        var character = it[j];
                        if (character == null)
                            continue;

                        character.Data.SetActiveSkilType(eBattleSkillType.None);
                        offenses.Add(character);
                        character.GetComponent<Collider2D>().enabled = false;
                        character.StopDust();
                        character.SetRigidbodySimulated(false);
                    }
                }
                for (int i = 0, count = Stage.DefenseSpines.Count; i < count; ++i)
                {
                    var it = Stage.DefenseSpines[i];
                    if (it == null)
                        continue;

                    for (int j = 0, jCount = it.Count; j < jCount; ++j)
                    {
                        var character = it[j];
                        if (character == null)
                            continue;

                        character.Data.SetActiveSkilType(eBattleSkillType.None);
                        defenses.Add(character);
                        character.GetComponent<Collider2D>().enabled = false;
                        character.StopDust();
                        character.SetRigidbodySimulated(false);
                    }
                }

                targets = GetCameraTargets();

                for (int i = 0, count = offenses.Count; i < count; ++i)
                {
                    if (offenses[i] == null || offenses[i].Data == null || offenses[i].Data.Death)
                        offenseDeath++;
                }
                for (int i = 0, count = defenses.Count; i < count; ++i)
                {
                    if (defenses[i] == null || defenses[i].Data == null || defenses[i].Data.Death)
                        defenseDeath++;
                }

                ChampionManager.Instance.SetPlay(true);
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                soundDelays.Clear();
                projectiles.Clear();
                TimeBuffCount = 0;

                for (int i = 0, count = offenses.Count; i < count; ++i)
                {
                    if (offenses[i] == null)
                        continue;

                    offenses[i].ClearBuffStat();
                    if (offenses[i].Data.Death)
                        continue;

                    offenses[i].ClearAnimation();
                    offenses[i].ClearEffectSpine();
                    offenses[i].Data.SetActionCoroutine(null);
                    offenses[i].Data.ClearTypeInfo(eSkillEffectType.AIRBORNE);
                    offenses[i].Data.ClearTypeInfo(eSkillEffectType.STUN);
                    offenses[i].Data.ClearTypeInfo(eSkillEffectType.AGGRO);
                    offenses[i].Data.ClearTypeInfo(eSkillEffectType.AGGRO_R);
                }
                EffectReceiverClearEvent.Send();

                for (int i = 0, count = defenses.Count; i < count; ++i)
                {
                    if (defenses[i] == null)
                        continue;

                    defenses[i].ClearBuffStat();
                    if (defenses[i].Data.Death)
                        continue;

                    defenses[i].ClearAnimation();
                    defenses[i].ClearEffectSpine();
                    defenses[i].Data.SetActionCoroutine(null);
                    defenses[i].Data.ClearTypeInfo(eSkillEffectType.AIRBORNE);
                    defenses[i].Data.ClearTypeInfo(eSkillEffectType.STUN);
                    defenses[i].Data.ClearTypeInfo(eSkillEffectType.AGGRO);
                    defenses[i].Data.ClearTypeInfo(eSkillEffectType.AGGRO_R);
                }

                Data.CheckTimeOver();
                if (defenseDeath == defenses.Count)
                    ChampionManager.Instance.ChampionData.SetWinType(eChampionWinType.SIDE_A_WIN);
                else if (offenseDeath == offenses.Count)
                    ChampionManager.Instance.ChampionData.SetWinType(eChampionWinType.SIDE_B_WIN);

                return true;
            }
            return false;
        }


        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return UpdateLogic(dt);
            }
            return !IsPlaying;
        }
        

        protected virtual bool UpdateLogic(float dt)
        {
            if (!ChampionManager.Instance.Playing)
                return false;

            Data.Time += dt;
            if (Data.CheckTimeOver())
                return false;

            CheckTimeBuff();

            var frameLog = ChampionManager.Instance.PopFrameLog();
            if (frameLog != null)
            {
                UpdateLog(dt, frameLog);
            }
            else
            {
                //오류 예외사항
                UpdateSeed(dt, frameLog);
            }

            UpdateProjectile(dt);//투사채 처리
            UpdateSounds(dt);//사운드 재생
            CharacterSKill();//스킬 처리

            bool deathCheck = DeathCheck();
            if (targets != null)
            {
                targets.Sort((a, b) =>
                {
                    if (a.TargetPosition.x > b.TargetPosition.x)
                        return 1;
                    else if (a.TargetPosition.x < b.TargetPosition.x)
                        return -1;
                    else
                        return 0;
                });
                int i = 0;

                if (tempInfluence == null)
                    tempInfluence = new();
                else
                    tempInfluence.Clear();
                for (int t = 0; t < targets.Count; t++)
                {
                    targets[t].TargetInfluence = 0f;
                    targets[t].TargetOffset = Vector2.zero;
                    var targetSpine = targets[t].TargetTransform.GetComponent<BattleSpine>();
                    if (targetSpine != null)
                    {
                        if (!targetSpine.Data.Death)
                        {
                            tempInfluence.Add(targets[t]);
                            i++;
                        }
                    }
                }

                if (tempInfluence != null && tempInfluence.Count > 0)
                {
                    int min = 0;
                    int max = tempInfluence.Count - 1;
                    tempInfluence[min].TargetInfluenceH = 0f;
                    tempInfluence[max].TargetInfluenceH = 0f;
                    tempInfluence[min].TargetInfluenceH += .5f;
                    tempInfluence[max].TargetInfluenceH += .5f;
                }
            }

            return deathCheck || projectiles.Count > 0;
        }

        void UpdateLog(float dt, FrameLog replayLogs)
        {
            for (int i = 0, count = offenses.Count; i < count; ++i)
            {
                var character = offenses[i];
                if (character == null)
                    continue;

                CharacterActionByLog(character.Data, dt, replayLogs.GetActionLogA(character.Data.ID));
                character.UpdateStatus(dt);
                SkillCast(character.Data);
            }
            for (int i = 0, count = defenses.Count; i < count; ++i)
            {
                var character = defenses[i];
                if (character == null)
                    continue;

                CharacterActionByLog(character.Data, dt, replayLogs.GetActionLogB(character.Data.ID));
                character.UpdateStatus(dt);
                SkillCast(character.Data);
            }
        }

        void UpdateSeed(float dt, FrameLog replayLogs)
        {
            for (int i = 0, count = offenses.Count; i < count; ++i)
            {
                var character = offenses[i];
                if (character == null)
                    continue;

                CharacterActionByLogSeed(character.Data, dt, replayLogs != null ? replayLogs.GetActionLogA(character.Data.ID) : null);
                character.UpdateStatus(dt);
                SkillCast(character.Data);
            }
            for (int i = 0, count = defenses.Count; i < count; ++i)
            {
                var character = defenses[i];
                if (character == null)
                    continue;

                CharacterActionByLogSeed(character.Data, dt, replayLogs != null ? replayLogs.GetActionLogB(character.Data.ID) : null);
                character.UpdateStatus(dt);
                SkillCast(character.Data);
            }
        }
        private float GetNextBuffTime()
        {
            return (TimeBuffCount + 1) * GameConfigTable.GetArenaBuffTime();
        }
        public void CheckTimeBuff()
        {
            if (GetNextBuffTime() > Data.Time)
                return;

            ++TimeBuffCount;
            var effect = SkillEffectData.GetGroup(GameConfigTable.GetArenaBuffEffectGroup());
            if (effect == null)
                return;

            ToastManager.Instance.Set(StringData.GetStringByStrKey("arena_buff_message"), false, 2);
            for (int e = 0, eCount = effect.Count; e < eCount; ++e)
            {
                if (effect[e] == null)
                    continue;

                for (int i = 0, count = offenses.Count; i < count; ++i)
                {
                    var character = offenses[i];
                    if (character == null || character.Data == null || character.Data.Death)
                        continue;

                    EffectTriggerTarget(character.Data, character.Data, effect[e]);
                }
                for (int i = 0, count = defenses.Count; i < count; ++i)
                {
                    var character = defenses[i];
                    if (character == null || character.Data == null || character.Data.Death)
                        continue;

                    EffectTriggerTarget(character.Data, character.Data, effect[e]);
                }
            }
            ArenaTimeBuff.Send();
        }

        protected virtual void CharacterActionByLog(IBattleCharacterData aData, float dt, ActionLog log)
        {
            if (aData == null)
            {
                return;
            }

            var spine = aData.GetSpine();
            if (spine == null)
            {
                return;
            }

            if (aData.Death)
            {
                return;
            }

            ActionLog.ActionType type = ActionLog.ActionType.None;
            if (log != null)
            {
                type = log.actionType;
                aData.Transform.position = log.GetPosition();
            }

            
            var skill = CheckSkill(aData);
            
            switch (type)
            {
                case ActionLog.ActionType.None:
                {
                    //의도적 패싱
                }
                break;
                case ActionLog.ActionType.Skip:
                {
                    var action = log as SkipActionLog;
                    if (action.isKnockback)
                    {
                        if (WallCheck(aData, dt))
                        {
                            for (int i = 0, count = aData.Infos.Count; i < count; ++i)
                            {
                                if (SBDefine.TYPE_KNOCKBACK != aData.Infos[i].GetType())
                                    continue;

                                KnockbackEffect eff = (KnockbackEffect)aData.Infos[i];
                                if (eff.IsPlaying)
                                    eff.SetStopBack();
                            }
                        }
                    }
                    return;
                }
                break;
                case ActionLog.ActionType.Move:
                {
                    var action = log as MoveActionLog;

                    if (action.IsActioning)
                    {
                        spine.ClearEffectSpine();
                        spine.ClearAnimation();
                        aData.SetActiveSkilType(eBattleSkillType.None);
                        aData.SetActionCoroutine(null);
                    }

                    spine.Controller.MoveWorldTargetUpdate(dt, action.goal, false, SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED);
                    spine.SetAnimation(eSpineAnimation.WALK);

                }
                break;
                case ActionLog.ActionType.Skill:
                {
                    var action = log as SkillActionLog;

                    if (action.IsActioning)
                    {
                        spine.ClearEffectSpine();
                        spine.ClearAnimation();
                        aData.SetActiveSkilType(eBattleSkillType.None);
                        aData.SetActionCoroutine(null);
                    }

                    aData.SetActionCoroutine(AttackCoroutine(aData, action.GetSkill(aData)));
                }
                break;
                case ActionLog.ActionType.Attack:
                {
                    var action = log as ActtackActionLog;

                    aData.SetActionCoroutine(AttackCoroutine(aData, action.GetSkill(aData)));
                }
                break;
                case ActionLog.ActionType.SetPos:
                {
                    var action = log as SetPosActionLog;

                    spine.SetAnimation(eSpineAnimation.IDLE);

                    var spaceOffset = action.goal;
                    if (spaceOffset != Vector3.zero)
                    {
                        spine.Controller.MoveWorldTargetUpdate(dt, aData.Transform.position + spaceOffset.normalized, false, SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED * 0.5f);
                    }
                }
                break;
                default:
                {
                    Debug.Log(":::: 여기로 들어오면 안되는데..");
                }
                break;
            }

            WallCheck(aData, dt);
        }

        protected virtual void CharacterActionByLogSeed(IBattleCharacterData aData, float dt, ActionLog log)
        {
            if (aData == null)
            {
                return;
            }

            var spine = aData.GetSpine();
            if (spine == null)
            {
                return;
            }

            if (aData.Death)
            {
                return;
            }

            ActionLog.ActionType type = ActionLog.ActionType.None;
            if (log != null)
            {
                type = log.actionType;
                if(aData.Transform.position != log.GetPosition())
                {
                    Debug.LogError("Diff Position : " + aData.Transform.position + " / " + log.GetPosition());
                }
            }

            if (aData.IsActionSkip())
            {
                if(type != ActionLog.ActionType.Skip)
                {
                    Debug.LogError("Diff Type : Skip / " + type);
                }

                if (aData.IsEffectInfo(eSkillEffectType.KNOCK_BACK))
                {
                    if (WallCheck(aData, dt))
                    {
                        for (int i = 0, count = aData.Infos.Count; i < count; ++i)
                        {
                            if (SBDefine.TYPE_KNOCKBACK != aData.Infos[i].GetType())
                                continue;

                            KnockbackEffect eff = (KnockbackEffect)aData.Infos[i];
                            if (eff.IsPlaying)
                                eff.SetStopBack();
                        }
                    }
                }

                return;
            }
             

            var skill = CheckSkill(aData);
            if (skill == null)
            {
                return;
            }

            IBattleCharacterData moveTarget = FindTarget(aData, skill);
            if (moveTarget != null)//이동 할 곳이 있음
            {
                var goal = GetMoveDestinationPosition(aData, moveTarget);
                var direction = goal - spine.Controller.transform.position;
                var x = direction.x;
                if (x < 0)
                    spine.Controller.transform.localScale = new Vector3(spine.Controller.IsRight ? Mathf.Abs(spine.Controller.transform.localScale.x) : -Mathf.Abs(spine.Controller.transform.localScale.x), spine.Controller.transform.localScale.y, spine.Controller.transform.localScale.z);
                else if (x > 0)
                    spine.Controller.transform.localScale = new Vector3(spine.Controller.IsRight ? -Mathf.Abs(spine.Controller.transform.localScale.x) : Mathf.Abs(spine.Controller.transform.localScale.x), spine.Controller.transform.localScale.y, spine.Controller.transform.localScale.z);

                goal = direction.normalized * SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED;
                goal += spine.Controller.transform.position;

                if (type != ActionLog.ActionType.Move)
                {
                    Debug.LogError("Diff Type : Move / " + type);
                }

                if (aData.IsActioning)
                {
                    spine.ClearEffectSpine();
                    spine.ClearAnimation();
                    aData.SetActiveSkilType(eBattleSkillType.None);
                    aData.SetActionCoroutine(null);
                }

                spine.Controller.MoveWorldTargetUpdate(dt, goal, false, SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED);

                spine.SetAnimation(eSpineAnimation.WALK);
            }
            else//공격 대상 탐색
            {

                if (skill.SkillType is eBattleSkillType.Skill1)
                {
                    if (type != ActionLog.ActionType.Skill)
                    {
                        Debug.LogError("Diff Type : Skill / " + type);
                    }

                    if (aData.IsActioning)
                    {
                        spine.ClearEffectSpine();
                        spine.ClearAnimation();
                        aData.SetActiveSkilType(eBattleSkillType.None);
                        aData.SetActionCoroutine(null);
                    }

                    aData.SetActionCoroutine(AttackCoroutine(aData, skill));

                }
                else if (aData.IsAttackSkip() is false)
                {
                    if (type != ActionLog.ActionType.Attack)
                    {
                        Debug.LogError("Diff Type : Attack / " + type);
                    }

                    aData.SetActionCoroutine(AttackCoroutine(aData, skill));
                }
                else//정지 => 자기 포지션을 찾아 이동 해당 타겟이 바뀐다면 멈춤
                {
                    var spaceOffset = GetSpaceOffsetVector(aData, true);

                    if (type != ActionLog.ActionType.SetPos)
                    {
                        Debug.LogError("Diff Type : SetPos / " + type);
                    }

                    spine.SetAnimation(eSpineAnimation.IDLE);

                    if (spaceOffset != Vector3.zero)
                    {
                        spine.Controller.MoveWorldTargetUpdate(dt, aData.Transform.position + spaceOffset.normalized, false, SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED * 0.5f);
                    }

                }
            }

            WallCheck(aData, dt);
        }

        protected override Vector3 GetSpaceOffsetVector(IBattleCharacterData mine, bool both = true)
        {
            Vector3 ret = Vector2.zero;
            var myCollider = mine.GetCircleCollider();
            Vector3 center = myCollider.bounds.center;
            var mySpine = mine.GetSpine();

            bool isContain = false;
            if (both || !mine.IsEnemy)
            {
                foreach (BattleSpine checker in offenses)
                {
                    if (checker == null || checker == mySpine || checker.Data.Death)
                        continue;

                    var collider = checker.Controller.myCollider;
                    if (false == collider.bounds.Contains(center))
                        continue;

                    var colCenter = collider.bounds.center;
                    if (center == colCenter || center.y == colCenter.y)
                    {
                        if (mine.IsEnemy)
                            ret.x += 0.5f;
                        else
                            ret.x -= 0.5f;

                        if (mine.Position < checker.Data.Position)
                            ret.y += 0.3f;
                        else
                            ret.y -= 0.3f;
                    }
                    else
                    {
                        if (mine.IsEnemy == checker.Data.IsEnemy)
                            ret += (center - colCenter) * 0.5f;
                        else
                            ret += (center - colCenter) * 0.6f;
                    }

                    isContain = true;
                    break;
                }
            }

            if (both || mine.IsEnemy)
            {
                foreach (BattleSpine checker in defenses)
                {
                    if (checker == null || checker == mySpine || checker.Data.Death)
                        continue;

                    var collider = checker.Controller.myCollider;
                    if (false == collider.bounds.Contains(center))
                        continue;

                    var colCenter = collider.bounds.center;
                    if (center == colCenter || center.y == colCenter.y)
                    {
                        if (mine.IsEnemy)
                            ret.x += 0.5f;
                        else
                            ret.x -= 0.5f;

                        if (mine.Position < checker.Data.Position)
                            ret.y += 0.3f;
                        else
                            ret.y -= 0.3f;
                    }
                    else
                    {
                        if (mine.IsEnemy == checker.Data.IsEnemy)
                            ret += (center - colCenter) * 0.5f;
                        else
                            ret += (center - colCenter) * 0.6f;
                    }
                    isContain = true;
                    break;
                }
            }

            if (isContain)
                return OffsetCheck(mine, ret);
            else
                return Vector3.zero;
        }

        protected override Vector3 OffsetCheck(IBattleCharacterData mine, Vector3 ret)
        {
            if (ret.x == 0f)
            {
                if (mine.KnockBackDirection == eDirectionBit.Right)
                    ret.x += 0.05f;
                else
                    ret.x -= 0.05f;
            }
            else
                ret.x = 0.1f * (mine.KnockBackDirection == eDirectionBit.Right ? Mathf.Abs(ret.x) : -Mathf.Abs(ret.x));

            if (ret.y == 0f)
            {
                if (mine.GetCircleCollider().bounds.center.y >= 0f)
                    ret.y += 0.05f;
                else
                    ret.y -= 0.05f;
            }

            return ret;
        }
        protected override Vector3 EffectPos(SkillResourceData data, IBattleCharacterData casterData, IBattleCharacterData targetData, Vector3 pos)
        {
            if (data == null)
                return Vector3.zero;

            //Vector3 cPos;
            //if (casterData.Transform != null && casterData.Transform.localScale.x < 0)
            //    cPos = new Vector3(targetData.Transform.position.x - pos.x, targetData.Transform.position.y + pos.y, 0f);
            //else
            //    cPos = new Vector3(targetData.Transform.position.x + pos.x, targetData.Transform.position.y + pos.y, 0f);
            var cPos = new Vector3(targetData.Transform.position.x + pos.x, targetData.Transform.position.y + pos.y, 0f);

            switch (data.LOCATION)
            {
                case eSkillResourceLocation.TOP:
                {
                    var effectTr = targetData.EffectTransform;
                    if (effectTr != null)
                        cPos = new Vector3(effectTr.position.x + pos.x, effectTr.position.y + pos.y, 0f);
                }
                break;
                case eSkillResourceLocation.BOTTOM:
                    break;
                case eSkillResourceLocation.COLLIDER:
                {
                    if (casterData != targetData)
                    {
                        Vector2 rv = new Vector2(0.0f, 0.0f).normalized;
                        var collider = targetData.GetCircleCollider();
                        if (collider != null)
                        {
                            Vector3 targetScale = targetData.Transform.localScale;
                            rv *= (collider.radius * targetScale.x) * 0.5f;
                            pos.x += rv.x + (collider.offset.x * targetScale.x);
                            pos.y += rv.y + (collider.offset.y * targetScale.y);
                        }
                    }
                }
                break;
                default:
#if DEBUG
                    Debug.LogError("EffectPos => data LOCATION Error");
#endif
                    break;
            }
            return cPos;
        }

        protected override void OnUISkillEffect(int id, eSpineAnimation skill, float v, BattleSpine caster)
        {
            //UISkillCutSceneEvent.Send(id, skill, v, caster);
        }

        public override float RandomVal()
        {
            return Random(0.0f, 1.0f);
        }

        public override int Random(int min, int max, RandomLog.RandomReason reason = RandomLog.RandomReason.NONE)
        {
            int b = ChampionManager.Instance.Random.Next(min, max);

            int ret = ChampionManager.Instance.OnRandomLog(Data.Time, b, reason);

            return ret;
        }


        public override float Random(float min, float max, RandomLog.RandomReason reason = RandomLog.RandomReason.NONE)
        {
            float ret = min + ((max - min) * ((float)ChampionManager.Instance.Random.NextDouble()));

            return ret;
        }


        protected override int BaseCalc(IBattleCharacterData caster, IBattleCharacterData target, SkillLevelStat skillStat, eBattleSkillType type)
        {
            var b = BaseCalcLogic(caster, target, skillStat, type);
            var ret = ChampionManager.Instance.OnDamage(caster, target, b, Data.Time);

            return ret;
        }

        protected virtual int BaseCalcLogic(IBattleCharacterData caster, IBattleCharacterData target, SkillLevelStat skillStat, eBattleSkillType type)
        {
            return base.BaseCalc(caster, target, skillStat, type);
        }
    }

    public class ChampionPracticeColosseumBattle : ChampionBattleColosseumBattle
    {
        protected override bool UpdateLogic(float dt)
        {
            Data.Time += dt;
            if (Data.CheckTimeOver())
                return false;

            CheckTimeBuff();
            //전투 로직
            for (int i = 0, count = offenses.Count; i < count; ++i)
            {
                var character = offenses[i];
                if (character == null)
                    continue;
                CharacterAction(character.Data, dt);
                character.UpdateStatus(dt);

                SkillCast(character.Data);
            }
            for (int i = 0, count = defenses.Count; i < count; ++i)
            {
                var character = defenses[i];
                if (character == null)
                    continue;
                CharacterAction(character.Data, dt);
                character.UpdateStatus(dt);

                SkillCast(character.Data);
            }
            //

            UpdateProjectile(dt);//투사채 처리
            UpdateSounds(dt);//사운드 재생
            CharacterSKill();//스킬 처리

            bool deathCheck = DeathCheck();
            if (targets != null)
            {
                targets.Sort((a, b) =>
                {
                    if (a.TargetPosition.x > b.TargetPosition.x)
                        return 1;
                    else if (a.TargetPosition.x < b.TargetPosition.x)
                        return -1;
                    else
                        return 0;
                });
                int i = 0;

                if (tempInfluence == null)
                    tempInfluence = new();
                else
                    tempInfluence.Clear();
                for (int t = 0; t < targets.Count; t++)
                {
                    targets[t].TargetInfluence = 0f;
                    targets[t].TargetOffset = Vector2.zero;
                    var targetSpine = targets[t].TargetTransform.GetComponent<BattleSpine>();
                    if (targetSpine != null)
                    {
                        if (!targetSpine.Data.Death)
                        {
                            tempInfluence.Add(targets[t]);
                            i++;
                        }
                    }
                }

                if (tempInfluence != null && tempInfluence.Count > 0)
                {
                    int min = 0;
                    int max = tempInfluence.Count - 1;
                    tempInfluence[min].TargetInfluenceH = 0f;
                    tempInfluence[max].TargetInfluenceH = 0f;
                    tempInfluence[min].TargetInfluenceH += .5f;
                    tempInfluence[max].TargetInfluenceH += .5f;
                }
            }

            return deathCheck || projectiles.Count > 0;
        }

        public override int Random(int min, int max, RandomLog.RandomReason reason = RandomLog.RandomReason.NONE)
        {
            int b = ChampionManager.Instance.Random.Next(min, max);

            int ret = b;// ChampionManager.Instance.OnRandomLog(Data.Time, b, reason);

            return ret;
        }

        public override float Random(float min, float max, RandomLog.RandomReason reason = RandomLog.RandomReason.NONE)
        {
            float ret = min + ((max - min) * ((float)ChampionManager.Instance.Random.NextDouble()));

            return ret;
        }


        protected override int BaseCalc(IBattleCharacterData caster, IBattleCharacterData target, SkillLevelStat skillStat, eBattleSkillType type)
        {
            var b = BaseCalcLogic(caster, target, skillStat, type);
            var ret = b;// ChampionManager.Instance.OnDamage(caster, target, b, Data.Time);

            return ret;
        }
    }

}