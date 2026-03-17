using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ShopMenuData : TableData<DBShop_menu>
    {
        public int KEY => int.Parse(Data.UNIQUE_KEY);
        private int ASSET_TYPE => Data.ASSET_TYPE;
        public eStoreType TYPE => (eStoreType)Data.TYPE;
        public int SORT => Data.SORT;
        public eShopPageType PAGE_TYPE => (eShopPageType)Data.PAGE_TYPE;
        public bool USE_GOLD { get { return (ASSET_TYPE & 1) > 0; } }
        public bool USE_DIA { get { return (ASSET_TYPE & 2) > 0; } }
        public bool USE_MAGNET { get { return (ASSET_TYPE & 4) > 0; } }
        public bool USE_STAMINA { get { return (ASSET_TYPE & 8) > 0; } }
        public bool USE_PVP_TICKET { get { return (ASSET_TYPE & 16) > 0; } }

        public bool USE => Data.USE > 0;
        public bool TIME_LIMIT => Data.TIME_LIMIT > 0;
        public DateTime START_TIME => SBFunc.DateTimeParse(Data.START_TIME);
        public DateTime END_TIME => SBFunc.DateTimeParse(Data.END_TIME);
        public bool IS_VALID 
        { 
            get 
            {
                if (!USE)
                    return false;

                if (TIME_LIMIT)
                    return TimeManager.GetDateTime() > START_TIME && TimeManager.GetDateTime() < END_TIME;

                return true;
            } 
        }

        public string NAME { get { return StringData.GetStringByStrKey("shop_menu:" + KEY.ToString()); } }
        public Sprite BG_IMAGE { get { return ResourceManager.GetResource<Sprite>(eResourcePath.UISpritePath, string.Format("bg_title_{0:D2}", BG_TYPE)); } }
        private string icon_resource => Data.ICON;
        public Sprite ICON
        {
            get
            {
                if (string.IsNullOrEmpty(icon_resource))
                    return null;

                var ret = ResourceManager.GetResource<Sprite>(eResourcePath.ShopMenuIconPath, icon_resource);
                if (ret == null)
                {
                    ret = ResourceManager.GetResource<Sprite>(eResourcePath.ShopMenuIconPath, "icon_default");
                }

                return ret;
            }
        }


        private int BG_TYPE => Data.BG_TYPE;

        public int TabColor => Data.TAP_COLOR;

        public bool IsMenuReddot()
        {
            foreach (var data in ChildGoods)
            {
                if (data.Reddot)
                {
                    return true;
                }
            }

            return false;
        }

        List<ShopGoodsData> childGoods = null;
        public List<ShopGoodsData> ChildGoods
        {
            get
            {
                if (childGoods == null)
                {
                    childGoods = ShopGoodsData.GetByMenuID(KEY);
                }

                if (childGoods == null)
                    childGoods = new List<ShopGoodsData>();

                List<ShopGoodsData> ret = new List<ShopGoodsData>();
                foreach (var c in childGoods)
                {
                    if (c != null && c.IS_VALIDE)
                    {
                        ret.Add(c);
                    }
                }

                ret.Sort((a, b) =>
                {
                    var ret = a.SORT.CompareTo(b.SORT);
                    if (ret == 0)
                    {
                        ret = b.Reddot.CompareTo(a.Reddot);
                    }
                    return ret;
                });

                return ret;
            }
        }

        static private ShopMenuTable table = null;
        static public ShopMenuData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<ShopMenuTable>();

            return table.Get(key);
        }
        public bool IsValid()
        {
            if (TIME_LIMIT)
            {
                if (START_TIME > TimeManager.GetDateTime() || END_TIME < TimeManager.GetDateTime())
                    return false;
            }

            foreach (var child in ChildGoods)
            {
                if (child.IS_VALIDE)
                    return true;
            }

            return false;
        }
        public bool IsShowUI()
        {
            if (TIME_LIMIT)
            {
                if (START_TIME > TimeManager.GetDateTime() || END_TIME < TimeManager.GetDateTime())
                    return false;
            }

            return true;
        }

        static public List<ShopMenuData> GetShopMenus(eStoreType shopType, bool valid_only = true)
        {
            List<ShopMenuData> ret = new List<ShopMenuData>();
            if (table == null)
                table = TableManager.GetTable<ShopMenuTable>();
            foreach (var menu in table.GetAllList())
            {
                if (menu.TYPE == shopType && menu.USE)
                {
                    if (menu.IsValid() && valid_only)
                    {
                        ret.Add(menu);
                    }
                    else if (menu.IsShowUI() && !valid_only)
                    {
                        ret.Add(menu);
                    }
                }
            }

            return ret;
        }
        public void AddGoods(ShopGoodsData goods)
        {
            childGoods.Add(goods);
        }
    }


    public class ShopGoodsData : TableData<DBShop_goods>
    {
        public enum RewardType
        {
            UNKNOWN = -1,
            NONE = 0,
            POST_REWARD = 1,
            DIRECT_REWARD = 2,
        }


        static private ShopGoodsTable table = null;

        static public ShopGoodsData Get(int id)
        {
            if (table == null)
                table = TableManager.GetTable<ShopGoodsTable>();
            return table.Get(id);
        }

        static public List<ShopGoodsData> GetByMenuID(int id)
        {
            if (table == null)
                table = TableManager.GetTable<ShopGoodsTable>();
            return table.GetByMenuID(id);
        }

        static public ShopGoodsData GetBySKU(ShopSKUData skuData)
        {
            if (skuData == null)
                return null;

            if (table == null)
                table = TableManager.GetTable<ShopGoodsTable>();

            return table.Get(skuData.KEY);
        }

        static public int GetKeyBySpecificAsset(eGoodType goodType, int itemNo)
        {
            if (table == null)
                table = TableManager.GetTable<ShopGoodsTable>();
            return table.GetKeyBySpecificAsset(goodType, itemNo);
        }

        public string KEY => Data.UNIQUE_KEY;
        public int ID => Int(Data.UNIQUE_KEY);
        public int MENU => Data.MENU;
        public bool USE => Data.USE > 0;
        public int LEVEL => Data.USE; // 레벨 달성 방식 상점에서 사용하는 값
        public eShopType TYPE => (eShopType)Data.TYPE;
        public int SORT => Data.SORT;
        public string RESOURCE => Data.RESOURCE;
        public bool TIME_LIMIT => Data.TIME_LIMIT > 0;
        public DateTime START_TIME => SBFunc.DateTimeParse(Data.START_TIME);
        public DateTime END_TIME => SBFunc.DateTimeParse(Data.END_TIME);
        public eBuyLimitType BUY_TYPE => (eBuyLimitType)Data.BUY_TYPE;
        public int BUY_LIMIT => Data.BUY_LIMIT;
        public RewardType REWARD_TYPE => (RewardType)Data.REWARD_TYPE;
        public int REWARD_ID => Data.REWARD_ID;

        private List<Asset> rewards = null;
        public List<Asset> REWARDS
        {
            get
            {
                if (rewards == null)
                {
                    rewards = new List<Asset>();
                    switch (REWARD_TYPE)
                    {
                        case RewardType.DIRECT_REWARD:
                            foreach (var data in ItemGroupData.Get(REWARD_ID))
                            {
                                rewards.Add(data.Reward);
                            }
                            break;
                        case RewardType.POST_REWARD:
                            foreach (var data in PostRewardData.GetGroup(REWARD_ID))
                            {
                                rewards.Add(data.Reward);
                            }
                            break;
                    }
                }
                return rewards;
            }
        }
        public bool IS_VALIDE
        {
            get
            {
                if (!USE)
                    return false;

                if (TIME_LIMIT)
                {
                    if (START_TIME > TimeManager.GetDateTime() || END_TIME < TimeManager.GetDateTime())
                        return false;
                }

                if (TYPE == eShopType.PRIVATE)
                {
                    if (!ShopManager.Instance.PrivateGoods.ContainsKey(ID))
                    {
                        return false;
                    }
                    if (ShopManager.Instance.PrivateGoods[ID] < TimeManager.GetDateTime())
                    {
                        return false;
                    }
                }

                if (TYPE == eShopType.RANDOM)
                {
                    if (ShopManager.Instance.ArenaRandomGoods.Contains(ID) || ShopManager.Instance.FriendPointRandomGoods.Contains(ID))
                    {
                        return true;
                    }

                    return false;
                }

                if (PRICE.GoodType == eGoodType.MAGNET)
                {
                    if (!User.Instance.ENABLE_P2E)
                        return false;
                }

                return true;
            }
        }

        private eGoodType price_type => (eGoodType)Data.PRICE_TYPE;
        private int price_param => Data.PRICE_PARAM;
        private int price_amount => Data.PRICE_AMOUNT;
        public Asset PRICE { get; private set; } = null;
        public ShopBannerData BANNER { get { return ShopBannerData.Get(KEY); } }
        public Sprite SPRITE
        {
            get
            {
                if (string.IsNullOrEmpty(RESOURCE))
                {
                    foreach (var reward in REWARDS)
                    {
                        if (reward.ICON != null)
                        {
                            return reward.ICON;
                        }
                    }
                    return null;
                }

                return CDNManager.LoadBanner(SBFunc.GetResourceNameByLang(RESOURCE, "store"));
            }
        }
        public string Name
        {
            get
            {
                string key = "shop_goods:" + KEY.ToString();
                if (StringData.IsContainStrKey(key))
                    return StringData.GetStringByStrKey(key);

                foreach (var reward in REWARDS)
                {
                    return reward.GetName();
                }

                return "";
            }
        }
        public string Desc
        {
            get
            {
                string key = "shop_goods:desc:" + KEY.ToString();
                if (StringData.IsContainStrKey(key))
                    return StringData.GetStringByStrKey(key);

                foreach (var reward in REWARDS)
                {
                    if (reward.GetDesc() == string.Empty)
                    {
                        switch (reward.GoodType)
                        {
                            case eGoodType.ENERGY:
                                return StringData.GetStringByStrKey("item_base:desc:10000002");
                            case eGoodType.ARENA_TICKET:
                                return StringData.GetStringByStrKey("item_base:desc:10000007");
                            case eGoodType.ITEM:
                            default:
                                return reward.GetDesc();
                        }
                    }
                    return reward.GetDesc();
                }
                return "";
            }
        }
        public bool Reddot
        {
            get
            {
                if (TYPE == eShopType.PRIVATE)
                {
                    var state = ShopManager.Instance.GetGoodsState(ID);
                    if (state.IS_VALIDE)
                        return true;
                }

                if (TYPE == eShopType.SUBSCRIBE)
                {
                    var state = ShopManager.Instance.GetGoodsState(ID);
                    if (state.SubscribeState == eStoreSubscribeState.REWARD_ABLE)
                        return true;
                }

                if (PRICE.GoodType == eGoodType.ADVERTISEMENT)
                {
                    var state = ShopManager.Instance.GetAdvertiseState(PRICE.ItemNo);
                    if (state.IS_VALIDE)
                        return true;
                }

                if (TYPE == eShopType.SUBSCRIBE)
                {
                    var state = ShopManager.Instance.GetGoodsState(ID);
                    if (state.PurchasedCount > 0)
                    {
                        return state.SubscribeState == eStoreSubscribeState.REWARD_ABLE;
                    }
                }

                return false;
            }
        }

        public override void Init()
        {
            base.Init();

            if (!string.IsNullOrEmpty(RESOURCE))
                CDNManager.AddCDNResourceQueue(SBFunc.GetResourceNameByLang(RESOURCE, "store"));

            PRICE = new Asset(price_type, price_param, price_amount);
        }
    }


    public class ShopSubscriptionData : TableData<DBSubscription_item>
    {
        static private ShopSubscriptionTable table = null;
        static public ShopSubscriptionData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<ShopSubscriptionTable>();

            return table.Get(key);
        }

        static public ShopSubscriptionData GetByGroup(int group)
        {
            if (table == null)
                table = TableManager.GetTable<ShopSubscriptionTable>();

            return table.GetSubscription(group);
        }
        public int GROUP_ID => Data.GROUP_ID;
        public int DAY => Data.DAY;

        public int REWARD_ID => Data.REWARD_ID;        
    }

    public class PostRewardData : TableData<DBPost_reward>
    {
        static private PostRewardTable table = null;
        static public List<PostRewardData> GetGroup(int group_id)
        {
            if (table == null)
                table = TableManager.GetTable<PostRewardTable>();

            return table.GetGroup(group_id);
        }
        public int GROUP_ID => Data.GROUP_ID;

        private eGoodType reward_type => SBFunc.ConvertStringToItemType(Data.TYPE);
        private int reward_param => Data.VALUE;
        private int reward_amount => Data.NUM;

        public Asset Reward { get; private set; } = null;

        public int Order => Data.ORDER;

        public override void Init()
        {
            base.Init();
            Reward = new Asset(reward_type, reward_param, reward_amount);
        }
    }

    public class PersonalGoodsData : TableData<DBPersonal_goods>
    {
        static private PersonalGoodsTable table = null;
        static public int GetTime(int key)
        {
            if (table == null)
                table = TableManager.GetTable<PersonalGoodsTable>();

            var data = table.Get(key);
            if (data != null)
                return data.TIME;
            return -1;
        }
        static public PersonalGoodsData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<PersonalGoodsTable>();
            return table.Get(key);
        }

        //condition 조건은 이젠 클라이언트가 알아야 됨
        public ePersonalGoodsConditionType CONDITION_TYPE => (ePersonalGoodsConditionType)Data.CONDITION_TYPE;
        public int TIME => Data.TIME;
        private int conditionValue => Data.VALUE;
        public string TITLE_STRING { get { return StringData.GetStringByStrKey("shop_badge_condition_popup_title" + ((int)CONDITION_TYPE).ToString("D2")); } }
        public string DESC_STRING { get { return StringData.GetStringFormatByStrKey("shop_badge_condition_popup_desc" + ((int)CONDITION_TYPE).ToString("D2"), conditionValue); } }
        public string NAME_STRING { get { return string.IsNullOrEmpty(str_key) ? string.Empty : StringData.GetStringByStrKey(str_key); } }
        private string str_key => Data.STR_KEY;
    }

    public class ShopBannerData : TableData<DBShop_banner>
    {


        static private ShopBannerTable table = null;
        static public ShopBannerData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<ShopBannerTable>();

            return table.Get(key);
        }

        static public List<ShopBannerData> GetByType(BANNER_TYPE type)
        {
            if (table == null)
                table = TableManager.GetTable<ShopBannerTable>();
            return table.GetByType(type);
        }

        public string KEY => Data.UNIQUE_KEY;
        public BANNER_TYPE TYPE => (BANNER_TYPE)Data.TYPE;
        public string RESOURCE => Data.RESOURCE;

        public Sprite SPRITE
        {
            get
            {
                if (!string.IsNullOrEmpty(RESOURCE))
                    return CDNManager.LoadBanner(SBFunc.GetResourceNameByLang(RESOURCE, "store"));

                return null;
            }
        }

        public string ICON_RESOURCE => Data.ICON_RESOURCE;
        
        public Sprite BG_SPRITE { get { return ResourceManager.GetResource<Sprite>(eResourcePath.UISpritePath, string.Format("bg_title_{0:D2}", BG_TYPE)); } }
        public int BG_TYPE => Data.BG_TYPE;

        public override void Init()
        {
            base.Init();
            if (!string.IsNullOrEmpty(ICON_RESOURCE))
                CDNManager.AddCDNResourceQueue("store/" + ICON_RESOURCE);
        }
    }

    public class ShopSKUData : TableData<DBShop_sku>
    {
        static private ShopSKUTable table = null;
        static public ShopSKUData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<ShopSKUTable>();

            return table.Get(key);
        }

        enum STORE_TYPE
        {
            APPLE = 0,
            GOOGLE = 1,
            ONE = 2,
            MAX = 3,
        }

        public string KEY => Data.UNIQUE_KEY;
        private string APPLE => Data.SKU_APPLE;
        private string GOOGLE => Data.SKU_GOOGLE;
        private string ONE => Data.SKU_ONESTORE;

        private string[] KRW = new string[(int)STORE_TYPE.MAX] { "￦ -", "￦ -", "￦ -" };
        private string[] USD = new string[(int)STORE_TYPE.MAX] { "$ -", "$ -", "$ -" };
        private string[] JPY = new string[(int)STORE_TYPE.MAX] { "￥ -", "￥ -", "￥ -" };
        public string SKU
        {
            get
            {
                switch (CUR_STORE)
                {
                    case STORE_TYPE.APPLE:
                        return APPLE;
                    case STORE_TYPE.GOOGLE:
                        return GOOGLE;
                    case STORE_TYPE.ONE:
                        return ONE;
                }

                return GOOGLE;
            }
        }

        STORE_TYPE CUR_STORE
        {
            get
            {
                return
#if UNITY_IOS
                STORE_TYPE.APPLE;
#else
#if ONESTORE
                STORE_TYPE.ONE;
#else
                STORE_TYPE.GOOGLE;
#endif
#endif
            }
        }

