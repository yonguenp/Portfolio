using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SimulatorDragonEditStatController : MonoBehaviour
    {
        [SerializeField] private DragonStatPanel statPanel = null;
        [SerializeField] private SBSimulatorDropDown dragonLevelDrop = null;//장비 레벨 드롭
        int dragonTag = -1;

        CharExpTable expTable = null;
        List<UserPart> userPart = new List<UserPart>();
        UserPet userPet = null;

        int tempDragonLevel = 0;
        int tempDragonSLevel = 0;
        public int TempLevel { get { return tempDragonLevel; } }

        public void SetPart(List<UserPart> _userPart)
        {
            if(userPart == null)
            {
                userPart = new List<UserPart>();
            }
            userPart.Clear();

            userPart = _userPart;
        }

        public void SetPet(UserPet _userPet)
        {
            userPet = _userPet;
        }

        public void SetSLevel(int _sLevel)
        {
            tempDragonSLevel = _sLevel;
        }

        void SetStatTag(int tag)
        {
            dragonTag = tag;
        }

        void SetTable()
        {
            if(expTable == null)
            {
                expTable = TableManager.GetTable<CharExpTable>();
            }
        }

        public void init(int tag)
        {
            SetTable();
            SetStatTag(tag);
            SetDragonLevelData();
            SetDragonTempLevel();
            RefreshDragonLevelDropOption();
        }

        public void RefreshDragonStatPanel()
        {
            if (statPanel != null)
            {
                statPanel.CustomRefreshDragonStat(dragonTag,tempDragonLevel, tempDragonSLevel,userPart, userPet);
            }
        }

        void SetDragonTempLevel()
        {
            var dragonData = User.Instance.DragonData.GetDragon(dragonTag);
            if(dragonData != null)
            {
                tempDragonLevel = dragonData.Level;
            }
            else
            {
                tempDragonLevel = 1;
            }
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

            for (var i = 1; i <= maxLevel; i++)
            {
                levelList.Add(i.ToString());
            }

            dragonLevelDrop.AddOptions(levelList);
        }

        void RefreshDragonLevelDropOption()
        {
            if(dragonTag <= 0)
            {
                return;
            }

            var dragonLevel = tempDragonLevel;
            dragonLevelDrop.captionText.text = dragonLevel.ToString();
            dragonLevelDrop.value = dragonLevel - 1;
        }

        public void onClickDragonLevelDropDown()//레벨 선택 시
        {
            var selectedText = dragonLevelDrop.captionText.text;
            var selectLevel = int.Parse(selectedText);

            tempDragonLevel = selectLevel;

            RefreshDragonStatPanel();//스탯 표시 패널 레벨에 따른 갱신
        }
    }
}