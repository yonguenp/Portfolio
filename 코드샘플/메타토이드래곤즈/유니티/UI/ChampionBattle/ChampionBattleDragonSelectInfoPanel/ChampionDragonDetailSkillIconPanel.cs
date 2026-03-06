using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionDragonDetailSkillIconPanel : MonoBehaviour
    {    
        [SerializeField]
        Image skillIconFrame = null;
        [SerializeField] Color[] color_frame;
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
        ChampionDragonDetailPopup ParentPopup { get { return PopupManager.GetPopup<ChampionDragonDetailPopup>(); } }

        ChampionDragon dragon = null;
        public void Init()
        {
            dragon = ParentPopup.Dragon;
            RefreshCurrentDragonData();
        }

        void RefreshCurrentDragonData()
        {
            if (dragon == null)
            {
                Debug.Log("user's dragon Data is null");
                return;
            }

            SetSkillDescData();
        }
        void SetSkillDescData()
        {
            Color frame_clr = color_frame[0];
            int tempSkillLevel;

            if(dragon != null)
            {
                tempSkillLevel = dragon.SLevel;
                frame_clr = color_frame[dragon.Grade()];
            }
            else
                tempSkillLevel = GameConfigTable.GetSkillLevelMax();

            Frame.color = frame_clr;

            var designData = GetDragonDesignData(dragon.Tag);
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
