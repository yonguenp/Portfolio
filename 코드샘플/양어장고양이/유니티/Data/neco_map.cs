using System;
using System.Collections.Generic;

[Serializable]
public class neco_map : game_data
{
    static public void TmpNecoMapData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MAP);
        if (necoData == null)
        {
            GameDataManager.Instance.SetGameDataArray(GameDataManager.DATA_TYPE.NECO_MAP);
            necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MAP);
        }

        necoData.Clear();

        uint[] mapList = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        for (uint i = 0; i < mapList.Length; i++)
        {
            neco_map map = new neco_map();
            map.id = mapList[i];
            map.spots = new List<neco_spot>();

            uint foodSpotID = 100 + mapList[i];
            neco_spot.AddDebugNecoSpot(foodSpotID, neco_spot.SPOT_TYPE.FOOD_SPOT, map);
            map.spots.Add(neco_spot.GetNecoSpot(foodSpotID));

            List<uint> spotIDList = neco_object_maps.GetSpotIDListInMap(mapList[i]);
            foreach(uint spotID in spotIDList)
            {
                neco_spot.AddDebugNecoSpot(spotID, neco_spot.SPOT_TYPE.OBJECT_SPOT, map);
                map.spots.Add(neco_spot.GetNecoSpot(spotID));
            }
            
            necoData.Add(map);
        }
    }

    static public neco_map GetNecoMap(uint mapID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MAP);
        if (necoData != null)
        {
            foreach (neco_map data in necoData)
            {
                if (data != null && data.GetMapID() == mapID)
                {
                    return data;
                }
            }
        }

        return null;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_MAP; }

    [NonSerialized]
    uint id = 0;

    [NonSerialized]
    List<neco_spot> spots = null;

    [NonSerialized]
    neco_spot foodspot = null;
    public uint GetMapID() { return id; }
    public List<neco_spot> GetSpots() { return spots; }

    public neco_spot GetFoodSpot()
    {
        if (foodspot == null)
        {
            foreach(neco_spot spot in spots)
            {
                if(spot.GetSpotType() == neco_spot.SPOT_TYPE.FOOD_SPOT)
                {
                    foodspot = spot;
                }
            }
        }

        return foodspot;
    }

    public bool IsOpened()
    {
        if(GetMapID() == 8 || GetMapID() == 7)
        {
            foreach(neco_spot spot in GetSpots())
            {
                if (spot.GetCurItem() != null)
                    return true;
            }

            return false;
        }

        return true;
    }
}

