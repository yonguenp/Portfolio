using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankRewardInfoItem : MonoBehaviour
{
    public Text name;
    public Text amount;
    public UIBundleItem UIBundleItem;

    public void Setdata(string name, int package_id)
    {
        this.name.text = StringManager.GetString(name);
        var reward = ShopPackageGameData.GetRewardDataList(package_id);
        UIBundleItem.SetRewards(reward);

        if (reward[0].goods_type == 3 || reward[0].goods_type == 13)
            amount.color = Color.clear;
        else
            amount.color = Color.white;

        amount.text = reward[0].goods_amount.ToString();
    }
}
