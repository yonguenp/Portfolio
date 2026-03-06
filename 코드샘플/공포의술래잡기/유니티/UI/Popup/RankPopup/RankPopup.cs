using Newtonsoft.Json.Linq;
using SBCommonLib;
using SBSocketSharedLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class RankPopup : Popup
{
    [SerializeField] GameObject TypeA;
    [SerializeField] GameObject TypeB;
    [SerializeField] GameObject RewardedInfo;
    [SerializeField] GameObject Loading;

    [SerializeField] GameObject Board1;
    [SerializeField] GameObject Board2;

    [SerializeField] GameObject newRankItem;
    [SerializeField] GameObject newClanItem;

    [SerializeField] RankRewardInfoItem infoItem;
    [SerializeField] GameObject leftInfo_rank;
    [SerializeField] Text remainTime;
    [SerializeField] GameObject leftInfo_honor;
    [SerializeField] GameObject leftInfo_clan;

    [SerializeField] Button[] btnGroups;
    [SerializeField] newRankItem myRankItem;
    [SerializeField] newClanRankItem myClanRankItem;
    [SerializeField] GameObject noneClanItem;
    [SerializeField] GameObject rewardInfoColumn;
    [SerializeField] Text seassonName;

    public bool bundleType = true;

    private IList<SeasonRankUser> seasonRanker;
    private SeasonRankUser mySeasonRank;

    private TimeSpan updateTime;
    private IEnumerator coroutine = null;
    Action refresh_cb = null;



    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);

        if (bundleType)
            Managers.Network.SendSeasonRank();
    }
    public override void Close()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        base.Close();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        RewardedInfo.SetActive(false);

        if (bundleType)
        {
            TypeB.SetActive(true);
            TypeA.SetActive(false);
            Board1.SetActive(true);
            Board2.SetActive(false);

            var datas = Managers.Data.GetData(GameDataManager.DATA_TYPE.ranking);
            foreach (RankingGameData item in datas)
            {
                if (item.uid == Managers.UserData.MyRankingSeasonID)
                {
                    seassonName.text = item.GetName();
                    break;
                }
            }
        }
        else
        {
            TypeB.SetActive(false);
            TypeA.SetActive(true);
            Board1.SetActive(true);
            Board2.SetActive(false);

            var datas = Managers.Data.GetData(GameDataManager.DATA_TYPE.ranking);
            foreach (RankingGameData item in datas)
            {
                if (item.uid == Managers.UserData.RANK_SEANSON_ID)
                {
                    TypeA.transform.Find("Seasson_name").GetComponent<Text>().text = item.GetName();
                    break;
                }
            }
        }

    }
    public void GetRanDataInit(SCSeasonRank d)
    {
        seasonRanker = d.RankInfos;
        mySeasonRank = d.MyRankInfo;
        bundleType = true;
        RankBoardRefreshUI();
    }
    public void Clear()
    {
        foreach (Transform child in newRankItem.transform.parent)
        {
            if (child == newRankItem.transform)
                continue;
            Destroy(child.gameObject);
        }

        foreach (Transform child in newClanItem.transform.parent)
        {
            if (child == newClanItem.transform)
                continue;
            Destroy(child.gameObject);
        }
    }

    public void OnSubPopup(bool active)
    {
        RewardedInfo.SetActive(active);
        if (active)
            SubRefreshUI();
        else
        {
            foreach (Transform child in infoItem.transform.parent)
            {
                if (child == infoItem.transform)
                    continue;
                Destroy(child.gameObject);
            }
        }
    }
    public void SubRefreshUI()
    {
        var InfoData = Managers.Data.GetData(GameDataManager.DATA_TYPE.ranking_reward);

        foreach (Transform child in infoItem.transform.parent)
        {
            if (child == infoItem.transform)
                continue;
            Destroy(child);
        }

        infoItem.gameObject.SetActive(true);
        int i = 1;
        foreach (RankingRewardData info in InfoData)
        {
            if (info.group != Managers.UserData.seasonData.seasonID)
                continue;
            var obj = GameObject.Instantiate(infoItem, infoItem.transform.parent);
            string temp = $"ui_rank_detail_{i}";
            obj.GetComponent<RankRewardInfoItem>().Setdata(temp, info.reward);
            i++;
        }

        infoItem.gameObject.SetActive(false);
    }
    public bool GetSubPopup()
    {
        return RewardedInfo.activeInHierarchy;
    }

    public void RankBoardRefreshUI()
    {
        //rewardInfoColumn.SetActive(true);
        Clear();
        leftInfo_rank.SetActive(true);
        leftInfo_honor.SetActive(false);
        leftInfo_clan.SetActive(false);
        Board1.SetActive(true);
        Board2.SetActive(false);

        int seasonId = Managers.UserData.RANK_SEANSON_ID;
        if (seasonId == 0)
            Close();

        var rankData = (Managers.Data.GetData(GameDataManager.DATA_TYPE.ranking, seasonId) as RankingGameData);

        //랭크 시간 계산
        var endTime = DateTime.Parse(rankData.GetValue("end_time"));
        InitTime((endTime - SBUtil.KoreanTime), () =>
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_rseason_end"));
        });
        ///////////////////////////////////////////////////////////////////////////////

        if (btnGroups.SingleOrDefault(_ => _.name == "Tab_SeassonRank") != null)
            SetFocusBtn(btnGroups.SingleOrDefault(_ => _.name == "Tab_SeassonRank"));

        newRankItem.SetActive(true);
        myRankItem.gameObject.SetActive(true);
        if (seasonRanker != null && seasonRanker.Count > 0)
        {
            foreach (var item in seasonRanker)
            {
                var obj = GameObject.Instantiate(newRankItem, newRankItem.transform.parent);
                obj.GetComponent<newRankItem>().SetData(item.Nick, item.Rank, item.Point, item.GradePoint);
            }
            Loading.SetActive(false);
        }
        else
        {
            Loading.SetActive(true);
        }

        if (mySeasonRank != null)
            myRankItem.SetData(Managers.UserData.MyName, mySeasonRank.Rank, mySeasonRank.Point, Managers.UserData.MyPoint);
        else
            myRankItem.SetData(Managers.UserData.MyName, -1, 0, Managers.UserData.MyPoint);

        newRankItem.SetActive(false);
    }
    public void HonorBoardRefreshUI()
    {
        rewardInfoColumn.SetActive(false);
        Clear();
        leftInfo_rank.SetActive(false);
        leftInfo_honor.SetActive(true);
        leftInfo_clan.SetActive(false);
        Board1.SetActive(true);
        Board2.SetActive(false);

        if (btnGroups.SingleOrDefault(_ => _.name == "Tab_Honor") != null)
            SetFocusBtn(btnGroups.SingleOrDefault(_ => _.name == "Tab_Honor"));

        if (Managers.UserData.dayRankData == null)
        {
            Loading.SetActive(true);
            myRankItem.SetData(Managers.UserData.MyName, -1, 0, Managers.UserData.MyPoint, false);

            SBWeb.GetRank((response) =>
            {
                HonorBoardRefreshUI();
            });
            return;
        }

        IList<SeasonRankUser> myDayRank = null;
        SeasonRankUser dayRankers = null;

        if (Managers.UserData.dayRankData.Type == JTokenType.Object)
        {
            if (Managers.UserData.dayRankData["my"] != null)
            {
                if (Managers.UserData.dayRankData["my"].Type == JTokenType.Object)
                {
                    JObject myInfo = (JObject)Managers.UserData.dayRankData["my"];
                    if (myInfo != null)
                    {
                        dayRankers = new SeasonRankUser();
                        dayRankers.Nick = myInfo["nick"].Value<string>();
                        dayRankers.Rank = myInfo["index_no"].Value<int>();
                        dayRankers.Point = myInfo["rank_point"].Value<int>();
                        dayRankers.GradePoint = myInfo["rank_point"].Value<int>();
                    }
                }
            }
            if (Managers.UserData.dayRankData["rank"] != null)
            {
                JArray ranks = (JArray)Managers.UserData.dayRankData["rank"];
                if (ranks != null)
                {
                    myDayRank = new List<SeasonRankUser>();
                    foreach (JObject rank in ranks)
                    {
                        SeasonRankUser ranker = new SeasonRankUser();
                        ranker.Nick = rank["nick"].Value<string>();
                        ranker.Rank = rank["index_no"].Value<int>();
                        ranker.Point = rank["rank_point"].Value<int>();
                        ranker.GradePoint = rank["rank_point"].Value<int>();
                        myDayRank.Add(ranker);
                    }
                }
            }
        }

        newRankItem.SetActive(true);
        myRankItem.gameObject.SetActive(true);

        myDayRank = myDayRank.OrderBy(_ => _.Rank).ToList();
        if (myDayRank != null && myDayRank.Count > 0)
        {
            foreach (var item in myDayRank)
            {
                var obj = GameObject.Instantiate(newRankItem, newRankItem.transform.parent);
                obj.GetComponent<newRankItem>().SetData(item.Nick, item.Rank, item.Point, item.GradePoint, false);
            }

            Loading.SetActive(false);
        }
        else
        {
            Loading.SetActive(true);
        }

        if (dayRankers != null)
            myRankItem.SetData(Managers.UserData.MyName, dayRankers.Rank, dayRankers.Point, dayRankers.GradePoint, false);
        else
            myRankItem.SetData(Managers.UserData.MyName, -1, 0, Managers.UserData.MyPoint, false);

        newRankItem.SetActive(false);
    }

    public void ClanBoardRefreshUI()
    {
        rewardInfoColumn.SetActive(false);
        Clear();
        leftInfo_rank.SetActive(false);
        leftInfo_honor.SetActive(false);
        leftInfo_clan.SetActive(true);
        Board1.SetActive(false);
        Board2.SetActive(true);

        if (btnGroups.SingleOrDefault(_ => _.name == "Tab_Clan") != null)
            SetFocusBtn(btnGroups.SingleOrDefault(_ => _.name == "Tab_Clan"));

        if (Managers.UserData.dayClanRankData == null)
        {
            Loading.SetActive(true);
            noneClanItem.SetActive(true);
            myClanRankItem.gameObject.SetActive(false);
            myClanRankItem.SetData(Managers.UserData.MyClanName, -1, 0, 0, 0, 0);

            SBWeb.GetClanRank((response) =>
            {
                ClanBoardRefreshUI();
            });
            return;
        }

        IList<ClanRank> myClanRank = null;
        ClanRank myClan = null;

        if (Managers.UserData.dayClanRankData.Type == JTokenType.Object)
        {
            if (Managers.UserData.dayClanRankData["my"] != null)
            {
                if (Managers.UserData.dayClanRankData["my"].Type == JTokenType.Object)
                {
                    JObject myInfo = (JObject)Managers.UserData.dayClanRankData["my"];
                    if (myInfo != null)
                    {
                        if (myInfo["status"].Value<int>() >= 3)
                        {
                            myClan = new ClanRank();
                            myClan.ClanNo = myInfo["no"].Value<long>();
                            myClan.Rank = myInfo["index_no"].Value<int>();
                            myClan.Point = myInfo["exp"].Value<int>();
                            myClan.Cnt = myInfo["headcount"].Value<int>();
                            myClan.Name = myInfo["name"].Value<string>();
                            myClan.Level = myInfo["level"].Value<int>();
                            myClan.Icon = myInfo["icon"].Value<int>();
                        }
                    }
                }
            }
            if (Managers.UserData.dayClanRankData["rank"] != null)
            {
                JArray ranks = (JArray)Managers.UserData.dayClanRankData["rank"];
                if (ranks != null)
                {
                    int index_no = 1;
                    myClanRank = new List<ClanRank>();
                    foreach (JObject rank in ranks)
                    {
                        ClanRank ranker = new ClanRank();
                        ranker.ClanNo = rank["no"].Value<long>();
                        ranker.Rank = index_no++;
                        ranker.Point = rank["exp"].Value<int>();
                        ranker.Icon = rank["icon"].Value<int>();
                        ranker.Cnt = rank["headcount"].Value<int>();
                        ranker.Name = rank["name"].Value<string>();
                        ranker.Level = rank["level"].Value<int>();
                        myClanRank.Add(ranker);
                    }
                }
            }
        }

        newClanItem.SetActive(true);
        
        myClanRank = myClanRank.OrderBy(_ => _.Rank).ToList();
        if (myClanRank != null && myClanRank.Count > 0)
        {
            foreach (var item in myClanRank)
            {
                var obj = GameObject.Instantiate(newClanItem, newClanItem.transform.parent);
                obj.GetComponent<newClanRankItem>().SetData(item.Name, item.Rank, item.Icon, item.Level, item.Cnt, item.Point);
            }

            Loading.SetActive(false);
        }
        else
        {
            Loading.SetActive(true);
        }

        if (myClan == null)
        {
            noneClanItem.SetActive(true);
            myClanRankItem.gameObject.SetActive(false);
            myClanRankItem.SetData(Managers.UserData.MyClanName, -1, 0, 0, 0, 0);
        }
        else
        {
            noneClanItem.SetActive(false);
            myClanRankItem.gameObject.SetActive(true);
            myClanRankItem.SetData(myClan.Name, myClan.Rank, myClan.Icon, myClan.Level, myClan.Cnt, myClan.Point);
        }

        newClanItem.SetActive(false);
    }

    public void OnStartSeasson()
    {
        TypeA.SetActive(false);
        TypeB.SetActive(true);

        bundleType = true;
        Close();
    }

    public void SetFocusBtn(Button clicked)
    {
        foreach (var item in btnGroups)
        {
            if (item.gameObject.name == "Tab_SeassonRank")
                item.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/friend/tab_friend_04");
            else if (item.gameObject.name == "Tab_Clan")
                item.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/friend/tab_friend_05");
            else
                item.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/friend/tab_friend_06");

            item.GetComponentInChildren<Text>().color = Color.white;
        }

        if (clicked.gameObject.name == "Tab_SeassonRank")
            clicked.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/friend/tab_friend_01");
        else if (clicked.gameObject.name == "Tab_Clan")
            clicked.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/friend/tab_friend_02");
        else
            clicked.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/friend/tab_friend_03");

        clicked.GetComponentInChildren<Text>().color = Color.black;
    }

    public void InitTime(TimeSpan remainTime, Action action = null)
    {
        updateTime = remainTime;
        if (action != null)
            refresh_cb = action;

        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = UpdateTimeCo();
        StartCoroutine(coroutine);

    }

    IEnumerator UpdateTimeCo()
    {
        while (updateTime > TimeSpan.Zero)
        {
            var tempText = string.Empty;
            if (updateTime.Days >= 1)
            {
                tempText += StringManager.GetString("ui_day", updateTime.Days.ToString("D2"));
                tempText += StringManager.GetString("ui_hour", updateTime.Hours.ToString("D2"));
            }
            else
            {
                if (updateTime.Hours >= 1)
                {
                    tempText += StringManager.GetString("ui_hour", updateTime.Hours.ToString("D2"));
                    tempText += StringManager.GetString("ui_min", updateTime.Minutes.ToString("D2"));
                }
                else
                {
                    tempText += StringManager.GetString("ui_min", updateTime.Minutes.ToString("D2"));
                    tempText += StringManager.GetString("ui_second", updateTime.Seconds.ToString("D2"));
                }
            }

            remainTime.text = StringManager.GetString("ui_left_time", tempText);

            yield return new WaitForSeconds(1f);

            updateTime -= new TimeSpan(0, 0, 1);
        }

        if (remainTime.text == string.Empty)
            remainTime.text = StringManager.GetString("time_season_end");

        refresh_cb?.Invoke();
    }


}

