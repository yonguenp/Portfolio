using Coffee.UIEffects;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ChildComponentTool : MonoBehaviour
{
    [ContextMenu("Remove Child Script")]
    void RemoveScriptComponent()
    {
        int removeCount = 0;

        MonoBehaviour[] childScripts = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour childScript in childScripts)
        {
            if (childScript == null) { continue; }
            if (childScript.gameObject.transform == transform) { continue; }

            Type scriptType = childScript.GetType();
            var scope = scriptType.Namespace;
            if (scope == null || scope.StartsWith("SandboxNetwork"))
            {
                DestroyImmediate(childScript);
                removeCount++;
            }
        }

        string resultContext = string.Format("Remove Child Script Finish --> Remove Count : {0}", removeCount);
        Debug.Log(resultContext);
    }

    [ContextMenu("Remove Child Button")]
    void RemoveButtonComponent()
    {
        int removeCount = 0;

        Button[] childButtons = gameObject.GetComponentsInChildren<Button>(true);
        foreach (Button childButton in childButtons)
        {
            if (childButton == null) { continue; }
            if (childButton.gameObject.transform == gameObject.transform) { continue; }

            DestroyImmediate(childButton);
            removeCount++;
        }

        string resultContext = string.Format("Remove Child Button Finish --> Remove Count : {0}", removeCount);
        Debug.Log(resultContext);
    }

    [ContextMenu("Remove Child Scroll")]
    void RemoveScrollComponent()
    {
        int removeScrollCount = 0;
        int removeBarCount = 0;

        ScrollRect[] childScrolls = gameObject.GetComponentsInChildren<ScrollRect>(true);
        foreach (ScrollRect childScroll in childScrolls)
        {
            if (childScroll == null) { continue; }
            if (childScroll.gameObject.transform == transform) { continue; }

            DestroyImmediate(childScroll);
            removeScrollCount++;
        }

        Scrollbar[] childScrollbars = gameObject.GetComponentsInChildren<Scrollbar>(true);
        foreach (Scrollbar childScrollbar in childScrollbars)
        {
            if (childScrollbar == null) { continue; }

            DestroyImmediate(childScrollbar);
            removeBarCount++;
        }

        string resultContext = string.Format("Remove Child Scroll Finish --> Remove ScrollRect Count : {0} / Remove ScrollBar Count : {1}", removeScrollCount, removeBarCount);
        Debug.Log(resultContext);
    }

    [ContextMenu("Remove Child For Tutorial")]
    void RemoveChildForTutorial()
    {
        RemoveButtonComponent();
        RemoveScrollComponent();
        RemoveScriptComponent();
    }


    [SerializeField]
    Font targetFont = null;
    [ContextMenu("Font Change")]
    void ChangeChildFont()
    {
        if (targetFont == null)
        {
            Debug.LogError("targetFont를 설정해주세요.");
            return;
        }

        int removeTextCount = 0;

        Text[] childTexts = gameObject.GetComponentsInChildren<Text>(true);
        foreach (Text childText in childTexts)
        {
            if (childText == null) { continue; }
            if (childText.gameObject.transform == transform) { continue; }

            childText.font = targetFont;
            removeTextCount++;
        }

        string resultContext = string.Format("Change Font Finish --> Change Font Count : {0}", removeTextCount);
        Debug.Log(resultContext);
    }

    [ContextMenu("ButtonFont Change")]
    void ChangeButtonChildFont()
    {
        List<string> buttonNameList = new List<string>();
        Button[] childButtons = gameObject.GetComponentsInChildren<Button>(true);
        foreach (Button button in childButtons)
        {
            if (button == null) { continue; }
            if (button.gameObject.transform == transform) { continue; }

            var text = button.GetComponentInChildren<Text>(true);
            if (text == null) { continue; }

            var rect = button.GetComponent<RectTransform>();

            if(rect.sizeDelta.x * rect.localScale.x > 300 && rect.sizeDelta.y * rect.localScale.y > 150)//원본 사이즈 기준 비교
            {
                text.fontSize = 50;
                text.resizeTextMinSize = 2;
                text.resizeTextMaxSize = 50;
                text.resizeTextForBestFit = true;

                var shadow = text.GetComponent<UIShadow>();
                if (shadow == null)
                    shadow = text.gameObject.AddComponent<UIShadow>();

                shadow.effectColor = Color.black;
                shadow.effectDistance = new Vector2(0,-4);
                shadow.style = ShadowStyle.Shadow;

                var outLine = text.GetComponent<Outline>();
                if (outLine == null)
                    outLine = text.gameObject.AddComponent<Outline>();

                outLine.effectColor = Color.black;
                outLine.effectDistance = new Vector2(2,-2);

                buttonNameList.Add(button.name);
            }
        }

        if(buttonNameList.Count > 0)
        {
            string tempStr = "";
            foreach (var buttonName in buttonNameList)
            {
                tempStr += (buttonName + " ,");
            }

            Debug.Log("change Button List : " + tempStr);
        }
    }
}
