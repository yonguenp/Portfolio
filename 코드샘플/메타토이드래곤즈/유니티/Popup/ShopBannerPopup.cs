using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using DG.Tweening;

namespace SandboxNetwork
{
    public class ShopBannerPopup : Popup<ShopBuyPopupData>
    {
        [SerializeField]
        Text explainText = null;
        [SerializeField]
        private Button buyBtn;
        [SerializeField]
        private Image goodsIcon;
        [SerializeField]
        private Text priceText;

        [SerializeField]
        GameObject buyLimitObj = null;
        [SerializeField]
        Text buyLimitText = null;
        [SerializeField]
        Text limitBuyAbleText = null;

        [SerializeField]
        private Image backImage;
        [SerializeField]
        private Image upperImage;

        [SerializeField]
        private TimeObject timeObj;
        [SerializeField]
        private GameObject timeLayer;
        [SerializeField]
        private Text timeText;


        [Header("RegularLayer")]
        [SerializeField]
        GameObject regularLayerObj = null;
        [SerializeField]
        Transform itemParentTr = null;
        [SerializeField]
        GameObject packageItem = null;
        [SerializeField]
        TableView tableView = null;
        [SerializeField]
        RectTransform itemParent =  null;



        [Header("subscribe layer - only daily reward")]
        [SerializeField]
        GameObject SubScribeLayer_OnlyDaily = null;
        [SerializeField]
        TableView OnlyDailyRewardTableView = null;
        [SerializeField]
        RectTransform onlyDailyRewardParent = null;
        [SerializeField]
        Text dailyAlertText2 = null;

        [SerializeField]
        GameObject tempBattlePassLayer = null;
        [SerializeField]
        Text battlePassTitleText = null;
        [SerializeField]
        Image passIconImage = null;
        [SerializeField]
        List<Sprite> passIconList = new List<Sprite>();
        [SerializeField]
        GameObject offerIcon = null;

        bool buyAble = false;
        ShopGoodsData curData;
        VoidDelegate buyCallBack = null;

        bool[] isInitTable = { false,false };  // 0 기본형, 1 구독형
        

        bool isSubScribe = false;

        Tween bgTween = null;

