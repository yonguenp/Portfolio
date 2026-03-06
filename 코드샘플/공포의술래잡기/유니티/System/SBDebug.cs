using SBCommonLib.Json;
using System.Diagnostics;

public static class SBDebug
{
    [Conditional("VERBOSE")]
    public static void Log(string message)
    {
        UnityEngine.Debug.Log(message);
    }

    [Conditional("VERBOSE")]
    public static void Log(object obj)
    {
        UnityEngine.Debug.Log(SBJson.ToString(obj));
    }

    [Conditional("VERBOSE")]
    [Conditional("VERBOSE_WARNING")]
    public static void LogWarning(string message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    [Conditional("VERBOSE")]
    [Conditional("VERBOSE_WARNING")]
    public static void LogWarning(object obj)
    {
        UnityEngine.Debug.LogWarning(SBJson.ToString(obj));
    }

    public static void LogError(string message)
    {
        UnityEngine.Debug.LogError(message);
    }

    public static void LogError(object obj)
    {
        UnityEngine.Debug.LogError(SBJson.ToString(obj));
    }

    public static void LogException(System.Exception e)
    {
        UnityEngine.Debug.LogError(e.Message);
    }
}
