using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchTutorial : MonoBehaviour
{
    public delegate void Callback();
    Callback callback = null;

    public void SetCallback(Callback cb)
    {
        callback = cb;
    }
    public void OnTouchThisObject()
    {
        Destroy(gameObject);
        VideoManager.GetInstance().ResumeVideo();

        callback?.Invoke();
        callback = null;
    }


}
