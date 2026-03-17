using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardInfo : MonoBehaviour
{
    public Image rewardIcon;
    public Text rewardCountText;
    public Text rewardItemNameText;

    public void SetRewardInfoData(RewardData rewardData)
    {
        if (rewardData == null)
        {
            return;
        }

        if (rewardData.gold > 0)
        {
            rewardIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");

            rewardItemNameText.text = LocalizeData.GetText("LOCALIZE_334");
            rewardCountText.text = string.Format("{0}", rewardData.gold.ToString("n0"));
        }
        else if (rewardData.catnip > 0)
        {
            rewardIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");

            rewardItemNameText.text = LocalizeData.GetText("LOCALIZE_348");
            rewardCountText.text = string.Format("{0}", rewardData.catnip.ToString("n0"));
        }
        else if (rewardData.point > 0)
        {   
            rewardIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");

            rewardItemNameText.text = LocalizeData.GetText("LOCALIZE_335");
            rewardCountText.text = string.Format("{0}", rewardData.point.ToString("n0"));
        }
        else if (rewardData.itemData != null)
        {
            rewardIcon.sprite = rewardData.itemData.GetItemIcon();
            rewardItemNameText.text = rewardData.itemData.GetItemName();
            rewardCountText.text = string.Format("{0}", rewardData.count.ToString("n0"));
        }
        else if(rewardData.memoryData != null)
        {
            rewardIcon.sprite = Resources.Load<Sprite>(rewardData.memoryData.GetNecoMemoryThumbnail());
            //rewardItemNameText.text = rewardData.memoryData.GetNecoMemoryTitle();
            rewardCountText.text = "";
        }
    }
}
