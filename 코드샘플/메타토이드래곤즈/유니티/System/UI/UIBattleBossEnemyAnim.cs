using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIBattleBossEnemyAnim : UIObject
    {
        [SerializeField]
        private Action callBack = null;
        private bool isEnd = false;

        private void OnDisable()
        {
            AnimEnd();
        }

        public void SetCallBack(Action cb)
        {
            callBack = cb;
            isEnd = false;
        }

        public void AnimEnd()
        {
            if (isEnd)
                return;

            if (callBack != null)
            {
                callBack.Invoke();
                callBack = null;
            }
            isEnd = true;
        }
    }
}
