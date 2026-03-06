
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;




//대회 정보
public class ChampionInfo
{
    public enum ROUND_STATE
    {
        NONE = 0,
        PREPARATION = 1,
        ROUND_OF_16 = 2,
        QUARTER_FINALS = 3,
        SEMI_FINALS = 4,
        FINAL = 5,
        RESULT = 6
    }

    public enum ROUND_STEP
    {
        NONE = 0,

        // 참가 신청 및 참가자 선발
        APPLICATION = 1,
        SELECTION = 2,
        ANNOUNCE = 3,

        //매치 진행
        MATCH_TEAM_SETTING = 4,
        MATCH_DEFENSE_OPEN = 5,
        MATCH_ATTACK_OPEN = 6,
        MATCH = 7,

        RESULT = 9,
    }

    public enum USER_STATE
    {
        NONE = 0,
        APPLIER = 1,
        PARTICIPANT = 2,
        SPECTATOR = 3,
        ROUND_OF_16 = 4,
        QUARTER_FINALS = 5,
        SEMI_FINALS = 6,
        FINAL = 7,
        WINNER = 8,
    }

    public enum CONTENTS_TIME
    {
        MIN = 0,

        APPLICATION_PLAYER,
        SELECTION_PLAYER,
        ANNOUNCE_PLAYER,
        ROUND_OF16,
        QUARTER_FINAL,
        SEMI_FINAL,
        FINAL,

        DEF_TEAM_SET,
        ATK_TEAM_SET,
        HIDDEN_TEAM_SET,

        ROUND_16_BET_END,
        QUARTER_FINAL_BET_END,
        SEMI_FINAL_BET_END,
        FINAL_BET_END,

        MAX
    }

    public int CurSeason { get; private set; } = -1;
    public ROUND_STATE CurState { get; private set; } = ROUND_STATE.NONE;
    public ROUND_STEP CurStep { get; private set; } = ROUND_STEP.NONE;
    public USER_STATE UserState { get; private set; } = USER_STATE.NONE;
    public bool ParticipationQualifications { get; private set; } = false;
    public int PrevArenaSeason { get; private set; } = -1;
    public int PrevArenaGrade { get; private set; } = -1;
    public JObject TotalPrize { get; private set; } = null;
    public DateTime StartDate { get; private set; } = new DateTime(TimeManager.GetDateTime().Year,TimeManager.GetDateTime().Month, GameConfigTable.CHAMPION_START_DAY, GameConfigTable.CHAMPION_CONTENT_TIME, 0, 0, 0, DateTimeKind.Utc).AddHours(TimeManager.UTC_KOREA_HOUR);
    public DateTime EndDate { get; private set; } = new DateTime(TimeManager.GetDateTime().Year, TimeManager.GetDateTime().Month, GameConfigTable.CHAMPION_START_DAY, GameConfigTable.CHAMPION_CONTENT_TIME, 0, 0, 0, DateTimeKind.Utc).AddDays(14).AddHours(TimeManager.UTC_KOREA_HOUR);
    public DateTime NextStartDate
    {
        get
        {
            var curTime = TimeManager.GetDateTime();
            int year = curTime.Year;
            int month = curTime.Month;

            DateTime thirdTursday = Get3rdThursday(year, month);

            // 아직 3번째 수요일이 안 지났으면 이번 달
            if (curTime < thirdTursday)
            {
                return thirdTursday.AddHours(TimeManager.UTC_KOREA_HOUR);
            }

            // 이미 지났으면 다음 달
            month++;
            if (month > 12)
            {
                month = 1;
                year++;
            }

            thirdTursday = Get3rdThursday(year, month);
            return thirdTursday.AddHours(TimeManager.UTC_KOREA_HOUR);
        }
    }

    public ChampionSurpportInfo SurpportInfo { get; private set; } = new ChampionSurpportInfo();

    DateTime Get3rdThursday(int year, int month)
    {
        // 해당 달의 1일
        DateTime first = new DateTime(year, month, 1);

        // 첫 번째 수요일까지 이동
        int diff = ((int)DayOfWeek.Thursday - (int)first.DayOfWeek + 7) % 7;
        DateTime firstWednesday = first.AddDays(diff);

        // 3번째 수요일 = 첫 번째 수요일 + 14일
        return firstWednesday.AddDays(14);
    }

    public DateTime NextEndDate
    {
        get
        {
            return NextStartDate.AddDays(13).AddHours(TimeManager.UTC_KOREA_HOUR);
        }
    }


    public int ContentsStepRemainTime
    {
        get
        {
            int timestamp = 0;
            switch (CurState)
            {
                case ROUND_STATE.PREPARATION:
                {
                    if (CurStep == ROUND_STEP.APPLICATION)
                    {
                        timestamp = GetContentsTime(CONTENTS_TIME.APPLICATION_PLAYER);
                    }
                    else if (CurStep == ROUND_STEP.SELECTION)
                    {
                        timestamp = GetContentsTime(CONTENTS_TIME.SELECTION_PLAYER);
                    }
                    else
                    {
                        timestamp = GetContentsTime(CONTENTS_TIME.ANNOUNCE_PLAYER);
                    }
                }
                break;
                case ROUND_STATE.ROUND_OF_16:
                {
                    timestamp = GetContentsTime(CONTENTS_TIME.ROUND_OF16);
                    if (TimeManager.GetTimeCompare(timestamp) < 0)
                    {
                        timestamp = GetContentsTime(CONTENTS_TIME.QUARTER_FINAL);
                    }
                }
                break;
                case ROUND_STATE.QUARTER_FINALS:
                {
                    timestamp = GetContentsTime(CONTENTS_TIME.QUARTER_FINAL);
                    if (TimeManager.GetTimeCompare(timestamp) < 0)
                    {
                        timestamp = GetContentsTime(CONTENTS_TIME.SEMI_FINAL);
                    }
                }
                break;
                case ROUND_STATE.SEMI_FINALS:
                {
                    timestamp = GetContentsTime(CONTENTS_TIME.SEMI_FINAL);
                    if (TimeManager.GetTimeCompare(timestamp) < 0)
                    {
                        timestamp = GetContentsTime(CONTENTS_TIME.FINAL);
                    }
                }
                break;
                case ROUND_STATE.FINAL:
                {
                    timestamp = GetContentsTime(CONTENTS_TIME.FINAL);
                    if (TimeManager.GetTimeCompare(timestamp) < 0)
                    {
                        timestamp = TimeManager.GetTimeStamp(NextStartDate);
                    }
                }
                break;
                case ROUND_STATE.NONE:
                {
                    timestamp = TimeManager.GetTimeStamp(StartDate);
                    if (TimeManager.GetTimeCompare(timestamp) < 0)
                    {
                        timestamp = TimeManager.GetTimeStamp(NextStartDate);
                    }
                }
                break;
                default:
                {
                    timestamp = TimeManager.GetTimeStamp(NextStartDate);
                }
                break;
            }

            return TimeManager.GetTimeCompare(timestamp);
        }
    }
    public ParticipantData MyInfo
    {
        get
        {
            switch (CurState)
            {
                case ROUND_STATE.PREPARATION:
                case ROUND_STATE.ROUND_OF_16:
                case ROUND_STATE.QUARTER_FINALS:
                case ROUND_STATE.SEMI_FINALS:
                case ROUND_STATE.FINAL:
                    if (AmIParticipant)
                        return Participants[MyUserNo];
                    break;
                default:
                    return null;
            }

            return null;
        }
    }

    public bool AmIParticipant
    {
        get
        {
            switch (CurState)
            {
                case ROUND_STATE.PREPARATION:
                    if (CurStep == ROUND_STEP.ANNOUNCE)
                    {
                        return (UserState >= USER_STATE.PARTICIPANT) && Participants.ContainsKey(MyUserNo) && Participants[MyUserNo].SERVER == NetworkManager.ServerTag;
                    }
                    else
                    {
                        return false;
                    }
                case ROUND_STATE.ROUND_OF_16:
                    return (UserState >= USER_STATE.ROUND_OF_16);
                case ROUND_STATE.QUARTER_FINALS:
                    return (UserState >= USER_STATE.QUARTER_FINALS);
                case ROUND_STATE.SEMI_FINALS:
                    return (UserState >= USER_STATE.SEMI_FINALS);
                case ROUND_STATE.FINAL:
                    return (UserState >= USER_STATE.FINAL);
                default:
                    return false;
            }
        }
    }

    public long MyUserNo { get { return User.Instance.UserAccountData.UserNumber; } }
    public long WinUserNo { get; private set; }
    public Dictionary<long, ParticipantData> Participants { get; private set; } = new Dictionary<long, ParticipantData>();

    private Dictionary<string, int> ContentsTime = new Dictionary<string, int>();
    
    public Dictionary<ChampionLeagueTable.ROUND_INDEX, ChampionMatchData> MatchData { get; private set; } = new Dictionary<ChampionLeagueTable.ROUND_INDEX, ChampionMatchData>();

    public JToken HallOfFameData { get; private set; } = null;
    public ChampionInfo()
    {
        
    }
    public void SetData(JObject data, bool forceClear = false)
    {
        if (data != null && data.ContainsKey("tournament"))
        {
            data = data["tournament"] as JObject;
        }

        if (data == null)
            return;

        if(data.ContainsKey("support") && data["support"].Type == JTokenType.Object)
        {
            SurpportInfo.SetDataOnlyRate((JObject)data["support"]);
        }

        if (data.ContainsKey("user_state"))
            UserState = (USER_STATE)data["user_state"].Value<int>();

        if (data.ContainsKey("is_possible"))
        {
            ParticipationQualifications = data["is_possible"].Value<bool>();
        }

        if (data.ContainsKey("rank_grade"))
            PrevArenaGrade = data["rank_grade"].Value<int>();

        SetSeasonData(data, forceClear);

        //response by tournament/setdragon
        if (data.ContainsKey("dragon_info") && data["dragon_info"].Type == JTokenType.Object)
        {
            MyInfo?.UpdateDragonInfo((JObject)data["dragon_info"]);
        }
    }

