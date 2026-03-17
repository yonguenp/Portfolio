using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DailyMachine : SimpleStateMachine<DailyState>
    {
        private DailyStage Stage { get; set; } = null;
        private DailyBattleData Data { get; set; } = null;
        public DailyMachine(DailyStage stage, DailyBattleData data)
        {
            Stage = stage;
            Data = data;
        }
        private T CreateState<T>() where T : DailyState, new()
        {
            T state = new();
            state.Set(Stage, Data);
            return state;
        }
        public override void SetState()
        {
            AddState(CreateState<DailyStateStart>());
            AddState(CreateState<DailyStateDragonMove>());
            AddState(CreateState<DailyStateMonsterMove>());
            AddState(CreateState<DailyStateBattle>());
            AddState(CreateState<DailyStateResult>());
            AddState(CreateState<DailyStateEnd>());

            ChangeState<DailyStateStart>();
        }

        public bool Update(float dt)
        {
            if (CurState == null)
                return false;

            return CurState.Update(dt);
        }

        public override void StateDataClear()
        {
            if (CurState == null)
                return;

            CurState.Clear();
        }

        public override void Destroy()
        {
            Stage = null;
            Data = null;
            base.Destroy();
        }
    }
}