using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

namespace SandboxNetwork
{
    public class DragonPortraitFrame : MonoBehaviour
    {
        const int DRAGON_LEGENDARY_GRADE = (int)eDragonGrade.Legend;

        [SerializeField]
        Image gradeBG = null;
        [SerializeField]
        GameObject[] gradeNode = null;
        [SerializeField]
        Image sprIcon = null;
        [SerializeField]
        Text levelNode = null;
        [SerializeField]
        Text levelText = null;
        [SerializeField]
        Image elementIcon = null;
        [SerializeField]
        Image classIcon = null;
        [SerializeField]
        GameObject hideLayerNode;
        [SerializeField]
        GameObject selectCheckNode = null;//선택되어진 상태면 딤드 + 체크 처리
        [SerializeField]
        GameObject lockCheckNode = null;//선택되어진 상태면 딤드 + 자물쇠 처리
        [SerializeField]
        SlotFrameController frame = null;

        [SerializeField]
        Transform skinParent = null;
        [SerializeField]
        UIDragonSpine skin = null;

        [SerializeField]
        Sprite[] customGradeBGList = null;//임시 (드래곤 리스트용도의 커스텀BG) - 어차피 전체적으로 수정해야될텐데 픽스 안되서 일단 임시처리
        [SerializeField]
        Sprite defaultCustomImage = null;
        [SerializeField]
        Text statusText = null;
        [SerializeField]
        Transform StarParentTr = null;
        [SerializeField]
        GameObject[] TranscendenceStars = null;


        public delegate void func(string CustomEventData);

        private func clickCallback = null;

        protected int dragonID = 0;
        protected UserDragon dragonData = null;
        /**
         * 슬릇 초기화 func
         * @param index index번호
         */

        public virtual void SetDragonPortraitFrame(UserDragon _dragonData, bool isSelectCheck = false, bool clickEnable = true, bool isSpineOn = true)
        {
            InitLockNode();
            if (_dragonData == dragonData)
            {
                SetSelectCheckNode(isSelectCheck);
                SetLevelNode(_dragonData.Level);
                SetTranscendenceStar(_dragonData.TranscendenceStep, dragonData.Grade());
                dragonData = _dragonData;
                return;
            }

            dragonData = _dragonData;
            var level = dragonData.Level;
            var dragonTag = dragonData.Tag;
            dragonID = dragonTag;

            hideLayerNode.SetActive(false);
            gameObject.GetComponent<Button>().enabled = clickEnable;
            
            var dragonInfo = CharBaseData.Get(dragonTag.ToString());

            SetSelectCheckNode(isSelectCheck);
            SetTranscendenceStar(_dragonData.TranscendenceStep,dragonData.Grade());
            SetLevelNode(level);
            SetBG(dragonInfo, true, isSpineOn);
        }

        public void ClearStatusUI()
        {
            statusText.text = "";
            statusText.gameObject.SetActive(false);
        }
        //public void SetStatusAdventure()
        //{
        //    statusText.gameObject.SetActive(true);
        //    if (!string.IsNullOrEmpty(statusText.text))
        //        statusText.text += "\n";

        //    statusText.text += StringData.GetStringFormatByStrKey("탐험");
        //}

        //public void SetStatusArenaATK()
        //{
        //    statusText.gameObject.SetActive(true);
        //    if (!string.IsNullOrEmpty(statusText.text))
        //        statusText.text += "\n";

        //    statusText.text += StringData.GetStringFormatByStrKey("아레나공격");
        //}
        //public void SetStatusArenaDEF()
        //{
        //    statusText.gameObject.SetActive(true);
        //    if (!string.IsNullOrEmpty(statusText.text))
        //        statusText.text += "\n";

        //    statusText.text += StringData.GetStringFormatByStrKey("아레나방어");
        //}

        //public void SetStatusTravel()
        //{
        //    statusText.gameObject.SetActive(true);
        //    if (!string.IsNullOrEmpty(statusText.text))
        //        statusText.text += "\n";

        //    statusText.text += StringData.GetStringFormatByStrKey("여행");
        //}

        public void SetLockedDragonPortraitFrame(UserDragon _dragonData)//잠금 상태만들기
        {
            if (dragonData != null && dragonData.Tag == _dragonData.Tag)
                return;
                
            hideLayerNode.SetActive(false);
            if (lockCheckNode != null)
                lockCheckNode.gameObject.SetActive(true);
            if (selectCheckNode != null)
                selectCheckNode.gameObject.SetActive(false);

            dragonData = _dragonData;

            var level = dragonData.Level;
            var dragonTag = dragonData.Tag;
            var dragonInfo = CharBaseData.Get(dragonTag.ToString());

            SetLevelNode(level);
            SetBG(dragonInfo, false);

            dragonID = dragonTag;
            gameObject.GetComponent<Button>().enabled = true;
        }
        public void SetCustomPotraitFrame(int dragonTag, int level,int transcendLv=0, bool isHide = false, bool isSpine = true, bool isShowNameText = false)
        {
            if (hideLayerNode != null)
            {
                if (isHide)
                {
                    hideLayerNode.SetActive(true);
                    return;
                }
                hideLayerNode.SetActive(false);
            }

            var dragonInfo = CharBaseData.Get(dragonTag);
            if(dragonInfo != null)
            {
                dragonID = dragonTag;
            }
            SetLevelNode(level);
            SetBG(dragonInfo, true, isSpine);
            if(isHide == false)
            {
                 SetTranscendenceStar(transcendLv, (int)eDragonGrade.Legend);
            }
        }

