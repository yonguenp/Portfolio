using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonListFilterPopup : Popup<FilterPopupData>
    {
        [SerializeField]
        Color selectedBgColor = new Color();
        [SerializeField]
        Color deselectedBgColor = new Color();
        [SerializeField]
        Color selectedCheckBoxColor = new Color();
        [SerializeField]
        Color deselectedCheckBoxColor = new Color();

        [SerializeField]
        GameObject[] gradeToggleGroupList = null;

        [SerializeField]
        GameObject[] elementToggleGroupList = null;

        [SerializeField]
        GameObject[] jobToggleGroupList = null;

        [SerializeField]
        GameObject[] formationToggleGroupList = null;


        [SerializeField]
        protected GameObject[] allSetToggleGroupList = null;

        protected eGradeFilter gradeFilter = eGradeFilter.None;
        protected eElementFilter elementFilter = eElementFilter.None;
        protected eJobFilter jobFilter = eJobFilter.None;
        protected eJoinedContentFilter formationFilter = eJoinedContentFilter.None;

        public delegate void Func(FilterPopupData data);

        protected Func applyCallback;

        


        public Func ApplyCallback
        {
            get { return applyCallback; }
            set { applyCallback = value; }
        }


        public override void InitUI()
        {
            if (Data == null)
                return;

            SetData();
        }
        void SetData()
        {
            SetCurrentFilter();
            RefreshToggleButtonByFilter();//플래그 기준으로 체크값 리세팅

            RefreshAllSetToggle();//전체 선택 기능 토글
        }

        void SetCurrentFilter()//처음 팝업 열 때 호출
        {
            if (Data != null)
            {
                gradeFilter = Data.gradeFilter;
                jobFilter = Data.jobFilter;
                formationFilter = Data.formationFilter;
                elementFilter = Data.elementFilter;
            }
        }

        protected void RefreshToggleButtonByFilter()//바깥쪽에서 받아온 필터 값으로 토글 버튼 UI 갱신
        {
            CalcGradeCheck();
            CalcElementCheck();
            CalcJobCheck();
            CalcFormationCheck();
        }

        void CalcGradeCheck()
        {
            if(gradeToggleGroupList ==null || gradeToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 등급 누락");
                return;
            }
            for (var i = 0; i < (int)eDragonListFilterCount.Grade; ++i)
            {
                var checkIndex = -1;
                var pow = (int)Math.Pow(2, i);
                if (((int)gradeFilter & pow) != 0)
                {
                    checkIndex = i;
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = GetToggleComponent(gradeToggleGroupList[setIndex]);
                SetToggleButton(toggle, !isOff);
            }
        }

        void CalcElementCheck()//각 단계 별로 케이스 돌려서 껏다 켰다를 다시 체크
        {
            if (elementToggleGroupList == null || elementToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 속성 누락");
                return;
            }
            //AllClearToggleUI(gradeToggleGroupList);//버튼 on/off 상태를 강제로 타기 때문에, 주석

            for (var i = 0; i < (int)eDragonListFilterCount.Element; i++)
            {
                var checkIndex = -1;
                var pow = (int)Math.Pow(2, i);
                if (((int)elementFilter & pow) != 0)
                {
                    checkIndex = i;
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = GetToggleComponent(elementToggleGroupList[setIndex]);
                SetToggleButton(toggle, !isOff);
            }
        }

        void CalcJobCheck()//각 단계 별로 케이스 돌려서 껏다 켰다를 다시 체크
        {
            if (jobToggleGroupList == null || jobToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 클래스 누락");
                return;
            }

            //AllClearToggleUI(typeToggleGroupList);//버튼 on/off 상태를 강제로 타기 때문에, 주석

            for (var i = 0; i < (int)eDragonListFilterCount.Job; i++)
            {
                var checkIndex = -1;
                var pow = (int)Math.Pow(2, i);
                if (((int)jobFilter & pow) != 0)
                {
                    checkIndex = i;
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = GetToggleComponent(jobToggleGroupList[setIndex]);
                SetToggleButton(toggle, !isOff);
            }
        }
        void CalcFormationCheck()//각 단계 별로 케이스 돌려서 껏다 켰다를 다시 체크
        {
            if (formationToggleGroupList == null || formationToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 팀포메이션 누락");
                return;
            }

            //AllClearToggleUI(levelToggleGroupList);//버튼 on/off 상태를 강제로 타기 때문에, 주석

            for (var i = 0; i < (int)eDragonListFilterCount.TeamFormation; i++)
            {
                var checkIndex = -1;
                var pow = (int)Math.Pow(2, i);
                if (((int)formationFilter & pow) != 0)
                {
                    checkIndex = i;
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = GetToggleComponent(formationToggleGroupList[setIndex]);
                SetToggleButton(toggle, !isOff);
            }
        }

        void AllClearToggleUI(GameObject[] nodeList)
        {
            for (var i = 0; i < nodeList.Length; i++)
            {
                var node = nodeList[i];
                if (node == null)
                {
                    continue;
                }

                var toggleComp = GetToggleComponent(node);
                if (toggleComp == null)
                {
                    continue;
                }

                SetToggleButton(toggleComp, false);
            }
        }

        Toggle GetToggleComponent(GameObject targetNode)
        {
            Toggle toggleComp = targetNode.GetComponentInChildren<Toggle>();
            return toggleComp;
        }

        Toggle GetToggleComponentByList(string group, int index)
        {
            Toggle toggleComp = null;
            switch (group)
            {
                case "grade":
                {
                    if (gradeToggleGroupList.Length > 0 && index < gradeToggleGroupList.Length)
                    {
                        toggleComp = gradeToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
                case "element":
                {
                    if (elementToggleGroupList.Length > 0 && index < elementToggleGroupList.Length)
                    {
                        toggleComp = elementToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
                case "job":
                {
                    if (jobToggleGroupList.Length > 0 && index < jobToggleGroupList.Length)
                    {
                        toggleComp = jobToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
                case "formation":
                {
                    if (formationToggleGroupList.Length > 0 && index < formationToggleGroupList.Length)
                    {
                        toggleComp = formationToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
            }

            return toggleComp;
        }

        public void onClickToggleButton(string customEventData)
        {
            var dataSet = customEventData.Split("_");
            if (dataSet.Length <= 1)
            {
                return;
            }

            var sortIndex = int.Parse(dataSet[1]);
            var group = dataSet[0];


            Toggle toggleComp = GetToggleComponentByList(group, sortIndex);

            var powIndex = (int)Math.Pow(2, sortIndex);

            var isChecked = toggleComp.isOn;
            // flag 값 바로 적용함
            if (isChecked)//체크 누르면
            {
                AddFlag(group, powIndex);
            }
            else//체크 풀면
            {
                RemoveFlag(group, powIndex);
            }

            RefreshToggleUI(toggleComp);

            RefreshAllSetToggle();
        }

        void AddFlag(string group, int index)
        {
            switch (group)
            {
                case "grade":
                {
                    gradeFilter |= (eGradeFilter)index;
                }
                break;
                case "element":
                {
                    elementFilter |= (eElementFilter)index;
                }
                break;
                case "job":
                {
                    jobFilter |= (eJobFilter)index;
                }
                break;
                case "formation":
                {
                    formationFilter |= (eJoinedContentFilter)index;
                }
                break;
            }
        }

        void RemoveFlag(string group, int index)
        {
            switch (group)
            {
                case "grade":
                {
                    gradeFilter &= ~(eGradeFilter)index;
                }
                break;
                case "element":
                {
                    elementFilter &= ~(eElementFilter)index;
                }
                break;
                case "job":
                {
                    jobFilter &= ~(eJobFilter)index;
                }
                break;
                case "formation":
                {
                    formationFilter &= ~(eJoinedContentFilter)index;
                }
                break;
            }
        }

        protected void SetToggleButton(Toggle toggleTarget, bool isCheck)
        {
            if (toggleTarget != null)
            {
                toggleTarget.isOn = isCheck;

                RefreshToggleUI(toggleTarget);
            }
        }


        protected void RefreshToggleUI(Toggle toggleTarget)
        {
            var isCheck = toggleTarget.isOn;
            var toggleParentBG = toggleTarget.transform.parent.GetComponent<Image>();
            if (toggleParentBG != null)
                toggleParentBG.color = isCheck ? selectedBgColor : deselectedBgColor;

            toggleTarget.targetGraphic.GetComponent<Image>().color = isCheck ? selectedCheckBoxColor : deselectedCheckBoxColor;
        }

        public virtual void onClickConfirm()//적용 버튼 누르면
        {
            PopupManager.ClosePopup<DragonListFilterPopup>();

            if (applyCallback != null)
            {
                FilterPopupData filter = new FilterPopupData();
                filter.Init();

                filter.SetFilter(gradeFilter,elementFilter, jobFilter, formationFilter);

                applyCallback(filter);
            }
        }

        protected virtual void RefreshAllSetToggle()
        {
            if (allSetToggleGroupList == null || allSetToggleGroupList.Length <= 0)
                return;

            var allToggle = allSetToggleGroupList[0].GetComponentInChildren<Toggle>();
            if (allToggle == null)
                return;

            var isOnFlag = gradeFilter == eGradeFilter.ALL && elementFilter == eElementFilter.ALL && jobFilter == eJobFilter.ALL && formationFilter == eJoinedContentFilter.ALL;//모두가 전체 선택이면 
            allToggle.isOn = isOnFlag;
        }

        public virtual void OnClickAllSetToggle()//전체 선택 / 해제 기능
        {
            var allToggle = allSetToggleGroupList[0].GetComponentInChildren<Toggle>();
            if (allToggle == null)
                return;

            allToggle.isOn = !allToggle.isOn;
            if (allToggle.isOn)//체크 누르면 - 전체 선택
            {
                gradeFilter = eGradeFilter.ALL;
                elementFilter = eElementFilter.ALL;
                jobFilter = eJobFilter.ALL;
                formationFilter = eJoinedContentFilter.ALL;
            }
            else//체크 풀면 - 전체 선택 해제
            {
                gradeFilter = eGradeFilter.None;
                elementFilter = eElementFilter.None;
                jobFilter = eJobFilter.None;
                formationFilter = eJoinedContentFilter.None;
            }

            RefreshToggleButtonByFilter();
        }
    }
}

