using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandboxNetwork;
using Newtonsoft.Json.Linq;
using TMPro;

public class ChampionBattlePlayerInfo : Popup<PopupData>
{
    [Header("Team Info")]
    [SerializeField]
    private Text offenceBattlePointLabel = null;
    [SerializeField]
    private Text defenceBattlePointLabel = null;
    [SerializeField]
    private Text hiddenBattlePointLabel = null;
    public void init()
    {
        RefreshData();
    }
    public void RefreshData()
    {
        RefreshOffenceBattlePoint();
        RefreshDefenceBattlePoint();
        RefreshHiddenBattlePoint();
    }

    private void RequestSceneChange(string targetSceneName)
    {
        LoadingManager.Instance.EffectiveSceneLoad(targetSceneName, eSceneEffectType.CloudAnimation);
    }

    public void RefreshOffenceBattlePoint()
    {
        if(offenceBattlePointLabel != null)
            offenceBattlePointLabel.text = ChampionManager.Instance.CurChampionInfo.AmIParticipant ? CalcTotalBattlePoint(ChampionManager.Instance.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.ATTACK)).ToString() : "0";
    }
    public void RefreshDefenceBattlePoint()
    {
        if (defenceBattlePointLabel != null)
            defenceBattlePointLabel.text = ChampionManager.Instance.CurChampionInfo.AmIParticipant ? CalcTotalBattlePoint(ChampionManager.Instance.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.DEFFENCE)).ToString() : "0";
    }
    public void RefreshHiddenBattlePoint()
    {
        if (hiddenBattlePointLabel != null)
            hiddenBattlePointLabel.text = ChampionManager.Instance.CurChampionInfo.AmIParticipant ? CalcTotalBattlePoint(ChampionManager.Instance.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.HIDDEN)).ToString() : "0";
    }
    public override void InitUI()
    {

    }

    public int CalcTotalBattlePoint(ChampionBattleLine line)
    {
        int totalPoint = 0;
        if (line == null) return totalPoint;

        return line.GetTotalINF();
    }

    public void onClickExpectGameAlphaUpdate()
    {

    }
}

