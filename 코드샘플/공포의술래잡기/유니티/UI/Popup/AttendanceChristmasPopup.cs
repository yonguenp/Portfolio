using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttendanceChristmasPopup : AttendancePopup
{
    protected override int GetAttandanceID() { return 2001; }
    protected override void ReqAttendanceInfo()
    {
        SBWeb.GetAttendanceInfoChristmas22(ResponseData);
    }

    public override void ResponseData(JToken response)
    {
        JObject jo_Data = (JObject)response["attendance"];
        var curProgress = jo_Data["progress"].Value<int>();

        CacheUserData.SetInt("atten_count_" + GetAttandanceID(), curProgress);
        
        if (curProgress > 7)
        {
            Close();
            return;
        }

        bool rewarded = ((JObject)response).ContainsKey("rewards");
        for (int i = 0; i < 7; i++)
        {
            UIBundleItem item = GetBundle(i);
            Transform pos = null;
            if (item.transform.Find("Checker") != null)
                pos = item.transform.Find("Checker").transform;

            GameObject effect = GetStamp(i);
            if (effect != null)
            {
                effect.transform.position = pos.position;
                effect.SetActive(false);
            }

            AttendanceGameData ad = AttendanceGameData.GetAttendanceData(GetAttandanceID(), i + 1);
            if (ad == null)
            {
                Close();
                return;
            }

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

}
