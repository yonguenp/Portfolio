using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapObjectSpot_MovableTower : MapObjectSpot
{
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
            case 20:
                if (user_items.GetUserItemAmount(154) > 0)
                {
                    PlayerPrefs.SetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_154", 1);
                    OnInitSpot();
                }
                break;
            case 25:
                {
                    PlayerPrefs.SetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_154", 0);
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
