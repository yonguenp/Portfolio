using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBattleDragonSelectSkillIconPanel : MonoBehaviour
    {    
        [SerializeField]
        Image skillIconFrame = null;
        [SerializeField] Color[] color_frame = null;
        [SerializeField]
        Image Frame = null;
        [SerializeField]
        Text skillIconLevelLabel = null;
        [SerializeField]
        GameObject  skillLocked = null;
        [SerializeField]
        Button skillButton = null;
        [SerializeField]
        GameObject skillIconFrameObject = null;
        ChampionBattleDragonSelectPopup ParentPopup { get { return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>(); } }
        public void Init()
        {
            RefreshCurrentDragonData();
        }

        void RefreshCurrentDragonData()
        {
            if (ParentPopup.DragonTag != 0)//드래곤 태그값
            {
                SetSkillDescData(ParentPopup.Dragon);
            }
        }
        void SetSkillDescData(ChampionDragon dragonData)
        {
            Color frame_clr = color_frame[0];
            int tempSkillLevel;

            if (dragonData != null)
            {
                tempSkillLevel = dragonData.SLevel;
                frame_clr = color_frame[dragonData.Grade()];
            }
            else
                tempSkillLevel = 1;

            Frame.color = frame_clr;

            var designData = dragonData.BaseData;
            if (designData == null)
                return;

            var skillData = designData.SKILL1;
            if (skillData == null)
            {
                if (skillLocked != null)
                    skillLocked.gameObject.SetActive(true);

                if (skillButton != null)
                    skillButton.SetButtonSpriteState(false);

                return;
            }

            if (skillLocked != null)
                skillLocked.gameObject.SetActive(false);

            if (skillButton != null)
                skillButton.SetButtonSpriteState(true);

            SetSkillIconImage(skillData.GetIcon());
            SetSkillLevelLabel(tempSkillLevel);
        }

        void SetSkillIconImage(Sprite icon)
        {
            skillIconFrame.sprite = icon;
        }
        void SetSkillLevelLabel(int slevel)
        {
            string levelFormat = string.Format(StringData.GetStringByIndex(100000056), slevel);
            skillIconLevelLabel.text = levelFormat;
        }

        CharBaseData GetDragonDesignData(int _tag)
        {
            var dragonData = CharBaseData.Get(_tag.ToString());
            if (dragonData != null)
                return dragonData;
            else
                return null;
        }
    }
}
