using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonPartAutoSelectLayer : TabLayer
    {
        [SerializeField]
        Button compoundButton = null;

        [SerializeField]
        Text compoundCountLabel = null;

        [SerializeField]
        List<DragonPartAutoCompoundSlot> buttonSlotList = new List<DragonPartAutoCompoundSlot>();
        List<UserPart> currentPartList = new List<UserPart>();

        public override void InitUI(TabTypePopupData datas = null)//데이터가 있는 갱신
        {
            base.InitUI(datas);

            if (DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT < 0)
                DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT = GameConfigTable.GetPartMergeMaterialMaxCount();

            SetUI();
        }

        public override void RefreshUI()//데이터 유지 갱신
        {

        }

        void SetUI()//버튼 갱신 , 합성시 획득 갯수 라벨 클릭 전 초기화
        {
            InitSlotUI();
            SetCountLabel();
        }
        void SetCountLabel(int _count = -1)
        {
            string count = _count < 0 ? "-" : _count.ToString();
            if (compoundCountLabel != null)
                compoundCountLabel.text = StringData.GetStringFormatByStrKey("합성시획득개수", count);//합성시획득개수 - "합성 시 획득 개수 : {0}"
        }

        void InitSlotUI()//버튼 클릭전 UI 갱신
        {
            if (buttonSlotList == null || buttonSlotList.Count <= 0)
                return;

            currentPartList = User.Instance.PartData.GetAllUserParts();//유저 데이터 미리 세팅

            RefreshAllButtonSlot();
            RefreshCompoundButtonUI();
        }

        void RefreshAllButtonSlot(bool _onlyUI = false)
        {
            for (int i = 0; i < buttonSlotList.Count; i++)
            {
                var buttonData = buttonSlotList[i];
                if (buttonData == null)
                    continue;

                var gradeCount = GetCountGradeList(i + 1);
                if (!_onlyUI)
                {
                    buttonData.SetData(i, gradeCount, DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT);
                }

                buttonData.RefreshUI(false, gradeCount >= DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT);
            }
        }

        public void OnClickSlot(int slotIndex)//클릭 시 버튼 상태 갱신
        {
            //block 상태면 toast 처리
            if (slotIndex < 0 && buttonSlotList == null && buttonSlotList.Count <= 0)
                return;

            if (buttonSlotList.Count <= slotIndex)
                return;

            var buttonData = buttonSlotList[slotIndex];
            var isBlock = buttonData.IsBlock;
            if(isBlock)
            {
                ToastManager.On(StringData.GetStringByStrKey("일괄합성요청오류"));//합성 개수가 부족합니다.
                return;
            }

            var grade = buttonData.Grade;
            var currentCompoundCount = GetCountGradeList(grade);
            SetCountLabel(currentCompoundCount / DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT);//조합 가능 라벨 갱신

            RefreshAllButtonSlot(true);//전체 슬롯 초기화
            buttonData.SelectSlot(true);//클릭 슬롯 갱신
            RefreshCompoundButtonUI();
        }

        public void OnClickAutoCompound()//선택 아무것도 없으면 토스트 처리 (subIndex를 현재 선택된 grade로 넘기기)
        {
            var currentGrade = GetSelectGrade();
            if(currentGrade <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("일괄합성등급선택오류"));
                return;
            }

            var popup = PopupManager.GetPopup<DragonPartAutoCompoundPopup>();
            if (popup != null)
                popup.moveTab(new DragonPartAutoTabTypePopupData(1, currentGrade, GetCountGradeList(currentGrade) / DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT));//subIndex로 현재 선택된 grade를 세팅
        }

        int GetSelectGrade()//현재 선택된 버튼의 grade가져오기
        {
            var currentSelectIndex = -1;
            for (int i = 0; i < buttonSlotList.Count; i++)
            {
                var buttonData = buttonSlotList[i];
                if (buttonData == null)
                    continue;

                if (buttonData.IsBlock)
                    continue;

                if (buttonData.IsSelect)
                    return buttonData.Grade;
            }

            return currentSelectIndex;
        }

        int GetCountGradeList(int grade)//자신 포함 귀속드래곤만 제외
        {
            if (currentPartList == null || currentPartList.Count <= 0)
                return 0;

            var checkList = currentPartList.FindAll((Element) => {
                if (Element == null)
                    return false;

                var isbelonged = (Element.LinkDragonTag > 0);//-1또는 0이면 귀속
                if (isbelonged)
                    return false;

                var isZeroReinforce = (Element.Reinforce <= 0);
                if (!isZeroReinforce)
                    return false;

                var isLocked = User.Instance.Lock.IsLockPart(Element.Tag);//잠겨 있다면
                if (isLocked)
                    return false;

                var isEqualGrade = (Element.Grade() == grade);
                return isEqualGrade;

            }).ToList();

            return checkList.Count;
        }

        void RefreshCompoundButtonUI()
        {
            if (compoundButton != null)
                compoundButton.SetButtonSpriteState(GetSelectGrade() > 0);
        }
    }
}
