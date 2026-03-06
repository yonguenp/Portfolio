using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ConditionalBuyPopup : Popup<ConditionBuyData>
    {
        [SerializeField]
        private Text titleText = null;
        [SerializeField]
        private Text conditionText = null;

        [SerializeField]
        private Button GetBtn = null;

        [SerializeField]
        private Transform itemParentTr = null;

        [SerializeField]
        private GameObject itemPrefab = null;

        [SerializeField]
        private Image bgImg = null;

        [SerializeField]
        private Image bannerImg = null;
        
        private int curKey = 0;

        private VoidDelegate itemGetCallBack = null;

        bool buyAbleState = false;

        bool privateConditionState = false;
        public override void InitUI()
        {
            curKey = Data.Key;



            buyAbleState = ShopManager.Instance.GetGoodsState(curKey).RemainGoodsCount > 0;
            privateConditionState = ShopManager.Instance.PrivateGoods.ContainsKey(curKey);
            var data = PersonalGoodsData.Get(curKey);
            GetBtn.SetButtonSpriteState(buyAbleState && privateConditionState);
            GetBtn.interactable = buyAbleState;
            
            bgImg.sprite = ShopBannerData.Get(curKey.ToString()).BG_SPRITE;

            if (!string.IsNullOrEmpty(ShopBannerData.Get(curKey.ToString()).RESOURCE))
            {
                CDNManager.TrySetBannerCatchDefault(ShopBannerData.Get(curKey.ToString()).RESOURCE, "store", bannerImg, () =>
                {
                    var bannerRect = bannerImg.GetComponent<RectTransform>();
                    var bannerSprite = ShopBannerData.Get(curKey.ToString()).SPRITE;
                    if (bannerSprite != null)
                    {
                        float sizeRatio = bannerSprite.bounds.size.y / bannerSprite.bounds.size.x;
                        bannerRect.sizeDelta = new Vector2(bannerRect.sizeDelta.x, bannerRect.sizeDelta.x * sizeRatio);
                    }
                });
            }

            if (data != null)
            {
                conditionText.text = data.DESC_STRING;
                titleText.text = data.TITLE_STRING;
            }
            
            SetReward();
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
        }

        public void SetReward()
        {
            SBFunc.RemoveAllChildrens(itemParentTr);
            var rewardDatas =PostRewardData.GetGroup(curKey);
            if (rewardDatas != null)
            {
                foreach (var info in rewardDatas)
                {
                    var item = Instantiate(itemPrefab, itemParentTr).GetComponent<ShopPackageItem>();
                    item.SetData(info.Reward);
                }
                return;
            }
            var rewardItemGroupDatas = ItemGroupData.Get(ShopGoodsData.Get(curKey).REWARD_ID);
            if(rewardItemGroupDatas != null)
            {
                foreach (var info in rewardItemGroupDatas)
                {
                    var item = Instantiate(itemPrefab, itemParentTr).GetComponent<ShopPackageItem>();
                    item.SetData(info.Reward);
                }
                return;
            }
        }

        public void SetRewardCallBack(VoidDelegate itemGetCB)
        {
            itemGetCallBack = itemGetCB;
        }


        void ItemGetProcess()
        {
            bool btnState = ShopManager.Instance.GetGoodsState(curKey).RemainGoodsCount > 0;
            GetBtn.SetButtonSpriteState(btnState);
            GetBtn.interactable = btnState;
            UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
            itemGetCallBack?.Invoke();
        }
        public void OnClickGetReward()
        {
            if (buyAbleState == false)
                return;
            if (PopupManager.IsPopupOpening(PopupManager.GetPopup<ShopPopup>()) && buyAbleState ==false)
                return;
            if(buyAbleState && privateConditionState ==false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("상점이동유도문구"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"), () =>
                {
                    PopupManager.ClosePopup<ConditionalBuyPopup>();
                    PopupManager.ClosePopup<SystemPopup>();
                    PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData());
                }, 
                () =>
                {
                    PopupManager.ClosePopup<SystemPopup>();
                }, 
                () =>
                {
                    PopupManager.ClosePopup<SystemPopup>();
                });
                return;
            }

            ShopGoodsData goodsData = ShopGoodsData.Get(curKey);
            if (goodsData.REWARD_TYPE == ShopGoodsData.RewardType.DIRECT_REWARD)
            {
                List<Asset> AllItems = new List<Asset>();

                foreach (var data in ItemGroupData.Get(goodsData.REWARD_ID))
                {
                    AllItems.Add(data.Reward);
                }
                if (User.Instance.CheckInventoryGetItem(AllItems))
                {
                    IsFullBagAlert();
                    return;
                }

            }
            switch (goodsData.PRICE.GoodType)
            {
                case eGoodType.CASH:
                    IAPManager.Instance.TryPurchase(curKey,
                    (JToken response) =>
                    {
                        ItemGetProcess();
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
                    }, (JToken response) =>
                    {
                        ToastManager.On(StringData.GetStringByStrKey("구매불가"));
                        Debug.Log(response);
                    });
                    break;
                default:
                    WWWForm param = new WWWForm();
                    param.AddField("prod", curKey);
                    param.AddField("count", 1);
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
                                            if(PopupManager.IsPopupOpening(PopupManager.GetPopup<SystemRewardPopup>()))
                                            {
                                                PopupManager.GetPopup<SystemRewardPopup>().SetExitCallback(() =>
                                                SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(data["rewards"]), true))
                                                    );
                                            }
                                            else
                                            {
                                                SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(data["rewards"]), true));
                                            }
                                            
                                        }
                                        else if (goodsData.REWARD_TYPE == ShopGoodsData.RewardType.POST_REWARD)
                                        {
                                            ToastManager.On(100002506);//보상은 우편으로 지급됩니다.
                                        }
                                        ItemGetProcess();
                                    }
                                }
                                break;
                            }
                        }
                    });
                    break;
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
    }
}