        void SetSubCamTextureOn()
        {
            Town.Instance?.SetSubCamState(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        void SetSubCamTextureOff()
        {
            Town.Instance?.SetSubCamState(false);
            UICanvas.Instance.EndBackgroundBlurEffect();
        }

        public override void InitUI()
        {
            //bgTween?.Kill();
            //bgTween = backImage.transform.DOLocalMove(Vector2.one * -500, 30f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
            //backImage.transform.localPosition = Vector2.zero;
            curData = Data.ShopGoodsData;
            //upperImage.sprite = curData.SPRITE;

            if (!string.IsNullOrEmpty(curData.BANNER.RESOURCE))
            {
                CDNManager.TrySetBannerCatchDefault(curData.BANNER.RESOURCE, "store", upperImage, () =>
                {
                    var bannerSprite = curData.BANNER.SPRITE;
                    if (bannerSprite != null)
                    {
                        var xVal = upperImage.rectTransform.sizeDelta.x;
                        float sizeRatio = bannerSprite.bounds.size.y / bannerSprite.bounds.size.x;
                        upperImage.rectTransform.sizeDelta = new Vector2(xVal, sizeRatio * xVal);
                    }
                },
                ()=>
                {
                    CDNManager.SetBanner("store/shop_b_default.png", upperImage);
                });
            }
            tempBattlePassLayer.SetActive(false);            
            backImage.sprite = curData.BANNER.BG_SPRITE;
            SetBuyLimitInfo();
            
            
            if (curData.END_TIME != DateTime.MinValue) // 시즌상품
            {
                SetTimeObj(curData.START_TIME, curData.END_TIME);
            }
            else if(curData.TYPE == eShopType.PRIVATE) // 핫딜상품
            {
                SetTimeObj(TimeManager.GetCustomDateTime(0), ShopManager.Instance.PrivateGoods[int.Parse(curData.KEY)]);
            }
            else
            {
                timeLayer.SetActive(false);
                timeObj.Refresh = null;
            }
            if(curData.PRICE == null)
            {
                ClosePopup();
                Debug.Log("price data is null!!");
            }
                

            goodsIcon.sprite = SBFunc.GetGoodTypeIcon(curData.PRICE.GoodType);

            if (offerIcon != null)
                offerIcon.SetActive(false);

            if (curData.PRICE.GoodType == eGoodType.CASH)
            {
                goodsIcon.gameObject.SetActive(false);
                var SKU = ShopSKUData.Get(curData.ID);
                if (SKU != null)
                {
                    priceText.text = SKU.PRICE == "0" ? StringData.GetStringByStrKey("free") : SKU.PRICE;
                    if (SKU.IsOfferTime())
                        offerIcon.SetActive(true);
                }
            }
            else
            {
                goodsIcon.gameObject.SetActive(true);
                priceText.text = curData.PRICE.Amount == 0 ? StringData.GetStringByStrKey("free") : SBFunc.CommaFromNumber(curData.PRICE.Amount);
            }


            RefreshBuyState();
            var subScribeData = ShopSubscriptionData.GetByGroup(int.Parse(curData.KEY));
            if(subScribeData != null)
            {
                SetSubscribeShop();
                isSubScribe = true;
            }
            else
            {   
                SetRegularShop();
                isSubScribe = false;
            }

            //Data.ShopGoodsData; 
            SetSubCamTextureOn();
        }

        public void SetSubscribeShop()
        {
            var subScribeData = ShopSubscriptionData.GetByGroup(int.Parse(curData.KEY));
            explainText.text = StringData.GetStringByStrKey("상점청약약관_구독");
            string dailyText= subScribeData.DAY==-1 ? StringData.GetStringByStrKey("상점구독형매일지급") : StringData.GetStringFormatByStrKey("상점구독형제한매일지급", subScribeData.DAY);
            dailyAlertText2.text = dailyText;

            regularLayerObj.SetActive(false);
            SubScribeLayer_OnlyDaily.SetActive(true);
            if (isInitTable[1] == false)
            {
                OnlyDailyRewardTableView.OnStart();
                isInitTable[1] = true;
            }


            if (curData.REWARD_ID != 0) // 최초 보상 있음
            {
                
                #region 최초 보상 세팅 - 최초보상 有

                List<ITableData> tableViewItemList = new List<ITableData>();
                foreach (var itemInfo in PostRewardData.GetGroup(curData.REWARD_ID))
                {
                    tableViewItemList.Add(new SubscribeTableData( itemInfo, eSubscribeItemType.FirstSubscribeGet));
                }
                foreach (var itemInfo in PostRewardData.GetGroup(subScribeData.REWARD_ID))
                {
                    tableViewItemList.Add(new SubscribeTableData(itemInfo, eSubscribeItemType.DailySubsscribeGet));
                }

                OnlyDailyRewardTableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject itemNode, ITableData item) =>
                {
                    if (itemNode == null || item == null)
                    {
                        return;
                    }
                    var packageItem = itemNode.GetComponent<ShopPackageItem>();
                    if (packageItem == null)
                    {
                        return;
                    }
                    var data = (SubscribeTableData)item;
                    if(data.Type == eSubscribeItemType.FirstSubscribeGet)
                        packageItem.SetData(data.Reward, StringData.GetStringByStrKey("상점구독형즉시지급"));
                    else
                        packageItem.SetData(data.Reward, dailyText);
                }));
                OnlyDailyRewardTableView.ReLoad();

                #endregion
            }
            else // 최초 보상 없음
            {
                List<ITableData> tableViewItemList3 = new List<ITableData>();
                foreach (var itemInfo in PostRewardData.GetGroup(subScribeData.REWARD_ID))
                {
                    tableViewItemList3.Add(itemInfo);
                }
                OnlyDailyRewardTableView.SetDelegate(new TableViewDelegate(tableViewItemList3, (GameObject itemNode, ITableData item) =>
                {
                    if (itemNode == null || item == null)
                    {
                        return;
                    }
                    var packageItem = itemNode.GetComponent<ShopPackageItem>();
                    if (packageItem == null)
                    {
                        return;
                    }
                    packageItem.SetData(((PostRewardData)item).Reward, dailyText);
                }));
                OnlyDailyRewardTableView.ReLoad();
                
            }

        }


