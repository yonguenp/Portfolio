using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class LandmarkGemDungeon : Landmark
    {
        public static LandmarkGemDungeon Get()
        {
            return User.Instance.GetLandmarkData<LandmarkGemDungeon>();
        }
        public int Level { get; private set; } = 0;
        public eBuildingState State { get; private set; } = eBuildingState.NONE;
        public Dictionary<int, LandmarkGemDungeonFloor> FloorDatas { get; private set; } = null;
        public Dictionary<int, LandmarkGemDungeonDragon> DragonDatas { get; private set; } = null;

        public bool NanochipsetSmithOpened { get; private set; } = false;
        public bool GoldGemSmithOpened { get; private set; } = false;

        public LandmarkGemDungeon() : base(eLandmarkType.GEMDUNGEON)
        {
            Initailize();
        }

        public void Initailize()
        {
            if (FloorDatas == null)
                FloorDatas = new();
            else
                FloorDatas.Clear();
            if (DragonDatas == null)
                DragonDatas = new();
            else
                DragonDatas.Clear();
        }

        #region SetData
        public override void SetData(JToken jsonData)
        {
            base.SetData(jsonData);

            //GemdungeonData
            SetDataState(jsonData);
            SetDataLevel(jsonData);
            if (SBFunc.IsJTokenCheck(jsonData["data"]))
            {
                JObject data = null;
                if (jsonData["data"].Type == JTokenType.Object)
                    data = (JObject)jsonData["data"];

                if (data != null && data.ContainsKey("flr"))
                {
                    SetDataFloors(data["flr"]);
                    if (data.ContainsKey("fdr"))
                    {
                        JObject fdr = (JObject)data["fdr"];
                        SetBlackSmith(fdr);
                    }
                }
                else
                {
                    //하위호환
                    SetDataFloors(jsonData["data"]);
                }
            }

            //if(SBFunc.IsJTokenCheck(jsonData["chipset_smith"]))
            //{
            //    NanochipsetSmithOpened = jsonData["chipset_smith"].Value<int>() > 0;
            //}
            //if (SBFunc.IsJTokenCheck(jsonData["goldgem_smith"]))
            //{
            //    GoldGemSmithOpened = jsonData["goldgem_smith"].Value<int>() > 0;
            //}

            LandmarkUpdateEvent.Send(eLandmarkType.GEMDUNGEON);
        }

        public void SetBlackSmith(JObject fdr)
        {
            NanochipsetSmithOpened = fdr["nc"].Value<int>() > 0;
            GoldGemSmithOpened = fdr["gg"].Value<int>() > 0;
        }
        private void SetDataState(JToken jsonData)
        {
            if (SBFunc.IsJTokenType(jsonData["state"], JTokenType.Integer))
            {
                State = (eBuildingState)jsonData["state"].Value<int>();
            }
        }
        private void SetDataLevel(JToken jsonData)
        {
            if (SBFunc.IsJTokenType(jsonData["level"], JTokenType.Integer))
            {
                Level = jsonData["level"].Value<int>();
            }
        }
        private void SetDataFloors(JToken jsonData)
        {
            if (null != jsonData && jsonData.Type == JTokenType.Array)
            {
                JArray array = (JArray)jsonData;
                for (int i = 0, count = array.Count; i < count; ++i)
                {
                    var floor = LandmarkGemDungeonFloor.ParseFloor(array[i]);
                    if (floor == 0)
                        continue;
                    if (false == FloorDatas.TryGetValue(floor, out var floorData))
                    {
                        floorData = new LandmarkGemDungeonFloor();
                        FloorDatas.Add(floor, floorData);
                    }
                    floorData.SetData(array[i]);

                    for(int j = 0, jCount = floorData.Dragons.Count; j < jCount; ++j)
                    {
                        var dragon = GetDragonData(floorData.Dragons[j]);
                        if (dragon != null)
                            dragon.SetFloor(floorData.Floor);
                    }
                }
            }
        }
        #endregion
        #region GetData
        public LandmarkGemDungeonFloor GetFloorData(int floor)
        {
            if (FloorDatas == null)
                return null;

            if(FloorDatas.TryGetValue(floor, out LandmarkGemDungeonFloor data))
                return data;

            return null;
        }
        public LandmarkGemDungeonDragon GetDragonData(int dragonNo)
        {
            if (DragonDatas == null)
                return null;

            if (false == DragonDatas.TryGetValue(dragonNo, out LandmarkGemDungeonDragon data))
            {
                data = new LandmarkGemDungeonDragon();
                data.SetData(0, User.Instance.DragonData.GetDragon(dragonNo));
                DragonDatas.Add(dragonNo, data);
            }
            return data;
        }
        #endregion
    }
}
