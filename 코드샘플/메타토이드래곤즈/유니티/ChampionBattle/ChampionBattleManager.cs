using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SandboxNetwork;
using System.Linq;

public class ChampionBattleDragonInfo
{
    public int Tag { get; private set; } = 0;
    public int Level { get; private set; } = 0;
    public int TranscendenceStep { get; private set; } = 0;

    public ChampionBattleDragonInfo(int tag, int lv, int transcendenceStep = 0)
    {
        Tag = tag;
        Level = lv;
        TranscendenceStep = transcendenceStep;
    }
}

public class ChampionBattleResultDragonStat
{
    public ChampionBattleResultDragonStat(char bTag, int dTag, int level, JArray jsonArray)
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

public partial class ChampionManager
{
    private static ChampionManager instance;

    public static ChampionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ChampionManager();
                instance.Init();
            }
            return instance;
        }
    }
    public ChampionMatchData CurMatchData { get; private set; } = null;

    private ChampionBattleBattleData championBattleData = null;
    public PracticeBattleData PracticeBattleData { get; private set; } = null;
    public ChampionBattleBattleData ChampionData
    {
        get
        {
            if (PracticeBattleData != null)
                return PracticeBattleData;

            if (championBattleData == null)
            {
                championBattleData = new ChampionBattleBattleData();
                championBattleData.Initialize();
            }
            return championBattleData;
        }
    }


    public delegate void Callback();
    private Callback RefreshUICallBack = null;

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
    public int CurRoundIndex { get; private set; } = 0;// zero base 
    public bool Playing { get; private set; } = true;
    public void SetPlay(bool p)
    {
        Playing = p;
    }
    public void SetArenaBattleInfo(BattleInfo info)
    {
        battleInfo = info;
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
        //toto
        //CurMatchData setting
    }

    public void OnPracticeStart(JArray off, JArray def)
    {
        PracticeBattleData = new PracticeBattleData();
        PracticeBattleData.Set(off, def);
    }

    public void OnPracticeEnd()
    {
        PracticeBattleData = null;
    }

    public void OnReplayStart(ChampionLeagueTable.ROUND_INDEX index)
    {
        CurMatchData = CurChampionInfo.GetMatchData(index);
        OnRoundStart();
    }
    public void OnRoundEnd()
    {
        LogRelease();
        OnRoundStart(CurRoundIndex + 1);
    }

    public void ForceExitWithErrPopup()
    {
        if (ChampionManager.Instance.Playing)
        {
            SetPlay(false);
            var logPath = GetLogPath(CurRoundIndex);

            LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() =>
            {
                DeleteLog(logPath);
                MatchInfoPopup.OpenPopup();
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("리플레이이상동작"));
            }));
        }
    }

    public void OnRoundStart(int round = 0)
    {
        CurRoundIndex = round;
        
        var logPath = GetLogPath(CurRoundIndex);
        bool returnMenu = string.IsNullOrEmpty(logPath);
        if(!returnMenu)
        {
            switch(round)
            {
                case 0:
                    returnMenu = !(CurMatchData.Detail.Round1Result == eChampionWinType.SIDE_A_WIN || CurMatchData.Detail.Round1Result == eChampionWinType.SIDE_B_WIN);
                    break;
                case 1:
                    returnMenu = !(CurMatchData.Detail.Round2Result == eChampionWinType.SIDE_A_WIN || CurMatchData.Detail.Round2Result == eChampionWinType.SIDE_B_WIN);
                    break;
                case 2:
                    returnMenu = !(CurMatchData.Detail.Round3Result == eChampionWinType.SIDE_A_WIN || CurMatchData.Detail.Round3Result == eChampionWinType.SIDE_B_WIN);
                    break;
            }
            
        }

        if(returnMenu)
        {
            LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => {
                CurMatchData.ShowResult();
                MatchInfoPopup.OpenPopup();
            }));
            return;
        }

        //indecator 필요없을까?
        Game.Instance.StartCoroutine(LoadLog(logPath, () => {
            switch (round)
            {
                case 0:
                    ChampionData.Set(CurLoger.UserA.DefenceTeam, CurLoger.UserB.OffenceTeam);
                    break;
                case 1:
                    ChampionData.Set(CurLoger.UserA.OffenceTeam, CurLoger.UserB.DefenceTeam);
                    break;
                case 2:
                    ChampionData.Set(CurLoger.UserA.HiddenTeam, CurLoger.UserB.HiddenTeam);
                    break;
            }
            LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleColosseum", eSceneEffectType.CloudAnimation, ChampionData.StartLoadingCO());            
        }, ()=> {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("데이터 로드에 실패하였습니다."));
        }));
    }

    public string GetLogPath(int matchRound)
    {
        ////todo 서버에서 로그 이름 가져오기
        //return SBGameManager.CurServerTag + "_" + CurMatchData.season_id + "_" + CurMatchData.round + "_" + CurMatchData.match_slot + "_" + matchRound + ".dat";
        return CurMatchData.Detail.GetLogFileName(matchRound);
    }
}