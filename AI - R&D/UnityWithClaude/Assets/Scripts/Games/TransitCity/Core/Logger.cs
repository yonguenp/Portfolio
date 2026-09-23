using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 레벨 기반 무음 로거. 기본은 조용함(Verbosity=0) — 필요할 때만 값을 올려서 켠다.
    /// GoStop의 Logger.cs를 계승하되, 그쪽에 없던 verbosity 스위치를 추가했다.
    /// </summary>
    public static class Logger
    {
        public static int Verbosity = 0;

        public static void Log(string message, int level = 1)
        {
            if (level > Verbosity) return;
            Debug.Log($"[TransitCity] {message}");
        }

        public static void Warn(string message, int level = 1)
        {
            if (level > Verbosity) return;
            Debug.LogWarning($"[TransitCity] {message}");
        }
    }
}
