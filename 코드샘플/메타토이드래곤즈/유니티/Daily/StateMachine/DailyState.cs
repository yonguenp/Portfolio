using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{

    public struct DailyStateEvent
    {
        private static DailyStateEvent e = default;
        public bool Pause;
        public static void Send(bool isPause)
        {
            e.Pause = isPause;
            EventManager.TriggerEvent(e);
        }
    }
    public class DailyState : BattleStateLogic, EventListener<DailyStateEvent>
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

        public virtual void OnEvent(DailyStateEvent e)
        {
            if (e.Pause)
                OnPause();
            else
                OnResume();
        }
    }
}