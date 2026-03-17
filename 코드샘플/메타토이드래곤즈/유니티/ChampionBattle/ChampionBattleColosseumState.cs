using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleColosseumState : BattleState
    {
        public void Set(BattleStage stage, ChampionBattleBattleData data)
        {
            this.Stage = stage;
            this.Data = data;
        }
    }
}