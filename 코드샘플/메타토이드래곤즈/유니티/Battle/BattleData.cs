using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class BattleData : IBattleData
    {
        public abstract float Speed { get; set; }
        public virtual bool IsAuto { get; set; }
        public abstract float Time { get; set; }
        public abstract eBattleType BattleType { get; }
        public int BattleTag { get; protected set; } = -1;
        public eBattleState State { get; protected set; } = eBattleState.None;
        public abstract int Wave { get; protected set; }
        public abstract int MaxWave { get; protected set; }
        public Dictionary<int, List<BattleCharacter>> OffensePos { get; protected set; } = null;
        public Dictionary<int, List<BattleCharacter>> DefensePos { get; protected set; } = null;
        public List<IBattleCharacterData> Characters { get; protected set; } = null;
        public Dictionary<int, IBattleCharacterData> OffenseDic { get; protected set; } = null;
        public Dictionary<int, IBattleCharacterData> DefenseDic { get; protected set; } = null;
        #region LogData
        public List<Dictionary<float, string>> Logs { get; protected set; } = null;
        public Dictionary<int, Dictionary<string, int>> Damages { get; protected set; } = null;
        #endregion
        protected Dictionary<eBattleSide, BattleSkillQueue> SkillQueue { get; set; } = null;
        public IBattleCharacterData SelectSkillCharacter { get; protected set; } = null;
        public Dictionary<eBattleSide, float> GlobalDelay { get; protected set; } = null;
        public virtual void Initialize()
        {
            Time = 0f;
            BattleTag = -1;
            MaxWave = -1;
            State = eBattleState.None;
            Wave = -1;

            if (OffensePos == null)
                OffensePos = new();
            else
                OffensePos.Clear();

            if (Characters == null)
                Characters = new();
            else
                Characters.Clear();

            if (OffenseDic == null)
                OffenseDic = new();
            else
                OffenseDic.Clear();

            if (DefensePos == null)
                DefensePos = new();
            else
                DefensePos.Clear();

            if (DefenseDic == null)
                DefenseDic = new();
            else
                DefenseDic.Clear();

            if (DefensePos == null)
                DefensePos = new();
            else
                DefensePos.Clear();

            if (DefenseDic == null)
                DefenseDic = new();
            else
                DefenseDic.Clear();

            if (Damages == null)
                Damages = new();
            else
                Damages.Clear();

            if (SkillQueue == null)
                SkillQueue = new();
            else
                SkillQueue.Clear();

            if (GlobalDelay == null)
                GlobalDelay = new();
            else
                GlobalDelay.Clear();

            if (!SkillQueue.ContainsKey(eBattleSide.OffenseSide_1))
                SkillQueue.Add(eBattleSide.OffenseSide_1, new(eBattleSide.OffenseSide_1));
            if (!SkillQueue.ContainsKey(eBattleSide.DefenseSide_1))
                SkillQueue.Add(eBattleSide.DefenseSide_1, new(eBattleSide.DefenseSide_1));
        }
        public virtual void InitializeWave() { }
        public virtual void InitializeReward() { }
        public abstract bool CheckTimeOver();
        public abstract void SetData(JObject jsonData);
        public virtual void SetSelectSkillCharacter(IBattleCharacterData character)
        {
            SelectSkillCharacter = character;
        }
        public virtual void SetGlobalDelay(eBattleSide side, float delay)
        {
            if (GlobalDelay.ContainsKey(side))
                GlobalDelay[side] = delay;
            else
                GlobalDelay.Add(side, delay);
        }
        public virtual float GetGlobalDelay(eBattleSide side)
        {
            if (GlobalDelay.TryGetValue(side, out var value))
                return value;

            return 0f;
        }
        public abstract void Update(float dt);
        public void SkillQueueClear(eBattleSide side)
        {
            if (SkillQueue == null || SkillQueue[side] == null)
                return;

            SkillQueue[side].Clear();
        }
        public void SkillQueueSortCast(eBattleSide side)
        {
            if (SkillQueue == null || SkillQueue[side] == null)
                return;

            SkillQueue[side].SortCast();
        }
        public BattleSkill GetSkill(IBattleCharacterData caster, eBattleSide side)
        {
            if (SkillQueue == null || SkillQueue[side] == null)
                return null;

            var skill = SkillQueue[side].Get();
            if (skill == null || skill.Character != caster)
                return null;

            return skill;
        }
        public void SkillCast(IBattleCharacterData character, eBattleSide side)
        {
            if (SkillQueue == null || SkillQueue[side] == null)
                return;

            SkillQueue[side].Cast(character);

        }
        protected void UpdateGlobalDelay(float dt)
        {
            for (var i = eBattleSide.START; i < eBattleSide.MAX; ++i)
            {
                if (false == GlobalDelay.ContainsKey(i))
                    continue;

                GlobalDelay[i] -= dt;
                if (GlobalDelay[i] <= 0)
                    GlobalDelay[i] = 0;
            }
        }
    }
}