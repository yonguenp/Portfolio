using System.Collections.Generic;

public class RewardGameData : GameData
{
    public int Point { get; private set; }
    public bool CenterAlarm { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        Point = Int(data["point"]);
        CenterAlarm = Int(data["center_alarm"]) == 1;
    }
}
