using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 
    public class ArenaMatchListRefreshPopup : Popup<PopupData>
    {
        [SerializeField]
        Text priceAmountLabel = null;
        [SerializeField]
        Button priceButton = null;
        [SerializeField]
        Image priceIcon = null;

        [SerializeField]
        Text advertiseLabel = null;
        [SerializeField]
        Button advertiseButton = null;

        int refreshPrice=0;


        ArenaManager.Callback cb = null;
        public override void InitUI()
        {
            RefreshButton();
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        public void SetCallBack(ArenaManager.Callback CallBack)
        {
            if (CallBack != null)
            {
                cb = CallBack;
            }
        }

        void RefreshButton()
        {
            refreshPrice = int.Parse(GameConfigTable.GetConfigValue("PVP_FREE_LIST_RESET_COST_NUM"));
            var itemType = GameConfigTable.GetConfigValue("PVP_FREE_LIST_RESET_COST_TYPE");
            switch (itemType)
            {
                case "GEMSTONE":
                    bool isSufficient = User.Instance.GEMSTONE >= refreshPrice;
                    if (priceAmountLabel != null)
                    {
                        priceAmountLabel.text = refreshPrice.ToString();
                        priceAmountLabel.color = User.Instance.GEMSTONE < refreshPrice ? Color.red : Color.white;   
                    }
                    if( priceIcon != null)
                    {
                        priceIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                    }
                    if (priceButton != null)
                    {
                        priceButton.interactable = isSufficient;
                        priceButton.SetButtonSpriteState(isSufficient);
                    }
                        
                    break;
                case "gold":
                    bool isSufficientGold = User.Instance.GOLD >= refreshPrice;
                    if (priceAmountLabel != null)
                    {
                        priceAmountLabel.text = refreshPrice.ToString();
                        priceAmountLabel.color = User.Instance.GOLD < refreshPrice ? Color.red : Color.white;
                    }
                    if (priceIcon != null)
                    {
                        priceIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                    }
                    if (priceButton != null)
                    {
                        priceButton.interactable = isSufficientGold;
                        priceButton.SetButtonSpriteState(isSufficientGold);
                    }
                    break;
            }

            var adv_id = GameConfigTable.GetConfigIntValue("AD_PVP_LIST_RESET");
            if (adv_id > 0)
            {
                var adInfo = ShopManager.Instance.GetAdvertiseState(adv_id);
                if (adInfo != null)
                {
                    if(adInfo.VIEW_COUNT < adInfo.VIEW_LIMIT)
                    {
                        advertiseButton.gameObject.SetActive(true);
                        if(adInfo.IS_VALIDE)
                        {
                            advertiseButton.interactable = true;
                            advertiseLabel.text = StringData.GetStringByStrKey("광고보기") + "\n" + (adInfo.VIEW_LIMIT - adInfo.VIEW_COUNT).ToString() + "/" + adInfo.VIEW_LIMIT;
                        }
                        else
                        {
                            advertiseButton.interactable = false;
                            advertiseLabel.text = SBFunc.TimeString(adInfo.Remain);
                        }
                    }
                    else
                    {
                        advertiseButton.gameObject.SetActive(false);
                    }
                }
                else
                {
                    advertiseButton.gameObject.SetActive(false);
                }
            }
            else
            {
                advertiseButton.gameObject.SetActive(false);
            }

            Invoke("SetTimer", 1.0f);
        }

        public void SetTimer()
        {
            CancelInvoke("SetTimer");

            var adv_id = GameConfigTable.GetConfigIntValue("AD_PVP_LIST_RESET");
            if (adv_id > 0)
            {
                var adInfo = ShopManager.Instance.GetAdvertiseState(adv_id);
                if (adInfo != null)
                {
                    if (adInfo.VIEW_COUNT < adInfo.VIEW_LIMIT)
                    {
                        advertiseButton.gameObject.SetActive(true);
                        if (adInfo.IS_VALIDE)
                        {
                            advertiseButton.interactable = true;
                            advertiseLabel.text = StringData.GetStringByStrKey("광고보기") + "\n" + (adInfo.VIEW_LIMIT - adInfo.VIEW_COUNT).ToString() + "/" + adInfo.VIEW_LIMIT;
                            return;
                        }
                        else
                        {
                            advertiseButton.interactable = false;
                            advertiseLabel.text = SBFunc.TimeString(adInfo.Remain);
                        }
                    }
                    else
                    {
                        advertiseButton.gameObject.SetActive(false);
                    }
                }
                else
                {
                    advertiseButton.gameObject.SetActive(false);
                }
            }
            else
            {
                advertiseButton.gameObject.SetActive(false);
            }

            Invoke("SetTimer", 1.0f);
        }

        public void OnClickRechargeBtn()
        {
            if (User.Instance.GOLD< refreshPrice)
            {
                ToastManager.On(100000104);// 토스트 메세지 재화 부족
                return;
            }
            ArenaManager.Instance.RequestNewMatchList(cb);
        }

        public void OnClickAdvertiseBtn()
        {
            AdvertiseManager.Instance.TryADWithPopup((log) =>
            {
                ArenaManager.Instance.RequestNewMatchList(cb, true, log);
            }, ()=> { ToastManager.On(StringData.GetStringByStrKey("ad_empty_alert")); });
        }
    }

}