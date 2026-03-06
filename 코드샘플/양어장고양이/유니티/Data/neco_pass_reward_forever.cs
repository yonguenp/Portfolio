using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_pass_reward_forever : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_PASS_REWARD_FOREVER; }

    static public List<neco_pass_reward_forever> GetNecoPassRewardList()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PASS_REWARD_FOREVER);
        if (necoData == null)
        {
            return null;
        }

        List<neco_pass_reward_forever> passRewardList = new List<neco_pass_reward_forever>();

        object obj;
        foreach (neco_pass_reward_forever passRewardData in necoData)
        {
            passRewardList.Add(passRewardData);
        }

        return passRewardList;
    }

    static public neco_pass_reward_forever GetNecoPassReward(uint level)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PASS_REWARD_FOREVER);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_pass_reward_forever rewardData in necoData)
        {
            if (rewardData.data.TryGetValue("level", out obj))
            {
                if (level == (uint)obj)
                {
                    return rewardData;
                }
            }
        }

        return null;
    }

    [NonSerialized]
    private uint necoPassRewardID = 0;
    public uint GetNecoPassRewardID()
    {
        if (necoPassRewardID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoPassRewardID = (uint)obj;
            }
        }

        return necoPassRewardID;
    }

    [NonSerialized]
    private string necoPassRewardDesc = "";
    public string GetNecoPassRewardDesc()
    {
        if (necoPassRewardDesc == "")
        {
            object obj;
            if (data.TryGetValue("desc", out obj))
            {
                necoPassRewardDesc = (string)obj;
            }
        }

        return necoPassRewardDesc;
    }

    [NonSerialized]
    private uint necoPassRewardLevel = 0;
    public uint GetNecoPassRewardLevel()
    {
        if (necoPassRewardLevel == 0)
        {
            object obj;
            if (data.TryGetValue("level", out obj))
            {
                necoPassRewardLevel = (uint)obj;
            }
        }

        return necoPassRewardLevel;
    }

    [NonSerialized]
    private uint necoPassRewardExp = 0;
    public uint GetNecoPassRewardExp()
    {
        if (necoPassRewardExp == 0)
        {
            object obj;
            if (data.TryGetValue("need_exp", out obj))
            {
                necoPassRewardExp = (uint)obj;
            }
        }

        return necoPassRewardExp;
    }

    public string GetRewardGradeTypeInfo(string searchRewardTypeName)
    {
        string rewardInfo = "";

        object obj;
        if (data.TryGetValue(searchRewardTypeName, out obj))
        {
            rewardInfo = (string)obj;
        }


        return rewardInfo;
    }

    public uint GetRewardGradeItemInfo(string searchRewardTypeName)
    {
        uint rewardInfo = 0;

        object obj;
        if (data.TryGetValue(searchRewardTypeName, out obj))
        {
            rewardInfo = (uint)obj;
        }


        return rewardInfo;
    }

    public List<KeyValuePair<string, KeyValuePair<uint, uint>>> GetRewardGradeList()
    {
        const int REWARD_GRADE_COUNT = 1;

        List<KeyValuePair<string, KeyValuePair<uint, uint>>> rewardGradeList = new List<KeyValuePair<string, KeyValuePair<uint, uint>>>();

        string findRewardType = string.Empty;
        string findRewardID = string.Empty;
        string findRewardCount = string.Empty;

        string rewardType = "";
        uint rewardID = 0;
        uint rewardCount = 0;
        for (int i = 1; i <= REWARD_GRADE_COUNT; ++i)
        {
            findRewardType = string.Format("reward_type", i);
            rewardType = GetRewardGradeTypeInfo(findRewardType);

            findRewardID = string.Format("reward_id", i);
            rewardID = GetRewardGradeItemInfo(findRewardID);

            findRewardCount = string.Format("reward_count", i);
            rewardCount = GetRewardGradeItemInfo(findRewardCount);

            KeyValuePair<uint, uint> itemPair = new KeyValuePair<uint, uint>(rewardID, rewardCount);
            KeyValuePair<string, KeyValuePair<uint, uint>> rewardPair = new KeyValuePair<string, KeyValuePair<uint, uint>>(rewardType, itemPair);

            rewardGradeList.Add(rewardPair);
        }

        return rewardGradeList;
    }

    public KeyValuePair<string, KeyValuePair<uint, uint>> GetRewardGradeInfo(int grade)
    {
        KeyValuePair<string, KeyValuePair<uint, uint>> rewardGradeInfo = new KeyValuePair<string, KeyValuePair<uint, uint>>();

        string findRewardType = string.Empty;
        string findRewardID = string.Empty;
        string findRewardCount = string.Empty;

        string rewardType = "";
        uint rewardID = 0;
        uint rewardCount = 0;

        findRewardType = string.Format("reward_type", grade);
        rewardType = GetRewardGradeTypeInfo(findRewardType);

        findRewardID = string.Format("reward_id", grade);
        rewardID = GetRewardGradeItemInfo(findRewardID);

        findRewardCount = string.Format("reward_count", grade);
        rewardCount = GetRewardGradeItemInfo(findRewardCount);

        KeyValuePair<uint, uint> itemPair = new KeyValuePair<uint, uint>(rewardID, rewardCount);
        rewardGradeInfo = new KeyValuePair<string, KeyValuePair<uint, uint>>(rewardType, itemPair);

        return rewardGradeInfo;
    }

    public bool IsRecivedReward(uint index)
    {
        neco_pass_data mission = neco_data.Instance.GetPassData();
        uint levelStatus = mission.GetRewardStatus(GetNecoPassRewardLevel());
        
        return (levelStatus & (1 << ((int)index - 1))) != 0;
    }
}
