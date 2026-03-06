using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class ArenaRankTable : TableBase<ArenaRankData, DBPvp_rank> 
    {
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }

        //현재 포인트로 티어 업에 필요한 포인트 수치 가져옴
        public int GetNeedPointByTierLevelUP(int currentPoint)
        {
  
            List<ArenaRankData> totalData = new List<ArenaRankData>(datas.Values);

            if (totalData == null || totalData.Count <= 0)
            {
                return -1;
            }

            totalData.Sort((a, b) => a.NEED_POINT.CompareTo(b.NEED_POINT));//return a.NEED_POINT - b.NEED_POINT;

            for (var i = 0; i < totalData.Count; i++)
            {
                var currentIndex = i;
                var nextindex = i + 1;

                var currentNeedPoint = totalData[i].NEED_POINT;
                var nextNeedPoint = 0;
                if (totalData.Count > nextindex)
                {
                    nextNeedPoint = totalData[nextindex].NEED_POINT;
                }

                if (nextNeedPoint >= 0)
                {
                    if (currentPoint >= currentNeedPoint && nextNeedPoint > currentPoint)
                    {
                        return nextNeedPoint;
                    }
                }
                else
                {
                    return currentNeedPoint;
                }
            }
            return -1;
        }

        public ArenaRankData GetCurrentRankData(int currentPoint)
        {


            List<ArenaRankData> totalData = new List<ArenaRankData>(datas.Values);

            if (totalData == null || totalData.Count <= 0)
            {
                return null;
            }

            totalData.Sort((a, b) => a.NEED_POINT.CompareTo(b.NEED_POINT));//return a.NEED_POINT - b.NEED_POINT;

            if(currentPoint == 0)
            {
                return totalData[0];
            }

            for (var i = 0; i < totalData.Count; i++)
            {
                var currentIndex = i;
                var nextindex = i + 1;

                var currentNeedPoint = totalData[i].NEED_POINT;
                var nextNeedPoint = -1;
                if (totalData.Count > nextindex)
                {
                    nextNeedPoint = totalData[nextindex].NEED_POINT;
                }

                if (nextNeedPoint >= 0)
                {
                    if (currentPoint >= currentNeedPoint && nextNeedPoint > currentPoint)
                    {
                        return totalData[currentIndex];
                    }
                }
                else
                {
                    return totalData[currentIndex];
                }
            }
            return null;
        }

        public ArenaRankData GetFirstInGroup(int group)
        {
            foreach (var item in datas.Values)
            {
                if(item.GROUP == group)
                {
                    return item;
                }
            }
            return null;
        }
        public Dictionary<int, List<ArenaRankData>> GetAllResetRankDic()
        {
            Dictionary<int, List<ArenaRankData>> ret = new();
            foreach(var item in datas.Values)
            {
                var resetRank = item.RESET_RANK;
                if (ret.ContainsKey(resetRank) ==false)
                {
                    ret[resetRank] = new List<ArenaRankData>();
                }
                ret[resetRank].Add(item);
            }
            return ret;
        }
    }

    public class ArenaRankSeasonRewardTable : TableBase<ArenaRankSeasonRewardData, DBPvp_rank_season_reward>
    {
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
    }

    public class ArenaSeasonTable : TableBase<ArenaSeasonData, DBPvp_season>
    {
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
    }
}
