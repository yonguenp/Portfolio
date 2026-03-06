using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIShopObject : UIObject
{
    const eReddotEvent reddotType = eReddotEvent.SHOP;

    private void OnEnable()
    {
        CheckReddot();
    }
    public static void CheckReddot()
    {
        Refresh();
    }

    public static void Refresh()
    {
        // 메뉴 구매 버튼 관련
        var menus = ShopMenuData.GetShopMenus(eStoreType.SHOP);
        foreach (var menu in menus)
        {
            if(menu.IsMenuReddot())
            {
                ReddotManager.Set(reddotType, true);
                return;
            }
        }

        // 첫구매 관련
        var privateGoodNums = ShopManager.Instance.PrivateGoods.Keys.ToArray();

        for (int i = 0, count = privateGoodNums.Length; i < count; ++i)
        {
            var Data = ShopBannerData.Get(privateGoodNums[i].ToString());

            switch (Data.TYPE)
            {
                case BANNER_TYPE.SMALL:
                    if (ShopManager.Instance.GetGoodsState(int.Parse(Data.KEY)).RemainGoodsCount > 0)
                    {
                        ReddotManager.Set(reddotType, true);
                    }
                    return;
            }
        }

        ReddotManager.Set(reddotType, false);
    }
}
