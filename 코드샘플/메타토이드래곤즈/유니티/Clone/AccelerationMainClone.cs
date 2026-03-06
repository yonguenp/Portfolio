using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class AccelerationMainClone : MonoBehaviour
    {
        [Header("[Cash Layer]")]
        public GameObject cashLayerObject = null;
        public Button useCashButton = null;
        public Image cashIcon = null;
        public Text cashAmountText = null;

        [Space(20)]

        [Header("[Ticket Layer]")]
        public GameObject ticketLayerObject = null;
        public GameObject useAllBubbleLayerObject = null;

        public Button useOneButton = null;
        public Button useAllButton = null;

        public Transform itemSlotParent = null;

        public Text itemNameText = null;
        public Text itemAmontText = null;
        public Text useAllButtonText = null;

        eGoodType goodType = eGoodType.NONE;
        ItemBaseData currentItemData = null;
        AccelerationMainData currentMainData = null;

        AccelerationMainPopup parentPopup = null;

        int useAllAmount = 0;
        int cashItemID = 0;
        int cashAmount = 0;

        // config데이터
        int CASH_VALUE = 0;
        float CASH_TIME_VALUE = 0;

        bool isNetworkState = false;
        eAccelerationType accType = eAccelerationType.NONE;
        public void Init(AccelerationMainPopup parent, eGoodType type, AccelerationMainData mainData, ItemBaseData itemData = null)
        {
            ClearState();

            parentPopup = parent;
            goodType = type;
            currentMainData = mainData;
            currentItemData = itemData;
            accType = mainData.accelerateType;

            CASH_VALUE = int.Parse(GameConfigTable.GetConfigValue("ACCELERATION_CASH_VALUE"));

            CASH_TIME_VALUE = 0.0f;
            if (float.TryParse(GameConfigTable.GetConfigValue("ACCELERATION_CASH_TIME_VALUE"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ret))
                CASH_TIME_VALUE = ret;

            InitUI();
            isNetworkState = false;
        }

        public void InitUI()
        {
            if (CASH_VALUE == 0 || CASH_TIME_VALUE == 0) { return; }    // 데이터로드 실패 시 ui 로드 하지 않음
            if (ticketLayerObject == null || cashLayerObject == null) { return; }

            if (currentMainData == null)
                return;

            int currentTime = TimeManager.GetTimeCompare(currentMainData.accMainEndTime);

            switch (goodType)
            {
                case eGoodType.GOLD:
                {
                    cashIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");

                    cashItemID = 10000001;

                    int cashValue = Mathf.CeilToInt(currentTime / CASH_TIME_VALUE);
                    cashAmount = cashValue * CASH_VALUE;
                    cashAmountText.text = cashAmount.ToString();
                    cashLayerObject.SetActive(true);

                    cashAmountText.color = User.Instance.GOLD < cashAmount ? Color.red : Color.white;
                }
                break;
                case eGoodType.GEMSTONE:
                {
                    cashIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");

                    cashItemID = 10000005;

                    int cashValue = Mathf.CeilToInt(currentTime / CASH_TIME_VALUE);
                    cashAmount = cashValue * CASH_VALUE;
                    cashAmountText.text = cashAmount.ToString();
                    cashLayerObject.SetActive(true);

                    cashAmountText.color = User.Instance.GEMSTONE < cashAmount ? Color.red : Color.white;
                }
                break;
                case eGoodType.ITEM:
                    if (currentItemData == null) { return; }

                    int itemCount = User.Instance.GetItemCount(currentItemData.KEY);

                    if (itemCount > 0)
                    {
                        var clone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), itemSlotParent);
                        ItemFrame itemframe = clone.GetComponent<ItemFrame>();
                        itemframe?.SetFrameItemInfo(currentItemData.KEY, itemCount);//0으로 초기화

                        itemNameText.text = currentItemData.NAME;
                        itemAmontText.text = string.Format(StringData.GetStringByIndex(100000798), itemCount);

                        ticketLayerObject.SetActive(true);

                        useAllAmount = (currentTime / currentItemData.VALUE) + 1;
                        useAllAmount = useAllAmount < 0 ? 0 : useAllAmount;
                        useAllAmount = itemCount < useAllAmount ? itemCount : useAllAmount;
                        useAllButtonText.text = string.Format("+{0}", useAllAmount);
                    }
                    else
                    {
                        OffCloneObject();
                    }

                    break;
            }
        }

        public void Refresh()
        {
            if (ticketLayerObject == null || cashLayerObject == null) { return; }

            int currentTime = TimeManager.GetTimeCompare(currentMainData.accMainEndTime);

            switch (goodType)
            {
                case eGoodType.GOLD:
                {
                    int cashValue = currentTime < 0 ? 0 : Mathf.CeilToInt(currentTime / CASH_TIME_VALUE);
                    cashAmount = cashValue * CASH_VALUE;
                    cashAmountText.text = cashAmount.ToString();

                    cashAmountText.color = User.Instance.GOLD < cashAmount ? Color.red : Color.white;
                }
                    break;
                case eGoodType.GEMSTONE:
                {
                    int cashValue = currentTime < 0 ? 0 : Mathf.CeilToInt(currentTime / CASH_TIME_VALUE);
                    cashAmount = cashValue * CASH_VALUE;
                    cashAmountText.text = cashAmount.ToString();

                    cashAmountText.color = User.Instance.GEMSTONE < cashAmount ? Color.red : Color.white;
                }
                    break;
                case eGoodType.ITEM:
                    if (currentItemData == null) { return; }

                    int itemCount = User.Instance.GetItemCount(currentItemData.KEY);

                    if (itemCount > 0)
                    {
                        itemNameText.text = currentItemData.NAME;
                        itemAmontText.text = string.Format(StringData.GetStringByIndex(100000798), itemCount);

                        useAllAmount = (currentTime / currentItemData.VALUE) + 1;
                        useAllAmount = useAllAmount < 0 ? 0 : useAllAmount;
                        useAllAmount = itemCount < useAllAmount ? itemCount : useAllAmount;
                        useAllButtonText.text = string.Format("+{0}", useAllAmount);
                    }
                    else
                    {
                        OffCloneObject();
                    }

                    break;
            }

            
        }

        public void CloseBubbleLayer()
        {
            useAllBubbleLayerObject.SetActive(false);
        }

        public void OnClickUseCashButton()
        {
            // 연속 입력 방어
            if (parentPopup == null) return;
            if (parentPopup.CheckAccState == false) return;
            parentPopup.CheckAccState = false;

            // 보유 재화 체크
            if (goodType == eGoodType.GOLD)
            {
                if (User.Instance.GOLD < cashAmount)
                {
                    ToastManager.On(100000620);
                    return;
                }
            }
            else if (goodType == eGoodType.GEMSTONE)
            {
                if (User.Instance.GEMSTONE < cashAmount)
                {
                    ToastManager.On(100000105);
                    return;
                }
            }

            // 캐쉬 사용 즉시완료 처리
            WWWForm paramData = new WWWForm();
            paramData.AddField("type", (int)accType);
            paramData.AddField("tag", currentMainData.accMainTag);
            paramData.AddField("item", cashItemID);
            if(currentMainData.platform != 0)paramData.AddField("platform", currentMainData.platform);
            paramData.AddField("count", cashAmount);
            paramData.AddField("full", accType == eAccelerationType.JOB && currentMainData.isFull ? 1 : 0);

            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("building/haste", paramData, (jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            
                            if (currentMainData.accelerateType == eAccelerationType.EXCHANGE)
                            {
                                User.Instance.Exchange.OnExchangeData(jsonData);
                                PopupManager.ForceUpdate<AccelerationMainPopup>(); // exchange 는 데이터 갱신 후 force 업데이트 해야 됨
                                OpenBubbleLayer(); // 버블 레이어 처리
                            }
                            else
                            {
                                PopupManager.ForceUpdate<AccelerationMainPopup>();
                                OpenBubbleLayer(); // 버블 레이어 처리
                                Town.Instance.RefreshMap();
                            }
                            break;
                        case (int)eApiResCode.COST_SHORT:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002249));
                            break;
                    }
                }
            }
            , (string arg) =>
            {
                isNetworkState = false;
            });
        }

        public void OnClickUseOneButton()
        {
            // 연속 입력 방어
            if (parentPopup == null) return;
            if (parentPopup.CheckAccState == false) return;
            parentPopup.CheckAccState = false;

            // 소지 아이템 체크
            if (goodType == eGoodType.ITEM)
            {
                if (User.Instance.GetItemCount(currentItemData.KEY) < 1)
                {
                    ToastManager.On(100000945);
                    return;
                }
            }

            // 1개 사용 처리
            WWWForm paramData = new WWWForm();
            paramData.AddField("type", (int)currentMainData.accelerateType);
            paramData.AddField("tag", currentMainData.accMainTag);
            paramData.AddField("item", currentItemData.KEY);
            if (currentMainData.platform != 0) paramData.AddField("platform", currentMainData.platform);
            paramData.AddField("count", 1);

            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("building/haste", paramData, (jsonData) => 
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            // 버블 레이어 처리
                            OpenBubbleLayer();
                            
                            if (currentMainData.accelerateType == eAccelerationType.EXCHANGE)
                            {
                                User.Instance.Exchange.OnExchangeData(jsonData);
                                PopupManager.ForceUpdate<AccelerationMainPopup>();
                            }
                            else
                            {
                                PopupManager.ForceUpdate<AccelerationMainPopup>();
                                Town.Instance.RefreshMap();
                            }
                            break;
                        case (int)eApiResCode.COST_SHORT:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002249));
                            break;
                    }
                }
            },(string arg) =>
            {
                isNetworkState = false;
            });
        }

        public void OnClickUseAllButton()
        {
            // 연속 입력 방어
            if (parentPopup == null) return;
            if (parentPopup.CheckAccState == false) return;
            parentPopup.CheckAccState = false;

            // 소지 아이템 체크
            if (goodType == eGoodType.ITEM)
            {
                if (User.Instance.GetItemCount(currentItemData.KEY) < useAllAmount)
                {
                    ToastManager.On(100000945);
                    return;
                }
            }

            // 전체 사용 처리
            WWWForm paramData = new WWWForm();
            paramData.AddField("type", (int)currentMainData.accelerateType);
            paramData.AddField("tag", currentMainData.accMainTag);
            paramData.AddField("item", currentItemData.KEY);
            if (currentMainData.platform != 0) paramData.AddField("platform", currentMainData.platform);
            paramData.AddField("count", useAllAmount);
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("building/haste", paramData, (jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:

                            if (currentMainData.accelerateType == eAccelerationType.EXCHANGE)
                            {
                                User.Instance.Exchange.OnExchangeData(jsonData);
                                PopupManager.ForceUpdate<AccelerationMainPopup>();
                            }
                            else
                            {
                                PopupManager.ForceUpdate<AccelerationMainPopup>();
                                Town.Instance.RefreshMap();
                            }
                            break;
                        case (int)eApiResCode.COST_SHORT:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002249));
                            break;
                    }
                }
            },(string arg) =>
            {
                isNetworkState = false;
            });
        }

        void OpenBubbleLayer()
        {
            parentPopup?.CloseAllBubble();
            parentPopup?.RegistCloneUpdate(this);
            useAllBubbleLayerObject.SetActive(true);
        }

        void ClearState()
        {
            cashLayerObject?.SetActive(false);
            ticketLayerObject?.SetActive(false);
            useAllBubbleLayerObject?.SetActive(false);
        }

        void OffCloneObject()
        {
            gameObject.SetActive(false);
        }
    }
}