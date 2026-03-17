using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class DailyDungeonProgressData
    {
        public List<int> TodayWorldIndex { get; private set; } = null;
        public int DailyDungeonTicketCount { get; private set; } = -1;
        //젬 충전 잔여 횟수 -> 광고 충전 잔여 횟수
        public int DailyDungeonGemRemainCount { get; private set; } = -1;
        public Dictionary<int, JObject> WorldList { get; private set; } = null;

        public void Init()
        {
            if (WorldList == null)
                WorldList = new Dictionary<int, JObject>();
            else
                WorldList.Clear();
        }

        public void SetData(JArray jsonData)
        {
            WorldList.Clear();
            int arrayLength = jsonData.Count();
            for (int i = 0; i < arrayLength; i++)
            {
                var data = jsonData[i];
                if (data == null || data.HasValues == false)
                {
                    continue;
                }

                SetWorldData(JObject.FromObject(data));
            }
        }

        public void SetDailyInfoData(JObject jsonData)
        {
            if (jsonData == null)
                return;

            SetTodayWorldIndex(jsonData["world"]);
            SetDailyDungeonTicketCount(jsonData["battle_count"]);
            SetDailyDungeonGemRemainCount(jsonData["gem_use"]);
        }

        public void SetTodayWorldIndex(JToken world)
        {
            if (SBFunc.IsJArray(world))
                TodayWorldIndex = new List<int>(world.ToObject<int[]>());
        }

        public void SetDailyDungeonTicketCount(JToken count)
        {
            if (SBFunc.IsJTokenType(count, JTokenType.Integer))
                DailyDungeonTicketCount = count.Value<int>();
        }

        public void SetDailyDungeonGemRemainCount(JToken count)
        {
            if (SBFunc.IsJTokenType(count, JTokenType.Integer))
                DailyDungeonGemRemainCount = count.Value<int>();
        }

        public void SetWorldData(JObject jsonData)//{world: 101, diff: 1, stages: Array(10)}
        {
            int worldIndex = jsonData["world"].Value<int>();
            WorldList[worldIndex] = jsonData;//그냥 통으로 넣어버림
        }

        public JObject GetDailyDungeonInfoData(int worldIndex)// key는 worldIndex (ex. Mon == 101)
        {
            var data = WorldList[worldIndex];
            if (data == null || data.HasValues == false)
                return null;
            else
                return data;
        }

        public List<int> GetDailyProgressData(int worldIndex)
        {
            var data = GetDailyDungeonInfoData(worldIndex);
            if(data == null) { 
                return null;
            }
            else
            {
                var arr = (JArray)data["stages"];
                return arr.ToObject<List<int>>();
            }
        }

        public int GetLastestStage(int worldIndex)
        {
            var worldData = GetDailyProgressData(worldIndex);
            if(worldData != null)
            {
                int clearCnt = 0;
                foreach(var stage in worldData)
                {
                    if (stage != 0)
                    {
                        ++clearCnt;
                    }
                }
                return Mathf.Min(clearCnt + 1, worldData.Count);
            }
            return 0;
        }

    }
}