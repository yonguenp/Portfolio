using SBCommonLib;

public class TimeManager
{
    // 모든 시간은 milliseconds

    private long diffTime = 0;

    public void SetBaseTime(long serverTimestamp, long clientTimestamp)
    {
        if (clientTimestamp == 0L)
            clientTimestamp = SBUtil.GetCurrentMilliSecTimestamp();

        diffTime = serverTimestamp - clientTimestamp;

        SBDebug.LogWarning($"SetBaseTime : Server {serverTimestamp} / Client {clientTimestamp} / Time Diff {diffTime}");
    }

    public long GetClientTimestamp(long serverTimeStamp)
    {
        return serverTimeStamp + diffTime;
    }

    public long GetClientTimestamp()
    {
        return SBUtil.GetCurrentMilliSecTimestamp() + diffTime;
    }
}
