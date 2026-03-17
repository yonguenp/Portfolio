using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DailyStateStart : DailyState, EventListener<UIBattleObjectEndEvent>
    {
        private float minDelay = 0.5f;
        bool animEnd = false;
        bool eventSended = false;
        bool moving = false;

        private float speedRate = 3f;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                EventManager.AddListener<UIBattleObjectEndEvent>(this);
                var spinesList = Stage.OffenseSpines;
                spinesList.Clear();
                var dics = Data.OffenseDic;
                var it = Data.OffensePos.GetEnumerator();
                float camOffset = Camera.main.transform.position.x;
                var fieldPos = GetOffenseStartFieldPosition(camOffset);
                var field = Stage.Map.OffenseBeacon;
                field.transform.localPosition = fieldPos;

                while (it.MoveNext())
                {
                    var x = it.Current.Key;
                    var list = it.Current.Value;

                    var spineList = new List<BattleSpine>();
                    for (int y = 0, count = list.Count; y < count; ++y)
                    {
                        var curPos = list[y];

                        if (!dics.ContainsKey(curPos.BattleTag) || y >= 2)
                            continue;

                        var targetVec = GetDragonStartPosition(x, y, count) - new Vector3(3, 0, 0);
                        targetVec.x += camOffset;

                        var dragonSpine = Stage.GetOffenseSpine(dics[curPos.BattleTag]);
                        dragonSpine.transform.localPosition = targetVec;
                        spineList.Add(dragonSpine);
                        dragonSpine.GetComponent<Collider2D>().enabled = false;

                        Stage.SetGreenHpBar(dragonSpine.Data);

                        if (dragonSpine.Data.PetID > 0)
                        {
                            var pet = User.Instance.PetData.GetPet(dragonSpine.Data.PetID);
                            if (pet != null)
                            {
                                var petData = pet.GetPetDesignData();
                                if (petData != null)
                                {
                                    var prefab = SBFunc.GetPetSpineByName(petData.IMAGE);
                                    if (prefab != null)
                                    {
                                        var petNode = Object.Instantiate(prefab, Stage.Map.Beacon);
                                        var petSkin = petNode.GetComponent<PetSpine>();
                                        if (petSkin != null)
                                        {
                                            petSkin.SetData(petData);
                                        }
                                        var follow = petNode.GetComponent<FollowSpine>();
                                        if (follow == null)
                                            follow = petNode.AddComponent<FollowSpine>();

                                        follow.Set(dragonSpine.SpineTransform, dragonSpine.transform, dragonSpine.Data, new Vector3(-0.2f, -0.01f, 0f), true);

                                        dragonSpine.SetPet(follow);
                                    }
                                }
                            }
                        }
                    }
                    spinesList.Add(spineList);
                }

                animEnd = false;

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

                        //var targetVec = GetDragonStartPosition(x, y, yCount);
                        var targetVec = GetDragonEndPosition(x, y, yCount);
                        targetVec.x += camOffset;

                        var local = curDragon.transform.localScale;
                        local.x = Mathf.Abs(local.x);
                        curDragon.transform.localScale = local;

                        curDragon.Controller.StopCO();
                        curDragon.Controller.MoveLocalTarget(targetVec, 0.01f, false, 100f * curDragon.Data.MOVE_SPEED * speedRate);

                        curDragon.SetDefaultSpeed(speedRate);
                        curDragon.SetDustDragonPos();
                    }
                }

                return true;
            }
            return false;
        }

        public void OnEvent(UIBattleObjectEndEvent eventType)
        {
            if(Data != null)
                Time.timeScale = Data.Speed;
            else
                Time.timeScale = PlayerPrefs.GetFloat("DailySpeed", 1.0f);

            animEnd = true;
        }
        public override bool OnPause()
        {
            if (base.OnPause())
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

                        curDragon.SetDefaultSpeed(0f);
                        curDragon.StopDust();
                    }
                }
                return true;
            }
            return false;
        }
        public override bool OnResume()
        {
            if (base.OnResume())
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

                        curDragon.SetDefaultSpeed(speedRate);
                        curDragon.SetDustDragonPos();
                    }
                }
                return true;
            }
            return false;
        }

        public override bool OnExit()
        {
            if (base.OnExit())
            {
                animEnd = false;
                EventManager.RemoveListener<UIBattleObjectEndEvent>(this);
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                moving = false;
                minDelay -= dt;
                //data.Time += dt;
                if (Data.CheckTimeOver())
                    return false;
                Stage.Map.UpdateLeft(dt * 2.5f);
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

                        curDragon.SetAnimation(eSpineAnimation.WALK);
                        curDragon.SetDustDragonPos();
                        if (curDragon.Controller.IsMove)
                        {
                            moving = true;
                        }
                        else
                        {
                            curDragon.SetSpeed(1.5f);
                            UpdateSinusoidalCharacter(dt, curDragon.Data);
                        }
                    }
                }

                if (minDelay <= 0f && !eventSended)
                {
                    eventSended = true;
                    UIBattleObjectStartEvent.Send();
                }

                return !animEnd || moving;
            }
            return !IsPlaying;
        }
    }
}