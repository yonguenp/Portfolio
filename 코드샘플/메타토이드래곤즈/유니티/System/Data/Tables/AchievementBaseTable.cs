using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AchievementBaseTable : TableBase<AchievementBaseData, DBAchievements_info>
    {
        public int GetAchievementTotalCount()
        {
            LoadAll();
            return datas.Values.Count;
        }
    }
}
