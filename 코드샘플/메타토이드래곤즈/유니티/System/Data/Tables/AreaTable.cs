using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.Mathematics;
using UnityEngine;

namespace SandboxNetwork
{
    public class AreaExpansionTable : TableBase<AreaExpansionData, DBArea_expansion>
    {
        Dictionary<int, List<AreaExpansionData>> groupDic = null;
        public override void Init()
        {
            base.Init();
            groupDic = new();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (groupDic == null)
                groupDic = new();
            else
                groupDic.Clear();
        }

        public AreaExpansionData GetFloorData(int floor)
        {
            var target = floor - 1;
            AreaExpansionData pickData = null;

            if (datas != null) 
            {
                var it = datas.GetEnumerator();
                while(it.MoveNext())
                {
                    var cur = it.Current.Value;
                    if (cur == null)
                        continue;

                    if (cur.FLOOR >= target && (pickData == null || (pickData != null && pickData.FLOOR > cur.FLOOR)))
                        pickData = cur;
                }
            }

            return pickData;
        }

        public Vector3Int GetBetweenFloor(int areaGroup, int areaLevel)
        {
            var vec = new Vector3Int(0, 0, 0);

            if (datas != null)
            {
                var it = datas.GetEnumerator();
                while (it.MoveNext())
                {
                    var value = it.Current.Value;
                    if (value == null)
                        continue;

                    if (areaGroup >= value.AREA_GROUP)
                    {
                        if (vec.x >= value.FLOOR)
                        {
                            vec.x = value.FLOOR;
                        }
                        if (vec.y <= value.FLOOR)
                        {
                            vec.y = value.FLOOR;
                        }
                        if (areaLevel >= value.OPEN_LEVEL)
                        {
                            if (vec.z >= value.FLOOR)
                            {
                                vec.z = value.FLOOR;
                            }
                        }
                    }
                }
            }

            return vec;
        }

        public int GetFloorByLevel(int areaLv)
        {
            int limitFloor = 2;
            if (datas != null)
            {
                var it = datas.GetEnumerator();
                while (it.MoveNext())
                {
                    var cur = it.Current.Value;
                    if (cur == null)
                        continue;
                    if (cur.OPEN_LEVEL <= areaLv)
                    {
                        limitFloor = math.max(limitFloor, cur.ORIGIN_FLOOR);
                    }
                }
            }
            return limitFloor;
        }

        public int GetMaxFloorLevel()
        {
            int maxLevel = 0;

            if (datas != null)
            {
                var it = datas.GetEnumerator();
                while (it.MoveNext())
                {
                    var cur = it.Current.Value;
                    if (cur == null)
                        continue;

                    maxLevel = maxLevel < cur.ORIGIN_FLOOR ? cur.ORIGIN_FLOOR : maxLevel;
                }
            }

            return maxLevel;
        }

        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(AreaExpansionData data)
        {
            if (base.Add(data))
            {
                if (!groupDic.ContainsKey(data.AREA_GROUP))
                    groupDic.Add(data.AREA_GROUP, new());

                groupDic[data.AREA_GROUP].Add(data);
                return true;
            }
            return false;
        }
    }

    public class AreaLevelTable : TableBase<AreaLevelData, DBArea_level>
    {
        public override void Preload()
        {
            base.Preload();

            LoadAll();
        }
        public AreaLevelData GetByLevel(int level)
        {
            var values = new List<AreaLevelData>(datas.Values);
            var count = values.Count;

            for (var i = 0; i < count; i++)
            {
                var data = values[i];
                if (data == null)
                    continue;

                if (data.LEVEL == level)
                    return data;
            }

            return null;
        }

        public int GetMaxLevel()
        {
            var values = new List<AreaLevelData>(datas.Values);
            var count = values.Count;

            int level = -1;
            for (var i = 0; i < count; i++)
            {
                AreaLevelData curData = values[i];
                if (curData == null)
                    continue;

                if (level < curData.LEVEL)
                    level = curData.LEVEL;
            }

            return level;
        }
    }

    public class TravelTable : TableBase<TravelData, DBWorld_trip>
    {
        public Dictionary<int, TravelData> worldDatas = null;

        public override void Init()
        {
            base.Init();
            worldDatas = new Dictionary<int, TravelData>();
        }

        public override void DataClear()
        {
            base.DataClear();
            if (worldDatas == null)
                worldDatas = new Dictionary<int, TravelData>();
            else
                worldDatas.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(TravelData data)
        {
            if (base.Add(data))
            {
                if (worldDatas.ContainsKey(data.WORLD))
                    return false;

                worldDatas.Add(data.WORLD, data);
                return true;
            }
            return false;
        }
        public TravelData GetByWorldID(int world)
        {
            var values = new List<TravelData>(datas.Values);
            var count = values.Count;

            for (var i = 0; i < count; i++)
            {
                var data = values[i];
                if (data == null)
                    continue;

                if (data.WORLD == world)
                    return data;
            }

            return null;
        }
    }


    public class RestrictedAreaTable : TableBase<RestrictedAreaData, DBWorld_trip_hell>
    {
        public Dictionary<StageDifficult, Dictionary<int, RestrictedAreaData>> worldDatas = null;

        public override void Init()
        {
            base.Init();
            worldDatas = new Dictionary<StageDifficult, Dictionary<int, RestrictedAreaData>>();
        }

        public override void DataClear()
        {
            base.DataClear();
            if (worldDatas == null)
                worldDatas = new Dictionary<StageDifficult, Dictionary<int, RestrictedAreaData>>();
            else
                worldDatas.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(RestrictedAreaData data)
        {
            if (base.Add(data))
            {
                if (!worldDatas.ContainsKey((StageDifficult)data.NEED_WORLD_DIFFICULT))
                    worldDatas.Add((StageDifficult)data.NEED_WORLD_DIFFICULT, new Dictionary<int, RestrictedAreaData>());

                if (worldDatas[(StageDifficult)data.NEED_WORLD_DIFFICULT].ContainsKey(data.NEED_WORLD_CLEAR))
                {
                    return false;
                }

                worldDatas[(StageDifficult)data.NEED_WORLD_DIFFICULT].Add(data.NEED_WORLD_CLEAR, data);
                return true;
            }
            return false;
        }
        public RestrictedAreaData GetByWorldDiff(int world, StageDifficult diff)
        {
            if(worldDatas.ContainsKey(diff))
            {
                if (worldDatas[diff].ContainsKey(world))
                    return worldDatas[diff][world];
            }

            return null;
        }
    }
    public class AreaLevelMissionTable: TableBase<AreaLevelMissionData, DBArea_level_mission>
	{
        private Dictionary<int, List<AreaLevelMissionData>> groupDic = null;
        public override void Init()
        {
            base.Init();
            if (groupDic == null)
                groupDic = new();
            else
                groupDic.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (groupDic == null)
                groupDic = new();
            else
                groupDic.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(AreaLevelMissionData data)
        {
            if (base.Add(data))
            {
                if (!groupDic.ContainsKey(data.GROUP))
                    groupDic.Add(data.GROUP, new());

                groupDic[data.GROUP].Add(data);
                return true;
            }
            return false;
        }
        public List<AreaLevelMissionData> GetGroup(int group)
		{
            if (groupDic == null || groupDic.ContainsKey(group) == false)
                return null;
 
			return groupDic[group];
		}
	}
}
