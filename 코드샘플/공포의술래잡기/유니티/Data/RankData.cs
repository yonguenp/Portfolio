using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankData : GameData
{
    public int userNo { get; private set; }
    public string userNick { get; private set; }
    public int Point { get; private set; }
    public int bestPoint { get; private set; }
    public int idx_Rank { get; private set; }
    public RankData(int no, int point, string nick, int best_point, int rank_index) { userNo = no; Point = point; userNick = nick; bestPoint = best_point; idx_Rank = rank_index; }
}

public class RankType : GameData
{
    public int start_point { get; private set; }
    public int end_point { get; private set; }

    public int win_point { get; private set; }
    public int lose_point { get; private set; }

    public List<ShopPackageGameData> rank_reward { get; private set; }
    public Sprite rank_resource { get; private set; }
    
    static public Sprite DUMMY_RANK_ICON = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/UI/Icon/Icon_missing");

    static public RankType MaxRank = null;
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        start_point = Int(data["start_point"]);
        end_point = Int(data["end_point"]);
        if (end_point <= 0)
        {
            end_point = int.MaxValue;
        }

        
        if (MaxRank == null)
            MaxRank = this;

        if (MaxRank.end_point < end_point)
        {
            MaxRank = this;
        }

            win_point = Int(data["win_point"]);
        lose_point = Int(data["lose_point"]);

        rank_reward = ShopPackageGameData.GetRewardDataList(Int(data["rank_reward"]));

        rank_resource = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/" + data["rank_resource"]);
        if (rank_resource == null)
        {
            rank_resource = DUMMY_RANK_ICON;
        }
    }

    public static RankType GetRankFromPoint(int point)
    {
        List<GameData> ranks = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.rank_grade);
        RankType max = null;
        foreach (RankType rank in ranks)
        {
            if (rank.start_point <= point && rank.end_point > point)
                return rank;
            // if (Enumerable.Range(rank.start_point, rank.end_point).Contains(point))      //사용법이 시작값, 풀(카운트) 사용방법이 잘못됬음.
            //     return rank;

            if(max == null || max.end_point < rank.end_point)
                max = rank;
        }

        if(max.end_point < point)
        {
            return max;
        }
        if (ranks == null || ranks.Count == 0)
            return null;

        return ranks[0] as RankType;
    }

    public static RankType GetRankDataFromRank(int rank)
    {
        List<GameData> rankList = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.rank_grade);
        RankType max = null;
        foreach (RankType rankType in rankList)
        {
            if (rankType.GetID() == rank)
                return rankType;
            // if (Enumerable.Range(rank.start_point, rank.end_point).Contains(point))      //사용법이 시작값, 풀(카운트) 사용방법이 잘못됬음.
            //     return rank;
        }

        if (rankList == null || rankList.Count == 0)
            return null;

        return rankList[0] as RankType;
    }
}