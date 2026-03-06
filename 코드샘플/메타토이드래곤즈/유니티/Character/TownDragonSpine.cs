using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using Spine.Unity;
using UnityEngine.EventSystems;

namespace SandboxNetwork
{
    public enum eTownDragonEventState
    {
        NONE,
        TRAVEL_TRACKING,
        BUILD_DONE_TELEPORT
    }

    public class TownDragonSpine : SBSpine<eSpineAnimation>, EventListener<BuildCompleteEvent>, EventListener<DragonHideEvent>, EventListener<DragonShowEvent>
    {
        public UserDragon Data { get; private set; } = null;
        public Spine.Unity.SkeletonAnimation Skeleton { get { return skeletonAni; } }

        public SBController Controller { get; private set; } = null;
        public statusEffect StatusEffect { get; protected set; } = null;

        protected Rigidbody2D body = null;
        protected Collider2D Collider = null;
        protected DragonEmotion emotion = null;
        public DragonEmotion Emotion { 
            get {
                if (emotion == null)
                {
                    GameObject ui = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.DragonClonePath, "TownEmoticon"), transform);
                    if (ui != null)
                    {
                        emotion = ui.GetComponent<DragonEmotion>();
                        if (emotion != null)
                        {
                            emotion.Clear();
                            emotion.SetOrder(Order + 1);
                        }
                    }
                }

