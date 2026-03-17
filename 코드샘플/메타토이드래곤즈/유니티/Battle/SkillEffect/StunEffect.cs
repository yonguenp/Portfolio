using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace SandboxNetwork
{
    public class StunEffect : EffectInfo
    {
        protected override void TriggerEvent()
        {
            if (Target == null || Data == null)
                return;

            var spine = Target.GetSpine();
            if (spine == null)
                return;

            spine.SkillActionCancle();
            spine.SkeletonAni.Skeleton.SetColor(Color.gray);
            spine.ClearAnimation();
            spine.SetAnimation(eSpineAnimation.IDLE);
            if (EFFECT_TYPE == eSkillEffectType.FROZEN)
                spine.SetSpeed(0f);
            SkillBuffEvent.RegistBuff(Target.ID, this, Target.IsEnemy);
        }
        protected override void CompleteEvent()
        {
            if (Target == null || Data == null)
                return;

            var spine = Target.GetSpine();
            if (spine == null)
                return;

            spine.SkeletonAni.Skeleton.SetColor(Color.white);
            if (EFFECT_TYPE == eSkillEffectType.FROZEN)
                spine.SetDefaultScale();
            SkillBuffEvent.DeleteBuff(Target.ID, this, Target.IsEnemy);
        }
        public override void SetPassiveData(IBattleCharacterData caster, IBattleCharacterData target, SkillPassiveData passive, SkillEffectData data, int skillLevel)
        {
            base.SetPassiveData(caster, target, passive, data, skillLevel);
            if (Passive != null)
            {
                Time = Time * (Passive.CONVERT_VALUE + (Passive.CONVERT_VALUE * (Caster != null ? Caster.Stat.GetTotalStatusConvert(eStatusType.RATIO_PASSIVE_EFFECT) : 0.0f)));
                MaxTime = Time;
            }
        }
        public override bool IsEquals(EffectInfo info)
        {
            if (info.Target != Target)
                return false;

            if (info.EFFECT_TYPE != EFFECT_TYPE)
                return false;

            return true;
        }
        public override bool IsCover(EffectInfo info)
        {
            if (info.Time > Time)
                return true;

            return false;
        }
    }
}