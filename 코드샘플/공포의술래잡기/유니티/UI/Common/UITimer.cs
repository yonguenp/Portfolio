using System;
using UnityEngine;
using UnityEngine.UI;


public class UITimer : MonoBehaviour
{
    [SerializeField]
    protected Text TimerText;
    public int Remain { get; private set; }

    Action callback = null;
    public void SetTimer(int remain, Action cb = null)
    {
        Remain = remain;
        callback = cb;

        SetTimer();
    }

    public void SetTimer()
    {
        CancelInvoke("SetTimer");

        SetText(Remain);

        if (Remain > 0)
        {
            Remain--;
            Invoke("SetTimer", 1.0f);
        }       
        else
        {
            callback?.Invoke();
        }
    }

    public virtual void SetText(int Remain)
    {
        if(TimerText != null)
            TimerText.text = Remain.ToString();
    }
}
