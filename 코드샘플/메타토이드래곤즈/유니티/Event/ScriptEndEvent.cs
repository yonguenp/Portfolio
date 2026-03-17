using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct ScriptEndEvent
    {
        public int scriptID;

        public ScriptEndEvent(int scriptGroupKey = 0)
        {
            scriptID = scriptGroupKey;
        }

        static public void Event(int scriptGroupKey = 0)
        {
            EventManager.TriggerEvent(new ScriptEndEvent(scriptGroupKey));
        }
    }
}
