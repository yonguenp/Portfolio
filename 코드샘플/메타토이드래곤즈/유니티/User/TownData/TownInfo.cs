using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownInfo
{
    public int AreaLevel { get; private set; } = -1;
    public int AreaExLevel { get { return areaLvData != null ? areaLvData.EXPANSION_AREA : -1; } }
    public int Width { get { return areaLvData != null ? areaLvData.WIDTH : -1; } }
    public int FloorMin { get { return areaExData != null ? areaExData.x : -1; } }
    public int FloorMax { get { return areaExData != null ? areaExData.y : -1; } }
    public int AreaOpenMinLevel { get { return areaExData != null ? areaExData.z : -1; } }
    public int AreaOpenMaxLevel { get { return User.Instance.ExteriorData.ExteriorFloor - 1; } }

    public Dictionary<string, List<BuildingOpenData>> Buildings = new Dictionary<string, List<BuildingOpenData>>();

    private AreaLevelData areaLvData = null;
    private Vector3Int areaExData = Vector3Int.zero;

    public void RefreshData()
    {
        AreaLevel = User.Instance.ExteriorData.ExteriorLevel;
        areaLvData = AreaLevelData.GetByLevel(AreaLevel);
        areaExData = AreaExpansionData.GetBetweenFloor(AreaExLevel, AreaLevel);

        Buildings.Clear();
        var buildingDatas = BuildingOpenData.GetOpenListByLevel(AreaLevel);
        if (buildingDatas != null)
        {
            BuildingOpenData.GetOpenListByLevel(AreaLevel).ForEach(building =>
            {
                if (!Buildings.ContainsKey(building.BUILDING))
                {
                    Buildings.Add(building.BUILDING, new List<BuildingOpenData>());
                }

                Buildings[building.BUILDING].Add(building);
            });
        }
    }

    public int GetBuildingCount(string buildingName)
    {
        int ret = 0;
        if (Buildings.ContainsKey(buildingName))
        {
            foreach(var building in Buildings[buildingName])
            {
                ret += building.COUNT;
            }
        }

        return ret;
    }

}
