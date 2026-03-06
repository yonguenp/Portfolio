
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct UIEffectEvent
    {
        public enum eUIEffect
        {
            NONE = 0,
            FIRE_WORK = 1,
            HIGH_LIGHT = 2,
        }

        public enum eEvent
        {
            EFFECT_HIDE = 0,
            EFFECT_SHOW = 1,
            EFFECT_PLAY  =2
        }
        public eEvent eventType;
        public eUIEffect effectType;
        public float effectTime;
        public UIEffectEvent(eEvent _event, eUIEffect _target, float _effectTime)
        {
            eventType = _event;
            effectType = _target;
            effectTime = _effectTime;
        }

        static public void Event(eEvent _event, eUIEffect _target = eUIEffect.NONE, float _effectTime = 0f)
        {
            EventManager.TriggerEvent(new UIEffectEvent(_event, _target, _effectTime));
        }
    }

    public class UIEffectController : MonoBehaviour, EventListener<UIEffectEvent>
    {
        [SerializeField]
        public GameObject fireworkObj = null;
        [SerializeField]
        public GameObject[] fireworks = null;
        [SerializeField]
        public GameObject circleHighligtObj = null;

        private void Start()
        {
            EventManager.AddListener(this);
        }

        public void OnEvent(UIEffectEvent effectEvent)
        {
            GameObject effectObj = null;
            switch (effectEvent.effectType)
            {
                case UIEffectEvent.eUIEffect.FIRE_WORK:
                    effectObj = fireworkObj;
                    if (effectObj == null) return;

                    switch (effectEvent.eventType)
                    {
                        case UIEffectEvent.eEvent.EFFECT_SHOW:
                            effectObj.SetActive(true);
                            FireEffectClear();
                            StartCoroutine(FireworkDelay());
                            break;
                        case UIEffectEvent.eEvent.EFFECT_HIDE:
                            effectObj.SetActive(false);
                            FireEffectClear();
                            break;
                    }
                    break;
                case UIEffectEvent.eUIEffect.HIGH_LIGHT:
                    effectObj = circleHighligtObj;
                    if (effectObj == null) return;
                    switch (effectEvent.eventType)
                    {
                        case UIEffectEvent.eEvent.EFFECT_SHOW:
                            effectObj.transform.localScale = Vector3.one;
                            effectObj.SetActive(true);
                            break;
                        case UIEffectEvent.eEvent.EFFECT_HIDE:
                            effectObj.SetActive(false);
                            break;
                        case UIEffectEvent.eEvent.EFFECT_PLAY:
                            effectObj.transform.DOScale(Vector3.one*2f, effectEvent.effectTime);
                            break;
                    }
                    break;
            }
        }

        void FireEffectClear()
        {
            foreach (GameObject fire in fireworks)
            {
                fire.SetActive(false);
            }
        }

        IEnumerator FireworkDelay()
        {
            foreach(GameObject fire in fireworks)
            {
                fire.SetActive(true);
                yield return SBDefine.GetWaitForSeconds(SBFunc.Random(0.5f,2.5f));
            }
        }

        public void OnCllickHighlightDim()
        {

        }
    }
}
