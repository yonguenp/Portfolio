using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HalloweenRewardNotiSample : MonoBehaviour
{
    [SerializeField] Text itemName;
    [SerializeField] Text itemDes;
    [SerializeField] UIBundleItem rewardItem;
    [SerializeField] GameObject clearLine;

    public void SetItem(string name, string des, bool clear = false)
    {
        if (itemName != null)
            itemName.text = name;
        if (itemDes != null)
            itemDes.text = des;

        clearLine.gameObject.SetActive(clear);
    }
    public ShopPackageGameData SetReward(List<ShopPackageGameData> rewards)
    {
        if (rewards[0].goods_type == 4 && rewardItem != null)
        {
            rewardItem.SetCharacterInfo(rewards[0].GetParam());
        }
        else
        {
            if (rewardItem != null)
                rewardItem.SetReward(rewards[0]);
        }
        return rewards[0];
    }

    public bool GetClear()
    {
        return clearLine.activeSelf;
    }
}
