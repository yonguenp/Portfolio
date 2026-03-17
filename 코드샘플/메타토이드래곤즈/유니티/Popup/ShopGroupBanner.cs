
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SandboxNetwork
{
    public class ShopGroupBanner : Popup<PopupBase>
    {
        [SerializeField]
        Text MenuTimer;
        [SerializeField]
        ShopBuyObjMedium CloneTarget;
        [SerializeField]
        Transform Parent;

        const int SHOP_GROUP_BANNER_KEY = 19;
        public static ShopMenuData GroupMenuData
        {
            get
            {
                return ShopMenuData.Get(SHOP_GROUP_BANNER_KEY);
            }
        }
        public static bool IsValid
        {
            get
            {
                if (GroupMenuData != null && GroupMenuData.IS_VALID)
                {
                    return !ShopManager.Instance.IsSoldOutMenu(SHOP_GROUP_BANNER_KEY);
                }
                return false;
            }
        }
        public override void InitUI()
        {
            Refresh();
        }

        public void Refresh()
        {
            RefreshTimer();

            foreach (Transform child in Parent)
            {
                Destroy(child.gameObject);
            }

            if (GroupMenuData != null)
            {
                int i = 0;
                foreach (var goods in GroupMenuData.ChildGoods)
                {
                    var obj = Instantiate(CloneTarget, Parent).GetComponent<ShopBuyObjMedium>();
                    var goodsStateData = ShopManager.Instance.GetGoodsState(int.Parse(goods.KEY));
                    if (goodsStateData.BaseData.USE)
                    {
                        obj.gameObject.SetActive(true);
                        obj.SetBuyLayer(i++, goods, goodsStateData.RemainGoodsCount, Refresh, null, goodsStateData.SubscribeState, goodsStateData.SubscribeDay);
                    }
                }
            }
        }
        public void RefreshTimer()
        {
            CancelInvoke("RefreshTimer");

            MenuTimer.text = "-";

            if (GroupMenuData == null)
                return;

            int time = TimeManager.GetTimeCompare(TimeManager.GetTimeStamp(GroupMenuData.END_TIME));
            if (time >= 0)
            {
                MenuTimer.text = StringData.GetStringFormatByIndex(100002626, SBFunc.TimeString(time));
                Invoke("RefreshTimer", 1.0f);
            }
        }
    }
}