using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 현재 버튼의 상태만 갱신하는 용도로 관리 객체 따로 뺌
/// 버튼은 업그레이드 버튼 (독립) 과 업글 중, 채굴 중 시간 표시
/// 채굴 결과 마그넷 라벨 
/// 즉시 완료 요청 버튼 총 3개
/// </summary>
namespace SandboxNetwork
{
    public class MineButtonLayerController : MonoBehaviour, EventListener<MiningPopupEvent>
    {
        protected enum eUpgradeButtonState
        {
            LOCK,
            ConstructAble,
            Constructing,
            ConstructFinish,
            UpgradeDisable,
            UpgradeAble,
            Upgrading,
            UpgradeFinish,
            UpgradeMax,
            None,
        }

        [SerializeField] Button accelerateUpgradeButton = null;//업글 중일 때는 기존 업글 버튼 끄고 업글가속 버튼 켜기
        [SerializeField] Button miningStartButton = null;//채굴시작 & 채굴중 & 업글 중& 채굴완료 전체 관리 버튼
        [SerializeField] Button miningUpgradeButton = null;//업그레이드 버튼 독립적으로 사용 - 채굴 시작 대기 일때만 활성화 되는 종속적 조건

        [SerializeField] Text miningUpgradeButtonText = null;
        [SerializeField] Text miningStartConditionText = null;//채굴 시작
        [SerializeField] GameObject miningMultiConditionNode = null;//채굴중, 업글 중 분리 노드 안쪽에 두개 노드 다있음
        [SerializeField] GameObject magnetCheckNode = null;//채굴 완료 상태 일때만 켜지는 노드
        [SerializeField] GameObject advertiseIcon = null;
        [SerializeField] GameObject timeCheckNode = null;
        [SerializeField] Text magnetAmountText = null;
        [SerializeField] Text timeMineUpgradeRemainText = null;//업글 중 , 채굴 중 시간 표시 텍스트
        [SerializeField] Text mineUpgradeConditionText = null;//업글 중, 채굴 중, 채굴완료 라벨 텍스트
        [SerializeField] TimeEnable upgradeMiningTimeObject = null;//시간 세팅 - 각 시간 별로 다음 액션 처리 추가

        [Header("[Reddot]")]
        [SerializeField] GameObject updgradeButtonReddot = null;//업그레이드 조건 달성 시
        [SerializeField] GameObject mineReddotButtonReddot = null;//채굴완료 시 레드닷