        protected void SetLevelNode(int level)
        {
            if (levelNode != null)
            {
                bool isOverZeroLevel = level > 0;
                levelNode.gameObject.SetActive(isOverZeroLevel);

                var levelStr = level.ToString();
                levelNode.text = levelStr;

                if (levelText != null)
                    levelText.gameObject.SetActive(isOverZeroLevel);
            }
        }

        protected void SetBG(CharBaseData dragonInfo, bool isMine = true, bool isSpineOn = true)//grade frame ,color, legendary spine 처리 및 sprite 처리 통합
        {
            var grade = dragonInfo != null ? dragonInfo.GRADE : 0;
            if (dragonInfo != null)
            {
                //2023.06.01 WJ 모든 드래곤들 초상화를spine으로 해달라고함. - 일단 빌드 해보고 테스트 해보려함.(너무 느림)
                bool isSpinePortrait = grade == DRAGON_LEGENDARY_GRADE && isMine && isSpineOn;//너무 느려서 다시 되돌려놓음
                sprIcon.gameObject.SetActive(true);
                sprIcon.sprite = dragonInfo.GetThumbnail();
                if (skin != null)
                {
                    skin.ReleasePortraitLoad();
                    skin.SetEnableSkeletonGraphic(false);
                    skin.gameObject.SetActive(false);
                }

                skinParent.gameObject.SetActive(isSpinePortrait);
                if (isSpinePortrait)
                {
                    if (skin != null && dragonInfo != null)
                    {
                        //skin.SetPortrait(SBFunc.RandomValue * 120.0f + 3.0f, dragonInfo, () =>
                        //{
                        //    sprIcon.gameObject.SetActive(false);
                        //});
                    }
                }

                if (gradeBG != null)
                    gradeBG.sprite = GetBGIconSpriteByIndex(dragonInfo);

                if (isMine)
                {
                    if (sprIcon != null)
                    {
                        Coffee.UIEffects.UIEffect effect = sprIcon.gameObject.GetComponent<Coffee.UIEffects.UIEffect>();
                        if (effect != null)
                            effect.enabled = false;
                    }

                    if (gradeBG != null)
                    {
                        Coffee.UIEffects.UIEffect effect = gradeBG.gameObject.GetComponent<Coffee.UIEffects.UIEffect>();
                        if (effect != null)
                            effect.enabled = false;
                    }
                }
                else
                {
                    if (sprIcon != null)
                    {
                        Coffee.UIEffects.UIEffect effect = sprIcon.gameObject.GetComponent<Coffee.UIEffects.UIEffect>();
                        if (effect == null)
                            effect = sprIcon.gameObject.AddComponent<Coffee.UIEffects.UIEffect>();

                        effect.enabled = true;
                        effect.effectMode = Coffee.UIEffects.EffectMode.Grayscale;
                        effect.effectFactor = 1.0f;
                        effect.colorMode = Coffee.UIEffects.ColorMode.Multiply;
                        effect.colorFactor = 1.0f;
                        //effect.blurMode = Coffee.UIEffects.BlurMode.FastBlur;
                        //effect.blurFactor = 1.0f;
                        effect.blurMode = Coffee.UIEffects.BlurMode.None;
                    }

                    if (gradeBG != null)
                    {
                        Coffee.UIEffects.UIEffect effect = gradeBG.gameObject.GetComponent<Coffee.UIEffects.UIEffect>();
                        if (effect == null)
                            effect = gradeBG.gameObject.AddComponent<Coffee.UIEffects.UIEffect>();

                        effect.enabled = true;
                        effect.effectMode = Coffee.UIEffects.EffectMode.Grayscale;
                        effect.effectFactor = 1.0f;
                        effect.colorMode = Coffee.UIEffects.ColorMode.Multiply;
                        effect.colorFactor = 1.0f;
                        effect.blurMode = Coffee.UIEffects.BlurMode.None;
                    }
                }

                frame.SetColor(grade);
                SetGradeFrameNode(grade);
            }
            else
            {
                sprIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, "mtdz_cap");
                if (gradeBG != null)
                {
                    gradeBG.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, "default_infobg");
                    skinParent.gameObject.SetActive(false);
                }
            }

