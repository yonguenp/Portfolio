using Newtonsoft.Json.Linq;
using SBCommonLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassPopup : Popup
{
    [SerializeField]
    Image seasonBannerImage;
    [SerializeField]
    Text seasonName;
    [SerializeField]
    Text seasonDate;
    [SerializeField]
    Text seasonLevel;
    [SerializeField]
    Text nextLevel;
    [SerializeField]
    GameObject nextLevelItem;
    [SerializeField]
    Text nextLevelAmountText;

    [SerializeField]
    Image seasonExp;
    [SerializeField]
    const float expWidth = 560;
    [SerializeField]
    Text seasonExpText;
    [SerializeField]
    PassItem sampleGroupItem;
    [SerializeField]
    PassItem sampleGroupItem_infinity;
    [SerializeField]
    ScrollRect scrollView;
    [SerializeField]
    GameObject BattlePassBuyButton;
    [SerializeField]
    Text BattlePassNotice;

    public override void Open(CloseCallback cb = null)
    {
        ClearUI();

        base.Open(cb);

        SBWeb.GetPassData((response) =>
        {
            DataRefresh(response);

            var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
            if (LobbyScene != null)
            {
                LobbyScene.RefreshBattlePassIcon();
            }
        });
    }
    public override void Close()
    {
        base.Close();
        ClearUI();
    }

    public void DataRefresh(JToken response)
    {
        JObject pass = (JObject)response["pass"];

        Managers.UserData.SetPassData(pass);

        RefreshUI();
    }

    void ClearUI()
    {
        seasonBannerImage.sprite = null;

        foreach (Transform child in sampleGroupItem.transform.parent)
        {
            if (child == sampleGroupItem.transform)
                continue;
            if (child == sampleGroupItem_infinity.transform)
                continue;

            Destroy(child.gameObject);
        }
        sampleGroupItem.gameObject.SetActive(false);
        sampleGroupItem_infinity.gameObject.SetActive(false);
    }
    public override void RefreshUI()
    {
        ClearUI();

        base.RefreshUI();
        var curData = Managers.UserData.seasonData;

        if (curData == null || curData.seasonID <= 0)
        {
            return;
        }

        PassGameData passData = Managers.Data.GetData(GameDataManager.DATA_TYPE.pass, curData.seasonID) as PassGameData;
        if (passData == null)
        {
            return;
        }
        seasonName.text = passData.GetName();
        seasonLevel.text = curData.level.ToString();
        nextLevel.gameObject.SetActive(true);
        nextLevel.text = (curData.level + 1).ToString();

        if (nextLevelItem.GetComponentInParent<Image>() != null)
            nextLevelItem.GetComponentInParent<Image>().color = Color.white;
        nextLevelItem.SetActive(false);

        string remainText = "";
        var diff = passData.end_time - SBUtil.KoreanTime;
        if (diff.Days >= 1.0f)
        {
            remainText = StringManager.GetString("ui_day", diff.Days.ToString()) + " " + StringManager.GetString("ui_hour", diff.Hours.ToString());
        }
        else
        {
            remainText = StringManager.GetString("ui_hour", diff.Hours.ToString()) + " " + StringManager.GetString("ui_min", diff.Minutes.ToString());
        }

        seasonDate.text = StringManager.GetString("ui_left_time", remainText);
        if (passData == null)
        {
            Close();
            PopupCanvas.Instance.ShowFadeText("msg_battle_error");
            return;
        }

        List<PassItemGameData> curSeasonItems = new List<PassItemGameData>();
        List<PassItemGameData> maxSeasonItems = new List<PassItemGameData>();
        foreach (PassItemGameData d in Managers.Data.GetData(GameDataManager.DATA_TYPE.pass_item))
        {
            if (d.group == passData.pass_item_group)
            {
                curSeasonItems.Add(d);
            }
        }
        curSeasonItems.Sort((a, b) => { return a.level.CompareTo(b.level); });

        var maxData = curSeasonItems[curSeasonItems.Count - 1];

        curSeasonItems.Remove(maxData);
        maxSeasonItems.Add(maxData);

        int addReward = 0;
        if (curData.level >= maxData.level)
        {
            addReward = (curData.exp - curSeasonItems[curSeasonItems.Count - 1].next_point) / maxData.next_point;

            for (int i = 0; i < addReward; i++)
            {
                maxSeasonItems.Add(maxData);
            }
        }


        sampleGroupItem.gameObject.SetActive(true);

        foreach (PassItemGameData d in curSeasonItems)
        {
            GameObject passItem = null;
            passItem = Instantiate(sampleGroupItem.gameObject);

            passItem.transform.SetParent(sampleGroupItem.transform.parent);
            passItem.transform.localPosition = Vector3.zero;
            passItem.transform.localScale = Vector3.one;

            passItem.GetComponent<PassItem>().Init(d, d.level, curData, false);
        }
        sampleGroupItem.gameObject.SetActive(false);

        sampleGroupItem_infinity.gameObject.SetActive(true);
        {
            GameObject passItem = null;
            passItem = Instantiate(sampleGroupItem_infinity.gameObject);

            passItem.transform.SetParent(sampleGroupItem_infinity.transform.parent);
            passItem.transform.localPosition = Vector3.zero;
            passItem.transform.localScale = Vector3.one;

            passItem.GetComponent<PassItem>().Init(maxData, maxData.level, curData, true);
        }
        sampleGroupItem_infinity.gameObject.SetActive(false);

        int prevPoint = 0;
        int nextPoint = 0;
        if (curData.level >= maxData.level)
        {
            prevPoint = curSeasonItems[curSeasonItems.Count - 1].next_point;
            nextPoint = prevPoint + maxData.next_point;
            while (curData.exp - nextPoint >= 0)
            {
                prevPoint += maxData.next_point;
                nextPoint = prevPoint + maxData.next_point;
            }
            scrollView.horizontalNormalizedPosition = 1.0f;
            seasonLevel.text = (maxData.level - 1).ToString();
            nextLevel.gameObject.SetActive(false);
            nextLevelAmountText.text = string.Empty;
            if (nextLevelItem.GetComponentInParent<Image>() != null)
                nextLevelItem.GetComponentInParent<Image>().color = Color.clear;
            nextLevelItem.SetActive(true);

            int cnt = 0;
            int bonusCnt = 0;
            foreach (var id in curData.vip_rewarded)
            {
                if (id >= maxData.level)
                    bonusCnt++;
            }
            cnt = curData.level - (maxData.level - 1) - bonusCnt;
            nextLevelAmountText.text = "x" + cnt.ToString();
        }
        else
        {
            for (int i = 0; i < curSeasonItems.Count; i++)
            {
                if (curSeasonItems[i].next_point > curData.exp)
                {
                    nextPoint = curSeasonItems[i].next_point;
                    break;
                }
                prevPoint = curSeasonItems[i].next_point;
            }

            scrollView.horizontalNormalizedPosition = (float)(curData.level - 1) / maxData.level;
        }

        seasonExpText.text = (curData.exp - prevPoint).ToString() + " / " + (nextPoint - prevPoint).ToString();
        (seasonExp.transform as RectTransform).sizeDelta = new Vector2(Mathf.Min(1.0f, (float)(curData.exp - prevPoint) / (nextPoint - prevPoint)) * expWidth, (seasonExp.transform as RectTransform).sizeDelta.y);


        seasonBannerImage.sprite = Resources.Load<Sprite>(passData.banner_image);
        BattlePassBuyButton.SetActive(curData.vip < 2);

        BattlePassNotice.text = StringManager.GetString(curData.vip < 2 ? "ui_battle_pass_noti" : "ui_battle_pass_noti_2");
    }

    public void ReceiveAll()
    {
        var curData = Managers.UserData.seasonData;

        if (curData == null)
        {
            return;
        }

        PassGameData passData = Managers.Data.GetData(GameDataManager.DATA_TYPE.pass, curData.seasonID) as PassGameData;
        List<PassItemGameData> curSeasonItems = new List<PassItemGameData>();
        foreach (PassItemGameData d in Managers.Data.GetData(GameDataManager.DATA_TYPE.pass_item))
        {
            if (d.group == passData.pass_item_group)
            {
                curSeasonItems.Add(d);
            }
        }
        curSeasonItems.Sort((a, b) => { return a.level.CompareTo(b.level); });

        var maxData = curSeasonItems[curSeasonItems.Count - 1];

        bool enable = false;
        for (int i = 0; i < curData.level; i++)
        {
            if (i < maxData.level && !curData.rewarded.Contains(i))
            {
                enable = true;
                break;
            }
            if (curData.vip > 1)
            {
                if (!curData.vip_rewarded.Contains(i))
                {
                    enable = true;
                    break;
                }
            }
        }

        if (enable)
        {
            SBWeb.GetPassRewards((response) =>
            {
                PassItem.RewardCheck(response);

                DataRefresh(response);

            });
        }
        else
        {
            PopupCanvas.Instance.ShowFadeText("ui_no_reward");
        }
    }



    public void OnVip()
    {
        const int passItemID = 999999999;
        PopupCanvas.Instance.ShowBuyPopup(ShopItemGameData.GetShopData(passItemID), (cnt) =>
        {
            if (Managers.UserData.seasonData.seasonID > 10010)
            {
                SBWeb.OnBuy(passItemID, 1, (response) =>
                {
                    SBWeb.GetPassData((response) =>
                        {
                            DataRefresh(response);
                            PopupCanvas.Instance.ShowFadeText("결제성공");
                        });
                });
            }
            else
            {
                Managers.IAP.TryPurchase(passItemID, ShopPackageGameData.GetIAPConstants(passItemID), (responseArr) =>
                {
                    SBWeb.GetPassData((response) =>
                    {
                        DataRefresh(response);
                    });
                    PopupCanvas.Instance.ShowFadeText("결제성공");
                }, (responseArr) =>
                {
                    PopupCanvas.Instance.ShowFadeText("결제실패");
                });
            }
        });
    }
}