public class ClanRank
{
    public long ClanNo { get; set; }
    public int Point { get; set; }      //포인트
    public int Rank { get; set; }       //등수
    public int Cnt { get; set; }       //등급 포인트
    public string Name { get; set; }
    public int Level { get; set; }
    public int Icon { get; set; }
}

public class RankingGameData : GameData
{
    public int uid { get; private set; }
    public string start_time { get; private set; }
    public string end_time { get; private set; }
    public List<RankingRewardData> rewards { get; private set; } = new List<RankingRewardData>();

    public override void SetValue(Dictionary<string, string> tmp)
    {
        base.SetValue(tmp);
        uid = Int(data["uid"]);
        start_time = data["start_time"];
        end_time = data["end_time"];
    }
}

public class RankingRewardData : GameData
{
    public int uid { get; private set; }
    public int group { get; private set; }
    public int ranking_max { get; private set; }
    public int ranking_min { get; private set; }
    public int reward { get; private set; }

    public override void SetValue(Dictionary<string, string> tmp)
    {
        base.SetValue(tmp);
        uid = Int(data["uid"]);
        group = Int(data["group"]);
        ranking_max = Int(data["ranking_max"]);
        ranking_min = Int(data["ranking_min"]);
        reward = Int(data["reward"]);
    }
}