#if !UNITY_EDITOR
#if ONESTORE
        OneStore.Purchasing.ProductDetail productInfo = null;
#elif EM_UIAP
        UnityEngine.Purchasing.Product productInfo = null;
#endif
#endif
        public string StorePrice
        { 
            get
            {
#if !UNITY_EDITOR
#if ONESTORE
                if (productInfo == null)
                {
                    productInfo = OneStoreManager.Instance.GetProductDetail(SKU);
                }

                if (productInfo != null)
                {
                    System.Globalization.CultureInfo culture = GetCultureInfoFromISOCurrencyCode(productInfo.priceCurrencyCode);
                    if (culture != null)
                    {
                        return (decimal.Parse(productInfo.price)).ToString("C", culture);
                    }
                }
#elif EM_UIAP
                if (productInfo == null)
                {
                    if(EasyMobile.InAppPurchasing.IsInitialized())
                        productInfo = EasyMobile.InAppPurchasing.StoreController.products.WithID(SKU);                    
                }
                
                if(productInfo != null)
                {
                    System.Globalization.CultureInfo culture = GetCultureInfoFromISOCurrencyCode(productInfo.metadata.isoCurrencyCode);
                    if (culture != null) 
                    {
                        return productInfo.metadata.localizedPrice.ToString("C", culture);
                    }
                }
#endif
#endif
                return "";
            }
        }

        public string LocalPrice
        {
            get
            {
                string price = "";
                switch (GamePreference.Instance.GameLanguage)
                {
                    case SystemLanguage.Korean:
                        price = KRW[(int)CUR_STORE]; break;
                    case SystemLanguage.English:
                        price = USD[(int)CUR_STORE]; break;
                    case SystemLanguage.Japanese:
                        price = JPY[(int)CUR_STORE]; break;
                    case SystemLanguage.Portuguese:
                        price = USD[(int)CUR_STORE]; break;
                    default:
                        price = USD[(int)CUR_STORE]; break;
                }

                string[] splits = price.Split('.');
                if (splits.Length > 1)
                {
                    price = SBFunc.CommaFromNumber(Int(splits[0])) + "." + splits[1];
                }
                else
                {
                    price = SBFunc.CommaFromNumber(Int(price));
                }

                return PRICE_SIGN + price;
            }
        }
        public string PRICE
        {
            get
            {
#if !UNITY_EDITOR
                var ret = StorePrice;
                if(!string.IsNullOrEmpty(ret))
                    return ret;
#endif

                return LocalPrice;
            }
        }

        public string PRICE_SIGN
        {
            get
            {
                switch (GamePreference.Instance.GameLanguage)
                {
                    case SystemLanguage.Korean:
                        return "￦";
                    case SystemLanguage.English:
                        return "$";
                    case SystemLanguage.Japanese:
                        return "￥";
                    case SystemLanguage.Portuguese:
                        return "$";
                    default:
                        return "$";
                }
            }
        }

        public ShopGoodsData ShopGoods { get { return ShopGoodsData.GetBySKU(this); } }
        public override void Init()
        {
            base.Init();
            KRW[(int)STORE_TYPE.APPLE] = Data.KRW_APPLE;
            KRW[(int)STORE_TYPE.GOOGLE] = Data.KRW_GOOGLE;
            KRW[(int)STORE_TYPE.ONE] = Data.KRW_ONE;

            USD[(int)STORE_TYPE.APPLE] = Data.USD_APPLE;
            USD[(int)STORE_TYPE.GOOGLE] = Data.USD_GOOGLE;
            USD[(int)STORE_TYPE.ONE] = Data.USD_ONE;

            JPY[(int)STORE_TYPE.APPLE] = Data.JPY_APPLE;
            JPY[(int)STORE_TYPE.GOOGLE] = Data.JPY_GOOGLE;
            JPY[(int)STORE_TYPE.ONE] = Data.JPY_ONE;
        }

        public static System.Globalization.CultureInfo GetCultureInfoFromISOCurrencyCode(string code)
        {
            foreach (System.Globalization.CultureInfo ci in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.SpecificCultures))
            {
                System.Globalization.RegionInfo ri = new System.Globalization.RegionInfo(ci.LCID);
                if (ri.ISOCurrencySymbol == code)
                    return ci;
            }
            return null;
        }


        public DateTime OFFER_START_TIME => SBFunc.DateTimeParse(Data.OFFER_START_TIME);
        public DateTime OFFER_END_TIME => SBFunc.DateTimeParse(Data.OFFER_END_TIME);

        public bool IsOfferTime()
        {
            return (OFFER_START_TIME < TimeManager.GetDateTime() && OFFER_END_TIME > TimeManager.GetDateTime());
        }
    }
}

