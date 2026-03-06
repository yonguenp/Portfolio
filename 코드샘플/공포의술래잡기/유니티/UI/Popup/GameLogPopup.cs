using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameLogPopup : Popup
{
    [SerializeField] Text titleText;
    [SerializeField] GameObject scroll;
    [SerializeField] GameObject nullLog;
    [SerializeField] GameLogItem sampleObject;

    JToken logDatas;
    public override void Open(CloseCallback cb = null)
    {
        SBWeb.GetGameLogDB(GameLogData);
        sampleObject.gameObject.SetActive(false);
        base.Open(cb);
    }
    public override void Close()
    {
        base.Close();
    }

    public override void RefreshUI()
    {
        if (logDatas == null)
            return;

        Clear();
        nullLog.SetActive(true);

        if (logDatas.HasValues)
        {
            scroll.SetActive(true);
            
            sampleObject.gameObject.SetActive(true);

            List<KeyValuePair<DateTime, JToken>> tokens = new List<KeyValuePair<DateTime, JToken>>();
            foreach (var data in logDatas)
            {
                tokens.Add(new KeyValuePair<DateTime, JToken>(DateTime.Parse(data["time"].Value<string>()), data));
            }
            tokens = tokens.OrderByDescending(_ => _.Key).ToList();

            int i = 0;
            foreach (var item in tokens)
            {
                GameLogItem obj = GameObject.Instantiate(sampleObject, sampleObject.GetComponent<Transform>().parent) as GameLogItem;
                obj.Init(item.Value, i++);
                nullLog.SetActive(false);
            }

            sampleObject.gameObject.SetActive(false);
        }
        else
        {
            scroll.SetActive(false);            
        }
    }

    public void GameLogData(JToken datas)
    {
        logDatas = datas;
        RefreshUI();
    }

    public void Clear()
    {
        foreach (Transform item in sampleObject.transform.parent)
        {
            if (item == sampleObject.transform)
                continue;
            Destroy(item.gameObject);
        }

        sampleObject.gameObject.SetActive(false);
    }
}
