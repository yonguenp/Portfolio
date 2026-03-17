using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_user_cat : game_data
{
    static public void UpdateUserCatInfo(JObject data)
    {
        if (data.ContainsKey("id"))
        {
            uint id = data["id"].Value<uint>();
            neco_user_cat uc = neco_user_cat.GetUserCatInfo(id);
            if(uc == null)
            {
                List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_USER_CAT);
                if (necoData != null)
                {
                    uc = new neco_user_cat();
                    uc.data["id"] = id;
                    necoData.Add(uc);
                    UINotifiedManager.UpdateData("cat_" + id.ToString());
                }
            }

            List<string> keys = new List<string>();
            keys.Add("obtain");
            keys.Add("memory");            
            keys.Add("visits");
            keys.Add("fav_food");
            keys.Add("fav_toy");
            keys.Add("map");
            keys.Add("state");

            foreach (string key in keys)
            {
                if (data.ContainsKey(key))
                {
                    uc.data[key] = data[key].Value<uint>();
                }
            }

            if(data.ContainsKey("rewards"))
                uc.data["rewards"] = data["rewards"].Value<string>();

            uc.memories.Clear();
            if (data.ContainsKey("mem"))
            {
                JArray array = (JArray)data["mem"];
                foreach (JToken m in array)
                {
                    uint memoryID = m.Value<uint>();
                    uc.memories.Add(memoryID);

                    string newMemoryKey = string.Format("{0}_{1}", SamandaLauncher.GetAccountNo(), memoryID);
                    if (PlayerPrefs.HasKey(newMemoryKey) == false)
                    {
                        PlayerPrefs.SetInt(newMemoryKey, 0);
                    }
                }

                NecoCanvas.GetUICanvas()?.UpdateCatListAlarm();
                NecoCanvas.GetUICanvas()?.UpdateMainMenuRedDot();
            }

            if(data.ContainsKey("state"))
            {
                if(data["state"].Value<uint>() == 1)
                {
                    NecoCanvas.GetUICanvas()?.RefreshNewCatAlarm();
                }

                NecoCanvas.GetUICanvas()?.RefreshMainMenuRedDot();
            }
        }
    }

    static public neco_user_cat GetUserCatInfo(uint catID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_USER_CAT);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_user_cat cat in necoData)
        {
            if(cat.GetCatID() == catID)
            {
                return cat;
            }
        }

        return null;
    }

    static public List<neco_user_cat> GetGainUserCatList()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_USER_CAT);
        if (necoData == null)
        {
            return null;
        }

        List<neco_user_cat> resultList = new List<neco_user_cat>();

        foreach (neco_user_cat cat in necoData)
        {
            if (cat.GetState() == 3)
            {
                resultList.Add(cat);
            }
        }

        return resultList;
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_USER_CAT; }

    [NonSerialized]
    uint catID = 0;
    [NonSerialized]
    DateTime obtainDate = DateTime.MinValue;

    public uint GetCatID()
    {
        if (catID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                catID = (uint)obj;
            }
        }

        return catID;
    }

    public DateTime GetObtainDate()
    {
        if (obtainDate == DateTime.MinValue)
        {
            object obj;
            if (data.TryGetValue("obtain", out obj))
            {
                DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                obtainDate = startTime.AddSeconds((uint)obj);
            }
        }

        return obtainDate;
    }

    public uint GetMemoryCount()
    {
        return (uint)memories.Count;
    }

    public uint GetPhotoMemoryCount()
    {
        uint count = 0;

        foreach (uint memoryID in memories)
        {
            neco_cat_memory mem = neco_cat_memory.GetNecoMemory(memoryID);
            if (mem != null)
            {
                string memoryType = mem.GetNecoMemoryType();

                if (memoryType == "photo" || memoryType == "ani")
                {
                    count++;
                }
            }
        }    

        return count;
    }

    public uint GetMovieMemoryCount()
    {
        uint count = 0;

        foreach (uint memoryID in memories)
        {
            neco_cat_memory mem = neco_cat_memory.GetNecoMemory(memoryID);
            if (mem != null)
            {
                string memoryType = mem.GetNecoMemoryType();

                if (memoryType == "movie")
                {
                    count++;
                }
            }
        }

        return count;
    }

    [NonSerialized]
    List<uint> memories = new List<uint>();
    public List<uint> GetMemories()
    {
        return memories;
    }

    public uint GetVisitCount()
    {
        object obj;
        if (data.TryGetValue("visits", out obj))
        {
            return (uint)obj;
        }

        return 0;
    }

    public bool IsDiscoverFavoriteFood()
    {
        object obj;
        if (data.TryGetValue("fav_food", out obj))
        {
            return (uint)obj != 0;
        }

        return false;
    }

    public bool IsDiscoverFavoriteObject()
    {
        object obj;
        if (data.TryGetValue("fav_toy", out obj))
        {
            return (uint)obj != 0;
        }

        return false;
    }

    public items GetGavenItem(int index)
    {
        object obj;
        if (data.TryGetValue("rewards", out obj))
        {
            string item = index.ToString();
            string rewards = (string)obj;

            if (rewards == "")
            {
                return null;
            }

            string[] reward = rewards.Split(',');

            if(reward.Length > index && reward[index] != "")
            {
                return items.GetItem(uint.Parse(reward[index]));
            }
        }

        return null;
    }

    public uint CurMap()
    {
        object obj;
        if (data.TryGetValue("map", out obj))
        {
            return (uint)obj;
        }

        return 0;
    }

    public uint GetState()
    {
        object obj;
        if (data.TryGetValue("state", out obj))
        {
            return (uint)obj;
        }

        return 0;
    }
}

