using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class BuildingBaseTable : TableBase<BuildingBaseData, DBBuilding_base>
    {
        public static BuildingBaseData Dozer { get; private set; } = null;
        public static BuildingBaseData Travel { get; private set; } = null;
        public static BuildingBaseData Subway { get; private set; } = null;

        public List<BuildingBaseData> ProductBuilding { get; set; } = new();

        public override void Init()
        {
            base.Init();
            Dozer = null;
            Travel = null;
            Subway = null;
            if (ProductBuilding == null)
                ProductBuilding = new();
            else
                ProductBuilding.Clear();
        }

        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(BuildingBaseData data)
        {
            if (base.Add(data))
            {
                if (data.IS_LANDMARK)
                {
                    switch (data.BUILDING_ID)
                    {
                        case 1:
                            Dozer = data;
                            break;
                        case 2:
                            Travel = data;
                            break;
                        case 3:
                            Subway = data;
                            break;
                    }
                }
                else
                {
                    ProductBuilding.Add(data);
                }
                return true;
            }
            return false;
        }

        public override void DataClear()
        {
            base.DataClear();
            Dozer = null;
            Travel = null;
            Subway = null;
            if (ProductBuilding == null)
                ProductBuilding = new();
            else
                ProductBuilding.Clear();
        }

        public BuildingBaseData GetIndex(string index)
        {
            foreach (KeyValuePair<string, BuildingBaseData> element in datas)
            {
                if (element.Value._NAME == index)
                {
                    return element.Value;
                }
            }

            return null;
        }

        public List<BuildingBaseData> GetProductBuildingList()
        {
            return ProductBuilding;
        }

        public BuildingBaseData GetBuildingDataWithTag(int tag)
        {
            BuildingBaseData resultData = null;

            BuildingOpenData openData = BuildingOpenData.GetByInstallTag(tag);
            if (openData != null)
            {
                resultData = Get(openData.BUILDING);
            }

            return resultData;
        }
    }
    public class BuildingLevelTable : TableBase<BuildingLevelData, DBBuilding_level>
    {
        Dictionary<string, List<BuildingLevelData>> dicGroup = new Dictionary<string, List<BuildingLevelData>>();
        public override void DataClear()
        {
            base.DataClear();
            dicGroup.Clear();
        }
        public override void Init()
        {
            base.Init();
            if (dicGroup == null)
                dicGroup = new();
            else
                dicGroup.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(BuildingLevelData data)
        {
            if (base.Add(data))
            {
                if (!dicGroup.ContainsKey(data.BUILDING_GROUP))
                {
                    dicGroup[data.BUILDING_GROUP] = new List<BuildingLevelData>();
                }
                dicGroup[data.BUILDING_GROUP].Add(data);
                return true;
            }
            return false;
        }

        public int GetBuildingMaxLevelByGroup(string group)
        {
            int result = -1;
            List<BuildingLevelData> totalDataList = GetDataByGroup(group);

            if (totalDataList == null || totalDataList.Count <= 0)
            {
                return result;
            }

            totalDataList.Sort((a, b) => a.LEVEL.CompareTo(b.LEVEL));

            result = totalDataList[totalDataList.Count - 1].LEVEL;

            return result;
        }

        public int GetLevelUpNeedAreaLevel(string buildingGroup, int level)
        {
            var instance = TableManager.GetTable<BuildingLevelTable>();
            int tempLevel = -1;
            if (instance.datas == null || instance.datas.Keys.Count <= 0)
            {
                return tempLevel;
            }

            List<BuildingLevelData> buildingLevelLIst = instance.GetDataByGroup(buildingGroup);
            if (buildingLevelLIst.Count <= 0)
            {
                return tempLevel;
            }

            foreach (BuildingLevelData levelData in buildingLevelLIst)
            {
                if (levelData.LEVEL == level)
                {
                    tempLevel = levelData.NEED_AREA_LEVEL;
                    break;
                }
            }
            return tempLevel;
        }

        public List<BuildingLevelData> GetDataByGroup(string group)
        {
            if(dicGroup.ContainsKey(group))
                return dicGroup[group];

            return null;
        }
        public BuildingLevelData GetDataByGroupAndLevel(string group, int level)
        {
            BuildingLevelData returnData = null;
            var keys = datas.Keys;

            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    var data = datas[key];
                    if (data != null)
                    {
                        if (data.BUILDING_GROUP == group && data.LEVEL == level)
                        {
                            returnData = data;
                            break;
                        }
                    }
                }
            }

            return returnData;
        }

        
    }

    public class BuildingOpenTable : TableBase<BuildingOpenData, DBBuilding_open>
    {
        Dictionary<int, List<BuildingOpenData>> dicOpenLevel = new Dictionary<int, List<BuildingOpenData>>();
        Dictionary<int, BuildingOpenData> dicInstallTag = new Dictionary<int, BuildingOpenData>();
        Dictionary<string, List<BuildingOpenData>> dicBuildingGroup = new Dictionary<string, List<BuildingOpenData>>();

        public List<BuildingOpenData> GetByOpenLevel(int keyLevel)
        {
            if (dicOpenLevel.ContainsKey(keyLevel))
            {
                return dicOpenLevel[keyLevel];
            }
            return null;
        }
        public List<BuildingOpenData> GetByBuildable(int keyLevel)
        {
            List<BuildingOpenData> returnDatas = new List<BuildingOpenData>();
            for (int i = keyLevel; i > 0; --i)
            {
                if (dicOpenLevel.ContainsKey(i))
                {
                    List<BuildingOpenData> openDatas = dicOpenLevel[i];
                    for (int j = 0; j < openDatas.Count; ++j)
                    {
                        returnDatas.Add(openDatas[j]);
                    }
                }
            }
            return returnDatas;
        }

        public BuildingOpenData GetByInstallTag(int keyInstallTag)
        {
            if (dicInstallTag.ContainsKey(keyInstallTag))
            {
                return dicInstallTag[keyInstallTag];
            }
            return null;
        }

        public List<BuildingOpenData> GetByBuildingGroup(string keyGroup)
        {
            if (dicBuildingGroup.ContainsKey(keyGroup))
            {
                return dicBuildingGroup[keyGroup];
            }
            return null;
        }

        public override void Init()
        {
            base.Init();
            dicOpenLevel = new();
            dicInstallTag = new();
            dicBuildingGroup = new();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (dicOpenLevel == null)
                dicOpenLevel = new();
            else
                dicOpenLevel.Clear();
            if (dicInstallTag == null)
                dicInstallTag = new();
            else
                dicInstallTag.Clear();
            if (dicBuildingGroup == null)
                dicBuildingGroup = new();
            else
                dicBuildingGroup.Clear();
        }
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        protected override bool Add(BuildingOpenData data)
        {
            if (base.Add(data))
            {
                Add(data, data.OPEN_LEVEL, dicOpenLevel);
                Add(data, data.INSTALL_TAG, dicInstallTag);
                Add(data, data.BUILDING, dicBuildingGroup);
                return true;
            }
            return false;
        }
        bool Add<T>(BuildingOpenData data, T key, Dictionary<T, BuildingOpenData> targetDic)
        {
            if (data == null)
                return false;

            if (targetDic.ContainsKey(key))
            {
                UnityEngine.Debug.LogError("SBTableBase Error => " + key);
                return false;
            }

            targetDic.Add(key, data);
            return true;
        }

        bool Add<T>(BuildingOpenData data, T key, Dictionary<T, List<BuildingOpenData>> targetDic)
        {
            if (data == null)
                return false;

            if (!targetDic.ContainsKey(key))
            {
                targetDic[key] = new();
            }
            targetDic[key].Add(data);

            return true;
        }

        public List<int> GetTagList(string buildingGroup)
        {
            List<int> tagList = new List<int>();
            if (!dicBuildingGroup.ContainsKey(buildingGroup))
                return tagList;

            dicBuildingGroup[buildingGroup].ForEach(building => 
            {
                tagList.Add(building.INSTALL_TAG); 
            });

            return tagList;
        }

        public BuildingOpenData GetWithTag(int tag)
        {
            if (dicInstallTag.ContainsKey(tag))
            {
                return dicInstallTag[tag];
            }
            return null;
        }

        public List<BuildingOpenData> GetWithBuildingGroup(string buildingGroup)
        {
            List<BuildingOpenData> resultData = new List<BuildingOpenData>();
            foreach (KeyValuePair<int, BuildingOpenData> openData in dicInstallTag)
            {
                if (openData.Value.BUILDING == buildingGroup)
                {
                    resultData.Add(openData.Value);
                }
            }

            return resultData;
        }

        public List<BuildingOpenData> GetBuildingGroupExceptBaseType(string buildingGroup, int exceptType)
        {
            List<BuildingOpenData> resultData = new List<BuildingOpenData>();
            foreach (KeyValuePair<int, BuildingOpenData> openData in dicInstallTag)
            {
                if (openData.Value.BUILDING == buildingGroup)
                {
                    BuildingBaseData baseData = openData.Value.BaseData;
                    if (baseData != null && baseData.TYPE != exceptType)
                    {
                        resultData.Add(openData.Value);
                    }
                }
            }

            return resultData;
        }

        // 건물 키 값으로 현재 레벨에 건설 가능한 건물 1개 반환
        public BuildingOpenData GetAvailTotalBuilding(string buildType)
        {
            List<BuildingOpenData> availList = GetAvailTotalBuildingList(buildType);
            availList.Sort((BuildingOpenData d1, BuildingOpenData d2) => { return d1.INSTALL_TAG - d2.INSTALL_TAG; });
            if (availList != null)
            {
                foreach (BuildingOpenData buildingData in availList)
                {
                    BuildInfo userBuildingData = User.Instance.GetUserBuildingInfoByTag(buildingData.INSTALL_TAG);
                    if (userBuildingData == null)   // 없다면 건설되지 않은 건물임
                    {
                        return buildingData;
                    }
                }
            }

            return null;
        }

        // 건물 키 값으로 현재 레벨에 건설 가능한 건물 리스트 반환
        public List<BuildingOpenData> GetAvailTotalBuildingList(string buildType)
        {
            List<BuildingOpenData> totalBuildingList = new();
            foreach (int openLevel in dicOpenLevel.Keys)
            {
                if (User.Instance.GetAreaLevel() >= openLevel)
                {
                    totalBuildingList.AddRange(dicOpenLevel[openLevel]);
                }
            }

            return totalBuildingList.FindAll(element => element.BUILDING == buildType);
        }

        public List<BuildingOpenData> GetTotalBuildingList(string buildType)
        {
            List<BuildingOpenData> totalBuildingList = new();
            foreach (int openLevel in dicOpenLevel.Keys)
            {
                totalBuildingList.AddRange(dicOpenLevel[openLevel]);
            }

            return totalBuildingList.FindAll(element => element.BUILDING == buildType);
        }

        public List<string> GetUserBuildingKeyList()
        {
            BuildingOpenTable instance = TableManager.GetTable<BuildingOpenTable>();

            List<BuildingBaseData> buildingInfoList = BuildingBaseData.GetProductBuildingList();

            Dictionary<int, string> arr = new Dictionary<int, string>();
            List<BuildInfo> userBuildingList = User.Instance.GetUserBuildingList();

            instance.dicInstallTag.Values.ToList().ForEach((element) =>
            {
                int tag = element.INSTALL_TAG;
                var building = User.Instance.GetUserBuildingInfoByTag(tag);
                if(building != null && building.State >= eBuildingState.CONSTRUCTING)
                {
                    for (int i = 0; i < buildingInfoList.Count; i++)
                    {
                        if (buildingInfoList[i].TYPE >= 2 && element.BUILDING == buildingInfoList[i].KEY.ToString())
                        {
                            if (!arr.ContainsKey(tag))
                            {
                                arr.Add(tag, buildingInfoList[i].KEY.ToString());
                                break;
                            }
                        }
                    }
                }
            });

            //tag 오름차순 세팅
            List<int> arrKeys = new List<int>(arr.Keys);
            arrKeys.Sort();

            List<string> buildingIndexList = new List<string>();

            if (arrKeys.Count <= 0)
            {
                return buildingIndexList;
            }

            foreach (int key in arrKeys)
            {
                buildingIndexList.Add(arr[key]);
            }

            return buildingIndexList;
        }

        public List<BuildingOpenData> GetBuildingOpenListByTagGroup(int tag)
        {
            List<BuildingOpenData> resultData = new List<BuildingOpenData>();
            foreach (KeyValuePair<int, BuildingOpenData> openData in dicInstallTag)
            {
                int targetTagGroup = openData.Value.INSTALL_TAG / SBDefine.BUILDING_TAG_MULT;
                if (targetTagGroup == tag / SBDefine.BUILDING_TAG_MULT)
                {
                    resultData.Add(openData.Value);
                }
            }

            return resultData;
        }
    }
}