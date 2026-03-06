using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonCardFrame : MonoBehaviour
    {
        [SerializeField]
        Color[] gradeColor = null;
        [SerializeField]
        Sprite[] gradeSprite = null;

        [SerializeField]
        GameObject[] gradeNode = null;

        [SerializeField]
        GameObject dragonImageParentNode = null;
        [SerializeField]
        Vector3 characterScale = new Vector3(1, 1, 1);

        [SerializeField]
        Vector3 characterPos = new Vector3(0, 0, 0);

        [SerializeField]
        private Image bgSprite = null;

        [SerializeField]
        private Image sprFrame = null;

        [SerializeField]
        private Image sprIcon = null;

        [SerializeField]
        private GameObject secretMask = null;

        [SerializeField]
        private Image elementIcon = null;

        [SerializeField]
        private GameObject selectCheckNode = null;//선택되어진 상태면 딤드 + 체크 처리

        [SerializeField]
        private GameObject duplicationNode = null;

        [SerializeField]
        private GameObject minusIcon = null;
        [SerializeField]
        private Text amountText = null;

        private UserDragonCard targetCard = null;

        public bool IsSelect { get { return isSelect; } }
        private bool isSelect = false;
        private Sprite character_icon = null;
        public delegate void func(DragonCardFrame frame, UserDragonCard cardData);

        private func clickCallback;
        UIDragonSpine dragonSpine = null;
        public func ClickCallBack
        {
            get { return clickCallback; }
            set { clickCallback = value; }
        }

        /**
         * 슬릇 초기화 func
         * @param index index번호
         */
        public void InitCardFrame(UserDragonCard _cardData, bool _isSelectCheck = false , bool _isCustomFrame = false, int amount = 1)
        {
            if (targetCard != null && targetCard == _cardData)
            {
                SetAmount(amount);
                return;
            }

            targetCard = _cardData;
            var index = _cardData.DragonTag;

            var dragonInfo = CharBaseData.Get(index.ToString());
            if (dragonInfo == null)
                return;

            character_icon = dragonInfo.GetThumbnail();

            var elementType = dragonInfo.ELEMENT;
            var grade = dragonInfo.GRADE;

            if (elementIcon != null)
            {
                elementIcon.gameObject.SetActive(elementType > 0);
                var elementFrame = GetElementIconSpriteByIndex(elementType);
                elementIcon.sprite = elementFrame;
            }

            if (bgSprite == null)
            {
                var bgNode = SBFunc.GetChildrensByName(gameObject.transform, new string[] { "Background" });
                if (bgNode != null)
                    bgSprite = bgNode.GetComponent<Image>();
            }

            if (bgSprite != null)
                bgSprite.sprite = dragonInfo.GetBackGround();

            if (secretMask != null)
                secretMask.gameObject.SetActive(false);

            if (sprIcon != null)
            {
                sprIcon.gameObject.SetActive(true);
                sprIcon.sprite = character_icon;
            }

            if (minusIcon != null)
                minusIcon.gameObject.SetActive(false);

            SetGradeFrameNode(-1);
            //SetSelect(_isSelectCheck);

            if (_isCustomFrame)
                SetCustomFrame(grade);

            SetAmount(amount);
        }

        void SetAmount(int _amount)
        {
            if (_amount > 1)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = _amount.ToString();
            }
            else
                amountText.gameObject.SetActive(false);
        }


        public void InitCardSecret(int grade)
        {
            if (bgSprite == null)
            {
                var bgNode = SBFunc.GetChildrensByName(gameObject.transform, new string[] { "Background" });
                if (bgNode != null)
                    bgSprite = bgNode.GetComponent<Image>();
            }

            if (bgSprite != null)
                bgSprite.sprite = GetBGIconSpriteByIndex("NONE", grade, SBFunc.Random((int)eElementType.FIRE, (int)eElementType.MAX));

            if (secretMask != null)
                secretMask.SetActive(true);

            if (sprIcon != null)
                sprIcon.gameObject.SetActive(false);

            SetGradeFrameColor(-1);
            SetGradeFrameNode(-1);
            //SetSelect(false);
        }

        public void SetSelect(bool isSelectCheck)
        {
            isSelect = isSelectCheck;
            if (selectCheckNode != null)
                selectCheckNode.gameObject.SetActive(isSelect);
        }

        void SetGradeFrameColor(int gradeIndex)//grade는 1부터 시작이니 -1 처리
        {
            if (gradeColor == null || gradeColor.Length < 1 || gradeColor.Length < gradeIndex)
                return;

            if (sprFrame == null)
                return;

            if (gradeIndex < 0)
                sprFrame.color = Color.white;

            sprFrame.color = gradeColor[gradeIndex - 1];
        }

        public void SetGradeFrameNode(int gradeIndex)//grade는 1부터 시작이니 -1 처리
        {
            if (gradeNode == null || gradeNode.Length < 1)
                return;

            if (gradeNode.Length < gradeIndex || gradeIndex < 0)
            {
                var gradeCountCondition = gradeNode.Length;
                for (var i = 0; i < gradeCountCondition; ++i)
                {
                    var node = gradeNode[i];
                    if (node == null)
                        continue;

                    node.gameObject.SetActive(false);
                }
            }
            else
            {
                var target = gradeIndex - 1;
                var gradeCount = gradeNode.Length;
                for (var i = 0; i < gradeCount; ++i)
                {
                    var node = gradeNode[i];
                    if (node == null)
                        continue;

                    node.gameObject.SetActive(target == i);
                }
            }
        }

        public void SetVisibleMinusIcon(bool isVisible)
        {
            if (minusIcon != null)
                minusIcon.gameObject.SetActive(isVisible);
        }

        public void OnClickFrame()
        {
            if (clickCallback != null)
                clickCallback(this, targetCard);
        }

        private Sprite GetElementIconSpriteByIndex(int e_type)
        {
            var elementStr = GetElementConvertString(e_type);
            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("type_", elementStr));
        }

        private Sprite GetBGIconSpriteByIndex(string background,int grade,int element)
        {
            if (background == "NONE")
            {
                var fullStr = MakeStringByGradeAndElement(grade, element);
                return ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, fullStr);
            }
            else
            {
                return ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, background);
            }
        }

        private string GetGradeConvertString(int grade)
        {
            var gradeNameStrIndex = CharGradeData.Get(grade.ToString())._NAME;
            var gradeString = StringData.GetStringByIndex(gradeNameStrIndex).ToLower();
            return gradeString;
        }

        private string GetElementConvertString(int e_type)
            {
            var elementStr = "";
            switch (e_type)
            {
                case 1:
                    elementStr = "fire";
                    break;
                case 2:
                    elementStr = "water";
                    break;
                case 3:
                    elementStr = "soil";
                    break;
                case 4:
                    elementStr = "wind";
                    break;
                case 5:
                    elementStr = "light";
                    break;
                case 6:
                    elementStr = "dark";
                    break;
                default:
                    break;
            }
            return elementStr;
        }

        private string MakeStringByGradeAndElement(int grade,int element)
        {
            string gradeString = "";
            if (grade == (int)eDragonGrade.Legend)
            {
                gradeString = GetGradeConvertString(grade);
                return gradeString + "_00" + SBFunc.Random(1, 7);
            }

            var elementString = GetElementConvertString(element);
            gradeString = GetGradeConvertString(grade);

            return elementString + "_" + gradeString + "_infobg";
        }

        void SetDuplicationNode(bool isVisible)
        {
            if (duplicationNode != null)
                duplicationNode.gameObject.SetActive(isVisible);
        }

        void SetCustomFrame(int _grade)
        {
            var modifyIndex = _grade - 1;
            if(gradeColor != null && gradeSprite != null && gradeColor.Length > 0 && gradeSprite.Length > 0
                && modifyIndex >= 0 && gradeColor.Length > modifyIndex && gradeSprite.Length > modifyIndex)
            {
                if(bgSprite != null)
                    bgSprite.sprite = gradeSprite[modifyIndex];

                if (sprFrame != null)
                    sprFrame.color = gradeColor[modifyIndex];
            }
        }
        void RefreshDragonCharacterSlot(int _dragonTag, CharBaseData _baseData)
        {
            if (dragonImageParentNode == null)
                return;

            SBFunc.RemoveAllChildrens(dragonImageParentNode.transform);

            var dragonImageName = GetDragonImageName(_dragonTag);
            if (dragonImageName != "")
            {
                var dragonPrefab = _baseData.GetUIPrefab();
                if (dragonPrefab != null)
                {
                    var clone = Instantiate(dragonPrefab, dragonImageParentNode.transform);
                    clone.GetComponent<RectTransform>().anchoredPosition = characterPos;
                    clone.GetComponent<RectTransform>().localScale = characterScale;
                    dragonSpine = clone.GetComponent<UIDragonSpine>();
                    if (dragonSpine != null)
                    {
                        dragonSpine.SetData(_baseData);
                        dragonSpine.InitComplete = SpineInitCallback;
                    }
                }
            }
        }

        void SpineInitCallback(UIDragonSpine spineData)
        {
            spineData.SkeletonAni.timeScale = 0f;
        }
        string GetDragonImageName(int dragonTag)
        {
            var dragonData = CharBaseData.Get(dragonTag.ToString());
            if (dragonData != null)
                return dragonData.IMAGE;
            else
                return "";
        }
    }
}
