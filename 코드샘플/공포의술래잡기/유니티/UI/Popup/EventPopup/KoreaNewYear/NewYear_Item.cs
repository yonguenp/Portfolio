using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewYear_Item : MonoBehaviour
{
    [SerializeField] Text itemName;
    [SerializeField] Text per;
    [SerializeField] UIBundleItem item;

    public void Init(EventBoxData data, int total_rates)
    {
        if (itemName != null)
            data.GetName();
        if (per != null)
            per.text = (((float)data.rates / total_rates) * 100).ToString("F2") + "%";
        if (item != null)
        {
            var reward = ShopPackageGameData.GetRewardDataList(data.goods_id);
            if (reward.Count > 0)
            {
                item.SetReward(reward[0]);
                string str = "";

                str = reward[0].GetName();
                if (reward.Count == 1)
                {
                    switch ((ASSET_TYPE)reward[0].goods_type)
                    {
                        case ASSET_TYPE.GOLD:
                            str = StringManager.GetString("gold_name");
                            break;
                        case ASSET_TYPE.DIA:
                            str = StringManager.GetString("dia_name");
                            break;
                        case ASSET_TYPE.MILEAGE:
                            str = StringManager.GetString("mileage_name");
                            break;
                        case ASSET_TYPE.ITEM:
                        case ASSET_TYPE.EQUIPMENT:
                        case ASSET_TYPE.BUFF_ITEM:
                            if (reward[0].targetItem != null)
                                str = reward[0].targetItem.GetName();
                            break;
                        case ASSET_TYPE.CHARACTER:
                            if (reward[0].targetCharacter != null)
                                str = reward[0].targetCharacter.GetName();
                            break;
                        default:
                            str = reward[0].GetName();
                            break;
                    }

                    if (reward[0].goods_amount > 1)
                        str += " " + StringManager.GetString("ui_count", reward[0].goods_amount);
                }

                if(itemName != null)
                {
                    itemName.text = str;
                }
            }
        }
    }

    public void SetItem(EventBoxData data)
    {
        if (item != null)
        {
            if (item != null)
            {
                var reward = ShopPackageGameData.GetRewardDataList(data.goods_id);
                if (reward.Count > 0)
                {
                    item.SetReward(reward[0]);
                    string str = "";

                    str = reward[0].GetName();
                    if (reward.Count == 1)
                    {
                        switch ((ASSET_TYPE)reward[0].goods_type)
                        {
                            case ASSET_TYPE.GOLD:
                                str = StringManager.GetString("gold_name");
                                break;
                            case ASSET_TYPE.DIA:
                                str = StringManager.GetString("dia_name");
                                break;
                            case ASSET_TYPE.MILEAGE:
                                str = StringManager.GetString("mileage_name");
                                break;
                            case ASSET_TYPE.ITEM:
                            case ASSET_TYPE.EQUIPMENT:
                            case ASSET_TYPE.BUFF_ITEM:
                                if (reward[0].targetItem != null)
                                    str = reward[0].targetItem.GetName();
                                break;
                            case ASSET_TYPE.CHARACTER:
                                if (reward[0].targetCharacter != null)
                                    str = reward[0].targetCharacter.GetName();
                                break;
                            default:
                                str = reward[0].GetName();
                                break;
                        }

                        if (reward[0].goods_amount > 1)
                            str += " " + StringManager.GetString("ui_count", reward[0].goods_amount);
                    }

                    if (itemName != null)
                    {
                        itemName.text = str;
                    }
                }
            }
        }
    }

}