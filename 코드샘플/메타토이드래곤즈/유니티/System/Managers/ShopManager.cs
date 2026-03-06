using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class ShopGoodsState
    {
        public int ID { get; private set; } = 0;

        /// <summary>
        /// 상품 남은 갯수
        /// </summary>
        public int RemainGoodsCount 
        {
            get {
                if (BaseData == null)
                    return 0;

                if (BaseData.BUY_TYPE == eBuyLimitType.UNLIMIT && BaseData.BUY_LIMIT == 0)
                    return 1;
                
                return PurchasedLimit - PurchasedCount;
            } 
        }

        public int PurchasedLimit 
        { 
            get 
            {
                if (BaseData != null)
                {
                    if(BaseData.PRICE.GoodType == eGoodType.ADVERTISEMENT)
                    {
                        var state = ShopManager.Instance.GetAdvertiseState(BaseData.PRICE.ItemNo);
                        return state != null ? state.VIEW_LIMIT : 0;
                    }    
                    return BaseData.BUY_LIMIT;
                }
                return 0; 
            } 
        }
        int purchasedCount = 0;
        public int PurchasedCount 
        { 
            get {
                if (BaseData.PRICE.GoodType == eGoodType.ADVERTISEMENT)
                {
                    var state = ShopManager.Instance.GetAdvertiseState(BaseData.PRICE.ItemNo);
                    return state != null ? state.VIEW_COUNT : 0;
                }

                return purchasedCount;
            } 
        }

        public void ClearPurchasedCount()
        {
            purchasedCount = 0;
        }


        /// <summary>
        /// 조건형 상품 종료 시간
        /// </summary>
        public DateTime endTime { get; private set; } = TimeManager.GetDateTime();
        /// <summary>
        /// 조건형 상품 조건 만족 상태
        /// </summary>
        public bool IS_VALIDE 
        { 
            get 
            {
                if (BaseData == null)
                    return false;

                if (!BaseData.USE)
                    return false;
                if (!BaseData.IS_VALIDE)
                    return false;

                return RemainGoodsCount > 0;
            } 
        }


        /// <summary>
        /// 구독형 상품 상태
        /// </summary>
        public eStoreSubscribeState SubscribeState { get; private set; } = eStoreSubscribeState.NOT_SUB;
        /// <summary>
        /// 현재 구독 상품의 구독기간
        /// </summary>
        public int SubscribeDay { get; private set; } = 0;

        public int SubscribeMax { get; private set; } =1;

        public int NextSubscribeGetTime { get; private set; } = 0;
        

        private ShopGoodsData baseData = null;
        public ShopGoodsData BaseData 
        { 
            get 
            { 
                if(baseData == null)
                {
                    baseData = ShopGoodsData.Get(ID);
                }

                return baseData;
            } 
        }
        public ShopGoodsState(int goodsNo, int count = 0)
        {
            ID = goodsNo;
            UpdatePurchasedCount(count);
        }

        public void UpdatePurchasedCount(int count)
        {
            purchasedCount = count;
        }
        
        public void UpdateNextSubScribeTime(int time)
        {
            NextSubscribeGetTime = time;
        }
        

        public void UpdateSubscribe(int index, int max, bool rewardable)
        {
            SubscribeState = rewardable ? eStoreSubscribeState.REWARD_ABLE : eStoreSubscribeState.REWARDED;
            
            SubscribeDay = index;
            SubscribeMax = max;

            if (SubscribeDay > SubscribeMax)
                SubscribeState = eStoreSubscribeState.NOT_SUB;
        }
    }

    public class AdvertiseState
    {
        public int ID { get; private set; } = 0;

        private int viewCount = 0;
        private AdvertisementData baseData = null;
        public AdvertisementData BaseData
        {
            get
            {
                if (baseData == null)
                {
                    baseData = AdvertisementData.Get(ID);
                }

                return baseData;
            }
        }
        public int Remain
        {
            get {
                if (BaseData == null)
                    return 0;
                return (int)(LAST_VIEWDATE.AddSeconds(BaseData.TERM) - TimeManager.GetDateTime()).TotalSeconds;
            }
        }
        public DateTime LAST_VIEWDATE { get; private set; } = DateTime.MinValue;
        public int VIEW_COUNT
        {
            get
            {
                if (TimeManager.GetDailyStartTime() > LAST_VIEWDATE)
                {
                    viewCount = 0;
                }
                return viewCount;
            }
        }
        public int VIEW_LIMIT
        {
            get {
                if (BaseData == null)
                    return 0;

                return BaseData.LIMIT;
            }
        }

        public bool IS_VALIDE
        {
            get
            {
                if (BaseData == null)
                    return false;

                if (VIEW_COUNT >= VIEW_LIMIT)
                    return false;
                
                if (LAST_VIEWDATE.AddSeconds(BaseData.TERM) > TimeManager.GetDateTime())
                    return false;

                return true;
            }
        }

        public AdvertiseState(int adID)
        {
            ID = adID;
            UpdateViewCount(0);
        }
        public void UpdateAdvertise(int v, int stamp)
        {
            LAST_VIEWDATE = TimeManager.GetCustomDateTime(stamp);
            UpdateViewCount(v);
        }

        public void UpdateAdvertise(int v, DateTime last)
        {
            LAST_VIEWDATE = last;
            UpdateViewCount(v);
        }

        public void UpdateViewCount(int count)
        {
            viewCount = count;
        }
    }

    public class ShopManager
    {
        static ShopManager instance = null;
        public static ShopManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShopManager();
                    instance.ShopRefreshTime = TimeManager.GetDateTime();
                }
                return instance;
            }
        }
        public Dictionary<int, ShopGoodsState> GoodsInfo { get; private set; } = new Dictionary<int, ShopGoodsState>();
        public Dictionary<int, AdvertiseState> AdvertiseInfo { get; private set; } = new Dictionary<int, AdvertiseState>();
        public Dictionary<int, DateTime> PrivateGoods { get; private set; } = new Dictionary<int, DateTime>();
        public List<int> ArenaRandomGoods { get; private set; } = new List<int>();
        public int ArenaRandomGoodsUpdateTime { get; private set; } = 0;

        public List<int> FriendPointRandomGoods { get; private set; } = new List<int>();
        public int FriendPointRandomGoodsUpdateTime { get; private set; } = 0;

        private DateTime ShopRefreshTime = DateTime.MinValue;
        List<int> shownPrivateGoodsCache = new List<int>();
        public void Clear()
        {
            GoodsInfo.Clear();
            PrivateGoods.Clear();
            ArenaRandomGoods.Clear();
            FriendPointRandomGoods.Clear();
            ArenaRandomGoodsUpdateTime = 0;
            FriendPointRandomGoodsUpdateTime = 0;
        }

        public void SetPurchased(JObject json)
        {
            var properties = json.Properties();

            foreach (var obj in properties.Select((value, i) => (value, i)))
            {
                string key = obj.value.Name;
                int index = int.Parse(key);

                UpdatePurchased(index, json[key].Value<int>());
            }
        }

        public void SetPrivateGoods(JObject json)
        {
            PrivateGoods.Clear();

            var properties = json.Properties();

            foreach (var obj in properties.Select((value, i) => (value, i)))
            {
                string key = obj.value.Name;
                int index = int.Parse(key);
                PrivateGoods[index] = TimeManager.GetCustomDateTime(json[key].Value<long>());
            }
        }
        public void SetArenaRandomGoods(JToken json)
        {
            ArenaRandomGoods.Clear();

            JObject listData = (JObject)json;
            // 아레나 상점 품목 ID 리스트 
            if (listData.ContainsKey("goods_list") && SBFunc.IsJTokenType(listData["goods_list"],JTokenType.String))
            {
                string goodsListString = json["goods_list"].Value<string>();
                if (string.IsNullOrWhiteSpace(goodsListString) == false)
                {
                    ArenaRandomGoods = goodsListString.Split(',').ToList().ConvertAll(int.Parse);
                }
            }

            // 아레나 상점 품목 구매 갯수 리스트
            if (listData.ContainsKey("buy_cnt_list"))
            {
                if(SBFunc.IsJTokenType(listData["buy_cnt_list"], JTokenType.String))
                {
                    string goodsBuyCountListString = json["buy_cnt_list"].Value<string>();
                    if (string.IsNullOrWhiteSpace(goodsBuyCountListString) == false)
                    {
                        var list = goodsBuyCountListString.Split(',').ToList().ConvertAll(int.Parse);
                        foreach (var goods in ArenaRandomGoods)
                        {
                            UpdatePurchased(goods, list.FindAll(goodsID => goods == goodsID).Count);
                        }
                    }
                }
                else
                {
                    foreach (var menu in ShopMenuData.GetShopMenus(eStoreType.ARENA_POINT))
                    {
                        foreach (var goods in menu.ChildGoods)
                        {
                            UpdatePurchased(goods.ID, 0);
                        }
                    }
                }
            }

            // 마지막 아레나 상점 갱신 시간
            if (listData.ContainsKey("last_refresh_at"))
            {
                if(DateTime.TryParse(listData["last_refresh_at"].Value<string>(), out DateTime resultTime))
                    ArenaRandomGoodsUpdateTime = TimeManager.GetTimeStamp(resultTime);
                else
                    ArenaRandomGoodsUpdateTime = json["last_refresh_at"].Value<int>();
            }
        }

        public void SetFriendRandomGoods(JToken json)
        {
            FriendPointRandomGoods.Clear();

            JObject listData = (JObject)json;
            // 아레나 상점 품목 ID 리스트 
            if (listData.ContainsKey("goods_list") && SBFunc.IsJTokenType(listData["goods_list"], JTokenType.String))
            {
                string goodsListString = json["goods_list"].Value<string>();
                if (string.IsNullOrWhiteSpace(goodsListString) == false)
                {
                    FriendPointRandomGoods = goodsListString.Split(',').ToList().ConvertAll(int.Parse);
                }
            }

            // 아레나 상점 품목 구매 갯수 리스트
            if (listData.ContainsKey("buy_cnt_list"))
            {
                if(SBFunc.IsJTokenType(listData["buy_cnt_list"], JTokenType.String))
                {
                    string goodsBuyCountListString = json["buy_cnt_list"].Value<string>();
                    if (string.IsNullOrWhiteSpace(goodsBuyCountListString) == false)
                    {
                        var list = goodsBuyCountListString.Split(',').ToList().ConvertAll(int.Parse);
                        foreach (var goods in FriendPointRandomGoods)
                        {
                            UpdatePurchased(goods, list.FindAll(goodsID => goods == goodsID).Count);
                        }
                    }
                }
                else
                {
                    foreach(var menu in ShopMenuData.GetShopMenus(eStoreType.FRIEND_POINT))
                    {
                        foreach(var goods in menu.ChildGoods)
                        {
                            UpdatePurchased(goods.ID, 0);
                        }
                    }
                }
            }

            // 마지막 아레나 상점 갱신 시간
            if (listData.ContainsKey("last_refresh_at"))
            {
                if (DateTime.TryParse(listData["last_refresh_at"].Value<string>(), out DateTime resultTime))
                    FriendPointRandomGoodsUpdateTime = TimeManager.GetTimeStamp(resultTime);
                else
                    FriendPointRandomGoodsUpdateTime = json["last_refresh_at"].Value<int>();
            }
        }
        public void UpdatePrivateGoods(int index, long stamp)
        {
            UpdatePrivateGoods(index, TimeManager.GetCustomDateTime(stamp));
        }
        public void UpdatePrivateGoods(int index, DateTime time)
        {
            PrivateGoods[index] = time;
            if (Town.Instance != null && !Town.Instance.IsCamZooming && !PopupManager.IsPopupOpening())
            {
                NotificationManager.Instance.RefreshNotifications();
            }
        }

        public void SetSubscribeGoods(JToken json)
        {
            if(json.Type == JTokenType.Object)
            {
                UpdateSubscribeGoods((JObject)json);
            }   
            else if (json.Type == JTokenType.Array)
            {
                foreach(var info in (JArray)json)
                {
                    UpdateSubscribeGoods((JObject)info);
                }
            }
        }

        public void UpdateSubscribeGoods(JObject info)
        {
            var state = GetGoodsState(info["prod"].Value<int>());
            state.UpdateSubscribe(info["idx"].Value<int>(), info["tot"].Value<int>(), info["today"].Value<int>() > 0);
        }

        public void UpdatePurchased(int index, int count)
        {
            if (GoodsInfo.ContainsKey(index))
                GoodsInfo[index].UpdatePurchasedCount(count);
            else
                GoodsInfo.Add(index, new ShopGoodsState(index, count));
        }


        public ShopGoodsState GetGoodsState(int goodsNumber)
        {
            if (!GoodsInfo.ContainsKey(goodsNumber))
            {
                GoodsInfo.Add(goodsNumber, new ShopGoodsState(goodsNumber));    
            }

            return GoodsInfo[goodsNumber];
        }

        public AdvertiseState GetAdvertiseState(int advertiseID)
        {
            if (!AdvertiseInfo.ContainsKey(advertiseID))
            {
                AdvertiseInfo.Add(advertiseID, new AdvertiseState(advertiseID));
            }

            return AdvertiseInfo[advertiseID];
        }

        public int GetNeedShowPrivateGoods()
        {
            foreach (var pair in PrivateGoods)
            {
                var goods = GetGoodsState(pair.Key);
                if (goods.IS_VALIDE)
                {
                    if (!shownPrivateGoodsCache.Contains(goods.ID) && !CacheUserData.GetBoolean("private_goods_" + goods.ID, false))
                    {
                        shownPrivateGoodsCache.Add(goods.ID);
                        return goods.ID;
                    }
                }
            }
            return -1;
        }

        public bool IsSoldOutMenu(int menuNum)
        {
            var menuData = ShopMenuData.Get(menuNum);
            if (menuData == null)
                return true;
            foreach(var goods in menuData.ChildGoods)
            {
                var goodsStateData = GetGoodsState(int.Parse(goods.KEY));
                if (goodsStateData.SubscribeState == eStoreSubscribeState.REWARD_ABLE)
                    return false;
                
                if (goodsStateData.RemainGoodsCount > 0) 
                    return false;
            }
            return true;
        }

        
        public void SetShownPrivateGoods(int key)
        {
            CacheUserData.SetBoolean("private_goods_" + key, true);

            if (!CacheUserData.GetBoolean("private_reseted_" + NetworkManager.ServerTag, false))
            {
                CacheUserData.SetBoolean("private_reseted_" + NetworkManager.ServerTag, true);
                const int pack_id_min = 1015000;
                const int pack_id_max = 1015058;
                if (key >= pack_id_min && key <= pack_id_max)
                {
                    foreach (var pair in PrivateGoods)
                    {
                        if (pair.Key >= pack_id_min && pair.Key <= pack_id_max)
                        {
                            var goods = GetGoodsState(pair.Key);
                            if (goods.IS_VALIDE)
                            {
                                shownPrivateGoodsCache.Add(goods.ID);
                                CacheUserData.SetBoolean("private_goods_" + goods.ID, true);
                            }
                        }
                    }
                }
            }
        }

        public List<ShopGoodsState> GetArenaRandomGoods()
        {
            List<ShopGoodsState> ret = new List<ShopGoodsState>();
            foreach (var goods in ArenaRandomGoods)
            {
                ret.Add(GetGoodsState(goods));
            }

            ret.OrderBy(goods => goods.BaseData.SORT);

            return ret;
        }
        public List<ShopGoodsState> GetFriendRandomGoods()
        {
            List<ShopGoodsState> ret = new List<ShopGoodsState>();
            foreach (var goods in FriendPointRandomGoods)
            {
                ret.Add(GetGoodsState(goods));
            }

            ret.OrderBy(goods => goods.BaseData.SORT);

            return ret;
        }
        void SetSubscribeData(JToken response)
        {
            if (response["subs"] != null && SBFunc.IsJArray(response["subs"]))
            {
                foreach (var sub in (JArray)response["subs"])
                {
                    int id = sub["prod"].ToObject<int>();
                    int day = sub["today"].ToObject<int>();
                    int maxday = sub["tot"].ToObject<int>();
                    int nextRewardAbleRemainTime = sub["exp"].ToObject<int>();
                    GoodsInfo[id].UpdateNextSubScribeTime(nextRewardAbleRemainTime);
                    GoodsInfo[id].UpdateSubscribe(day, maxday, day==1);
                }
            }
        }

        public int GetRefreshRemainTimeSubScribe() //갱신하기전에 필요한 가장 낮은 시간 값 찾음
        {
            int temp = int.MaxValue;
            foreach(var goodData in GoodsInfo.Values)
            {
                temp = Mathf.Min(goodData.NextSubscribeGetTime, temp);
            }
            if (temp == int.MaxValue) return -1;
            return temp;
        }

        public void RefreshSubscribe()
        {
            IAPManager.Instance.TrySubscribe(0, eShopIAPCheckType.SubscribeLookUp,
                    (JToken response) =>
                    {
                        SetSubscribeData(response);
                    }, (JToken response) =>
                    {
                        SetSubscribeData(response);
                        Debug.Log(response);
                    });
        }



        public void CheckRefreshGoodsByDayChange()
        {
            if (ShopRefreshTime < TimeManager.GetDailyStartTime())
            {
                WWWForm data = new WWWForm();
                data.AddField("op", (int)eShopIAPCheckType.Refresh);
                NetworkManager.Send("shop/iap", data, (root) =>
                {

                    if (SBFunc.IsJTokenType(root["rs"], JTokenType.Integer))
                    {
                        if ((int)root["rs"] == (int)eApiResCode.OK)
                        {
                            if (SBFunc.IsJObject(root["cnt"]))
                            {
                                foreach (JProperty dat in root["cnt"])
                                {
                                    if(dat != null)
                                        UpdatePurchased(int.Parse(dat.Name), dat.Value.ToObject<int>());
                                }
                                
                            }
                            ShopRefreshTime = TimeManager.GetDateTime();
                        }
                    }
                });
                 
            }
        }



    }
}
