using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NecoCatUpdater : MonoBehaviour
{
    Dictionary<neco_map, List<neco_cat>> cat_position = new Dictionary<neco_map, List<neco_cat>>();
    List<neco_cat> gainCats = new List<neco_cat>();

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        
    }

    public void StartNecoCatUpdate()
    {
        if(cat_position.Count == 0)
        {
            cat_position[neco_map.GetNecoMap(1)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(2)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(3)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(4)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(5)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(6)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(7)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(8)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(9)] = new List<neco_cat>();
            cat_position[neco_map.GetNecoMap(10)] = new List<neco_cat>();

            List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);
            foreach (neco_cat data in necoData)
            {
                if (data != null && data.IsGainCat())
                {
                    if (!gainCats.Contains(data))
                    {
                        gainCats.Add(data);
                        uint remain = (uint)(Random.value * 60.0f);
                        if (remain > data.updateRemainTime)
                        {
                            data.updateRemainTime = remain;
                        }

                        if (data.CurMap() != 0)
                        {
                            cat_position[neco_map.GetNecoMap(data.CurMap())].Add(data);
                        }
                    }
                }
            }
        }

        StopAllCoroutines();
        StartCoroutine(NecoCatUpdate());
    }

    IEnumerator NecoCatUpdate()
    {
        while (true)
        {
            uint curTime = NecoCanvas.GetCurTime();
            List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);
            foreach (neco_cat data in necoData)
            {
                if (data != null)
                {
                    if (data.IsGainCat())
                    {
                        if (!gainCats.Contains(data))
                        {
                            gainCats.Add(data);
                            uint remain = (uint)(Random.value * 60.0f);
                            if (remain > data.updateRemainTime)
                            {
                                data.updateRemainTime = remain;
                            }

                            if (data.CurMap() != 0)
                            {
                                cat_position[neco_map.GetNecoMap(data.CurMap())].Add(data);
                            }
                        }
                    }

                    if (data.IsOnSpot())
                    {
                        neco_map map = data.GetSpot().GetCurMapData();
                        if(!cat_position[map].Contains(data))
                        {
                            foreach (KeyValuePair<neco_map, List<neco_cat>> cp in cat_position)
                            {
                                if (cp.Value.Contains(data))
                                {
                                    cp.Value.Remove(data);
                                    break;
                                }
                            }

                            cat_position[map].Add(data);
                        }
                    }
                }
            }


            List<neco_cat> needUpdate = new List<neco_cat>();
            List<neco_cat> idleCats = new List<neco_cat>();
            foreach (neco_cat cat in gainCats)
            {
                if (cat.updateRemainTime <= 0)
                {
                    needUpdate.Add(cat);
                }
                else
                {
                    bool onMap = false;
                    foreach (KeyValuePair<neco_map, List<neco_cat>> cp in cat_position)
                    {
                        if (cp.Value.Contains(cat))
                        {
                            onMap = true;
                            break;
                        }
                    }

                    if (onMap == false)
                        idleCats.Add(cat);
                }

                if(cat.updateRemainTime > 0)
                    cat.updateRemainTime -= 1;
            }

            foreach (KeyValuePair<neco_map, List<neco_cat>> cp in cat_position)
            {
                if (cp.Key.GetFoodSpot().GetItemRemain() <= 0)
                {
                    foreach (neco_cat cat in cp.Value)
                    {
                        if(cat.updateRemainTime > 0)
                            needUpdate.Add(cat);
                    }

                    cp.Value.Clear();
                }
                else
                {
                    List<neco_cat> removeMapCat = new List<neco_cat>();
                    foreach (neco_cat cat in cp.Value)
                    {
                        if (needUpdate.Contains(cat))
                        {
                            removeMapCat.Add(cat);
                        }
                    }

                    foreach (neco_cat cat in removeMapCat)
                    {
                        cp.Value.Remove(cat);
                    }
                }
            }

            Shuffle(needUpdate);

            List<neco_cat> doneUpdate = new List<neco_cat>();

            for (int i = 0; i < needUpdate.Count; i++)
            {
                if (needUpdate[i] == null)
                    continue;

                List<neco_map> mapList = needUpdate[i].GetAbleMapList();
                if (mapList == null)
                    continue;

                Shuffle(mapList);

                bool added = false;
                foreach(neco_map map in mapList)
                {
                    if (cat_position[map].Contains(needUpdate[i]))
                    {
                        cat_position[map].Remove(needUpdate[i]);
                    }

                    if (!added  && map.IsOpened() && map.GetFoodSpot().GetItemRemain() > 0)
                    {
                        if (cat_position[map].Count <= 5)
                        {
                            cat_position[map].Add(needUpdate[i]);
                            needUpdate[i].updateRemainTime = 60;
                            doneUpdate.Add(needUpdate[i]);

                            WWWForm data = new WWWForm();
                            data.AddField("api", "neco");
                            data.AddField("op", 2);
                            data.AddField("map", needUpdate[i].GetCatID().ToString() + ":" + map.GetMapID());
                            
                            yield return StartCoroutine(NetworkManager.GetInstance().SendSimplePostCorutine("neco", 2, data, (response) => {
                                JObject root = JObject.Parse(response);
                                JToken apiToken = root["api"];
                                if (null == apiToken || apiToken.Type != JTokenType.Array
                                    || !apiToken.HasValues)
                                {
                                    return;
                                }

                                JArray apiArr = (JArray)apiToken;
                                foreach (JObject row in apiArr)
                                {
                                    string uri = row.GetValue("uri").ToString();
                                    if (uri == "object")
                                    {
                                        NetworkManager.GetInstance().OnResponseNecoObject(row);
                                    }
                                    if (uri == "user")
                                    {
                                        NetworkManager.GetInstance().OnResponseUser(row);
                                        NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
                                    }
                                }
                            }));                            

                            Debug.Log("Set map : " + map.GetMapID().ToString() + " in neco cat :" + needUpdate[i].GetCatID());

                            added = true;
                        }
                    }
                }                
            }

            Shuffle(idleCats);
            for(int i = 0; i < idleCats.Count; i++)
            {
                if (idleCats[i] == null)
                    continue;

                List<neco_map> mapList = idleCats[i].GetAbleMapList();
                if (mapList == null)
                    continue;

                Shuffle(mapList);

                bool added = false;
                foreach (neco_map map in mapList)
                {
                    if (cat_position[map].Contains(idleCats[i]))
                    {
                        cat_position[map].Remove(idleCats[i]);
                    }
                    if (!added && map.IsOpened() && map.GetFoodSpot().GetItemRemain() > 0)
                    {
                        if (map.GetFoodSpot().GetItemRemain() > 0)
                        {
                            if (cat_position[map].Count <= 5)
                            {
                                cat_position[map].Add(idleCats[i]);
                                idleCats[i].updateRemainTime = 60;

                                WWWForm data = new WWWForm();
                                data.AddField("api", "neco");
                                data.AddField("op", 2);
                                data.AddField("map", idleCats[i].GetCatID().ToString() + ":" + map.GetMapID());
                                
                                yield return StartCoroutine(NetworkManager.GetInstance().SendSimplePostCorutine("neco", 2, data, (response) => {
                                    JObject root = JObject.Parse(response);
                                    JToken apiToken = root["api"];
                                    if (null == apiToken || apiToken.Type != JTokenType.Array
                                        || !apiToken.HasValues)
                                    {
                                        return;
                                    }

                                    JArray apiArr = (JArray)apiToken;
                                    foreach (JObject row in apiArr)
                                    {
                                        string uri = row.GetValue("uri").ToString();
                                        if (uri == "object")
                                        {
                                            NetworkManager.GetInstance().OnResponseNecoObject(row);
                                        }
                                        if (uri == "user")
                                        {
                                            NetworkManager.GetInstance().OnResponseUser(row);
                                            NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
                                        }
                                    }
                                }));

                                Debug.Log("Set map : " + map.GetMapID().ToString() + " in neco cat :" + idleCats[i].GetCatID());

                                added = true;
                            }
                        }
                    }
                }
            }


            foreach (neco_cat cat in doneUpdate)
            {
                needUpdate.Remove(cat);
            }

            foreach (neco_cat cat in needUpdate)
            {
                cat.updateRemainTime = 10 + ((uint)(Random.value * 30.0f));

                foreach (KeyValuePair<neco_map, List<neco_cat>> cp in cat_position)
                {
                    if (cp.Value.Contains(cat))
                    {
                        cp.Value.Remove(cat);
                        break;
                    }
                }

                WWWForm data = new WWWForm();
                data.AddField("api", "neco");
                data.AddField("op", 2);
                data.AddField("map", cat.GetCatID().ToString() + ":" + 0);
                yield return StartCoroutine(NetworkManager.GetInstance().SendSimplePostCorutine("neco", 2, data, (response) => {
                    JObject root = JObject.Parse(response);
                    JToken apiToken = root["api"];
                    if (null == apiToken || apiToken.Type != JTokenType.Array
                        || !apiToken.HasValues)
                    {
                        return;
                    }

                    JArray apiArr = (JArray)apiToken;
                    foreach (JObject row in apiArr)
                    {
                        string uri = row.GetValue("uri").ToString();
                        if (uri == "object")
                        {
                            NetworkManager.GetInstance().OnResponseNecoObject(row);
                        }
                        if (uri == "user")
                        {
                            NetworkManager.GetInstance().OnResponseUser(row);
                            NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
                        }
                    }
                }));

                //Debug.Log("Set map : " + 0 + " in neco cat :" + cat.GetCatID());
            }

            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.RefreshCat(cat_position[mapController.GetMapData()]);
            }

            float sec = 1.0f;
            while(sec > 0.0f)
            {
                sec -= Time.deltaTime;

                if (mapController != NecoCanvas.GetGameCanvas().GetCurMapController())
                {
                    break;
                }
                else
                { 
                    yield return new WaitForEndOfFrame();
                }
            }
            
        }
    }

    public bool ForceMoveCat(uint catID)
    {
        neco_cat cat = neco_cat.GetNecoCat(catID);
        if (cat == null)
            return false;

        if (cat_position.Count == 0)
            return false;

        neco_map curMap = null;
        foreach (KeyValuePair<neco_map, List<neco_cat>> cp in cat_position)
        {
            if (cp.Value.Contains(cat))
            {
                curMap = cp.Key;
                cp.Value.Remove(cat);
                break;
            }
        }

        List<neco_map> mapList = cat.GetAbleMapList();
        Shuffle(mapList);

        foreach (neco_map map in mapList)
        {
            if (map.IsOpened() && curMap != map && map.GetFoodSpot().GetItemRemain() > 0)
            {
                if (cat_position[map].Count <= 5)
                {
                    cat_position[map].Add(cat);
                    cat.updateRemainTime = 60;

                    WWWForm data = new WWWForm();
                    data.AddField("api", "neco");
                    data.AddField("op", 2);
                    data.AddField("map", cat.GetCatID().ToString() + ":" + map.GetMapID());
                    NetworkManager.GetInstance().SendPostSimple("neco", 2, data, (response) => {
                        JObject root = JObject.Parse(response);
                        JToken apiToken = root["api"];
                        if (null == apiToken || apiToken.Type != JTokenType.Array
                            || !apiToken.HasValues)
                        {
                            return;
                        }

                        JArray apiArr = (JArray)apiToken;
                        foreach (JObject row in apiArr)
                        {
                            string uri = row.GetValue("uri").ToString();
                            if (uri == "object")
                            {
                                NetworkManager.GetInstance().OnResponseNecoObject(row);
                            }
                            if (uri == "user")
                            {
                                NetworkManager.GetInstance().OnResponseUser(row);
                                NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
                            }
                        }
                    });

                    MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                    if (mapController != null)
                    {
                        mapController.RefreshCat(cat_position[mapController.GetMapData()]);
                    }


                    Debug.Log("Set map : " + map.GetMapID().ToString() + " in neco cat :" + cat.GetCatID());
                    return true;
                }
            }
        }

        {
            cat.updateRemainTime = 10;
            WWWForm data = new WWWForm();
            data.AddField("api", "neco");
            data.AddField("op", 2);
            data.AddField("map", cat.GetCatID().ToString() + ":0");
            NetworkManager.GetInstance().SendPostSimple("neco", 2, data, (response) => {
                JObject root = JObject.Parse(response);
                JToken apiToken = root["api"];
                if (null == apiToken || apiToken.Type != JTokenType.Array
                    || !apiToken.HasValues)
                {
                    return;
                }

                JArray apiArr = (JArray)apiToken;
                foreach (JObject row in apiArr)
                {
                    string uri = row.GetValue("uri").ToString();
                    if (uri == "object")
                    {
                        NetworkManager.GetInstance().OnResponseNecoObject(row);
                    }
                    if (uri == "user")
                    {
                        NetworkManager.GetInstance().OnResponseUser(row);
                        NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
                    }
                }
            });

            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.RefreshCat(cat_position[mapController.GetMapData()]);
            }

            return true;
        }

        return false;
    }

    public void Shuffle<T>(List<T> list)
    {
        if (list == null)
            return;

        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public bool IsCatOnMap(neco_map map, neco_cat cat)
    {
        if(cat_position.ContainsKey(map))
        {
            return cat_position[map].Contains(cat);
        }
        return false;
    }
}
