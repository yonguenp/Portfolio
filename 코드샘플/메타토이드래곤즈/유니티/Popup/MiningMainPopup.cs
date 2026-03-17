using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public enum eMiningPopupRefreshType//치트 용도 말고는 사용을 안해서 걷어낼 예정.
    {
        ALL,
        MINING,
        MINER,
    }

    public struct MiningPopupEvent
    {
        public enum MiningPopupEventEnum
        {
            REQUEST_DATA,//전체 갱신 용도 - 아직까진 쓸곳 없을 것 같음.

            USE_BOOSTER_ITEM,//부스트 아이템 사용 요청 이후 rs 값

            REQUEST_CLAIM,//마그넷 수령 요청

            REPAIR_DRILL,//드릴 수리 요청

            OPEN_STATUS_PANEL,//상세보기 패널 오픈 - mine/state 갱신 이후에 오픈이라 메인UI 예상채굴량 갱신해주기

            CONSTRUCT_FINISH,//건설 완료 (building_update, landmark_update 가 날아오는 데이터라 전면 상태 갱신 필요함



            /*
             * eMiningApiOp 예전 enum 내용 참조
             * DRILL_UPGRADE = 1,
             * DRILL_UPGRADE_ACCELERATE,
             * DRILL_INSERT,
             * DRILL_EXCHANGE,
             * USE_TICKET,
             * GET_REWARD_MAGNET,
             * GET_REWARD_TICKET,
             */

        }
        static MiningPopupEvent e;

        public int index;
        public MiningPopupEventEnum Event;
        public eMiningPopupRefreshType requestRefreshType;
        public JObject responseData;
        public MineBoosterItem targetItem;
        public MiningPopupEvent(MiningPopupEventEnum _Event, int _index, eMiningPopupRefreshType _requestType, JObject _responseData, MineBoosterItem _targetItem)
        {
            Event = _Event;
            index = _index;
            requestRefreshType = _requestType;
            responseData = _responseData;
            targetItem = _targetItem;
        }
        //전체(data, ui) 갱신
        public static void RequestAndRefreshUI()
        {
            e.Event = MiningPopupEventEnum.REQUEST_DATA;
            EventManager.TriggerEvent(e);
        }
        //부스터 아이템 사용
        public static void RequestUseBoosterItem(MineBoosterItem _targetItem, JObject _rs)
        {
            e.Event = MiningPopupEventEnum.USE_BOOSTER_ITEM;
            e.responseData = _rs;
            e.targetItem = _targetItem;
            EventManager.TriggerEvent(e);
        }
        //드릴 수리
        public static void RequestDrillRepair(JObject _rs)
        {
            e.Event = MiningPopupEventEnum.REPAIR_DRILL;
            e.responseData = _rs;
            EventManager.TriggerEvent(e);
        }
        //마그넷 수령 - push 쪽 데이터로 전부 돌아가는 방식이라 UI 갱신용으로 씀
        public static void RequestClaim()
        {
            e.Event = MiningPopupEventEnum.REQUEST_CLAIM;
            EventManager.TriggerEvent(e);
        }
        //건설 완료
        public static void ConstructFinish()
        {
            e.Event = MiningPopupEventEnum.CONSTRUCT_FINISH;
            EventManager.TriggerEvent(e);
        }
        //상세패널 보기
        public static void OpenStatusPanel()
        {
            e.Event = MiningPopupEventEnum.OPEN_STATUS_PANEL;
            EventManager.TriggerEvent(e);
        }

    }

    public class MiningMainPopup : Popup<MiningMainPopupData>
    {
        [SerializeField] MiningInfoLayerController miningInfoController = null;         //메인 UI 세팅 객체
        [SerializeField] MineBoostInfoLayerController miningBoostItemController = null; //부스터 아이템 관리 객체
        [SerializeField] MineStatusInfoLayerController miningStatusController = null;   //상세보기 관리 객체
        [SerializeField] MineButtonLayerController mineButtonController = null;         //버튼 상태 관련 객체

        private void OnEnable()
        {

        }
        private void OnDisable()
        {
            PopupManager.Instance.Top.SetMagnetUI(false);
            PopupManager.AllClosePopup();
        }

        public override void InitUI()
        {
            miningStatusController?.HideUI();

            // 홀더 체크
            if ((GameConfigTable.WEB3_MENU_OPEN_ON_KOREAN || User.Instance.ENABLE_P2E) == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("홀더전용광산알림"),
                    () =>
                    {
                        ClosePopup();
                    }, null,
                    () =>
                    {
                        ClosePopup();
                    }
                );
                return;
            }

            // UI 관련 처리
            PopupManager.Instance.Top.SetMagnetUI(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            InitPopup();
        }

        //안쓸 생각.
        public override void ForceUpdate(MiningMainPopupData data)
        {
            base.ForceUpdate(data);
            //MiningManager.Instance.UpdateMiningData(InitPopup);
        }

        void InitPopup()
        {
            miningInfoController?.InitController();
            mineButtonController?.InitController();
            miningBoostItemController?.InitController();
        }

        /// <summary>
        /// 현재는 치트 용도로만 쓰고 있음.
        /// </summary>
        /// <param name="refreshType"></param>
        public void RefreshPopup(eMiningPopupRefreshType refreshType = eMiningPopupRefreshType.ALL)
        {
            switch (refreshType)
            {
                case eMiningPopupRefreshType.ALL:
                    miningInfoController.RefreshLayer();
                    break;
                case eMiningPopupRefreshType.MINING:
                    miningInfoController.RefreshLayer();
                    break;
                case eMiningPopupRefreshType.MINER:
                    break;
            }
        }

        /// <summary>
        /// 버튼관련 - 업그레이드 / 티켓 사용 / 가속은 MineButtonLayerController 컴포넌트에서 제어
        /// </summary>
        //public void OnClickUpgradeButton()
        //{
        //    PopupManager.OpenPopup<MiningDrillUpgradePopup>();
        //}
        //public void OnClickUseTicketButton(int ticketType)
        //{
        //    MiningUseTicketPopup newPopup = PopupManager.OpenPopup<MiningUseTicketPopup>();
        //    newPopup?.SetPopupState((eMiningBuffTicketType)ticketType);
        //}
        //public void OnClickDrillExchangeButton()
        //{
        //    PopupManager.OpenPopup<MiningDrillExchangePopup>();
        //}
        //public void OnClickAccelerationUpgradeButton()
        //{
        //    PopupManager.OpenPopup<MiningDrillUpgradeAccelerationPopup>();
        //}

        /// <summary>
        /// 제련소 버튼 - 마그넷으로 교환
        /// </summary>
        public void OnClickShopPopup()
        {
            var goodsState = ShopManager.Instance.GetGoodsState(MiningManager.MINE_SHOP_GOODS_KEY);
            ShopBuyPopup newPopup = PopupManager.OpenPopup<ShopBuyPopup>(new ShopBuyPopupData(goodsState.BaseData));
            newPopup?.SetPopupData(() =>
            {
                //구매 이후의 액션 처리
            });
        }

        /// <summary>
        /// 도움말 팝업 열기
        /// </summary>
        public void OnClickHelpInfoPopup()
        {
            PopupManager.OpenPopup<HelpDescriptionPopup>(new HelpPopupData(StringData.GetStringByStrKey("광산도움말"),new List<string>() {
                "mine_help_title:1","mine_help_desc:1","mine_help_title:2","mine_help_desc:2","mine_help_title:3","mine_help_desc:3"
            }));
        }

        void Clear()
        {

        }

        public override void ClosePopup()
        {
            base.ClosePopup();

            UICanvas.Instance.EndBackgroundBlurEffect();

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);

            //입장을 할 때 mine/state를 타고 온 상태라 끌 때는 현재 상태로만 갱신
            ReddotManager.Set(eReddotEvent.MINING, MiningManager.Instance.IsReddotCondition());//끌때 레드닷 갱신
        }
    }
}