using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AdventureDragonSpine : BattleDragonSpine
    {
        protected override void CompleteHandleAnimation(TrackEntry trackEntry)
        {
            if (AdventureManager.Instance.Data.State == eBattleState.Win || AdventureManager.Instance.Data.State == eBattleState.Lose)
                return;

            base.CompleteHandleAnimation(trackEntry);
        }
    }
}