public class PassGameData : GameData
{
    public int uid { get; private set; }
    public int use { get; private set; }
    public DateTime start_time;
    public DateTime end_time;
    public int pass_item_group;
    public string banner_image { get; private set; }

    public override void SetValue(Dictionary<string, string> tmp)
    {
        base.SetValue(tmp);

        uid = Int(data["uid"]);
        use = Int(data["use"]);
        start_time = DateTime.Parse(data["start_time"]);
        end_time = DateTime.Parse(data["end_time"]);
        pass_item_group = Int(data["pass_item_group"]);
        banner_image = data["banner_image"];
    }
}
public class PassItemGameData : GameData
{
    public int uid { get; private set; }
    public int group { get; private set; }
    public int level { get; private set; }
    public int next_point { get; private set; }
    public int free_reward { get; private set; }
    public int vip_reward { get; private set; }

    public List<ShopPackageGameData> free_reward_shop { get { return ShopPackageGameData.GetRewardDataList(free_reward); } }
    public List<ShopPackageGameData> vip_reward_shop { get { return ShopPackageGameData.GetRewardDataList(vip_reward); } }


    public override void SetValue(Dictionary<string, string> tmp)
    {
        base.SetValue(tmp);

        uid = Int(data["uid"]);
        group = Int(data["group"]);
        level = Int(data["level"]);
        next_point = Int(data["next_point"]);
        free_reward = Int(data["free_reward"]);
        vip_reward = Int(data["vip_reward"]);
    }
}
public class SeasonData
{
    public int seasonID;
    public int level;
    public int exp;
    public int vip;
    public List<int> rewarded;
    public List<int> vip_rewarded;

