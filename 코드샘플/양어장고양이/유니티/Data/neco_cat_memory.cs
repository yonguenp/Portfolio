using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_cat_memory : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_CAT_MEMORY; }

    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_MEMORY);
        if (necoData == null)
        {
            return;
        }

        foreach (neco_cat_memory data in necoData)
        {
            data.necoMemeoryTitle = "";
        }
    }

    static public neco_cat_memory GetNecoMemory(uint memoryID)
    {
        List<game_data> memorylist = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_MEMORY);
        if (memorylist == null)
            return null;

        foreach (neco_cat_memory data in memorylist)
        {
            if (data.GetNecoMemoryID() == memoryID)
            {
                return data;
            }
        }

        return null;
    }

    static public List<neco_cat_memory> GetNecoMemoryByCatID(uint neco_id)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_MEMORY);
        if (necoData == null)
        {
            return null;
        }

        List<neco_cat_memory> memory_list = new List<neco_cat_memory>();

        object obj;
        foreach (neco_cat_memory memoryData in necoData)
        {
            if (memoryData.data.TryGetValue("cat_id", out obj))
            {
                if ((uint)obj == neco_id)
                {
                    memory_list.Add(memoryData);
                }
            }
        }

        return memory_list;
    }

    static public List<neco_cat_memory> GetNecoMemoryByType(uint neco_id, string type)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_MEMORY);
        if (necoData == null)
        {
            return null;
        }

        List<neco_cat_memory> memory_list = new List<neco_cat_memory>();

        object obj;
        foreach (neco_cat_memory memoryData in necoData)
        {
            if (memoryData.data.TryGetValue("cat_id", out obj))
            {
                if ((uint)obj == neco_id)
                {
                    if (memoryData.data.TryGetValue("type", out obj))
                    {
                        if ((string)obj == type)
                        {
                            memory_list.Add(memoryData);
                        }
                    }
                }
            }
        }

        return memory_list;
    }

    static public int GetNecoMemoryCount(uint neco_id)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_MEMORY);
        if (necoData == null)
        {
            return 0;
        }

        int ret = 0;

        object obj;
        foreach (neco_cat_memory memoryData in necoData)
        {
            if (memoryData.data.TryGetValue("cat_id", out obj))
            {
                if ((uint)obj == neco_id)
                {
                    ret++;
                }
            }
        }

        return ret;
    }

    static public List<neco_cat_memory> GetNecoMemoryPhotoOnly(uint neco_id)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_MEMORY);
        if (necoData == null)
        {
            return null;
        }

        List<neco_cat_memory> memory_list = new List<neco_cat_memory>();

        object obj;
        foreach (neco_cat_memory memoryData in necoData)
        {
            if (memoryData.data.TryGetValue("cat_id", out obj))
            {
                if ((uint)obj == neco_id)
                {
                    if (memoryData.data.TryGetValue("type", out obj))
                    {
                        if ((string)obj == "photo" || (string)obj == "ani")
                        {
                            memory_list.Add(memoryData);
                        }
                    }
                }
            }
        }

        return memory_list;
    }

    static public List<neco_cat_memory> GetNecoMemoryMovieOnly(uint neco_id)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT_MEMORY);
        if (necoData == null)
        {
            return null;
        }

        List<neco_cat_memory> memory_list = new List<neco_cat_memory>();

        object obj;
        foreach (neco_cat_memory memoryData in necoData)
        {
            if (memoryData.data.TryGetValue("cat_id", out obj))
            {
                if ((uint)obj == neco_id)
                {
                    if (memoryData.data.TryGetValue("type", out obj))
                    {
                        if ((string)obj == "movie")
                        {
                            memory_list.Add(memoryData);
                        }
                    }
                }
            }
        }

        return memory_list;
    }

    [NonSerialized]
    private uint necoMemoryID = 0;
    public uint GetNecoMemoryID()
    {
        if (necoMemoryID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoMemoryID = (uint)obj;
            }
        }

        return necoMemoryID;
    }

    [NonSerialized]
    private uint necoMemoryCatID = 0;
    public uint GetNecoMemoryCatID()
    {
        if (necoMemoryCatID == 0)
        {
            object obj;
            if (data.TryGetValue("cat_id", out obj))
            {
                necoMemoryCatID = (uint)obj;
            }
        }

        return necoMemoryCatID;
    }

    [NonSerialized]
    private string necoMemoryType = "";
    public string GetNecoMemoryType()
    {
        if (necoMemoryType == "")
        {
            object obj;
            if (data.TryGetValue("type", out obj))
            {
                necoMemoryType = (string)obj;
            }
        }

        return necoMemoryType;
    }

    [NonSerialized]
    private string necoMemoryThumbnail = "";
    public string GetNecoMemoryThumbnail()
    {
        if (necoMemoryThumbnail == "")
        {
            object obj;
            if (data.TryGetValue("thumbnail", out obj))
            {
                necoMemoryThumbnail = (string)obj;
            }
        }

        return necoMemoryThumbnail;
    }

    [NonSerialized]
    private string source = "";
    public string GetNecoMemorySource()
    {
        if (string.IsNullOrEmpty(source))
        {
            object obj;
            if (data.TryGetValue("source", out obj))
            {
                source = (string)obj;
            }
        }

        return source;
    }

    [NonSerialized]
    private string necoMemeoryTitle = "";
    public string GetNecoMemoryTitle()
    {
        if (string.IsNullOrEmpty(necoMemeoryTitle))
        {
            necoMemeoryTitle = LocalizeData.GetText("neco_cat_memory:title:" + GetNecoMemoryID().ToString());
        }

        return necoMemeoryTitle;
    }

    public int GetTotalNecoMemroyCountByType(string type)
    {
        List<neco_cat_memory> memoryList = new List<neco_cat_memory>();

        if (type == "photo" || type == "ani")
        {
            memoryList = GetNecoMemoryPhotoOnly(GetNecoMemoryCatID());
        }
        else if (type == "movie")
        {
            memoryList = GetNecoMemoryMovieOnly(GetNecoMemoryCatID());
        }
        

        return memoryList.Count;
    }
}
