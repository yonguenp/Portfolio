using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class EffectInfo
    {
        protected static uint uniqueKey = 0;
        public string Name { get; protected set; } = "";
        public IBattleCharacterData Caster { get; protected set; } = null;
        public IBattleCharacterData Target { get; protected set; } = null;
        public SkillLevelStat Stat { get; protected set; } = null;
        public SkillEffectData Data { get; protected set; } = null;
        public SkillPassiveData Passive { get; protected set; } = null;
        public float Time { get; protected set; } = 0f;
        public float MaxTime { get; protected set; } = 0f;
        protected int SkillLevel { get; set; } = 1;
        protected SBFollowObject FollowEffect { get; set; } = null;
        public bool IsActive { get; protected set; } = false;
        public int NestCount { get; protected set; } = 1;
        protected virtual string NAME 
        { 
            get
            {
                if (Data == null)
                    return "";

                if (Data.NEST_GROUP == 0)
                {
                    ++uniqueKey;
                    return SBFunc.StrBuilder(uniqueKey, "-");
                }
                else
                    return Data.GetSkillName();
            } 
        }
        #region SetData 전용
        public virtual eSkillEffectType EFFECT_TYPE { get => Data != null ? Data.TYPE : eSkillEffectType.NONE; }
        public virtual eStatusType STAT_TYPE { get => Data != null ? Data.STAT_TYPE : eStatusType.NONE; }
        public virtual eStatusValueType VALUE_TYPE { get => Data != null ? Data.VALUE_TYPE : eStatusValueType.NONE; }
        public virtual int NEST_GROUP { get => Data != null ? Data.NEST_GROUP : 0; }
        public virtual int NEST_COUNT { get => Data != null ? Data.NEST_COUNT : 0; }
        public virtual float VALUE { get => Stat != null ? Stat.VALUE : Data != null ? Data.VALUE : 0; }
        public virtual float MAX_TIME { get => Stat != null ? Stat.MAX_TIME : Data != null ? Data.MAX_TIME : 0; }
        #endregion

        public virtual void Update(float dt)
        {
            if (!IsActive && Time >= 0f)
                Trigger();
            Time -= dt;
            if (IsActive && Time <= 0f)
                Complete();
        }
        public virtual void SetData(EffectInfo info)
        {
            if (info == null)
                return;

            if (IsActive)
                CompleteEvent();

            if (info.Passive == null)
                SetEffectData(info.Caster, info.Target, info.Data, info.SkillLevel);
            else
                SetPassiveData(info.Caster, info.Target, info.Passive, info.Data, info.SkillLevel);

            if (IsActive)
                TriggerEvent();
        }
        public virtual void SetEffectData(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData data, int skillLevel)
        {
            if (caster == null || target == null)
                return;

            Caster = caster;
            Target = target;
            Data = data;
            if (Name == "")
            {
                Name = NAME;
            }

            SkillLevel = skillLevel;
            if (Data != null)
                Stat = Data.GetEffectStat(SkillLevel);
            Time = MAX_TIME;
            MaxTime = Time;

            if (IsActive)
                NestCount++;
            else
                NestCount = 1;

            if (NestCount > NEST_COUNT)
                NestCount = NEST_COUNT;

            if (NestCount < 1)
                NestCount = 1;

            if (FollowEffect != null)
            {
                FollowEffect.SetDuration(Time);
            }
        }
        public virtual void SetPassiveData(IBattleCharacterData caster, IBattleCharacterData target, SkillPassiveData passive, SkillEffectData data, int skillLevel)
        {
            if (caster == null || target == null || passive == null)
                return;

            Passive = passive;
            SetEffectData(caster, target, data, skillLevel);
        }
        protected void Trigger()
        {
            IsActive = true;
            TriggerEvent();
        }
        protected void Complete()
        {
            IsActive = false;
            if(FollowEffect != null)
            {
                if (FollowEffect.Effect != null)
                    Object.Destroy(FollowEffect.Effect.gameObject);
                FollowEffect = null;
            }
            CompleteEvent();
        }
        protected abstract void TriggerEvent();
        protected abstract void CompleteEvent();
        public void SetFollowEffect(SBFollowObject follow)
        {
            FollowEffect = follow;
        }
        public virtual bool IsEquals(EffectInfo info)
        {
            if (info.Target != Target)
                return false;

            if (info.EFFECT_TYPE != EFFECT_TYPE)
                return false;

            if (info.NEST_GROUP < 1)
                return false;

            return info.NEST_GROUP == NEST_GROUP;
        }
        public virtual bool IsCover(EffectInfo info)
        {
            if (NestCount > NEST_COUNT)
                return false;

            if (info.GetValue() >= GetValueDefault())
                return true;

            return false;
        }
        public virtual float GetValue()
        {
            return GetValueDefault() * NestCount;
        }
        protected virtual float GetValueDefault()
        {
            if (Stat == null && Data != null)
                Stat = Data.GetEffectStat(SkillLevel);
            if (Stat != null)
                return Stat.VALUE;

            return Data.VALUE;
        }
        public virtual int SetDamage(int damage)
        {
            return damage;
        }
        public virtual void TimeEnd()
        {
            if (IsActive)
                Time = 0f;
        }
        public virtual void ReduceTime(float value)
        {
            if (IsActive)
            {
                Time -= MaxTime * value;
                if (Time < 0f)
                    Time = 0f;
            }
        }
        public virtual bool IsEffectEnd()
        {
            return FollowEffect == null || FollowEffect.Effect == null || FollowEffect.IsEnd;
        }
    }
}