public class AdvertisementData : TableData<DBAdv_limit>
{
    static private AdvertisementTable table = null;
    static public AdvertisementData Get(int key)
    {
        if (table == null)
            table = TableManager.GetTable<AdvertisementTable>();

        return table.Get(key);
    }

    public string KEY => Data.UNIQUE_KEY;
    public int CUR
    {
        get
        {
            var info = ShopManager.Instance.GetAdvertiseState(int.Parse(KEY));
            if (info != null)
                return info.VIEW_COUNT;

            return 0;
        }
    }
    public int LIMIT => Data.LIMIT;
    public int TERM => Data.TERM;    
}


public class ShopRandomData : TableData<DBShop_random>
{
    static private ShopRandomTable table = null;
    static public ShopRandomData Get(int key)
    {
        if (table == null)
            table = TableManager.GetTable<ShopRandomTable>();

        return table.Get(key);
    }

    public int MENU_GROUP => Data.MENU_GROUP;
    public int GOOD_GROUP => Data.GOOD_GROUP;
    public int GOOD_ID => Data.GOOD_ID;
    public int RATE => Data.RATE;
}

public class EventBannerData : TableData<DBEvent_banner>
{

    static private EventBannerTable table = null;
    static public EventBannerData Get(int key)
    {
        if (table == null)
            table = TableManager.GetTable<EventBannerTable>();

        return table.Get(key);
    }

