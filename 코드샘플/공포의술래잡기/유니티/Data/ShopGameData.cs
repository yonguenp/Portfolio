using EasyMobile;
using Newtonsoft.Json.Linq;
using SBCommonLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenuGameData : GameData
{
    public enum PAGE_TYPE
    {
        UNKNOWN = 0,
        COLUMN_3 = 1,
        COLUMN_4 = 2,
        RANDOM = 3,
        PACKAGE_COLUMN_3 = 4,
        SOULCARD_TRADE = 5,
        EXCHANGE_EVENT = 6,
        EQUIPMENT_TRADE= 7,
    }

    public enum UI_TYPE
    {
        NONE = 0,
        GOLD = 1 << 0,//1
        DIA = 1 << 1,//2
        MILEAGE = 1 << 2,//4
        SOULSTONE = 1 << 3, //8
    }

    public int priority { get; private set; }
    public int shop_type { get; private set; }
    public PAGE_TYPE pageType { get; private set; }
    public UI_TYPE assetType { get; private set; }
    public DateTime start_event { get; private set; }
    public DateTime end_event { get; private set; }
    public string resource { get; private set; }
    public bool use { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);
        
        priority = Int(data["priority"]);
        shop_type = Int(data["shop_type"]);
        pageType = (PAGE_TYPE)Int(data["page_type"]);
        assetType = (UI_TYPE)Int(data["asset_type"]);

        if (data["start_event"] == "always")
            start_event = DateTime.MinValue;
        else
            start_event = DateTime.Parse(data["start_event"]);
        if (data["end_event"] == "always")
            end_event = DateTime.MaxValue;
        else
            end_event = DateTime.Parse(data["end_event"]);
        resource = data["resource"];
        use = Int(data["use"]) > 0;
    }

    static public List<GameData> GetShopMenuListWithSort()
    {
        List<GameData> datas = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu);
        List<GameData> list = new List<GameData>();
        foreach (ShopMenuGameData data in datas)
        {
            if (data.use)
            {
                if (data.shop_type == 1)
                    list.Add(data);
            }
        }

        list.Sort((a, b) =>
        {
            return ((ShopMenuGameData)a).priority.CompareTo(((ShopMenuGameData)b).priority);//맘에안든다.. 한번하고 캐싱하게할까.
        });

        return list;
    }

    static public List<GameData> GetTraderMenuListWithSort()
    {
        List<GameData> datas = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu);
        List<GameData> list = new List<GameData>();
        foreach (ShopMenuGameData data in datas)
        {
            if (data.use)
            {
                if (data.shop_type == 2 && data.end_event >= SBUtil.KoreanTime)
                    list.Add(data);
            }
        }

        list.Sort((a, b) =>
        {
            return ((ShopMenuGameData)a).priority.CompareTo(((ShopMenuGameData)b).priority);//맘에안든다.. 한번하고 캐싱하게할까.
        });

        return list;
    }
}

public class ShopItemGameData : GameData
{
    public int menu_id { get; private set; }
    public int priority { get; private set; }
    public ShopItem price { get; private set; }
    
    public DateTime startTime { get; private set; }
    public DateTime endTime { get; private set; }
    public int buyLimit { get; private set; }
    public int buyType { get; private set; }
    public bool use { get; private set; }
    public string resource_path { get; private set; } = "";
    public Sprite resource { get; private set; } = null;
    public List<ShopPackageGameData> rewards
    {
        get
        {
            if (results == null)
                results = ShopPackageGameData.GetRewardDataList(result_id);

            return results;
        }
    }

    int result_id = 0;
    List<ShopPackageGameData> results = null;
    private Coroutine resourceSyncCoroutine = null;

    public Dictionary<int, SubscriptionItem> SubScriptionItems { get; private set; } = new Dictionary<int, SubscriptionItem>();
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        menu_id = Int(data["menu_id"]);
        priority = Int(data["priority"]);
        price = new ShopItem(Int(data["price_type"]), Int(data["price_param"]), Int(data["price_amount"]));

