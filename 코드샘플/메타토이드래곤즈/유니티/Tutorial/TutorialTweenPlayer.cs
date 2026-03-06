using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SandboxNetwork
{
    public enum eTutorialTweenType
    {
        NONE,
        FADE_IN,
        FADE_OUT,
        BLINK
    }

    public enum eTutorialTweenSeqType
    {
        NONE,
        APPEND,
        JOIN,
        DELAY,
    }

    [Serializable]
    public class TutorialTweenSettingData
    {
        public eTutorialTweenType tweenType = eTutorialTweenType.NONE;
        public eTutorialTweenSeqType tweenSeqType = eTutorialTweenSeqType.NONE;

        public CanvasGroup tweenObject = null;
        public float duration = 1;
        public int repeatTime = 1;
    }

    public class TutorialTweenPlayer : MonoBehaviour
    {
        public bool isFirstPage = false;        // 첫페이지 인지 여부
        public bool isPlayOnce = true;          // 트윈을 반복플레이 여부

        bool isPlayed = false;       // 처음 열린 것인지 확인 여부

        public List<TutorialTweenSettingData> tweenList = new List<TutorialTweenSettingData>();

        Sequence totalTweenSequence;      // 완성된 최종 트윈 시퀀스

        private void Start()
        {
            InitTweenData();
            if (totalTweenSequence == null) { return; }

            if (isFirstPage)
            {
                if (isPlayOnce)
                {
                    if (isPlayed)
                    {
                        totalTweenSequence.Complete();
                    }
                    else
                    {
                        totalTweenSequence.Play();
                        isPlayed = true;
                    }
                }
                else
                {
                    totalTweenSequence.Rewind();
                    totalTweenSequence.Restart();
                }
            }
        }

        private void OnDestroy()
        {
            totalTweenSequence?.Kill();
        }

        public void PlayTweenAnim()
        {
            if (totalTweenSequence == null) { return; }

            if (isPlayOnce)
            {
                if (isPlayed)
                {
                    totalTweenSequence.Complete();
                }
                else
                {
                    totalTweenSequence.Play();
                    isPlayed = true;
                }
            }
            else
            {
                totalTweenSequence.Rewind();
                totalTweenSequence.Restart();
            }
        }

        public void StopTweenAnim()
        {
            if (totalTweenSequence == null) { return; }

            totalTweenSequence.Pause();
        }

        // 트윈 데이터를 구성
        public void InitTweenData()
        {
            if (tweenList == null || tweenList.Count <= 0) { return; }

            totalTweenSequence = DOTween.Sequence();

            foreach (TutorialTweenSettingData tweenData in tweenList)
            {
                switch (tweenData.tweenSeqType)
                {
                    case eTutorialTweenSeqType.NONE:
                        
                        break;
                    case eTutorialTweenSeqType.APPEND:
                        totalTweenSequence.Append(MakeTweenSequence(tweenData));
                        break;
                    case eTutorialTweenSeqType.JOIN:
                        totalTweenSequence.Join(MakeTweenSequence(tweenData));
                        break;
                    case eTutorialTweenSeqType.DELAY:
                        //totalTweenSequence.Append(MakeTweenSequence(tweenData));
                        break;
                }
            }
       
            totalTweenSequence.Pause();
        }

        Tween MakeTweenSequence(TutorialTweenSettingData paramTweenData)
        {
            Tween resultTween = null;

            switch (paramTweenData.tweenType)
            {
                case eTutorialTweenType.NONE:
                    
                    break;
                case eTutorialTweenType.FADE_IN:
                    paramTweenData.tweenObject.alpha = 0;
                    resultTween = paramTweenData.tweenObject.DOFade(1, paramTweenData.duration);
                    break;
                case eTutorialTweenType.FADE_OUT:
                    paramTweenData.tweenObject.alpha = 1;
                    resultTween = paramTweenData.tweenObject.DOFade(0, paramTweenData.duration);
                    break;
                case eTutorialTweenType.BLINK:
                    resultTween = paramTweenData.tweenObject.DOFade(0, paramTweenData.duration).SetEase(Ease.InCubic).SetLoops(paramTweenData.repeatTime * 2, LoopType.Yoyo);
                    break;
            }

            return resultTween;
        }
    }
}
