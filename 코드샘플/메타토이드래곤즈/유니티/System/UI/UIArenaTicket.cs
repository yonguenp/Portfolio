using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class UIArenaTicket : UIObject
    {
        [SerializeField]
        private TimeObject ticketTimeObj = null;
        [SerializeField]
        private Text ticketAmountLabel = null;
        [SerializeField]
        private GameObject timeBubble = null;
        [SerializeField]
        private Text ticketChargeTimeLabel = null;
        [SerializeField]
        public delegate void CallBack();
        CallBack timeEndCallback;
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            RefreshTicketTime();
        }
        public override bool RefreshUI(eUIType targetType)
        {
            base.RefreshUI(targetType);
            RefreshTicketTime();

            return curSceneType != targetType;
        }
        public void setCallBack(CallBack cb)
        {
            if(cb != null) { 
                timeEndCallback = cb;
            }
        }
        public void onClickArenaTicket()
        {
            var shop = PopupManager.GetPopup<ShopPopup>();
            if (PopupManager.IsPopupOpening(shop))
            {
                return;
            }

            PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(14));

            //var popup = PopupManager.OpenPopup<ArenaTicketRechargePopup>();
            //popup.SetCallBack(()=> {
            //    RefreshTicketTime(); 
            //    if(timeEndCallback != null) { 
            //        timeEndCallback();
            //    }
            //});
        }

        private void OnEnable()
        {
            ticketAmountLabel.text = string.Format("{0} / {1}", ArenaManager.Instance.UserArenaData.Arena_Ticket, GameConfigTable.GetArenaUserMaxTicketCount());
        }


        public void RefreshTicketTime()
        {
            if (ticketAmountLabel == null) return;
            int exp = ArenaManager.Instance.GetNextArenaEnergyExpire().Arena_Ticket_Exp;
            ticketAmountLabel.text = string.Format("{0} / {1}", ArenaManager.Instance.UserArenaData.Arena_Ticket, GameConfigTable.GetArenaUserMaxTicketCount());
            timeBubble.SetActive(exp > 0);
            Transform ticketChargeTimeLabelParent = ticketChargeTimeLabel.transform.parent;
            if (ticketChargeTimeLabelParent != null)
            {
                ticketChargeTimeLabelParent.gameObject.SetActive(exp > 0);
            }
            if (exp > 0)
            {
                if (ticketTimeObj.Refresh == null)
                {
                    ArenaManager.Instance.SetTimeObject(ticketTimeObj);
                    ticketTimeObj.Refresh = () =>
                    {
                        float remain = TimeManager.GetTimeCompare(exp);
                        //ticketChargeTimeLabel.text = SBFunc.TimeString(remain);
                        ticketChargeTimeLabel.text = SBFunc.TimeString(remain);
                        if (0 >= remain)
                        {
                            ticketTimeObj.Refresh = null;
                            ArenaManager.Instance.DeleteTimeObject(ticketTimeObj);
                            NetworkManager.Send("user/arenaticket", null, (JObject jsonData) =>
                            {
                                ArenaManager.Instance.UserArenaData.SetUserTicketInfo(jsonData["arena_ticket"].Value<int>(), jsonData["arena_ticket_tick"].Value<int>());
                                RefreshTicketTime();
                                timeEndCallback?.Invoke();
                            });
                        }
                    };
                }
            }
        }
    }
}
