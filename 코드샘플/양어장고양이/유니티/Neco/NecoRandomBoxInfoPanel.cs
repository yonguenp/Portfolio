using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoRandomBoxInfoPanel : MonoBehaviour
{
    [Header("[RandomBox Info List]")]
    public GameObject itemListScrollContainer;
    public GameObject itemListCloneObject;

    [Header("[Layout List]")]
    public RectTransform layoutRect;
    public RectTransform topLayoutRect;

    public void OnClickCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.RANDOMBOX_INFO);
    }

    public void SetBoxInfoData(uint boxID)
    {
        if (itemListScrollContainer == null || itemListCloneObject == null) { return; }

        foreach (Transform child in itemListScrollContainer.transform)
        {
            if (child.gameObject != itemListCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        itemListCloneObject.SetActive(true);

        List<neco_box> boxList = neco_box.GetNecoBoxListByBoxID(boxID);

        foreach (neco_box boxData in boxList)
        {
            GameObject boxInfoUI = Instantiate(itemListCloneObject);
            boxInfoUI.transform.SetParent(itemListScrollContainer.transform);
            boxInfoUI.transform.localScale = itemListCloneObject.transform.localScale;
            boxInfoUI.transform.localPosition = itemListCloneObject.transform.localPosition;

            BoxItemData boxItemData = GetBoxItemData(boxData);
            boxInfoUI.GetComponent<BoxItemInfo>().SetBoxItemInfoData(boxItemData);
        }

        itemListCloneObject.SetActive(false);

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }

        itemListScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }

    BoxItemData GetBoxItemData(neco_box boxData)
    {
        if (boxData == null) { return null; }

        BoxItemData newBoxData = new BoxItemData();

        switch (boxData.GetNecoBoxRewardType()) 
        {
            case "item":
                items itemData = items.GetItem(boxData.GetNecoRewardID());
                newBoxData.itemImage = itemData?.GetItemIcon();
                newBoxData.itemName = itemData?.GetItemName();
                newBoxData.boxItemType = "item";
                newBoxData.itemCount = boxData.GetNecoRewardCount().ToString("n0");
                break;
            case "gold":
                newBoxData.itemImage = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                newBoxData.itemName = LocalizeData.GetText("LOCALIZE_229");
                newBoxData.boxItemType = "gold";
                newBoxData.itemCount = boxData.GetNecoRewardCount().ToString("n0");
                break;
            case "memory":
                neco_cat_memory memoryData = neco_cat_memory.GetNecoMemory(boxData.GetNecoRewardID());
                newBoxData.itemImage = Resources.Load<Sprite>(memoryData?.GetNecoMemoryThumbnail());
                //newBoxData.itemName = memoryData?.GetNecoMemoryTitle();
                newBoxData.itemName = "";
                newBoxData.boxItemType = "memory";
                //newBoxData.itemCount = boxData.GetNecoRewardCount().ToString("n0");
                newBoxData.itemCount = "";
                break;
        }

        return newBoxData;
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null && topLayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(topLayoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null && topLayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(topLayoutRect);
        }
    }
}
