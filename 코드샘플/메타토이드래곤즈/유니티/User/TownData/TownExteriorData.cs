using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class TownExteriorData
    {
        public int ExteriorLevel { get; private set; } = -1;
        public int ExteriorFloor { get; private set; } = -1;
        public eBuildingState ExteriorState { get; private set; } = eBuildingState.NONE;
        public int ExteriorTime { get; private set; } = -1;
        public Dictionary<int, Dictionary<int, int>> ActiveGrid { get; private set; } = new Dictionary<int, Dictionary<int, int>>();
        public Dictionary<int, Dictionary<int, int>> ExteriorGrid { get; private set; } = new Dictionary<int, Dictionary<int, int>>();
        public Dictionary<int, Dictionary<int, int>> ClickGrid { get; private set; } = new Dictionary<int, Dictionary<int, int>>();

        public void Set(JObject jsonData)
        {
            if (SBFunc.IsJTokenCheck(jsonData["exterior"]) && SBFunc.IsJTokenType(jsonData["exterior"]["state"], JTokenType.Integer))
            {
                ExteriorState = (eBuildingState)jsonData["exterior"]["state"].Value<int>();
            }
            if (SBFunc.IsJTokenType(jsonData["state"], JTokenType.Integer))
            {
                ExteriorState = (eBuildingState)jsonData["state"].Value<int>();
            }

            if (SBFunc.IsJTokenCheck(jsonData["exterior"]) && SBFunc.IsJTokenType(jsonData["exterior"]["level"], JTokenType.Integer))
            {
                ExteriorLevel = jsonData["exterior"]["level"].Value<int>();

                if (ExteriorState == eBuildingState.CONSTRUCT_FINISHED)
                    ExteriorLevel--;
            }
            if (SBFunc.IsJTokenType(jsonData["level"], JTokenType.Integer))
            {
                ExteriorLevel = jsonData["level"].Value<int>();

                if (ExteriorState == eBuildingState.CONSTRUCT_FINISHED)
                    ExteriorLevel--;
            }

            if (jsonData.ContainsKey("exterior") && SBFunc.IsJTokenType(jsonData["exterior"]["floor"], JTokenType.Integer))
                ExteriorFloor = jsonData["exterior"]["floor"].Value<int>();
            if (SBFunc.IsJTokenType(jsonData["floor"], JTokenType.Integer))
                ExteriorFloor = jsonData["floor"].Value<int>();

            if (SBFunc.IsJTokenCheck(jsonData["exterior"]) && SBFunc.IsJTokenType(jsonData["exterior"]["construct_exp"], JTokenType.Integer))
                ExteriorTime = jsonData["exterior"]["construct_exp"].Value<int>();
            if (SBFunc.IsJTokenType(jsonData["construct_exp"], JTokenType.Integer))
                ExteriorTime = jsonData["construct_exp"].Value<int>();

            SetGrid(jsonData);
        }

        public void SetGrid(JObject jsonData)
        {
            if (SBFunc.IsJTokenCheck(jsonData["exterior"]) && SBFunc.IsJTokenCheck(jsonData["exterior"]["grid"]) || SBFunc.IsJTokenCheck(jsonData["grid"]))
            {
                JObject grid = null;
                if (SBFunc.IsJTokenCheck(jsonData["exterior"]) && SBFunc.IsJTokenCheck(jsonData["exterior"]["grid"]))
                {
                    grid = (JObject)jsonData["exterior"]["grid"];
                }
                else
                {
                    grid = (JObject)jsonData["grid"];
                }

                foreach (var item in grid.Properties())
                {
                    var key = item.Name;
                    if (grid[key].Type != JTokenType.Array)
                        continue;

                    var array = (JArray)grid[key];

                    var numKey = int.Parse(key.ToString().Replace('B', '-'));
                    if (numKey > 0)
                    {
                        numKey -= 1;
                    }

                    ActiveGrid[numKey] = new Dictionary<int, int>();
                    ExteriorGrid[numKey] = new Dictionary<int, int>();
                    ClickGrid[numKey] = new Dictionary<int, int>();

                    int lastData = 0;
                    var dataCount = array.Count;
                    for (var j = 0; j < dataCount; j++)
                    {
                        int arrayData = 0;
                        if (array[j].Type == JTokenType.Integer)
                            arrayData = array[j].Value<int>();
                        ExteriorGrid[numKey].Add(j, arrayData);
                        if (arrayData == 0)
                            continue;

                        if (arrayData == 1)
                        {
                            ClickGrid[numKey].Add(j, lastData);
                            continue;
                        }

                        lastData = arrayData;
                        ActiveGrid[numKey].Add(j, arrayData);
                        ClickGrid[numKey].Add(j, arrayData);
                    }
                }
            }
        }

        public void SetExteriorState(eBuildingState state)
        {
            ExteriorState = state;
        } 

        public void Clear()
        {
            ActiveGrid.Clear();
            ExteriorGrid.Clear();
            ClickGrid.Clear();
        }
    }
}