using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class ExchangeTable : TableBase<ExchangeData, DBExchange_base>
    {
    }

    public class ExchangeGroupTable : TableBase<ExchangeGroupData, DBExchange_group>
    {
    }

    public class Exchange
    {
        public enum EXCHANGE_STATE 
        {
            WAIT,
            HIDE,
            BEGGING,
        }

        public int slot_id { get; private set; } = -1;
        public int dragon_no { get; private set; } = -1;
        public DateTime regist_time { get; private set; } = DateTime.MinValue;
        
        private int need1 = -1;
        private int need2 = -1;
        private int need3 = -1;
        private int need4 = -1;

        public List<ExchangeGroupData> NeedItemInfo { get; private set; } = new List<ExchangeGroupData>();

        public EXCHANGE_STATE State { 
            get {
                if (need1 <= 0)
                {
                    PlayerPrefs.SetInt("EXCHANGE_" + slot_id.ToString(), 1);
                    return EXCHANGE_STATE.WAIT;
                }

                if (PlayerPrefs.GetInt("EXCHANGE_" + slot_id.ToString(), 0) > 0)
                {
                    if((regist_time < TimeManager.GetDailyStartTime()))
                    {
                        PlayerPrefs.SetInt("EXCHANGE_" + slot_id.ToString(), 0);
                        return EXCHANGE_STATE.BEGGING;
                    }

                    return EXCHANGE_STATE.HIDE;
                }

                return EXCHANGE_STATE.BEGGING;
            } 
        }

        public Exchange(int slot)
        {
            slot_id = slot;
        }

        public void OnData(int id, JObject data)
        {
            Clear();

            slot_id = id;
            regist_time = SBFunc.DateTimeParse(data["regist_time"].Value<string>());

            dragon_no = data["dragon_no"].Value<int>();

            need1 = data["need1"].Value<int>();
            NeedItemInfo.Add(User.Instance.Exchange.GetExchangeGroupData(need1));
            need2 = data["need2"].Value<int>();
            NeedItemInfo.Add(User.Instance.Exchange.GetExchangeGroupData(need2));
            need3 = data["need3"].Value<int>();
            NeedItemInfo.Add(User.Instance.Exchange.GetExchangeGroupData(need3));
            need4 = data["need4"].Value<int>();
            NeedItemInfo.Add(User.Instance.Exchange.GetExchangeGroupData(need4));

            NeedItemInfo.RemoveAll(o => o == null);
        }

        public void Clear()
        {
            regist_time = DateTime.MinValue;
            need1 = -1;
            need2 = -1;
            need3 = -1;
            need4 = -1;

            NeedItemInfo.Clear();
        }
    }

    public class ExchangeManager
    {
        const int DailyRewardCount = 3;
        public int FIRST_EXCHANGE_DAILY_REWARD_COUNT    { get { return GetConfig("FIRST_EXCHANGE_DAILY_REWARD_COUNT"); } }
        public int SECOND_EXCHANGE_DAILY_REWARD_COUNT   { get { return GetConfig("SECOND_EXCHANGE_DAILY_REWARD_COUNT"); } }
        public int THIRD_EXCHANGE_DAILY_REWARD_COUNT    { get { return GetConfig("THIRD_EXCHANGE_DAILY_REWARD_COUNT"); } }
        public int EXCHANGE_RENEW_LEAD_TIME             { get { return GetConfig("EXCHANGE_RENEW_LEAD_TIME"); } }
        public int FIRST_DAILY_REWARD_ITEM_GROUP        { get { return GetConfig("FIRST_DAILY_REWARD_ITEM_GROUP"); } }
        public int SECOND_DAILY_REWARD_ITEM_GROUP       { get { return GetConfig("SECOND_DAILY_REWARD_ITEM_GROUP"); } }
        public int THIRD_DAILY_REWARD_ITEM_GROUP        { get { return GetConfig("THIRD_DAILY_REWARD_ITEM_GROUP"); } }
        public int EXCHANGE_DELIVERY_MIN_NUM            { get { return GetConfig("EXCHANGE_DELIVERY_MIN_NUM"); } }
        public int EXCHANGE_DELIVERY_MAX_NUM            { get { return GetConfig("EXCHANGE_DELIVERY_MAX_NUM"); } }

        public Exchange[] Exchange { get; private set; } = new Exchange[4];
        public int ClearCount { get; private set; } = 0;
        public bool[] Rewarded { get; private set; } = new bool[DailyRewardCount];

        private Dictionary<string, int> ConfigValues = null;
        private Dictionary<ExchangeData, List<ExchangeGroupData>> ExchangeData = null;
        
        bool NeedRefresh
        {
            get
            {
                foreach (var ex in Exchange)
                {
                    if (ex == null)
                        return true;

                    switch (ex.State)
                    {
                        case SandboxNetwork.Exchange.EXCHANGE_STATE.WAIT:
                            int remainTime = TimeManager.GetTimeCompare(ex.regist_time);
                            if (remainTime < 0)
                                return true;
                            break;
                    }

                }

                return false;
            }
        }

        public bool Requesting = false;

        private int GetConfig(string key)
        {
            if(ConfigValues.ContainsKey(key))
            {
                if(ConfigValues[key] >= 0)
                    return ConfigValues[key];
            }

            return -1;
        }

        public void Clear()
        {
            foreach(var data in Exchange)
            {
                if(data != null)
                    data.Clear();   
            }

            ClearCount = 0;
            Rewarded = new bool[DailyRewardCount] { true, true, true };
        }

        public void Init()
        {
            ConfigValues = new Dictionary<string, int>();
            string[] configKeys = {
                "FIRST_EXCHANGE_DAILY_REWARD_COUNT",
                "SECOND_EXCHANGE_DAILY_REWARD_COUNT",
                "THIRD_EXCHANGE_DAILY_REWARD_COUNT",
                "EXCHANGE_RENEW_LEAD_TIME",
                "FIRST_DAILY_REWARD_ITEM_GROUP",
                "SECOND_DAILY_REWARD_ITEM_GROUP",
                "THIRD_DAILY_REWARD_ITEM_GROUP",
                "EXCHANGE_DELIVERY_MIN_NUM",
                "EXCHANGE_DELIVERY_MAX_NUM",
            };

            foreach(string key in configKeys)
            {
                var config = GameConfigTable.Instance.Get(key);
                if (config != null)
                {
                    ConfigValues[key] = Convert.ToInt32(config.VALUE);
                }
            }

            ExchangeData = new Dictionary<ExchangeData, List<ExchangeGroupData>>();
            if (TableManager.GetTable<ExchangeTable>() == null) return;
            foreach (var data in TableManager.GetTable<ExchangeTable>().GetAllList())
            {
                foreach (var groupData in TableManager.GetTable<ExchangeGroupTable>().GetAllList())
                {
                    if (groupData.GROUP != data.KEY)
                        continue;

                    if (!ExchangeData.ContainsKey(data))
                        ExchangeData.Add(data, new List<ExchangeGroupData>());

                    ExchangeData[data].Add(groupData);
                }
            }

            Clear();
        }

        public ExchangeData GetExchangeData(int key)
        {
            return TableManager.GetTable<ExchangeTable>().Get(key);
        }

        public ExchangeData GetExchangeDataByGroupData(ExchangeGroupData data)
        {
            if (data == null)
                return null;
            
            return GetExchangeData(data.GROUP);
        }

        public ExchangeGroupData GetExchangeGroupData(int key)
        {
            return TableManager.GetTable<ExchangeGroupTable>().Get(key);
        }

        public void OnExchangeData(JToken response, VoidDelegate rewardCloseCB = null)
        {
            Clear();

            JObject data = (JObject)response;
            List<Asset> rewards = new List<Asset>();

            if (data.ContainsKey("exchange"))
            {
                JArray exchange = (JArray)data["exchange"];

                foreach (JObject exData in exchange)
                {
                    int id = exData["slot_id"].Value<int>();
                    if (Exchange[id - 1] == null)
                        Exchange[id - 1] = new Exchange(id);

                    Exchange[id - 1].OnData(id, exData);
                    if(exData.ContainsKey("reward"))
                    {
                        foreach (var reward in (JArray)exData["reward"])
                        {
                            rewards.Add(new Asset(reward));
                        }
                    }
                }
            }

            if(data.ContainsKey("info"))
            {
                JObject info = (JObject)data["info"];

                ClearCount = info["count"].Value<int>();
                Rewarded[0] = info["first"].Value<int>() > 0;
                Rewarded[1] = info["second"].Value<int>() > 0;
                Rewarded[2] = info["third"].Value<int>() > 0;

                if (info.ContainsKey("reward"))
                {
                    foreach (var reward in (JArray)info["reward"])
                    {
                        rewards.Add(new Asset(reward));
                    }
                }
            }

            if(rewards.Count > 0)
            {
                SystemRewardPopup.OpenPopup(rewards, rewardCloseCB, true);
            }
            else
            {
                rewardCloseCB?.Invoke();
            }
        }

        public void Prepare(Action cb = null)
        {
            if (NeedRefresh && !Requesting)
            {
                Requesting = true;
                NetworkManager.Send("exchange/exchange", null, (data) =>
                {
                    OnExchangeData(data);
                    cb?.Invoke();

                    Requesting = false;
                }, (fail) => {

                    JObject root = null;
                    if (!string.IsNullOrEmpty(fail))
                    {
                        root = JObject.Parse(fail);
                        if(root != null)
                        {
                            NetworkManager.Instance.OnError(root);
                        }
                    }

                    Requesting = false; 
                });
            }
            else
            {
                cb?.Invoke();
            }
        }

        public void OnRefresh(int slot_id, Action cb = null)  // 갱신하기
        {
            WWWForm data = new WWWForm();
            data.AddField("slot_id", slot_id);
            data.AddField("refresh", 1);

            NetworkManager.Send("exchange/exchange", data, (data) => {
                OnExchangeData(data);
                cb?.Invoke();
            });
        }

        public void OnExchange(int slot_id, ExchangeObject.OnExchangeCallback rewardCloseCB = null) // 건네주기 
        {
            WWWForm data = new WWWForm();
            data.AddField("slot_id", slot_id);
            data.AddField("refresh", 0);

            NetworkManager.Send("exchange/exchange", data, (data) => {
                if (data.ContainsKey("rs") &&(eApiResCode)data["rs"].ToObject<int>() == eApiResCode.COST_SHORT)
                {
                    rewardCloseCB?.Invoke(null);
                    ToastManager.On(100002249);
                    return;
                }

                rewardCloseCB?.Invoke(data);
            });
        }

        public void OnReward(int reward, Action cb = null) // 일일보상 
        {
            WWWForm data = new WWWForm();
            data.AddField("reward", reward);

            NetworkManager.Send("exchange/exchange", data, (data) => {
                OnExchangeData(data);

                //todo 보상
                cb?.Invoke();
            });
        }
    }
}