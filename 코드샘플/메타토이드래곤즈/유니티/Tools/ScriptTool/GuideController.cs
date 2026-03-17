using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideController : MonoBehaviour
{
    [SerializeField] RectTransform HighlightArea = null;
    [SerializeField] RectTransform GuideTextFrame = null;
    [SerializeField] Text GuideText = null;

    Action callback = null;
    public void OnGuide(RectTransform guideArea, string text, Action cb = null)
    {
        GuideText.text = text;
        HighlightArea.sizeDelta = guideArea.sizeDelta + new Vector2(200, 0);
        HighlightArea.position = guideArea.position;
        HighlightArea.localPosition = HighlightArea.localPosition + new Vector3(-80, 0, 0);

        gameObject.SetActive(true);
        callback = cb;
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
        if(callback != null)
        {
            callback.Invoke();
            callback = null;
        }
    }

    private void Update()
    {
        GuideTextFrame.sizeDelta = (GuideText.transform as RectTransform).sizeDelta + new Vector2(150, 100);
    }
}
