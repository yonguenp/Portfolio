using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class TutorialContentsController : MonoBehaviour
    {
        public List<TutorialTweenPlayer> tweenPlayerList = new List<TutorialTweenPlayer>();

        public void PlayTutorialTween(ref int tutoSeq)
        {
            if (tweenPlayerList == null || tweenPlayerList.Count < 0 || tweenPlayerList.Count < tutoSeq) { return; }

            StopTutorialTween();
            tweenPlayerList[tutoSeq].PlayTweenAnim();
        }

        void StopTutorialTween()
        {
            if (tweenPlayerList == null || tweenPlayerList.Count < 0) { return; }

            foreach (TutorialTweenPlayer tween in tweenPlayerList)
            {
                tween.StopTweenAnim();
            }
        }
    }
}