        void SetRegularShop()
        {
            regularLayerObj.SetActive(true);
            SubScribeLayer_OnlyDaily.SetActive(false);
            if (isInitTable[0] == false)
            {
                tableView.OnStart();
                isInitTable[0] = true;
            }

            
            // 패스 아이템 인가
            if (PassInfoData.IsPassGoods(int.Parse(curData.KEY)) || curData.ID == LevelPassPopup.SHOP_LEVEL_PASS_GOODS_ID)
                explainText.text = StringData.GetStringByStrKey("상점청약약관_구독");
            else if (curData.ID == 20003)
                explainText.text = StringData.GetStringByStrKey("상점청약약관_구독");
            else
                explainText.text = StringData.GetStringByStrKey("상점청약약관");

            List<ITableData> tableViewItemList = new List<ITableData>();
            var itemList = PostRewardData.GetGroup(curData.REWARD_ID);
            if(itemList != null && itemList.Count > 0)
            {
                foreach (var itemInfo in PostRewardData.GetGroup(curData.REWARD_ID))
                {
                    tableViewItemList.Add(itemInfo);
                }
            }

            tableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject itemNode, ITableData item) =>
            {
                if (itemNode == null || item == null)
                {
                    return;
                }
                var packageItem = itemNode.GetComponent<ShopPackageItem>();
                if (packageItem == null)
                {
                    return;
                }
                packageItem.SetData(((PostRewardData)item).Reward);
            }));

            tableView.ReLoad();
            
        }


        void SetBuyLimitInfo()
        {
            if (curData.BUY_LIMIT > 0)
            {
                buyLimitObj.SetActive(true);
                buyLimitText.text = StringData.GetStringFormatByStrKey("상점 계정당 구매가능 횟수", ShopManager.Instance.GetGoodsState(int.Parse(curData.KEY)).RemainGoodsCount, curData.BUY_LIMIT);
                limitBuyAbleText.gameObject.SetActive(true);
                string strKey = string.Empty;
                switch (curData.BUY_TYPE)
                {
                    case eBuyLimitType.UNLIMIT:
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
                limitBuyAbleText.text = curData.BUY_LIMIT > 0 ? StringData.GetStringFormatByStrKey(strKey, curData.BUY_LIMIT) : string.Empty;
            }
            else
            {
                buyLimitObj.SetActive(false);
                limitBuyAbleText.gameObject.SetActive(false);
                limitBuyAbleText.text = string.Empty;
            }
            
            
        }

        public override void ClosePopup()
        {
            SetSubCamTextureOff();
            base.ClosePopup();
            //bgTween?.Kill();
            buyCallBack = null;
        }

        void SetTimeObj(DateTime startTime, DateTime endTime)
        {

            if (timeObj != null)
            {
                bool isOverStartTime = startTime <= TimeManager.GetDateTime();
                DateTime targetTime = isOverStartTime ? endTime : startTime;
                int textFormatIndex = isOverStartTime ? 100002626 : 100005149;
                buyAble = isOverStartTime;
                if (isOverStartTime==false)
                {
                    buyBtn.interactable = false;
                    buyBtn.SetButtonSpriteState(false);
                }
                timeObj.Refresh = () => {
                    int time = TimeManager.GetTimeCompare(TimeManager.GetTimeStamp(targetTime));
                    if (time >= 0)
                    {
                        timeText.text = StringData.GetStringFormatByIndex(textFormatIndex, SBFunc.TimeString(time));
                    }
                    else
                    {
                        timeText.text = StringData.GetStringFormatByIndex(textFormatIndex, SBFunc.TimeString(0));
                        if (isOverStartTime == false) //[판매 시작까지 남은 카운트] 만료시
                        {
                            SetTimeObj(startTime, endTime);
                            buyBtn.interactable = true;
                            buyBtn.SetButtonSpriteState(true);
                            
                        }
                        else  // 판매 기간 종료
                        {
                            timeObj.Refresh = null;
                            buyAble = false;
                            buyBtn.interactable = false;
                            buyBtn.SetButtonSpriteState(false);
                            // 여기서 구매 버튼 리프레쉬 해줘야 됨 
                        }

                    }
                };
            }
        }

        public void SetBuyCallBack(VoidDelegate cb)
        {
            buyCallBack = cb;
        }

        public void OnClickBuyButton()
        {

            if (curData.REWARD_TYPE == ShopGoodsData.RewardType.DIRECT_REWARD)
            {
                List<Asset> AllItems = new List<Asset>();

                foreach (var data in ItemGroupData.Get(curData.REWARD_ID))
                {
                    AllItems.Add(data.Reward);
                }
                if (User.Instance.CheckInventoryGetItem(AllItems))
                {
                    IsFullBagAlert();
                    return;
                }

            }
            int goodsNum = int.Parse(curData.KEY);
            if(curData.PRICE.Amount <= 0)
            {
                buyCallBack?.Invoke();
                return;
            }

            IAPManager.Instance.TryPurchase(goodsNum,
                            (JToken response) =>
                            {
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

                                Debug.Log(response);
                                RefreshBuyState();

                                if (isSubScribe)
                                {
                                    ShopManager.Instance.GetGoodsState(goodsNum).UpdateSubscribe(1, ShopSubscriptionData.GetByGroup(goodsNum).DAY, false);
                                }

                                buyCallBack?.Invoke();

                                if (PersonalGoodsData.Get(int.Parse(curData.KEY)) != null)
                                {
                                    UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
                                }

                            }, (JToken response) =>
                            {
                                ToastManager.On(StringData.GetStringByStrKey("구매불가"));
                                Debug.Log(response);
                            });
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

        public void RefreshBuyState()
        {
            buyLimitText.text = StringData.GetStringFormatByStrKey("상점 계정당 구매가능 횟수", ShopManager.Instance.GetGoodsState(int.Parse(curData.KEY)).RemainGoodsCount, curData.BUY_LIMIT);
            if (ShopManager.Instance.GetGoodsState(int.Parse(curData.KEY)).RemainGoodsCount == 0)
            {
                limitBuyAbleText.gameObject.SetActive(true);
                bool isSub = ShopSubscriptionData.GetByGroup(int.Parse(curData.KEY)) != null;
                limitBuyAbleText.text = StringData.GetStringByStrKey(isSub ? "상점구독형보상수령완료" : "상점물품매진");
                priceText.color = Color.gray;
                buyBtn.interactable = false;
                buyBtn.SetButtonSpriteState(false);
            }
            else
            {
                priceText.color = Color.white;
                buyBtn.interactable = true;
                buyBtn.SetButtonSpriteState(true);
            }
        }

        /// <summary>
        /// 아이템 아이콘 세팅도 추가로 필요함 - 논의 후 결정
        /// </summary>
        /// <param name="_isBattlePass"></param>
        public void SetBattlePassUI(bool _isBattlePass = true)
        {
            regularLayerObj.SetActive(false);
            SubScribeLayer_OnlyDaily.SetActive(false);
            tempBattlePassLayer.SetActive(true);
            if (battlePassTitleText != null)
                battlePassTitleText.text = _isBattlePass ? StringData.GetStringByStrKey("배틀패스이용권") : StringData.GetStringByStrKey("레벨패스이용권");
            if(passIconImage != null)
                passIconImage.sprite = _isBattlePass ? passIconList[0] : passIconList[1];
        }
    }

    public class SubscribeTableData : ITableData
    {

        public string key { get; private set; } = "";

        public eSubscribeItemType Type { get; private set; }

        public Asset Reward { get; private set; }
        string ITableData.GetKey()
        {
            return key;
        }


        public SubscribeTableData(PostRewardData postData, eSubscribeItemType type)
        {
            key = postData.GetKey();
            Reward = postData.Reward;
            Type = type;

        }


        void ITableData.Init()
        {
            
        }
    }
}
