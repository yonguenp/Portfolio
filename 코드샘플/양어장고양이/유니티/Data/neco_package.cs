using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_package : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_PACKAGE; }

    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PACKAGE);
        if (necoData == null)
        {
            return;
        }

        foreach (neco_package data in necoData)
        {
            data.necoPackgeDesc = "";
        }
    }

    static public neco_package GetNecoPackageByID(uint packageID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PACKAGE);
        if (necoData == null)
        {
            return null;
        }

        object obj;
        foreach (neco_package packageData in necoData)
        {
            if (packageData.data.TryGetValue("package_id", out obj))
            {
                if (packageID == (uint)obj)
                {
                    return packageData;
                }
            }
        }

        return null;
    }

    static public List<neco_package> GetNecoPackageListByID(uint packageID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PACKAGE);
        if (necoData == null)
        {
            return null;
        }

        List<neco_package> packageList = new List<neco_package>();

        object obj;
        foreach (neco_package packageData in necoData)
        {
            if (packageData.data.TryGetValue("package_id", out obj))
            {
                if (packageID == (uint)obj)
                {
                    packageList.Add(packageData);
                }
            }
        }

        return packageList;
    }

    [NonSerialized]
    private uint necoPackageID = 0;
    public uint GetNecoPackageID()
    {
        if (necoPackageID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoPackageID = (uint)obj;
            }
        }

        return necoPackageID;
    }

    [NonSerialized]
    private string necoPackgeDesc = "";
    public string GetNecoPackageDesc()
    {
        if (necoPackgeDesc == "")
        {
            necoPackgeDesc = LocalizeData.GetText("neco_package:package_desc:" + GetNecoPackageID().ToString());
        }

        return necoPackgeDesc;
    }

    [NonSerialized]
    private uint necoPackageItemID = 0;
    public uint GetNecoPackageItemID()
    {
        if (necoPackageItemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                necoPackageItemID = (uint)obj;
            }
        }

        return necoPackageItemID;
    }

    [NonSerialized]
    private uint necoPackageCount = 0;
    public uint GetNecoPackageCount()
    {
        if (necoPackageCount == 0)
        {
            object obj;
            if (data.TryGetValue("count", out obj))
            {
                necoPackageCount = (uint)obj;
            }
        }

        return necoPackageCount;
    }

    [NonSerialized]
    private string necoPackageIType = "";
    public string GetNecoPackageType()
    {
        if (necoPackageIType == "")
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                necoPackageIType = (string)obj;
            }
        }

        return necoPackageIType;
    }

    public bool IsIncludeBoxItem(uint packageID)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_PACKAGE);
        if (necoData == null)
        {
            return false;
        }

        List<neco_package> packageList = new List<neco_package>();

        object obj;
        foreach (neco_package packageData in necoData)
        {
            if (packageData.data.TryGetValue("package_id", out obj))
            {
                if (packageID == (uint)obj)
                {
                    packageList.Add(packageData);
                }
            }
        }

        foreach (neco_package packageData in packageList)
        {
            if (packageData.GetNecoPackageType() == "item")
            {
                items itemData = items.GetItem(packageData.GetNecoPackageItemID());

                if (itemData?.GetItemType() == "BOX")
                {
                    return true;
                }
            }
        }

        return false;
    }
}