    public void SetSeasonData(JObject data, bool forceClear = false)
    {
        if (data != null && data.ContainsKey("season"))
        {
            data = data["season"] as JObject;
        }

        if (data == null)
            return;

        if (data.ContainsKey("season_id"))
            CurSeason = data["season_id"].Value<int>();

        if (data.ContainsKey("participant_info") && data["participant_info"].Type == JTokenType.Object)
        {
            var participant_info = (JObject)data["participant_info"];
            Participants.Clear();
            foreach (var val in participant_info.Properties())
            {
                var no = long.Parse(val.Name);
                if (no > 0)
                    Participants.Add(no, new ParticipantData(no, (JObject)val.Value));
            }
        }

        if (data.ContainsKey("win_user_no"))
        {
            WinUserNo = data["win_user_no"].Value<long>();
        }

        if (data.ContainsKey("round"))
            CurState = (ROUND_STATE)data["round"].Value<int>();

        if (data.ContainsKey("step"))
            CurStep = (ROUND_STEP)data["step"].Value<int>();

        if (data.ContainsKey("base_arena_season"))
            PrevArenaSeason = data["base_arena_season"].Value<int>();

        if (data.ContainsKey("start_ts"))
            StartDate = SBFunc.TimeStampToDateTime(data["start_ts"].Value<int>());
        if (data.ContainsKey("end_ts"))
            EndDate = SBFunc.TimeStampToDateTime(data["end_ts"].Value<int>());

        for (CONTENTS_TIME i = CONTENTS_TIME.MIN; i < CONTENTS_TIME.MAX; i++)
        {
            SetContentTime(i, data);
        }

        if (data.ContainsKey("dragons") && data["dragons"].Type == JTokenType.Object)
        {
            MyInfo?.SetSelectedDragons((JObject)data["dragons"]);
        }


        if (forceClear)
            MatchData.Clear();

        if (data.ContainsKey("match_table") && data["match_table"].Type == JTokenType.Object)
        {
            SetMatchData((JObject)data["match_table"]);
        }

        if (data.ContainsKey("prize") && data.Type == JTokenType.Object)
            TotalPrize = (JObject)data["prize"];
    }

    private string GetContentsKey(CONTENTS_TIME type)
    {
        switch (type)
        {
            case CONTENTS_TIME.APPLICATION_PLAYER:
                return "application_ts";
            case CONTENTS_TIME.SELECTION_PLAYER:
                return "selection_ts";
            case CONTENTS_TIME.ANNOUNCE_PLAYER:
                return "announce_ts";
            case CONTENTS_TIME.ROUND_OF16:
                return "round_of_16_ts";
            case CONTENTS_TIME.QUARTER_FINAL:
                return "quarter_finals_ts";
            case CONTENTS_TIME.SEMI_FINAL:
                return "semi_finals_ts";
            case CONTENTS_TIME.FINAL:
                return "final_ts";

            case CONTENTS_TIME.DEF_TEAM_SET:
                return "def_team_set_ts";
            case CONTENTS_TIME.ATK_TEAM_SET:
                return "atk_team_set_ts";
            case CONTENTS_TIME.HIDDEN_TEAM_SET:
                return "hidden_team_set_ts";

            case CONTENTS_TIME.ROUND_16_BET_END:
                return "round_of_16_bet_ts";
            case CONTENTS_TIME.QUARTER_FINAL_BET_END:
                return "quarter_finals_bet_ts";
            case CONTENTS_TIME.SEMI_FINAL_BET_END:
                return "semi_finals_bet_ts";
            case CONTENTS_TIME.FINAL_BET_END:
                return "final_bet_ts";
        }

        return "";
    }

    private void SetContentTime(CONTENTS_TIME type, JObject data)
    {
        string key = GetContentsKey(type);
        if (string.IsNullOrEmpty(key))
            return;

        if (data.ContainsKey(key))
        {
            if (ContentsTime.ContainsKey(key))
                ContentsTime[key] = (data[key].Value<int>());
            else
                ContentsTime.Add(key, (data[key].Value<int>()));
        }
    }
    public int GetContentsTime(CONTENTS_TIME type)
    {
        string key = GetContentsKey(type);
        if (string.IsNullOrEmpty(key))
            return -1;

        return ContentsTime.ContainsKey(key) ? ContentsTime[key] : -1;
    }

    private void SetMatchData(JObject data)
    {
        MatchData.Clear();
        ChampionMatchData.ResultUIState.Clear();

        foreach (var round_it in data.Properties())
        {
            if (round_it.Value.Type == JTokenType.Array)
            {
                JArray round = (JArray)round_it.Value;
                for (int i = 0; i < round.Count; i++)
                {
                    JObject it = (JObject)round[i];
                    var index = GetRoundIndex((ROUND_STATE)int.Parse(round_it.Name), i);
                    if (!MatchData.ContainsKey(index))
                        MatchData.Add(index, new ChampionMatchData(it));
                    else
                        MatchData[index].SetData(it);
                }
            }

            if(round_it.Value.Type == JTokenType.Object)
            {
                JObject round = (JObject)round_it.Value;
                foreach (var it in round.Properties())
                {                    
                    var index = GetRoundIndex((ROUND_STATE)int.Parse(round_it.Name), int.Parse(it.Name));

                    if (!MatchData.ContainsKey(index))
                        MatchData.Add(index, new ChampionMatchData((JObject)it.Value));
                    else
                        MatchData[index].SetData((JObject)it.Value);
                }
            }
        }
    }

    public ChampionMatchData GetMatchData(ChampionLeagueTable.ROUND_INDEX index)
    {
        if (MatchData.ContainsKey(index))
            return MatchData[index];
        return null;
    }

    public void ReqRegist(NetworkManager.SuccessCallback cb)
    {
        WWWForm param = new WWWForm();
        param.AddField("season_id", CurSeason);
        param.AddField("base_arena_season", PrevArenaSeason);
        param.AddField("rank_grade", PrevArenaGrade);
        NetworkManager.Send("unifiedtournament/regist", param, (jsonData) =>
        {
            if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                return;

            switch ((eApiResCode)jsonData["rs"].Value<int>())
            {
                case eApiResCode.OK:
                {
                    SetData(jsonData);
                }
                break;
            }

            cb?.Invoke(jsonData);
        });
    }

    public void ReqSelectDragons(int[] dragons, NetworkManager.SuccessCallback cb)
    {
        JArray dragonArray = JArray.FromObject(dragons);
        WWWForm param = new WWWForm();
        param.AddField("season_id", CurSeason);
        param.AddField("dragons", dragonArray.ToString());
        NetworkManager.Send("unifiedtournament/selectdragon", param, (jsonData) =>
        {
            if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                return;

            switch ((eApiResCode)jsonData["rs"].Value<int>())
            {
                case eApiResCode.OK:
                {
                    SetData(jsonData);
                }
                break;
            }

            cb?.Invoke(jsonData);
        });
    }

    public void ReqSetDragonInfo(ChampionDragon dragon, NetworkManager.SuccessCallback cb, bool random = false)
    {
        if (dragon == null)
            return;

        var param = dragon.MakePacket(random);
        param.AddField("season_id", CurSeason);

        NetworkManager.Send("unifiedtournament/setdragon", param, (jsonData) =>
        {
            if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                return;

            switch ((eApiResCode)jsonData["rs"].Value<int>())
            {
                case eApiResCode.OK:
                {
                    SetData(jsonData);
                }
                break;
            }

            cb?.Invoke(jsonData);
        });
    }

    public void ReqSetTeam(ParticipantData.eTournamentTeamType type, ChampionBattleLine line, NetworkManager.SuccessCallback cb)
    {
        WWWForm param = new WWWForm();
        param.AddField("season_id", CurSeason);
        param.AddField("team_type", (int)type);
        param.AddField("team", SBFunc.ListToString(line.GetList()));
        NetworkManager.Send("unifiedtournament/setteam", param, (jsonData) =>
        {
            if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                return;

            switch ((eApiResCode)jsonData["rs"].Value<int>())
            {
                case eApiResCode.OK:
                {
                    SetData(jsonData);
                }
                break;
            }

            cb?.Invoke(jsonData);
        });
    }

    public void ReqHallOfFame(Action cb)
    {
        if (HallOfFameData == null)
        {
            WWWForm param = new WWWForm();
            NetworkManager.Send("unifiedtournament/history", param, (jsonData) =>
            {
                if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                    return;

                switch ((eApiResCode)jsonData["rs"].Value<int>())
                {
                    case eApiResCode.OK:
                    {
                        HallOfFameData = jsonData["history"];
                        cb?.Invoke();
                    }
                    break;
                }
            });
        }
        else
        {
            cb.Invoke();
        }
    }

    public void ReqBet(ChampionMatchData data, ParticipantData target, int amount, Action cb)
    {
        if (data == null || target == null || amount < 0)
            return;

        WWWForm param = new WWWForm();
        param.AddField("season_id", data.season_id);
        param.AddField("round", (int)data.round);
        param.AddField("match_slot", data.match_slot);
        param.AddField("participant", target.USER_NO.ToString());
        param.AddField("amount", amount);

        NetworkManager.Send("unifiedtournament/bet", param, (jsonData) =>
        {
            if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                return;

            switch ((eApiResCode)jsonData["rs"].Value<int>())
            {
                case eApiResCode.OK:
                {
                    if(jsonData.Type == JTokenType.Object)
                        data.SetBetLog(jsonData);
                    cb.Invoke();
                }
                break;
            }
        });
    }

