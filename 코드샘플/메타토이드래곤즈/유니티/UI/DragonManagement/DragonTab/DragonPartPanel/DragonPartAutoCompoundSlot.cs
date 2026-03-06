using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 장비 일괄 합성 보유갯수 슬롯 데이터 (각 등급별 갯수 및 상태 체크)
 */

namespace SandboxNetwork
{
    public enum AutoCompoundSlotState
    {
        NONE,
        LOCKED,
        OPEN,
        SELECT,
    }

    public class DragonPartAutoCompoundSlot : MonoBehaviour
    {
        

        [SerializeField]
        GameObject selectPanel = null;
        [SerializeField]
        GameObject blockPanel = null;
        [SerializeField]
        Text hasCountLabel = null;
        [SerializeField]
        Text percentLabel = null;//% 붙이기


        AutoCompoundSlotState currentState = AutoCompoundSlotState.NONE;

        int grade = -1;
        public int Grade { get { return grade; } }

        int buttonIndex = -1;

        public int ButtonIndex { get { return buttonIndex; } }

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        bool isSelect = false;
        public bool IsSelect { get { return isSelect; } }

        bool isBlock = false;
        public bool IsBlock { get { return isBlock; } }

        public void SetData(int index , int compoundCount, int checkMaxCount)//RefreshUI 하기전에 먼저 호출
        {
            buttonIndex = index;
            grade = index + 1;

            if (hasCountLabel != null)
            {
                hasCountLabel.text = compoundCount.ToString();
                hasCountLabel.color = compoundCount < checkMaxCount ? Color.red : Color.white;
            }
            if (percentLabel != null)
                percentLabel.text = GetMergeBaseSuccessPercent() + "%";
        }

        public void RefreshUI(bool _isSelect, bool _isOpen)
        {
            SetState(_isSelect, _isOpen);
        }

        public void SelectSlot(bool _isSelect)
        {
            SetState(_isSelect, true);
        }

        void SetState(bool _isSelect, bool _isOpen)
        {
            if(_isOpen)
                currentState = AutoCompoundSlotState.OPEN;
            else
                currentState = AutoCompoundSlotState.LOCKED;

            if (_isSelect)
                currentState = AutoCompoundSlotState.SELECT;

            SetObjVisible(_isSelect, _isOpen);
        }

        void SetObjVisible(bool _isSelect, bool _isOpen)
        {
            SetOpenedPanel(_isOpen);
            SetSelectedPanel(_isSelect);
        }

        void SetOpenedPanel(bool _isOpened)//block의 반대
        {
            if (selectPanel != null)
                selectPanel.SetActive(false);//선택 노드 일단 끄고

            isOpen = _isOpened;
            SetBlockPanel(!_isOpened);
        }

        void SetSelectedPanel(bool _isSelected)
        {
            if (selectPanel != null)
                selectPanel.SetActive(_isSelected);

            isSelect = _isSelected;
        }

        void SetBlockPanel(bool _isBlock)
        {
            if (blockPanel != null)
                blockPanel.SetActive(_isBlock);

            isBlock = _isBlock;
        }

        float GetMergeBaseSuccessPercent()//현재 등급 베이스 확률
        {
            var partBaseData = GetMergeBaseData();
            float success_rate = 0;
            if (partBaseData == null)
            {
                return success_rate;
            }

            success_rate = partBaseData.RATE;
            var baseNum = partBaseData.BASE_NUM;//기본 갯수
            var addAmountBonus = PartMergeEquipAmountBonusData.GetRateByGradeAndBonusAmountNum(grade,DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT - baseNum);

            if (addAmountBonus > 0)
                success_rate += addAmountBonus;

            return (float)Math.Round((success_rate / (float)SBDefine.MILLION * 100), 2);
        }
        PartMergeBaseData GetMergeBaseData()
        {
            var mergeDefaultData = PartMergeBaseData.GetDataByGrade(grade);
            if (mergeDefaultData == null || mergeDefaultData.Count <= 0)
            {
                return null;
            }
            return mergeDefaultData[0];
        }
    }
}
