using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GachaMenuButton : MonoBehaviour
    {
        [SerializeField] Button gachaButton = null;
        [SerializeField] Image buttonImage = null;

        [Header("[Normal Layer]")]
        [SerializeField] GameObject normalLayerObject = null;
        [SerializeField] Image buttonPriceImage = null;
        [SerializeField] Text buttonDescText = null;
        [SerializeField] Text buttonPriceText = null;

        [Header("[CoolTime Layer]")]
        [SerializeField] GameObject coolTimeLayerObject = null;
        [SerializeField] TimeObject timeObject = null;
        [SerializeField] Text timeText = null;

        [Header("[Button Sprite]")]
        [SerializeField] Sprite singleGachaButtonSprite = null;
        [SerializeField] Sprite bundleGachaButtonSprite = null;
        [SerializeField] Sprite heavybundleGachaButtonSprite = null;
        [SerializeField] Sprite adGachaButtonSprite = null;
        [SerializeField] Sprite disableButtonSprite = null;

        GachaTypeData currentTypeData = null;
        AdvertisementData currentAdvData = null;

        public delegate void GachaButtonCallback(GachaTypeData typeData, string log = "");
        GachaButtonCallback gachaButtonCallback;
        VoidDelegate advWaitEndCB = null;

        bool isAvailGacha = false;

        bool isCoolTimeState = false;

        public bool IsAvailAdvCondition => currentAdvData != null && currentAdvData.CUR < currentAdvData.LIMIT; // 광고 시청 가능 상태 확인

        public void InitButton(GachaTypeData typeData, GachaButtonCallback callback, VoidDelegate advWaitTimeEndCallBack)
        {
            if (typeData == null) return;

            currentTypeData = typeData;
            if (currentTypeData.price_type == eGoodType.ADVERTISEMENT)
            {
                currentAdvData = AdvertisementData.Get(currentTypeData.price_uid);
            }

            gachaButtonCallback = callback;
            advWaitEndCB = advWaitTimeEndCallBack;

            buttonPriceImage.sprite = SBFunc.GetGoodTypeIcon(currentTypeData.price_type, currentTypeData.price_uid);
            
            // 버튼 텍스트 관련 처리
            if (currentTypeData.repeats == 1)
            {
                buttonImage.sprite = singleGachaButtonSprite;
            }
            else if (currentTypeData.repeats == 10)
            {
                buttonImage.sprite = bundleGachaButtonSprite;
            }
            else if (currentTypeData.repeats == 100)
            {
                buttonImage.sprite = heavybundleGachaButtonSprite;
            }
            buttonDescText.text = StringData.GetStringFormatByStrKey("gacha_btn_02", currentTypeData.repeats);

            RefreshMenuButton();
        }

        public void RefreshMenuButton(bool isRefreshCoolTime = true)
        {
            // 광고 쿨타임 관련 처리
            if (IsAvailAdvCondition && isRefreshCoolTime)
            {
                UpdateAdvCoolTime();
            }

            isAvailGacha = CheckAvailGachaState();

            if (isAvailGacha == false)
            {
                buttonImage.sprite = disableButtonSprite;
            }

            normalLayerObject.SetActive(!isCoolTimeState);
            coolTimeLayerObject.SetActive(isCoolTimeState);
        }

        public void OnClickGachaButton()
        {
            if (isAvailGacha)
            {
                if ((eGoodType)currentTypeData.price_type == eGoodType.ADVERTISEMENT)
                {
                    var gachaRateDat = currentTypeData.Rate;
                    int requireInvenSpace = 0;
                    foreach (var dat in gachaRateDat)
                    {
                        if (dat.reward_type == "GACHA_GROUP")
                        {
                            var gachaDats = GachaRateData.GetGroup(dat.result_id);
                            foreach (var gachaDat in gachaDats)
                            {
                                requireInvenSpace += GachaItemCnt(gachaDat);
                            }
                        }
                        else if (dat.reward_type == "DICE_GROUP")
                        {
                            requireInvenSpace += GachaItemCnt(dat);
                        }
                    }
                    if (User.Instance.Inventory.GetEmptySlotCount() < requireInvenSpace)
                    {
                        IsFullBagAlert();
                        return;
                    }
                    AdvertiseManager.Instance.TryADWithPopup((log) =>
                    {
                        gachaButtonCallback?.Invoke(currentTypeData, log);
                    },
                    ()=> {
                        ToastManager.On(StringData.GetStringByStrKey("ad_empty_alert"));
                    });
                }
                else
                {
                    gachaButtonCallback?.Invoke(currentTypeData);
                }
            }
            else if (isCoolTimeState)
            {
                ToastManager.On(StringData.GetStringByStrKey("adv_cooltime"));
            }
            else
            {
                ShopMenuData target = null;
                ShopPopup shop = PopupManager.GetPopup<ShopPopup>();
                if (shop != null)
                {
                    int price = 0;
                    foreach (var tab in shop.GetShopTab())
                    {
                        if (tab.IS_VALID)
                        {
                            foreach (var goods in tab.ChildGoods)
                            {
                                if (!goods.IS_VALIDE)
                                    continue;

                                if (price > goods.PRICE.Amount)
                                    continue;

                                foreach (var reward in goods.REWARDS)
                                {
                                    if (reward.GoodType == currentTypeData.price_type)
                                    {
                                        switch(currentTypeData.price_type)
                                        {
                                            case eGoodType.GOLD:
                                                target = tab;
                                                price = goods.PRICE.Amount;
                                                break;
                                            case eGoodType.GEMSTONE:
                                                target = tab;
                                                price = goods.PRICE.Amount;
                                                break;
                                            case eGoodType.ENERGY:
                                                target = tab;
                                                price = goods.PRICE.Amount;
                                                break;
                                            case eGoodType.ITEM:
                                            default:
                                                if (reward.ItemNo == currentTypeData.price_uid)
                                                {
                                                    target = tab;
                                                    price = goods.PRICE.Amount;
                                                }
                                                break;
                                        }
                                        
                                    }
                                }

                            }
                        }
                    }
                }

                if(target != null)
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("gacha_item_shortage_alert_title"), StringData.GetStringByStrKey("gacha_item_shortage_alert_content"),
                        () =>
                        {
                            PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(target.KEY));
                        },
                        () => {
                            //cancel
                        }, null
                    );
                }
                else
                {
                    ToastManager.On(StringData.GetStringByStrKey("gacha_empty_ticket"));//가지고 있는 티켓이 부족 합니다.
                }
            }
        }

        int GachaItemCnt(GachaRateData gachaDat)
        {
            int cnt = 0;
            if (gachaDat.reward_type == "DICE_GROUP")
            {
                var itemDiceGroup = ItemGroupData.Get(gachaDat.result_id);
                foreach (var itemGroup in itemDiceGroup)
                {
                    bool isItemExist = false;
                    foreach (var items in itemGroup.Child)
                    {
                        if (items.Reward.GoodType == eGoodType.ITEM)
                            isItemExist = true;
                    }
                    if (isItemExist)
                        ++cnt;
                }
            }
            return cnt;
        }
        void IsFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    PopupManager.OpenPopup<InventoryPopup>();
                }, () => { }, () => { });
        }
        bool CheckAvailGachaState()
        {
            // 뽑기 가능 상태 확인
            bool isAvailContidion = false;
            switch (currentTypeData.price_type)
            {
                case eGoodType.ITEM:
                    buttonPriceText.text = StringData.GetStringFormatByStrKey("장", currentTypeData.price_value);

                    isAvailContidion = User.Instance.GetItemCount(currentTypeData.price_uid) >= currentTypeData.price_value;
                    break;
                case eGoodType.ADVERTISEMENT:
                    buttonImage.sprite = adGachaButtonSprite;
                    buttonPriceText.text = StringData.GetStringByStrKey("gacha_watch_ads");

                    isAvailContidion = IsAvailAdvCondition;
                    break;
                case eGoodType.GEMSTONE:
                    buttonPriceText.text = StringData.GetStringFormatByStrKey("production_quantity", currentTypeData.price_value);

                    isAvailContidion = User.Instance.GEMSTONE >= currentTypeData.price_value;
                    break;
            }

            //gachaButton.SetInteractable(isAvailGacha);

            // 뽑기 가능 기간 확인
            bool isAvailPeriod = false;
            var gachaMenuData = GachaMenuData.Get(currentTypeData.menu_id);
            if (gachaMenuData != null)
            {
                isAvailPeriod = gachaMenuData.IsValid();
            }

            return isAvailContidion && isAvailPeriod && !isCoolTimeState;
        }

        void UpdateAdvCoolTime()
        {
            if (timeObject == null || currentAdvData == null) return;

            var advInfo = ShopManager.Instance.GetAdvertiseState(int.Parse(currentAdvData.KEY));
            if (advInfo != null)
            {
                timeObject.Refresh = () =>
                {
                    var remain = advInfo.Remain;
                    if (remain > 0)
                    {
                        isCoolTimeState = true;
                        timeText.text = SBFunc.TimeCustomString(remain, 2);
                    }
                    else
                    {
                        isCoolTimeState = false;
                        timeText.text = SBFunc.TimeString(0);
                        timeObject.Refresh = null;
                        advWaitEndCB?.Invoke();
                        RefreshMenuButton(false);
                    }
                };
            }
        }
    }
}