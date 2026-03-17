using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBProjectileTarget : SBProjectile
    {
        public IBattleCharacterData Target { get; protected set; } = null;
        protected Vector3 targetPos = Vector3.zero;
        public override Vector3 TargetPos { get => IsMove() ? Target.Transform.position : targetPos; protected set => targetPos = value; }
        protected override bool IsMove()
        {
            if(base.IsMove())
            {
                return Target != null && Target.Transform != null;
            }
            return false;
        }
        protected override void Launch()
        {
            base.Launch();
            if (IsMove())
                targetPos = Target.Transform.position;
        }
        public virtual void Set(IBattleCharacterData Caster, IBattleCharacterData Target, SBSkill Skill, VoidDelegate CallBack, int idx)
        {
            this.Caster = Caster;
            this.Target = Target;
            this.Skill = Skill;
            this.CallBack = CallBack;
            SkillIndex = idx;
            Launch();
        }
        protected override bool UpdateTile(float dt)
        {
            //날아가는 도중 대상이 죽더라도 마지막 포지션까지는 이동.
            TargetPos = TargetPos;
            //
            return base.UpdateTile(dt);
        }
    }
}