        buyType = Int(data["buy_type"]);
        buyLimit = Int(data["buy_limit"]);

        if (data["start_time"] == "always")
            startTime = DateTime.MinValue;
        else
        {
            DateTime.TryParse(data["start_time"], out DateTime time);
            startTime = time;
        }

        if (data["end_time"] == "always")
            endTime = DateTime.MaxValue;
        else
        {
            DateTime.TryParse(data["end_time"], out DateTime time);
            endTime = time;
        }

        resource = null;

        if (data.ContainsKey("resource") && !string.IsNullOrEmpty(data["resource"]))
        {
            resource = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/" + data["resource"]);
            if(resource == null)
            {
                string[] strPath = data["resource"].Split('/');
                if(strPath.Length >= 2 && strPath[0] == "banners")
                {
                    resource_path = data["resource"];
                }
            }
        }

        if (resource == null)
        {
            resource = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_loading");
        }

        result_id = Int(data["result_id"]);
        use = Int(data["use"]) > 0;
    }
    public void ApplySprite(Image target)
    {
        if (!string.IsNullOrEmpty(resource_path))
        {
            if (resourceSyncCoroutine == null)
            {
                resourceSyncCoroutine = Managers.Instance.StartCoroutine(ResourceSync(target));
            }
            return;
        }

        target.sprite = resource;
    }
    System.Collections.IEnumerator ResourceSync(Image target)
    {
        if (!target.IsDestroyed())
        {
            target.sprite = null;
            Color color = Color.white;
            color.a = 0.0f;
            target.color = color;
        }

        if (File.Exists(ClientConstants.AssetBundleDownloadPath + "shop/" + resource_path + ".png"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(ClientConstants.AssetBundleDownloadPath + "shop/" + resource_path + ".png"));

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            resource = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));

            resource_path = "";
        }
        else
        {
            UnityEngine.Networking.UnityWebRequest wr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(SBWeb.CDN_URL + resource_path + ".png");

            UnityEngine.Networking.DownloadHandlerTexture texDI = new UnityEngine.Networking.DownloadHandlerTexture(true);
            wr.downloadHandler = texDI;

            yield return wr.SendWebRequest();

            if (wr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                SBDebug.LogError("상점 배너 다운로드 오류!!!!!!!!!!!!!!! : " + resource_path + ".png");
            }
            else
            {
                if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath))
                {
                    Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath);
                }

                if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath + "shop/"))
                {
                    Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath + "shop/");
                }

                string[] folders = resource_path.Split('/');
                string folder = "";
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    folder += folders[i] + "/";
                    if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath + "shop/" + folder))
                    {
                        Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath + "shop/" + folder);
                    }
                }

                File.WriteAllBytes(ClientConstants.AssetBundleDownloadPath + "shop/" + resource_path + ".png", texDI.data);

                Rect rect = new Rect(0, 0, texDI.texture.width, texDI.texture.height);
                resource = Sprite.Create(texDI.texture, rect, new Vector2(0.5f, 0.5f));


                resource_path = "";
            }
        }

        if (!target.IsDestroyed())
        {
            target.sprite = resource;
            target.color = Color.white;
        }

        resourceSyncCoroutine = null;
    }

    static public List<GameData> GetShopItemWithSort(int menuID)
    {
        List<GameData> list = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_goods);
        List<GameData> ret = new List<GameData>();
        List<GameData> limitOverItems = new List<GameData>();


        foreach (ShopItemGameData data in list)
        {
            if (!data.use)
                continue;

            if (data.menu_id == menuID && data.IsDateValid())
            {
                bool enable = false;

                bool advertiseMenu = false;

                switch (data.GetID())
                {
                    case 1001:
                        {
                            advertiseMenu = true;
                            DateTime pivot = System.DateTime.MaxValue;
                            DateTime ableTime = pivot;
                            pivot = Managers.UserData.ADSeen_PACK1;
                            if (pivot < System.DateTime.MaxValue)
                            {
                                ableTime = pivot.AddHours(4);//.AddDays(1);
                                //ableTime = ableTime.AddHours((ableTime.Hour * -1) + 4).AddMinutes((ableTime.Minute * -1)).AddSeconds((ableTime.Second * -1));
                            }

                            enable = pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                            int cnt = Managers.UserData.GetMyShopHistory(data.GetID());
                            if (enable && cnt > 0)
                            {
                                Managers.UserData.UpdateMyShopInfo(data.GetID(), (cnt * -1));//0으로 만들기
                            }
                            if(!enable && cnt <= 0)
                            {
                                Managers.UserData.UpdateMyShopInfo(data.GetID(), (cnt * -1) + 1);//1로 만들기
                            }
                        }
                        break;
                    case 1002:
                        {
                            advertiseMenu = true;
                            DateTime pivot = System.DateTime.MaxValue;
                            DateTime ableTime = pivot;
                            pivot = Managers.UserData.ADSeen_PACK2;
                            if (pivot < System.DateTime.MaxValue)
                            {
                                ableTime = pivot.AddHours(4);//.AddDays(1);
                                //ableTime = ableTime.AddHours((ableTime.Hour * -1) + 4).AddMinutes((ableTime.Minute * -1)).AddSeconds((ableTime.Second * -1));
                            }

                            enable = pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                            int cnt = Managers.UserData.GetMyShopHistory(data.GetID());
                            if (enable && cnt > 0)
                            {
                                Managers.UserData.UpdateMyShopInfo(data.GetID(), (cnt * -1));//0으로 만들기
                            }
                            if (!enable && cnt <= 0)
                            {
                                Managers.UserData.UpdateMyShopInfo(data.GetID(), (cnt * -1) + 1);//1로 만들기
                            }
                        }
                        break;
                    case 1003:
                        {
                            advertiseMenu = true;
                            DateTime pivot = System.DateTime.MaxValue;
                            DateTime ableTime = pivot;
                            pivot = Managers.UserData.ADSeen_PACK3;
                            if (pivot < System.DateTime.MaxValue)
                                ableTime = pivot.AddMonths(1);

                            enable = pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                            int cnt = Managers.UserData.GetMyShopHistory(data.GetID());
                            if (enable && cnt > 0)
                            {
                                Managers.UserData.UpdateMyShopInfo(data.GetID(), (cnt * -1));//0으로 만들기
                            }
                            if (!enable && cnt <= 0)
                            {
                                Managers.UserData.UpdateMyShopInfo(data.GetID(), (cnt * -1) + 1);//1로 만들기
                            }
                        }
                        break;
                    default:
                        enable = true;
                        break;
                }

                bool isPC = false;
#if !UNITY_EDITOR && UNITY_ANDROID
                var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                isPC = packageManager.Call<bool>("hasSystemFeature", "com.google.android.play.feature.HPE_EXPERIENCE");                
#endif

                if (advertiseMenu && isPC)
                {
                    continue;
                }

                if (!enable)
                {
                    limitOverItems.Add(data);
                    continue;
                }

                if (!data.IsEnableLimitItem())
                    continue;

                if (data.IsBuyLimitValid())
                    ret.Add(data);
                else
                    limitOverItems.Add(data);
            }
        }

        ret.Sort((a, b) =>
        {
            return ((ShopItemGameData)a).priority.CompareTo(((ShopItemGameData)b).priority);
        });

        limitOverItems.Sort((a, b) =>
        {
            return ((ShopItemGameData)a).priority.CompareTo(((ShopItemGameData)b).priority);
        });

        ret.AddRange(limitOverItems);

        return ret;
    }

    static public List<GameData> GetShopRandomItemWithSort(int menuID)
    {
        List<GameData> list = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_goods);

        List<GameData> ret = new List<GameData>();
        List<GameData> limitOverItems = new List<GameData>();

        foreach (ShopItemGameData data in list)
        {
            if (!data.use)
                continue;

            if (data.menu_id == menuID && data.IsDateValid())
            {
                if (!data.IsEnableLimitItem())
                    continue;

                if (data.IsBuyLimitValid())
                    ret.Add(data);
                else
                    limitOverItems.Add(data);
            }
        }

        ret.Sort((a, b) =>
        {
            return ((ShopItemGameData)a).priority.CompareTo(((ShopItemGameData)b).priority);
        });

        limitOverItems.Sort((a, b) =>
        {
            return ((ShopItemGameData)a).priority.CompareTo(((ShopItemGameData)b).priority);
        });

        ret.AddRange(limitOverItems);

        return ret;
    }

    public bool IsEnableLimitItem()
    {
        switch (GetID())
        {
            case 90005:
            case 90006:
            case 90007:
            case 90008:
            case 90009:
                {
                    RankType curRankType = Managers.UserData.MyRank;
                    int[] rank_iaps = { 1, 2, 5, 8, 11 };

                    foreach (int id in rank_iaps)
                    {
                        if (id > curRankType.GetID())
                        {
                            int pid = 0;
                            switch (id)
                            {
                                case 1: pid = 90005; break;
                                case 2: pid = 90006; break;
                                case 5: pid = 90007; break;
                                case 8: pid = 90008; break;
                                case 11: pid = 90009; break;
                            }

                            if (GetID() == pid)
                            {
                                return false;
                            }
                        }
                    }
                }
                break;
            default:
                {
                    if (price.type == ASSET_TYPE.CASH && price.param > 0)
                    {
                        var limitInfo = Managers.UserData.LimitedIAP;
                        if (limitInfo != null)
                        {
                            if (limitInfo.ContainsKey("iap") && limitInfo["iap"].Type == JTokenType.Object)
                            {
                                if (((JObject)limitInfo["iap"]).ContainsKey(GetID().ToString()))
                                {
                                    endTime = SBUtil.ConvertFromUnixSecTimestamp(limitInfo["iap"][GetID().ToString()].Value<long>()).ToLocalTime();
                                    if (DateTime.Now < endTime)
                                        return true;
                                }
                            }
                        }

                        return false;
                    }
                }
                break;
        }


        return true;
    }

    static public ShopItemGameData GetShopData(int id, bool quiet = false)
    {
        return (ShopItemGameData)GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_goods, id, quiet);
    }

    public bool IsDateValid()
    {
        return use && SBCommonLib.SBUtil.KoreanTime >= startTime && SBCommonLib.SBUtil.KoreanTime < endTime;
    }

    public bool IsBuyLimitValid()
    {
        return use && (buyLimit == 0 || buyLimit > Managers.UserData.GetMyShopHistory(GetID()));
    }

    public string GetTerms()
    {
        return StringManager.Instance.GetString(dataType.ToString(), "terms", GetID());
    }
    public virtual string GetBonusString()
    {
        string ret = "";
        var row = StringsGameData.GetStringData($"{dataType}:bonus:{GetID()}");
        if(row != null)
        {
            switch (GameConfig.Instance.OPTION_LANGUAGE)
            {
                case SystemLanguage.Korean:
                    ret = row.korean;
                    break;
                case SystemLanguage.Japanese:
                    ret = row.japanese;
                    break;
                default:
                    ret = row.english;
                    break;
            }
        }

        return ret;
    }

    public void SetSubscriptionItem(int day, SubscriptionItem item)
    {
        SubScriptionItems[day] = item;
    }
}

