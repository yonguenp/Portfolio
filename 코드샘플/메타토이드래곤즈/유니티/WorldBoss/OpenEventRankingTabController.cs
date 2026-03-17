using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eOpenEventRankingPage
    {
        NONE,
        CHAMPIONSCUP = 1000001,
        UNIONRANK = 1000004,
        RAIDMASTER = 1000006,
        UNIONRAIDRANK = 1000007,
        CHAMPIONBET_RATIO = 1000008,
        CHAMPIONBET_VALUE = 1000009,
        RESTRICTED_AREA_MAGNET = 1000010,

        RESTRICTED_XMAS1 = 1000011,
        RESTRICTED_XMAS2 = 1000012,
        ARENA_XMAS1 = 1000013,
        ARENA_XMAS2 = 1000014,
        RAID_XMAS1 = 1000015,
        RAID_XMAS2 = 1000016,

        DICE_HOLIDAY = 800003,
    }

    public class OpenEventRankingData : ITableData
    {
        public string icon { get; private set; } = "";
        public int user_no { get; private set; } = 0;
        public string nick { get; private set; } = "";
        public long point { get; private set; } = 0;
        public int rank { get; private set; } = 0;


        public int GuildNo { get; private set; } = 0;
        public string GuildName { get; private set; } = string.Empty;
        public int GuildMarkNo { get; private set; } = 0;
        public int GuildEmblemNo { get; private set; } = 0;

        public PortraitEtcInfoData PortraitData { get; private set; } = null;
        public OpenEventRankingData(JObject rankInfo)
        {
            user_no = rankInfo["user_no"].Value<int>();
            nick = rankInfo["nick"].Value<string>();
            point = rankInfo["point"].Value<long>();
            if(SBFunc.IsJTokenType(rankInfo["rank"], JTokenType.Integer))
                rank = rankInfo["rank"].Value<int>();
            icon = rankInfo["icon"].Value<string>();

            var guildNo = 0;
            if (SBFunc.IsJTokenType(rankInfo["guild_no"], JTokenType.Integer))
                GuildNo = rankInfo["guild_no"].Value<int>();

            if (SBFunc.IsJTokenType(rankInfo["guild_name"], JTokenType.String))
                GuildName = rankInfo["guild_name"].Value<string>();

            if (SBFunc.IsJTokenType(rankInfo["mark_no"], JTokenType.Integer))
                GuildMarkNo = rankInfo["mark_no"].Value<int>();

            if (SBFunc.IsJTokenType(rankInfo["emblem_no"], JTokenType.Integer))
                GuildEmblemNo = rankInfo["emblem_no"].Value<int>();

            if (rankInfo.ContainsKey("portrait") && SBFunc.IsJObject(rankInfo["portrait"]))
                PortraitData = new PortraitEtcInfoData(rankInfo["portrait"]);
        }
        public virtual void Init() { }
        public string GetKey()
        {
            return "";
        }
    }

    
    /// <summary>
    /// 각 탭별로 보상 & 랭킹 세팅
    /// </summary>
    public class OpenEventRankingTabController : MonoBehaviour
    {
        [System.Serializable]
        class Tab
        {
            public eOpenEventRankingPage tab;
            public Button btn;

            public Tab()
            {

            }

            public Tab(eOpenEventRankingPage page, Button btnObj)
            {
                tab = page;
                btn = btnObj;
            }
        }
        [Header("Tab Button")]
        [SerializeField]
        private GameObject tab;
        [SerializeField]
        private List<Tab> tabButtons;

        [SerializeField]
        private TableView rankingInfoTableView = null;
        [SerializeField]
        private TableView rankingGuildTableView = null;

        [SerializeField]
        private TableView rankingRewardTableView = null;

        [SerializeField]
        private OpenEventRankingInfoSlot myRankSlot = null;
        [SerializeField]
        private OpenEventRankingGuildSlot myGuildRankSlot = null;
        [SerializeField]
        private GameObject[] emptyRankingText = null;

        private bool isFirst = false;

        private eOpenEventRankingPage tabIndex = 0;

        public Dictionary<eOpenEventRankingPage, List<OpenEventRankRewardInfoData>> rewards = null;
        private void OnDisable()
        {
            tabIndex = 0;
        }
        public void Init(eOpenEventRankingPage _clickIndex = 0)
        {
            if (isFirst == false)
            {
                rankingInfoTableView?.OnStart();
                rankingRewardTableView?.OnStart();
                rankingGuildTableView?.OnStart();

                if (myRankSlot != null)
                    myRankSlot.InitMyRanking();

                isFirst = true;
            }

            tabIndex = _clickIndex;

            RefreshButton();

            RequestRankingPage();
        }

        void RequestRankingPage()
        {
            var curPage = tabIndex;

            if (curPage == eOpenEventRankingPage.NONE)
                return;

            switch (curPage)
            {
                case eOpenEventRankingPage.CHAMPIONSCUP:
                {
                    rankingInfoTableView.gameObject.SetActive(true);
                    rankingGuildTableView.gameObject.SetActive(false);

                    myRankSlot.gameObject.SetActive(true);
                    myGuildRankSlot.gameObject.SetActive(false);
                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.CHAMPIONSCUP);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshArenaRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.UNIONRANK:
                {
                    rankingInfoTableView.gameObject.SetActive(false);
                    rankingGuildTableView.gameObject.SetActive(true);

                    myRankSlot.gameObject.SetActive(false);
                    myGuildRankSlot.gameObject.SetActive(true);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.UNIONRANK);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshUnionRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;                
                case eOpenEventRankingPage.UNIONRAIDRANK:
                {
                    rankingInfoTableView.gameObject.SetActive(false);
                    rankingGuildTableView.gameObject.SetActive(true);

                    myRankSlot.gameObject.SetActive(false);
                    myGuildRankSlot.gameObject.SetActive(true);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.UNIONRAIDRANK);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshUnionRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.CHAMPIONBET_RATIO:
                {
                    rankingInfoTableView.gameObject.SetActive(true);
                    rankingGuildTableView.gameObject.SetActive(false);

                    myRankSlot.gameObject.SetActive(true);
                    myGuildRankSlot.gameObject.SetActive(false);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.CHAMPIONBET_RATIO);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshRaidRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.CHAMPIONBET_VALUE:
                {
                    rankingInfoTableView.gameObject.SetActive(true);
                    rankingGuildTableView.gameObject.SetActive(false);

                    myRankSlot.gameObject.SetActive(true);
                    myGuildRankSlot.gameObject.SetActive(false);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.CHAMPIONBET_VALUE);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshRaidRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.RESTRICTED_AREA_MAGNET:
                {
                    rankingInfoTableView.gameObject.SetActive(false);
                    rankingGuildTableView.gameObject.SetActive(true);

                    myRankSlot.gameObject.SetActive(false);
                    myGuildRankSlot.gameObject.SetActive(true);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.RESTRICTED_AREA_MAGNET);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshUnionRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.RESTRICTED_XMAS1:
                {
                    rankingInfoTableView.gameObject.SetActive(false);
                    rankingGuildTableView.gameObject.SetActive(true);

                    myRankSlot.gameObject.SetActive(false);
                    myGuildRankSlot.gameObject.SetActive(true);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.RESTRICTED_XMAS1);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshUnionRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.RESTRICTED_XMAS2:
                {
                    rankingInfoTableView.gameObject.SetActive(false);
                    rankingGuildTableView.gameObject.SetActive(true);

                    myRankSlot.gameObject.SetActive(false);
                    myGuildRankSlot.gameObject.SetActive(true);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.RESTRICTED_XMAS2);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshUnionRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.ARENA_XMAS1:
                {
                    rankingInfoTableView.gameObject.SetActive(true);
                    rankingGuildTableView.gameObject.SetActive(false);

                    myRankSlot.gameObject.SetActive(true);
                    myGuildRankSlot.gameObject.SetActive(false);
                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.ARENA_XMAS1);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshArenaRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.ARENA_XMAS2:
                {
                    rankingInfoTableView.gameObject.SetActive(true);
                    rankingGuildTableView.gameObject.SetActive(false);

                    myRankSlot.gameObject.SetActive(true);
                    myGuildRankSlot.gameObject.SetActive(false);
                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.ARENA_XMAS2);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshArenaRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.RAID_XMAS1:
                {
                    rankingInfoTableView.gameObject.SetActive(true);
                    rankingGuildTableView.gameObject.SetActive(false);

                    myRankSlot.gameObject.SetActive(true);
                    myGuildRankSlot.gameObject.SetActive(false);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.RAID_XMAS1);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshRaidRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
                case eOpenEventRankingPage.RAID_XMAS2:
                {
                    rankingInfoTableView.gameObject.SetActive(true);
                    rankingGuildTableView.gameObject.SetActive(false);

                    myRankSlot.gameObject.SetActive(true);
                    myGuildRankSlot.gameObject.SetActive(false);

                    WWWForm form = new WWWForm();
                    form.AddField("event_key", (int)eOpenEventRankingPage.RAID_XMAS2);

                    NetworkManager.Send("event/ranking", form, (JObject jsonData) =>
                    {
                        RefreshRaidRankingScrollview(jsonData);//서버에서 리스폰스 성공 이후에 호출해야함.
                    });
                }
                break;
            }

            SetRankingRewardTable();
        }

        int RefreshDefenceBattlePoint()
        {
            var defBattleLine = User.Instance.PrefData.ArenaFormationData.TeamFormationDEF[0];
            return CalcTotalBattlePoint(defBattleLine);
        }
        int CalcTotalBattlePoint(List<int> dragonTagList)
        {
            var totalPoint = 0;
            if (dragonTagList == null || dragonTagList.Count <= 0)
            {
                return totalPoint;
            }
            foreach (int tag in dragonTagList)
            {
                var dragonData = User.Instance.DragonData.GetDragon(tag);
                if (dragonData == null) continue;

                totalPoint += (int)dragonData.Status.GetTotalINF();//레벨 스탯, 장비, 등등 포함 총 값
            }
            return totalPoint;
        }

        protected void RefreshArenaRankingScrollview(JObject jsonData)
        {
            if (jsonData["ranking"] != null)
            {
                var rankingList = new List<ITableData>();
                if (jsonData["ranking"].Type == JTokenType.Array)
                {
                    foreach (var arrToken in jsonData["ranking"].ToObject<JArray>())
                    {
                        if (!arrToken.HasValues)
                            continue;

                        OpenEventRankingData tempMatchDataSet = new OpenEventRankingData((JObject)arrToken);
                        rankingList.Add(tempMatchDataSet);
                    }
                }

                var isEmpty = rankingList.Count <= 0;
                if (emptyRankingText != null)
                {
                    foreach(var t in emptyRankingText)
                        t.SetActive(isEmpty);
                }

                rankingInfoTableView.SetDelegate(
                     new TableViewDelegate(rankingList, (GameObject node, ITableData item) => {
                         if (node == null) return;
                         var slotInfo = node.GetComponent<OpenEventRankingInfoSlot>();
                         if (slotInfo == null) return;
                         slotInfo.Init((OpenEventRankingData)item);
                         node.SetActive(true);
                     }));
                rankingInfoTableView.ReLoad();
            }

            if (jsonData.ContainsKey("mine") && jsonData["mine"].Type == JTokenType.Object)
            {
                var myData = new OpenEventRankingData((JObject)jsonData["mine"]);
                myRankSlot?.Init(myData);
            }
            else
            {
                myRankSlot.gameObject.SetActive(false);
            }
        }

        protected void RefreshUnionRankingScrollview(JObject jsonData)
        {
            if (jsonData["ranking"] != null)
            {
                var rankingList = new List<ITableData>();
                if (jsonData["ranking"].Type == JTokenType.Array)
                {
                    foreach (var arrToken in jsonData["ranking"].ToObject<JArray>())
                    {
                        if (!arrToken.HasValues)
                            continue;

                        OpenEventRankingData tempMatchDataSet = new OpenEventRankingData((JObject)arrToken);
                        rankingList.Add(tempMatchDataSet);
                    }
                }

                var isEmpty = rankingList.Count <= 0;
                if (emptyRankingText != null)
                {
                    foreach (var t in emptyRankingText)
                        t.SetActive(isEmpty);
                }

                rankingGuildTableView.SetDelegate(
                     new TableViewDelegate(rankingList, (GameObject node, ITableData item) => {
                         if (node == null) return;
                         var slotInfo = node.GetComponent<OpenEventRankingGuildSlot>();
                         if (slotInfo == null) return;
                         slotInfo.Init((OpenEventRankingData)item);
                         node.SetActive(true);
                     }));
                rankingGuildTableView.ReLoad();
            }

            if (jsonData.ContainsKey("mine") && jsonData["mine"].Type == JTokenType.Object)
            {
                var myData = new OpenEventRankingData((JObject)jsonData["mine"]);
                myGuildRankSlot?.Init(myData);
            }
            else
            {
                myGuildRankSlot.gameObject.SetActive(false);
            }
        }

        protected void RefreshRaidRankingScrollview(JObject jsonData)
        {
            if (jsonData["ranking"] != null)
            {
                var rankingList = new List<ITableData>();
                if (jsonData["ranking"].Type == JTokenType.Array)
                {
                    foreach (var arrToken in jsonData["ranking"].ToObject<JArray>())
                    {
                        if (!arrToken.HasValues)
                            continue;

                        OpenEventRankingData tempMatchDataSet = new OpenEventRankingData((JObject)arrToken);
                        rankingList.Add(tempMatchDataSet);
                    }
                }

                var isEmpty = rankingList.Count <= 0;
                if (emptyRankingText != null)
                {
                    foreach (var t in emptyRankingText)
                        t.SetActive(isEmpty);
                }

                rankingInfoTableView.SetDelegate(
                     new TableViewDelegate(rankingList, (GameObject node, ITableData item) => {
                         if (node == null) return;
                         var slotInfo = node.GetComponent<OpenEventRankingInfoSlot>();
                         if (slotInfo == null) return;
                         slotInfo.Init((OpenEventRankingData)item);
                         node.SetActive(true);
                     }));
                rankingInfoTableView.ReLoad();
            }

            if (jsonData.ContainsKey("mine") && jsonData["mine"].Type == JTokenType.Object)
            {
                var myData = new OpenEventRankingData((JObject)jsonData["mine"]);
                myRankSlot?.Init(myData);
            }
            else
            {
                myRankSlot.gameObject.SetActive(false);
            }
        }

        List<WorldBossRankingUserData> GetRankingData(JObject _jsonData, bool isMine = false)
        {
            List<WorldBossRankingUserData> ret = new List<WorldBossRankingUserData>();

            var properties = _jsonData.Properties();
            foreach (JProperty it in properties)
            {
                var rankInfo = (JObject)it.Value;

                var rank = rankInfo["rank"].Value<int>();
                var userNo = isMine ? User.Instance.UserAccountData.UserNumber : rankInfo["user_no"].Value<long>();
                var nick = isMine ? User.Instance.UserData.UserNick : rankInfo["nick"].Value<string>();
                var userPortrait = isMine ? User.Instance.UserData.UserPortrait : rankInfo["icon"].Value<string>();
                var userLevel = isMine ? User.Instance.UserData.Level : rankInfo["level"].Value<int>();
                var userBattlePoint = rankInfo["combat_power"].Value<int>();
                uint highScore = rankInfo["high_score"].Value<uint>();
                string prefix_total = rankInfo["total_high"].Value<string>();
                string postfix_total = rankInfo["total_low"].Value<string>();
                string totalPoint = prefix_total == "0" ? postfix_total : prefix_total + postfix_total;

                var guildNo = 0;
                if (SBFunc.IsJTokenType(rankInfo["guild_no"], JTokenType.Integer))
                    guildNo = rankInfo["guild_no"].Value<int>();

                var guildName = "";
                if (guildNo > 0 && SBFunc.IsJTokenType(rankInfo["guild_name"], JTokenType.String))
                    guildName = rankInfo["guild_name"].Value<string>();

                var guildMarkNo = 0;
                if (guildNo > 0 && SBFunc.IsJTokenType(rankInfo["mark_no"], JTokenType.Integer))
                    guildMarkNo = rankInfo["mark_no"].Value<int>();

                var guildEmblemNo = 0;
                if (guildNo > 0 && SBFunc.IsJTokenType(rankInfo["emblem_no"], JTokenType.Integer))
                    guildEmblemNo = rankInfo["emblem_no"].Value<int>();

                PortraitEtcInfoData tempPortraitData = null;

                if(isMine)//userData 에서 가져옴 - 해당 api 들어올 때 push api 들어옴(portrait_update)
                    tempPortraitData = User.Instance.UserData.UserPortraitFrameInfo;
                else
                {
                    if (rankInfo.ContainsKey("portrait") && SBFunc.IsJObject(rankInfo["portrait"]))
                        tempPortraitData = new PortraitEtcInfoData(rankInfo["portrait"]);
                }

                WorldBossRankingUserData info = new WorldBossRankingUserData(userNo , nick, userPortrait, userLevel, userBattlePoint, highScore, totalPoint,rank, guildNo, guildName, guildMarkNo, guildEmblemNo, tempPortraitData);

                ret.Add(info);
            }

            return ret;
        }

        public void SetRewards(Dictionary<eOpenEventRankingPage, List<OpenEventRankRewardInfoData>> r)
        {
            rewards = r;
            SetRankingRewardTable();
        }

        void SetRankingRewardTable()
        {
            if (rankingRewardTableView == null) return;
            if (rewards == null) return;
            if (!rewards.ContainsKey(tabIndex))
                return;

            List<ITableData> rankRewardData = new List<ITableData>();
            foreach(var r in rewards[tabIndex])
            {
                rankRewardData.Add(r);
            }

            rankingRewardTableView.SetDelegate(
                new TableViewDelegate(rankRewardData, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<OpenEventRankingRewardSlot>();
                    if (slotInfo == null) return;
                    var data = (OpenEventRankRewardInfoData)item;
                    slotInfo.Init(data.Key, data.startRank, data.endRank, data.Items);
                }));
            rankingRewardTableView.ReLoad();
        }

        public void OnClickTabButton(int _index)
        {
            if (tabButtons.Count <= _index)
                return;

            eOpenEventRankingPage page = tabButtons[_index].tab;
            if (tabIndex == page)
                return;

            Init(page);
        }
        public void SetTabList(List<eOpenEventRankingPage> pages)
        {
            if (tab != null)
            {
                foreach (Transform child in tab.transform.parent)
                {
                    if(child != tab.transform)
                        Destroy(child.gameObject);
                }
                tabButtons.Clear();

                tab.gameObject.SetActive(true);
                foreach(var page in pages)
                {
                    var newTab = Instantiate(tab, tab.transform.parent);
                    var TabData = new Tab(page, newTab.GetComponent<Button>());
                    
                    Text text = TabData.btn.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        string key = "event:tab:" + (int)page;
                        if (text.GetComponent<LocalizeString>() == null)
                            text.text = StringData.GetStringByStrKey(key);
                    }

                    int index = tabButtons.Count;
                    TabData.btn.onClick.RemoveAllListeners();
                    TabData.btn.onClick.AddListener(() => { OnClickTabButton(index); });

                    tabButtons.Add(TabData);
                }
                
                tab.gameObject.SetActive(false);
            }
        }
        public virtual void RefreshButton()
        {
            foreach (var tab in tabButtons)
            {
                int no = (int)tab.tab;

                string key = "event:tab:" + no;
                if (StringData.HasStringKey(key))
                {
                    Text text = tab.btn.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        if (text.GetComponent<LocalizeString>() == null)
                            text.text = StringData.GetStringByStrKey(key);
                    }
                }
                

                if(tab != null)
                    tab.btn.interactable = tab.tab != tabIndex;
            }
            
        }
    }
}
