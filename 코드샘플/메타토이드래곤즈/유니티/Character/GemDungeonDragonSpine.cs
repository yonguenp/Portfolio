using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{

    public class GemDungeonDragonSpine : BattleDragonSpine 
    {
        public override void Init()
        {
            base.Init();
            if(Animation != eSpineAnimation.NONE)
                SetAnimation(Animation);
        }
        /// <summary> 넉백 방지용도 </summary>
        public override void UpdateStatus(float dt)
        {
            Data.Update(dt);
            //KnockBack
            //
        }
    }
}
