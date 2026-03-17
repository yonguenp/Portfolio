using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SandboxNetwork
{
    public class ArenaColosseumBattle : BattleStateLogic
    {
        private int TimeBuffCount = 0;
        private List<CameraTarget> tempInfluence = null;
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
                        character.GetComponent<Collider2D>().enabled = true;
                        character.StopDust();
                        character.SetRigidbodySimulated(true);
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
                        character.GetComponent<Collider2D>().enabled = true;
                        character.StopDust();
                        character.SetRigidbodySimulated(true);
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
                    ArenaManager.Instance.ColosseumData.SetWinType(eArenaWinType.Offense);
                else if (offenseDeath == offenses.Count)
                    ArenaManager.Instance.ColosseumData.SetWinType(eArenaWinType.Defense);

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
            return !IsPlaying;
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
        //아레나 움직임 제한 제거 2024-01-03
        //protected override Vector3 GetEnemyOffsetVector(IBattleCharacterData mine)
        //{
        //    var ret = base.GetEnemyOffsetVector(mine);
        //    var myCollider = mine.GetCircleCollider();
        //    var frontChar = GetFrontPos(mine, GetTeam(mine.IsEnemy ? eBattleSide.OffenseSide : eBattleSide.DefenseSide));
        //    if (frontChar != null)
        //    {
        //        var front = frontChar.Data.GetCircleCollider();
        //        var frontCenter = front.bounds.center;
        //        if (frontChar.Data.IsEnemy)
        //        {
        //            var diff = frontCenter.x - 0.1f;
        //            if (myCollider.bounds.center.x >= diff)
        //            {
        //                ret.x = diff - myCollider.bounds.center.x;
        //            }
        //        }
        //        else
        //        {
        //            var diff = frontCenter.x + 0.1f;
        //            if (myCollider.bounds.center.x <= diff)
        //            {
        //                ret.x = diff - myCollider.bounds.center.x;
        //            }
        //        }
        //    }
        //    return ret;
        //}
        //protected override Vector3 GetSpaceOffsetVector(IBattleCharacterData mine, bool both = true)
        //{
        //    var ret = base.GetSpaceOffsetVector(mine);
        //    var myCollider = mine.GetCircleCollider();
        //    var frontChar = GetFrontPos(mine, GetTeam(mine.IsEnemy ? eBattleSide.OffenseSide : eBattleSide.DefenseSide));
        //    if (frontChar != null)
        //    {
        //        var front = frontChar.Data.GetCircleCollider();
        //        var frontCenter = front.bounds.center;
        //        if (frontChar.Data.IsEnemy)
        //        {
        //            var diff = frontCenter.x - 0.1f;
        //            if (myCollider.bounds.center.x >= diff)
        //            {
        //                ret.x = diff - myCollider.bounds.center.x;
        //            }
        //        }
        //        else
        //        {
        //            var diff = frontCenter.x + 0.1f;
        //            if (myCollider.bounds.center.x <= diff)
        //            {
        //                ret.x = diff - myCollider.bounds.center.x;
        //            }
        //        }
        //    }
        //    return ret;
        //}
    }
}