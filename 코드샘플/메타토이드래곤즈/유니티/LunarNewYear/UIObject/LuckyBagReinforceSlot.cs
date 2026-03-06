using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class LuckyBagReinforceSlot : MonoBehaviour
    {
        [SerializeField] List<Sprite> iconImageList = new List<Sprite>();
        [SerializeField] Image iconTarget = null;
        [SerializeField] GameObject lockNode = null;
        [SerializeField] GameObject successEffectNode = null;
        [SerializeField] GameObject failEffectNode = null;
        [SerializeField] float productionTime = 1f;

        Sequence bagTween = null;
        private void OnDisable()
        {
            InitUI();
        }

        void InitUI()
        {
            InitEffectNode();
            InitTween();
        }

        void InitTween()
        {
            if (bagTween != null)
                bagTween.Kill();

            bagTween = null;
        }

        void InitEffectNode()
        {
            if (successEffectNode != null)
                successEffectNode.SetActive(false);
            if (failEffectNode != null)
                failEffectNode.SetActive(false);
        }

        void ShowEffect(bool _isSuccess)
        {
            if (_isSuccess)
            {
                successEffectNode.SetActive(true);
                successEffectNode.GetComponent<UIParticle>().Clear();
                successEffectNode.GetComponent<UIParticle>().Play();
            }
            else
            {
                failEffectNode.SetActive(true);
                failEffectNode.GetComponent<UIParticle>().Clear();
                failEffectNode.GetComponent<UIParticle>().Play();
            }
        }

        public void SetBag(int _reinforceStep, bool _isLock, bool _isInit = false)
        {
            if (iconImageList == null || iconImageList.Count <= 0)
                return;

            if (_reinforceStep < 0 || iconImageList.Count <= _reinforceStep)
                return;

            if (_isInit)
                InitUI();

            iconTarget.sprite = iconImageList[_reinforceStep];

            if (lockNode != null)
                lockNode.SetActive(_isLock);
            if (iconTarget != null)
                iconTarget.gameObject.SetActive(!_isLock);
        }

        public void StartProduction(int _reinforceStep, bool _isSuccess)
        {
            InitUI();

            iconTarget.sprite = iconImageList[_reinforceStep > 0 ? _reinforceStep - 1 : _reinforceStep];

            bagTween = DOTween.Sequence();
            bagTween.AppendCallback(() => {
                ShowEffect(_isSuccess);
                iconTarget.gameObject.SetActive(_isSuccess);
            });
            bagTween.AppendInterval(productionTime);
            bagTween.AppendCallback(()=> {
                InitEffectNode();
                SetBag(_reinforceStep, !_isSuccess);
            });
            bagTween.Play();
        }
    }
}

