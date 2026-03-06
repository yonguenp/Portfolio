using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DotEffect : EffectInfo
    {
        public delegate void DotDelegate(EffectInfo info);
        public DotEffect(DotDelegate tickDelegate) { TickDelegate = tickDelegate; }

        public DotDelegate TickDelegate { get; private set; } = null;
        private float CurTime { get; set; } = 0f;
        private int CurTerm { get; set; } = 1;
        private float Team { get; set; } = 0.05f;

        protected override void TriggerEvent()
        {
            CurTime = 0f;
            CurTerm = 1;
            if (MAX_TIME > 0 && Data.HIT_COUNT > 0)
                Team = MAX_TIME / Data.HIT_COUNT;
            else
                Team = 0.01f;
        }
        public override void Update(float dt)
        {
            if (Target == null || Target.Death)
            {
                Time = 0f;
            }
            else if (IsActive)
            {
                if(CurTerm > Data.HIT_COUNT)
                {
                    Time = 0f;
                }
                else
                {
                    CurTime += dt;
                    if (CurTime > Team * CurTerm)
                    {
                        ++CurTerm;
                        TickDelegate?.Invoke(this);
                    }
                }
            }
            base.Update(dt);
        }
        public override void SetData(EffectInfo info)
        {
            if (info is DotEffect tinfo)
                TickDelegate = tinfo.TickDelegate;

            base.SetData(info);
        }
        protected override void CompleteEvent()
        {
            while (CurTerm <= Data.HIT_COUNT)
            {
                ++CurTerm;
                TickDelegate?.Invoke(this);
            }
        }
    }
}