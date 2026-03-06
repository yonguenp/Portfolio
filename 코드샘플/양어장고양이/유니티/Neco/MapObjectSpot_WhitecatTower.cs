using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectSpot_WhitecatTower : MapObjectSpot
{
    public GameObject butterfly;
    
    protected override void Awake()
    {
        RefreshUpgrade();
        base.Awake();
    }
    public override uint GetSpotID()
    {
        RefreshUpgrade();
        return SpotID;
    }

    void RefreshUpgrade()
    {
        switch (SpotID)
        {
            case 5:
                if (user_items.GetUserItemAmount(157) > 0)
                {
                    butterfly.SetActive(true);
                    PlayerPrefs.SetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_157", 1);
                    OnInitSpot();
                }
                break;
            case 26:
                {
                    butterfly.SetActive(false);
                    PlayerPrefs.SetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_157", 0);
                    OnInitSpot();
                }
                break;
        }
    }

    public void OnIAPObjectHelpPopup()
    {
        NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP].GetComponent<IAPObjectHelpPopup>().SetUIType(0);
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP);
    }
}
