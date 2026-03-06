using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

/// <summary>
/// SandBox Common Library
/// </summary>

namespace SBCommonLib
{
    /// <summary>
    /// 로그 레벨
    /// </summary>
    public enum SBLogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
        Off = 6,
    }

    /// <summary>
    /// 로그 처리용 추상화 클래스
    /// </summary>
    public abstract class SBLogger /*: IJobQueue*/
    {
        /// <summary>
        /// 로그 메시지를 활용한 후속 처리(UI 표시 등)를 위한 Action
        /// </summary>
        private Action<string> _invoke = null;

#region Job Queue Test용 (미사용 중)
        //private static JobQueue _jobQueue = new JobQueue();

        //public void Push(Action job_)
        //{
        //    _jobQueue.Push(job_);
        //}
#endregion

        /// <summary>
        /// Trace 로그
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public abstract void Trace(string message_, Exception e_, string calledBy_ = "");

        /// <summary>
        /// Debug 로그
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public abstract void Debug(string message_, Exception e_, string calledBy_ = "");

        /// <summary>
        /// Info 로그
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public abstract void Info(string message_, Exception e_, string calledBy_ = "");

        /// <summary>
        /// Warn 로그
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public abstract void Warn(string message_, Exception e_, string calledBy_ = "");

        /// <summary>
        /// Error 로그
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public abstract void Error(string message_, Exception e_, string calledBy_ = "");

        /// <summary>
        /// Fatal 로그
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public abstract void Fatal(string message_, Exception e_, string calledBy_ = "");

        /// <summary>
        /// 로그 설정 셋팅
        /// </summary>
        /// <param name="logLevel_">로그 레벨</param>
        public abstract void SetConfiguration(SBLogLevel logLevel_);

        /// <summary>
        /// 로그 메시지를 활용한 후속 처리(UI 표시 등)를 위한 invoke 함수 설정
        /// </summary>
        /// <param name="invokeFunc_">Invoke 함수</param>
        public void SetInvoke(Action<string> invokeFunc_)
        {
            _invoke = invokeFunc_;
        }

        /// <summary>
        /// Invoke Action 처리
        /// </summary>
        /// <param name="logLevel_">로그 레벨</param>
        /// <param name="message_">로그 메시지</param>
        protected void InvokeAction(string logLevel_, string message_)
        {
            if (null != _invoke)
            {
                try
                {
                    _invoke($"[{logLevel_}] {DateTime.Now:yyyy-MM-dd HH\\:mm\\:ss} : {message_}");
                }
                catch (NullReferenceException)
                {
                }
            }
        }
    }

#if false
    public class SBProcessLogTask
    {
        public Action Action { get; set; } = null;
    }

    public class SBProcessLogThread : SBThread
    {
        private SBThreadTaskQueue<SBProcessLogTask> _taskQueue = new SBThreadTaskQueue<SBProcessLogTask>();
        private AutoResetEvent _event = new AutoResetEvent(false);

        public SBProcessLogThread()
            : base(0)
        {
        }

        public void RequestTask(Action action_)
        {
            if (null == action_)
            {
                return;
            }

            SBProcessLogTask task = new SBProcessLogTask();
            task.Action = action_;

            _taskQueue.Enqueue(task);

            _event.Set();
        }

        protected override void PreTask()
        {
        }

        protected override void ProcessTask()
        {
            SBProcessLogTask task = null;
            ConcurrentQueue<SBProcessLogTask> queue = _taskQueue.Dequeue();
            while (queue.TryDequeue(out task))
            {
                if (task == null)
                    continue;

                Action action = task.Action;

                if (action != null)
                {
                    action.Invoke();
                }
            }
        }

        protected override void PostTask()
        {
            _event.WaitOne();
        }
    }
#endif
    /// <summary>
    /// 로그 클래스
    /// </summary>
    public class SBLog
    {
        /// <summary>
        /// 로그 처리용 객체
        /// </summary>
        private static SBLogger _logger;

        /// <summary>
        /// 로그 처리 시작 함수
        /// </summary>
        /// <typeparam name="TLogger">로그 처리 클래스 타입</typeparam>
        /// <param name="logLevel_">로그 레벨</param>
        public static void StartLogProcess<TLogger>(SBLogLevel logLevel_ = SBLogLevel.Off) where TLogger : SBLogger, new()
        {
            _logger = new TLogger();
            _logger.SetConfiguration(logLevel_);
        }

        /// <summary>
        /// 로그 메시지를 활용한 후속 처리(UI 표시 등)를 위한 invoke 함수 설정
        /// </summary>
        /// <param name="invokeFunc_">Invoke 함수</param>
        public static void SetInvoke(Action<string> invokeFunc_)
        {
            if (null != _logger)
                _logger.SetInvoke(invokeFunc_);
        }

#if true
        /// <summary>
        /// Trace 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintTrace(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Trace(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_);
        }

        /// <summary>
        /// Debug 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintDebug(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Debug(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_);
        }

        /// <summary>
        /// Info 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintInfo(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Info(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_);
        }

        /// <summary>
        /// Warn 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintWarn(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Warn(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_);
        }

        /// <summary>
        /// Error 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintError(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Error(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_);
        }

        /// <summary>
        /// Fatal 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintFatal(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Fatal(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_);
        }
#else
        /// <summary>
        /// Trace 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintTrace(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Push(() => _logger.Trace(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_));
        }

        /// <summary>
        /// Debug 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintDebug(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Push(() => _logger.Debug(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_));
        }

        /// <summary>
        /// Info 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintInfo(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Push(() => _logger.Info(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_));
        }

        /// <summary>
        /// Warn 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintWarn(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Push(() => _logger.Warn(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_));
        }

        /// <summary>
        /// Error 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintError(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Push(() => _logger.Error(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_));
        }

        /// <summary>
        /// Fatal 로그 출력 함수
        /// </summary>
        /// <param name="message_">로그 메시지</param>
        /// <param name="calledBy_">호출한 위치</param>
        public static void PrintFatal(string message_, Exception e_ = null, string calledBy_ = "SBLog", [CallerMemberName] string memberName_ = "", [CallerLineNumber] int lineNumber_ = 0)
        {
            if (null == _logger)
                return;

            _logger.Push(() => _logger.Fatal(message_ + $" (MemberName: {memberName_}, LineNo: {lineNumber_})", e_, calledBy_));
        }
#endif
    }
}
