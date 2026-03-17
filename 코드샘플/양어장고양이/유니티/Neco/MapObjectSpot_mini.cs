using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapObjectSpot_mini : MapObjectSpot
{
    public GameObject[] SubStateObjects;
    enum SUB_STATE { EMPTY_DURABILITY, NORMAL, CAT_APPEAR }
    public override void RefreshSpot()
    {
        CancelInvoke("RefreshSpot");
        ClearMapObject();

        if (curSpotData == null)
            return;

        int index = -1;

        switch (curSpotData.GetSpotState())
        {
            case neco_spot.SPOT_STATE.UNKNOWN:
                //gameObject.SetActive(false);
                return;
            case neco_spot.SPOT_STATE.NOTHING:
                index = (int)neco_spot.SPOT_STATE.NOTHING;
                break;
            case neco_spot.SPOT_STATE.OBJECT_SET:
                index = (int)neco_spot.SPOT_STATE.OBJECT_SET;
                break;
            case neco_spot.SPOT_STATE.ON_CAT:
                index = (int)neco_spot.SPOT_STATE.ON_CAT;
                break;
        }

        if (EmptyDurabilityIcon != null)
        {
            //아이콘 제작안됨.
            EmptyDurabilityIcon.SetActive(false);
        }

        curUIState = (neco_spot.SPOT_STATE)index;
        SUB_STATE state = SUB_STATE.NORMAL;

        if (MapObject[index] != null)
        {
            MapObject[index].SetActive(true);
        }

        if (curUIState == neco_spot.SPOT_STATE.ON_CAT)
        {
            state = SUB_STATE.CAT_APPEAR;
            neco_cat cat = curSpotData.GetCurSpotCat(2);
            if (cat == null)
            {
                if (MapObject[(int)neco_spot.SPOT_STATE.OBJECT_SET] != null)
                {
                    MapObject[(int)neco_spot.SPOT_STATE.OBJECT_SET].SetActive(true);
                }
            }
        }

        foreach (NecoCatSpotContainer catContiner in CatSpot)
        {
            catContiner.RefreshCat(curSpotData);
        }

        
        if (curSpotData.GetSpotItemDurability() <= 0 && curUIState != neco_spot.SPOT_STATE.ON_CAT)
        {
            state = SUB_STATE.EMPTY_DURABILITY;
        }

        foreach(GameObject sub in SubStateObjects)
        {
            sub.SetActive(false);
        }

        SubStateObjects[(int)state].SetActive(true);
    }

    public void OnFixMiniObject()
    {
        NecoCanvas.GetPopupCanvas().ShowRepairPopup(curSpotData);
    }
}
