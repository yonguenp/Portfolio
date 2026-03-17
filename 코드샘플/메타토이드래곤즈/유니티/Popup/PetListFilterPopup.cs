using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetListFilterPopup : Popup<PetFilterData>
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
        GameObject[] statToggleGroupList = null;

        [SerializeField]
        GameObject[] optionToggleGroupList = null;

        [SerializeField]
        protected GameObject[] allSetToggleGroupList = null;

        protected eGradeFilter gradeFilter = eGradeFilter.None;
        protected eElementFilter elementFilter = eElementFilter.None;
        protected ePetStatFilter statFilter = ePetStatFilter.None;
        protected ePetOptionFilter optionFilter = ePetOptionFilter.None;

        protected eJobFilter jobFilter = eJobFilter.None;
        protected eJoinedContentFilter formationFilter = eJoinedContentFilter.None;
        bool isOnlyLockShow = false;
        public delegate void Func(PetFilterData data);

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
            SetLockShowToggle();
            RefreshAllSetToggle();//전체 선택 기능 토글
        }

        void SetCurrentFilter()//처음 팝업 열 때 호출
        {
            if (Data != null)
            {
                gradeFilter = Data.gradeFilter;
                elementFilter = Data.elementFilter;
                statFilter = Data.statFilter;
                optionFilter = Data.optionFilter;

                isOnlyLockShow = Data.isShowOnlyLockState;
            }
        }

        protected void RefreshToggleButtonByFilter()//바깥쪽에서 받아온 필터 값으로 토글 버튼 UI 갱신
        {
            CalcGradeCheck();
            CalcElementCheck();
            CalcStatCheck();
            CalcOptionCheck();
        }

        void CalcGradeCheck()
        {
            if (gradeToggleGroupList == null || gradeToggleGroupList.Length <= 0)
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

        void CalcStatCheck()
        {
            if (statToggleGroupList == null || statToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 스텟 누락");
                return;
            }

            var pets = User.Instance.PetData.GetAllUserPets();
            int[] counts = new int[(int)eDragonListFilterCount.PetStat];
            for (var i = 0; i < pets.Count; ++i)
            {
                var stats = SBFunc.GetPetStatFilterType(pets[i]);
                for (int j = 0; j < (int)eDragonListFilterCount.PetStat; j++)
                {
                    if (stats.HasFlag((ePetStatFilter)(1 << j)))
                    {
                        counts[j]++;
                    }
                }
            }

            for (var i = 0; i < (int)eDragonListFilterCount.PetStat; ++i)
            {
                var checkIndex = -1;
                var pow = (int)Math.Pow(2, i);
                if (((int)statFilter & pow) != 0)
                {
                    checkIndex = i;
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = GetToggleComponent(statToggleGroupList[setIndex]);
                SetToggleButton(toggle, !isOff);

                Text Count = toggle.transform.Find("Count")?.GetComponent<Text>();
                if (Count != null)
                    Count.text = SBFunc.CommaFromNumber(counts[i]);
            }
        }

        void CalcOptionCheck()
        {
            if (optionToggleGroupList == null || optionToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 옵션 누락");
                return;
            }

            var pets = User.Instance.PetData.GetAllUserPets();
            int[] counts = new int[(int)eDragonListFilterCount.PetOption];
            for (var i = 0; i < pets.Count; ++i)
            {
                var stats = SBFunc.GetPetOptionFilterType(pets[i]);
                for (int j = 0; j < (int)eDragonListFilterCount.PetOption; j++)
                {
                    if (stats.HasFlag((ePetOptionFilter)(1 << j)))
                    {
                        counts[j]++;
                    }
                }
            }

            for (var i = 0; i < (int)eDragonListFilterCount.PetOption; ++i)
            {
                var checkIndex = -1;
                var pow = (int)Math.Pow(2, i);
                if (((int)optionFilter & pow) != 0)
                {
                    checkIndex = i;
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = GetToggleComponent(optionToggleGroupList[setIndex]);
                SetToggleButton(toggle, !isOff);

                Text Count = toggle.transform.Find("Count")?.GetComponent<Text>();
                if (Count != null)
                    Count.text = SBFunc.CommaFromNumber(counts[i]);
            }
        }

        void SetLockShowToggle()
        {
            var toggle = GetToggleComponent(allSetToggleGroupList[1]);
            toggle.isOn = isOnlyLockShow;
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
                case "stat":
                {
                    if (statToggleGroupList.Length > 0 && index < statToggleGroupList.Length)
                    {
                        toggleComp = statToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
                case "option":
                {
                    if (optionToggleGroupList.Length > 0 && index < optionToggleGroupList.Length)
                    {
                        toggleComp = optionToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
            }

            return toggleComp;
        }

        public void OnClickToggleButton(string customEventData)
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
                case "stat":
                {
                    statFilter |= (ePetStatFilter)index;
                }
                break;
                case "option":
                {
                    optionFilter |= (ePetOptionFilter)index;
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
                case "stat":
                {
                    statFilter &= ~(ePetStatFilter)index;
                }
                break;
                case "option":
                {
                    optionFilter &= ~(ePetOptionFilter)index;
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

        public virtual void OnClickConfirm()//적용 버튼 누르면
        {
            PopupManager.ClosePopup<PetListFilterPopup>();

            if (applyCallback != null)
            {
                PetFilterData filter = new PetFilterData();
                filter.Init();
                filter.SetFilter(gradeFilter, elementFilter, statFilter, optionFilter, isOnlyLockShow);
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

            var isOnFlag = gradeFilter == eGradeFilter.ALL && elementFilter == eElementFilter.ALL /*&& jobFilter == eJobFilter.ALL && formationFilter == eJoinedContentFilter.ALL*/ && statFilter == ePetStatFilter.ALL && optionFilter == ePetOptionFilter.ALL;//모두가 전체 선택이면 
            allToggle.isOn = isOnFlag;

            allSetToggleGroupList[2].GetComponentInChildren<Toggle>().isOn = gradeFilter == eGradeFilter.ALL;
            allSetToggleGroupList[3].GetComponentInChildren<Toggle>().isOn = elementFilter == eElementFilter.ALL;
            allSetToggleGroupList[4].GetComponentInChildren<Toggle>().isOn = statFilter == ePetStatFilter.ALL;
            allSetToggleGroupList[5].GetComponentInChildren<Toggle>().isOn = optionFilter == ePetOptionFilter.ALL;
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
                statFilter = ePetStatFilter.ALL;
                optionFilter = ePetOptionFilter.ALL;
            }
            else//체크 풀면 - 전체 선택 해제
            {
                gradeFilter = eGradeFilter.None;
                elementFilter = eElementFilter.None;
                jobFilter = eJobFilter.None;
                formationFilter = eJoinedContentFilter.None;
                statFilter = ePetStatFilter.None;
                optionFilter = ePetOptionFilter.None;
            }

            RefreshToggleButtonByFilter();
        }

        public void OnClickLockShowToggle() // 잠금 상태 펫만 보기 or Not
        {
            var lockShowToggle = allSetToggleGroupList[1].GetComponentInChildren<Toggle>();
            if (lockShowToggle == null)
                return;

            lockShowToggle.isOn = !lockShowToggle.isOn;
            isOnlyLockShow = lockShowToggle.isOn;
        }

        public virtual void OnClickAllGradeToggle()//전체 선택 / 해제 기능
        {
            var toggle = allSetToggleGroupList[2].GetComponentInChildren<Toggle>();
            if (toggle == null)
                return;

            toggle.isOn = !toggle.isOn;
            if (toggle.isOn)//체크 누르면 - 전체 선택
            {
                gradeFilter = eGradeFilter.ALL;
            }
            else//체크 풀면 - 전체 선택 해제
            {
                gradeFilter = eGradeFilter.None;
            }

            RefreshToggleButtonByFilter();
        }

        public virtual void OnClickAllElementalToggle()//전체 선택 / 해제 기능
        {
            var toggle = allSetToggleGroupList[3].GetComponentInChildren<Toggle>();
            if (toggle == null)
                return;

            toggle.isOn = !toggle.isOn;
            if (toggle.isOn)//체크 누르면 - 전체 선택
            {
                elementFilter = eElementFilter.ALL;
            }
            else//체크 풀면 - 전체 선택 해제
            {
                elementFilter = eElementFilter.None;
            }

            RefreshToggleButtonByFilter();
        }

        public virtual void OnClickAllStatToggle()//전체 선택 / 해제 기능
        {
            var toggle = allSetToggleGroupList[4].GetComponentInChildren<Toggle>();
            if (toggle == null)
                return;

            toggle.isOn = !toggle.isOn;
            if (toggle.isOn)//체크 누르면 - 전체 선택
            {
                statFilter = ePetStatFilter.ALL;
            }
            else//체크 풀면 - 전체 선택 해제
            {
                statFilter = ePetStatFilter.None;
            }

            RefreshToggleButtonByFilter();
        }

        public virtual void OnClickAllOptionToggle()//전체 선택 / 해제 기능
        {
            var toggle = allSetToggleGroupList[5].GetComponentInChildren<Toggle>();
            if (toggle == null)
                return;

            toggle.isOn = !toggle.isOn;
            if (toggle.isOn)//체크 누르면 - 전체 선택
            {
                optionFilter = ePetOptionFilter.ALL;
            }
            else//체크 풀면 - 전체 선택 해제
            {
                optionFilter = ePetOptionFilter.None;
            }

            RefreshToggleButtonByFilter();
        }
    }
}

