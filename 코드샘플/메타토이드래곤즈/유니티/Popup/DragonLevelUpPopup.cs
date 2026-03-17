using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonLevelUpPopup : Popup<DragonLevelPopupData>
    {
        [SerializeField]
        GameObject spineParent = null;

        [SerializeField]
        Text upperText = null;
        [SerializeField]
        Text levelLabel = null;
        [SerializeField]
        Text nameLabel = null;

        [SerializeField]
        DragonLevelupDescPanel nextLevelComp = null;//현재 및 다음 레벨 변화 표시 노드 (컴포넌트 따로 뺌)

        [SerializeField]
        Animator[] textAnimationController = null;

        [SerializeField]
        GameObject tweenObj = null;

        [SerializeField]
        Text botText = null;

        [SerializeField]
        StatPanel statPanel = null;//현재 및 다음 레벨 변화 표시 노드 (컴포넌트 따로 뺌)

        Tween textTween = null;

        int prevLevel = 0;
        int finalLevel = 0;
        int dragonTag = 0;

        bool buttonLock = false;
        public override void InitUI()
        {
            var currentLevel = Data.CurrentDragonLevel;//레벨업 전 레벨
            var finalLevel = Data.NextDragonLevel;//레벨업 이후 레벨
            var prevStat = Data.PrevDragonStat;

            this.finalLevel = finalLevel;
            this.prevLevel = currentLevel;

            RefreshCurrentDragonData();
            SetSpineData();
            SetDetailData(prevStat, currentLevel);
            SetCurrentLevel();
            TextAnimationControl();
            SoundManager.Instance.PlaySFX("FX_LEVELUP_DRAGON1");
            
            
            if(textTween != null)
            {
                textTween.Kill();
            }
            botText.color = Color.clear;
            textTween =  botText.DOColor(Color.white, 1f).SetLoops(-1,LoopType.Yoyo).SetDelay(3f);
            //UICanvas.Instance.StartBackgroundBlurEffect();

            CancelInvoke("ButtonUnlock");
            buttonLock = true;
            Invoke("ButtonUnlock", 0.5f);
        }

        void ButtonUnlock()
        {
            CancelInvoke("ButtonUnlock");
            buttonLock = false;
        }

        void RefreshCurrentDragonData()
        {

            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag != 0)//드래곤 태그값
            {
                var dragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
                var dragonData = User.Instance.DragonData;
                if (dragonData == null)
                {
                    Debug.Log("user's dragon Data is null");
                    return;
                }

                var userDragonInfo = dragonData.GetDragon(dragonTag);
                if (userDragonInfo == null)
                {
                    Debug.Log("user Dragon is null");
                    return;
                }

                this.dragonTag = dragonTag;

                if (nameLabel != null)
                {
                    nameLabel.text = userDragonInfo.Name();
                }
            }
        }
        void SetDetailData(CharacterStatus prevStat, int prevLevel)
        {
            var dragonData = User.Instance.DragonData.GetDragon(dragonTag);
            if (dragonData == null)
                return;

            if (statPanel != null)
            {
                var baseData = dragonData.BaseData;
                statPanel.PrevINF = prevStat.GetTotalINF();
                statPanel.Initialize(dragonData.Status.GetTotalINF(), SBFunc.BaseCharStatus(prevLevel, baseData, StatFactorData.Get(baseData.FACTOR)), SBFunc.BaseCharStatus(dragonData.Level, baseData, StatFactorData.Get(baseData.FACTOR)));
            }
            else if (nextLevelComp != null)
            {
                nextLevelComp.Init(dragonTag, prevStat, prevLevel, true);

                ChangeBattlePoint();
            }
        }

        void SetSpineData()
        {
            SBFunc.RemoveAllChildrens(spineParent.transform);

            if (dragonTag <= 0)
            {
                return;
            }

            var dragonData = User.Instance.DragonData.GetDragon(dragonTag);
            if (dragonData == null)
            {
                return;
            }
            var baseData = dragonData.BaseData;
            var dragonPrefab = baseData.GetUIPrefab();
            if (dragonPrefab != null)
            {
                var clone = Instantiate(dragonPrefab, spineParent.transform);
                clone.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                clone.GetComponent<RectTransform>().localScale = Vector3.one;
                var dragonSpine = clone.GetComponent<UIDragonSpine>();
                if (dragonSpine != null)
                {
                    dragonSpine.SetData(dragonData);
                    dragonSpine.InitComplete = ChangeAnimation;
                }
            }
        }

        void ChangeAnimation(UIDragonSpine data)
        {
            data.SetAnimation(eSpineAnimation.WIN);
        }

        string GetDragonImageName(int dragonTag)
        {
            var dragonData = CharBaseData.Get(dragonTag.ToString());
            if (dragonData != null)
            {
                return dragonData.IMAGE;
            }
            return "";
        }

        void SetCurrentLevel()
        {
            if (levelLabel != null)
            {
                levelLabel.text = prevLevel.ToString();//이전 레벨로 세팅
            }
        }

        public void ChangeLevelText()//애니메이션에 의해 호출하면 증감된 숫자로 변경하기
        {
            if (upperText != null)
            {
                upperText.text = SBFunc.StrBuilder("+", finalLevel - prevLevel);
            }

            if (levelLabel != null)
            {
                levelLabel.text = finalLevel.ToString();//이전 레벨로 세팅
            }
        }

        //텍스트 애니메이션 차례대로 켜기(시간순 제어)
        void TextAnimationControl()
        {
            if (textAnimationController == null || textAnimationController.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < textAnimationController.Length; i++)
            {
                var anim = textAnimationController[i];
                if (anim == null)
                {
                    continue;
                }

                anim.speed = 0;//전부 초기화
            }

            var tweenSeq = DOTween.Sequence();

            for (var i = 0; i < textAnimationController.Length; i++)
            {
                var anim = textAnimationController[i];
                if (anim == null)
                {
                    continue;
                }

                tweenSeq.AppendInterval(0.5f);
                tweenSeq.AppendCallback(() =>
                {
                    anim.speed = 1;
                });
            }

            tweenSeq.Play();
        }

        void ChangeBattlePoint()
        {
            var currentStat = nextLevelComp.CurrentStat;
            var prevStat = nextLevelComp.PrevStat;

            if (nextLevelComp.battleLabel != null)
                nextLevelComp.battleLabel.DOCounter(prevStat.GetTotalINF(), currentStat.GetTotalINF(), 1f).SetEase(Ease.InOutQuad);
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            //UICanvas.Instance.EndBackgroundBlurEffect();
        }
        public void OnCloseLevelUPPopup()
        {
            if (buttonLock)
                return;

            ClosePopup();
        }
    }
}