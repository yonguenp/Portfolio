using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatnipBuyPopup : MonoBehaviour
{
    public GameObject catLeafBuyListScrollContainer;
    public GameObject catLeafListCloneObject;

    public void OnEnable()
    {
        SetCatLeafItemList();
    }

    public void OnClickOkButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CATNIP_BUY_POPUP);        
    }

    void SetCatLeafItemList()
    {
        if (catLeafListCloneObject == null) { return; }

        if (catLeafBuyListScrollContainer == null) { return; }

        
        foreach (Transform child in catLeafBuyListScrollContainer.transform)
        {
            if(child != catLeafListCloneObject)
                Destroy(child.gameObject);
        }
        catLeafListCloneObject.SetActive(true);

        List<neco_shop> catListShopList = neco_shop.GetNecoShopListByType("dia");
        foreach (neco_shop shopData in catListShopList)
        {
            if (!string.IsNullOrEmpty(shopData.GetIAPConstants()))
            {
                GameObject catLeafBuy = Instantiate(catLeafListCloneObject);
                catLeafBuy.transform.SetParent(catLeafBuyListScrollContainer.transform);
                catLeafBuy.transform.localScale = Vector3.one;
                (catLeafBuy.transform as RectTransform).sizeDelta = new Vector2(85, 0);
                catLeafBuy.transform.localPosition = Vector2.zero;

                catLeafBuy.GetComponent<ShopCashItemInfo>().SetCatLeafBuyData(shopData, null);
                Button btn = catLeafBuy.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.AddListener(OnClickOkButton);
            }
        }

        catLeafListCloneObject.SetActive(false);
    }
}
