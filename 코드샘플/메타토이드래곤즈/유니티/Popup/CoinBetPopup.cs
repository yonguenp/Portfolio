using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class CoinBetPopup : Popup<PopupBase>
    {
        [SerializeField] Text userAmountText = null;
        [SerializeField] Text betAmountText = null;

        [SerializeField] Slider inputSlider = null;

        private int step = 100; // 100단위씩 계산하기 위함
        private int min_bet = 100;
        int betAmount = 100; // 사용예정
        int userAmount = 0; // 보유

        public ChampionMatchData MatchData { get; private set; } = null;
        public ParticipantData ParticipantData { get; private set; } = null;
        public static void OpenPopup(ChampionMatchData match, ParticipantData user)
        {
            var popup = PopupManager.OpenPopup<CoinBetPopup>();
            popup.MatchData = match;
            popup.ParticipantData = user;
        }

        public override void InitUI()
        {
            SetData();
        }

        public void SetData()
        {
            betAmount = min_bet;
            userAmount = 0; 

            userAmount = User.Instance.ORACLE;
            userAmountText.text = "" + userAmount;

            userAmount = Mathf.RoundToInt(userAmount / step) * step;
            UpdateState();
        }


        public void OnClickMinusButton()
        {
            if (betAmount == min_bet || userAmount < min_bet)
            {
                return;
            }
            betAmount -= step;

            UpdateState();
        }

        public void OnClickPlusButton()
        {
            if (betAmount == userAmount || userAmount < min_bet)
            {
                return;
            }
            betAmount += step;
            UpdateState();
        }

        public void OnClickMaxButton()
        {
            if (betAmount == userAmount || userAmount < min_bet)
            {
                return;
            }

            betAmount = Mathf.RoundToInt(userAmount / step) * step;
            UpdateState();
        }

        public void OnClickMinButton()
        {
            if (betAmount == min_bet || userAmount < min_bet)
            {
                return;
            }

            betAmount = min_bet;

            UpdateState();
        }
        public void OnInputSlider()
        {
            if (inputSlider.value == betAmount)
            {
                return;
            }

            if(inputSlider.value == userAmount)
            {
                inputSlider.value = Mathf.RoundToInt(inputSlider.value / step) * step;
                betAmount = Mathf.RoundToInt(inputSlider.value / step) * step;
                UpdateState();
                return;
            }
            inputSlider.value = Mathf.RoundToInt(inputSlider.value / step) * step;
            betAmount = Mathf.RoundToInt(inputSlider.value / step) * step;
            UpdateState();
        }

        public void OnClickOkButton()
        {
            if (userAmount <= 0 || betAmount < min_bet)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("코인개수경고"));
                return;
            }

            SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("챔피언배팅확인문구", SBFunc.CommaFromNumber(betAmount)), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
            () => {
                ChampionManager.Instance.CurChampionInfo.ReqBet(MatchData, ParticipantData, betAmount,
                () => {
                    ClosePopup();
                    var popup = PopupManager.GetPopup<MatchInfoPopup>();
                    if (popup != null)
                    {
                        popup.RefreshUI();
                    }
                });
            },
            () => { }
            );
        }
        
        void UpdateState()
        {
            betAmountText.text = SBFunc.CommaFromNumber(betAmount).ToString();

            if(userAmount < min_bet)
            {
                betAmount = 0;
                if (inputSlider != null)
                {
                    inputSlider.gameObject.SetActive(false);
                }
            }
            else
            {
                if (inputSlider != null)
                {
                    if (betAmount < min_bet)
                    {
                        betAmount = min_bet;
                    }
                    inputSlider.gameObject.SetActive(true);
                    inputSlider.minValue = 100;
                    inputSlider.maxValue = userAmount;
                    inputSlider.value = betAmount;                   
                }
            }
        }

        public void OnClickClose()
        {
            ClosePopup();
        }
    }
}