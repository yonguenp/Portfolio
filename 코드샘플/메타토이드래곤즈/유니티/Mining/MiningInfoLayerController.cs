using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class MiningInfoLayerController : MonoBehaviour, EventListener<MiningPopupEvent>
    {
        [Header("[Mining Info]")]
        [SerializeField] Text miningLevelText = null;
        [SerializeField] Slider miningProgressBar = null;
        [SerializeField] Text miningProgressText = null;

        [SerializeField] Text expectMiningAmountText = null;
        [SerializeField] TimeEnable miningAmountCheckTimeObject = null; // 누적 채굴량 갱신 용도 - state 다시 치는 용도
        [SerializeField] Text remainMiningTimeText = null;

        [Header("[Boost Item Info]")]
        [SerializeField] List<MineBoostStateUISlot> BoostSlotUIList = new List<MineBoostStateUISlot>();

        [Header("Drill Repair")]
        [SerializeField] Button repairButton = null;

        BuildInfo currentMineInfo { get { return MiningManager.Instance.MineBuildingInfo; } }

        int prevMiningValue = 0;//이전 광산 프로그래스 값 (연출용)
        private void OnEnable()
        {
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        //메인UI에서 전체 갱신을 치는건 최대한 지양 - 깡으로 전체 갱신 치지말고 찢어서 세팅으로 변경하기
        public void OnEvent(MiningPopupEvent eventType)
        {
            switch (eventType.Event)
            {
                case MiningPopupEvent.MiningPopupEventEnum.USE_BOOSTER_ITEM:    //부스터 아이템 사용 - 예상 채굴량 & 부스터 상태
                    RefreshBaseText();
                    RefreshBoostState();
                    break;
                case MiningPopupEvent.MiningPopupEventEnum.REQUEST_CLAIM:       //마그넷 수령 완료 - 드릴 프로그레스, 수리 버튼
                    RefreshRepairButton();
                    RefreshProgressBar();
                    RefreshBaseText();
                    break;
                case MiningPopupEvent.MiningPopupEventEnum.REPAIR_DRILL:        //드릴 수리 완료 - 드릴 프로그레스, 수리 버튼
                    RefreshRepairButton();
                    RefreshProgressBar();
                    break;
                case MiningPopupEvent.MiningPopupEventEnum.CONSTRUCT_FINISH:    //건설 완료 - 전체 갱신(광산 레벨, 드릴 내구도, 수리 버튼,)
                    InitController();//일단 메인UI 전체 갱신
                    break;
                case MiningPopupEvent.MiningPopupEventEnum.OPEN_STATUS_PANEL:   //상세보기 패널 오픈 시(mine/state 쏘긴함) 누적 채굴량 갱신
                    InitController();//일단 메인UI 전체 갱신
                    break;
            }
        }

        public void InitController()
        {
            RefreshLayer();
        }
        
        public void RefreshLayer()
        {
            //기본 라벨 (광산 레벨표시, 누적 채굴량 라벨)
            RefreshBaseText();

            // 프로그레스 바 관련
            RefreshProgressBar();

            //부스트(버프) 상태 관리 
            RefreshBoostState();

            //수리 버튼 상태
            RefreshRepairButton();
        }

        /// <summary>
        /// 광산 레벨 & 누적 채굴량 라벨 갱신
        /// </summary>
        void RefreshBaseText()
        {
            miningLevelText.text = StringData.GetStringFormatByStrKey("마그넷광산타이틀", currentMineInfo.Level);
            expectMiningAmountText.text = MiningManager.Instance.UserMiningData.VALUE_DESC;
        }

        void RefreshProgressBar(bool _doProduction = false)
        {
            var curDrillData = MiningManager.Instance.GetUserDrillData();
            miningProgressBar.value = curDrillData == null ? 0 : (float)MiningManager.Instance.UserMiningData.MiningDurability / curDrillData.MINE_DURABILITY;
            miningProgressText.text = curDrillData == null ? "0/0" : $"{MiningManager.Instance.UserMiningData.MiningDurability}/{ curDrillData.MINE_DURABILITY}";


            //if (miningProgressText.text != null)
            //    miningProgressText.DOCounter(prevMiningValue, MiningManager.Instance.UserMiningData.MiningDurability, 1f).SetEase(Ease.InOutQuad);

            prevMiningValue = MiningManager.Instance.UserMiningData.MiningDurability;
        }

        /// <summary>
        /// 현재 가동 중인 부스터 아이템 정보 리스트 - (퍼센트, 플러스, 이벤트 순)
        /// </summary>
        void RefreshBoostState()
        {
            if (BoostSlotUIList == null || BoostSlotUIList.Count <= 0)
                return;

            foreach(var boostSlotUI in BoostSlotUIList)
            {
                if (boostSlotUI == null)
                    continue;
                boostSlotUI.SetDisableState();
            }

            BoostSlotUIList[0].SetBoost(MiningManager.Instance.UserMiningData.percentBoostItem);
            BoostSlotUIList[1].SetBoost(MiningManager.Instance.UserMiningData.plusBoostItem);
            BoostSlotUIList[2].SetBoost(MiningManager.Instance.UserMiningData.eventBoostItem);
        }

        //누적 채굴량 갱신 용도 - 채굴 중일 때 분단위로 mine/state를 보내고, mining 과 wait_claim 상태까지만 누적 채굴량 표시 , 그외에는 0으로 초기화
        void SetExpectMineAmount()
        {
            var state = MiningManager.Instance.UserMiningData.MiningState;
            //if(isMiningState)
            //{
            //    expectMiningAmountText.text = MiningManager.Instance.UserMiningData.MiningTotalAmountValue.ToString("F2");
            //}
            //else
            //    expectMiningAmountText.text = 0.ToString("F2");

            switch(state)
            {
                case eMiningState.MINING:
                    if(miningAmountCheckTimeObject == null)
                    {
                        expectMiningAmountText.text = 0.ToString("F2");
                        return;
                    }

                    miningAmountCheckTimeObject.Refresh = () => { 

                    };

                    break;
                case eMiningState.WAIT_CLAIM:
                    miningAmountCheckTimeObject.Refresh = null;
                    expectMiningAmountText.text = MiningManager.Instance.UserMiningData.VALUE_DESC;
                    break;
                case eMiningState.NONE:
                case eMiningState.UPGRADE:
                case eMiningState.START:
                    miningAmountCheckTimeObject.Refresh = null;
                    expectMiningAmountText.text = 0.ToString("F2");
                    break;
            }
        }

        void RefreshRepairButton()
        {
            if (repairButton != null)
                repairButton.SetButtonSpriteState(IsRepairCondition());
        }
        public void OnClickDrillRepairButton()
        {
            if (!IsRepairCondition(true))
                return;

            PopupManager.OpenPopup<MiningDrillRepairPopup>();
        }

        bool IsRepairCondition(bool _showToast = false)
        {
            var resultButtonState = false;
            var userBuildingData = MiningManager.Instance.MineBuildingInfo;
            var productData = MiningManager.Instance.GetProductData();
            var curDurability = MiningManager.Instance.UserMiningData.MiningDurability;

            if (userBuildingData.Level >= 1)
            {
                if (productData.ProductItem.Amount > curDurability)//현재드릴 내구도 < 회당 채굴량
                    resultButtonState = true;
                else
                {
                    if (_showToast)
                        ToastManager.On(StringData.GetStringByStrKey("광산토스트8"));
                    
                    resultButtonState = false;
                }
            }
            else
            {
                if (_showToast)
                    ToastManager.On(StringData.GetStringByStrKey("광산토스트7"));
            }

            return resultButtonState;
        }

        void ClearData()
        {

        }
    }
}