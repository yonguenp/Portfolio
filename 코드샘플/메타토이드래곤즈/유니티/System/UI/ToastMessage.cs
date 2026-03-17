using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ToastMessage : MonoBehaviour
    {
        [SerializeField]
        private GameObject titleNode = null;
        [SerializeField]
        private Text titleLabel = null;
        [SerializeField]
        private Text bodyLabel = null;
        [SerializeField]
        private string targetString = "";
        [SerializeField]
        private RectTransform textBox = null;
        [SerializeField]
        private LayoutElement textBoxLayoutElem = null;
        [SerializeField]
        private RectTransform textRect = null;
        [SerializeField]
        Sprite backgroundNormal = null;
        [SerializeField]
        Sprite backgroundEffect = null;
        [SerializeField]
        Image background = null;

        bool isSystem = false;
        bool isSystemColor = true;
        ToastMessage prev = null;
        ToastMessage next = null;
        CanvasGroup canvasGroup = null;
        Sequence sequence = null;
        int pushCount = 0;
        bool isPrevExistance = false;
        public string String
        {
            private get { return targetString; }
            set 
            { 
                targetString = value;
                SetString();
            }
        }

        private void Start()
        {
            SetString();
        }

        private void OnDestroy()
        {
            if(next != null)
            {
                next.SetPrev(null);
            }
            if(prev != null)
            {
                prev.SetNext(null);
            }
        }

        public void SetPrev(ToastMessage prev_toast)
        {
            if (prev == prev_toast)
                return;

            prev = prev_toast;
            if(prev != null)
                prev.SetNext(this);
        }
        public void SetNext(ToastMessage next_toast)
        {
            if (next == next_toast)
                return;

            next = next_toast;
            if (next != null)
                next.SetPrev(this);
        }

        public void SetData(string message, bool isSystemMessage, float delay, bool isEffect, ToastMessage prev_toast)
        {
            if(background != null)
            {
                background.sprite = isEffect ? backgroundEffect : backgroundNormal;
            }

            targetString = message;
            isSystem = isSystemMessage;
            SetPrev(prev_toast);

            SetString();

            if (prev != null)
            {
                prev.MoveUp();
            }

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, 0.2f));            
            sequence.AppendInterval(delay);
            sequence.Append(canvasGroup.DOFade(0f, 0.2f));
            sequence.AppendCallback(() => {
                Clear();
            });
        }

        public void SetData(string _title, string _detail, float delay, ToastMessage prev_toast)
        {
            isSystemColor = false;

            if (titleLabel != null)
                titleLabel.text = _title;

            targetString = _detail;
            
            SetPrev(prev_toast);

            SetString();

            isPrevExistance = false;
            if (prev != null)
            {
                prev.CAProduction();
                isPrevExistance = true;
            }

            var color = bodyLabel.color;
            bodyLabel.color = new Color(color.r, color.g, color.b, 0);

            if (isPrevExistance)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1;
                sequence = DOTween.Sequence();
                sequence.Append(bodyLabel.DOFade(1,0.3f));
                sequence.AppendInterval(delay);
                sequence.Append(canvasGroup.DOFade(0f, 0.2f));
                sequence.AppendCallback(() => {
                    Clear();
                });
            }
            else
            {
                canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.alpha = 0;

                sequence = DOTween.Sequence();
                sequence.Append(canvasGroup.DOFade(1f, 0.2f));
                sequence.Append(bodyLabel.DOFade(1, 0.3f));
                sequence.AppendInterval(delay);
                sequence.Append(canvasGroup.DOFade(0f, 0.2f));
                sequence.AppendCallback(() => {
                    Clear();
                });
            }
        }

        public void Clear()
        {
            if (next != null)
            {
                next.SetPrev(null);
            }
            if (prev != null)
            {
                prev.SetNext(null);
            }
            Destroy(gameObject);
        }

        public void MoveUp()
        {
            if (prev != null)
            {
                prev.MoveUp();
            }

            pushCount++;
            textBox.transform.DOLocalMoveY(600f + (100f * pushCount) + (textBox.sizeDelta.y * 0.5f), 0.2f);
            textBox.transform.DOScale(0.8f / pushCount, 0.2f);

            sequence.Kill();
            sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(0f, 0.2f * canvasGroup.alpha));
            sequence.AppendCallback(() =>
            {
                Clear();
            });
        }

        public void CAProduction()
        {
            if (prev != null)
            {
                prev.gameObject.transform.SetAsLastSibling();
                prev.CAProduction();
            }

            if (titleNode != null)
                titleNode.gameObject.SetActive(false);

            background.color = new Color(1, 1, 1, 0);

            pushCount++;
            textBox.transform.DOScale(0.8f / pushCount, 0.2f);

            sequence.Kill();
            sequence = DOTween.Sequence();
            sequence.Append(bodyLabel.DOFade(0f, 0.5f * canvasGroup.alpha));
            sequence.Append(canvasGroup.DOFade(0f, 0.2f * canvasGroup.alpha));
            sequence.AppendCallback(() =>
            {
                Clear();
            });
        }

        private void SetString()
        {
            if(bodyLabel != null)
            {
                bodyLabel.text = targetString;
                if(textBox != null) { 
                    if(textRect != null) { 
                        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(textBox);
                    if (textBox.sizeDelta.x > 850)
                    {
                        textBoxLayoutElem.preferredWidth = 850;
                    }
                }

                if(isSystemColor)
                    bodyLabel.color = isSystem ? Color.white : Color.white;
            }
        }
     }
}