    public void ReqBetLog(Action cb)
    {
        WWWForm param = new WWWForm();
        NetworkManager.Send("unifiedtournament/betlog", param, (jsonData) =>
        {
            if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                return;

            switch ((eApiResCode)jsonData["rs"].Value<int>())
            {
                case eApiResCode.OK:
                {
                    if (jsonData.Type == JTokenType.Object)
                    {
                        JObject betlog = (JObject)jsonData["betlog"];
                        foreach (var round_it in betlog.Properties())
                        {
                            if (round_it.Value.Type == JTokenType.Array)
                            {
                                JArray round = (JArray)round_it.Value;
                                for (int i = 0; i < round.Count; i++)
                                {
                                    if (round[i].Type == JTokenType.Object)
                                    {
                                        JObject it = (JObject)round[i];
                                        var index = GetRoundIndex((ROUND_STATE)int.Parse(round_it.Name), i);
                                        if (MatchData.ContainsKey(index))
                                        {
                                            MatchData[index].SetBetLog(it);
                                        }
                                    }
                                }
                            }
                            if (round_it.Value.Type == JTokenType.Object)
                            {
                                JObject round = (JObject)round_it.Value;
                                foreach (var it in round.Properties())
                                {
                                    if (it.Type == JTokenType.Object)
                                    {
                                        var index = GetRoundIndex((ROUND_STATE)int.Parse(round_it.Name), int.Parse(it.Name));
                                        if (MatchData.ContainsKey(index))
                                        {
                                            MatchData[index].SetBetLog((JObject)it.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    cb?.Invoke();                    
                }
                break;
            }
        });
    }

    public ChampionLeagueTable.ROUND_INDEX GetRoundIndex(ROUND_STATE round, int slot)
    {
        switch (round)
        {
            case ROUND_STATE.ROUND_OF_16:
                return ChampionLeagueTable.ROUND_INDEX.ROUND16_START + slot;
            case ROUND_STATE.QUARTER_FINALS:
                return ChampionLeagueTable.ROUND_INDEX.ROUND8_START + slot;
            case ROUND_STATE.SEMI_FINALS:
                return ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_START + slot;
            case ROUND_STATE.FINAL:
                return ChampionLeagueTable.ROUND_INDEX.FINAL_START + slot;
        }

        return ChampionLeagueTable.ROUND_INDEX.NONE;
    }


    public void ReqMatchInfo(ROUND_STATE round, int slot, Action cb)
    {        
        WWWForm param = new WWWForm();
        param.AddField("season_id", CurSeason);
        param.AddField("round", (int)round);
        param.AddField("slot", slot);

        NetworkManager.Send("unifiedtournament/matchinfo", param, (jsonData) =>
        {
            if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                return;

            switch ((eApiResCode)jsonData["rs"].Value<int>())
            {
                case eApiResCode.OK:
                {
                    var index = GetRoundIndex(round, slot);
                    if (MatchData.ContainsKey(index))
                        MatchData[index].SetData((JObject)jsonData["match_info"]);
                    else
                        MatchData.Add(index, new ChampionMatchData((JObject)jsonData["match_info"]));

                    cb?.Invoke();
                }break;
            }
        });
    }
}

public class ChampionDragon : UserDragon
{
    public ParticipantData.eTournamentTeamType TeamType { get { return ChampionManager.Instance.MyInfo.GetForamtionType(this); } }
    public ChampionPet ChampionPet { get; protected set; } = null;
    public Dictionary<int, ChampionPart> ChampionPart { get; protected set; } = new Dictionary<int, ChampionPart>();
    public ChampionDragon(int tag, JObject data)
    {
        InitData(tag, data);
    }

    public void InitData(int tag, JObject data)
    {
        SetBaseData(tag, eDragonState.Normal, CharExpData.GetDragonMaxTotalExp((int)eDragonGrade.Legend), GameConfigTable.GetDragonLevelMax(), GameConfigTable.GetSkillLevelMax());
        SetTranscendenceData(CharTranscendenceData.GetStepMax(eDragonGrade.Legend), CharTranscendenceData.GetMaxSkillSlot(eDragonGrade.Legend));

        if (data != null)
        {
            if(data.ContainsKey("pet"))
                SetPet((JObject)data["pet"]);
            if(data.ContainsKey("equips"))
                SetParts(tag, (JArray)data["equips"]);
            if(data.ContainsKey("passive_skill"))
                SetPassive((JArray)data["passive_skill"]);
        }

        RefreshALLStatus();
    }
    
    private void SetPet(JObject data)
    {
        var id = 0;
        if (data.ContainsKey("pet_id") && data["pet_id"].Type == JTokenType.Integer)
        {
            id = data["pet_id"].Value<int>();
        }

        if (id > 0)
        {
            ChampionPet = new ChampionPet(Tag, id, data);
        }
        else
        {
            ChampionPet = null;
        }

        SetPetTag(id);
    }

    public virtual void ReqSaveDragon(NetworkManager.SuccessCallback cb = null, bool random = false)
    {
        ChampionManager.Instance.CurChampionInfo.ReqSetDragonInfo(this, cb, random);
    }
    public void SetPet(ChampionPet pet, NetworkManager.SuccessCallback cb = null)
    {
        ChampionPet = pet;

        SetPetTag(ChampionPet.Tag);
        ReqSaveDragon(cb);
    }

    public void AddPassive(int dragonTag, List<int> optionList, NetworkManager.SuccessCallback cb = null)
    {
        TranscendenceData.SetPassiveData(optionList);
        ReqSaveDragon(cb);
    }


    private void SetParts(int dragonTag, JArray arr)
    {
        ClearPart();
        if (arr != null)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                var v = arr[i];
                if (v == null)
                    continue;

                if (v.Type != JTokenType.Object)
                {
                    continue;
                }

                var data = (JObject)v;

                if (data.ContainsKey("equip_id"))
                {
                    int key = data["equip_id"].Value<int>();
                    List<SubOptionData> subs = new List<SubOptionData>();
                    foreach (var sub in (JArray)data["equip_subs"])
                    {
                        var sk = sub.Value<int>();
                        if (sk <= 0)
                            continue;

                        subs.Add(SubOptionData.Get(sk));
                    }

                    PartFusionData fusion = null;
                    if (data.ContainsKey("equip_fusions") || data.ContainsKey("equip_fusion"))
                    {
                        if (data["equip_fusions"].Type == JTokenType.Array)
                        {
                            JArray fusions = (JArray)data["equip_fusions"];
                            if (fusions.Count > 0)
                            {
                                int fusion_key = fusions[0].Value<int>();
                                if (fusion_key > 0)
                                {
                                    fusion = PartFusionData.Get(fusion_key);
                                }
                            }
                        }
                        else if (data["equip_fusion"].Type == JTokenType.Integer)
                        {
                            fusion = PartFusionData.Get(data["equip_fusion"].Value<int>());
                        }
                    }

                    var part = new ChampionPart(key, subs, fusion);
                    part.SetTag(i, Tag);
                    ChampionPart.Add(i, part);
                }
            }
        }
    }

    public void ClearPart()
    {
        ChampionPart.Clear();
    }

    public void RemovePart(int dragonTag, int slotIndex, NetworkManager.SuccessCallback cb = null)
    {
        ChampionPart prevPartData = null;
        if (!ChampionPart.ContainsKey(slotIndex))
        {
            return;
        }

        prevPartData = ChampionPart[slotIndex];
        ChampionPart.Remove(slotIndex);

        ReqSaveDragon((jsonData) =>
        {
            if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
            {
                switch ((eApiResCode)jsonData["rs"].Value<int>())
                {
                    case eApiResCode.OK:
                    {
                        cb?.Invoke(jsonData);
                    }
                    break;
                    default:
                    {
                        ChampionPart.Add(slotIndex, prevPartData);
                    }
                    break;
                }
            }
            else
            {
                cb?.Invoke(jsonData);
            }

        });
    }

    public void AddPart(int slotIndex, ChampionPart part, NetworkManager.SuccessCallback cb = null)
    {
        part.SetTag(slotIndex, Tag);

        ChampionPart prevPartData = null;
        if (!ChampionPart.ContainsKey(slotIndex))
            ChampionPart.Add(slotIndex, part);
        else
        {
            prevPartData = ChampionPart[slotIndex];
            ChampionPart[slotIndex] = part;
        }

        ReqSaveDragon((jsonData) =>
        {
            if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
            {
                switch ((eApiResCode)jsonData["rs"].Value<int>())
                {
                    case eApiResCode.OK:
                    {
                        cb?.Invoke(jsonData);
                    }
                    break;
                    default:
                    {
                        if (prevPartData != null)
                            ChampionPart[slotIndex] = prevPartData;
                        else
                            ChampionPart.Remove(slotIndex);
                    }
                    break;
                }
            }
            else
            {
                cb?.Invoke(jsonData);
            }
        });
    }


    public override UserPart[] GetPartsList()
    {
        return new ChampionPart[6] { GetPart(0), GetPart(1), GetPart(2), GetPart(3), GetPart(4), GetPart(5) };
    }

    public ChampionPart GetPart(int index)
    {
        if (index >= 0 && index < 6)
        {
            if (ChampionPart.ContainsKey(index))
                return ChampionPart[index];
        }

        return null;
    }


    public void SetPassive(JArray data)
    {
        TranscendenceData.SetPassiveData(data.ToObject<List<int>>());
    }

    public bool IsPartsFullSetting()
    {
        for (int i = 0; i < 6; i++)
        {
            if (!ChampionPart.ContainsKey(i))
                return false;

            if (ChampionPart[i] == null)
                return false;

            for (int j = 0; j < 4; j++)
            {
                if (ChampionPart[i].SubOptionList.Count <= j)
                    return false;
                if (ChampionPart[i].SubOptionList[j].Key <= 0)
                    return false;
            }

            if (!ChampionPart[i].IsFusion)
                return false;
        }

        return true;
    }

    public bool IsPetFullSetting()
    {
        if (ChampionPet == null)
            return false;
        if (ChampionPet.ID <= 0)
            return false;

        for (int i = 0; i < 4; i++)
        {
            if (ChampionPet.Stats.Count <= i)
                return false;

            if (ChampionPet.Stats[i].Key <= 0)
                return false;

            if (ChampionPet.SubOptionList.Count <= i)
                return false;

            if (ChampionPet.SubOptionList[i].Key <= 0)
                return false;
        }

        if (ChampionPart == null)
            return false;

        return true;
    }

    public bool IsPassiveFullSetting()
    {
        for (int i = 0; i < 2; i++)
        {
            if (PassiveSkills.Count <= i)
                return false;

            if (PassiveSkills[i] <= 0)
                return false;
        }

        return true;
    }

    public bool IsFullSetting()
    {
        if (!IsPartsFullSetting() || !IsPetFullSetting() || !IsPassiveFullSetting()) 
            return false;

        return true;
    }
    public WWWForm MakePacket(bool random = false)
    {
        WWWForm param = new WWWForm();
        param.AddField("dragon_id", Tag);
        param.AddField("team_type", (int)TeamType);

        System.Random rnd = null;        
        if (random)
            rnd = new System.Random();

        int petID = 0;
        if (ChampionPet != null)
        {
            petID = ChampionPet.ID;
        }
        else if (rnd != null)
        {
            var pets = ChampionManager.GetSelectablePets();
            petID = pets[rnd.Next(pets.Count)].KEY;
        }

        param.AddField("pet_id", petID);

        JArray stats = new JArray();
        for (int i = 0; i < 4; i++)
        {
            stats.Add(0);
            if (ChampionPet != null && ChampionPet.Stats.Count > i && ChampionPet.Stats[i].Key != 0)
            {
                stats[i] = ChampionPet.Stats[i].Key;
            }
            else if (rnd != null)
            {
                var petstats = ChampionManager.GetSelectablePetStatsList();                
                stats[i] = petstats[rnd.Next(petstats.Count)].KEY;
            }
        }
        param.AddField("pet_stats", stats.ToString());


        JArray subs = new JArray();
        for (int i = 0; i < 4; i++)
        {
            subs.Add(0);
            if (ChampionPet != null && ChampionPet.SubOptionList.Count > i && ChampionPet.SubOptionList[i].Key != 0)
            {
                subs[i] = ChampionPet.SubOptionList[i].Key;
            }
            else if (rnd != null)
            {
                var petoptions = ChampionManager.GetSelectableSubOptions(PetBaseData.Get(petID), i);
                subs[i] = petoptions[rnd.Next(petoptions.Count)].KEY;
            }
        }
        param.AddField("pet_subs", subs.ToString());


        JArray parts = new JArray();
        JObject partSub = new JObject();
        JArray partFusion = new JArray();
        for (int i = 0; i < 6; i++)
        {
            parts.Add(0);
            if (ChampionPart != null && ChampionPart.ContainsKey(i) && ChampionPart[i] != null)
            {
                parts[i] = ChampionPart[i].ID;

                JArray sub = new JArray();
                for (int j = 0; j < 4; j++)
                {
                    sub.Add(0);
                    if (ChampionPart[i].SubOptionList.Count > j)
                    {
                        sub[j] = ChampionPart[i].SubOptionList[j].Key;
                    }
                    else if (rnd != null)
                    {
                        var petoptions = ChampionManager.GetSelectableSubOptions(ChampionPart[i].GetPartDesignData(), j);
                        sub[j] = petoptions[rnd.Next(petoptions.Count)].KEY;
                    }
                }

                partSub.Add(i.ToString(), sub);


                JArray fusion = new JArray();
                if (ChampionPart[i].IsFusion)
                {
                    fusion.Add(ChampionPart[i].FusionStatKey);
                }
                else if (rnd != null)
                {
                    var fusionList = ChampionManager.GetSelectableFusionOptions();
                    fusion.Add(fusionList[rnd.Next(fusionList.Count)].KEY);
                }
                else
                {
                    fusion.Add(0);
                }
                partFusion.Add(fusion);
            }
            else if (rnd != null)
            {
                var partlist = ChampionManager.GetSelectableParts();
                parts[i] = partlist[rnd.Next(partlist.Count)].KEY;

                JArray sub = new JArray();
                for (int j = 0; j < 4; j++)
                {
                    sub.Add(0);
                    var partOptions = ChampionManager.GetSelectableSubOptions(PartBaseData.Get(parts[i].Value<int>()), j);
                    sub[j] = partOptions[rnd.Next(partOptions.Count)].KEY;
                }

                partSub.Add(i.ToString(), sub);

                JArray fusion = new JArray();
                var fusionList = ChampionManager.GetSelectableFusionOptions();
                fusion.Add(fusionList[rnd.Next(fusionList.Count)].KEY);
                partFusion.Add(fusion);
            }
        }

        param.AddField("equip_ids", parts.ToString());
        param.AddField("equip_subs", partSub.ToString());
        param.AddField("equip_fusions", partFusion.ToString());

        JArray passive = new JArray();
        for (int i = 0; i < 2; i++)
        {
            passive.Add(0);
            if (PassiveSkills.Count > i && PassiveSkills[i] > 0)
            {
                passive[i] = PassiveSkills[i];
            }
            else if (rnd != null)
            {
                var passivelist = ChampionManager.GetSelectablePassive(BaseData.JOB, i);
                passive[i] = passivelist[rnd.Next(passivelist.Count)].KEY;
            }
        }

        param.AddField("passive_skill", passive.ToString());

        return param;
    }

    public override CharacterStatus GetALLStatus(int customLevel = -1)
    {
        if (customLevel <= 0)
            customLevel = Level;

        var status = GetDragonBaseStatus(Tag, customLevel);
        if (status == null)
            return null;

        var addedStatus = new List<UnitStatus>();
        var skillData = BaseData.SKILL1;

        List<UserPart> equipedParts = new List<UserPart>();
        foreach (var element in ChampionPart)
        {
            equipedParts.Add(element.Value);
        }

        equipedParts.ForEach((element) =>
        {
            if (element == null)
                return;

            addedStatus.Add(element.GetALLStat());
        });

        SetPartSetEffectOption();
        //장착 세트 효과 계산
        if (PartsSetList != null && PartsSetList.Count() > 0)
            addedStatus.Add(SBFunc.GetPartSetEffectOption(PartsSetList));
        //

        var petData = ChampionPet;
        //펫 장착 시 스킬 효과 추가
        if (petData != null)
        {
            addedStatus.Add(petData.GetALLStat());

            var petBaseData = petData.GetPetDesignData();
            if (petBaseData != null)
            {
                var rainforceData = PetReinforceData.GetDataByGradeAndStep(petBaseData.GRADE, petData.Reinforce);
                if (rainforceData != null && petData.Element() == Element())//현재 드래곤과 펫의 속성이 같다면 보너스 수치 추가
                {
                    if (rainforceData.ELEMENT_BUFF > 0)
                        status.IncreaseStatus(eStatusCategory.RATIO, petBaseData.ELEMENT_BUFF_TYPE, rainforceData.ELEMENT_BUFF);
                }
            }
        }
        //
        //추가되는 스텟 반영
        for (int i = 0, count = addedStatus.Count; i < count; ++i)
        {
            if (addedStatus[i] == null)
                continue;

            status.IncreaseStatus(addedStatus[i]);
        }

        CalcPassiveSkill(status);


        //스킬레벨 전투력 반영
        status.SetSkillLevel(SLevel);
        //계산
        status.CalcStatusAll();
        //
        return status;
    }
}

public class ChampionPet : UserPet
{
    public ChampionPet(int DragonTag, int id, JObject jsonData)
        : base(id, id, GameConfigTable.GetPetLevelMax((int)eDragonGrade.Legend), PetExpData.GetCurrentAccumulateLevelExp(GameConfigTable.GetPetLevelMax((int)eDragonGrade.Legend), (int)eDragonGrade.Legend), GameConfigTable.GetPetReinforceLevelMax((int)eDragonGrade.Legend), 0, 0)
    {
        SetLinkDragonTag(DragonTag);
        if (jsonData != null)
        {
            JArray[] petStats = new JArray[4];

            if (SBFunc.IsJTokenCheck(jsonData["pet_stats"]) && SBFunc.IsJTokenType(jsonData["pet_stats"], JTokenType.Array))
            {
                JArray array = (JArray)jsonData["pet_stats"];
                for (int i = 0; i < array.Count; i++)
                {
                    if (i < petStats.Length)
                    {
                        var data = array[i];
                        JArray val = new JArray();
                        val.Add(data.Value<int>());
                        val.Add(1);
                        petStats[i] = val;
                    }
                }
            }
            JArray petSubs = new JArray();
            if (SBFunc.IsJTokenCheck(jsonData["pet_subs"]) && SBFunc.IsJTokenType(jsonData["pet_subs"], JTokenType.Array))
            {
                JArray array = (JArray)jsonData["pet_subs"];
                for (int i = 0; i < array.Count; i++)
                {
                    var data = array[i];

                    int key = data.Value<int>();
                    float value = 0f;

                    var suboption = SubOptionData.Get(key);
                    if (suboption != null)
                    {
                        value = suboption.VALUE_MAX;
                    }

                    JArray val = new JArray();
                    val.Add(key);
                    val.Add(value);
                    petSubs.Add(val);
                }
            }

            SetStats(petStats, petSubs);
        }
    }

    public ChampionPet(PetBaseData petBase)
        : base(petBase.KEY, petBase.KEY, GameConfigTable.GetPetLevelMax((int)eDragonGrade.Legend), PetExpData.GetCurrentAccumulateLevelExp(GameConfigTable.GetPetLevelMax((int)eDragonGrade.Legend), (int)eDragonGrade.Legend), GameConfigTable.GetPetReinforceLevelMax((int)eDragonGrade.Legend), 0, 0)
    {

    }

    public void SetStat(PetStatData stat, bool geno, int index)
    {
        if (index >= PetGradeData.Get((int)eDragonGrade.Legend).START_STAT_NUM)
            return;

        while (Stats.Count < PetGradeData.Get((int)eDragonGrade.Legend).START_STAT_NUM)
        {
            Stats.Add(default);
        }

        Stats[index] = new UserPetStat(stat, geno);
    }

    public void ClearOptions()
    {
        SubOptionList.Clear();
    }

    public void SetOption(SubOptionData option, int index)
    {
        SetPetOption(new(option.KEY, option.VALUE_MAX), index);
    }
}

public class ChampionPart : UserPart
{
    //    public ChampionPart(int tag, int equipDragonTag, int id, List<SubOptionData> options = null)
    //        : base(tag, id, 0, equipDragonTag, PartReinforceData.GetMaxReinforceStep((int)ePartGrade.Legend))
    //    {
    //        if (options != null)
    //        {
    //            for (int i = 0; i < options.Count; i++)
    //            {
    //                SetPartOption(new(options[i].KEY, options[i].VALUE_MAX), i);
    //            }
    //        }
    //    }

    public ChampionPart(int id, List<SubOptionData> options = null, PartFusionData fusion = null)
        : base(-1, id, 0, -1, PartReinforceData.GetMaxReinforceStep((int)ePartGrade.Legend))
    {
        if (options != null)
        {
            for (int i = 0; i < options.Count; i++)
            {
                SetPartOption(new(options[i].KEY, options[i].VALUE_MAX), i);
            }
        }

        if(fusion != null)
        {
            SetFusionStat(int.Parse(fusion.KEY), fusion.VALUE_MAX + fusion.LEGEND_BONUS + (fusion.VALUE_REINFORCE * 3));
        }
    }

    public void ClearOptions()
    {
        SubOptionList.Clear();
    }

    public void SetOption(SubOptionData option, int index)
    {
        SetPartOption(new(option.KEY, option.VALUE_MAX), index);
    }


    public void SetTag(int tag, int equipDragonTag)
    {
        Tag = tag;
        SetLink(equipDragonTag);
    }
}

public class ChampionMatchData
{
    public ChampionLeagueTable.ROUND_INDEX UIROUND_INDEX { get; private set; } = ChampionLeagueTable.ROUND_INDEX.NONE;
    public eChampionWinType MatchResult { get; private set; } = eChampionWinType.None;
    public eChampionWinType Result_Type { get; private set; } = eChampionWinType.None;
    public int season_id { get { return GetBaseInt("season_id", -1); } }
    public ChampionInfo.ROUND_STATE round { get { return (ChampionInfo.ROUND_STATE)GetBaseInt("round", -1); } }
    public int match_slot { get { return GetBaseInt("match_slot", -1); } }
    public long luser_no { get; private set; } = -1;
    public int luser_server { get; private set; } = -1;
    public long ruser_no { get; private set; } = -1;
    public int ruser_server { get; private set; } = -1;
    public long win_user_no { get { return GetBaseInt("win_user_no", -1); } }

    public static Dictionary<ChampionLeagueTable.ROUND_INDEX, bool> ResultUIState = new Dictionary<ChampionLeagueTable.ROUND_INDEX, bool>();
    public DetailData Detail { get; private set; } = null;
    public class DetailData
    {
        public eChampionWinType Round1Result { get; private set; } = eChampionWinType.None;
        public eChampionWinType Round2Result { get; private set; } = eChampionWinType.None;
        public eChampionWinType Round3Result { get; private set; } = eChampionWinType.None;
        
        public class Team
        {
            public ChampionDragon[] OffenceTeam { 
                get {
                    ChampionDragon[] ret = new ChampionDragon[6];
                    JArray data = Off;
                    if (data == null && data.Type == JTokenType.Array)
                        return ret;

                    for (int i = 0; i < 6; i++)
                    {
                        ChampionDragon dragon = null;
                        if (data.Count > i && data[i].Type == JTokenType.Object)
                        {
                            JObject dragonData = (JObject)data[i];
                            if (dragonData != null && dragonData.ContainsKey("dragon_id"))
                                dragon = new ChampionDragon(dragonData["dragon_id"].Value<int>(), dragonData);
                        }

                        ret[i] = dragon;
                    }

                    return ret;
                } 
            }
            public ChampionDragon[] DefenceTeam
            {
                get
                {
                    ChampionDragon[] ret = new ChampionDragon[6];
                    JArray data = Def;
                    if (data == null && data.Type == JTokenType.Array)
                        return ret;

                    for (int i = 0; i < 6; i++)
                    {
                        ChampionDragon dragon = null;
                        if (data.Count > i && data[i].Type == JTokenType.Object)
                        {
                            JObject dragonData = (JObject)data[i];
                            if (dragonData != null && dragonData.ContainsKey("dragon_id"))
                                dragon = new ChampionDragon(dragonData["dragon_id"].Value<int>(), dragonData);
                        }

                        ret[i] = dragon;
                    }

                    return ret;
                }
            }
            public ChampionDragon[] HiddenTeam
            {
                get
                {
                    ChampionDragon[] ret = new ChampionDragon[6];
                    JArray data = Hid;
                    if (data == null && data.Type == JTokenType.Array)
                        return ret;

                    for (int i = 0; i < 6; i++)
                    {
                        ChampionDragon dragon = null;
                        if (data.Count > i && data[i].Type == JTokenType.Object)
                        {
                            JObject dragonData = (JObject)data[i];
                            if (dragonData != null && dragonData.ContainsKey("dragon_id"))
                                dragon = new ChampionDragon(dragonData["dragon_id"].Value<int>(), dragonData);
                        }

                        ret[i] = dragon;
                    }

                    return ret;
                }
            }

            JObject data = null;
            public Team(JObject raw)
            {
                data = raw;
            }

            public JArray Off { get { return GetData("a"); } }
            public JArray Def { get { return GetData("d"); } }
            public JArray Hid { get { return GetData("h"); } }
            public JArray GetData(string key)
            {
                return data.ContainsKey(key) && data[key].Type == JTokenType.Array ? (JArray)data[key] : null;
            }
        }

        public Team UserADragons { get; private set; } = null;
        public Team UserBDragons { get; private set; } = null;

        public string GetLogFileName(int matchRound)
        {
            var files = parent.GetBaseValue("filenames");
            if (files.Type == JTokenType.Array)
            {
                JArray array = (JArray)files;
                if (array.Count > matchRound)
                    return array[matchRound].ToString();
            }

            return null;
        }

        ChampionMatchData parent = null;

        public DetailData(ChampionMatchData p)
        {
            parent = p;
        }

        public void SetData(JObject data)
        {
            if (data.ContainsKey("dragons"))
            {
                if (data["dragons"].Type == JTokenType.Array)
                {
                    if(data["dragons"][0].Type == JTokenType.Object)
                        UserADragons = new Team((JObject)data["dragons"][0]);
                    if (data["dragons"][1].Type == JTokenType.Object)
                        UserBDragons = new Team((JObject)data["dragons"][1]);
                }
            }

            if (data.ContainsKey("round_result") && data["round_result"].Type == JTokenType.Array)
            {
                Round1Result = (eChampionWinType)data["round_result"][0].Value<int>();
                Round2Result = (eChampionWinType)data["round_result"][1].Value<int>();
                Round3Result = (eChampionWinType)data["round_result"][2].Value<int>();
            }

            SetBetLog(data);
        }

        public int TOTAL_BET { get { return SIDE_A_BET + SIDE_B_BET; } }
        public int SIDE_A_BET { get; private set; } = 0;
        public int SIDE_B_BET { get; private set; } = 0;
        public eChampionWinType BET_TYPE { get; private set; } = eChampionWinType.None;
        public int MY_BET { get; private set; } = 0;
        public decimal EXPECTED_DIVIDEND { get; private set; } = 0;

        public void SetBetLog(JObject data)
        {
            if(data.ContainsKey("bet_info") && data["bet_info"].Type == JTokenType.Object)
            {
                data = (JObject)data["bet_info"];
            }

            if(data.ContainsKey("my_bet_info"))
            {
                JObject my_bet_info = (JObject)data["my_bet_info"];
                if(my_bet_info.ContainsKey("my_bet_amount"))
                {
                    MY_BET = my_bet_info["my_bet_amount"].Value<int>();
                }
                if (my_bet_info.ContainsKey("my_bet_participant"))
                {
                    var betuser = my_bet_info["my_bet_participant"].Value<long>();
                    if (betuser > 0)
                    {
                        if (parent.luser_no == betuser)
                            BET_TYPE = eChampionWinType.SIDE_A_WIN;
                        else if (parent.ruser_no == betuser)
                            BET_TYPE = eChampionWinType.SIDE_B_WIN;
                    }
                }                
            }

            if(data.ContainsKey("participant_bet") && data["participant_bet"].Type == JTokenType.Object)
            {
                JObject participant_bet = (JObject)data["participant_bet"];
                if (parent.luser_no > 0 && participant_bet.ContainsKey(parent.luser_no.ToString()))
                {
                    SIDE_A_BET = participant_bet[parent.luser_no.ToString()].Value<int>();
                }
                if (parent.ruser_no > 0 && participant_bet.ContainsKey(parent.ruser_no.ToString()))
                {
                    SIDE_B_BET = participant_bet[parent.ruser_no.ToString()].Value<int>();
                }
            }

            if(data.ContainsKey("expected_dividend"))
            {
                EXPECTED_DIVIDEND = decimal.Parse(data["expected_dividend"].Value<string>());
            }
        }
    }


    private JObject baseData = null;

    private JToken GetBaseValue(string key)
    {
        if (baseData != null && baseData.ContainsKey(key))
            return baseData[key];

        return null;
    }

    private int GetBaseInt(string key, int defaultValue = -1)
    {
        var value = GetBaseValue(key);
        if (value != null)
            return value.Value<int>();

        return defaultValue;
    }

    private string GetBaseStr(string key, string defaultValue = "")
    {
        var value = GetBaseValue(key);
        if (value != null)
            return value.Value<string>();

        return defaultValue;
    }

    public ParticipantData WINNER
    {
        get
        {
            switch (MatchResult)
            {
                case eChampionWinType.SIDE_A_WIN:
                    return A_SIDE;
                case eChampionWinType.SIDE_B_WIN:
                    return B_SIDE;
            }

            return null;
        }
    }

    public static List<ChampionLeagueTable.ROUND_INDEX> GetChildRounds(ChampionLeagueTable.ROUND_INDEX index)
    {
        List<ChampionLeagueTable.ROUND_INDEX> ret = new List<ChampionLeagueTable.ROUND_INDEX>();
        switch (index)
        {
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:            
                break;
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND16_A);
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND16_B);
                break;
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND16_C);
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND16_D);
                break;
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND16_E);
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND16_F);
                break;
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND16_G);
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND16_H);
                break;
            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND8_A);
                ret.AddRange(GetChildRounds(ChampionLeagueTable.ROUND_INDEX.ROUND8_A));
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND8_B);
                ret.AddRange(GetChildRounds(ChampionLeagueTable.ROUND_INDEX.ROUND8_B));
                break;
            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND8_C);
                ret.AddRange(GetChildRounds(ChampionLeagueTable.ROUND_INDEX.ROUND8_C));
                ret.Add(ChampionLeagueTable.ROUND_INDEX.ROUND8_D);
                ret.AddRange(GetChildRounds(ChampionLeagueTable.ROUND_INDEX.ROUND8_D));
                break;
            case ChampionLeagueTable.ROUND_INDEX.FINAL:
                ret.Add(ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A);
                ret.AddRange(GetChildRounds(ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A));
                ret.Add(ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B);
                ret.AddRange(GetChildRounds(ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B));
                break;
        }

        return ret;
    }

    public void ShowResult(bool withChild = true, bool sendEvent = true)
    {
        CacheUserData.SetBoolean("champion_view_" + ChampionManager.Instance.CurChampionInfo.CurSeason + "_" + UIROUND_INDEX, true);
        if (ResultUIState.ContainsKey(UIROUND_INDEX))
            ResultUIState[UIROUND_INDEX] = true;
        else
            ResultUIState.Add(UIROUND_INDEX, true);

        if (withChild)
        {
            foreach (var child in GetChildRounds(UIROUND_INDEX))
            {
                CacheUserData.SetBoolean("champion_view_" + ChampionManager.Instance.CurChampionInfo.CurSeason + "_" + child, true);
                if (ResultUIState.ContainsKey(child))
                    ResultUIState[child] = true;
                else
                    ResultUIState.Add(child, true);
            }
        }

        if (sendEvent)
        {
            string toast = "";
            if (MatchResult == eChampionWinType.INVALIDITY)
            {
                if(!User.Instance.ENABLE_P2E)
                    toast = StringData.GetStringFormatByStrKey("덱세팅미완료VS덱세팅미완료_국내");
                else
                    toast = StringData.GetStringFormatByStrKey("덱세팅미완료VS덱세팅미완료");
            }
            else if (A_SIDE == null || B_SIDE == null)
            {
                toast = StringData.GetStringFormatByStrKey("덱세팅완료VS유저없음"); //"덱세팅미완료VS유저없음" 도 동일한 string
            }
            else
            {
                string WinnerNick = "";
                string LoserNick = "";
                if (MatchResult == eChampionWinType.SIDE_A_WIN || Result_Type == eChampionWinType.UNEARNED_WIN_A)
                {
                    WinnerNick = A_SIDE.NICK;
                    LoserNick = B_SIDE.NICK;

                }
                else if (MatchResult == eChampionWinType.SIDE_B_WIN || Result_Type == eChampionWinType.UNEARNED_WIN_B)
                {
                    WinnerNick = B_SIDE.NICK;
                    LoserNick = A_SIDE.NICK;
                }

                if (Result_Type > eChampionWinType.INVALIDITY)
                {
                    toast = StringData.GetStringFormatByStrKey("덱세팅완료VS덱세팅미완료", LoserNick, WinnerNick);
                }
                else
                {
                    toast = StringData.GetStringFormatByStrKey("챔피언승리토스트", WinnerNick);
                }
            }

            if (!string.IsNullOrEmpty(toast))
                ToastManager.On(toast);

            ChampionResultUpdate.Send();
        }
    }
    static int count = 0;
    public static bool IsShowResult(ChampionLeagueTable.ROUND_INDEX index, JObject data = null)
    {
        if (!ResultUIState.ContainsKey(index))
        {
            foreach (var child in GetChildRounds(index))
            {
                if (!IsShowResult(child, data))
                {
                    ResultUIState.Add(index, false);
                    return false;
                }
            }

            if (ChampionManager.Instance.CurChampionInfo.MatchData.ContainsKey(index))
            {
                if (ChampionManager.Instance.CurChampionInfo.MatchData[index].MatchResult == eChampionWinType.INVALIDITY)
                {
                    ResultUIState.Add(index, true);
                    return true;
                }

                if (ChampionManager.Instance.CurChampionInfo.MatchData[index].A_SIDE == null || ChampionManager.Instance.CurChampionInfo.MatchData[index].B_SIDE == null)
                {
                    ResultUIState.Add(index, true);
                    return true;
                }
            }
        
            string key = "champion_view_" + ChampionManager.Instance.CurChampionInfo.CurSeason + "_" + index;
            ResultUIState.Add(index, CacheUserData.GetBoolean(key, false));
        }

        return ResultUIState[index];
    }
    private static ParticipantData GetWinUser(ChampionLeagueTable.ROUND_INDEX index)
    {
        if (ChampionManager.Instance.CurChampionInfo.MatchData.ContainsKey(index)
                    && IsShowResult(index))
            return ChampionManager.Instance.CurChampionInfo.MatchData[index].WINNER;

        return null;
    }

    public ParticipantData A_SIDE
    {
        get
        {
            return GetASideUser(UIROUND_INDEX);
        }
    }

    public static ParticipantData GetASideUser(ChampionLeagueTable.ROUND_INDEX index)
    {
        switch (index)
        {
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
            {
                var slot = (index - ChampionLeagueTable.ROUND_INDEX.ROUND16_A) * 2;
                foreach (var info in ChampionManager.Instance.CurChampionInfo.Participants.Values)
                {
                    if (info.SLOT == slot)
                    {
                        return info;
                    }
                }
            }
            break;
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND16_A);
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND16_C);
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND16_E);
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND16_G);
            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND8_A);
            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND8_C);
            case ChampionLeagueTable.ROUND_INDEX.FINAL:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A);
        }

        return null;
    }
    public ParticipantData B_SIDE
    {
        get
        {
            return GetBSideUser(UIROUND_INDEX);
        }
    }

    public static ParticipantData GetBSideUser(ChampionLeagueTable.ROUND_INDEX index)
    {
        switch (index)
        {
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
            {
                var slot = (index - ChampionLeagueTable.ROUND_INDEX.ROUND16_A) * 2 + 1;
                foreach (var info in ChampionManager.Instance.CurChampionInfo.Participants.Values)
                {
                    if (info.SLOT == slot)
                    {
                        return info;
                    }
                }
            }
            break;
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND16_B);
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND16_D);
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND16_F);
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND16_H);
            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND8_B);
            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.ROUND8_D);
            case ChampionLeagueTable.ROUND_INDEX.FINAL:
                return GetWinUser(ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B);
        }

        return null;
    }

    public ChampionMatchData(JObject d)
    {
        Detail = new DetailData(this);

        SetData(d);
    }

    public void SetData(JObject data)
    {
        baseData = data;

        var index = ChampionManager.Instance.CurChampionInfo.GetRoundIndex(round, match_slot);
        if(index != ChampionLeagueTable.ROUND_INDEX.NONE)
        {
            UIROUND_INDEX = index;
        }

        if (data.ContainsKey("participants") && data["participants"].Type == JTokenType.Array)
        {
            luser_no = data["participants"][0].Value<long>();
            ruser_no = data["participants"][1].Value<long>();
        }

        if (data.ContainsKey("participants_server") && data["participants_server"].Type == JTokenType.Array)
        {
            luser_server = data["participants_server"][0].Value<int>();
            ruser_server = data["participants_server"][1].Value<int>();
        }

        if (data.ContainsKey("win_user_no"))
        {
            MatchResult = eChampionWinType.None;
            long winUserNo = data["win_user_no"].Value<long>();
            if (winUserNo > 0)
            {
                if (luser_no == winUserNo)
                {
                    MatchResult = eChampionWinType.SIDE_A_WIN;
                }
                if (ruser_no == winUserNo)
                {
                    MatchResult = eChampionWinType.SIDE_B_WIN;
                }
            }

            if(data.ContainsKey("result_type"))
            {
                Result_Type = (eChampionWinType)data["result_type"].Value<int>();
                switch (Result_Type)
                {
                    case eChampionWinType.UNEARNED_WIN_A:
                        MatchResult = eChampionWinType.SIDE_A_WIN;
                        break;
                    case eChampionWinType.UNEARNED_WIN_B:
                        MatchResult = eChampionWinType.SIDE_B_WIN;
                        break;
                    case eChampionWinType.INVALIDITY:
                        MatchResult = eChampionWinType.INVALIDITY;
                        break;
                }
            }
        }

        Detail.SetData(data);
    }

    public void SetBetLog(JObject data)
    {
        Detail.SetBetLog(data);
    }

    public bool IsBetTime()
    {
        ChampionInfo.CONTENTS_TIME eTime = ChampionInfo.CONTENTS_TIME.MIN;
        switch (UIROUND_INDEX)
        {
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
            case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
            {
                eTime = ChampionInfo.CONTENTS_TIME.ROUND_16_BET_END;
            }
            break;
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:                
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:                
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:                
            case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
            {
                eTime = ChampionInfo.CONTENTS_TIME.QUARTER_FINAL_BET_END;
            }
            break;
            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
            case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
            {
                eTime = ChampionInfo.CONTENTS_TIME.SEMI_FINAL_BET_END;
            }
            break;
            case ChampionLeagueTable.ROUND_INDEX.FINAL:
            {
                eTime = ChampionInfo.CONTENTS_TIME.FINAL;
            }
            break;
        }

        return TimeManager.GetTimeCompare(ChampionManager.Instance.CurChampionInfo.GetContentsTime(eTime)) > 0;
    }
}
//참가자라면 들고있는 데이터
public class ParticipantData
{
    public enum eTournamentTeamType
    {
        NONE = 0,
        DEFFENCE = 1,
        ATTACK = 2,
        HIDDEN = 3,
    }
    public UserDragonData ChampionDragons { get; private set; } = new UserDragonData();
    private ChampionBattleFormationData championBattleFormationData = new ChampionBattleFormationData();

