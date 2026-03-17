using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossStateInit : WorldBossState, EventListener<UIBattleObjectEndEvent>, EventListener<UIBattleBossEndEvent> //드래곤 생성 및 연출 및 대기
    {
        private float minDelay = 0.5f;
        bool bossAlert = false;
        bool animEnd = false;
        bool moving = false;

        private float speedRate = 3f;
        BattleWorldBossSpine bossSpine = null;

        private SBController dragonBeacon { get { return Stage.Map.OffenseBeacon; } }
        private SBController monsterBeacon { get { return Stage.Map.DefenseBeacon; } }
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                EventManager.AddListener<UIBattleBossEndEvent>(this);
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

                        var targetVec = GetDragonStartPosition(x, y, count);
                        targetVec.x += camOffset;

                        var dragonSpine = Stage.GetOffenseSpine(dics[curPos.BattleTag]);                        
                        spineList.Add(dragonSpine);
                        dragonSpine.GetComponent<Collider2D>().enabled = false;
                        var local = dragonSpine.transform.localScale;
                        local.x = Mathf.Abs(local.x);

                        if(x < 2)
                        {
                            local *= 0.9f;
                        }

                        if (((WorldBossStage)Stage).GetDirectionFromPartyIndex(x) == eDirectionBit.Right)
                        {
                            targetVec -= new Vector3(6, 0, 0);
                        }
                        else
                        {
                            targetVec += new Vector3(6, 0, 0);
                            local.x *= -1.0f;
                        }
                       
                        dragonSpine.transform.localPosition = targetVec;
                        dragonSpine.transform.localScale = local;

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

                //몬스터 세팅
                spinesList = Stage.DefenseSpines;
                spinesList.Clear();
                dics = Data.DefenseDic;
                it = Data.DefensePos.GetEnumerator();
                IBattleCharacterData bossData = null;

                float startOffset = 0;

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

                        bossSpine = ((WorldBossStage)Stage).GetWorldBossSpine(dics[curPos.BattleTag]) as BattleWorldBossSpine;
                        //Vector3 pos = GetMonsterStartPosition(x, y, count, (data.Wave == 1 ? 3f : 0f), startOffset);
                        //Vector3 pos = GetMonsterStartPosition(x, y, count, 0f, startOffset);

                        bossSpine.transform.localPosition = Vector3.zero;
                        bossSpine.OffDust();
                        bossSpine.SetRigidbodySimulated(false);
                        
                        //enemySpine.Controller.MoveLocalTarget(GetMonsterBattlePosition(x, y, count), 0.01f, true);

                        spineList.Add(bossSpine);

                        //Stage.SetRedHpBar(enemySpine.Data);

                        bossData = bossSpine.CData;


                        //ProCamera2D.Instance.AddCameraTarget(bossSpine.transform);
                    }
                    spinesList.Add(spineList);
                }

                Camera.main.orthographicSize = 1f;
                UIBattleBossStartEvent.Send(bossData);
                return true;
            }
            return false;
        }

        public void OnEvent(UIBattleObjectEndEvent eventType)
        {
            if (Stage == null)//반복플레이하다보면 꼬일떄가있음.
                return;

            //BossAlert();
            //animEnd = true;

            //InitStep();
        }

        void InitStep()
        {
            if (Stage == null || Stage.OffenseSpines == null)
                return;

            if (Data != null)
                Time.timeScale = Data.Speed;
            else
                Time.timeScale = PlayerPrefs.GetFloat("WorldBossSpeed", 1.0f);

            float camOffset = Camera.main.transform.position.x;
            for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
            {
                var list = Stage.OffenseSpines[x];
                if (list == null)
                    continue;

                for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                {
                    var curDragon = list[y];

                    //var targetVec = GetDragonStartPosition(x, y, yCount);
                    var targetVec = ((WorldBossStage)Stage).GetDragonEndPosition(curDragon);
                    targetVec.x += camOffset;

                    var local = curDragon.transform.localScale;
                    local.x = Mathf.Abs(local.x);

                    if (((WorldBossStage)Stage).GetPartyDirection(curDragon) != eDirectionBit.Right)
                    {
                        local.x *= -1.0f;
                    }

                    curDragon.transform.localScale = local;

                    curDragon.Controller.StopCO();
                    curDragon.Controller.MoveLocalTarget(targetVec, 0.01f, false, 100f * curDragon.Data.MOVE_SPEED * speedRate);

                    curDragon.SetAnimation(eSpineAnimation.WALK);
                    curDragon.SetDefaultSpeed(speedRate);
                    curDragon.SetDustDragonPos(((WorldBossStage)Stage).GetPartyDirection(curDragon) == eDirectionBit.Right);
                }
            }
        }

        public void OnEvent(UIBattleBossEndEvent eventType)
        {
            if (Stage == null)//반복플레이하다보면 꼬일떄가있음.
                return;

            bossAlert = true;

            //InitStep();

            ProCamera2D.Instance.CalculateScreenSize();
            ProCamera2D.Instance.Zoom(1.5f, 0.4f, EaseType.EaseIn);
            animEnd = true;

            bossSpine.OnBossShowAnimation();
            bossSpine.StartCoroutine(NextStep());
        }
        private IEnumerator NextStep()
        {
            yield return SBDefine.GetWaitForSeconds(0.2f);
            InitStep();
            yield break;
        }

        public void BossAlert()
        {
            var dics = Data.DefenseDic;
            IBattleCharacterData bossData = null;
            foreach (var it in Data.DefensePos)
            {
                var listEnemy = it.Value;

                for (int y = 0, count = listEnemy.Count; y < count; ++y)
                {
                    var curPos = listEnemy[y];

                    if (!dics.ContainsKey(curPos.BattleTag))
                        continue;
                    var curData = dics[curPos.BattleTag] as BattleMonsterData;
                    if (curData != null && curData.SpawnData.IS_BOSS == 1)
                        bossData = dics[curPos.BattleTag];
                }
            }
            if (bossData != null)
                UIBattleBossStartEvent.Send(bossData);
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
                        curDragon.SetDustDragonPos(((WorldBossStage)Stage).GetPartyDirection(curDragon) == eDirectionBit.Right);
                    }
                }
                return true;
            }
            return false;
        }

        public void ClearState()
        {
            bossAlert = false;
            animEnd = false;
            EventManager.RemoveListener<UIBattleObjectEndEvent>(this);
            EventManager.RemoveListener<UIBattleBossEndEvent>(this);
        }
        public override bool OnExit()
        {
            ClearState();
            return base.OnExit();
        }

        public override bool Destroy()
        {
            ClearState();
            return base.Destroy();
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

                        if (curDragon.Controller.IsMove)
                        {
                            curDragon.SetAnimation(eSpineAnimation.WALK);
                            curDragon.SetDustDragonPos(((WorldBossStage)Stage).GetPartyDirection(curDragon) == eDirectionBit.Right);
                            moving = true;
                        }
                        else
                        {
                            //curDragon.SetSpeed(1.0f);
                            //pdateSinusoidalCharacter(dt, curDragon.Data);
                            curDragon.SetAnimation(eSpineAnimation.IDLE);
                            curDragon.StopDust();
                        }
                    }
                }

                return !animEnd || moving;
            }
            return !IsPlaying;
        }
    }
}