            SetClassIcon(dragonInfo);
            SetElementSpr(dragonInfo != null ? dragonInfo.ELEMENT : 0);
        }

        public void SetFrameColor(Color color)
        {
            frame.SetColor(color);
        }
        void SetElementSpr(int _elementType)
        {
            if (elementIcon != null)
            {
                if (_elementType <= 0)
                {
                    elementIcon.gameObject.SetActive(false);
                }
                else
                {
                    elementIcon.gameObject.SetActive(true);
                }
                var elementFrame = GetElementIconSpriteByIndex(_elementType);
                elementIcon.sprite = elementFrame;
            }
        }

        void SetClassIcon(CharBaseData dragonInfo)
        {
            if (classIcon == null) return;

            if (dragonInfo == null)
            {
                classIcon.gameObject.SetActive(false);
                return;
            }

            classIcon.sprite = dragonInfo.GetClassIcon();
        }

        void SetGradeFrameNode(int gradeIndex)//grade는 1부터 시작이니 -1 처리
        {
            if (gradeNode == null || gradeNode.Length < 1 || gradeNode.Length < gradeIndex)
            {
                return;
            }

            var target = gradeIndex - 1;
            var gradeCount = gradeNode.Length;
            for (var i = 0; i < gradeCount; ++i)
            {
                var gradeNode = this.gradeNode[i];
                if (gradeNode == null)
                {
                    continue;
                }

                gradeNode.gameObject.SetActive(target == i);
            }
        }
        public void SetDragonDimmed(bool isDim)
        {
            if (isDim)
            {
                sprIcon.color = Color.gray;
            }
            else
            {
                sprIcon.color = Color.white;
            }
        }
        public void setCallback(func ok_cb)
        {
            if (ok_cb != null)
            {
                clickCallback = ok_cb;
            }
        }
        public void onClickFrame()
        {
            if (clickCallback != null)
            {
                clickCallback(dragonID.ToString());
            }
        }
        public string createDummyRandomDragon()
        {
            var randomStr = Math.Round((double)SBFunc.Random(1, 5));
            return randomStr.ToString();
        }

        private Sprite GetElementIconSpriteByIndex(int e_type)
        {
            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("icon_property_", SBDefine.ConvertToElementString(e_type)));
        }

        private Sprite GetBGIconSpriteByIndex(CharBaseData dragonInfo)
        {
            if (customGradeBGList != null && customGradeBGList.Length > 0)//&& dragonInfo.GRADE < DRAGON_LEGENDARY_GRADE
                return GetCustomGradeBG(dragonInfo);
            else//레전일 때 - 디자인 변경으로 레전 전용 백판 없애달라고함
            {
                var backgound = dragonInfo.BACKGROUND;
                if(backgound == "NONE")
                    return GetDefaultGradeBG(dragonInfo);
                else
                    return ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, backgound);
            }
        }

        Sprite GetCustomGradeBG(CharBaseData dragonInfo)
        {
            var grade = dragonInfo.GRADE;
            if (grade > 0  && grade -1 < customGradeBGList.Length)
            {
                return customGradeBGList[grade - 1];
            }
            else
                return defaultCustomImage;
        }

        Sprite GetDefaultGradeBG(CharBaseData dragonInfo)
        {
            if (dragonInfo == null)
                return null;

            return dragonInfo.GetBackGround();
        }

        void InitLockNode()
        {
            if (lockCheckNode != null)
                lockCheckNode.SetActive(false);
        }

        public void SetVisibleLockNode(bool _isVisible)
        {
            if (lockCheckNode != null)
                lockCheckNode.SetActive(_isVisible);
        }
        public void SetVisibleSelectNode(bool _isVisible)
        {
            if (selectCheckNode != null)
                selectCheckNode.SetActive(_isVisible);
        }

        public void SetElemIconState(bool _isVisible)
        {
            elementIcon.gameObject.SetActive(_isVisible);
        }

        protected void SetTranscendenceStar(int starCount, int grade)
        {
            int StepMax = CharTranscendenceData.GetStepMax((eDragonGrade)grade);
            if (StarParentTr == null)
                return;
            StarParentTr.gameObject.SetActive(true);
            for (int i = 0, count = TranscendenceStars.Length; i < count; ++i)
            {
                if (TranscendenceStars[i] != null)
                {
                    TranscendenceStars[i].SetActive(i < starCount && i < StepMax);
                }
            }
        }

        protected virtual void SetSelectCheckNode(bool _isVisible)
        {
            if (selectCheckNode != null)
                selectCheckNode.gameObject.SetActive(_isVisible);
        }

        /// <summary>
        /// WJ - 기존 드래곤 프레임은 성능이슈 & 스파인 로드 느림으로 안썼던 기능인데, 개별 on/off 기능 외부에서 제어 되게끔 수정.
        /// </summary>
        /// <param name="_dragonInfo"></param>
        public void SetSpine(CharBaseData _dragonInfo)
        {
            sprIcon.gameObject.SetActive(false);

            if (skin != null)
            {
                //skin.ReleasePortraitLoad();
                skin.SetEnableSkeletonGraphic(true);
                skin.gameObject.SetActive(true);
                skin.SetData(_dragonInfo);
            }

            skinParent.gameObject.SetActive(true);

            //skin.SetPortrait(0f, _dragonInfo, () =>
            //{
            //    sprIcon.gameObject.SetActive(false);
            //});
        }
    }
}
