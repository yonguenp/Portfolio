using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public delegate void SBLandEvent(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData effect, SBSkill skill, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>> passives, bool isPassiveStart);
    public class SBLand : MonoBehaviour
    {
        protected IBattleData battleData = null;
        protected SBObject sbObject = null;
        protected List<IBattleCharacterData> isExist = new List<IBattleCharacterData>();
        protected Dictionary<IBattleCharacterData, Dictionary<eSkillPassiveStartType, List<SkillPassiveData>>> activePassive = new ();
        IBattleCharacterData caster = null;
        protected SBSkill skill = null;
        protected int idx = 0;
        SBLandEvent triggerEvent = null;
        public float Last { get; protected set; } = 0f;
        public float Time { get; protected set; } = 0f;
        public float MaxTime { get; protected set; } = 0f;
        public bool IsActive { get; protected set; } = false;
        protected bool IsEnd { get => Time <= 0f; }

        public void SetLandData(IBattleData battleData, IBattleCharacterData caster, SBSkill skill, SBLandEvent triggerEvent, int idx)
        {
            if (battleData == null || caster == null || skill == null || skill.GetSummon(idx) == null)
                return;
            this.battleData = battleData;
            this.caster = caster;
            this.skill = skill;
            this.idx = idx;
            this.triggerEvent = triggerEvent;
            InitializeSkill();
        }
        protected void InitializeSkill()
        {
            if (skill == null)
                return;

            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            MaxTime = summon.VALUE1;
            Time = summon.VALUE1;
            Last = summon.VALUE1;
            if (sbObject == null)
            {
                switch (summon.RANGE_TYPE)
                {
                    case eSkillRangeType.CIRCLE_C:
                        sbObject = new Circle(GetPosition(), summon.RANGE_X, summon.RANGE_Y);
                        break;
                    case eSkillRangeType.CIRCLE_F:
                        sbObject = new Circle(GetPosition(), summon.RANGE_X, summon.RANGE_Y);
                        sbObject.SetDirection(caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                        break;
                    case eSkillRangeType.SQUARE_C:
                        sbObject = new SBRect(GetPosition(), summon.RANGE_X, summon.RANGE_Y);
                        break;
                    case eSkillRangeType.SQUARE_F:
                        sbObject = new SBRect(GetPosition(), summon.RANGE_X, summon.RANGE_Y);
                        sbObject.SetDirection(caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right);
                        break;
                    case eSkillRangeType.SECTOR_F:
                        sbObject = new Cone(GetPosition(), caster.IsEnemy ? eDirectionBit.Left : eDirectionBit.Right, summon.RANGE_X, summon.RANGE_Y);
                        break;
                    default:
                        return;
                }
            }
        }
        void FixedUpdate()
        {
            if (!IsActive && Time >= 0f)
                Trigger();

            Time -= SBGameManager.Instance.DTime;
            TargetCheckEvent();

            if (IsActive && Time <= 0f)
                Complete();
        }
        private void Trigger()
        {
            IsActive = true;
        }
        private void Complete()
        {
            IsActive = false;
            Destroy(gameObject);
        }
        protected void TargetCheckEvent()
        {
            if (battleData == null || sbObject == null)
                return;

            var summon = skill.GetSummon(idx);
            if (summon == null)
                return;

            var tickTime = Last - Time;
            if (summon.VALUE2 > tickTime)
                return;

            Last = Time;
            var tick = Mathf.FloorToInt(tickTime / summon.VALUE2);

            if(sbObject.Position != GetPosition())
            {
                sbObject.SetPosition(GetPosition());
                sbObject.Refresh();
            }

            isExist.Clear();
            foreach (var info in battleData.OffenseDic)
            {
                var characterData = info.Value;
                if (characterData == null || characterData.Death || characterData.Transform == null)
                    continue;

                if (summon.TARGET_TYPE.IsTarget(caster, characterData) && sbObject.IsContain(characterData.Transform.position))
                    TargetEvent(characterData, tick);
            }

            foreach (var info in battleData.DefenseDic)
            {
                var characterData = info.Value;
                if (characterData == null || characterData.Death || characterData.Transform == null)
                    continue;

                if (summon.TARGET_TYPE.IsTarget(caster, characterData) && sbObject.IsContain(characterData.Transform.position))
                    TargetEvent(characterData, tick);
            }
        }

        protected void TargetEvent(IBattleCharacterData data, int tick)
        {
            if (data != null)
            {
                if (isExist.Contains(data))
                    return;
                isExist.Add(data);

                bool isStart = false;
                if (false == activePassive.TryGetValue(data, out var passives))
                {
                    activePassive.Add(data, passives = SBFunc.GetPassives(caster, data, battleData.BattleType));
                    isStart = true;
                }

                if (skill == null)
                    return;

                var effects = skill.GetEffect(idx);
                if (effects == null)
                    return;

                for (int i = 0, count = effects.Count; i < count; ++i)
                {
                    if (effects[i] == null)
                        continue;

                    for (int j = tick, jMin = 0; j > jMin; --j)
                    {
                        triggerEvent?.Invoke(caster, data, effects[i], skill, passives, isStart);
                    }
                }
            }
        }
        protected Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}