        BuildInfo Buildinfo {get{return MiningManager.Instance.MineBuildingInfo;} }
        private void OnEnable()
        {
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        public void OnEvent(MiningPopupEvent eventType)
        {
            switch (eventType.Event)
            {
                case MiningPopupEvent.MiningPopupEventEnum.REPAIR_DRILL://드릴 수리 요청 성공 시 채굴 버튼 상태 갱신
                    InitController();
                    
                    break;
            }
        }
        public void InitController()
        {
            RefreshUpgradeButton();
            RefreshMiningButton();
        }

        //업그레이드 버튼 상태 정의
        public void RefreshUpgradeButton()
        {
            var isAvailUpgrade = MiningManager.Instance.IsUpgradeCondition(false, true);//업글 가능 상태 정의
            if (miningUpgradeButton != null)
                miningUpgradeButton.SetButtonSpriteState(isAvailUpgrade);

            if (accelerateUpgradeButton != null)
                accelerateUpgradeButton.gameObject.SetActive(false);

            if (updgradeButtonReddot != null)
                updgradeButtonReddot.SetActive(false);

            switch (Buildinfo.State)
            {
                case eBuildingState.LOCKED:
                    SetLockState();
                    break;
                case eBuildingState.NOT_BUILT:
                    SetNotBuiltState();

                    if(isAvailUpgrade && updgradeButtonReddot != null)
                        updgradeButtonReddot.SetActive(true);

                    break;
                case eBuildingState.CONSTRUCTING:
                    SetConstructingState();
                    break;
                case eBuildingState.CONSTRUCT_FINISHED:
                    SetConstructFinishState();

                    if (updgradeButtonReddot != null)
                        updgradeButtonReddot.SetActive(true);

                    break;
                default:
                    SetNormalState();
                    break;
            }
        }
        protected virtual void SetUpgradeBtnState(eUpgradeButtonState state)
        {
            miningUpgradeButton.onClick.RemoveAllListeners();
            miningUpgradeButton.gameObject.SetActive(true);
            switch (state)
            {
                case eUpgradeButtonState.LOCK:
                    miningUpgradeButton.onClick.AddListener(() => {
                        OnClickConstruct();
                    });
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000019);
                    miningUpgradeButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.ConstructAble:
                    miningUpgradeButton.onClick.AddListener(() => OnClickConstruct());
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000019);
                    break;
                case eUpgradeButtonState.Constructing:
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000068);
                    miningUpgradeButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.ConstructFinish:
                    miningUpgradeButton.onClick.AddListener(() => OnClickConstructFinish());
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000073);
                    miningUpgradeButton.SetButtonSpriteState(true);
                    break;
                case eUpgradeButtonState.UpgradeDisable:
                    miningUpgradeButton.onClick.AddListener(() => OnClickConstruct());
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000020);
                    miningUpgradeButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.UpgradeAble:
                    miningUpgradeButton.onClick.AddListener(() => OnClickConstruct());
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000020);
                    break;
                case eUpgradeButtonState.Upgrading:
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000108);
                    miningUpgradeButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.UpgradeFinish:
                    miningUpgradeButton.onClick.AddListener(() => OnClickConstructFinish());
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000074);
                    miningUpgradeButton.SetButtonSpriteState(true);
                    break;
                case eUpgradeButtonState.UpgradeMax:
                    miningUpgradeButtonText.text = StringData.GetStringByIndex(100000329);
                    miningUpgradeButton.onClick.AddListener(() => { ToastManager.On(100002530); });
                    miningUpgradeButton.SetButtonSpriteState(false);
                    break;
                case eUpgradeButtonState.None:
                    miningUpgradeButton.SetButtonSpriteState(false);
                    break;
            }
        }

        void SetLockState()//현재 건설 불가 상태
        {
            SetUpgradeBtnState(eUpgradeButtonState.LOCK);

        }

        void SetNotBuiltState()//건설 가능 상태
        {
            SetUpgradeBtnState(eUpgradeButtonState.ConstructAble);
            
        }

        void SetNormalState()//기본 상태 - 업그레이드 항상 켜짐
        {
            if(MiningManager.Instance.GetMineMaxLevel() == MiningManager.Instance.MineBuildingInfo.Level)
                SetUpgradeBtnState(eUpgradeButtonState.UpgradeMax);
            else
                SetUpgradeBtnState(eUpgradeButtonState.UpgradeAble);
        }

        void SetConstructingState()//건설 중 상태
        {
            if (Buildinfo.Level == 0)
                SetUpgradeBtnState(eUpgradeButtonState.Constructing);
            else
                SetUpgradeBtnState(eUpgradeButtonState.Upgrading);

            if (accelerateUpgradeButton != null)
                accelerateUpgradeButton.gameObject.SetActive(true);

            //업글 타이머 세팅 - 채굴쪽 버튼 상태에 따라 on/off 추가해야함.
            if (TimeManager.GetTimeCompare(Buildinfo.ActiveTime) > 0 && Buildinfo.State != eBuildingState.CONSTRUCT_FINISHED)
            {
                upgradeMiningTimeObject.Time = TimeManager.GetTime();
                upgradeMiningTimeObject.Refresh = () =>
                {
                    timeMineUpgradeRemainText.text = TimeManager.GetTimeCompareString(Buildinfo.ActiveTime);
                    if (TimeManager.GetTimeCompare(Buildinfo.ActiveTime) <= 0)
                    {
                        //완료
                        if (upgradeMiningTimeObject != null)
                            upgradeMiningTimeObject.Refresh = null;

                        if (accelerateUpgradeButton != null)
                            accelerateUpgradeButton.gameObject.SetActive(false);

                        if (Buildinfo.Level == 1)
                            SetUpgradeBtnState(eUpgradeButtonState.ConstructFinish);
                        else
                            SetUpgradeBtnState(eUpgradeButtonState.UpgradeFinish);
                    }
                };

                upgradeMiningTimeObject.Refresh();
            }
            else
            {
                upgradeMiningTimeObject.Refresh = null;

                if (accelerateUpgradeButton != null)
                    accelerateUpgradeButton.gameObject.SetActive(false);

                if (Buildinfo.Level == 1)
                    SetUpgradeBtnState(eUpgradeButtonState.ConstructFinish);
                else
                    SetUpgradeBtnState(eUpgradeButtonState.UpgradeFinish);
            }
        }

        void SetConstructFinishState()//건설 완료 상태 - (채광은 불가능한 상태)
        {
            if(Buildinfo.Level >= 1)
                SetUpgradeBtnState(eUpgradeButtonState.UpgradeFinish);
            else
                SetUpgradeBtnState(eUpgradeButtonState.ConstructFinish);
        }


        //건설 요청 팝업 연결
        void OnClickConstruct()
        {
            if(!MiningManager.Instance.IsUpgradeCondition(true))
                return;

            var popup = PopupManager.OpenPopup<BuildingMineUpgradePopup>(new BuildingPopupData(MiningManager.MINE_BUILDING_INSTALL_TAG));
            popup.SetUpgradeCallBack(()=> {
                //업그레이드 요청 성공 시
                InitController();//일단 검증 용
            });
        }

        void OnClickConstructFinish()
        {
            WWWForm data = new WWWForm();
            data.AddField("tag", MiningManager.MINE_BUILDING_INSTALL_TAG);
            NetworkManager.Send("building/complete", data, (jsonData) =>
            {
                InitController();//일단 검증 용
                MiningPopupEvent.ConstructFinish();
                
                BuildingCompletePopup.OpenPopup(MiningManager.Instance.MineBuildingInfo.Level > 1, ()=> {
                    
                });
            });
        }

        public void OnClickHaste()
        {
            eAccelerationType type = Buildinfo.Level == 0 ? eAccelerationType.CONSTRUCT : eAccelerationType.LEVELUP;
            AccelerationMainPopup.OpenPopup(type, new BuildingPopupData(MiningManager.MINE_BUILDING_INSTALL_TAG),()=> {
                InitController();//일단 검증 용
            });
        }

        //채광 시작 요청 버튼
        public void OnClickStartMining()
        {
            var productData = MiningManager.Instance.GetProductData();
            if(productData == null)
            {
                ToastManager.On(StringData.GetStringByStrKey("광산토스트7"));
                return;
            }

            var state = MiningManager.Instance.UserMiningData.MiningState;
            switch(state)
            {
                case eMiningState.NONE:
                    ToastManager.On(StringData.GetStringByStrKey("광산토스트2"));
                    return;
                case eMiningState.START:
                    if (MiningManager.Instance.UserMiningData.MiningDurability < productData.ProductItem.Amount)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("광산토스트3"));
                        return;
                    }
                    break;
                case eMiningState.WAIT_CLAIM:
                {
                    if (GameConfigTable.ON_MAGNET_CLAIM_WITH_AD)
                        AdvertiseManager.Instance.TryADWithPopup((log) => { TryClaim(state, log); }, () => { ToastManager.On(100007692); });
                    else
                        TryClaim(state);

                    return;
                }
                case eMiningState.UPGRADE:
                {
                    ToastManager.On(StringData.GetStringByStrKey("광산토스트4"));
                    return;
                }
                case eMiningState.MINING:
                    ToastManager.On(StringData.GetStringByStrKey("광산토스트5"));
                    return;
            }

            WWWForm data = new WWWForm();
            data.AddField("rid", productData.KEY);//현재 해당 광산이 생성할 수 있는 레시피 key 값 (building_product 의 key)
            NetworkManager.Send("mine/start", data, (jsonData) =>
            {
                if (jsonData.ContainsKey("rs"))
                {
                    JToken resultResponse = jsonData["rs"];
                    if (resultResponse != null && resultResponse.Type == JTokenType.Integer)
                    {
                        int rs = resultResponse.Value<int>();
                        if ((eApiResCode)rs == eApiResCode.OK)
                        {
                            InitController();//일단 검증 용

                        }
                    }
                }
            });
        }

        void TryClaim(eMiningState _state, string log = "")
        {
            if (_state != eMiningState.WAIT_CLAIM)
                return;

            var endMiningTime = MiningManager.Instance.UserMiningData.MiningEndTimeStamp;
            var isEndInterval = TimeManager.GetTimeCompare(endMiningTime) <= 0;//채광 시간 완료
            var hasMineAmount = MiningManager.Instance.UserMiningData.MiningTotalAmountValue > 0; // 받을게 있다.

            //수락 요청
            if(isEndInterval && hasMineAmount)
            {
                WWWForm data = new WWWForm();
                data.AddField("ad_log", log);

                NetworkManager.Send("mine/claim", data, (jsonData) =>
                {
                    if (jsonData.ContainsKey("rs"))
                    {
                        JToken resultResponse = jsonData["rs"];
                        if (resultResponse != null && resultResponse.Type == JTokenType.Integer)
                        {
                            int rs = resultResponse.Value<int>();
                            if ((eApiResCode)rs == eApiResCode.OK)
                            {
                                if(jsonData.ContainsKey("reward"))
                                {
                                    var assetList = new List<Asset>();
                                    var rewardDatas = (JArray)jsonData["reward"];

                                    if(rewardDatas.Count == 3)
                                    {
                                        var itemType = rewardDatas[0].Value<int>();
                                        var itemNo = rewardDatas[1].Value<int>();
                                        var amount = rewardDatas[2].Value<int>();

                                        SystemRewardPopup.OpenPopup(new List<Asset>() { new Asset(itemNo, amount, itemType) }).SetText(StringData.GetStringByIndex(100001194));
                                    }
                                }

                                //마그넷 수락을 하고나면 내구도가 차감하는 방식이기 때문에,
                                MiningPopupEvent.RequestClaim();//마그넷 수령 - 메인 UI 갱신 용도
                                InitController();//버튼 상태 갱신
                            }
                        }
                    }
                });
            }
        }

        //채광 상태에 따른 채굴 버튼 동작  추가
        void RefreshMiningButton()
        {
            var state = MiningManager.Instance.UserMiningData.MiningState;

            if(state == eMiningState.START)//채굴 시작 가능 조건 일때, 기본 내구도 보다 적은 내구도면 강제로 시작 불가 상태 세팅
            {
                var productData = MiningManager.Instance.GetProductData();
                if (productData == null || MiningManager.Instance.UserMiningData.MiningDurability < productData.ProductItem.Amount)
                    state = eMiningState.NONE;
            }

            if (mineReddotButtonReddot != null)
                mineReddotButtonReddot.SetActive(false);

            switch (state)
            {
                case eMiningState.NONE:
                    SetVisibleNodeUI(StringData.GetStringByStrKey("채굴시작"), false);
                    
                    if(miningStartButton != null)
                        miningStartButton.SetButtonSpriteState(false);

                    break;
                case eMiningState.START:
                    SetVisibleNodeUI(StringData.GetStringByStrKey("채굴시작"), false);

                    if (miningStartButton != null)
                        miningStartButton.SetButtonSpriteState(true);

                    break;
                case eMiningState.MINING:
                    SetMiningAndUpgradeTimeNode(true);

                    if (miningStartButton != null)
                        miningStartButton.SetButtonSpriteState(false);
                    break;
                case eMiningState.WAIT_CLAIM:
                    SetVisibleNodeUI(StringData.GetStringByStrKey("채굴완료"), true, eMiningState.WAIT_CLAIM);

                    if (miningStartButton != null)
                        miningStartButton.SetButtonSpriteState(true);

                    if (magnetAmountText != null)
                        magnetAmountText.text = MiningManager.Instance.UserMiningData.VALUE_DESC;

                    if (mineReddotButtonReddot != null)
                        mineReddotButtonReddot.SetActive(true);
                    break;
                case eMiningState.UPGRADE:
                    SetMiningAndUpgradeTimeNode(false);

                    if (miningStartButton != null)
                        miningStartButton.SetButtonSpriteState(false);
                    break;
            }
        }

        //업글 중 시간 세팅은 상단 SetConstructingState() 여기서 해줌 - 채광 중일 때만 세팅
        void SetMiningAndUpgradeTimeNode(bool _isMiningState)
        {
            var state = MiningManager.Instance.UserMiningData.MiningState;

            SetVisibleNodeUI(_isMiningState ? StringData.GetStringByStrKey("채굴중") : StringData.GetStringByStrKey("town_upgrade_text_09"), true, state);

            if (state == eMiningState.UPGRADE)//업글 중이면 리턴
                return;

            //채굴 중 시간 세팅
            int endTime = MiningManager.Instance.UserMiningData.MiningEndTimeStamp;
            var timeCompareNow = TimeManager.GetTimeCompare(endTime);
            if(timeCompareNow > 0)
            {
                upgradeMiningTimeObject.Refresh = () =>
                {
                    int endTime = MiningManager.Instance.UserMiningData.MiningEndTimeStamp;
                    var timeCompareNow = TimeManager.GetTimeCompare(endTime);
                    if (timeCompareNow <= 0)//채굴 완료
                    {
                        //완료
                        if (upgradeMiningTimeObject != null)
                            upgradeMiningTimeObject.Refresh = null;

                        InitController();
                    }
                    else
                        timeMineUpgradeRemainText.text = SBFunc.TimeString(timeCompareNow);
                };

                upgradeMiningTimeObject.Refresh();
            }
            else
            {
                upgradeMiningTimeObject.Refresh = null;
            }
        }
        //UI 껏다 켰다여기서 제어
        void SetVisibleNodeUI(string _titleText, bool _isMultiple = true, eMiningState _state = eMiningState.NONE)
        {
            if (miningStartConditionText != null)
                miningStartConditionText.gameObject.SetActive(!_isMultiple);

            if (miningMultiConditionNode != null)
                miningMultiConditionNode.SetActive(_isMultiple);

            if (advertiseIcon != null)
                advertiseIcon.SetActive(false);

            if (magnetCheckNode != null && timeCheckNode != null && _state != eMiningState.NONE)
            {
                magnetCheckNode.SetActive(false);
                timeCheckNode.SetActive(false);
                switch (_state)
                {
                    case eMiningState.UPGRADE:
                        timeCheckNode.SetActive(true);
                        break;
                    case eMiningState.WAIT_CLAIM:
                        if (advertiseIcon != null)
                            advertiseIcon.SetActive(GameConfigTable.ON_MAGNET_CLAIM_WITH_AD && !User.Instance.ADVERTISEMENT_PASS);

                        magnetCheckNode.SetActive(true);                        
                        break;
                    case eMiningState.MINING:
                        timeCheckNode.SetActive(true);
                        break;

                }
            }

            if(_isMultiple)
                mineUpgradeConditionText.text = _titleText;
            else
                miningStartConditionText.text = _titleText;
        }
    }
}

