using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class KnockbackEffect : EffectInfo
    {
        const float TimeValue = 0.01f;
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
                var spine = Target.GetSpine();
                if (spine != null)
                {
                    //if (spine.Controller != null && spine.Controller.myCollider != null)
                    //    spine.Controller.myCollider.isTrigger = true;

                    Refresh();
                }
            }
        }
        public override void SetEffectData(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData data, int skillLevel)
        {
            base.SetEffectData(caster, target, data, skillLevel);
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
            var spine = Target.GetSpine();
            if (IsPlaying)
            {   
                if (spine != null)
                {
                    Time = 0f;
                    Refresh();
                }
            }

            IsPlaying = false;
            if (spine != null)
            {
                var drgonSpine = spine as BattleDragonSpine;
                if (drgonSpine != null)
                {
                    drgonSpine.SetAnimation(eSpineAnimation.IDLE);
                }

                spine.SetDustDragonPos(spine.Data.IsLeft);
                spine.OffDust();
            }
        }
        protected virtual void Refresh()
        {
            //if (spine.Controller != null && spine.Controller.myCollider != null)
            //    spine.Controller.myCollider.isTrigger = false;

            var value = GetValue();
            var spine = Target.GetSpine();
            if (spine != null)
            {
                var worldPos = startPosition;
                worldPos.x = worldPos.x + SBFunc.BezierCurveSpeed(0f, value * TileValue * SBDefine.BattleTileX, MaxTime - Time, MaxTime, new Vector4(0f, 1f, 0f, 1f));
                spine.transform.position = worldPos;
            }
        }

        protected override void TriggerEvent()
        {
            dir = Target.KnockBackDirection;
            var spine = Target.GetSpine();
            if (spine != null)
            {
                spine.SkillActionCancle();
                var drgonSpine = spine as BattleDragonSpine;
                if (drgonSpine != null)
                {
                    spine.SetAnimation(eSpineAnimation.LOSE);                    
                }

                spine.SetDustDragonPos(!spine.Data.IsLeft);
                spine.OnDust();
            }

            IsPlaying = true;            
        }
        public override float GetValue()
        {
            float value = 1.0f;
            if (Passive != null)
            {
                value = (Passive.CONVERT_VALUE + (Passive.CONVERT_VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f)));                
            }

            var passive_resis = (Target != null ? Target.Stat.GetTotalStatusConvert(eStatusType.DEL_KNOCKBACK_DISTANCE) : 0.0f);
            value = value - passive_resis;

            switch (dir)
            {
                case eDirectionBit.Right:
                    return base.GetValue() * value;
                case eDirectionBit.Left:
                    return -base.GetValue() * value;
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
