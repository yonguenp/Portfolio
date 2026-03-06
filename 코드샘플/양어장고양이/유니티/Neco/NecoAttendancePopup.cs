using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Newtonsoft.Json.Linq;
using System.Linq;

[Serializable]
public class neco_check_reward : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_CHECK_REWARD; }
    
    [NonSerialized]
    RewardData reward = null;

    public static List<neco_check_reward> GetReward(uint period)
    {
        List<neco_check_reward> ret = new List<neco_check_reward>();
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CHECK_REWARD);
        if (necoData == null)
        {
            return ret;
        }

        foreach (neco_check_reward r in necoData)
        {
            if (r.GetPeriod() == period)
                ret.Add(r);
        }

        if(ret.Count > 0)
            ret = ret.OrderBy(x => x.GetDay()).ToList();

        return ret;
    }

    [NonSerialized]
    uint period = 0;

    public uint GetPeriod()
    {
        if (period == 0)
        {
            object obj;
            if (data.TryGetValue("period", out obj))
            {
                period = (uint)obj;
            }
        }

        return period;
    }

    [NonSerialized]
    uint day = 0;

    public uint GetDay()
    {
        if (day == 0)
        {
            object obj;
            if (data.TryGetValue("day", out obj))
            {
                day = (uint)obj;
            }
        }

        return day;
    }

    public RewardData GetReward()
    {
        if(reward == null)
        {
            object obj;
            if (data.TryGetValue("item_type", out obj))
            {
                reward = new RewardData();
                switch ((string)obj)
                {
                    case "gold":
                        if (data.TryGetValue("item_count", out obj))
                        {
                            reward.gold = (uint)obj;
                        }
                        break;
                    case "point":
                        if (data.TryGetValue("item_count", out obj))
                        {
                            reward.point = (uint)obj;
                        }
                        break;
                    case "item":
                        if (data.TryGetValue("item_id", out obj))
                        {
                            reward.itemData = items.GetItem((uint)obj);
                            if (data.TryGetValue("item_count", out obj))
                            {
                                reward.count = (uint)obj;

                            }
                        }
                        break;
                    case "memory":
                        if (data.TryGetValue("item_id", out obj))
                        {
                            reward.memoryData = neco_cat_memory.GetNecoMemory((uint)obj);
                        }
                        break;
                }

            }
        }

        return reward;
    }
}

[Serializable]
public class neco_monthly_reward : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_MONTHLY_REWARD; }

    public static neco_monthly_reward GetMonthyReward(uint period, uint month)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MONTHLY_REWARD);
        if (necoData == null)
        {
            return null;
        }

        foreach (neco_monthly_reward r in necoData)
        {
            if (r.GetPeriod() == period && r.GetMonth() == month)
                return r;
        }

        return null;
    }

    [NonSerialized]
    uint period = 0;

    public uint GetPeriod()
    {
        if (period == 0)
        {
            object obj;
            if (data.TryGetValue("period", out obj))
            {
                period = (uint)obj;
            }
        }

        return period;
    }

    [NonSerialized]
    uint month = 0;

    public uint GetMonth()
    {
        if (month == 0)
        {
            object obj;
            if (data.TryGetValue("month", out obj))
            {
                month = (uint)obj;
            }
        }

        return month;
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
                        if (data.TryGetValue("item_count", out obj))
                        {
                            reward.gold = (uint)obj;
                        }
                        break;
                    case "point":
                        if (data.TryGetValue("item_count", out obj))
                        {
                            reward.point = (uint)obj;
                        }
                        break;
                    case "item":
                        if (data.TryGetValue("item_id", out obj))
                        {
                            reward.itemData = items.GetItem((uint)obj);
                            if (data.TryGetValue("item_count", out obj))
                            {
                                reward.count = (uint)obj;

                            }
                        }
                        break;
                    case "memory":
                        if (data.TryGetValue("item_id", out obj))
                        {
                            reward.memoryData = neco_cat_memory.GetNecoMemory((uint)obj);
                        }
                        break;
                }

            }
        }

        return reward;
    }

    public string GetDesc()
    {
        object obj;
        if (data.TryGetValue("id", out obj))
        {
            return LocalizeData.GetText("neco_monthly_reward_new:desc:" + ((uint)obj).ToString());
            //return (string)obj;
        }

        return "";
    }
}