    public long USER_NO { get; private set; } = -1;
    public string PORTRAIT { get; private set; } = "";
    public string NICK { get; private set; } = "";
    public int LEVEL { get; private set; } = -1;
    public int RANK { get; private set; } = -1;
    public int SLOT { get; private set; } = -1;
    public int GUILD_NO { get; private set; } = -1;
    public string GUILD_NAME { get; private set; } = "";
    public int GUILD_MARK { get; private set; } = -1;
    public int GUILD_EMBLEM { get; private set; } = -1;
    public int SERVER { get; private set; } = -1;
    public ChampionInfo.USER_STATE STATE { get; private set; } = ChampionInfo.USER_STATE.NONE;

    public int PracticeCount { get; private set; } = -1;

    public bool HasGuild { get { return GUILD_NO > 0; } }
    public PortraitEtcInfoData EtcInfo { get; set; } = null;
    public ParticipantData(long no, JObject data)
    {
        USER_NO = no;
        NICK = data["nick"].Value<string>();
        PORTRAIT = data["icon"].Value<string>();
        LEVEL = data["level"].Value<int>();
        RANK = data["rank_grade"].Value<int>();
        SLOT = data["slot"].Value<int>();
        STATE = (ChampionInfo.USER_STATE)data["round_state"].Value<int>();
        GUILD_NO = data["guild_no"].Value<int>();
        GUILD_NAME = data["guild_name"].Value<string>();
        GUILD_MARK = data["guild_mark"].Value<int>();
        GUILD_EMBLEM = data["guild_emblem"].Value<int>();
        
        if (data.ContainsKey("portrait"))
        {
            EtcInfo = new PortraitEtcInfoData(data["portrait"]);
        }

        if (data.ContainsKey("teams"))
            SetChampionBattleFormation(data["teams"]);

        if(data.ContainsKey("server_id"))
            SERVER = data["server_id"].Value<int>();
    }

