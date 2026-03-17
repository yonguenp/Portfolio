using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SystemMessageSlot : MonoBehaviour
    {
        [SerializeField] Animator animator = null;

        [SerializeField] Text msgText = null;
        [SerializeField] Button actionBtn = null;
        [SerializeField] CanvasGroup group = null;

        Action btnAction = null;
        Coroutine coroutine = null;
        Coroutine delayCoroutine = null;
        public bool IsShowMessage { get; private set; } = false;        // 현재 메시지가 UI에 노출중인지 여부
        private void OnEnable()
        {
            if (group != null)
                group.alpha = 0f;

            if (animator != null)
                animator.speed = 1f;
        }
        private void OnDisable()
        {
            ClearCoroutine();
        }

        public void OnMessage(eSystemMessageType eType, string msg, Action action = null)
        {
            msgText.text = msg;
            switch (eType)
            {
                case eSystemMessageType.WISPER:
                {
                    msgText.color = new Color(0.95f, 0.5f, 1f, 1f);
                } break;
                case eSystemMessageType.GUILD:
                {
                    msgText.color = new Color(0.5f, 1f, 0.5f, 1f);
                } break;
                case eSystemMessageType.NORMAL:
                default:
                {
                    msgText.color = Color.white;
                } break;
            }
            btnAction = action;
            if (group != null)
            {
                bool isAction = btnAction != null;
                group.blocksRaycasts = isAction;
                group.interactable = isAction;
            }

            ClearCoroutine();
            coroutine = StartCoroutine(Show());
        }

        private void ClearCoroutine()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
                delayCoroutine = null;
            }

            if (animator != null)
                animator.speed = 1f;
        }
        private IEnumerator Show()
        {
            yield return SBDefine.GetWaitForEndOfFrame();
            animator.Play("SystemMsg");
            yield break;
        }

        public void OnClickBtn()
        {
            btnAction?.Invoke();
        }

        public void AnimationDelayCheck()
        {
            delayCoroutine = StartCoroutine(AnimationDelay());
        }
        public IEnumerator AnimationDelay()
        {
            animator.speed = 0f;
            int delay = 0;
            while (SystemMessage.Count == 0 && delay++ < 20)
            {
                yield return SBDefine.GetWaitForSecondsRealtime(0.05f);
            }
            animator.speed = 1f;
            delayCoroutine = null;
            yield break;
        }

        public void Clear()
        {
            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
            }
            SystemMessage.MessageEnd(this);
        }
    }
}