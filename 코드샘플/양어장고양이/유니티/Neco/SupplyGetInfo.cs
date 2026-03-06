using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SupplyGetInfo : MonoBehaviour
{
    public Image supplyItemIcon;

    public Text supplyItemNameText;
    public Text supplyCountText;

    public GameObject dimmedLayer;

    RewardData curSupplyItemData;

    public void InitSupplyItemInfo(RewardData rewardData)
    {
        curSupplyItemData = rewardData;

        if (curSupplyItemData == null) { return; }

        object obj;
        supplyItemIcon.sprite = curSupplyItemData.itemData.GetItemIcon();
        supplyItemNameText.text = curSupplyItemData.itemData.GetItemName();
        
        supplyCountText.text = string.Format("{0}", curSupplyItemData.count);

        dimmedLayer.SetActive(curSupplyItemData.count == 0);

        //StartCoroutine(PlayAnimation(index));
    }

    //IEnumerator PlayAnimation(uint index)
    //{
    //    yield return new WaitForSeconds(0.2f * index);

    //    animation.Play();

    //    while (animation.isPlaying)
    //    {
    //        yield return null;
    //    }

    //    Destroy(this.gameObject);
    //}
}
