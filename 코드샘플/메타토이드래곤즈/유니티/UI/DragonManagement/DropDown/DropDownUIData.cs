using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [System.Serializable]
    public enum eDropDownType
    {
        DEFAULT,//드래곤 및 장비 펫, 기본 소팅 용도
        ELEMENT,//원소
        CLASS,//탱커, 원딜, 근딜 같은 것이 있나봄(column Name : JOB)
        TEAM_FORMATION,//팀포메이션 용도 (탐험, 요일던전,아레나 공격,아레나 방어), 등
    }
    [System.Serializable]
    public class DropDownUIData 
    {
        public eDropDownType type;
        private int dropdownIndex;
        public int DropdownIndex { get { return dropdownIndex; } set { dropdownIndex = value; } }
        public GameObject dropdownObject;
        public Text dropdownLabel;
        public Button[] buttonList;

        public void SetVisibleDropDown(bool _isVisible)
        {
            if (dropdownObject != null)
                dropdownObject.SetActive(_isVisible);
        }

        public void ChangeDropdownObjectVisible()
        {
            if (dropdownObject != null)
                dropdownObject.gameObject.SetActive(!dropdownObject.activeInHierarchy);
        }
        public void InitDropDownIndex()
        {
            dropdownIndex = 0;
        }
        public void RefreshClickSortFilterLabel()
        {
            switch (type)
            {
                case eDropDownType.DEFAULT:
                    if (dropdownLabel != null)
                        dropdownLabel.text = GetDragonSortLabel();
                    break;
                case eDropDownType.ELEMENT:
                case eDropDownType.CLASS:
                case eDropDownType.TEAM_FORMATION:
                    if (dropdownLabel != null)
                    {
                        switch(type)
                        {
                            case eDropDownType.CLASS:
                                dropdownLabel.text = GetClassFilterLabel();
                                break;
                            case eDropDownType.ELEMENT:
                                RefreshButtonInteraction();
                                dropdownLabel.text = GetElementFilterLabel();
                                break;
                            case eDropDownType.TEAM_FORMATION:
                                RefreshTeamFormationButton();
                                dropdownLabel.text = GetTeamFormationFilterLabel();
                                break;
                        }
                    }
                    break;
            }
        }
        string GetDragonSortLabel()
        {
            string buttonString = "";

            var length = buttonList.Length;
            if (length > dropdownIndex)
            {
                buttonString = buttonList[dropdownIndex].GetComponentInChildren<LocalizeString>()?.GetText();
            }
            return buttonString;
        }

        string GetElementFilterLabel()
        {
            string tempElementSort = "dragon_sorting_05";
            switch (dropdownIndex)
            {
                case 0:
                    tempElementSort = "dragon_sorting_05";
                    break;
                case 1:
                    tempElementSort = "dragon_property_01";
                    break;
                case 2:
                    tempElementSort = "dragon_property_02";
                    break;
                case 3:
                    tempElementSort = "dragon_property_03";
                    break;
                case 4:
                    tempElementSort = "dragon_property_04";
                    break;
                case 5:
                    tempElementSort = "dragon_property_05";
                    break;
                case 6:
                    tempElementSort = "dragon_property_06";
                    break;
            }
            return StringData.GetStringByStrKey(tempElementSort);
        }

        string GetClassFilterLabel()
        {
            string tempClassSort = "dragon_sorting_05";
            switch (dropdownIndex)
            {
                case 0:
                    tempClassSort = "dragon_sorting_05";
                    break;
                case 1:
                    tempClassSort = "탱커";
                    break;
                case 2:
                    tempClassSort = "워리어";
                    break;
                case 3:
                    tempClassSort = "어쌔신";
                    break;
                case 4:
                    tempClassSort = "캐논";
                    break;
                case 5:
                    tempClassSort = "아처";
                    break;
                case 6:
                    tempClassSort = "서포터";
                    break;
            }
            return StringData.GetStringByStrKey(tempClassSort);
        }
        string GetTeamFormationFilterLabel()
        {
            string tempFormationSort = "전체";
            switch (dropdownIndex)
            {
                case 0:
                    tempFormationSort = "전체";
                    break;
                case 1:
                    tempFormationSort = "탐험";
                    break;
                case 2:
                    tempFormationSort = "요일던전";
                    break;
                case 3:
                    tempFormationSort = "아레나공격";
                    break;
                case 4:
                    tempFormationSort = "아레나방어";
                    break;
            }
            return StringData.GetStringByStrKey(tempFormationSort);
        }

        void RefreshButtonInteraction()
        {
            if (buttonList == null || buttonList.Length <= 0)
                return;

            if (dropdownIndex < 0)
                dropdownIndex = 0;

            for (var i = 0; i < buttonList.Length; i++)
            {
                var button = buttonList[i];
                if (button == null)
                    continue;

                button.SetButtonSpriteState(!(dropdownIndex == i));
            }
        }
        /// <summary>
        /// 팀 배치 쪽은 드래곤이 등록이 안되어있으면 기본적으로 터치 안되게 해달라고 요청...
        /// </summary>

        /**
         * 정렬 타입
         * 0 : "전체";
         * 1 : "탐험";
         * 2 : "요일던전";
         * 3 : "아레나 공격";
         * 4 : "아레나 방어";
         */

        void RefreshTeamFormationButton()
        {
            if (buttonList == null || buttonList.Length <= 0)
                return;

            if (dropdownIndex < 0)
                dropdownIndex = 0;

            for (var i = 0; i < buttonList.Length; i++)
            {
                var button = buttonList[i];
                if (button == null)
                    continue;

                var isEmpty = IsFomationEmptyByIndex(i);
                var interactableFlag = dropdownIndex == i || isEmpty;

                button.SetInteractable(!interactableFlag);
            }
        }

        bool IsFomationEmptyByIndex(int _index)
        {
            if(_index <= 0)
                return false;

            List<int> formation = null;
            switch (_index)
            {
                case 1:
                    formation = User.Instance.PrefData.GetAdventureFormation();
                    break;
                case 2://데일리지만 0이아닌 WorldIndex로 읽어와야함.
                    formation = User.Instance.PrefData.GetDailyFormation(0);
                    break;
                case 3:
                    formation = User.Instance.PrefData.GetArenaFomation();
                    break;
                case 4:
                    formation = User.Instance.PrefData.GetArenaFomation(false);
                    break;
            }
            if (formation == null)
                return true;

            return formation.Count == 0;
        }
    }
}
