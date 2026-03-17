
public class NecoFishfarmBooster : NecoBooster
{
    public void OnClickFishFarmLayer()
    {
        if (neco_data.Instance.GetFarmBoostTime() > NecoCanvas.GetCurTime())
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_240"), LocalizeData.GetText("LOCALIZE_241"));
            return;
        }

        // 양어장 부스터 관련 처리
        ConfirmPopupData popupData = SetConfirmPopupData(BOOST_TYPE.FISH_FARM);

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, TryPurchaseBooster);
    }

    public void TryPurchaseBooster()
    {
        PurchaseBoostItem(BOOST_TYPE.FISH_FARM, RefreshData);
    }

    public void RefreshData()
    {

    }
}
