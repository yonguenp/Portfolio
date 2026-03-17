using Firebase;
using Newtonsoft.Json.Utilities;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager>
{
    int frameRate = 60;

    protected override void Awake()
    {
        base.Awake();

        // 앱 기본 설정을 세팅 
        Application.targetFrameRate = frameRate;
        QualitySettings.vSyncCount = 0;
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if !SB_TEST && !UNITY_EDITOR
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
#else
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
#endif

        AotHelper.EnsureList<Pos>();
    }
}