    public SeasonData()
    {
        Clear();
    }

    public void Clear()
    {
        seasonID = -1;
        level = 0;
        exp = 0;
        vip = 1;
        rewarded = new List<int>();
        vip_rewarded = new List<int>();
    }

    public void SetData(int id, int lv, int ex, int v, JArray r, JArray vr)
    {
        seasonID = id;
        level = lv;
        exp = ex;
        vip = v;
        rewarded = new List<int>();
        foreach (var d in r)
        {
            rewarded.Add(d.Value<int>());
        }
        vip_rewarded = new List<int>();
        foreach (var d in vr)
        {
            vip_rewarded.Add(d.Value<int>());
        }
    }

    public bool CheckEnableReward()
    {
        var curData = Managers.UserData.seasonData;
        if (curData == null || curData.seasonID <= 0)
        {
            return false;
        }

        PassGameData passData = Managers.Data.GetData(GameDataManager.DATA_TYPE.pass, curData.seasonID) as PassGameData;
        List<PassItemGameData> curSeasonItems = new List<PassItemGameData>();
        foreach (PassItemGameData d in Managers.Data.GetData(GameDataManager.DATA_TYPE.pass_item))
        {
            if (d.group == passData.pass_item_group)
            {
                curSeasonItems.Add(d);
            }
        }
        curSeasonItems.Sort((a, b) => { return a.level.CompareTo(b.level); });

        var maxData = curSeasonItems[curSeasonItems.Count - 1];

        bool enable = false;
        for (int i = 0; i < curData.level; i++)
        {
            if (i < maxData.level && !curData.rewarded.Contains(curData.level))
            {
                enable = true;
                break;
            }
            if (curData.vip > 1)
            {
                if (!curData.vip_rewarded.Contains(curData.level))
                {
                    enable = true;
                    break;
                }
            }
        }
        return enable;
    }
}

