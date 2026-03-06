using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiftInfo : MonoBehaviour
{
    [Header("[Have Gift]")]
    public GameObject haveObject;

    public Image giftIcon;
    public Text giftName;

    [Header("[UnKnown Gift]")]
    public GameObject unknownObject;

    public void SetGiftInfo(items itemData)
    {
        haveObject.SetActive(false);
        unknownObject.SetActive(false);

        if (itemData == null)
        {
            unknownObject.SetActive(true);
        }
        else
        {
            haveObject.SetActive(true);
            giftIcon.sprite = itemData.GetItemIcon();
            giftName.text = itemData.GetItemName();
        }
    }
}
