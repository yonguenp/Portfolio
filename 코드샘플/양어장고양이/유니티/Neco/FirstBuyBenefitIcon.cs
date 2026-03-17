using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstBuyBenefitIcon : MonoBehaviour
{
    public void OnClickIcon()
    {
        NecoCanvas.GetPopupCanvas().ShowBanner(LocalizeData.GetText("LOCALIZE_465"), LocalizeData.GetText("LOCALIZE_466"), NecoBannerPopup.BANNER_TYPE.FIRST_BANNER,
            () => {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.PACKAGE); 
            });
    }
}
