using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTypeGameData : GameData
{
    public enum GAME_MODE
    {
        UNKNOWN,//0
        CHASE_GAME,//1
    }

    public GAME_MODE game_mode { get; private set; }
    public Sprite map_icon { get; private set; } = null;
    public string map_file { get; private set; }
    public string map_resource { get; private set; }
    public bool use { get; private set; }
    public bool use_custom { get; private set; } = false;
    public AudioClip map_sound { get; private set; }

    MapData mapData;
    GameObject mapObject;

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        game_mode = (GAME_MODE)Int(data["game_mod"]);
        map_file = "Map/" + data["map_data_resource"];
        map_resource = "Map/" + data["map_resource"];
        map_icon = Managers.Resource.LoadAssetsBundle<Sprite>(data["map_icon_resource_assetsbundle"]);
        if (map_icon == null)
        {
            SBDebug.Log("구버전의 패스 사용");
            map_icon = Managers.Resource.LoadAssetsBundle<Sprite>(data["map_icon_resource"]);
        }
        use = Int(data["use"]) > 0;
        use_custom = Int(data["use_custom"]) > 0;
        map_sound = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/bgm/" + data["map_sound"]);
        if (map_sound == null)
        {
            SBDebug.Log("구버전의 패스 사용");
            Managers.Resource.Load<AudioClip>("Sounds/bgm/" + data["map_sound"]);
        }
    }

    static public List<string> GetMapFileList()
    {
        List<string> ret = new List<string>();
        List<GameData> data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.map_type);
        if (data != null)
        {
            foreach (MapTypeGameData mapTypeData in data)
            {
                if(mapTypeData.use || mapTypeData.use_custom)
                    ret.Add(mapTypeData.map_file);
            }
        }

        return ret;
    }

    static public MapTypeGameData GetMapTypeData(int id)
    {
        GameData data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.map_type, id);
        if (data != null)
        {
            return data as MapTypeGameData;
        }

        return null;
    }

    public GameObject GetMapObject()
    {
        if (mapObject == null && !string.IsNullOrEmpty(map_resource))
        {
            mapObject = Managers.Resource.InstantiateFromBundle(map_resource);
        }

        return mapObject;
    }

    public MapData GetMapData()
    {
        if (mapData == null && !string.IsNullOrEmpty(map_file))
        {
            mapData = Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(GameDataManager.GetSavedGameData(map_file));
        }

        return mapData;
    }
}
