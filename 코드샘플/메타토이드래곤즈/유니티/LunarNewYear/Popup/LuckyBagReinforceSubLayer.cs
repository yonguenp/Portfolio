using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 복주머니 강화 UI
    /// </summary>
    public class LuckyBagReinforceSubLayer : SubLayer
    {
        int MAX_LUCKY_BAG_COUNT { get {
                return Convert.ToInt32(GameConfigData.Get("POCKET_SLOT_NUM").VALUE);
            } }

        int MAX_REINFORCE_BAG_STEP
        {
            get
            {
                return Convert.ToInt32(GameConfigData.Get("POCKET_REINFORCE_MAX_LEVEL").VALUE);
            }
        }

        int CUR_MAX_INPUT_COUNT
        {
            get {
                if (eventData == null)
                    return 0;

                var currentItemCount = eventData.GetEventItem().Amount;//현재 복주머니 수량
                return currentItemCount >= MAX_LUCKY_BAG_COUNT ? MAX_LUCKY_BAG_COUNT : currentItemCount;
            }
        }

        [Header("Left Layout")]
        [SerializeField] List<LuckyBagReinforceSlot> slotList = new List<LuckyBagReinforceSlot>();//UI용 슬롯
        [SerializeField] Text reinforceStepText = null;
        [SerializeField] Text successRateText = null;
        [SerializeField] Button getRewardButton = null;
        [SerializeField] Button reinforceButton = null;

        [SerializeField] GameObject rateNode = null;//10강 도달시 꺼야함.
        [SerializeField] GameObject reinforceCheckNode = null;//inputNode 끄면 켤 노드
        [SerializeField] Text inputText = null;//10강 도달시 text 바꿔야함.
        [SerializeField] GameObject inputNode = null;
        [SerializeField] Button leftButton = null;//0강 일때 갯수 조절 -
        [SerializeField] Button rightButton = null;//0강 일때 갯수 조절 +
        [SerializeField] Text countSelectText = null;

        [SerializeField] Button maxButton = null;//(임시) max 버튼

        [Space(10)]
        [Header("Right Layout")]
        [SerializeField] ItemFrame currentBagItem = null;
        [SerializeField] Button buyButton = null;
        [SerializeField] TableView tableView = null;
        [SerializeField] Text bagPoint = null;
        [SerializeField] Text emptyLabelText = null;


        EventLuckyBagBaseData eventData = null;

        bool isInitTable = false;

        int currentSelectCount = 0;
        public override void Init()
        {
            if (eventData == null)
                eventData = LuckyBagEventPopup.GetEventData();

            if(tableView != null && !isInitTable)
            {
                tableView.OnStart();
                isInitTable = true;
            }

            currentSelectCount = 0;

            SetSlot();
            SetDefaultUI();
        }
        /// <summary>
        /// 슬롯 상태 체크
        /// </summary>
        void SetSlot()//
        {
            if (eventData == null)
                return;

            var reinforceStep = eventData.ReinforceStep;//현재 강화단계
            var remainCount = eventData.RemainCount;//잔여수량
            var uiMaxSlotCount = slotList.Count;

            if (reinforceStep == 0)//슬롯 초기 세팅
            {
                RefreshSlotUI();//초기 세팅
            }
            else//서버에서 준 데이터를 기준으로 세팅
            {
                for (int i = 0; i < uiMaxSlotCount; i++)
                {
                    if (i < remainCount)
                        slotList[i].SetBag(reinforceStep, false, true);
                    else
                        slotList[i].SetBag(reinforceStep, true, true);
                }
            }
        }
        //강화에 대한 연출 
        void SetProductionSlot(int _failCount)
        {
            if (eventData == null)
                return;

            var reinforceStep = eventData.ReinforceStep;//현재 강화단계
            var remainCount = eventData.RemainCount;//잔여수량
            var uiMaxSlotCount = slotList.Count;
            var failIndex = remainCount + _failCount;

            for (int i = 0; i < uiMaxSlotCount; i++)
            {
                if (i < remainCount)
                    slotList[i].StartProduction(reinforceStep, true);
                else if(i >= remainCount && i < failIndex)
                    slotList[i].StartProduction(reinforceStep, false);
                else
                    slotList[i].SetBag(reinforceStep, true);
            }
        }

        void SetDefaultUI()
        {
            SetReinforceStep();
            SetRinforceRate();
            SetEventItem();
            SetPointText();
            SetRewardTable();
            RefreshButton();
            RefreshCountButton();
            RefreshInputNode();
        }
        /// <summary>
        /// 강화 단계 표시
        /// </summary>
        void SetReinforceStep()
        {
            if (eventData == null)
                return;

            if (reinforceStepText != null)
                reinforceStepText.text = StringData.GetStringFormatByStrKey("업그레이드단계", eventData.ReinforceStep);
        }
        /// <summary>
        /// 강화 성공 확률 표시
        /// </summary>
        void SetRinforceRate()
        {
            if (eventData == null)
                return;

            var scheduleDataKey = eventData.GetScheduleDataKey();
            var diceData = DiceBoardData.GetBoards(scheduleDataKey);
            if (diceData == null)
                return;

            var curStep = eventData.ReinforceStep;
            var isMaxStep = eventData.ReinforceStep == MAX_REINFORCE_BAG_STEP;
            var nextRate = diceData.Find(element => element.BOARD_ID == curStep);

            var resultStr = "--%";
            if(nextRate != null && curStep < MAX_REINFORCE_BAG_STEP)
            {
                var successPercent = (float)(nextRate.RATE / (float)SBDefine.MILLION * 100);
                resultStr = SBFunc.StrBuilder(Math.Round(successPercent, 2), "%");
            }

            if (rateNode != null)
                rateNode.SetActive(!isMaxStep);

            if (successRateText != null)//단계퍼센트
                successRateText.text = resultStr;
        }

        void SetEventItem()
        {
            if (eventData == null)
                return;

            var curItem = eventData.GetEventItem();
            if (curItem == null)
                return;

            currentBagItem.SetFrameItemInfo(curItem.ItemNo, curItem.Amount,-1);
        }

        void SetPointText()
        {
            if (eventData == null)
                return;

            if (bagPoint != null)
                bagPoint.text = eventData.LuckyBagPoint.ToString();
        }

        /// <summary>
        /// 각 강화 단계별 보상 세팅
        /// </summary>
        void SetRewardTable()
        {
            if (eventData == null)
                return;

            var scheduleDataKey = eventData.GetScheduleDataKey();
            var diceData = DiceBoardData.GetBoards(scheduleDataKey);
            if (diceData == null)
                return;

            var curStep = eventData.ReinforceStep;
            var diceBoardData = diceData.Find(element => element.BOARD_ID == curStep);
            var rewardGroupID = 0;
            if (diceBoardData != null)
                rewardGroupID = diceBoardData.REWARD_ID;


            List<ITableData> rewardList = new List<ITableData>();
            List<ItemGroupData> tableList = ItemGroupData.Get(rewardGroupID);

            if(tableList != null && tableList.Count > 0)
            {
                foreach (var itemGroupData in tableList)
                {
                    rewardList.Add(itemGroupData);
                }
            }

            if (emptyLabelText != null)
                emptyLabelText.gameObject.SetActive(rewardList.Count <= 0);

            tableView.SetDelegate(new TableViewDelegate(rewardList, (GameObject node, ITableData item) =>
            {
                if (node == null)
                    return;

                var frame = node.GetComponent<LuckyBagReinforceRewardSlot>();
                if (frame == null)
                    return;

                var itemData = (ItemGroupData)item;
                frame.SetData(itemData);
            }));

            tableView.ReLoad();
        }
        /// <summary>
        /// 일단 중단 / 강화 버튼 / -버튼 + 버튼   
        /// </summary>
        void RefreshButton()
        {
            if (eventData == null)
                return;

            var reinforceStep = eventData.ReinforceStep;
            var isFirstStepCondition = reinforceStep == 0 && currentSelectCount > 0;// 0강일 때 보상받기 컨디션
            var reinforceContinue = reinforceStep > 0;//강화 시도 중
            var isMaxStep = MAX_REINFORCE_BAG_STEP == reinforceStep;

            if (getRewardButton != null)
                getRewardButton.SetButtonSpriteState(isFirstStepCondition || reinforceContinue);

            if (reinforceButton != null)
            {
                if(eventData.ReinforceStep == 0)
                    reinforceButton.SetButtonSpriteState(currentSelectCount > 0);
                else
                    reinforceButton.SetButtonSpriteState(!isMaxStep);
            }
        }

        void RefreshCountButton()//0강일 때는 계산 때리고, 강화하는 중이면 강화 막기
        {
            if (maxButton != null)
                maxButton.gameObject.SetActive(false);

            if (eventData == null)
                return;

            var currentStep = eventData.ReinforceStep;
            if(currentStep > 0)//강화 돌리는 중 - 버튼 입력 끄기
            {
                if (leftButton != null)
                    leftButton.SetButtonSpriteState(false);
                if (rightButton != null)
                    rightButton.SetButtonSpriteState(false);

#if DEBUG || UNITY_EDITOR
                if (maxButton != null)
                {
                    maxButton.gameObject.SetActive(true);
                    maxButton.SetButtonSpriteState(false);
                }
#endif
            }
            else//초기상태 - 서버쪽에서 InputCount 이걸 0으로 주긴해야됨.
            {
                if (leftButton != null)
                    leftButton.SetButtonSpriteState(currentSelectCount > 0);
                if(rightButton != null)
                    rightButton.SetButtonSpriteState(currentSelectCount < CUR_MAX_INPUT_COUNT);

#if DEBUG || UNITY_EDITOR
                if (maxButton != null)
                {
                    maxButton.gameObject.SetActive(true);
                    maxButton.SetButtonSpriteState(currentSelectCount < CUR_MAX_INPUT_COUNT);

                }
#endif
            }

            if (countSelectText != null)
                countSelectText.text = string.Format("x{0}", currentSelectCount);
        }
        
        void RefreshInputNode()
        {
            if (eventData == null)
                return;

            var isFirstReinforce = eventData.ReinforceStep <= 0;

            if (inputNode != null)
                inputNode.SetActive(isFirstReinforce);
            if (reinforceCheckNode != null)
                reinforceCheckNode.SetActive(!isFirstReinforce);

            if(!isFirstReinforce && inputText != null)
                inputText.text = eventData.ReinforceStep == MAX_REINFORCE_BAG_STEP ? StringData.GetStringByStrKey("최고단계달성") : StringData.GetStringByStrKey("강화도전중");
        }

        void RefreshSlotUI()//-+ 버튼 연동 로직 - 사실 강화 0 단계에서만 돌릴 용도
        {
            var uiMaxSlotCount = slotList.Count;
            for (int i = 0; i < uiMaxSlotCount; i++)
            {
                if (i < currentSelectCount)
                    slotList[i].SetBag(0, false, true);
                else
                    slotList[i].SetBag(0, true, true);
            }
        }
        public void OnClickCountButton(bool _isPlus)
        {
            if (eventData == null)
                return;

            var currentStep = eventData.ReinforceStep;
            if(currentStep > 0)//현재 강화 진행중
            {
                ToastManager.On(StringData.GetStringByStrKey("복주머니수량불가문구"));
                return;
            }

            if (_isPlus)//오른쪽
            {
                var tempCheck = currentSelectCount + 1;
                if (tempCheck >= CUR_MAX_INPUT_COUNT)
                    currentSelectCount = CUR_MAX_INPUT_COUNT;
                else
                    currentSelectCount += 1;
            }
            else
            {
                var tempCheck = currentSelectCount - 1;
                if (tempCheck <= 0)
                    currentSelectCount = 0;
                else
                    currentSelectCount -= 1;
            }

            RefreshButton();
            RefreshCountButton();
            RefreshSlotUI();
        }

        public void OnClickMaxButton()
        {
            currentSelectCount = CUR_MAX_INPUT_COUNT;

            RefreshButton();
            RefreshCountButton();
            RefreshSlotUI();
        }

        public void OnClickGetReward()
        {
            if (eventData == null)
                return;

            //강화 0단계에서 선택한 슬롯 갯수 없으면 불가
            if(eventData.ReinforceStep == 0 && currentSelectCount <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("보상불가알림문구"));
                return;
            }

            var isMaxStep = eventData.ReinforceStep == MAX_REINFORCE_BAG_STEP;
            if(!isMaxStep)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("복주머니중단문구"), () =>
                {
                    RequestRewardToServer();
                },() => {},() => {}
                );
            }
            else
            {
                RequestRewardToServer();
            }
        }

        void RequestRewardToServer()
        {
            var requestCount = eventData.ReinforceStep == 0 && currentSelectCount > 0 ? currentSelectCount : 1;
            eventData.RequestToServer(eLuckyBagEventState.REQUEST_GET_REWARD, (jsonData) =>
            {
                List<Asset> rewardList = new List<Asset>();

                if (jsonData.ContainsKey("reward"))//보상
                    rewardList = SBFunc.ConvertSystemRewardDataList((JArray)jsonData["reward"]);

                if (rewardList != null && rewardList.Count > 0)//보상팝업 호출
                    SystemRewardPopup.OpenPopup(rewardList);

                //슬롯 초기화 -> (연출 없어도 될지 논의)
                Init();//기준 데이터로 다시 그리기
            }, (failString) => {
                //서버 응답 실패 시
                ToastManager.On(StringData.GetStringByStrKey("서버요청실패"));
            }, requestCount);
        }
        public void OnClickReinforce()
        {
            var isMaxStep = eventData.ReinforceStep == MAX_REINFORCE_BAG_STEP;
            if (isMaxStep)
            {
                ToastManager.On(StringData.GetStringByStrKey("강화불가알림문구"));
                return;
            }

            if(currentSelectCount <= 0 && eventData.ReinforceStep <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("복주머니선택알림문구"));
                return;
            }

            eventData.RequestToServer(eLuckyBagEventState.REQUEST_BAG_REINFORCE,(jsonData) => 
            {
                var failCount = 0;
                if(jsonData.ContainsKey("fail"))
                    failCount = jsonData["fail"].Value<int>();

                if (currentSelectCount > 0 && failCount <= 0)//최초 강화 했을 때 다 터질 경우 - 강화 중이면 currentSelectCount 무조건 0 (해당 값 제어 노드를 끔)
                {
                    failCount = currentSelectCount;
                    currentSelectCount = 0;
                }

                SetProductionSlot(failCount);//슬롯연출
                SetDefaultUI();//UI만 갱신
            }, (failString) => 
            {
                //서버 응답 실패 시
                ToastManager.On(StringData.GetStringByStrKey("서버요청실패"));
            }, currentSelectCount);
        }
        /// <summary>
        /// 구매 버튼 -> sublayer 1(복주머니 상점)로 이동
        /// </summary>
        public void OnClickBuyLuckyBag()
        {
            if (eventData == null)
                return;

            bool isPeriod = eventData.IsEventPeriod(false);
            if (!isPeriod)
            {
                ToastManager.On(StringData.GetStringByStrKey("이벤트종료안내"));
                return;
            }

            LuckyBagEventPopup.MoveTabForce(new TabTypePopupData(0, 1));
        }

        public override void ForceUpdate() { }
        public override bool backBtnCall() { return base.backBtnCall(); } //백 버튼 콜백이 없으면 false 를 출력
    }
}