    public void Clear()
    {
        ChampionDragons.ClearData();

        championBattleFormationData = new();
    }

    //서버에서 온 데이터 세팅
    public void SetChampionBattleFormation(JToken jsonData)
    {
        championBattleFormationData.SetJsonData(jsonData);
    }
    public eTournamentTeamType GetForamtionType(ChampionDragon dragon)
    {
        return championBattleFormationData.GetForamtionType(dragon.Tag);
    }
    public eTournamentTeamType GetForamtionType(int tag)
    {
        return championBattleFormationData.GetForamtionType(tag);
    }
    public ChampionBattleLine GetChampionBattleFomation(eTournamentTeamType type)
    {
        if (championBattleFormationData == null)
            return null;

        return championBattleFormationData.GetChampionBattleFormation(type);
    }
    public void SaveChampionBattleFormation(eTournamentTeamType type, ChampionBattleLine line, NetworkManager.SuccessCallback cb = null)
    {
        ChampionManager.Instance.CurChampionInfo.ReqSetTeam(type, line, cb);
    }
    public List<ChampionDragon> GetAllChampionDragons()
    {
        List<ChampionDragon> ret = new List<ChampionDragon>();
        foreach (ChampionDragon dragon in ChampionDragons.GetAllUserDragons())
        {
            ret.Add(dragon);
        }

        return ret;
    }
    public List<ChampionDragon> GetFormationFreeDragons()
    {
        List<ChampionDragon> ret = new List<ChampionDragon>();
        foreach (ChampionDragon dragon in ChampionDragons.GetAllUserDragons())
        {
            if (GetForamtionType(dragon) == eTournamentTeamType.NONE)
                ret.Add(dragon);
        }

        return ret;
    }
    public void SetSelectedDragons(JObject dragons)
    {
        ChampionDragons.ClearData();
        foreach (var data in dragons.Properties())
        {
            int tag = int.Parse(data.Name);

            var dragon = new ChampionDragon(tag, (JObject)data.Value);
            ChampionDragons.AddUserDragon(tag, dragon);
        }
    }

