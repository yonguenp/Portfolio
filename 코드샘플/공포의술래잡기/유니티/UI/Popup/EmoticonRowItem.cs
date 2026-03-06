using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmoticonRowItem : MonoBehaviour
{
    [SerializeField]
    GameObject[] background;
    [SerializeField]
    Image icon;
    [SerializeField]
    GameObject check;
    [SerializeField]
    GameObject reddot;

    bool bEquiped = false;

    EmoticonRow parentRow;
    EmoticonItemData curData;
    public void SetData(EmoticonItemData data, EmoticonRow row, bool bEquip = false)
    {
        curData = data;
        parentRow = row;

        bool mine = Managers.UserData.GetMyItemCount(data.GetID()) > 0;
        if (mine)
        {
            icon.color = Color.white;
            background[0].GetComponent<Image>().color = Color.white;
        }
        else
        {
            icon.color = Color.gray;
            background[0].GetComponent<Image>().color = Color.gray;
        }

        background[1].SetActive(parentRow.parent.SelectedItemNo == data.GetID());

        icon.sprite = data.sprite;
        bEquiped = bEquip;
        check.SetActive(bEquiped);
    }

    public void OnButton()
    {
        if (bEquiped)
            return;
        if (parentRow == null)
            return;

        parentRow.OnEmotionSelect(curData);
    }


}
