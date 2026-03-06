using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SandboxNetwork;
using System.Linq;

public class DragonInfo
{
    public int Tag { get; private set; } = 0;
    public int Level { get; private set; } = 0;
    public int TranscendenceStep { get; private set; } = 0;

    public DragonInfo(int tag, int lv, int transcendenceStep = 0)
    {
        Tag = tag;
        Level = lv;
        TranscendenceStep = transcendenceStep;
    }
}
public class LogStruct
{
    public int time;
    public List<string> log;
    public LogStruct(int time, List<string> log)
    {
        this.time = time;
        this.log = log;
    }
}
public class ArenaTeamData : ThumbnailUserData
{
    public int SlotID { get; set; } = -1;
    public int CombatID { get; set; } = 0;
    public List<DragonInfo> AtkDeck { get; private set; } = new List<DragonInfo>();
    public List<DragonInfo> DefDeck { get; private set; } = new List<DragonInfo>();
    public int AtkBattlePoint { get; set; } = 0;
    public int DefBattlePoint { get; set; } = 0;
    /// <summary> 방어 기록 표시에 증감 상태(방어 실패, 방어 성공) </summary>
    public int ChangePoint { get; set; } = 0; 
    public string Status { get; set; } = "";//전투를 지금 안했는지(0) 이겻는지(1) 졌는지(2) (복수 가능 / 이김 / 짐)
    public int Rank { get; set; } = 0; //유저 랭크(랭크 리스트용)
    public eArenaRankGrade RankGrade { get; set; } = eArenaRankGrade.NONE;
    public int Point { get; set; } = 0;
    public int GuildNo { get; set; } = 0;
    public string GuildName { get; set; } = string.Empty;
    public int GuildMarkNo { get; set; } = 0;
    public int GuildEmblemNo { get; set; } = 0;

    public int log_time = 0;
    public int day = 0; //복수 리스트 시간 일
    public int hour = 0;//복수 리스트 시간 시 
    public int minutes = 0; //복수 리스트 시간 분

    public void InitData()
    {
        UID = 0;
        SlotID = -1;
        AtkDeck = new List<DragonInfo>();
        DefDeck = new List<DragonInfo>();
        PortraitIcon = "";
        Level = 1;
        Point = 0;
        AtkBattlePoint = 0;
        DefBattlePoint = 0;
        ChangePoint = 0;
        Status = "";
        Nick = "";
        log_time = 0;
        day = 0;
        hour = 0;
        minutes = 0;
        Rank = 0;
        RankGrade = eArenaRankGrade.NONE;
        EtcInfo = null;
        GuildName = "";
        GuildMarkNo = 0;
    }
    public ArenaTeamData() { }
    public ArenaTeamData(JObject jsonData)
    {
        if (SBFunc.IsJTokenCheck(jsonData["slot"]))
            SlotID = jsonData["slot"].Value<int>();

        if (SBFunc.IsJTokenCheck(jsonData["trophy_point"]))
            Point = (int)jsonData["trophy_point"];

        if (jsonData.ContainsKey("atkDeck"))
        {
            foreach (var dragonDataSet in jsonData["atkDeck"])
            {
                if (dragonDataSet == null || dragonDataSet.Type != JTokenType.Array)
                    continue;

                JArray dragonInfo = (JArray)dragonDataSet;
                if ((int)dragonInfo[0] == 0)
                    continue;

                DragonInfo tempDataSet = new DragonInfo((int)dragonInfo[0], (int)dragonInfo[1], dragonInfo.Count > 2 ? (int)dragonInfo[2] : 0);
                AtkDeck.Add(tempDataSet);
            }
            if (jsonData.ContainsKey("atkBp"))
                AtkBattlePoint = (int)jsonData["atkBp"];
        }

        if (jsonData.ContainsKey("defDeck"))
        {
            foreach (var dragonDataSet in jsonData["defDeck"])
            {
                if (dragonDataSet == null || dragonDataSet.Type != JTokenType.Array)
                    continue;

                JArray dragonInfo = (JArray)dragonDataSet;
                DragonInfo tempDataSet = new DragonInfo((int)dragonInfo[0], (int)dragonInfo[1], dragonInfo.Count > 2 ? (int)dragonInfo[2] : 0);
                DefDeck.Add(tempDataSet);
            }
            if (jsonData.ContainsKey("defBp"))
                DefBattlePoint = (int)jsonData["defBp"];
        }

        if (jsonData.ContainsKey("day"))
        {
            day = (int)jsonData["day"];
        }
        if (jsonData.ContainsKey("hour"))
        {
            hour = (int)jsonData["hour"];
        }
        if (jsonData.ContainsKey("minutes"))
        {
            minutes = (int)jsonData["minutes"];
        }
        if (jsonData.ContainsKey("combat_id"))
        {
            CombatID = (int)jsonData["combat_id"];
        }
        if (jsonData.ContainsKey("rank"))
        {
            Rank = (int)jsonData["rank"];
        }
        if (jsonData.ContainsKey("points"))
        {
            ChangePoint = (int)jsonData["points"];
        }
        if (jsonData.ContainsKey("icon"))
        {
            PortraitIcon = (string)jsonData["icon"];
        }
        if (jsonData.ContainsKey("level"))
        {
            Level = (int)jsonData["level"];
        }
        if (jsonData.ContainsKey("name"))
        {
            Nick = (string)jsonData["name"];
        }
        if (jsonData.ContainsKey("status"))
        {
            Status = (string)jsonData["status"];
        }
        if (jsonData.ContainsKey("rank_grade"))
        {
            RankGrade = (eArenaRankGrade)jsonData["rank_grade"].Value<int>();
        }
        if (jsonData.ContainsKey("portrait"))
        {
            EtcInfo = new PortraitEtcInfoData(jsonData["portrait"]);
        }
        if (jsonData.ContainsKey("portrait"))
        {
            EtcInfo = new PortraitEtcInfoData(jsonData["portrait"]);
        }
        if (jsonData.ContainsKey("guild_no"))
        {
            GuildNo = (int)jsonData["guild_no"];
        }
        if (jsonData.ContainsKey("guild_name"))
        {
            GuildName = (string)jsonData["guild_name"];
        }
        if (jsonData.ContainsKey("emblem_no"))
        {
            GuildEmblemNo = (int)jsonData["emblem_no"];
        }
        if (jsonData.ContainsKey("mark_no"))
        {
            GuildMarkNo = (int)jsonData["mark_no"];
        }

    }
}

