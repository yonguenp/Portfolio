using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public interface IBattleStage
    {
        public IBattleMap Map { get; }
        public List<List<BattleSpine>> OffenseSpines { get; }
        public List<List<BattleSpine>> DefenseSpines { get; }
        public List<List<BattleSpine>> PrevSpines { get; }
        public BattleSpine GetOffenseSpine(IBattleCharacterData data);
        public BattleSpine GetDefenseSpine(IBattleCharacterData data);
        public void SetGreenHpBar(IBattleCharacterData data);
        public void SetRedHpBar(IBattleCharacterData data);
    }
}