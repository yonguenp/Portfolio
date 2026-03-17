using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMailIconObject : UIObject
{
    const eReddotEvent reddotType = eReddotEvent.POST_MAIL;
    static UIMailIconObject instance = null;
    private void Awake()
    {
        instance = this;
        ReddotUI.AddReddot(transform as RectTransform, reddotType, ReddotUI.Anchor.LEFT);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
    private void OnEnable()
    {
        ReddotRefresh();
    }

    public void ReddotRefresh()
    {
        CheckReddot();
    }

    public static void CheckReddot()
    {
        List<MailItem> mails = new List<MailItem>();
        PostListPopup popup = PopupManager.GetPopup<PostListPopup>();
        if (popup != null)
        {
            Refresh(popup.MailData);
        }
    }

    static void Refresh(List<MailItem> mails)
    {
        bool isOn = ReddotManager.IsOn(reddotType);

        var mailTable = TableManager.GetTable<ReservMailTable>();
        if (mailTable != null)
        {
            System.DateTime cur = TimeManager.GetDateTime();
            System.DateTime next = System.DateTime.MaxValue;
            foreach (ReservMailData data in mailTable.GetAllList())
            {
                if (data.CheckActivate(cur))
                    isOn = true;

                if (data.is_activated)
                {
                    if (data.send_start > cur)
                    {
                        if (data.send_start < next)
                        {
                            next = data.send_start;
                        }
                    }
                }
            }

            if (next != System.DateTime.MaxValue && next > cur && instance != null)
            {
                float sec = (float)(next - cur).TotalSeconds;
                instance.Invoke("ReddotRefresh", sec);
            }
        }

        if (mails != null && mails.Count > 0)
        {
            foreach(var val in mails)
            {
                if(!val.is_receive)
                {
                    ReddotManager.Set(reddotType, true);
                    return;
                }
            }
        }

        var reward_gem_id = GameConfigTable.GetConfigIntValue("AD_MAIL_REWARD_GEM");
        if (reward_gem_id > 0)
        {
            var goods = ShopManager.Instance.GetGoodsState(reward_gem_id);
            if (goods != null && goods.IS_VALIDE)
            {
                ReddotManager.Set(reddotType, true);
                return;
            }
        }

        var reward_item_id = GameConfigTable.GetConfigIntValue("AD_MAIL_REWARD_ITEM");
        if (reward_item_id > 0)
        {
            var goods = ShopManager.Instance.GetGoodsState(reward_item_id);
            if (goods != null && goods.IS_VALIDE)
            {
                ReddotManager.Set(reddotType, true);
                return;
            }
        }

        ReddotManager.Set(reddotType, isOn);
    }
}
