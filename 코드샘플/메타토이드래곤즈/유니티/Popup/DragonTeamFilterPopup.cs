using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 팀 배치 필터는 formationFilter가 없음.
    /// </summary>
    public class DragonTeamFilterPopup : DragonListFilterPopup
    {
        public override void onClickConfirm()//적용 버튼 누르면
        {
            PopupManager.ClosePopup<DragonTeamFilterPopup>();

            if (applyCallback != null)
            {
                FilterPopupData filter = new FilterPopupData();
                filter.Init();

                filter.SetFilter(elementFilter, jobFilter, formationFilter);

                applyCallback(filter);
            }
        }

        protected override void RefreshAllSetToggle()
        {
            if (allSetToggleGroupList == null || allSetToggleGroupList.Length <= 0)
                return;

            var allToggle = allSetToggleGroupList[0].GetComponentInChildren<Toggle>();
            if (allToggle == null)
                return;

            var isOnFlag = elementFilter == eElementFilter.ALL && jobFilter == eJobFilter.ALL;//모두가 전체 선택이면 
            allToggle.isOn = isOnFlag;
        }

        public override void OnClickAllSetToggle()//전체 선택 / 해제 기능
        {
            var allToggle = allSetToggleGroupList[0].GetComponentInChildren<Toggle>();
            if (allToggle == null)
                return;

            allToggle.isOn = !allToggle.isOn;
            if (allToggle.isOn)//체크 누르면 - 전체 선택
            {
                elementFilter = eElementFilter.ALL;
                jobFilter = eJobFilter.ALL;
            }
            else//체크 풀면 - 전체 선택 해제
            {
                elementFilter = eElementFilter.None;
                jobFilter = eJobFilter.None;
            }

            RefreshToggleButtonByFilter();
        }
    }
}