public class SubscriptionItem : GameData
{
    public int group_id { get; private set; }
    public int day { get; private set; }
    public int result { get; private set; }
    public List<ShopPackageGameData> rewards
    {
        get
        {
            if (results == null)
                results = ShopPackageGameData.GetRewardDataList(result);

            return results;
        }
    }

    List<ShopPackageGameData> results = null;

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        group_id = Int(data["group_id"]);
        day = Int(data["day"]);
        result = Int(data["result"]);

        ShopItemGameData parentData = ShopItemGameData.GetShopData(group_id);
        if(parentData != null)
        {
            parentData.SetSubscriptionItem(day, this);
        }        
    }
}



public class ShopPackageGameData : GameData
{
    public int goods_id { get; private set; }
    public int goods_type { get; private set; }
    public int goods_amount { get; private set; }

    private int goods_param = 0;

    public ItemGameData targetItem { get; private set; } = null;
    public CharacterGameData targetCharacter { get; private set; } = null;

    private Sprite iconSprite = null;
    static public Sprite DUMMY_PACKAGE_ICON = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/UI/Icon/icon_exclamation_black_white");
    static private Dictionary<int, string> in_app_sku = new Dictionary<int, string>();
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        goods_id = Int(data["goods_id"]);
        goods_type = Int(data["goods_type"]);
        goods_amount = Int(data["goods_amount"]);

