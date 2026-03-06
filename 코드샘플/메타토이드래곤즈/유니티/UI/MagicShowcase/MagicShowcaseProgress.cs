using Coffee.UIEffects;
using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 현재 레벨에 따른 이미지(배경)프로그래스 , 기본 프로그래스 세팅
    /// </summary>
    public class MagicShowcaseProgress : MagicShowcaseComponent
    {
        [SerializeField] List<Sprite> imageList = new List<Sprite>();
        [SerializeField] Image targetImage = null;
        [SerializeField] Image maskImage = null;
        [SerializeField] UIGradient gradient = null;
        [SerializeField] float completePos = 620f;
        [SerializeField] float maxPos = 696f;

        [SerializeField] Text progressLabel = null;
        [SerializeField] Slider slider = null;

        [SerializeField] GameObject effectParentNode = null;
        [SerializeField] SkeletonGraphic targetSpine = null;
        [SerializeField] GameObject levelupEffectNode = null;

        [SerializeField] GameObject maxLevelParticleEffect = null;
        [SerializeField] GameObject maxLevelSpineNode = null;
        [SerializeField] List<SkeletonGraphic> maxLevelSpineList = new List<SkeletonGraphic>();

        [SerializeField] GameObject maxLevelSpineBgParentEffect = null;
        [SerializeField] List<GameObject> maxLevelSpineBgList = new List<GameObject>();

        int currentLevel = -1;
        int maxLevel = -1;

        bool isLevelupProduction = false;

        int tempPrevIndex = -1;

        DG.Tweening.Sequence boxEffect = null;
        DG.Tweening.Sequence levelupEffect = null;
        DG.Tweening.Sequence maxLevelupSpine = null;
        DG.Tweening.Sequence blindOutSequence = null;


        private void OnDisable()
        {
            tempPrevIndex = -1;
            currentLevel = -1;
        }

        public override void InitUI(int _currentTabIndex)
        {
            base.InitUI(_currentTabIndex);

            Init();

            SetData();
            SetPictureProgress();
            SetSlider();
            SetLabel();
            SetEffect();
            SetMaxLevelUI();
        }

        void Init()
        {
            isLevelupProduction = false;
            if (tempPrevIndex != index)
            {
                if (tempPrevIndex >= 0)
                    currentLevel = -1;

                tempPrevIndex = index;
            }

            if (effectParentNode != null)
                effectParentNode.SetActive(false);

            if (maxLevelSpineNode != null)
                maxLevelSpineNode.SetActive(false);
            
            if (maxLevelParticleEffect != null)
                maxLevelParticleEffect.SetActive(false);

            if (maskImage != null)
                maskImage.gameObject.SetActive(true);

            if (maxLevelSpineBgParentEffect != null)
                maxLevelSpineBgParentEffect.SetActive(false);

            if (levelupEffect != null)
                levelupEffect.Kill();
            levelupEffect = null;

            if (maxLevelupSpine != null)
                maxLevelupSpine.Kill();
            maxLevelupSpine = null;

            if (blindOutSequence != null)
                blindOutSequence.Kill();
            blindOutSequence = null;

            if (RefreshCallBack == null)
                RefreshCallBack = ShowBlindOutEffect;

        }

        void SetData()
        {
            if (currentLevel != infoData.LEVEL)
            {
                if (currentLevel >= 0)
                    isLevelupProduction = true;

                currentLevel = infoData.LEVEL;
            }

            maxLevel = MagicShowcaseData.GetMaxLevelByGroup(type);
        }

        private void Update()
        {
            if(gradient != null)
            {
                gradient.offset = (((float)currentLevel / maxLevel) * 2.0f) - 1.0f;//range -1 ~ 1
            }
        }

        void SetPictureProgress()
        {
            if (boxEffect != null)
                boxEffect.Kill();

            if (index >= 0 && index < imageList.Count)
            {
                targetImage.sprite = imageList[index];
                maskImage.sprite = imageList[index];
            }
            Vector2 goalPos = new(0, completePos * currentLevel / maxLevel);
            maskImage.transform.rotation = Quaternion.identity;
            if (isLevelupProduction)
            {
                float gaugeDelay = 0.3f;
                if (currentLevel == maxLevel)
                {
                    goalPos = new(0, maxPos);
                    gaugeDelay = 0.7f;
                }

                targetImage.gameObject.GetComponent<RectTransform>().anchoredPosition = new(0, completePos * (currentLevel - 1) / maxLevel);
                targetImage.gameObject.GetComponent<RectTransform>().DOAnchorPos(goalPos, gaugeDelay);

                boxEffect = DOTween.Sequence().Append(maskImage.GetComponent<RectTransform>().DOScale(1.2f, 0.15f)).AppendInterval(0.15f)
                    .Append(maskImage.GetComponent<RectTransform>().DOScale(1f, 0.15f)).Play();
            }
            else
            {
                targetImage.gameObject.GetComponent<RectTransform>().anchoredPosition = goalPos;
                maskImage.GetComponent<RectTransform>().localScale = Vector3.one;

                boxEffect = DOTween.Sequence().AppendInterval(3f)
                    .Append(maskImage.GetComponent<RectTransform>().DORotate(Vector3.forward * 5f, 0.3f).SetEase(Ease.InOutBack))
                    .Append(maskImage.GetComponent<RectTransform>().DORotate(Vector3.forward * -5f, 0.3f).SetEase(Ease.InOutBack))
                    .Append(maskImage.GetComponent<RectTransform>().DORotate(Vector3.zero, 0.3f).SetEase(Ease.InOutBack))
                    .AppendInterval(5f)
                    .SetLoops(-1, LoopType.Restart).Play();
            }
        }

        void SetSlider()
        {
            if (slider == null)
                return;

            float goalValue = (float)currentLevel / maxLevel;
            if (isLevelupProduction)
            {
                slider.value = (float)(currentLevel - 1) / maxLevel;
                slider.DOValue(goalValue, 0.5f);
            }
            else
                slider.value = goalValue;
        }

        void SetLabel()
        {
            if (progressLabel == null)
                return;

            var goalText = SBFunc.StrBuilder(currentLevel, "/", maxLevel);
            if (isLevelupProduction)
            {
                progressLabel.text = SBFunc.StrBuilder(currentLevel - 1, "/", maxLevel);
                progressLabel.DOText(goalText, 0.5f);
            }
            else
                progressLabel.text = goalText;
        }

        void SetEffect()
        {
            if (!isLevelupProduction)
                return;

            effectParentNode.SetActive(true);
            targetSpine.AnimationState.SetAnimation(0, "animation", false);

            if (levelupEffect != null)
                levelupEffect.Kill();

            levelupEffect = DOTween.Sequence();

            levelupEffect.AppendInterval(1.3f).AppendCallback(() => {

                if (maxLevel != infoData.LEVEL)
                    ToastManager.On(StringData.GetStringByStrKey("레벨업보상스탯획득"));
                else
                    PlayMaxLevelSpine();
            }).Play();

            levelupEffectNode.SetActive(true);
        }

        void ShowTargetMaxSpine()
        {
            for(int i = 0; i< maxLevelSpineList.Count; i++)
            {
                maxLevelSpineList[i].gameObject.SetActive(index == i);
            }
        }

        void ShowTargetMaxSpineBG()
        {
            for (int i = 0; i < maxLevelSpineBgList.Count; i++)
            {
                maxLevelSpineBgList[i].SetActive(index == i);
            }
        }

        SkeletonGraphic GetCurrentMaxSpine()
        {
            for (int i = 0; i < maxLevelSpineList.Count; i++)
            {
                if(index == i)
                    return maxLevelSpineList[i];
            }
            return null;
        }

        void PlayMaxLevelSpine()
        {
            if (maxLevelSpineNode == null)
                return;

            if (maxLevelupSpine != null)
                maxLevelupSpine.Kill();

            maxLevelupSpine = DOTween.Sequence();

            maxLevelSpineNode.SetActive(true);
            maxLevelSpineBgParentEffect.SetActive(true);
            ShowTargetMaxSpine();
            ShowTargetMaxSpineBG();
            SetMaxSpineAnimation("open", false);

            maskImage.gameObject.SetActive(false);
            maxLevelupSpine.AppendInterval(1f).AppendCallback(()=> {
                maxLevelParticleEffect.SetActive(true);
            }).AppendInterval(0.59f).AppendCallback(() =>
            {
                SetMaxSpineAnimation("idle", true);
                ToastManager.On(StringData.GetStringByStrKey("최대레벨도달"));
            }).Play();
        }

        void SetMaxSpineAnimation(string _animName, bool _isLoop)
        {
            GetCurrentMaxSpine().AnimationState.SetAnimation(0, _animName, _isLoop);
        }

        void SetMaxLevelUI()
        {
            if (infoData.LEVEL != maxLevel)
                return;

            if (isLevelupProduction)
                return;

            maskImage.gameObject.SetActive(false);
            maxLevelSpineNode.SetActive(true);
            maxLevelSpineBgParentEffect.SetActive(true);
            ShowTargetMaxSpine();
            ShowTargetMaxSpineBG();
            SetMaxSpineAnimation("idle", true);
        }
        void ShowBlindOutEffect()
        {
            if (blindOutSequence != null)
                blindOutSequence.Kill();
            blindOutSequence = DOTween.Sequence();
            blindOutSequence.Append(targetImage.DOFade(0, 0.3f)).Append(targetImage.DOFade(230 / 255f, 0.3f));
        }
    }
}