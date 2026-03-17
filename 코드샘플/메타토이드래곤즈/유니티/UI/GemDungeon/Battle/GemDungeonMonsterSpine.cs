using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonMonsterSpine : BattleMonsterSpine
    {
        protected override IEnumerator DeathCO()
        {
            SetShadow(false);
            
            var curDeathTime = 0f;
            var maxDeathTime = 0.7f;
            var lastPos = spineObj.transform.localPosition;
            Vector3 vec1 = new Vector3(lastPos.x, lastPos.y + SBDefine.DeathY1, lastPos.z);
            Vector3 vec2 = new Vector3(lastPos.x, lastPos.y + SBDefine.DeathY2, lastPos.z);
            vec1.x -= Data.ConvertPos(SBDefine.DeathX1);
            vec2.x -= Data.ConvertPos(SBDefine.DeathX2);
            SetAnimation(eSpineAnimation.DEATH);
            skeletonAni.timeScale = 3f;
            while (curDeathTime < maxDeathTime)
            {
                curDeathTime += SBGameManager.Instance.DTime;
                if (IsBoss ==false)
                {
                    spineObj.transform.localPosition = SBFunc.BezierCurve2Vec3(lastPos, vec1, vec2, curDeathTime, maxDeathTime);
                }
                yield return null;
            }
            if(IsBoss ==false)
                spineObj.transform.localPosition = SBFunc.BezierCurve2Vec3(lastPos, vec1, vec2, maxDeathTime, maxDeathTime);
            isDeath = true;

            ClearEffectSpine();
            yield break;
        }
    }

}
