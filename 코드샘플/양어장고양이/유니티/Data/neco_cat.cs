using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_cat : game_data
{
    public enum CAT_VISIT_STATE
    {
        NONE = 0,
        VISIT = 1
    };

    public enum CAT_SUDDEN_STATE
    {
        NONE = 0,
        TOUCH = 1,
        WATCH = 2,
        PHOTO = 3,
    };

    static public void AddDebugNecoCat(uint catID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);
        if(necoData == null)
        {
            GameDataManager.Instance.SetGameDataArray(GameDataManager.DATA_TYPE.NECO_CAT);
            necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);
        }

        neco_cat cat = new neco_cat();
        cat.neco_id = catID;

        necoData.Add(cat);
    }

    static public neco_cat GetNecoCat(uint catNo)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);
        foreach (neco_cat data in necoData)
        {
            if (data != null && data.GetCatID() == catNo)
            {
                return data;
            }
        }

        return null;
    }

    static public int GetTotalNecoCatCount()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);

        if (necoData == null)
        {
            return 0;
        }

        return necoData.Count;
    }
    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);
        if (necoData == null)
        {
            return;
        }

        foreach (neco_cat data in necoData)
        {
            data.neco_name = "";            
        }
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_CAT; }

    [NonSerialized]
    uint neco_id = 0;

    [NonSerialized]
    neco_spot cur_spot = null;

    [NonSerialized]
    string neco_name = "";
    
    [NonSerialized]
    bool favoriteAte = false;

    [NonSerialized]
    bool isActionEnable = false;

    [NonSerialized]
    uint sudden_ratio = 0;

    [NonSerialized]
    uint like_food = 0;

    [NonSerialized]
    uint like_object = 0;

    [NonSerialized]
    string icon_path = "";

    [NonSerialized]
    neco_user_cat user_info = null;

    [NonSerialized]
    CAT_VISIT_STATE visitState = CAT_VISIT_STATE.NONE;

    [NonSerialized]
    uint outTime = 0;

    [NonSerialized]
    CAT_SUDDEN_STATE sudden = CAT_SUDDEN_STATE.NONE;

    [NonSerialized]
    uint param = 0;

    [NonSerialized]
    GameObject silhouette = null;

    [NonSerialized]
    uint firstMemory = 0;

    public uint GetCatID() {
        if (neco_id == 0)
        {
            object obj;
            if (data.TryGetValue("neco_id", out obj))
            {
                neco_id = (uint)obj;
            }
        }

        return neco_id; 
    }

    public bool IsOnSpot()
    {
        return cur_spot != null;
    }

    public void OnSpot(neco_spot spot)
    {
        cur_spot = spot;
        if (cur_spot != null)
        {
            OnNecoVisit();
        }

        if(NecoCanvas.GetGameCanvas() != null)
            NecoCanvas.GetGameCanvas().RefreshCat(this);
    }

    public void OnSpotGone()
    {
        cur_spot = null;

        NecoCanvas.GetGameCanvas().RefreshCat(this);
    }

    public neco_spot GetSpot()
    {
        return cur_spot;
    }

    public string GetCatName()
    {
        if (string.IsNullOrEmpty(neco_name))
        {
            neco_name = LocalizeData.GetText("neco_cat:neco_name:" + GetCatID().ToString());
        }

        return neco_name;
    }

    public items GetCatGift(int index)
    {
        if (GetUserInfo() == null)
            return null;

        return GetUserInfo().GetGavenItem(index);
    }

    public bool IsGainCat()
    {
        if (GetCatState() >= 2)
            return true;

        return false;
    }

    public bool IsReadyCat()
    {
        if (GetCatState() == 1)
            return true;

        return false;
    }

    public uint GetCatState()
    {
        neco_user_cat info = GetUserInfo();
        if (info == null)
            return 0;

        return info.GetState();
    }

    public void OnNecoVisit()
    {
        if (GetUserInfo() != null)
        {
            //neco_user_cat user_data = GetUserInfo();
            //user_data.data["visits"] = (uint)(((uint)user_data.data["visits"]) + 1);
        }        
    }

    public uint GetFirstMemory()
    {
        if (firstMemory == 0)
        {
            object obj;
            if (data.TryGetValue("first_memory", out obj))
            {
                firstMemory = (uint)obj;
            }
        }

        return firstMemory;
    }

    public uint GetNecoVisitCount()
    {
        if (GetUserInfo() == null)
            return 0;

        return GetUserInfo().GetVisitCount();
    }

    public bool IsDiscoverFavoriteFood()
    {
        if (GetUserInfo() == null)
            return false;

        return GetUserInfo().IsDiscoverFavoriteFood();
    }

    public bool IsDiscoverFavoriteObject()
    {
        if (GetUserInfo() == null)
            return false;

        return GetUserInfo().IsDiscoverFavoriteObject();
    }

    //삭제예정
    public DateTime DELETE_DEBUG_DATA_GetLastVisitTime()
    {
        return DateTime.Parse(PlayerPrefs.GetString("NecoVisit_" + GetCatID(), DateTime.MinValue.ToString()));
    }

    public DateTime GetObtainDate()
    {
        if (GetUserInfo() == null)
            return DateTime.MinValue;

        return GetUserInfo().GetObtainDate();
    }

    public uint GetMemoryCount()
    {
        if (GetUserInfo() == null)
            return 0;

        return GetUserInfo().GetMemoryCount();
    }

    public bool IsActionEnable()
    {
        return isActionEnable;
    }

    public void SetActionDisable()
    {
        isActionEnable = false;
    }

    public uint GetSuddenRatio()
    {
        if (sudden_ratio == 0)
        {
            object obj;
            if (data.TryGetValue("sudden_ratio", out obj))
            {
                sudden_ratio = (uint)obj;
            }
        }

        return sudden_ratio;
    }

    public uint GetLikeFood()
    {
        if (like_food == 0)
        {
            object obj;
            if (data.TryGetValue("like_food", out obj))
            {
                like_food = (uint)obj;
            }
        }

        if (!IsDiscoverFavoriteFood())
            return 0;

        return like_food;
    }

    public uint GetLikeObject()
    {
        //if (like_object == 0)
        //{
        //    object obj;
        //    if (data.TryGetValue("like_object", out obj))
        //    {
        //        like_object = (uint)obj;
        //    }
        //}

        //if (!IsDiscoverFavoriteObject())
        //    return 0;

        //return like_object;
        if (like_object == 124 && user_items.GetUserItemAmount(154) > 0)
        {
            like_object = 154;
        }
        else if (like_object == 109 && user_items.GetUserItemAmount(157) > 0)
        {
            like_object = 157;
        }
        else if(like_object == 154 && user_items.GetUserItemAmount(154) == 0)
        {
            like_object = 124;
        }
        else if (like_object == 157 && user_items.GetUserItemAmount(157) == 0)
        {
            like_object = 109;
        }
        return like_object;
    }

    public void SetLikeObject(uint item)
    {
        like_object = item;
    }

    public string GetIconPath()
    {
        if (string.IsNullOrEmpty(icon_path))
        {
            object obj;
            if (data.TryGetValue("icon", out obj))
            {
                icon_path = (string)obj;
            }
        }

        return icon_path;
    }

    private neco_user_cat GetUserInfo()
    {
        if (user_info == null)
            user_info = neco_user_cat.GetUserCatInfo(GetCatID());
        
        return user_info;
    }
    
    public CAT_VISIT_STATE GetVisitState()
    {
        return visitState;
    }

    public uint GetVisitParam()
    {
        return param;
    }

    public CAT_SUDDEN_STATE GetSuddenType()
    {
        return sudden;
    }

    public void ClearSuddenEvent()
    {
        sudden = CAT_SUDDEN_STATE.NONE;
    }

    public void SetVisitState(CAT_VISIT_STATE state, CAT_SUDDEN_STATE _sudden = CAT_SUDDEN_STATE.NONE, neco_spot spot = null, uint _outTime = 0, uint _param = 0)
    {
        if (visitState == state && sudden == _sudden && cur_spot == spot && outTime == _outTime && _param == param)
            return;

        visitState = state;
        
        outTime = _outTime;
        param = _param;
        sudden = _sudden;

        if (spot != null)
            spot.Refresh();
        if (cur_spot != null)
            cur_spot.Refresh();

        cur_spot = spot;

        uint curTIme = NecoCanvas.GetCurTime();
        if (outTime > curTIme)
        {
            uint remain = outTime - curTIme;
            updateRemainTime = remain + 60;
            if (updateRemainTime > 60 * 5)
                updateRemainTime = 300;
        }
    }

    public uint GetLeaveTime()
    {
        return outTime; 
    }

    
    [NonSerialized]
    List<neco_map> ableMap = new List<neco_map>();            
    public List<neco_map> GetAbleMapList()
    {
        if (ableMap == null)
            return null;

        if (ableMap.Count == 0)
        {
            switch (neco_id)
            {
                case 1:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(2));
                    ableMap.Add(neco_map.GetNecoMap(3));
                    ableMap.Add(neco_map.GetNecoMap(10));
                    break;
                case 2:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(2));
                    ableMap.Add(neco_map.GetNecoMap(3));
                    ableMap.Add(neco_map.GetNecoMap(7));
                    ableMap.Add(neco_map.GetNecoMap(10));
                    break;
                case 3:
                    ableMap.Add(neco_map.GetNecoMap(2));
                    ableMap.Add(neco_map.GetNecoMap(3));
                    ableMap.Add(neco_map.GetNecoMap(5));
                    ableMap.Add(neco_map.GetNecoMap(6));
                    ableMap.Add(neco_map.GetNecoMap(8));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    ableMap.Add(neco_map.GetNecoMap(10));
                    break;
                case 4:                    
                    break;
                case 5:
                    ableMap.Add(neco_map.GetNecoMap(3));
                    ableMap.Add(neco_map.GetNecoMap(4));
                    ableMap.Add(neco_map.GetNecoMap(5));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    ableMap.Add(neco_map.GetNecoMap(10));
                    break;
                case 6:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(2));
                    ableMap.Add(neco_map.GetNecoMap(3));
                    ableMap.Add(neco_map.GetNecoMap(4));
                    ableMap.Add(neco_map.GetNecoMap(6));
                    ableMap.Add(neco_map.GetNecoMap(7));
                    break;
                case 7:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(4));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    ableMap.Add(neco_map.GetNecoMap(10));
                    break;
                case 8:
                    ableMap.Add(neco_map.GetNecoMap(2));                    
                    ableMap.Add(neco_map.GetNecoMap(5));
                    ableMap.Add(neco_map.GetNecoMap(7));
                    ableMap.Add(neco_map.GetNecoMap(8));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    ableMap.Add(neco_map.GetNecoMap(10));
                    break;
                case 9:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(2));
                    ableMap.Add(neco_map.GetNecoMap(3));
                    ableMap.Add(neco_map.GetNecoMap(5));
                    ableMap.Add(neco_map.GetNecoMap(6));
                    ableMap.Add(neco_map.GetNecoMap(8));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    break;
                case 10:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(5));
                    ableMap.Add(neco_map.GetNecoMap(6));
                    ableMap.Add(neco_map.GetNecoMap(8));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    break;
                case 11:
                    ableMap.Add(neco_map.GetNecoMap(2));
                    ableMap.Add(neco_map.GetNecoMap(4));
                    ableMap.Add(neco_map.GetNecoMap(5));
                    ableMap.Add(neco_map.GetNecoMap(6));
                    ableMap.Add(neco_map.GetNecoMap(7));
                    break;
                case 12:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(3));
                    ableMap.Add(neco_map.GetNecoMap(4));
                    ableMap.Add(neco_map.GetNecoMap(5));
                    ableMap.Add(neco_map.GetNecoMap(7));
                    break;
                case 13:
                    ableMap.Add(neco_map.GetNecoMap(4));
                    ableMap.Add(neco_map.GetNecoMap(5));
                    ableMap.Add(neco_map.GetNecoMap(7));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    break;
                case 14:
                    ableMap.Add(neco_map.GetNecoMap(3));
                    ableMap.Add(neco_map.GetNecoMap(6));
                    ableMap.Add(neco_map.GetNecoMap(7));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    break;
                case 15:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(10));
                    ableMap.Add(neco_map.GetNecoMap(6));
                    break;

                case 16:
                    ableMap.Add(neco_map.GetNecoMap(9));
                    break;

                case 17:
                    ableMap.Add(neco_map.GetNecoMap(4));
                    ableMap.Add(neco_map.GetNecoMap(9));
                    break;

                case 18:
                    ableMap.Add(neco_map.GetNecoMap(1));
                    ableMap.Add(neco_map.GetNecoMap(3));
                    break;

                case 19:
                    ableMap.Add(neco_map.GetNecoMap(5));
                    break;
            }
        }

        return ableMap;
    }

    [NonSerialized]
    public uint updateRemainTime;

    public uint CurMap()
    {
        if (GetUserInfo() == null)
            return 0;

        return GetUserInfo().CurMap();
    }
}



