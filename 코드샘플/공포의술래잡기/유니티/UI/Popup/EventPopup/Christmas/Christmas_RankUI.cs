using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Christmas_RankUI : MonoBehaviour
{
    [SerializeField] newRankItem rankSample;
    [SerializeField] newRankItem myRank;

    [SerializeField] GameObject rewardPopup;
    [SerializeField] GameObject rewardSampleItem;
    [SerializeField] Christmas_EquipRewardInfo detailEquipPopup;

    private EquipInfo equipInfo;
    ChristmasPopup parent = null; 
    public void Init(ChristmasPopup popup)
    {
        Clear();
        rewardSampleItem.SetActive(true);

        parent = popup;
        
        if(parent.rankList != null)
        {
            int index = 1;
            foreach(var rank in parent.rankList)
            {
                var obj = GameObject.Instantiate(rankSample, rankSample.transform.parent);
                obj.gameObject.SetActive(true);
                obj.SetData(index++, rank);
            }
        }

        myRank.SetData(parent.christmasInfo.rank, new ChristmasRank { 
            name = Managers.UserData.MyName,
            score = parent.christmasInfo.score,
        });

        var datas = Managers.Data.GetData(GameDataManager.DATA_TYPE.ranking_reward);
        List<RankingRewardData> rewardGameData = new List<RankingRewardData>();

        foreach (RankingRewardData data in datas)
        {
            if (data.group > 20000)
            {
                rewardGameData.Add(data);
            }
        }

        int idx = 1;
        foreach (var reward in rewardGameData)
        {
            var rankItem = GameObject.Instantiate(rewardSampleItem, rewardSampleItem.transform.parent);
            string des = StringManager.GetString($"ui_rank_detail_{idx}");

            rankItem.GetComponent<RankRewardInfoItem>().Setdata(des, reward.reward);

            if (idx == 1)
            {
                var data = ShopPackageGameData.GetRewardDataList(reward.reward);
                if(data.Count > 0)
                    equipInfo = Managers.Data.GetData(GameDataManager.DATA_TYPE.equipment_info, data[0].GetParam()) as EquipInfo;
            }
            idx++;
        }
        rewardSampleItem.SetActive(false);

    }
    public void OpenReward()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHRISTMAS_POPUP) as ChristmasPopup;
        popup.curpage++;
        rewardPopup.SetActive(true);
    }
    public void OpenEquipPopup()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHRISTMAS_POPUP) as ChristmasPopup;
        popup.curpage++;

        detailEquipPopup.gameObject.SetActive(true);

        detailEquipPopup.Init(equipInfo);
    }


    void Clear()
    {
        foreach (Transform item in rankSample.transform.parent)
        {
            if (item == rankSample.transform)
                continue;
            Destroy(item.gameObject);
        }

        foreach (Transform item in rewardSampleItem.transform.parent)
        {
            if (item == rewardSampleItem.transform)
                continue;
            Destroy(item.gameObject);
        }

        rankSample.gameObject.SetActive(false);
    }
}