public class RestrictedAreaTeamData : ArenaTeamData
{    
    public StageDifficult Difficult { get; private set; }
    public void SetDiffficult(StageDifficult diff)
    {
        Difficult = diff;
    }
}

public class ArenaResultDragonStat
{
    public ArenaResultDragonStat(char bTag, int dTag, int level, JArray jsonArray)
    {
        if (jsonArray == null || jsonArray.Count != 3)
            return;

        this.BTag = bTag;
        this.DTag = dTag;
        this.Level = level;
        Damage = jsonArray[0].Value<int>();
        TakenDamage = jsonArray[1].Value<int>();
        TrueDamage = jsonArray[2].Value<int>();
    }
    public char BTag
    {
        get;
        private set;
    }
    public int DTag
    {
        get;
        private set;
    }
    public int Level
    {
        get;
        private set;
    }
    public int Damage
    {
        get;
        private set;
    }
    public int TakenDamage
    {
        get;
        private set;
    }
    public int TrueDamage
    {
        get;
        private set;
    }
}

public class ArenaBaseData
{
    public enum SeasonType
    {
        Unkwown = -1,
        PreSeason = 0,
        RegularSeason = 1,
    }
    public int season_id { get; private set; } = 0;
    public SeasonType season_type { get; private set; } = SeasonType.Unkwown;
    public int offence_count { get; private set; } = 0;
    public int offence_failuer_count { get; private set; } = 0;
    public int defence_count { get; private set; } = 0;
    public int defence_failure_count { get; private set; } = 0;

    public int season_point { get; private set; } = 0;
    public int season_rank { get; private set; } = 0;
    public eArenaRankGrade SeasonGrade { get; private set; } = eArenaRankGrade.NONE;
    
    public List<int> season_reward_step { get; private set; } = new List<int>();
    public int season_start { get; private set; } = 0;
    public int season_remain_time { get; private set; } = 0;

    public int Arena_Match_Refresh_Check { get; private set; } = 0;
    public int Arena_Ticket { get; private set; } = 0;
    public int Arena_Ticket_refill_count { get; private set; } = 0;
    public int Arena_Ticket_refill_check { get; private set; } = 0;
    public int Arena_Ticket_Exp { get; private set; } = 0;

    public eElementType ATK_ELEM { get; private set; } = eElementType.None;
    public eElementType DEF_ELEM { get; private set; } = eElementType.None;
    public eElementType HP_ELEM { get; private set; } = eElementType.None;

