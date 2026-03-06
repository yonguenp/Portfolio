using SBSocketSharedLib;

public class GameRoomInfo
{
    // 여기서의 Time은 전부 ms 단위이다.
    public RoomInfo RoomInfo { get; set; }
    public int PlayerCount { get; set; }
    public int PlayTime { get; set; }
    public int EscapeTime { get; set; }
    public int MapId { get; set; }
    public int TargetEscapeCount { get; set; }
    public int BatteryGeneratorCoolTime { get; set; }
    public int BatteryGeneratorActiveTime { get; set; }
}
