using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SandboxNetwork
{
    public class TownElevatorData
    {
        public TownElevatorData()
        {
            dragons = new Dictionary<TownStateData, Transform>();
            needDragons = new List<TownStateData>();
            curFloor = 0;
            moveType = eElevatorMove.Up;
        }
        private List<TownStateData> needDragons = null;
        public Dictionary<TownStateData, Transform> dragons { get; private set; } = null;
        private Elevator elevator = null;
        private int curFloor = 0;
        private eElevatorMove moveType = eElevatorMove.Up;
        public eElevatorMove MoveType
        {
            get { return moveType; }
            set { moveType = value; }
        }
        public Elevator Elevator
        {
            get
            {
                return elevator;
            }
        }
        public SBController Controller
        {
            get
            {
                if (elevator == null)
                    return null;

                return elevator.Controller;
            }
        }
        public int CurFloor
        {
            get { return curFloor; }
            set { curFloor = value; }
        }

        public void SetData(Elevator elevator, int curFloor)
        {
            this.elevator = elevator;
            this.curFloor = curFloor;
        }
        public bool IsContain(TownStateData obj)
        {
            if (obj == null || dragons == null)
                return false;

            return dragons.ContainsKey(obj);
        }
        public bool IsNeedContain(TownStateData obj)
        {
            if (needDragons == null)
                return false;

            return needDragons.Contains(obj);
        }
        public void PushContain(TownStateData obj)
        {
            if (IsContain(obj))
                return;

            dragons.Add(obj, obj.Dragon.transform.parent);
        }
        public void PopContain(TownStateData obj)
        {
            if (!IsContain(obj))
                return;

            dragons.Remove(obj);
        }
        public void PushNeed(TownStateData obj)
        {
            if (IsNeedContain(obj))
                return;

            needDragons.Add(obj);
        }
        public void PopNeed(TownStateData obj)
        {
            if (!IsNeedContain(obj))
                return;

            needDragons.Remove(obj);
        }
        public Transform GetParent(TownStateData obj)
        {
            if (!IsContain(obj))
                return null;

            return dragons[obj];
        }
        public bool IsExitDragon()
        {
            if (dragons == null)
                return false;

            var dragonKeys = new List<TownStateData>(dragons.Keys);
            for (int i = 0, count = dragonKeys.Count; i < count; ++i)
            {
                var data = dragonKeys[i];
                if (curFloor == data.TargetCell.y)
                {   
                    return true;
                }
            }

            return false;
        }
        public bool IsInDragon(TownStateData obj)
        {
            if (obj == null)
                return false;

            if (obj.CurCell.y == curFloor)
            {
                switch (moveType)
                {
                    case eElevatorMove.Up:
                        if (obj.TargetCell.y > curFloor)
                            return true;
                        break;
                    case eElevatorMove.Down:
                        if (obj.TargetCell.y < curFloor)
                            return true;
                        break;
                }
            }

            return false;
        }
        public bool IsInDragon()
        {
            if (needDragons == null)
                return false;

            for(int i = 0, count = needDragons.Count; i < count; ++i)
            {
                if (IsInDragon(needDragons[i]))
                    return true;
            }

            return false;
        }
        public eElevatorMove GetNextMoveType()
        {
            if (dragons == null || needDragons == null)
                return eElevatorMove.None;

            int up = 0;
            int down = 0;

            switch (moveType)
            {
                case eElevatorMove.Up:
                    if (curFloor == (TownMap.OpenHeight - 1))
                        moveType = eElevatorMove.Down;
                    break;
                case eElevatorMove.Down:
                {
                    int minimumFloor = TownMap.Y;
                    var subway = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.SUBWAY);
                    if (subway == null || subway.State != eBuildingState.NORMAL)
                    {
                        minimumFloor = 0;
                    }

                    if (curFloor == minimumFloor)
                    {
                        moveType = eElevatorMove.Up;
                    }
                }break;
            }

            var dragonKeys = new List<TownStateData>(dragons.Keys);
            for (int i = 0, count = dragonKeys.Count; i < count; ++i)
            {
                if (curFloor < dragonKeys[i].TargetCell.y)
                {
                    ++up;
                }
                else if (curFloor > dragonKeys[i].TargetCell.y)
                {
                    ++down;
                }
                else
                {
                    return eElevatorMove.None;
                }
            }

            for (int i = 0, count = needDragons.Count; i < count; ++i)
            {
                if (curFloor < needDragons[i].CurCell.y)
                {
                    ++up;
                }
                else if (curFloor > needDragons[i].CurCell.y)
                {
                    ++down;
                }                
            }

            switch (moveType)
            {
                case eElevatorMove.Up:
                    if (up > 0)
                        moveType = eElevatorMove.Up;
                    if (up == 0 && down > 0)
                        moveType = eElevatorMove.Down;
                    break;
                case eElevatorMove.Down:
                    if (down > 0)
                        moveType = eElevatorMove.Down;
                    if (down == 0 && up > 0)
                        moveType = eElevatorMove.Up;
                    break;
            }

            for (int i = 0, count = needDragons.Count; i < count; ++i)
            {
                if (curFloor == needDragons[i].CurCell.y)
                {
                    switch (moveType)
                    {
                        case eElevatorMove.Up:
                            if (needDragons[i].TargetCell.y > curFloor)
                                return eElevatorMove.None;//기두려
                            break;
                        case eElevatorMove.Down:
                            if (needDragons[i].TargetCell.y < curFloor)
                                return eElevatorMove.None;//기두려
                            break;
                    }
                }
            }


            if (up == 0 && down == 0)
                return eElevatorMove.None;

            return moveType;
        }
    }
    public class TownElevatorStateMachine : SimpleStateMachine<TownElevatorAIState>
    {
        float BlinkTimeCircle { get { return 30.0f + (SBFunc.RandomValue * 60.0f); } }
        private float runingTime = 0.0f;
        private float blinkTime = 0.0f;
        private TownElevatorData data = null;

        private Transform TopGear
        {
            get
            {
                if (data != null && data.Elevator != null)
                    return data.Elevator.TopGear;

                return null;
            }
        }

        private Transform BotGear
        {
            get
            {
                if (data != null && data.Elevator != null)
                    return data.Elevator.BotGear;

                return null;
            }
        }
        private SpriteRenderer InSide
        {
            get
            {
                if (data != null && data.Elevator != null)
                    return data.Elevator.InSide;

                return null;
            }
        }
        public TownElevatorData Data
        {
            get { return data; }
        }
        public TownElevatorStateMachine()
        {
            data = new TownElevatorData();
            runingTime = BlinkTimeCircle;
        }

        public void SetData(Elevator obj, int curFloor)
        {
            data.SetData(obj, curFloor);
        }

        public override bool ChangeState(TownElevatorAIState state)
        {
            if (state == null)
                return false;

            state.Set(data);
            if (CurState == null)
            {
                if (state.OnEnter())
                {
                    CurState = state;
                    return true;
                }
            }
            else
            {
                if (CurState.OnExit() && state.OnEnter())
                {
                    CurState = state;
                    return true;
                }
            }

            return false;
        }
        public override void SetState()
        {
            AddState(new TownElevatorIdleState());
            AddState(new TownElevatorOpenState());
            AddState(new TownElevatorOutIdleState());
            AddState(new TownElevatorInIdleState());
            AddState(new TownElevatorCloseState());
            AddState(new TownElevatorMoveUpState());
            AddState(new TownElevatorMoveDownState());
        }
        public void Update(float dt)
        {
            if (CurState == null)
                return;

            if (blinkTime > 0.0f)
            {
                blinkTime -= dt;
                if (blinkTime <= 0.0f)
                {
                    SetNormal();                    
                }
                return;
            }

            
            if (CurState is TownElevatorMoveUpState)
            {
                GearUpdate(dt);

                runingTime -= dt;
                if (runingTime < 0.0f)
                {
                    blinkTime = SetBlink();
                    return;
                }
            }

            if (CurState is TownElevatorMoveDownState)
            {
                GearUpdate(dt * -1.0f);

                runingTime -= dt;
                if (runingTime < 0.0f)
                {
                    blinkTime = SetBlink();
                    return;
                }                
            }

            if (!CurState.Update(dt))
            {
                if (CurState is TownElevatorIdleState)
                {
                    switch (data.GetNextMoveType())
                    {
                        case eElevatorMove.Up:
                        {
                            data.MoveType = eElevatorMove.Up;
                            if (ChangeState<TownElevatorMoveUpState>())
                                CurState.Update(dt);
                            return;
                        }
                        case eElevatorMove.Down:
                        {
                            data.MoveType = eElevatorMove.Down;
                            if (ChangeState<TownElevatorMoveDownState>())
                                CurState.Update(dt);
                            return;
                        }
                        case eElevatorMove.None:
                        default:
                        {
                            ChangeState<TownElevatorOpenState>();
                            return;
                        }
                    }
                }

                if (CurState is TownElevatorMoveUpState)
                {
                    ChangeState<TownElevatorIdleState>();
                    return;
                }

                if (CurState is TownElevatorMoveDownState)
                {
                    ChangeState<TownElevatorIdleState>();
                    return;
                }

                if (CurState is TownElevatorOpenState)
                {
                    ChangeState<TownElevatorOutIdleState>();
                    return;
                }

                if (CurState is TownElevatorOutIdleState)
                {
                    ChangeState<TownElevatorInIdleState>();
                    return;
                }

                if (CurState is TownElevatorInIdleState)
                {
                    ChangeState<TownElevatorCloseState>();
                    return;
                }

                if (CurState is TownElevatorCloseState)
                {
                    ChangeState<TownElevatorIdleState>();
                    return;
                }
            }
        }

        void GearUpdate(float dt)
        {
            if (TopGear != null)
            {
                TopGear.transform.localEulerAngles += new Vector3(0f, 0f, dt * 150);
            }

            if (BotGear != null)
            {
                BotGear.transform.localEulerAngles -= new Vector3(0f, 0f, dt * 150);
            }
        }

        float SetBlink()
        {
            runingTime = BlinkTimeCircle;

            if (InSide != null)
            {
                float blinkTime = 0.1f;
                int repeat = SBFunc.Random(3, 6);

                InSide.DOKill();
                InSide.color = Color.white;
                InSide.DOColor(Color.gray, blinkTime).SetLoops(repeat).SetEase(Ease.InOutBounce).OnComplete(() =>
                {
                    InSide.color = Color.gray;
                });

                Data.Elevator.DOKill();
                Vector3 originPos = Data.Elevator.transform.localPosition;
                Data.Elevator.transform.DOLocalMoveY(originPos.y + (CurState is TownElevatorMoveUpState ? -0.1f : 0.1f), blinkTime * repeat).SetEase(Ease.InOutBounce).OnComplete(()=> {
                    Data.Elevator.transform.localPosition = originPos;
                });

                DragonsPanic();

                return (blinkTime * repeat) + 5.0f + (5.0f * SBFunc.RandomValue);
            }

            return 0.0f;
        }

        void SetNormal()
        {
            if (InSide != null)
            {
                float blinkTime = 0.1f;
                int repeat = SBFunc.Random(3, 6);

                InSide.DOKill();
                InSide.color = Color.gray;
                InSide.DOColor(Color.white, blinkTime).SetLoops(repeat).SetEase(Ease.InOutBounce).OnComplete(() =>
                {
                    InSide.color = Color.white;
                });
            }
        }

        void DragonsPanic()
        {
            foreach(var dragon in Data.dragons.Keys)
            {
                dragon.Dragon.SubStateCoroutine(DragonsPanicAction(dragon));                
            }
        }

        private IEnumerator DragonsPanicAction(TownStateData dragonState)
        {
            TownDragonSpine dragon = dragonState.Dragon;
            Transform dragonTr = dragon.transform;            
            Vector3 originScale = dragonTr.localScale;

            float _blinkTime = 0.1f;
            int repeat = SBFunc.Random(3, 6);

            Color originColor = new Color(dragon.Skeleton.skeleton.R, dragon.Skeleton.skeleton.G, dragon.Skeleton.skeleton.B);
            DOTween.To(() => originColor, newColor => {
                dragon.Skeleton.skeleton.R = newColor.r;
                dragon.Skeleton.skeleton.G = newColor.g;
                dragon.Skeleton.skeleton.B = newColor.b;
            }, Color.gray, _blinkTime).SetLoops(repeat).SetEase(Ease.InOutBounce).OnComplete(() =>
            {
                Color gray = Color.gray;
                dragon.Skeleton.skeleton.R = gray.r;
                dragon.Skeleton.skeleton.G = gray.g;
                dragon.Skeleton.skeleton.B = gray.b;
            });

            yield return SBDefine.GetWaitForSeconds(_blinkTime);

            yield return new WaitUntil(()=> !dragon.Controller.IsMove);

            dragon.SetEmotion(DragonEmotion.Emotion.LIGHT, DragonEmotion.EmotionColor.RED);

            while (blinkTime > 0.0f)
            {
                float animTime = Mathf.Min(blinkTime, 0.3f + (0.3f * SBFunc.RandomValue));
                float ranVal = SBFunc.RandomValue;
                if (ranVal > 0.4f) //60%
                {
                    dragon.SetAnimation(eSpineAnimation.IDLE);
                    Vector3 scale = dragonTr.localScale;
                    scale.x *= -1.0f;
                    dragonTr.localScale = scale;

                    yield return SBDefine.GetWaitForSeconds(animTime);
                }
                else if(ranVal > 0.1f) //30%
                {
                    dragon.SetAnimation(eSpineAnimation.WALK);
                    
                    float x = data.Elevator.transform.position.x + SBFunc.Random(-SBDefine.ElevatorRandX, SBDefine.ElevatorRandX);
                    if(x > dragonTr.position.x)
                    {
                        Vector3 scale = dragonTr.localScale;
                        scale.x = Mathf.Abs(scale.x);
                        dragonTr.localScale = scale;
                    }
                    else
                    {
                        Vector3 scale = dragonTr.localScale;
                        scale.x = Mathf.Abs(scale.x) * -1.0f;
                        dragonTr.localScale = scale;
                    }
                    yield return dragonTr.DOMoveX(x, animTime).WaitForCompletion();
                }
                else //10%
                {

                    dragon.SetEmotion(DragonEmotion.Emotion.BAD, DragonEmotion.EmotionColor.RED);
                    dragon.SetAnimation(eSpineAnimation.LOSE);
                    yield return SBDefine.GetWaitForSeconds(blinkTime);
                }
            }

            DOTween.To(() => Color.gray, newColor =>
            {
                dragon.Skeleton.skeleton.R = newColor.r;
                dragon.Skeleton.skeleton.G = newColor.g;
                dragon.Skeleton.skeleton.B = newColor.b;
            }, originColor, _blinkTime).SetLoops(repeat).SetEase(Ease.InOutBounce).OnComplete(() =>
            {
                dragon.Skeleton.skeleton.R = originColor.r;
                dragon.Skeleton.skeleton.G = originColor.g;
                dragon.Skeleton.skeleton.B = originColor.b;
            });


            dragon.SetEmotion(DragonEmotion.Emotion.GOOD, DragonEmotion.EmotionColor.YELLOW);
            dragon.SetAnimation(eSpineAnimation.WIN);
            dragonTr.localScale = originScale;

            yield return SBDefine.GetWaitForSeconds(2.0f);

            if(Data.IsContain(dragonState) && dragon.Animation == eSpineAnimation.WIN)
            {
                dragon.SetEmotion(DragonEmotion.Emotion.OK, DragonEmotion.EmotionColor.GREEN);
                dragon.SetAnimation(eSpineAnimation.IDLE);
            }

            yield break;
        }
    }
}
