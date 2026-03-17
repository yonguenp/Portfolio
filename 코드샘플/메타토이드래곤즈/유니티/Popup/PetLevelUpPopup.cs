using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [Serializable]
    public class PetLvUpStat {

        [SerializeField]
        private GameObject statObj = null;
        [SerializeField]
        private Text statNameText = null;
        [SerializeField]
        private Text beforeStatText = null;
        [SerializeField]
        private Text afterStatText = null;
        
        public void SetObjState(bool state)
        {
            statObj.SetActive(state);
        }
        public void SetBaseStatInfo(string statType, float optionValue, float nextOptionValue, bool isOptionValuePercent = false)
        {
            if (beforeStatText !=null && afterStatText !=null)
            {
                string optionValueString = "+" + optionValue.ToString("F2");
                string nextOptionValueString = "+" + nextOptionValue.ToString("F2");
                if (isOptionValuePercent)
                {
                    optionValueString += "%";
                    nextOptionValueString += "%";
                }
                statNameText.text = StatTypeData.GetDescStringByStatType(statType, isOptionValuePercent);
                beforeStatText.text = optionValueString;
                afterStatText.text = nextOptionValueString;

            }
        }

    }
    
    public class PetLevelUpPopup : Popup<PetLevelPopupData>
    {

        [SerializeField]
        Text currentLevelLabel = null;

        [SerializeField]
        Text nextLevelLabel = null;

        [SerializeField]
        Text petNameText = null;

        [SerializeField]
        UIPetSpine petSpine = null;

        [SerializeField]
        PetLvUpStat[] PetLvUpStats = null;

        [SerializeField]
        Animator[] textAnimationController = null;

        [SerializeField]
        Text botText = null;


        Tween textTween = null;
        bool buttonLock = false;
        public override void InitUI()
        {
            if (Data != null)
            {
                SetDetailData();
            }
            if (textTween != null)
            {
                textTween.Kill();
            }
            botText.color = Color.clear;
            textTween = botText.DOColor(Color.white, 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(3f);
            SoundManager.Instance.PlaySFX("FX_LEVELUP_DRAGON1");

            CancelInvoke("ButtonUnlock");
            buttonLock = true;
            Invoke("ButtonUnlock", 0.5f);
        }
        void ButtonUnlock()
        {
            CancelInvoke("ButtonUnlock");
            buttonLock = false;
        }

        void SetDetailData()//렙업 전 , 렙업 후
        {
            var currentLevel = Data.currentLevel;
            var nextLevel = Data.nextLevel;
            var petTag = Data.tag;

            if (petTag <= 0)
            {
                Debug.Log(SBFunc.StrBuilder("petTag is ", petTag, ", ERROR"));
                ClosePopup();
                return;
            }

            var petData = User.Instance.PetData.GetPet(petTag);
            
            if (petData == null)
            {
                Debug.Log("petData is null, ERROR");
                ClosePopup();
                return;
            }
            petSpine.SetSkin(PetBaseData.Get(petData.ID).SKIN);

            petNameText.text = petData.Name();
            var statList = petData.Stats;
            if (currentLevelLabel != null)
            {
                currentLevelLabel.text = SBFunc.StrBuilder("Lv. ", currentLevel);
            }
            if (nextLevelLabel != null)
            {
                nextLevelLabel.text = SBFunc.StrBuilder("Lv. ", nextLevel);
            }
            if (statList != null)
            {
                SetStatData(statList, currentLevel, nextLevel);
            }
            TextAnimationControl();

        }
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
                if (anim == null|| anim.gameObject.activeInHierarchy ==false)
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
        void SetStatData(List<UserPetStat> statList, int currentLevel, int nextLevel)
        {
            var currentStatCount = statList.Count;
            if (PetLvUpStats == null || PetLvUpStats.Length <= 0)
            {
                return;
            }

            foreach(var item in PetLvUpStats)
            {
                item.SetObjState(false);
            }

            var currentPetReinForce = Data.reinforce;
            for (var i = 0; i < currentStatCount; ++i)
            {
                var node = PetLvUpStats[i];
                if (node == null)
                {
                    continue;
                }
                node.SetObjState(true);
                
                if (i < currentStatCount)
                {
                    string statKey = statList[i].Key.ToString();
                    PetStatData data = PetStatData.Get(statKey);
                    bool isPercent = data.VALUE_TYPE == eStatusValueType.PERCENT;
                    float curValue = PetStatData.GetStatValue(statKey, currentLevel, currentPetReinForce, statList[i].IsStatus1);
                    float nextValue;
                    if (currentLevel >= nextLevel)
                    {
                        nextValue = curValue;
                    }
                    else
                    {
                        nextValue = PetStatData.GetStatValue(statKey, nextLevel, currentPetReinForce, statList[i].IsStatus1);
                    }
                    node.SetBaseStatInfo(data.STAT_TYPE, curValue, nextValue, isPercent);
                }
            }
        }

        public void OnCloseLevelUPPopup()
        {
            if (buttonLock)
                return;

            ClosePopup();
        }
    }
}
