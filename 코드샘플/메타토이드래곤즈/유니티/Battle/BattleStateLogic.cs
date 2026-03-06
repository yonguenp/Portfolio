using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public partial class BattleStateLogic : BattleState
    {
        private readonly float damageX = 0.2f;
        private readonly float damageY = -0.2f;

        protected Circle circle = new(Vector2.zero, 1f, 1f);
        protected SBRect rect = new(Vector2.zero, 1f, 1f);
        protected Cone cone = new(Vector2.zero, eDirectionBit.None, 1f, 1f);
        protected List<SBAttackSfxSound> soundDelays = new();
        public List<BattleSpine> offenses { get; protected set; } = new();
        public List<BattleSpine> defenses { get; protected set; } = new();
        protected List<IBattleEventObject> projectiles = new();
        public List<CameraTarget> targets { get; protected set; } = new();
        protected int offenseDeath = 0;
        protected int defenseDeath = 0;
        protected Explosion explosion = new();

        protected AdditionalStatus passiveCriStatus = new AdditionalStatus();

        List<Collider2D> mapColliders = new List<Collider2D>();

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                mapColliders = new List<Collider2D>(Stage.Map.Colliders.GetComponentsInChildren<Collider2D>());
                passiveCriStatus.Initialze();
                offenseDeath = 0;
                defenseDeath = 0;
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            passiveCriStatus.Initialze();
            offenseDeath = 0;
            defenseDeath = 0;

            for (int i = 0, count = offenses.Count; i < count; ++i)
            {
                var character = offenses[i];
                if (character == null)
                    continue;

                character.Data.ClearTypeInfo(eSkillEffectType.STAT);
            }

            for (int i = 0, count = defenses.Count; i < count; ++i)
            {
                var character = defenses[i];
                if (character == null)
                    continue;

                character.Data.ClearTypeInfo(eSkillEffectType.STAT);
            }

            return base.OnExit();
        }
        protected virtual void UpdateProjectile(float dt)
        {
            if (projectiles == null)
                return;

            for (int i = 0, count = projectiles.Count; i < count; ++i)
            {
                if (projectiles[i] == null)
                    continue;

                projectiles[i].Update(dt);
            }

            projectiles.RemoveAll(projectile => projectile.IsEnd);
        }
        protected virtual eBattleSide GetSide(IBattleCharacterData aData)
        {
            return aData.IsEnemy ? eBattleSide.DefenseSide_1 : eBattleSide.OffenseSide_1;
        }
        protected virtual void CharacterAction(IBattleCharacterData aData, float dt)
        {
            if (aData == null)
                return;

            var spine = aData.GetSpine();
            if (spine == null)
                return;

            if (aData.Death)
                return;

            if (aData.IsActionSkip())
            {
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
                return;

            IBattleCharacterData moveTarget = FindTarget(aData, skill);
            if (moveTarget != null)//이동 할 곳이 있음
            {
                if (aData.IsActioning)
                {
                    spine.ClearEffectSpine();
                    spine.ClearAnimation();
                    aData.SetActiveSkilType(eBattleSkillType.None);
                    aData.SetActionCoroutine(null);
                }

                var goal = GetMoveDestinationPosition(aData, moveTarget);
                var direction = goal - spine.Controller.transform.position;
                var x = direction.x;
                if (x < 0)
                    spine.Controller.transform.localScale = new Vector3(spine.Controller.IsRight ? Mathf.Abs(spine.Controller.transform.localScale.x) : -Mathf.Abs(spine.Controller.transform.localScale.x), spine.Controller.transform.localScale.y, spine.Controller.transform.localScale.z);
                else if (x > 0)
                    spine.Controller.transform.localScale = new Vector3(spine.Controller.IsRight ? -Mathf.Abs(spine.Controller.transform.localScale.x) : Mathf.Abs(spine.Controller.transform.localScale.x), spine.Controller.transform.localScale.y, spine.Controller.transform.localScale.z);

                goal = direction.normalized * SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED;
                goal += spine.Controller.transform.position;

                spine.Controller.MoveWorldTargetUpdate(dt, goal, false, SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED);

                spine.SetAnimation(eSpineAnimation.WALK);
            }
            else//공격 대상 탐색
            {
                if (skill.SkillType is eBattleSkillType.Skill1)
                {
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
                    aData.SetActionCoroutine(AttackCoroutine(aData, skill));
                }
                else//정지 => 자기 포지션을 찾아 이동 해당 타겟이 바뀐다면 멈춤
                {
                    spine.SetAnimation(eSpineAnimation.IDLE);

                    var spaceOffset = GetSpaceOffsetVector(aData, true);
                    if (spaceOffset != Vector3.zero)
                        spine.Controller.MoveWorldTargetUpdate(dt, aData.Transform.position + spaceOffset.normalized, false, SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED * 0.5f);
                }
            }

            WallCheck(aData, dt);
        }
        /// <summary>
        /// 스킬 대기열 시스템 CharacterAction 끝난 후 호출
        /// </summary>
        protected virtual void CharacterSKill()
        {
            if (!Data.IsAuto && Data.SelectSkillCharacter != null)
            {
                SkillCast(Data.SelectSkillCharacter);
                Data.SetSelectSkillCharacter(null);
            }
            Data.SkillQueueSortCast(eBattleSide.OffenseSide_1);
            Data.SkillQueueSortCast(eBattleSide.DefenseSide_1);
        }
        protected virtual bool WallCheck(IBattleCharacterData aData, float dt)
        {
            if (aData == null || aData.Transform == null)
                return false;

            var enemyCenter = GetEnemyOffsetVector(aData);
            var wallCenter = GetWallOffsetVector(aData);
            if (wallCenter != Vector3.zero)
            {
                aData.GetSpine().Controller.MoveWorldTargetUpdate(dt, aData.Transform.position + (wallCenter + enemyCenter).normalized, false, (SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED));
                return true;
            }
            else if (enemyCenter != Vector3.zero)
            {
                aData.GetSpine().Controller.MoveWorldTargetUpdate(dt, aData.Transform.position + enemyCenter.normalized, false, (SBDefine.AdventureSpeedRatio * aData.MOVE_SPEED));
            }
            return false;
        }
        protected List<BattleSpine> GetTeam(eBattleSide side)
        {
            return side switch
            {
                eBattleSide.DefenseSide_1 => defenses,
                _ => offenses
            };
        }
        protected virtual Vector3 OffsetCheck(IBattleCharacterData mine, Vector3 ret)
        {
            if (ret.x == 0f)
            {
                if (mine.KnockBackDirection == eDirectionBit.Right)
                    ret.x += Random(0.01f, 0.1f);
                else
                    ret.x -= Random(0.01f, 0.1f);
            }
            else
                ret.x = 0.1f * (mine.KnockBackDirection == eDirectionBit.Right ? Mathf.Abs(ret.x) : -Mathf.Abs(ret.x));

            if (ret.y == 0f)
            {
                if (mine.GetCircleCollider().bounds.center.y >= 0f)
                    ret.y += Random(0.1f, 0.5f);
                else
                    ret.y -= Random(0.1f, 0.5f);
            }

            return ret;
        }
        protected virtual Vector3 GetMoveDestinationPosition(IBattleCharacterData mine, IBattleCharacterData target)
        {
            if (mine == null || target == null || target.Transform == null)
                return Vector3.zero;

            Vector3 position = (GetCircleColliderPos(mine.GetCircleCollider())
                - GetCircleColliderPos(target.GetCircleCollider())).normalized
                * GetCircleColliderRadius(mine.GetCircleCollider());

            return target.Transform.position + position;
        }
        protected Vector3 GetWallOffsetVector(IBattleCharacterData mine)
        {
            Vector3 ret = Vector2.zero;
            var myCollider = mine.GetCircleCollider();
            Vector3 center = myCollider.bounds.center;

            bool contain = false;

            foreach (Collider2D collider in mapColliders)
            {
                if (!collider.bounds.Contains(center))
                    continue;

                ret += center - collider.bounds.center;
                contain = true;
            }

            ret.x = 0f;
            if (contain)
                return ret;
            else
                return Vector3.zero;
        }
        protected virtual Vector3 GetEnemyOffsetVector(IBattleCharacterData mine)
        {
            Vector3 ret = Vector2.zero;
            var myCollider = mine.GetCircleCollider();
            Vector3 center = myCollider.bounds.center;

            bool contain = false;
            var cols = GetTeam(mine.IsEnemy ? eBattleSide.OffenseSide_1 : eBattleSide.DefenseSide_1);
            if (cols != null)
            {
                for (int i = 0, count = cols.Count; i < count; ++i)
                {
                    if (cols[i] == null || cols[i].Data.Death)
                        continue;

                    var col = cols[i].Controller.myCollider;
                    if (!col.bounds.Contains(center))
                        continue;

                    ret += (col.bounds.center - center);
                    contain = true;
                    break;
                }
            }

            if (contain)
                return OffsetCheck(mine, ret);
            else
                return Vector3.zero;
        }
        protected virtual Vector3 GetSpaceOffsetVector(IBattleCharacterData mine, bool both = true)
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
                            ret.y += Random(0.1f, 0.5f);
                        else
                            ret.y -= Random(0.1f, 0.5f);
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
                            ret.y += Random(0.1f, 0.5f);
                        else
                            ret.y -= Random(0.1f, 0.5f);
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

        protected Vector2 GetCircleColliderPos(CircleCollider2D collider)
        {
            if (collider == null)
                return Vector2.zero;

            return collider.bounds.center;
        }
        protected float GetCircleColliderRadius(CircleCollider2D collider)
        {
            if (collider == null)
                return 0.0f;

            return collider.radius * Mathf.Abs(collider.transform.localScale.x);
        }
        protected IEnumerator AttackCoroutine(IBattleCharacterData casterData, SBSkill skill)
        {
            switch (skill.SkillType)
            {
                case eBattleSkillType.Skill1:
                    StatisticsMananger.Instance.AddAtkCount(casterData.ID, casterData.IsEnemy, true);
                    yield return SkillCoroutine(casterData, skill);
                    break;
                case eBattleSkillType.Normal:
                default:
                    StatisticsMananger.Instance.AddAtkCount(casterData.ID, casterData.IsEnemy, false);
                    yield return NormalCoroutine(casterData, skill);
                    break;
            }
            yield break;
        }
        protected virtual IEnumerator NormalCoroutine(IBattleCharacterData casterData, SBSkill skill)
        {
            if (casterData == null || skill == null)
            {
                casterData.EndActionCoroutine();
                yield break;
            }

            var caster = casterData.GetSpine();
            if (caster != null && caster.Controller != null && casterData.Transform != null)
                caster.Controller.MoveWorldTargetUpdate(0f, casterData.Transform.position + (casterData.IsEnemy ? Vector3.left : Vector3.right));

            float casterSpeed = 1 / casterData.Stat.GetAttackSpeed();

            casterData.SetActiveSkilType(eBattleSkillType.Normal);
            casterData.SetWeakDelay(skill.Skill.WEAK_TIME);
            casterData.SetCastingDelay(skill.Skill.CASTING_TIME);

            var afterDelay = skill.Skill.AFTER_DELAY;
            if (afterDelay < 0.05f)
                afterDelay = 0.05f;
            casterData.SetAfterDelay(afterDelay);

            if (skill.Skill != null && skill.Skill.CASTING_SOUND != "NONE")
            {
                soundDelays.Add(new SBAttackSfxSound(skill.Skill.CASTING_SOUND, skill.Skill.CASTING_SOUND_DELAY * casterSpeed));
            }

            if (casterData.CastingDelay > 0)
            {
                caster.SetAnimation(eSpineAnimation.A_CASTING);
                while (casterData.CastingDelay > 0)
                {
                    yield return SBDefine.GetWaitForSeconds(SBGameManager.Instance.DTime);
                }
            }

            casterData.SetNormalDelay(casterData.NormalSkill.COOL_TIME);//공속에 관련 있는 노말 스킬만 적용.
            casterData.SetAfterDelay(afterDelay);
            caster.SetAnimation(eSpineAnimation.ATTACK);

            yield return ActiveSkill(casterData, skill);
            casterData.EndActionCoroutine();
            yield break;
        }
        protected virtual IEnumerator SkillCoroutine(IBattleCharacterData casterData, SBSkill skill)
        {
            if (casterData == null || skill == null)
            {
                casterData.EndActionCoroutine();
                yield break;
            }

            var caster = casterData.GetSpine();
            if (caster != null && caster.Controller != null && casterData.Transform != null)
                caster.Controller.MoveWorldTargetUpdate(0f, casterData.Transform.position + (casterData.IsEnemy ? Vector3.left : Vector3.right));

            eBattleSide side = GetSide(casterData);

            var queueSkill = Data.GetSkill(casterData, side);
            if (queueSkill == null || queueSkill.SkillActive(casterData) is false)
            {
                casterData.EndActionCoroutine();
                yield break;
            }

            casterData.SetActiveSkilType(skill.SkillType);
            casterData.SetCastingDelay(skill.Skill.CASTING_TIME);
            casterData.SetWeakDelay(skill.Skill.WEAK_TIME);
            casterData.SetSkill1Delay(skill.Skill.COOL_TIME);
            casterData.SetAfterDelay(skill.Skill.AFTER_DELAY);
            Data.SetGlobalDelay(side, skill.Skill.GLOBAL_COOL_TIME);

            SoundManager.Instance.PlaySFX("sfx_skill_cast1");
            if (skill.Skill != null && skill.Skill.CASTING_SOUND != "NONE")
                soundDelays.Add(new SBAttackSfxSound(skill.Skill.CASTING_SOUND, skill.Skill.CASTING_SOUND_DELAY));

            if (side == eBattleSide.OffenseSide_1 && !casterData.IsEnemy)
            {
                OnUISkillEffect(casterData.ID, eSpineAnimation.SKILL, 1f, caster);
            }

            if (casterData.CastingDelay > 0)
            {
                caster.SetAnimation(eSpineAnimation.CASTING);
                while (casterData.CastingDelay > 0)
                {
                    yield return SBDefine.GetWaitForSeconds(SBGameManager.Instance.DTime);
                }
            }

            casterData.SetAfterDelay(skill.Skill.AFTER_DELAY);
            caster.SetAnimation(eSpineAnimation.SKILL);

            yield return ActiveSkill(casterData, skill);
            casterData.SetActionCoroutine(null);
            yield break;
        }

        protected virtual void OnUISkillEffect(int id, eSpineAnimation skill, float v, BattleSpine caster)
        {
            UISkillCutSceneEvent.Send(id, skill, v, caster);
        }
        #region Target 찾기
        protected IBattleCharacterData FindTarget(IBattleCharacterData caster, SBSkill skill)
        {
            if (caster == null || skill == null)
                return null;

            if (caster.IsMoveSkip())
                return null;

            if (caster.PriorityTarget != null && caster.Transform != null)
            {
                var target = caster.PriorityTarget;                
                if (target == null || IsCastContain(GetCastCircle(caster, skill), caster.Transform.position, target.Transform.position, caster.GetCircleCollider(), target.GetCircleCollider()))
                    return null;
                else
                    return target;
            }

            return CheckCastingTarget(caster, skill);
        }
        protected void DirReverse(ref eSkillEffectDirectionType type)
        {
            if (type == eSkillEffectDirectionType.Back)
                type = eSkillEffectDirectionType.Front;
            else if (type == eSkillEffectDirectionType.Front)
                type = eSkillEffectDirectionType.Back;
        }
        #endregion
        #region Check
        protected virtual List<BattleSpine> CheckSummon(IBattleCharacterData caster, SkillSummonData summon)
        {
            var list = GetSkillSummonList(caster, summon);
            if (list.Count < 1)
                return list;

            //확인 필요
            //CircleCollider2D collider = caster.GetCircleCollider();
            //if (collider != null)
            //{
            //    position.x = collider.bounds.center.x;
            //    position.y = collider.bounds.center.y;
            //    RangeX += collider.radius * Mathf.Abs(collider.transform.localScale.x);
            //    RangeY += collider.radius * Mathf.Abs(collider.transform.localScale.y);
            //}

            SBObject sbObj = GetSummonRange(caster, summon);

            list.RemoveAll((target) =>
            {
                if (sbObj == null)
                    return false;

                CircleCollider2D targetCollider = target.Data.GetCircleCollider();
                if (targetCollider == null)
                {
                    if (sbObj.IsContain(target.transform.position))
                        return false;
                }
                else
                {
                    Vector3 tp = targetCollider.bounds.center;
                    if (sbObj.IsContain(tp))
                        return false;

                    Vector2 near1 = (sbObj.Position - tp).normalized;
                    tp.x += near1.x * targetCollider.radius;
                    tp.y += near1.y * targetCollider.radius;

                    var casterCollider = caster.GetCircleCollider();
                    if (casterCollider != null)
                    {
                        tp.x += near1.x * 0.02f;
                        tp.y += near1.y * 0.02f;
                    }
                    if (sbObj.IsContain(tp))
                        return false;
                }

                return true;
            });

            if (summon.TYPE == eSkillSummonType.RAPID_R)
                return list;

            if (summon.TARGET_COUNT >= 0 && summon.TARGET_COUNT < list.Count)
                list.RemoveRange(summon.TARGET_COUNT, list.Count - summon.TARGET_COUNT);

            return list;
        }
        protected virtual eDirectionBit GetAttackDirection(IBattleCharacterData caster)
        {
            return caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right;
        }
        protected virtual SBObject GetSummonRange(IBattleCharacterData caster, SkillSummonData summon)
        {
            if (caster.Transform == null)
                return null;

            Vector3 position = caster.Transform.position;
            var RangeX = summon.RANGE_X;
            var RangeY = summon.RANGE_Y;
            SBObject sbObj = null;

            switch (summon.RANGE_TYPE)
            {
                case eSkillRangeType.CIRCLE_F:
                    circle.SetPosition(position);
                    circle.SetDirection(GetAttackDirection(caster));
                    circle.SetEllipse(RangeX, RangeY);
                    sbObj = circle;
                    break;
                case eSkillRangeType.SQUARE_C:
                    rect.SetPosition(position);
                    rect.SetDirection(eDirectionBit.None);
                    rect.SetEllipse(RangeX, RangeY);
                    sbObj = rect;
                    break;
                case eSkillRangeType.SQUARE_F:
                    rect.SetPosition(position);
                    rect.SetDirection(GetAttackDirection(caster));
                    rect.SetEllipse(RangeX, RangeY);
                    sbObj = rect;
                    break;
                case eSkillRangeType.SECTOR_F:
                    cone.SetPosition(position);
                    cone.SetDirection(GetAttackDirection(caster));
                    cone.SetCone(RangeX, RangeY);
                    sbObj = cone;
                    break;
                case eSkillRangeType.CIRCLE_C:
                    circle.SetPosition(position);
                    circle.SetDirection(eDirectionBit.None);
                    circle.SetEllipse(RangeX, RangeY);
                    sbObj = circle;
                    break;
                default:
                    sbObj = null;
                    break;
            }

            return sbObj;
        }


        protected virtual Circle GetCastCircle(IBattleCharacterData caster, SBSkill skill, Vector2 addedRange = default)
        {
            if (caster.Transform == null)
                return null;

            if (circle == null)
                circle = new(caster.Transform.position, skill.Skill.RANGE_X + addedRange.x, skill.Skill.RANGE_Y + addedRange.y);
            else
            {
                circle.SetPosition(caster.Transform.position);
                circle.SetDirection(eDirectionBit.None);
                circle.SetEllipse(skill.Skill.RANGE_X + addedRange.x, skill.Skill.RANGE_Y + addedRange.y);
            }
            return circle;
        }
        protected virtual IBattleCharacterData CheckCastingTarget(IBattleCharacterData caster, SBSkill skill)
        {
            if (caster.IsMoveSkip())
                return null;

            var list = GetSkillCharList(caster, skill);
            if (list.Count < 1)
            {
                return null;
            }

            var target = list[0];

            if (caster.Transform != null && target.Data.Transform != null)
            {
                if (IsCastContain(GetCastCircle(caster, skill), caster.Transform.position, target.Data.Transform.position, caster.GetCircleCollider(), target.Data.GetCircleCollider()))
                    return null;
            }
            
            return target.Data;
        }
        protected virtual bool IsCastContain(SBObject obj, Vector3 position, Vector3 tp, CircleCollider2D castCol, CircleCollider2D targetCol)
        {
            if (obj == null)
                return false;

            if (targetCol == null)
            {
                return obj.IsContain(tp);
            }
            else
            {
                tp = targetCol.bounds.center;
                if (obj.IsContain(tp))
                    return true;

                Vector2 near = (position - tp).normalized;
                tp.x += near.x * targetCol.radius;
                tp.y += near.y * targetCol.radius;
                if (castCol != null)
                {
                    tp.x += near.x * 0.02f;
                    tp.y += near.y * 0.02f;
                }

                if (obj.IsContain(tp))
                    return true;
            }
            return false;
        }
        protected virtual bool DeathCheck()
        {
            if (offenseDeath >= offenses.Count || defenseDeath >= defenses.Count)
            {
                targets = GetCameraTargets();
                return false;
            }
            return true;
        }
        #endregion
        #region GetDistanceList
        public int SortPriority(BattleSpine caster, BattleSpine d1, BattleSpine d2)
        {
            if (caster.Data.PriorityTarget == d1.Data)
                return -1;
            else if (caster.Data.PriorityTarget == d2.Data)
                return 1;
            else
                return 0;
        }
        public int SortNear(Vector3 position, BattleSpine d1, BattleSpine d2)
        {
            var a = Vector2.Distance(position, d1.Data.Transform.position);
            var b = Vector2.Distance(position, d2.Data.Transform.position);

            if (a > b)
                return 1;
            else if (a < b)
                return -1;
            else
                return 0;
        }
        public int SortFar(Vector3 position, BattleSpine d1, BattleSpine d2)
        {
            var a = Mathf.Abs(position.x - d1.Data.Transform.position.x);
            var b = Mathf.Abs(position.x - d2.Data.Transform.position.x);

            if (a > b)
                return -1;
            else if (a < b)
                return 1;
            else
            {
                a = Mathf.Abs(position.y - d1.Data.Transform.position.y);
                b = Mathf.Abs(position.y - d2.Data.Transform.position.y);

                if (a > b)
                    return 1;
                else if (a < b)
                    return -1;

                return 0;
            }
        }
        public int SortFront(BattleSpine d1, BattleSpine d2)
        {
            var a = d1.Data.Transform.position.x;
            var b = d2.Data.Transform.position.x;

            if (a > b)
                return 1;
            else if (a < b)
                return -1;
            else
                return 0;
        }
        public int SortBack(BattleSpine d1, BattleSpine d2)
        {
            var a = d1.Data.Transform.position.x;
            var b = d2.Data.Transform.position.x;

            if (a > b)
                return -1;
            else if (a < b)
                return 1;
            else
                return 0;
        }
        public int SortHpHigh(BattleSpine d1, BattleSpine d2)
        {
            var a = d1.Data.HP + d1.Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
            var b = d2.Data.HP + d2.Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);

            if (a < b)
                return 1;
            else if (a > b)
                return -1;
            else
                return 0;
        }
        public int SortHpLow(BattleSpine d1, BattleSpine d2)
        {
            var a = d1.Data.HP + d1.Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
            var b = d2.Data.HP + d2.Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);

            if (a > b)
                return 1;
            else if (a < b)
                return -1;
            else
                return 0;
        }
        public int SortStatHigh(eStatusType stat, BattleSpine d1, BattleSpine d2)
        {
            var a = d1.Data.Stat.GetTotalStatusInt(stat);
            var b = d2.Data.Stat.GetTotalStatusInt(stat);
            if (stat == eStatusType.HP)
            {
                a += d1.Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
                b += d2.Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
            }

            if (a < b)
                return 1;
            else if (a > b)
                return -1;
            else
                return 0;
        }
        public int SortStatLow(eStatusType stat, BattleSpine d1, BattleSpine d2)
        {
            var a = d1.Data.Stat.GetTotalStatusInt(stat);
            var b = d2.Data.Stat.GetTotalStatusInt(stat);
            if (stat == eStatusType.HP)
            {
                a += d1.Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
                b += d2.Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
            }

            if (a > b)
                return 1;
            else if (a < b)
                return -1;
            else
                return 0;
        }
        protected virtual List<BattleSpine> GetTargetList(IBattleCharacterData caster, eSkillTargetType type)
        {
            var list = new List<BattleSpine>();
            switch (type)
            {
                case eSkillTargetType.ALL:
                    list.AddRange(defenses);
                    list.AddRange(offenses);
                    break;
                case eSkillTargetType.ALLY:
                    list.AddRange(GetTeam(caster.IsEnemy ? eBattleSide.DefenseSide_1 : eBattleSide.OffenseSide_1));
                    list.Remove(caster.GetSpine());
                    break;
                case eSkillTargetType.FRIENDLY:
                    list.AddRange(GetTeam(caster.IsEnemy ? eBattleSide.DefenseSide_1 : eBattleSide.OffenseSide_1));
                    break;
                case eSkillTargetType.SELF:
                    list.Add(caster.GetSpine());
                    break;
                case eSkillTargetType.ENEMY:
                default:
                    list.AddRange(GetTeam(caster.IsEnemy ? eBattleSide.OffenseSide_1 : eBattleSide.DefenseSide_1));
                    break;
            }
            list.RemoveAll((spine) =>
            {
                if (spine == null || spine.Data.Death || spine.Data.IsTargetSkip())
                    return true;

                return false;
            });
            return list;
        }
        protected void SortTargetList(List<BattleSpine> list, IBattleCharacterData caster, Vector3 targetPosition, eSkillTargetSort type)
        {
            if (list == null)
                return;

            switch (type)
            {
                case eSkillTargetSort.FAR:
                    list.Sort((d1, d2) => SortFar(targetPosition, d1, d2));
                    break;
                case eSkillTargetSort.FHP_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.HP, d1, d2));
                    break;
                case eSkillTargetSort.FHP_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.HP, d1, d2));
                    break;
                case eSkillTargetSort.HP_HIGH:
                    list.Sort(SortHpHigh);
                    break;
                case eSkillTargetSort.HP_LOW:
                    list.Sort(SortHpLow);
                    break;
                case eSkillTargetSort.ATK_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.ATK, d1, d2));
                    break;
                case eSkillTargetSort.ATK_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.ATK, d1, d2));
                    break;
                case eSkillTargetSort.DEF_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.DEF, d1, d2));
                    break;
                case eSkillTargetSort.DEF_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.DEF, d1, d2));
                    break;
                case eSkillTargetSort.LIGHT_DMG_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.LIGHT_DMG, d1, d2));
                    break;
                case eSkillTargetSort.LIGHT_DMG_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.LIGHT_DMG, d1, d2));
                    break;
                case eSkillTargetSort.DARK_DMG_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.DARK_DMG, d1, d2));
                    break;
                case eSkillTargetSort.DARK_DMG_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.DARK_DMG, d1, d2));
                    break;
                case eSkillTargetSort.WATER_DMG_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.WATER_DMG, d1, d2));
                    break;
                case eSkillTargetSort.WATER_DMG_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.WATER_DMG, d1, d2));
                    break;
                case eSkillTargetSort.FIRE_DMG_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.FIRE_DMG, d1, d2));
                    break;
                case eSkillTargetSort.FIRE_DMG_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.FIRE_DMG, d1, d2));
                    break;
                case eSkillTargetSort.WIND_DMG_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.WIND_DMG, d1, d2));
                    break;
                case eSkillTargetSort.WIND_DMG_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.WIND_DMG, d1, d2));
                    break;
                case eSkillTargetSort.EARTH_DMG_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.EARTH_DMG, d1, d2));
                    break;
                case eSkillTargetSort.EARTH_DMG_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.EARTH_DMG, d1, d2));
                    break;
                case eSkillTargetSort.CRI_PROC_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.CRI_PROC, d1, d2));
                    break;
                case eSkillTargetSort.CRI_PROC_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.CRI_PROC, d1, d2));
                    break;
                case eSkillTargetSort.CRI_DMG_HIGH:
                    list.Sort((d1, d2) => SortStatHigh(eStatusType.CRI_DMG, d1, d2));
                    break;
                case eSkillTargetSort.CRI_DMG_LOW:
                    list.Sort((d1, d2) => SortStatLow(eStatusType.CRI_DMG, d1, d2));
                    break;
                case eSkillTargetSort.NEARBY:
                default:
                    list.Sort((d1, d2) => SortNear(targetPosition, d1, d2));
                    break;
            }
        }
        protected List<BattleSpine> GetSkillCharList(IBattleCharacterData caster, SBSkill skill)
        {
            if (skill == null)
                return null;

            var list = GetTargetList(caster, skill.Skill.TARGET_TYPE);
            SortTargetList(list, caster, caster.Transform.position, skill.Skill.TARGET_SORT);
            //도발 걸리면 우선순위 업 폭발에는 적용하면 안됨
            list.Sort((d1, d2) => SortPriority(caster.GetSpine(), d1, d2));

            return list;
        }
        protected virtual List<BattleSpine> GetSkillSummonList(IBattleCharacterData caster, SkillSummonData summon)
        {
            if (summon == null)
                return null;

            var list = GetTargetList(caster, summon.TARGET_TYPE);
            SortTargetList(list, caster, caster.Transform.position, summon.TARGET_SORT);
            //도발 걸리면 우선순위 업 폭발에는 적용하면 안됨
            list.Sort((d1, d2) => SortPriority(caster.GetSpine(), d1, d2));

            return list;
        }
        protected List<BattleSpine> GetSkillEffectList(IBattleCharacterData caster, Vector3 targetWorldPos, SkillEffectData skill)
        {
            if (skill == null)
                return null;

            var list = GetTargetList(caster, skill.EX_TARGET_TYPE);
            SortTargetList(list, caster, targetWorldPos, skill.EX_TARGET_SORT);

            return list;
        }
        #endregion
        #region CheckSkill
        private bool IsCaster(IBattleCharacterData caster)
        {
            return caster.ActiveSkillType is eBattleSkillType.Skill1;
        }
        protected SBSkill CheckSkill(IBattleCharacterData caster)
        {
            eBattleSide side = GetSide(caster);
            bool silence = caster.IsCastingSkip();
            bool skill1Ing = caster.Skill1 != null && caster.Skill1Delay <= 0f;

            var skill = caster.NormalSkill;
            var summon = caster.NormalSummon;
            var skillType = eBattleSkillType.Normal;

            if (caster.NormalSkill == null)
            {
                if (false == skill1Ing ||
                    silence ||
                    null == Data.GetSkill(caster, side))
                    return null;

                if (caster.Skill1 != null)
                {
                    var sbSkill = new SBSkill();
                    skillType = eBattleSkillType.Skill1;
                    skill = caster.Skill1;
                    summon = caster.Skill1Summon;
                    sbSkill.SetCastData(caster, skill, summon, skillType);
                    return sbSkill;
                }
                else
                    return null;
            }

            if (silence)
            {
                if (IsCaster(caster))
                {
                    var spine = caster.GetSpine();
                    if (spine != null)
                        spine.SkillActionCancle();
                }

                var sbSkill = new SBSkill();
                sbSkill.SetCastData(caster, skill, summon, skillType);
                return sbSkill;
            }
            else if (Data.GetGlobalDelay(side) <= 0 && skill1Ing)
            {
                var queueSkill = Data.GetSkill(caster, side);
                if (queueSkill != null)
                {
                    skillType = eBattleSkillType.Skill1;
                    skill = caster.Skill1;
                    summon = caster.Skill1Summon;
                }
            }

            var resultSkill = new SBSkill();
            resultSkill.SetCastData(caster, skill, summon, skillType, caster.Transform);
            return resultSkill;
        }
        #endregion
        protected virtual void UpdateSounds(float dt)
        {
            if (soundDelays == null || soundDelays.Count < 1)
                return;

            for (int i = 0, count = soundDelays.Count; i < count; ++i)
            {
                if (soundDelays[i] == null)
                    continue;

                soundDelays[i].Update(dt);
            }
            var targets = soundDelays.RemoveAll((target) =>
            {
                return target == null || target.IsPlaying;
            });
        }

        protected void SimulatorLogDataInit()
        {
#if DEBUG
            SimulatorLoger.ClearLog();
#endif
        }

        /*
         * dragonTag : 드래곤 태그
         * damage : 드래곤이 던진 데미지
         * isAttack : 드래곤이 던진 데미지 인지 받은 데미지 인지
         * taken_damage : 받은 데미지 (데미지 감소 계산 전 순수 데미지)
         * taken_real_damage : 실제 받은 데미지 (데미지 감소 계산 공식 이후)
         */
        protected void SaveDamageLog(int dragonTag, float damage, bool isAttack = true, float taken_damage = 0f, float taken_real_damage = 0f)
        {
#if DEBUG
            if (User.Instance.UserAccountData.UserNumber > 0)
                return;

            SimulatorLoger.UpdateLog(dragonTag, SimulatorLoger.LogType.DAMAGE, damage);
            SimulatorLoger.UpdateLog(dragonTag, SimulatorLoger.LogType.TAKEN_DAMAGE, taken_damage);
            SimulatorLoger.UpdateLog(dragonTag, SimulatorLoger.LogType.TAKEN_ORIGIN_DAMAGE, taken_real_damage);
#endif
        }
        public override void Clear()
        {
            base.Clear();
            if (projectiles == null)
                projectiles = new List<IBattleEventObject>();
            else
                projectiles.Clear();
        }

        protected virtual List<CameraTarget> GetCameraTargets()
        {
            ProCamera2D.Instance.CameraTargets = ProCamera2D.Instance.CameraTargets.FindAll(target => target != null && target.TargetTransform != null);
            return ProCamera2D.Instance.CameraTargets;
        }
        public virtual float RandomVal()
        {
            return SBFunc.RandomValue;
        }
        public virtual int Random(int min = 0, int max = 100, RandomLog.RandomReason reason = RandomLog.RandomReason.NONE)
        {
            var ret = SBFunc.Random(min, max);
            return ret;
        }
        public virtual float Random(float min = 0, float max = 100, RandomLog.RandomReason reason = RandomLog.RandomReason.NONE)
        {
            float ret = SBFunc.Random(min, max);
            return ret;
        }
        #region 기존 로직에 영향 덜 가기위해 상속 받은 쪽에서 사용 중.
        //아레나 움직임 제한 제거 2024-01-03
        /// <summary> 기존 로직에 영향 덜 가기위해 상속 받은 쪽에서 Offset 계산할때 사용중. </summary>
        /// <param name="mine">대상</param>
        /// <param name="spines">대상의 반대 팀</param>
        /// <returns>대상 반대 팀의 가장 앞에있는 Char</returns>
        protected BattleSpine GetFrontPos(IBattleCharacterData mine, List<BattleSpine> spines)
        {
            var list = spines.ToArray();
            if (mine.IsEnemy)
                list = list.OrderByDescending(n => n.transform.position.x).ToArray();
            else
                list = list.OrderBy(n => n.transform.position.x).ToArray();

            for (int i = 0, count = list.Length; i < count; ++i)
            {
                if (list[i] == null || list[i].Data.Death)
                    continue;

                return list[i];
            }

            return null;
        }
        #endregion
    }
}