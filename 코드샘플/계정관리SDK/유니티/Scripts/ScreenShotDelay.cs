using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotDelay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnShotEvent()
    {
        UnityEngine.UI.Button button = gameObject.GetComponent<UnityEngine.UI.Button>();
        if (button)
        {
            button.enabled = false;
        }

        Transform iconTransform = transform.GetChild(0);
        if (iconTransform)
        {
            iconTransform.gameObject.SetActive(false);
        }
        Transform sandClockTransform = transform.GetChild(1);
        if (sandClockTransform)
        {
            sandClockTransform.gameObject.SetActive(true);
            Animator iconAnimator = sandClockTransform.GetComponent<Animator>();
            if (iconAnimator)
                iconAnimator.Play("In");
        }

        Invoke("OnNormalState", 5);
    }

    public void OnNormalState()
    {
        UnityEngine.UI.Button button = gameObject.GetComponent<UnityEngine.UI.Button>();
        if (button)
        {
            button.enabled = true;
        }

        Transform iconTransform = transform.GetChild(0);
        if (iconTransform)
        {
            iconTransform.gameObject.SetActive(true);
        }
        Transform sandClockTransform = transform.GetChild(1);
        if (sandClockTransform)
        {
            sandClockTransform.gameObject.SetActive(false);            
        }
    }
}
