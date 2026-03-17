using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossStateBattle : WorldBossState
    {
        private List<CameraTarget> tempInfluence = null;
        BattleWorldBossData bossData = null;
        WorldBossStage BossStage { get { return Stage as WorldBossStage; } }
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                offenses.Clear();
                defenses.Clear();
                targets.Clear();
                soundDelays.Clear();
                projectiles.Clear();

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
                        character.SetDefaultSpeed(1f);
                        offenses.Add(character);
                        character.GetComponent<Collider2D>().enabled = true;
                        targets.Add(ProCamera2D.Instance.AddCameraTarget(character.transform, 0, 0));
                        character.StopDust();
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
                        character.SetDefaultSpeed(1f);
                        //targets.Add(ProCamera2D.Instance.AddCameraTarget(character.transform, 0, 0));
                        character.StopDust();
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


                bossData = ((WorldBossBattleData)Data).BossData;
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

                for (int i = 0, count = offenses.Count; i < count; ++i)
                {
                    if (offenses[i] == null)
                        continue;

                    if (offenses[i].Data.Death)
                        continue;

                    offenses[i].ClearAnimation();
                    offenses[i].ClearEffectSpine();
                    offenses[i].Data.SetActionCoroutine(null);
                }

                EffectReceiverClearEvent.Send();
                ProCamera2D.Instance.RemoveAllCameraTargets();

                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                Data.Time += dt;
                if (Data.CheckTimeOver())
                    return false;

                //전투 로직
                for (int i = 0; i < offenses.Count; ++i)
                {
                    var character = offenses[i];
                    if (character == null)
                        continue;
                    character.UpdateStatus(dt);
                    CharacterAction(character.Data, dt);

                    if (Data.IsAuto)
                        SkillCast(character.Data);
                }
                for (int i = 0; i < defenses.Count; ++i)
                {
                    var character = defenses[i];
                    if (character == null)
                        continue;
                    character.UpdateStatus(dt);                    
                    CharacterAction(character.Data, dt);

                    SkillCast(character.Data);
                }

                BossPartsAction(dt);

                //

                UpdateProjectile(dt);//투사채 처리
                UpdateSounds(dt);//사운드 재생
                CharacterSKill();//스킬 처리

                bool deathCheck = DeathCheck();
                if (false && targets != null)
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
                        if (targets[t].TargetTransform != null)
                        {
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
            return !IsPlaying;
        }
        protected override bool DeathCheck()
        {
            if (offenseDeath >= offenses.Count)
            {
                targets = GetCameraTargets();
                return false;
            }

            return true;
        }
        protected override eBattleSide GetSide(IBattleCharacterData aData)
        {
            if(aData.IsEnemy)
                return eBattleSide.DefenseSide_1;
            else if(aData is WorldBossBattleDragonData dData)
            {
                switch (dData.PartyIndex)
                {
                    case 1:
                        return eBattleSide.OffenseSide_2;
                    case 2:
                        return eBattleSide.OffenseSide_3;
                    case 3:
                        return eBattleSide.OffenseSide_4;
                    case 0:
                    default:
                        return eBattleSide.OffenseSide_1;
                }
            }
            else
                return eBattleSide.OffenseSide_1;
        }
        protected override void CharacterSKill()
        {
            if (!Data.IsAuto && Data.SelectSkillCharacter != null)
            {
                SkillCast(Data.SelectSkillCharacter);
                Data.SetSelectSkillCharacter(null);
            }
            Data.SkillQueueSortCast(eBattleSide.OffenseSide_1);
            Data.SkillQueueSortCast(eBattleSide.OffenseSide_2);
            Data.SkillQueueSortCast(eBattleSide.OffenseSide_3);
            Data.SkillQueueSortCast(eBattleSide.OffenseSide_4);
            Data.SkillQueueSortCast(eBattleSide.DefenseSide_1);
        }

        protected override bool IsCastContain(SBObject obj, Vector3 position, Vector3 tp, CircleCollider2D castCol, CircleCollider2D targetCol)
        {
            if (targetCol == null)
            {
                return obj.IsContain(tp);
            }

            return obj.IsContain(targetCol.bounds.center, targetCol.radius * Mathf.Min(targetCol.transform.lossyScale.x, targetCol.transform.lossyScale.y));
        }

        protected override Vector3 GetEnemyOffsetVector(IBattleCharacterData mine)
        {
            return Vector3.zero;
        }

        protected override Circle GetCastCircle(IBattleCharacterData caster, SBSkill skill, Vector2 addedRange = default)
        {
            if (circle == null)
                circle = new(caster.Transform.position, skill.Skill.RANGE_X + addedRange.x, skill.Skill.RANGE_Y + addedRange.y);
            else
            {
                circle.SetPosition(caster.Transform.position);
                circle.SetDirection(eDirectionBit.None);
                circle.SetEllipse(skill.Skill.RANGE_X + addedRange.x, skill.Skill.RANGE_Y + addedRange.y);
            }

            var circleCollider = caster.GetCircleCollider();
            if (circleCollider != null)
                circle.AddRadius(circleCollider.radius);

            return circle;
        }
        protected override SBObject GetSummonRange(IBattleCharacterData caster, SkillSummonData summon)
        {
            Vector3 position = caster.Transform.position;
            var RangeX = summon.RANGE_X;
            var RangeY = summon.RANGE_Y;
            SBObject sbObj = null;

            switch (summon.RANGE_TYPE)
            {
                case eSkillRangeType.CIRCLE_F:
                {
                    circle.SetPosition(position);
                    circle.SetDirection(GetAttackDirection(caster));
                    circle.SetEllipse(RangeX, RangeY);
                    sbObj = circle;
                } break;
                case eSkillRangeType.SQUARE_F:
                case eSkillRangeType.SQUARE_C:
                    rect.SetPosition(position);
                    rect.SetDirection(eDirectionBit.None);
                    rect.SetEllipse(RangeX, RangeY);
                    sbObj = rect;
                    break;
                //case eSkillRangeType.SQUARE_F:
                //{
                //    rect.SetPosition(position);
                //    rect.SetDirection(GetTargetDirection(caster));
                //    rect.SetEllipse(RangeX, RangeY);
                //    sbObj = rect;
                //} break;
                case eSkillRangeType.SECTOR_F:
                {
                    cone.SetPosition(position);
                    cone.SetDirection(GetAttackDirection(caster));
                    cone.SetCone(RangeX, RangeY);
                    sbObj = cone;
                } break;
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

            var circleCollider = caster.GetCircleCollider();
            if (circleCollider != null)
                circle.AddRadius(circleCollider.radius);

            return sbObj;
        }
        protected override eDirectionBit GetAttackDirection(IBattleCharacterData caster)
        {
            if (caster is WorldBossBattleDragonData dragon)
            {
                return dragon.PartyDirection switch
                {
                    eDirectionBit.Left => !caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right,
                    _ => caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right
                };
            }
            return caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right;
        }
        protected override Vector3 GetAddedPosition(IBattleCharacterData targetData, SkillSummonData data)
        {
            if (targetData == null || data == null)
                return Vector3.zero;

            return GetAttackDirection(targetData) switch
            {
                eDirectionBit.Left => new Vector3(targetData.ConvertPos(-data.POSITION_X), data.POSITION_Y) * SBDefine.CONVERT_FLOAT,
                _ => new Vector3(targetData.ConvertPos(data.POSITION_X), data.POSITION_Y) * SBDefine.CONVERT_FLOAT
            };
        }

        protected override List<BattleSpine> GetTargetList(IBattleCharacterData caster, eSkillTargetType type)
        {
            //파티 드래곤 전용
            var list = new List<BattleSpine>();
            
            if (caster.IsEnemy)
            {
                WorldBossPartData partInfo = WorldBossPartData.Get(caster.ID);
                if (partInfo != null)
                {
                    var targetPartyDragons = partInfo.GetTargets(Stage.OffenseSpines);
                    switch (type)
                    {
                        case eSkillTargetType.ALL:
                            list.Add(bossData.GetSpine());
                            foreach (var d in defenses)
                            {
                                WorldBossPartData pd = WorldBossPartData.Get(d.Data.ID);
                                if (pd == null)
                                    continue;

                                if ((partInfo.ATTACK_PRIORITY & pd.TARGET_PRIORITY) > 0)
                                    list.Add(d);
                            }

                            if (!list.Contains(caster.GetSpine()))
                                list.Add(caster.GetSpine());

                            list.AddRange(targetPartyDragons);                            
                            break;
                        case eSkillTargetType.ALLY:
                            list.Add(bossData.GetSpine());
                            foreach (var d in defenses)
                            {
                                WorldBossPartData pd = WorldBossPartData.Get(d.Data.ID);
                                if (pd == null)
                                    continue;

                                if ((partInfo.ATTACK_PRIORITY & pd.TARGET_PRIORITY) > 0)
                                    list.Add(d);
                            }

                            if (!list.Contains(caster.GetSpine()))
                                list.Add(caster.GetSpine());

                            list.Remove(caster.GetSpine());
                            break;
                        case eSkillTargetType.FRIENDLY:
                            list.Add(bossData.GetSpine());
                            foreach (var d in defenses)
                            {
                                WorldBossPartData pd = WorldBossPartData.Get(d.Data.ID);
                                if (pd == null)
                                    continue;

                                if ((partInfo.ATTACK_PRIORITY & pd.TARGET_PRIORITY) > 0)
                                    list.Add(d);
                            }

                            if (!list.Contains(caster.GetSpine()))
                                list.Add(caster.GetSpine());
                            break;
                        case eSkillTargetType.SELF:
                            list.Add(caster.GetSpine());
                            break;
                        case eSkillTargetType.ENEMY:
                        default:
                            list.AddRange(targetPartyDragons);
                            break;
                    }
                    list.RemoveAll((spine) =>
                    {
                        if (spine == null || spine.Data.Death || spine.Data.IsTargetSkip())
                            return true;

                        return false;
                    });
                }
                else
                {
                    list = base.GetTargetList(caster, type);
                }
                return list;
            }


            WorldBossBattleDragonData dragon = (WorldBossBattleDragonData)caster;
            switch (type)
            {
                case eSkillTargetType.ALL:
                    if (dragon != null)
                    {
                        int partyIndex = dragon.PartyIndex;
                        foreach (var d in defenses)
                        {
                            WorldBossPartData PartData = WorldBossPartData.Get(d.Data.ID);
                            if (PartData == null)
                                continue;

                            var targetIndexs = PartData.GetAttackPartyIndexs();
                            if (targetIndexs.Contains(partyIndex))
                            {
                                list.Add(d);
                            }
                        }
                    }
                    list.AddRange(GetSamePartyDragons(caster.GetSpine()));
                    break;
                case eSkillTargetType.ALLY:
                    list.AddRange(GetSamePartyDragons(caster.GetSpine()));
                    list.Remove(caster.GetSpine());
                    break;
                case eSkillTargetType.FRIENDLY:
                    list.AddRange(GetSamePartyDragons(caster.GetSpine()));
                    break;
                case eSkillTargetType.SELF:
                    list.Add(caster.GetSpine());
                    break;
                case eSkillTargetType.ENEMY:
                default:                    
                    if (dragon != null)
                    {
                        int partyIndex = dragon.PartyIndex;
                        foreach (var d in defenses)
                        {
                            WorldBossPartData PartData = WorldBossPartData.Get(d.Data.ID);
                            if (PartData == null)
                                continue;

                            var targetIndexs = PartData.GetAttackPartyIndexs();
                            if (targetIndexs.Contains(partyIndex))
                            {
                                list.Add(d);
                            }
                        }

                        if (bossData != null && dragon != null)
                        {
                            foreach (var part in bossData.ActiveParts)
                            {
                                if (part == null)
                                    continue;

                                if (part.PartsSpine.IsScarecrow())
                                {
                                    var targetIndexs = part.PartsSpine.PartData.GetAttackPartyIndexs();
                                    if (targetIndexs.Contains(partyIndex))
                                    {
                                        list.Add(part.PartsSpine);
                                    }
                                }
                            }
                        }
                    }
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

        private List<BattleSpine> GetSamePartyDragons(BattleSpine target)
        {
            for (int i = 0; i < Stage.OffenseSpines.Count; i++)
            {
                foreach (var spine in Stage.OffenseSpines[i])
                {
                    if (spine != null)
                    {
                        if (spine == target)
                        {
                            return Stage.OffenseSpines[i];
                        }
                    }
                }
            }

            return null;
        }

        protected override void OnUISkillEffect(int id, eSpineAnimation skill, float v, BattleSpine caster)
        {
            //스킬컷신제거
            //UISkillCutSceneEvent.Send(id, skill, v, caster);
        }

        void BossPartsAction(float dt)
        {
            if(bossData != null && bossData.ActiveParts != null)
            {
                for (int i = bossData.ActiveParts.Count - 1; i >= 0; i--)
                {
                    var part = bossData.ActiveParts[i];
                    part.Update(dt);
                    CharacterAction(part, dt);
                    SkillCast(part);
                }
            }
        }

        protected override void SetDamage(IBattleCharacterData caster, IBattleCharacterData target, int DMG, bool isSkill, bool isPassive = false)
        {
            if (false == DeathCheck() || Data.CheckTimeOver())
                return;

            base.SetDamage(caster, target, DMG, isSkill, isPassive);
        }

        protected override void SpecialTrigger(IBattleCharacterData casterData, SBSkill skill, int idx)
        {
            if (casterData.IsEnemy)
            {
                int num = casterData.BaseData.KEY;
                if (num <= 0)
                    return;

                switch (num % 100)
                {
                    case 0:
                    {
                        if (skill == null)
                            return;

                        var summon = skill.GetSummon(idx);
                        if (summon == null)
                            return;

                        var effects = skill.GetEffect(idx);
                        if (effects == null)
                            return;

                        var small_eff = SkillEffectData.GetGroup(501010021);
                        if (small_eff == null)
                            return;

                        foreach (var space in WorldBossStage.Instance.GetSpaceInfoList())
                        {
                            if (space.dangerEffectKey > 0)
                                CreateFieldEffect(SkillResourceData.Get(space.dangerEffectKey), casterData, space.dangerSpace.transform.position + new Vector3(space.dangerSpace.offset.x, space.dangerSpace.offset.y, 0), Vector2.zero, skill.TargetScale);
                            if (space.weakEffectKey > 0)
                                CreateFieldEffect(SkillResourceData.Get(space.weakEffectKey), casterData, space.weakSpace.position, Vector2.zero, skill.TargetScale);
                        }

                        var summonList = CheckSummon(casterData, summon);
                        foreach (var target in summonList)
                        {
                            if (WorldBossStage.Instance.IsOnDangerSpace(target))
                            {
                                for (int j = 0, jCount = effects.Count; j < jCount; ++j)
                                {
                                    var effect = effects[j];
                                    if (effect == null)
                                        continue;

                                    EffectTrigger(casterData, target.Data, effect, skill);
                                }
                            }
                            else
                            {
                                for (int j = 0, jCount = small_eff.Count; j < jCount; ++j)
                                {
                                    var effect = small_eff[j];
                                    if (effect == null)
                                        continue;

                                    EffectTrigger(casterData, target.Data, effect, skill);
                                }
                            }
                        }
                    }
                    break;
                    case 9://왼쪽 레이져
                    case 8://오른쪽 레이져
                    case 10://강화 왼쪽 레이져
                    case 11://강화 오른쪽 레이져
                    {
                        BattleWorldBossPartsData battlePart = casterData as BattleWorldBossPartsData;
                        if (battlePart != null)
                        {
                            var partSpine = battlePart.GetSpine() as BattleWorldBossPartsSpine;
                            if (partSpine != null)
                            {
                                battlePart.OnSpecialSkill(partSpine.PartData.GetTargets(Stage.OffenseSpines), skill, EffectTriggerTarget);
                            }
                        }
                    }
                    break;
                }
            }
        }
        protected override Vector3 GetSpaceOffsetVector(IBattleCharacterData mine, bool both = true)
        {
            if (mine is BattleWorldBossSummonMonsterData)
                return Vector3.zero;

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

                        if (center.y >= 0f)
                            ret.y += SBFunc.Random(0.1f, 0.5f);
                        else
                            ret.y -= SBFunc.Random(0.1f, 0.5f);
                    }
                    else
                    {
                        if (mine.IsEnemy == checker.Data.IsEnemy)
                            ret += (center - colCenter) * 0.5f;
                        else
                            ret += (center - colCenter) * 0.6f;
                    }

                    isContain = true;
                }
            }

            if (!mine.IsEnemy)
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

                        if (center.y >= 0f)
                            ret.y += SBFunc.Random(0.1f, 0.5f);
                        else
                            ret.y -= SBFunc.Random(0.1f, 0.5f);
                    }
                    else
                    {
                        if (mine.IsEnemy == checker.Data.IsEnemy)
                            ret += (center - colCenter) * 0.5f;
                        else
                            ret += (center - colCenter) * 0.6f;
                    }
                    isContain = true;
                }
            }

            if (isContain)
                return OffsetCheck(mine, ret);
            else
                return Vector3.zero;
        }
        protected override bool WallCheck(IBattleCharacterData aData, float dt)
        {
            if (aData is BattleWorldBossSummonMonsterData)
                return false;

            return base.WallCheck(aData, dt);
        }

        protected override IEnumerator NormalCoroutine(IBattleCharacterData casterData, SBSkill skill)
        {
            if (casterData.IsEnemy)
            {
                while (WorldBossStage.Instance.IsSummoningParts(casterData))
                    yield return new WaitForEndOfFrame();

                var summon = skill.GetSummon(0);
                if (summon != null)
                {
                    switch (summon.TYPE)
                    {
                        case eSkillSummonType.IMMEDIATELY:
                            while (GetSkillCharList(casterData, skill).Count <= 0)
                                yield return new WaitForEndOfFrame();
                            break;
                        case eSkillSummonType.SUMMON:
                            WorldBossPartData partInfo = WorldBossPartData.Get((int)summon.EFFECT_GROUP_KEY);
                            if (partInfo != null)
                            {
                                var targets = partInfo.GetTargets(WorldBossStage.Instance.OffenseSpines);
                                while (targets == null || targets.Count <= 0)
                                    yield return new WaitForEndOfFrame();
                            }
                            break;
                    }
                }
            }

            yield return base.NormalCoroutine(casterData, skill);
        }

        protected override IEnumerator SkillCoroutine(IBattleCharacterData casterData, SBSkill skill)
        {
            if (casterData.IsEnemy)
            {
                while (WorldBossStage.Instance.IsSummoningParts(casterData))
                    yield return new WaitForEndOfFrame();

                var summon = skill.GetSummon(0);
                if (summon != null)
                {
                    switch (summon.TYPE)
                    {
                        case eSkillSummonType.IMMEDIATELY:
                            while (GetSkillCharList(casterData, skill).Count <= 0)
                                yield return new WaitForEndOfFrame();
                            break;
                        case eSkillSummonType.SUMMON:
                            WorldBossPartData partInfo = WorldBossPartData.Get((int)summon.EFFECT_GROUP_KEY);
                            if (partInfo != null)
                            {
                                var targets = partInfo.GetTargets(WorldBossStage.Instance.OffenseSpines);
                                while (targets == null || targets.Count <= 0)
                                    yield return new WaitForEndOfFrame();
                            }
                            break;
                    }
                }
            }

            yield return base.SkillCoroutine(casterData, skill);
        }
        public override bool Destroy()
        {
            if (base.Destroy())
            {
                tempInfluence = null;
                bossData = null;
                return true;
            }
            return false;
        }
    }
}