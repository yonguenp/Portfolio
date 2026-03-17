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
    public class LunaQuestSubLayer : EventQuestSubLayer
    {
        [SerializeField] Text QuestDesc;
        
        public override void Init()
        {
            base.Init();
        }
        /// <summary>
        /// 일일 퀘스트 쪽 혹시 몰라서 end_time 이후 값일때는 일일퀘스트 데이터를 안받게 처리
        /// </summary>
        /// <returns></returns>
        protected override bool IsEventPeriod()
        {
            var HolidayTypeDataList = EventScheduleData.GetEventTypeData(eActionType.LUNASERVER_OPEN_EVENT, true);
            if (HolidayTypeDataList == null || HolidayTypeDataList.Count <= 0)
                return false;

            var holidayData = HolidayTypeDataList[0];
            if (holidayData == null)
                return false;

            var isPeriod = holidayData.IsEventPeriod(false);//end_time 기준 체크
            return isPeriod;
        }
        
        public override bool backBtnCall() { return base.backBtnCall(); } //백 버튼 콜백이 없으면 false 를 출력

        public override int GetEventKey()
        {
            return LunaServerEventPopup.EVENT_KEY;
        }

        protected override void SetDailyMissionData()//퀘스트 데이터 세팅(진행중 퀘스트 목록에서 보상 받으면 진행 데이터에서 빠지기 때문에,완료 목록 더해야함)
        {
            base.SetDailyMissionData();

            var popup = PopupManager.GetPopup<LunaServerEventPopup>();
            if (popup != null)
            {
                var curTab = popup.GetCurTab();
                int[] curQeustArray = null;
                switch(curTab)
                {
                    case 1: 
                        curQeustArray = popup.DragonTrainingQuest;
                        QuestDesc.text = StringData.GetStringByStrKey("루나서버이벤트설명_1");
                        break;
                    case 2: 
                        curQeustArray = popup.GemReinforceQuest;
                        QuestDesc.text = StringData.GetStringByStrKey("루나서버이벤트설명_2");
                        break;
                    
                    case 0: default: 
                        curQeustArray = popup.BurningServerQuest;
                        QuestDesc.text = StringData.GetStringByStrKey("루나서버이벤트설명_0"); 
                        break;
                }

                for (int i = 0; i < eventQuestList.Count;)
                {
                    if(!curQeustArray.Contains(eventQuestList[i].ID))
                    {
                        eventQuestList.RemoveAt(i);
                    }
                    else
                        i++;
                }
            }
        }

    }
}

