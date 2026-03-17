using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DailyStateMonsterMove : DailyState
    {
        bool moving = false;
        bool dmoving = false;
        float multi = 1f;
        public float moveDuration = 5f;
        public float moveStartSpeed = 4f;
        public Ease ease = Ease.OutCubic;

        float startOffset = 0;
        private SBController dragonBeacon = null;
        private SBController monsterBeacon = null;

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                dragonBeacon = Stage.Map.OffenseBeacon;
                monsterBeacon = Stage.Map.DefenseBeacon;

                var spinesList = Stage.DefenseSpines;
                spinesList.Clear();
                var dics = Data.DefenseDic;
                var it = Data.DefensePos.GetEnumerator();
                IBattleCharacterData bossData = null;

                float camOffset = Camera.main.transform.position.x;

                // 해상도에 따른 몬스터 생성 오프셋 생성
                var proCamera = Camera.main;
                ProCamera2DZoomToFitTargets proCameraZoomToFit = null;
                if (proCamera != null)
                {
                    proCameraZoomToFit = proCamera.GetComponent<ProCamera2DZoomToFitTargets>();

                    if (proCameraZoomToFit != null)
                    {
                        startOffset = proCameraZoomToFit.offsetX + 1;
                    }
                }
                monsterBeacon.transform.localPosition = GetMonsterStartFieldPosition(camOffset, startOffset);

                while (it.MoveNext())
                {
                    var x = it.Current.Key;
                    var list = it.Current.Value;

                    var spineList = new List<BattleSpine>();
                    for (int y = 0, count = list.Count; y < count; ++y)
                    {
                        var curPos = list[y];

                        if (!dics.ContainsKey(curPos.BattleTag))
                            continue;

                        var enemySpine = Stage.GetDefenseSpine(dics[curPos.BattleTag]) as BattleMonsterSpine;
                        Vector3 pos = GetMonsterStartPosition(x, y, count, 0f, startOffset);

                        enemySpine.transform.localPosition = pos;
                        enemySpine.OffDust();
                        enemySpine.SetRigidbodySimulated(false);

                        spineList.Add(enemySpine);

                        Stage.SetRedHpBar(enemySpine.Data);

                        if (enemySpine.IsBoss)
                            bossData = enemySpine.CData;
                    }
                    spinesList.Add(spineList);
                }

                multi = moveStartSpeed;

                DOTween.To(x => multi = x, moveStartSpeed, 1f, moveDuration).SetEase(ease).Play();
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
                {
                    var list = Stage.OffenseSpines[x];
                    if (list == null)
                        continue;

                    for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                    {
                        var curDragon = list[y];
                        if (curDragon == null || curDragon.Data.Death)
                            continue;

                        curDragon.SetSpeed(1f);
                        curDragon.OffDust();
                        ResetSinusoidalCharacter(curDragon.Data);
                    }
                }

                for (int x = 0, count = Stage.DefenseSpines.Count; x < count; ++x)
                {
                    var list = Stage.DefenseSpines[x];
                    if (list == null)
                        continue;

                    for (int y = 0, yCount = list.Count; y < yCount; ++y)//몬스터 정위치 이동
                    {
                        var curEnemy = list[y];
                        if (curEnemy == null || curEnemy.Data.Death)
                            continue;

                        curEnemy.SetSpeed(1f);
                    }
                }

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

                float ease = dt * multi;

                dmoving = false;
                bool alive = false;
                float camOffset = Camera.main.transform.position.x;

                var dragonFieldPos = GetOffenseEndFieldPosition(camOffset);
                float dragonFieldDist = Vector3.Distance(dragonBeacon.transform.position, dragonFieldPos);
                if (dragonFieldDist > 0.05f)
                {
                    dragonBeacon.MoveWorldTargetUpdate(dt, dragonFieldPos, false, multi * 100f);
                    dmoving = true;
                }

                for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
                {
                    var list = Stage.OffenseSpines[x];
                    if (list == null)
                        continue;

                    for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                    {
                        var curDragon = list[y];
                        if (curDragon == null || curDragon.Data.Death)
                            continue;

                        alive = true;

                        curDragon.SetAnimation(eSpineAnimation.WALK);
                        curDragon.UpdateStatus(dt);
                        UpdateSinusoidalCharacter(dt, curDragon.Data);
                    }
                }
                if (!alive)
                {
                    return false;
                }

                Stage.Map.UpdateLeft(ease + (dmoving ? dt : 0f));

                if (!dmoving)
                {
                    moving = true;

                    var enemyFieldPos = GetMonsterEndFieldPosition(camOffset);
                    float enemyFieldDist = Vector3.Distance(monsterBeacon.transform.position, enemyFieldPos);
                    if (enemyFieldDist > 0.05f)
                    {
                        float tempSpeed = startOffset * 0.1f;
                        tempSpeed = tempSpeed < 0 ? 0 : tempSpeed;
                        monsterBeacon.MoveLocalTargetUpdate(dt, enemyFieldPos, false, (multi + tempSpeed) * 200f);
                    }
                    else
                    {
                        moving = false;
                    }

                    for (int x = 0, count = Stage.DefenseSpines.Count; x < count; ++x)
                    {
                        var list = Stage.DefenseSpines[x];
                        if (list == null)
                            continue;

                        for (int y = 0, yCount = list.Count; y < yCount; ++y)//몬스터 정위치 이동
                        {
                            var curEnemy = list[y];
                            if (curEnemy == null || curEnemy.Data.Death)
                                continue;

                            curEnemy.UpdateStatus(dt);
                            if (moving)
                            {
                                curEnemy.SetSpeed(multi);
                                curEnemy.SetAnimation(eSpineAnimation.WALK);
                            }
                            else
                            {
                                curEnemy.SetSpeed(1f);
                            }
                        }
                    }
                }

                for (int x = 0, count = events.Count; x < count; ++x)
                {
                    var eventTarget = events[x];
                    if (eventTarget == null)
                        continue;

                    eventTarget.Translate(new Vector3(-ease - (dmoving ? dt : 0f), 0));
                }

                return (moving || dmoving);
            }
            return !IsPlaying;
        }
    }
}