using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 주사위를 얻기 위한 퀘스트 목록(달성도 및 진행상태)을 표시하는 레이어
    /// </summary>
    public class DiceQuestSubLayer : EventQuestSubLayer
    {
        public override void Init()
        {
            base.Init();
            DiceUIEvent.RefreshTabReddot();
        }
        /// <summary>
        /// 일일 퀘스트 쪽 혹시 몰라서 end_time 이후 값일때는 일일퀘스트 데이터를 안받게 처리
        /// </summary>
        /// <returns></returns>
        protected override bool IsEventPeriod()
        {
            var holidayData = DiceEventPopup.GetHolidayData();
            if (holidayData == null)
                return false;

            var isPeriod = holidayData.IsEventPeriod(false);//end_time 기준 체크
            return isPeriod;
        }
        
        public override bool backBtnCall() { return base.backBtnCall(); } //백 버튼 콜백이 없으면 false 를 출력

        public override int GetEventKey()
        {
            var holidayData = DiceEventPopup.GetHolidayData();
            if (holidayData == null)
                return -1;

            return holidayData.GetScheduleDataKey();
        }
    }
}