    public void UpdateDragonInfo(JObject data)
    {
        int tag = data["dragon_id"].Value<int>();

        if (ChampionDragons.IsContainsDragon(tag))
        {
            (ChampionDragons.GetDragon(tag) as ChampionDragon).InitData(tag, data);            
        }
        else
        {
            //일로 타면 원래안됨
            Debug.LogError("이상한 드래곤 데이터?");
            ChampionDragons.AddUserDragon(tag, new ChampionDragon(tag, data));
        }
    }

    public void SetPracticeCount(int count)
    {
        PracticeCount = count;
    }
}

public partial class ChampionManager
{
    public const int CHEER_COIN = 200000001;
    public ChampionInfo CurChampionInfo { get; private set; } = new ChampionInfo();
    public ParticipantData MyInfo { get { return CurChampionInfo.MyInfo; } }
    //챔피언에서 쓸수있는 데이터만 모음
    public static List<CharBaseData> GetSelectableDragons()
    {
        return CharBaseData.GetAllForChampion();
    }
    public static List<PetBaseData> GetSelectablePets()
    {
        return PetBaseData.GetAllForChampion();
    }
    public static List<PetStatData> GetSelectablePetStatsList()
    {
        return PetStatData.GetAllForChampion();
    }
    public static List<SubOptionData> GetSelectableSubOptions(PetBaseData pet, int index)
    {
        return SubOptionData.GetForChampion(pet, index);
    }

