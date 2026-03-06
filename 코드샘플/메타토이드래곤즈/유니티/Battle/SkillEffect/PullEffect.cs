using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PullEffect : EffectInfo
    {
        public PullEffect(Vector3 pullPos) { PullPos = pullPos; }
        const float TimeValue = 0.008333f;
        const float TileValue = 0.016667f;
        public bool IsPlaying { get; private set; } = false;
        private Vector3 unitVec = Vector3.zero;
        private Vector3 startPosition = Vector3.zero;
        private Vector3 PullPos { get; set; } = default;
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
        protected virtual void Refresh()
        {
            var spine = Target.GetSpine();
            if (spine != null)
            {
                unitVec = (PullPos - spine.transform.position).normalized;

                var worldPos = startPosition;
                worldPos.x = worldPos.x + SBFunc.BezierCurveSpeed(0f, GetValue() * TileValue * SBDefine.BattleTileX * unitVec.x, MaxTime - Time, MaxTime, new Vector4(0f, 1f, 0f, 1f));
                worldPos.y = worldPos.y + SBFunc.BezierCurveSpeed(0f, GetValue() * TileValue * SBDefine.BattleTileX * unitVec.y, MaxTime - Time, MaxTime, new Vector4(0f, 1f, 0f, 1f));
                spine.transform.position = worldPos;
            }
        }
        public override void SetEffectData(IBattleCharacterData caster, IBattleCharacterData target, SkillEffectData data, int skillLevel)
        {
            base.SetEffectData(caster, target, data, skillLevel);
            Time = GetValueDefault() * TimeValue;
            MaxTime = Time;

            var spine = Target.GetSpine();
            if (spine != null)
                startPosition = spine.transform.position;
        }
        public override bool IsCover(EffectInfo info)
        {
            return true;
        }
        protected override void CompleteEvent()
        {
            IsPlaying = false;
        }

        protected override void TriggerEvent()
        {
            IsPlaying = true;
        }
        public void SetStopBack()
        {
            IsPlaying = false;
            Time = 0f;
        }
    }
}