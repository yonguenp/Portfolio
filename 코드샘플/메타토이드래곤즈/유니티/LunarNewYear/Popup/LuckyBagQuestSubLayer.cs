using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 복주머니 일일퀘스트 sublayer - 탭 구조 변경으로 인해서 '안쓰지만' 혹시 몰라서 냅둠.
/// </summary>
namespace SandboxNetwork
{
    public class LuckyBagQuestSubLayer : EventQuestSubLayer
    {
        public override void Init()
        {
            base.Init();

            LuckyBagUIEvent.RefreshTabReddot();
        }
        /// <summary>
        /// 일일 퀘스트 쪽 규칙 : end_time 이후 값일때는 일일퀘스트 데이터를 안받게 처리
        /// </summary>
        /// <returns></returns>
        protected override bool IsEventPeriod()
        {
            var eventData = LuckyBagEventPopup.GetEventData();
            if (eventData == null)
                return false;

            var isPeriod = eventData.IsEventPeriod(false);//end_time 기준 체크
            return isPeriod;
        }

        public override bool backBtnCall() { return base.backBtnCall(); } //백 버튼 콜백이 없으면 false 를 출력

        public override int GetEventKey()
        {
            var eventData = LuckyBagEventPopup.GetEventData();
            if (eventData == null)
                return -1;

            return eventData.GetScheduleDataKey();
        }
    }
}

