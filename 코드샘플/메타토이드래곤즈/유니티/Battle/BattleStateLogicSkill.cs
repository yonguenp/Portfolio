using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    //스킬 쪽만 분리
    public partial class BattleStateLogic
    {
        protected void SkillCast(IBattleCharacterData caster)
        {
            Data?.SkillCast(caster, GetSide(caster));
        }
        protected bool IsNextEffect(SkillEffectData data)
        {
            if (data == null)
                return false;

            switch (data.TRIGGER_TYPE)
            {
                case eSkillTriggerType.NEXT:
                    return data.NEXT_EFFECT > 0;
                case eSkillTriggerType.DEAD:
                case eSkillTriggerType.END:
                case eSkillTriggerType.NONE:
                default:
                    return false;
            }
        }
        public IEnumerator ActiveSkill(IBattleCharacterData casterData, SBSkill skill, int idx = 0)
        {
            if (skill == null)
                yield break;

            do
            {
                var summon = skill.GetSummon(idx);
                if (summon == null)
                    break;

                if (summon.DELAY > 0f)
                    yield return SBDefine.GetWaitForSeconds(summon.DELAY);

                switch (summon.TYPE)
                {
                    case eSkillSummonType.ARROW:
                        ArrowTrigger(casterData, skill, idx);
                        break;
                    case eSkillSummonType.PIERCE:
                        PierceTrigger(casterData, skill, idx);
                        break;
                    case eSkillSummonType.LAND:
                        LandTrigger(casterData, skill, idx);
                        break;
                    case eSkillSummonType.RAPID_R:
                        yield return RapidTrigger(casterData, skill, idx);
                        break;
                    case eSkillSummonType.BACKSTAB:
                        BackStabTrigger(casterData, skill, idx);
                        break;
                    case eSkillSummonType.SUMMON:
                        SummonTrigger(casterData, skill, idx);
                        break;
                    case eSkillSummonType.SPECIAL:
                        SpecialTrigger(casterData, skill, idx);
                        break;
                    case eSkillSummonType.CHARGE:
                        yield return ChargeTrigger(casterData, skill, idx);
                        break;
                    case eSkillSummonType.IMMEDIATELY:
                    case eSkillSummonType.NONE:
                    default:
                        SkillTrigger(casterData, skill, idx);
                        break;
                }
                idx++;
            } while (skill.NextSummon());

            yield break;
        }
        #region Skill
        protected virtual void NextSkill(int idx, SBSkill skill, IBattleCharacterData casterData, List<BattleSpine> targets)
        {
            var curSummon = skill.GetSummon(idx);
            if (curSummon == null || targets == null)
                return;

            for (int i = 0, count = targets.Count; i < count; ++i)
            {
                NextSkill(skill, casterData, curSummon, targets[i].transform);
            }
        }
        protected virtual void NextSkill(int idx, SBSkill skill, IBattleCharacterData casterData, Transform target, Vector3 center = default, bool isPos = false)
        {
            NextSkill(skill, casterData, skill.GetSummon(idx), target, center, isPos);
        }
        protected virtual void NextSkill(SBSkill skill, IBattleCharacterData casterData, SkillSummonData curSummon, Transform target, Vector3 center = default, bool isPos = false)
        {
            if (curSummon == null)
                return;

            var curSkill = new SBSkill();
            curSkill.SetCastData(casterData, skill.Skill, curSummon, skill.SkillType, target);
            if (isPos)
                curSkill.SetTargetPos(center);
            Stage.StartCoroutine(ActiveSkill(casterData, curSkill));
        }
        protected virtual void ArrowTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            var targets = CheckSummon(casterData, summon);
            if (targets == null)
                return;

            switch (summon.TARGET)
            {
                case eSkillTarget.CENTER:
                {
                    var center = Vector3.zero;
                    var posX = 0f;
                    var posY = 0f;
                    int count = targets.Count;
                    if (count < 1)
                        return;

                    for (int i = 0; i < count; ++i)
                    {
                        var target = targets[i];
                        if (target == null)
                            continue;

                        posX += target.transform.position.x + 10000f;
                        posY += target.transform.position.y + 10000f;
                    }
                    center.x = posX / count - 10000f;
                    center.y = posY / count - 10000f;

                    var arrowResourceData = summon.GetArrowResource();
                    var curArrow = CreateFieldEffect(arrowResourceData, casterData, casterData, Vector3.zero, casterData.Transform.localScale);
                    if (curArrow != null)
                    {
                        var sbProjectile = curArrow.GetComponent<SBProjectileCenter>();
                        if (sbProjectile == null)
                        {
                            if (arrowResourceData != null && arrowResourceData.IMAGE != "NONE")
                                sbProjectile = curArrow.AddComponent<SBProjectileCenterGeneric>();
                            else
                                sbProjectile = curArrow.AddComponent<SBProjectileCenter>();
                        }

                        sbProjectile.SetAutoDirection(arrowResourceData);
                        sbProjectile.Set(casterData, center, skill, () =>
                        {
                            if (summon.SKILL_EFFECT_RSC_KEY > 0)
                            {
                                var skillEffect = CreateFieldEffect(summon.GetEffectResource(), casterData, skill.TargetPosition, Vector3.zero, skill.TargetScale);
                                if (skillEffect != null)
                                    skillEffect.transform.position = center;
                            }
                            var effects = summon.GetEffects();
                            if (effects == null)
                            {
                                if (skill.HitSummon())
                                {
                                    NextSkill(idx + 1, skill, casterData, null, center, true);
                                }
                                return;
                            }

                            for (int i = 0, count = effects.Count; i < count; ++i)
                            {
                                var effect = effects[i];
                                if (effect == null)
                                    continue;

                                EffectTriggerExplosion(casterData, center, effect, skill);
                            }

                            if (skill.HitSummon())
                            {
                                NextSkill(idx + 1, skill, casterData, null, center, true);
                            }
                        }, idx);
                    }
                }
                break;
                case eSkillTarget.NONE:
                default:
                {
                    for (int i = 0, count = targets.Count; i < count; ++i)
                    {
                        var targetSpine = targets[i];
                        var arrowResourceData = summon.GetArrowResource();
                        var curArrow = CreateFieldEffect(arrowResourceData, casterData, skill.TargetPosition, GetAddedPosition(casterData, summon), casterData.Transform.localScale);
                        if (curArrow != null)
                        {
                            var sbProjectile = curArrow.GetComponent<SBProjectileTarget>();
                            if (sbProjectile == null)
                            {
                                if (arrowResourceData != null && arrowResourceData.IMAGE != "NONE")
                                    sbProjectile = curArrow.AddComponent<SBProjectileTargetGeneric>();
                                else
                                    sbProjectile = curArrow.AddComponent<SBProjectileTarget>();
                            }

                            VoidDelegate func = () =>
                            {
                                var effectPos = targetSpine.transform.position;
                                if (summon.SKILL_EFFECT_RSC_KEY > 0)
                                    CreateFieldEffect(summon.GetEffectResource(), casterData, targetSpine.Data, Vector2.zero, casterData.Transform.localScale);

                                var effects = summon.GetEffects();
                                if (effects == null)
                                {
                                    if (skill.HitSummon())
                                    {
                                        NextSkill(idx + 1, skill, casterData, targetSpine.transform);
                                    }
                                    return;
                                }

                                for (int i = 0, count = effects.Count; i < count; ++i)
                                {
                                    if (effects[i] == null)
                                        continue;
                                    EffectTrigger(casterData, targetSpine.Data, effects[i], skill);
                                }

                                if (skill.HitSummon())
                                {
                                    NextSkill(idx + 1, skill, casterData, targetSpine.transform);
                                }
                            };
                            sbProjectile.SetAutoDirection(arrowResourceData);
                            sbProjectile.Set(casterData, targetSpine.Data, skill, func, idx);
                        }
                    }
                }
                break;
            }
        }
        protected void LandTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            var arrowResourceData = summon.GetArrowResource();
            var curArrow = CreateFieldEffect(arrowResourceData, casterData, skill.TargetPosition, GetAddedPosition(casterData, summon), casterData.Transform.localScale);
            if (curArrow != null)
            {
                var sbLand = curArrow.GetComponent<SBLand>();
                if (sbLand == null)
                    sbLand = curArrow.AddComponent<SBLand>();

                sbLand.SetLandData(Data, casterData, skill, EffectTriggerLand, idx);
            }
            if (summon.SKILL_EFFECT_RSC_KEY > 0)
                CreateFieldEffect(summon.GetEffectResource(), casterData, skill.TargetPosition, GetAddedPosition(casterData, summon), skill.TargetScale);
        }
        protected void PierceTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            var arrowResourceData = summon.GetArrowResource();
            var curArrow = CreateFieldEffect(arrowResourceData, casterData, casterData, GetAddedPosition(casterData, summon), casterData.Transform.localScale);
            if (curArrow != null)
            {
                var projectile = curArrow.GetComponent<SBProjectileTime>();
                if (projectile == null)
                    projectile = curArrow.AddComponent<SBProjectileTime>();

                projectile.SetAutoDirection(arrowResourceData);
                projectile.Set(casterData, null, skill, null, idx);
                projectile.SetDicrection(GetAttackDirection);
                projectile.SetTriggerData(Data, EffectTrigger);
            }
            if (summon.SKILL_EFFECT_RSC_KEY > 0)
                CreateFieldEffect(summon.GetEffectResource(), casterData, skill.TargetPosition, GetAddedPosition(casterData, summon), skill.TargetScale);
        }
        protected IEnumerator RapidTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            var summon = skill.GetSummon(idx);
            if (summon == null)
                yield break;

            var arrowResourceData = summon.GetArrowResource();
            int index = 0;
            while (index < summon.TARGET_COUNT)
            {
                var targets = CheckSummon(casterData, summon);
                if (targets == null || targets.Count < 1)
                {
                    break;
                }

                var rnd = Random(0, targets.Count, RandomLog.RandomReason.RapidTrigger);

                var targetSpine = targets[rnd];
                var curArrow = CreateFieldEffect(arrowResourceData, casterData, casterData, GetAddedPosition(casterData, summon), casterData.Transform.localScale);
                if (curArrow != null)
                {
                    var sbProjectile = curArrow.GetComponent<SBProjectileTarget>();
                    if (sbProjectile == null)
                    {
                        if (arrowResourceData != null && arrowResourceData.IMAGE != "NONE")
                            sbProjectile = curArrow.AddComponent<SBProjectileTargetGeneric>();
                        else
                            sbProjectile = curArrow.AddComponent<SBProjectileTarget>();
                    }

                    sbProjectile.SetAutoDirection(arrowResourceData);
                    sbProjectile.Set(casterData, targetSpine.Data, skill, () =>
                    {
                        var effectPos = targetSpine.transform.position;
                        if (summon.SKILL_EFFECT_RSC_KEY > 0)
                            CreateFieldEffect(summon.GetEffectResource(), casterData, targetSpine.Data, GetAddedPosition(casterData, summon), (Vector3)casterData.Transform.localScale);

                        var effects = summon.GetEffects();
                        if (effects == null)
                            return;

                        for (int i = 0, count = effects.Count; i < count; ++i)
                        {
                            if (effects[i] == null)
                                continue;
                            EffectTrigger(casterData, targetSpine.Data, effects[i], skill);
                        }

                        if (skill.HitSummon())
                        {
                            NextSkill(idx + 1, skill, casterData, targetSpine.transform);
                        }
                    }, idx);
                }
                
                yield return SBDefine.GetWaitForSeconds(summon.VALUE1);
                
                index++;
            }
            yield break;
        }
        protected IEnumerator ChargeTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            var summon = skill.GetSummon(idx);
            if (summon == null)
                yield break;

            var effects = skill.GetEffect(idx);
            if (effects == null)
                yield break;

            var startPos = casterData.Transform.position.x;
            var dir = casterData.KnockBackDirection.Reverse();
            var maxtime = summon.VALUE1;
            var time = 0f;
            var distance = summon.VALUE2;
            List<BattleSpine> container = new();
            float endPos;
            if (dir == eDirectionBit.Right)
                endPos = startPos + distance * SBDefine.BattleTileX;
            else
                endPos = startPos - distance * SBDefine.BattleTileX;

            if (summon.SKILL_EFFECT_RSC_KEY > 0)
                CreateFieldEffect(summon.GetEffectResource(), casterData, casterData, GetAddedPosition(casterData, summon), casterData.Transform.localScale);
            
            bool isCancle = false;
            while (time < maxtime)
            {
                var targets = CheckSummon(casterData, summon);
                if (targets != null)
                {
                    for (int i = 0, count = targets.Count; i < count; ++i)
                    {
                        var target = targets[i];
                        if (target == null || target.Data.Death)
                            continue;

                        if (container.Contains(target))
                            continue;

                        for (int j = 0, jCount = effects.Count; j < jCount; ++j)
                        {
                            if (effects[j] == null)
                                continue;

                            EffectTrigger(casterData, target.Data, effects[j], skill);
                        }

                        container.Add(target);
                        if (target.Data.HasImmunity(eCharacterImmunity.KNOCK_BACK) || target.Data.IsEffectInfo(eSkillEffectType.IMN_KNOCK, eSkillEffectType.IMN_CC))
                            isCancle = true;

                        if (skill.HitSummon())
                        {
                            NextSkill(idx + 1, skill, casterData, target.transform);
                        }
                    }
                }

                if (isCancle)
                {
                    var caster = casterData.GetSpine();
                    if (caster != null)
                        caster.SkillCancle();
                    yield break;
                }

                var waitTime = SBGameManager.Instance.DTime;
                yield return SBDefine.GetWaitForSeconds(waitTime);
                time += waitTime;
                var curX = SBFunc.BezierCurve(startPos, endPos, time, maxtime);
                casterData.Transform.position = new Vector3(curX, casterData.Transform.position.y, casterData.Transform.position.z);
            }

            yield break;
        }
        protected void BackStabTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            var caster = casterData.GetSpine();
            var summon = skill.GetSummon(idx);
            var effects = skill.GetEffect(idx);
            var animScale = caster.GetAnimScale();
            var summonTarget = CheckSummon(casterData, summon);
            var teleportObject = new SBTeleportObject();
            teleportObject.Set(caster, () =>
            {
                casterData.SetUntouchable(true);
                EffectReceiverClearEvent.Send();
                //if (caster.CurrentTrack != null)
                //{
                //    caster.CurrentTrack.TrackTime = skill.Skill.CASTING_TIME;
                //    caster.SkeletonAni.Update(0f);
                //}
                
                if (summonTarget != null && summonTarget.Count > 0)
                {
                    var target = summonTarget[0];

                    var circlePos = GetAddedPosition(casterData, summon);
                    caster.SpineTransform.position = ColliderPos(target.Data.Transform.position, circlePos, caster, target);

                    var scale = caster.SpineTransform.localScale;
                    scale.x = -scale.x;
                    caster.SpineTransform.localScale = scale;
                    if (summon != null && summon.ARROW_RSC_KEY > 0)
                    {
                        var obj = CreateFieldEffect(SkillResourceData.Get(summon.ARROW_RSC_KEY), caster.Data, target.Data, GetAddedPosition(casterData, summon), caster.transform.localScale);
                        if (obj != null)
                            obj.transform.position = caster.SpineTransform.position;
                    }
                }
                SkillTrigger(caster.Data, skill, idx);

                if (skill.HitSummon())
                {
                    //꽂은데 생성되게끔 수정하였었음.                    
                    //사이드 이펙트 우려 때문에 코드 원복함. 데이터 수정으로 극복해 봄
                    Transform targetTransform = casterData.Transform;
                    //if (summonTarget != null && summonTarget.Count > 0)
                    //{
                    //    var next = skill.GetSummon(idx + 1);
                    //    if (next.TYPE == eSkillSummonType.LAND
                    //    && next.TARGET_TYPE == eSkillTargetType.ENEMY
                    //    && next.TARGET == eSkillTarget.NONE)
                    //    {
                    //        targetTransform = summonTarget[0].transform;
                    //    }
                    //}

                    NextSkill(idx + 1, skill, casterData, targetTransform);
                }
                caster.SetShadow(false);

                var pet = caster.GetPet();
                if (pet != null)
                {
                    pet.FollowImmediate();
                }
            }, () =>
            {
                if (summonTarget != null && summonTarget.Count > 0)
                {
                    var target = summonTarget[0];
                    if (summon != null && summon.SKILL_EFFECT_RSC_KEY > 0)
                    {
                        var obj = CreateFieldEffect(SkillResourceData.Get(summon.SKILL_EFFECT_RSC_KEY), caster.Data, target.Data, GetAddedPosition(casterData, summon), caster.transform.localScale);
                        if (obj != null)
                            obj.transform.position = caster.SpineTransform.position;
                    }
                }

                var scale = caster.SpineTransform.localScale;
                scale.x = Mathf.Abs(scale.x);
                caster.SpineTransform.localScale = scale;
                caster.SpineTransform.localPosition = Vector3.zero;
                var pet = caster.GetPet();
                if (pet != null)
                    pet.FollowImmediate();
                caster.Data.SetUntouchable(false);

                caster.SetShadow(!caster.Data.Death);
            }, summon.VALUE1 / animScale);
            projectiles.Add(teleportObject);
        }

        protected void SummonTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            var targets = CheckSummon(casterData, summon);
            if (targets == null)
                return;

            var rnd = Random(0, targets.Count, RandomLog.RandomReason.SummonTrigger);

            var targetSpine = targets[rnd];

            if (summon.SKILL_EFFECT_RSC_KEY > 0)
                CreateFieldEffect(summon.GetEffectResource(), casterData, targetSpine.Data, GetAddedPosition(casterData, summon), casterData.Transform.localScale);

            List<MonsterSpawnData> spawnDatas = MonsterSpawnData.GetBySpawnGroup((int)summon.EFFECT_GROUP_KEY);
            if (spawnDatas == null)
                return;

            foreach (var spawnData in spawnDatas)
            {
                if (spawnData == null)
                    continue;

                var baseData = MonsterBaseData.Get(spawnData.MONSTER.ToString());
                if (baseData == null)
                    continue;

                if (casterData.IsEnemy)
                {
                    var spine = Stage.MakeSummonMonsterData(casterData, spawnData.MONSTER);
                    if (spine != null)
                    {
                        if (spine.Data.MOVE_SPEED == 0)
                        {
                            var rigid = spine.GetComponent<Rigidbody2D>();
                            if (rigid != null)
                                rigid.simulated = false;
                        }
                        spine.transform.position = casterData.Transform.position + GetAddedPosition(casterData, summon);
                    }
                }
                else
                {
                    //todo 구현할까
                }
            }
        }

        protected virtual void SpecialTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {

        }
        #endregion

        #region SkillTrigger
        protected void SkillTrigger(IBattleCharacterData caster, SBSkill skill, int idx)
        {
            if (caster == null || skill == null)
                return;

            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            switch (summon.TARGET)
            {
                case eSkillTarget.TARGET:
                    SkillTriggerTarget(caster, skill, idx);
                    break;
                case eSkillTarget.CENTER:
                    SkillTriggerCenter(caster, skill, idx);
                    break;
                case eSkillTarget.NONE://여기로오면 데이터가 잘못됨
                default:
#if DEBUG
                    Debug.LogError("SkillTrigger : TargetNONE Error");
#endif
                    break;
            }
        }
        protected void SkillTriggerTarget(IBattleCharacterData caster, SBSkill skill, int idx)
        {
            if (skill == null)
                return;

            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            var effects = skill.GetEffect(idx);
            if (effects == null)
                return;

            if (summon.SKILL_EFFECT_RSC_KEY > 0)
                CreateFieldEffect(summon.GetEffectResource(), caster, skill.TargetPosition, GetAddedPosition(caster, summon), skill.TargetScale);

            var summonList = CheckSummon(caster, summon);
            for (int j = 0, jCount = effects.Count; j < jCount; ++j)
            {
                var effect = effects[j];
                if (effect == null)
                    continue;

                for (int i = 0, count = summonList.Count; i < count; ++i)
                {
                    EffectTrigger(caster, summonList[i].Data, effect, skill);
                }
            }
            caster.SetActiveSkilType(eBattleSkillType.None);

            if (skill.HitSummon())
            {
                NextSkill(idx + 1, skill, caster, summonList);
            }
        }
        protected void SkillTriggerCenter(IBattleCharacterData caster, SBSkill skill, int idx)
        {
            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            var summonList = CheckSummon(caster, summon);
            if (summonList == null)
                return;

            var center = Vector3.zero;
            var posX = 0f;
            var posY = 0f;
            int count = summonList.Count;
            if (count < 1)
                return;

            for (int i = 0; i < count; ++i)
            {
                var target = summonList[i];
                if (target == null)
                    continue;

                posX += target.transform.position.x + 10000f;
                posY += target.transform.position.y + 10000f;
            }
            center.x = posX / count - 10000f;
            center.y = posY / count - 10000f;
            if (summon.SKILL_EFFECT_RSC_KEY > 0)
            {
                var skillEffect = CreateFieldEffect(summon.GetEffectResource(), caster, skill.TargetPosition, GetAddedPosition(caster, summon), skill.TargetScale);
                if (skillEffect != null)
                    skillEffect.transform.position = center;
            }
            var effects = skill.GetEffect(idx);
            if (effects != null)
            {
                for (int j = 0, jCount = effects.Count; j < jCount; ++j)
                {
                    var effect = effects[j];
                    if (effect == null)
                        continue;

                    EffectTriggerExplosion(caster, center, effect, skill);
                }
            }

            caster.SetActiveSkilType(eBattleSkillType.None);

            if (skill.HitSummon())
            {
                NextSkill(idx + 1, skill, caster, null, center, true);
            }
        }
        /// <summary> SBFunc.IsTarget과 분리된 이유 -> Default를 All로 잡았기 때문 </summary>
        protected bool IsNotEffectTarget(SkillEffectData effect, IBattleCharacterData caster, IBattleCharacterData target)
        {
            if (target == null || target.IsTargetSkip())
                return true;

            switch (effect.EX_TARGET_TYPE)
            {
                case eSkillTargetType.ENEMY:
                    return caster.IsEnemy == target.IsEnemy;
                case eSkillTargetType.ALLY:
                    return caster.IsEnemy != target.IsEnemy || caster == target;
                case eSkillTargetType.FRIENDLY:
                    return caster.IsEnemy != target.IsEnemy;
                case eSkillTargetType.SELF:
                    return caster != target;
                default:
                    return false;
            }
        }
        protected void EffectTrigger(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, SBSkill skill)
        {
            //폭팔이 없음
            if (effect.EX_RANGE_TYPE == eSkillRangeType.NONE || effect.EXPLOSION_X <= 0f || effect.EXPLOSION_Y <= 0f)
            {
                EffectTriggerTarget(caster, target, effect, skill);
            }
            else//폭팔이 있음
            {
                EffectTriggerExplosion(caster, target.Transform.position, effect, skill);
            }
        }
        protected void EffectTriggerLand(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, SBSkill skill, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> passives, bool isPassiveStart)
        {
            SkillEffectData curEffect = null;
            do
            {
                if (curEffect == null)
                    curEffect = effect;
                else
                    curEffect = SkillEffectData.Get(curEffect.NEXT_EFFECT);

                if (IsNotEffectTarget(curEffect, caster, target))
                    continue;

                var skillLevel = skill.SkillType switch
                {
                    eBattleSkillType.Skill1 => caster.SkillLevel,
                    _ => 1
                };

                var effectStat = curEffect.GetEffectStat(skillLevel);
                if (effectStat.EFFECT_RATE < 100)
                {
                    var rnd = Random(0, 100, RandomLog.RandomReason.EffectTriggerLand);
                    if (rnd >= effectStat.EFFECT_RATE)//효과 발동 실패
                        continue;
                }

                switch (curEffect.TYPE)
                {
                    case eSkillEffectType.DMG:
                    {
                        VoidDelegate dmgFunc = () =>
                        {
                            Damage(caster, target, curEffect, skill.SkillType, 1f, passives, isPassiveStart);
                        };
                        if (curEffect.DELAY > 0)
                        {
                            var delayObj = new SBDelayObject();
                            delayObj.Set(null, 0f, dmgFunc, curEffect.DELAY);
                            projectiles.Add(delayObj);
                        }
                        else
                            dmgFunc?.Invoke();
                    }
                    break;
                    case eSkillEffectType.TRIGGER://이거로 들어오면 오류(미사용 하기로함)
                    {
#if DEBUG
                        Debug.LogError("EffectTriggerTarget => effect Type Error : TRIGGER");
#endif
                    }
                    break;
                    case eSkillEffectType.EFFECT:
                    {
                        if (isPassiveStart)
                        {
                            if (curEffect.TARGET_EFFECT_RSC_KEY > 0)
                                CreateFieldEffect(SkillResourceData.Get(curEffect.TARGET_EFFECT_RSC_KEY), caster, target, Vector3.zero, target.Transform.localScale);
                        }
                    }
                    break;
                    default:
                    {
                        VoidDelegate infoFunc = () =>
                        {
                            SetEffectInfo(caster, target, curEffect, skill, 1f, passives, isPassiveStart);
                        };
                        if (curEffect.DELAY > 0)
                        {
                            var delayObj = new SBDelayObject();
                            delayObj.Set(null, 0f, infoFunc, curEffect.DELAY);
                            projectiles.Add(delayObj);
                        }
                        else
                            infoFunc?.Invoke();
                    }
                    break;
                }
            } while (IsNextEffect(curEffect));
        }
        protected void EffectTriggerExplosion(IBattleCharacterData caster, Vector3 targetWorldPos, SkillEffectData effect, SBSkill skill)
        {
            var RangeX = effect.EXPLOSION_X;
            var RangeY = effect.EXPLOSION_Y;
            SBObject sbObj;
            switch (effect.EX_RANGE_TYPE)
            {
                case eSkillRangeType.CIRCLE_F:
                    circle.SetPosition(targetWorldPos);
                    circle.SetDirection(GetAttackDirection(caster));
                    circle.SetEllipse(RangeX, RangeY);
                    sbObj = circle;
                    break;
                case eSkillRangeType.SQUARE_C:
                    rect.SetPosition(targetWorldPos);
                    rect.SetDirection(eDirectionBit.None);
                    rect.SetEllipse(RangeX, RangeY);
                    sbObj = rect;
                    break;
                case eSkillRangeType.SQUARE_F:
                    rect.SetPosition(targetWorldPos);
                    rect.SetDirection(GetAttackDirection(caster));
                    rect.SetEllipse(RangeX, RangeY);
                    sbObj = rect;
                    break;
                case eSkillRangeType.SECTOR_F:
                    cone.SetPosition(targetWorldPos);
                    cone.SetDirection(GetAttackDirection(caster));
                    cone.SetCone(RangeX, RangeY);
                    sbObj = cone;
                    break;
                case eSkillRangeType.CIRCLE_C:
                default:
                    circle.SetPosition(targetWorldPos);
                    circle.SetDirection(eDirectionBit.None);
                    circle.SetEllipse(RangeX, RangeY);
                    sbObj = circle;
                    break;
            }

            bool isExplosionCheck = false;
            switch (effect.EX_RANGE_TYPE)
            {
                case eSkillRangeType.CIRCLE_C:
                case eSkillRangeType.CIRCLE_F:
                case eSkillRangeType.SQUARE_C:
                case eSkillRangeType.SQUARE_F:
                    explosion.SetData(caster, targetWorldPos, effect);
                    isExplosionCheck = true;
                    break;
                default:
                    break;
            }
            var curCount = 0;
            var targetList = GetSkillEffectList(caster, targetWorldPos, effect);
            for (int i = 0, count = targetList.Count; i < count; ++i)
            {
                var target = targetList[i];

                if (sbObj.IsContain(target.transform.position))
                {
                    EffectTriggerTarget(caster, target.Data, effect, skill, isExplosionCheck ? explosion.ExplosionValue(target.transform.position) : 1f);
                    curCount++;
                }

                if (curCount >= effect.EX_TARGET_COUNT)
                    break;
            }
        }
        /// <summary>
        /// 실제 전투 사용용도의 Target
        /// 페시브 발동은 여기서
        /// </summary>
        protected void EffectTriggerTarget(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, SBSkill skill, float exValue = 1f)
        {
            var passives = SBFunc.GetPassives(caster, target, Data.BattleType);
            AdditionalStatus passiveHitStatus = null;
            int refCount = 0;
            if (passives != null)
            {
                if (passives.TryGetValue(skill.SkillType == eBattleSkillType.Skill1 ? eSkillPassiveStartType.SKILL_ATTACK : eSkillPassiveStartType.NORMAL_ATTACK, out var attackPassive))
                {
                    for (int p = 0, count = attackPassive.Count; p < count; ++p)
                    {
                        if (null == attackPassive[p])
                            continue;

                        var passiveTarget = attackPassive[p].IsPassiveSelf() ? caster : target;
                        if (attackPassive[p].IsPassiveEffect(eSkillPassiveEffect.HIT))
                        {
                            if (passiveHitStatus == null)
                            {
                                passiveHitStatus = new();
                                passiveHitStatus.Initialze();
                            }
                            AddPassiveHitBuff(passiveHitStatus, attackPassive[p], caster, passiveTarget);
                        }
                        else
                        {
                            ActivePassiveEffect(attackPassive[p], caster, passiveTarget, 0);
                        }
                    }
                }
            }
            if (passiveHitStatus != null)
            {
                caster.Stat.IncreaseStatus(passiveHitStatus, true);
            }
            refCount++;
            VoidDelegate refFunc = () =>
            {
                --refCount;
                if (refCount <= 0 && passiveHitStatus != null)
                {
                    caster.Stat.DecreaseStatus(passiveHitStatus, true);
                    passiveHitStatus.ClearAll();
                    passiveHitStatus = null;
                    refFunc = null;
                }
            };
            SkillEffectData curEffect = null;
            do
            {
                if (curEffect == null)
                    curEffect = effect;
                else
                    curEffect = SkillEffectData.Get(curEffect.NEXT_EFFECT);

                if (IsNotEffectTarget(curEffect, caster, target))
                    continue;

                var skillLevel = skill.SkillType switch
                {
                    eBattleSkillType.Skill1 => caster.SkillLevel,
                    _ => 1
                };

                var effectStat = curEffect.GetEffectStat(skillLevel);
                if (effectStat.EFFECT_RATE < 100)
                {
                    var rnd = Random(0, 100, RandomLog.RandomReason.EffectTriggerTarget1);
                    if (rnd >= effectStat.EFFECT_RATE)//효과 발동 실패
                        continue;
                }

                switch (curEffect.TYPE)
                {
                    case eSkillEffectType.DMG:
                    {
                        ++refCount;
                        VoidDelegate dmgFunc = () =>
                        {
                            if (IsMiss(caster, target))
                            {
                                UIDamageEvent.Send(0, eDamageType.MISS, target.Transform, new Vector3(-target.ConvertPos(damageX), damageY), false, false, false);
                                Debug.Log("MISS !");
                            } 
                            else
                            {
                                Damage(caster, target, curEffect, skill.SkillType, exValue, passives);
                            }
                            refFunc?.Invoke();
                        };
                        if (curEffect.DELAY > 0)
                        {
                            var delayObj = new SBDelayObject();
                            delayObj.Set(null, 0f, dmgFunc, curEffect.DELAY);
                            projectiles.Add(delayObj);
                        }
                        else
                            dmgFunc?.Invoke();
                    }
                    break;
                    case eSkillEffectType.TRIGGER://이거로 들어오면 오류(미사용 하기로함)
                    {
#if DEBUG
                        Debug.LogError("EffectTriggerTarget => effect Type Error : TRIGGER");
#endif
                    }
                    break;
                    case eSkillEffectType.EFFECT:
                    {
                        if (curEffect.TARGET_EFFECT_RSC_KEY > 0)
                            CreateFieldEffect(SkillResourceData.Get(curEffect.TARGET_EFFECT_RSC_KEY), caster, target, Vector3.zero, target.Transform.localScale);
                    }
                    break;
                    default:
                    {
                        ++refCount;
                        VoidDelegate infoFunc = () =>
                        {
                            SetEffectInfo(caster, target, curEffect, skill, exValue, passives);
                            refFunc?.Invoke();
                        };
                        if (curEffect.DELAY > 0)
                        {
                            var delayObj = new SBDelayObject();
                            delayObj.Set(null, 0f, infoFunc, curEffect.DELAY);
                            projectiles.Add(delayObj);
                        }
                        else
                            infoFunc?.Invoke();
                    }
                    break;
                }
            } while (IsNextEffect(curEffect));
            refFunc?.Invoke();
        }
        /// <summary>
        /// 실제 전투중에는 사용하지 않음
        /// 사용처 : 이동중 회복, 아레나 시간지나고 버프 등
        /// 페시브 스킬 발동을 하지 맙시다
        /// </summary>
        protected void EffectTriggerTarget(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, eBattleSkillType skillType = eBattleSkillType.Skill1, float exValue = 1f)
        {
            SkillEffectData curEffect = null;
            do
            {
                if (curEffect == null)
                    curEffect = effect;
                else
                    curEffect = SkillEffectData.Get(curEffect.NEXT_EFFECT);

                if (IsNotEffectTarget(curEffect, caster, target))
                    continue;

                var skillLevel = skillType switch
                {
                    eBattleSkillType.Skill1 => caster.SkillLevel,
                    _ => 1
                };

                var effectStat = curEffect.GetEffectStat(skillLevel);
                if (effectStat.EFFECT_RATE < 100)
                {
                    var rnd = Random(0, 100, RandomLog.RandomReason.EffectTriggerTarget2);
                    if (rnd >= effectStat.EFFECT_RATE)//효과 발동 실패
                        continue;
                }

                switch (curEffect.TYPE)
                {
                    case eSkillEffectType.DMG:
                    //{
                    //    VoidDelegate dmgFunc = () =>
                    //    {
                    //        Damage(caster, target, curEffect, skillType, exValue);
                    //    };
                    //    if (curEffect.DELAY > 0)
                    //    {
                    //        var delayObj = new SBDelayObject();
                    //        delayObj.Set(null, 0f, dmgFunc, curEffect.DELAY);
                    //        projectiles.Add(delayObj);
                    //    }
                    //    else
                    //        dmgFunc?.Invoke();
                    //} break;
                    case eSkillEffectType.EFFECT:
                    case eSkillEffectType.TRIGGER://이거로 들어오면 오류
                    {
#if DEBUG
                        Debug.LogError(SBFunc.StrBuilder("EffectTriggerTarget => effect Type Error : ", curEffect.TYPE.ToString()));
#endif
                    }
                    break;
                    default:
                    {
                        VoidDelegate infoFunc = () =>
                        {
                            SetEffectInfo(caster, target, curEffect, skillType, exValue, null);
                        };
                        if (curEffect.DELAY > 0)
                        {
                            var delayObj = new SBDelayObject();
                            delayObj.Set(null, 0f, infoFunc, curEffect.DELAY);
                            projectiles.Add(delayObj);
                        }
                        else
                            infoFunc?.Invoke();
                    }
                    break;
                }
            } while (IsNextEffect(curEffect));
        }
        #endregion
        #region Skill 효과
        protected virtual void Damage(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, eBattleSkillType type, float exValue = 1f, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> passives = null, bool isEffect = true)
        {
            if (target == null || target.Death || target.Untouchable)
                return;

            if (type == eBattleSkillType.Normal)
                SoundManager.Instance.PlaySFX("hit_01");

            var spine = target.GetSpine();

            var isINVINCIBILITY = target.IsEffectInfo(eSkillEffectType.IMMUNE_DMG);
            if (!isINVINCIBILITY)
            {
                if (spine != null)
                {
                    spine.Hit();
                    spine.KnockBackHit();
                }
            }

            if (effect != null)
            {
                var isSkill = type == eBattleSkillType.Skill1;
                int skillLevel = isSkill ? caster.SkillLevel : 1;
                var skilLevelStat = effect.GetEffectStat(skillLevel);
                var extraValue = exValue * skilLevelStat.VALUE * SBDefine.CONVERT_FLOAT;
                var rnd = Random(0, 100, RandomLog.RandomReason.Damage);

                if (skilLevelStat.CRI <= 0)
                    rnd = int.MaxValue;

                var element = eDamageType.ELEMENT_NORMAL;
                var eType = rnd < (caster.Stat.GetTotalStatus(eStatusType.CRI_PROC) - target.Stat.GetTotalStatusConvert(eStatusType.CRI_RESIS)) ? eDamageType.CRITICAL : element;

                bool isHitBuff = false;
                if (null != passives)
                {
                    if (eType == eDamageType.CRITICAL && passives.TryGetValue(eSkillPassiveStartType.CRITICAL_ATTACK, out var cValue))
                    {
                        for (int p = 0, pCount = cValue.Count; p < pCount; ++p)
                        {
                            if (null == cValue[p])
                                continue;

                            var passiveTarget = cValue[p].IsPassiveSelf() ? caster : target;
                            if (cValue[p].IsPassiveEffect(eSkillPassiveEffect.HIT))
                            {
                                AddPassiveHitBuff(passiveCriStatus, cValue[p], caster, passiveTarget, isEffect);
                                isHitBuff = true;
                            }
                            else
                            {
                                ActivePassiveEffect(cValue[p], caster, passiveTarget, 0, isEffect);
                            }
                        }
                    }
                }
                if (isHitBuff)
                    caster.Stat.IncreaseStatus(passiveCriStatus, true);

                var BaseDMG = BaseCalc(caster, target, skilLevelStat, type);
                var ElementDMG = ElementCalc(caster, target, skilLevelStat, type);

                var CriDMG = 0;
                if (eType == eDamageType.CRITICAL)
                {
                    CriDMG = CriCalc(caster, target, skilLevelStat, type);
                    if (CriDMG < 1)
                        eType = element;
                }

                var dmgPos = new Vector3(-target.ConvertPos(damageX), damageY);

                var allResis = target.Stat.GetTotalStatusConvert(eStatusType.ALL_DMG_RESIS);
                //보스 대미지 계산 - 흡혈 이후에 더하기위해 미리 따로 계산
                var bossDMG = 0f;
                if (target.IsBoss)
                {
                    bossDMG = caster.Stat.GetTotalStatusInt(eStatusType.BOSS_DMG) * extraValue;
                    if (isSkill)
                        bossDMG += bossDMG * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_SKILL_DMG);
                    bossDMG *= (skilLevelStat.BOSS * SBDefine.CONVERT_FLOAT) + caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_BOSS_DMG);
                    bossDMG -= bossDMG * allResis;

                    if (target.MOVE_SPEED == 0.0f)//world boss는 멈춰있어서
                        dmgPos += new Vector3(Random(-0.25f, 0.25f), Random(-0.25f, 0.25f));
                }
                //

                var DMG = Mathf.FloorToInt((BaseDMG + ElementDMG + CriDMG) * extraValue);
                //스킬 계산
                if (isSkill)
                {
                    DMG += Mathf.FloorToInt(DMG * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_SKILL_DMG));
                    DMG += caster.Stat.GetTotalStatusInt(eStatusType.ADD_SKILL_DMG);
                }
                //
                //모든 대미지 저항 값 빼기
                DMG -= Mathf.FloorToInt(DMG * allResis);
                //

                //흡혈 처리는 보스대미지 처리 계산 전에
                SetVamp(caster, DMG);
                //

                //보스 대미지는 미리 계산하고 흡혈이후에 더함
                DMG += Mathf.FloorToInt(bossDMG);
                //
                //보스 대미지 저항 계산
                if (caster.IsBoss)
                {
                    DMG -= Mathf.FloorToInt(DMG * target.Stat.GetTotalStatusConvert(eStatusType.BOSS_DMG_RESIS));
                }
                //
                //SHIELD_BREAKER 최종 대미지 완료 이후
                var shieldBreaker = 0;
                var shieldPoint = target.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
                if (shieldPoint > 0f)
                {
                    shieldBreaker = Mathf.FloorToInt(DMG * caster.Stat.GetTotalStatusConvert(eStatusType.SHIELD_BREAKER));
                    DMG += shieldBreaker;
                }
                //
                if (DMG < 1)
                    DMG = 1;

                if (isHitBuff)
                {
                    caster.Stat.DecreaseStatus(passiveCriStatus, true);
                    passiveCriStatus.ClearAll();
                }

                UIDamageEvent.Send(DMG, eType, target.Transform, dmgPos, caster.IsEnemy, false, shieldBreaker > 0);
                if (effect.TARGET_EFFECT_RSC_KEY > 0)
                    CreateFieldEffect(SkillResourceData.Get(effect.TARGET_EFFECT_RSC_KEY), caster, target, Vector2.zero, target.IsEnemy ? Vector3.one : Vector3.one * 0.5f);
                SetDamage(caster, target, DMG, type == eBattleSkillType.Skill1);
            }
        }
        protected virtual void SetStatistics(int casterID, int targetID, int DMG, bool isEnemy, bool isSkill, int shieldPoint)
        {
            StatisticsMananger.Instance.AddDamage(casterID, targetID, DMG, isEnemy, isSkill, DMG > shieldPoint ? shieldPoint : DMG);
        }
        protected virtual void DotSkillDamage(IBattleCharacterData caster, IBattleCharacterData target, EffectInfo effect, eBattleSkillType type, float exValue = 1f)
        {
            if (target == null || target.Death || target.Untouchable)
                return;

            //도트 데미지는 타격이펙트 안하기
            //var spine = target.GetSpine();
            //var isINVINCIBILITY = target.IsEffectInfo(eSkillEffectType.IMMUNE_DMG);
            //if (!isINVINCIBILITY)
            //{
            //    if (spine != null)
            //    {
            //        spine.Hit();
            //        spine.KnockBackHit();
            //    }
            //}

            if (effect != null)
            {
                var isSkill = type == eBattleSkillType.Skill1;
                int skillLevel = isSkill ? caster.SkillLevel : 1;
                var skilLevelStat = effect.Data.GetEffectStat(skillLevel);
                var extraValue = exValue * skilLevelStat.VALUE * SBDefine.CONVERT_FLOAT;
                var BaseDMG = BaseCalc(caster, target, skilLevelStat, type);
                var ElementDMG = ElementCalc(caster, target, skilLevelStat, type);

                var rnd = Random(0, 100, RandomLog.RandomReason.DotSkillDamage);
                if (skilLevelStat.CRI <= 0)
                    rnd = int.MaxValue;

                var element = eDamageType.ELEMENT_NORMAL;
                var eType = rnd < (caster.Stat.GetTotalStatus(eStatusType.CRI_PROC) - target.Stat.GetTotalStatusConvert(eStatusType.CRI_RESIS)) ? eDamageType.CRITICAL : element;
                
                bool isHitBuff = false;
                var passives = AttackPassive(caster, target, eBattleSkillType.None, eType == eDamageType.CRITICAL);
                if (null != passives)
                {
                    for (int p = 0, pCount = passives.Count; p < pCount; ++p)
                    {
                        if (null == passives[p])
                            continue;

                        var passiveTarget = passives[p].IsPassiveSelf() ? caster : target;
                        if (passives[p].IsPassiveEffect(eSkillPassiveEffect.HIT))
                        {
                            AddPassiveHitBuff(passiveCriStatus, passives[p], caster, passiveTarget);
                            isHitBuff = true;
                        }
                        else
                        {
                            ActivePassiveEffect(passives[p], caster, passiveTarget, 0);
                        }
                    }
                }
                if (isHitBuff)
                    caster.Stat.IncreaseStatus(passiveCriStatus, true);

                var CriDMG = 0;
                if (eType == eDamageType.CRITICAL)
                {
                    CriDMG = CriCalc(caster, target, skilLevelStat, type);
                    if (CriDMG < 1)
                        eType = element;
                }

                var allResis = target.Stat.GetTotalStatusConvert(eStatusType.ALL_DMG_RESIS);
                //보스 대미지 계산 - 흡혈 이후에 더하기위해 미리 따로 계산
                var bossDMG = 0f;
                if (target.IsBoss)
                {
                    bossDMG = caster.Stat.GetTotalStatusInt(eStatusType.BOSS_DMG);
                    bossDMG = bossDMG > 0 ? bossDMG * extraValue / effect.Data.HIT_COUNT : 0;
                    if (isSkill)
                        bossDMG += bossDMG * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_SKILL_DMG);
                    bossDMG *= skilLevelStat.BOSS * SBDefine.CONVERT_FLOAT;
                    bossDMG -= bossDMG * allResis;
                }
                //

                var dmgPos = new Vector3(-target.ConvertPos(damageX), damageY);

                var DMG = Mathf.FloorToInt((BaseDMG + ElementDMG + CriDMG) * extraValue / effect.Data.HIT_COUNT);
                if (isSkill)
                {
                    DMG += Mathf.FloorToInt(DMG * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_SKILL_DMG));
                    var addSkillDmg = caster.Stat.GetTotalStatusInt(eStatusType.ADD_SKILL_DMG);
                    DMG += addSkillDmg > 0 ? addSkillDmg / effect.Data.HIT_COUNT : 0;
                }
                //모든 대미지 저항 값 빼기
                DMG -= Mathf.FloorToInt(DMG * allResis);

                //흡혈 처리는 보스대미지 처리 계산 전에
                SetVamp(caster, DMG);
                //

                DMG += Mathf.FloorToInt(bossDMG);
                //보스 대미지 저항 계산
                if (caster.IsBoss)
                {
                    DMG -= Mathf.FloorToInt(DMG * target.Stat.GetTotalStatusConvert(eStatusType.BOSS_DMG_RESIS));
                }
                //
                //실드가 있다면 BREAKER 최종 대미지 타이밍에 계산
                var shieldBreaker = 0;
                var shieldPoint = target.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
                if (shieldPoint > 0f)
                {
                    shieldBreaker = Mathf.FloorToInt(DMG * caster.Stat.GetTotalStatusConvert(eStatusType.SHIELD_BREAKER));
                    DMG += shieldBreaker;
                }
                //
                if (DMG < 1)
                    DMG = 1;

                if (isHitBuff)
                {
                    caster.Stat.DecreaseStatus(passiveCriStatus, true);
                    passiveCriStatus.ClearAll();
                }
                
                UIDamageEvent.Send(DMG, eType, target.Transform, dmgPos, caster.IsEnemy, false, shieldBreaker > 0);
                SetDamage(caster, target, DMG, true);
            }
        }
        protected virtual void SetVamp(IBattleCharacterData caster, int DMG)
        {
            if (caster.Death)
                return;

            //피흡처리
            var vamp = caster.Stat.GetTotalStatusConvert(eStatusType.VAMP);
            if (vamp > 0f)
            {
                var hpRecorvery = Mathf.FloorToInt(DMG * vamp);
                caster.HP += hpRecorvery;
                if (caster.HP > caster.MaxHP)
                {
                    hpRecorvery -= caster.HP - caster.MaxHP;
                    caster.HP = caster.MaxHP;
                }
                UIDamageEvent.Send(hpRecorvery, eDamageType.RECORVERY, caster.Transform, new Vector3(-caster.ConvertPos(damageX), damageY), true);
            }
            //
        }
        protected virtual void SetDamage(IBattleCharacterData caster, IBattleCharacterData target, int DMG, bool isSkill, bool isPassive = false)
        {
            if (target.Death)
                return;

            int shieldAmount = target.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
            if (shieldAmount > 0)
            {
                if (DMG > shieldAmount)
                {
                    StatisticsMananger.Instance.AddRecieveDmg(target.ID, shieldAmount, target.IsEnemy, true);
                    StatisticsMananger.Instance.AddRecieveDmg(target.ID, DMG - shieldAmount, target.IsEnemy, false);
                }
                else
                {   // 쉴드 >= 데미지
                    StatisticsMananger.Instance.AddRecieveDmg(target.ID, DMG, target.IsEnemy, true);
                }
            }
            else
                StatisticsMananger.Instance.AddRecieveDmg(target.ID, DMG, target.IsEnemy, false);

            if (false == isPassive)
                ActivePassiveDamage(target, caster, DMG);

            SetStatistics(caster.ID, target.ID, DMG, caster.IsEnemy, isSkill, shieldAmount > 0? shieldAmount:0);
            target.GetSpine().SetDamage(DMG, caster);
            if (target.Death)
            {
                StatisticsMananger.Instance.SetDeath(target.ID, target.IsEnemy);
                if (target.IsActioning)
                {
                    target.GetSpine().ClearEffectSpine();
                    target.GetSpine().ClearAnimation();
                    target.SetActiveSkilType(eBattleSkillType.None);
                    target.SetActionCoroutine(null);
                }
                if (target.IsEnemy)
                    ++defenseDeath;
                else
                    ++offenseDeath;
            }
        }
        protected virtual void SetEffectInfo(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, SBSkill skill, float exValue, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> passives = null, bool isEffect = true)
        {
            if (caster == null || target == null || effect == null || target.Transform == null || caster.Transform == null)
                return;

            var skillLevel = skill.SkillType switch
            {
                eBattleSkillType.Skill1 => caster.SkillLevel,
                _ => 1
            };

            EffectInfo effectInfo;
            switch (effect.TYPE)
            {
                case eSkillEffectType.PULL:
                    if (target.Untouchable || target.HasImmunity(eCharacterImmunity.PULL) || target.IsEffectInfo(eSkillEffectType.IMN_PULL, eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new PullEffect(skill.TargetPosition);
                    break;
                default:
                    SetEffectInfo(caster, target, effect, skill.SkillType, exValue, passives, isEffect);
                    return;
            }

            if (effectInfo == null)
                return;

            effectInfo.SetEffectData(caster, target, effect, skillLevel);
            if (target.SetEffectInfo(effectInfo) && isEffect)
                effectInfo.SetFollowEffect(CreateFollowEffect(SkillResourceData.Get(effect.TARGET_EFFECT_RSC_KEY), effect, caster, target, effectInfo.MaxTime));
        }
        protected bool IsEffectTargetSkip(eSkillEffectType type, IBattleCharacterData target)
        {
            switch (type)
            {
                case eSkillEffectType.EFFECT:
                    return false;
                case eSkillEffectType.BUFF:
                    return false;
                case eSkillEffectType.BUFF_MAIN_ELEMENT:
                    return false;
                case eSkillEffectType.ENV_BUFF:
                    return false;
                case eSkillEffectType.IMMUNE_DMG:
                    return false;
                case eSkillEffectType.IMMUNE_HARM:
                    return false;
                case eSkillEffectType.HEAL:
                    return false;
                case eSkillEffectType.SHIELD:
                    return false;
                case eSkillEffectType.D_DEBUFF:
                    return false;
                case eSkillEffectType.D_ADEBUFF:
                    return false;
                case eSkillEffectType.D_DOT:
                    return false;
                case eSkillEffectType.D_STUN:
                    return false;
                case eSkillEffectType.D_AGGRO:
                    return false;
                case eSkillEffectType.NONE:
                case eSkillEffectType.EFFECT_MAX:
                default:
                    return target.Untouchable;
            }
        }
        protected bool IsPassiveSkip(eSkillPassiveEffect type, IBattleCharacterData target)
        {
            switch (type)
            {
                case eSkillPassiveEffect.STAT:
                case eSkillPassiveEffect.STAT_MAIN_ELEMENT:
                case eSkillPassiveEffect.REDUCE_COOLTIME:
                case eSkillPassiveEffect.REDUCE_BUFF:
                case eSkillPassiveEffect.REDUCE_DEBUFF:
                case eSkillPassiveEffect.STRONG_BUFF:
                case eSkillPassiveEffect.STRONG_DEBUFF:
                case eSkillPassiveEffect.HIT:
                case eSkillPassiveEffect.DMG_REFLECT:
                    return true;
                case eSkillPassiveEffect.BUFF:
                case eSkillPassiveEffect.BUFF_MAIN_ELEMENT:
                case eSkillPassiveEffect.DEBUFF:
                case eSkillPassiveEffect.CC_REFLECT:
                case eSkillPassiveEffect.SILENCE:
                case eSkillPassiveEffect.R_KNOCK_BACK:
                    return false;
                case eSkillPassiveEffect.NONE:
                case eSkillPassiveEffect.MAX:
                default:
                    return target.Untouchable;
            }
        }
        protected virtual void SetEffectInfo(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, eBattleSkillType skillType, float exValue, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> passives, bool isEffect = true)
        {
            if (caster == null || target == null || effect == null)
                return;

            var skillLevel = skillType switch
            {
                eBattleSkillType.Skill1 => caster.SkillLevel,
                _ => 1
            };

            if (IsEffectTargetSkip(effect.TYPE, target))
                return;

            bool miss = false;
            EffectInfo effectInfo;
            switch (effect.TYPE)
            {
                case eSkillEffectType.NONE:
#if DEBUG
                    Debug.LogWarning(SBFunc.StrBuilder("SetEffectInfo => 의도적인 스킵인지 확인 ->", effect.TYPE.ToString()));
#endif
                    return;

                case eSkillEffectType.DMG:
                case eSkillEffectType.NORMAL_DMG:
                case eSkillEffectType.SKILL_DMG:
                case eSkillEffectType.SKILL_CRI_DMG:
                case eSkillEffectType.SKILL_ELEMENT_DMG:
                case eSkillEffectType.PULL:
#if DEBUG
                    Debug.LogError(SBFunc.StrBuilder("SetEffectInfo => 들어오면 안되는 타입이 들어옴 ->", effect.TYPE.ToString()));
#endif
                    return;
                case eSkillEffectType.DOT:
                case eSkillEffectType.POISON:
                case eSkillEffectType.TICK_DMG:
                    miss = IsMiss(caster, target);
                    effectInfo = new DotEffect((effectInfo) => DotSkillDamage(caster, target, effectInfo, eBattleSkillType.Skill1, exValue));
                    break;
                case eSkillEffectType.STUN:
                case eSkillEffectType.FROZEN:
                    if (target.HasImmunity(eCharacterImmunity.STUN) || target.IsEffectInfo(eSkillEffectType.IMN_STUN, eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new StunEffect();
                    if (false == caster.IsEffectInfo(eSkillEffectType.STUN, eSkillEffectType.AIRBORNE))
                    {
                        var curPassive = AbnormalPassive(target, caster);
                        if (curPassive != null)
                            ActiveAbnormalPassive(curPassive, target, caster, new StunEffect(), effect, skillLevel);
                    }
                    break;
                case eSkillEffectType.AIRBORNE:
                    if (target.HasImmunity(eCharacterImmunity.AIRBORNE) || target.IsEffectInfo(eSkillEffectType.IMN_AIR, eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new AirborneEffect();
                    if (false == caster.IsEffectInfo(eSkillEffectType.AIRBORNE))
                    {
                        var curPassive = AbnormalPassive(target, caster);
                        if (curPassive != null)
                            ActiveAbnormalPassive(curPassive, target, caster, new AirborneEffect(), effect, skillLevel);
                    }
                    break;
                case eSkillEffectType.IMMUNE_DMG:
                    effectInfo = new InvincibilityEffect();
                    break;
                case eSkillEffectType.SILENCE:
                    if (target.HasImmunity(eCharacterImmunity.SILENCE) || target.IsEffectInfo(eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new SilenceEffect();
                    break;
                case eSkillEffectType.SHIELD:
                    effectInfo = new ShieldEffect();
                    if (passives != null)
                    {
                        if (passives.TryGetValue(skillType == eBattleSkillType.Skill1 ? eSkillPassiveStartType.SKILL_ATTACK : eSkillPassiveStartType.NORMAL_ATTACK, out var passiveList))
                        {
                            var passive = GetTypePassive(passiveList, eSkillPassiveEffect.STRONG_BUFF);
                            if (passive != null)
                            {
                                effectInfo.SetPassiveData(caster, target, passive, effect, skillLevel);
                                if (isEffect)
                                {
                                    var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                                    if (selfEffect != null)
                                        CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                                    var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                                    if (targetEffect != null)
                                        CreateFollowEffect(targetEffect, passive, caster, target, effectInfo.MaxTime);
                                }
                            }
                        }
                    }
                    break;
                case eSkillEffectType.HEAL:
                    effectInfo = new HealEffect();
                    break;
                case eSkillEffectType.AGGRO:
                    if (target.HasImmunity(eCharacterImmunity.AGGRO) || target.IsEffectInfo(eSkillEffectType.IMN_AGGRO, eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new AggroEffect();
                    break;
                case eSkillEffectType.AGGRO_R:
                    effectInfo = new ReverseAggroEffect();
                    break;
                case eSkillEffectType.KNOCK_BACK:
                    if (target.HasImmunity(eCharacterImmunity.KNOCK_BACK) || target.IsEffectInfo(eSkillEffectType.IMN_KNOCK, eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new KnockbackEffect();
                    if (false == caster.IsEffectInfo(eSkillEffectType.KNOCK_BACK, eSkillEffectType.AIRBORNE))
                    {
                        var curPassive = AbnormalPassive(target, caster);
                        if (curPassive != null)
                            ActiveAbnormalPassive(curPassive, target, caster, new KnockbackEffect(), effect, skillLevel);
                    }
                    break;
                case eSkillEffectType.DEBUFF:
                    effectInfo = new DebuffEffect();
                    if (passives != null)
                    {
                        if (passives.TryGetValue(skillType == eBattleSkillType.Skill1 ? eSkillPassiveStartType.SKILL_ATTACK : eSkillPassiveStartType.NORMAL_ATTACK, out var passiveList))
                        {
                            var passive = GetTypePassive(passiveList, eSkillPassiveEffect.STRONG_DEBUFF);
                            if(passive != null)
                            {
                                effectInfo.SetPassiveData(caster, target, passive, effect, skillLevel);
                                if (isEffect)
                                {
                                    var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                                    if (selfEffect != null)
                                        CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                                    var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                                    if (targetEffect != null)
                                        CreateFollowEffect(targetEffect, passive, caster, target, effectInfo.MaxTime);
                                }
                            }
                        }
                    }
                    break;
                case eSkillEffectType.BUFF:
                    effectInfo = new BuffEffect();
                    if (passives != null)
                    {
                        if (passives.TryGetValue(skillType == eBattleSkillType.Skill1 ? eSkillPassiveStartType.SKILL_ATTACK : eSkillPassiveStartType.NORMAL_ATTACK, out var passiveList))
                        {
                            var passive = GetTypePassive(passiveList, eSkillPassiveEffect.STRONG_BUFF);
                            if (passive != null)
                            {
                                effectInfo.SetPassiveData(caster, target, passive, effect, skillLevel);
                                if (isEffect)
                                {
                                    var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                                    if (selfEffect != null)
                                        CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                                    var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                                    if (targetEffect != null)
                                        CreateFollowEffect(targetEffect, passive, caster, target, effectInfo.MaxTime);
                                }
                            }
                        }
                    }
                    break;
                case eSkillEffectType.BUFF_MAIN_ELEMENT:
                    effectInfo = new BuffMainElementEffect();
                    if (passives != null)
                    {
                        if (passives.TryGetValue(skillType == eBattleSkillType.Skill1 ? eSkillPassiveStartType.SKILL_ATTACK : eSkillPassiveStartType.NORMAL_ATTACK, out var passiveList))
                        {
                            var passive = GetTypePassive(passiveList, eSkillPassiveEffect.STRONG_BUFF);
                            if (passive != null)
                            {
                                effectInfo.SetPassiveData(caster, target, passive, effect, skillLevel);
                                if (isEffect)
                                {
                                    var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                                    if (selfEffect != null)
                                        CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                                    var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                                    if (targetEffect != null)
                                        CreateFollowEffect(targetEffect, passive, caster, target, effectInfo.MaxTime);
                                }
                            }
                        }
                    }
                    break;
                case eSkillEffectType.D_BUFF:
                    effectInfo = new CancleBuffEffect();
                    break;
                case eSkillEffectType.STAT:
                    effectInfo = new StatEffect();
                    break;
                case eSkillEffectType.D_ABUFF:
                    effectInfo = new AllCancleBuffEffect();
                    break;
                case eSkillEffectType.D_DEBUFF:
                    effectInfo = new CancleDebuffEffect();
                    break;
                case eSkillEffectType.D_ADEBUFF:
                    effectInfo = new AllCancleDebuffEffect();
                    break;
                case eSkillEffectType.D_DOT:
                    effectInfo = new CancleDotEffect();
                    break;
                case eSkillEffectType.D_SHIELD:
                    effectInfo = new AllCancleShieldEffect();
                    break;
                case eSkillEffectType.D_STUN:
                    effectInfo = new CancleStunEffect();
                    break;
                case eSkillEffectType.D_AGGRO:
                    effectInfo = new CancleAggroEffect();
                    break;
                case eSkillEffectType.ENV_BUFF:
                    effectInfo = new EnvBuffEffect();
                    break;
                case eSkillEffectType.IMN_STUN:
                    effectInfo = new IMStunEffect();
                    break;
                case eSkillEffectType.IMN_AGGRO:
                    effectInfo = new IMAggroEffect();
                    break;
                case eSkillEffectType.IMN_AIR:
                    effectInfo = new IMAirborneEffect();
                    break;
                case eSkillEffectType.IMN_PULL:
                    effectInfo = new IMPullEffect();
                    break;
                case eSkillEffectType.IMN_KNOCK:
                    effectInfo = new IMKnockbackEffect();
                    break;
                case eSkillEffectType.IMN_CC:
                    effectInfo = new IMCCEffect();
                    break;
                default:
#if DEBUG
                    Debug.LogError("SetEffectInfo => 정의되지 않은 타입이 들어옴");
#endif
                    return;
            }

            if (miss)
            {
                UIDamageEvent.Send(0, eDamageType.MISS, target.Transform, new Vector3(-target.ConvertPos(damageX), damageY), false, false, false);
                Debug.Log("MISS !");
                return;
            }

            if (effectInfo == null)
                return;

            effectInfo.SetEffectData(caster, target, effect, skillLevel);
            if (target.SetEffectInfo(effectInfo) && isEffect)
                effectInfo.SetFollowEffect(CreateFollowEffect(SkillResourceData.Get(effect.TARGET_EFFECT_RSC_KEY), effect, caster, target, effectInfo.MaxTime));
        }
        protected virtual void SetPassiveEffectInfo(IBattleCharacterData caster, IBattleCharacterData target, SkillPassiveData passive, bool isEffect = true)
        {
            if (caster == null || target == null || passive == null)
                return;

            if (IsPassiveSkip(passive.PASSIVE_EFFECT, target))
                return;

            EffectInfo effectInfo = null;
            switch (passive.PASSIVE_EFFECT)
            {
                case eSkillPassiveEffect.BUFF:
                    effectInfo = new PassiveBuffEffect();
                    break;
                case eSkillPassiveEffect.BUFF_MAIN_ELEMENT:
                    effectInfo = new PassiveMainElementBuff();
                    break;
                case eSkillPassiveEffect.DEBUFF:
                    effectInfo = new PassiveDebuffEffect();
                    break;
                case eSkillPassiveEffect.SILENCE:
                    if (target.HasImmunity(eCharacterImmunity.SILENCE) || target.IsEffectInfo(eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new PassiveSilenceEffect();
                    break;
                case eSkillPassiveEffect.R_KNOCK_BACK:
                    if (target.HasImmunity(eCharacterImmunity.KNOCK_BACK) || target.IsEffectInfo(eSkillEffectType.KNOCK_BACK, eSkillEffectType.IMN_KNOCK, eSkillEffectType.IMN_CC))
                        return;

                    effectInfo = new PassiveKnockBack();
                    break;
                default:
#if DEBUG
                    Debug.LogError(SBFunc.StrBuilder("SetEffectInfo => 들어오면 안되는 타입이 들어옴 ->", passive.PASSIVE_EFFECT.ToString()));
#endif
                    break;
            }

            if (effectInfo == null)
                return;

            effectInfo.SetPassiveData(caster, target, passive, null, 0);
            if (isEffect)
            {
                var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                if (selfEffect != null)
                    CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
            }
            if (target.SetEffectInfo(effectInfo) && isEffect)
            {
                var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                if (targetEffect != null)
                    effectInfo.SetFollowEffect(CreateFollowEffect(targetEffect, passive, caster, target, effectInfo.MaxTime));
            }
        }
                
        #endregion
        #region Damage
        /// <summary>
        /// []는 PVP시에만 적용
        /// 최종 공격력 = 공격력 * 스킬공격력계수 + 최대체력 * 스킬체력계수 + 방어력 * 스킬방어력계수
        /// 방어력 감소치 = 방어력 / (100 + 방어력)
        /// 기본 대미지 = (최종 공격력 * (1f - 방어력 감소치) * 기본 대미지 증폭 [* PVP 대미지 증폭] + 추가 기본 대미지 [+ 추가 PVP 대미지]) * (1f - 기본 대미지 저항)
        /// 속성과 크리는 따로 계산함
        /// </summary>
        /// <returns>계산된 기본 대미지</returns>
        protected virtual int BaseCalc(IBattleCharacterData caster, IBattleCharacterData target, SkillLevelStat skillStat, eBattleSkillType type)
        {
            if (target.IsEffectInfo(eSkillEffectType.IMMUNE_DMG))
                return 0;

            //최종 공격력 수치
            float atk = 0;
            if (skillStat.ATK > 0f)
                atk += SBFunc.CalcRatio(caster.Stat.GetTotalStatusInt(eStatusType.ATK), skillStat.ATK);
            if (skillStat.DEF > 0f)
                atk += SBFunc.CalcRatio(caster.Stat.GetTotalStatusInt(eStatusType.DEF), skillStat.DEF);
            if (skillStat.HP > 0f)
                atk += SBFunc.CalcRatio(caster.Stat.GetTotalStatusInt(eStatusType.HP), skillStat.HP);

            //방어력 감소치
            float DEF_RATE = SBFunc.Defense(target.Stat.GetTotalStatusInt(eStatusType.DEF));
            //방어력 계산된 대미지
            int DMG = SBFunc.Offense(atk, DEF_RATE);

            float ratio = 1f + caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_ATK_DMG);
            float added = caster.Stat.GetTotalStatus(eStatusType.ADD_ATK_DMG);
            if (Data.BattleType == eBattleType.ARENA || Data.BattleType == eBattleType.ChampionBattle)
            {
                ratio += caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PVP_DMG);
                ratio += skillStat.PVP * SBDefine.CONVERT_FLOAT;
                added += caster.Stat.GetTotalStatus(eStatusType.ADD_PVP_DMG);
            }
            float BD = DMG * ratio + added;

            float pierce = caster.Stat.GetTotalStatusConvert(eStatusType.PHYS_DMG_PIERCE);
            float resis = 1f - (target.Stat.GetTotalStatusConvert(eStatusType.ATK_DMG_RESIS) - pierce);
            if (resis < 0f)
                resis = 0f;
            if (resis > 1.0f)
                resis = 1.0f;

            //기본 대미지
            var Damage = Mathf.FloorToInt(BD * resis);
            if (Damage < 1)
                Damage = 1;

            SaveDamageLog(caster.ID, Damage);
            SaveDamageLog(target.ID, Damage, true, DMG, Damage);
            return Damage;
        }
        /// <summary>
        /// 불 속성 공격력 = 불 속성 스텟 * 불 속성 스킬계수 * 불 속성 보너스계수 * (1f - 불 속성 저항)
        /// 물 속성 공격력 = 물 속성 스텟 * 물 속성 스킬계수 * 물 속성 보너스계수 * (1f - 물 속성 저항)
        /// 땅 속성 공격력 = 땅 속성 스텟 * 땅 속성 스킬계수 * 땅 속성 보너스계수 * (1f - 땅 속성 저항)
        /// 바람 속성 공격력 = 바람 속성 스텟 * 바람 속성 스킬계수 * 바람 속성 보너스계수 * (1f - 바람 속성 저항)
        /// 어둠 속성 공격력 = 어둠 속성 스텟 * 어둠 속성 스킬계수 * 어둠 속성 보너스계수 * (1f - 어둠 속성 저항)
        /// 빛 속성 공격력 = 빛 속성 스텟 * 빛 속성 스킬계수 * 빛 속성 보너스계수 * (1f - 빛 속성 저항)
        /// 최종 속성 공격력 = 불 속성 공격력 + 물 속성 공격력 + 땅 속성 공격력 +
        ///                    바람 속성 공격력 + 어둠 속성 공격력 + 빛 속성 공격력
        /// </summary>
        /// <returns>계산된 최종 속성 대미지</returns>
        protected int ElementCalc(IBattleCharacterData caster, IBattleCharacterData target, SkillLevelStat skillStat, eBattleSkillType type)
        {
            if (target.IsEffectInfo(eSkillEffectType.IMMUNE_DMG))
                return 0;

            float DMG = 0;
            float ED = 0;
            for (var curElement = eElementType.START; curElement < eElementType.MAX; ++curElement)
            {
                // 각 속성 스킬계수
                float skillValue = 0f;
                // 각 속성 스텟
                float damage = 0f;
                // 각 속성 저항
                float resis = 1f;
                // 각 속성 관통
                float pierce = 0f;
                // 각 속성 보너스
                float elementBonus = 1f;

                float myElemDamage = 0;
                //내 속성 추가 대미지
                if (caster.Element == curElement)
                {
                    myElemDamage = caster.Stat.GetTotalStatusInt(eStatusType.ADD_MAIN_ELEMENT_DMG);
                }

                var elementData = ElementRateData.Get(curElement);
                if (elementData != null)
                {
                    switch (curElement)
                    {
                        case eElementType.FIRE:
                            skillValue = skillStat.FIRE;
                            damage = caster.Stat.GetTotalStatusInt(eStatusType.FIRE_DMG);
                            resis = target.Stat.GetTotalStatusConvert(eStatusType.FIRE_DMG_RESIS);
                            pierce = caster.Stat.GetTotalStatusConvert(eStatusType.FIRE_DMG_PIERCE);
                            break;
                        case eElementType.WATER:
                            skillValue = skillStat.WATER;
                            damage = caster.Stat.GetTotalStatusInt(eStatusType.WATER_DMG);
                            resis = target.Stat.GetTotalStatusConvert(eStatusType.WATER_DMG_RESIS);
                            pierce = caster.Stat.GetTotalStatusConvert(eStatusType.WATER_DMG_PIERCE);
                            break;
                        case eElementType.EARTH:
                            skillValue = skillStat.EARTH;
                            damage = caster.Stat.GetTotalStatusInt(eStatusType.EARTH_DMG);
                            resis = target.Stat.GetTotalStatusConvert(eStatusType.EARTH_DMG_RESIS);
                            pierce = caster.Stat.GetTotalStatusConvert(eStatusType.EARTH_DMG_PIERCE);
                            break;
                        case eElementType.WIND:
                            skillValue = skillStat.WIND;
                            damage = caster.Stat.GetTotalStatusInt(eStatusType.WIND_DMG);
                            resis = target.Stat.GetTotalStatusConvert(eStatusType.WIND_DMG_RESIS);
                            pierce = caster.Stat.GetTotalStatusConvert(eStatusType.WIND_DMG_PIERCE);
                            break;
                        case eElementType.LIGHT:
                            skillValue = skillStat.LIGHT;
                            damage = caster.Stat.GetTotalStatusInt(eStatusType.LIGHT_DMG);
                            resis = target.Stat.GetTotalStatusConvert(eStatusType.LIGHT_DMG_RESIS);
                            pierce = caster.Stat.GetTotalStatusConvert(eStatusType.LIGHT_DMG_PIERCE);
                            break;
                        case eElementType.DARK:
                            skillValue = skillStat.DARK;
                            damage = caster.Stat.GetTotalStatusInt(eStatusType.DARK_DMG);
                            resis = target.Stat.GetTotalStatusConvert(eStatusType.DARK_DMG_RESIS);
                            pierce = caster.Stat.GetTotalStatusConvert(eStatusType.DARK_DMG_PIERCE);
                            break;
                    }

                    switch (target.Element)
                    {
                        case eElementType.FIRE:
                            elementBonus = elementData.T_FIRE * SBDefine.CONVERT_ELEMENT;
                            break;
                        case eElementType.WATER:
                            elementBonus = elementData.T_WATER * SBDefine.CONVERT_ELEMENT;
                            break;
                        case eElementType.EARTH:
                            elementBonus = elementData.T_EARTH * SBDefine.CONVERT_ELEMENT;
                            break;
                        case eElementType.WIND:
                            elementBonus = elementData.T_WIND * SBDefine.CONVERT_ELEMENT;
                            break;
                        case eElementType.LIGHT:
                            elementBonus = elementData.T_LIGHT * SBDefine.CONVERT_ELEMENT;
                            break;
                        case eElementType.DARK:
                            elementBonus = elementData.T_DARK * SBDefine.CONVERT_ELEMENT;
                            break;
                    }
                    resis = 1f - (resis - pierce);
                    if (resis < 0f)
                        resis = 0f;
                    if (resis > 1f)
                        resis = 1f;

                    damage = SBFunc.CalcRatio(damage + myElemDamage, skillValue) * elementBonus;
                    DMG += damage;
                    ED += damage * resis;
                }
            }
            if (ED < 1)
                ED = 1;

            SaveDamageLog(caster.ID, ED);
            SaveDamageLog(target.ID, ED, true, DMG, ED);
            return Mathf.FloorToInt(ED);
        }
        /// <summary>
        /// []는 PVP시에만 동작
        /// 최종 크리 대미지 = (크리 대미지 * 크리 대미지 증폭 [* PVP 크리 대미지 증폭] + 추가 크리 대미지 [+ 추가 PVP 크리 대미지]) * 스킬 크리 대미지 * (1f - 크리 대미지 저항)
        /// </summary>
        /// <returns>계산된 최종 크리 대미지</returns>
        protected virtual int CriCalc(IBattleCharacterData caster, IBattleCharacterData target, SkillLevelStat skillStat, eBattleSkillType type)
        {
            if (target.IsEffectInfo(eSkillEffectType.IMMUNE_DMG))
                return 0;

            if (skillStat.CRI < 1)
                return 0;

            float pierce = caster.Stat.GetTotalStatusConvert(eStatusType.PHYS_DMG_PIERCE) + caster.Stat.GetTotalStatusConvert(eStatusType.CRI_DMG_PIERCE);
            var resis = 1f - (target.Stat.GetTotalStatusConvert(eStatusType.CRI_DMG_RESIS) - pierce);
            if (resis < 0f)
                resis = 0f;
            if (resis > 1.0f)
                resis = 1.0f;

            float DMG = caster.Stat.GetStatus(eStatusCategory.BASE, eStatusType.CRI_DMG);
            float ratio = 1.0f + (caster.Stat.GetStatus(eStatusCategory.RATIO, eStatusType.CRI_DMG) + caster.Stat.GetStatus(eStatusCategory.RATIO_BUFF, eStatusType.CRI_DMG)) * SBDefine.CONVERT_FLOAT;
            float added = caster.Stat.GetStatus(eStatusCategory.ADD, eStatusType.CRI_DMG) + caster.Stat.GetStatus(eStatusCategory.ADD_BUFF, eStatusType.CRI_DMG);
            if (Data.BattleType == eBattleType.ARENA || Data.BattleType == eBattleType.ChampionBattle)
            {
                ratio += caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PVP_CRI_DMG);
                added += caster.Stat.GetTotalStatus(eStatusType.ADD_PVP_CRI_DMG);
            }
            var C_DMG = DMG * ratio + added;
            C_DMG = SBFunc.CalcRatio(C_DMG, skillStat.CRI) * resis;
            if (C_DMG < 1)
                C_DMG = 1;
            SaveDamageLog(caster.ID, C_DMG);
            SaveDamageLog(target.ID, C_DMG, true, DMG, C_DMG);
            return Mathf.FloorToInt(C_DMG);
        }
        #endregion
        #region Passive
        protected virtual List<SkillPassiveData> AttackPassive(IBattleCharacterData caster, IBattleCharacterData target, eBattleSkillType skillType, bool isCri)
        {
            if (caster.TranscendenceData.Step > 0 && caster.TranscendenceData.PassiveSlot > 0)
            {
                var res = new List<SkillPassiveData>();
                for (int i = 1, count = caster.TranscendenceData.PassiveSlot; i <= count; ++i)
                {
                    var passive = caster.TranscendenceData.GetPassiveData(i);
                    //패시브 스킬 타입 및 돌릴지 확인
                    if (null == passive)
                        continue;

                    switch (skillType)
                    {
                        case eBattleSkillType.Normal:
                        {
                            if (false == passive.IsPassiveStartSkip(eSkillPassiveStartType.NORMAL_ATTACK) &&
                                false == passive.IsPassiveTargetSkip(caster, target) &&
                                false == passive.IsPassiveContentSkip(Data.BattleType) &&
                                true == passive.IsPassiveRateCheck(caster, target))
                            {
                                res.Add(passive);
                            }
                        } break;
                        case eBattleSkillType.Skill1:
                        {
                            if (false == passive.IsPassiveStartSkip(eSkillPassiveStartType.SKILL_ATTACK) &&
                                false == passive.IsPassiveTargetSkip(caster, target) &&
                                false == passive.IsPassiveContentSkip(Data.BattleType) &&
                                true == passive.IsPassiveRateCheck(caster, target))
                            {
                                res.Add(passive);
                            }
                        } break;
                        default: break;
                    }

                    if(isCri)
                    {
                        if (false == passive.IsPassiveStartSkip(eSkillPassiveStartType.CRITICAL_ATTACK) &&
                            false == passive.IsPassiveTargetSkip(caster, target) &&
                            false == passive.IsPassiveContentSkip(Data.BattleType) &&
                            true == passive.IsPassiveRateCheck(caster, target))
                        {
                            res.Add(passive);
                        }
                    }
                }
                if (res.Count > 0)
                    return res;
            }
            return null;
        }
        protected virtual void AddPassiveHitBuff(AdditionalStatus passiveStatus, SkillPassiveData passive, IBattleCharacterData caster, IBattleCharacterData target, bool isEffect = true)
        {
            var category = eStatusCategory.ADD;
            if (eStatusValueType.PERCENT == passive.EFFECT_VALUE)
                category = eStatusCategory.RATIO;

            passiveStatus.IncreaseStatus(category, passive.STAT, passive.VALUE + (passive.VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT)));

            if (isEffect)
            {
                var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                if (selfEffect != null)
                    CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                if (targetEffect != null)
                    CreateFollowEffect(targetEffect, passive, caster, target, -1f);
            }
        }
        protected virtual SkillPassiveData GetTypePassive(List<SkillPassiveData> passives, eSkillPassiveEffect type)
        {
            SkillPassiveData passive = null;
            for (int p = 0, count = passives.Count; p < count; ++p)
            {
                if (null == passives[p])
                    continue;

                if (passives[p].IsPassiveEffect(type))
                {
                    if (passive == null || passive.VALUE < passives[p].VALUE)
                        passive = passives[p];
                }
            }
            return passive;
        }
        protected virtual SkillPassiveData AbnormalPassive(IBattleCharacterData caster, IBattleCharacterData target)
        {
            SkillPassiveData resPassive = null;
            if (caster.TranscendenceData.Step > 0 && caster.TranscendenceData.PassiveSlot > 0)
            {
                for (int i = 1, count = caster.TranscendenceData.PassiveSlot; i <= count; ++i)
                {
                    var passive = caster.TranscendenceData.GetPassiveData(i);
                    if (null == passive)
                        continue;
                    //패시브 스킬 타입 및 돌릴지 확인
                    if (passive.IsPassiveStartSkip(eSkillPassiveStartType.ABNORMAL_STATUS) ||
                        passive.IsPassiveTargetSkip(caster, target) ||
                        passive.IsPassiveContentSkip(Data.BattleType) ||
                        false == passive.IsPassiveEffect(eSkillPassiveEffect.CC_REFLECT) ||
                        false == passive.IsPassiveRateCheck(caster, target))
                        continue;

                    if (resPassive == null)
                        resPassive = passive;
                    else if (resPassive.VALUE < passive.VALUE)
                        resPassive = passive;
                }
            }
            return resPassive;
        }
        protected virtual void ActiveAbnormalPassive(SkillPassiveData curPassive, IBattleCharacterData caster, IBattleCharacterData target, EffectInfo effectInfo, SkillEffectData effect, int skillLevel)
        {
            effectInfo.SetPassiveData(caster, target, curPassive, effect, skillLevel);
            if (target.SetEffectInfo(effectInfo))
                effectInfo.SetFollowEffect(CreateFollowEffect(SkillResourceData.Get(effect.TARGET_EFFECT_RSC_KEY), effect, caster, target, effectInfo.MaxTime));

            var selfEffect = SkillResourceData.Get(curPassive.SELF_EFFECT_RESOURCE);
            if (selfEffect != null)
                CreateFollowEffect(selfEffect, curPassive, caster, caster, effectInfo.MaxTime);
            var targetEffect = SkillResourceData.Get(curPassive.TARGET_EFFECT_RESOURCE);
            if (targetEffect != null)
                CreateFollowEffect(targetEffect, curPassive, caster, target, effectInfo.MaxTime);
        }
        protected virtual void ActivePassiveDamage(IBattleCharacterData caster, IBattleCharacterData target, int DMG)
        {
            if (caster.TranscendenceData.Step > 0 && caster.TranscendenceData.PassiveSlot > 0)
            {
                for (int i = 1, count = caster.TranscendenceData.PassiveSlot; i <= count; ++i)
                {
                    var passive = caster.TranscendenceData.GetPassiveData(i);
                    if (null == passive)
                        continue;
                    var passiveTarget = passive.IsPassiveSelf() ? caster : target;
                    //패시브 스킬 타입 및 돌릴지 확인
                    if (passive.IsPassiveStartSkip(eSkillPassiveStartType.HIT) ||
                        passive.IsPassiveTargetSkip(caster, passiveTarget) ||
                        passive.IsPassiveContentSkip(Data.BattleType) || 
                        passive.IsPassiveEffect(eSkillPassiveEffect.REDUCE_BUFF, eSkillPassiveEffect.REDUCE_DEBUFF))
                        continue;

                    if (passive.IsPassiveRateCheck(caster, target))
                    {
                        ActivePassiveEffect(passive, caster, passiveTarget, DMG);
                    }
                }
            }
        }
        protected virtual void ActivePassiveEffect(SkillPassiveData passive, IBattleCharacterData caster, IBattleCharacterData target, int DMG, bool isEffect = true)
        {
            switch (passive.PASSIVE_EFFECT)
            {
                //시작 전에 스텟에서 계산
                //case eSkillPassiveEffect.STAT:
                //case eSkillPassiveEffect.STAT_MAIN_ELEMENT:
                //    break;
                //스킬 사용에 따른 자리선정 필요
                //case eSkillPassiveEffect.HIT:
                //case eSkillPassiveEffect.STRONG_BUFF:
                //case eSkillPassiveEffect.STRONG_DEBUFF:
                //case eSkillPassiveEffect.REDUCE_COOLTIME:
                //case eSkillPassiveEffect.CC_REFLECT:
                //    break;

                //적군 버프시간 감소
                case eSkillPassiveEffect.REDUCE_BUFF:
                {
                    if (isEffect)
                    {
                        target.ReduceEffectInfo(eSkillEffectType.BUFF, passive.CONVERT_VALUE + (passive.CONVERT_VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT)));
                        target.ReduceEffectInfo(eSkillEffectType.BUFF_MAIN_ELEMENT, passive.CONVERT_VALUE + (passive.CONVERT_VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT)));
                        target.ReduceEffectInfo(eSkillEffectType.SHIELD, passive.CONVERT_VALUE + (passive.CONVERT_VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT)));
                        var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                        if (selfEffect != null)
                            CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                        var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                        if (targetEffect != null)
                            CreateFollowEffect(targetEffect, passive, caster, target, -1f);
                    }
                } break;
                //아군 디버프 시간 감소
                case eSkillPassiveEffect.REDUCE_DEBUFF:
                {
                    if (isEffect)
                    {
                        target.ReduceEffectInfo(eSkillEffectType.DEBUFF, passive.CONVERT_VALUE + (passive.CONVERT_VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT)));
                        target.ReduceEffectInfo(eSkillEffectType.AGGRO_R, passive.CONVERT_VALUE + (passive.CONVERT_VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT)));
                        var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                        if (selfEffect != null)
                            CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                        var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                        if (targetEffect != null)
                            CreateFollowEffect(targetEffect, passive, caster, target, -1f);
                    }
                } break;
                //남은 스킬 쿨타임 즉시감소
                case eSkillPassiveEffect.REDUCE_COOLTIME:
                {
                    if (isEffect)
                    {
                        if (passive.EFFECT_VALUE == eStatusValueType.PERCENT)
                            target.SetSkill1Delay(target.Skill1Delay - target.Skill1Delay * (passive.CONVERT_VALUE + (passive.CONVERT_VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT))));
                        else
                            target.SetSkill1Delay(target.Skill1Delay - (passive.VALUE + (passive.VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT))));

                        var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                        if (selfEffect != null)
                            CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                        var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                        if (targetEffect != null)
                            CreateFollowEffect(targetEffect, passive, caster, target, -1f);
                    }
                } break;
                //대미지 반사
                case eSkillPassiveEffect.DMG_REFLECT:
                {
                    var passiveDMG = Mathf.FloorToInt(DMG * (passive.CONVERT_VALUE + (passive.CONVERT_VALUE * caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT))));
                    UIDamageEvent.Send(passiveDMG, eDamageType.ELEMENT_NORMAL, target.Transform, new Vector3(-target.ConvertPos(damageX), damageY), caster.IsEnemy, false);
                    SetDamage(caster, target, passiveDMG, true, true);

                    var selfEffect = SkillResourceData.Get(passive.SELF_EFFECT_RESOURCE);
                    if (selfEffect != null)
                        CreateFollowEffect(selfEffect, passive, caster, caster, -1f);
                    var targetEffect = SkillResourceData.Get(passive.TARGET_EFFECT_RESOURCE);
                    if (targetEffect != null)
                        CreateFollowEffect(targetEffect, passive, caster, target, -1f);
                } break;
                case eSkillPassiveEffect.DEBUFF:                //디버프
                case eSkillPassiveEffect.BUFF:                  //버프
                case eSkillPassiveEffect.BUFF_MAIN_ELEMENT:     //자기속성 강화버프
                case eSkillPassiveEffect.SILENCE:               //침묵
                case eSkillPassiveEffect.R_KNOCK_BACK:          //긴급탈출
                    SetPassiveEffectInfo(caster, target, passive, isEffect);
                    break;
                default: break;
            }
        }

        bool IsMiss(IBattleCharacterData caster, IBattleCharacterData target)
        {
            const float base_pivot = 100f;
            float rate = (caster.Stat.GetTotalStatus(eStatusType.ACCURACY) - target.Stat.GetTotalStatus(eStatusType.EVASION));

            if (rate >= 0)
                return false;

            if (rate <= base_pivot * -1f)
                return true;

            return RandomVal() > Mathf.Clamp01((base_pivot + rate) / base_pivot);
        }
        #endregion
    }

}