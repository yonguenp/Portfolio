using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandboxNetwork;
using Newtonsoft.Json.Linq;

public class ArenaTicketRechargePopup : Popup<PopupData>
{
    [SerializeField]
    private Text rechargeAmountLabel = null;
    [SerializeField]
    private Text rechargePriceLabel = null;
    [SerializeField]
    private Button rechargeButton = null;
    [SerializeField]
    private Image rechargeGoodsImg = null;

    Color normalLabelColor = Color.white;
    Color redLabelColor = Color.red;

    int rechargeCount = 0;
    int rechargePrice = 0;

    public delegate void CallBack();
    private CallBack successCallBack;

    public override void InitUI()
    {
        if(rechargeButton != null)
        {
            rechargeButton.SetButtonSpriteState(false);
        }

        rechargeAmountLabel.text = "0";
        ArenaManager.Instance.RequestUserArenaTicketRefillCount(() =>
        {
            rechargeCount = ArenaManager.Instance.UserArenaData.Arena_Ticket_refill_count;
            if(rechargeAmountLabel != null)
                rechargeAmountLabel.text = string.Format(StringData.GetStringByIndex(100001201), rechargeCount);

            RefreshButton();
        });
        
        rechargePrice = int.Parse(GameConfigTable.GetConfigValue("PVP_TICKET_RECHARGE_COST_NUM"));
        RefreshButton();
    }

    void RefreshButton()
    {
        var rechargeGoods = GameConfigTable.GetConfigValue("PVP_TICKET_RECHARGE_COST_TYPE");
        switch (rechargeGoods)
        {
            case "GEMSTONE":
                if (rechargePriceLabel != null)
                {
                    rechargePriceLabel.color = User.Instance.GEMSTONE < rechargePrice ? redLabelColor : normalLabelColor;
                    rechargePriceLabel.text = rechargePrice.ToString();
                }

                if (rechargeButton != null) {
                    bool isSufficient = rechargeCount > 0 && User.Instance.GEMSTONE >= rechargePrice;
                    rechargeButton.interactable = isSufficient;
                    rechargeButton.SetButtonSpriteState(isSufficient);
                }
                if(rechargeGoodsImg!= null)
                {
                    rechargeGoodsImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                }
                break;
            case "gold":
                if (rechargePriceLabel != null)
                {
                    rechargePriceLabel.color = User.Instance.GOLD < rechargePrice ? redLabelColor : normalLabelColor;
                    rechargePriceLabel.text = rechargePrice.ToString();
                }

                if (rechargeButton != null) {
                    bool isSufficient = rechargeCount > 0 && User.Instance.GOLD >= rechargePrice;
                    rechargeButton.interactable = isSufficient;
                    rechargeButton.SetButtonSpriteState(isSufficient);
                }
                if (rechargeGoodsImg != null)
                {
                    rechargeGoodsImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                }
                break;
        }

    }

    public void SetCallBack(CallBack cb)
    {
        if (cb != null)
        {
            successCallBack = cb;
        }
    }

    public void OnClickRechargeButton()
    {
        if (User.Instance.GOLD < rechargePrice)
        {
            //토스트 메세지 재화 부족!
            ToastManager.On(100000104);
            return;
        }

        ArenaManager.Instance.RechargeArenaTicket(this, () =>
        {
            if (successCallBack != null) successCallBack();
            PopupManager.ClosePopup<ArenaTicketRechargePopup>();
            // 토스트 메세지 -충전 완료
            ToastManager.On(100001199); //임시
        });
    }
    // Start is called before the first frame update
}
