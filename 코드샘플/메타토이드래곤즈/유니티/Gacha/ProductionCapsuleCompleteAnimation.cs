using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ProductionCapsuleCompleteAnimation : MonoBehaviour
    {
        public void SendCompleteAnimation()
        {
            GachaEvent.CapsuleOpenAnimationComplete();
        }

        public void SendCapsuleOpenIndex(string _dataString)
        {
            var splitData = _dataString.Split('_');
            GachaEvent.CapsuleIndexOpen(int.Parse(splitData[0]) , int.Parse(splitData[1]));
        }
    }
}