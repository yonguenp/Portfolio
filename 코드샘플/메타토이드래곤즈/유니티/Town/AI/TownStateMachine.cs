using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class TownStateMachine : SimpleStateMachine<TownAIState>
    {
        public TownStateMachine()
        {
            data = new TownStateData();
        }

        protected TownStateData data = null;
        public TownStateData Data { get { return data; } }

        private T CreateState<T>() where T : TownAIState, new()
        {
            T state = new T();
            state.Set(this, data);
            return state;
        }

        public override void SetState()
        {
            AddState(CreateState<TownIdle>());
            AddState(CreateState<TownMove>());
            AddState(CreateState<TownElevator>());
            AddState(CreateState<TownChitchat>());
            AddState(CreateState<TownJump>());
            AddState(CreateState<TownJumpLoop>());
            AddState(CreateState<TownHappy>());
            AddState(CreateState<TownElevatorFast>());
            AddState(CreateState<TownMoveFast>());
            AddState(CreateState<TownIdleWithTracking>());
            AddState(CreateState<TownSwagger>());
            AddState(CreateState<TownMoveAndRandomSliding>());
            AddState(CreateState<TownMoveCrash>());
            AddState(CreateState<TownSwaggerEffect>());
        }

        public override bool Update(float dt)
        {
            if (CurState == null)
                return false;

            return CurState.Update(dt);
        }

        public void SetNextState()
        {
            if (CurState == null)
            {
                ChangeState<TownIdle>();
            }
            else
            {
                ChangeState(CurState.GetNextState());
            }
        }
    }
}