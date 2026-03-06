using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HalloweenAttendancePanel : MonoBehaviour
{
    public RectTransform DaysGroupTranform;
    public RectTransform EffectContainer;
    public GameObject TargetEffect;
    public ShopPackageInfo PackageInfo;
    uint curDay;
    int curDayCount = 7;

    List<RewardData> rewardList = new List<RewardData>();

    bool enableCheck = false;
    bool toggleChat = false;
    bool eventCheck = false;

    private void OnEnable()
    {
        RefreshData();
        PackageInfo.SetPackageInfoData(neco_shop.GetNecoShopData(53), null);
    }

    public void SetAttendanceUI()
    {
        ShowEffect();

        halloween_event eventData = null;
        halloween_event.halloween_attendance attendanceData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.HALLOWEEN)
                eventData = (halloween_event)evt;
        }

        if (eventData != null)
        {
            attendanceData = eventData.GetAttendanceData();
        }

        if (attendanceData == null) { return; }

        curDay = attendanceData.totalAttendanceDays;
        enableCheck = attendanceData.enableAttendance;

        int now = (int)curDay;
        int index = 0;
        if (GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS) == null)
            return;

        rewardList.Clear();

        foreach (neco_event_halloween_attendance attendance in neco_event_halloween_attendance.GetNecoAttendanceList())
        {
            rewardList.Add(attendance.GetReward());
        }

        foreach (RectTransform day in DaysGroupTranform)
        {
            if (curDayCount <= index)
            {
                day.gameObject.SetActive(false);
                continue;
            }
            else
            {
                day.gameObject.SetActive(true);

                if (day.name == "dummy")
                    continue;
            }

            if (day.GetComponent<Button>() != null)
            {
                DestroyImmediate(day.GetComponent<Button>());
            }

            RectTransform reward = day.Find("reward") as RectTransform;
            RectTransform dayText = day.Find("Text") as RectTransform;
            if (reward == null)
                continue;

            RectTransform amount = reward.Find("amount") as RectTransform;
            RectTransform checker = reward.Find("checker") as RectTransform;

            if (rewardList.Count <= index)
                return;

            Image rewardImage = reward.GetComponent<Image>();
            Text rewardName = null;
            if(reward.Find("Name_text") != null)
            {
                rewardName = reward.Find("Name_text").GetComponent<Text>();
            }
            RewardData curReward = rewardList[index];
            if (curReward.itemData != null)
            {
                items item = curReward.itemData;
                uint amountCount = curReward.count;

                if (rewardImage != null)
                    rewardImage.sprite = item.GetItemIcon();
                if (rewardName != null)
                    rewardName.text = item.GetItemName();
                amount.GetComponent<Text>().text = amountCount > 1 ? amountCount.ToString() : "";
            }
            else if (curReward.memoryData != null)
            {
                neco_cat_memory memory = curReward.memoryData;
                uint amountCount = curReward.count;

                if (rewardImage != null)
                    rewardImage.sprite = Resources.Load<Sprite>(memory.GetNecoMemoryThumbnail());
                if (rewardName != null)
                    rewardName.text = memory.GetNecoMemoryTitle();
                amount.GetComponent<Text>().text = amountCount > 1 ? amountCount.ToString() : "";
            }
            else
            {
                if (curReward.gold > 0)
                {
                    if (rewardImage != null)
                        rewardImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                    if (rewardName != null)
                        rewardName.text = LocalizeData.GetText("LOCALIZE_334");
                    amount.GetComponent<Text>().text = curReward.gold > 1 ? curReward.gold.ToString() : "";

                }
                if (curReward.point > 0)
                {
                    if (rewardImage != null)
                        rewardImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");
                    if (rewardName != null)
                        rewardName.text = LocalizeData.GetText("LOCALIZE_335");
                    amount.GetComponent<Text>().text = curReward.point > 1 ? curReward.point.ToString() : "";
                }
                if (curReward.catnip > 0)
                {
                    if (rewardImage != null)
                        rewardImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
                    if (rewardName != null)
                        rewardName.text = LocalizeData.GetText("LOCALIZE_348");
                    amount.GetComponent<Text>().text = curReward.catnip > 1 ? curReward.catnip.ToString() : "";
                }
            }

            day.DOKill();
            day.localScale = Vector3.one;
            checker.gameObject.SetActive(false);

            RectTransform Select_stroke = reward.Find("Select_stroke") as RectTransform;
            if (Select_stroke != null)
                Select_stroke.gameObject.SetActive(false);
            if (now == 0 && enableCheck)
            {
                day.GetComponent<Image>().color = new Color(1.0f, 0.9882353f, 0.7411765f, 1.0f);
                if (rewardImage != null)
                    rewardImage.color = Color.white;
                Button btn = day.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(OnAttendance);

                day.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);

                if (Select_stroke != null)
                    Select_stroke.gameObject.SetActive(true);

                OnAttendance();
            }
            else
            {
                if (now <= 0)
                {
                    day.GetComponent<Image>().color = Color.white;
                    if (rewardImage != null)
                        rewardImage.color = Color.white;
                }
                else
                {
                    day.GetComponent<Image>().color = Color.gray;
                    if (rewardImage != null)
                        rewardImage.color = Color.gray;

                    checker.gameObject.SetActive(true);
                }
            }

            // text 문구 적용
            if (dayText != null)
            {
                dayText.GetComponent<Text>().text = string.Format(LocalizeData.GetText("N일차보상"), index + 1);
            }

            index += 1;
            now -= 1;
        }
    }

    public void OnAttendance()
    {
        if (!enableCheck)
            return;

        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.HALLOWEEN);
        data.AddField("op", 41);

        NetworkManager.GetInstance().SendApiRequest("event", 41, data, (response) =>
        {
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
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            //curDay = (int)row["progress"].Value<uint>();
                            enableCheck = false;

                            List<RewardData> ret = new List<RewardData>();
                            if (row.ContainsKey("rew"))
                            {
                                JObject income = (JObject)row["rew"];
                                if (income.ContainsKey("gold"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.gold = income["gold"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("catnip"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.catnip = income["catnip"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("point"))
                                {
                                    RewardData reward = new RewardData();
                                    reward.point = income["point"].Value<uint>();
                                    ret.Add(reward);
                                }

                                if (income.ContainsKey("item"))
                                {
                                    JArray item = (JArray)income["item"];
                                    foreach (JObject rw in item)
                                    {
                                        RewardData reward = new RewardData();
                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                        reward.count = rw["amount"].Value<uint>();
                                        ret.Add(reward);
                                    }
                                }

                                if (income.ContainsKey("memory"))
                                {
                                    JArray memory = (JArray)income["memory"];

                                    Dictionary<string, uint> memoryDic = new Dictionary<string, uint>();
                                    memoryDic.Add("point", 0);
                                    foreach (JArray rw in memory)
                                    {
                                        neco_cat_memory catMemory = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                                        if (catMemory == null) continue;

                                        if (rw[1].Value<uint>() > 0)
                                        {
                                            memoryDic["point"] += rw[1].Value<uint>();  // 포인트로 합산
                                        }
                                        else if (memoryDic.ContainsKey(catMemory.GetNecoMemoryID().ToString()) == false)
                                        {
                                            memoryDic.Add(catMemory.GetNecoMemoryID().ToString(), 1);
                                        }
                                    }

                                    foreach (var memoryPair in memoryDic)
                                    {
                                        RewardData reward = new RewardData();

                                        if (memoryPair.Key == "point")
                                        {
                                            reward.point = memoryPair.Value;
                                        }
                                        else
                                        {
                                            reward.memoryData = neco_cat_memory.GetNecoMemory(uint.Parse(memoryPair.Key));
                                        }

                                        ret.Add(reward);
                                    }
                                }
                            }

                            NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_233"), LocalizeData.GetText("LOCALIZE_234"), ret, () => {
                                RefreshData();
                            });
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("Event_Res_1"); break;
                                case 2: msg = LocalizeData.GetText("Event_Res_2"); break;
                                case 11: msg = LocalizeData.GetText("Event_Res_11"); break;
                                case 12: msg = LocalizeData.GetText("Event_Res_12"); break;
                                case 13: msg = LocalizeData.GetText("Event_Res_13"); break;
                                case 14: msg = LocalizeData.GetText("Event_Res_14"); break;
                                case 31: msg = LocalizeData.GetText("Event_Res_31"); break;
                                case 32: msg = LocalizeData.GetText("Event_Res_32"); break;
                                case 41: msg = LocalizeData.GetText("Event_Res_41"); break;
                                case 42: msg = LocalizeData.GetText("Event_Res_42"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), msg);
                        }
                    }
                }
            }
        });
    }

    void RefreshData()
    {
        rewardList.Clear();

        foreach (RectTransform day in DaysGroupTranform)
        {
            day.gameObject.SetActive(false);
        }

        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.HALLOWEEN);
        data.AddField("op", 40);

        NetworkManager.GetInstance().SendApiRequest("event", 40, data, (response) =>
        {
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
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("SetAttendanceUI", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("Event_Res_1"); break;
                                case 2: msg = LocalizeData.GetText("Event_Res_2"); break;
                                case 11: msg = LocalizeData.GetText("Event_Res_11"); break;
                                case 12: msg = LocalizeData.GetText("Event_Res_12"); break;
                                case 13: msg = LocalizeData.GetText("Event_Res_13"); break;
                                case 14: msg = LocalizeData.GetText("Event_Res_14"); break;
                                case 31: msg = LocalizeData.GetText("Event_Res_31"); break;
                                case 32: msg = LocalizeData.GetText("Event_Res_32"); break;
                                case 41: msg = LocalizeData.GetText("Event_Res_41"); break;
                                case 42: msg = LocalizeData.GetText("Event_Res_42"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), msg);
                        }
                    }
                }
            }
        });

        eventCheck = false;
    }


    public bool IsCheckAble()
    {
        return enableCheck;
    }

    public void OnClose()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.HALLOWEEN_POPUP);

        if(toggleChat)
        {
            if (!NecoCanvas.GetUICanvas().ChatUI.IsChatUIActive())
                NecoCanvas.GetUICanvas().ChatUI.OnToggleChat();
            else
                NecoCanvas.GetUICanvas().ChatUI.OnShowChatUI();

            toggleChat = false;
        }
    }

    public void OnBanner()
    {
        OnClose();
        NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.PACKAGE);
    }

    void ShowEffect()
    {
        if (EffectContainer.transform.childCount > 2)
            return;

        Vector2 size = (EffectContainer.transform as RectTransform).sizeDelta;
        GameObject effect = Instantiate(TargetEffect);
        effect.transform.SetParent(EffectContainer.transform);
        RectTransform rt = effect.GetComponent<RectTransform>();

        rt.localPosition = new Vector3((size.x * 0.5f) - (UnityEngine.Random.value * size.x), 10 + (size.y * 0.5f) - (UnityEngine.Random.value * size.y), TargetEffect.transform.localPosition.z);
        rt.localScale = Vector3.one;

        effect.SetActive(true);

        Invoke("ShowEffect", 0.1f);
    }
    public bool EnableAttendance()
    {
        bool result = false;

        halloween_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.HALLOWEEN)
                eventData = (halloween_event)evt;
        }

        if (eventData != null)
        {
            if (eventData.IsEnableAttendance())
            {
                halloween_event.halloween_attendance attendanceData = eventData.GetAttendanceData();
                if (attendanceData != null)
                {
                    result = attendanceData.enableAttendance;
                }
            }
        }

        return result;
    }

    public void SetToggleChat()
    {
        toggleChat = true;
    }
}

