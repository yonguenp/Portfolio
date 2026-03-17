using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossMachine : SimpleStateMachine<BattleState>
    {
        private WorldBossStage Stage { get; set; } = null;
        public WorldBossBattleData Data { get; private set; } = null;
        public StageBaseData BaseData { get { return Data.BaseData; } }
        public override BattleState BattleState { get; protected set; }
        public WorldBossMachine(WorldBossStage stage, WorldBossBattleData data)
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
            AddState(CreateState<WorldBossStateInit>());

            BattleState = CreateState<WorldBossStateBattle>();
            AddState(BattleState);
#if DEBUG
            if(User.Instance.UserAccountData.UserNumber < 0)
                AddState(CreateState<SimulatorStateResult>());
            else
#endif
            AddState(CreateState<WorldBossStateResult>());
            AddState(CreateState<WorldBossStateEnd>());

            ChangeState<WorldBossStateInit>();
        }

        public override void StateDataClear()
        {
            if (CurState == null)
                return;

            CurState.Clear();
        }
        public override bool ChangeState(BattleState state)
        {
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