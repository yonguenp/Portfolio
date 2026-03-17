using Coffee.UIExtensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttendanceMonthPopup : Popup
{
    [SerializeField]
    UIBundleItem[] bundleItems;
    [SerializeField]
    Text[] day_texts;
    [SerializeField]
    GameObject stampEffects;

    protected virtual int GetAttandanceID() { return 2002; }
    public override void Open(CloseCallback cb = null)
    {
        CloseCallback checkSchedule = () =>
        {
            bool checkedSchedule = false;
            foreach (EventScheduleData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.event_schedule))
            {
                if (data.event_type != 3)
                {
                    continue;
                }

                if (data.GetID() == GetAttandanceID())
                {
                    checkedSchedule = true;
                    continue;
                }

                if (!checkedSchedule)
                    continue;

                if (data.use <= 0)
                    continue;

                if (data.IsEventEnable())
                {
                    data.ShowAttendance(cb);
                    return;
                }
            }
            cb?.Invoke();
        };
        
        base.Open(checkSchedule);
    }

    public override void Close()
    {
        DateTime nowDate = SBCommonLib.SBUtil.KoreanTime.AddHours(-4);
        CacheUserData.SetInt("attendance_" + GetAttandanceID().ToString(), nowDate.DayOfYear + 1);

        base.Close();
    }

    public override void RefreshUI()
    {
        ClearUI();
        for (int i = 0; i < 28; i++)
        {
            UIBundleItem item = bundleItems[i];
            AttendanceGameData ad = AttendanceGameData.GetAttendanceData(GetAttandanceID(), i + 1);
            if (ad == null)
            {
                Close();
                return;
            }

            item.SetRewards(ad.reward);
            day_texts[i].text = StringManager.GetString("check_day_count", i + 1);
        }

        SBWeb.GetAttendanceMonthInfo((response) =>
        {
            JObject attendanceData = (JObject)response["attendance_month"];
            int curProgress = attendanceData["progress"].Value<int>();

            CacheUserData.SetInt("atten_count_" + GetAttandanceID(), curProgress);

            if (curProgress > 28)
            {
                Close();
                return;
            }

            bool rewarded = ((JObject)response).ContainsKey("rewards");
            for (int i = 0; i < 28; i++)
            {
                UIBundleItem item = bundleItems[i];
                Transform pos = null;
                if (item.transform.Find("Checker") != null)
                    pos = item.transform.Find("Checker").transform;

                //GameObject effect = stampEffects;
                //if (effect != null)
                //{
                //    effect.transform.position = pos.position;
                //    effect.SetActive(false);
                //}

                AttendanceGameData ad = AttendanceGameData.GetAttendanceData(GetAttandanceID(), i + 1);
                if (rewarded)
                {
                    item.SetRewards(ad.reward, curProgress - 1 > i);

                    if (curProgress - 1 == i)
                    {
                        if (stampEffects != null)
                        {
                            JArray rew = (JArray)response["rewards"];
                            if (rew != null && rew.Count != 0)
                            {
                                JArray arr = (JArray)rew;
                                List<SBWeb.ResponseReward> rewards = new List<SBWeb.ResponseReward>();
                                foreach (JToken reward_array in arr)
                                {
                                    JArray rewardData = (JArray)reward_array;

                                    bool newReward = true;
                                    //foreach (SBWeb.ResponseReward gainReward in rewards)
                                    //{
                                    //    if ((int)gainReward.Type == rewardData[0].Value<int>() && (int)gainReward.Id == rewardData[1].Value<int>())
                                    //    {
                                    //        gainReward.AddAmount(rewardData[2].Value<int>());
                                    //        newReward = false;
                                    //    }
                                    //}
                                    //출석에서 합쳐주면 햇갈리지않을까

                                    if (newReward)
                                        rewards.Add(new SBWeb.ResponseReward(int.Parse(rewardData[0].ToString()), int.Parse(rewardData[1].ToString()), int.Parse(rewardData[2].ToString())));
                                }
                                if (item.transform.Find("Checker") != null)
                                    pos = item.transform.Find("Checker").transform;

                                if (stampEffects != null)
                                {
                                    stampEffects.transform.position = pos.position;
                                    stampEffects.SetActive(false);
                                }

                                StartCoroutine(ShowRewardPopup(rewards, item, stampEffects));
                            }
                        }
                    }
                }
                else
                {
                    item.SetRewards(ad.reward, curProgress > i);
                }
            }
        });
    }

    IEnumerator ShowRewardPopup(List<SBWeb.ResponseReward> rewards, UIBundleItem item, GameObject effect)
    {
        yield return new WaitForSeconds(0.1f);
        effect.SetActive(true);
        effect.GetComponent<UIParticle>().Play();
        yield return new WaitForSeconds(0.2f);
        item.SetChecker(true);
        yield return new WaitForSeconds(0.5f);

        PopupCanvas.Instance.ShowRewardResult(rewards);
    }

    void ClearUI()
    {
        foreach (UIBundleItem item in bundleItems)
        {
            item.Clear();
        }
    }
}

//public class AttendanceMonthGameData : GameData
//{
//    public int uid { get; private set; }
//    public int day { get; private set; }
//    int reward_id = 0;
//    public List<ShopPackageGameData> reward { get { return ShopPackageGameData.GetRewardDataList(reward_id); } }

//    public override void SetValue(Dictionary<string, string> data)
//    {
//        base.SetValue(data);

//        uid = Int(data["uid"]);
//        day = Int(data["day"]);
//        reward_id = Int(data["reward"]);
//    }

//    public static AttendanceMonthGameData GetAttendanceMonthData(int day)
//    {
//        foreach (AttendanceMonthGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.attendance_month))
//        {
//            if (data.day == day)
//                return data;
//        }

//        return null;
//    }
//}