    public static List<SubOptionData> GetSelectableSubOptions(PartBaseData part, int index)
    {
        return SubOptionData.GetForChampion(part, index);
    }

    public static List<PartFusionData> GetSelectableFusionOptions()
    {
        return PartFusionData.GetForChampion();
    }

    public static List<SkillPassiveData> GetSelectablePassive(eJobType job, int slotIndex)
    {
        return SkillPassiveData.GetForChampion(job, slotIndex);
    }

    public static List<PartBaseData> GetSelectableParts()
    {
        return PartBaseData.GetAllForChampion();
    }

    public bool SetChampionData(JObject jsonData, bool forceClear = false)
    {
        if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
            return false;

        switch ((eApiResCode)jsonData["rs"].Value<int>())
        {
            case eApiResCode.OK:
            {
                CurChampionInfo.SetData(jsonData, forceClear);
                return true;
            }
            break;
            default:
                break;
        }

        return false;
    }
}

public class ChampionSurpportInfo
{
    public enum eSurpportType
    {
        NONE = -1,
        PHYS_DMG_RESIS,
        ALL_ELEMENT_DMG_RESIS,
        CRI_DMG_RESIS,
    }

    public class SurpportDetial
    {
        public int AngelSurpportValue { get; private set; } = 0;
        public int WonderSurpportValue { get; private set; } = 0;
        public int LunaSurpportValue { get; private set; } = 0;

        public int MySurpportValue { get; private set; } = 0;

