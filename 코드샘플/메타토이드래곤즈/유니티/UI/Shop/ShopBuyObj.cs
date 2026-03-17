using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ShopBuyObj : MonoBehaviour
    {
        [SerializeField]
        protected Text itemNameText = null;
        [SerializeField]
        protected Text itemCountText = null;
        [SerializeField]
        protected Image itemImage = null;

        [SerializeField]
        protected GameObject timeLayer = null;
        [SerializeField]
        protected Text remainTimeText = null;
        [SerializeField]
        protected TimeObject timeObject = null;
        [SerializeField]
        protected Text limitBuyAbleText = null;
        [SerializeField]
        protected Image goodsIcon = null;
        [SerializeField]
        protected Text priceText = null;

        [SerializeField]
        protected Button buyButton = null;


        [SerializeField]
        protected GameObject buyLimitObj = null;
        [SerializeField]
        protected Text buyLimitText = null;

        [SerializeField]
        private GameObject reddot = null;

        [SerializeField]
        protected Transform itemGroupTransform = null;
        [SerializeField]
        protected ItemFrame sampleItem = null;

        [SerializeField]
        protected GameObject OfferIcon = null;

        protected int goodsNum = -1;
        public bool BuyAble { get; private set; } = false;
        public int SortByBuy { get; protected set; } = 0;
        protected VoidDelegate buyCB = null;

        public int Sort { get; private set; } = -1;
        protected ShopGoodsData Data;
        protected bool isSubScribe = false;

        Coroutine timerCoroutine = null;
        private void OnDisable()
        {
            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);

            timerCoroutine = null;
        }

        protected virtual void ClearItemList()
        {
            if (itemGroupTransform != null)
            {
                foreach (Transform child in itemGroupTransform)
                {
                    if (sampleItem.transform == child)
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

        protected virtual void SetItemInfo(ShopGoodsData data)
        {
            if (itemNameText != null)
                itemNameText.text = data.Name;
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
                        int amount = 0;
                        foreach (var reward in rewardGroup)
                        {
                            amount += reward.Reward.Amount;
                        }
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
                        int amount = 0;
                        foreach (var reward in rewardGroup)
                        {
                            amount += reward.Reward.Amount;
                        }
                        itemCountText.text = amount > 1 ? amount.ToString() : string.Empty;
                    }
                    else
                    {
                        itemCountText.text = string.Empty;
                    }
                }
            }
            
            
        }
        public Vector3 GetItemLocalPosition(int count, int index)
        {
            Vector2 pivot = sampleItem.transform.localPosition;

            if ((index / 5) == (count / 5))
            {
                count = (count) % 5;
                switch (count)
                {
                    case 0:
                        break;
                    case 2:
                    case 4:
                        break;
                    case 1:
                    case 3:
                        pivot.x += 45.0f;
                        break;
                }
            }
            pivot.y += (index / 5 * 90.0f);
            pivot.x += (index - 2) * 90f;

            return pivot;
        }
        public virtual void SetBuyLayer(int goodsNumber, ShopGoodsData data, int buyAbleCount, VoidDelegate buyCallBack) 
        {
            ClearItemList();
            SetItemInfo(data);

            Data = data;
            goodsNum = int.Parse(Data.KEY);            
            isSubScribe = false;

            eGoodType priceType = Data.PRICE.GoodType;
            int price = Data.PRICE.Amount;
            int curBuyAbleCount = buyAbleCount;
            Sort = Data.SORT;
            
            timeLayer.SetActive(true);
            buyCB = buyCallBack;
            


            //if (itemSprite != null)
            //{
            //    float spriteRate = itemParentTransform.sizeDelta.y / itemSprite.bounds.size.y;
            //    itemRectTransform.sizeDelta = new Vector2(itemSprite.bounds.size.x * spriteRate, itemRectTransform.sizeDelta.y);
            //}

            SetResourceImage();

            string originPrice = "";
            goodsIcon.gameObject.SetActive(true);
            switch (priceType)
            {
                case eGoodType.GOLD:
                    goodsIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                    BuyAble = User.Instance.GOLD >= price;
                    priceText.text = price == 0 ? StringData.GetStringByStrKey("free") : SBFunc.CommaFromNumber(price);
                    break;
                case eGoodType.GEMSTONE:
                    goodsIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                    BuyAble = User.Instance.GEMSTONE >= price;
                    priceText.text = price == 0 ? StringData.GetStringByStrKey("free") : SBFunc.CommaFromNumber(price);
                    break;
                case eGoodType.CASH:
                    goodsIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "cash");
                    BuyAble = true;
                    goodsIcon.gameObject.SetActive(false);
                    var SKU = ShopSKUData.Get(Data.ID);
                    if (SKU != null)
                    {
                        priceText.text = SKU.PRICE == "0" ? StringData.GetStringByStrKey("free") : SKU.PRICE;
                        if (SKU.IsOfferTime())
                        {
                            originPrice = SKU.LocalPrice;
#if UNITY_EDITOR
                            //각 앱 스토어의 가격을 로드하지만 에디터에서는 로드할게없어서 shop_goods price로 할인가를 추측
                            priceText.text = SBFunc.CommaFromMoney(price);
#endif
                        }
                    }
                    else
                    {
                        priceText.text = price == 0 ? StringData.GetStringByStrKey("free") : SBFunc.CommaFromNumber(price);
                    }

                    break;
                case eGoodType.MAGNET:
                    goodsIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "magnet");
                    BuyAble = User.Instance.UserData.Magnet >= price;
                    priceText.text = price == 0 ? StringData.GetStringByStrKey("free") : SBFunc.CommaFromNumber(price);
                    break;
                case eGoodType.ADVERTISEMENT:
                    int adv_param = data.PRICE.ItemNo;
                    var advState = ShopManager.Instance.GetAdvertiseState(adv_param);
                    goodsIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "icon_ad");
                    
                    int viewCount = advState.VIEW_COUNT;
                    int advLimit = advState.VIEW_LIMIT;
                    
                    curBuyAbleCount = advLimit - viewCount;
                    BuyAble = advState.IS_VALIDE;
                    if (buyLimitText != null)
                        buyLimitText.text = StringData.GetStringFormatByStrKey("상점 계정당 구매가능 횟수", curBuyAbleCount, advLimit);                    
                    break;
            }

            BuyAble &= curBuyAbleCount > 0;
            SetDisableLayerState(!BuyAble);

            foreach(Transform child in limitBuyAbleText.transform)
            {
                Destroy(child.gameObject);
            }

            if (OfferIcon != null)
                OfferIcon.SetActive(false);

            if (curBuyAbleCount > 0)
            {
                timeLayer.SetActive(true);

                if (ShopManager.Instance.PrivateGoods.ContainsKey(goodsNum))
                {
                    int remain = (int)(ShopManager.Instance.PrivateGoods[goodsNum] - TimeManager.GetDateTime()).TotalSeconds;
                    SetTimeObj(remain);
                }
                else
                {
                    SetTimeObj(data.TIME_LIMIT, data.START_TIME, data.END_TIME);
                }
                
                
                string strKey = string.Empty;

                limitBuyAbleText.gameObject.SetActive(true);
                switch (data.BUY_TYPE)
                {
                    case eBuyLimitType.UNLIMIT:
                        limitBuyAbleText.gameObject.SetActive(false);
                        strKey = string.Empty;
                        break;
                    case eBuyLimitType.SEASON:
                        limitBuyAbleText.gameObject.SetActive(false);
                        strKey = string.Empty;
                        break;
                    case eBuyLimitType.WEEK:
                        strKey = "buy_limit_weekly";
                        break;
                    case eBuyLimitType.MONTH:
                        strKey = "buy_limit_monthly";
                        break;
                    case eBuyLimitType.DAILY:
                        strKey = "buy_limit_daily";
                        break;
                    case eBuyLimitType.ACCOUNT:
                        strKey = "buy_limit_account";
                        break;
                }   
                limitBuyAbleText.text = data.BUY_LIMIT > 0 ? StringData.GetStringFormatByStrKey(strKey, data.BUY_LIMIT) : string.Empty;
                priceText.color = BuyAble ? Color.white : Color.red;

                if (OfferIcon != null)
                    OfferIcon.SetActive(!string.IsNullOrEmpty(originPrice));

                if(!string.IsNullOrEmpty(originPrice))
                {
                    limitBuyAbleText.text = originPrice;

                    var textRect = limitBuyAbleText.GetComponent<RectTransform>();
                    var lineObj = Instantiate(new GameObject(), limitBuyAbleText.transform);
                    var lineRect = lineObj.GetComponent<RectTransform>();
                    if(lineRect == null)
                        lineRect = lineObj.AddComponent<RectTransform>();

                    var line = lineObj.gameObject.AddComponent<Image>();
                    line.color = new Color(0.82745f, 0.0f, 0.160784f);

                    float width = textRect.rect.width - 90f;
                    float height = textRect.rect.height- 15f;

                    // 대각선 길이는 피타고라스 공식
                    float diagonal = Mathf.Sqrt(width * width + height * height);

                    // 길이 재설정
                    lineRect.sizeDelta = new Vector2(diagonal, 8f);

                    // 텍스트 중앙에 배치
                    lineRect.anchoredPosition = Vector2.zero;

                    // 회전 (왼쪽 아래 → 오른쪽 위)
                    float angle = Mathf.Atan2(height, width) * Mathf.Rad2Deg;
                    lineRect.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
            else
            {
                timeObject.Refresh = null;
                timeLayer.SetActive(false);
                bool isSub = ShopSubscriptionData.GetByGroup(int.Parse(data.KEY))!= null;


                limitBuyAbleText.text = StringData.GetStringByStrKey(isSub ? "상점구독형보상수령완료" : "상점물품매진");

                priceText.color = Color.gray;
            }

            if(priceType== eGoodType.ADVERTISEMENT)
            {
                SortByBuy = curBuyAbleCount > 0 ? 1 : 0;
            }
            else
            {
                SortByBuy = BuyAble ? 1 : 0;
            }
            
            
            if (buyLimitObj != null)
                buyLimitObj.SetActive(data.BUY_LIMIT > 0);

            if (data.PRICE.GoodType != eGoodType.ADVERTISEMENT)
            {
                buyLimitText.text = StringData.GetStringFormatByStrKey("상점 계정당 구매가능 횟수", buyAbleCount, data.BUY_LIMIT);
            }
            else//data.PRICE.GoodType == eGoodType.ADVERTISEMENT
            {
                int adv_param = data.PRICE.ItemNo;
                var advState = ShopManager.Instance.GetAdvertiseState(adv_param);
                if (!advState.IS_VALIDE)                    
                {
                    if (curBuyAbleCount > 0)
                    {
                        int remain = advState.Remain;
                        timerCoroutine = StartCoroutine(AdvertiseTerm(remain));
                    }
                    else
                    {
                        priceText.text = StringData.GetStringByStrKey("광고매진");
                    }
                }
                else
                {
                    priceText.text = StringData.GetStringByStrKey("광고시청버튼");
                }
            }

            if (reddot != null)
                reddot.SetActive(data.Reddot);
        }
        IEnumerator AdvertiseTerm(int remain)
        {
            while (remain > 0)
            {
                priceText.text = SBFunc.TimeCustomString(remain--, 2);
                yield return SBDefine.GetWaitForSeconds(1.0f);
            }

            priceText.text = StringData.GetStringByStrKey("광고시청");
            SetDisableLayerState(false);
            priceText.color = Color.white;
        }
        void SetResourceImage()
        {
            Sprite itemSprite = null;
            if (Data != null)
                itemSprite = Data.SPRITE;
            if (itemSprite == null)
            {
                itemSprite = ResourceManager.GetResource<Sprite>(eResourcePath.StoreImagePath, GetDefaultPath());
                if (Data != null)
                {
                    CDNManager.TrySetBannerCatchDefault(Data.RESOURCE, "store", itemImage, null, () =>
                    {
                        string resource = "shop_m_default.png";
                        switch (Data.MENU)
                        {
                            case 1:
                                resource = "shop_l_default.png";
                                break;
                            case 10:
                            case 14:
                            case 16:
                                resource = "shop_s_default.png";
                                break;

                            default:
                                break;
                        }

                        CDNManager.SetBanner("store/" + resource, itemImage);
                    });
                }
            }

            itemImage.sprite = itemSprite;
        }

        protected virtual string GetDefaultPath()
        {
            return "shop_banner_small_default";
        }
        protected void SetDisableLayerState(bool state)
        {

            priceText.color = state ? Color.red : Color.white;
            buyButton.SetButtonSpriteState(!state);
            buyButton.interactable = !state;
        }

        void SetTimeObj(int remainT)
        {
            BuyAble =  remainT>0;
            int curTime = TimeManager.GetTime();
            int remain = remainT + curTime;
            timeObject.Refresh = delegate {
                int remainTime = TimeManager.GetTimeCompare(remain);
                if (remainTime >= 0)
                    remainTimeText.text = StringData.GetStringFormatByIndex(100002626, SBFunc.TimeString(remainTime));
                else
                {
                    timeObject.Refresh = null;
                    BuyAble = false;
                    SetDisableLayerState(false);
                }
            };
        }

        void SetTimeObj(bool isLimitTime, DateTime startTime, DateTime endTime)
        {
            if(isLimitTime == false)
            {
                timeLayer.SetActive(false);
                timeObject.Refresh = null;
                return;
            }
            
            if(timeObject!= null)
            {
                bool isOverStartTime = startTime <= TimeManager.GetDateTime();
                DateTime targetTime = isOverStartTime ? endTime : startTime;
                int textFormatIndex = isOverStartTime ? 100002626 : 100005162;
                BuyAble = isOverStartTime;
                timeObject.Refresh = () => {
                    int time = TimeManager.GetTimeCompare(TimeManager.GetTimeStamp(targetTime));
                    if (time >= 0)
                    {
                        remainTimeText.text = StringData.GetStringFormatByIndex(textFormatIndex, SBFunc.TimeString(time));
                    }
                    else
                    {
                        remainTimeText.text = StringData.GetStringFormatByIndex(textFormatIndex, SBFunc.TimeString(0));
                        if (isOverStartTime == false) //[판매 시작까지 남은 카운트] 만료시
                        {
                            SetTimeObj(isLimitTime, startTime, endTime);
                            SetDisableLayerState(false);
                        }
                        else  // 판매 기간 종료
                        {
                            timeObject.Refresh = null;
                            BuyAble = false;
                            SetDisableLayerState(false);
                        }
                        
                    }
                };
            }
        }
        
        public virtual void OnClickBuy()
        {

            if (goodsNum >= 0&& BuyAble)
            {
                // goodsNum 이걸로 뭐 구매하지는 서버에 보내자

                if(Data.REWARD_TYPE == ShopGoodsData.RewardType.DIRECT_REWARD)
                {
                    List<Asset> AllItems = new List<Asset>();

                    foreach (var data in ItemGroupData.Get(Data.REWARD_ID))
                    {
                        AllItems.Add(data.Reward);
                    }
                    if (User.Instance.CheckInventoryGetItem(AllItems))
                    {
                        IsFullBagAlert();
                        return;
                    }

                }

                switch (Data.PRICE.GoodType)
                {
                    case eGoodType.ADVERTISEMENT:
                        if (!buyButton.interactable)
                        {
                            return;
                        }

                        AdvertiseManager.Instance.TryADWithPopup(AdvertiseShowCallBack, () => { ToastManager.On(StringData.GetStringByStrKey("ad_empty_alert")); });

                        break;
                    case eGoodType.CASH:
                        if (ShopBannerData.Get(Data.KEY) != null)
                        {
                            var popup = PopupManager.OpenPopup<ShopBannerPopup>(new ShopBuyPopupData(Data));
                            popup.SetBuyCallBack(buyCB);
                        }
                        else
                        {
                            IAPManager.Instance.TryPurchase(goodsNum,
                                (JToken response) =>
                                {
                                    Debug.Log(response);
                                    buyCB?.Invoke();
                                    if (((JObject)response).ContainsKey("rs"))
                                    {
                                        if (response["rs"].Value<int>() == 0)
                                        {
                                            ToastManager.On(StringData.GetStringByStrKey("결제완료"));
                                        }
                                        else
                                            ToastManager.On(StringData.GetStringByStrKey("결제오류"));
                                    }
                                    else
                                    {
                                        ToastManager.On(StringData.GetStringByStrKey("결제오류"));
                                    }
                                    if (PersonalGoodsData.Get(goodsNum) != null)
                                    {
                                        UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
                                    }
                                }, (JToken response) =>
                                {
                                    ToastManager.On(StringData.GetStringByStrKey("구매불가"));
                                    Debug.Log(response);
                                });
                        }
                        break;
                    default:
                        if (ShopBannerData.Get(Data.KEY) != null)
                        {
                            var popup = PopupManager.OpenPopup<ShopBannerPopup>(new ShopBuyPopupData(Data));
                            popup.SetBuyCallBack(buyCB);
                        }
                        else
                        {
                            ShopBuyPopup newPopup = PopupManager.GetPopup<ShopBuyPopup>();                            
                            if (Data.PRICE.Amount <= 0)
                            {
                                newPopup.SetPopupData(new ShopBuyPopupData(Data), () =>
                                {
                                    // refresh 해줘야 됨
                                    buyCB?.Invoke();
                                });
                                newPopup?.OnClickBuyButton();
                                return;
                            }

                            newPopup = PopupManager.OpenPopup<ShopBuyPopup>(new ShopBuyPopupData(Data));
                            newPopup?.SetPopupData(() =>
                            {
                                // refresh 해줘야 됨
                                buyCB?.Invoke();
                            });
                        }
                        break;
                }
                
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("구매불가"));
            }
        }


        protected void AdvertiseShowCallBack(string log = "")
        {
            //IAPManager.Instance.TryPurchase(goodsNum,
            //                     (JToken response) =>
            //                     {
            //                         Debug.Log(response);
            //                         buyCB?.Invoke();

            //                     }, (JToken response) =>
            //                     {
            //                         ToastManager.On("구매 불가");
            //                         Debug.Log(response);
            //                     });
            WWWForm param = new WWWForm();
            param.AddField("prod", Data.KEY);
            param.AddField("count", 1);
            param.AddField("ad_log", log);
            NetworkManager.Send("shop/buy", param, (response) => {
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
                                    buyCB?.Invoke();
                                }
                                else if (Data.REWARD_TYPE == ShopGoodsData.RewardType.POST_REWARD)
                                {
                                    ToastManager.On(100002506);//보상은 우편으로 지급됩니다.
                                    buyCB?.Invoke();
                                }
                            }
                        }
                        break;
                    }
                }
            });
        }


        protected void IsFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    //메인팝업 열기
                    PopupManager.OpenPopup<InventoryPopup>();
                    PopupManager.ClosePopup<PostListPopup>();
                }, () => { }, () => { });
        }
    }
}

