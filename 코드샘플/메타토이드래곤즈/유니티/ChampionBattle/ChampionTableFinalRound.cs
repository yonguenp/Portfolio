using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionTableFinalRound : ChampionTableRound
{
    [SerializeField]
    ChampionTableUserSlot ChampionWinnerSlot;

    [SerializeField]
    private GameObject LeftTimePanel = null;
    [SerializeField]
    private Text LeftTime = null;

    [SerializeField]
    private Sprite TrophySprite = null;
    [SerializeField]
    private Sprite TrophyDimSprite = null;
    [SerializeField]
    private Image TrophyImage = null;

    [SerializeField]
    private Sprite TopbarSprite = null;
    [SerializeField]
    private Sprite TopbarDimSprite = null;
    [SerializeField]
    private Image TopbarImage = null;

    [SerializeField]
    GameObject SurpportUI = null;

    private ChampionInfo CurChampInfo { get { return ChampionManager.Instance.CurChampionInfo; } }
    private TimeEnable OpenTimer = null;
    public override void SetRoundInfo(ChampionLeagueTable.ROUND_INDEX index, ParticipantData a, ParticipantData b, eChampionWinType winType)
    {
        base.SetRoundInfo(index, a, b, winType);

        ParticipantData winner = null;

        if (!ChampionMatchData.IsShowResult(index))
        {
            winType = eChampionWinType.HIDE;
        }

        if (winType == eChampionWinType.SIDE_A_WIN)
        {
            winner = a;
        }
        if (winType == eChampionWinType.SIDE_B_WIN)
        {
            winner = b;
        }

        if (winner == null)
        {
            ChampionWinnerSlot.SetActive(false);
        }
        else
        {
            ChampionWinnerSlot.SetActive(true);
            ChampionWinnerSlot.SetUser(winner, winner != null, true);
        }

        SetLeftTimePanel();
    }
    private void RefreshTime()
    {
        if (LeftTime == null)
            return;

        int time = CurChampInfo.ContentsStepRemainTime;
        if (time > 0)
        {
            LeftTime.text = SBFunc.TimeString(time);

            if (time <= 3600) 
                LeftTime.color = Color.yellow;
            else
                LeftTime.color = Color.white;
        }
        else
        {
            LeftTime.text = SBFunc.TimeString(0);
            LeftTimePanel.SetActive(false);
        }
    }

    void SetLeftTimePanel()
    {

        if (LeftTimePanel == null || LeftTime == null) return;

        var curChampionState = ChampionManager.Instance.CurChampionInfo.CurState;

        SurpportUI.SetActive(false);

        if (curChampionState > ChampionInfo.ROUND_STATE.PREPARATION) 
        {
            LeftTimePanel.SetActive(false);
            TrophyImage.sprite = TrophySprite;
            TopbarImage.sprite = TopbarSprite;
            if(curChampionState < ChampionInfo.ROUND_STATE.RESULT)
                SurpportUI.SetActive(true);
        }
        else
        {
            LeftTimePanel.SetActive(true);
            TrophyImage.sprite = TrophyDimSprite;
            TopbarImage.sprite = TopbarDimSprite;

            OpenTimer = LeftTime.GetComponent<TimeEnable>();

            if (OpenTimer == null)
                OpenTimer = LeftTime.gameObject.AddComponent<TimeEnable>();

            if (OpenTimer != null)
                OpenTimer.Refresh = RefreshTime;
        }
    }
}

