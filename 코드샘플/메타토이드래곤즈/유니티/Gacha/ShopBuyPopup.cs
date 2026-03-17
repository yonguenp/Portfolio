using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct ShopBuyPopupEvent
    {
        public enum eEventType
        {
            Buy
        }
        public static ShopBuyPopupEvent e;

        public eEventType EventType;

        public static void Buy()
        {
            e.EventType = eEventType.Buy;
            EventManager.TriggerEvent(e);
        }
    }
    public class ShopBuyPopup : Popup<ShopBuyPopupData>
    {
        [SerializeField] Text titleText = null;
        [SerializeField] GameObject productObject = null;
        [SerializeField] GameObject ItemClone = null;

        [SerializeField] Image itemBG = null;
        [SerializeField] Image itemIcon = null;
        [SerializeField] Text itemAmountText = null;

        [SerializeField] Text buyAmountText = null;

        [SerializeField] Image priceIcon = null;
        [SerializeField] Text priceText = null;

        [SerializeField] Image buyButton = null;

        [SerializeField] GameObject amountLayer = null;
        [SerializeField] GameObject scrollObject = null;
        [SerializeField] Slider inputSlider = null;

        int purchaseAmount = 0;
        int maxPurchaseAmount = 0;
        int userGoodsAmount = 0; // 유저가 이 상품을 구매하기 위해 가진 재화


        ShopGoodsState shopGoodStateData = null;
        ItemGroupData currentItemGroupData = null;

        public delegate void BuyCallback();
        BuyCallback buyCallback = null;

        bool isRandomBuy = false;
        Asset rewardAssetData = null;

        public override void InitUI()
        {
            Clear();
        }

        public override void DataRefresh(ShopBuyPopupData data)
        {
            base.DataRefresh(data);

            if (Data == null || Data.ShopGoodsData == null) return;

            shopGoodStateData = ShopManager.Instance.GetGoodsState(int.Parse(Data.ShopGoodsData.KEY));

            List<ItemGroupData> ItemGroupDataList = ItemGroupData.Get(shopGoodStateData.BaseData.REWARD_ID);
            if (ItemGroupDataList != null && ItemGroupDataList.Count > 0)
            {
                currentItemGroupData = ItemGroupDataList[0];
            }

            if (Data.ShopGoodsData.REWARDS != null && Data.ShopGoodsData.REWARDS.Count > 0)
            {
                rewardAssetData = Data.ShopGoodsData.REWARDS[0];
            }

            purchaseAmount = 1;
        }

        public void SetPopupData(ShopBuyPopupData data, BuyCallback callback)
        {
            buyCallback = callback;

            DataRefresh(data);
        }

        public void SetPopupData(BuyCallback callback, bool isFriendShop = false)
        {
            buyCallback = callback;

            SetItemInfoData(isFriendShop);
        }

        void SetItemInfoData(bool isFriendShop = false)
        {
            if (Data == null || Data.ShopGoodsData == null) return;

            shopGoodStateData = ShopManager.Instance.GetGoodsState(int.Parse(Data.ShopGoodsData.KEY));
            
            List<ItemGroupData> ItemGroupDataList = ItemGroupData.Get(shopGoodStateData.BaseData.REWARD_ID);
            if (ItemGroupDataList != null && ItemGroupDataList.Count > 0)
            {
                currentItemGroupData = ItemGroupDataList[0];
            }

            if (Data.ShopGoodsData.REWARDS != null && Data.ShopGoodsData.REWARDS.Count > 0)
            {
                rewardAssetData = Data.ShopGoodsData.REWARDS[0];
            }

            if (rewardAssetData?.BaseData?.ASSET_TYPE == eGoodType.EQUIPMENT)
            {
                itemBG.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PartsIconPath, SBFunc.StrBuilder("bggrade_board_", rewardAssetData.BaseData.GRADE));
                itemBG.gameObject.SetActive(true);
            }
            else
            {
                itemBG.gameObject.SetActive(false);
            }

            titleText.text = Data.ShopGoodsData.Name;

            // 리소스 데이터 관련 세팅
            List<ItemGroupData> groupDataList = ItemGroupData.Get(Data.ShopGoodsData.REWARD_ID);
            if (groupDataList != null && groupDataList.Count == 1 && groupDataList[0].Reward.ICON != null)
            {
                productObject.SetActive(true);
                itemIcon.sprite = groupDataList[0].Reward.ICON;
                if (Data.IsShowGoodsCount)
                    itemAmountText.text = currentItemGroupData.Reward.Amount == 1 ? string.Empty : currentItemGroupData.Reward.Amount.ToString();
                else
                    itemAmountText.text = string.Empty;


                scrollObject.GetComponent<RectTransform>().anchoredPosition = isFriendShop ? new Vector3(0,0,0) : new Vector3(0,72.5f,0);
                amountLayer.SetActive(!isFriendShop);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Data.ShopGoodsData.RESOURCE))
                {
                    if (groupDataList != null && groupDataList.Count > 0)
                    {
                        productObject.SetActive(false);
                        ItemClone.SetActive(true);
                        foreach(var res in groupDataList)
                        {
                            var newItemSlot = Instantiate(ItemClone, ItemClone.transform.parent);
                            ItemFrame itemframe = newItemSlot.GetComponent<ItemFrame>();
                            if (itemframe != null)
                                itemframe.SetFrameItem(res.Reward);
                        }
                        ItemClone.SetActive(false);

                        itemIcon.sprite = groupDataList[0].Reward.ICON;
                        itemAmountText.text = groupDataList[0].Reward.Amount == 1 ? string.Empty : groupDataList[0].Reward.Amount.ToString();
                    }                    
                }
                else
                {
                    productObject.SetActive(true);
                    itemIcon.sprite = Data.ShopGoodsData.SPRITE;
                    itemAmountText.text = string.Empty;
                }
            }

            priceIcon.sprite = SBFunc.GetGoodTypeIcon(Data.ShopGoodsData.PRICE.GoodType);

            isRandomBuy = (Data.ShopGoodsData.MENU == (int)eShopMenuType.ARENA_POINT_SHOP) || (Data.ShopGoodsData.MENU == (int)eShopMenuType.FRIEND_POINT_SHOP);

            CalcPurchaseAmount();

            UpdatePurchaseState();
        }

        public void OnClickMinusButton()
        {
            purchaseAmount--;

            purchaseAmount = purchaseAmount < 1 ? 1 : purchaseAmount;

            UpdatePurchaseState();
        }

        public void OnClickPlusButton()
        {
            ++purchaseAmount;
            if (shopGoodStateData.PurchasedLimit > 0)
            {
                purchaseAmount = purchaseAmount > shopGoodStateData.RemainGoodsCount ? shopGoodStateData.RemainGoodsCount : purchaseAmount;
            }
            //else
            //{
            //    purchaseAmount = purchaseAmount > maxPurchaseAmount ? maxPurchaseAmount : purchaseAmount;
            //}
            UpdatePurchaseState();
        }

        public void OnClickMaxButton()
        {
            
            if (shopGoodStateData.PurchasedLimit > 0)
            {
                purchaseAmount = shopGoodStateData.RemainGoodsCount;
            }
            else
            {
                purchaseAmount = maxPurchaseAmount;
            }
            UpdatePurchaseState();
        }

        public void OnInputSlider()
        {
            if(inputSlider.value == purchaseAmount)
            {
                return;
            }

            purchaseAmount = (int)inputSlider.value;
            UpdatePurchaseState();
        }

        public void OnClickBuyButton()
        {

            if (shopGoodStateData.RemainGoodsCount <= 0)
                return;

            if (purchaseAmount * Data.ShopGoodsData.PRICE.Amount > userGoodsAmount)
            {
                switch (Data.ShopGoodsData.PRICE.GoodType)
                {
                    case eGoodType.GOLD:
                        ToastManager.On(StringData.GetStringByStrKey("골드부족"));
                        return;
                    case eGoodType.GEMSTONE:
                        ToastManager.On(StringData.GetStringByStrKey("다이아부족"));
                        return;
                    case eGoodType.MAGNET:
                        ToastManager.On(StringData.GetStringByStrKey("마그넷부족"));
                        return;
                    case eGoodType.ARENA_POINT:
                        ToastManager.On(StringData.GetStringByStrKey("아레나포인트부족"));
                        return;
                    case eGoodType.FRIENDLY_POINT:
                        ToastManager.On(StringData.GetStringByStrKey("우정포인트부족"));
                        return;
                    default:
                        ToastManager.On(StringData.GetStringByStrKey("재화부족"));
                        return;
                }
            }
            if (Data.ShopGoodsData.REWARD_TYPE == ShopGoodsData.RewardType.DIRECT_REWARD)
            {
                List<Asset> AllItems = new List<Asset>();
                foreach (var data in ItemGroupData.Get(Data.ShopGoodsData.REWARD_ID))
                {
                    AllItems.Add(data.Reward);
                }
                if (User.Instance.CheckInventoryGetItem(AllItems))
                {
                    IsFullBagAlert();
                    return;
                }
            }
            if (isRandomBuy)
            {
                if(purchaseAmount < 1)
                {
                    ToastManager.On(StringData.GetStringByStrKey("구매불가"));
                    return;
                }

                WWWForm param = new WWWForm();
                param.AddField("prod", Data.ShopGoodsData.KEY);
                param.AddField("count", purchaseAmount);

                NetworkManager.Send("shop/randombuy", param, (response) => {
                    var shopgoods = Data.ShopGoodsData;
                    buyCallback.Invoke();

                    ClosePopup();

                    var data = response;
                    var isSuccess = (data["err"].Value<int>() == 0);
                    if (data.ContainsKey("rs"))
                    {
                        var rs = (eApiResCode)data["rs"].Value<int>();
                        switch (rs)
                        {
                            case eApiResCode.OK:
                            {
                                if (isSuccess)
                                {
                                    if (data.ContainsKey("rewards"))
                                    {
                                        SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(data["rewards"])));
                                    }
                                    else if (shopgoods != null && shopgoods.REWARD_TYPE == ShopGoodsData.RewardType.POST_REWARD)
                                    {
                                        ToastManager.On(100002506);//보상은 우편으로 지급됩니다.
                                    }
                                }
                            }
                            break;
                            // to do - 현재 리스트(아레나 리스트나 친구 상점 리스트)의 목록과는 다른 Data.ShopGoodsData.KEY를 던지는 case가 있었음.
                            // 1. 클라쪽에서 아레나상점인지, 친구 상점인지 분기 처리해서 해당 목록에 존재하는 Data.ShopGoodsData.KEY인지 선검증하는 스텝 필요함.
                            // 2. 서버쪽에서 새로주는 리스트를 데이터에 넣고 UI갱신처리 들어오는 param은 getrandomlist 의 값과 포맷 동일.
                            case eApiResCode.CURRENTLY_NOT_AVAILABLE:
                            {

                            }
                            break;
                        }
                    }
                });
            }
            else
            {
                if (purchaseAmount < 1)
                {
                    ToastManager.On(StringData.GetStringByStrKey("구매불가"));
                    return;
                }

                WWWForm param = new WWWForm();
                param.AddField("prod", Data.ShopGoodsData.KEY);
                param.AddField("count", purchaseAmount);

                NetworkManager.Send("shop/buy", param, (response) => {
                    var shopgoods = Data.ShopGoodsData;
                    buyCallback.Invoke();

                    ClosePopup();

                    var data = response;
                    var isSuccess = (data["err"].Value<int>() == 0);
                    if (data.ContainsKey("rs"))
                    {
                        var rs = (eApiResCode)data["rs"].Value<int>();
                        switch (rs)
                        {
                            case eApiResCode.OK:
                            {
                                if (isSuccess)
                                {
                                    if (data.ContainsKey("rewards"))
                                    {
                                        SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(data["rewards"]),true));
                                        ShopBuyPopupEvent.Buy();
                                    }
                                    else if (shopgoods != null && shopgoods.REWARD_TYPE == ShopGoodsData.RewardType.POST_REWARD)
                                    {
                                        ToastManager.On(100002506);//보상은 우편으로 지급됩니다.
                                    }
                                }
                            }
                            break;
                        }
                    }
                });
            }
        }
        void IsFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    //메인팝업 열기
                    PopupManager.OpenPopup<InventoryPopup>();
                    PopupManager.ClosePopup<PostListPopup>();
                }, () => { }, () => { });
        }
        void UpdatePurchaseState()
        {
            buyAmountText.text = purchaseAmount.ToString();
            
            var amountCount = purchaseAmount * Data.ShopGoodsData.PRICE.Amount;
            priceText.text = Data.ShopGoodsData.PRICE.Amount == 0 ? StringData.GetStringByStrKey("free") : amountCount.ToString();

            if(userGoodsAmount < amountCount)
            {
                //buyButton.color = Color.gray;
                priceText.color = Color.red;

                if (inputSlider != null)
                {
                    inputSlider.gameObject.SetActive(false);
                }
            }
            else
            {
               // buyButton.color = Color.white;
                priceText.color = Color.white;

                if (inputSlider != null)
                {
                    if (shopGoodStateData.PurchasedLimit <= 0)
                    {
                        if (purchaseAmount > maxPurchaseAmount)
                            purchaseAmount = maxPurchaseAmount;

                        inputSlider.gameObject.SetActive(true);
                        inputSlider.minValue = 1;
                        inputSlider.maxValue = maxPurchaseAmount;
                        inputSlider.value = purchaseAmount;
                    }
                }
            }
        }

        void CalcPurchaseAmount()
        {
            if (Data == null || Data.ShopGoodsData == null) return;

            switch (Data.ShopGoodsData.PRICE.GoodType)
            {
                case eGoodType.GOLD:
                    maxPurchaseAmount = Data.ShopGoodsData.PRICE.Amount == 0 ? 1 : User.Instance.GOLD / Data.ShopGoodsData.PRICE.Amount;
                    userGoodsAmount = User.Instance.GOLD;
                    break;
                case eGoodType.MILEAGE:
                    maxPurchaseAmount = User.Instance.UserData.Mileage / Data.ShopGoodsData.PRICE.Amount;
                    userGoodsAmount = User.Instance.UserData.Mileage;
                    break;
                case eGoodType.GEMSTONE:
                    maxPurchaseAmount = User.Instance.GEMSTONE / Data.ShopGoodsData.PRICE.Amount;
                    userGoodsAmount = User.Instance.GEMSTONE;
                    break;
                case eGoodType.MAGNET:
                    if (shopGoodStateData != null && shopGoodStateData.PurchasedLimit > 0)
                    {
                        int maxMagnetAmount = User.Instance.UserData.Magnet / Data.ShopGoodsData.PRICE.Amount;
                        int maxRemainAmount = shopGoodStateData.RemainGoodsCount;

                        maxPurchaseAmount = maxRemainAmount < maxMagnetAmount ? maxRemainAmount : maxMagnetAmount;
                    }
                    else
                    {
                        maxPurchaseAmount = User.Instance.UserData.Magnet / Data.ShopGoodsData.PRICE.Amount;
                    }
                    userGoodsAmount = User.Instance.UserData.Magnet;
                    break;
                case eGoodType.ARENA_POINT:
                    if (shopGoodStateData != null && shopGoodStateData.PurchasedLimit > 0)
                    {
                        int maxArenaPointAmount = User.Instance.UserData.Arena_Point / Data.ShopGoodsData.PRICE.Amount;
                        int maxAvailAmount = shopGoodStateData.RemainGoodsCount;

                        maxPurchaseAmount = maxAvailAmount < maxArenaPointAmount ? maxAvailAmount : maxArenaPointAmount;
                    }
                    userGoodsAmount = User.Instance.UserData.Arena_Point;
                    break;
                case eGoodType.FRIENDLY_POINT:
                    if (shopGoodStateData != null && shopGoodStateData.PurchasedLimit > 0)
                    {
                        int maxFriendlyPoint = User.Instance.UserData.Friendly_Point / Data.ShopGoodsData.PRICE.Amount;
                        int maxAvailAmount = shopGoodStateData.RemainGoodsCount;

                        maxPurchaseAmount = maxAvailAmount < maxFriendlyPoint ? maxAvailAmount : maxFriendlyPoint;
                    }
                    userGoodsAmount = User.Instance.UserData.Friendly_Point;
                    break;
                case eGoodType.GUILD_POINT:
                    if (shopGoodStateData != null)
                    {
                        int maxGuildPointAmount = User.Instance.UserData.Guild_Point / Data.ShopGoodsData.PRICE.Amount;
                        int maxAvailAmount = (shopGoodStateData.BaseData.BUY_TYPE == eBuyLimitType.UNLIMIT) ? int.MaxValue : shopGoodStateData.RemainGoodsCount;
                        maxPurchaseAmount = maxAvailAmount < maxGuildPointAmount ? maxAvailAmount : maxGuildPointAmount;
                    }
                    userGoodsAmount = User.Instance.UserData.Guild_Point;
                    break;
            }
            
            if(shopGoodStateData.PurchasedLimit > 0)
            {
                maxPurchaseAmount = Mathf.Min(shopGoodStateData.RemainGoodsCount, 1);
            }

            purchaseAmount = 1;
        }

        void Clear()
        {
            purchaseAmount = 0;
            maxPurchaseAmount = 0;

            buyAmountText.text = purchaseAmount.ToString();
            priceText.text = "0";

            if (inputSlider != null)
            {
                inputSlider.gameObject.SetActive(false);
                inputSlider.value = 0;
            }

            foreach (Transform child in ItemClone.transform.parent)
            {
                if (ItemClone.transform == child || productObject.transform == child)
                    continue;

                Destroy(child.gameObject);
            }
            productObject.SetActive(false);
            ItemClone.SetActive(false);
        }

        public void OnItemInfo()
        {
            if (Data != null && Data.ShopGoodsData != null && Data.ShopGoodsData.REWARDS != null && Data.ShopGoodsData.REWARDS.Count > 0)
                ItemToolTip.OnItemToolTip(Data.ShopGoodsData, itemIcon.gameObject);
        }
    }
}