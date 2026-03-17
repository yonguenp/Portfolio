using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatLeafShopPanel : MonoBehaviour
{
    public GameObject catLeafShopListContainer;

    [Header("[CatLeaf Item Info List]")]
    public GameObject catLeafItemListScrollContainer;
    public GameObject catLeafBuyListScrollContainer;

    [Header("[Item Clone Objects]")]
    public GameObject catLeafListCloneObject;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    NecoShopPanel rootParentPanel;

    public void InitCatLeafShopPanel(NecoShopPanel parentPanel)
    {
        rootParentPanel = parentPanel;

        ClearData();

        SetCatLeafItemList();

        catLeafShopListContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void SetCatLeafItemList()
    {
        if (catLeafListCloneObject == null) { return; }

        if (catLeafItemListScrollContainer == null || catLeafBuyListScrollContainer == null) { return; }

        foreach (Transform child in catLeafItemListScrollContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in catLeafBuyListScrollContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<neco_shop> catListShopList = neco_shop.GetNecoShopListByType("dia");
        catListShopList.Sort((neco_shop pkgA, neco_shop pkgB) =>
        {
            //후에 GetNecoShopGoodsID 를 GetNecoShopWeight으로 변경
            if (pkgA.GetNecoShopOrder() < pkgB.GetNecoShopOrder())
                return 1;
            else if (pkgA.GetNecoShopOrder() > pkgB.GetNecoShopOrder())
                return -1;
            else
                return 0;
        });

        foreach (neco_shop shopData in catListShopList)
        {
            if (shopData.GetNecoShopPriceType() == "dia")
            {
                GameObject catLeafItem = Instantiate(catLeafListCloneObject);
                catLeafItem.transform.SetParent(catLeafItemListScrollContainer.transform);
                catLeafItem.transform.localScale = catLeafListCloneObject.transform.localScale;
                catLeafItem.transform.localPosition = catLeafListCloneObject.transform.localPosition;

                catLeafItem.GetComponent<ShopCashItemInfo>().SetCatLeafItemData(shopData, rootParentPanel);
            }
            else
            {
                GameObject catLeafBuy = Instantiate(catLeafListCloneObject);
                catLeafBuy.transform.SetParent(catLeafBuyListScrollContainer.transform);
                catLeafBuy.transform.localScale = catLeafListCloneObject.transform.localScale;
                catLeafBuy.transform.localPosition = catLeafListCloneObject.transform.localPosition;

                catLeafBuy.GetComponent<ShopCashItemInfo>().SetCatLeafBuyData(shopData, rootParentPanel);
            }
        }
    }

    public void RefreshData()
    {
        ClearData();

        SetCatLeafItemList();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    void ClearData()
    {

    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }
    }
}
