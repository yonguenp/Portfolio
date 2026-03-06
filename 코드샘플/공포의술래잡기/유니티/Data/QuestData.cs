using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestData : GameData
{
    public static Dictionary<int, QuestData> questDictionary = new Dictionary<int, QuestData>();
    public static Dictionary<int, List<int>> prevQuests = new Dictionary<int, List<int>>();
    private static List<QuestData> quests = null;
    public static List<QuestData> GetQuests()
    {
        if (quests == null)
        {
            List<QuestData> ret = new List<QuestData>();

            foreach (var q in questDictionary)
            {
                if (q.Value.use)
                {
                    ret.Add(q.Value);
                }
            }

            quests = ret;
        }

        return quests;
    }

    public static QuestData GetQuestData(int id)
    {
        return questDictionary[id];
    }

    public static List<int> GetPrevQuests(int id)
    {
        if (GetQuestData(id) == null)
            return new List<int>();

        if (prevQuests.ContainsKey(id))
            return prevQuests[id];

        return new List<int>();
    }
    //public enum QuestType
    //{
    //    NONE,
    //    DAILY,
    //    WEEK,
    //    ACHIEVEMNET,
    //    CHARACTER,
    //    CHARACTER_COLLECTION,
    //    EVENT,
    //}

    public int group_uid { get; private set; }
    public int next { get; private set; }
    public int quest_type { get; private set; }
    public int quest_num { get; private set; }
    public int quest_clear_type { get; private set; }
    public int param { get; private set; }
    public int clear_count { get; private set; }
    public int reward_index { get; private set; }
    public bool use { get; private set; }
    public List<ShopPackageGameData> rewards { get; private set; }

    public Sprite resource { 
        get {
            if (_resource == null)
                ReloadResource();

            return _resource; 
        } 
    }
    private Sprite _resource = null;

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        group_uid = Int(data["group_uid"]);
        next = Int(data["next"]);
        quest_type = Int(data["quest_type"]);
        quest_num = Int(data["quest_num"]);
        quest_clear_type = Int(data["quest_clear_type"]);
        param = Int(data["param"]);
        clear_count = Int(data["clear_count"]);
        
        reward_index = Int(data["reward_index"]);

        rewards = ShopPackageGameData.GetRewardDataList(reward_index);
        use = Int(data["use"]) > 0;

        questDictionary[GetID()] = this;
        if (next > 0)
        {
            if (!prevQuests.ContainsKey(next))
            {
                prevQuests[next] = new List<int>();
            }
            prevQuests[next].Add(GetID());
        }

        ReloadResource();
    }

    void ReloadResource()
    {
        _resource = Managers.Resource.LoadAssetsBundle<Sprite>(data["resource_path_assetsbundle"]);
        if (_resource == null)
        {
            SBDebug.Log("구버전의 패스 사용");
            _resource = Managers.Resource.LoadAssetsBundle<Sprite>(data["resource_path"]);
        }
    }
}
