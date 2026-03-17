using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonMachine : SimpleStateMachine<BattleState>
    {
        protected BattleStage stage = null;
        private IBattleData data = null;
        public override BattleState BattleState { get; protected set; }
        public int Floor { get; private set; } = 0;

        public GemDungeonMachine(BattleStage stage, IBattleData data, int floor)
        {
            this.stage = stage;
            this.data = data;
            Floor = floor;
        }

        public override void SetState()
        {
            AddState(CreateState<GemDungeonStateIdle>());
            BattleState = CreateState<GemDungeonStateBattle>();
            AddState(BattleState);
            AddState(CreateState<GemDungeonStateEnd>());
            
        }
        public T CreateState<T>() where T : BattleState, new()
        {
            T t = new();
            t.Set(stage, data);
            return t;
        }

        public override void Destroy()
        {
            stage = null;
            data = null;
            base.Destroy();
        }
    }
}

