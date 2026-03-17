using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class neco_spot : game_data
{
    public enum SPOT_TYPE
    {
        UNKNOWN,
        FOOD_SPOT,
        OBJECT_SPOT,
    };

    public enum SPOT_STATE
    {
        UNKNOWN,
        NOTHING,
        OBJECT_SET,
        ON_CAT,
        STATE_MAX,
    };

    static public void AddDebugNecoSpot(uint spotID, SPOT_TYPE type, neco_map map)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
        if (necoData == null)
        {
            GameDataManager.Instance.SetGameDataArray(GameDataManager.DATA_TYPE.NECO_SPOT);
            necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
        }

        neco_spot spot = neco_spot.GetNecoSpot(spotID);
        if (spot == null)
        {
            spot = new neco_spot();
            spot.id = spotID;
            spot.state = SPOT_STATE.NOTHING;
            
            necoData.Add(spot);
        }

        spot.type = type;
        spot.parent_map = map;
    }

    //static public void ClearUpdateTime()
    //{
    //    List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
    //    if(necoData != null)
    //    {
    //        foreach (neco_spot data in necoData)
    //        {
    //            if (data != null)
    //            {
    //                data.nextRequest = 0;
    //            }
    //        }
    //    }
    //}

    static public neco_spot GetNecoSpot(uint spotID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
        if (necoData != null)
        {
            foreach (neco_spot data in necoData)
            {
                if (data != null && data.GetSpotID() == spotID)
                {
                    return data;
                }
            }
        }

        return null;
    }

    static public neco_spot GetNecoSpotObjectByItemID(uint itemID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
        if (necoData != null)
        {
            foreach (neco_spot data in necoData)
            {
                uint curObjectItem = objects.GetSpotItem(data.GetSpotID());
                if (data != null && curObjectItem == itemID)
                {
                    return data;
                }
            }
        }

        return null;
    }

    static public neco_spot AddNecoSpot(uint spotID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
        if(necoData == null)
        {
            necoData = GameDataManager.Instance.GetEmptyGameDataWithType(GameDataManager.DATA_TYPE.NECO_SPOT);
        }

        if (necoData != null)
        {
            neco_spot spot = new neco_spot();
            spot.id = spotID;
            spot.type = spotID >= 100 ? SPOT_TYPE.FOOD_SPOT : SPOT_TYPE.OBJECT_SPOT;
            necoData.Add(spot);

            return spot;
        }

        return null;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_SPOT; }

    [NonSerialized]
    uint id = 0;

    [NonSerialized]
    uint level = 0;

    //[NonSerialized]
    //uint nextRequest = 0;

    //[NonSerialized]
    //uint foodPeriod  = 0;

    [NonSerialized]
    neco_cat[] curCats = new neco_cat[3];

    [NonSerialized]
    SPOT_TYPE type = SPOT_TYPE.UNKNOWN;

    [NonSerialized]
    SPOT_STATE state = SPOT_STATE.UNKNOWN;

    [NonSerialized]
    MapObjectSpot spotUI = null;

    [NonSerialized]
    items curItem = null;

    [NonSerialized]
    uint itemRemainTick = 0;

    [NonSerialized]
    uint itemDurability = 0;

    [NonSerialized]
    neco_map parent_map = null;


    public uint GetSpotID() { return id; }
    public SPOT_TYPE GetSpotType() { return type; }
    public SPOT_STATE GetSpotState() { return state; }

    public void SetUI(MapObjectSpot ui) 
    {
        //test;
        //SetState((SPOT_STATE)Random.Range((int)SPOT_STATE.NOTHING, (int)SPOT_STATE.STATE_MAX));

        spotUI = ui;

        RefreshItem();
        Refresh();
    }

    public MapObjectSpot GetUI()
    {
        return spotUI;
    }

    public void Refresh()
    {
        CheckState();

        if (spotUI != null)
        {
            spotUI.RefreshSpot();
        }
    }

    public void CheckState()
    {
        SPOT_STATE nextState = SPOT_STATE.NOTHING;
        if(GetCurItem() != null)        
        {
            if(GetSpotType() == SPOT_TYPE.FOOD_SPOT)
            {
                if(GetItemRemain() > 0)
                    nextState = SPOT_STATE.OBJECT_SET;
            }
            else
            {
                nextState = SPOT_STATE.OBJECT_SET;
            }


            if (nextState == SPOT_STATE.OBJECT_SET)
            {
                foreach (neco_cat cat in curCats)
                {
                    if (cat != null)
                    {
                        nextState = SPOT_STATE.ON_CAT;
                    }
                }
            }
        }

        foreach(neco_cat cat in curCats)
        {
            if(cat != null)
            {
                cat.OnSpot(this);
            }
        }

        state = nextState;
    }


    public neco_cat GetCurSpotCat(uint index)
    {
        return curCats[index];
    }

    public void GoneSpotCat(uint index)
    {
        if(curCats[index] != null)
            curCats[index].OnSpotGone();

        curCats[index] = null;
        Refresh();
    }

    public items GetCurItem()
    {
        return curItem;
    }

    public void RefreshItem()
    {
        if (GetSpotType() != SPOT_TYPE.FOOD_SPOT && curItem == null)
        {
            if (curItem == null)
            {
                uint curObjectItem = objects.GetSpotItem(GetSpotID());
                List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
                if (user_items == null)
                    return;

                foreach (game_data data in user_items)
                {
                    user_items userItem = (user_items)data;
                    uint userItem_id = userItem.GetItemID();

                    if (curObjectItem == userItem_id)
                    {
                        SetItem(items.GetItem(curObjectItem));
                        Refresh();

                        neco_map map = neco_map.GetNecoMap(1);
                        if (map != null)
                        {
                            neco_spot foodSpot = map.GetFoodSpot();
                            if (foodSpot != this && foodSpot.GetCurItem() != null && foodSpot.GetItemRemain() > 0)
                            {
                                //if (curItem.GetItemID() == 106)
                                //{
                                //    if (!neco_data.ShownFlag.Contains(2230))
                                //    {
                                //        NecoCanvas.GetGameCanvas().CallCat(2, 2, 3, 0);
                                //    }
                                //}

                                //if (curItem.GetItemID() == 109)
                                //{
                                //    if (!neco_data.ShownFlag.Contains(1530))
                                //    {
                                //        NecoCanvas.GetGameCanvas().CallCat(1, 5, 3, 0);
                                //    }
                                //}
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetItem(items item)
    {
        curItem = item;
    }

    public void SetItemRemainTick(uint remain)
    {
        itemRemainTick = remain;
    }

    public uint GetItemRemain()
    {
        if (curItem != null)
        {
            uint curTime = NecoCanvas.GetCurTime(); 

            if(itemRemainTick > curTime)
                return itemRemainTick - curTime;
        }

        return 0;
    }

    public void RefreshCatAppearCheck()
    {
        PlayerPrefs.SetString("AppearCheck_" + GetSpotID(), DateTime.Now.ToString());        
    }

    public DateTime GetCatAppearCheckTime()
    {
        return DateTime.Parse(PlayerPrefs.GetString("AppearCheck_" + GetSpotID(), DateTime.MinValue.ToString()));
    }

    public neco_map GetCurMapData()
    {
        return parent_map;
    }

    public void UpdateData(JObject data)
    {
        level = data["lvl"].Value<uint>();

        if (GetSpotType() == SPOT_TYPE.FOOD_SPOT)
        {
            SetItem(null);
            SetItemRemainTick(0);
            if (data.ContainsKey("food_id"))
            {
                SetItem(items.GetItem(data["food_id"].Value<uint>()));
                if (data.ContainsKey("food_exp"))
                {
                    SetItemRemainTick(data["food_exp"].Value<uint>());
                }
            }
        }

        itemDurability = data["dur"].Value<uint>();

        neco_cat[] newCats = new neco_cat[3];

        if (data.ContainsKey("visit"))
        {
            JArray visits = (JArray)data["visit"];
            foreach(JToken visit in visits)
            {
                string dataString = visit.Value<string>();
                string[] catData = dataString.Split(',');
                
                if(catData.Length == 5)
                {
                    int index = int.Parse(catData[0]);
                    uint catID = uint.Parse(catData[1]);
                    uint state = uint.Parse(catData[2]);
                    uint outTime = uint.Parse(catData[3]);
                    uint param = uint.Parse(catData[4]);

                    neco_cat cat = neco_cat.GetNecoCat(catID);
                    cat.SetVisitState(neco_cat.CAT_VISIT_STATE.VISIT, (neco_cat.CAT_SUDDEN_STATE)state, this, outTime, param);
                    
                    newCats[index - 1] = cat;
                }
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if(curCats[i] != newCats[i])
            {
                if(curCats[i] != null)
                {
                    if(curCats[i].GetSpot() == this)
                    {
                        curCats[i].SetVisitState(neco_cat.CAT_VISIT_STATE.NONE);
                        GoneSpotCat((uint)i);
                    }
                }

                curCats[i] = newCats[i];
            }
        }

        //Debug 
        //if(GetSpotType() == SPOT_TYPE.FOOD_SPOT)
        //{
        //    if (GetItemRemain() > 0)
        //    {
        //        if (curCats[2] == null)
        //        {
        //            neco_cat cat = neco_cat.GetNecoCat(1);
        //            if (cat.IsOnSpot() == false)
        //            {
        //                cat.SetVisitState(neco_cat.CAT_VISIT_STATE.VISIT, this, data["food_exp"].Value<uint>());
        //                curCats[2] = cat;
        //            }
        //        }
        //    }
        //}

        Refresh();
    }

    //public uint GetFoodRemain()
    //{
    //    return foodPeriod;
    //    //uint spendCount = GetItemSpendCount();
    //    //return foodPeriod + (spendCount * CurItemPeriod());
    //}

    //public void SetFoodPeriod(uint tick)
    //{
    //    foodPeriod = tick;
    //}

    public uint GetItemMaxDuration()
    {
        if(curItem != null)
        {
            return neco_food.GetFoodDuration(curItem.GetItemID());
        }

        return 0;
    }

    //public uint GetItemSpendCount()
    //{
    //    uint ret = 0;

    //    int curTime = NetworkManager.GetInstance().ServerTime;
    //    if (NecoCanvas.GetGameCanvas() != null)        
    //    {
    //        curTime = NecoCanvas.GetCurTime();
    //    }
    //    uint nextPeriod = foodPeriod;

    //    while (nextPeriod < curTime)
    //    {
    //        nextPeriod += CurItemPeriod();
    //        ret++;
    //    }

    //    return ret;
    //}

    public uint GetSpotItemDurability()
    {
        switch (GetSpotID())
        {
            case 25:
                return neco_spot.GetNecoSpot(20).GetSpotItemDurability();
            case 26:
                return neco_spot.GetNecoSpot(5).GetSpotItemDurability();
        }

        return itemDurability;
    }

    public uint GetSpotLevel()
    {
        switch(GetSpotID())
        {
            case 25:
                return neco_spot.GetNecoSpot(20).GetSpotLevel();
            case 26:
                return neco_spot.GetNecoSpot(5).GetSpotLevel();
        }

        return level;
    }
}

[Serializable]
public class objects : game_data
{
    static public uint GetSpotItem(uint spotID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.OBJECTS);
        if (necoData != null)
        {
            foreach (objects data in necoData)
            {
                if (data != null)
                {
                    if (data.GetSpotID() == spotID)
                    {
                        return data.GetItemID();
                    }   
                }
            }
        }

        return 0;
    }

    static public objects GetObjectInfo(uint itemID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.OBJECTS);
        if (necoData != null)
        {
            foreach (objects data in necoData)
            {
                if (data != null && data.GetItemID() == itemID)
                {
                    return data;
                }
            }
        }

        return null;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.OBJECTS; }

    [NonSerialized]
    uint spot_id = 0;

    [NonSerialized]
    uint item_id = 0;
    public uint GetSpotID()
    {
        if (spot_id == 0)
        {
            object obj;
            if (data.TryGetValue("object_id", out obj))
            {
                spot_id = (uint)obj;
            }
        }

        return spot_id;
    }

    public uint GetItemID()
    {
        if (item_id == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                item_id = (uint)obj;
            }
        }

        return item_id;
    }

}