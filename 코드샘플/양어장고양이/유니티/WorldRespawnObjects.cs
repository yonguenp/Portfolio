using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRespawnObjects : MonoBehaviour
{
    public Transform[] RespawnPoints;
    public WorldRespawnObject RespawnObject;
    public FarmCanvas farmCanvas;

    List<WorldRespawnObject> RespawnedObjects = new List<WorldRespawnObject>();

    //[ContextMenu("AutoAddRespawnPoints")]
    //public void AutoAddRespawnPoints()
    //{
    //    for(int i = 0; i < transform.childCount - 1; i++)
    //    {
    //        RespawnPoints[i] = transform.GetChild(i + 1);
    //    }
    //}
    

    private void Awake()
    {
        RespawnObject.gameObject.SetActive(false);
    }

    public void OnRespawn(uint index)
    {
        if (IsRespawned(index))
            return;

        GameObject respawnGameObject = Instantiate(RespawnObject.gameObject);

        respawnGameObject.transform.SetParent(RespawnPoints[index]);
        respawnGameObject.transform.position = RespawnPoints[index].transform.position;
        respawnGameObject.transform.localScale = RespawnObject.transform.localScale;
        respawnGameObject.gameObject.SetActive(true);

        WorldRespawnObject respawnObject = respawnGameObject.GetComponent<WorldRespawnObject>();
        respawnObject.InitRespawnObject(index, this);
        RespawnedObjects.Add(respawnObject);
    }

    public bool IsRespawned(uint index)
    {
        Transform respawnPoint = RespawnPoints[index].transform;
        foreach (WorldRespawnObject obj in RespawnedObjects)
        {
            if (obj.transform.parent == respawnPoint)
            {
                return true;
            }
        }

        return false;
    }

    public void OnRandomRespawn()
    {
        CancelInvoke("OnRandomRespawn");

        List<Transform> enableRespawnPoints = new List<Transform>();
        foreach(Transform respawnPoint in RespawnPoints)
        {
            bool isRespawned = false;
            foreach(WorldRespawnObject obj in RespawnedObjects)
            {
                if(obj.transform.parent == respawnPoint)
                {
                    isRespawned = true;
                }
            }

            if(!isRespawned)
                enableRespawnPoints.Add(respawnPoint);
        }

        if (enableRespawnPoints.Count == 0)
            return;

        uint index = System.Convert.ToUInt32(Random.Range(0, enableRespawnPoints.Count));
        
        OnRespawn(index);

        Invoke("OnRandomRespawn", 30.0f);
    }

    private void OnEnable()
    {
        for (int i = 0; i < 3; i++)
            OnRandomRespawn();
    }

    private void OnDisable()
    {
        CancelInvoke("OnRandomRespawn");
        ClearRespawnObjects();
    }

    public void ClearRespawnObjects()
    {
        foreach(WorldRespawnObject respawnObject in RespawnedObjects)
        {
            Destroy(respawnObject.gameObject);
        }

        RespawnedObjects.Clear();
    }

    public void OnSelectObject(WorldRespawnObject obj)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "chore");
        data.AddField("op", 1);

        //WorldRespawnObject.Reward reward = new WorldRespawnObject.Reward();
        //if(Random.value <= 0.5f)
        //{
        //    data.AddField("gold", 100);
        //    reward.type = "gold";
        //    reward.amount = 100;
        //}
        //else
        //{
        //    if (Random.value <= 0.5f)
        //    {
        //        data.AddField("item", 11);
        //        reward.type = "item";
        //        reward.amount = 1;
        //    }
        //    else
        //    {
        //        data.AddField("exp", 10);
        //        reward.type = "exp";
        //        reward.amount = 10;
        //    }
        //}        
        
        NetworkManager.GetInstance().SendApiRequest("chore", 1, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            WorldRespawnObject.Reward reward = new WorldRespawnObject.Reward();
            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "chore")
                {
                    JToken opCode = row["op"];
                    if (opCode != null && opCode.Type == JTokenType.Integer)
                    {
                        int op = opCode.Value<int>();
                        switch (op)
                        {
                            case 1: //OpPlant::HARVEST  
                                {
                                    
                                    if (row.ContainsKey("rew"))
                                    {
                                        JObject income = (JObject)row["rew"];
                                        if (income.ContainsKey("gold"))
                                        {
                                            reward.type = "gold";
                                            reward.amount = income["gold"].Value<uint>();
                                        }
                                        if (income.ContainsKey("exp"))
                                        {
                                            reward.type = "exp";
                                            reward.amount = income["exp"].Value<uint>();
                                        }
                                        if (income.ContainsKey("item"))
                                        {
                                            reward.type = "item";
                                            JArray item = (JArray)income["item"];
                                            foreach (JObject rw in item)
                                            {
                                                reward.amount = rw["amount"].Value<uint>();
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            RespawnedObjects.Remove(obj);
            obj.OnClearObject(reward);

            farmCanvas.FarmUIPanel.Invoke("Refresh", 0.01f);
        });
    }
}
