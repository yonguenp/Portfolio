using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class GuildDragonStateMachine : SimpleStateMachine<GuildDragonState>
    {
        protected GuildDragonSpine Spine { get; set; } = null;
        bool happy = false;
        float time = 0.0f;
        public void SetHappy(bool on)
        {
            happy = on;
            if(on)
                time = 3.0f + (Random.value * 3.0f);
        }

        public void Initialize(GuildDragonSpine spine)
        {
            Spine = spine;
        }
        public override void SetState()
        {
            AddState(CreateState<GuildDragonIdle>());
            AddState(CreateState<GuildDragonHappy>());
            AddState(CreateState<GuildDragonMove>());

            ChangeState<GuildDragonMove>();
        }
        private T CreateState<T>() where T : GuildDragonState, new()
        {
            T state = new();
            state.Initialize(Spine);
            return state;
        }
        public override bool Update(float dt)
        {
            if (happy)
            {
                time -= dt;
                if (time < 0.0f)
                {
                    happy = false;
                }
            }

            if (Spine.IsActive() && false == base.Update(dt))
            {
                if (IsState<GuildDragonIdle>() || IsState<GuildDragonHappy>())
                {
                    var rnd = SBFunc.Random(0, 20);
                    if(Spine.transform.localPosition.x < -2.5f || Spine.transform.localPosition.x > 2.5f)
                    {
                        //화면밖이여서 좀더 잘나오게 수정
                        rnd = SBFunc.Random(0, 30);
                    }

                    if (rnd > 7)
                    {
                        ChangeState<GuildDragonMove>();
                    }
                }
                else
                {
                    if (happy)
                    {
                        ChangeState<GuildDragonHappy>();                        
                    }
                    else
                        ChangeState<GuildDragonIdle>();
                }
                return true;
            }
            return false;
        }
    }
    public abstract class GuildDragonState : StateBase
    {
        protected GuildDragonSpine Spine { get; set; } = null;

        public virtual void Initialize(GuildDragonSpine spine)
        {
            Spine = spine;
        }
    }
    public class GuildDragonIdle : GuildDragonState
    {
        private float Remain { get; set; } = 0f;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                Remain = SBFunc.Random(4f, 10f);
                Spine.SetAnimation(eSpineAnimation.IDLE);
                Spine.SetSpeed(SBDefine.TownDefaultSpeed);
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                Remain -= dt;
                if (Remain > 0f)
                    return true;
                else
                    Remain = SBFunc.Random(4f, 10f);
            }
            return false;
        }
    }
    public class GuildDragonMove : GuildDragonState
    {
        private Vector3 MovePosition = Vector3.zero;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                Spine.SetAnimation(eSpineAnimation.WALK);
                Spine.SetSpeed(SBFunc.Random(65, 250));
                MovePosition = new Vector2( SBFunc.Random(-SBDefine.GuildBuildingSizeFormX, SBDefine.GuildBuildingSizeFormX), SBFunc.Random(-0.3f, -0.24f) );
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                Spine.Controller.MoveLocalTargetUpdate(dt, MovePosition);
                return Vector3.Distance(Spine.transform.localPosition, MovePosition) > 0.05f;
            }
            return false;
        }
    }

    public class GuildDragonHappy : GuildDragonState
    {
        private float Remain { get; set; } = 0f;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                Remain = SBFunc.Random(4f, 10f);
                Spine.SetAnimation(eSpineAnimation.WIN);
                Spine.SetSpeed(SBDefine.TownDefaultSpeed);
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                Remain -= dt;
                if (Remain > 0f)
                    return true;
                else
                    Remain = SBFunc.Random(4f, 10f);
            }
            return false;
        }
    }
}