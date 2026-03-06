using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
	enum CabsuleGrade
	{
		NONE,
		NORMAL,
		RARE,
		SUPER_RARE,
		ULTRA_RARE,
		LEGEND
	}

	public class GachaPortraitFrame: MonoBehaviour
    {
        [SerializeField]
        GameObject[] gradeNode = null;

        [SerializeField]
        Image sprIcon = null;

        [SerializeField]
        Text levelNode = null;

        [SerializeField]
        Image elementIcon = null;

        private Image bgSprite = null;

        public delegate void func(string CustomEventData);

        private int dragonID = 0;
        private UserDragon dragonData = null;
        /**
         * 슬릇 초기화 func
         * @param index index번호
         */

        public void SetPortraitFrame(UserDragon dragonData, bool isSelectCheck = false)
        {
            this.dragonData = dragonData;

            //index : number, level : number = 0, elementType : number = 0
            var level = this.dragonData.Level;
            var dragonTag = this.dragonData.Tag;
            var dragonInfo = CharBaseData.Get(dragonTag.ToString());

            var elementType = dragonInfo != null ? dragonInfo.ELEMENT : 0;
            var grade = dragonInfo != null ? dragonInfo.GRADE : 0;

            if (levelNode != null)
            {
                if (level <= 0)
                {
                    levelNode.gameObject.SetActive(false);
                }
                else
                {
                    levelNode.gameObject.SetActive(true);
                }

                var levelStr = level.ToString();
                levelNode.text = levelStr;
            }

            if (elementIcon != null)
            {
                if (elementType <= 0)
                {
                    elementIcon.gameObject.SetActive(false);
                }
                else
                {
                    elementIcon.gameObject.SetActive(true);
                }
                var elementFrame = this.GetElementIconSpriteByIndex(elementType);
                elementIcon.sprite = elementFrame;
            }

            Sprite icon = null;
            if (dragonInfo != null)
                icon = dragonInfo.GetThumbnail();

            if (bgSprite == null)
            {
                var bgNode = SBFunc.GetChildrensByName(gameObject.transform, new string[] { "Background" });
                if (bgNode != null)
                {
                    bgSprite = bgNode.GetComponent<Image>();
                }
            }

            if (bgSprite != null)
            {
                bgSprite.sprite = dragonInfo.GetBackGround();
            }

            sprIcon.sprite = icon;
            dragonID = dragonTag;
            SetGradeFrameNode(grade);
        }
		
        public void SetCustomPotraitFrame(UserDragon dragonData)
        {
            this.dragonData = dragonData;

            var level = this.dragonData.Level;
            var dragonTag = this.dragonData.Tag;
            var dragonInfo = CharBaseData.Get(dragonTag); // TableManager.GetTable<CharBaseTable>().datas[dragonTag.ToString()];

            var elementType = dragonInfo != null ? dragonInfo.ELEMENT : 0;
            var grade = dragonInfo != null ? dragonInfo.GRADE : 0;

            if (levelNode != null)
            {
                if (level <= 0)
                {
                    levelNode.gameObject.SetActive(false);
                }
                else
                {
                    levelNode.gameObject.SetActive(true);
                }

                var levelStr = level.ToString();
                levelNode.text = levelStr;
            }

            if (elementIcon != null)
            {
                if (elementType <= 0)
                {
                    elementIcon.gameObject.SetActive(false);
                }
                else
                {
                    elementIcon.gameObject.SetActive(true);
                }
                var elementFrame = GetElementIconSpriteByIndex(elementType);
                elementIcon.sprite = elementFrame;
            }

            Sprite icon = null;
            if (dragonInfo != null)
                icon = dragonInfo.GetThumbnail();

            if (bgSprite == null)
            {
                var bgNode = SBFunc.GetChildrensByName(gameObject.transform, new string[] { "Background" });
                if (bgNode != null)
                {
                    bgSprite = bgNode.GetComponent<Image>();
                }
            }

            if (bgSprite != null)
            {
                bgSprite.sprite = dragonInfo.GetBackGround();
            }

            sprIcon.sprite = icon;
            dragonID = dragonTag;
            SetGradeFrameNode(grade);
        }

        void SetGradeFrameNode(int gradeIndex)//grade는 1부터 시작이니 -1 처리
        {
            if (gradeNode == null || gradeNode.Length < 1 || gradeNode.Length < gradeIndex)
            {
                return;
            }

            var target = gradeIndex - 1;
            var gradeCount = this.gradeNode.Length;
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

        public string createDummyRandomDragon()
        {
            var randomStr = Math.Round((double)SBFunc.Random(1, 5));
            return randomStr.ToString();
        }

        private Sprite GetElementIconSpriteByIndex(int e_type)
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

            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("type_", elementStr));
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

        private string MakeStringByGradeAndElement(int grade, int element)
        {
            var elementString = this.GetElementConvertString(element);
            var gradeString = this.GetGradeConvertString(grade);

            return elementString + "_" + gradeString + "_infobg";
        }
    }
}
