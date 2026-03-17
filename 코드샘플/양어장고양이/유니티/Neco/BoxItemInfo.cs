using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxItemData
{
    public Sprite itemImage;
    public string boxItemType;
    public string itemName;
    public string itemCount;
}

public class BoxItemInfo : MonoBehaviour
{
    public Image boxItemImage;
    public Text boxItemNameText;
    public Text boxItemCountText;

    public void SetBoxItemInfoData(BoxItemData boxItemData)
    {
        if (boxItemData == null) { return; }

        boxItemImage.sprite = boxItemData.itemImage;
        boxItemCountText.text = boxItemData.itemCount.ToString();
        if (boxItemData.boxItemType == "item")
        {
            boxItemCountText.text += LocalizeData.GetText("amount");
        }
        boxItemNameText.text = boxItemData.itemName;
    }
}
