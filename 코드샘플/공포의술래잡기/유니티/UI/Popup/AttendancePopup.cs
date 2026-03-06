using Coffee.UIExtensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AttendancePopup : Popup
{
    [SerializeField]
    UIBundleItem[] bundleItems;
    [SerializeField]
    GameObject[] stampEffects;

    [SerializeField]
    Image deco_R;
    [SerializeField]
    Image deco_L;

    [SerializeField]
    Text title_txt; 


    protected UIBundleItem GetBundle(int index)
    {
        return bundleItems[index];
    }

    protected GameObject GetStamp(int index)
    {
        return stampEffects[index];
    }

    int _attandanceID = 1;
    protected virtual int GetAttandanceID() { return _attandanceID; }
    public void SetID(int id)
    {
        _attandanceID = id;
    }
    public override void Open(CloseCallback cb = null)
    {
        int curID = GetAttandanceID();
        CloseCallback checkSchedule = () =>
        {
            bool checkedSchedule = false;
            if (curID == 1)
                checkedSchedule = true;

            foreach (EventScheduleData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.event_schedule))
            {
                if (data.event_type != 3)
                {
                    continue;
                }

                if (data.GetID() == curID)
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

        deco_L.sprite = null;
        deco_R.sprite = null;

        title_txt.text = StringManager.GetString("ui_check_day");
        if (curID > 1)
        {
            var data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.event_schedule).FirstOrDefault(_ => _.GetID() == curID) as EventScheduleData;

            if (data != null && data.event_type == 3)
            {
                Vector2 origin_pos_R = new Vector2(-525f, 100f);
                Vector2 origin_pos_Y = new Vector2(0f, 100f);

                if (deco_R != null && deco_L != null && data.background_deco_path != null)
                {
                    //리소스 업데이트 
                    deco_R.sprite = data.background_deco_path;
                    deco_L.sprite = data.background_deco_path;

                    deco_R.GetComponent<RectTransform>().anchoredPosition = origin_pos_R + new Vector2(data.attendance_bg_pos_x, data.attendance_bg_pos_y);
                    deco_L.GetComponent<RectTransform>().anchoredPosition = origin_pos_Y + new Vector2(-data.attendance_bg_pos_x, data.attendance_bg_pos_y);
                }
            }

            title_txt.text = StringManager.GetString(data.GetDesc());
        }

        deco_L.gameObject.SetActive(deco_L.sprite != null);
        deco_R.gameObject.SetActive(deco_R.sprite != null);

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
        for (int i = 0; i < 7; i++)
        {
            UIBundleItem item = bundleItems[i];

            item.SetRewards(GetDayReward(i + 1));
        }

        ReqAttendanceInfo();
    }

    public virtual List<ShopPackageGameData> GetDayReward(int day)
    {
        return GetDayReward(GetAttandanceID(), day);
    }
    public virtual List<ShopPackageGameData> GetDayReward(int g_id, int day)
    {
        AttendanceGameData ad = AttendanceGameData.GetAttendanceData(g_id, day);
        if (ad == null)
            return null;

        return ad.reward;
    }

    protected virtual void ReqAttendanceInfo()
    {
        SBWeb.GetAttendanceInfo(GetAttandanceID(), ResponseData);
    }

    public virtual void ResponseData(JToken response)
    {
        JObject attendanceData = (JObject)response["attendance"];
        int curProgress = attendanceData["progress"].Value<int>();

        CacheUserData.SetInt("atten_count_" + GetAttandanceID(), curProgress);

        bool rewarded = ((JObject)response).ContainsKey("rewards");
        for (int i = 0; i < 7; i++)
        {
            UIBundleItem item = bundleItems[i];
            Transform pos = null;
            if (item.transform.Find("Checker") != null)
                pos = item.transform.Find("Checker").transform;

            GameObject effect = stampEffects[i];
            if (effect != null)
            {
                effect.transform.position = pos.position;
                effect.SetActive(false);
            }

            AttendanceGameData ad = AttendanceGameData.GetAttendanceData(GetAttandanceID(), i + 1);
            if (rewarded)
            {
                item.SetRewards(ad.reward, curProgress - 1 > i);

                if (curProgress - 1 == i)
                {
                    if (effect != null)
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

                            StartCoroutine(ShowRewardPopup(rewards, item, effect));
                        }
                    }
                }
            }
            else
            {
                item.SetRewards(ad.reward, curProgress > i);
            }
        }
    }

    protected IEnumerator ShowRewardPopup(List<SBWeb.ResponseReward> rewards, UIBundleItem item, GameObject effect)
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

public class AttendanceGameData : GameData
{
    public int uid { get; private set; }
    public int group_uid { get; private set; } = 1;
    public int day { get; private set; }
    int reward_id = 0;
    public List<ShopPackageGameData> reward { get { return ShopPackageGameData.GetRewardDataList(reward_id); } }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        uid = Int(data["uid"]);

        if (data.ContainsKey("group_uid"))
            group_uid = Int(data["group_uid"]);
        day = Int(data["day"]);
        reward_id = Int(data["reward"]);
    }

    public static AttendanceGameData GetAttendanceData(int g_id, int day)
    {
        foreach (AttendanceGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.attendance))
        {
            if (data.day == day && data.group_uid == g_id)
                return data;
        }

        return null;
    }

    public static int GetAttendanceDayCount(int g_id)
    {
        int ret = 0;
        foreach (AttendanceGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.attendance))
        {
            if (data.day > ret)
                ret = data.day;
        }

        return ret;
    }
}

//public class XmasAttendanceGameData : GameData
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

//    public static XmasAttendanceGameData GetAttendanceData(int day)
//    {
//        foreach (XmasAttendanceGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.attendance_christmas))
//        {
//            if (data.day == day)
//                return data;
//        }

//        return null;
//    }
//}
