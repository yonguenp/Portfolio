using System.Collections.Generic;
using System.Linq;

public class RewardPointManager
{
    public enum FirstReward
    {
        Invalid = 0,

        // 생존자용
        Charge_FullGauge,
        Charge_15,

        // 추격자용
        Kill,
        Kill_5_Vehicle,
        Kill_5,
        Kill_10,
        Kill_15,
        Kill_20,

        // 생존자용
        Charge,
    }

    private Dictionary<int, int> rewardPointDic;
    private List<FirstReward> firstCheckList;

    public RewardPointManager()
    {
        rewardPointDic = new Dictionary<int, int>();
        firstCheckList = new List<FirstReward>();
    }

    public void Init()
    {
        rewardPointDic.Clear();
        firstCheckList.Clear();
    }

    public bool IsFirst(FirstReward type)
    {
        return !firstCheckList.Contains(type);
    }

    public void SetFirst(FirstReward type)
    {
        if (firstCheckList.Contains(type))
            SBDebug.LogError($"{type} : 이미 first가 떴는데 또 뜸?");
        else
            firstCheckList.Add(type);
    }

    public void AddReward(int key, int amount = 1)
    {
        if (rewardPointDic.ContainsKey(key))
            rewardPointDic[key] += amount;
        else
            rewardPointDic[key] = amount;

        // SBDebug.Log($"AddReward : {key} x{amount}");

        var data = Managers.Data.GetData(GameDataManager.DATA_TYPE.reward_point, key) as RewardGameData;
        if (data != null)
        {
            CharacterObject charObj = Managers.Object.FindCharacterById(Managers.UserData.MyUserID);
            Game.Instance.HudNode.OnPoint(charObj, data.Point);
        }
    }

    public int GetTotalPoint()
    {
        int result = 0;
        foreach (var kv in rewardPointDic)
        {
            var key = kv.Key;
            var amount = kv.Value;

            var data = Managers.Data.GetData(GameDataManager.DATA_TYPE.reward_point, key) as RewardGameData;
            if (data == null) continue;
            result += data.Point * amount;
        }

        return result;
    }

    public Dictionary<int, int> GetRewardPointResult()
    {
        var sortedDict = from entry in rewardPointDic orderby entry.Key ascending select entry;
        return sortedDict.ToDictionary(x => x.Key, x => x.Value);
    }
}
