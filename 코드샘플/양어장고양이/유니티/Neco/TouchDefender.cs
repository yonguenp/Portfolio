using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDefender : MonoBehaviour
{
    public delegate void Callback();

    public void SetDefend(float time, Callback cb)
    {
        StartCoroutine(DelayHideAndCallback(time, cb));
    }

    IEnumerator DelayHideAndCallback(float time, Callback cb)
    {
        yield return new WaitForSeconds(time);

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.TOUCH_DEFENDER);
        cb?.Invoke();
    }
}
