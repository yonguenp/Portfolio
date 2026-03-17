using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

namespace SandboxNetwork
{
    public class AdventureStateDragonMove : BattleStateLogic, EventListener<AdventureStateEvent>, EventListener<UIBattleBossEndEvent>
    {
        bool isEffect = false;
        bool animEnd = false;
        private bool moving = false;
        private float minDel = 1.5f;
        private float speedRate = 2.3f;
        private float stageRate = 1.5f;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                Transform frontDragon = null;
                EventManager.AddListener<UIBattleBossEndEvent>(this);
                EventManager.AddListener<AdventureStateEvent>(this);
                for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
                {
                    var list = Stage.OffenseSpines[x];
                    if (list == null)
                        continue;

                    for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                    {
                        var curDragon = list[y];
                        if (curDragon == null || curDragon.Data.Death || y >= 2)
                            continue;

                        if (frontDragon == null)
                            frontDragon = curDragon.transform;
                        else if(frontDragon.position.x < curDragon.transform.position.x)
                            frontDragon = curDragon.transform;

                        var local = curDragon.transform.localScale;
                        local.x = Mathf.Abs(local.x);
                        curDragon.transform.localScale = local;
                        curDragon.Controller.StopCO();
                        curDragon.SetDefaultSpeed(speedRate);

                        curDragon.SetRigidbodySimulated(false);
                        if (frontDragon != null)
                            curDragon.transform.SetParent(Stage.Map.Beacon, true);
                    }
                }
                if (ProCamera2D.Instance.CameraTargets != null)
                {
                    ProCamera2D.Instance.CameraTargets.Clear();
                    ProCamera2D.Instance.AddCameraTarget(ProCamera2D.Instance.transform);
                }

                IBattleCharacterData bossData = null;
                var it = Data.DefensePos.GetEnumerator();
                var dics = Data.DefenseDic;

                while (it.MoveNext())
                {
                    var list = it.Current.Value;
                    for (int y = 0, count = list.Count; y < count; ++y)
                    {
                        var curPos = list[y];
                        var characterData = dics[curPos.BattleTag];
                        if (characterData == null || false == characterData.IsBoss)
                            continue;

                        bossData = characterData;
                    }
                }

                animEnd = bossData == null;

                isEffect = Data.Wave == 1;
                if (frontDragon != null)
                {
                    var pos = Stage.Map.OffenseBeacon.transform.position;
                    pos.x = frontDragon.position.x;
                    Stage.Map.OffenseBeacon.transform.position = pos;

                    for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
                    {
                        var list = Stage.OffenseSpines[x];
                        if (list == null)
                            continue;

                        for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                        {
                            var curDragon = list[y];
                            if (curDragon == null || curDragon.Data.Death || y >= 2)
                                continue;

                            curDragon.transform.SetParent(Stage.Map.OffenseBeacon.transform, true);
                        }
                    }
                }
                minDel = isEffect ? SBFunc.Random(0.6f, 0.9f) : SBFunc.Random(1.5f, 2f);

                return true;
            }
            return false;
        }

        public override bool OnExit()
        {
            for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
            {
                var list = Stage.OffenseSpines[x];
                if (list == null)
                    continue;

                for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                {
                    var curDragon = list[y];
                    if (curDragon == null || curDragon.Data.Death || y >= 2)
                        continue;

                    curDragon.SetSpeed(1.0f);
                    curDragon.SetDefaultSpeed(1.0f);
                    ResetSinusoidalCharacter(curDragon.Data);
                }
            }

            var it = Stage.PrevSpines.GetEnumerator();
            while (it.MoveNext())
            {
                var itit = it.Current.GetEnumerator();
                while (itit.MoveNext())
                {
                    GameObject.Destroy(itit.Current.gameObject);
                }
            }
            Stage.PrevSpines.Clear();
            EventManager.RemoveListener<UIBattleBossEndEvent>(this);
            EventManager.RemoveListener<AdventureStateEvent>(this);

            return base.OnExit();
        }
        public void OnEvent(UIBattleBossEndEvent eventType)
        {
            animEnd = true;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                moving = false;
                minDel -= dt;
                Data.Time += dt;
                if (Data.CheckTimeOver())
                    return false;

                bool alive = false;
                Stage.Map.UpdateLeft(dt * speedRate * stageRate);

                float animSpeed = speedRate;
                if (animSpeed < 1f)
                    animSpeed = 1f;

                float camOffset = Camera.main.transform.position.x;
                for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
                {
                    var list = Stage.OffenseSpines[x];
                    if (list == null)
                        continue;

                    for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                    {
                        var curDragon = list[y] as BattleDragonSpine;
                        if (curDragon == null || curDragon.Data.Death)
                            continue;

                        alive = true;
                        if (curDragon.Data.IsEffectInfo(eSkillEffectType.STUN, eSkillEffectType.AIRBORNE, eSkillEffectType.FROZEN))
                        {
                            curDragon.SetAnimation(eSpineAnimation.IDLE);
                            curDragon.UpdateStatus(dt);
                            curDragon.transform.Translate(new Vector3(-dt * speedRate, 0));
                        }
                        else
                        {
                            curDragon.SetAnimation(eSpineAnimation.WALK);
                            curDragon.UpdateStatus(dt);
                            curDragon.SetDustDragonPos();
                            //Vector3 targetPos = data.Wave == 1 ? GetDragonStartPosition(x, y, yCount) : GetDragonEndPosition(x, y, yCount);
                            Vector3 targetPos = GetDragonEndPosition(x, y, yCount);

                            float dist = Vector3.Distance(curDragon.transform.localPosition, targetPos);
                            if (dist > 0.05f)
                            {
                                curDragon.SetSpeed(animSpeed);
                                curDragon.Controller.MoveLocalTargetUpdate(dt, targetPos, false, 100f * speedRate);
                                moving = true;
                            }
                            else
                            {
                                UpdateSinusoidalCharacter(dt, curDragon.Data);
                                curDragon.SetSpeed(1f);
                            }
                        }
                    }
                }

                if (!alive)
                {
                    return false;
                }

                if(!isEffect && !moving)
                {
                    var group = GameConfigTable.GetConfigIntValue("BATTLE_HEAL");
                    var skillEffect = SkillEffectData.GetGroup(group);
                    if(skillEffect != null)
                    {
                        for (int i = 0, count = Data.Characters.Count; i < count; ++i)
                        {
                            if (Data.Characters[i] == null || Data.Characters[i].Death)
                                continue;

                            for(int j = 0, jCount = skillEffect.Count; j < jCount; ++j)
                            {
                                EffectTriggerTarget(Data.Characters[i], Data.Characters[i], skillEffect[j]);
                            }
                        }
                    }
                    isEffect = true;
                }

                for (int x = 0, count = Stage.PrevSpines.Count; x < count; ++x)
                {
                    var list = Stage.PrevSpines[x];
                    if (list == null)
                        continue;

                    for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                    {
                        var prvEnemy = list[y];
                        if (prvEnemy == null || !prvEnemy.Data.Death)
                            continue;

                        prvEnemy.transform.Translate(new Vector3(-dt * speedRate * stageRate, 0));
                    }
                }

                for (int x = 0, count = events.Count; x < count; ++x)
                {
                    var eventTarget = events[x];
                    if (eventTarget == null)
                        continue;

                    eventTarget.Translate(new Vector3(-dt * speedRate * stageRate, 0));
                }
                return !animEnd || moving || minDel > 0;
            }
            return !IsPlaying;
        }

        public virtual void OnEvent(AdventureStateEvent e)
        {
            if (e.Pause)
                OnPause();
            else
                OnResume();
        }
    }
}