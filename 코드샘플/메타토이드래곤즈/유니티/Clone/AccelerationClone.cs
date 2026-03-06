using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class AccelerationClone : MonoBehaviour
    {
        [Header("[Cash Layer]")]
        public GameObject cashLayerObject = null;
        public Button useCashButton = null;
        public Image cashIcon = null;
        public Text cashAmountText = null;

        [Space(20)]

        [Header("[Ticket Layer]")]
        public GameObject ticketLayerObject = null;
        public Button useAllButton = null;

        public Transform itemSlotParent = null;

        public Text itemNameText = null;
        public Text itemAmontText = null;
        public Text useAllButtonText = null;

        eGoodType goodType = eGoodType.NONE;
        ItemBaseData currentItemData = null;

        AccelerationImmediatelyPopup parentPopup = null;

        int useAllAmount = 0;
        int cashItemID = 0;
        int cashAmount = 0;

        // config데이터
        int CASH_VALUE = 0;
        float CASH_TIME_VALUE = 0;

        bool isNetworkState = false;
        public void Init(AccelerationImmediatelyPopup parent, eGoodType type, ItemBaseData itemData = null)
        {
            ClearState();

            parentPopup = parent;
            goodType = type;
            currentItemData = itemData;

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
            
            int currentTime = parentPopup.RemainTime;

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
                        ticketLayerObject.SetActive(true);

                        var clone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), itemSlotParent);
                        ItemFrame itemframe = clone.GetComponent<ItemFrame>();
                        itemframe?.SetFrameItemInfo(currentItemData.KEY, itemCount);//0으로 초기화

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

        public void Refresh()
        {
            int currentTime = parentPopup.RemainTime;

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
                        //useAllAmount = useAllAmount < 0 ? 0 : useAllAmount;
                        //useAllAmount = itemCount < useAllAmount ? itemCount : useAllAmount;
                        useAllButtonText.text = string.Format("X {0}", useAllAmount);

                        useAllButton.interactable = itemCount >= useAllAmount;
                    }
                    else
                    {
                        OffCloneObject();
                    }

                    break;
            }

            
        }

        public void OnClickUseCashButton()
        {
            parentPopup.OnAcceleration(new Asset(eGoodType.GEMSTONE, 0, cashAmount));
        }

        
        public void OnClickUseAllButton()
        {
            parentPopup.OnAcceleration(new Asset(eGoodType.ITEM, currentItemData.KEY, useAllAmount));
        }

        void ClearState()
        {
            cashLayerObject?.SetActive(false);
            ticketLayerObject?.SetActive(false);
        }

        void OffCloneObject()
        {
            gameObject.SetActive(false);
        }
    }
}