using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public interface IBattleCharacterData
    {
        int Position { get; }
        object Target { get; }
        int ID { get; }
        int Level { get; }
        int SkillLevel { get; }
        int HP { get; set; }
        int MaxHP { get; }
        int MaxShield { get; }
        int PetID { get; }
        bool Death { get; }
        float MOVE_SPEED { get; }
        bool IsEnemy { get; }
        bool IsLeft { get; }
        eElementType Element { get; }
        float Size { get; }
        float WeakDelay { get; }
        float CastingDelay { get; }
        float AfterDelay { get; }
        float NormalDelay { get; }
        float Skill1Delay { get; }
        bool IsActioning { get; }
        bool Untouchable { get; }
        bool IsBoss { get; }
        int SkillRaito { get; }
        eBattleSkillType ActiveSkillType { get; }
        ICharacterBaseData BaseData { get; }
        SkillCharData NormalSkill { get; }
        SkillSummonData NormalSummon { get; }
        List<SkillEffectData> NormalEffect { get; }
        SkillCharData Skill1 { get; }
        SkillSummonData Skill1Summon { get; }
        List<SkillEffectData> Skill1Effect { get; }
        CharacterStatus Stat { get; }
        TranscendenceData TranscendenceData { get; }
        Transform Transform { get; }
        Transform EffectTransform { get; }
        IBattleCharacterData PriorityTarget { get; }

        public List<EffectInfo> Infos { get; }

        void SetData(int pos, JToken token);
        void SetActiveSkilType(eBattleSkillType skillType);
        void SetUntouchable(bool untouchable);
        bool IsWeak();
        bool IsMoveSkip();
        bool IsActionSkip();
        bool IsAttackSkip();
        bool IsTargetSkip();
        bool IsCastingSkip();
        bool IsSkillSkip();
        public bool SetEffectInfo(EffectInfo info);
        public void ReduceEffectInfo(eSkillEffectType type, float timeValue);
        public bool IsEffectInfo(eSkillEffectType type, int nestGroup = -1);
        public bool IsEffectInfo(params eSkillEffectType[] types);
        public void ClearTypeInfo(eSkillEffectType type);
        public void ClearBuffStat();
        public void SetActionCoroutine(IEnumerator time);
        public void EndActionCoroutine();
        public void SetWeakDelay(float time);
        public void SetCastingDelay(float time);
        public void SetAfterDelay(float time);
        public void SetNormalDelay(float time);
        public void SetSkill1Delay(float time);
        public void SetTransform(Transform target);
        public void AddPriorityTarget(IBattleCharacterData target, int value);
        public void DelPriorityTarget(IBattleCharacterData target, int value);
        void Update(float dt);
        BattleSpine GetSpine();
        CircleCollider2D GetCircleCollider();
        List<IBattleCharacterData> GetPriorityTargets(int count, List<IBattleCharacterData> targets = null);

        eDirectionBit KnockBackDirection { get; }
        public bool HasImmunity(params eCharacterImmunity[] immunity);
    }
}