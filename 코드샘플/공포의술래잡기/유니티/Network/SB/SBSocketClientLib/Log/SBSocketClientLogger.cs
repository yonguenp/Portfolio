using System;

#if UNITY_EDITOR_WIN && false
using NLog;
using NLog.Config;
using NLog.Targets;
#endif//UNITY_EDITOR_WIN

using SBCommonLib;

namespace SBSocketClientLib
{
    /// <summary>
    /// Socket Client 용 Log 클래스
    /// </summary>
    public class SBSocketClientLogger : SBLogger
    {
        /// <summary>
        /// NLog.Logger
        /// </summary>
#if UNITY_EDITOR_WIN && false
        private Logger _logger = null;
#endif//UNITY_EDITOR_WIN

        /// <summary>
        /// Trace Log
        /// </summary>
        /// <param name="message_">Log Message</param>
        /// <param name="calledBy_">호출한 위치</param>
        public override void Trace(string message_, Exception e_, string calledBy_)
        {
#if UNITY_EDITOR_WIN && false        
            //throw new System.NotImplementedException(); // default generated code.
            if (null == _logger)
                return;

            if (_logger.IsTraceEnabled)
            {
                //if (calledBy_ != string.Empty)
                //    _logger = LogManager.GetLogger(calledBy_);
                //_logger = LogManager.GetLogger((calledBy_ != string.Empty) ? calledBy_ : "SBLog");
                _logger = LogManager.GetLogger((string.IsNullOrWhiteSpace(calledBy_)) ? "SBLog" : calledBy_);
                _logger.Trace(e_, message_);
                InvokeAction("TRACE", message_);
            }            
#endif//UNITY_EDITOR_WIN
        }

        /// <summary>
        /// Debug Log
        /// </summary>
        /// <param name="message_">Log Message</param>
        /// <param name="calledBy_">호출한 위치</param>
        public override void Debug(string message_, Exception e_, string calledBy_)
        {
#if UNITY_EDITOR_WIN && false
            //throw new System.NotImplementedException(); // default generated code.
            if (null == _logger)
                return;

            if (_logger.IsDebugEnabled)
            {
                //if (calledBy_ != string.Empty)
                //    _logger = LogManager.GetLogger(calledBy_);
                //_logger = LogManager.GetLogger((calledBy_ != string.Empty) ? calledBy_ : "SBLog");
                _logger = LogManager.GetLogger((string.IsNullOrWhiteSpace(calledBy_)) ? "SBLog" : calledBy_);
                _logger.Debug(e_, message_);
                InvokeAction("DEBUG", message_);
            }
#endif//UNITY_EDITOR_WIN
        }

        /// <summary>
        /// Info Log
        /// </summary>
        /// <param name="message_">Log Message</param>
        /// <param name="calledBy_">호출한 위치</param>
        public override void Info(string message_, Exception e_, string calledBy_)
        {
#if UNITY_EDITOR_WIN && false
            //throw new System.NotImplementedException(); // default generated code.
            if (null == _logger)
                return;

            if (_logger.IsInfoEnabled)
            {
                //if (calledBy_ != string.Empty)
                //    _logger = LogManager.GetLogger(calledBy_);
                //_logger = LogManager.GetLogger((calledBy_ != string.Empty) ? calledBy_ : "SBLog");
                _logger = LogManager.GetLogger((string.IsNullOrWhiteSpace(calledBy_)) ? "SBLog" : calledBy_);
                _logger.Info(e_, message_);
                InvokeAction("INFO", message_);
            }
#endif//UNITY_EDITOR_WIN
        }

        /// <summary>
        /// Warning Log
        /// </summary>
        /// <param name="message_">Log Message</param>
        /// <param name="calledBy_">호출한 위치</param>
        public override void Warn(string message_, Exception e_, string calledBy_)
        {
#if UNITY_EDITOR_WIN && false
            //throw new System.NotImplementedException(); // default generated code.
            if (null == _logger)
                return;

            if (_logger.IsWarnEnabled)
            {
                //if (calledBy_ != string.Empty)
                //    _logger = LogManager.GetLogger(calledBy_);
                //_logger = LogManager.GetLogger((calledBy_ != string.Empty) ? calledBy_ : "SBLog");
                _logger = LogManager.GetLogger((string.IsNullOrWhiteSpace(calledBy_)) ? "SBLog" : calledBy_);
                _logger.Warn(e_, message_);
                InvokeAction("WARN", message_);
            }
#endif//UNITY_EDITOR_WIN
        }

        /// <summary>
        /// Error Log
        /// </summary>
        /// <param name="message_">Log Message</param>
        /// <param name="calledBy_">호출한 위치</param>
        public override void Error(string message_, Exception e_, string calledBy_)
        {
#if UNITY_EDITOR_WIN && false
            //throw new System.NotImplementedException(); // default generated code.
            if (null == _logger)
                return;

            if (_logger.IsErrorEnabled)
            {
                //if (calledBy_ != string.Empty)
                //    _logger = LogManager.GetLogger(calledBy_);
                //_logger = LogManager.GetLogger((calledBy_ != string.Empty) ? calledBy_ : "SBLog");
                _logger = LogManager.GetLogger((string.IsNullOrWhiteSpace(calledBy_)) ? "SBLog" : calledBy_);
                _logger.Error(e_, message_);
                InvokeAction("ERROR", message_);
            }
#endif//UNITY_EDITOR_WIN
        }

