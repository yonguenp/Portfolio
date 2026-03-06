using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OpenEventRankRewardInfoData : ITableData
{
    public int startRank { get; private set; } = -1;
    public int endRank { get; private set; } = -1;
    public List<Asset> Items { get; private set; } = null;
    public int Key { get; private set; } = -1;
    public OpenEventRankRewardInfoData(eOpenEventRankingPage key, int start_rank, int end_rank, List<Asset> assets)
    {
        Key = (int) key;
        endRank = end_rank;
        startRank = start_rank;
        Items = assets;
    }
    public string GetKey() { return Key.ToString(); }
    public void Init() { }
}

public class EventRankingPopupData : PopupData
{
    public eOpenEventRankingPage page { get; private set; } = eOpenEventRankingPage.NONE;

    public EventRankingPopupData(int event_id)
    {
        switch (event_id)
        {
            case 1000004:
                page = eOpenEventRankingPage.UNIONRANK;
                break;
            case 1000006:
                page = eOpenEventRankingPage.RAIDMASTER;
                break;
            case 1000007:
                page = eOpenEventRankingPage.UNIONRAIDRANK;
                break;
            case 1000008:
                page = eOpenEventRankingPage.CHAMPIONBET_RATIO;
                break;
            case 1000009:
                page = eOpenEventRankingPage.CHAMPIONBET_VALUE;
                break;
            case 1000010:
                page = eOpenEventRankingPage.RESTRICTED_AREA_MAGNET;
                break;
            case 1000001:
            default:
                page = eOpenEventRankingPage.CHAMPIONSCUP;
                break;
        }
    }
}


public class OpenEventRankingPopup : Popup<EventRankingPopupData>
{
    [SerializeField] protected OpenEventRankingTabController controller = null;


    Dictionary<eOpenEventRankingPage, List<OpenEventRankRewardInfoData>> rewards = null;
    public override void InitUI()
    {
        eOpenEventRankingPage page = 0;
        if(Data != null)
        {
            page = Data.page;
        }

        if (controller != null)
            controller.Init(page);
    }

    private void OnEnable()
    {
        if (rewards == null)
        {
            rewards = new Dictionary<eOpenEventRankingPage, List<OpenEventRankRewardInfoData>>();
            NetworkManager.Send("event/list", null, (JObject jsonData) =>
            {
                if (jsonData.ContainsKey("list"))
                {
                    if (jsonData["list"].Type == JTokenType.Array)
                    {
                        foreach (JObject obj in (JArray)jsonData["list"])
                        {
                            eOpenEventRankingPage key = 0;
                            if (!obj.ContainsKey("key") || obj["key"].Type != JTokenType.Integer)
                                continue;

                            key = (eOpenEventRankingPage)obj["key"].Value<int>();
                            if (rewards.ContainsKey(key))
                                rewards.Remove(key);

                            if (obj.ContainsKey("rewards") && obj["rewards"].Type == JTokenType.Array)
                            {
                                rewards.Add(key, new List<OpenEventRankRewardInfoData>());
                                foreach (JObject val in (JArray)obj["rewards"])
                                {
                                    int start = -1;                                    
                                    if (((JArray)val["rank_range"]).Count > 0)
                                        start = val["rank_range"][0].Value<int>();

                                    int end = start;
                                    if (((JArray)val["rank_range"]).Count > 1)
                                        end = val["rank_range"][1].Value<int>();

                                    List<Asset> assets = new List<Asset>();
                                    foreach(var r in (JArray)val["web2"])
                                    {
                                        assets.Add(new Asset(r));
                                    }

                                    rewards[key].Add(new OpenEventRankRewardInfoData(key, start, end, assets));
                                }
                            }
                        }

                        controller.SetRewards(rewards);
                    }
                }
            });
        }
    }
}
