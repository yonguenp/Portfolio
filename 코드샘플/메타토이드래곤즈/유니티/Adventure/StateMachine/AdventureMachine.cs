using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct StateEvent
    {
        public IStateBase e;
        public AdventureMachine machine;

        public StateEvent(IStateBase _Event, AdventureMachine _machine)
        {
            e = _Event;
            machine = _machine;
        }

        static public void Event(IStateBase _event, AdventureMachine _machine)
        {
            EventManager.TriggerEvent(new StateEvent(_event, _machine));
        }
    }
    public class AdventureMachine : SimpleStateMachine<BattleState>
    {
        private AdventureStage Stage { get; set; } = null;
        public AdventureBattleData Data { get; private set; } = null;
        public StageBaseData BaseData { get { return Data.BaseData; } }

        public override BattleState BattleState { get; protected set; }
        public AdventureMachine(AdventureStage stage, AdventureBattleData data)
        {
            Stage = stage;
            Data = data;
        }
        private T CreateState<T>() where T : BattleState, new()
        {
            T state = new();
            state.Set(Stage, Data);
            return state;
        }
        public override void SetState()
        {
            AddState(CreateState<AdventureStateStart>());
            AddState(CreateState<AdventureStateDragonMove>());
            AddState(CreateState<AdventureStateMonsterMove>());
            
            BattleState = CreateState<AdventureStateBattle>();
            AddState(BattleState);
#if DEBUG
            if(User.Instance.UserAccountData.UserNumber < 0)
                AddState(CreateState<SimulatorStateResult>());
            else
#endif
            AddState(CreateState<AdventureStateResult>());
            AddState(CreateState<AdventureStateEnd>());

            ChangeState<AdventureStateStart>();
        }

        public override void StateDataClear()
        {
            if (CurState == null)
                return;

            CurState.Clear();
        }
        public override bool ChangeState(BattleState state)
        {
            if (ScenarioManager.Instance.OnAdventureEvent(state, this))
            {
                Time.timeScale = 1.0f;

                return false;
            }

            if (ScenarioManager.Instance.IsPlaying)
                return false;

            Time.timeScale = AdventureManager.Instance.Data.Speed;

            return base.ChangeState(state);
        }

        public override void Destroy()
        {
            Stage = null;
            Data = null;
            base.Destroy();
        }
    }
}