    // 아레나 랭크 변동 관련 파트
    public bool isRankChange { get; private set; } = false;
    public eArenaRankGrade LastUserGrade 
    { 
        get {
            return (eArenaRankGrade)CacheUserData.GetInt("LastUserGrade", (int)SeasonGrade);
        } 
        set {
            CacheUserData.SetInt("LastUserGrade", (int)value);
        } 
    }

    public int season_high_point { get; private set; } = 0;

    public int first_reward { get; private set; } = 0;

    public JArray rank_up_rewards { get; private set; } = null;


    public void SetData(JToken _data)
    {
        JObject data = (JObject)_data;

        if (data.ContainsKey("offence"))
            offence_count = data["offence"].Value<int>();
        if (data.ContainsKey("offence_failure"))
            offence_failuer_count = data["offence_failure"].Value<int>();
        if (data.ContainsKey("defence"))
            defence_count = data["defence"].Value<int>();
        if (data.ContainsKey("defence_failure"))
            defence_failure_count = data["defence_failure"].Value<int>();

        if (data.ContainsKey("point"))
            season_point = data["point"].Value<int>();
        if (data.ContainsKey("season_rank"))
            season_rank = data["season_rank"].Value<int>();
        if (data.ContainsKey("season_id"))
            season_id = data["season_id"].Value<int>();

        if(data.ContainsKey("rank_grade"))
            SetUserSeasonGrade(data["rank_grade"].Value<int>());

        if (data.ContainsKey("advance_reward"))
        {
            foreach (int item in data["advance_reward"])
            {
                season_reward_step.Add(item);
            }
        }
        if (data.ContainsKey("season_start"))
            season_start = data["season_start"].Value<int>();

        if (data.ContainsKey("season_remain_time") )
            season_remain_time = data["season_remain_time"].Value<int>();
        if (data.ContainsKey("season_buff") && data["season_buff"].Type == JTokenType.Array)
        {
            JArray array = (JArray)data["season_buff"];
            if (array.Count == 3)
            {
                ATK_ELEM = (eElementType)array[0].Value<int>();
                DEF_ELEM = (eElementType)array[1].Value<int>();
                HP_ELEM = (eElementType)array[2].Value<int>();
            }
        }
        if (data.ContainsKey("refresh_check"))
            Arena_Match_Refresh_Check = data["refresh_check"].Value<int>();
        if (data.ContainsKey("ticket") )
            Arena_Ticket = data["ticket"].Value<int>();
        if (data.ContainsKey("refill"))
            Arena_Ticket_refill_count = data["refill"].Value<int>();
        if (data.ContainsKey("refill_check") )
            Arena_Ticket_refill_check = data["refill_check"].Value<int>();
        if (data.ContainsKey("ticket_tick") )
            Arena_Ticket_Exp = data["ticket_tick"].Value<int>();


        if (data.ContainsKey("first_reward"))
            first_reward = data["first_reward"].Value<int>();
        if (data.ContainsKey("season_high"))
            season_high_point = data["season_high"].Value<int>();
        if (data.ContainsKey("reward"))
            rank_up_rewards = (JArray)data["reward"];
    }

    public void UpdateSeasonType(int type)
    {
        season_type = (SeasonType)type;
    }

    public void SetArenaTicketCount(int count)
    {
        Arena_Ticket = count;
    }
    public void SetArenaTicketExp(int exp)
    {
        Arena_Ticket_Exp = exp;
    }
    public void InitUserArenaDefaultData()
    {
        season_point = 0;
        offence_count = 0;
        offence_failuer_count = 0;
        defence_count = 0;
        defence_failure_count = 0;
    }
    public void SetUserTicketInfo(int Arena_Ticket, int Arena_Ticket_exp)
    {
        this.Arena_Ticket = Arena_Ticket;
        this.Arena_Ticket_Exp = Arena_Ticket_exp;
    }
    public void SetRefillData(int count, int check)
    {
        Arena_Ticket_refill_count = count;
        Arena_Ticket_refill_check = check;
    }
    public void SetArena_Match_Refresh_Check(int time)
    {
        Arena_Match_Refresh_Check = time;
    }
    public void SetUserArenaTicket(JObject jsonData)
    {
        if (jsonData["ticket"] != null)
        {
            Arena_Ticket = jsonData["ticket"].Value<int>();
            UIManager.Instance.MainUI.RefreshTicket();
        }
        if (jsonData["ticket_tick"] != null)
        {
            Arena_Ticket_Exp = jsonData["ticket_tick"].Value<int>();
        }
        if (jsonData["refill"] != null)
        {
            Arena_Ticket_refill_count = jsonData["refill"].Value<int>();
        }
    }

    public void SetUserSeasonRank(int rank)
    {
        season_rank = rank;
    }

