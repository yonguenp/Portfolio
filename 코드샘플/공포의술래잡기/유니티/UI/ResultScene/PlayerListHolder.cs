using SBSocketSharedLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;

public class PlayerListHolder : MonoBehaviour
{
    [SerializeField] PlayerListItem item;

    Dictionary<long, int> chaserPoints = new Dictionary<long, int>();
    Dictionary<long, int> survivorPoints = new Dictionary<long, int>();
    private void OnEnable()
    {
        chaserPoints.Clear();
        survivorPoints.Clear();

        item.gameObject.SetActive(false);
        foreach (Transform child in transform)
        {
            if (child != item.transform)
                Destroy(child.gameObject);
        }
    }

    public void AddItem(RoomPlayerInfo roomPlayer)
    {
        var newItem = Instantiate(item);
        newItem.transform.SetParent(transform);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localScale = Vector3.one;
        newItem.gameObject.SetActive(true);

        newItem.Initialize(roomPlayer);

        if (CharacterGameData.IsChaserCharacter(roomPlayer.SelectedCharacter.CharacterType))
        {
            newItem.transform.SetAsFirstSibling();
            chaserPoints.Add(roomPlayer.UserId, -2);
        }
        else
        {
            survivorPoints.Add(roomPlayer.UserId, -2);
        }
    }

    public void AddItem(JObject roomPlayer, int point)
    {
        var newItem = Instantiate(item);
        newItem.transform.SetParent(transform);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localScale = Vector3.one;
        newItem.gameObject.SetActive(true);

        newItem.Initialize(roomPlayer, point);

        if (CharacterGameData.IsChaserCharacter(roomPlayer["char_id"].Value<int>()))
        {
            newItem.transform.SetAsFirstSibling();
            if(!chaserPoints.ContainsKey(roomPlayer["user_no"].Value<long>()))
                chaserPoints.Add(roomPlayer["user_no"].Value<long>(), -2);
        }
        else
        {
            if (!survivorPoints.ContainsKey(roomPlayer["user_no"].Value<long>()))
                survivorPoints.Add(roomPlayer["user_no"].Value<long>(), -2);
        }
    }

    public List<PlayerListItem> GetItems()
    {
        var list = new List<PlayerListItem>();
         foreach (Transform child in transform)
        {
            var playerListItemScript = child.GetComponent<PlayerListItem>();
            if(playerListItemScript != null)
                list.Add(playerListItemScript);
        }

        return list;
    }

     public PlayerListItem GetItem(string id)
    {
        foreach (Transform child in transform)
        {
            var playerListItemScript = child.GetComponent<PlayerListItem>();
            if(playerListItemScript != null)
            {
                if(playerListItemScript.UserId == id)
                    return playerListItemScript;
            }
        }

        return null;
    }

    public void SetPoint(long user_id, int point)
    {
        if(chaserPoints.ContainsKey(user_id))
            chaserPoints[user_id] = point;
        if(survivorPoints.ContainsKey(user_id))
            survivorPoints[user_id] = point;
    }

    public void SortWithPoint()
    {
        List<PlayerListItem> items = new List<PlayerListItem>();
        foreach (Transform child in transform)
        {
            var playerListItemScript = child.GetComponent<PlayerListItem>();
            if (playerListItemScript != null)
            {
                items.Add(playerListItemScript);
            }
        }

        int sibIndex = 0;
        foreach(var cp in chaserPoints.OrderByDescending(i => i.Value))
        {
            string key = cp.Key.ToString();
            foreach (var p in items)
            {
                if(key == p.UserId)
                {
                    p.transform.SetSiblingIndex(sibIndex);
                    sibIndex++;
                }
            }
        }

        foreach (var sp in survivorPoints.OrderByDescending(i => i.Value))
        {
            string key = sp.Key.ToString();
            foreach (var p in items)
            {
                if (key == p.UserId)
                {
                    p.transform.SetSiblingIndex(sibIndex);
                    sibIndex++;
                }
            }
        }
    }
}
