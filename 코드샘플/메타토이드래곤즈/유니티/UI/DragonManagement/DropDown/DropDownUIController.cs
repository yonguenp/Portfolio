using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DropDownUIController : MonoBehaviour, EventListener<SettingEvent>
    {
        [Header("DropDown")]
        [SerializeField]
        private List<DropDownUIData> dropdownInfoList = new List<DropDownUIData>();//각 eDropDownType 별로 1개씩 있다고 픽스하고 size 3으로 데이터 먼저 세팅
        private void Awake()
        {
            EventManager.AddListener(this);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(this);
        }
        public void InitDropDown()
        {
            foreach (var dropdown in dropdownInfoList)
                dropdown.SetVisibleDropDown(false);
        }
        public void OnClickChangeSort(int sortTypeIndex)
        {
            for (int i = 0; i < dropdownInfoList.Count; i++)
            {
                var dropInfo = dropdownInfoList[i];
                if (i == sortTypeIndex)
                    dropInfo.ChangeDropdownObjectVisible();
                else
                    dropInfo.SetVisibleDropDown(false);
            }
        }
        public void ResetAllDropDown()//모든 드롭다운 플래그 및 라벨 초기화
        {
            foreach (var dropdown in dropdownInfoList)
            {
                dropdown.InitDropDownIndex();
                dropdown.RefreshClickSortFilterLabel();
            }
        }
        public void RefreshAllFilterLabel()
        {
            foreach (var dropdown in dropdownInfoList)
            {
                dropdown.RefreshClickSortFilterLabel();
            }
        }
        public void RefreshSpecificFilterLabel(eDropDownType _type)
        {
            dropdownInfoList[(int)_type].RefreshClickSortFilterLabel();
        }
        public void SetDropDownVisible(eDropDownType _type, bool _isVisible)
        {
            dropdownInfoList[(int)_type].SetVisibleDropDown(_isVisible);
        }
        public void SetDropdownIndex(eDropDownType _type, int _index)
        {
            dropdownInfoList[(int)_type].DropdownIndex = _index;
        }
        public int GetDropdownIndex(eDropDownType _type)
        {
            return dropdownInfoList[(int)_type].DropdownIndex;
        }

        public void OnEvent(SettingEvent eventType)
        {
            switch (eventType.Event)
            {
                case SettingEvent.eSettingEventEnum.REFRESH_STRING:
                    foreach (var dropdown in dropdownInfoList)
                    {
                        dropdown.RefreshClickSortFilterLabel();
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
