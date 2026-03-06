using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorDragonEditSkillController : MonoBehaviour
    {
        [SerializeField] private SBSimulatorDropDown dragonLevelDrop = null;//장비 레벨 드롭
        [SerializeField] private Image iconImage = null;
        [SerializeField] private Sprite emptySprite = null;

        int dragonTag = -1;

        int tempDragonSLevel = 0;

        public int TempSLevel { get { return tempDragonSLevel; } }

        void SetStatTag(int tag)
        {
            dragonTag = tag;
        }

        public void init(int tag)
        {
            SetStatTag(tag);
            SetDragonLevelData();
            SetDragonTempSLevel();
            setSkillIcon();
            RefreshDragonLevelDropOption();
        }

        void setSkillIcon()
        {
            var skillData = GetDragonSkillData();
            if (skillData == null)
            {
                setSkillIconImage("");
                return;
            }

            iconImage.sprite = skillData.GetIcon();
        }
        void setSkillIconImage(string skillIcon)
        {
            Sprite skillImage = skillIcon == "" ? emptySprite : ResourceManager.GetResource<Sprite>(eResourcePath.SkillIconPath, skillIcon);

            iconImage.sprite = skillImage;
        }

        SkillCharData GetDragonSkillData()
        {
            var skillData = CharBaseData.Get(dragonTag).SKILL1;
            
            return skillData;
        }

        void SetDragonLevelData()
        {
            if (dragonLevelDrop == null)
            {
                return;
            }

            dragonLevelDrop.ClearOptions();


            var maxLevel = GameConfigTable.GetDragonLevelMax();
            List<string> levelList = new List<string>();

            levelList.Add("none");
            for (var i = 1; i <= maxLevel; i++)
            {
                levelList.Add(i.ToString());
            }

            dragonLevelDrop.AddOptions(levelList);
        }
        void SetDragonTempSLevel()
        {
            var dragonData = User.Instance.DragonData.GetDragon(dragonTag);
            if (dragonData != null)
            {
                tempDragonSLevel = dragonData.SLevel;
            }
            else
            {
                tempDragonSLevel = 1;
            }
        }

        void RefreshDragonLevelDropOption()
        {
            if (dragonTag <= 0)
            {
                return;
            }

            var skillData = GetDragonSkillData();
            string dropText;
            int dropValue;
            if (skillData != null)
            {
                dropText = tempDragonSLevel.ToString();
                dropValue = tempDragonSLevel;
            }
            else
            {
                dropText = "none";
                dropValue = 0;
            }
            dragonLevelDrop.captionText.text = dropText;
            dragonLevelDrop.value = dropValue;
        }

        public void onClickDragonLevelDropDown()//레벨 선택 시
        {
            var skillData = GetDragonSkillData();
            if(skillData == null)
            {
                RefreshDragonLevelDropOption();
                ToastManager.On("스킬이 없는 드래곤입니다");
                return;
            }

            var selectedText = dragonLevelDrop.captionText.text;
            int selectLevel;
            if (selectedText == "none")
            {
                ToastManager.On("스킬이있는 드래곤이라 none 안됨");
                selectLevel = 1;
            }
            else
            {
                selectLevel = int.Parse(selectedText);
            }

            var dragonData = User.Instance.DragonData.GetDragon(dragonTag);
            if (dragonData == null)
            {
                return;
            }

            tempDragonSLevel = selectLevel;

            RefreshDragonLevelDropOption();

            //팝업 ForceUpdate 쳐서 능력치 갱신 추가
            PopupManager.ForceUpdate<SimulatorDragonEditPopup>();
        }
    }
}

#endif