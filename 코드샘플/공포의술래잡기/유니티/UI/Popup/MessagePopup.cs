using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePopup : Popup
{
    [SerializeField]
    Text textComponent;
    [SerializeField]
    Text buttonTextComponent;

    [SerializeField]
    Vector2 PenalOffset = new Vector2(100, 200);

    public void ShowMessage(string text, string buttonText)
    {
        textComponent.text = text;
        if(buttonTextComponent != null)
            buttonTextComponent.text = buttonText;
    }

    public void ShowMessage(string text)
    {
        textComponent.text = text;
    }
}