    static public List<EventBannerData> GetBanners()
    {
        if (table == null)
            table = TableManager.GetTable<EventBannerTable>();

        List<EventBannerData> ret = new List<EventBannerData>();
        DateTime now = TimeManager.GetDateTime();
        foreach (var data in table.GetAllList())
        {
            if (data.USE)
            {
                if (data.START_TIME < now && data.END_TIME > now)
                {
                    ret.Add(data);
                }
            }
        }

        ret.Sort((a, b) => a.SORT.CompareTo(b.SORT));

        return ret;
    }

    public DateTime START_TIME => SBFunc.DateTimeParse(Data.START_TIME);
    public DateTime END_TIME => SBFunc.DateTimeParse(Data.END_TIME);
    public eActionType ACTION => (eActionType)Data.ACTION;
    public string ACTION_PARAM => Data.ACTION_PARAM;
    public string RESOURCE => Data.RESOURCE_PATH;
    public Sprite SPRITE
    {
        get
        {
            if (!string.IsNullOrEmpty(RESOURCE))
                return CDNManager.LoadBanner(SBFunc.GetResourceNameByLang(RESOURCE, "event"));
            return null;
        }
    }

    public bool USE 
    { 
        get {
            if (Data.USE > 0)
            {
                if (Data.USE == (int)eEventUseFlag.USE)
                    return true;

                switch (SBGameManager.CurServerTag)
                {
                    case 0:
                        if ((Data.USE & (int)eEventUseFlag.SERVER_0) <= 0)
                            return false;
                        break;
                    case 1:
                        if ((Data.USE & (int)eEventUseFlag.SERVER_1) <= 0)
                            return false;
                        break;
                    case 2:
                        if ((Data.USE & (int)eEventUseFlag.SERVER_2) <= 0)
                            return false;
                        break;
                    case 3:
                        if ((Data.USE & (int)eEventUseFlag.SERVER_3) <= 0)
                            return false;
                        break;
                }

                if (User.Instance.ENABLE_P2E)
                {
                    return (Data.USE & (int)eEventUseFlag.WEB3) > 0;
                }
                else
                {
                    return (Data.USE & (int)eEventUseFlag.WEB2) > 0;
                }
            }

            return false;
        } 
    }
    public int SORT => Data.SORT;

