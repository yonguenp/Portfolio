using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace SBCommonLib
{
    public class SBUtil
    {
        /// <summary>
        /// 기본 인코딩
        /// </summary>
        public static readonly Encoding kDefaultEncoding = Encoding.Default;

        /// <summary>
        /// 유니코드 인코딩
        /// </summary>
        //public static readonly Encoding kUnicodeEncoding = Encoding.Unicode;

        /// <summary>
        /// UTF8 인코딩
        /// </summary>
        public static readonly Encoding kUtf8Encoding = Encoding.UTF8;
        public static readonly Encoding kUnicodeEncoding = Encoding.Unicode; // UTF-16

        public static readonly DateTime kOriginTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public static DateTime KoreanTime { get { return DateTime.UtcNow.AddHours(9); } }
        /// <summary>
        /// 호스트 PC 아이피 주소 가져오기
        /// </summary>
        public static string GetHostIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var address in host.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    string[] addressSplit = address.ToString().Split('.');
                    //if ( addressSplit[ 0 ].Equals( "192" ) || addressSplit[ 0 ].Equals( "10" ) )
                    if (addressSplit[0].Equals("172") || addressSplit[0].Equals("10"))
                    {
                        continue;
                    }

                    return address.ToString();
                }
            }

            return string.Empty;
        }

        public static string CallStackLog(SBLogLevel logLevel_ = SBLogLevel.Debug)
        {
            var level = "";
            switch (logLevel_)
            {
                case SBLogLevel.Trace:  level = "TRACE";    break;
                case SBLogLevel.Debug:  level = "DEBUG";    break;
                case SBLogLevel.Info:   level = "INFO";     break;
                case SBLogLevel.Warn:   level = "WARN";     break;
                case SBLogLevel.Error:  level = "ERROR";    break;
                case SBLogLevel.Fatal:  level = "FATAL";    break;
                default:                level = "None";     break;
            }

            var stackTrace = new StackTrace(true);
            var callStack = "";
            foreach (var frame in stackTrace.GetFrames())
            {
                callStack += $"\n[{level}] FileName: {frame.GetFileName()}, LineNo: {frame.GetFileLineNumber()}, Method: {frame.GetMethod()}";
            }

            return callStack;
        }

        /// <summary>
        /// 현재 DateTime 값을 Timestamp 값으로 변환
        /// </summary>
        /// <param name="isUtc_">UTC 기준 사용 여부(default: true)</param>
        /// <returns>TimeSpan Timestamp</returns>
        public static TimeSpan GetCurrentTimestamp(bool isUtc_ = true)
        {
#if false
            var now = DateTime.UtcNow;
            if (false == isUtc_)
                now = DateTime.Now;
            var origin = new DateTime(1970, 1, 1, 0, 0, 0);
            var timeSpan = now - origin;
            SBLog.PrintTrace($"[GetCurrentTimestamp] Now: {now}, origin: {origin}, TimeSpan: {timeSpan}, Timestamp: {timeSpan.TotalSeconds}", calledBy_: typeof(SBUtil).Name);
#else
            //var timeSpan = (((isUtc_) ? DateTime.UtcNow : DateTime.Now) - new DateTime(1970, 1, 1, 0, 0, 0));
#endif
            //return timeSpan;
            return (((isUtc_) ? DateTime.UtcNow : DateTime.Now) - new DateTime(1970, 1, 1, 0, 0, 0));
        }

        /// <summary>
        /// 현재 DateTime 값을 초 단위 Timestamp 값으로 변환
        /// </summary>
        /// <param name="isUtc_">UTC 기준 사용 여부(default: true)</param>
        /// <returns>Seconds Timestamp</returns>
        public static long GetCurrentSecTimestamp(bool isUtc_ = true)
        {
            return (long)(GetCurrentTimestamp(isUtc_).TotalSeconds);
        }

        /// <summary>
        /// 현재 DateTime 값을 밀리초 단위 Timestamp 값으로 변환
        /// </summary>
        /// <param name="isUtc_">UTC 기준 사용 여부(default: true)</param>
        /// <returns>MilliSeconds Timestamp</returns>
        public static long GetCurrentMilliSecTimestamp(bool isUtc_ = true)
        {
            return (long)(GetCurrentTimestamp(isUtc_).TotalMilliseconds);
        }

        /// <summary>
        /// 특정 DateTime 값을 Timestamp 값으로 변환
        /// </summary>
        /// <param name="dateTime_">DateTime Value</param>
        /// <returns>TimeSpan Timestamp value</returns>
        public static TimeSpan ConvertToUnixTimestamp(DateTime dateTime_)
        {
#if false
            var origin = new DateTime(1970, 1, 1, 0, 0, 0);
            var timeSpan = dateTime_ - origin;
            SBLog.PrintTrace($"[GetCurrentTimestamp] DateTime: {dateTime_}, origin: {origin}, TimeSpan: {timeSpan}, Timestamp: {timeSpan.TotalSeconds}", calledBy_: typeof(SBUtil).Name);
#else
            //var timeSpan = (dateTime_ - new DateTime(1970, 1, 1, 0, 0, 0));
#endif
            //return timeSpan;
            return (dateTime_ - new DateTime(1970, 1, 1, 0, 0, 0));
        }

        /// <summary>
        /// 특정 DateTime 값을 초 단위 Timestamp 값으로 변환
        /// </summary>
        /// <param name="dateTime_">DateTime Value</param>
        /// <returns>Seconds Timestamp value</returns>
        public static long ConvertToUnixSecTimestamp(DateTime dateTime_)
        {
            return (long)(ConvertToUnixTimestamp(dateTime_).TotalSeconds);
        }

        /// <summary>
        /// 특정 DateTime 값을 밀리초 단위 Timestamp 값으로 변환
        /// </summary>
        /// <param name="dateTime_">DateTime Value</param>
        /// <returns>MilliSeconds Timestamp value</returns>
        public static long ConvertToUnixMilliSecTimestamp(DateTime dateTime_)
        {
            return (long)(ConvertToUnixTimestamp(dateTime_).TotalMilliseconds);
        }

