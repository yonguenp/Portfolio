using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public interface IBattleData
    {
        public eBattleType BattleType { get; }//배틀이 많아지면서 변경
        public int BattleTag { get; }
        public bool IsAuto { get; }
        public int Wave { get; }
        public int MaxWave { get; }
        public float Speed { get; }
        public eBattleState State { get; }
        public Dictionary<int, List<BattleCharacter>> OffensePos { get; }
        public Dictionary<int, List<BattleCharacter>> DefensePos { get; }
        public List<IBattleCharacterData> Characters { get; }
        public Dictionary<int, IBattleCharacterData> OffenseDic { get; }
        public Dictionary<int, IBattleCharacterData> DefenseDic { get; }
        public float Time { get; set; }
        public Dictionary<int, Dictionary<string, int>> Damages { get; }
        public void Initialize();
        public void InitializeWave();
        public void InitializeReward();
        public bool CheckTimeOver();
        public void SetData(JObject jsonData);
        public IBattleCharacterData SelectSkillCharacter { get; }
        public void SetSelectSkillCharacter(IBattleCharacterData character);
        public Dictionary<eBattleSide, float> GlobalDelay { get; }
        public void SetGlobalDelay(eBattleSide side, float delay);
        public float GetGlobalDelay(eBattleSide side);
        public void Update(float dt);
        public void SkillQueueClear(eBattleSide side);
        public void SkillQueueSortCast(eBattleSide side);
        public void SkillCast(IBattleCharacterData character, eBattleSide side);
        public BattleSkill GetSkill(IBattleCharacterData caster, eBattleSide side);
    }
}