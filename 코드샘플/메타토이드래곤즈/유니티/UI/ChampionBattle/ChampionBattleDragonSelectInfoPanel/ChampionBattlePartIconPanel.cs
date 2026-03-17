using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
//드래곤 현재 레벨에 따른 슬롯 열림 / 닫힌 상태 파악 및 해당 레벨에 따른 해금 노티
namespace SandboxNetwork
{
    public class ChampionBattlePartIconPanel : MonoBehaviour
    {
        const int PART_SET_DENOMINATOR = 100;

        int currentDragonLevel = 0;
        int currentDragonTag = 0;
        bool isPartChangeState = false;
        public bool IsPartChangeState { set { isPartChangeState = value; } }

        [SerializeField]
        ChampionBattleDragonSelectTabLayer tablayer = null;

        [SerializeField]
        List<DragonPartIconSlot> slotList = new List<DragonPartIconSlot>();

        [SerializeField]
        GameObject partEaterNode = null;

        [SerializeField]
        Text titleLabel = null;

        public void Init()
        {
            isPartChangeState = false;//장비 교체 상태 초기화
            RefreshCurrentDragonData();
        }
        void RefreshCurrentDragonData()
        {
            if (PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag != 0)//드래곤 태그값
            {
                var dragonTag = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag;
                var userDragonData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
                if (userDragonData == null)//미소유 상태에선 안보여줌.
                    return;

                RefreshDragonEquipSlot(dragonTag, userDragonData.Level);
                RefreshTitleLabel(userDragonData);
            }
        }

        void RefreshDragonEquipSlot(int dragonTag ,int currentLevel)
        {
            var dragonInfo = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            if (dragonInfo == null)
            {
                return;
            }

            var dragonPartInfo = dragonInfo.GetPartsList();//고정 길이 6으로 옴. 내부에서 세팅
            var dragonSetInfo = dragonInfo.GetPartEffectOption(dragonPartInfo).ToList();

            currentDragonLevel = currentLevel;
            currentDragonTag = dragonTag;
            var currentOpenSlotCount = dragonInfo.GetCurrentSlotOpenCount();//현재 오픈된 슬롯 개수
            if (slotList == null || slotList.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < slotList.Count; i++)
            {
                var buttonNode = slotList[i];
                if (buttonNode == null)
                {
                    continue;
                }

                buttonNode.refreshSlot(dragonPartInfo[i] as ChampionPart);
                buttonNode.InitEffect();//모든 이펙트 전부 off
            }

            if(dragonSetInfo != null && dragonSetInfo.Count > 0 && dragonSetInfo[0] != 0)//세트효과 이펙트 - 3셋일때, 6셋일때 추가 이펙트 적용해야함
            {
                //두개 이상 (6세트 및 3세트 두개) 일 때 두개의 그룹이 같은지 선체크
                bool isSixPieceSet = false;

                if(dragonSetInfo.Count > 1)
                {
                    isSixPieceSet = PartSetData.Get(dragonSetInfo[0].ToString()).GROUP == PartSetData.Get(dragonSetInfo[1].ToString()).GROUP;//그룹 키가 같으면 6세트효과
                }
                else
                {
                    isSixPieceSet = PartSetData.Get(dragonSetInfo[0].ToString()).NUM == ePartSetNum.SET_6;
                }

                foreach(var setOptionKey in dragonSetInfo)
                {
                    var setOptionData = PartSetData.Get(setOptionKey.ToString());
                    if (setOptionData == null)
                        continue;

                    var group = setOptionData.GROUP;
                    var count = (int)setOptionData.NUM;//세트효과 적용 갯수
                    var type = setOptionData.STAT_TYPE;

                    int checkIndex = 0;
                    foreach(var slot in slotList)
                    {
                        if (slot == null)
                            continue;

                        if (checkIndex >= count)
                            break;

                        var partTag = slot.SlotPartTag;
                        var partData = dragonInfo.GetPart(partTag);
                        if (partData == null)
                            continue;

                        var statType = partData.GetPartDesignData().STAT_TYPE;
                        var grade = partData.Grade();

                        if (statType == type)
                        {
                            slot.MakeStepGradeOptionEffect(grade);
                            slot.MakeSetOptionEffect(isSixPieceSet);//세트효과 추가 이펙트

                            checkIndex++;
                        }
                    }
                }
            }
        }

