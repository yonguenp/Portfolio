using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionCharGroupData : GameData
{
    public int collect_group_uid { get; private set; }  
    public int char_uid { get; private set; }
    public int level_value { get; private set; }
    public int skill_value { get; private set; }

    public override void SetValue(Dictionary<string, string> tmp)
    {
        base.SetValue(tmp);

        collect_group_uid = Int(data["collect_group_uid"]);
        char_uid = Int(data["char_uid"]);
        level_value = Int(data["level_value"]);
        skill_value = Int(data["skill_value"]);
    }

    public static List<CollectionCharGroupData> GetGroupData(int group_uid)
    {
        List<CollectionCharGroupData> ret = new List<CollectionCharGroupData>();
        foreach (CollectionCharGroupData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.collection_char_group))
        {
            if(data.collect_group_uid == group_uid)
                ret.Add(data);
        }

        return ret;
    }
}
