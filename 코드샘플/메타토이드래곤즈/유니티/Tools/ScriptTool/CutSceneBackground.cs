using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public class CutSceneBackground : MonoBehaviour
    {
        [SerializeField] Image blackFadeImg = null;
        [SerializeField] Color fadeColor = Color.black;
        [SerializeField] float fadeInTime = 1.0f;
        [SerializeField] RectTransform spineRect;
        [SerializeField] bool scrolling = false;
        [SerializeField] float scrollTime = 10.0f;

        Coroutine curCor = null;

        public void Init(RectTransform parent)
        {
            var rt = (transform as RectTransform);
            rt.localScale = Vector3.one;
            //rt.anchorMin = parent.anchorMin;
            //rt.anchorMax = parent.anchorMax;
            //rt.offsetMin = parent.offsetMin;
            //rt.offsetMax = parent.offsetMax;
            if (spineRect != null)
            {
                float spineRectX = spineRect.sizeDelta.x;
                float spineRectY = spineRect.sizeDelta.y;
                RectTransform canvasRect = UICanvas.Instance.GetCanvasRectTransform();
                float screenX = canvasRect.sizeDelta.x;//rt.rect.width;
                float screenY = canvasRect.sizeDelta.y;//rt.rect.height;
                float ratio = 0f;

                bool fixedToWidth = (spineRectX / spineRectY) < (screenX / screenY);
                if (fixedToWidth)
                {
                    ratio = screenX / spineRectX;
                }
                else // 겔럭시 폴드 펼친 거나 아이패드 같은 애들 대응
                {
                    ratio = screenY / spineRectY;
                }
                blackFadeImg.GetComponent<RectTransform>().sizeDelta = canvasRect.sizeDelta;
                spineRect.localScale = ratio * Vector3.one;

                var spineAnim = spineRect.GetComponent<SkeletonGraphic>();
                if (spineAnim != null)
                {
                    var anim = spineAnim.AnimationState.SetAnimation(0, "start", false);
                    anim.Complete += (anim) => {
                        spineAnim.AnimationState.SetAnimation(0, "idle", true);
                    };
                }

            }
            if (fadeInTime > 0.0f && blackFadeImg != null)
            {
                if(curCor!= null)
                    StopCoroutine(curCor);
                curCor = StartCoroutine(FadeInBlack());
            }
            else
            {
                if (scrolling)
                {
                    rt.DOPivotY(0.0f, scrollTime);
                }
            }
        }

        private void OnDestroy()
        {
            if (curCor != null)
            {
                spineRect.gameObject.SetActive(false);
                StopCoroutine(curCor);
            }
                
        }

        IEnumerator FadeInBlack()
        {
            blackFadeImg.gameObject.SetActive(true);
            blackFadeImg.color = fadeColor;

            float originAlpha = fadeColor.a;
            float fadeTime = fadeInTime;
            while (fadeTime > 0.0f)
            {
                fadeTime -= Time.deltaTime;

                fadeColor.a = (originAlpha * (fadeTime / fadeInTime));
                blackFadeImg.color = fadeColor;
                yield return new WaitForEndOfFrame();
            }

            blackFadeImg.gameObject.SetActive(false);

            if (scrolling)
            {
                (transform as RectTransform).DOPivotY(0.0f, scrollTime);
            }
        }

        public void SetFadeOut(float fadeOutTime, VoidDelegate fadeOutCompleteCB = null)
        {
            if (curCor != null)
                StopCoroutine(curCor);
            curCor = StartCoroutine(FadeOut(fadeOutTime, fadeOutCompleteCB));
        }

        IEnumerator FadeOut(float fadeOutTime, VoidDelegate fadeOutCompleteCB = null)
        {
#if UNITY_EDITOR
            Debug.Log("intro fade out is called");
#endif
            float fadeTime = fadeOutTime;
            var spineAnim = spineRect.GetComponent<SkeletonGraphic>();
            spineAnim.color = new Color(1f, 1f, 1f, 1f);
            
            blackFadeImg.gameObject.SetActive(false);
            while (fadeTime > 0.0f)
            {
                fadeTime -= Time.deltaTime;
                var opacity = Mathf.Max(fadeTime / fadeOutTime, 0.0f);
                spineAnim.color = new Color(1f, 1f, 1f, opacity);
                yield return new WaitForEndOfFrame();
            }

            spineAnim.color = Color.clear;
            //spineRect.transform.localScale = Vector3.zero;

            fadeOutCompleteCB?.Invoke();
        }

    }


    
}
