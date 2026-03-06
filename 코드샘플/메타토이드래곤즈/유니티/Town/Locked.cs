using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class Locked : MonoBehaviour
    {
        [SerializeField]
        protected Transform lockBG = null;
        public void Resize()
        {
            if (lockBG != null)
            {
                var size = TownMap.Width;
                var scale = lockBG.localScale;
                scale.x = (size - 2) * 324 + 2 * 348;
                lockBG.localScale = scale;
            }
        }
    }
}