    public override void Init()
    {
        base.Init();
        if (!string.IsNullOrEmpty(RESOURCE))
            CDNManager.AddCDNResourceQueue(SBFunc.GetResourceNameByLang(RESOURCE, "event"));
    }    
}

public class EventScheduleData : TableData<DBEvent_schedule>
{
    static private EventScheduleTable table = null;

    static public EventScheduleData Get(int key)
    {
        if (table == null)
            table = TableManager.GetTable<EventScheduleTable>();

        return table.Get(key);
    }
    static public List<EventScheduleData> GetAll()
    {
        if (table == null)
            table = TableManager.GetTable<EventScheduleTable>();

        return table.GetAllList();
    }

    static public List<EventScheduleData> GetAttendanceTypeDatas()
    {
        var dataList = GetActiveEvents(false);
        if (dataList == null || dataList.Count <= 0)
            return null;

        List<EventScheduleData> ret = new List<EventScheduleData>();
        foreach(var data in dataList)
        {
            if(data.TYPE == eActionType.EVENT_ATTENDANCE)
                ret.Add(data);
        }

        return ret;
    }

    static public List<EventScheduleData> GetEventTypeData(eActionType _type , bool _isUITime = false)
    {
        List<EventScheduleData> ret = new List<EventScheduleData>();
        var dataList = GetActiveEvents(_isUITime);
        if (dataList == null || dataList.Count <= 0)
            return ret;
        
        foreach (var data in dataList)
        {
            if (data.TYPE == _type)
                ret.Add(data);
        }

        return ret;
    }

