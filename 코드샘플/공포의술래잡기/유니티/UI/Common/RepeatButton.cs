using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RepeatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float trigerButtonCheckTick = 0.5f;
    public float repeatButtonCheckTick = 0.1f;

    public GameObject target;
    public string fuctionName;
    public string exitFuctionName;

    private bool isBtnDown = false;
    private float triger = 0.0f;
    private float repeater = 0.0f;
    private int repeatCount = 0;

    private void OnDisable()
    {
        isBtnDown = false;
        triger = 0.0f;
        repeater = 0.0f;
        repeatCount = 0;
    }

    private void Update()
    {
        if (isBtnDown)
        {
            if(triger < 0.0f)
            {
                if(repeater < 0.0f)
                {
                    if (!string.IsNullOrEmpty(fuctionName))
                        target.SendMessage(fuctionName, repeatCount);
                    
                    repeater = repeatButtonCheckTick;
                    repeatCount = repeatCount + 1;
                }
                else
                {
                    repeater -= Time.deltaTime;
                }
            }            
            else
            {
                triger -= Time.deltaTime;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (target == null || string.IsNullOrEmpty(fuctionName))
            return;

        isBtnDown = true;
        triger = trigerButtonCheckTick;
        repeater = 0.0f;
        repeatCount = 1;

        foreach (MaskableGraphic child in GetComponentsInChildren<MaskableGraphic>())
        {
            child.color = new Color(0.7843137f, 0.7843137f, 0.7843137f, 1.0f);
        }

        target.SendMessage(fuctionName, repeatCount);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isBtnDown = false;
        triger = 0.0f;
        repeater = 0.0f;
        repeatCount = 0;

        foreach (MaskableGraphic child in GetComponentsInChildren<MaskableGraphic>())
        {
            if (child.GetComponent<Text>() != null)
                continue;
            child.color = Color.white;
        }

        if(!string.IsNullOrEmpty(exitFuctionName))
            target.SendMessage(exitFuctionName, repeatCount);
    }

}

