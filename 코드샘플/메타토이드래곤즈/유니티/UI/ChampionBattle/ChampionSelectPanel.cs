using DG.Tweening;
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionSelectPanel : MonoBehaviour
{
    [SerializeField]
    Image PanelImage;
    [SerializeField]
    Color NoneColor;
    [SerializeField]
    Color NormalColor;

    [SerializeField]
    Text StateText;

    [SerializeField]
    Button RegistButton;
    [SerializeField]
    Text RegistText;
    [SerializeField]
    Sprite EnableSprite;
    [SerializeField]
    Sprite DisableSprite;

    [SerializeField]
    GameObject StepObject;
    [SerializeField]
    Text StepText;

    [SerializeField]
    GameObject StatusObject;
    [SerializeField]
    Text StatusText;

    [SerializeField]
    Text TimerDesc;
    [SerializeField]
    Text Timer;

    [SerializeField]
    Text DateDesc;
    [SerializeField]
    Text StartDate;
    [SerializeField]
    Text EndDate;

    [SerializeField]
    CanvasGroup canvasGroup;

    private TimeEnable championBattleTimeEnable = null;
    private ChampionInfo CurChampInfo { get { return ChampionManager.Instance.CurChampionInfo; } }
    private ParticipantData MyInfo { get { return ChampionManager.Instance.MyInfo; } }

    float height { get { return (transform as RectTransform).sizeDelta.y; } }
    public void OnEnable()
    {
        transform.DOKill();
        transform.localPosition = new Vector3(transform.localPosition.x, (height/2) + 300f, transform.localPosition.z);
        transform.DOLocalMoveY((height / 2), 0.5f);

        canvasGroup.DOKill();
        canvasGroup.alpha = 0.0f;
        canvasGroup.DOFade(1.0f, 0.5f);
    }
    public void OnClose()
    {
        transform.DOKill();
        transform.localPosition = new Vector3(transform.localPosition.x, (height / 2), transform.localPosition.z);
        transform.DOLocalMoveY((height / 2) - 300f, 0.5f);

        canvasGroup.DOKill();
        canvasGroup.alpha = 1.0f;
        canvasGroup.DOFade(0.0f, 0.5f);
    }

    public void RefreshUI()
    {
        if (Timer != null)
        {
            championBattleTimeEnable = Timer.GetComponent<TimeEnable>();
            if (championBattleTimeEnable == null)
                championBattleTimeEnable = Timer.gameObject.AddComponent<TimeEnable>();

            if (championBattleTimeEnable != null)
                championBattleTimeEnable.Refresh = RefreshChampionTime;
        }

        Color color = NormalColor;
        bool onRegistButton = false;
        string stateText = "";
        string statusText = "";
        string stepText = "";
        string timerDesc = "";
        string registText = "";

        switch (CurChampInfo.CurState)
        {
            case ChampionInfo.ROUND_STATE.NONE:
                stateText = "";
                color = NoneColor;
                timerDesc = StringData.GetStringByStrKey("다음챔피언대전남은시간");
                stateText = StringData.GetStringByStrKey("챔피언일정종료");
                break;
            case ChampionInfo.ROUND_STATE.PREPARATION:
                stateText = StringData.GetStringByStrKey("챔피언참가신청");
                switch(CurChampInfo.CurStep)
                {
                    case ChampionInfo.ROUND_STEP.APPLICATION:
                        onRegistButton = true;
                        Image graphic = RegistButton.targetGraphic as Image;
                        if (graphic != null)
                        {
                            if (CurChampInfo.UserState == ChampionInfo.USER_STATE.NONE)
                            {
                                if (!CurChampInfo.ParticipationQualifications) 
                                {
                                    graphic.sprite = DisableSprite;
                                    registText = StringData.GetStringByStrKey("참가자격미달");
                                }
                                else
                                {
                                    graphic.sprite = EnableSprite;
                                    registText = StringData.GetStringByStrKey("참가신청");
                                }
                            }
                            else
                            {
                                graphic.sprite = DisableSprite;
                                registText = StringData.GetStringByStrKey("참가신청완료");
                            }
                        }
                        timerDesc = StringData.GetStringByStrKey("챔피언참가신청남은시간");
                        break;
                    case ChampionInfo.ROUND_STEP.SELECTION:
                        stepText = StringData.GetStringByStrKey("챔피언선수선발중");
                        timerDesc = StringData.GetStringByStrKey("챔피언발표남은시간");
                        break;
                    case ChampionInfo.ROUND_STEP.ANNOUNCE:
                        stepText = StringData.GetStringByStrKey("챔피언선수발표");
                        statusText = CurChampInfo.AmIParticipant ? StringData.GetStringByStrKey("챔피언참가자") : StringData.GetStringByStrKey("챔피언관전자");
                        timerDesc = StringData.GetStringByStrKey("개막남은시간");
                        break;
                    default:                        
                        break;
                }
                break;
            case ChampionInfo.ROUND_STATE.ROUND_OF_16:
                stateText = StringData.GetStringByStrKey("챔피언16강");
                stepText = GetBattleStepText();
                statusText = CurChampInfo.AmIParticipant ? StringData.GetStringByStrKey("챔피언참가자") : StringData.GetStringByStrKey("챔피언관전자");
                timerDesc = StringData.GetStringByStrKey("다음경기남은시간");
                break;
            case ChampionInfo.ROUND_STATE.QUARTER_FINALS:
                stateText = StringData.GetStringByStrKey("챔피언8강");
                stepText = GetBattleStepText();
                statusText = CurChampInfo.AmIParticipant ? StringData.GetStringByStrKey("챔피언참가자") : StringData.GetStringByStrKey("챔피언관전자");
                timerDesc = StringData.GetStringByStrKey("다음경기남은시간");
                break;
            case ChampionInfo.ROUND_STATE.SEMI_FINALS:
                stateText = StringData.GetStringByStrKey("챔피언4강");
                stepText = GetBattleStepText();
                statusText = CurChampInfo.AmIParticipant ? StringData.GetStringByStrKey("챔피언참가자") : StringData.GetStringByStrKey("챔피언관전자");
                timerDesc = StringData.GetStringByStrKey("다음경기남은시간");
                break;
            case ChampionInfo.ROUND_STATE.FINAL:
                stateText = StringData.GetStringByStrKey("챔피언결승");
                stepText = GetBattleStepText();
                statusText = CurChampInfo.AmIParticipant ? StringData.GetStringByStrKey("챔피언참가자") : StringData.GetStringByStrKey("챔피언관전자");
                timerDesc = StringData.GetStringByStrKey("다음경기남은시간");
                break;
            case ChampionInfo.ROUND_STATE.RESULT:
                stateText = StringData.GetStringByStrKey("챔피언결과");
                statusText = StringData.GetStringByStrKey("우승자확인하기");
                timerDesc = StringData.GetStringByStrKey("다음경기남은시간");
                if (ChampionMatchData.IsShowResult(ChampionLeagueTable.ROUND_INDEX.FINAL_START) && CurChampInfo.Participants.Count > 0)
                {
                    if (CurChampInfo.Participants.ContainsKey(CurChampInfo.WinUserNo))
                        statusText = CurChampInfo.Participants[CurChampInfo.WinUserNo].NICK;
                }
                break;
        }

        RegistButton.gameObject.SetActive(onRegistButton);
        RegistText.text = registText;
        StateText.text = stateText;
        StatusObject.SetActive(!string.IsNullOrEmpty(statusText));
        StatusText.text = statusText;
        StepObject.SetActive(!string.IsNullOrEmpty(stepText));
        StepText.text = stepText;
        PanelImage.color = color;
        TimerDesc.text = timerDesc;

        if(CurChampInfo.EndDate < TimeManager.GetDateTime())
        {
            transform.parent.SetAsLastSibling();
            DateDesc.text = StringData.GetStringByStrKey("예정대회기간");
            StartDate.text = CurChampInfo.NextStartDate.ToString("yyyy.MM.dd");
            EndDate.text = CurChampInfo.NextEndDate.ToString("yyyy.MM.dd");

            //transform.SetAsLastSibling();
        }
        else
        {
            transform.parent.SetSiblingIndex(1);
            DateDesc.text = StringData.GetStringByStrKey("대회기간");
            StartDate.text = CurChampInfo.StartDate.ToString("yyyy.MM.dd");
            EndDate.text = CurChampInfo.EndDate.ToString("yyyy.MM.dd");

            //transform.SetSiblingIndex(1);
        }
    }
    private void RefreshChampionTime()
    {
        if (Timer == null)
            return;

        int time = CurChampInfo.ContentsStepRemainTime;
        if (time > 0)
            Timer.text = SBFunc.TimeString(time);
        else
            Timer.text = SBFunc.TimeString(0);
    }
    public void OnClickChampionBattleButton()
    {
        if(CurChampInfo.CurState == ChampionInfo.ROUND_STATE.PREPARATION)
        {
            switch (CurChampInfo.CurStep)                
            {
                case ChampionInfo.ROUND_STEP.APPLICATION:
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언대전선수등록중"));
                    return;
                case ChampionInfo.ROUND_STEP.SELECTION:
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언대전선수선발중"));
                    return;
            }
        }

        PopupManager.ClosePopup<DungeonSelectPopup>();

        if (CurChampInfo.AmIParticipant && (ChampionManager.Instance.MyInfo == null || ChampionManager.Instance.MyInfo.GetAllChampionDragons().Count <= 0))//참가자 이면서 드래곤 세팅이 안되있는 유저는 드래곤 선택 UI
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트11"), StringData.GetStringByStrKey("확인"), "",
            () => { ChampionBattleDragonSelectPopup.OpenPopup(0); }
            );
        }
        else
            LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation);        
    }

    public void OnRegist()
    {
        if (CurChampInfo.UserState == ChampionInfo.USER_STATE.APPLIER || CurChampInfo.UserState == ChampionInfo.USER_STATE.PARTICIPANT)
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트3"), StringData.GetStringByStrKey("확인"), "",
                () => { }
            );
            return;
        }

        if (!CurChampInfo.ParticipationQualifications)
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("참가자격미달"));
            return;
        }

        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언텍스트1"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
                () => {
                    ChampionManager.Instance.CurChampionInfo.ReqRegist((jsonData) => {
                        if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                            return;

                        switch ((eApiResCode)jsonData["rs"].Value<int>())
                        {
                            case eApiResCode.OK:
                            {
                                NetworkManager.Send("user/dungeonstate", null, (jsonData) => {
                                    ChampionManager.Instance.SetChampionData(jsonData);
                                    RefreshUI();
                                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언신청완료"));
                                },
                                (json) => {
                                });

                                
                                return;
                            }
                            break;
                            default:
                            {
                                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("챔피언신청실패"));
                                return;
                            }
                            break;
                        }
                    });
                },
                () => { }
                );
    }

    public string GetBattleStepText()
    {
        string stepText = "";

        switch (CurChampInfo.CurStep)
        {
            case ChampionInfo.ROUND_STEP.MATCH_TEAM_SETTING:
                stepText = StringData.GetStringByStrKey("챔피언팀설정");
                break;
            case ChampionInfo.ROUND_STEP.MATCH_DEFENSE_OPEN:
                stepText = StringData.GetStringByStrKey("챔피언방어공개");
                break;
            case ChampionInfo.ROUND_STEP.MATCH_ATTACK_OPEN:
                stepText = StringData.GetStringByStrKey("챔피언공격공개");
                break;
            case ChampionInfo.ROUND_STEP.MATCH:
                stepText = StringData.GetStringByStrKey("챔피언매치중");
                break;
            case ChampionInfo.ROUND_STEP.RESULT:
                stepText = StringData.GetStringByStrKey("챔피언매치결과");
                break;
        }

        return stepText;
    }
}
