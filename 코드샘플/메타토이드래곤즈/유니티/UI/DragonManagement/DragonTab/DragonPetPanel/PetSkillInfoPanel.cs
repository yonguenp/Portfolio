using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetSkillInfoPanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] skillImageNode = null;
        [SerializeField]
        private Sprite[] SkillBGSprite = null;


        public void onChangeVisible(int petTag)
        {
            onShowInfo(petTag);
        }

        void onShowInfo(int petTag)
        {
            gameObject.SetActive(true);

            var petData = User.Instance.PetData.GetPet(petTag);
            if (petData == null)
            {
                return;
            }

            RefreshPetSkillIcon(petData);
        }

        void RefreshPetSkillIcon(UserPet petInfo)
        {
            initSkillIconButton();

            if (petInfo == null)
            {
                return;
            }

            var petSkillList = petInfo.SkillsID;//고유스킬은 빠진다고함
            var currentSkillIconLength = skillImageNode.Length;
            if (petSkillList == null || petSkillList.Length <= 0)
            {
                return;
            }

            if (petSkillList.Length > currentSkillIconLength)
            {
                return;
            }

            var petNormalSkillLevel = petInfo.Level;

            for (var i = 0; i < petSkillList.Length; i++)
            {
                var totalNode = skillImageNode[i];
                if (totalNode == null)
                {
                    continue;
                }
                var iconNode = SBFunc.GetChildrensByName(totalNode.transform, new string[] { "icon_bg", "skill_icon" });
                if (iconNode == null)
                {
                    continue;
                }
                var spriteComp = iconNode.GetComponent<Image>();
                if (spriteComp == null)
                {
                    continue;
                }

                var labelNode = SBFunc.GetChildrensByName(totalNode.transform, new string[] { "Label" });
                if (labelNode == null)
                {
                    continue;
                }
                var skillLabel = labelNode.GetComponent<Text>();
                if (skillLabel == null)
                {
                    continue;
                }

                var bgNode = SBFunc.GetChildrensByName(totalNode.transform, new string[] { "icon_bg" });
                if (bgNode == null)
                {
                    continue;
                }

                var petSkillID = petSkillList[i];
                if (petSkillID <= 0)
                {
                    continue;
                }

                var skillData = PetSkillNormalData.Get(petSkillID.ToString());
                if (skillData == null)
                {
                    continue;
                }

                var skillDesc = skillData.desc;
                var skillDescStr = StringData.GetStringByIndex(skillDesc);
                totalNode.SetActive(true);

                var skillValue = PetSkillNormalData.GetSkillValue(petSkillID, petNormalSkillLevel);
                skillLabel.text = string.Format(skillDescStr, Math.Round(skillValue,2));

                var skillStat = skillData.stat;
                var tempBGIndex = 0;
                switch (skillStat)//0 : 파랑(def), 1 : 빨강(atk), 2 : 보라(cri), 3 : 초록 (hp)
                {
                    case "DEF":
                        tempBGIndex = 0;
                        break;
                    case "ATK":
                        tempBGIndex = 1;
                        break;
                    case "CRI_RATE":
                        tempBGIndex = 2;
                        break;
                    case "HP":
                        tempBGIndex = 3;
                        break;
                }

                var spriteBGComp = bgNode.GetComponent<Image>();
                if (spriteBGComp != null && tempBGIndex < SkillBGSprite.Length)
                {
                    spriteBGComp.sprite = SkillBGSprite[tempBGIndex];
                }

                var bgButton = bgNode.GetComponent<Button>();
                if (bgButton != null)
                {
                    bgButton.SetInteractable(false);
                }

                //임시로 스킬 아이콘 버프로 씁니다.
                spriteComp.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.SkillIconPath, skillData.icon);
            }
        }

        void initSkillIconButton()
        {
            if (skillImageNode == null || skillImageNode.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < skillImageNode.Length; i++)
            {
                var imageNode = skillImageNode[i];
                if (imageNode == null)
                {
                    continue;
                }

                imageNode.SetActive(false);
            }
        }

        public void onHideInfo()
        {
            gameObject.SetActive(false);
        }
    }
}