    public void SetUserSeasonGrade(int grade)
    {
        SeasonGrade = (eArenaRankGrade)grade;
    }

    public int GetRewardSeasonID()
    {
        //프리 시즌이라면 전시즌 정보로 보여야 함.
        return season_type == SeasonType.PreSeason ? season_id - 1 : season_id;
    }

    public void ClearRankReward()
    {
        rank_up_rewards = null;
    }
}
public class ArenaManager
{
    private static ArenaManager instance;

    public static ArenaManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ArenaManager();
                instance.Init();
            }
            return instance;
        }
    }

    private ArenaBaseData userArenaBaseData = new ArenaBaseData();
    public ArenaBaseData UserArenaData
    {
        get
        {
            return userArenaBaseData;
        }
    }


    List<ArenaTeamData> matchList = new List<ArenaTeamData>();
    public List<ArenaTeamData> MatchList
    {
        get
        {
            return matchList;
        }
    }
    List<ArenaTeamData> defenceList = new List<ArenaTeamData>();
    public List<ArenaTeamData> DefenceList
    {
        get
        {
            return defenceList;
        }
    }
    List<ArenaTeamData> rankingList = new List<ArenaTeamData>();
    public List<ArenaTeamData> RankingList
    {
        get
        {
            return rankingList;
        }
    }

    List<TimeObject> timeObjectList = new List<TimeObject>();


    private ArenaBattleData colosseumData = null;
    public ArenaBattleData ColosseumData
    {
        get
        {
            if (colosseumData == null)
            {
                colosseumData = new ArenaBattleData();
                colosseumData.Initialize();
            }
            return colosseumData;
        }
    }

    private int currentDefencePage = 0;
    public int CurrentDefencePage
    {
        get
        {
            return currentDefencePage;
        }
    }
    private int totalDefencePage = 0;
    public int TotalDefencePage
    {
        get
        {
            return totalDefencePage;
        }
    }

    public delegate void Callback();
    private Callback RefreshUICallBack = null;

    //"ArenaTeamMode" 에 관련된 데이터 이전
    public bool IsAtk { get; private set; } = false;
    public int OtherTeamIndex { get; private set; } = -1;
    public bool IsMatchList { get; private set; } = false;

    public ArenaTeamData FriendInfoDataSet = null;//친구 대전 정보
    public RestrictedAreaTeamData RestrictedDataSet = null;

    /// <summary>
    ///  defDeck , name, def_bp 3개로 일단 만듦.
    /// </summary>
    public void SetFriendTeamDataSet(ArenaTeamData _friendTeamDataSet)
    {
        FriendInfoDataSet = new ArenaTeamData();
        FriendInfoDataSet.InitData();
        FriendInfoDataSet = _friendTeamDataSet;
    }

    public void ClearFriendTeamDataSet()
    {
        if (FriendInfoDataSet != null)
        {
            FriendInfoDataSet.InitData();
            FriendInfoDataSet = null;
        }
    }

    public ArenaTeamData GetFriendFightDataSet()
    {
        return FriendInfoDataSet;
    }

    public bool IsFriendFightDataFlag { get{ return FriendInfoDataSet != null; } }


    public void SetRestrictedTeamDataSet(RestrictedAreaTeamData restrictAreaTeamData)
    {
        RestrictedDataSet = new RestrictedAreaTeamData();
        RestrictedDataSet.InitData();
        RestrictedDataSet = restrictAreaTeamData;
    }

    public void ClearRestrictedTeamDataSet()
    {
        if (RestrictedDataSet != null)
        {
            RestrictedDataSet.InitData();
            RestrictedDataSet = null;
        }
    }

    public ArenaTeamData GetRestrictedFightDataSet()
    {
        return RestrictedDataSet;
    }

    public bool IsRestrictedFightDataFlag { get { return RestrictedDataSet != null; } }

    public struct BattleInfo
    {
        public int myBP;
        public int enemyBP;
        public string enemyNick;

        public BattleInfo(int mBP, int eBP, string eNick)
        {
            myBP = mBP;
            enemyBP = eBP;
            enemyNick = eNick;
        }
        public void Init()
        {
            myBP = 0;
            enemyBP = 0;
            enemyNick = string.Empty;
        }
    }

    public BattleInfo battleInfo { get; private set; } = new BattleInfo { myBP = 0, enemyBP = 0, enemyNick = "" };

    public int battleInfoTabIdx = 1;
    public void SetArenaBattleInfo(BattleInfo info)
    {
        battleInfo = info;
    }

    public void SetArenaTeamModeData(bool isAtk = false, int otherTeamIdx = -1, bool isMatchLis = false)
    {
        IsAtk = isAtk;
        OtherTeamIndex = otherTeamIdx;
        IsMatchList = isMatchLis;
    }

    //"ArenaVersusTeam" 에 관련된 데이터 이전
    public int VersusTeamIndex { get; private set; } = -1;
    public bool IsVersusMatchList { get; private set; } = false;

    public void SetArenaVersusTeamData(int teamIndex, bool isMatchList)
    {
        VersusTeamIndex = teamIndex;
        IsVersusMatchList = isMatchList;
    }

    public void SetRefreshUICallback(Callback ok_cb)
    {
        if (ok_cb != null)
        {
            RefreshUICallBack = ok_cb;
        }
    }
    public void RefreshUI()
    {
        if (RefreshUICallBack != null)
        {
            RefreshUICallBack();
        }
    }

    private void Init()
    {
        userArenaBaseData.InitUserArenaDefaultData();
        initDefencePageIndex();
    }

    public void initDefencePageIndex()
    {
        currentDefencePage = 0;
        totalDefencePage = 0;
    }

    public void ReqArenaData(Callback callback = null, Callback failResponseCallback = null)
    {
        //WWWForm data = new WWWForm();
        //data.AddField("uno", User.Instance.UNO); //유저번호
        //data.AddField("sid", NetworkManager.Instance.SessionID); //세션 아이디
        NetworkManager.Send("arena/arena", null, (jsonData)=> {
            if (SetArenaData(jsonData))
                callback?.Invoke();
            else
                failResponseCallback?.Invoke();
        }, (string val) =>
        {
            Debug.Log(val);
            if (failResponseCallback != null)
            {
                failResponseCallback();
            }
        });
    }

    public bool SetArenaData(JObject jsonData)
    {
        if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
            return false;

        switch ((eApiResCode) jsonData["rs"].Value<int>())
        {
            case eApiResCode.OK:
            {
                var data = jsonData["arena"];
                if (data != null)
                {
                    var arenaBaseData = data["base"];
                    if (arenaBaseData != null)
                    {
                        userArenaBaseData.SetData(arenaBaseData);
                    }

                    if (data["match_list"] != null && SBFunc.IsJArray(data["match_list"]))
                    {
                        var matchLists = data["match_list"].ToObject<JArray>();
                        if (matchLists.Count > 0)
                        {
                            matchList = new List<ArenaTeamData>();
                            matchList = SetOtherTeamList(matchLists);
                        }
                    }
                    var seasonData = data["season"];
                    if (seasonData != null && seasonData.HasValues)
                    {
                        userArenaBaseData.SetData(seasonData);
                    }

                    var season_type = data["season_type"];
                    if (season_type != null && season_type.Type == JTokenType.Integer)
                    {
                        userArenaBaseData.UpdateSeasonType(season_type.Value<int>());
                    }

                    return true;
                }                            
            } break;                
            default:
                break;
        }

        return false;
    }

    public void SetArenaDefenceData(Callback callback = null, int requestPageIndex = 0)
    {
        WWWForm param = new WWWForm();
        param.AddField("page", requestPageIndex);
        NetworkManager.Send("arena/defencelist", param, (JObject jsonData) =>
        {
            if ((int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
            {
                var DefenceList = jsonData["defence_list"];
                if (DefenceList != null)
                {
                    defenceList = new List<ArenaTeamData>();
                    defenceList = SetOtherTeamList(DefenceList.ToObject<JArray>());
                }
                if (jsonData["pages"] != null)
                {
                    totalDefencePage = (int)jsonData["pages"];
                }

                if (jsonData["cur_page"] != null)
                {
                    currentDefencePage = (int)jsonData["cur_page"];
                }
                if (callback != null)
                {
                    callback();
                }
                RefreshTimeObject();
            }
        }, (string val) =>
        {
            Debug.Log(val);
        });
    }

    public void pushArenaMatchList(JArray arr)
    {
        matchList = SetOtherTeamList(arr);
    }

    public List<ArenaTeamData> SetOtherTeamList(JArray arr)
    {
        List<ArenaTeamData> targetList = new List<ArenaTeamData>();
        foreach (var arrToken in arr)
        {
            if (!arrToken.HasValues)
                continue;
            ArenaTeamData tempMatchDataSet = new ArenaTeamData((JObject)arrToken);
            targetList.Add(tempMatchDataSet);
        }

        return targetList;
    }

    public void RequestNewMatchList(Callback cb = null, bool isAdvertise = false, string addparam = "")
    {
        WWWForm param = new WWWForm();
        if(isAdvertise)
        {
            param.AddField("advertisement", 1);
            param.AddField("ad_log", addparam);
        }

        NetworkManager.Send("arena/refreshmatchlist", param, (JObject jsonData) =>
          {
              if (jsonData["err"] != null && (int)jsonData["err"] == 0 && jsonData["rs"] != null && (int)jsonData["rs"] == 0)
              {
                  var data = jsonData["arena"];
                  if (data != null)
                  {
                      var timeData = data["refresh_check"];
                      if (timeData != null)
                      {
                          userArenaBaseData.SetArena_Match_Refresh_Check(timeData.Value<int>());
                      }
                      RefreshUI();
                      cb?.Invoke();
                      PopupManager.ClosePopup<ArenaMatchListRefreshPopup>();
                      //if (data["match_list"] != null && data["match_list"].HasValues && SBFunc.IsJArray(data["match_list"]))
                      //{
                      //    var matchLists = data["match_list"].ToObject<JArray>();
                      //    if (matchLists.Count > 0)
                      //    {
                      //        matchList = new List<otherTeamDataSet>();
                      //        matchList = SetOtherTeamList(matchLists);

                      //    }
                      //}
                  }
              }
          });
    }

    public void RefreshRankList(Callback callback)
    {
        NetworkManager.Send("arena/ranking", null, (JObject jsonData) =>
        {
            if (jsonData["err"] != null && (int)jsonData["err"] == 0 && jsonData["rs"] != null && (int)jsonData["rs"] == 0)
            {
                if (jsonData["ranking"] != null)
                {
                    rankingList = new List<ArenaTeamData>();
                    rankingList = SetOtherTeamList(jsonData["ranking"].ToObject<JArray>());
                }

                userArenaBaseData.SetUserSeasonRank(jsonData["myrank"].Value<int>());
                userArenaBaseData.SetUserSeasonGrade(jsonData["myrankgrade"].Value<int>());

                if (callback != null)
                {
                    callback();
                }
            }
        });
    }

    public void RechargeArenaTicket(Popup<PopupData> requestClosePopup, Callback callback = null)
    {
        NetworkManager.Send("arena/refillticket", null, (JObject jsonData) =>
        {
            if (jsonData["err"] != null && (int)jsonData["err"] == 0 && (int)jsonData["rs"] == 0)
            {
                if (callback != null)
                {
                    callback();
                }

                if (requestClosePopup != null)
                {
                    PopupManager.ClosePopup<SystemPopup>();
                }
            }
        });
    }

    public ArenaBaseData GetNextArenaEnergyExpire()
    {
        ArenaBaseData returnData = new ArenaBaseData();
        int exp = userArenaBaseData.Arena_Ticket_Exp;
        var maxDailyTicket = GameConfigTable.GetArenaUserMaxTicketCount();
        var arenaTicketTime = GameConfigTable.GetArenaOneTicketRechargeTime();
        int ticket = userArenaBaseData.Arena_Ticket;
        var totalTick = exp + ((maxDailyTicket - ticket) * arenaTicketTime);

        if (ticket >= maxDailyTicket)
        {
            returnData.SetArenaTicketCount(ticket);
            returnData.SetArenaTicketExp(-1);
        }
        else if (exp < 0 || totalTick <= TimeManager.GetTime())
        {
            userArenaBaseData.SetArenaTicketCount(maxDailyTicket);
            returnData.SetArenaTicketCount(maxDailyTicket);
            returnData.SetArenaTicketExp(-1);
        }
        else if (exp > TimeManager.GetTime())
        {
            returnData.SetArenaTicketCount(ticket);
            returnData.SetArenaTicketExp(exp);
        }
        else if (totalTick > TimeManager.GetTime())
        {
            for (int i = ticket + 1; i < maxDailyTicket; ++i)
            {
                userArenaBaseData.SetArenaTicketExp(userArenaBaseData.Arena_Ticket_Exp + arenaTicketTime);
                userArenaBaseData.SetArenaTicketCount(i);
                if (userArenaBaseData.Arena_Ticket_Exp > TimeManager.GetTime())
                {
                    break;
                }
            }
            returnData.SetArenaTicketCount(userArenaBaseData.Arena_Ticket);
            returnData.SetArenaTicketExp(userArenaBaseData.Arena_Ticket_Exp);
        }

        return returnData;
    }

    public int GetRefreshMatchListTime()
    {
        return userArenaBaseData.Arena_Match_Refresh_Check + int.Parse(GameConfigTable.GetConfigValue("PVP_FREE_LIST_RESET_TIME"));
    }

    public void RequestUserArenaTicketRefillCount(Callback callback)
    {
        NetworkManager.Send("arena/refillstate", null, (JObject jsonData) =>
        {
            if (jsonData["err"] != null && (int)jsonData["err"] == 0 && jsonData != null && (int)jsonData["rs"] == 0)
            {
                var data = jsonData["arena"];
                if (data != null && data.HasValues)
                {
                    var arenaBaseData = data["base"];
                    if (arenaBaseData != null && arenaBaseData.HasValues)
                    {
                        userArenaBaseData.SetRefillData(arenaBaseData["refill"].Value<int>(), arenaBaseData["refill_check"].Value<int>());
                        if (callback != null)
                        {
                            callback();
                        }
                    }
                }

            }
        });
    }

    public void SetTimeObject(TimeObject time)
    {
        if (time == null) return;
        if (timeObjectList.IndexOf(time) >= 0) return;
        timeObjectList.Add(time);
    }

    public void RefreshTimeObject()
    {
        if (timeObjectList == null || timeObjectList.Count <= 0) return;
        foreach (TimeObject time in timeObjectList)
        {
            if (time == null) continue;
            time.Refresh();
        }
    }
    public void DeleteTimeObject(TimeObject time)
    {
        if (time == null) return;
        int indexChk = this.timeObjectList.IndexOf(time);
        if (indexChk < 0) return;
        timeObjectList = timeObjectList.FindAll(element => element != time);
    }
    public void ClearTimeObject()
    {
        timeObjectList.Clear();
    }
    public void SendInvade(int matchID, List<int> deck, NetworkManager.SuccessCallback callback)
    {
        AppsFlyerSDK.AppsFlyer.sendEvent("arena_fight", new Dictionary<string, string>() { { "type", "invade" } });
        
        WWWForm param = new WWWForm();
        param.AddField("deck", SBFunc.ListToString(deck));
        param.AddField("match", matchID);
        
        NetworkManager.Send("arena/invade", param, callback);
    }
    public void SendRevenge(int combatID, List<int> deck, NetworkManager.SuccessCallback callback)
    {
        AppsFlyerSDK.AppsFlyer.sendEvent("arena_fight", new Dictionary<string, string>() { { "type", "revenge" } });

        WWWForm param = new WWWForm();
        param.AddField("deck", SBFunc.ListToString(deck));
        param.AddField("cid", combatID);
        
        NetworkManager.Send("arena/revenge", param, callback);
    }

    private string GetElementConvertString(int e_type)
    {
        var elementStr = "";
        switch (e_type)
        {
            case 1:
                elementStr = "fire";
                break;
            case 2:
                elementStr = "water";
                break;
            case 3:
                elementStr = "soil";
                break;
            case 4:
                elementStr = "wind";
                break;
            case 5:
                elementStr = "light";
                break;
            case 6:
                elementStr = "dark";
                break;
            default:
                elementStr = "fire";
                break;
        }
        return elementStr;
    }

    private string GetElementConvertString(eElementType e_type)
    {
        var elementStr = "";
        switch (e_type)
        {
            case eElementType.FIRE:
                elementStr = "fire";
                break;
            case eElementType.WATER:
                elementStr = "water";
                break;
            case eElementType.EARTH:
                elementStr = "soil";
                break;
            case eElementType.WIND:
                elementStr = "wind";
                break;
            case eElementType.LIGHT:
                elementStr = "light";
                break;
            case eElementType.DARK:
                elementStr = "dark";
                break;
            case eElementType.MAX:
            case eElementType.None:
            default:
                elementStr = "fire";
                break;
        }
        return elementStr;
    }
    private string GetGradeConvertString(int grade)
    {
        var gradeString = "";
        switch (grade)
        {
            case 1:
                gradeString = "n";
                break;
            case 2:
                gradeString = "r";
                break;
            case 3:
                gradeString = "sr";
                break;
            case 4:
                gradeString = "ur";
                break;
            default:
                gradeString = "n";
                break;
        }
        return gradeString;
    }
    public string MakeStringByGradeAndElement(string key)
    {
        //cbt 에서는 디폴트 백판
        return "default_infobg";
        CharBaseData charData = CharBaseData.Get(key);
        if (charData == null)
            return "light_n_infobg";
        string backImgName = charData.GRADE switch
        {
            1 => "bggrade_common",
            2 => "bggrade_uncommon",
            3 => "bggrade_rare",
            4 => "bggrade_unique",
            5 => "bggrade_legendary",
            _ => "default_infobg"
        }; ;
        return backImgName;
    }

    
    
    public bool IsRankUp()
    {
        var lastUserGrade = UserArenaData.LastUserGrade;
        var currentGrade = UserArenaData.SeasonGrade;
        if (lastUserGrade < currentGrade && lastUserGrade != eArenaRankGrade.NONE)
        {
            int seasonHighPoint = UserArenaData.season_high_point;
            int currentUserGrade = (int)UserArenaData.SeasonGrade;

            int nextNeedPoint = -1;
            ArenaRankData nextRankData = ArenaRankData.GetFirstInGroup(currentUserGrade + 1);
            if (nextRankData != null)
                nextNeedPoint = nextRankData.NEED_POINT;

            if (nextNeedPoint > seasonHighPoint)
                return true;
        }
        return false;
    }





    public void SendArenaTest()
    {
        WWWForm data = new();
        data.AddField("enemy", User.Instance.UserAccountData.UserNumber.ToString());

        var json1 = SBFunc.ListToString(User.Instance.PrefData.ArenaFormationData.TeamFormationATK[CacheUserData.GetInt("presetArenaAtkDeck", 0)]);
        data.AddField("deck", json1);
        var json2 = SBFunc.ListToString(User.Instance.PrefData.ArenaFormationData.TeamFormationDEF[0]);
        data.AddField("enemyDeck", json2);

        NetworkManager.Send("dev/arenafight", data, (JObject jsonData) =>
        {
            if (SBFunc.IsJTokenType(jsonData["err"], JTokenType.Integer) && (eApiResCode)jsonData["err"].Value<int>() == eApiResCode.OK && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
            {
                ColosseumData.Set(jsonData, true);
                //LoadingManager.ImmediatelySceneLoad("ArenaColosseum");
                LoadingManager.Instance.EffectiveSceneLoad("ArenaColosseum", eSceneEffectType.CloudAnimation);
            }
            else
            {
                Debug.LogError(jsonData.ToString());
            }
        });
    }

    public void SendFriendFight()
    {
        if(FriendInfoDataSet == null)
        {
            ToastManager.On(StringData.GetStringByStrKey("친구정보누락"));
            return;
        }

        WWWForm data = new();
        data.AddField("enemy", FriendInfoDataSet.UID.ToString());

        var json1 = SBFunc.ListToString(User.Instance.PrefData.ArenaFormationData.TeamFormationATK[CacheUserData.GetInt("presetArenaAtkDeck", 0)]);
        data.AddField("deck", json1);
        var json2 = SBFunc.ListToString(FriendInfoDataSet.DefDeck);
        data.AddField("enemyDeck", json2);

        NetworkManager.Send("friend/fight", data, (JObject jsonData) =>
        {
            if (SBFunc.IsJTokenType(jsonData["err"], JTokenType.Integer) && (eApiResCode)jsonData["err"].Value<int>() == eApiResCode.OK && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
            {
                ColosseumData.Set(jsonData, true);
                //LoadingManager.ImmediatelySceneLoad("ArenaColosseum");
                LoadingManager.Instance.EffectiveSceneLoad("ArenaColosseum", eSceneEffectType.CloudAnimation);
            }
            else
            {
                Debug.LogError(jsonData.ToString());
            }
        });
    }

    public void SendRestrictedAreaFight(int world, List<int> deck, StageDifficult diff, Asset asset = null)
    {
        if (RestrictedDataSet == null)
        {
            ToastManager.On(StringData.GetStringByStrKey("제한구역상대누락"));
            return;
        }

        WWWForm data = new();
        data.AddField("world", world);
        data.AddField("diff", (int)diff);
        data.AddField("deck", Newtonsoft.Json.JsonConvert.SerializeObject(deck.ToArray()));
        if(asset != null)
        {
            data.AddField("cost", "[" + (int)asset.GoodType + "," + asset.ItemNo + "," + asset.Amount + "]");
        }

        NetworkManager.Send("travelinst/fight", data, (JObject jsonData) =>
        {
            if (SBFunc.IsJTokenType(jsonData["err"], JTokenType.Integer) && (eApiResCode)jsonData["err"].Value<int>() == eApiResCode.OK && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
            {
                ColosseumData.Set(jsonData, true);
                RestrictedDataSet.CombatID = jsonData["travel_tag"].Value<int>();
                RestrictedDataSet.SetDiffficult(diff);
                LoadingManager.Instance.EffectiveSceneLoad("RestrictedAreaColosseum", eSceneEffectType.CloudAnimation);
            }
            else
            {
                Debug.LogError(jsonData.ToString());
            }
        });
    }
}