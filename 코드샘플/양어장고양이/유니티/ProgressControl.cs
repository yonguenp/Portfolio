using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressControl : MonoBehaviour
{
    public Image gauge;
    float limitTime = 0;
    float curTime = 0;

    public delegate void Callback();
    Callback doneCallback;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateProgressBar()
    {
        if (limitTime > 0)
        {
            curTime += Time.deltaTime;
            if (limitTime <= curTime)
            {
                doneCallback?.Invoke();
                SetNormalMode();
                return;
            }

            gauge.fillAmount = 1.0f - (curTime / limitTime);
        }
    }

    public void SetNormalMode()
    {
        doneCallback = null;
        limitTime = 0;
        gameObject.SetActive(false);
    }

    public void SetInteractionMode(float time, Callback callback)
    {
        doneCallback = callback;
        gameObject.SetActive(true);
        gauge.fillAmount = 1.0f;
        
        curTime = 0;
        limitTime = time;
    }
}
