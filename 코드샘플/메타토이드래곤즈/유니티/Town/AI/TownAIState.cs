using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SandboxNetwork
{
    public abstract class TownAIState : StateBase
    {
        protected TownStateData data = null;
        protected TownStateMachine machine = null;
        protected Tween tween = null;
        protected bool IsDragon { get { return data != null && data.Dragon != null; } }
        public virtual void Set(TownStateMachine machine, TownStateData data)
        {
            this.machine = machine;
            this.data = data;
        }
        public override bool OnEnter()
        {
            return base.OnEnter() && IsDragon;
        }
        public override bool OnExit()
        {
            return base.OnExit() && IsDragon;
        }
        public override bool Update(float dt)
        {
            return base.Update(dt) && IsDragon;
        }

        public virtual TownAIState GetNextState()
        {
            return machine.GetState<TownIdle>();
        }

        public void TweenClear()
        {
            if (tween != null)
                tween.Kill();
            tween = null;
        }
    }
    public class TownStateData
    {
        private TownDragonSpine dragon = null;
        public TownDragonSpine Dragon
        {
            get { return dragon; }
            set { dragon = value; }
        }
        private Vector3Int curCell = Vector3Int.zero;
        public Vector3Int CurCell
        {
            get { return curCell; }
            set { curCell = value; }
        }
        private Vector3Int targetCell = Vector3Int.zero;
        public Vector3Int TargetCell
        {
            get { return targetCell; }
            set { targetCell = value; }
        }
    }
    public class TownChitchat : TownAIState
    {
        public enum CHITCHAT_TYPE
        { 
            NORMAL,
            BOOLY,
            VICTIM,
            OTHER,
        }

        public const float NEAR_MAX_DISTANCE = 0.8f;
        public const float NEAR_MIN_DISTANCE = 0.3f;

        CHITCHAT_TYPE type = CHITCHAT_TYPE.NORMAL;
        Vector3 originalScale = Vector3.zero;        
        private bool isDone = false;

        int count = SBFunc.Random(5, 15);
        public void SetChitChatCount(CHITCHAT_TYPE t, int c)
        {
            type = t;
            count = c;
        }

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed * 3);
                data.Dragon.Controller.StopCO();
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                originalScale = data.Dragon.Skeleton.transform.localScale;

                isDone = false;
                data.Dragon.SubStateCoroutine(ChatAction());
                
                return true;
            }
            return false;
        }

        public override bool OnExit()
        {
            if (base.OnExit())
            {
                TweenClear();

                data.Dragon.SubStateCoroutine(null);
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed);
                data.Dragon.Skeleton.transform.localScale = originalScale;

                return true;
            }
            return false;
        }

        private IEnumerator ChatAction()
        {
            TweenClear();

            while (count > 0)
            {
                tween = data.Dragon.Skeleton.transform.DOScaleY(originalScale.y * 1.1f, 0.3f);
                tween.SetAutoKill(false);
                yield return tween.WaitForCompletion();
                
                tween.PlayBackwards();
                if(count % 2 == 1)
                    data.Dragon.SetRandomEmotion();

                yield return tween.WaitForCompletion();

                TweenClear();
                count--;
            }

            TweenClear();

            Spine.TrackEntry anim = null;
            switch (type)
            {
                case CHITCHAT_TYPE.NORMAL:
                    data.Dragon.SetEmotion(DragonEmotion.Emotion.RANDOM, DragonEmotion.EmotionColor.RANDOM);
                    break;
                case CHITCHAT_TYPE.BOOLY:
                    data.Dragon.SetEmotion(DragonEmotion.Emotion.SURPRISE, DragonEmotion.EmotionColor.RED);

                    anim = data.Dragon.SetAnimation(eSpineAnimation.A_CASTING);
                    if (anim != null)
                        yield return new Spine.Unity.WaitForSpineAnimationComplete(anim);

                    anim = data.Dragon.SetAnimation(eSpineAnimation.ATTACK);
                    if (anim != null)
                        yield return new Spine.Unity.WaitForSpineAnimationComplete(anim);

                    yield return SBDefine.GetWaitForSeconds(0.5f);

                    break;
                case CHITCHAT_TYPE.VICTIM:
                    data.Dragon.SetEmotion(DragonEmotion.Emotion.SAD, DragonEmotion.EmotionColor.ORANGE);

                    anim = data.Dragon.SetAnimation(eSpineAnimation.LOSE);
                    if (anim != null)
                    {
                        yield return new Spine.Unity.WaitForSpineAnimationComplete(anim);

                        yield return SBDefine.GetWaitForSeconds(SBFunc.Random(3.0f, 10.0f));
                    }
                    break;
                case CHITCHAT_TYPE.OTHER:
                    data.Dragon.SetEmotion(DragonEmotion.Emotion.LIGHT, DragonEmotion.EmotionColor.RANDOM);
                    break;
            }

            isDone = true;
            data.Dragon.SetForceAnimation(eSpineAnimation.IDLE);            
        }

        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                if (isDone)
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public override TownAIState GetNextState()
        {
            switch (type)
            {
                case CHITCHAT_TYPE.NORMAL:
                    return machine.GetState<TownIdle>();
                case CHITCHAT_TYPE.BOOLY:
                    return machine.GetState<TownMoveAndRandomSliding>();
                case CHITCHAT_TYPE.VICTIM:
                    return machine.GetState<TownIdle>();
                case CHITCHAT_TYPE.OTHER:
                    return machine.GetState<TownMoveAndRandomSliding>();
            }

            return machine.GetState<TownIdle>();
        }
    }

    public class TownJump : TownAIState
    {
        Vector3 originalPos = Vector3.zero;
        
        protected virtual int GetJumpCount()
        {
            return SBFunc.Random(2, 5);
        }
        protected virtual void SetEmotion()
        {
            data.Dragon.SetRandomEmotion();
        }

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                TweenClear();

                int jump = GetJumpCount();

                //data.Dragon.SetSpeed(0); 무의미한짓
                data.Dragon.Controller.StopCO();
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                originalPos = data.Dragon.Skeleton.transform.localPosition;

                Sequence seq = DOTween.Sequence();
                for (int i = 0; i < jump; i++)
                {
                    seq.Append(data.Dragon.Skeleton.transform.DOLocalMoveY(originalPos.y + 0.125f, 0.3f));
                    seq.Append(data.Dragon.Skeleton.transform.DOLocalMoveY(originalPos.y, 0.3f));
                }

                tween = seq;
                SetEmotion();

                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                TweenClear();

                data.Dragon.Skeleton.transform.localPosition = originalPos;
                
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                if (tween == null || tween.active == false || !tween.IsPlaying())
                {
                    return false;
                }

                return true;
            }
            return false;
        }
    }

    public class TownSwaggerEffect : TownJump
    {
        protected override int GetJumpCount()
        {
            return 1;
        }
        protected override void SetEmotion()
        {
            data.Dragon.SetEmotion(DragonEmotion.Emotion.HEART, DragonEmotion.EmotionColor.YELLOW);
        }
        public override TownAIState GetNextState()
        {
            return machine.GetState<TownHappy>();
        }
    }

    public class TownJumpLoop : TownJump
    {
        protected override int GetJumpCount()
        {
            return -1;
        }
    }

    public class TownHappy : TownAIState
    {
        private float curTime = 0.0f;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed);
                data.Dragon.Controller.StopCO();
                data.Dragon.SetAnimation(eSpineAnimation.WIN);
                data.Dragon.SetEmotion(DragonEmotion.Emotion.HAPPY, DragonEmotion.EmotionColor.RANDOM);

                curTime = 3.0f;
                return true;
            }

            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                curTime = 0f;
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                curTime -= dt;
                if (curTime < 0f)
                {
                    data.Dragon.SetForceAnimation(eSpineAnimation.IDLE);
                    return false;
                }
                return true;
            }
            return false;
        }
    }

    public class TownSwagger : TownAIState
    {
        public const float NEAR_MAX_DISTANCE = 2.0f;
        public const float NEAR_MIN_DISTANCE = 0.0f;

        private bool isDone = false;

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                isDone = false;

                data.Dragon.Controller.StopCO();
                
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed);
                data.Dragon.SetEmotion(DragonEmotion.Emotion.STAR, DragonEmotion.EmotionColor.YELLOW);
                data.Dragon.SubStateCoroutine(SwaggerAction());
                return true;
            }
            return false;
        }

        private IEnumerator SwaggerAction()
        {
            Spine.TrackEntry anim = data.Dragon.SetAnimation(eSpineAnimation.CASTING);
            if (anim != null)
            {
                data.Dragon.OnSwag();
                yield return new Spine.Unity.WaitForSpineAnimationComplete(anim);
            }

            anim = data.Dragon.SetAnimation(eSpineAnimation.SKILL);
            if (anim != null)
            {
                yield return new Spine.Unity.WaitForSpineAnimationComplete(anim);
            }

            anim = data.Dragon.SetAnimation(eSpineAnimation.WIN);
            if (anim != null)
            {
                yield return new WaitForSeconds(1.0f);
            }

            data.Dragon.SetForceAnimation(eSpineAnimation.IDLE);
            isDone = true;
        }

        public override bool OnExit()
        {
            if (base.OnExit())
            {
                data.Dragon.SubStateCoroutine(null);
                isDone = true;

                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return !isDone;
            }
            return false;
        }
    }

    public class TownIdle : TownAIState
    {
        protected float curTime = 0f;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                curTime = 0f;
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed);
                data.Dragon.Controller.StopCO();
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                data.Dragon.OffDust();

                if(data.Dragon.Skeleton != null )
                {
                    data.Dragon.Skeleton.transform.localPosition = Vector3.zero;
                }
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                curTime = 0f;
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed);
                data.Dragon.Controller.StopCO();
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return Idle(dt);
            }
            return false;
        }

        protected virtual bool Idle(float dt)
        {
            curTime -= dt;
            if (curTime < 0f)
            {
                curTime = SBFunc.Random(1f, 5f);
                var curSpeed = SBFunc.Random(50, 80);
                data.Dragon.SetSpeed(curSpeed);

                if (SBFunc.Random(0, 10) < 4)
                {
                    var targetPos = TownMap.GetRandomCellPos(data.CurCell.x, data.CurCell.y);
                    targetPos.y = data.Dragon.transform.localPosition.y;
                    data.Dragon.Controller.MoveLocalTarget(targetPos);
                }
            }

            if (data.Dragon.Controller.IsMove)
                data.Dragon.SetAnimation(eSpineAnimation.WALK);
            else
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);

            return true;
        }
    }

    public class TownIdleWithTracking : TownIdle
    {
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                curTime = 10.0f;
                return true;
            }
            return false;
        }

        protected override bool Idle(float dt)
        {
            curTime -= dt;
            if (curTime < 0f)
            {
                curTime = SBFunc.Random(10f, 15f);
                var curSpeed = SBFunc.Random(50, 80);
                data.Dragon.SetSpeed(curSpeed);

                var offset = SBFunc.Random(0.1f, 0.5f);
                if (SBFunc.RandomValue > 0.5f)
                    offset *= -1.0f;

                var targetPos = TownMap.GetCellPos(data.CurCell.x, data.CurCell.y);
                targetPos.y = data.Dragon.transform.localPosition.y;

                targetPos.x += offset * SBDefine.CellSpancing;

                //data.Dragon.transform.localScale = new Vector3(targetPos.x > data.Dragon.transform.localPosition.x ? Mathf.Abs(data.Dragon.transform.localScale.x) : -Mathf.Abs(data.Dragon.transform.localScale.x), data.Dragon.transform.localScale.y, data.Dragon.transform.localScale.z);

                data.Dragon.Controller.MoveLocalTarget(targetPos);
            }

            if (data.Dragon.Controller.IsMove)
                data.Dragon.SetAnimation(eSpineAnimation.WALK);
            else
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);

            return true;
        }
    }

    public class TownMove : TownAIState
    {
        protected Vector2 targetPos = Vector2.zero;
        protected bool moveDone = false;
        protected virtual int GetSpeed()
        {
            int speed = 0;
            var rnd = SBFunc.Random(0, 10);
            if (rnd < 2)
            {
                speed = SBFunc.Random(110, 180);
            }
            else if (rnd < 3)
            {
                speed = SBFunc.Random(50, 90);
            }
            else
            {
                speed = SBDefine.TownDefaultSpeed;
            }

            return speed;
        }

        protected virtual void SetTargetPos()
        {
            targetPos = TownMap.GetRandomCellPos(data.TargetCell.x, data.TargetCell.y);
            targetPos.y = data.Dragon.transform.localPosition.y;            
        }

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                SetTargetPos();

                var speed = GetSpeed();
                
                data.Dragon.Controller.StopCO();
                data.Dragon.SetSpeed(speed);

                moveDone = false;

                data.Dragon.SetAnimation(eSpineAnimation.WALK);
                data.Dragon.Controller.MoveLocalTarget(targetPos, OnMoveDone);
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed);
                data.Dragon.Controller.StopCO();
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                return true;
            }
            return false;
        }

        public virtual void OnMoveDone()
        {
            data.Dragon.SetAnimation(eSpineAnimation.IDLE);
            moveDone = true;
        }

        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return !moveDone;
            }
            return false;
        }
    }
    public class TownMoveFast : TownMove
    {
        protected override int GetSpeed()
        {
            return SBFunc.Random(110, 180);
        }

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                bool isRight = targetPos.x > data.Dragon.transform.position.x;

                data.Dragon.OnDust(isRight);
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                data.Dragon.OffDust();
                return true;
            }
            return false;
        }
    }

    public class TownMoveCrash : TownMoveFast
    {
        public const float CRASHABLE_DISTANCE = 1.2f;
        public const float BOUND_DISATANCE = 0.6f;
        protected override int GetSpeed()
        {
            return 170;
        }

        private float crashPosX = 0.0f;
        private float boundPosX { get { return crashPosX - (BOUND_DISATANCE * data.Dragon.transform.localScale.x); } }
        private Vector2 boundPos { get { return new Vector2(boundPosX, data.Dragon.transform.localPosition.y); } }

        TownDragonSpine targetSpine = null;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                TweenClear();
                return true;
            }
            return false;
        }

        public override bool OnExit()
        {
            if (base.OnExit())
            {
                data.Dragon.SubStateCoroutine(null);

                TweenClear();
                return true;
            }
            return false;
        }


        public void SetCrashPosX(float posX, TownDragonSpine target)
        {
            crashPosX = posX;
            targetSpine = target;
        }
        protected override void SetTargetPos()
        {
            targetPos = new Vector2(crashPosX, data.Dragon.transform.localPosition.y);
        }

        public override void OnMoveDone()
        {
            data.Dragon.OffDust();

            TweenClear();

            data.Dragon.SubStateCoroutine(StartCrashAction());            
        }

        private IEnumerator StartCrashAction()
        {
            TweenClear();

            float power = 0.0f;
            if (targetSpine != null)
            {
                switch(targetSpine.Data.Grade() - data.Dragon.Data.Grade())
                {
                    case -4:
                        power += 0.0f;
                        break;
                    case -3:
                        power += 0.1f;
                        break;
                    case -2:
                        power += 0.15f;
                        break;
                    case -1:
                        power += 0.2f;
                        break;
                    case 0:
                        power += 0.25f;
                        break;
                    case 1:
                        power += 0.3f;
                        break;
                    case 2:
                        power += 0.35f;
                        break;
                    case 3:
                        power += 0.4f;
                        break;
                    case 4:
                        power += 0.45f;
                        break;
                }

                switch(targetSpine.Data.TranscendenceStep - data.Dragon.Data.TranscendenceStep)
                {
                    case -3:
                        power += -0.25f;
                        break;
                    case -2:
                        power += -0.15f;
                        break;
                    case -1:
                        power += -0.1f;
                        break;
                    case 0:
                        power += 0.0f;
                        break;
                    case 1:
                        power += 0.05f;
                        break;
                    case 2:
                        power += 0.1f;
                        break;
                    case 3:
                        power += 0.15f;
                        break;                    
                }
            }

            if (power > 0.0f)
            {
                data.Dragon.SetEmotion(power < 0.5f ? DragonEmotion.Emotion.BAD : DragonEmotion.Emotion.SAD, DragonEmotion.EmotionColor.RANDOM);

                data.Dragon.SetAnimation(eSpineAnimation.LOSE);

                var seq = DOTween.Sequence();
                seq.Append(data.Dragon.Skeleton.transform.DOLocalJump(data.Dragon.Skeleton.transform.localPosition, power, 2, power));
                seq.Join(data.Dragon.transform.DOLocalMove(boundPos, power));
                
                tween = seq;

                yield return tween.WaitForCompletion();

                TweenClear();

                yield return SBDefine.GetWaitForSeconds(power * 4.0f);

                moveDone = true;
            }
            else
            {
                data.Dragon.SetEmotion(DragonEmotion.Emotion.QUESTION, DragonEmotion.EmotionColor.RANDOM);

                data.Dragon.SetAnimation(eSpineAnimation.WALK);
                var pos = data.Dragon.transform.localPosition;
                pos.x += (BOUND_DISATANCE * 0.4f) * data.Dragon.transform.localScale.x; 

                data.Dragon.Controller.MoveLocalTarget(pos, ()=> { moveDone = true; });
            }
        }
    }

    public class TownMoveAndRandomSliding : TownAIState
    {
        public TownMoveAndRandomSliding()
        {
            posQueue = new Queue<eDragonTownActionType>();
        }
        public eDragonTownActionType CurState
        {
            get { return posQueue.Peek(); }
        }

        private Queue<eDragonTownActionType> posQueue = null;
        private Vector2 targetPos;
        bool isRight = false;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                posQueue.Clear();
                var speed = SBFunc.Random(150, 230);
                data.Dragon.Controller.StopCO();
                data.Dragon.SetSpeed(speed);
                targetPos = SBFunc.Random(0, 2) < 1 ? TownMap.GetRightCellPos(data.TargetCell.x, data.TargetCell.y) + Vector2.left : TownMap.GetLeftCellPos(data.TargetCell.x, data.TargetCell.y) + Vector2.right;
                isRight = targetPos.x > data.Dragon.transform.position.x;
                posQueue.Enqueue(eDragonTownActionType.RunFast);
                if (SBFunc.Random(0, 3) < 1)
                {
                    posQueue.Enqueue(eDragonTownActionType.FallState);
                    posQueue.Enqueue(eDragonTownActionType.ShakeLeftAndRight);
                }
                posQueue.Enqueue(eDragonTownActionType.None);

                data.Dragon.SubStateCoroutine(StartQueueAction());
                return true;
            }
            return false;
        }
        private IEnumerator StartQueueAction()
        {
            while (posQueue.Count > 0)
            {
                if (data.Dragon.Controller.IsMove)
                {
                    yield return null;
                    continue;
                }

                switch (CurState)
                {
                    case eDragonTownActionType.RunFast:
                    {
                        data.Dragon.SetEmotion(DragonEmotion.Emotion.HAPPY, DragonEmotion.EmotionColor.RANDOM);

                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        targetPos.y = data.Dragon.transform.localPosition.y;
                        data.Dragon.Controller.MoveLocalTarget(targetPos, ()=> {
                            posQueue.Dequeue();
                        });
                        data.Dragon.OnDust(isRight);                        

                        while(CurState == eDragonTownActionType.RunFast)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    break;
                    case eDragonTownActionType.FallState:
                    {
                        data.Dragon.SetEmotion(DragonEmotion.Emotion.SURPRISE, DragonEmotion.EmotionColor.RANDOM);

                        data.Dragon.OffDust();
                        data.Dragon.Controller.MoveLocalTarget(targetPos + (isRight ? Vector2.right : Vector2.left), ()=> {
                            posQueue.Dequeue();
                        });
                        data.Dragon.MixAnim(SBDefine.GetDragonAnimTypeToName(eSpineAnimation.IDLE), SBDefine.GetDragonAnimTypeToName(eSpineAnimation.LOSE), 0.7f, 1f, 1.5f);

                        while (CurState == eDragonTownActionType.FallState)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    break;
                    case eDragonTownActionType.ShakeLeftAndRight:
                    {
                        data.Dragon.SetEmotion(DragonEmotion.Emotion.SAD, DragonEmotion.EmotionColor.RANDOM);

                        data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                        int doridoriCount = SBFunc.Random(2, 3);
                        Transform dragonTr = data.Dragon.transform;                        
                        for (int i = 0; i < doridoriCount; ++i)
                        {
                            dragonTr.localScale = new Vector3(-1, 1, 1);
                            yield return SBDefine.GetWaitForSeconds(0.25f);
                            dragonTr.localScale = Vector3.one;
                            yield return SBDefine.GetWaitForSeconds(0.25f);
                        }

                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonTownActionType.None:
                    default:
                    {
                        data.Dragon.SubStateCoroutine(null);
                        yield break;
                    }
                }
                yield return null;
            }

            data.Dragon.SubStateCoroutine(null);
            yield break;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return CurState != eDragonTownActionType.None;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed);
                data.Dragon.Controller.StopCO();
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                data.CurCell = data.TargetCell;
                data.Dragon.OffDust();
                return true;
            }
            return false;
        }
    }
    
    public class TownElevator : TownAIState
    {
        protected Elevator elevator = null;
        protected Escalator escalator = null;
        protected Queue<eDragonElevatorStateType> posQueue = new Queue<eDragonElevatorStateType>();
        public eDragonElevatorStateType CurState
        {
            get { return posQueue.Peek(); }
        }
        protected eElevatorType elevatorType = eElevatorType.None;

        protected virtual int GetSpeed()
        {
            var speed = 0;
            var rnd = SBFunc.Random(0, 10);
            if (rnd < 2)
            {
                speed = SBFunc.Random(110, 180);
            }
            else if (rnd < 3)
            {
                speed = SBFunc.Random(50, 90);
            }
            else
            {
                speed = SBDefine.TownDefaultSpeed;
            }

            return speed;
        }

        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                posQueue.Clear();

                var speed = GetSpeed();

                data.Dragon.Controller.StopCO();
                data.Dragon.SetSpeed(speed);
                
                var isRight = data.Dragon.transform.localPosition.x >= 0;
                elevatorType = isRight ? eElevatorType.Right : eElevatorType.Left;
                elevator = Town.Instance.GetElevator(isRight);
                escalator = Town.Instance.GetEscalator(isRight);
                if (data.CurCell.z == SBDefine.UnderFrontOrder)
                {
                    posQueue.Enqueue(eDragonElevatorStateType.InEscalatorStartMove);
                    posQueue.Enqueue(eDragonElevatorStateType.InEscalatorEndMove);
                }
                posQueue.Enqueue(eDragonElevatorStateType.InMove);
                posQueue.Enqueue(eDragonElevatorStateType.InCall);
                posQueue.Enqueue(eDragonElevatorStateType.In);
                posQueue.Enqueue(eDragonElevatorStateType.InOrderMove);
                posQueue.Enqueue(eDragonElevatorStateType.InContainMove);
                posQueue.Enqueue(eDragonElevatorStateType.Contain);
                posQueue.Enqueue(eDragonElevatorStateType.ElevatorMove);
                posQueue.Enqueue(eDragonElevatorStateType.ExitOrderMove);
                posQueue.Enqueue(eDragonElevatorStateType.ExitContainMove);
                if (data.TargetCell.z == SBDefine.UnderFrontOrder)
                {
                    posQueue.Enqueue(eDragonElevatorStateType.ExitEscalatorStartMove);
                    posQueue.Enqueue(eDragonElevatorStateType.ExitEscalatorEndMove);
                }
                posQueue.Enqueue(eDragonElevatorStateType.ExitMove);
                posQueue.Enqueue(eDragonElevatorStateType.Exit);
                
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                elevator.Pop(data);
                data.Dragon.SetSpeed(SBDefine.TownDefaultSpeed);
                data.Dragon.Controller.StopCO();
                data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                elevator = null;
                escalator = null;
                posQueue.Clear();
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                return ElevatorUpdate();
            }
            return false;
        }
        

        public bool ElevatorUpdate()
        {
            if (posQueue.Count > 0)
            {
                if (data.Dragon.Controller.IsMove)
                {
                    return true;
                }

                switch (CurState)
                {
                    case eDragonElevatorStateType.InEscalatorStartMove:
                    {
                        data.Dragon.SetEmotion(DragonEmotion.Emotion.RANDOM, DragonEmotion.EmotionColor.RANDOM);

                        var pos = escalator.GetEndPos();
                        pos.z = 0f;
                        data.Dragon.Controller.MoveWorldTarget(pos);
                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.InEscalatorEndMove:
                    {
                        var pos = escalator.GetStartPos();
                        pos.z = 0f;
                        data.Dragon.Controller.MoveWorldTarget(pos, 0f, true, 70);
                        data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.InMove:
                    {
                        var pos = new Vector3();
                        pos.x = elevator.transform.position.x + (elevatorType == eElevatorType.Right ? -SBDefine.ElevatorInOutX : SBDefine.ElevatorInOutX);
                        pos.y = data.Dragon.transform.position.y;
                        pos.z = 0f;
                        data.Dragon.Controller.MoveWorldTarget(pos);
                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.InCall:
                    {
                        elevator.PushNeed(data);
                        data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.In:
                    {
                        if (elevator.IsState<TownElevatorInIdleState>() && elevator.Data.IsInDragon(data))
                        {
                            posQueue.Dequeue();
                        }
                    }
                    break;
                    case eDragonElevatorStateType.InOrderMove:
                    {
                        data.Dragon.SetEmotion(DragonEmotion.Emotion.RANDOM, DragonEmotion.EmotionColor.RANDOM);

                        var pos = new Vector3();
                        pos.x = elevator.transform.position.x + (elevatorType == eElevatorType.Right ? -SBDefine.ElevatorOrderX : SBDefine.ElevatorOrderX);
                        pos.y = data.Dragon.transform.position.y;
                        pos.z = 0f;
                        data.Dragon.Controller.MoveWorldTarget(pos);
                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.InContainMove:
                    {
                        var pos = new Vector3();
                        pos.x = elevator.transform.position.x + SBFunc.Random(-SBDefine.ElevatorRandX, SBDefine.ElevatorRandX);
                        pos.y = data.Dragon.transform.position.y;
                        pos.z = 0f;
                        elevator.Push(data);
                        data.Dragon.Controller.MoveWorldTarget(pos);
                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        data.Dragon.SetTownOrder(SBDefine.DefaultOrder);

                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.Contain:
                    {
                        data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                        data.Dragon.transform.localScale = new Vector3(data.Dragon.transform.localScale.x * -1.0f, data.Dragon.transform.localScale.y, data.Dragon.transform.localScale.z);
                        elevator.PopNeed(data);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.ElevatorMove:
                    {
                        if (elevator.Data.CurFloor != data.TargetCell.y)
                        {
                            return true;
                        }
                        if (elevator.IsState<TownElevatorOutIdleState>())
                        {
                            posQueue.Dequeue();
                        }
                    }
                    break;
                    case eDragonElevatorStateType.ExitOrderMove:
                    {
                        var pos = Vector3.zero;
                        pos.x = elevator.transform.position.x + (elevatorType == eElevatorType.Right ? -SBDefine.ElevatorOrderX : SBDefine.ElevatorOrderX);
                        pos.y = data.Dragon.transform.position.y;

                        data.Dragon.Controller.MoveWorldTarget(pos);
                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.ExitContainMove:
                    {
                        var pos = Vector3.zero;
                        pos.x = elevator.transform.position.x + (elevatorType == eElevatorType.Right ? -SBDefine.ElevatorInOutX : SBDefine.ElevatorInOutX);
                        pos.y = data.Dragon.transform.position.y;

                        elevator.Pop(data);
                        data.Dragon.Controller.MoveWorldTarget(pos);
                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        data.Dragon.SetTownOrder(data.TargetCell.z);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.ExitEscalatorStartMove:
                    {
                        var pos = escalator.GetStartPos();
                        pos.z = 0f;
                        data.Dragon.Controller.MoveWorldTarget(pos, 0f, false);
                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.ExitEscalatorEndMove:
                    {
                        var pos = escalator.GetEndPos();
                        pos.z = 0f;
                        data.Dragon.Controller.MoveWorldTarget(pos, 0f, true, 70);
                        data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.ExitMove:
                    {
                        var pos = data.Dragon.transform.position;
                        pos.x = SBFunc.Random(0, 2) < 1 ? TownMap.GetRightCellPos(data.TargetCell.x, data.TargetCell.y).x : TownMap.GetLeftCellPos(data.TargetCell.x, data.TargetCell.y).x;
                        data.Dragon.Controller.MoveWorldTarget(pos);
                        data.Dragon.SetAnimation(eSpineAnimation.WALK);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.Exit:
                    {
                        data.CurCell = data.TargetCell;
                        data.Dragon.SetAnimation(eSpineAnimation.IDLE);
                        posQueue.Dequeue();
                    }
                    break;
                    case eDragonElevatorStateType.None:
                    default:
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }
    }

    public class TownElevatorFast : TownElevator
    {
        protected override int GetSpeed()
        {
            return SBFunc.Random(110, 180);
        }
    }
}