    static public List<EventScheduleData> GetActiveEvents(bool _isUITime)
    {
        List<EventScheduleData> ret = new List<EventScheduleData>();
        if (table == null)
            table = TableManager.GetTable<EventScheduleTable>();

        var dataList = table.GetAllList();
        if (dataList == null || dataList.Count <= 0)
            return ret;

        foreach(var data in dataList)
        {
            if (data.IsEventPeriod(_isUITime))
                ret.Add(data);
        }

        return ret;
    }

    public string KEY => Data.UNIQUE_KEY;
    public DateTime START_TIME => SBFunc.DateTimeParse(Data.START_TIME);
    public DateTime END_TIME => SBFunc.DateTimeParse(Data.END_TIME);
    public DateTime UI_END_TIME => SBFunc.DateTimeParse(Data.UI_END_TIME);
    public eActionType TYPE => (eActionType)Data.TYPE;
    public string RESOURCE => Data.RESOURCE_PATH;
    public Sprite SPRITE
    {
        get
        {
            if (!string.IsNullOrEmpty(RESOURCE))
                return CDNManager.LoadBanner("event/" + RESOURCE);
            return null;
        }
    }

    public bool USE
    {
        get {
            if (Data.USE > 0)
            {
                if (Data.USE == (int)eEventUseFlag.USE)
                    return true;

                switch (SBGameManager.CurServerTag)
                {
                    case 0:
                        if ((Data.USE & (int)eEventUseFlag.SERVER_0) <= 0)
                            return false;
                        break;
                    case 1:
                        if ((Data.USE & (int)eEventUseFlag.SERVER_1) <= 0)
                            return false;
                        break;
                    case 2:
                        if ((Data.USE & (int)eEventUseFlag.SERVER_2) <= 0)
                            return false;
                        break;
                    case 3:
                        if ((Data.USE & (int)eEventUseFlag.SERVER_3) <= 0)
                            return false;
                        break;
                }

                if (User.Instance.ENABLE_P2E)
                {
                    return (Data.USE & (int)eEventUseFlag.WEB3) > 0;
                }
                else
                {
                    return (Data.USE & (int)eEventUseFlag.WEB2) > 0;
                }
            }

            return false;
        }
    }
    public int SORT => Data.SORT;

