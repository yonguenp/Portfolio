using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DiceEventRankingLayer : EventRankingTabLayer
    {
        const string EVENT_DAILY_RANK_PREFIX = "2023_EVENT_DAILY_RANK:";

        EventDiceBaseData holidayData = null;

        protected override void SetEventData()
        {
            if (holidayData == null)
                holidayData = DiceEventPopup.GetHolidayData();
        }
        protected override void ShowRankPageBySubLayerIndex()
        {
            switch (SubLayerIndex)
            {
                case 0:  // 누적랭크
                    ShowPageProcess(false);
                    break;
                case 1: // 일일랭크
                    SetTodayTitleText(StringData.GetStringByStrKey("2023_EVENT_MENU3_2_DAILYRANK"));
                    SetVisibleTipText(true);
                    ShowPageProcess(true);
                    break;
                case 2: // pvp 랭크                    
                    ShowPageProcess(false);
                    break;
            }
        }
        protected override void RequestRankingData(int _page)//1,2,3
        {
            if (holidayData == null)
                return;

            ClearRankingScroll(_page);

            holidayData.RequestEventRankingPage(_page, (jsonData) =>
            {
                RefreshRankingScrollview(jsonData);
            });
        }

        protected override void SetMissionText(int curEvent)
        {
            if (SubLayerIndex == 1)
            {
                if (rankRewardMissionText != null)
                {
                    switch (curEvent)
                    {
                        case 0:
                            rankRewardMissionText.text = StringData.GetStringByStrKey("이벤트종료안내");
                            break;
                        default:
                            rankRewardMissionText.text = StringData.GetStringByStrKey("다이스일일랭킹:" + curEvent);
                            break;
                    }
                }
            }
        }
    }
}
