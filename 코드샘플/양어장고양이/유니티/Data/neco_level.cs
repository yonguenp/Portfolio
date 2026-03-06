using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_level : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_LEVEL; }

    static public neco_level GetNecoLevelData(string type, uint level)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_LEVEL);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_level levelData in necoData)
        {
            if (levelData.data.TryGetValue("type", out obj))
            {
                if (type == (string)obj)
                {
                    if (levelData.data.TryGetValue("level", out obj))
                    {
                        if (level == (uint)obj)
                        {
                            return levelData;
                        }
                    }
                }
            }
        }

        return null;
    }

    static public neco_level GetNecoLevelDataByObjectID(uint objectID, uint level)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_LEVEL);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_level levelData in necoData)
        {
            if (levelData.data.TryGetValue("object_id", out obj))
            {
                if (objectID == (uint)obj)
                {
                    if (levelData.data.TryGetValue("level", out obj))
                    {
                        if (level == (uint)obj)
                        {
                            return levelData;
                        }
                    }
                }
            }
        }

        return null;
    }

    [NonSerialized]
    private uint necoLevelID = 0;
    public uint GetNecoLevelID()
    {
        if (necoLevelID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoLevelID = (uint)obj;
            }
        }

        return necoLevelID;
    }

    [NonSerialized]
    private string necoLevelType = "";
    public string GetNecoLevelType()
    {
        if (necoLevelType == "")
        {
            object obj;
            if (data.TryGetValue("type", out obj))
            {
                necoLevelType = (string)obj;
            }
        }

        return necoLevelType;
    }

    [NonSerialized]
    private uint necoLevelObjectID = 0;
    public uint GetNecoLevelObjectID()
    {
        if (necoLevelObjectID == 0)
        {
            object obj;
            if (data.TryGetValue("object_id", out obj))
            {
                necoLevelObjectID = (uint)obj;
            }
        }

        return necoLevelObjectID;
    }

    [NonSerialized]
    private uint necoLevel = 0;
    public uint GetNecoLevel()
    {
        if (necoLevel == 0)
        {
            object obj;
            if (data.TryGetValue("level", out obj))
            {
                necoLevel = (uint)obj;
            }
        }

        return necoLevel;
    }

    [NonSerialized]
    private uint necoNeedGold = 0;
    public uint GetNecoNeedGold()
    {
        if (necoNeedGold == 0)
        {
            object obj;
            if (data.TryGetValue("need_gold", out obj))
            {
                necoNeedGold = (uint)obj;
            }
        }

        return necoNeedGold;
    }

    [NonSerialized]
    private uint necoNeedCatnip = 0;
    public uint GetNecoNeedCatnip()
    {
        if (necoNeedCatnip == 0)
        {
            object obj;
            if (data.TryGetValue("need_catnip", out obj))
            {
                necoNeedCatnip = (uint)obj;
            }
        }

        return necoNeedCatnip;
    }

    [NonSerialized]
    private uint necoLevelValue1 = 0;
    public uint GetNecoLevelValue1()
    {
        if (necoLevelValue1 == 0)
        {
            object obj;
            if (data.TryGetValue("value1", out obj))
            {
                necoLevelValue1 = (uint)obj;
            }
        }

        return necoLevelValue1;
    }

    [NonSerialized]
    private uint necoLevelValue2 = 0;
    public uint GetNecoLevelValue2()
    {
        if (necoLevelValue2 == 0)
        {
            object obj;
            if (data.TryGetValue("value2", out obj))
            {
                necoLevelValue2 = (uint)obj;
            }
        }

        return necoLevelValue2;
    }

    [NonSerialized]
    private uint tic = 0;
    public uint GetNecoLevelTick()
    {
        if (tic == 0)
        {
            object obj;
            if (data.TryGetValue("tic", out obj))
            {
                tic = (uint)obj;
            }
        }

        return tic;
    }

    public uint GetMaterialInfo(string searchMatTypeName)
    {
        uint matInfo = 0;

        object obj;
        if (data.TryGetValue(searchMatTypeName, out obj))
        {
            matInfo = (uint)obj;
        }
        

        return matInfo;
    }

    public List<KeyValuePair<uint, uint>> GetMaterialList()
    {
        const int MATERIAL_COUNT = 6;

        List<KeyValuePair<uint, uint>> matList = new List<KeyValuePair<uint, uint>>();

        string findMatType = string.Empty;
        string findMatCountType = string.Empty;

        uint matID = 0;
        uint matCount = 0;
        for (int i = 1; i <= MATERIAL_COUNT; ++i)
        {
            findMatType = string.Format("material{0}", i);
            matID = GetMaterialInfo(findMatType);

            findMatCountType = string.Format("material{0}_count", i);
            matCount = GetMaterialInfo(findMatCountType);

            matList.Add(new KeyValuePair<uint, uint>(matID, matCount));
        }

        return matList;
    }
}
