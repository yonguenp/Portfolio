using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GachaRate
{
    public int uid;
    public int groupId;
    public int weight;
    public int resultId;
    public int resultType;
    public double rate;
    public int sort;
    //public GachaRate(int uid, int groupId, int weight, int resultId, int resultType)
    //{
    //    this.uid = uid;
    //    this.groupId = groupId;
    //    this.weight = weight;
    //    this.resultId = resultId;
    //    this.resultType = resultType;
    //    this.rate = 0f;
    //}
    //public GachaRate(int uid, int groupId, int weight, int resultId, int resultType, double rate)
    //{
    //    this.uid = uid;
    //    this.groupId = groupId;
    //    this.weight = weight;
    //    this.resultId = resultId;
    //    this.resultType = resultType;
    //    this.rate = rate;
    //}

    public GachaRate(JObject rateObject)
    {
        uid = rateObject["uid"].Value<int>();
        groupId = rateObject["group_id"].Value<int>();
        weight = rateObject["weight"].Value<int>();
        resultId = rateObject["result_id"].Value<int>();
        resultType = rateObject["result_type"].Value<int>();
        if (rateObject.ContainsKey("rate"))
            rate = rateObject["rate"].Value<double>();
        else
            rate = 0.0f;
        sort = rateObject["sort"].Value<int>();
    }
}
public class GachaTablePopup : Popup
{
    [SerializeField] Text desc1;
    [SerializeField] Text desc2;

    [SerializeField] Text gachaGradeText;
    [SerializeField] GameObject itemSample;
    [SerializeField] Transform parent;

    List<GachaRate> gachaRateList = new List<GachaRate>();
    List<CharacterRateItem> prefabs = new List<CharacterRateItem>();

    public int gach_base { get; private set; } = 0;
    bool isDirty = false;
    public override void Close()
    {
        base.Close();
    }

    public void SetGachaBase(int type)
    {
        if (gach_base != type)
        {
            isDirty = true;
        }

        gach_base = type;

        if (EquipConfig.Config["equip_gacha_proc_group"] == gach_base)
        {
            SetEquipText();
        }
        else
        {
            SetNormalText();
        }
    }

    void SetEquipText()
    {
        desc1.text = StringManager.GetString("ui_pickup_info_equip");
        desc2.text = StringManager.GetString("장비확률표_확률표안내");
    }

    void SetNormalText()
    {
        desc1.text = StringManager.GetString("ui_pickup_info");
        desc2.text = StringManager.GetString("가차확률표_확률표안내");
    }

    public override void Open(CloseCallback cb = null)
    {
        if (isDirty)
            SBWeb.GetGachaRate(gach_base, OnRateData);

        closeCallback = cb;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (popupAnimation)
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }
    public override void RefreshUI()
    {
        string gradeText = "";

        gachaRateList.Sort((A, B) =>
        {
            if (A.uid < B.uid)
                return 1;
            else
                return -1;
        });

        foreach (GachaRate rate in gachaRateList)
        {
            if (rate.resultType != 0)
                continue;

            gradeText += StringManager.Instance.GetString("gacha_rates", "name", rate.uid);
            gradeText += string.Format(": {0}% ", rate.rate.ToString("F1"));
        }
        gachaGradeText.text = gradeText;

        CreateListItem();

        //if (prefabs != null)
        //    prefabs.Sort((x, y) =>
        //    {
        //        return x.grade.CompareTo(y.grade);
        //    });
    }

    public void OnRateData(JToken res)
    {
        if (res == null)
            return;

        var rateData = (JArray)res;

        gachaRateList.Clear();

        foreach (JToken rateObject in rateData)
        {
            if (rateObject.Type != JTokenType.Object)
                continue;

            GachaRate temp = new GachaRate((JObject)rateObject);
            gachaRateList.Add(temp);
        }
        gachaRateList.Reverse();

        CalGachaGradeRate();
    }

    public void CalGachaGradeRate()
    {
        int totalWeight = 0;
        for (int i = 0; i < gachaRateList.Count; i++)
        {
            if (gachaRateList[i].resultType == 0)
                totalWeight += gachaRateList[i].weight;
        }

        for (int i = 0; i < gachaRateList.Count; i++)
        {
            if (gachaRateList[i].resultType == 0)
            {
                gachaRateList[i].rate = (double)gachaRateList[i].weight / (double)totalWeight * 100.0f;
            }
        }
        RefreshUI();
    }

    public void CreateListItem()
    {
        if (itemSample == null)
            return;
        itemSample.SetActive(false);

        foreach (var item in prefabs)
        {
            Destroy(item.gameObject);
        }
        prefabs.Clear();

        gachaRateList.Sort((x, y) =>
        {
            return x.sort.CompareTo(y.sort);
        });

        foreach (var item in gachaRateList)
        {
            if (item.resultType > 0)
            {
                GameObject obj = GameObject.Instantiate(itemSample, parent);
                var rewards = ShopPackageGameData.GetRewardDataList(item.resultId);
                if (rewards.Count > 0)
                {
                    var reward = rewards[0];
                    if(reward.targetCharacter != null)
                        obj.GetComponent<CharacterRateItem>().Init(reward.targetCharacter, item.rate * 100);
                    else if (reward.targetItem != null)
                        obj.GetComponent<CharacterRateItem>().Init(reward.targetItem, item.rate * 100);
                }
                
                obj.SetActive(true);
                prefabs.Add(obj.GetComponent<CharacterRateItem>());
            }
        }

    }
}
