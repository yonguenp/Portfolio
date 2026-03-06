using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using static ChampionLeagueTable;

namespace SandboxNetwork
{
    public class ChampionBetResultLayer : MonoBehaviour
    {
        [SerializeField]
        private GameObject resultPrefab = null;
        [SerializeField]
        private Transform resultParent = null;
        [SerializeField]
        private Text noMatchLabel = null;

        [SerializeField]
        private GameObject bottomObject = null;
        [SerializeField]
        private Text totalBet = null;
        [SerializeField]
        private Text totalDivide = null;
        [SerializeField]
        private Text winRate = null;

        [SerializeField]
        private Text bottomNotice_inter = null;
        [SerializeField]
        private Text bottomNotice_dome = null;

        private int TotalBet = 0;
        private decimal TotalDivid = 0;
        private int WinRate = 0;
        private float TotalRoundCnt = 0;
        private float MyBetCnt = 0;
        UI_INDEX GetUIIndexByRoundIndex(ROUND_INDEX index)
        {
            switch (index)
            {
                case ROUND_INDEX.ROUND16_A: return UI_INDEX.ROUND16_A;
                case ROUND_INDEX.ROUND16_B: return UI_INDEX.ROUND16_B;
                case ROUND_INDEX.ROUND16_C: return UI_INDEX.ROUND16_C;
                case ROUND_INDEX.ROUND16_D: return UI_INDEX.ROUND16_D;
                case ROUND_INDEX.ROUND16_E: return UI_INDEX.ROUND16_E;
                case ROUND_INDEX.ROUND16_F: return UI_INDEX.ROUND16_F;
                case ROUND_INDEX.ROUND16_G: return UI_INDEX.ROUND16_G;
                case ROUND_INDEX.ROUND16_H: return UI_INDEX.ROUND16_H;
                case ROUND_INDEX.ROUND8_A: return UI_INDEX.ROUND8_A;
                case ROUND_INDEX.ROUND8_B: return UI_INDEX.ROUND8_B;
                case ROUND_INDEX.ROUND8_C: return UI_INDEX.ROUND8_C;
                case ROUND_INDEX.ROUND8_D: return UI_INDEX.ROUND8_D;
                case ROUND_INDEX.SEMI_FINAL_A: return UI_INDEX.SEMI_FINAL_A;
                case ROUND_INDEX.SEMI_FINAL_B: return UI_INDEX.SEMI_FINAL_B;
                case ROUND_INDEX.FINAL: return UI_INDEX.FINAL;
            }

            return UI_INDEX.NONE;
        }

        public void Init()
        {
            ClearData();
            var matchData = ChampionManager.Instance.CurChampionInfo.MatchData;

            //배팅기록이 있다면 보여주기
            var isBetEmpty = true;
            foreach(var data in matchData)
            {
                if(data.Value?.Detail?.BET_TYPE != eChampionWinType.None)
                {
                    isBetEmpty = false;
                    break;
                }
            }
            noMatchLabel.gameObject.SetActive(isBetEmpty);

            if(bottomObject != null)
                bottomObject.SetActive(User.Instance.ENABLE_P2E);

            if (isBetEmpty)
            {
                resultPrefab.SetActive(false);
                return;
            }

            foreach (Transform child in resultParent)
            {
                if (child == resultPrefab.transform)
                    continue;

                Destroy(child.gameObject);
            }

            resultPrefab.SetActive(true);

            foreach (var data in matchData)
            {
                if(data.Value != null && data.Value.Detail != null)
                {
                    if (data.Value.Detail.BET_TYPE != eChampionWinType.SIDE_A_WIN && data.Value.Detail.BET_TYPE != eChampionWinType.SIDE_B_WIN)
                        continue;
                }

                var clone = Instantiate(resultPrefab, resultParent);
                clone.SetActive(true);
                var slotInfo = clone.GetComponent<ChampionResultSlot>();
                slotInfo.Init(data);

                if (data.Value.Detail.BET_TYPE == eChampionWinType.SIDE_A_WIN || data.Value.Detail.BET_TYPE == eChampionWinType.SIDE_B_WIN)
                {
                    if (data.Value.Result_Type == eChampionWinType.UNEARNED_WIN_A || data.Value.Result_Type == eChampionWinType.UNEARNED_WIN_B || data.Value.MatchResult == eChampionWinType.INVALIDITY)
                    {
                        TotalBet += data.Value.Detail.MY_BET;
                        TotalDivid += data.Value.Detail.MY_BET;
                    }
                    else if (data.Value.Detail.BET_TYPE == data.Value.MatchResult)
                    {
                        TotalRoundCnt++;
                        MyBetCnt++;
                        TotalBet += data.Value.Detail.MY_BET;
                        TotalDivid += (data.Value.Detail.EXPECTED_DIVIDEND + data.Value.Detail.MY_BET); 
                    }
                    else if (data.Value.MatchResult == eChampionWinType.SIDE_A_WIN || data.Value.MatchResult == eChampionWinType.SIDE_B_WIN)
                    {
                        TotalRoundCnt++;
                        TotalBet += data.Value.Detail.MY_BET;
                    }
                    else
                    {
                        TotalBet += data.Value.Detail.MY_BET;
                    }
                }
            }
            resultPrefab.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(resultParent.GetComponent<RectTransform>());

            SetBottomUI();
        }

        void SetBottomUI()
        {
            if (bottomNotice_inter != null && bottomNotice_dome != null)
            {
                bottomNotice_inter.gameObject.SetActive(User.Instance.ENABLE_P2E);
                bottomNotice_dome.gameObject.SetActive(!User.Instance.ENABLE_P2E);
            }

            if (totalDivide != null && totalBet != null && winRate != null)
            {
                totalBet.text = StringData.GetStringFormatByStrKey("총배팅금액", SBFunc.CommaFromNumber(TotalBet));
                totalDivide.text = StringData.GetStringFormatByStrKey("총예상배당금", (System.Math.Floor(TotalDivid * 100) / 100).ToString("N2"));
                if(TotalRoundCnt > 0)
                    WinRate = (int)((MyBetCnt / TotalRoundCnt) * 100);
                winRate.text = StringData.GetStringFormatByStrKey("적중률", WinRate.ToString());
            }
        }

        void ClearData()
        {
            TotalBet = 0;
            TotalDivid = 0;
            WinRate = 0;
            TotalRoundCnt = 0;
            MyBetCnt = 0;

            SetBottomUI();
        }
    }
}