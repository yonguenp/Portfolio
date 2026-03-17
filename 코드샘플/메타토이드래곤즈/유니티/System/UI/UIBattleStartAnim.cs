using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIBattleStartAnim : UIObject
    {
        [SerializeField]
        private UnityEngine.Events.UnityEvent callBack = null;
        
        public void StartAnimEnd()
        {
            if (callBack != null)
                callBack.Invoke();
        }
    }
}