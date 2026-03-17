using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct BossDeathEvent
    {
        public static BossDeathEvent e;
        public static void Send()
        {
            EventManager.TriggerEvent(e);
        }
    }
    public class AdventureStateBattle : AdventureState, EventListener<BossDeathEvent>
    {
        private List<CameraTarget> tempInfluence = null;
        private bool isBossDeath = false;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                EventManager.AddListener<BossDeathEvent>(this);

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
                        targets.Add(ProCamera2D.Instance.AddCameraTarget(character.transform, 0, 0));
                        character.StopDust();
                    }
                }

                targets = GetCameraTargets();

                isBossDeath = false;

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
                EventManager.RemoveListener<BossDeathEvent>(this);
                isBossDeath = false;

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
                for (int i = 0, count = offenses.Count; i < count; ++i)
                {
                    var character = offenses[i];
                    if (character == null)
                        continue;
                    character.UpdateStatus(dt);
                    CharacterAction(character.Data, dt);

                    if (Data.IsAuto)
                        SkillCast(character.Data);
                }
                for (int i = 0, count = defenses.Count; i < count; ++i)
                {
                    var character = defenses[i];
                    if (character == null)
                        continue;
                    character.UpdateStatus(dt);
                    CharacterAction(character.Data, dt);

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
        protected override bool DeathCheck()
        {
            if (isBossDeath)
            {
                foreach (var defense in defenses)
                {
                    if (defense == null || defense.Data == null || defense.Data.Death)
                        continue;

                    SetDamage(defense.Data, defense.Data, defense.Data.HP, false);
                }
            }
            return base.DeathCheck();
        }
        public virtual void OnEvent(BossDeathEvent e)
        {
            isBossDeath = true;
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