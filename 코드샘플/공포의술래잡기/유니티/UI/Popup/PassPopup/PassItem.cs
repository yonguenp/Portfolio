using Coffee.UIExtensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassItem : MonoBehaviour
{
    //[SerializeField]
    //Image backImage;
    [SerializeField]
    UIBundleItem vipItem;
    [SerializeField]
    GameObject vipReddot;
    [SerializeField]
    UIBundleItem nomarlItem;
    [SerializeField]
    GameObject normalReddot;
    [SerializeField]
    Text point;
    [SerializeField]
    Image gaugeBG;
    [SerializeField]
    Image gauge;
    [SerializeField]
    UIParticle fx_bp_vip03;

    PassItemGameData curData = null;
    int level = 0;
    SeasonData seasonData;


    bool isInfinity = false;
    public void Init(PassItemGameData data, int sp, SeasonData season, bool infinity)
    {
        curData = data;
        level = sp;
        isInfinity = infinity;
        seasonData = season;

        Refresh();
    }

    void Refresh()
    {
        if(normalReddot != null)
            normalReddot.SetActive(false);
        if (vipReddot != null)
            vipReddot.SetActive(false);

        if (gauge != null)
        {
            gauge.gameObject.SetActive(curData.level <= 50);
            gauge.color = seasonData.level > curData.level ? Color.white : Color.clear;
            if (curData.level <= 50 && level == seasonData.level - 1 && gauge.color != Color.clear)
                fx_bp_vip03.Play();
        }

        if (!isInfinity)
        {
            point.text = level.ToString();

            bool normalRewarded = seasonData.rewarded.Contains(level);
            bool vipRewarded = seasonData.vip_rewarded.Contains(level);
            bool enableReward = seasonData.level >= level;

            if (enableReward)
            {
                if (normalRewarded)
                {
                    nomarlItem.SetRewards(curData.free_reward_shop, true);
                    nomarlItem.SetDim(true);
                }
                else
                {
                    nomarlItem.SetRewards(curData.free_reward_shop, false, TryRewardNormal);
                    if(normalReddot != null)
                        normalReddot.SetActive(true);
                }
                point.transform.parent.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/pass/battle_pass_bg_04");
            }
            else
            {
                //backImage.color = Color.gray;
                nomarlItem.SetRewards(curData.free_reward_shop, false, OnNeedMoreLevel);
                point.transform.parent.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/pass/battle_pass_bg_03");
            }

            if (seasonData.vip >= 2)
            {
                if (enableReward)
                {
                    if (vipRewarded)
                    {
                        vipItem.SetRewards(curData.vip_reward_shop, true);
                        vipItem.SetDim(true);
                    }
                    else
                    {
                        vipItem.SetRewards(curData.vip_reward_shop, false, TryRewardVip);
                        if (vipReddot != null)
                            vipReddot.SetActive(true);
                    }
                }
                else
                    vipItem.SetRewards(curData.vip_reward_shop, false, OnNeedMoreLevel);
            }
            else
            {
                //backImage.color = Color.gray;
                vipItem.SetRewards(curData.vip_reward_shop, false, OnNeedVip);
                vipItem.SetLock(true);
            }

        }
        else
        {
            point.text = level.ToString();
            bool vipRewarded = true;
            for (int i = curData.level; i <= seasonData.level; i++)
            {
                if (!seasonData.vip_rewarded.Contains(i))
                {
                    vipRewarded = false;
                }
            }

            bool enableReward = !vipRewarded && seasonData.level >= level;

            if (seasonData.vip >= 2)
            {
                if (enableReward)
                {
                    if (vipRewarded)
                    {
                        vipItem.SetRewards(curData.vip_reward_shop, true);
                        vipItem.SetDim(true);
                    }
                    else
                    {
                        vipItem.SetRewards(curData.vip_reward_shop, false, TryRewardVip);
                        if (vipReddot != null)
                            vipReddot.SetActive(true);
                    }
                }
                else
                {
                    vipItem.SetRewards(curData.vip_reward_shop, false, OnNeedMoreLevel);
                }
            }
            else
            {
                //backImage.color = Color.gray;
                vipItem.SetRewards(curData.vip_reward_shop, false, OnNeedVip);
                vipItem.SetLock(true);
            }

        }
    }

    public void TryRewardNormal()
    {
        SBWeb.GetPassReward(curData.level, 1, (response) =>
        {
            RewardCheck(response);

            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.BATTLEPASS_POPUP) as BattlePassPopup).DataRefresh(response);
        });
    }

    public void TryRewardVip()
    {
        int level = curData.level;

        var seasonData = Managers.UserData.seasonData;
        if (seasonData != null)
        {
            PassGameData passData = Managers.Data.GetData(GameDataManager.DATA_TYPE.pass, seasonData.seasonID) as PassGameData;
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

            if (level >= maxData.level)
            {
                for (int i = curData.level; i <= seasonData.level; i++)
                {
                    if (!seasonData.vip_rewarded.Contains(i))
                    {
                        level = i;
                        break;
                    }
                }
            }
        }

        

        SBWeb.GetPassReward(level, 2, (response) =>
        {
            RewardCheck(response);

            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.BATTLEPASS_POPUP) as BattlePassPopup).DataRefresh(response);
        });
    }

    public void OnNeedMoreLevel()
    {
        PopupCanvas.Instance.ShowFadeText("msg_battle_point_fail");
    }

    public void OnNeedVip()
    {
        const int passItemID = 999999999;
        PopupCanvas.Instance.ShowBuyPopup(ShopItemGameData.GetShopData(passItemID), (cnt) =>
        {
            Managers.IAP.TryPurchase(passItemID, ShopPackageGameData.GetIAPConstants(passItemID), (responseArr) =>
            {
                SBWeb.GetPassData((response) =>
                {
                    (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.BATTLEPASS_POPUP) as BattlePassPopup).DataRefresh(response);
                });

                PopupCanvas.Instance.ShowFadeText("결제성공");
            }, (responseArr) =>
            {
                PopupCanvas.Instance.ShowFadeText("결제실패");
            });
        });
    }

    public static void RewardCheck(JToken response)
    {
        bool rewarded = ((JObject)response).ContainsKey("rewards");
        if (!rewarded)
        {
            PopupCanvas.Instance.ShowFadeText("ui_no_reward");
            return;
        }

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

            PopupCanvas.Instance.ShowRewardResult(rewards);
        }
    }
}