                return emotion;
            } 
        }

        protected TownStateMachine stateMachine = null;
        public TownStateData StateData
        {
            get
            {
                if (stateMachine == null)
                    return null;

                return stateMachine.Data;
            }
        }

        public List<Vector3Int> trackingCells = new List<Vector3Int>();
        public Vector3Int TrackingCell { get; private set; } = Vector3Int.zero;

        private Coroutine coStateUpdate = null;

        System.Action trackingCallback = null;
        eTownDragonEventState dragonEventState = eTownDragonEventState.NONE;
        ParticleSystem dust = null;
        //건설 완료 연출 관련
        static int TownJumpDragonCount = 6;
        readonly float[] xPositionsInCell = new float[6] { -.2f, .2f, -.3f, .3f, -.4f, .4f };
        bool isJump = false;
        Tweener tweener = null;
        public void SetData(UserDragon data)
        {
            Data = data;
            if (Data.State.HasFlag(eDragonState.Travel)|| Data.State.HasFlag(eDragonState.GemDungeon))
                gameObject.SetActive(false);
            
        }
        public override void InitializeTypeFunc()
        {
            GetNameToType = SBDefine.GetDragonAnimNameToType;
            GetTypeToName = GetAnimName;
            GetTypeToLoop = SBFunc.IsTypeToLoop;
            GetTypeToSkip = SBFunc.IsAnimSkip;
        }
        protected string GetAnimName(eSpineAnimation anim)
        {
            return anim switch
            {
                eSpineAnimation.ATTACK => SBDefine.GetDragonAnimTypeToName(anim, 1),
                eSpineAnimation.SKILL => SBDefine.GetDragonAnimTypeToName(anim, Data.BaseData.SKILL1.ANI),//skill_type추가 필요
                _ => SBDefine.GetDragonAnimTypeToName(anim, 1),
            };
        }

        public override void Init()
        {
            base.Init();
            TownJumpDragonCount = 6;
            if (stateMachine == null)
                stateMachine = new TownStateMachine();

            stateMachine.StateInit();

            Controller = gameObject.GetComponent<SBController>();
            if (Controller == null)
                Controller = gameObject.AddComponent<SBController>();

            sortingGroup = GetComponent<SortingGroup>();

            if (body == null)
                body = GetComponent<Rigidbody2D>();

            if (body != null)
                body.gravityScale = 0f;

            if (Collider == null)
                Collider = GetComponent<Collider2D>();

            if (Collider != null)
                Collider.enabled = false;

            if (StatusEffect == null)
                StatusEffect = GetComponent<statusEffect>();

            if (Data != null)
            {
                SetData(Data.BaseData);
                SetSkin(Data.BaseData.SKIN);
                SetAnimation(eSpineAnimation.IDLE);
                SetTranscendEffect(Data.TranscendenceData.Step);
                RandomBatch();

                SetSpecialDragon(Data.BaseData.KEY);
            }

            if (Controller.myCollider != null && Controller.myCollider.attachedRigidbody != null)
            {
                Controller.myCollider.attachedRigidbody.simulated = false;
            }
        }

        public void SetSpecialDragon(int key)
        {
            switch(key)
            {
                case 14075://문워크
                    if (Controller != null)
                        Controller.IsRight = true;
                    break;
            }
        }

        public virtual void SetTranscendEffect(int transcendStep)
        {
            if (StatusEffect == null)
                StatusEffect = GetComponent<statusEffect>();

            if (transcendStep <= 0)
            {
                SetShadow(true);
                return;
            }

            if (StatusEffect == null)
            {
                SetShadow(true);
                return;
            }

            var TranscendParent = StatusEffect.TranscendTr;
            if (TranscendParent.childCount > 0)
                SBFunc.RemoveAllChildrens(TranscendParent);

            TranscendParent.gameObject.SetActive(true);
            SetShadow(false);
            GameObject obj = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, SBDefine.GetTranscendEffectName(transcendStep));
            if (obj != null)
            {
                Instantiate(obj, TranscendParent);
                SBFunc.SetLayer(TranscendParent, "town_dragon");
            }
        }

        protected override void Start()
        {
            base.Start();
            
        }
        protected void OnDestroy()
        {
            
        }

        private void OnEnable()
        {
            stateMachine?.CurStateClear();
            if (Town.Instance.TownBaseTransoform != null)
            {
                transform.SetParent(Town.Instance.TownBaseTransoform);
            }
            RandomBatch();

            if (coStateUpdate != null)
            {
                StopCoroutine(coStateUpdate);
                coStateUpdate = null;
            }

            coStateUpdate = StartCoroutine(StateUpdate());
            EventManager.AddListener<BuildCompleteEvent>(this);
            EventManager.AddListener<DragonHideEvent>(this);
            EventManager.AddListener<DragonShowEvent>(this);
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
            if (tweener != null)
                tweener.Kill();
            tweener = null;

            stateMachine?.CurState?.TweenClear();

            if (coStateUpdate != null)
            {
                StopCoroutine(coStateUpdate);
                coStateUpdate = null;
            }

            EventManager.RemoveListener<BuildCompleteEvent>(this);
            EventManager.RemoveListener<DragonHideEvent>(this);
            EventManager.RemoveListener<DragonShowEvent>(this);

            ClearElevator();
        }

        public void ClearElevator()
        {
            Town.Instance.GetElevator(true).Remove(StateData);
            Town.Instance.GetElevator(false).Remove(StateData);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public bool IsActive()
        {
            return gameObject.activeInHierarchy;
        }

        public void SetMoveSpeed(float speed)
        {
            if (Controller != null)
                Controller.Speed = speed;
        }
        public void SetSpeed(int speed)
        {
            if (speed <= 0)
                return;

            float rate = (float)speed / SBDefine.TownDefaultSpeed;
            SetSpeed(rate);
        }
        public void SetSpeed(float scale)
        {
            skeletonAni.timeScale = scale;

            SetMoveSpeed((scale * SBDefine.TownDefaultSpeed) * Data.GetMoveSpeed());
        }

        public void RandomBatch()
        {
            if (stateMachine != null && stateMachine.Data != null)
            {
                stateMachine.Data.Dragon = this;
                stateMachine.Data.CurCell = TownMap.GetRandomBatchCell();
                stateMachine.Data.TargetCell = TownMap.GetRandomBatchCell();

                Vector3 lPos = TownMap.GetRandomCellPos(stateMachine.Data.CurCell.x, stateMachine.Data.CurCell.y);                
                Order = stateMachine.Data.CurCell.z;
                if (stateMachine.Data.CurCell.y >= 0)
                {
                    lPos.y += SBDefine.DragonY;
                }
                else
                {
                    if (Order == SBDefine.UnderFrontOrder)
                    {
                        lPos.y += SBDefine.UnderFrontDragonY;
                    }
                    else
                    {
                        lPos.y += SBDefine.UnderBackDragonY;
                    }
                }

                lPos.y += SBFunc.Random(-0.05f, 0.05f);
                skeletonAni.timeScale = 1;
                transform.localPosition = lPos;
                stateMachine.ChangeState<TownIdle>();
            }
        }

        //특정 위치에 특정 상태로 배치함
        public void SpecificStateBatch<T>(int cell, int floor, int order = 0, Vector2 AddPos = new(), bool noState = false) where T : class, IStateBase
        {
            stateMachine?.CurStateClear();
            if (stateMachine != null && stateMachine.Data != null)
            {
                stateMachine.Data.Dragon = this;
                stateMachine.Data.CurCell = new Vector3Int(cell,floor,order);
                Order = order;
                Vector3 DragonPos = TownMap.GetCellPos(cell, floor) + AddPos;
                if (stateMachine.Data.CurCell.y >= 0)
                {
                    DragonPos.y += SBDefine.DragonY;
                }
                else
                {
                    if (Order == SBDefine.UnderFrontOrder)
                    {
                        DragonPos.y += SBDefine.UnderFrontDragonY;
                    }
                    else
                    {
                        DragonPos.y += SBDefine.UnderBackDragonY;
                    }
                }

                //lPos.y += SBFunc.Random(-0.05f, 0.05f);
                skeletonAni.timeScale = 1;
                transform.localPosition = DragonPos;
                if (noState)
                {
                    stateMachine.ChangeState<T>();
                }
            }
        }

        IEnumerator StateUpdate()
        {
            //이벤트가 있을수있기때문에 1프레임 대기 후 루틴
            yield return SBDefine.GetWaitForEndOfFrame();

            float time = -1.0f;

            while (true)
            {
                if (stateMachine == null)
                {
                    yield return null;
                    continue;
                }

                float curTime = SBGameManager.Instance.DTime;
                if (time < 0f)
                {
                    time = SBFunc.Random(5f, 10f);
                    if (TrackingCell != Vector3Int.zero)
                        time = SBFunc.Random(1f, 3f);

                    if (stateMachine.CurState is TownIdle or TownIdleWithTracking)
                    {
                        if (TrackingCell != Vector3Int.zero)
                        {
                            switch (dragonEventState)
                            {
                                case eTownDragonEventState.TRAVEL_TRACKING:
                                    OnTravelStartedProcess();
                                    break;
                                //case eTownDragonEventState.BUILD_DONE_TELEPORT:
                                //    OnTeleport();
                                //    break;
                            }
                            
                            continue;
                        }


                        switch (SBFunc.Random(0, 30))
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            {
                                stateMachine.Data.TargetCell = TownMap.GetRandomCellByFloor(stateMachine.Data.CurCell);
                                stateMachine.ChangeState<TownMove>();
                            }break;

                            case 4:
                            {
                                if (StateData.CurCell.y < 0)
                                    continue;

                                stateMachine.Data.TargetCell = TownMap.GetRandomCellByFloor(stateMachine.Data.CurCell);
                                stateMachine.ChangeState<TownMoveAndRandomSliding>();
                            }
                            break;

                            case 5:
                            case 6:
                            case 7:
                            //case 8://엘레베이터 확률 줄이기로 됨
                            //case 9:
                            {
                                Vector3 lPos = transform.position;
                                if (stateMachine.Data.CurCell.y >= 0)
                                {
                                    lPos.y -= SBDefine.DragonY;
                                }
                                else
                                {
                                    if (Order == SBDefine.UnderFrontOrder)
                                    {
                                        lPos.y -= SBDefine.UnderFrontDragonY;
                                    }
                                    else
                                    {
                                        lPos.y -= SBDefine.UnderBackDragonY;
                                    }
                                }
                                //SpacingOffset
                                lPos.y += SBDefine.DragonY;

                                Vector3Int cellPos = TownMap.GetCellPosByWorldPos(lPos);
                                if (cellPos.y == stateMachine.Data.CurCell.y)
                                {
                                    stateMachine.Data.TargetCell = TownMap.GetRandomFloorCell(stateMachine.Data.CurCell);
                                    stateMachine.ChangeState<TownElevator>();
                                }
                                else
                                {
                                    //어떤 그지같은 State에서 CurCell 븅신같이 처리하는 듯
                                    //예외처리안하면 드래곤 하늘나라감.
                                    stateMachine.ChangeState<TownJump>();
                                }
                            }break;

                            case 10:
                            {
                                if (StateData.CurCell.y < 0)
                                    continue;

                                stateMachine.ChangeState<TownJump>();
                            }break;

                            
                            case 12:
                            {
                                if (StateData.CurCell.y < 0)
                                    continue;

                                TryCrash();
                            }
                            break;

                            case 13:
                            {
                                TryChitchat();
                            }
                            break;

                            case 14:
                            case 15:
                            {
                                if (StateData.CurCell.y < 0)
                                    continue;

                                if (Data.BaseData.GRADE == (int)eDragonGrade.Legend && Data.TranscendenceData.Step > 0)//초월 한놈만 깝치기
                                {
                                    stateMachine.ChangeState<TownSwagger>();
                                }
                            }
                            break;
                        }
                        continue;
                    }
                }

                if (!stateMachine.Update(curTime))
                {
                    stateMachine.SetNextState();
                }

                time -= curTime;
                yield return null;
            }
        }

        public void OnTravelStartedProcess()
        {
            if (stateMachine == null || TrackingCell == Vector3Int.zero)
                return;

            stateMachine.Data.TargetCell = TrackingCell;

            if (TrackingCell.y == stateMachine.Data.CurCell.y) // 나와 같은 층
            {
                float enteranceLeft = TownMap.GetCellPosX(TrackingCell.x) - SBDefine.CellSpancing * .5f;
                float enteranceRight = TownMap.GetCellPosX(TrackingCell.x) + SBDefine.CellSpancing * .5f;
                float curPosX = transform.localPosition.x;
                
                if (enteranceLeft < curPosX && enteranceRight > curPosX)
                {
                    if (trackingCallback == null)
                    {
                        float val = SBFunc.RandomValue;
                        if (val < 0.3f)
                        {
                            stateMachine.ChangeState<TownJump>();
                        }
                        else
                            stateMachine.ChangeState<TownIdleWithTracking>();
                    }
                    else
                    {
                        trackingCallback?.Invoke();
                        trackingCallback = null;
                    }
                }
                else
                {
                    stateMachine.ChangeState<TownMoveFast>();
                }
            }
            else
            {
                stateMachine.ChangeState<TownElevatorFast>();
            }
        }

        public void OnTeleport()
        {
            if (stateMachine == null || TrackingCell == Vector3Int.zero)
                return;
            if (gameObject.activeSelf == false) return;

            if (TownJumpDragonCount > 0)
            {
                float xPosInCell = xPositionsInCell[--TownJumpDragonCount];
                stateMachine.Data.TargetCell = TrackingCell;
                if (stateMachine.Data.CurCell != TrackingCell)
                {
                    transform.localPosition = TownMap.GetBuildingPos(TrackingCell.x, TrackingCell.y) + new Vector2(xPosInCell * SBDefine.CellSpancing, SBDefine.DragonY);
                    transform.localScale = new Vector3(xPosInCell < 0 ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    stateMachine.Data.CurCell = TrackingCell;
                    stateMachine.ChangeState<TownJumpLoop>();
                }
            }
        }

        public void OnEvent(BuildCompleteEvent eventType)
        {
            //if (eventType.building == null)
            //    return;
            //if (Data != null && Data.State == eDragonState.WorldTrip)
            //    return;

            //Vector3Int cur = new Vector3Int(eventType.building.Cell, eventType.building.Floor);
            //if (eventType.eType == eBuildingState.CONSTRUCT_FINISHED)
            //{
            //    trackingCells.Add(cur);

            //    if (TrackingCell == Vector3Int.zero)
            //        TrackingCell = cur;
            //    dragonEventState = eTownDragonEventState.BUILD_DONE_TELEPORT;
            //    if (stateMachine != null && stateMachine.CurState is TownIdle)
            //    {
            //        OnTeleport();
            //    }
            //}
            //else
            //{
            //    isJump = false;
            //    if (stateMachine == null) return;
            //    if (TrackingCell == cur && stateMachine.Data.CurCell == cur)
            //        stateMachine.ChangeState<TownHappy>();

            //    List<Vector3Int> del = new List<Vector3Int>();
            //    foreach (var tr in trackingCells)
            //    {
            //        if (tr == cur)
            //        {
            //            del.Add(tr);
            //        }
            //    }

            //    foreach (var tr in del)
            //    {
            //        trackingCells.Remove(tr);
            //    }


            //    if (trackingCells.Count > 0)
            //    {
            //        TrackingCell = trackingCells[SBFunc.Random(0, trackingCells.Count)];
            //    }
            //    else
            //    {
            //        TrackingCell = Vector3Int.zero;
            //    }
            //}
        }

        public void OnEvent(DragonHideEvent eventType)
        {
            if (!gameObject.activeInHierarchy || Data == null)
                return;

            if(Data.State.HasFlag(eDragonState.Travel))
            {
                var buildingInfo = Town.Instance.GetBuilding((int)eLandmarkType.Travel);
                if (buildingInfo == null)
                    return;

                Vector3Int cur = new Vector3Int(buildingInfo.Cell, buildingInfo.Floor);
                TrackingCell = cur;

                trackingCallback = OnDeparture;
                dragonEventState = eTownDragonEventState.TRAVEL_TRACKING;
                SpecificStateBatch<TownIdle>(cur.x, cur.y, 0, Vector2.left * SBFunc.Random(0, SBDefine.CellBothSpancing * 0.5f),true);
                OnTravelStartedProcess();
            }
            if (Data.State.HasFlag(eDragonState.GemDungeon))
            {
                gameObject.SetActive(false);
                shadow.gameObject.SetActive(false);
            }
            if (Data.State.HasFlag(eDragonState.Guild))
            {
                gameObject.SetActive(false);
                shadow.gameObject.SetActive(false);
            }
        }

        public void OnEvent(DragonShowEvent eventType)
        {
            if (Data.State.HasFlag(eDragonState.Travel) || 
                Data.State.HasFlag(eDragonState.GemDungeon) ||
                Data.State.HasFlag(eDragonState.Guild))
            {
                return;
            }

            gameObject.SetActive(true);
            shadow.gameObject.SetActive(true);

            if (tweener != null)
                tweener.Kill();
            tweener = DOTween.To(() => Skeleton.skeleton.A, co => { Skeleton.skeleton.A = co; }, 1.0f, 1.0f);

            if(stateMachine != null && stateMachine.CurState is TownIdle or TownMove or TownIdleWithTracking)
            {
                stateMachine.ChangeState<TownHappy>();
            }
        }

        void OnDeparture()
        {
            shadow.gameObject.SetActive(false);

            if (tweener != null)
                tweener.Kill();
            tweener = DOTween.To(() => Skeleton.skeleton.A , co => { Skeleton.skeleton.A = co; }, .0f, 2.0f).OnComplete(()=> {
                gameObject.SetActive(false);
            }).SetDelay(1.0f);

            TravelDepartureEvent.Send();
        }
        public void OnDust(bool isRight)
        {
            if (dust == null)
            {
                Vector3 pos = transform.position;
                if (Controller.myCollider != null)
                {
                    pos.x = Controller.myCollider.bounds.center.x;
                    pos.y = Controller.myCollider.bounds.center.y - (Controller.myCollider.radius * transform.localScale.y) + 0.2f;
                }

                GameObject dust_resource = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, "fx_common_skill_006");
                if (dust_resource == null)
                    return;

                GameObject dustObject = Instantiate(dust_resource, transform);
                dust = dustObject.GetComponent<ParticleSystem>();
                dustObject.transform.position = pos;
                dustObject.transform.GetChild(0).localScale = new Vector3(isRight?1:-1, 1,1);
            }

            if (dust != null)
            {
                if (!dust.gameObject.activeSelf)
                    dust.gameObject.SetActive(true);
                dust.transform.GetChild(0).localScale = new Vector3(isRight ? 1 : -1, 1, 1);
                dust.Play();
            }
        }
        public void StopDust()
        {
            if (dust != null)
            {
                dust.Stop();
            }
        }
        public void OffDust()
        {
            if (dust != null)
            {
                dust.gameObject.SetActive(false);
            }
        }
        public void SetTownOrder(int order)
        {
            Order = order;
        }


        public override void SetOrder(int order)
        {
            base.SetOrder(order);

            float rate = 1.0f;
            Color color = Color.white;
            if (order != SBDefine.DefaultOrder)
            {
                if (order == SBDefine.UnderFrontOrder)
                    rate = 1.2f;
                else if (order == SBDefine.UnderBackOrder)
                {
                    rate = 0.8f;
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
            }

            if (Emotion != null)
                Emotion.SetOrder(order + 1);

            transform.localScale = new Vector3(transform.localScale.x < 0 ? -rate : rate, rate, rate);
            Skeleton.Skeleton.SetColor(color);
        }

        public void SetRandomEmotion()
        {
            SetEmotion(DragonEmotion.Emotion.RANDOM, DragonEmotion.EmotionColor.RANDOM);
        }
        public void SetEmotion(DragonEmotion.Emotion emo, DragonEmotion.EmotionColor color)
        {
            if(StateData.CurCell.y < 0)
            {
                if (StateData.CurCell.z == SBDefine.UnderBackOrder)
                    return;
            }

            if (Emotion != null)
            {
                if (SBFunc.RandomValue <= 0.5f)
                    Emotion.SetEmotion(emo, color);                
            }
        }

        void TryChitchat()
        {
            List<TownDragonSpine> nears = new List<TownDragonSpine>();

            foreach (var it in Town.Instance.TownDragons)
            {
                if (it.Value == null || it.Value == this)
                    continue;

                var dragon = it.Value;
                if (dragon.stateMachine != null && (dragon.stateMachine.CurState is TownIdle or TownIdleWithTracking))
                {
                    if (dragon.stateMachine.Data.CurCell.y != stateMachine.Data.CurCell.y)
                        continue;

                    float distance = Mathf.Abs(dragon.transform.localPosition.x - transform.localPosition.x);
                    if ((distance < TownChitchat.NEAR_MAX_DISTANCE) && (distance > TownChitchat.NEAR_MIN_DISTANCE))
                        nears.Add(dragon);
                }
            }

            if (nears.Count > 0)
            {
                TownChitchat.CHITCHAT_TYPE firstType = TownChitchat.CHITCHAT_TYPE.NORMAL;
                TownChitchat.CHITCHAT_TYPE secondType = TownChitchat.CHITCHAT_TYPE.NORMAL;
                int chatCount = SBFunc.Random(10, 30);

                if (SBFunc.Random(0, 10) == 0)
                {
                    chatCount = SBFunc.Random(5, 15);
                    firstType = TownChitchat.CHITCHAT_TYPE.BOOLY;
                    secondType = TownChitchat.CHITCHAT_TYPE.VICTIM;
                }

                var targetDragon = nears[0];
                transform.localScale = new Vector3(targetDragon.transform.localPosition.x > transform.localPosition.x ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

                TownChitchat state = stateMachine.GetState<TownChitchat>();
                if (state != null)
                    state.SetChitChatCount(firstType, chatCount);

                stateMachine.ChangeState<TownChitchat>();

                if(nears.Count > 3)
                {
                    nears = nears.GetRange(0, 3);
                }

                foreach (var target in nears)
                {
                    target.transform.localScale = new Vector3(transform.localPosition.x > target.transform.localPosition.x ? Mathf.Abs(target.transform.localScale.x) : -Mathf.Abs(target.transform.localScale.x), target.transform.localScale.y, target.transform.localScale.z);
                    TownChitchat targetState = target.stateMachine.GetState<TownChitchat>();
                    if (targetState != null)
                        targetState.SetChitChatCount(secondType, chatCount);

                    target.stateMachine.ChangeState<TownChitchat>();

                    if (secondType == TownChitchat.CHITCHAT_TYPE.VICTIM)
                        secondType = TownChitchat.CHITCHAT_TYPE.OTHER;
                }
            }
        }

        void TryCrash()
        {
            List<TownDragonSpine> fars = new List<TownDragonSpine>();

            foreach (var it in Town.Instance.TownDragons)
            {
                if (it.Value == null || it.Value == this)
                    continue;

                var dragon = it.Value;
                if (dragon.stateMachine != null && (dragon.stateMachine.CurState is TownIdle or TownIdleWithTracking))
                {
                    if (dragon.stateMachine.Data.CurCell.y != stateMachine.Data.CurCell.y)
                        continue;

                    float distance = Mathf.Abs(dragon.transform.localPosition.x - transform.localPosition.x);
                    
                    if (distance > TownMoveCrash.CRASHABLE_DISTANCE)
                        fars.Add(dragon);
                }
            }

            if (fars.Count > 0)
            {
                var target = fars[SBFunc.Random(0, fars.Count)];
                if (target != null)
                {
                    float mid_pos = (target.transform.localPosition.x + transform.localPosition.x) / 2.0f;

                    TownMoveCrash state = stateMachine.GetState<TownMoveCrash>();
                    state.SetCrashPosX(mid_pos, target);
                    state = target.stateMachine.GetState<TownMoveCrash>();
                    state.SetCrashPosX(mid_pos, this);

                    stateMachine.ChangeState<TownMoveCrash>();
                    target.stateMachine.ChangeState<TownMoveCrash>();
                }
            }
        }

        public void OnSwag()
        {
            List<TownDragonSpine> nears = new List<TownDragonSpine>();

            foreach (var it in Town.Instance.TownDragons)
            {
                if (it.Value == null || it.Value == this)
                    continue;

                var dragon = it.Value;
                if (dragon.stateMachine != null && (dragon.stateMachine.CurState is TownIdle or TownIdleWithTracking))
                {
                    if (dragon.stateMachine.Data.CurCell.y != stateMachine.Data.CurCell.y)
                        continue;

                    float distance = Mathf.Abs(dragon.transform.localPosition.x - transform.localPosition.x);
                    if ((distance < TownSwagger.NEAR_MAX_DISTANCE) && (distance > TownSwagger.NEAR_MIN_DISTANCE))
                        nears.Add(dragon);
                }
            }

            foreach(var near in nears)
            {
                near.stateMachine.ChangeState<TownSwaggerEffect>();

                near.transform.localScale = new Vector3(Mathf.Abs(near.transform.localScale.x) * (transform.localPosition.x > near.transform.localPosition.x ? 1.0f : -1.0f), near.transform.localScale.y, near.transform.localScale.z);
            }
        }

        Coroutine subStateCoroutine = null;
        public void SubStateCoroutine(IEnumerator routine)
        {
            if (subStateCoroutine != null)
                StopCoroutine(subStateCoroutine);

            if (!gameObject.activeInHierarchy)
                return;

            if (routine == null)
                return;

            subStateCoroutine = StartCoroutine(routine);
        }

        public override void SetOutline(bool active)
        {
            if (outlineRenderer == null)
            {
                outlineRenderer = GetComponentInChildren<OutlineRenderer>(true);
            }

            if (outlineRenderer == null)
                return;

            outlineRenderer.SetOutline(active);
        }
        public Spine.TrackEntry SetForceAnimation(eSpineAnimation anim)
        {
            if (Animation == anim)
                return null;

            Animation = anim;
            var animName = GetTypeToName(anim);

            if (skeletonAni == null || skeletonAni.AnimationName == animName)
                return null;

            return SetAnimation(0, animName, SBFunc.IsTypeToLoop(anim));
        }
    }
}