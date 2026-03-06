using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class GuildMarkIndexData : ITableData
    {
        public int Idx { get; private set; }

        public GuildMarkIndexData(int index)
        {
            Idx = index;
        }
        public string GetKey()
        {
            return Idx.ToString();
        }
        public void Init()
        {

        }
    }
    public class GuildBaseData : ITableData // 상대방 UI 등 길드 정보에 표시할 때 사용되는 최소 정보 모음
    {
        public int GuildID { get; protected set; } = 0;
        public string GuildName { get; protected set; } = "";
        public int GuildEmblem { get; protected set; } = 0;
        public int GuildMark { get; protected set; } = 0;

        public GuildBaseData()
        {

        }
        public GuildBaseData(int guildId, string name, int emblem, int mark)
        {
            GuildID = guildId;
            GuildName = name;
            GuildEmblem = emblem;
            GuildMark = mark;
        }

        public void Init() { }

        public int GetGuildID() { return GuildID; }
        public int GetGuildEmblem() { return GuildEmblem; }
        public int GetGuildMark() { return GuildMark; }
        public string GetKey() { return GuildName; }
    }

    public class GuildRankData : ITableData
    {
        public int GuildNo { get; private set; }
        public string GuildName { get; private set; }
        public int GuildExp { get; private set; }
        public string LeaderNick { get; private set; }
        public int Rank { get; private set; }
        //public long AccumPt { get; private set; }
        public int WeeklyRank { get; private set; }
        public int TotalPt { get; private set; }
        public int WeeklyPt { get; private set; }
        public int MonthlyRank { get; private set; }
        public int MonthlyPt { get; private set; }
        public int EmblemNo { get; private set; }
        public int MarkNo { get; private set; }
        public int CurUserCnt { get; private set; }
        public int MaxUserCnt { get; private set; }

        public int Server { get; private set; } = -1;

        public GuildRankData(JToken dat)
        {
            int guildNo =0;
            string guildName = string.Empty;
            int exp = 0;
            string leaderNick = string.Empty;
            int rank = 0;
            int expHigh = 0;
            int expLow = 0;
            int weekly_rank = 0;
            int weekly_point = 0;
            int monthly_rank = 0;
            int monthly_point = 0;
            int emblemNo = 0;
            int markNo = 0;
            int userCnt = 0;
            int maxUseCnt = 0;
            int total_point = 0;

            if (SBFunc.IsJTokenCheck(dat["guild_no"]))
                guildNo = dat["guild_no"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["guild_name"]))
                guildName = dat["guild_name"].ToString();
            if (SBFunc.IsJTokenCheck(dat["exp"]))
                exp = dat["exp"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["total_point"]))
                total_point = dat["total_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["master_nick"]))
                leaderNick = dat["master_nick"].ToString();
            if (SBFunc.IsJTokenCheck(dat["rank"]))
                rank = dat["rank"].ToObject<int>();
            //if (SBFunc.IsJTokenCheck(dat["exp_point_high"]))
            //    expHigh = dat["exp_point_high"].ToObject<int>();
            //if (SBFunc.IsJTokenCheck(dat["exp_point_low"]))
            //    expLow = dat["exp_point_low"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["weekly_rank"]))
                weekly_rank = dat["weekly_rank"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["weekly_total_point"]))
                weekly_point = dat["weekly_total_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["monthly_rank"]))
                monthly_rank = dat["monthly_rank"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["monthly_total_point"]))
                monthly_point = dat["monthly_total_point"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["emblem_no"]))
                emblemNo = dat["emblem_no"].ToObject<int>();
            if (SBFunc.IsJTokenCheck(dat["mark_no"]))
                markNo = dat["mark_no"].ToObject<int>();
            if(SBFunc.IsJTokenCheck(dat["member_count"]))
                userCnt = dat["member_count"].ToObject<int>();
            if(SBFunc.IsJTokenCheck(dat["member_limit"]))
                maxUseCnt = dat["member_limit"].ToObject<int>();

            if (SBFunc.IsJTokenCheck(dat["server_id"]))
                Server = dat["server_id"].ToObject<int>();

            GuildNo = guildNo;
            GuildName = guildName;
            GuildExp = exp;
            LeaderNick = leaderNick;
            Rank = rank;
            TotalPt = total_point;
            WeeklyRank = weekly_rank;
            WeeklyPt = weekly_point;
            MonthlyRank = monthly_rank;
            MonthlyPt = monthly_point;
            MarkNo = markNo;
            EmblemNo = emblemNo;
            CurUserCnt = userCnt;
            MaxUserCnt = maxUseCnt;
        }
        public int GetGuildLevel()
        {
            return GuildExpData.GetLvByExp(GuildExp);
        }

        public void Init()
        {
            
        }

        public string GetKey()
        {
            return GuildNo.ToString();
        }
    }


    public class GuildInfoData : GuildBaseData  // 길드 소속 전 길드 리스트에서 필요한 데이터
    {
        public int GuildRank { get; protected set; } = 0;
        public int GuildLv { get; protected set; } = 0;
        public int GuildPeopleCount { get; protected set; } = 0;
        public int GuildMaxPeopleCount { get; protected set; } = 0;

        public long GuildExp { get; protected set; } = 0;
        public string GuildLeaderNick { get; protected set; } = "";
        public string GuildDesc { get; protected set; } = "";
        public string GuildNotice { get; protected set; } = "";
        public eGuildJoinType GuildJoinType { get; protected set; } = eGuildJoinType.None;

        public GuildInfoData() : base()
        {

        }
        public GuildInfoData (JObject _jsonData)
        {
            UpdateData(_jsonData);
        }
        public GuildInfoData(int guildId, string name, int emblem, int mark, int rank ,int lv, string desc,string notice, int curHeadCnt,int maxHeadCnt,int guildExp,string leaderNick, eGuildJoinType joinType) : base(guildId,name,emblem,mark)
        {
            GuildRank = rank;
            GuildLv = lv;
            GuildPeopleCount = curHeadCnt;
            GuildMaxPeopleCount = maxHeadCnt;
            GuildExp = guildExp;
            GuildLeaderNick = leaderNick;
            GuildNotice = notice;
            GuildDesc = desc;
            GuildJoinType = joinType;
        }

        public void UpdateExp(int value)
        {
            GuildExp = value;
            var expData = GuildManager.Instance.GuildExpData;
            
            if(expData != null)
            {
                if (expData.TOTAL_EXP <= GuildExp)
                {
                    GuildManager.Instance.NetworkSend("guild/state", new WWWForm());
                }
            }
        }

        public void UpdateData(JObject _jsonData)
        {

            if (SBFunc.IsJTokenType(_jsonData["guild_no"], JTokenType.Integer))
            {
                GuildID = _jsonData["guild_no"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["guild_name"], JTokenType.String))
            {
                GuildName = _jsonData["guild_name"].ToObject<string>();
            }
            if (SBFunc.IsJTokenType(_jsonData["level"], JTokenType.Integer))
            {
                GuildLv = _jsonData["level"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["exp"], JTokenType.Integer))
            {
                GuildExp = _jsonData["exp"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["join_type"], JTokenType.Integer))
            {
                GuildJoinType = (eGuildJoinType)_jsonData["join_type"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["member_limit"], JTokenType.Integer))
            {
                GuildMaxPeopleCount = _jsonData["member_limit"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["member_count"], JTokenType.Integer))
            {
                GuildPeopleCount = _jsonData["member_count"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["emblem_no"], JTokenType.Integer))
            {
                GuildEmblem = _jsonData["emblem_no"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["mark_no"], JTokenType.Integer))
            {
                GuildMark = _jsonData["mark_no"].ToObject<int>();
            }
            
            if (SBFunc.IsJTokenType(_jsonData["guild_notice"], JTokenType.String))
            {
                GuildNotice = _jsonData["guild_notice"].ToObject<string>();
            }
            else
            {
                GuildNotice = string.Empty;
            }

            if (SBFunc.IsJTokenType(_jsonData["guild_desc"], JTokenType.String))
            {
                GuildDesc = _jsonData["guild_desc"].ToObject<string>();
            }
            if (SBFunc.IsJTokenType(_jsonData["rank"], JTokenType.Integer))
            {
                GuildRank = _jsonData["rank"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["master_nick"], JTokenType.String))
            {
                GuildLeaderNick = _jsonData["master_nick"].ToObject<string>();
            }
        }

        public int GetGuildRank() { return GuildRank; }
        public int GetGuildLevel() { return GuildLv; }
        public int GetGuildGuildPeopleCount() { return GuildPeopleCount; }
        public int GetGuildGuildPeopleMaxCount() { return GuildMaxPeopleCount; }
        public long GetGuildExp() { return GuildExp; }
        public string GetGuildLeaderNick() { return GuildLeaderNick; }
    }

    public class GuildOperatorAuthorityData
    {
        /// <summary>
        /// 가입 승인 거절 권한
        /// </summary>
        public bool IsManageJoin { get; private set; }
        /// <summary>
        /// 운영진 임명 권한
        /// </summary>
        public bool IsAppointAble { get; private set; }
        /// <summary>
        /// 조합원 추방권한
        /// </summary>
        public bool IsFireNormalUser { get; private set; }
        /// <summary>
        /// 가입형태 변경 권한
        /// </summary>
        public bool IsChangeJoinType { get; private set; }
        public bool IsWalletWithdraw { get; private set; } = false;

        public GuildOperatorAuthorityData()
        {
            IsManageJoin = false;
            IsAppointAble = false;
            IsFireNormalUser = false;
            IsChangeJoinType = false;
            IsWalletWithdraw = false;
        }
        //public GuildOperatorAuthorityData(bool manageJoin, bool appointAble, bool fireNormalUser,bool changeJoin)
        //{
        //    IsManageJoin = manageJoin;
        //    IsAppointAble = appointAble;
        //    IsFireNormalUser = fireNormalUser;
        //    IsChangeJoinType = changeJoin;
        //}
        //public void SetData(bool manageJoin, bool appointAble, bool fireNormalUser, bool changeJoin)
        //{
        //    IsManageJoin = manageJoin;
        //    IsAppointAble = appointAble;
        //    IsFireNormalUser = fireNormalUser;
        //    IsChangeJoinType = changeJoin;
        //}
        public GuildOperatorAuthorityData(JObject _jsonData)
        {
            if (SBFunc.IsJTokenType(_jsonData["manager_auth"], JTokenType.Integer))
            {
                int authValue = _jsonData["manager_auth"].ToObject<int>();
                IsManageJoin = (authValue & (1 << 0)) > 0;
                IsAppointAble = (authValue & (1 << 1)) > 0;
                IsFireNormalUser = (authValue & (1 << 2)) > 0;
                IsChangeJoinType = (authValue & (1 << 3)) > 0;
                IsWalletWithdraw = (authValue & (1 << 4)) > 0;
            }
        }
        public void UpdateData(JObject _jsonData)
        {
            if (SBFunc.IsJTokenType(_jsonData["manager_auth"], JTokenType.Integer))
            {
                int authValue = _jsonData["manager_auth"].ToObject<int>();
                IsManageJoin = (authValue & (1 << 0)) > 0;
                IsAppointAble = (authValue & (1 << 1)) > 0;
                IsFireNormalUser = (authValue & (1 << 2)) > 0;
                IsChangeJoinType = (authValue & (1 << 3)) > 0;
                IsWalletWithdraw = (authValue & (1 << 4)) > 0;
            }
        }
    }

    public class GuildTimeData
    {
        public uint RenameDate { get; private set; } = 0;
        public uint ChangeEmblemDate { get; private set; } = 0;
        public uint DissolutionDate { get; private set; } = 0;
        public uint DissolutionCancelDate { get; private set; } = 0;
        public uint JoinDate { get; private set; } = 0;
        public uint LeaveDate { get; private set; } = 0;
        public uint AttendenceData { get; private set; } = 0;
        public eGuildLeaveState LeaveType { get; private set; } = eGuildLeaveState.None;

        public GuildTimeData()
        {

        }
        public GuildTimeData(uint renameDate, uint changeEmblemDate, uint dissolutionDate, uint dissolutionCancelDate, uint joinDate, uint leaveDate, uint attendenceData, eGuildLeaveState _LeaveType)
        {
            RenameDate = renameDate;
            ChangeEmblemDate = changeEmblemDate;
            DissolutionDate = dissolutionDate;
            DissolutionDate = dissolutionCancelDate;
            JoinDate = joinDate;
            LeaveDate = leaveDate;
            LeaveType = _LeaveType;
            AttendenceData = attendenceData;
        }
        public void SetData(uint renameDate, uint changeEmblemDate, uint dissolutionDate, uint dissolutionCancelDate, uint joinDate, uint leaveDate, uint attendenceData, eGuildLeaveState _LeaveType)
        {
            RenameDate = renameDate;
            ChangeEmblemDate = changeEmblemDate;
            DissolutionDate = dissolutionDate;
            DissolutionCancelDate = dissolutionCancelDate;
            JoinDate = joinDate;
            LeaveDate = leaveDate;
            LeaveType = _LeaveType;
            AttendenceData = attendenceData;
        }
        public GuildTimeData(JObject _jsonData)
        {
            if (SBFunc.IsJTokenType(_jsonData["join_at"], JTokenType.Integer))
            {
                JoinDate = _jsonData["join_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["leave_at"], JTokenType.Integer))
            {
                LeaveDate = _jsonData["leave_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["leave_state"], JTokenType.Integer))
            {
                LeaveType = (eGuildLeaveState)_jsonData["leave_state"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["rename_at"], JTokenType.Integer))
            {
                RenameDate = _jsonData["rename_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["change_emblem_at"], JTokenType.Integer))
            {
                ChangeEmblemDate = _jsonData["change_emblem_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["dissolution_accept_at"], JTokenType.Integer))
            {
                DissolutionDate = _jsonData["dissolution_accept_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["dissolution_cancel_at"], JTokenType.Integer))
            {
                DissolutionCancelDate = _jsonData["dissolution_cancel_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["attendence_at"], JTokenType.Integer))
            {
                AttendenceData = _jsonData["attendence_at"].ToObject<uint>();
            }
        }
        public void UpdateData(JObject _jsonData)
        {
            if (SBFunc.IsJTokenType(_jsonData["join_at"], JTokenType.Integer))
            {
                JoinDate = _jsonData["join_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["leave_at"], JTokenType.Integer))
            {
                LeaveDate = _jsonData["leave_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["leave_state"], JTokenType.Integer))
            {
                LeaveType = (eGuildLeaveState)_jsonData["leave_state"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["rename_at"], JTokenType.Integer))
            {
                RenameDate = _jsonData["rename_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["change_emblem_at"], JTokenType.Integer))
            {
                ChangeEmblemDate = _jsonData["change_emblem_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["dissolution_accept_at"], JTokenType.Integer))
            {
                DissolutionDate = _jsonData["dissolution_accept_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["dissolution_cancel_at"], JTokenType.Integer))
            {
                DissolutionCancelDate = _jsonData["dissolution_cancel_at"].ToObject<uint>();
            }
            if (SBFunc.IsJTokenType(_jsonData["attendence_at"], JTokenType.Integer))
            {
                AttendenceData = _jsonData["attendence_at"].ToObject<uint>();
            }
        }
    }
    public class GuildDetailData
    {
        public GuildStatus GuildStatus { get; private set; } = null;
        public GuildInfoData InfoData { get; private set; } = null;

        public Dictionary<long, GuildUserData> GuildUserDictionary { get; private set; } = null;
        public List<GuildUserData> GuildUserList
        {
            get
            {
                if(GuildUserDictionary == null)
                     return null;
                return GuildUserDictionary.Values.ToList();
            }
        }
        public GuildOperatorAuthorityData OperatorAuthority { get; private set; } = null;
        public GuildTimeData GuildTimeData { get; private set; } = null;

        public long GuildMagnet { get; private set; } = 0;
        public long GuildMagnite { get; private set; } = 0;
        
        /// <summary>
        /// 이미 진행한 기부하기 횟수에 관한 카운트
        /// </summary>

        public GuildDetailData() {
            GuildStatus = new GuildStatus();
            GuildStatus.Initialze();
            InfoData = new GuildInfoData();
            GuildUserDictionary = new();
            OperatorAuthority = new GuildOperatorAuthorityData();
            GuildTimeData = new GuildTimeData(); 
        }
        public GuildDetailData(JObject _jsonData)
        {
            UpdateData(_jsonData);
        }

        public void UpdateData(JObject jsonData)
        {
            InfoData.UpdateData(jsonData);
            OperatorAuthority.UpdateData(jsonData);
            GuildTimeData.UpdateData(jsonData);
            if (SBFunc.IsJTokenType(jsonData["list"], JTokenType.Array))
            {
                UpdateGuildUserData((JArray)jsonData["list"]);
            }
            if (SBFunc.IsJTokenType(jsonData["guild_buff"], JTokenType.Object))
            {
                var statData = (JObject)jsonData["guild_buff"];
                UpdateGuildBuffData(statData);
            }
            UpdateGuildAsset(jsonData);
        }
        void UpdateGuildBuffData(JObject _jsonData)
        {
            var check = SetBuff(_jsonData);
            if (check)
                User.Instance.DragonData.RefreshALLDragonStat();//버프 업데이트 이후, 드래곤스탯 업데이트
        }

        void UpdateGuildAsset(JObject jsonData)
        {
            if (jsonData.ContainsKey("magnet"))
            {
                try
                {
                    GuildMagnet = jsonData["magnet"].Value<long>();
                }
                catch
                {
                    GuildMagnet = 0;
                    Debug.LogError("guild_magnet_error");
                }
            }
            if (jsonData.ContainsKey("magnite"))
            {
                try
                {
                    GuildMagnite = jsonData["magnite"].Value<long>();
                }
                catch
                {
                    GuildMagnite = 0;
                    Debug.LogError("guild_magnite_error");
                }
            }
        }

        public void UpdateMagnite(long value)
        {
            GuildMagnite = value;
            GuildAssetEvent.Send();
        }

        public void UpdateMagnet(long value)
        {
            GuildMagnet = value;
            GuildAssetEvent.Send();
        }
        public void UpdateGuildUserData(JArray userList)
        {
            if (SBFunc.IsJTokenType(userList, JTokenType.Array))
            {
                List<long> newKeys = new List<long>();
                foreach (var data in userList)
                {
                    long key = 0;
                    if (SBFunc.IsJTokenCheck(data["user_no"]))
                        key = data["user_no"].ToObject<long>();
                    if (key == 0)
                        continue;
                    newKeys.Add(key);
                    if (GuildUserDictionary.ContainsKey(key))
                        GuildUserDictionary[key].UpdateData(data);
                    else
                        GuildUserDictionary.Add(key, new GuildUserData(data));
                }
                var currentKeys = GuildUserDictionary.Keys.ToList();
                foreach (var key in currentKeys)
                {
                    if (newKeys.Contains(key) == false)
                        GuildUserDictionary.Remove(key);
                }
            }
        }
        bool SetBuff(JObject _jobect)
        {
            bool _checkDirty = false;
            foreach (var val in _jobect.Properties().ToArray())
            {
                var key = int.Parse(val.Name);//eStatusType
                var value = val.Value.Value<float>();
                eStatusCategory category;
                eStatusType type;
                if ((int)eStatusType.MAX <= key)
                {
                    category = eStatusCategory.RATIO;
                    type = key - eStatusType.PERC_BASE;
                }
                else
                {
                    category = eStatusCategory.ADD;
                    type = (eStatusType)key;
                }

                if (type <= eStatusType.NONE)
                    continue;

                var prev = GuildStatus.GetStatus(category, type);
                if (prev == value)//기존 값이 같으면 패스
                    continue;

                GuildStatus.ClearCategory(category);
                GuildStatus.IncreaseStatus(category, type, value);
                _checkDirty = true;
            }
            return _checkDirty;
        }

        public int GetGuildID() { return InfoData == null ? 0 : InfoData.GetGuildID(); }
        public string GetGuildName() { return InfoData == null ? string.Empty : InfoData.GetKey(); }
        public int GetGuildMark() { return InfoData == null ? 0 : InfoData.GetGuildMark(); }
        public int GetGuildEmblem() { return InfoData == null ? 0 : InfoData.GetGuildEmblem(); }
        public int GetGuildRank() { return InfoData == null ? -1 : InfoData.GetGuildRank();}
        public int GetGuildLevel() { return InfoData == null ? 0 : InfoData.GetGuildLevel(); }
        public int GetGuildGuildPeopleCount() { return InfoData == null ? 0 : InfoData.GetGuildGuildPeopleCount(); }
        public long GetGuildExp() { return InfoData == null ? 0 : InfoData.GetGuildExp(); }
        public string GetLeaderNick() { return InfoData == null ? string.Empty : InfoData.GetGuildLeaderNick(); }
        public eGuildJoinType GetGuildJoinType() { return InfoData == null ? eGuildJoinType.None : InfoData.GuildJoinType; }
        public GuildStatus GetGuildBuff() { return GuildStatus; }

    }
    public class GuildManager :IManagerBase
    {
        private static GuildManager instance = null;
        public static GuildManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GuildManager();
                }
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitPlayMode()
        {
            if (instance != null)
            {
                instance = null;
            }
        }


        public int GuildDonationCount { get; private set; } = 0;



        #region 관리 권한
        public bool IsManageUserAble { get 
            {
                if(MyData == null)
                    return false;
                switch (MyData.GuildPosition)
                {
                    case eGuildPosition.Normal:
                        return false;
                    case eGuildPosition.Operator:
                            return true;
                    case eGuildPosition.Leader:
                        return true;
                }
                return false;
            }
        }

        public bool IsManageJoinAble
        {
            get
            {
                if(MyData == null)
                    return false;
                switch (MyData.GuildPosition)
                {
                    case eGuildPosition.Normal:
                        return false;
                    case eGuildPosition.Operator:
                        if (MyGuildInfo.OperatorAuthority.IsManageJoin)
                            return true;
                        return false;
                    case eGuildPosition.Leader:
                        return true;
                }
                return false;
            }
        }
        public bool IsChangeUserTypeAble 
        { 
            get
            {
                if (MyData == null)
                    return false;
                switch (MyData.GuildPosition)
                {
                    case eGuildPosition.Normal:
                        return false;
                    case eGuildPosition.Operator:
                        if (MyGuildInfo.OperatorAuthority.IsAppointAble)
                            return true;
                        return false;
                    case eGuildPosition.Leader:
                        return true;
                }
                return false;
            } 
        }
        public bool IsFireNormalUserAble
        {
            get
            {
                if (MyData == null)
                    return false;
                switch (MyData.GuildPosition)
                {
                    case eGuildPosition.Normal:
                        return false;
                    case eGuildPosition.Operator:
                        if (MyGuildInfo.OperatorAuthority.IsFireNormalUser)
                            return true;
                        return false;
                    case eGuildPosition.Leader:
                        return true;
                }
                return false;
            }
        }
        public bool IsChangeJoinTypeAble
        {
            get
            {
                if (MyData == null)
                    return false;
                switch (MyData.GuildPosition)
                {
                    case eGuildPosition.Normal:
                        return false;
                    case eGuildPosition.Operator:
                        if (MyGuildInfo.OperatorAuthority.IsChangeJoinType)
                            return true;
                        return false;
                    case eGuildPosition.Leader:
                        return true;
                }
                return false;
            }
        }

        public bool IsWalletWithdraw
        {
            get
            {
                if (MyData == null)
                    return false;
                switch (MyData.GuildPosition)
                {
                    case eGuildPosition.Normal:
                        return false;
                    case eGuildPosition.Operator:
                        if (MyGuildInfo.OperatorAuthority.IsWalletWithdraw)
                            return true;
                        return false;
                    case eGuildPosition.Leader:
                        return true;
                }
                return false;
            }
        }
        #endregion

        public GuildBaseData MyBaseData
        {
            get
            {
                if(MyGuildInfo == null)
                    return null;

                return MyGuildInfo.InfoData;
            }
        }
        public GuildDetailData MyGuildInfo { get; private set; } = null;  //서버를 통한 데이터 세팅 부(나의 길드 정보가 있다면)
        public GuildUserData MyData { get; private set; } = null;         //서버를 통한 나의 길드 정보

        public int GuildID
        {
            get
            {
                if (MyGuildInfo == null)
                    return 0;
                else
                    return MyGuildInfo.GetGuildID();
            }
        }

        public int LastGuildID { get; private set; } = 0;
        public eGuildLeaveState LastGuildLeaveState { get; private set; } = eGuildLeaveState.None;
        public int LastGuildLeaveAt { get; private set; } = 0;

        public string GuildName
        {
            get
            {
                if (MyGuildInfo == null)
                    return "";
                else
                    return MyGuildInfo.GetGuildName();
            }
        }

        public Dictionary<int,GuildInfoData> RecommandGuildList { private set; get; } = null;//길드 추천 리스트
        public Dictionary<int, GuildInfoData> ReqGuildList { private set; get; } = null; // 내가 여러 길드에 가입 신청한 리스트
        public List<string> JoinApplyToMyGuildList { private set; get; } = null;  // 내 길드에 가입 신청한 리스트 정보
        public bool IsNoneGuild {
            get {
                if (MyGuildInfo == null || MyGuildInfo.InfoData.GuildID == 0 || MyGuildInfo.GuildTimeData.LeaveDate>0 || LastGuildLeaveAt > 0)
                    return true;
                return false; 
            }
        }

        public long NextNameChangeTimeStamp
        {
             get{
                var timeStamp = MyGuildInfo.GuildTimeData.RenameDate;
                return timeStamp + GameConfigTable.GetConfigIntValue("GUILD_NAME_CHANGE_PENALTY_TIME", 2592000);
            } 
        }
        public long NextEmblemChangeTimeStamp
        {
            get
            {
                var timeStamp = MyGuildInfo.GuildTimeData.ChangeEmblemDate;
                return timeStamp + GameConfigTable.GetConfigIntValue("GUILD_MARK_CHANGE_PENALTY_TIME", 2592000);
            }
        }
        public long NextRejoinGuildTimeStamp
        {
            get
            {
                var timeStamp = LastGuildLeaveAt;
                if(LastGuildLeaveState== eGuildLeaveState.Leave)
                {
                    return timeStamp + GameConfigTable.GetConfigIntValue("GUILD_LEAVE_PENALTY_TIME", 2592000);
                }
                else if (LastGuildLeaveState == eGuildLeaveState.Expel)
                {
                    return timeStamp + GameConfigTable.GetConfigIntValue("GUILD_EXPEL_PENALTY_TIME", 2592000);
                }
                else
                {
                    return 0;
                }
            }
        }
        public bool IsAttendenceAble
        {
            get
            {
                var timeStamp = MyGuildInfo.GuildTimeData.AttendenceData;
                if(timeStamp < TimeManager.GetContentClearTime(true))
                {
                    return true;
                }

                return false;
            }
        }
        /// <summary>
        /// 현재 길드가 파괴되고 있는가??
        /// </summary>
        public bool IsDestroying
        {
            get
            {
                if (MyGuildInfo == null)
                    return false;
                if (MyGuildInfo.GuildTimeData.LeaveType != eGuildLeaveState.None)
                    return false;
                var timeStamp = MyGuildInfo.GuildTimeData.DissolutionDate;
                var cancelTime = MyGuildInfo.GuildTimeData.DissolutionCancelDate;
                return timeStamp >cancelTime;
            }
        }
        /// <summary>
        /// 파괴 맞다면 예정 시각
        /// </summary>
        public long DestroyTimeStamp
        {
            get
            {
                if (MyGuildInfo == null)
                    return 0;
                var timeStamp = MyGuildInfo.GuildTimeData.DissolutionDate;
                var cancelTime = MyGuildInfo.GuildTimeData.DissolutionCancelDate;
                if(timeStamp > cancelTime)
                {
                    return timeStamp + GameConfigTable.GetConfigIntValue("GUILD_DISSOLUTION_TIME", 604800);
                }
                return 0;
            }
        }
        public long DestroyAbleTimeStamp
        {
            get
            {
                if (MyGuildInfo == null)
                    return 0;
                var cancelTime = MyGuildInfo.GuildTimeData.DissolutionCancelDate;
                return cancelTime + GameConfigTable.GetConfigIntValue("GUILD_DISSOLUTION_CANCEL_PENALTY_TIME", 2592000);
               
            }
        }

        public GuildExpData GuildExpData
        {
            get
            {
                return GuildExpData.GetDataByLv(MyGuildInfo.InfoData.GuildLv);
            }
        }
        
        public int GuildRemainDonationCount
        {
            get
            {
                return GameConfigTable.GetConfigIntValue("GUILD_DONATION_LIMIT", 3) - GuildDonationCount;
            }
        }

        public bool GuildWorkAble
        {
            get
            {
                return GameConfigTable.GetConfigIntValue("GUILD_UPDATE_SHOW_CONDITION", 0) ==1;
            }
        }

        public bool GuildBuildingShowAble
        {
            get
            {
                return GameConfigTable.GetConfigIntValue("GUILD_TERRITORY_CONDITION", 0) == 1;
            }
        }

        public List<GuildRankData> GuildRankList { get; private set; } = new List<GuildRankData>();
        public List<GuildRankData> UnifiedGuildRankList { get; private set; } = new List<GuildRankData>();

        public void OpenGuild()
        {
            WWWForm form = new WWWForm();
            NetworkSend("guild/state", form, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch ((int)jsonData["rs"])
                    {
                        case (int)eApiResCode.OK:
                            UpdateData(jsonData);
                            if (IsNoneGuild)
                            {
                                GuildSelectPopup.Show(() =>
                                {
                                    if (NextRejoinGuildTimeStamp >= TimeManager.GetTime())
                                    {
                                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_errorcode_10"), true, false, true);
                                        return;
                                    }

                                    PopupManager.OpenPopup<GuildMakePopup>();
                                });                                
                            }
                            else
                            {
                                if (SBFunc.IsJObject(jsonData["recomm_guild_list"]))
                                {
                                    GuildSelectPopup.Show(() =>
                                    {
                                        PopupManager.OpenPopup<GuildMakePopup>();
                                    });
                                }
                                else
                                {
                                    PopupManager.OpenPopup<GuildInfoPopup>();
                                }                               
                            }
                            break;
                    }
                }

            });
        }


        /// <summary>
        /// 길드 api 전송 일괄 처리용 코드
        /// </summary>
        public void NetworkSend(string api, WWWForm packet, VoidDelegate callBack =null)
        {
            NetworkManager.Send(api, packet, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    UpdateData(jsonData);
                    switch ((int)jsonData["rs"])
                    {
                        case (int)eApiResCode.OK:
                            callBack?.Invoke();
                            break;
                        case (int)eApiResCode.NICKNAME_DUPLICATES:
                        case (int)eApiResCode.ACCOUNT_EXISTS:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832));
                            break;
                        case (int)eApiResCode.INVALID_NICK_CHAR:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832));
                            break;
                        case (int)eApiResCode.GUILD_DATA_ERROR:
                            break;
                        case (int)eApiResCode.GUILD_DUPLICATE_NAME:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_1"));
                            break;
                        case (int)eApiResCode.GUILD_ALREADY_REQ_JOIN:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_2"));
                            break;
                        case (int)eApiResCode.GUILD_NO_REQ_JOIN:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_3"));
                            break;
                        case (int)eApiResCode.GUILD_REQ_CANCEL_FAIL:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_4"));
                            break;
                        case (int)eApiResCode.GUILD_REQ_DENY_FAIL:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_5"));
                            break;
                        case (int)eApiResCode.GUILD_NO_AUTH:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_6"));
                            break;
                        case (int)eApiResCode.GUILD_UNABLE_JOIN:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_7"));
                            break;
                        case (int)eApiResCode.GUILD_MEMBER_FULL:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_8"));
                            break;
                        case (int)eApiResCode.GUILD_ALREADY_BELONG:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_9"));
                            break;
                        case (int)eApiResCode.GUILD_CANNOT_JOIN_YET:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_10"));
                            break;
                        case (int)eApiResCode.GUILD_NO_GUILD_YOU_CAN_JOIN:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_11"));
                            break;
                        case (int)eApiResCode.GUILD_INVALID_OPEN_CONDITION:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_12"));
                            break;
                        case (int)eApiResCode.GUILD_CHANGE_GUILD_NAME_FAIL:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_13"));
                            break;
                        case (int)eApiResCode.GUILD_CHANGE_EMBLEM_FAIL:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_14"));
                            break;
                        case (int)eApiResCode.GUILD_NO_CHANGE_GRADE:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_15"));
                            break;
                        case (int)eApiResCode.GUILD_NOT_CAN_BE_CHANGE:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_16"));
                            break;
                        case (int)eApiResCode.GUILD_INVALID_CHANGE_MEMBER_TYPE_CONDITION:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_17"));
                            break;
                        case (int)eApiResCode.GUILD_CANNOT_LEAVE:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_18"));
                            break;
                        case (int)eApiResCode.GUILD_CANNOT_EXPEL:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_19"));
                            break;
                        case (int)eApiResCode.GUILD_CANNOT_CLOSE_AGAIN:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_20"));
                            break;
                        case (int)eApiResCode.GUILD_CANNOT_DONATE_YET:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_21"));
                            break;
                        case (int)eApiResCode.GUILD_DONATION_COUNT_IS_FULL:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_22"));
                            break;
                        case (int)eApiResCode.GUILD_CANNOT_ATTENDENCE_YET:
                            ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_23"));
                            break;
                    }
                }
            });
        }

        public void NetworkSend(string api, WWWForm packet, JsonDelegate callBack)
        {
            if (packet == null)
                packet = new WWWForm();

            if (MyBaseData != null)
            {
                packet.AddField("gno", MyBaseData.GuildID);
            }

            NetworkManager.Send(api, packet, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch ((int)jsonData["rs"])
                    {
                        case (int)eApiResCode.OK:
                            UpdateData(jsonData);
                            callBack?.Invoke(jsonData);
                            break;
                        case (int)eApiResCode.NICKNAME_DUPLICATES:
                        case (int)eApiResCode.ACCOUNT_EXISTS:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832));
                            break;
                        case (int)eApiResCode.INVALID_NICK_CHAR:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("길드이름오류"));
                            break;
                    }
                }
            });
        }
        /// <summary>
        /// 서버쪽 update 연결 (로그인 & push)
        /// </summary>
        /// <param name="_jsonData"></param>
        ///

        public void UpdateData(JObject _jsonData)
        {
            if (_jsonData == null) return;

            
            if (SBFunc.IsJTokenCheck(_jsonData["guild_info"]))
            {
                UpdateGuildData((JObject)_jsonData["guild_info"]);
                LastGuildLeaveState = eGuildLeaveState.None;
            }
            if (SBFunc.IsJTokenType(_jsonData["leave_state"], JTokenType.Integer))
            {
                LastGuildLeaveState = (eGuildLeaveState)_jsonData["leave_state"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(_jsonData["leave_at"], JTokenType.Integer))
            {
                LastGuildLeaveAt = _jsonData["leave_at"].ToObject<int>();
            }

            if (SBFunc.IsJTokenCheck(_jsonData["recomm_guild_list"]))
            {
                UpdateRecommendGuildList(_jsonData["recomm_guild_list"]);
            }
            if (SBFunc.IsJTokenCheck(_jsonData["req_guild_list"]))
            {
                UpdateUserReqGuildList(_jsonData["req_guild_list"]);
            }
            else
            {
                ClearReqGuildData();
            }

            if (SBFunc.IsJTokenType(_jsonData["guild_no"],JTokenType.Integer))
                LastGuildID = _jsonData["guild_no"].ToObject<int>();

            if(SBFunc.IsJTokenType(_jsonData["rewards"], JTokenType.Array))
            {
                var rewards = SBFunc.ConvertSystemRewardDataList((JArray)_jsonData["rewards"]);
                SystemRewardPopup.OpenPopup(rewards);
            }
            if (IsNoneGuild)
                GuildEvent.RefreshGuildUI(GuildEvent.eGuildEventType.LostGuild);
            else
            {
                GuildEvent.RefreshGuildUI(GuildEvent.eGuildEventType.GuildRefresh);
            }            
        }
        public void UpdateUserReqGuildList(JToken _jsonData)
        {
            List<int> newKeyList = new List<int>();
            foreach (JProperty key in _jsonData)
            {
                int keyInt = int.Parse(key.Name);
                newKeyList.Add(keyInt);
                if (ReqGuildList.ContainsKey(keyInt)==false)
                {
                    ReqGuildList.Add(keyInt, new GuildInfoData());
                }
                ReqGuildList[keyInt].UpdateData((JObject)_jsonData[key.Name]);
            }
            var currentKeys = ReqGuildList.Keys.ToList();
            foreach (var key in currentKeys)
            {
                if (newKeyList.Contains(key) == false)
                    ReqGuildList.Remove(key);
            }
        }

        public void ClearReqGuildData()
        {
            if(ReqGuildList != null)
            {
                ReqGuildList.Clear();
            }
        }

        public void RemoveReqGuildData(int guildId)
        {
            if(ReqGuildList.ContainsKey(guildId))
                ReqGuildList.Remove(guildId);
        }

        public void UpdateRecommendGuildList(JToken _jsonData)
        {
            //RecommandGuildList
            List<int> newKeyList = new List<int>();
            foreach(JProperty key in  _jsonData)
            {
                int keyInt = int.Parse(key.Name);
                newKeyList.Add(keyInt);
                if (RecommandGuildList.ContainsKey(keyInt)==false)
                {
                    var data = _jsonData[key.Name];
                    int authValue = 0;
                    int renameDate = 0;
                    int changeEmblemDate = 0;
                    int dissolutionDate = 0;
                    int dissolutionCancelDate = 0;

                    if (SBFunc.IsJTokenType(data["manager_auth"], JTokenType.Integer))
                    {
                        authValue = data["manager_auth"].ToObject<int>();
                    }
                    if (SBFunc.IsJTokenType(data["rename_at"], JTokenType.Integer))
                    {
                        renameDate = data["rename_at"].ToObject<int>();
                    }
                    if (SBFunc.IsJTokenType(data["change_emblem_at"], JTokenType.Integer))
                    {
                        changeEmblemDate = data["change_emblem_at"].ToObject<int>();
                    }
                    if (SBFunc.IsJTokenType(data["dissolution_accept_at"], JTokenType.Integer))
                    {
                        dissolutionDate = data["dissolution_accept_at"].ToObject<int>();
                    }
                    if (SBFunc.IsJTokenType(data["dissolution_cancel_at"], JTokenType.Integer))
                    {
                        dissolutionCancelDate = data["dissolution_cancel_at"].ToObject<int>();
                    }
                    bool JOIN = (authValue & (1 << 0)) > 0;
                    bool APPOINT_MANAGER = (authValue & (1 << 1)) > 0;
                    bool EXPEL_MEMBER = (authValue & (1 << 2)) > 0;
                    bool CHANGE_JOIN_TYPE = (authValue & (1 << 3)) > 0;
                    //GuildOperatorAuthorityData operatorAuth = new GuildOperatorAuthorityData(JOIN, APPOINT_MANAGER, EXPEL_MEMBER, CHANGE_JOIN_TYPE); // 지금은 안씀
                    RecommandGuildList.Add(keyInt, new GuildInfoData((JObject)data));
                }
                else
                {
                    RecommandGuildList[keyInt].UpdateData((JObject)_jsonData[key.Name]);
                }
            }
            var currentKeys = RecommandGuildList.Keys.ToList();
            foreach (var key in currentKeys)
            {
                if(newKeyList.Contains(key)==false)
                    RecommandGuildList.Remove(key);
            }
        }

        public void UpdateGuildData(JObject _jsonData)
        {
            if (SBFunc.IsJTokenType(_jsonData["donation_count"], JTokenType.Integer))
            {
                GuildDonationCount = _jsonData["donation_count"].ToObject<int>();
            }

            if (MyGuildInfo == null)
                MyGuildInfo = new GuildDetailData(_jsonData);
            else
                MyGuildInfo.UpdateData(_jsonData);

            /// 길드가 바뀌면 캐싱 데이터 최신화.
            if (User.Instance.UserData.LastGuildNo != GuildID)
            {
                User.Instance.UserData.UpdateLastGuildID(GuildID);
                ChatManager.Instance.LoadDBGuildData(GuildID);
                ClearReqGuildList();
            }

            JoinApplyToMyGuildList.Clear();
            if (SBFunc.IsJTokenType(_jsonData["req_user_list"], JTokenType.Array))
            {
                foreach (var user in (JArray)_jsonData["req_user_list"])
                {
                    string userNo = (string)user;
                    if (JoinApplyToMyGuildList.Contains(userNo)==false)
                        JoinApplyToMyGuildList.Add(userNo);
                }
            }
            
            MyData = MyGuildInfo.GuildUserList.Find((data) => data.UID == User.Instance.UserAccountData.UserNumber);
        }

       

        public void UpdateGuildUserData(JArray jArray)
        {
            if(MyGuildInfo != null)
            {

                MyGuildInfo.UpdateGuildUserData(jArray);
            }
        }

        public void Initialize()
        {
            ClearData();
        }

        public void Update(float dt)
        {
            
        }

        void ClearGuildData()
        {
            MyGuildInfo = new();
        }
        void ClearRecommendData()
        {
            if (RecommandGuildList == null)
                RecommandGuildList = new Dictionary<int, GuildInfoData>();
            RecommandGuildList.Clear();
        }
        void ClearReqGuildList()
        {
            if (ReqGuildList == null)
                ReqGuildList = new Dictionary<int, GuildInfoData>();
        }
        void ClearApplyToMyGuildData()
        {
            if (JoinApplyToMyGuildList == null)
                JoinApplyToMyGuildList = new();
        }

        void ClearData()
        {
            ClearGuildData();
            ClearRecommendData();
            ClearReqGuildList();
            ClearApplyToMyGuildData();
        }

        public void UpdateExp(int value)
        {
            if (MyGuildInfo != null && MyGuildInfo.InfoData != null)
                MyGuildInfo.InfoData.UpdateExp(value);

            GuildEvent.RefreshGuildUI(GuildEvent.eGuildEventType.GuildRefresh);
        }

        public void ReqGuildRanking(Action cb = null)
        {
            GuildRankList.Clear();
            WWWForm form = new WWWForm();
            form.AddField("gno", GuildManager.Instance.GuildID);
            form.AddField("page", (int)eGuildRankingPage.Guild);

            NetworkSend("guild/ranking", form, (JsonDelegate)((JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData["rs"]) && jsonData["rs"].ToObject<int>() == (int)eApiResCode.OK)
                {
                    if (SBFunc.IsJArray(jsonData["glist"]))
                    {
                        foreach (var dat in (JArray)jsonData["glist"])
                        {
                            GuildRankList.Add(new GuildRankData(dat));
                        }
                    }
                }

                GuildRankList = GuildRankList.OrderByDescending((Func<GuildRankData, int>)(rank => {
                    if (rank.Rank > 0)
                        return (int)-rank.Rank;
                    return int.MinValue;
                })).ToList();

                cb?.Invoke();
            }));
        }

        public void ReqUnifiedGuildRanking(Action cb = null)
        {
            UnifiedGuildRankList.Clear();
            WWWForm form = new WWWForm();
            form.AddField("gno", GuildManager.Instance.GuildID);
            form.AddField("page", (int)eGuildRankingPage.Unified);

            NetworkSend("guild/ranking", form, (JsonDelegate)((JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData["rs"]) && jsonData["rs"].ToObject<int>() == (int)eApiResCode.OK)
                {
                    if (SBFunc.IsJArray(jsonData["ulist"]))
                    {
                        foreach (var dat in (JArray)jsonData["ulist"])
                        {
                            UnifiedGuildRankList.Add(new GuildRankData(dat));
                        }
                    }
                }

                UnifiedGuildRankList = UnifiedGuildRankList.OrderByDescending((Func<GuildRankData, int>)(rank => {
                    if (rank.Rank > 0)
                        return (int)-rank.Rank;
                    return int.MinValue;
                })).ToList();

                cb?.Invoke();
            }));
        }

        public int GetGuildRanking(int guild_no)
        {
            foreach(var guildinfo in GuildRankList)
            {
                if(guildinfo.GuildNo == guild_no)
                {
                    return guildinfo.Rank;
                }
            }

            return -1;
        }
    }
}