[Serializable]
public class neco_event_halloween_attendance : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_EVENT_HALLOWEEN_ATTENDANCE; }

    static public List<neco_event_halloween_attendance> GetNecoAttendanceList()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_EVENT_HALLOWEEN_ATTENDANCE);
        if (necoData == null)
        {
            return null;
        }

        List<neco_event_halloween_attendance> attendanceList = new List<neco_event_halloween_attendance>();

        foreach (neco_event_halloween_attendance attendanceData in necoData)
        {
            attendanceList.Add(attendanceData);
        }

        return attendanceList;
    }

    [NonSerialized]
    private uint necoEventAttendanceID = 0;
    public uint GetNecoEventAttendanceID()
    {
        if (necoEventAttendanceID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                necoEventAttendanceID = (uint)obj;
            }
        }

        return necoEventAttendanceID;
    }

    [NonSerialized]
    private uint necoEventAttendanceDay = 0;
    public uint GetNecoEventAttendanceDay()
    {
        if (necoEventAttendanceDay == 0)
        {
            object obj;
            if (data.TryGetValue("day", out obj))
            {
                necoEventAttendanceDay = (uint)obj;
            }
        }

        return necoEventAttendanceDay;
    }

    [NonSerialized]
    private string necoEventAttendanceItemType;
    public string GetNecoEventAttendanceItemType()
    {
        if (necoEventAttendanceItemType == null)
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                necoEventAttendanceItemType = (string)obj;
            }
        }

        return necoEventAttendanceItemType;
    }

    [NonSerialized]
    private uint necoEventAttendanceItemID = 0;
    public uint GetNecoEventAttendanceItemID()
    {
        if (necoEventAttendanceItemID == 0)
        {
            object obj;
            if (data.TryGetValue("item_id", out obj))
            {
                necoEventAttendanceItemID = (uint)obj;
            }
        }

        return necoEventAttendanceItemID;
    }

    [NonSerialized]
    private uint necoEventAttendanceCount = 0;
    public uint GetNecoEventAttendanceCount()
    {
        if (necoEventAttendanceCount == 0)
        {
            object obj;
            if (data.TryGetValue("count", out obj))
            {
                necoEventAttendanceCount = (uint)obj;
            }
        }

        return necoEventAttendanceCount;
    }

    [NonSerialized]
    RewardData reward = null;
    public RewardData GetReward()
    {
        if (reward == null)
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                reward = new RewardData();
                switch ((string)obj)
                {
                    case "gold":
                        if (data.TryGetValue("count", out obj))
                        {
                            reward.gold = (uint)obj;
                        }
                        break;
                    case "dia":
                        if (data.TryGetValue("count", out obj))
                        {
                            reward.catnip = (uint)obj;
                        }
                        break;
                    case "item":
                        if (data.TryGetValue("item_id", out obj))
                        {
                            reward.itemData = items.GetItem((uint)obj);
                            if (data.TryGetValue("count", out obj))
                            {
                                reward.count = (uint)obj;

                            }
                        }
                        break;
                }

            }
        }

        return reward;
    }
}