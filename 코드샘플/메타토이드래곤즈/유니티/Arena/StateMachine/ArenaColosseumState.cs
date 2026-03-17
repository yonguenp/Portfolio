using System.Collections.Generic;

namespace SandboxNetwork
{
    public class ArenaColosseumState : BattleState
    {
        public void Set(BattleStage stage, ArenaBattleData data)
        {
            this.Stage = stage;
            this.Data = data;
        }
    }
}