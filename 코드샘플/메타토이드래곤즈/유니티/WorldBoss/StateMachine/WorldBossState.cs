using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct WorldBossStateEvent
    {
        private static WorldBossStateEvent e = default;
        public bool Pause;
        public static void Send(bool isPause)
        {
            e.Pause = isPause;
            EventManager.TriggerEvent(e);
        }
    }
    public class WorldBossState : BattleStateLogic, EventListener<WorldBossStateEvent>
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

        public virtual void OnEvent(WorldBossStateEvent e)
        {
            if (e.Pause)
                OnPause();
            else
                OnResume();
        }
    }
}
