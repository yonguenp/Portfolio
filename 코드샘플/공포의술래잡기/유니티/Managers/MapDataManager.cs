using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using System.Text;

public class MapDataManager
{
    static public bool SaveMapData(string path, object mapData)
    {
        //var jo = JObject.FromObject(mapData);        
        //File.WriteAllText(path, jo.ToString());
        File.WriteAllText(path, JsonConvert.SerializeObject(mapData, Formatting.Indented));
        //var strJson = JsonUtility.ToJson(mapData);
        return true;
    }

    static public MapData LoadMapDataJson(string fileName)
    {
        string path = new StringBuilder().AppendFormat("Data/Map/{0}", fileName).ToString();
        var textData = Resources.Load(path) as TextAsset;
        SBDebug.Log($"LoadMapDataJson]{textData.text}");
        MapData md = JsonConvert.DeserializeObject<MapData>(textData.text);
        //JObject json = (JObject)JToken.Parse(textData.text);
        //MapData md = json.ToObject<MapData>();
        return md;
    }
}
