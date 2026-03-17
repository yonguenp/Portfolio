using Crosstales;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasAttendanceLayer : MonoBehaviour
{
    uint totalDay = 0;
    uint todayCheck = 0;
    RewardData reward;

    public List<RewardInfo> rewards;
    public List<GameObject> rewardCheckers;

    private void OnEnable()
    {
        reward = new RewardData();
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
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
                            JObject info = row.GetValue("info").ToObject<JObject>();
                            totalDay = info["checked"].Value<uint>(); 
                            todayCheck = info["today"].Value<uint>();
                            InitAttendanceReward();
                            Invoke(nameof(CheckAttendance), 0.1f);
                        }
                        else
                        {
                            ChristmasEventPopup.ChristmasEventWWWErrorCode(rs, true);
                        }
                    }
                }
            }
        });
    }

    void InitAttendanceReward()
    {
        List<game_data> datas = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_ATTENDANCE);
        int index = 0;

        if (datas == null)
        {
            //Err !!
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            neco_event_xmas_attendance info = (neco_event_xmas_attendance)datas[i];
            RewardData reward = new RewardData();

            switch (info.GetNecoEventItemType())
            {
                case "gold":
                    reward.gold = info.GetNecoEventItemCount();
                    break;
                case "dia":
                    reward.catnip = info.GetNecoEventItemCount();
                    break;
                case "point":
                    reward.point = info.GetNecoEventItemCount();
                    break;
                case "item":
                    reward.itemData = items.GetItem(info.GetNecoEventItemID());
                    reward.count = info.GetNecoEventItemCount();
                    break;
                case "memory":
                    reward.memoryData = neco_cat_memory.GetNecoMemory(info.GetNecoEventItemID());
                    break;
            }

            rewards[index].SetRewardInfoData(reward);
            ++index;
        }

        for(int i = 0; i < totalDay; i++)
        {
            rewardCheckers[i].SetActive(true);
        }

        for(int i = 1; i <= 7; i++)
        {
            gameObject.CTFind("DayText_" + i).GetComponent<Text>().text = string.Format(LocalizeData.GetText("N일차"), i);
        }
    }

    void CheckAttendance()
    {
        //출첵 애니메이션
        if (todayCheck == 1)    //1회만 체크 또는 체크 데이가 다르면 다시 체크
        {
            //출첵 시도
            WWWForm data = new WWWForm();
            data.AddField("uri", "event");
            data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
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
                                JToken checkedDay = row.GetValue("checked").ToObject<JToken>();
                                totalDay = checkedDay.Value<uint>();

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
                                                if (memoryPair.Value == 0)
                                                    continue;

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

                                NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_233"), LocalizeData.GetText("LOCALIZE_234"), ret, CallRewardPopup);
                            }
                            else
                            {
                                ChristmasEventPopup.ChristmasEventWWWErrorCode(rs, true);
                            }
                        }
                    }
                }
            });
        }
    }

    void CallRewardPopup()
    {
        InitAttendanceReward();
    }
}