        goods_param = Int(data["goods_param"]);

        if (iconSprite == null)
        {
            switch ((ASSET_TYPE)goods_type)
            {
                case ASSET_TYPE.GOLD:
                    iconSprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gold");
                    break;
                case ASSET_TYPE.DIA:
                    iconSprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gem");
                    break;
                case ASSET_TYPE.ITEM:
                case ASSET_TYPE.EQUIPMENT:
                case ASSET_TYPE.BUFF_ITEM:
                    targetItem = ItemGameData.GetItemData(goods_param);
                    if (targetItem != null)
                    {
                        iconSprite = targetItem.sprite;
                    }
                    break;
                case ASSET_TYPE.CHARACTER:
                    targetCharacter = CharacterGameData.GetCharacterData(goods_param);
                    if (targetCharacter == null)
                    {
                        iconSprite = targetCharacter.sprite_ui_resource;
                    }
                    break;
                default:
                    break;
            }
        }
    }
    static public List<ShopPackageGameData> GetRewardDataList(int goods_id)
    {
        List<ShopPackageGameData> ret = new List<ShopPackageGameData>();
        foreach (ShopPackageGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_package))
        {
            if (data.goods_id == goods_id)
            {
                ret.Add(data);
            }
        }

        return ret;
    }
    public Sprite GetIcon()
    {
        if (iconSprite == null)
        {
            switch ((ASSET_TYPE)goods_type)
            {
                case ASSET_TYPE.GOLD:
                    iconSprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gold");
                    break;
                case ASSET_TYPE.DIA:
                    iconSprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gem");
                    break;
                case ASSET_TYPE.ITEM:
                case ASSET_TYPE.EQUIPMENT:
                case ASSET_TYPE.BUFF_ITEM:
                    targetItem = ItemGameData.GetItemData(goods_param);
                    if (targetItem != null)
                    {
                        iconSprite = targetItem.sprite;
                    }
                    break;
                case ASSET_TYPE.CHARACTER:
                    targetCharacter = CharacterGameData.GetCharacterData(goods_param);
                    if (targetCharacter == null)
                    {
                        iconSprite = targetCharacter.sprite_ui_resource;
                    }
                    break;
                default:
                    break;
            }

            if (iconSprite == null)
            {
                return DUMMY_PACKAGE_ICON;
            }
        }

        return iconSprite;
    }

    public int GetParam()
    {
        return goods_param;
    }

    public static string GetIAPConstants(int packageID)
    {
        return in_app_sku[packageID];
    }

    public static void SetIAPConstants(JToken datas)
    {
        bool changed = false;
        if (datas.Type == JTokenType.Array)
        {
            foreach(JObject data in (JArray)datas)
            {
                int pid = data["product_id"].Value<int>();

#if UNITY_IOS
                string skuKey = "apple_sku";
#else
                string skuKey = "google_sku";
#endif
                string sku = data[skuKey].Value<string>();

                in_app_sku[pid] = sku;

                
                IAPProduct pd = InAppPurchasing.GetIAPProductByName(sku);
                if (pd == null)
                {
                    ShopItemGameData sd = ShopItemGameData.GetShopData((int)pid, true);
                    if (sd != null)
                    {
                        IAPProduct.StoreSpecificId[] ids = new IAPProduct.StoreSpecificId[2];
                        ids[0] = new IAPProduct.StoreSpecificId(IAPStore.GooglePlay, sku);
                        ids[1] = new IAPProduct.StoreSpecificId(IAPStore.AppleAppStore, sku);
                        IAPProduct[] products_array = EM_Settings.InAppPurchasing.Products;
                        List<IAPProduct> products = new List<IAPProduct>(products_array);
                        bool contains = false;
                        foreach(IAPProduct pro in products)
                        {
                            if(pro.Id == sku)
                            {
                                contains = true;
                                break;
                            }
                        }

                        if (!contains)
                        {
                            changed = true;
                            products.Add(new IAPProduct(sku, ids, sku, IAPProductType.Consumable, sd.price.amount.ToString() + "KRW"));
                            EM_Settings.InAppPurchasing.Products = products.ToArray();
                        }
                    }
                }
            }
        }

        if(changed)
        {
            InAppPurchasing.InitializePurchasing(true);
        }
    }
}

