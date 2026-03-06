using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBProjectileTargetPosY : SBProjectileTarget
    {
        /// <summary> 최종 목적지에 도달했을때 위치 </summary>
        [SerializeField]
        protected Transform target = null;
        /// <summary> 중간점에 도달했을때 위치 </summary>
        [SerializeField]
        protected Transform wayPoint = null;
        /// <summary> 현재 어디정도까지 도달했는가 </summary>
        protected float degree = 0f;
        /// <summary> 투사체 Object 처음 위치 기록 </summary>
        protected float startPosY = 0f;
        protected override void Launch()
        {
            base.Launch();
            degree = 0f;
        }
        public override void Set(IBattleCharacterData Caster, IBattleCharacterData Target, SBSkill Skill, VoidDelegate CallBack, int idx)
        {
            base.Set(Caster, Target, Skill, CallBack, idx);
            startPosY = projectileObject.transform.localPosition.y;
        }
        protected override bool UpdateTile(float dt)
        {
            if (false == base.UpdateTile(dt))
            {
                degree = curDistance > 0f ? 1f - curDistance / startDistance : 1f;
                if (degree > 1f)
                    degree = 1f;
                if (target != null)
                {
                    var pos = projectileObject.transform.localPosition;
                    if (wayPoint != null)
                        pos.y = SBFunc.BezierCurve2Speed(startPosY, wayPoint.localPosition.y, target.localPosition.y, degree, 1f, new Vector4(1f, 0f, 1f, 1f));
                    else
                        pos.y = SBFunc.BezierCurveSpeed(startPosY, target.localPosition.y, degree, 1f, new Vector4(1f, 0f, 1f, 1f));
                    projectileObject.transform.localPosition = pos;
                }
                return false;
            }
            return true;
        }
    }
}