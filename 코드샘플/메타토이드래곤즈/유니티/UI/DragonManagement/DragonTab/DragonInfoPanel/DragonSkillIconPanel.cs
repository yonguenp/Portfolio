using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonSkillIconPanel : MonoBehaviour
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
        [SerializeField]
        UIEffect effect = null;
        public void Init()
        {
            RefreshCurrentDragonData();
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

                SetSkillDescData(dragonTag);
            }
        }
        void SetSkillDescData(int _dragonTag)
        {
            Color frame_clr = color_frame[0];
            int tempSkillLevel;
            var hasDragon = User.Instance.DragonData.IsUserDragon(_dragonTag);
            if (hasDragon)
            {
                var dragonData = User.Instance.DragonData.GetDragon(_dragonTag);
                if (dragonData != null)
                {
                    tempSkillLevel = dragonData.SLevel;
                    frame_clr = color_frame[dragonData.Grade()];
                }
                else
                    tempSkillLevel = 1;

                effect.effectMode = EffectMode.None;
            }
            else
            {
                tempSkillLevel = GameConfigTable.GetSkillLevelMax();
                effect.effectMode = EffectMode.Grayscale;
            }


            Frame.color = frame_clr;

            var designData = GetDragonDesignData(_dragonTag);
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
            {
                skillButton.SetButtonSpriteState(hasDragon && tempSkillLevel != GameConfigTable.GetSkillLevelMax());
            }


            SetSkillIconImage(skillData.GetIcon());
            SetSkillLevelLabel(hasDragon ? tempSkillLevel : 0);
        }

        void SetSkillIconImage(Sprite icon)
        {
            skillIconFrame.sprite = icon;
        }
        void SetSkillLevelLabel(int slevel)
        {
            if (slevel > 0)
            {
                string levelFormat = string.Format(StringData.GetStringByIndex(100000056), slevel);
                skillIconLevelLabel.text = levelFormat;
            }
            else
            {
                skillIconLevelLabel.text = "";
            }
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
