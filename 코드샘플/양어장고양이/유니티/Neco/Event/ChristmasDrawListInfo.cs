using Crosstales;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasDrawListInfo : MonoBehaviour
{
    Image itemIcon = null;
    Image bg = null;

    public Text boxLevel;
    public Color ActiveColor;
    public Color DeactiveColor;

    public void Init(uint level)
    {
        if (bg == null)
        {
            bg = GetComponent<Image>();
        }

        if (itemIcon == null)
        {
            itemIcon = gameObject.CTFind("itemIcon").GetComponent<Image>();
        }
        boxLevel.text = level.ToString();
        Deactive();
    }

    public void SetReward(ChristmasDrawLayer.DrawInfo drawInfo)
    {
        if (drawInfo == null)
        {
            return;
        }

        RewardData rewardData = drawInfo.ToRewardData();
        if (rewardData.gold > 0)
        {
            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
        }
        else if (rewardData.catnip > 0)
        {
            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
        }
        else if (rewardData.point > 0)
        {
            itemIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");
        }
        else if (rewardData.itemData != null)
        {
            itemIcon.sprite = rewardData.itemData.GetItemIcon();
        }
        else if (rewardData.memoryData != null)
        {
            itemIcon.sprite = Resources.Load<Sprite>(rewardData.memoryData.GetNecoMemoryThumbnail());
        }

        itemIcon.gameObject.SetActive(true);
        bg.color = ActiveColor;
    }

    public void Deactive()
    {
        itemIcon.gameObject.SetActive(false);
        bg.color = DeactiveColor;
    }
}
