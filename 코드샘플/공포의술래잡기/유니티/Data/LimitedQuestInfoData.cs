using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitedQuestInfoData : GameData
{
    public int quest_group_uid { get; private set; }
    public string start_day { get; private set; }
    public string end_day { get; private set; }
    public string resource_path { get; private set; }

    public override void SetValue(Dictionary<string, string> tmp)
    {
        base.SetValue(tmp);

        quest_group_uid = Int(data["quest_group_uid"]);
        start_day = data["start_day"];
        end_day = data["end_day"];
        resource_path = data["resource_path"];
    }
}
