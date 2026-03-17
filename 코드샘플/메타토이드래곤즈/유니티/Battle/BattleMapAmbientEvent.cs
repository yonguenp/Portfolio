using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct BattleMapAmbientEvent
    {
        private static BattleMapAmbientEvent e = default;
        public Color color;
        public float time;
        public static void Send(Color color, float time)
        {
            e.color = color;
            e.time = time;

            EventManager.TriggerEvent(e);
        }
    }
}