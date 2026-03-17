
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{

    
    public class ShopBuyObjMedium : ShopBuyObj
    {


        [SerializeField]
        Button dailyRewardBtn =null;
        [SerializeField]
        GameObject subScribeLayer = null;

        [SerializeField]
        GameObject itemInfoObj = null;



        [Header("subscribe layer - before buy")]
        [SerializeField]
        GameObject noneSubScribeLayer = null;
        [SerializeField]
        GameObject plusObj = null;
        [SerializeField]
        Transform firstRewardLayout = null;
        [SerializeField]
        Transform secondRewardLayout = null;

        [Header("subscribe layer - after buy")]
        [SerializeField]
        GameObject didSubScribeLayer =null;
        [SerializeField]
        Transform dailyRewardLayout = null;
        [SerializeField]
        Text getItemBtnText = null;


        [SerializeField]
        protected Transform itemGroupRow = null;

        List<ShopPackageItem> subscribeRewardObjectsLeft = new List<ShopPackageItem>();
        List<ShopPackageItem> subscribeRewardObjectsRight = new List<ShopPackageItem>();
        List<ShopPackageItem> subscribeRewardObjectsDaily = new List<ShopPackageItem>();

        VoidDelegate ItemGetCB = null;
        
        public eStoreSubscribeState CurSubState { get; private set; } = eStoreSubscribeState.NONE;

        protected override void ClearItemList()
        {
            if (itemGroupTransform != null)
            {
                foreach (Transform child in itemGroupTransform)
                {
                    if (sampleItem.transform == child)
                        continue;
                    if (itemGroupRow == child)
                        continue;

                    Destroy(child.gameObject);
                }

                itemGroupTransform.gameObject.SetActive(false);
            }

            if (itemNameText != null && itemCountText != null)
            {
                itemNameText.gameObject.SetActive(false);
                itemCountText.gameObject.SetActive(false);
            }
        }
        protected override void SetItemInfo(ShopGoodsData data)
        {
            if (sampleItem != null)
            {
                itemGroupTransform.gameObject.SetActive(true);
                sampleItem.gameObject.SetActive(true);

                Transform row = itemGroupTransform;
                if (itemGroupRow != null)
                {
                    itemGroupRow.gameObject.SetActive(true);
                    row = Instantiate(itemGroupRow, itemGroupTransform);
                }

                int index = 5 - (data.REWARDS.Count % 5);
                foreach (var item in data.REWARDS)
                {
                    if (itemGroupRow != null && index % 5 == 0)
                    {
                        row = Instantiate(itemGroupRow, itemGroupTransform);
                    }

                    var clone = Instantiate(sampleItem, row);
                    ItemFrame itemframe = clone.GetComponent<ItemFrame>();
                    itemframe.SetFrameItem(item);
                    //clone.transform.localPosition = GetItemLocalPosition(data.REWARDS.Count, index);

                    index++;
                }
                var subScribeData = ShopSubscriptionData.GetByGroup(int.Parse(data.KEY));
                if (subScribeData != null)
                {
                    foreach (var item in PostRewardData.GetGroup(subScribeData.REWARD_ID))
                    {
                        if (itemGroupRow != null && index % 5 == 0)
                        {
                            row = Instantiate(itemGroupRow, itemGroupTransform);
                        }

                        var clone = Instantiate(sampleItem, row);
                        ItemFrame itemframe = clone.GetComponent<ItemFrame>();

                        string dailyText = StringData.GetStringByStrKey("매일 지급"); 
                        itemframe.SetFrameItem(item.Reward, dailyText);

                        index++;
                    }
                }

                sampleItem.gameObject.SetActive(false);
                if (itemGroupRow != null)
                    itemGroupRow.gameObject.SetActive(false);
            }

            if (itemNameText != null)
                itemNameText.text = data.Name;

            if (data.REWARDS.Count == 1)
            {
                if (itemNameText != null && itemCountText != null)
                {
                    ClearItemList();

                    itemNameText.gameObject.SetActive(true);
                    itemCountText.gameObject.SetActive(true);

                    if (data.REWARD_TYPE == ShopGoodsData.RewardType.POST_REWARD)
                    {
                        var rewardGroup = PostRewardData.GetGroup(data.REWARD_ID);
                        if (rewardGroup != null)
                        {
                            int amount = rewardGroup[0].Reward.Amount;
                            itemCountText.text = amount > 1 ? amount.ToString() : string.Empty;
                        }
                        else
                        {
                            itemCountText.text = string.Empty;
                        }
                    }
                    else if (data.REWARD_TYPE == ShopGoodsData.RewardType.DIRECT_REWARD)
                    {
                        var rewardGroup = ItemGroupData.Get(data.REWARD_ID);
                        if (rewardGroup != null)
                        {
                            int amount = rewardGroup[0].Reward.Amount;
                            itemCountText.text = amount > 1 ? amount.ToString() : string.Empty;
                        }
                        else
                        {
                            itemCountText.text = string.Empty;
                        }
                    }
                }
            }
        }
        public void SetBuyLayer(int goodsNumber, ShopGoodsData data, int buyAbleCount, VoidDelegate buyCallBack, VoidDelegate itemGetCallBack, eStoreSubscribeState subState = eStoreSubscribeState.NOT_SUB, int subscribeDay = 1)
        {
            base.SetBuyLayer(goodsNumber, data, buyAbleCount, buyCallBack);

            subScribeLayer.SetActive(false);
            dailyRewardBtn.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(true);

            //if (data.TYPE == eShopType.SUBSCRIBE)
            //{
            //    SetSubScribeLayer(data, itemGetCallBack, subState, subscribeDay);
            //}
            if (data.TYPE == eShopType.SUBSCRIBE)
            {
                SetSubscribeButton(subState);
            }
            else
            {
                isSubScribe = false;
                CurSubState = eStoreSubscribeState.NONE;
            }

            switch (data.BUY_TYPE)
            {
                case eBuyLimitType.UNLIMIT:
                    buyLimitObj.SetActive(true);
                    buyLimitText.text = StringData.GetStringByStrKey("buy_unlimit");
                    break;
            }
        }


        void SetSubscribeButton(eStoreSubscribeState subState)
        {
            isSubScribe = true;
            CurSubState = subState;
            switch (CurSubState)
            {
                case eStoreSubscribeState.NOT_SUB:
                    SortByBuy = 1;
                    dailyRewardBtn.gameObject.SetActive(false);
                    buyButton.gameObject.SetActive(true);
                    break;
                case eStoreSubscribeState.REWARD_ABLE:
                    SortByBuy = 2;
                    buyButton.gameObject.SetActive(false);
                    dailyRewardBtn.gameObject.SetActive(true);
                    getItemBtnText.text = StringData.GetStringByStrKey("상점구독형보상수령가능");
                    dailyRewardBtn.SetButtonSpriteState(true);
                    dailyRewardBtn.interactable = true;
                    break;
                case eStoreSubscribeState.REWARDED:
                    SortByBuy = 1;
                    buyButton.gameObject.SetActive(false);
                    dailyRewardBtn.gameObject.SetActive(true);
                    getItemBtnText.text = StringData.GetStringByStrKey("상점구독형보상수령완료");
                    dailyRewardBtn.SetButtonSpriteState(false);
                    dailyRewardBtn.interactable = false;
                    //타임 오브젝트를 통해서 갱신이 필요할듯
                    break;

            }
        }



        void SetSubScribeLayer(ShopGoodsData data, VoidDelegate itemGetCallBack, eStoreSubscribeState subState, int subscribeDay)
        {
            var subScribeData = ShopSubscriptionData.GetByGroup(int.Parse(data.KEY));
            if (subScribeData == null)
            {
                return;
            }
            CurSubState = subState;
            SetDisableLayerState(false);
            ItemGetCB = itemGetCallBack;
            subScribeLayer.SetActive(true);
            OffChildObj();
            var REWARDS = PostRewardData.GetGroup(data.REWARD_ID);
            var SUBSCRIBE_REWARD = PostRewardData.GetGroup(subScribeData.REWARD_ID);
            //buyLimitObj.SetActive(false);

            noneSubScribeLayer.SetActive(false);
            didSubScribeLayer.SetActive(false);
            return;

            switch (CurSubState)
            {
                case eStoreSubscribeState.NOT_SUB:
                    if (REWARDS == null)
                    {
                        firstRewardLayout.gameObject.SetActive(false);
                        plusObj.gameObject.SetActive(false);
                    }
                    else
                    {
                        for (int i = 0, count = REWARDS.Count; i < count; ++i)
                        {
                            if (subscribeRewardObjectsLeft.Count <= i)
                            {
                                var obj = Instantiate(itemInfoObj, firstRewardLayout).GetComponent<ShopPackageItem>();
                                subscribeRewardObjectsLeft.Add(obj);
                            }
                            subscribeRewardObjectsLeft[i].SetData(REWARDS[i].Reward);
                        }

                        firstRewardLayout.gameObject.SetActive(REWARDS.Count > 0);
                        plusObj.gameObject.SetActive(REWARDS.Count > 0);
                    }

                    for (int i = 0, count = SUBSCRIBE_REWARD.Count; i < count; ++i)
                    {
                        if (subscribeRewardObjectsRight.Count <= i)
                        {
                            var obj = Instantiate(itemInfoObj, secondRewardLayout).GetComponent<ShopPackageItem>();
                            subscribeRewardObjectsRight.Add(obj);
                        }
                        subscribeRewardObjectsRight[i].SetData(SUBSCRIBE_REWARD[i].Reward);
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(firstRewardLayout.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(secondRewardLayout.GetComponent<RectTransform>());
                    noneSubScribeLayer.SetActive(true);
                    didSubScribeLayer.SetActive(false);
                    dailyRewardBtn.gameObject.SetActive(false);
                    break;
                case eStoreSubscribeState.REWARD_ABLE:
                    noneSubScribeLayer.SetActive(false);
                    didSubScribeLayer.SetActive(true);
                    dailyRewardBtn.gameObject.SetActive(true);
                    dailyRewardBtn.SetButtonSpriteState(true);
                    for (int i = 0, count = SUBSCRIBE_REWARD.Count; i < count; ++i)
                    {
                        if (subscribeRewardObjectsDaily.Count <= i)
                        {
                            var obj = Instantiate(itemInfoObj, dailyRewardLayout).GetComponent<ShopPackageItem>();
                            subscribeRewardObjectsDaily.Add(obj);
                        }
                        subscribeRewardObjectsDaily[i].SetData(SUBSCRIBE_REWARD[i].Reward);
                    }
                    getItemBtnText.text = StringData.GetStringByStrKey("상점구독형아이템수령");
                    break;
                case eStoreSubscribeState.REWARDED:
                    noneSubScribeLayer.SetActive(false);
                    subScribeLayer.SetActive(true);
                    didSubScribeLayer.SetActive(true);
                    dailyRewardBtn.gameObject.SetActive(true);
                    dailyRewardBtn.SetButtonSpriteState(false);
                    for (int i = 0, count = SUBSCRIBE_REWARD.Count; i < count; ++i)
                    {
                        if (subscribeRewardObjectsDaily.Count <= i)
                        {
                            var obj = Instantiate(itemInfoObj, dailyRewardLayout).GetComponent<ShopPackageItem>();
                            subscribeRewardObjectsDaily.Add(obj);
                        }
                        subscribeRewardObjectsDaily[i].SetData(SUBSCRIBE_REWARD[i].Reward);
                    }
                    getItemBtnText.text = StringData.GetStringByStrKey("상점구독형아이템수령완료");
                    break;
                case eStoreSubscribeState.NONE:
                    subScribeLayer.SetActive(false);
                    return;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(dailyRewardLayout.GetComponent<RectTransform>());
        }

        protected override string GetDefaultPath()
        {
            return "shop_banner_middle_default";
        }

        void OffChildObj()
        {
            foreach (var child in subscribeRewardObjectsLeft)
            {
                if (child == null) continue;

                child.gameObject.SetActive(false);
            }
            foreach (var child in subscribeRewardObjectsRight)
            {
                if (child == null) continue;

                child.gameObject.SetActive(false);
            }
            foreach (var child in subscribeRewardObjectsDaily)
            {
                if (child == null) continue;

                child.gameObject.SetActive(false);
            }
        }

        
        public void OnClickLayer()
        {
            if(goodsNum >= 0 && BuyAble && (CurSubState == eStoreSubscribeState.NONE|| CurSubState == eStoreSubscribeState.NOT_SUB))
            {
                OnClickBuy();
            }
            else
            {
                if (isSubScribe)
                {
                    if (CurSubState == eStoreSubscribeState.REWARD_ABLE)
                        OnClickGetItemBtn();
                    else if (CurSubState == eStoreSubscribeState.REWARDED)
                        ToastManager.On(StringData.GetStringByStrKey("상점구독형보상수령완료"));
                }
                else
                {
                    ToastManager.On(StringData.GetStringByStrKey("구매불가"));
                }
            }
        }
        public void OnClickGetItemBtn()
        {
            IAPManager.Instance.TrySubscribe(Data.ID, eShopIAPCheckType.SubscribeDailyReward,
            (JToken response) =>
            {
                ToastManager.On(StringData.GetStringByStrKey("구독수령"));
                SetSubscribeButton(ShopManager.Instance.GetGoodsState(Data.ID).SubscribeState);
            }, (JToken response) =>
            {
                ToastManager.On(StringData.GetStringByStrKey("구독수령실패"));
                Debug.Log(response);
            });
            ItemGetCB?.Invoke();
        }


    }
}

