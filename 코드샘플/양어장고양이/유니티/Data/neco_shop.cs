using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_shop : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_SHOP; }

    static public List<neco_shop> GetNecoShopListByType(string type)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SHOP);
        if (necoData == null)
        {
            return null;
        }

        List<neco_shop> shopList = new List<neco_shop>();

        object obj;
        foreach (neco_shop shopData in necoData)
        {
            if (shopData.GetNecoShopProductType() == type)
            {
                shopList.Add(shopData);
            }
        }

        return shopList;
    }

    static public neco_shop GetNecoShopData(uint shopId)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SHOP);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_shop shopData in necoData)
        {
            if (shopData.GetNecoShopID() == shopId)
            {
                return shopData;
            }
        }

        return null;
    }

    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SHOP);
        if (necoData == null)
        {
            return;
        }

        foreach (neco_shop shopData in necoData)
        {
            shopData.necoShopProductDesc = "";
            shopData.necoShopDetail = "";
            shopData.necoShopName = "";
        }
    }

    [NonSerialized]
    private uint necoShopID = 0;
    public uint GetNecoShopID()
    {
        if (necoShopID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoShopID = (uint)obj;
            }
        }

        return necoShopID;
    }

    [NonSerialized]
    private string necoShopDesc = "";
    public string GetNecoShopDesc()
    {
        if (necoShopDesc == "")
        {
            object obj;
            if (data.TryGetValue("desc", out obj))
            {
                necoShopDesc = (string)obj;
            }
        }

        return necoShopDesc;
    }

    [NonSerialized]
    private string necoShopName = "";
    public string GetNecoShopName()
    {
        if (necoShopName == "")
        {
            necoShopName = LocalizeData.GetText("neco_shop:name:" + GetNecoShopID().ToString());
        }

        return necoShopName;
    }

    [NonSerialized]
    private string necoShopProductDesc = "";
    public string GetNecoShopProductDesc()
    {
        if (necoShopProductDesc == "")
        {
            necoShopProductDesc = LocalizeData.GetText("neco_shop:product_desc:" + GetNecoShopID().ToString());
            //object obj;
            //if (data.TryGetValue("product_desc", out obj))
            //{
            //    necoShopProductDesc = (string)obj;
            //}
        }

        return necoShopProductDesc.Replace("\\n", "\n");
    }

    [NonSerialized]
    private string necoShopDetail = "";
    public string GetNecoShopDetail()
    {
        if (necoShopDetail == "")
        {
            //object obj;
            //if (data.TryGetValue("detail", out obj))
            //{
            //    necoShopDetail = (string)obj;
            //}
            necoShopDetail = LocalizeData.GetText("neco_shop:detail:" + GetNecoShopID().ToString());
        }

        return necoShopDetail.Replace("\\n", "\n");
    }

    [NonSerialized]
    private string necoShopProductType = "";
    public string GetNecoShopProductType()
    {
        if (necoShopProductType == "")
        {
            object obj;
            if (data.TryGetValue("product_type", out obj))
            {
                necoShopProductType = (string)obj;
            }
        }

        return necoShopProductType;
    }

    [NonSerialized]
    private string necoShopGoodsType = "";
    public string GetNecoShopGoodsType()
    {
        if (necoShopGoodsType == "")
        {
            object obj;
            if (data.TryGetValue("goods_type", out obj))
            {
                necoShopGoodsType = (string)obj;
            }
        }

        return necoShopGoodsType;
    }

    [NonSerialized]
    private uint necoShopGoodsID = 0;
    public uint GetNecoShopGoodsID()
    {
        if (necoShopGoodsID == 0)
        {
            object obj;
            if (data.TryGetValue("goods_id", out obj))
            {
                necoShopGoodsID = (uint)obj;
            }
        }

        return necoShopGoodsID;
    }

    [NonSerialized]
    private string necoShopPriceType = "";
    public string GetNecoShopPriceType()
    {
        if (necoShopPriceType == "")
        {
            object obj;
            if (data.TryGetValue("price_typeColumn1", out obj))
            {
                necoShopPriceType = (string)obj;
            }
        }

        return necoShopPriceType;
    }

    [NonSerialized]
    private uint necoShopPrice = 0;
    public uint GetNecoShopPrice()
    {
        if (necoShopPrice == 0)
        {
            object obj;
            if (data.TryGetValue("price", out obj))
            {
                necoShopPrice = (uint)obj;
            }
        }

        return necoShopPrice;
    }

    [NonSerialized]
    private string necoShopPurchaseType = "";
    public string GetNecoShopPurchaseType()
    {
        if (necoShopPurchaseType == "")
        {
            object obj;
            if (data.TryGetValue("purchase_type", out obj))
            {
                necoShopPurchaseType = (string)obj;
            }
        }

        return necoShopPurchaseType;
    }

    [NonSerialized]
    private uint necoShopPurchaseLimit = 0;
    public uint GetNecoShopPurchaseLimit()
    {
        if (necoShopPurchaseLimit == 0)
        {
            object obj;
            if (data.TryGetValue("purchase_limit", out obj))
            {
                necoShopPurchaseLimit = (uint)obj;
            }
        }

        return necoShopPurchaseLimit;
    }

    [NonSerialized]
    private string necoShopResPath = "";
    public string GetNecoShopPackageResource()
    {
        if (necoShopResPath == "")
        {
            object obj;
            if (data.TryGetValue("package_resource", out obj))
            {
                necoShopResPath = (string)obj;
            }
        }

        return necoShopResPath;
    }

    [NonSerialized]
    Sprite necoShopIcon = null;
    public Sprite GetNecoShopIcon()
    {
        if (necoShopIcon == null)
        {
            object obj;
            if (data.TryGetValue("package_resource", out obj))
            {
                necoShopIcon = Resources.Load<Sprite>((string)obj);
            }
        }

        return necoShopIcon;
    }

    public Sprite GetNecoShopPriceIcon()
    {
        Sprite priceIcon = null;

        object obj;
        if (data.TryGetValue("price_typeColumn1", out obj))
        {
            switch ((string)obj)
            {
                case "dia":
                    priceIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
                    break;
                case "gold":
                    priceIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_coin");
                    break;
                case "point":
                    priceIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_point");
                    break;
            }
        }

        return priceIcon;
    }

    public string GetPurchaseTypeString()
    {
        string result = "";

        object obj;
        if (data.TryGetValue("purchase_type", out obj))
        {
            switch ((string)obj)
            {
                case "account":
                    result = LocalizeData.GetText("LOCALIZE_186");
                    break;
                case "weekly":
                    result = LocalizeData.GetText("LOCALIZE_187");
                    break;
                case "daily":
                    result = LocalizeData.GetText("LOCALIZE_487");
                    break;
                case "subscribe":
                    result = LocalizeData.GetText("기간별구매제한");
                    break;
                case "none":
                    result = LocalizeData.GetText("구매횟수제한없음");
                    break;
            }
        }

        return result;
    }

    [NonSerialized]
    private DateTime start_date = DateTime.MaxValue;
    [NonSerialized]
    private DateTime end_date = DateTime.MinValue;

    public DateTime GetStartDate()
    {
        if (start_date == DateTime.MaxValue)
        {
            object obj;
            if (data.TryGetValue("sale_start_date", out obj))
            {
                if (obj != null)
                {
                    start_date = DateTime.Parse((string)obj);
                }
            }
        }

        return start_date;
    }

    public DateTime GetEndDate()
    {
        if (end_date == DateTime.MaxValue)
        {
            object obj;
            if (data.TryGetValue("sale_end_date", out obj))
            {
                end_date = DateTime.Parse((string)obj);
            }
        }

        return end_date;
    }

    [NonSerialized]
    private uint necoShopOrder = uint.MaxValue;
    public uint GetNecoShopOrder()
    {
        if (necoShopOrder == uint.MaxValue)
        {
            object obj;
            if (data.TryGetValue("order", out obj))
            {
                necoShopOrder = (uint)obj != 0 ? (uint)obj : uint.MaxValue;
            }
        }

        return necoShopOrder;
    }

    public string GetIAPConstants()
    {
        switch(GetNecoShopID())
        {
            case 1:
                return EasyMobile.EM_IAPConstants.Product_hahaha_pkg_1_mhouse;
            case 2:
                return EasyMobile.EM_IAPConstants.Product_hahaha_pkg_2_mcar;
            case 3:
                return EasyMobile.EM_IAPConstants.Product_hahaha_pkg_3_starter;
            case 4:
                return EasyMobile.EM_IAPConstants.Product_hahaha_pkg_4_gold;
            case 5:
                return EasyMobile.EM_IAPConstants.Product_hahaha_pkg_5_cook;

            case 9:
                return EasyMobile.EM_IAPConstants.Product_hahaha_catnip_330;
            case 10:
                return EasyMobile.EM_IAPConstants.Product_hahaha_catnip_1100;
            case 11:
                return EasyMobile.EM_IAPConstants.Product_hahaha_catnip_5500;
            case 12:
                return EasyMobile.EM_IAPConstants.Product_hahaha_catnip_11000;

            case 24:
                return EasyMobile.EM_IAPConstants.Product_mini_package;
            case 34:
                return EasyMobile.EM_IAPConstants.Product_lv4_package;

            case 35:
                return EasyMobile.EM_IAPConstants.Product_daily_photo_package;
            case 36:
                return EasyMobile.EM_IAPConstants.Product_lv5_package;
            case 37:
                return EasyMobile.EM_IAPConstants.Product_lv6_package;
            case 38:
                return EasyMobile.EM_IAPConstants.Product_happy_subscription;
            case 39:
                return EasyMobile.EM_IAPConstants.Product_performance_package;

            case 40:
                return EasyMobile.EM_IAPConstants.Product_chuseok_package;
            case 41:
                return EasyMobile.EM_IAPConstants.Product_economical_package;
            case 42:
                return EasyMobile.EM_IAPConstants.Product_lv7_package;
            case 43:
                return EasyMobile.EM_IAPConstants.Product_lv8_package;
            case 44:
                return EasyMobile.EM_IAPConstants.Product_chuseok_dice_package;

            case 45:
                return EasyMobile.EM_IAPConstants.Product_lv9_package;
            case 46:
                return EasyMobile.EM_IAPConstants.Product_repair_package;

            case 51:
                return EasyMobile.EM_IAPConstants.Product_special_material_package;
            case 52:
                return EasyMobile.EM_IAPConstants.Product_catnipfarm_package;
            case 53:
                return EasyMobile.EM_IAPConstants.Product_halloween_package;
            case 54:
                return EasyMobile.EM_IAPConstants.Product_premium_starter_package;
            case 55:
                return EasyMobile.EM_IAPConstants.Product_new_mini_package;

            
            case 57:
                return EasyMobile.EM_IAPConstants.Product_xmasbox10_package;
            case 58:
                return EasyMobile.EM_IAPConstants.Product_xmasbox22_package;
            case 59:
                return EasyMobile.EM_IAPConstants.Product_xmasbox50_package;
            case 60:
                return EasyMobile.EM_IAPConstants.Product_xmas_package;
        }

        return "";
    }
}
