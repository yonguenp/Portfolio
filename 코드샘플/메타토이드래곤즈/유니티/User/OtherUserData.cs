using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class OtherUserData
    {
        public OtherUserData(User user)
        {
            UID = user.UserAccountData.UserNumber;
            Nick = user.UserData.UserNick;
        }
        public OtherUserData(long _UID, string _Nick)
        {
            UID = _UID;
            Nick = _Nick;
        }

        public OtherUserData() { }

        public long UID { get; set; } = 0;
        public string Nick { get; set; } = "";
    }
    public class ThumbnailUserData : OtherUserData
    {
        public ThumbnailUserData(User user) : base(user)
        {
            Level = user.UserData.Level;
            PortraitIcon = user.UserData.UserPortrait;
            EtcInfo = user.UserData.UserPortraitFrameInfo;
        }
        public ThumbnailUserData(long _UID, string _Nick) : base(_UID, _Nick)
        {
        }

        public ThumbnailUserData(long _UID, string _Nick, string _PortraitIcon, int _Level) : base(_UID, _Nick)
        {
            Level = _Level;
            PortraitIcon = _PortraitIcon;
        }

        public ThumbnailUserData(long _UID, string _Nick, string _PortraitIcon, int _Level, PortraitEtcInfoData _EtcInfo) : base(_UID, _Nick)
        {
            Level = _Level;
            PortraitIcon = _PortraitIcon;
            EtcInfo = _EtcInfo;
        }
        public ThumbnailUserData(ChatDataInfo chatData) : base(chatData.SendUID, chatData.SendNickname)
        {
            Level = -1;
            PortraitIcon = chatData.SendIcon;
            EtcInfo = new PortraitEtcInfoData((ePortraitEtcType)chatData.PortraitType, chatData.PortraitValue);
            LastActiveTime = (int)chatData.SendUserLastEnterTimestamp;
        }
        public ThumbnailUserData() : base() { }
        public int Level { get; set; } = 0;
        public string PortraitIcon { get; set; } = "";
        public virtual PortraitEtcInfoData EtcInfo { get; set; } = null;
        public int LastActiveTime { get; set; } = 0;
        public ePortraitEtcType PortraitType { get => EtcInfo.GetDefaultType(); }
        public int PortraitValue { get => EtcInfo.GetValue(PortraitType); }
    }
    public class FriendUserData : ThumbnailUserData
    {
        public FriendUserData(JToken item) : base(0, "")
        {
            SetData(item);
        }
        public void SetSendPoint(int _point)
        {
            SendGiftPoint = _point;
        }
        public void SetRecvPoint(int _point)
        {
            ReceiveGiftPoint = _point;
        }
        public int SendGiftPoint { get; private set; }//0이면 안보냄, 1이면 보냄
        public int ReceiveGiftPoint { get; private set; }// 0: 상대가 우정포인트를 안보냄, 1: 상대가 우정포인트를 보냄, 2: 상대가 보낸 우정포인트를 받음
        public bool IsFriend { get; protected set; } = false;
        //이미 친구 요청함
        public bool IsCanReqFriend { get; set; } = false;
        public int Rank { get; protected set; } = 0;
        public eArenaRankGrade RankGrade { get; protected set; } = eArenaRankGrade.NONE;
        public int GuildNo { get; protected set; } = 0;
        public string GuildName { get; protected set; } = "";
        public int GuildEmblemNo { get; protected set; } = 0;
        public int GuildMarkNo { get; protected set; } = 0;
        public void Initialize()
        {
            UID = 0;
            Nick = "";
            Level = 0;
            PortraitIcon = "";
            LastActiveTime = 0;
            if (EtcInfo == null)
                EtcInfo = new PortraitEtcInfoData();
            else
                EtcInfo.Clear();
            GuildNo = 0;
            GuildName = "";
            GuildEmblemNo = 0;
            GuildMarkNo = 0;
            Rank = -1;
            RankGrade = eArenaRankGrade.NONE;

            IsFriend = false;
            IsCanReqFriend = false;
            SendGiftPoint = 0;
            ReceiveGiftPoint = 0;
        }
        public void SetData(JToken item)
        {
            UID = SBFunc.IsJTokenCheck(item["user_no"]) ? item["user_no"].Value<long>() : 0;
            Nick = SBFunc.IsJTokenCheck(item["nick"]) ? item["nick"].Value<string>() : "";
            Level = SBFunc.IsJTokenCheck(item["level"]) ? item["level"].Value<int>() : 0;
            PortraitIcon = SBFunc.IsJTokenCheck(item["icon"]) ? item["icon"].Value<string>() : "";
            LastActiveTime = SBFunc.IsJTokenCheck(item["last_active_time"]) ? item["last_active_time"].Value<int>() : 0;
            if (EtcInfo == null)
                EtcInfo = new PortraitEtcInfoData();
            else
                EtcInfo.Clear();
            EtcInfo.UpdateInfo(item["portrait"]);
            GuildNo = SBFunc.IsJTokenCheck(item["guild_no"]) ? item["guild_no"].Value<int>() : 0;
            GuildName = SBFunc.IsJTokenCheck(item["guild_name"]) ? item["guild_name"].Value<string>() : "";
            GuildEmblemNo = SBFunc.IsJTokenCheck(item["emblem_no"]) ? item["emblem_no"].Value<int>() : 0;
            GuildMarkNo = SBFunc.IsJTokenCheck(item["mark_no"]) ? item["mark_no"].Value<int>() : 0;
            Rank = SBFunc.IsJTokenCheck(item["myrank"]) ? item["myrank"].Value<int>() : -1;
            RankGrade = SBFunc.IsJTokenCheck(item["myrankgrade"]) ? (eArenaRankGrade)item["myrankgrade"].Value<int>() : eArenaRankGrade.NONE;

            IsFriend = SBFunc.IsJTokenCheck(item["is_friend"]) ? item["is_friend"].Value<bool>() : false;
            IsCanReqFriend = SBFunc.IsJTokenCheck(item["can_req_friend"]) ? item["can_req_friend"].Value<bool>() : false;
            SendGiftPoint = SBFunc.IsJTokenCheck(item["sfp"]) ? item["sfp"].Value<int>() : 0;
            ReceiveGiftPoint = SBFunc.IsJTokenCheck(item["rfp"]) ? item["rfp"].Value<int>() : 0;
        }
    }
    public class ProfileUserData : ThumbnailUserData
    {
        public ProfileUserData(JToken item) : base(0, "")
        {
            UID = SBFunc.IsJTokenCheck(item["user_no"]) ? item["user_no"].Value<long>() : 0;
            Nick = SBFunc.IsJTokenCheck(item["nick"]) ? item["nick"].Value<string>() : "";
            Level = SBFunc.IsJTokenCheck(item["level"]) ? item["level"].Value<int>() : 0;
            PortraitIcon = SBFunc.IsJTokenCheck(item["icon"]) ? item["icon"].Value<string>() : "";
            LastActiveTime = SBFunc.IsJTokenCheck(item["last_active_time"]) ? item["last_active_time"].Value<int>() : 0;
            if (EtcInfo == null)
                EtcInfo = new PortraitEtcInfoData();
            EtcInfo.UpdateInfo(item["portrait"]);
            GuildNo = SBFunc.IsJTokenCheck(item["guild_no"]) ? item["guild_no"].Value<int>() : 0;
            GuildName = SBFunc.IsJTokenCheck(item["guild_name"]) ? item["guild_name"].Value<string>() : "";
            GuildEmblemNo = SBFunc.IsJTokenCheck(item["emblem_no"]) ? item["emblem_no"].Value<int>() : 0;
            GuildMarkNo = SBFunc.IsJTokenCheck(item["mark_no"]) ? item["mark_no"].Value<int>() : 0;
            Rank = SBFunc.IsJTokenCheck(item["myrank"]) ? item["myrank"].Value<int>() : -1;
            RankGrade = SBFunc.IsJTokenCheck(item["myrankgrade"]) ? (eArenaRankGrade)item["myrankgrade"].Value<int>() : eArenaRankGrade.NONE;

            IsFriend = SBFunc.IsJTokenCheck(item["is_friend"]) ? item["is_friend"].Value<bool>() : false;
            IsCanReqFriend = SBFunc.IsJTokenCheck(item["can_req_friend"]) ? item["can_req_friend"].Value<bool>() : false;
        }
        public bool IsFriend { get; protected set; } = false;
        public bool IsCanReqFriend { get; protected set; } = false;
        public int Rank { get; protected set; } = 0;
        public eArenaRankGrade RankGrade { get; protected set; } = eArenaRankGrade.NONE;
        public int GuildNo { get; protected set; } = 0;
        public string GuildName { get; protected set; } = "";
        public int GuildEmblemNo { get; protected set; } = 0;
        public int GuildMarkNo { get; protected set; } = 0;
    }
    public class ArenaUserData : ThumbnailUserData
    {
        /// <summary> 본인 </summary>
        public ArenaUserData(User user, int _Point, eArenaRankGrade _RankGrade) : base(user)
        {
            RankGrade = _RankGrade;
            Point = _Point;
            RankIconName = ArenaRankData.GetIconNameByPoint(RankGrade, Point);
            GuildNo = GuildManager.Instance.GuildID;
            GuildName = GuildManager.Instance.GuildName;
            GuildMarkNo = GuildManager.Instance.MyGuildInfo.GetGuildMark();
            GuildEmblemNo = GuildManager.Instance.MyGuildInfo.GetGuildEmblem();
        }
        public ArenaUserData(long _UID, string _Nick, string _PortraitIcon, int _Level, int _Point, eArenaRankGrade _RankGrade, PortraitEtcInfoData _EtcInfo,int guildNo, string guildName, int markNo, int emblemNo) 
            : base(_UID, _Nick, _PortraitIcon, _Level, _EtcInfo)
        {
            RankGrade = _RankGrade;
            Point = _Point;
            RankIconName = ArenaRankData.GetIconNameByPoint(RankGrade, Point);
            GuildNo = guildNo;
            GuildName = guildName;
            GuildMarkNo = markNo;
            GuildEmblemNo = emblemNo;
        }

        public ArenaUserData(ChampionUserInfo data) 
            : base(data.UID, data.Nick, data.PortraitIcon, data.Level)
        {
            RankGrade = eArenaRankGrade.NONE;
            Point = 0;
            RankIconName = ArenaRankData.GetIconNameByPoint(RankGrade, Point);
            GuildNo = data.GuildNo;
            GuildName = data.GuildName;
            GuildMarkNo = data.GuildMarkNo;
            GuildEmblemNo = data.GuildEmblemNo;
        }
        public eArenaRankGrade RankGrade { get; set; } = eArenaRankGrade.NONE;
        public int Point { get; set; } = 0;
        public string RankIconName { get; set; } = "";
        public int GuildNo { get; private set; } = 0;
        public string GuildName { get; private set; } = string.Empty;
        public int GuildMarkNo { get; private set; } = 0;
        public int GuildEmblemNo { get; private set; } = 0;
    }
    /// <summary>
    /// UserInfo 각 컨텐츠별로 찢어져 있던 것 일원화 예정.
    /// </summary>
    public class GuildUserData : ThumbnailUserData, ITableData
    {
        /// <summary> 본인 이후 추가 </summary>
        public GuildUserData(User user, int rank, long sumDonate, int weekDonate, int monthlyDonate, eArenaRankGrade arenaGrade, eGuildPosition guildPosition) : base(user)
        {
            Rank = rank;
            MonthlyContribution = monthlyDonate;
            WeekContribution = weekDonate;
            ArenaGrade = arenaGrade;
            GuildPosition = guildPosition;
            CanRequest = false;
        }
        /// <summary> 필요시 추가 </summary>
        public GuildUserData(long _UID, string _Nick, string _PortraitIcon, int _Level, PortraitEtcInfoData _EtcInfo, int rank, long sumDonate, int weekDonate, int monthlyDonate, eArenaRankGrade arenaGrade, eGuildPosition guildPosition, bool canFriendRequest) : base(_UID, _Nick, _PortraitIcon, _Level, _EtcInfo)
        {
            Rank = rank;
            MonthlyContribution = monthlyDonate;
            WeekContribution = weekDonate;
            ArenaGrade = arenaGrade;
            GuildPosition = guildPosition;
            CanRequest = canFriendRequest;
        }

        public GuildUserData(JToken jsonData)
        {
            int expHigh = 0;
            int expLow = 0;
            EtcInfo = new PortraitEtcInfoData();
            if (SBFunc.IsJTokenCheck(jsonData["user_no"]))
                UID = jsonData["user_no"].ToObject<long>();
            if (SBFunc.IsJTokenCheck(jsonData["nick"]))
                Nick = jsonData["nick"].ToObject<string>();
            if (SBFunc.IsJTokenCheck(jsonData["level"]))
                Level = jsonData["level"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["member_type"]))
                GuildPosition = (eGuildPosition)jsonData["member_type"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["leave_state"]))
                LeaveState = (eGuildLeaveState)jsonData["leave_state"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["rank"]))
                Rank = jsonData["rank"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["exp_point_high"]))
                expHigh = jsonData["exp_point_high"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["exp_point_low"]))
                expLow = jsonData["exp_point_low"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["weekly_rank"]))
                WeekRank = jsonData["weekly_rank"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["weekly_exp_point"]))
                WeekContribution = jsonData["weekly_exp_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_rank"]))
                MonthRank = jsonData["monthly_rank"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_exp_point"]))
                MonthlyContribution = jsonData["monthly_exp_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck("portrait"))
                EtcInfo.UpdateInfo(jsonData["portrait"]);
            if (SBFunc.IsJTokenCheck("icon"))
                PortraitIcon = jsonData["icon"].ToObject<string>();
            if (SBFunc.IsJTokenCheck("last_active_time"))
                LastActiveTime = jsonData["last_active_time"].ToObject<int>();
            if (SBFunc.IsJTokenCheck("arena_rank_grade"))
                ArenaGrade = (eArenaRankGrade) ArenaRankData.Get( jsonData["arena_rank_grade"].ToObject<string>()).GROUP;

            CanRequest = false;
        }
        public void UpdateData(JToken jsonData)
        {
            int expHigh = 0;
            int expLow = 0;
            EtcInfo = new PortraitEtcInfoData();
            if (SBFunc.IsJTokenCheck(jsonData["user_no"]))
                UID = jsonData["user_no"].ToObject<long>();
            if (SBFunc.IsJTokenCheck(jsonData["nick"]))
                Nick = jsonData["nick"].ToObject<string>();
            if (SBFunc.IsJTokenCheck(jsonData["level"]))
                Level = jsonData["level"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["member_type"]))
                GuildPosition = (eGuildPosition)jsonData["member_type"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["leave_state"]))
                LeaveState = (eGuildLeaveState)jsonData["leave_state"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["rank"]))
                Rank = jsonData["rank"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["exp_point_high"]))
                expHigh = jsonData["exp_point_high"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["exp_point_low"]))
                expLow = jsonData["exp_point_low"].ToObject<int>();

            if (SBFunc.IsJTokenCheck(jsonData["weekly_rank"]))
                WeekRank = jsonData["weekly_rank"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["weekly_exp_point"]))
                WeekContribution = jsonData["weekly_exp_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_rank"]))
                MonthRank = jsonData["monthly_rank"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_exp_point"]))
                MonthlyContribution = jsonData["monthly_exp_point"].ToObject<int>();


            ArenaPoint = new int[3] { 0, 0, 0 };
            if (SBFunc.IsJTokenCheck(jsonData["arena_point"]))
                ArenaPoint[0] = jsonData["arena_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["weekly_arena_point"]))
                ArenaPoint[1] = jsonData["weekly_arena_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_arena_point"]))
                ArenaPoint[2] = jsonData["monthly_arena_point"].ToObject<int>();

            RaidPoint = new int[3] { 0, 0, 0 };
            if (SBFunc.IsJTokenCheck(jsonData["raid_point"]))
                RaidPoint[0] = jsonData["raid_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["weekly_raid_point"]))
                RaidPoint[1] = jsonData["weekly_raid_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_raid_point"]))
                RaidPoint[2] = jsonData["monthly_raid_point"].ToObject<int>();

            ExpPoint = new int[3] { 0, 0, 0 };
            if (SBFunc.IsJTokenCheck(jsonData["exp_point"]))
                ExpPoint[0] = jsonData["exp_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["weekly_exp_point"]))
                ExpPoint[1] = jsonData["weekly_exp_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_exp_point"]))
                ExpPoint[2] = jsonData["monthly_exp_point"].ToObject<int>();

            MagnetPoint = new int[3] { 0, 0, 0 };
            if (SBFunc.IsJTokenCheck(jsonData["magnet_point"]))
                MagnetPoint[0] = jsonData["magnet_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["weekly_magnet_point"]))
                MagnetPoint[1] = jsonData["weekly_magnet_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_magnet_point"]))
                MagnetPoint[2] = jsonData["monthly_magnet_point"].ToObject<int>();

            TotalPoint = new int[3] { 0, 0, 0 };
            if (SBFunc.IsJTokenCheck(jsonData["total_point"]))
                TotalPoint[0] = jsonData["total_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["weekly_total_point"]))
                TotalPoint[1] = jsonData["weekly_total_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(jsonData["monthly_total_point"]))
                TotalPoint[2] = jsonData["monthly_total_point"].ToObject<int>();

            if (SBFunc.IsJTokenCheck("portrait"))
                EtcInfo.UpdateInfo(jsonData["portrait"]);
            if (SBFunc.IsJTokenCheck("icon"))
                PortraitIcon = jsonData["icon"].ToObject<string>();
            if (SBFunc.IsJTokenCheck("last_active_time"))
                LastActiveTime = jsonData["last_active_time"].ToObject<int>();
            if (SBFunc.IsJTokenCheck("arena_rank_grade"))
                ArenaGrade = (eArenaRankGrade)ArenaRankData.Get(jsonData["arena_rank_grade"].ToObject<string>()).GROUP;

            CanRequest = false;
        }


        public int Rank { get; private set; } = 0;
        public long SumContribution { get { return TotalPoint[(int)GuildUserManageLayer.GUILDUSERMENU.SUM_TOTAL]; } }
        public int MonthRank { get; private set; } = 0;
        public int MonthlyContribution { get; private set; } = 0;
        public int WeekRank { get; private set; } = 0;
        public int WeekContribution { get; private set; } = 0;
        public eArenaRankGrade ArenaGrade { get; private set; } = eArenaRankGrade.NONE;
        public eGuildPosition GuildPosition { get; private set; } = eGuildPosition.Normal;
        public eGuildLeaveState LeaveState { get; private set; } = eGuildLeaveState.None;
        public bool CanRequest { get; private set; } = false;//이미 친구 요청함


        public int[] ArenaPoint { get; private set; } = new int[3] { 0, 0, 0 };
        public int[] RaidPoint { get; private set; } = new int[3] { 0, 0, 0 };
        public int[] ExpPoint { get; private set; } = new int[3] { 0, 0, 0 };
        public int[] MagnetPoint { get; private set; } = new int[3] { 0, 0, 0 };

        public int[] TotalPoint { get; private set; } = new int[3] { 0, 0, 0 };
        void ITableData.Init()
        {

        }

        string ITableData.GetKey()
        {
            return UID.ToString();
        }
    }



    public class WorldBossRankingUserData : ThumbnailUserData, ITableData
    {
        public int userBattlePoint { get; private set; } = 0;       //전투력
        public uint userWorldBossPoint { get; private set; } = 0;
        public string userTotalWorldBossPoint { get; private set; } = "";   //누적 포인트

        public long userTotalWorldBossPointLongType
        {
            get
            {
                if (long.TryParse(userTotalWorldBossPoint, out long resultLong))
                    return resultLong;
                else
                    return 0;
            }
        }
        public int userRanking { get; private set; } = 0;


        //길드 관련
        public int GuildNo { get; private set; } = 0;
        public string GuildName { get; set; } = string.Empty;
        public int GuildMarkNo { get; set; } = 0;
        public int GuildEmblemNo { get; set; } = 0;

        public WorldBossRankingUserData(long _uno, string _nick, string _thumbnailTag, int _level, int _userBattlePoint, uint _userBossPoint, string _userTotalBossPoint, int _userRanking,
            int guildNo, string _guildName, int _guildMarkNo, int _guildEmblemNo, PortraitEtcInfoData _portraitInfoData): base(_uno, _nick, _thumbnailTag, _level, _portraitInfoData)
        {
            userBattlePoint = _userBattlePoint < 0 ? 0 : _userBattlePoint;
            userWorldBossPoint = _userBossPoint;
            userTotalWorldBossPoint = _userTotalBossPoint;
            userRanking = _userRanking;
            GuildNo = guildNo;
            GuildName = _guildName;
            GuildMarkNo = _guildMarkNo;
            GuildEmblemNo = _guildEmblemNo;
        }
        public virtual void Init() { }
        public string GetKey() { return ""; }//안씀
    }
}