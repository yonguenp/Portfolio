using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SimulatorDragonEditPetSkillSlot : MonoBehaviour // 펫 부옵 스킬 표시 부
    {
        [SerializeField]
        private Sprite emptySprite = null;

        [SerializeField]
        private Sprite emptyBGSprite = null;

        [SerializeField]
        private Image skillBG = null;

        [SerializeField]
        private Image skillIcon = null;
        
        [SerializeField]
        private Text valueLabel = null;

        [SerializeField]
        private Sprite[] SkillBGSprite = null;

        private PetSkillNormalTable petSkillTable = null;

        void initTable()
        {
            if(petSkillTable == null)
            {
                petSkillTable = TableManager.GetTable<PetSkillNormalTable>();
            }
        }

        public void RefreshPetSkillIcon(int petLevel, int petSkillID)
        {
            var petNormalSkillLevel = petLevel;
            if(petSkillID == 0)
            {
                initSkillSlot();
                return;
            }

            var skillData = petSkillTable.Get(petSkillID);
            if (skillData == null)
            {
                initSkillSlot();
                return;
            }

            var skillDesc = skillData.desc;
            var skillDescStr = StringData.GetStringByIndex(skillDesc);
            var skillValue = petSkillTable.GetSkillValue(petSkillID, petNormalSkillLevel);
            valueLabel.text = Math.Round(skillValue, 2).ToString() + "%";//string.Format(skillDescStr, Math.Round(skillValue, 2));

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

            if (skillBG != null && tempBGIndex < SkillBGSprite.Length)
            {
                skillBG.sprite = SkillBGSprite[tempBGIndex];
            }

            skillIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.SkillIconPath, skillData.icon);
        }
        public void initSkillSlot()
        {
            initTable();

            if (valueLabel != null)
            {
                valueLabel.text = StringData.GetStringByIndex(0);
            }

            if(skillBG != null)
            {
                skillBG.sprite = emptyBGSprite;
            }

            if(skillIcon != null)
            {
                skillIcon.sprite = emptySprite;
            }
        }

        public void SetVisibleSkillSlot(bool _isVisible)
        {
            gameObject.SetActive(_isVisible);
        }
    }
}