#if false
        public static long ConvertToUnixTimestamp2()
        {
#if DEBUG
            var utcNow = DateTime.UtcNow.Ticks;
            var origin = new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
            var origin2 = DateTime.Parse("01/01/1970 00:00:00").Ticks;
            var ticks = utcNow - origin;
            var timestamp = ticks / 10000000;
            SBLog.PrintTrace($"[GetCurrentTimestamp2] Utc Now: {utcNow}, origin: {origin}, origin2: {origin2}, TimeSpan: {ticks}, Timestamp: {timestamp}", calledBy_: typeof(SBUtil).Name);
#else
            //Find unix timestamp (seconds since 01/01/1970)
            long timestamp = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
            timestamp /= 10000000; //Convert windows ticks to seconds
            //timestamp = timestamp.ToString();
#endif

            return timestamp;
        }

        public static double ConvertToUnixTimestamp3(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }
#endif

        /// <summary>
        /// 특정 초 단위 Timestamp 값을 DateTime 값으로 변환
        /// </summary>
        /// <param name="secTimestamp_">Timestamp value</param>
        /// <returns>DateTime value</returns>
        public static DateTime ConvertFromUnixSecTimestamp(long secTimestamp_)
        {
#if false
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            origin.AddSeconds(timestamp_);
            SBLog.PrintTrace($"[GetCurrentTimestamp] Timestamp: {timestamp_}, origin: {origin}", calledBy_: typeof(SBUtil).Name);
            return origin;
#else
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(secTimestamp_);
#endif
        }

        /// <summary>
        /// 특정 밀리초 단위 Timestamp 값을 DateTime 값으로 변환
        /// </summary>
        /// <param name="milliSecTimestamp_">Timestamp value</param>
        /// <returns></returns>
        public static DateTime ConvertFromUnixMilliSecTimestamp(long milliSecTimestamp_)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddMilliseconds(milliSecTimestamp_);
        }

        /// <summary>
        /// 요청 만료 시간 초과 확인(초 단위)
        /// </summary>
        /// <param name="startTime_">DateTime</param>
        /// <param name="limitTimeSec_">만료 기준 초</param>
        /// <returns>true: 시간 초과.</returns>
        public static bool CheckRequestTimeoutSec(DateTime startTime_, int limitTimeSec_)
        {
            var start = ConvertToUnixSecTimestamp(startTime_);
            return CheckRequestTimeoutSec(start, limitTimeSec_);
        }

        /// <summary>
        /// 요청 만료 시간 초과 확인(초 단위)
        /// </summary>
        /// <param name="startTime_">Timestamp</param>
        /// <param name="limitTimeSec_">만료 기준 초</param>
        /// <returns></returns>
        public static bool CheckRequestTimeoutSec(long startTime_, int limitTimeSec_)
        {
            return ((startTime_ + limitTimeSec_) < GetCurrentSecTimestamp());
        }

        /// <summary>
        /// 요청 만료 시간 초과 확인(밀리초 단위)
        /// </summary>
        /// <param name="startTime_">DateTime</param>
        /// <param name="limitTimeMilliSec_">만료 기준 밀리초</param>
        /// <returns>true: 시간 초과.</returns>
        public static bool CheckRequestTimeoutMilliSec(DateTime startTime_, int limitTimeMilliSec_)
        {
            var start = ConvertToUnixMilliSecTimestamp(startTime_);
            return CheckRequestTimeoutMilliSec(start, limitTimeMilliSec_);
        }

        /// <summary>
        /// 요청 만료 시간 초과(밀리초 단위)
        /// </summary>
        /// <param name="startTime_">Timestamp</param>
        /// <param name="limitTimeMilliSec_">만료 기준 밀리초</param>
        /// <returns></returns>
        public static bool CheckRequestTimeoutMilliSec(long startTime_, int limitTimeMilliSec_)
        {
            return ((startTime_ + limitTimeMilliSec_) < GetCurrentMilliSecTimestamp());
        }

        #region 스레드 확인해보려고 만들어 본 것.
#if false
        private static bool _isExit = false;
        public static bool IsExit
        {
            get => _isExit;
            set => _isExit = value;
        }
        private static ProcessThreadCollection _processThreadCollection;
        public static ProcessThreadCollection ProcessThreadCollection
        {
            get => _processThreadCollection;
            set => _processThreadCollection = value;
        }
        public static void ThreadInfo()
        {
            while (false == _isExit)
            {
                _processThreadCollection = Process.GetCurrentProcess().Threads;
                SBLog.PrintDebug($"****** 현재 프로세스에서 실행중인 스레드 수 : {_processThreadCollection.Count}, Time: {DateTime.Now.ToString()} ******");

                var index = 0;

                foreach (ProcessThread processThread in _processThreadCollection)
                {
                    SBLog.PrintDebug($"****** {++index} 번째 스레드 정보 ****** ThreadId : {processThread.Id}, 상태 : {processThread.ThreadState}, 시작시간 : {processThread.StartTime}, 우선순위 : {processThread.BasePriority}");
                }

                Thread.Sleep(10000);
            }
        }
#endif
        #endregion
    }
}
