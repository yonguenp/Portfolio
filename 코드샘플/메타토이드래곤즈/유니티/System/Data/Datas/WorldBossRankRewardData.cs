using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /// <summary>
    /// WJ - 레이드 보스 컨텐츠 보상 테이블 (탐험 보스가 아님!)
    /// 기존 소스들을 Raid로 가기엔 작업된 내용이 너무많음...
    /// (참고)기존 소스들은 "WorldBoss~" 로 시작하는 워딩을 사용.
    /// </summary>
    public class WorldBossRankRewardData : TableData<DBRaid_boss_rank_reward>
    {
        static private WorldBossRankRewardTable table = null;
        static public List<WorldBossRankRewardData> GetGroup(int group)
        {
            if (table == null)
                table = TableManager.GetTable<WorldBossRankRewardTable>();

            return table.GetGroup(group);
        }

        public string KEY { get { return UNIQUE_KEY; } }
        public int GROUP_ID => Data.GROUP;
        public int HIGHEST_RANK => Data.HIGHEST_RANK;
        public uint LOWEST_RANK => Data.LOWEST_RANK;
        public int REWARD_GROUP => Data.REWARD_GROUP;
    }


}
