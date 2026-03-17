using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Google.Impl;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace SandboxNetwork
{
    enum betIconState
    {
        LOSE = 0,
        NOBET = 1,
        NOTYET = 2,
        WIN = 3,
        INVALIDITY = 4,
    }
    public class ChampionResultSlot : MonoBehaviour
    {
        [Header("SideA")]
        [SerializeField] 
        private UserArenaPortraitFrame portraitA = null;
        [SerializeField]
        private Text userNameLabelA = null;
        [SerializeField]
        private GuildBaseInfoObject guildBaseInfoObjectA = null;
        [SerializeField]
        private Text myBetA = null;
        [SerializeField]
        private Text totalBetA = null;
        [SerializeField]
        private GameObject winIconA = null;
        [SerializeField]
        private GameObject dimdA = null;
        [SerializeField]
        private GameObject cheerAObj = null;
        [SerializeField]
        ServerFlagUI serverA = null;

        [Space(10f)]
        [Header("SideB")]
        [SerializeField] 
        private UserArenaPortraitFrame portraitB = null;
        [SerializeField]
        private Text userNameLabelB = null;
        [SerializeField]
        private GuildBaseInfoObject guildBaseInfoObjectB = null;
        [SerializeField]
        private Text myBetB = null;
        [SerializeField]
        private Text totalBetB = null;
        [SerializeField]
        private GameObject winIconB = null; 
        [SerializeField]
        private GameObject dimdB = null;
        [SerializeField]
        private GameObject cheerBObj = null;
        [SerializeField]
        ServerFlagUI serverB = null;

        [Space(10f)]
        [Header("Result")]
        [SerializeField]
        private List<GameObject> betResult = null;
        [SerializeField]
        private GameObject betIcon = null;
        [SerializeField]
        private GameObject cardIcon = null;
        [SerializeField]
        private Text getAmount = null;
        [SerializeField]
        private Text getAmountWeb2 = null;
        [SerializeField]
        private GameObject betIconDimd = null;

        [SerializeField]
        private GameObject myBetAObj = null;
        [SerializeField]
        private GameObject myBetBObj = null;

        [Space(10f)]
        [Header("Round")]
        [SerializeField]
        private Text roundText = null;

        private string CurRoundIndex = "";

        private bool AmIBet = false;

        private ChampionMatchData CurrentData = null;

        private string[] RewardDataArr = null;

        //진행완료 + 진행예정(대전상대까지 발표된) 매치를 모두 보여주자.
        // 진행완료
        // 0. 배팅 안함
        //    0-1. 결과 : No Bet
        //    0-2. 보상 미노출
        // 1. 배팅 완료
        //    1-1. 배팅성공
        //         1-1-1. 결과 : Win
        //         1-1-2. 보상 노출
        //    1-2. 배팅실패
        //         1-2-1. 결과 : Lose
        //         1-2-2. 보상 노출 + 딤처리


        public void Init(KeyValuePair<ChampionLeagueTable.ROUND_INDEX, ChampionMatchData> MatchData)
        {
            //이긴쪽에 win icon 노출
            CurrentData = MatchData.Value;

            if (CurrentData.MatchResult == eChampionWinType.SIDE_A_WIN)
            {
                winIconA.SetActive(true);
                winIconB.SetActive(false);

                dimdB.SetActive(true);
                dimdA.SetActive(false);
            }
            else if(CurrentData.MatchResult == eChampionWinType.SIDE_B_WIN)
            {
                winIconA.SetActive(false);
                winIconB.SetActive(true);

                dimdA.SetActive(true);
                dimdB.SetActive(false);
            }
            else
            {
                winIconA.SetActive(false);
                winIconB.SetActive(false);

                dimdB.SetActive(false);
                dimdA.SetActive(false);
            }

            SetDomesticIcon();

            var myBet = CurrentData.Detail.MY_BET;

            switch (CurrentData.Detail.BET_TYPE)
            {
                case eChampionWinType.SIDE_A_WIN:
                {
                    AmIBet = true;
                    myBetA.text = SBFunc.CommaFromNumber(myBet);
                    myBetB.text = "0";

                    SetCheerObj(true);
                }
                break;
                case eChampionWinType.SIDE_B_WIN:
                {
                    AmIBet = true;
                    myBetA.text = "0";
                    myBetB.text = SBFunc.CommaFromNumber(myBet);

                    SetCheerObj(false);
                }
                break;
                default:
                {
                    AmIBet = false;
                    myBetA.text = "0";
                    myBetB.text = "0";

                    SetCheerObj(false, false);
                }
                break;
            }

            betIconState state = betIconState.NOBET;
            
            if (CurrentData.Detail.BET_TYPE == eChampionWinType.SIDE_A_WIN || CurrentData.Detail.BET_TYPE == eChampionWinType.SIDE_B_WIN)
            {
                if (CurrentData.Result_Type == eChampionWinType.UNEARNED_WIN_A || CurrentData.Result_Type == eChampionWinType.UNEARNED_WIN_B || CurrentData.MatchResult == eChampionWinType.INVALIDITY)
                {
                    state = betIconState.INVALIDITY;
                }
                else if (CurrentData.Detail.BET_TYPE == CurrentData.MatchResult)
                {
                    state = betIconState.WIN;
                }
                else if (CurrentData.MatchResult == eChampionWinType.SIDE_A_WIN || CurrentData.MatchResult == eChampionWinType.SIDE_B_WIN)
                {
                    state = betIconState.LOSE;
                }
                else
                {
                    state = betIconState.NOTYET;
                }
            }                        
            else
            {
                state = betIconState.NOBET;
            }

            //4R, 3R, 2R, 1R 텍스트표시
            if (MatchData.Key >= ChampionLeagueTable.ROUND_INDEX.ROUND16_A && MatchData.Key < ChampionLeagueTable.ROUND_INDEX.ROUND8_A)
            {
                roundText.text = StringData.GetStringByStrKey("16강");
                CurRoundIndex = ((int)ChampionLeagueTable.ROUND_INDEX.ROUND16_START).ToString();
            }
            else if (MatchData.Key >= ChampionLeagueTable.ROUND_INDEX.ROUND8_A && MatchData.Key < ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A)
            {
                roundText.text = StringData.GetStringByStrKey("8강");
                CurRoundIndex = ((int)ChampionLeagueTable.ROUND_INDEX.ROUND8_START).ToString();
            }
            else if (MatchData.Key >= ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A && MatchData.Key < ChampionLeagueTable.ROUND_INDEX.FINAL_START)
            {
                roundText.text = StringData.GetStringByStrKey("준결승");
                CurRoundIndex = ((int)ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_START).ToString();
            }
            else if (MatchData.Key >= ChampionLeagueTable.ROUND_INDEX.FINAL_START)
            {
                roundText.text = StringData.GetStringByStrKey("결승");
                CurRoundIndex = ((int)ChampionLeagueTable.ROUND_INDEX.FINAL_START).ToString();
            }

            SetDomesticReward();

            SetBetIcon(state);

            SetParticipants();

        }

        void SetDomesticReward()
        {
            string rewardData = GameConfigTable.GetCheerRewardWithRound(CurRoundIndex);
            RewardDataArr = rewardData.Split(',');
            cardIcon.GetComponent<Image>().sprite = ItemBaseData.Get(RewardDataArr[1]).ICON_SPRITE;
        }

        public void OnItemTooltip()
        {
            if (RewardDataArr != null && RewardDataArr.Length > 0 && cardIcon != null)
                ItemToolTip.OnItemToolTip(int.Parse(RewardDataArr[1]), cardIcon);
        }

        void SetParticipants()
        {
            if (portraitA != null)
            {
                if (CurrentData.A_SIDE != null && !string.IsNullOrEmpty(CurrentData.A_SIDE.NICK))
                    portraitA.SetUserPortraitFrame(new ThumbnailUserData(CurrentData.A_SIDE.USER_NO, CurrentData.A_SIDE.NICK, CurrentData.A_SIDE.PORTRAIT, CurrentData.A_SIDE.LEVEL, CurrentData.A_SIDE.EtcInfo), false);
                else
                    portraitA.SetUserPortraitFrame("");
            }

            if (portraitB != null)
            {
                if (CurrentData.B_SIDE != null && !string.IsNullOrEmpty(CurrentData.B_SIDE.NICK))
                    portraitB.SetUserPortraitFrame(new ThumbnailUserData(CurrentData.B_SIDE.USER_NO, CurrentData.B_SIDE.NICK, CurrentData.B_SIDE.PORTRAIT, CurrentData.B_SIDE.LEVEL, CurrentData.B_SIDE.EtcInfo), false);
                else
                    portraitB.SetUserPortraitFrame("");
            }

            if (userNameLabelA != null && userNameLabelB != null)
            {
                if (CurrentData.A_SIDE != null)
                {
                    userNameLabelA.text = CurrentData.A_SIDE.NICK;
                    SetGuildInfo(CurrentData.A_SIDE, guildBaseInfoObjectA);
                }
                else
                {
                    userNameLabelA.text = "";
                    guildBaseInfoObjectA.gameObject.SetActive(false);
                }

                if (CurrentData.B_SIDE != null)
                {
                    userNameLabelB.text = CurrentData.B_SIDE.NICK;
                    SetGuildInfo(CurrentData.B_SIDE, guildBaseInfoObjectB);
                }
                else
                {
                    userNameLabelB.text = "";
                    guildBaseInfoObjectB.gameObject.SetActive(false);
                }
            }

            if (serverA != null)
                serverA.SetFlag(CurrentData.A_SIDE.SERVER);

            if (serverB != null)
                serverB.SetFlag(CurrentData.B_SIDE.SERVER);

            if (totalBetA != null && totalBetB != null)
            {
                if (CurrentData.A_SIDE != null)
                {
                    totalBetA.text = SBFunc.CommaFromNumber(CurrentData.Detail.SIDE_A_BET);
                }
                else
                {
                    totalBetA.text = "";
                }

                if (CurrentData.B_SIDE != null)
                {
                    totalBetB.text = SBFunc.CommaFromNumber(CurrentData.Detail.SIDE_B_BET);
                }
                else
                {
                    totalBetB.text = "";
                }
            }
        }

        void SetDomesticIcon()
        {
            myBetAObj.SetActive(User.Instance.ENABLE_P2E);
            myBetBObj.SetActive(User.Instance.ENABLE_P2E);
            cheerAObj.SetActive(!User.Instance.ENABLE_P2E);
            cheerBObj.SetActive(!User.Instance.ENABLE_P2E);
        }

        void SetCheerObj(bool _isASideBet, bool _isBet = true)
        {
            cheerAObj.transform.Find("iconActive").GetComponent<RectTransform>().gameObject.SetActive(_isBet && _isASideBet);
            cheerAObj.transform.Find("iconDimd").GetComponent<RectTransform>().gameObject.SetActive(_isBet && !_isASideBet);
            cheerBObj.transform.Find("iconActive").GetComponent<RectTransform>().gameObject.SetActive(_isBet && !_isASideBet);
            cheerBObj.transform.Find("iconDimd").GetComponent<RectTransform>().gameObject.SetActive(_isBet && _isASideBet);

            if (!_isBet)
            {
                cheerAObj.transform.Find("iconDimd").GetComponent<RectTransform>().gameObject.SetActive(true);
                cheerBObj.transform.Find("iconDimd").GetComponent<RectTransform>().gameObject.SetActive(true);
            }
        }

        void SetBetIcon(betIconState betState)
        {
            SetResultIconState(betState);
            switch (betState)                
            {
                case betIconState.WIN:
                    cardIcon.SetActive(CurrentData.Detail.MY_BET <= 0);
                    betIcon.SetActive(!(CurrentData.Detail.MY_BET <= 0));

                    betIconDimd.SetActive(false);
                    getAmountWeb2.text = "" + (int.Parse(RewardDataArr[2]) > 0 ? RewardDataArr[2] : "");
                    getAmount.text = ((System.Math.Floor(CurrentData.Detail.EXPECTED_DIVIDEND * 100) / 100) + CurrentData.Detail.MY_BET).ToString("N2");
                    break;
                case betIconState.INVALIDITY:
                    cardIcon.SetActive(CurrentData.Detail.MY_BET <= 0);
                    betIcon.SetActive(!(CurrentData.Detail.MY_BET <= 0));
                    betIconDimd.SetActive(CurrentData.Detail.MY_BET <= 0);
                    getAmount.text = ((int)CurrentData.Detail.MY_BET).ToString();
                    getAmountWeb2.text = "";
                    break;
                case betIconState.LOSE:
                    cardIcon.SetActive(CurrentData.Detail.MY_BET <= 0);
                    betIcon.SetActive(!(CurrentData.Detail.MY_BET <= 0));
                    betIconDimd.SetActive(true);
                    getAmount.text = "0";
                    getAmountWeb2.text = "";
                    break;
                case betIconState.NOBET:
                    betIcon.SetActive(false);
                    cardIcon.SetActive(false);
                    betIconDimd.SetActive(true);
                    break;
                case betIconState.NOTYET:
                    betIcon.SetActive(false);
                    cardIcon.SetActive(false);
                    betIconDimd.SetActive(true);
                    break;
            }
        }

        void SetResultIconState(betIconState _betIconState)
        {
            for (int i = 0; i < betResult.Count; i++){
                if (i == (int)_betIconState)
                {
                    betResult[i].SetActive(true);
                    continue;
                }
                betResult[i].SetActive(false);
            }
        }

        void SetRankingInfoSlotData()
        {
            //if (!string.IsNullOrEmpty(currentArenaData.userName) && userNameLabel != null)
            //    userNameLabel.text = currentArenaData.userName;

            //if (portrait != null)
            //    portrait.SetUserPortraitFrame(currentArenaData.UserData, false);

            //userNameLabel.text = currentArenaData.userName;
            //battlePointLabel.text = currentArenaData.userBattlePoint.ToString();
            //arenaPointLabel.text = currentArenaData.UserData.Point.ToString();

            //int rank = currentArenaData.userRank;
            //if (rank > 0 && rank <= 3)
            //{
            //    rankIcon.gameObject.SetActive(true);
            //    rankLabel.gameObject.SetActive(false);
            //    rankIcon.sprite = rankIconSpriteArr[rank - 1];
            //    if (backGroundImg !=null) 
            //        backGroundImg.color = backGroundColors[rank - 1];
            //}
            //else
            //{
            //    rankIcon.gameObject.SetActive(false);
            //    rankLabel.gameObject.SetActive(true);
            //    rankLabel.text = rank < 0 ? "-" : rank.ToString();
            //    if (backGroundImg != null)
            //        backGroundImg.color = backGroundColors[3];
            //}
            SetOtherTeamThumbnail();
        }

        void SetOtherTeamThumbnail()
        {
            //var dragonTagList = currentArenaData.userDefenceTeamList;
            //if (dragonTagList == null || dragonTagList.Count <= 0) return;
            //if (UserDragonPortraits.Count < dragonTagList.Count)
            //{
            //    for (int i = UserDragonPortraits.Count; i < dragonTagList.Count; ++i)
            //    {
            //        var clone = Instantiate(dragonPortraitSlotPrefab, dragonPortraitParent);
            //        clone.SetActive(true);
            //        UserDragonPortraits.Add(clone);
            //    }

            //}
            //else if (UserDragonPortraits.Count > dragonTagList.Count)
            //{
            //    for (int i = dragonTagList.Count; i < UserDragonPortraits.Count; ++i)
            //    {
            //        UserDragonPortraits[i].SetActive(false);
            //    }
            //}
            //for (int i = 0; i < dragonTagList.Count; ++i)
            //{
                
            //    if (dragonTagList[i].Tag == 0)
            //    {
            //        UserDragonPortraits[i].SetActive(false);
            //        continue;
            //    }
            //    UserDragonPortraits[i].SetActive(true);
            //    var slot = UserDragonPortraits[i].GetComponent<DragonPortraitFrame>();
            //    slot.SetCustomPotraitFrame(dragonTagList[i].Tag, dragonTagList[i].Level, dragonTagList[i].TranscendenceStep ,dragonTagList[i].Tag < 0, false);
            //}
        }

        void SetGuildInfo(ParticipantData championInfo, GuildBaseInfoObject guildBaseInfoObject)
        {
            bool enableGuild = GuildManager.Instance.GuildWorkAble && championInfo.HasGuild;
            guildBaseInfoObject.gameObject.SetActive(enableGuild);
            if (enableGuild)                
            {

                int markNo = championInfo.GUILD_MARK;
                int emblemNo = championInfo.GUILD_EMBLEM;
                string guildName = championInfo.GUILD_NAME;
                int guildNo = championInfo.GUILD_NO;

                guildBaseInfoObject.Init(guildName, markNo, emblemNo, guildNo);
            }
        }
    }
}