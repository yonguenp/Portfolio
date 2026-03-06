using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PassiveKnockBack : EffectInfo
    {
        public override eSkillEffectType EFFECT_TYPE => eSkillEffectType.KNOCK_BACK;
        public override eStatusType STAT_TYPE => Passive.STAT;
        public override eStatusValueType VALUE_TYPE => Passive.EFFECT_VALUE;
        public override int NEST_GROUP { get => Passive.NEST_GROUP; }
        public override int NEST_COUNT { get => Passive.NEST_COUNT; }
        public override float VALUE { get => Passive.VALUE + PASSIVE_EFFECT - PASSIVE_RESIS; }

        float PASSIVE_EFFECT { get { return (Passive.VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f)); } }
        float PASSIVE_RESIS { get { return (Passive.VALUE * (Target != null ? Target.Stat.GetTotalStatusConvert(eStatusType.DEL_KNOCKBACK_DISTANCE) : 0.0f)); } }
        public override float MAX_TIME { get => Passive.MAX_TIME; }
        const float TimeValue = 0.0076923f;
        const float TileValue = 0.06666667f;
        public bool IsPlaying { get; private set; } = false;
        private eDirectionBit dir = eDirectionBit.None;
        private Vector3 startPosition = Vector3.zero;
        public override void Update(float dt)
        {
            if (Target.Death)
            {
                Time = 0f;
            }
            base.Update(dt);
            if (IsActive && IsPlaying)
            {
                Refresh();
            }
        }
        public override void SetPassiveData(IBattleCharacterData caster, IBattleCharacterData target, SkillPassiveData passive, SkillEffectData data, int skillLevel)
        {
            base.SetPassiveData(caster, target, passive, data, skillLevel);
            Time = GetValueDefault() * TimeValue;
            MaxTime = Time;

            dir = Target.KnockBackDirection;
            var spine = Target.GetSpine();
            if (spine != null)
                startPosition = spine.transform.position;
        }
        public override bool IsCover(EffectInfo info)
        {
            return false;
        }
        protected override void CompleteEvent()
        {
            if (IsPlaying)
            {
                var spine = Target.GetSpine();
                if (spine != null)
                {
                    Time = 0f;
                    Refresh();
                }
            }

            IsPlaying = false;
        }
        protected virtual void Refresh()
        {
            var spine = Target.GetSpine();
            if (spine != null)
            {
                var worldPos = startPosition;
                worldPos.x = worldPos.x + SBFunc.BezierCurveSpeed(0f, GetValue() * TileValue * SBDefine.BattleTileX, MaxTime - Time, MaxTime, new Vector4(0f, 1f, 0f, 1f));
                spine.transform.position = worldPos;
            }
        }

        protected override void TriggerEvent()
        {
            dir = Target.KnockBackDirection;
            IsPlaying = true;
        }
        protected override float GetValueDefault()
        {
            return VALUE;
        }
        public override float GetValue()
        {
            switch (dir)
            {
                case eDirectionBit.Left:
                    return -base.GetValue();
                case eDirectionBit.Right:
                    return base.GetValue();
                default:
                    return 0.0f;
            }
        }
        public void SetStopBack()
        {
            IsPlaying = false;
            Time = 0f;
        }
    }
}
