using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonStateIdle : GemDungeonState // 몬스터만 돌아다니기 // 이 과정에서 몬스터가 혼자 죽는 연출 있으면 안됨
    {
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                DragonSet(eGemDungeonState.IDLE);
                MonsterSet();
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                MonsterMove(dt);
                return true;
            }
            return false;
        }
    }
}

