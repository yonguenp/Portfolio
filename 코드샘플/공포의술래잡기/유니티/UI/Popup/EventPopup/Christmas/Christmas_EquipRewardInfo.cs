using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Christmas_EquipRewardInfo : MonoBehaviour
{
    [SerializeField] UIbundleEquip equip;
    [SerializeField] Text[] optionType;
    [SerializeField] Text[] optionValue;
    [SerializeField] Text equipName;


    public void Init(EquipInfo info)
    {
        equip.Init(info);
        equipName.text = info.itemData.GetName();
        var List = equip.EquipInfoTypeValue(info.group_id, 40);

        int index = 0;
        foreach (var item in List)
        {
            optionType[index].text = StringManager.GetString(item.Key);
            optionValue[index].text = item.Value;
            index++;
        }
    }
}
