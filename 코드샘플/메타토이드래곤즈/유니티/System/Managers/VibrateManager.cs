using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class VibrateManager : Singleton<VibrateManager>
    {
        public override void Initialize() { }
        //amplitude 0~255 강도
        public static void OnVibrate(float time, int amplitude)
        {
            if (!SBGameManager.Instance.GamePrefData.OPTION_VIBRATION)
                return;
#if UNITY_ANDROID
        //time 1000 == 1 sec        
        RDG.Vibration.Vibrate((int)(time * 1000f), amplitude, true);
#elif UNITY_IOS
        Handheld.Vibrate();
#endif
        }
    }
}
