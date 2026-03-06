using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Google.Impl;
using System;
using Newtonsoft.Json.Linq;
using static SandboxNetwork.ChampionBattleInfoSlot;

namespace SandboxNetwork
{
    public class ChampionBattleInfoSlot : MonoBehaviour
    {
        enum DeckRound
        {
            ROUND1,
            ROUND2,
            ROUND3,
            ROUND_MAX
        }
        enum DeckIconType
        {
            ATK,
            DEF,
            HID,
            DEFEAT_ATK,
            DEFEAT_DEF,
            DEFEAT_HID,
            MAX
        }

        [Serializable]
        public class RoundRow
        {
            [SerializeField]
            public GameObject[] deckIcon = null;
            [SerializeField]
            public Transform dragonPortraitParent = null;
            [SerializeField]
            public GameObject Screen = null;
            [SerializeField]
            public Text desc = null;

            public void Clear()
            {
                foreach (Transform child in dragonPortraitParent)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        [Header("portrait")]
        [SerializeField] private UserArenaPortraitFrame portrait = null;

        [SerializeField] private Text userNameLabel = null;
        [SerializeField] private Text myBetCnt = null;
        [SerializeField] private GameObject winImg = null;

        [SerializeField] private GameObject betInfo = null;

        [SerializeField] private GameObject dragonPortraitSlotPrefab = null;
        [SerializeField] private RoundRow[] roundRow = new RoundRow[(int)DeckRound.ROUND_MAX];


        [SerializeField] private GameObject wholeDimd = null;

        [SerializeField]
        private List<GameObject> domesticBetIcon = new List<GameObject>();
        [SerializeField]
        ServerFlagUI ServerFlag = null;

        [Header("guild")]
        [SerializeField] private GuildBaseInfoObject guildBaseInfoObject = null;

        private ChampionMatchData matchData = null;
        private ParticipantData currentData = null;

        private List<Vector3> AtkInitPos = new List<Vector3>();
        private List<Vector3> DefInitPos = new List<Vector3>();

        private bool IsWin = true;
        private bool IsOneSided = true;
        private bool IsMatchOver = false;

        private eChampionWinType round1 = eChampionWinType.None;
        private eChampionWinType round2 = eChampionWinType.None;
        private eChampionWinType round3 = eChampionWinType.None;
        private MatchInfoPopup parentPopup = null;

        bool bSideA = false;

        bool buttonLock = false;

        string PrefixText = "";

        void Start()
        {
            Refresh();
        }
        public void Init(bool sideA, ChampionMatchData ChampionInfo, MatchInfoPopup parent)
        {
            parentPopup = parent;

            if (ChampionInfo == null) return;

            matchData = ChampionInfo;
            bSideA = sideA;

            IsMatchOver = ChampionInfo.WINNER != null || ChampionInfo.round < ChampionManager.Instance.CurChampionInfo.CurState;

            Refresh();
        }

        public void Refresh()
        {
            if (matchData == null) return;

            //TODO 챔피언대전
            if (bSideA)
            {
                currentData = matchData.A_SIDE;
                IsWin = matchData.MatchResult == eChampionWinType.SIDE_A_WIN;
            }
            else
            {
                currentData = matchData.B_SIDE;
                IsWin = matchData.MatchResult == eChampionWinType.SIDE_B_WIN;
            }

            if (ChampionMatchData.IsShowResult(matchData.UIROUND_INDEX))
            {
                //라운드 결과
                round1 = matchData.Detail.Round1Result;
                round2 = matchData.Detail.Round2Result;
                round3 = matchData.Detail.Round3Result;
            }
            else
            {
                round1 = eChampionWinType.HIDE;
                round2 = eChampionWinType.HIDE;
                round3 = eChampionWinType.HIDE;
            }

            //2:0으로 게임이 끝났는지 ?
            IsOneSided = round1 == round2;

            if (userNameLabel != null)
            {
                if (currentData != null && !string.IsNullOrEmpty(currentData.NICK))
                    userNameLabel.text = currentData.NICK;
                else
                    userNameLabel.text = "";
            }

            if (portrait != null)
            {
                if (currentData != null && !string.IsNullOrEmpty(currentData.NICK))
                    portrait.SetUserPortraitFrame(new ThumbnailUserData(currentData.USER_NO, currentData.NICK, currentData.PORTRAIT, currentData.LEVEL, currentData.EtcInfo), false);
                else
                    portrait.SetUserPortraitFrame("");
            }

            if(ServerFlag != null)
            {
                ServerFlag.SetFlag(currentData.SERVER);
            }

            SetGuildInfo();

            betInfo.SetActive(User.Instance.ENABLE_P2E);

            if (!User.Instance.ENABLE_P2E)
            {
                //국내IP 승부예측시 예측아이콘노출, 반대편은 딤처리된 아이콘노출
                //승부예측한 쪽이라면
                switch (matchData.Detail.BET_TYPE)
                {
                    case eChampionWinType.SIDE_A_WIN:
                    {
                        domesticBetIcon[0].SetActive(bSideA);
                        domesticBetIcon[1].SetActive(!bSideA);
                    }
                    break;
                    case eChampionWinType.SIDE_B_WIN:
                    {
                        domesticBetIcon[0].SetActive(!bSideA);
                        domesticBetIcon[1].SetActive(bSideA);
                    }
                    break;
                    default:
                    {
                        domesticBetIcon[0].SetActive(false);
                        domesticBetIcon[1].SetActive(false);
                    }
                    break;
                }
            }
            else
            {
                //내 배팅금액 설정

                int mybet = matchData.Detail.MY_BET;

                switch (matchData.Detail.BET_TYPE)
                {
                    case eChampionWinType.SIDE_A_WIN:
                    {
                        if (bSideA)
                        {

                            myBetCnt.text = SBFunc.CommaFromNumber(mybet);
                        }
                        else
                        {
                            myBetCnt.text = "0";
                        }
                    }
                    break;
                    case eChampionWinType.SIDE_B_WIN:
                    {
                        if (bSideA)
                        {
                            myBetCnt.text = "0";
                        }
                        else
                        {

                            myBetCnt.text = SBFunc.CommaFromNumber(mybet);
                        }
                    }
                    break;
                    default:
                    {
                        myBetCnt.text = "0";
                    }
                    break;
                }
            }

            wholeDimd.SetActive(IsMatchOver && ChampionMatchData.IsShowResult(matchData.UIROUND_INDEX) && !IsWin);
            winImg.SetActive(IsMatchOver && ChampionMatchData.IsShowResult(matchData.UIROUND_INDEX) && IsWin);

            ChampionInfo.CONTENTS_TIME CurStateToContentsTime = ChampionInfo.CONTENTS_TIME.MIN;
            switch (matchData.round)
            {
                case ChampionInfo.ROUND_STATE.ROUND_OF_16:
                {
                    CurStateToContentsTime = ChampionInfo.CONTENTS_TIME.ROUND_OF16;
                }
                break;
                case ChampionInfo.ROUND_STATE.QUARTER_FINALS:
                {
                    CurStateToContentsTime = ChampionInfo.CONTENTS_TIME.QUARTER_FINAL;
                }
                break;
                case ChampionInfo.ROUND_STATE.SEMI_FINALS:
                {
                    CurStateToContentsTime = ChampionInfo.CONTENTS_TIME.SEMI_FINAL;
                }
                break;
                case ChampionInfo.ROUND_STATE.FINAL:
                {
                    CurStateToContentsTime = ChampionInfo.CONTENTS_TIME.FINAL;
                }
                break;
                default:
                    Debug.LogError("ROUND ERROR");
                    break;
            }


            for (DeckRound round = DeckRound.ROUND1; round < DeckRound.ROUND_MAX; round++)
            {
                var curRoundUI = roundRow[(int)round];
                foreach (var icon in curRoundUI.deckIcon)
                {
                    icon.SetActive(false);
                }

                curRoundUI.desc.text = "";

                PrefixText = "";
                bool bScreen = !IsMatchOver;
                if (!IsMatchOver)
                {
                    switch (ChampionManager.Instance.CurChampionInfo.CurStep)
                    {
                        //덱 공개전
                        case ChampionInfo.ROUND_STEP.MATCH_TEAM_SETTING:
                        {
                            bScreen = true;
                            if (round == DeckRound.ROUND1)
                            {
                                PrefixText = StringData.GetStringByStrKey("방어덱공개까지");
                                SetRemainTime(ChampionInfo.CONTENTS_TIME.DEF_TEAM_SET, curRoundUI, PrefixText);
                            }
                            else if (round == DeckRound.ROUND2)
                            {
                                PrefixText = StringData.GetStringByStrKey("공격덱공개까지");
                                SetRemainTime(ChampionInfo.CONTENTS_TIME.ATK_TEAM_SET, curRoundUI, PrefixText);
                            }
                            else
                            {
                                PrefixText = StringData.GetStringByStrKey("경기시작까지");
                                SetRemainTime(CurStateToContentsTime, curRoundUI, PrefixText);
                            }
                        }
                        break;
                        //공격덱만 공개
                        case ChampionInfo.ROUND_STEP.MATCH_DEFENSE_OPEN:
                        {
                            bScreen = !(round == DeckRound.ROUND1);
                            if (bScreen && round == DeckRound.ROUND2)
                            {
                                PrefixText = StringData.GetStringByStrKey("공격덱공개까지");
                                SetRemainTime(ChampionInfo.CONTENTS_TIME.ATK_TEAM_SET, curRoundUI, PrefixText);
                            }
                            else if (bScreen && round == DeckRound.ROUND3)
                            {
                                PrefixText = StringData.GetStringByStrKey("경기시작까지");
                                SetRemainTime(CurStateToContentsTime, curRoundUI, PrefixText);
                            }
                        }
                        break;
                        //공격덱 + 방어덱 공개
                        case ChampionInfo.ROUND_STEP.MATCH:
                        case ChampionInfo.ROUND_STEP.MATCH_ATTACK_OPEN:
                        {
                            bScreen = !(round == DeckRound.ROUND1 || round == DeckRound.ROUND2);
                            if (bScreen && round == DeckRound.ROUND3)
                            {
                                PrefixText = StringData.GetStringByStrKey("경기시작까지");
                                SetRemainTime(CurStateToContentsTime, curRoundUI, PrefixText);
                            }
                        }
                        break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch(round)
                    {
                        case DeckRound.ROUND3:
                            bScreen = round3 == eChampionWinType.HIDE;
                            break;
                    }
                }

                curRoundUI.Screen.SetActive(bScreen);

                switch (round)
                {
                    case DeckRound.ROUND1:
                        if (!bScreen)
                            SetThumbnail(curRoundUI, round, round1);
                        switch (round1)
                        {
                            case eChampionWinType.SIDE_A_WIN:
                                if (bSideA)
                                    curRoundUI.deckIcon[(int)DeckIconType.DEF].SetActive(true);
                                else
                                    curRoundUI.deckIcon[(int)DeckIconType.DEFEAT_ATK].SetActive(true);
                                break;
                            case eChampionWinType.SIDE_B_WIN:
                                if (bSideA)
                                    curRoundUI.deckIcon[(int)DeckIconType.DEFEAT_DEF].SetActive(true);
                                else
                                    curRoundUI.deckIcon[(int)DeckIconType.ATK].SetActive(true);
                                break;
                            default:
                                curRoundUI.deckIcon[(int)DeckIconType.DEF].SetActive(true);
                                break;
                        }
                        break;
                    case DeckRound.ROUND2:
                        if (!bScreen)
                            SetThumbnail(curRoundUI, round, round2);
                        switch (round2)
                        {
                            case eChampionWinType.SIDE_A_WIN:
                                if (bSideA)
                                    curRoundUI.deckIcon[(int)DeckIconType.ATK].SetActive(true);
                                else
                                    curRoundUI.deckIcon[(int)DeckIconType.DEFEAT_DEF].SetActive(true);
                                break;
                            case eChampionWinType.SIDE_B_WIN:
                                if (bSideA)
                                    curRoundUI.deckIcon[(int)DeckIconType.DEFEAT_ATK].SetActive(true);
                                else
                                    curRoundUI.deckIcon[(int)DeckIconType.DEF].SetActive(true);
                                break;
                            default:
                                curRoundUI.deckIcon[(int)DeckIconType.ATK].SetActive(true);
                                break;
                        }
                        break;
                    case DeckRound.ROUND3:
                        if (IsOneSided && IsWin)
                        {
                            curRoundUI.Screen.SetActive(true);
                            curRoundUI.deckIcon[(int)DeckIconType.HID].SetActive(true);
                            curRoundUI.desc.text = StringData.GetStringByStrKey("히든덱미공개");
                        }
                        else
                        {
                            if (!bScreen)
                                SetThumbnail(curRoundUI, round, round3);
                            switch (round3)
                            {
                                case eChampionWinType.SIDE_A_WIN:
                                    if (bSideA)
                                        curRoundUI.deckIcon[(int)DeckIconType.HID].SetActive(true);
                                    else
                                        curRoundUI.deckIcon[(int)DeckIconType.DEFEAT_HID].SetActive(true);
                                    break;
                                case eChampionWinType.SIDE_B_WIN:
                                    if (bSideA)
                                        curRoundUI.deckIcon[(int)DeckIconType.DEFEAT_HID].SetActive(true);
                                    else
                                        curRoundUI.deckIcon[(int)DeckIconType.HID].SetActive(true);
                                    break;
                                default:
                                    curRoundUI.deckIcon[(int)DeckIconType.HID].SetActive(true);
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private void SetRemainTime(ChampionInfo.CONTENTS_TIME _contentsTime, RoundRow _roundRow, string _prefixText)
        {
            var RemainTime = ChampionManager.Instance.CurChampionInfo.GetContentsTime(_contentsTime);
            var Time = TimeManager.GetTimeCompare(RemainTime);

            if (Time > 0 && _roundRow.desc != null)
            {
                _roundRow.desc.text = _prefixText + " " + SBFunc.TimeString(Time);

                parentPopup.AddTimer(() =>
                {
                    var remain = TimeManager.GetTimeCompare(RemainTime);
                    _roundRow.desc.text = _prefixText + " " + SBFunc.TimeString(remain);

                    return remain < 0;
                });
            }
        }

        void SetThumbnail(RoundRow roundRow, DeckRound round, eChampionWinType winType)
        {
            roundRow.Clear();
            var portraitParent = roundRow.dragonPortraitParent;

            ChampionMatchData.DetailData.Team team = null;

            if (bSideA)
                team = matchData.Detail.UserADragons;
            else
                team = matchData.Detail.UserBDragons;


            if (team == null)
            {
                if (string.IsNullOrEmpty(roundRow.desc.text))
                    roundRow.desc.text = StringData.GetStringByStrKey("챔피언세팅드래곤없음");
                return;
            }

            ChampionDragon[] dragonArray = null;

            switch (round)
            {
                case DeckRound.ROUND1:
                    if (winType == eChampionWinType.SIDE_A_WIN || winType == eChampionWinType.SIDE_B_WIN)
                    {
                        if (bSideA)
                            dragonArray = team.DefenceTeam;
                        else
                            dragonArray = team.OffenceTeam;
                    }
                    else
                    {
                        dragonArray = team.DefenceTeam;
                    }
                    break;
                case DeckRound.ROUND2:
                    if (winType == eChampionWinType.SIDE_A_WIN || winType == eChampionWinType.SIDE_B_WIN)
                    {
                        if (bSideA)
                            dragonArray = team.OffenceTeam;
                        else
                            dragonArray = team.DefenceTeam;
                    }
                    else
                    {
                        dragonArray = team.OffenceTeam;
                    }
                    break;
                case DeckRound.ROUND3:
                    dragonArray = team.HiddenTeam;
                    break;
            }

            if (dragonArray == null || dragonArray.Length != 6) return;


            foreach (Transform child in portraitParent)
            {
                if (child == dragonPortraitSlotPrefab.transform)
                    continue;

                Destroy(child.gameObject);
            }

            bool HasDragon = false;
            dragonPortraitSlotPrefab.SetActive(true);
            foreach (var dragon in dragonArray)
            {
                if (dragon == null)
                    continue;

                var clone = Instantiate(dragonPortraitSlotPrefab, portraitParent);
                clone.SetActive(true);
                var slot = clone.GetComponent<DragonPortraitFrame>();
                slot.SetCustomPotraitFrame(dragon.Tag, dragon.Level, dragon.TranscendenceStep, dragon.Tag < 0, false);
                slot.setCallback((param) =>
                {
                    var dragonData = new UserDragonData();
                    foreach (var dragon in dragonArray)
                    {
                        if (dragon == null)
                            continue;
                        dragonData.AddUserDragon(dragon.Tag, dragon);
                    }

                    ShowChampionDragonDetailPopup(dragonData, dragon);
                });

                HasDragon = true;
            }

            if(!HasDragon)
            {
                if(string.IsNullOrEmpty(roundRow.desc.text))
                    roundRow.desc.text = StringData.GetStringByStrKey("챔피언세팅드래곤없음");
            }

            dragonPortraitSlotPrefab.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(portraitParent.GetComponent<RectTransform>());
        }

        public void ShowChampionDragonDetailPopup(UserDragonData dragons, ChampionDragon dragon)
        {
            ChampionDragonDetailPopup.OpenPopup(dragon, dragons);

        }
        void SetGuildInfo()
        {
            if (guildBaseInfoObject == null)
                return;

            if (currentData == null)
            {
                guildBaseInfoObject.gameObject.SetActive(false);
                return;
            }    

            bool enableGuild = GuildManager.Instance.GuildWorkAble && currentData.HasGuild;
            guildBaseInfoObject.gameObject.SetActive(enableGuild);
            if (enableGuild)
            {
                int markNo = currentData.GUILD_MARK;
                int emblemNo = currentData.GUILD_EMBLEM;
                string guildName = currentData.GUILD_NAME;
                int guildNo = currentData.GUILD_NO;

                guildBaseInfoObject.Init(guildName, markNo, emblemNo, guildNo);
            }
        }
    }
}
