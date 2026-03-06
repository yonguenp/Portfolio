using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PartFilterPopup : Popup<FilterPopupData>
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
        GameObject[] typeToggleGroupList = null;

        [SerializeField]
        GameObject[] levelToggleGroupList = null;

        eGradeFilter gradeFilter  = eGradeFilter.None;
        eTypeFilter typeFilter = eTypeFilter.None;
        eReinforceLevelFilter levelFilter = eReinforceLevelFilter.None;

        public delegate void Func(FilterPopupData data);

        private Func applyCallback;

        public Func ApplyCallback
        {
            get { return applyCallback; }
            set { applyCallback = value; }
        }


        public override void InitUI() //파괴된 아이템 인덱스 및 갯수 넘어옴
        {
            this.SetData(Data);
        }
        void SetData(FilterPopupData data)
        {
            this.SetCurrentFilter();
            this.RefreshToggleButtonByFilter();//플래그 기준으로 체크값 리세팅
        }

        void SetCurrentFilter()//처음 팝업 열 때 호출
        {
            if (this.Data != null)
            {
                this.typeFilter = Data.typeFilter;
                this.levelFilter = Data.levelFilter;
                this.gradeFilter = Data.gradeFilter;
            }
        }

        void RefreshToggleButtonByFilter()//바깥쪽에서 받아온 필터 값으로 토글 버튼 UI 갱신
        {
            this.CalcGradeCheck();
            this.CalcTypeCheck();
            this.CalcLevelCheck();
        }

        void CalcGradeCheck()//각 단계 별로 케이스 돌려서 껏다 켰다를 다시 체크
        {
            if (this.gradeToggleGroupList == null || this.gradeToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 등급 누락");
                return;
            }
            //this.AllClearToggleUI(this.gradeToggleGroupList);//버튼 on/off 상태를 강제로 타기 때문에, 주석

            for (var i = 0; i < (int) eDcomposeCount.Grade; i++)
            {
                var checkIndex = -1;
                var pow = (int) Math.Pow(2, i);
                if (((int) gradeFilter & pow) != 0)
                {
                    switch ((eGradeFilter)pow)
                    {
                        case eGradeFilter.Common:
                        {
                            checkIndex = 0;
                        }
                        break;
                        case eGradeFilter.Uncommon:
                        {
                            checkIndex = 1;
                        }
                        break;
                        case eGradeFilter.Rare:
                        {
                            checkIndex = 2;
                        }
                        break;
                        case eGradeFilter.Unique:
                        {
                            checkIndex = 3;
                        }
                        break;
                        case eGradeFilter.Legendary:
                        {
                            checkIndex = 4;
                        }
                        break;
                    }
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = this.GetToggleComponent(this.gradeToggleGroupList[setIndex]);
                this.SetToggleButton(toggle, !isOff);
            }
        }

        void CalcTypeCheck()//각 단계 별로 케이스 돌려서 껏다 켰다를 다시 체크
        {
            if (this.typeToggleGroupList == null || this.typeToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 타입 누락");
                return;
            }

            //this.AllClearToggleUI(this.typeToggleGroupList);//버튼 on/off 상태를 강제로 타기 때문에, 주석

            for (var i = 0; i < (int) eDcomposeCount.Type; i++)
            {
                var checkIndex = -1;
                var pow = (int) Math.Pow(2, i);
                if ( ((int)this.typeFilter & pow) != 0 )
                {
                    checkIndex = i;
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = this.GetToggleComponent(this.typeToggleGroupList[setIndex]);
                this.SetToggleButton(toggle, !isOff);
            }
        }
        void CalcLevelCheck()//각 단계 별로 케이스 돌려서 껏다 켰다를 다시 체크
        {
            if (this.levelToggleGroupList == null || this.levelToggleGroupList.Length <= 0)
            {
                Debug.Log("토글 리스트 - 레벨 누락");
                return;
            }

            //this.AllClearToggleUI(this.levelToggleGroupList);//버튼 on/off 상태를 강제로 타기 때문에, 주석

            for (var i = 0; i < (int) eDcomposeCount.Level; i++)
            {
                var checkIndex = -1;
                var pow = (int) Math.Pow(2, i);
                if (((int)this.levelFilter & pow) != 0)
                {
                    checkIndex = i;
                }

                var isOff = checkIndex < 0;
                var setIndex = isOff ? i : checkIndex;
                var toggle = this.GetToggleComponent(this.levelToggleGroupList[setIndex]);
                this.SetToggleButton(toggle, !isOff);
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

                var toggleComp = this.GetToggleComponent(node);
                if (toggleComp == null)
                {
                    continue;
                }

                this.SetToggleButton(toggleComp, false);
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
                    if(gradeToggleGroupList.Length > 0 && index < gradeToggleGroupList.Length )
                    {
                        toggleComp = gradeToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
                case "type":
                {
                    if (typeToggleGroupList.Length > 0 && index < typeToggleGroupList.Length)
                    {
                        toggleComp = typeToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
                case "level":
                {
                    if (levelToggleGroupList.Length > 0 && index < levelToggleGroupList.Length)
                    {
                        toggleComp = levelToggleGroupList[index].GetComponentInChildren<Toggle>();
                    }
                }
                break;
            }

            return toggleComp;
        }

        public void onClickToggleButton(string customEventData)
        {
            var dataSet = customEventData.Split("_");
            if(dataSet.Length <= 1)
            {
                return;
            }

            var sortIndex = int.Parse(dataSet[1]);
            var group = dataSet[0];


            Toggle toggleComp = GetToggleComponentByList(group , sortIndex);

            var powIndex = (int)Math.Pow(2, sortIndex);

            var isChecked = toggleComp.isOn;
            // flag 값 바로 적용함
            if (isChecked)//체크 누르면
            {
                this.AddFlag(group, powIndex);
            }
            else//체크 풀면
            {
                this.RemoveFlag(group, powIndex);
            }

            RefreshToggleUI(toggleComp);
        }

        void AddFlag(string group,int index)
        {
            switch (group)
            {
                case "grade":
                {
                    this.gradeFilter |= (eGradeFilter)index;
                }
                break;
                case "type":
                {
                    this.typeFilter |= (eTypeFilter)index;
                }
                break;
                case "level":
                {
                    this.levelFilter |= (eReinforceLevelFilter)index;
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
                    this.gradeFilter &= ~(eGradeFilter)index;
                }
                break;
                case "type":
                {
                    this.typeFilter &= ~(eTypeFilter)index;
                }
                break;
                case "level":
                {
                    this.levelFilter &= ~(eReinforceLevelFilter)index;
                }
                break;
            }
        }

        void SetToggleButton(Toggle toggleTarget, bool isCheck)
        {
            if (toggleTarget != null)
            {
                toggleTarget.isOn = isCheck;

                RefreshToggleUI(toggleTarget);
            }
        }


        void RefreshToggleUI(Toggle toggleTarget)
        {
            var isCheck = toggleTarget.isOn;
            var toggleParentBG = toggleTarget.transform.parent.GetComponent<Image>();
            if (toggleParentBG != null)
                toggleParentBG.color = isCheck ? selectedBgColor : deselectedBgColor;

            toggleTarget.targetGraphic.GetComponent<Image>().color = isCheck ? selectedCheckBoxColor : deselectedCheckBoxColor;
        }

        public void onClickConfirm()//적용 버튼 누르면
        {
            PopupManager.ClosePopup<PartFilterPopup>();

            if(applyCallback != null)
            {
                FilterPopupData filter = new FilterPopupData();
                filter.Init();

                filter.SetFilter(gradeFilter,typeFilter, levelFilter);

                applyCallback(filter);
            }
        }

    }
}

