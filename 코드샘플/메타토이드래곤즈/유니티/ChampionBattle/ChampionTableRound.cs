using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionTableRound : MonoBehaviour
{
    [SerializeField]
    ChampionTableUserSlot User_A;

    [SerializeField]
    ChampionTableUserSlot User_B;

    [SerializeField]
    ChampionLeagueLine Line;

    [SerializeField]
    Sprite ResultSprite;
    [SerializeField]
    Sprite WaitSprite;
    [SerializeField]
    Sprite WaitDimSprite;

    [SerializeField]
    GameObject TimePanel;
    [SerializeField]
    Text TimeText;

    private ParticipantData SideA = null;
    private ParticipantData SideB = null;

    public ChampionLeagueTable.ROUND_INDEX UI_ROUND_INDEX { get; private set; } 
    public virtual void SetRoundInfo(ChampionLeagueTable.ROUND_INDEX index, ParticipantData a, ParticipantData b, eChampionWinType winType)
    {
        UI_ROUND_INDEX = index;
        SideA = a;
        SideB = b;

        if(winType == eChampionWinType.SIDE_A_WIN || winType == eChampionWinType.SIDE_B_WIN)
        {
            if(winType == eChampionWinType.SIDE_A_WIN && a == null)
            {
                winType = eChampionWinType.HIDE;
            }
            else if (winType == eChampionWinType.SIDE_B_WIN && b == null)
            {
                winType = eChampionWinType.HIDE;
            }
            else if (!ChampionMatchData.IsShowResult(UI_ROUND_INDEX))
            {
                winType = eChampionWinType.HIDE;
            }
        }

        Line.SetLine(winType);
        var curChampionState = ChampionManager.Instance.CurChampionInfo.CurState;

        bool after = false;
        if (winType != eChampionWinType.HIDE)
        {
            switch (UI_ROUND_INDEX)
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
                    after = curChampionState > ChampionInfo.ROUND_STATE.ROUND_OF_16;
                }
                break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                {
                    after = curChampionState > ChampionInfo.ROUND_STATE.QUARTER_FINALS;
                }
                break;
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                {
                    after = curChampionState > ChampionInfo.ROUND_STATE.SEMI_FINALS;
                }
                break;
                case ChampionLeagueTable.ROUND_INDEX.FINAL:
                {
                    after = curChampionState > ChampionInfo.ROUND_STATE.FINAL;
                }
                break;
                default:
                    TimePanel.SetActive(false);
                    break;
            }
        }

        TimePanel.SetActive(!after && curChampionState > ChampionInfo.ROUND_STATE.PREPARATION);

        User_A.SetUser(SideA, after, winType == eChampionWinType.SIDE_A_WIN);
        User_B.SetUser(SideB, after, winType == eChampionWinType.SIDE_B_WIN);

        bool isOnTime = true;
        if (TimePanel.activeInHierarchy)
        {
            int time = 0;
            switch(UI_ROUND_INDEX)
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
                    time = ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.ROUND_OF16);
                    isOnTime = curChampionState == ChampionInfo.ROUND_STATE.ROUND_OF_16;
                }
                break;
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                {
                    time = ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.QUARTER_FINAL);
                    isOnTime = curChampionState == ChampionInfo.ROUND_STATE.QUARTER_FINALS;
                }
                break;
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                {
                    time = ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.SEMI_FINAL);
                    isOnTime = curChampionState == ChampionInfo.ROUND_STATE.SEMI_FINALS;
                }
                break;
                case ChampionLeagueTable.ROUND_INDEX.FINAL:
                {
                    time = ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.FINAL);
                    isOnTime = curChampionState == ChampionInfo.ROUND_STATE.FINAL;
                }
                break;
                default:
                    TimePanel.SetActive(false);
                    break;
            }

            if (time - TimeManager.GetTime() > 0)
            {
                TimePanel.GetComponent<Image>().sprite = isOnTime ? WaitSprite : WaitDimSprite;
                TimeText.text = SBFunc.TimeStampDeepRemainString(time);

                Color color = TimeText.color;          
                color.a = isOnTime ? 1f : 0.5f;        
                TimeText.color = color;               
            }
            else if(winType == eChampionWinType.HIDE)
            {
                TimePanel.GetComponent<Image>().sprite = ResultSprite;
                TimeText.text = StringData.GetStringByStrKey("결과확인");
            }
            else
            {
                TimePanel.GetComponent<Image>().sprite = WaitSprite;
                TimeText.text = StringData.GetStringByStrKey("집계중");
            }
        }
    }

    public void OnSelectRound()
    {
        if (ChampionManager.Instance.CurChampionInfo.GetMatchData(UI_ROUND_INDEX) == null)
        {       
            return;
        }
        //Debug.LogError(ROUND_INDEX.ToString());
        MatchInfoPopup.OpenPopup(UI_ROUND_INDEX);
    }
}

