using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct AdventureStateEvent
    {
        private static AdventureStateEvent e = default;
        public bool Pause;
        public static void Send(bool isPause)
        {
            e.Pause = isPause;
            EventManager.TriggerEvent(e);
        }
    }
    public class AdventureState : BattleStateLogic, EventListener<AdventureStateEvent>
    {
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                EventManager.AddListener(this);
                return true;
            }
            return false;
        }
        public override bool OnExit()
        {
            if (base.OnExit())
            {
                EventManager.RemoveListener(this);
                return true;
            }
            return false;
        }

        public virtual void OnEvent(AdventureStateEvent e)
        {
            if (e.Pause)
                OnPause();
            else
                OnResume();
        }
    }
}
