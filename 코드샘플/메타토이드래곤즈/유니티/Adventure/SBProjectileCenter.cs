using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBProjectileCenter : SBProjectile
    {
        public override Vector3 TargetPos { get; protected set; } = Vector3.zero;
        public virtual void Set(IBattleCharacterData Caster, Vector3 TargetPos, SBSkill Skill, VoidDelegate CallBack, int idx)
        {
            this.Caster = Caster;
            this.TargetPos = TargetPos;
            this.Skill = Skill;
            this.CallBack = CallBack;
            SkillIndex = idx;
            Launch();
        }
    }
}