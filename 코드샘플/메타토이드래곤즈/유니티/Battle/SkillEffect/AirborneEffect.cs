using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AirborneEffect : EffectInfo
    {
        private eDirectionBit dir = eDirectionBit.None;
        private Vector3 startPosition = Vector3.zero;
        const float TimeValue = 0.0105263f;
        const float TileValue = 0.03428568f;
        const float HeightValue = 0.1f;
        public override void Update(float dt)
        {
            if (Target.Death)
            {
                Time = 0f;
            }
            base.Update(dt);
            if (IsActive)
            {
                Refresh();
            }
        }
        public override void SetEffectData(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData data, int skillLevel)
        {
            base.SetEffectData(caster, target, data, skillLevel);
            Time = VALUE * TimeValue;
            MaxTime = Time;
            
            dir = Target.KnockBackDirection;
            var spine = Target.GetSpine();
            if (spine != null)
                startPosition = spine.transform.position;
        }
        protected virtual void Refresh()
        {
            var spine = Target.GetSpine();
            if (spine != null)
            {
                //var worldPos = startPosition;
                //worldPos.x = worldPos.x + SBFunc.BezierCurveSpeed(0f, GetDirValue() * TileValue * SBDefine.BattleTileX, MaxTime - Time, MaxTime, new Vector4(0f, 1f, 0f, 1f));
                //worldPos.y = startPosition.y + SBFunc.BezierCurve2Speed(0f, GetValue() * HeightValue * SBDefine.BattleTileY, 0f, MaxTime - Time, MaxTime, new Vector4(0f, 0.675f, 1f, 0.325f));
                //spine.transform.position = worldPos;
                var local = spine.SpineTransform.localPosition;
                local.y = SBFunc.BezierCurve2Speed(0f, GetValue() * HeightValue * SBDefine.BattleTileX, 0f, MaxTime - Time, MaxTime, new Vector4(0f, 0.675f, 1f, 0.325f));
                spine.SpineTransform.localPosition = local;
                var worldPos = startPosition;
                worldPos.x = worldPos.x + SBFunc.BezierCurveSpeed(0f, GetDirValue() * TileValue * SBDefine.BattleTileX, MaxTime - Time, MaxTime, new Vector4(0f, 1f, 0f, 1f));
                spine.transform.position = worldPos;
            }
        }
        public override bool IsCover(EffectInfo info)
        {
            return false;
        }
        private float GetDirValue()
        {
            return dir == eDirectionBit.Right ? GetValue() : -GetValue();
        }
        public override float GetValue()
        {
            return base.GetValue() * (Passive != null ? Passive.CONVERT_VALUE + (Passive.CONVERT_VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f)) : 1f);
        }
        protected override void CompleteEvent()
        {
            var spine = Target.GetSpine();
            if (spine != null)
            {
                spine.SetDefaultScale();
                spine.SpineTransform.localPosition = Vector3.zero;

                Time = 0f;
                Refresh();
                if (!Target.Death)
                    spine.SetShadow(true);
            }
        }

        protected override void TriggerEvent()
        {
            var spine = Target.GetSpine();
            if (spine != null)
            {
                spine.SkillActionCancle();
                Target.GetSpine().SetSpeed(0f);
                spine.SpineTransform.localPosition = Vector3.zero;
                if (!Target.Death)
                    spine.SetShadow(false);
            }
        }
    }
}