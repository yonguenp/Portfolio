using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuseokIcon : MonoBehaviour
{
    public void OnClick()
    {
        NecoCanvas.GetPopupCanvas().OnChuseokEventPopupShow();
    }
}
