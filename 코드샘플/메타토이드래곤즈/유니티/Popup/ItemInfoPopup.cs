using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ItemInfoPopup : Popup<ItemInfoPopupData>
    {
        public Transform itemParent = null;
        public Text itemNameText = null;

        [Header("[Sell Layer]")]
        public GameObject sellLayerObject = null;
        public Slider sellAmountSlider = null;

        public Text ItemAmountText = null;
        public Text PriceText = null;

        [Space(20)]

        [Header("[Cash Layer]")]
        public GameObject descLayerObject = null;
        public GameObject openSellButtonObject = null;
        public GameObject dappButtonObject = null;
        public GameObject disableSellGuideObject = null;
        public Text descText = null;
        ItemBaseData itemData = null;

        [Space(20)]

        [Header("[DApp Layer]")]
        public GameObject dappLayerObject = null;
        public Slider convertAmountSlider = null;
        public Text ItemConvertAmountText = null;
        bool isAvailSell = false;

        int totalItemCount = 0;
        int sellAmount = 1;
        int convertAmount = 1;

        private bool isNetworkState = false;
        public override void ForceUpdate(ItemInfoPopupData data)
        {
            base.DataRefresh(data);
            Refresh();
        }

        public override void InitUI()
        {
            InitDescInfo();
        }

        public void OnClickOpenSellLayerButton()
        {
            sellLayerObject.SetActive(true);
            descLayerObject.SetActive(false);
            dappLayerObject.SetActive(false);

            InitSellLayerInfo();
        }

        public void OnClickMinusButton()
        {
            sellAmount--;

            sellAmount = sellAmount < 1 ? 1 : sellAmount;

            sellAmountSlider.value = sellAmount;
            UpdateSliderText();
        }

        public void OnClickPlusButton()
        {
            sellAmount++;

            sellAmount = sellAmount > totalItemCount ? totalItemCount : sellAmount;

            sellAmountSlider.value = sellAmount;
            UpdateSliderText();
        }

        public void OnClickSellButton()
        {
            WWWForm paramData = new WWWForm();
            paramData.AddField("item", itemData.KEY);
            paramData.AddField("count", sellAmount);
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("item/sell", paramData, (jsonObject) => 
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonObject["rs"]))
                {
                    switch (jsonObject["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            PopupManager.GetPopup<InventoryPopup>()?.RefreshUI();
                            PopupManager.ClosePopup<ItemInfoPopup>();
                            break;
                    }
                }
            },
            (string arg) =>
            {
                isNetworkState = false;
            });
        }

        public override void ClosePopup()
        {
            if (Data != null)
                Data.Frame.setFrameNormal();
            base.ClosePopup();
        }

        public void OnSliderValueChanged()
        {
            sellAmount = (int)sellAmountSlider.value;

            UpdateSliderText();
        }

        void InitDescInfo()
        {
            if (Data == null) { return; }

            itemData = ItemBaseData.Get(Data.Frame.GetItemID());

            if (itemData == null) { return; }

            ClearLayer();

            totalItemCount = User.Instance.GetItemCount(itemData.KEY);

            isAvailSell = itemData.SELL > 0;//판매 가능 인지
            
            isNetworkState = false;

            foreach(Transform child in itemParent)
            {
                Destroy(child.gameObject);
            }
            // 아이템 세팅
            var clone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), itemParent);
            ItemFrame itemframe = clone.GetComponent<ItemFrame>();
            itemframe?.SetFrameItemInfo(itemData.KEY, totalItemCount);
            itemframe?.SetVisibleNFTNode(itemData);
            itemNameText.text = itemData.NAME;
            descText.text = itemData.DESC;

            openSellButtonObject.SetActive(isAvailSell);

            bool isP2EUser = User.Instance.ENABLE_P2E;
            bool isNFT = itemData.ENABLE_NFT;
            if(isP2EUser)
            {
                disableSellGuideObject.SetActive(!isNFT && !isAvailSell);
                dappButtonObject.SetActive(isNFT && !isAvailSell);
            }
            else
            {
                disableSellGuideObject.SetActive(!isAvailSell);
                dappButtonObject.SetActive(false);
            }
        }

        void InitSellLayerInfo()
        {
            sellAmountSlider.minValue = 1;
            sellAmountSlider.maxValue = totalItemCount;
            sellAmountSlider.value = 1;
            sellAmount = 1;

            UpdateSliderText();
        }

        void UpdateSliderText()
        {
            ItemAmountText.text = string.Format("{0}", sellAmount);

            PriceText.text = (itemData.SELL * sellAmount).ToString();
        }

        public void OnClickMaxButton()
        {
            sellAmount = totalItemCount;

            ItemAmountText.text = string.Format("{0}", sellAmount);

            sellAmountSlider.value = sellAmount;

            UpdateSliderText();
        }

        void Refresh()
        {
            if (Data == null) { return; }
        }

        void ClearLayer()
        {
            sellLayerObject.SetActive(false);
            descLayerObject.SetActive(true);
            dappLayerObject.SetActive(false);

            openSellButtonObject.SetActive(false);
            disableSellGuideObject.SetActive(false);
        }

        /// <summary>
        /// dapp 호출 연결
        /// </summary>
        public void OnClickDApp()
        {
            sellLayerObject.SetActive(false);
            descLayerObject.SetActive(false);
            dappLayerObject.SetActive(true);

            convertAmount = 0;
            convertAmountSlider.minValue = 0;
            convertAmountSlider.maxValue = totalItemCount;
            convertAmountSlider.value = 0;
        }
        void UpdateSliderTextDApp()
        {
            ItemConvertAmountText.text = string.Format("{0}", convertAmount);
        }
        public void OnClickDAppMinusButton()
        {
            convertAmount--;

            convertAmount = convertAmount < 1 ? 0 : convertAmount;

            convertAmountSlider.value = convertAmount;
            UpdateSliderTextDApp();
        }

        public void OnClickDAppPlusButton()
        {
            convertAmount++;

            convertAmount = convertAmount > totalItemCount ? totalItemCount : convertAmount;

            convertAmountSlider.value = convertAmount;
            UpdateSliderTextDApp();
        }

        public void OnClickDAppMaxButton()
        {
            convertAmount = totalItemCount;

            ItemConvertAmountText.text = string.Format("{0}", convertAmount);

            convertAmountSlider.value = convertAmount;

            UpdateSliderTextDApp();
        }

        public void OnDAppSliderValueChanged()
        {
            convertAmount = (int)convertAmountSlider.value;

            UpdateSliderTextDApp();
        }

        public void OnClickDAppConvert()
        {
            if (convertAmount <= 0)
            {
                ClearLayer();
                return;
            }

            DAppManager.Instance.OpenDAppWithItem(itemData.KEY, convertAmount, () => {
                PopupManager.GetPopup<InventoryPopup>()?.RefreshUI();
                PopupManager.ClosePopup<ItemInfoPopup>();
            });
        }
    }
}