        public int AngelRateValue { get; private set; } = 0;
        public int WonderRateValue { get; private set; } = 0;
        public int LunaRateValue { get; private set; } = 0;

        public int AngelStatValue { get; private set; } = 0;
        public int WonderStatValue { get; private set; } = 0;
        public int LunaStatValue { get; private set; } = 0;

        public int TotalValue { get; private set; } = 0;

        public List<ChampionInfo.ROUND_STATE> RewardRound = new List<ChampionInfo.ROUND_STATE>();
        eSurpportType curType = eSurpportType.NONE;
        public SurpportDetial(eSurpportType type)
        {
            curType = type;
            switch (type)
            {
                case eSurpportType.PHYS_DMG_RESIS:
                    TotalValue = GameConfigTable.GetConfigIntValue("CHAMPION_SURPPORT_PHYS_TOTAL", 40);
                    break;
                case eSurpportType.ALL_ELEMENT_DMG_RESIS:
                    TotalValue = GameConfigTable.GetConfigIntValue("CHAMPION_SURPPORT_ELMT_TOTAL", 40);
                    break;
                case eSurpportType.CRI_DMG_RESIS:
                    TotalValue = GameConfigTable.GetConfigIntValue("CHAMPION_SURPPORT_CRID_TOTAL", 40);
                    break;
            }
        }
        //public SurpportDetial(eSurpportType type, int asv, int wsv, int lsv, int msv)
        //{
        //    curType = type;

        //    switch(type)
        //    {
        //        case eSurpportType.PHYS_DMG_RESIS:
        //            TotalValue = GameConfigTable.GetConfigIntValue("CHAMPION_SURPPORT_PHYS_TOTAL", 40);
        //            break;
        //        case eSurpportType.ALL_ELEMENT_DMG_RESIS:
        //            TotalValue = GameConfigTable.GetConfigIntValue("CHAMPION_SURPPORT_ELMT_TOTAL", 40);
        //            break;
        //        case eSurpportType.CRI_DMG_RESIS:
        //            TotalValue = GameConfigTable.GetConfigIntValue("CHAMPION_SURPPORT_CRID_TOTAL", 40);
        //            break;
        //    }
        //    AngelSurpportValue = asv;
        //    WonderSurpportValue = wsv;
        //    LunaSurpportValue = lsv;
        //    MySurpportValue = msv;

        //    AngelRateValue = (int)Math.Round(100f * asv / (asv + wsv + lsv));
        //    WonderRateValue = (int)Math.Round(100f * wsv / (asv + wsv + lsv));
        //    LunaRateValue = (int)Math.Round(100f * lsv / (asv + wsv + lsv));

        //    AngelStatValue = (int)Math.Round((float)TotalValue * (AngelRateValue * 0.01f));
        //    WonderStatValue = (int)Math.Round((float)TotalValue * (WonderRateValue * 0.01f));
        //    LunaStatValue = (int)Math.Round((float)TotalValue * (LunaRateValue * 0.01f));
        //}

        public void SetData(JObject data)
        {
            if(data.ContainsKey("server_magnite") && data["server_magnite"].Type == JTokenType.Object)
            {
                var server_magnite = (JObject)data["server_magnite"];

                AngelSurpportValue = 0;
                WonderSurpportValue = 0;
                LunaSurpportValue = 0;

                if (server_magnite.ContainsKey("1"))
                    AngelSurpportValue = server_magnite["1"].Value<int>();
                if (server_magnite.ContainsKey("2"))
                    WonderSurpportValue = server_magnite["2"].Value<int>();
                if (server_magnite.ContainsKey("3"))
                    LunaSurpportValue = server_magnite["3"].Value<int>();
            }

            if (data.ContainsKey("reward") && data["reward"].Type == JTokenType.Array)
            {
                JArray reward = (JArray)data["reward"];

                RewardRound.Clear();
                foreach (var r in reward)
                {
                    RewardRound.Add((ChampionInfo.ROUND_STATE)r.Value<int>());
                }
            }

            if (data.ContainsKey("buff_rate") && data["buff_rate"].Type == JTokenType.Object)
            {
                var buff_rate = (JObject)data["buff_rate"];

                AngelRateValue = 0;
                WonderRateValue = 0;
                LunaRateValue = 0;

                if (buff_rate.ContainsKey("1"))
                    AngelRateValue = buff_rate["1"].Value<int>();
                if (buff_rate.ContainsKey("2"))
                    WonderRateValue = buff_rate["2"].Value<int>();
                if (buff_rate.ContainsKey("3"))
                    LunaRateValue = buff_rate["3"].Value<int>();
            }
            if (data.ContainsKey("buff_value") && data["buff_value"].Type == JTokenType.Object)
            {
                var buff_value = (JObject)data["buff_value"];

                AngelStatValue = 0;
                WonderStatValue = 0;
                LunaStatValue = 0;

                if (buff_value.ContainsKey("1"))
                    AngelStatValue = buff_value["1"].Value<int>();
                if (buff_value.ContainsKey("2"))
                    WonderStatValue = buff_value["2"].Value<int>();
                if (buff_value.ContainsKey("3"))
                    LunaStatValue = buff_value["3"].Value<int>();
            }

        }

        public void SetMySurpport(int value)
        {
            MySurpportValue = value;
        }

        public void SetDataOnlyRate(JObject buff_rate)
        {
            AngelRateValue = 0;
            WonderRateValue = 0;
            LunaRateValue = 0;

            if (buff_rate.ContainsKey("1"))
                AngelRateValue = buff_rate["1"].Value<int>();
            if (buff_rate.ContainsKey("2"))
                WonderRateValue = buff_rate["2"].Value<int>();
            if (buff_rate.ContainsKey("3"))
                LunaRateValue = buff_rate["3"].Value<int>();
        }
    }

    public SurpportDetial PHYS_DMG_RESIS { get; private set; } = new SurpportDetial(eSurpportType.PHYS_DMG_RESIS);
    public SurpportDetial ALL_ELEMENT_DMG_RESIS { get; private set; } = new SurpportDetial(eSurpportType.ALL_ELEMENT_DMG_RESIS);
    public SurpportDetial CRI_DMG_RESIS { get; private set; } = new SurpportDetial(eSurpportType.CRI_DMG_RESIS);

    public int FinalPrize { get; private set; } = 0;
    public int SemiPrize { get; private set; } = 0;
    //public SurpportDetial PHYS_DMG_RESIS { get; private set; } = new SurpportDetial(eSurpportType.PHYS_DMG_RESIS, UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000));
    //public SurpportDetial ALL_ELEMENT_DMG_RESIS { get; private set; } = new SurpportDetial(eSurpportType.ALL_ELEMENT_DMG_RESIS, UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000));
    //public SurpportDetial CRI_DMG_RESIS { get; private set; } = new SurpportDetial(eSurpportType.CRI_DMG_RESIS, UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000), UnityEngine.Random.Range(0, 10000));
    public SurpportDetial GetSurpportInfo(eSurpportType type)
    {
        switch(type)
        {
            case eSurpportType.PHYS_DMG_RESIS:
                return PHYS_DMG_RESIS;
            case eSurpportType.ALL_ELEMENT_DMG_RESIS:
                return ALL_ELEMENT_DMG_RESIS;
            case eSurpportType.CRI_DMG_RESIS:
                return CRI_DMG_RESIS;
            default:
                return null;
        }
    }

    public void SetData(JObject data)
    {
        if(data.ContainsKey("pdr"))
        {
            PHYS_DMG_RESIS.SetData((JObject)data["pdr"]);
        }

        if(data.ContainsKey("aedr"))
        {
            ALL_ELEMENT_DMG_RESIS.SetData((JObject)data["aedr"]);
        }

        if(data.ContainsKey("cdr"))
        {
            CRI_DMG_RESIS.SetData((JObject)data["cdr"]);
        }

        if (data.ContainsKey("my_magnite") && data["my_magnite"].Type == JTokenType.Object)
        {
            JObject my_magnite = (JObject)data["my_magnite"];
            if (my_magnite.ContainsKey("pdr"))
                PHYS_DMG_RESIS.SetMySurpport(my_magnite["pdr"].Value<int>());
            if (my_magnite.ContainsKey("aedr"))
                ALL_ELEMENT_DMG_RESIS.SetMySurpport(my_magnite["aedr"].Value<int>());
            if (my_magnite.ContainsKey("cdr"))
                CRI_DMG_RESIS.SetMySurpport(my_magnite["cdr"].Value<int>());
        }

        if(data.ContainsKey("final_win_magnite"))
        {
            FinalPrize = data["final_win_magnite"].Value<int>();
        }
        if(data.ContainsKey("final_lost_magnite"))
        {
            SemiPrize = data["final_lost_magnite"].Value<int>();
        }

        ChampionSurpportUpdate.Send();
    }

    public void SetDataOnlyRate(JObject data)
    {
        if (data.ContainsKey("pdr"))
        {
            PHYS_DMG_RESIS.SetDataOnlyRate((JObject)data["pdr"]);
        }

        if (data.ContainsKey("aedr"))
        {
            ALL_ELEMENT_DMG_RESIS.SetDataOnlyRate((JObject)data["aedr"]);
        }

        if (data.ContainsKey("cdr"))
        {
            CRI_DMG_RESIS.SetDataOnlyRate((JObject)data["cdr"]);
        }

        if (data.ContainsKey("final_win_magnite"))
        {
            FinalPrize = data["final_win_magnite"].Value<int>();
        }
        if (data.ContainsKey("final_lost_magnite"))
        {
            SemiPrize = data["final_lost_magnite"].Value<int>();
        }

        ChampionSurpportUpdate.Send();
    }
}