public class NecoAttendancePopup : NecoAnimatePopup
{
    public RectTransform DaysGroupTranform;
    public Text TileText;
    public RectTransform Special;
    public Text BottomTitle;

    uint curPeriod = 0;
    uint curMonth;
    int curDay;
    int curDayCount = 14;
    List<RewardData> rewardList = new List<RewardData>();
    bool enableCheck = false;
    bool toggleChat = false;
    bool eventCheck = false;
    private void Awake()
    {
        
    }

    public override void OnAnimateDone()
    {

    }

    protected override void OnEnable()
    {
        base.OnEnable();

        rewardList.Clear();
        
        
        foreach (RectTransform day in DaysGroupTranform)
        {
            day.gameObject.SetActive(false);
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "attendance");
        data.AddField("op", 1);

        NetworkManager.GetInstance().SendApiRequest("attendance", 1, data, (response) =>
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
                if (uri == "attendance")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            SetAttendanceData(response);

                            Invoke("SetAttendanceUI", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_316"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }
        });

        eventCheck = false;
    }

    public void SetAttendanceData(string response)
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
            if (uri == "attendance")
            {
                JToken resultCode = row["rs"];
                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                {
                    int rs = resultCode.Value<int>();
                    if (rs == 0)
                    {
                        if(row.ContainsKey("month"))
                            curMonth = row["month"].Value<uint>();
                        if (row.ContainsKey("period"))
                            curPeriod = row["period"].Value<uint>();
                        curDay = (int)row["progress"].Value<uint>();
                        enableCheck = row["today"].Value<uint>() > 0;
                    }
                }
            }
        }

        TileText.text = string.Format(LocalizeData.GetText("LOCALIZE_232"), LocalizeData.GetText(curMonth.ToString() + "월"), LocalizeData.GetText(curPeriod.ToString() + "번째"));
        NecoCanvas.GetUICanvas().RefreshTopMenuRedDot();
    }

    public void SetAttendanceUI()
    {
        BottomTitle.text = string.Format(LocalizeData.GetText("LOCALIZE_397"), LocalizeData.GetText(curMonth.ToString() + "월"), LocalizeData.GetText(curPeriod.ToString() + "번째"));

        int now = curDay;
        int index = 0;
        if (GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ITEMS) == null)
            return;

        rewardList.Clear();

        foreach (neco_check_reward r in neco_check_reward.GetReward(curPeriod))
        {
            rewardList.Add(r.GetReward());
        }


        neco_monthly_reward monthReward = neco_monthly_reward.GetMonthyReward(curPeriod, curMonth);
        if (monthReward != null)
            rewardList.Add(monthReward.GetReward());


        foreach (RectTransform day in DaysGroupTranform)
        {
            if(curDayCount <= index)
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

            if(day.GetComponent<Button>() != null)
            {
                DestroyImmediate(day.GetComponent<Button>());
            }

            RectTransform reward = day.Find("reward") as RectTransform;
            if (reward == null)
                continue;

            RectTransform amount = reward.Find("amount") as RectTransform;
            RectTransform checker = reward.Find("checker") as RectTransform;

            if (rewardList.Count <= index)
                return;

            RewardData curReward = rewardList[index];
            if (curReward.itemData != null)
            {
                items item = curReward.itemData;
                uint amountCount = curReward.count;

                reward.GetComponent<Image>().sprite = item.GetItemIcon();
                amount.GetComponent<Text>().text = amountCount > 1 ? amountCount.ToString() : "";
            }
            else if (curReward.memoryData != null)
            {
                neco_cat_memory memory = curReward.memoryData;
                uint amountCount = curReward.count;

                reward.GetComponent<Image>().sprite = Resources.Load<Sprite>(memory.GetNecoMemoryThumbnail());
                amount.GetComponent<Text>().text = amountCount > 1 ? amountCount.ToString() : "";
            }
            else
            {
                if(curReward.gold > 0)
                {
                    reward.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                    amount.GetComponent<Text>().text = curReward.gold > 1 ? curReward.gold.ToString() : "";

                }
                if (curReward.point > 0)
                {
                    reward.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");
                    amount.GetComponent<Text>().text = curReward.point > 1 ? curReward.point.ToString() : "";
                }
            }

            day.DOKill();
            day.localScale = Vector3.one;
            checker.gameObject.SetActive(false);

            RectTransform Select_stroke = reward.Find("Select_stroke") as RectTransform;
            if(Select_stroke != null)
                Select_stroke.gameObject.SetActive(false);
            if (now == 0 && enableCheck)
            {
                day.GetComponent<Image>().color = new Color(1.0f, 0.9882353f, 0.7411765f, 1.0f);
                reward.GetComponent<Image>().color = Color.white;
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
                    reward.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    day.GetComponent<Image>().color = Color.gray;
                    reward.GetComponent<Image>().color = Color.gray;

                    checker.gameObject.SetActive(true);
                }
            }

            index += 1;
            now -= 1;
        }

        if (rewardList.Count > 0)
        {

            Text specialDesc = Special.Find("SubText").GetComponent<Text>();
            neco_monthly_reward curMonthlyData = neco_monthly_reward.GetMonthyReward(curPeriod, curMonth);
            RewardData curReward = curMonthlyData.GetReward();
            if (curReward == null)
            {
                specialDesc.text = "";
                return;
            }

            RectTransform SpecialReward = Special.Find("SpecialReward") as RectTransform;
            if (SpecialReward == null)
            {
                specialDesc.text = "";
                return;
            }

            RectTransform reward = SpecialReward.Find("reward") as RectTransform;
            RectTransform amount = reward.Find("amount") as RectTransform;
            RectTransform checker = reward.Find("checker") as RectTransform;

            if (curReward.itemData != null)
            {
                items item = curReward.itemData;
                uint amountCount = curReward.count;

                reward.GetComponent<Image>().sprite = item.GetItemIcon();
                amount.GetComponent<Text>().text = amountCount > 1 ? amountCount.ToString() : "";
            }
            else if (curReward.memoryData != null)
            {
                neco_cat_memory memory = curReward.memoryData;
                uint amountCount = curReward.count;

                reward.GetComponent<Image>().sprite = Resources.Load<Sprite>(memory.GetNecoMemoryThumbnail());
                amount.GetComponent<Text>().text = amountCount > 1 ? amountCount.ToString() : "";
            }
            else
            {
                if (curReward.gold > 0)
                {
                    reward.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                    amount.GetComponent<Text>().text = curReward.gold > 1 ? curReward.gold.ToString() : "";

                }
                if (curReward.point > 0)
                {
                    reward.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");
                    amount.GetComponent<Text>().text = curReward.point > 1 ? curReward.point.ToString() : "";
                }
            }

            checker.gameObject.SetActive(curDay >= 28);

            if(specialDesc)
            {
                specialDesc.text = curMonthlyData.GetDesc();
            }
        }

    }

    public void OnAttendance()
    {
        if (!enableCheck)
            return;

        WWWForm data = new WWWForm();
        data.AddField("api", "attendance");
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("attendance", 2, data, (response) =>
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
                if (uri == "attendance")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            curDay = (int)row["progress"].Value<uint>();
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
                                            if (memoryPair.Value > 0)
                                            {
                                                reward.point = memoryPair.Value;
                                            }
                                            else
                                            {
                                                continue;
                                            }
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
                                SetAttendanceUI();
                                NecoCanvas.GetUICanvas().RefreshTopMenuRedDot();
                            });
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_316"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }
        });
    }

    public void OnAttendanceButton()
    {   
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP);

        eventCheck = true;
    }

    public void OnClose()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP);

        if (toggleChat)
        {
            if (NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.HALLOWEEN_POPUP].GetComponent<HalloweenAttendancePanel>().EnableAttendance())
            {
                NecoCanvas.GetPopupCanvas().OnCheckEventAttendance(true);
            }
            else
            {
                if (!NecoCanvas.GetUICanvas().ChatUI.IsChatUIActive())
                    NecoCanvas.GetUICanvas().ChatUI.OnToggleChat();
                else
                    NecoCanvas.GetUICanvas().ChatUI.OnShowChatUI();
            }
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnCheckEventAttendance();
        }

        toggleChat = false;
    }

    public bool IsCheckAble()
    {
        return enableCheck;
    }

    public void SetToggleChat()
    {
        toggleChat = true;
    }
}
