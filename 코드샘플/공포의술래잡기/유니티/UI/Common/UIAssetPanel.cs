using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAssetPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public bool PointDownAction = false;
    public ASSET_TYPE AssetType;
    public Transform TooltipTransform;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipTransform == null)
            return;

        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP))
        {
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);
            return;
        }

        ShowTooltip();
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!PointDownAction)
            return;

        switch (AssetType)
        {
            case ASSET_TYPE.GOLD:
                PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
                break;
            case ASSET_TYPE.DIA:
                PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
                break;
            case ASSET_TYPE.MILEAGE:
                if (!PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.GACHARESULT_POPUP))
                    PopupCanvas.Instance.ShowGachaPopup(2);
                break;
            case ASSET_TYPE.ITEM:
                if (TooltipTransform.parent.GetComponent<UISoulstone>() != null)
                {
                    PopupCanvas.Instance.ShowExchangePopup(10);
                }
                else if (TooltipTransform.parent.GetComponent<UITicket>() != null)
                {
                    PopupCanvas.Instance.ShowShopPopup(3);
                }
                break;
        }
    }

    protected virtual void ShowTooltip()
    {
        Vector3 pos = TooltipTransform.position;
        pos.y = transform.TransformPoint(0.0f, ((transform as RectTransform).sizeDelta.y + 200.0f) * -1.0f + GameConfig.Instance.TOOLTIP_OFFSET, 0.0f).y;

        if (AssetType == ASSET_TYPE.ITEM && TooltipTransform.parent.GetComponent<UISoulstone>() != null)
            PopupCanvas.Instance.ShowTooltip(new BundleInfo(ASSET_TYPE.ITEM, 4, 0), pos);
        else if (AssetType == ASSET_TYPE.ITEM && TooltipTransform.parent.GetComponent<UITicket>() != null)
            PopupCanvas.Instance.ShowTooltip(new BundleInfo(ASSET_TYPE.ITEM, 18, 0), pos);
        else
        {
            PopupCanvas.Instance.ShowTooltip(AssetType, pos);
        }
    }
}
