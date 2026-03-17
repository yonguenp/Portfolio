using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class TimeManager : IManagerBase
    {
        public static readonly int UTC_KOREA_HOUR = 9;
        private static TimeManager instance = null;
        public static TimeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TimeManager();
                }
                return instance;
            }
        }

        private int curTime = 0;
        private float tempTime = 0f;

        private int server_ts = 0;
        private DateTime ts_regist = DateTime.Now;
        public int client_ts
        {
            get {
                return server_ts + (int)(DateTime.Now - ts_regist).TotalSeconds;
            }
        }

        private List<TimeObject> timeObjects = null;


        public void Initialize()
        {
            if (timeObjects == null)
            {
                timeObjects = new List<TimeObject>();
            }
            else
            {
                timeObjects.Clear();
            }
        }

        public void Update(float dt)
        {
            tempTime += dt;
            if (tempTime >= 1)
            {
                tempTime -= 1;
                curTime += 1;
                RefreshObject();
            }
        }

        public void RefreshObject()
        {
            var it = timeObjects.ToList().GetEnumerator();
            while (it.MoveNext())
            {
                if (it.Current == null)
                {
                    continue;
                }

                it.Current.Refresh?.Invoke();
            }
        }

        public static void AddObject(TimeObject target)
        {
            if (target == null)
            {
                return;
            }
            if (Instance.timeObjects == null)
            {
                Instance.timeObjects = new List<TimeObject>();
            }

            if(!Instance.timeObjects.Contains(target))
                Instance.timeObjects.Add(target);
        }

        public static void DelObject(TimeObject target)
        {
            if (target == null)
            {
                return;
            }

            Instance.timeObjects.Remove(target);
        }

        public static void TimeRefresh(int time)
        {
            Instance.curTime = time;
            Instance.tempTime = 0f;

            Instance.server_ts = time;
            Instance.ts_regist = DateTime.Now;
        }

        public static int GetTime()
        {
            return Instance.curTime;
        }

        public static int GetTimeCompare(DateTime target)
        {
            return (int)(target - GetDateTime()).TotalSeconds;
        }

        public static int GetTimeCompare(int target)
        {
            return target - Instance.curTime;
        }

        public static long GetTimeCompareInt64(long target)
        {
            return target - Instance.curTime;
        }

        public static int GetTimeCompareFromNow(int target)
        {
            return Instance.curTime - target;
        }

        public static string GetTimeCompareString(int target)
        {
            return SBFunc.TimeString(GetTimeCompare(target));
        }
        /// <summary>
        /// 서버타임의 DateTime 생성 많이 사용할 것 같지 않아 갱신 주기 때 최신화 사용하지 않음
        /// UTC+9 기준인 이유는 테이블 데이터에 사용된 DateTime이
        /// UTC+9 기준으로 작성되어 결정됨.
        /// </summary>
        /// <returns>UTC+9 기준 ServerTimeStemp DateTime</returns>
        private static DateTime GetDateTime()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(GetTime()).AddHours(UTC_KOREA_HOUR);
        }
        /// <summary>
        /// UTC+9 기준인 이유는 테이블 데이터에 사용된 DateTime이
        /// UTC+9 기준으로 작성되어 결정됨.
        /// </summary>
        /// <param name="timeStamp">TimeStamp</param>
        /// <returns>UTC+9 기준 TimeStemp DateTime</returns>
        public static DateTime GetCustomDateTime(long timeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp).AddHours(UTC_KOREA_HOUR);
        }
        /// <summary>
        /// 서버타임 원하는 시간만큼 더하고 가져오기
        /// </summary>
        /// <param name="addedDay">더하기 할 날짜</param>
        /// <param name="addedHour">더하기 할 시간</param>
        /// <param name="addedMin">더하기 할 분</param>
        /// <param name="addedSec">더하기 할 초</param>
        /// <returns></returns>
        public static DateTime GetDateTime(int addedDay = 0, int addedHour = 0, int addedMin = 0, int addedSec = 0)
        {
            var time = GetDateTime();

            if (addedSec != 0)
                time = time.AddSeconds(addedSec);
            if (addedMin != 0)
                time = time.AddMinutes(addedMin);
            if (addedHour != 0)
                time = time.AddHours(addedHour);
            if (addedDay != 0)
                time = time.AddDays(addedDay);

            return time;
        }
        /// <summary>
        /// 오늘의 초기화 시간 반환.
        /// 초기화 시간 설정용도.
        /// </summary>
        public static DateTime GetDailyStartTime()
        {
            var resetTime = GameConfigTable.GetDailyContentResetTime();
            var curTime = GetDateTime();
            var ret = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0, 0, DateTimeKind.Utc).AddHours(resetTime);
            if (curTime.Hour < resetTime)
                return ret.AddDays(-1);
            else
                return ret;
        }

        /// <summary>
        /// 내일의 초기화 시간 반환
        /// 남은 시간 계산용도.
        /// </summary>
        //static DateTime GetTomorrowStartTime()
        //{
        //    var resetTime = GameConfigTable.GetConfigIntValue("DAILY_CONTENT_RESET_TIME");
        //    var curTime = GetDateTime();
        //    var ret = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0, 0, DateTimeKind.Utc).AddHours(resetTime);
        //    if (curTime.Hour < resetTime)
        //        return ret;
        //    else
        //        return ret.AddDays(1);
        //}
        /// <summary>
        /// 리셋 타임까지 남은 시간 반환
        /// </summary>
        /// <returns>남은 시간</returns>
        //public static int GetContentResetTime()
        //{
        //    var resetTime = GetTomorrowStartTime();
        //    var time = resetTime - GetDateTime();
        //    if (int.TryParse(time.TotalSeconds.ToString(), out int result))
        //        return result;
        //    return 0;
        //}
        /// <summary>
        /// UTC+9 기준 DateTime을 TimeStamp로 만드는 함수
        /// UTC+9 기준인 이유는 테이블 데이터에 사용된 DateTime이
        /// UTC+9 기준으로 작성되어 결정됨.
        /// </summary>
        /// <param name="time">UTC+9 기준 DateTime</param>
        /// <returns>TimeStemp</returns>
        public static int GetTimeStamp(DateTime time)
        {
            return (int)(time - GetCustomDateTime(0)).TotalSeconds;
        }
        public static int GetContentClearTime(bool _isToday)//false 면 다음날
        {
            var goalDateTime = GetDailyStartTime();
            if (!_isToday)
                goalDateTime = goalDateTime.AddDays(1);

            var goalTime = (int)(goalDateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            return goalTime;
        }
        public static int GetServerCurrentTimeSpan()
        {
            return (int)(GetDateTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
        public static int GetContentResetTime()
        {
            var currentTime = GetServerCurrentTimeSpan();//현재 시간
            var currentGoalTime = GetContentClearTime(true);
            var currentNextTime = GetContentClearTime(false);//다음날 0시

            if (currentTime <= currentGoalTime)//다음날 0시
                return currentGoalTime - currentTime;
            else if (currentGoalTime < currentTime && currentTime < currentNextTime)//오늘새벽 4시 이후 내일 새벽4시 이전 사이시간
                return currentNextTime - currentTime;
            else//
            {
                Debug.Log("wrong Mission Date");
                return 0;
            }
        }

        #region 이벤트 시간 제어 (이벤트 일일 미션 등 - 기존 04시가 아닐 수 있음)
        public static DateTime GetDailyEventStartTime()
        {
            var resetTime = GameConfigTable.GetDailyEventContentResetTime();
            var curTime = GetDateTime();
            var ret = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0, 0, DateTimeKind.Utc).AddHours(resetTime);
            if (curTime.Hour < resetTime)
                return ret.AddDays(-1);
            else
                return ret;
        }
        public static int GetEventContentClearTime(bool _isToday)//false 면 다음날
        {
            var goalDateTime = GetDailyEventStartTime();
            if (!_isToday)
                goalDateTime = goalDateTime.AddDays(1);

            var goalTime = (int)(goalDateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            return goalTime;
        }
        public static int GetEventContentResetTime()
        {
            var currentTime = GetServerCurrentTimeSpan();//현재 시간
            var currentGoalTime = GetEventContentClearTime(true);
            var currentNextTime = GetEventContentClearTime(false);

            if (currentTime <= currentGoalTime)
                return currentGoalTime - currentTime;
            else if (currentGoalTime < currentTime && currentTime < currentNextTime)
                return currentNextTime - currentTime;
            else//
            {
                Debug.Log("wrong Mission Date");
                return 0;
            }
        }
        /// <summary>
        /// 다음주 월요일 구하기
        /// </summary>
        /// <param name="_specificDay"></param>
        /// <returns></returns>
        public static DateTime GetSpecificNextDay(DayOfWeek _specificDay = DayOfWeek.Monday)
        {
            //DateTime today = DateTime.Today;
            DateTime today = GetDateTime();
            DateTime todayInitialDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);

            int daysUntilMonday = ((int)_specificDay - (int)todayInitialDate.DayOfWeek + 7) % 7;
            DateTime nextMonday = todayInitialDate.AddDays(daysUntilMonday);//담주 월
            if (nextMonday == todayInitialDate)
                nextMonday = nextMonday.AddDays(7);

            return nextMonday;
        }


        #endregion
    }
}
