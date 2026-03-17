using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleColosseumMachine : SimpleStateMachine<BattleState>
    {
        protected BattleStage stage = null;
        public IBattleData data = null;
        public override BattleState BattleState { get; protected set; }
        public ChampionBattleColosseumMachine(BattleStage stage, IBattleData data)
        {
            this.stage = stage;
            this.data = data;
        }
        public Dictionary<char, ChampionBattleDragonSpine> dragonsDic = null;
        public T CreateState<T>() where T : BattleState, new()
        {
            T t = new();
            t.Set(stage, data);
            return t;
        }
        public override void SetState()
        {
            AddState(CreateState<ChampionBattleColosseumStart>());
            BattleState = CreateState<ChampionBattleColosseumBattle>();
            AddState(BattleState);
            AddState(CreateState<ChampionBattleColosseumEnd>());
        }

        public override void Destroy()
        {
            stage = null;
            data = null;
            base.Destroy();
        }
    }

    public class ChampionParacticeColosseumMachine : ChampionBattleColosseumMachine
    {
        public ChampionParacticeColosseumMachine(BattleStage stage, IBattleData data)
            : base(stage, data)
        {
            
        }

        public override void SetState()
        {
            AddState(CreateState<ChampionPracticeColosseumStart>());
            BattleState = CreateState<ChampionPracticeColosseumBattle>();
            AddState(BattleState);
            AddState(CreateState<ChampionPracticeColosseumEnd>());
        }
    }
}