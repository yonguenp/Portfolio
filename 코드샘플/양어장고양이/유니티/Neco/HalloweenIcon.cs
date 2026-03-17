using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalloweenIcon : MonoBehaviour
{
    public void OnClick()
    {
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.HALLOWEEN_POPUP);
    }
}
