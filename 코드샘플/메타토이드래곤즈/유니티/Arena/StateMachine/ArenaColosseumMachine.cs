using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ArenaColosseumMachine : SimpleStateMachine<BattleState>
    {
        protected BattleStage stage = null;
        public IBattleData data = null;
        public override BattleState BattleState { get; protected set; }
        public ArenaColosseumMachine(BattleStage stage, IBattleData data)
        {
            this.stage = stage;
            this.data = data;
        }
        public Dictionary<char, ArenaDragonSpine> dragonsDic = null;
        public T CreateState<T>() where T : BattleState, new()
        {
            T t = new();
            t.Set(stage, data);
            return t;
        }
        public override void SetState()
        {
            AddState(CreateState<ArenaColosseumStart>());
            BattleState = CreateState<ArenaColosseumBattle>();
            AddState(BattleState);
            AddState(CreateState<ArenaColosseumEnd>());
        }

        public override void Destroy()
        {
            stage = null;
            data = null;
            base.Destroy();
        }
    }
}