        /// <summary>
        /// Fatal Log
        /// </summary>
        /// <param name="message_">Log Message</param>
        /// <param name="calledBy_">호출한 위치</param>
        public override void Fatal(string message_, Exception e_, string calledBy_)
        {
#if UNITY_EDITOR_WIN && false
            //throw new System.NotImplementedException(); // default generated code.
            if (null == _logger)
                return;

            if (_logger.IsFatalEnabled)
            {
                //if (calledBy_ != string.Empty)
                //    _logger = LogManager.GetLogger(calledBy_);
                //_logger = LogManager.GetLogger((calledBy_ != string.Empty) ? calledBy_ : "SBLog");
                _logger = LogManager.GetLogger((string.IsNullOrWhiteSpace(calledBy_)) ? "SBLog" : calledBy_);
                _logger.Fatal(e_, message_);
                InvokeAction("FATAL", message_);
            }
#endif//UNITY_EDITOR_WIN
        }

        /// <summary>
        /// 로그 설정 셋팅
        /// </summary>
        /// <param name="logLevel_">로그 레벨</param>
        public override void SetConfiguration(SBLogLevel logLevel_)
        {
#if UNITY_EDITOR_WIN && false
            //throw new System.NotImplementedException(); // default generated code.

            // Config 설정은 https://github.com/nlog/NLog/wiki/File-target 참조.
            LoggingConfiguration config = new LoggingConfiguration();

#region Console Log Setting
            //ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
            //consoleTarget.Layout = @"${date} [${level:uppercase=true}] (${threadid}) ${message} |<${exception}>|<${logger}>|<${all-event-properties}>|";
            ////consoleTarget.Layout = @"C ${date} [${level:uppercase=true}] (${threadid}) ${message} |<${exception}>|<${logger}>|<${all-event-properties}>|"; // for test
            //consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.Cyan, ConsoleOutputColor.NoChange));
            //consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.Blue, ConsoleOutputColor.NoChange));
            //consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.Green, ConsoleOutputColor.NoChange));
            //consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            //consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            //consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.Magenta, ConsoleOutputColor.NoChange));

            //config.AddTarget("console", consoleTarget);

#endregion

#region File Log Setting

            FileTarget fileTarget = new FileTarget();
            fileTarget.Encoding = System.Text.Encoding.UTF8;
            fileTarget.LineEnding = LineEndingMode.Default;
            fileTarget.FileName = @"${basedir}/Logs/unity_current.log";
            fileTarget.ArchiveFileName = @"${basedir}/Logs/{#}.log";
            fileTarget.Layout = @"${date} [${level:uppercase=true}] (${threadid}) ${message} |<${exception}>|<${logger}>|<${all-event-properties}>|";
            //fileTarget.Layout = @"F ${date} [${level:uppercase=true}] (${threadid}) ${message} |<${exception}>|<${logger}>|<${all-event-properties}>|"; // for test
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            fileTarget.ArchiveDateFormat = "yyyy_MM_dd_HH_mm_ss";
            fileTarget.ArchiveAboveSize = 100000000;
            fileTarget.ArchiveOldFileOnStartup = true;

            config.AddTarget("file", fileTarget);

#endregion

#region CustomAsyncTaskTarget Test
#if false
            CustomAsyncTaskTarget asyncTarget = new CustomAsyncTaskTarget();
            asyncTarget.Layout = @"A ${date} [${level:uppercase=true}] (${threadid}) ${message} |<${exception}>|<${logger}>|<${all-event-properties}>|";

            config.AddTarget("async", asyncTarget);
#endif
#endregion

#region Rule Setting

            LogLevel level;

            switch (logLevel_)
            {
                case SBLogLevel.Trace: level = LogLevel.Trace; break;
                case SBLogLevel.Debug: level = LogLevel.Debug; break;
                case SBLogLevel.Info: level = LogLevel.Info; break;
                case SBLogLevel.Warn: level = LogLevel.Warn; break;
                case SBLogLevel.Error: level = LogLevel.Error; break;
                case SBLogLevel.Fatal: level = LogLevel.Fatal; break;
                default: level = LogLevel.Off; break;
            }

            //LoggingRule consoleRule = new LoggingRule("*", level, consoleTarget);
            //config.LoggingRules.Add(consoleRule);
            LoggingRule fileRule = new LoggingRule("*", level, fileTarget);
            config.LoggingRules.Add(fileRule);
#if false
            LoggingRule asyncRule = new LoggingRule("*", level, asyncTarget);
            config.LoggingRules.Add(asyncRule);
#endif

#endregion

            LogManager.Configuration = config;

            _logger = LogManager.GetLogger("SBLog");
#endif//UNITY_EDITOR_WIN
        }
    }
}
