using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    private int unique_id = -1;
    protected GameDataManager.DATA_TYPE dataType;
    protected Dictionary<string, string> data;

    public int GetID()
    {
        return unique_id;
    }

    public virtual void SetValue(Dictionary<string, string> tmp)
    {
        data = tmp;

        SetUniqueID();
    }

    public string GetValue(string key)
    {
        if (data.ContainsKey(key))
            return data[key];

        return null;
    }

    protected virtual string GetUniqueKeyName()
    {
        return "uid";
    }

    void SetUniqueID()
    {
        if (!data.ContainsKey(GetUniqueKeyName()))
            return;

        try
        {
            if (string.IsNullOrEmpty(data[GetUniqueKeyName()]))
                unique_id = 0;
            else
                unique_id = int.Parse(data[GetUniqueKeyName()]);
        }
        catch
        {
            unique_id = 0;
#if UNITY_EDITOR
            throw;
#endif
        }
    }

    public void SetDataType(GameDataManager.DATA_TYPE t)
    {
        dataType = t;
    }
    public static List<GameData> LoadGameData(GameDataManager.DATA_TYPE type, string path, char splitPivot = char.MinValue)
    {
        List<GameData> ret = new List<GameData>();
        var strData = GameDataManager.GetSavedGameData(type);

//#if UNITkY_EDITOR
        if (GameDataManager.Instance.UseLocalData())
            strData = FileReader.ReadString("Data/" + type.ToString());
//#endif
        var csvData = CSVReader.ReadWithUniqueKey(strData, splitPivot, GameDataManager.NewGameData(type).GetUniqueKeyName());

        int count = csvData.Count;
        for (int i = 0; i < count; ++i)
        {
            GameData data = GameDataManager.NewGameData(type);
            data.SetValue(csvData[i]);

            ret.Add(data);
        }

        return ret;
    }

    public virtual string GetName()
    {
        return StringManager.Instance.GetName(dataType, unique_id);
    }

    public virtual string GetDesc()
    {
        return StringManager.Instance.GetDesc(dataType, unique_id);
    }

    public string GetDumpString()
    {
        List<string> ret = new List<string>();
        foreach (KeyValuePair<string, string> d in data)
        {
            ret.Add(d.Key + ":" + d.Value);
        }

        return string.Join(",", ret);
    }

    protected int Int(string val)
    {
        if (string.IsNullOrEmpty(val))
            return 0;

        return int.Parse(val);
    }

    protected long Long(string val)
    {
        if (string.IsNullOrEmpty(val))
            return 0;

        return long.Parse(val);
    }
}
