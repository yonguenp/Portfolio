using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GemDungeonSelectDragonSlot :MonoBehaviour
    {
        [SerializeField] DragonPortraitFrame dragonFrame;
        [SerializeField] Text battlePointText;
        [SerializeField] Slider dragonFatigueSlider;
        [SerializeField] GameObject noFatigueMarkObj;
        [SerializeField] Image SliderFillImg;
        [SerializeField] Sprite sliderFillEnoughSprite;
        [SerializeField] Sprite sliderFillLeakSprite;
        [SerializeField] CanvasGroup healEffect;

        public delegate void func(string CustomEventData);

        private readonly int lowFatigueStandard = 20;
        private bool isSelected = false;
        private bool isStageChange = false;

        public int DragonTag { get; private set; } = 0;
        Tween sliderTween = null;
        Tween healTween = null;
        public void Init(UserDragon dragonData, bool isRegist,int fatigue,int maxFatigue, func clickCB) 
        {
            battlePointText.text = dragonData.GetTotalINF().ToString();
            DragonTag = dragonData.Tag;
            dragonFrame.SetDragonPortraitFrame(dragonData, isRegist);
            if (User.Instance.DragonData.IsFavorite(dragonData.Tag))
            {
                dragonFrame.SetFrameColor(new Color(0.0f, 0.8f, 0.0f));
            }
            dragonFrame.SetElemIconState(false);
            dragonFrame.setCallback((param) => clickCB(param));
            dragonFatigueSlider.maxValue = maxFatigue;
            dragonFatigueSlider.value = fatigue;
            SliderFillImg.sprite = fatigue > lowFatigueStandard ? sliderFillEnoughSprite : sliderFillLeakSprite;
            healEffect.gameObject.SetActive(false);
            healTween?.Kill();
            dragonFrame.SetVisibleLockNode(fatigue == 0);
            noFatigueMarkObj.SetActive(fatigue == 0);
        }
        public void Init(UserDragon dragonData, bool isRegist, int fatigue, int maxFatigue,bool isSelect, func clickCB)
        {
            battlePointText.text = dragonData.GetTotalINF().ToString();
            DragonTag = dragonData.Tag;
            dragonFrame.SetDragonPortraitFrame(dragonData, isRegist);
            if (User.Instance.DragonData.IsFavorite(dragonData.Tag))
            {
                dragonFrame.SetFrameColor(new Color(0.0f, 0.8f, 0.0f));
            }
            dragonFrame.SetElemIconState(false);
            dragonFrame.setCallback((param) => clickCB(param));
            dragonFatigueSlider.maxValue = maxFatigue;
            SliderFillImg.sprite = fatigue > lowFatigueStandard ? sliderFillEnoughSprite : sliderFillLeakSprite;
            healEffect.gameObject.SetActive(false);
            healTween?.Kill();
            if (isStageChange)
            {
                if (sliderTween != null)
                    sliderTween.Kill();
                if (isSelected)
                {
                    sliderTween = dragonFatigueSlider.DOValue(maxFatigue, 0.3f);
                    SliderFillImg.sprite = sliderFillEnoughSprite;
                }
                else
                {
                    dragonFatigueSlider.value = maxFatigue;
                    sliderTween = dragonFatigueSlider.DOValue(fatigue, 0.3f);
                }
                isStageChange = false;
            }
            else
            {
                dragonFatigueSlider.value = isSelect? maxFatigue : fatigue;
                if (isSelect)
                {
                    SliderFillImg.sprite = sliderFillEnoughSprite;
                }
            }
            dragonFrame.SetVisibleLockNode(fatigue == 0);
            noFatigueMarkObj.SetActive(fatigue == 0);
        }
        private void OnDisable()
        {
            sliderTween?.Kill();
            healTween?.Kill();
            isSelected = false;
            isStageChange = false;
        }

        public void SetSelectedState(bool state)
        {
            isSelected = state;
            isStageChange = true;
        }

        public void ShowHeal()
        {
            var seq = DOTween.Sequence();
            healEffect.gameObject.SetActive(true);
            healEffect.alpha = 0;
            seq.Append(healEffect.DOFade(1f, 0.2f))
                   .Append(healEffect.DOFade(0f, 0.4f))
                   .AppendCallback(() => healEffect.gameObject.SetActive(false) );
            healTween = seq.Play();
        }
    }
}