    public EventBaseData EventBaseData { get; private set; } = null;

    public override void Init()
    {
        base.Init();
        switch (TYPE)
        {
            case eActionType.EVENT_ATTENDANCE:
                EventBaseData = new EventAttendanceData(this);
                break;
            case eActionType.EVENT_DICE:
                EventBaseData = new EventDiceBaseData(this);
                break;
            case eActionType.EVENT_LUCKY_BAG:
                EventBaseData = new EventLuckyBagBaseData(this);
                break;
            default:
                break;
        }
    }
    
    
    public string GetEventString(string _eventPrefix = "event_schedule::")
    {
        return StringData.GetStringByStrKey(SBFunc.StrBuilder(_eventPrefix, KEY));
    }
    public string GetEventEndTimeString(string _eventPrefix, int count, bool _isUITime = true)
    {
        var endTime = _isUITime ? UI_END_TIME : END_TIME;
        string resultString = "";
        if (count-- > 0)
        {
            resultString = SBFunc.StrBuilder(resultString, endTime.Month.ToString(), StringData.GetStringByStrKey("time_month"));
        }
        if (count-- > 0)
        {
            resultString = SBFunc.StrBuilder(resultString, endTime.Day.ToString(), StringData.GetStringByStrKey("time_day"));
        }
        if (count-- > 0)
        {
            resultString = SBFunc.StrBuilder(resultString, endTime.Hour.ToString(), StringData.GetStringByStrKey("time_hr"));
        }
        if (count-- > 0)
        {
            resultString = SBFunc.StrBuilder(resultString, endTime.Minute.ToString(), StringData.GetStringByStrKey("time_min"));
        }

        return StringData.GetStringFormatByStrKey(SBFunc.StrBuilder(_eventPrefix, KEY), resultString);
    }

    public bool IsEventPeriod(bool _isUITime)//use && 이벤트 기간 체크
    {
        var inUse = USE;//사용 할건지
        var startTime = START_TIME;
        var endTime = _isUITime ? UI_END_TIME : END_TIME;
        var isPeriod = (int)(endTime - TimeManager.GetDateTime()).TotalSeconds > 0 && (int)(TimeManager.GetDateTime() - startTime).TotalSeconds > 0;//종료 시간보다 작고, 시작 시간보다 클 때

        if (inUse && isPeriod)
            return true;
        else
            return false;
    }
}

