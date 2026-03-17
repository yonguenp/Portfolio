using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EventAttendanceData : EventBaseData
    {
        const string EVENT_STRING_PREFIX = "attendance_event_";
        const string TITLE_PREFIX = "title:";
        const string TIME_PREFIX = "time:";
        const string BTN_PREFIX = "btn:";

        public int AttendanceTag { get; set; } = 0;
        public int AttendanceDay { get; set; } = 0;
        public bool IsAttendance { get; set; } = false;
        public DateTime LastDate { get; set; } = default;
        public int[] AttendanceList { get; set; } = null;

        public EventAttendanceData(EventScheduleData data) : base(data) { }
        public override void Clear()
        {
            AttendanceTag = 0;
            AttendanceDay = 0;
            IsAttendance = false;
        }
        public int GetEventKey()
        {
            return int.Parse(scheduleData.KEY);
        }

        public string GetEventEndDateTimeString()//시간표시 안하고 단순 노티로 변경
        {
            return scheduleData.GetEventString(SBFunc.StrBuilder(EVENT_STRING_PREFIX, TIME_PREFIX));
        }

        public string GetEventTitleString()
        {
            return scheduleData.GetEventString(SBFunc.StrBuilder(EVENT_STRING_PREFIX, TITLE_PREFIX));
        }
        public override string GetEventButtonString()
        {
            return scheduleData.GetEventString(SBFunc.StrBuilder(EVENT_STRING_PREFIX, BTN_PREFIX));
        }

        public override void SetData(JObject data)
        {
            if (SBFunc.IsJArray(data["event"]))
            {
                JArray jsonData = (JArray)data["event"];

                AttendanceTag = jsonData[0].Value<int>();
                AttendanceDay = jsonData[1].Value<int>();//seq 횟수
                LastDate = DateTime.MinValue;

                long date = jsonData[2].Value<long>();
                if (date > 0)
                    LastDate = TimeManager.GetCustomDateTime(date);

                IsAttendance = jsonData[3].Value<bool>();
            }
        }

        /// <summary>
        /// 이벤트 출석체크 기간인지? // inUse param 추가
        /// </summary>
        /// <returns></returns>
        public bool IsEventAttendancePeriod(bool _isUITime = true)
        {
            var isPeriod = scheduleData.IsEventPeriod(_isUITime);
            if (isPeriod)
                return true;
            else
                return false;
        }
        
        /// <summary>
        /// 이벤트 출첵 종료 남은 시간 true 면 UI용EndTime, false 면 데이터용
        /// </summary>
        /// <param name="_isUITime"></param>
        /// <returns></returns>
        public int GetRemainTime(bool _isUITime = true)
        {
            if (!IsEventAttendancePeriod(_isUITime))
                return 0;

            var endTime = _isUITime ? scheduleData.UI_END_TIME : scheduleData.END_TIME;
            return (int)(endTime - TimeManager.GetDateTime()).TotalSeconds;
        }

        public DateTime GetEventStartDateTime()
        {
            return scheduleData.START_TIME;
        }

        public Sprite GetEventIcon()
        {
            return scheduleData.SPRITE;
        }
        public void CheckAttendance()
        {
            IsAttendance = false;
        }

        public override bool IsNeedUpdate()
        {
            var rewards = EventRewardData.GetGroup(GetEventKey());
            if (rewards != null)
            {
                if (rewards.Count <= AttendanceDay)
                    return false;
            }

            if (LastDate < TimeManager.GetDailyEventStartTime())
                return true;

            return false;
        }
    }
}