public enum ASSET_TYPE
{
    NONE = 0,
    GOLD,//1
    DIA,//2
    ITEM,//3
    CHARACTER,//4
    CASH,//5
    MILEAGE,//6
    PASS,//7
    PACKAGE,//8
    ADVERTISEMENT,//9
    TALENT, //10
    RANK_POINT,//11
    PASS_POINT,//12
    EQUIPMENT,//13
    BUFF_ITEM,//14
}
public class ShopItem
{
    public ASSET_TYPE type { get; private set; }
    public int param { get; private set; }
    public int amount { get; private set; }

    public Sprite priceIcon { get; private set; }
    public ShopItem(int t, int p, int a)
    {
        type = (ASSET_TYPE)t;
        param = p;
        amount = a;

        switch (type)
        {
            case ASSET_TYPE.GOLD:
                priceIcon = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gold");
                break;
            case ASSET_TYPE.DIA:
                priceIcon = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gem");
                break;
            case ASSET_TYPE.ITEM:
            case ASSET_TYPE.EQUIPMENT:
            case ASSET_TYPE.BUFF_ITEM:
                ItemGameData targetItem = ItemGameData.GetItemData(param);
                priceIcon = targetItem.sprite;
                break;
            case ASSET_TYPE.MILEAGE:
                priceIcon = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/icon_mileage");
                break;
            case ASSET_TYPE.CHARACTER:
                CharacterGameData targetCharacter = CharacterGameData.GetCharacterData(param);
                priceIcon = targetCharacter.sprite_ui_resource;
                break;
            case ASSET_TYPE.CASH:
                priceIcon = null;
                break;
            case ASSET_TYPE.ADVERTISEMENT:
                priceIcon = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/icon_ad");
                break;
            default:
                break;
        }
    }

    public string ToString()
    {
        string str = "";
        switch (type)
        {
            case ASSET_TYPE.GOLD:
                str = StringManager.GetString("gold_name");
                break;
            case ASSET_TYPE.DIA:
                str = StringManager.GetString("dia_name");
                break;
            case ASSET_TYPE.MILEAGE:
                str = StringManager.GetString("mileage_name");
                break;
            case ASSET_TYPE.ITEM:
            case ASSET_TYPE.EQUIPMENT:
            case ASSET_TYPE.BUFF_ITEM:
                ItemGameData targetItem = ItemGameData.GetItemData(param);
                str = targetItem.GetName();
                break;
            case ASSET_TYPE.CHARACTER:
                CharacterGameData targetCharacter = CharacterGameData.GetCharacterData(param);
                str = targetCharacter.GetName();
                break;
            default:
                break;
        }

        str += StringManager.GetString("ui_count", amount);

        return str;
    }
}
