using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextLocalize : MonoBehaviour
{
    public string TextKey;
    public string[] param;

    Text curTextComponent;

    private void OnEnable()
    {
        SetText();
    }

    public void SetText()
    {
        if (string.IsNullOrEmpty(TextKey))
            return;

        if(curTextComponent == null)
            curTextComponent = GetComponent<Text>();

        if (curTextComponent)
        {
            if(param == null || param.Length <= 0)
            {
                curTextComponent.text = StringManager.GetString(TextKey);
            }
            else
            {
                curTextComponent.text = StringManager.GetString(TextKey, param);
            }
        }
            
    }
}
