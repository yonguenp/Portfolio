using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextLocalize : MonoBehaviour
{
    public string TextKey;
    
    private void OnEnable()
    {
        SetText();
    }

    public void SetText()
    {
        CancelInvoke("SetText");

        if (string.IsNullOrEmpty(TextKey))
            return;

        if (LocalizeData.isDataLoaded())
        {
            Text curTextComponent = GetComponent<Text>();
            if (curTextComponent)
            {
                curTextComponent.text = LocalizeData.GetText(TextKey);
            }

            enabled = false;
        }
        else
        {
            Invoke("SetText", 1.0f);
        }
    }

    public IEnumerator LocalizeDataLoadChecker()
    {
        while(!LocalizeData.isDataLoaded())
        {
            yield return new WaitForEndOfFrame();
        }

        Text curTextComponent = GetComponent<Text>();
        if (curTextComponent)
        {
            curTextComponent.text = LocalizeData.GetText(TextKey);            
        }
    }
}