        void RefreshTitleLabel(ChampionDragon dragonInfo)
        {
            if (titleLabel != null)
            {
                if (dragonInfo == null)
                {
                    return;
                }
                var dragonDesc = dragonInfo.Name();
                var partTitleLabel = string.Format(StringData.GetStringByIndex(100000339), dragonDesc);
                titleLabel.text = partTitleLabel;
            }
        }

        public void ShowAnimationSlot(int openSlotCount)
        {
            HideAllClickNode();

            if (slotList == null || slotList.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < slotList.Count; i++)
            {
                var buttonNode = slotList[i];
                if (buttonNode == null)
                {
                    continue;
                }

                if (i < openSlotCount)
                    buttonNode.ShowAnimArrowNode();
                else
                    buttonNode.HideAnimArrowNode();
            }
        }

        public void HideAllAnimationSlot()
        {
            HideAllClickNode();

            var dragonInfo = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            if (dragonInfo == null)
            {
                return;
            }
            var openSlotCount = dragonInfo.GetCurrentSlotOpenCount();

            if (slotList == null || slotList.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < slotList.Count; i++)
            {
                var buttonNode = slotList[i];
                if (buttonNode == null)
                    continue;

                if (i < openSlotCount)
                    buttonNode.HideAnimArrowNode();
            }
        }

        void HideAllClickNode()
        {
            if (slotList == null || slotList.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < slotList.Count; i++)
            {
                var buttonNode = slotList[i];
                if (buttonNode == null)
                    continue;

                buttonNode.HideClickNode();
            }
        }

        public void OnClickSlot(int customEventData)
        {
            var clickSlotIndex = customEventData;//현재 클릭한 슬롯 가져옴

            var dragonInfo = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
            if (dragonInfo == null)
                return;

            var miniMizeLevel = CharExpData.GetUnLockLevelByGradeAndSlotIndex(dragonInfo.BaseData.GRADE, clickSlotIndex);

            var slotUnlockChecker = miniMizeLevel <= currentDragonLevel;
            if (slotUnlockChecker)
            {
                MoveEquipPage();
            }
            else
            {
                ToastManager.On(100000328, miniMizeLevel);
            }
        }

        public void OnClickSlotDetailData(int buttonIndex)
        {
            if(slotList == null || slotList.Count <= 0)
                return;

            if (slotList.Count <= buttonIndex)
                return;

            var buttonObject = slotList[buttonIndex];
            if (buttonObject != null)
            {
                var buttonSlotPartTag = buttonObject.SlotPartID;

                if(isPartChangeState)
                {
                    if (buttonSlotPartTag > 0)//장비 이벤트 추가
                        DragonPartEvent.ShowEaterNode(buttonSlotPartTag, partEaterNode);
                }
                else
                {
                    HideAllClickNode();
                    if (buttonSlotPartTag > 0)//현재 교체 요청상태를 체크해야함.
                        DragonPartEvent.RefreshInfoPanel(buttonSlotPartTag, true, buttonObject.gameObject, buttonIndex);
                }
            }
        }

        void MoveEquipPage()
        {
            if (tablayer != null)
                tablayer.moveLayer(4);
        }

        public void SetALLPartIconDimmed(bool _isDimmed)
        {
            for (var i = 0; i < slotList.Count; i++)
            {
                var buttonNode = slotList[i];
                if (buttonNode == null)
                    continue;

                buttonNode.SetIconDimmed(_isDimmed);
            }
        }    
        public GameObject GetPartSlot(int _tag)
        {
            foreach(var slot in slotList)
            {
                if (slot == null)
                    continue;

                if (slot.SlotPartTag == _tag)
                    return slot.gameObject;
            }
            return null;
        }

        public void InitEquipAnimation()
        {
            if (slotList == null || slotList.Count <= 0)
                return;

            foreach (var slot in slotList)
            {
                if (slot == null)
                    continue;

                slot.InitAnimation();
            }
        }

        public void PartEquipAnim(int _tag)
        {
            if (slotList == null || slotList.Count <= 0)
                return;

            foreach(var slot in slotList)
            {
                if (slot == null)
                    continue;
                if(slot.SlotPartTag == _tag)
                {
                    slot.PlayEquipAnim();
                    break;
                }
            }
        }

        public void TryShowClickNode(int _tag)
        {
            foreach (var slot in slotList)
            {
                if (slot == null)
                    continue;
                if (slot.SlotPartTag == _tag)
                {
                    slot.ShowClickNode();
                    break;
                }
            }
        }
    }
}