public class EventAttendanceResourceData : TableData<DBEvent_attendance_resource>
{
    static private EventAttendanceResourceTable table = null;
    static public EventAttendanceResourceData Get(int key)
    {
        if (table == null)
            table = TableManager.GetTable<EventAttendanceResourceTable>();

        return table.Get(key);
    }

    public string KEY => Data.UNIQUE_KEY;
    public string ICON_RESOURCE => Data.ICON_RESOURCE_PATH;
    public string POPUP_RESOURCE => Data.POPUP_RESOURCE_PATH;
    public string BG_RESOURCE => Data.BG_RESOURCE_PATH;

    public Color DESC_COLOR
    {
        get
        {
            var DESC_COLOR_HEX = Data.DESC_HEX;
            
            if (!DESC_COLOR_HEX.Contains("#"))
                DESC_COLOR_HEX = DESC_COLOR_HEX.Insert(0, "#");

            if (!string.IsNullOrEmpty(DESC_COLOR_HEX) && ColorUtility.TryParseHtmlString(DESC_COLOR_HEX, out Color resultColor))
            {
                return resultColor;
            }
            else
                return Color.white;
        }
    }

    public Color REWARD_DESC_HEX
    {
        get
        {
            var DESC_COLOR_HEX = Data.REWARD_DESC_HEX;
            if (!DESC_COLOR_HEX.Contains("#"))
                DESC_COLOR_HEX = DESC_COLOR_HEX.Insert(0, "#");

            if (!string.IsNullOrEmpty(DESC_COLOR_HEX) && ColorUtility.TryParseHtmlString(DESC_COLOR_HEX, out Color resultColor))
            {
                return resultColor;
            }
            else
                return Color.white;
        }
    }


    public Sprite ICON_SPRITE
    {
        get
        {
            if (string.IsNullOrEmpty(ICON_RESOURCE))
                return null;

            if (USE_CDN)
                return CDNManager.LoadBanner(SBFunc.StrBuilder("event/attendance/", ICON_RESOURCE));
            else
            {
                var replaceStr = ICON_RESOURCE.Replace(".png", "");
                return ResourceManager.GetResource<Sprite>(eResourcePath.EventAttendancePath, replaceStr);
            }
        }
    }
    public Sprite BG_SPRITE
    {
        get
        {
            if (string.IsNullOrEmpty(BG_RESOURCE))
                return null;

            if (USE_CDN)
                return CDNManager.LoadBanner(SBFunc.StrBuilder("event/attendance/", BG_RESOURCE));
            else
            {
                var replaceStr = BG_RESOURCE.Replace(".png", "");
                return ResourceManager.GetResource<Sprite>(eResourcePath.EventAttendancePath, replaceStr);
            }
        }
    }

    public Sprite POPUP_SPRITE
    {
        get
        {
            if (string.IsNullOrEmpty(POPUP_RESOURCE))
                return null;

            if (USE_CDN)//팝업 리소스는 보더 잡아야함.
            {
                Sprite sp = CDNManager.LoadBanner(SBFunc.StrBuilder("event/attendance/", POPUP_RESOURCE));
                var tex = sp.texture;

                Sprite borderSpr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), sp.pivot, sp.pixelsPerUnit, 0, SpriteMeshType.FullRect,
                    new Vector4(360, 161, 360, 161));

                return borderSpr;
            }
            else
            {
                var replaceStr = POPUP_RESOURCE.Replace(".png", "");
                return ResourceManager.GetResource<Sprite>(eResourcePath.EventAttendancePath, replaceStr);
            }
        }
    }


    public bool USE_CDN => Data.USE_CDN > 0;
    public override void Init()
    {
        base.Init();
        if (!string.IsNullOrEmpty(ICON_RESOURCE))
            CDNManager.AddCDNResourceQueue(SBFunc.StrBuilder("event/attendance/", ICON_RESOURCE));
        if (!string.IsNullOrEmpty(POPUP_RESOURCE))
            CDNManager.AddCDNResourceQueue(SBFunc.StrBuilder("event/attendance/", POPUP_RESOURCE));
        if (!string.IsNullOrEmpty(BG_RESOURCE))
            CDNManager.AddCDNResourceQueue(SBFunc.StrBuilder("event/attendance/", BG_RESOURCE));       
    }
}





